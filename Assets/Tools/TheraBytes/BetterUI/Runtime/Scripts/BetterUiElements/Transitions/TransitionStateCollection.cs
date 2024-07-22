using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace TheraBytes.BetterUi
{
    //
    // GENERIC CLASS
    //
    public abstract class TransitionStateCollection<T> : TransitionStateCollection
    {
        [Serializable]
        public abstract class TransitionState : TransitionStateBase
        {
            public T StateObject;

            public TransitionState(string name, T stateObject)
                : base(name)
            {
                this.StateObject = stateObject;
            }
        }

        protected TransitionStateCollection(string[] stateNames)
        {
            foreach (string name in stateNames)
            {
                AddStateObject(name);
            }
        }

        public IEnumerable<TransitionState> GetStates()
        {
            foreach (var s in GetTransitionStates())
            {
                yield return s;
            }
        }

        public override void Apply(string stateName, bool instant)
        {
            var s = GetTransitionStates().FirstOrDefault((o) => o.Name == stateName);
            if (s != null)
            {
                ApplyState(s, instant);
            }
        }

        protected abstract IEnumerable<TransitionState> GetTransitionStates();
        protected abstract void ApplyState(TransitionState state, bool instant);
        protected abstract void AddStateObject(string stateName);

    }

    //
    // NON GENERIC CLASS
    //
    [Serializable]
    public abstract class TransitionStateCollection
    {
        public abstract UnityEngine.Object Target { get; }

        [Serializable]
        public abstract class TransitionStateBase
        {
            public string Name;
            public TransitionStateBase(string name)
            {
                this.Name = name;
            }
        }

        public abstract void Apply(string stateName, bool instant);
    }

}
