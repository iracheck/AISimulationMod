using GangWarSandbox.Peds;
using GTA;
using GTA.Math;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GangWarSandbox.Peds
{
    /// <summary>
    /// MoveTask is specifically for infantry. While Vehicle squads can use it, they have their own DriveTask etc. to use for navigation. But they use this one when on foot.
    /// </summary>
    internal class MoveTask : PedTask
    {
        List<Vector3> Waypoints => Parent.Waypoints;

        public MoveTask(Peds.Squad parent, Ped character) : base(parent, character)
        {
        }

        public override void Enter()
        {
            if (Waypoints.Count != 0 && IsLeader)
            {
                AISubTasks.RunToFarAway(Ped, Waypoints[0]);
            }
            else
            {
                AISubTasks.FollowPedAtRandomOffset(Ped, Parent.SquadLeader);
            }
        }

        public override bool TransitionState()
        {
            if (Parent.CheckForCombat(Ped))
            {
                SetTask(new AttackNearbyTask(Parent, Ped));
                return true;
            }

            if (IsLeader)
            {
                // if there are no waypoints, there's nowhere to go!
                if (Parent.Waypoints.Count == 0)
                {
                    SetTask(new IdleTask(Parent, Ped));
                    return true;
                }

                if (IsLeader && Parent.TargetPoint.Position.DistanceTo(Ped.Position) < 5f && 
                    Parent.Role == Squad.SquadRole.AssaultCapturePoint && Parent.Waypoints.Count == 1)
                {
                    SetTask(new CaptureTask(Parent, Ped));
                    return true;
                }
            }

            return false;
        }

        public override void Update()
        {
            if (IsLeader)
            {
                if (Waypoints.Count != 0 && Ped.Position.DistanceTo(Waypoints[0]) < 10f)
                {
                    Waypoints.RemoveAt(0);
                    AISubTasks.RunToFarAway(Ped, Waypoints[0]);
                }
            }
        }
    }
}
