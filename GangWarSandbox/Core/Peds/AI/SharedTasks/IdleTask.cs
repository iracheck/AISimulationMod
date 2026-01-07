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

        public override void Enter()
        {
            Ped.Task.ClearAllImmediately();
        }

        public override bool TransitionState()
        {
            if (Parent.CheckForCombat(Ped))
            {
                SetTask(new AttackNearbyTask(Parent, Ped));
                return true;
            }

            if (Parent.Waypoints.Count > 0)
            {
                SetTask(new MoveTask(Parent, Ped));
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
