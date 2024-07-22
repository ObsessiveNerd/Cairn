using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEditor;
using UnityEditor.UI;
using UnityEditorInternal;
using UnityEngine;
using UnityEngine.UI;

namespace TheraBytes.BetterUi.Editor
{
    [CustomEditor(typeof(BetterScrollRect))]
    public class BetterScrollRectEditor : ScrollRectEditor
    {
        SerializedProperty hProp, vProp;

        protected override void OnEnable()
        {
            base.OnEnable();

            hProp = serializedObject.FindProperty("horizontalStartPosition");
            vProp = serializedObject.FindProperty("verticalStartPosition");
        }

        public override void OnInspectorGUI()
        {
            base.OnInspectorGUI();

            BetterScrollRect obj = target as BetterScrollRect;

            if(obj.horizontal)
            {
                EditorGUILayout.PropertyField(hProp);

                if(obj.horizontalScrollbar != null)
                {
                    if(GUILayout.Button("From current Horizontal Scrollbar value"))
                    {
                        hProp.floatValue = obj.horizontalScrollbar.value;
                    }
                }

                EditorGUILayout.Separator();
            }

            if(obj.vertical)
            {
                EditorGUILayout.PropertyField(vProp);

                if (obj.verticalScrollbar != null)
                {
                    if (GUILayout.Button("From current Vertical Scrollbar value"))
                    {
                        vProp.floatValue = obj.verticalScrollbar.value;
                    }
                }
            }

            serializedObject.ApplyModifiedProperties();
        }

        [MenuItem("CONTEXT/ScrollRect/♠ Make Better")]
        public static void MakeBetter(MenuCommand command)
        {
            ScrollRect obj = command.context as ScrollRect;
            Betterizer.MakeBetter<ScrollRect, BetterScrollRect>(obj);
        }
    }
}
