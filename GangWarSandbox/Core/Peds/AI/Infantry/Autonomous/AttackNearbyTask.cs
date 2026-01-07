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

        private enum CurrentAttackState
        {
            Initialized,
            Attack,
            RunTo
        }

        CurrentAttackState AttackState = CurrentAttackState.Initialized;

        public AttackNearbyTask(Peds.Squad parent, Ped character) : base(parent, character)
        {
        }

        public override void Enter()
        {
            // Unlike other Tasks, Combat tasks are more time critical-- waiting an extra ~250ms on top of the ~250ms already waited is potentially dangerous.
            // Update();
            
            // commented out to test and see if it matters
        }

        public override bool TransitionState()
        {
            if (!Parent.CheckForCombat(Ped))
            {
                SetTask(new MoveTask(Parent, Ped));
                return true;
            }

            return false;
        }

        public override void Update()
        {
            Ped nearbyEnemy = Parent.PedTargetCache[Ped].enemy;

            if (nearbyEnemy == null) return;

            if (AISubTasks.HasLineOfSight(Ped, nearbyEnemy))
            {
                if (AttackState != CurrentAttackState.Attack)
                {
                    AISubTasks.AttackNearbyEnemies(Ped, GWSettings.AI_ATTACK_RADIUS);
                    AttackState = CurrentAttackState.Attack;
                }
                // Future: Tasks like ThrowGrenade, etc...
            }
            else
            {
                if (AttackState != CurrentAttackState.RunTo)
                {
                    AISubTasks.RunToFarAway(Ped, nearbyEnemy.Position);
                    AttackState = CurrentAttackState.RunTo;
                }
            }
        }
    }
}
