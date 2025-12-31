using GangWarSandbox.Peds;
using GTA;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GangWarSandbox.Core.Squad.AI.Infantry.Autonomous
{
    internal class MoveTask : PedTask
    {

        public MoveTask(Peds.Squad parent, Ped character) : base(parent, character)
        {
        }

        public override bool TransitionState()
        {
            if (IsLeader)
            {
                if (Parent.Waypoints.Count == 0)
                {
                    SetTask(new IdleTask(Parent, Character));
                    return true;
                }


            }
            else
            {

            }

            return false;
        }

        public override void Update()
        {
            
        }
    }
}
