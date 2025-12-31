
using GTA;
using GTA.Native;
using GTA.Math;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Windows.Forms;
using LemonUI;
using LemonUI.Menus;
using GangWarSandbox;
using System.Runtime.InteropServices;
using System.Drawing;
using System.Runtime.Serialization;
using System.ComponentModel;
using GangWarSandbox.Core.StrategyAI;
using GangWarSandbox.Core;
using GangWarSandbox.Gamemodes;

namespace GangWarSandbox.Peds
{
    public partial class Squad
    {
        static Random Rand = new Random();
        static AISubTasks PedAI = new AISubTasks();

        static GangWarSandbox ModData = GangWarSandbox.Instance;
        private Gamemode CurrentGamemode => ModData.CurrentGamemode;

        // Constants
        static float SQUAD_ATTACK_RANGE = GWSettings.AI_ATTACK_RADIUS;
        static int TARGET_REFIND_TIME = 1000; // time before cached targets are attempted to be refound (in milliseconds)

        // Squad Logic begins here

        // Lifecycle Control
        public bool JustSpawned = true;
        public double LastUpdateTime = 0; // uses GameTime, the last time (in milliseconds since the game has been opened) in which the squad was updated
        public int CyclesAlive = 0; // simply tracks for how many update cycles this squad has been active in the game -->

        // Members List
        public Ped SquadLeader;
        public List<Ped> Members = new List<Ped>();

        // Squad Metadata
        public Team Owner;
        public int squadValue; // lower value squads may be assigned to less important tasks

        // Abstract Orders
        // these are orders that come from the "Strategy AI" of each team
        public CapturePoint TargetPoint; // the location that the squad's role will be applied to-- variable

        // Squad Stuck Timer-- if the squad leader is stuck for too long, it will try to move again
        private int SquadLeaderStuckTicks = 0;

        // AI 
        public SquadRole Role;
        public SquadType Type;
        public SquadPersonality Personality;


        public Vehicle SquadVehicle = null;
        public bool IsWeaponizedVehicle; // this is set at spawn

        // Runs every 200ms (default) and updates all AI, squad states, etc.
        public bool Update()
        {
            CyclesAlive++;
            LastUpdateTime = Game.GameTime;

            if (IsEmpty())
            {
                Destroy();
                return false;
            }

            // If at the last waypoint, get a new target!
            if (Waypoints.Count == 0) SetTarget(CurrentGamemode.GetTarget(this));

            if (JustSpawned) JustSpawned = false;

            if (SquadLeader == null || SquadLeader.IsDead || !SquadLeader.Exists())
                PromoteLeader();

            if (Waypoints != null && Waypoints.Count > 0)
            {
                bool isCloseEnough = Waypoints.Count > 0 &&
                    (SquadLeader.Position.DistanceTo(Waypoints[0]) < 15f) ||
                    (SquadVehicle != null && SquadVehicle.Position.DistanceTo(Waypoints[0]) < 40f);

                bool waypointSkipped = Waypoints.Count > 1 &&
                    Waypoints[1] != null && Waypoints[1] != Vector3.Zero &&
                    SquadLeader.Position.DistanceTo(Waypoints[1]) < 50f &&
                    Waypoints[0].DistanceTo(SquadLeader.Position) > Waypoints[1].DistanceTo(SquadLeader.Position);

                if (isCloseEnough || waypointSkipped)
                {
                    Waypoints.RemoveAt(0);
                    foreach (var ped in Members)
                    {
                        if (PedAssignments[ped] == PedAssignment.RunToPosition || PedAssignments[ped] == PedAssignment.DriveToPosition)
                        {
                            PedAssignments[ped] = PedAssignment.Idle;
                        }
                    }
                }

                if (Waypoints.Count > 0 && Waypoints[0] == Vector3.Zero)
                {
                    Waypoints.RemoveAt(0);
                }
            }

            // Gamemode: should try to get a new target?
            if (CurrentGamemode.ShouldGetNewTarget(this))
            {
                SetTarget(CurrentGamemode.GetTarget(this));
            }

            for (int i = 0; i < Members.Count; i++)
            {
                Ped ped = Members[i];
                ped.AttachedBlip.Alpha = GetDesiredBlipVisibility(ped, Owner);

                if (ped == null || !ped.Exists() || !ped.IsAlive || ped.IsRagdoll) continue; // skip to the next ped

                // Block permanent events (e.g. automatic AI takeover in gta) when in vehicles
                // ped.BlockPermanentEvents = ped.IsInVehicle() && !ped.IsInCombat;

                // Gamemode based overrides
                if (CurrentGamemode.AIOverride(this, ped)) continue;

                // Handle logic with enemy detection, combat, etc.
                bool combat = PedAI_Combat(ped);

                // Handle logic on defending or assaulting capture points
                PedAI_CapturePoint(ped);

                if (ped.IsShooting && ped.IsInCombat|| PedAssignments[ped] == PedAssignment.AttackNearby || combat) continue;

                // Handle logic with ped moving to and from its target
                bool movementChecked = PedAI_Driving(ped);
                if (!movementChecked) PedAI_Movement(ped);
            }

            if (SquadVehicle != null && SquadVehicle.Exists() && SquadVehicle.AttachedBlip != null && SquadVehicle.AttachedBlip.Exists()) 
                    SquadVehicle.AttachedBlip.Alpha = GetDesiredVehicleBlipVisiblity(SquadVehicle, Owner);

            return true;
        }


        public void PromoteLeader()
        {
            foreach (var ped in Members)
            {
                if (ped.Exists() && !ped.IsDead)
                {
                    if (ped.IsInVehicle() && ped.CurrentVehicle.Driver == ped) continue; // do not promote a driver as leader
                    SquadLeader = ped;
                    return;
                }
            }
        }

        public bool IsEmpty()
        {
            if (Members.Count <= 0) return true;
            else return false;
        }

        // Temp until squad types in introduced
        private int GetSquadSizeByType(SquadType type)
        {
            return Owner.GetSquadSize();
        }

        // Squad points are used in autocalculated battles
        public int GetSquadPoints()
        {
            int squadSize = GetSquadSizeByType(Type);
            int members = Members.Count;
            float multiplier = members / squadSize;

            int points = 0;

            if (members <= 0) return 0;

            points = (int) (squadValue * multiplier) + 50;
            
            return points;
        }

        private int GetDesiredBlipVisibility(Ped ped, Team team)
        {
            int maxAlpha;

            // Absolute conditions
            if (ped.IsInVehicle() || ped.IsDead) return 0;
            else if (ped == SquadLeader) maxAlpha = 255;
            else maxAlpha = 200;

            if (team.TeamIndex == ModData.PlayerTeam || ModData.PlayerTeam == -1) return maxAlpha;

            if (ModData.DEBUG || CurrentGamemode.FogOfWar == false) return maxAlpha;

            // Relative conditions
            float healthPercent = (float)ped.Health / (float)ped.MaxHealth;
            maxAlpha = (int)(maxAlpha * healthPercent + 10); // health

            if (maxAlpha == 0) return 0;

            float dist = ped.Position.DistanceTo(Game.Player.Character.Position);

            // Distance conditions, only happens when player is on a team
            if (dist > 90f) return 0;
            else if (dist < 50f) return maxAlpha;
            else
            {
                maxAlpha = (int)(maxAlpha * (1 - (dist / 200f)));
            }

            return maxAlpha;

        }

        private int GetDesiredVehicleBlipVisiblity(Vehicle vehicle, Team team)
        {
            float distFromPlayer = vehicle.Position.DistanceTo(Game.Player.Character.Position);
            if (Game.Player.Character.CurrentVehicle == vehicle) return 0; // hide players current vehicle

            if (CurrentGamemode.FogOfWar == false) return 255;
            
            if (vehicle.PassengerCount != 0 && !(distFromPlayer > 160f))
            {
                if (vehicle.PassengerCount != 0 && distFromPlayer < 125f) return 255;
                return (int)(255 * (1 - (distFromPlayer / 160f)));
            }
            else return 0; // hide empty or distant vehicles
        }

        public bool IsVehicleSquad()
        {
            return Type == SquadType.CarVehicle || Type == SquadType.WeaponizedVehicle || Type == SquadType.AirHeli;
        }

    }




}