using GangWarSandbox.Utilities;
using GTA;
using GTA.Math;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;



namespace GangWarSandbox.MapElements
{
    static class MapElementManager
    {
        // Mod reference
        static GangWarSandbox Mod = GangWarSandbox.Instance;

        // Capture Points
        static public  List<CapturePoint> CapturePoints = new List<CapturePoint>();

        // Spawn Point Distance Check
        static Vector3 FirstSpawnpoint = Vector3.Zero;

        static public void AddCapturePoint()
        {
            if (!Mod.IsBattleRunning)
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


        static public void AddSpawnpoint(int teamIndex)
        {

            Vector3 pos;
            if (!Mod.IsBattleRunning)
            {
                if (Game.IsWaypointActive)
                {
                    pos = World.WaypointPosition;
                    Mod.Teams[teamIndex].AddSpawnpoint(pos);

                    NotificationHandler.Send($"Spawnpoint added for Team {teamIndex + 1} at your currently placed waypoint.");

                    World.RemoveWaypoint();


                }
                else
                {
                    pos = Game.Player.Character.Position;
                    pos.Z -= 1;
                    Mod.Teams[teamIndex].AddSpawnpoint(pos);

                    NotificationHandler.Send($"Spawnpoint added for Team {teamIndex + 1} at your character's position.");

                }
            }
            else
            {
                NotificationHandler.Send($"You must stop the battle to create a new spawnpoint.");
                return;
            }

            if (FirstSpawnpoint == Vector3.Zero)
            {
                FirstSpawnpoint = pos;
                return;
            }

            if (FirstSpawnpoint.DistanceTo(pos) > 300f)
            {
                NotificationHandler.Send("That spawnpoint is pretty far away! Due to the nature of GTA, depending on where you are the navmesh may not load, and thus infantry squads will be stuck. This will be fixed in version 2.0 (next update.)");
            }
        }

        static public void ClearAllPoints()
        {
            if (Mod.IsBattleRunning)
            {
                NotificationHandler.Send($"You must stop the battle to delete points.");
                return;
            }

            FirstSpawnpoint = Vector3.Zero;

            foreach (var team in Mod.Teams)
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
    }
}
