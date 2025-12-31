using GangWarSandbox.Peds;
using GTA;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GangWarSandbox.Peds
{
    internal class IdleTask : PedTask
    {

        public IdleTask(Squad parent, Ped character) : base(parent, character)
        {
        }

        public override bool TransitionState()
        {
            if (Parent.Waypoints.Count > 0)
            {
                SetTask(new MoveTask(Parent, Character));
                return true;
            }

            return false;
        }

        public override void Update()
        {
            if (Parent.Waypoints.Count == 0)
            {
                Parent.SetTarget(GangWarSandbox.Instance.CurrentGamemode.GetTarget(Parent));
            }
        }
    }
}
