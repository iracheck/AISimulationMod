using GangWarSandbox.Peds;
using GTA;
using GTA.Math;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace GangWarSandbox.Peds
{
    /// <summary>
    /// When reaching the target point, capture it. This is done by simply standing there until its done
    /// </summary>
    internal class CaptureTask : PedTask
    {

        public CaptureTask(Peds.Squad parent, Ped character) : base(parent, character)
        {
        }

        public override bool TransitionState()
        {
            if (!IsLeader) return false;

            // if, somehow, the squad got into the position where it got this task but it wasnt supposed to. just a fallback.
            if (Parent.Role != Squad.SquadRole.AssaultCapturePoint)
            {
                SetTask(new MoveTask(Parent, Ped));
                return true;
            }
            else if (Parent.TargetPoint.Owner == Parent.Owner) // if the capture point is captured, we can exit this task.
            {
                SetTask(new MoveTask(Parent, Ped));
                Parent.SetTarget(GangWarSandbox.Instance.CurrentGamemode.GetTarget(Parent));
                return true;
            }
            else return false;
        }

        public override void Update()
        {
            // just sits at the point
        }
    }
}
