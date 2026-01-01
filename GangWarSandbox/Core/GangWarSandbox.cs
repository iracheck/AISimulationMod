// GTA Gang War Sandbox - LemonUI Version
// Requirements:
// - ScriptHookVDotNet v3
// - LemonUI.SHVDN3

using GangWarSandbox;
using GangWarSandbox.Core;
using GangWarSandbox.Gamemodes;
using GangWarSandbox.Peds;
using GangWarSandbox.Utilities;
using GTA;
using GTA.Math;
using GTA.Native;
using LemonUI;
using LemonUI.Menus;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Runtime.Remoting.Messaging;
using System.Windows.Forms;

namespace GangWarSandbox
{
    public class GangWarSandbox : Script
    {
        public static GangWarSandbox Instance { get; private set; }

        // Configuration
        public bool DEBUG => GWSettings.DEBUG;

        // Constants
        public const int NUM_TEAMS = 4; // How many teams? In the future, it will be loaded from a settings file, but for now it's constant to keep stability
        private const int TIME_BETWEEN_SQUAD_SPAWNS = 3000; // Time in milliseconds between squad spawns for each team

        // Teams
        public int PlayerTeam = 0; // -1 is 
        public List<Team> Teams = new List<Team>();
        public Dictionary<string, Faction> Factions = new Dictionary<string, Faction>();
        public Dictionary<Team, float> LastSquadSpawnTime = new Dictionary<Team, float>(); // Track last spawn time for each team to prevent spamming or crowding

        public List<BlipSprite> BlipSprites = new List<BlipSprite>
        {
            BlipSprite.Number1,
            BlipSprite.Number2,
            BlipSprite.Number3,
            BlipSprite.Number4,
            BlipSprite.Number5,
            BlipSprite.Number6
        };

        public static readonly Dictionary<Color, BlipColor> TeamColors = new Dictionary<Color, BlipColor>
        {
            { Color.Green, BlipColor.Green },
            { Color.Red, BlipColor.Red },
            { Color.Blue, BlipColor.Blue },
            { Color.Yellow, BlipColor.Yellow },
            { Color.Purple, BlipColor.Purple },
            { Color.Orange, BlipColor.Orange }
        };



        // Tracked Peds
        public List<Ped> DeadPeds = new List<Ped>();
        public List<Vehicle> SquadlessVehicles = new List<Vehicle>();

        // Capture Points
        public List<CapturePoint> CapturePoints = new List<CapturePoint>();

        // Game State
        public bool IsBattleRunning = false;

        // Game Options
            // Options relating to the battle, e.g. unit counts or vehicles

        public bool UseVehicles = false;
        public bool UseWeaponizedVehicles = false;
        public bool UseHelicopters = false;

        public Gamemode CurrentGamemode;
        public List<Gamemode> AvaliableGamemodes = new List<Gamemode>
        {
            new InfiniteBattleGamemode(),
            new SurvivalGamemode(),
            // Add more gamemodes here as needed
            // Future expansion: allow users to make their own gamemodes in a dll?
        }; 


        // Player Info
        Ped Player = Game.Player.Character;
        bool PlayerDied = false;
        int TimeOfDeath;

        public GangWarSandbox()
        {
            Instance = this;
            CurrentGamemode = AvaliableGamemodes[0];

            Logger.Log("GangWarSandbox loaded using build " + GWSMeta.Version + ", built on date: " + GWSMeta.BuildDate.ToString() + ".\n", "META");

            // Ensure valid directories exist on startup
            ModFiles.EnsureDirectoriesExist();

            // Try to load the configuration files
            Factions = ConfigParser.LoadFactions();
            ConfigParser.LoadConfiguration();


            if (Factions.Count == 0)
            {
                NotificationHandler.Send("~r~Warning: ~w~GangWarSandbox requires at least one faction in order to load. Please create one, or use one of the default ones provided (redownload the mod if necessary).");
                return;
            }
            else if (Factions.Count == 1)
            {
                NotificationHandler.Send("~r~Warning: ~w~GangWarSandbox works best with more than one faction. While you can play with all teams being the same, it's more fun to add more. To add factions, visit the \"Factions\" subfolder in the mod files.");
            }
            else if (DEBUG)
            {
                NotificationHandler.Send("GangWarSandbox loaded in ~r~debug mode~w~.");
            }
            else
            {
                NotificationHandler.Send("GangWarSandbox loaded. Press " + GWSettings.OpenMenuKeybind + " to begin!");
            }

            Tick += OnTick;
            KeyDown += OnKeyDown;

            for (int i = 0; i < NUM_TEAMS; i++)
            {
                Teams.Add(new Team((i + 1).ToString())); // Initialize teams with default names and groups
                LastSquadSpawnTime[Teams[i]] = 0; // Initialize last spawn time for each team

                Teams[i].BlipSprite = BlipSprites[i]; // Assign a unique blip sprite for each team (for spawnpoints)
            }

            Logger.LogDebug(GWSettings.MAX_CORPSES.ToString() + GWSettings.MAX_SQUADLESS_VEHICLES.ToString() + GWSettings.VEHICLE_AI_UPDATE_FREQUENCY.ToString()
                + GWSettings.AI_UPDATE_FREQUENCY.ToString() + GWSettings.DEBUG.ToString());

            BattleSetupUI.SetupMenu();
        }

        private void OnTick(object sender, EventArgs e)
        {
            Stopwatch sw = Stopwatch.StartNew();

            BattleSetupUI.MenuPool.Process();

            CurrentGamemode.OnTick();

            DrawMarkers();

            int GameTime = Game.GameTime;

            if (IsBattleRunning)
            {
                // Essentially "fakes" that the player is wanted while battles are occuring. This allows the player to use weapons inside their safehouses AND prevents the player from swapping targets.
                Game.Player.DispatchsCops = false; // disable cop dispatches
                Function.Call(Hash.HIDE_HUD_COMPONENT_THIS_FRAME, 1);
                Function.Call(Hash.SET_BLOCK_WANTED_FLASH, true);
                Game.Player.WantedLevel = 1;

                Function.Call(Hash.CLEAR_AREA_OF_COPS, Player.Position.X, Player.Position.Y, Player.Position.Z, 1000f);

                if (Player.IsDead)
                {
                    PlayerDied = true;
                    CurrentGamemode.OnPlayerDeath(GameTime);
                }

                CurrentGamemode.OnTickGameRunning();

                SpawnSquads();

                // Collection error prevention
                var allSquads = Teams.SelectMany(t => t.GetAllSquads()).ToList();

                foreach (var squad in allSquads)
                {
                    // Ped AI 
                    if ((squad.SquadVehicle != null && squad.LastUpdateTime + GWSettings.VEHICLE_AI_UPDATE_FREQUENCY < GameTime) || (squad.LastUpdateTime + GWSettings.AI_UPDATE_FREQUENCY < GameTime) || squad.JustSpawned)
                    {
                        squad.Update();
                        CurrentGamemode.OnSquadUpdate(squad);
                    }

                    // Corpse Removal
                    List<Ped> deadPeds = squad.CleanupDead();

                    if (deadPeds == null) continue;

                    DeadPeds.AddRange(deadPeds);

                    while (DeadPeds.Count >= GWSettings.MAX_CORPSES)
                    {
                        if (DeadPeds[0] != null && DeadPeds[0].Exists())
                        {
                            DeadPeds[0].Delete();
                        }
                        DeadPeds.RemoveAt(0);
                    }
                }

                while (SquadlessVehicles.Count > GWSettings.MAX_SQUADLESS_VEHICLES)
                {
                    if (SquadlessVehicles[0] != null &&  SquadlessVehicles[0].Exists())
                    {
                        SquadlessVehicles[0].Delete();
                    }
                    SquadlessVehicles.RemoveAt(0);
                }

                foreach (var point in CapturePoints)
                {
                    point.CapturePointHandler(); // Process capture points
                }
            }

            sw.Stop();

            if (DEBUG && sw.ElapsedMilliseconds > 5)
            {
                Logger.LogDebug($"Tick took {sw.ElapsedMilliseconds} ms");
            }
        }

        private void OnKeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == GWSettings.OpenMenuKeybind)
            {
                if (!BattleSetupUI.MenuPool.AreAnyVisible)
                    BattleSetupUI.Show();
                else
                    BattleSetupUI.Hide();
            }
        }

        public void StartBattle()
        {
            if (IsBattleRunning || !CurrentGamemode.CanStartBattle()) return;

            ResetPlayerRelations();
            

            for (int i = 0; i < CapturePoints.Count; i++)
            {
                CapturePoints[i].BattleStart();
            }

            foreach (var team in Teams)
            {
                Logger.Log("Team Loaded: " + team.Name);
                team.RecolorBlips();
            }

            CurrentGamemode.OnStart();

            // Spawn squads for each team
            SpawnSquads();

            Game.Player.WantedLevel = 0; // Reset wanted level
            IsBattleRunning = true;
            Function.Call(Hash.SET_POLICE_IGNORE_PLAYER, Player.Handle, true);

        }

        public void StopBattle()
        {
            if (IsBattleRunning == false) return;

            IsBattleRunning = false;

            CurrentGamemode.OnEnd();

            CleanupAll();

            Game.Player.DispatchsCops = true; // Re-enable cop dispatches
            Game.Player.WantedLevel = 0;
            Function.Call(Hash.SET_POLICE_IGNORE_PLAYER, Player.Handle, false);

        }

        private void SpawnSquads()
        {

            foreach (var team in Teams)
            {
                int squadSize = team.GetSquadSize();

                if (squadSize <= 0) continue;

                if 
                (
                    (Game.GameTime - LastSquadSpawnTime[team] >= TIME_BETWEEN_SQUAD_SPAWNS
                    ||
                    team.Squads.Count <= 1)
                    &&
                    CurrentGamemode.ShouldSpawnSquad(team, squadSize)
                )
                {
                    LastSquadSpawnTime[team] = Game.GameTime;

                    Squad squad = null;

                    if (CurrentGamemode.ShouldSpawnVehicleSquad(team))
                    {
                        squad = new Squad(team, Squad.SquadType.CarVehicle);
                    }
                    else if (CurrentGamemode.ShouldSpawnWeaponizedVehicleSquad(team))
                    {
                        squad = new Squad(team, Squad.SquadType.WeaponizedVehicle);
                    }
                    else if (CurrentGamemode.ShouldSpawnHelicopterSquad(team))
                    {
                        squad = new Squad(team, Squad.SquadType.AirHeli);
                    }
                    else 
                    {
                        squad = new Squad(team, Squad.SquadType.Infantry);
                    }

                    if (squad != null) CurrentGamemode.OnSquadSpawn(squad);
                }


            }
        }

        public void AddCapturePoint()
        {
            if (!IsBattleRunning)
            {
                CapturePoint point;
                Vector3 pos;

                if (Game.IsWaypointActive)
                {
                    pos = World.WaypointPosition;

                    GTA.UI.Screen.ShowSubtitle($"Capture point created at waypoint.");
                    World.RemoveWaypoint();
                }
                else
                {
                    pos = Game.Player.Character.Position;

                    GTA.UI.Screen.ShowSubtitle($"Capture point created at player location.");
                }

                if (pos == Vector3.Zero) return;

                pos.Z = World.GetGroundHeight(pos);
                point = new CapturePoint(pos);

                CapturePoints.Add(point);
            }
            else
            {
                GTA.UI.Screen.ShowSubtitle("Stop the battle to create a new capture point.");
            }
        }


        public void AddSpawnpoint(int teamIndex)
        {
            if (!IsBattleRunning)
            {
                if (Game.IsWaypointActive)
                {
                    Vector3 waypointPos = World.WaypointPosition;
                    Teams[teamIndex].AddSpawnpoint(waypointPos);

                    GTA.UI.Screen.ShowSubtitle($"Spawnpoint added for Team {teamIndex + 1} at waypoint.");
                    World.RemoveWaypoint();

                    
                }
                else
                {
                    Vector3 charPos = Game.Player.Character.Position;
                    charPos.Z -= 1;
                    Teams[teamIndex].AddSpawnpoint(charPos);
                    GTA.UI.Screen.ShowSubtitle($"Spawnpoint added for Team {teamIndex + 1} at player location.");

                }
            }
            else
            {
                GTA.UI.Screen.ShowSubtitle("Stop the battle to create a new spawnpoint.");
            }
        }

        public void ClearAllPoints()
        {
            if (IsBattleRunning)
            {
                GTA.UI.Screen.ShowSubtitle("Stop the battle to remove spawnpoints.");
                return;
            }

            foreach (var team in Teams)
            {
                foreach (var blip in team.Blips)
                {
                    if (blip.Exists()) blip.Delete();
                }
                team.Blips.Clear();
                team.SpawnPoints.Clear();
            }

            foreach (var point in CapturePoints)
            {
                if (point.PointBlip.Exists()) point.PointBlip.Delete();
            }
            CapturePoints.Clear();
        }

        public void CleanupAll()
        {
            foreach (var team in Teams)
            {
                // Make a copy of the list to avoid modifying it while iterating
                var squadsToRemove = team.Squads.ToList();

                squadsToRemove.AddRange(team.VehicleSquads.ToList());
                squadsToRemove.AddRange(team.WeaponizedVehicleSquads.ToList());
                squadsToRemove.AddRange(team.HelicopterSquads.ToList());


                foreach (var squad in squadsToRemove)
                {
                    try
                    {
                        squad.Destroy(); // This can safely remove it from team.Squads now

                        team.Squads.Remove(squad);
                        team.VehicleSquads.Remove(squad);
                        team.WeaponizedVehicleSquads.Remove(squad);
                        team.HelicopterSquads.Remove(squad);
                    }
                    catch (Exception ex)
                    {
                        Logger.LogError($"Error cleaning up squad: {ex.Message}"); // Log any errors during cleanup
                    }
                }

                team.Squads.Clear();
                team.VehicleSquads.Clear();
                team.WeaponizedVehicleSquads.Clear();
                team.HelicopterSquads.Clear();

                // Clean up squadless vehicles
                foreach (var vehicle in SquadlessVehicles)
                {
                    vehicle.AttachedBlip?.Delete(); // Remove blip if it exists

                    if (vehicle == Player.CurrentVehicle) continue;

                    try
                    {
                        if (vehicle.Exists())
                            vehicle.Delete();
                    }
                    catch (Exception ex)
                    {
                        Logger.LogError($"Error cleaning up vehicle: {ex.Message}"); // Log any errors during cleanup
                    }
                }

                SquadlessVehicles.Clear();

                // Also clean up DeadPeds if needed
                foreach (var ped in DeadPeds.ToList())
                {
                    try
                    {
                        if (ped != null && ped.Exists())
                            ped.Delete();
                    }
                    catch (Exception ex)
                    {
                        GTA.UI.Screen.ShowSubtitle($"Dead ped cleanup error: {ex.Message}");
                    }
                }

                DeadPeds.Clear();
            }
        }

        public void ApplyFactionToTeam(Team team, string factionName)
        {
            if (Factions.TryGetValue(factionName, out var faction))
            {
                team.Models = faction.Models;
                team.Faction = faction;
                team.Tier1Weapons = faction.Tier1Weapons;
                team.Tier2Weapons = faction.Tier2Weapons;
                team.Tier3Weapons = faction.Tier3Weapons;
                team.MAX_SOLDIERS = faction.MaxSoldiers;
                team.BaseHealth = faction.BaseHealth;
                team.Accuracy = faction.Accuracy;
                team.TierUpgradeMultiplier = faction.TierUpgradeMultiplier;
                team.TeamIndex = Teams.IndexOf(team);

                team.TeamVehicles = faction.VehicleSet;

                SetColors(team);
            }
        }

        public void SetColors(Team team)
        {
            if (team == null || team.TeamIndex < 0) return;

            int teamIndex = team.TeamIndex;

            List<BlipColor> blipColors = TeamColors.Values.ToList();
            List<Color> colors = TeamColors.Keys.ToList();

            if (teamIndex >= blipColors.Count)
            {
                return;
            }
            else
            {
                team.BlipColor = blipColors[teamIndex];
                team.GenericColor = colors[teamIndex];
            }
        }

        public void ResetPlayerRelations()
        {
            if (PlayerTeam < -2 || PlayerTeam >= Teams.Count)
                PlayerTeam = -1;

            // Force player into custom group
            var PlayerGroup = Game.Player.Character.RelationshipGroup;

            foreach (var team in Teams)
            {
                team.IsPlayerTeam = false;

                // Default everyone to hate player
                Function.Call(Hash.SET_RELATIONSHIP_BETWEEN_GROUPS,
                    (int)Relationship.Hate,
                    PlayerGroup,
                    team.Group);

                Function.Call(Hash.SET_RELATIONSHIP_BETWEEN_GROUPS,
                    (int)Relationship.Hate,
                    team.Group,
                    PlayerGroup);
            }


            if (PlayerTeam == -2) return;
            else if (PlayerTeam == -1)
            {
                // Free agent: everyone respects player
                foreach (var team in Teams)
                {
                    Function.Call(Hash.SET_RELATIONSHIP_BETWEEN_GROUPS,
                        (int)Relationship.Respect,
                        PlayerGroup,
                        team.Group);

                    Function.Call(Hash.SET_RELATIONSHIP_BETWEEN_GROUPS,
                        (int)Relationship.Respect,
                        team.Group,
                        PlayerGroup);
                }
            }
            else
            {
                var playerTeam = Teams[PlayerTeam];

                Function.Call(Hash.SET_RELATIONSHIP_BETWEEN_GROUPS,
                    (int)Relationship.Companion,
                    PlayerGroup,
                    playerTeam.Group);

                Function.Call(Hash.SET_RELATIONSHIP_BETWEEN_GROUPS,
                    (int)Relationship.Companion,
                    playerTeam.Group,
                    PlayerGroup);

                playerTeam.IsPlayerTeam = true;
                playerTeam.Tier4Ped = Player;

                if (playerTeam.SpawnPoints.Count > 0)
                    Player.Position = playerTeam.SpawnPoints[0];
            }
        }



        /// <summary>
        /// Draws markers for capture points, and if debug mode is enabled, squad movement orders
        /// </summary>
        private void DrawMarkers()
        {
            foreach (var point in CapturePoints)
            {
                Color color = point.Owner?.GenericColor ?? Color.White;
                World.DrawMarker(MarkerType.VerticalCylinder, point.Position, Vector3.Zero, Vector3.Zero, new Vector3(CapturePoint.Radius, CapturePoint.Radius, 1f), point.GenericColor);
            }

            if (DEBUG && Teams != null)
            {
                foreach (var team in Teams)
                {
                    if (team?.Squads == null) continue;

                    foreach (var squad in team.Squads)
                    {
                        if (squad == null || squad.Waypoints == null || squad.Waypoints.Count == 0)
                            continue;

                        if (squad.SquadLeader == null || !squad.SquadLeader.Exists())
                            continue;

                        Vector3 squadLeaderPos = squad.SquadLeader.Position;
                        Vector3 targetPos = squad.Waypoints[0];

                        World.DrawLine(squadLeaderPos, targetPos, Color.LimeGreen);
                    }
                }
            }
        }
    }
}