using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEditor;
using UnityEngine;
using UnityEngine.UI;

namespace TheraBytes.BetterUi.Editor
{
    public abstract class BetterHorizontalOrVerticalLayoutGroupEditor<TSource, TBetter> : UnityEditor.Editor
        where TSource : HorizontalOrVerticalLayoutGroup
        where TBetter : TSource, IBetterHorizontalOrVerticalLayoutGroup
    {
        bool paddingFoldout;
        GridLayoutGroup.Corner startCornerVal;

        private SerializedProperty paddingFallback;
        private SerializedProperty paddingConfigs;
        private SerializedProperty spacingFallback;
        private SerializedProperty spacingConfigs;
        protected SerializedProperty settingsFallback;
        protected SerializedProperty settingsConfigs;
        

        protected static TBetter MakeBetterLogic(MenuCommand command)
        {
            TSource lg = command.context as TSource;
            var pad = new Margin(lg.padding);
            var space = lg.spacing;

            var newLg = Betterizer.MakeBetter<TSource, TBetter>(lg, "m_Padding");

            var betterLg = newLg as TBetter;
            if (betterLg != null)
            {
                betterLg.PaddingSizer.SetSize(pad);
                betterLg.SpacingSizer.SetSize(space);
            }
            else if(newLg != null)
            {
                pad.CopyValuesTo(newLg.padding);
            }

            Betterizer.Validate(newLg);

            return newLg as TBetter;
        }

        protected virtual void OnEnable()
        {
            this.paddingFallback = base.serializedObject.FindProperty("paddingSizerFallback");
            this.paddingConfigs = base.serializedObject.FindProperty("customPaddingSizers");
            this.spacingFallback = base.serializedObject.FindProperty("spacingSizerFallback");
            this.spacingConfigs = base.serializedObject.FindProperty("customSpacingSizers");
            this.settingsFallback = base.serializedObject.FindProperty("settingsFallback");
            this.settingsConfigs = base.serializedObject.FindProperty("customSettings");
        }

        public override void OnInspectorGUI()
        {
            DrawDefaultPaddingAndSpacing();
            DrawSettings("", null);
            
            serializedObject.ApplyModifiedProperties();
        }

        protected void DrawObsoleteWarning()
        {
            EditorGUILayout.HelpBox(
@"Component is obsolete!
Better Horizontal- and Better Vertical Layout Groups only exist for backwards compatibility.
Please use 'Better Axis Aligned Layout Group' instead.
To do so, just right click on the component and select '♠ Make Better' as usual.", MessageType.Warning);

        }

        protected void DrawDefaultPaddingAndSpacing()
        {
            EditorGUILayout.LabelField("Padding", EditorStyles.boldLabel);
            EditorGUILayout.PropertyField(paddingFallback);
            EditorGUILayout.LabelField("Spacing", EditorStyles.boldLabel);
            EditorGUILayout.PropertyField(spacingFallback);
        }

        protected void DrawPaddingAndSpacingConfigurations()
        {
            ScreenConfigConnectionHelper.DrawSizerGui("Padding", paddingConfigs, ref paddingFallback);
            ScreenConfigConnectionHelper.DrawSizerGui("Spacing", spacingConfigs, ref spacingFallback);
        }

        protected void DrawSettings(string configName, SerializedProperty parent)
        {
            Func<string, string, SerializedProperty> findProp = (nameOrig, nameSetting) =>
            {
                return (parent == null)
                    ? serializedObject.FindProperty(nameOrig)
                    : parent.FindPropertyRelative(nameSetting);
            };

            var oriantation = findProp("orientation", "Orientation");

            var childAlignment = findProp("m_ChildAlignment", "ChildAlignment");
          
            var childControlSizeWidth = findProp("m_ChildControlWidth", "ChildControlWidth");
            var childControlSizeHeight = findProp("m_ChildControlHeight", "ChildControlHeight");

            var childForceExpandWidth = findProp("m_ChildForceExpandWidth", "ChildForceExpandWidth");
            var childForceExpandHeight = findProp("m_ChildForceExpandHeight", "ChildForceExpandHeight");

            if(parent != null)
            {
                EditorGUILayout.BeginVertical("box");
            }

            if (oriantation != null)
            {
                EditorGUILayout.PropertyField(oriantation);
            }

            EditorGUILayout.PropertyField(childAlignment, true);

            float labelWidth = EditorGUIUtility.labelWidth;

            var version = UnityEditorInternal.InternalEditorUtility.GetUnityVersion();
            if (version >= new Version(5, 5))
            {
                Rect controlSizeRect = EditorGUILayout.GetControlRect();
                controlSizeRect = EditorGUI.PrefixLabel(controlSizeRect, -1, new GUIContent("Child Control Size"));
                controlSizeRect.width = Mathf.Max(50f, (controlSizeRect.width - 4f) / 3f);
                EditorGUIUtility.labelWidth = 50f;
                ToggleLeft(controlSizeRect, childControlSizeWidth, new GUIContent("Width"));
                controlSizeRect.x = controlSizeRect.x + (controlSizeRect.width + 2f);
                ToggleLeft(controlSizeRect, childControlSizeHeight, new GUIContent("Height"));

                EditorGUIUtility.labelWidth = labelWidth;
            }

            Rect forceExpandRect = EditorGUILayout.GetControlRect();
            forceExpandRect = EditorGUI.PrefixLabel(forceExpandRect, -2, new GUIContent("Child Force Expand"));
            forceExpandRect.width = Mathf.Max(50f, (forceExpandRect.width - 4f) / 3f);
            EditorGUIUtility.labelWidth = 50f;
            ToggleLeft(forceExpandRect, childForceExpandWidth, new GUIContent("Width"));
            forceExpandRect.x = forceExpandRect.x + (forceExpandRect.width + 2f);
            ToggleLeft(forceExpandRect, childForceExpandHeight, new GUIContent("Height"));


            if (parent != null)
            {
                EditorGUILayout.EndVertical();
            }

            EditorGUIUtility.labelWidth = labelWidth;
        }


        private static void ToggleLeft(Rect position, SerializedProperty property, GUIContent label)
        {
            bool flag = property.boolValue;
            EditorGUI.showMixedValue = property.hasMultipleDifferentValues;
            EditorGUI.BeginChangeCheck();

            int num = EditorGUI.indentLevel;
            EditorGUI.indentLevel = 0;
            flag = EditorGUI.ToggleLeft(position, label, flag);
            EditorGUI.indentLevel = num;
            if (EditorGUI.EndChangeCheck())
            {
                property.boolValue = (!property.hasMultipleDifferentValues ? !property.boolValue : true);
            }

            EditorGUI.showMixedValue = false;
        }

    }
}
