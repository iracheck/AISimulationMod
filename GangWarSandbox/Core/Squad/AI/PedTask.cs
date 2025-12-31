using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using GangWarSandbox.Peds;
using GTA;
using static GangWarSandbox.Peds.Squad;

namespace GangWarSandbox.Peds
{
    public abstract class PedTask
    {
        readonly protected Squad Parent; // reference to the squad this state (ped) belongs to
        readonly protected Ped Character;
        readonly protected bool IsLeader;

        public PedTask(Squad parent, Ped character)
        {
            this.Parent = parent;
            this.Character = character;
            this.IsLeader = parent.SquadLeader == Character;
        }

        /// <summary>
        /// Occurs immediately after the state is assigned. Useful for assigning initial values or entering "scripted states." **Does not NEED to be implemented.
        /// </summary>
        public virtual void Enter() { }

        /// <summary>
        /// Occurs immediately after the state exits. Useful for resetting values or exiting "scripted states." **Does not NEED to be implemented.
        /// </summary>
        public virtual void Exit() { }

        /// <summary>
        /// Updates the state and executes its actions every tick. 
        /// </summary>
        public abstract void Update();

        /// <summary>
        /// The conditions in which a state transitions to another state. You must include, at the very least, a transition to one other state, or else the AI will be forever stuck within that state.
        /// </summary>
        /// <returns>True if the state transitioned, False if the state remained the same</returns>
        public abstract bool TransitionState();

        public void SetTask(PedTask task)
        {
            
        }
    }
}
