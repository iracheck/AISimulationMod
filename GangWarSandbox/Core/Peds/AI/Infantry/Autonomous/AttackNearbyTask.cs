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
    /// Infantry combat
    /// </summary>
    internal class AttackNearbyTask : PedTask
    {
        public AttackNearbyTask(Peds.Squad parent, Ped character) : base(parent, character)
        {
        }

        public override void Enter()
        {

        }

        public override bool TransitionState()
        {
            if (!Parent.CheckForCombat(Character))
            {
                // TODO: swap to AttackNearbyTask
                return true;
            }
            if (IsLeader)
            {
                // if there are no waypoints, there's nowhere to go!
                if (Parent.Waypoints.Count == 0)
                {
                    SetTask(new IdleTask(Parent, Character));
                    return true;
                }
            }

            return false;
        }

        public override void Update()
        {
            if (IsLeader)
            {

            }
        }
    }
}
