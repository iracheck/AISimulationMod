using GangWarSandbox.Peds;
using GTA;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GangWarSandbox.Core.Squad.AI.Infantry.Autonomous
{
    internal class IdleTask : PedTask
    {

        public IdleTask(Peds.Squad parent, Ped character) : base(parent, character)
        {
        }

        public override bool TransitionState()
        {
            if (IsLeader)
            {
                bool hasValidTarget = false;

                if (Parent.Waypoints.Count == 0)
                {
                    Parent.SetTarget(GangWarSandbox.Instance.CurrentGamemode.GetTarget(Parent));
                }

                if (Parent.Waypoints.Count > 0)
                {
                    hasValidTarget = true;
                    // move to 'movement task'
                }

                return hasValidTarget;
            }
            else return false;
        }

        public override void Update()
        {
            
        }
    }
}
