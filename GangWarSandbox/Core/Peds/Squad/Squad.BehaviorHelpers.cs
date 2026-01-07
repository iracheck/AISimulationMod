using GTA.Math;
using GTA;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using GangWarSandbox.Peds;
using GangWarSandbox;
using System.ComponentModel;
using System.Runtime.Serialization;
using GTA.Native;
using System.Drawing;
using System.Runtime.InteropServices.WindowsRuntime;
using GangWarSandbox.Utilities;
using System.CodeDom;

namespace GangWarSandbox.Peds
{
    public partial class Squad
    {
        // Ped Assignnment -- what each ped is doing
        // the squad will behave in a similar way-- inheriting their leader's assignment
        public enum PedAssignment
        {
            Idle,
            AttackNearby,
            RunToPosition,
            DefendArea,
            FollowSquadLeader,
            PushLocation,


            GetIntoVehicle,
            ExitVehicle,
            DriveToPosition,
            VehicleChase,

            // These are all reserved for gamemode AI overrides, and aren't going to be used within this file.
            // Essentially, this is a way to account for AI assignments for gamemodes that may use unique ones
            GamemodeReserved1,
            GamemodeReserved2,
            GamemodeReserved3,
            GamemodeReserved4,
            GamemodeReserved5,
            GamemodeReserved6,
            GamemodeReserved7,
            GamemodeReserved8,
            GamemodeReserved9,
            GamemodeReserved10,
        }

        private void PedAI_CapturePoint(Ped ped)
        {
            if (SquadVehicle != null || !ped.IsInVehicle()) return; // ignore anyone inside a vehicle

            // Assault Capture Point
            if (Role == SquadRole.AssaultCapturePoint && PedAssignments[ped] != PedAssignment.PushLocation && TargetPoint != null)
            {
                if (PedAssignments[ped] != PedAssignment.PushLocation && ped.Position.DistanceTo(TargetPoint.Position) >= 60f)
                {
                    AISubTasks.RunToFarAway(ped, TargetPoint.Position);
                    PedAssignments[ped] = PedAssignment.PushLocation; // set the ped to assault the capture point
                }
            }

            // Defend Capture Point
            if (Role == SquadRole.DefendCapturePoint && PedAssignments[ped] != PedAssignment.DefendArea && TargetPoint != null)
            {
                if (PedAssignments[ped] != PedAssignment.RunToPosition && ped.Position.DistanceTo(TargetPoint.Position) >= 20f)
                {
                    PedAssignments[ped] = PedAssignment.RunToPosition; // set the ped to defend the area

                    SetTarget(TargetPoint.Position);
                }
                else
                {
                    AISubTasks.DefendArea(ped, TargetPoint.Position);
                    PedAssignments[ped] = PedAssignment.DefendArea; // set the ped to defend the area

                }
            }
        }

        private bool PedAI_Combat(Ped ped)
        {
            if (!ped.IsAlive) return false;

            // Ensure ped has a cache entry
            if (!PedTargetCache.ContainsKey(ped))
                PedTargetCache[ped] = (null, 0);

            Ped cachedEnemy = PedTargetCache[ped].enemy;
            int lastCheckedTime = PedTargetCache[ped].timestamp;
            Ped nearbyEnemy = cachedEnemy;

            // Re-evaluate target if necessary
            bool needsRefind = (Game.GameTime - lastCheckedTime > TARGET_REFIND_TIME) ||
                               (nearbyEnemy == null) ||
                               (nearbyEnemy.IsDead) ||
                               (ped.Position.DistanceTo(nearbyEnemy.Position) > SQUAD_ATTACK_RANGE);

            if (nearbyEnemy == null && PedTargetCache[SquadLeader].enemy != null)
            {
                nearbyEnemy = PedTargetCache[SquadLeader].enemy;
            }

            if (needsRefind)
            {
                Vector3 source = ped.IsInVehicle() ? ped.CurrentVehicle.Position : ped.Position;
                nearbyEnemy = FindNearbyEnemy(source, Owner, SQUAD_ATTACK_RANGE);
                PedTargetCache[ped] = (nearbyEnemy, Game.GameTime);
            }

            if (nearbyEnemy == null && !ped.IsInCombat)
                return false;

            bool canExitVehicle = CanGetOutVehicle(ped);
            float distanceToEnemy = ped.Position.DistanceTo(nearbyEnemy.Position);
            bool hasLOS = AISubTasks.HasLineOfSight(ped, nearbyEnemy);

            // VEHICLE LOGIC
            if (ped.IsInVehicle())
            {
                if (nearbyEnemy.IsInVehicle())
                {
                    ped.Task.VehicleChase(nearbyEnemy);
                    PedAssignments[ped] = PedAssignment.VehicleChase;
                }
                else if (distanceToEnemy < 100f)
                {
                    if (hasLOS)
                    {
                        ped.Task.VehicleShootAtPed(nearbyEnemy);
                        PedAssignments[ped] = PedAssignment.VehicleChase;
                    }
                    else if (distanceToEnemy < 70f && canExitVehicle)
                    {
                        ped.Task.FightAgainst(nearbyEnemy);
                        PedAssignments[ped] = PedAssignment.ExitVehicle;
                    }
                }
                return true;
            }

            // ON FOOT LOGIC
            if (distanceToEnemy <= SQUAD_ATTACK_RANGE)
            {
                if (PedAssignments[ped] != PedAssignment.AttackNearby)
                {
                    AISubTasks.AttackEnemy(ped, nearbyEnemy);
                    PedAssignments[ped] = PedAssignment.AttackNearby;
                }
            }
            else
            {
                if (ped.IsInCombat)
                {
                    AISubTasks.AttackNearbyEnemies(ped, 200f);
                    PedAssignments[ped] = PedAssignment.AttackNearby;
                }
                // Move toward last known position if out of LOS
                if (PedAssignments[ped] != PedAssignment.RunToPosition)
                {
                    AISubTasks.RunToFarAway(ped, nearbyEnemy.Position);
                    PedAssignments[ped] = PedAssignment.RunToPosition;
                }
            }

            return true;
        }


        // AI logic relating to ground based infantry navigation
        private bool PedAI_Movement(Ped ped)
        {
            if (ped == SquadLeader)
            {
                if (PedAssignments[ped] != PedAssignment.RunToPosition && Waypoints.Count > 0 && Waypoints[0] != Vector3.Zero) // if the squad has a target, but the squad leader is not moving toward it, move!
                {
                    AISubTasks.RunToFarAway(ped, Waypoints[0]);
                    PedAssignments[ped] = PedAssignment.RunToPosition;
                }

            }
            else // Squad members
            {
                // Follow the squad leader around
                if (PedAssignments[ped] != PedAssignment.FollowSquadLeader)
                {
                    ped.Task.FollowToOffsetFromEntity(SquadLeader, AISubTasks.GenerateRandomOffset(), 2.6f);
                    PedAssignments[ped] = PedAssignment.FollowSquadLeader;
                }
            }

            return true;
        }

        // AI logic relating to vehicle navigation
        private bool PedAI_Driving(Ped ped)
        {
            if (SquadVehicle == null || !SquadVehicle.Exists() || !SquadVehicle.IsAlive) return false;

            bool canExitVehicle = CanGetOutVehicle(ped); // check if the ped can exit the vehicle
            if (canExitVehicle) Function.Call(Hash.SET_PED_COMBAT_ATTRIBUTES, ped, 3, canExitVehicle); // if a ped is the last ped in a weaponized vehicle, ensure they are allowed to get out

            if (ped == SquadLeader)
            {
                // IF the ped is not in a vehicle, has more than one waypoint (not almost at its target), and is not currently entering a vehicle, enter a vehicle
                if (!ped.IsInVehicle() && Waypoints.Count > 1 && PedAssignments[ped] != PedAssignment.GetIntoVehicle)
                {
                    AISubTasks.EnterVehicle(ped, SquadVehicle);
                    PedAssignments[ped] = PedAssignment.GetIntoVehicle; // set the ped to follow the squad leader
                }
                else if (ped.IsInVehicle() && Waypoints.Count > 0)
                {
                    if (ped.CurrentVehicle.HasSiren && !SquadVehicle.IsSirenActive && SquadVehicle.Velocity.Length() > 5) SquadVehicle.IsSirenActive = true; // activate the siren if the ped is in a police vehicle

                    if (PedAssignments[ped] != PedAssignment.DriveToPosition)
                    {
                        bool squadInside = IsSquadInsideVehicle();

                        if (Waypoints.Count == 0 || Waypoints[0] == Vector3.Zero) return false; // no waypoints? can't do anything

                        if (squadInside && Waypoints.Count > 0)
                        {
                            AISubTasks.DriveTo(ped, SquadVehicle, Waypoints[0]);
                            PedAssignments[ped] = PedAssignment.DriveToPosition; // set the ped to drive to the target position
                        }
                    }
                }
                else return false;
            }
            else if (SquadLeader.IsInVehicle() && (!ped.IsInVehicle() || ped.CurrentVehicle != SquadLeader.CurrentVehicle))
            {
                AISubTasks.EnterVehicle(ped, SquadLeader.CurrentVehicle);
                PedAssignments[ped] = PedAssignment.GetIntoVehicle; // set the ped to follow the squad leader
            }

            return true;
        }


        /// EVERYTHING ABOVE IS TO BE DELETED IN AI REVAMP


        //
        // AI & Assignments
        //

        public List<Vector3> Waypoints = new List<Vector3>();
        public Dictionary<Ped, PedAssignment> PedAssignments = new Dictionary<Ped, PedAssignment>();
        public Dictionary<Ped, (Ped enemy, int timestamp)> PedTargetCache;

        //


        public bool CheckForCombat(Ped ped)
        {
            // Initialize ped to cache if missing
            if (!PedTargetCache.ContainsKey(ped) && ped.IsAlive)
                PedTargetCache[ped] = (null, 0);

            Ped nearbyEnemy = PedTargetCache[ped].enemy;

            if ( (ped.IsInCombat && ( PedTargetCache[ped].enemy == null || nearbyEnemy.IsDead) ) || (ped != SquadLeader && SquadLeader.IsInCombat) ||
                Game.GameTime - PedTargetCache[ped].timestamp > TARGET_REFIND_TIME)
            {
                nearbyEnemy = FindNearbyEnemy(ped.Position, Owner, GWSettings.AI_ATTACK_RADIUS);
                PedTargetCache[ped] = (nearbyEnemy, Game.GameTime);
            }

            if (nearbyEnemy == null) return false;
            else return true;
        }

        public Ped FindNearbyEnemy(Vector3 selfPosition, Team team, float distance, bool infiniteSearch = false)
        {
            Ped foundEnemy;
            Vehicle foundVehicle;

            List<Team> enemyTeams = ModData.Teams.Where(t => t != team && !team.AlliedIndexes.Contains(t.TeamIndex)).ToList();

            // Get all enemy squads from other teams
            var enemyPeds = enemyTeams.SelectMany(t => t.GetAllPeds()).ToList();

            var enemyVehicles = enemyTeams.SelectMany(t => t.GetAllSquads(true)).Select(s => s.SquadVehicle).ToList();

            if (ModData.PlayerTeam != -1 && Owner.TeamIndex != ModData.PlayerTeam)
                enemyPeds.Add(Game.Player.Character); // add the player's squad to the list of enemy squads if the squad is not on the player's team

            float range = SQUAD_ATTACK_RANGE;
            if (infiniteSearch) range = 999f;

            foundEnemy = enemyPeds.Where(p => p != null && p.Exists() && !p.IsDead && p.Position.DistanceTo(selfPosition) <= range)
                    .OrderBy(p => p.Position.DistanceTo(selfPosition))
                    .FirstOrDefault();

            foundVehicle = enemyVehicles.Where(p => p != null && p.Exists() && !p.IsDead && p.Position.DistanceTo(selfPosition) <= range)
                    .OrderBy(p => p.Position.DistanceTo(selfPosition))
                    .FirstOrDefault();


            if (foundEnemy == null || !foundEnemy.Exists() || foundEnemy.IsDead)
            {
                foundEnemy = null;
            }
            else
            {
                if (foundVehicle != null && foundVehicle.Driver != null &&
                    foundVehicle.Position.DistanceTo(selfPosition) < foundEnemy.Position.DistanceTo(selfPosition)) foundEnemy = foundVehicle.Driver;
            }

            return foundEnemy;
        }


        public void SetTarget(Vector3 target)
        {
            if (target == Vector3.Zero) return;

            if (SquadLeader.Position.DistanceTo(target) < 5f) return;

            bool hasVehicle = SquadVehicle != null && SquadVehicle.Exists() && SquadVehicle.IsAlive;
            Waypoints = AISubTasks.GetIntermediateWaypoints(SquadLeader.Position, target, hasVehicle); // set the waypoints to the target position

            if (PedAssignments.ContainsKey(SquadLeader)) PedAssignments[SquadLeader] = PedAssignment.Idle;
        }

        public bool IsSquadInsideVehicle()
        {
            return Members.All(m => m.IsInVehicle() && m.CurrentVehicle == SquadLeader.CurrentVehicle);
        }
   
        public bool CanGetOutVehicle(Ped ped)
        {
            if (SquadVehicle == null || !ped.IsInVehicle() || SquadVehicle.IsDead) return true;

            float healthPercent = SquadVehicle.Health / (float)SquadVehicle.MaxHealth;

            // if the vehicle is heavily damaged, can always get out
            if (SquadVehicle.IsOnFire || healthPercent < 0.1) return true;

            // Don't leave weaponized vehicles, unless squad is nearly wiped out
            if (IsWeaponizedVehicle && Members.Count(m => m.IsInVehicle() && m.CurrentVehicle == SquadVehicle) > 1) return false;

            // Finally, only exit if the vehicle is stopped/moving slowly
            if (SquadVehicle.Velocity.Length() <= 1 || SquadVehicle.IsStopped) return true;
            else return false; // otherwise: probably not ok!
        }

    }

}
