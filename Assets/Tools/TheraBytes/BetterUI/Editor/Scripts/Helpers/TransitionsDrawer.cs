using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEditor;
using UnityEngine;
using UnityEngine.UI;

namespace TheraBytes.BetterUi.Editor
{
    [CustomPropertyDrawer(typeof(Transitions))]
    public class TransitionsDrawer : PropertyDrawer
    {
        Transitions info;

        public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
        {
            return 0;
        }

        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            info = fieldInfo.GetValue(property.serializedObject.targetObject)
                as Transitions;
            
            DrawGui(info, property);
        }
        

        public static void DrawGui(Transitions sel, SerializedProperty property)
        {
            var mode = (Transitions.TransitionMode)EditorGUILayout.EnumPopup("Mode", sel.Mode);
            if(mode != sel.Mode)
            {
                sel.SetMode(mode);
            }

            if (sel.Mode == Transitions.TransitionMode.None)
                return;

            SerializedProperty transitionProp = null;
            List<string> postProps = new List<string>();
            

            switch (mode)
            {
                case Transitions.TransitionMode.ColorTint:
                    
                    transitionProp = property.FindPropertyRelative("colorTransitions");
                    postProps.Add("fadeDuration");
                    break;

                case Transitions.TransitionMode.Color32Tint:

                    transitionProp = property.FindPropertyRelative("color32Transitions");
                    postProps.Add("fadeDuration");
                    break;

                case Transitions.TransitionMode.SpriteSwap:

                    transitionProp = property.FindPropertyRelative("spriteSwapTransitions");
                    break;

                case Transitions.TransitionMode.Animation:

                    transitionProp = property.FindPropertyRelative("animationTransitions");
                    break;

                case Transitions.TransitionMode.ObjectActiveness:

                    transitionProp = property.FindPropertyRelative("activenessTransitions");
                    break;

                case Transitions.TransitionMode.Alpha:

                    transitionProp = property.FindPropertyRelative("alphaTransitions");
                    postProps.Add("fadeDuration");
                    break;

                case Transitions.TransitionMode.MaterialProperty:

                    transitionProp = property.FindPropertyRelative("materialPropertyTransitions");
                    postProps.Add("fadeDuration");
                    break;

            }

            var targetProp = transitionProp.FindPropertyRelative("target");
            EditorGUILayout.PropertyField(targetProp);

            if (sel.TransitionStates.Target != null)
            {
                if(mode == Transitions.TransitionMode.MaterialProperty)
                {
                    DrawMaterialPropertySelector(sel, transitionProp);
                }

                EditorGUI.indentLevel += 1;

                var statesProp = transitionProp.FindPropertyRelative("states");
                for (int i = 0; i < statesProp.arraySize; i++)
                {
                    var p = statesProp.GetArrayElementAtIndex(i);
                    var pName = p.FindPropertyRelative("Name");
                    var pVal = p.FindPropertyRelative("StateObject");

                    EditorGUILayout.PropertyField(pVal, new GUIContent(pName.stringValue));
                }

                EditorGUI.indentLevel -= 1;

                foreach (string pName in postProps)
                {
                    var p = transitionProp.FindPropertyRelative(pName);
                    EditorGUILayout.PropertyField(p);
                }
            }
            
        }

        private static void DrawMaterialPropertySelector(Transitions sel, SerializedProperty transitionProp)
        {
            var matPropTrans = (sel.TransitionStates as MaterialPropertyTransition);
            if (matPropTrans == null)
                return;

            var img = (matPropTrans.Target as BetterImage);
            if (img == null)
                return;

            var options = img.MaterialProperties.FloatProperties.Select(o => o.Name).ToArray();

            var sp = transitionProp.FindPropertyRelative("propertyIndex");
            int cur = sp.intValue;
            int matPropIndex = EditorGUILayout.Popup("Affected Property", cur, options);

            sp.intValue = matPropIndex;
            
        }
    }
}
