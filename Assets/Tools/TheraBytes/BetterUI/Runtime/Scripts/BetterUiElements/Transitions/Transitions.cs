using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using UnityEngine;

namespace TheraBytes.BetterUi
{
    [Serializable]
    public class Transitions
    {
        public static readonly string[] OnOffStateNames = { "On", "Off", };
        public static readonly string[] ShowHideStateNames = { "Show", "Hide", };
        public static readonly string[] SelectionStateNames = { "Normal", "Highlighted", "Pressed", "Disabled", };

        public enum TransitionMode
        {
            None = 0,
            ColorTint = 1,
            SpriteSwap = 2,
            Animation = 3,
            ObjectActiveness = 4,
            Alpha = 5,
            MaterialProperty = 6,
            Color32Tint = 7,
        }

        public TransitionMode Mode { get { return mode; } }
        public ReadOnlyCollection<string> StateNames { get { return stateNames.ToList().AsReadOnly(); } }

        [SerializeField]
        TransitionMode mode;


        [SerializeField]
        string[] stateNames;

        [SerializeField]
        ColorTransitions colorTransitions;
        [SerializeField]
        Color32Transitions color32Transitions;
        [SerializeField]
        SpriteSwapTransitions spriteSwapTransitions;
        [SerializeField]
        AnimationTransitions animationTransitions;
        [SerializeField]
        ObjectActivenessTransitions activenessTransitions;
        [SerializeField]
        AlphaTransitions alphaTransitions;
        [SerializeField]
        MaterialPropertyTransition materialPropertyTransitions;

        public TransitionStateCollection TransitionStates
        {
            get
            {
                switch (mode)
                {
                    case TransitionMode.ColorTint: return colorTransitions;
                    case TransitionMode.Color32Tint: return color32Transitions;
                    case TransitionMode.SpriteSwap: return spriteSwapTransitions;
                    case TransitionMode.Animation: return animationTransitions;
                    case TransitionMode.ObjectActiveness: return activenessTransitions;
                    case TransitionMode.Alpha: return alphaTransitions;
                    case TransitionMode.MaterialProperty: return materialPropertyTransitions;
                    default: return null;
                }
            }
        }

        public Transitions(params string[] stateNames)
        {
            this.stateNames = stateNames;
        }

        public void SetState(string stateName, bool instant)
        {
            if (TransitionStates == null)
                return;

            if (!stateNames.Contains(stateName))
                return;

            TransitionStates.Apply(stateName, instant);
        }

        public void SetMode(TransitionMode mode)
        {
            this.mode = mode;
            
            this.colorTransitions = null;
            this.color32Transitions = null;
            this.spriteSwapTransitions = null;
            this.animationTransitions = null;
            this.activenessTransitions = null;
            this.alphaTransitions = null;

            switch (mode)
            {
                case TransitionMode.None:
                    break;
                case TransitionMode.ColorTint:
                    this.colorTransitions = new ColorTransitions(stateNames);
                    break;
                case TransitionMode.Color32Tint:
                    this.color32Transitions = new Color32Transitions(stateNames);
                    break;
                case TransitionMode.SpriteSwap:
                    this.spriteSwapTransitions = new SpriteSwapTransitions(stateNames);
                    break;
                case TransitionMode.Animation:
                    this.animationTransitions = new AnimationTransitions(stateNames);
                    break;
                case TransitionMode.ObjectActiveness:
                    this.activenessTransitions = new ObjectActivenessTransitions(stateNames);
                    break;
                case TransitionMode.Alpha:
                    this.alphaTransitions = new AlphaTransitions(stateNames);
                    break;
                case TransitionMode.MaterialProperty:
                    this.materialPropertyTransitions = new MaterialPropertyTransition(stateNames);
                    break;
            }
        }
    }

}
