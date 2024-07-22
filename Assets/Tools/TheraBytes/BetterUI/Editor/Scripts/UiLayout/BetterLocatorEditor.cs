using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using UnityEditor;
using UnityEngine;

namespace TheraBytes.BetterUi.Editor
{
    [CustomEditor(typeof(BetterLocator))]
    public class BetterLocatorEditor : UnityEditor.Editor
    {
        private class Styles
        {
            public static readonly GUIStyle lockStyle = EditorStyles.miniButton;
            public static readonly GUIStyle measuringLabelStyle = new GUIStyle("PreOverlayLabel");
            public static readonly GUIContent anchorsContent = new GUIContent("Anchors");
            public static readonly GUIContent anchorMinContent = new GUIContent("Min", "The normalized position in the parent rectangle that the lower left corner is anchored to.");
            public static readonly GUIContent anchorMaxContent = new GUIContent("Max", "The normalized position in the parent rectangle that the upper right corner is anchored to.");
            public static readonly GUIContent positionContent = new GUIContent("Position", "The local position of the rectangle. The position specifies this rectangle's pivot relative to the anchor reference point.");
            public static readonly GUIContent sizeContent = new GUIContent("Size", "The size of the rectangle.");
            public static readonly GUIContent pivotContent = new GUIContent("Pivot", "The pivot point specified in normalized values between 0 and 1. The pivot point is the origin of this rectangle. Rotation and scaling is around this point.");
            public static readonly GUIContent transformScaleContent = new GUIContent("Scale", "The local scaling of this Game Object relative to the parent. This scales everything including image borders and text.");
            public static readonly GUIContent transformRotationContent = new GUIContent("Rotation");
            public static readonly GUIContent transformPositionZContent = new GUIContent("Pos Z", "Distance to offset the rectangle along the Z axis of the parent. The effect is visible if the Canvas uses a perspective camera, or if a parent RectTransform is rotated along the X or Y axis.");
            public static readonly GUIContent X = new GUIContent("X");
            public static readonly GUIContent Y = new GUIContent("Y");
            public static readonly GUIContent Z = new GUIContent("Z");

        }

        static int floatFieldHash = "FloatFieldHash".GetHashCode();

        SerializedProperty transformFallback, transformConfigs;
        BetterLocator locator;

        Dictionary<RectTransformData, bool> anchorExpands = new Dictionary<RectTransformData, bool>();

        bool autoPullFromTransform = true;
        bool autoPushToTransform = false;
        bool pauseAutoPushOnce = false;

        protected virtual void OnEnable()
        {
            this.locator = target as BetterLocator;
            transformFallback = serializedObject.FindProperty("transformFallback");
            transformConfigs = serializedObject.FindProperty("transformConfigs");

            this.locator.OnValidate();
        }

        public override void OnInspectorGUI()
        {
            EditorGUILayout.PrefixLabel("Live Update");
            EditorGUILayout.BeginHorizontal();
            autoPullFromTransform = GUILayout.Toggle(autoPullFromTransform, "↓   Auto-Pull", "ButtonLeft", GUILayout.MinHeight(30));
            autoPushToTransform = GUILayout.Toggle(autoPushToTransform, "↑   Auto-Push", "ButtonRight", GUILayout.MinHeight(30));
            EditorGUILayout.EndHorizontal();

            if (autoPullFromTransform)
            {
                locator.CurrentTransformData.PullFromTransform(locator.transform as RectTransform);
            }

            ScreenConfigConnectionHelper.DrawGui("Rect Transform Override", transformConfigs, ref transformFallback, DrawTransformData);

            if (autoPushToTransform && !(pauseAutoPushOnce))
            {
                locator.CurrentTransformData.PushToTransform(locator.transform as RectTransform);
            }

            pauseAutoPushOnce = false;
        }

        void DrawTransformData(string configName, SerializedProperty prop)
        {
            RectTransformData data = prop.GetValue<RectTransformData>();
            bool isCurrent = locator.CurrentTransformData == data;
            
            //SerializedProperty localPosition = prop.FindPropertyRelative("LocalPosition");
            //SerializedProperty anchoredPosition = prop.FindPropertyRelative("AnchoredPosition");
            //SerializedProperty sizeDelta = prop.FindPropertyRelative("SizeDelta");
            //SerializedProperty anchorMin = prop.FindPropertyRelative("AnchorMin");
            //SerializedProperty anchorMax = prop.FindPropertyRelative("AnchorMax");
            SerializedProperty pivot = prop.FindPropertyRelative("Pivot");
            SerializedProperty rotation = prop.FindPropertyRelative("Rotation");
            SerializedProperty scale = prop.FindPropertyRelative("Scale");

            if (!(anchorExpands.ContainsKey(data)))
            {
                anchorExpands.Add(data, true);
            }

            Rect bounds = EditorGUILayout.BeginVertical("box");


            bool canEdit = !(isCurrent) || !(autoPullFromTransform) || autoPushToTransform;

            EditorGUI.BeginDisabledGroup(isCurrent && autoPullFromTransform);
            if (GUI.Button(new Rect(bounds.position + new Vector2(10, 10), new Vector2(40, 40)), "↓"))
            {
                Undo.RecordObject(locator, "Pull From Rect Transform");
                data.PullFromTransform(locator.transform as RectTransform);
            }
            EditorGUI.EndDisabledGroup();

            EditorGUI.BeginDisabledGroup(!(canEdit) || (isCurrent && autoPushToTransform));
            if (GUI.Button(new Rect(bounds.position + new Vector2(60, 20), new Vector2(40, 40)), "↑"))
            {
                Undo.RecordObject(locator.transform, "Push To Rect Transform");

                data.PushToTransform(locator.transform as RectTransform);
                pauseAutoPushOnce = true;
            }
            EditorGUI.EndDisabledGroup();


            if(!canEdit)
            {
                EditorGUI.BeginDisabledGroup(true);
            }

            SmartPositionAndSizeFields(prop, data);
            SmartAnchorFields(prop, data);
            SmartPivotField(pivot, data);

            EditorGUILayout.Space();
            
            RotationField(rotation, data);
            ScaleField(scale, data);
            base.serializedObject.ApplyModifiedProperties();


            if (!canEdit)
            {
                EditorGUI.EndDisabledGroup();
            }

            EditorGUILayout.EndVertical();
        }


        private void SmartPositionAndSizeFields(SerializedProperty prop, RectTransformData data)
        {
            Rect controlRect = EditorGUILayout.GetControlRect(true, EditorGUIUtility.singleLineHeight * 4f);
            controlRect.height = EditorGUIUtility.singleLineHeight * 2f;

            
            bool equalAnchorX = data.AnchorMin.x == data.AnchorMax.x;
            bool equalAnchorY = data.AnchorMin.y == data.AnchorMax.y;

            // POS X
            Rect columnRect = this.GetColumnRect(controlRect, 0);
            if (equalAnchorX)
            {
                EditorGUI.BeginProperty(columnRect, null, prop.FindPropertyRelative("AnchoredPosition").FindPropertyRelative("x"));
                this.FloatFieldLabelAbove(columnRect, () => data.AnchoredPosition.x, (float val) => data.AnchoredPosition = new Vector2(val, data.AnchoredPosition.y), DrivenTransformProperties.AnchoredPositionX, new GUIContent("Pos X"));

                //this.SetFadingBasedOnControlID(ref data.ChangingPosX, EditorGUIUtility.s_LastControlID);
                EditorGUI.EndProperty();
            }
            else
            {
                EditorGUI.BeginProperty(columnRect, null, prop.FindPropertyRelative("AnchoredPosition").FindPropertyRelative("x"));
                EditorGUI.BeginProperty(columnRect, null, prop.FindPropertyRelative("SizeDelta").FindPropertyRelative("x"));

                this.FloatFieldLabelAbove(columnRect, () => data.OffsetMin.x, 
                    (float val) => data.OffsetMin = new Vector2(val, data.OffsetMin.y), 
                    DrivenTransformProperties.None, new GUIContent("Left"));

                //this.SetFadingBasedOnControlID(ref data.ChangingLeft, EditorGUIUtility.s_LastControlID);
                EditorGUI.EndProperty();
                EditorGUI.EndProperty();
            }

            // POS Y
            columnRect = this.GetColumnRect(controlRect, 1);
            if (equalAnchorY)
            {
                EditorGUI.BeginProperty(columnRect, null, prop.FindPropertyRelative("AnchoredPosition").FindPropertyRelative("y"));
                this.FloatFieldLabelAbove(columnRect, () => data.AnchoredPosition.y, (float val) => data.AnchoredPosition = new Vector2(data.AnchoredPosition.x, val), DrivenTransformProperties.AnchoredPositionY, new GUIContent("Pos Y"));

                //this.SetFadingBasedOnControlID(ref data.ChangingPosY, EditorGUIUtility.s_LastControlID);
                EditorGUI.EndProperty();
            }
            else
            {
                EditorGUI.BeginProperty(columnRect, null, prop.FindPropertyRelative("AnchoredPosition").FindPropertyRelative("y"));
                EditorGUI.BeginProperty(columnRect, null, prop.FindPropertyRelative("SizeDelta").FindPropertyRelative("y"));
                this.FloatFieldLabelAbove(columnRect, () => -data.OffsetMax.y, (float val) => data.OffsetMax = new Vector2(data.OffsetMax.x, -val), DrivenTransformProperties.None, new GUIContent("Top"));

                //this.SetFadingBasedOnControlID(ref data.ChangingTop, EditorGUIUtility.s_LastControlID);
                EditorGUI.EndProperty();
                EditorGUI.EndProperty();
            }

            // POS Z
            columnRect = this.GetColumnRect(controlRect, 2);
            EditorGUI.BeginProperty(columnRect, null, prop.FindPropertyRelative("LocalPosition.z"));
            this.FloatFieldLabelAbove(columnRect, () => data.LocalPosition.z, (float val) => data.LocalPosition = new Vector3(data.LocalPosition.x, data.LocalPosition.y, val), DrivenTransformProperties.AnchoredPositionZ, Styles.transformPositionZContent);
            EditorGUI.EndProperty();
            controlRect.y = controlRect.y + EditorGUIUtility.singleLineHeight * 2f;

            // Size Delta Width
            columnRect = this.GetColumnRect(controlRect, 0);
            if (equalAnchorX)
            {
                EditorGUI.BeginProperty(columnRect, null, prop.FindPropertyRelative("SizeDelta").FindPropertyRelative("x"));
                this.FloatFieldLabelAbove(columnRect, () => data.SizeDelta.x, (float val) => data.SizeDelta = new Vector2(val, data.SizeDelta.y), DrivenTransformProperties.SizeDeltaX, (equalAnchorX ? new GUIContent("Width") : new GUIContent("W Delta")));

                //this.SetFadingBasedOnControlID(ref data.ChangingWidth, EditorGUIUtility.s_LastControlID);
                EditorGUI.EndProperty();
            }
            else
            {
                EditorGUI.BeginProperty(columnRect, null, prop.FindPropertyRelative("AnchoredPosition").FindPropertyRelative("x"));
                EditorGUI.BeginProperty(columnRect, null, prop.FindPropertyRelative("SizeDelta").FindPropertyRelative("x"));
                this.FloatFieldLabelAbove(columnRect, () => -data.OffsetMax.x, (float val) => data.OffsetMax = new Vector2(-val, data.OffsetMax.y), DrivenTransformProperties.None, new GUIContent("Right"));

                //this.SetFadingBasedOnControlID(ref data.ChangingRight, EditorGUIUtility.s_LastControlID);
                EditorGUI.EndProperty();
                EditorGUI.EndProperty();
            }

            // Size Delta Height
            columnRect = this.GetColumnRect(controlRect, 1);
            if (equalAnchorY)
            {
                EditorGUI.BeginProperty(columnRect, null, prop.FindPropertyRelative("SizeDelta").FindPropertyRelative("y"));
                this.FloatFieldLabelAbove(columnRect, () => data.SizeDelta.y, (float val) => data.SizeDelta = new Vector2(data.SizeDelta.x, val), DrivenTransformProperties.SizeDeltaY, (equalAnchorY ? new GUIContent("Height") : new GUIContent("H Delta")));

                //this.SetFadingBasedOnControlID(ref data.ChangingHeight, EditorGUIUtility.s_LastControlID);
                EditorGUI.EndProperty();
            }
            else
            {
                EditorGUI.BeginProperty(columnRect, null, prop.FindPropertyRelative("AnchoredPosition").FindPropertyRelative("y"));
                EditorGUI.BeginProperty(columnRect, null, prop.FindPropertyRelative("SizeDelta").FindPropertyRelative("y"));
                this.FloatFieldLabelAbove(columnRect, () => data.OffsetMin.y, (float val) => data.OffsetMin = new Vector2(data.OffsetMin.x, val), DrivenTransformProperties.None, new GUIContent("Bottom"));

                //this.SetFadingBasedOnControlID(ref data.ChangingBottom, EditorGUIUtility.s_LastControlID);
                EditorGUI.EndProperty();
                EditorGUI.EndProperty();
            }

            columnRect = controlRect;
            columnRect.height = EditorGUIUtility.singleLineHeight;
            columnRect.y = columnRect.y + EditorGUIUtility.singleLineHeight;
            columnRect.yMin = columnRect.yMin - 2f;
            columnRect.xMin = columnRect.xMax - 26f;
            columnRect.x = columnRect.x - columnRect.width;
            //this.BlueprintButton(columnRect);
            //columnRect.x = columnRect.x + columnRect.width;
            //this.RawButton(columnRect);
        }

        private void SmartAnchorFields(SerializedProperty prop, RectTransformData data)
        {
            Rect controlRect = EditorGUILayout.GetControlRect(true, EditorGUIUtility.singleLineHeight * (float)((!anchorExpands[data] ? 1 : 3)));
            controlRect.x += 10;
            controlRect.width -= 10;

            controlRect.height = EditorGUIUtility.singleLineHeight;
            EditorGUI.BeginChangeCheck();
            this.anchorExpands[data] = EditorGUI.Foldout(controlRect, this.anchorExpands[data], Styles.anchorsContent);

            if (EditorGUI.EndChangeCheck())
            {
                EditorPrefs.SetBool("RectTransformEditor.showAnchorProperties", this.anchorExpands[data]);
            }

            if (!this.anchorExpands[data])
                return;

            EditorGUI.indentLevel = EditorGUI.indentLevel + 1;

            controlRect.y = controlRect.y + EditorGUIUtility.singleLineHeight;
            this.Vector2Field(controlRect, 
                () => data.AnchorMin.x, (float val) => data.AnchorMin.x = val, 
                () => data.AnchorMin.y, (float val) => data.AnchorMin.y = val,
                DrivenTransformProperties.AnchorMinX, DrivenTransformProperties.AnchorMinY, 
                prop.FindPropertyRelative("AnchorMin").FindPropertyRelative("x"), prop.FindPropertyRelative("AnchorMin").FindPropertyRelative("y"),
                Styles.anchorMinContent);

            controlRect.y = controlRect.y + EditorGUIUtility.singleLineHeight;
            this.Vector2Field(controlRect,
                () => data.AnchorMax.x, (float val) => data.AnchorMax.x = val, 
                () => data.AnchorMax.y, (float val) => data.AnchorMax.y = val, 
                DrivenTransformProperties.AnchorMaxX, DrivenTransformProperties.AnchorMaxY, 
                prop.FindPropertyRelative("AnchorMax").FindPropertyRelative("x"), prop.FindPropertyRelative("AnchorMax").FindPropertyRelative("y"),
                Styles.anchorMaxContent);

            EditorGUI.indentLevel = EditorGUI.indentLevel - 1;
        }

        private void SmartPivotField(SerializedProperty pivotProp, RectTransformData data)
        {
            Rect controlRect = EditorGUILayout.GetControlRect(new GUILayoutOption[0]);
            controlRect.x += 10;
            controlRect.width -= 10;

            this.Vector2Field(controlRect, 
                () => data.Pivot.x, (float val) => data.Pivot.x = val, 
                () => data.Pivot.y, (float val) => data.Pivot.y = val,
                DrivenTransformProperties.PivotX, DrivenTransformProperties.PivotY,
                pivotProp.FindPropertyRelative("x"), pivotProp.FindPropertyRelative("y"), 
                Styles.pivotContent);
        }

        private Rect GetColumnRect(Rect totalRect, int column)
        {
            totalRect.xMin = totalRect.xMin - 20 + (EditorGUIUtility.labelWidth - 1f);
            Rect rect = totalRect;
            rect.xMin = rect.xMin + ((totalRect.width - 4f) * ((float)column / 3f) + (float)(column * 2));
            rect.width = (totalRect.width - 4f) / 3f;
            return rect;
        }

        private void Vector2Field(Rect position, Func<float> xGetter, Action<float> xSetter, Func<float> yGetter, Action<float> ySetter,
            DrivenTransformProperties xDriven, DrivenTransformProperties yDriven, SerializedProperty xProperty, SerializedProperty yProperty, GUIContent label)
        {
            EditorGUI.PrefixLabel(position, -1, label);
            float lblW = EditorGUIUtility.labelWidth;
            int ident = EditorGUI.indentLevel;
            Rect columnRect = this.GetColumnRect(position, 0);
            Rect rect = this.GetColumnRect(position, 1);
            EditorGUIUtility.labelWidth = 13f;
            EditorGUI.indentLevel = 0;

            EditorGUI.BeginProperty(columnRect, Styles.X, xProperty);
            this.FloatField(columnRect, xGetter, xSetter, xDriven, Styles.X);
            EditorGUI.EndProperty();

            EditorGUI.BeginProperty(columnRect, Styles.Y, yProperty);
            this.FloatField(rect, yGetter, ySetter, yDriven, Styles.Y);
            EditorGUI.EndProperty();

            EditorGUIUtility.labelWidth = lblW;
            EditorGUI.indentLevel = ident;
        }

        private void ScaleField(SerializedProperty scaleProp, RectTransformData data)
        {
            Rect controlRect = EditorGUILayout.GetControlRect(new GUILayoutOption[0]);
            controlRect.x += 10;
            controlRect.width -= 10;

            EditorGUI.PrefixLabel(controlRect, -1, Styles.transformScaleContent);
            float lblW = EditorGUIUtility.labelWidth;
            int ident = EditorGUI.indentLevel;
            Rect rectX = this.GetColumnRect(controlRect, 0);
            Rect rectY = this.GetColumnRect(controlRect, 1);
            Rect rectZ = this.GetColumnRect(controlRect, 2);
            EditorGUIUtility.labelWidth = 13f;
            EditorGUI.indentLevel = 0;

            EditorGUI.BeginProperty(rectX, Styles.X, scaleProp.FindPropertyRelative("x"));
            this.FloatField(rectX, () => data.Scale.x, (val) => data.Scale.x = val, DrivenTransformProperties.ScaleX, Styles.X);
            EditorGUI.EndProperty();

            EditorGUI.BeginProperty(rectX, Styles.Y, scaleProp.FindPropertyRelative("y"));
            this.FloatField(rectY, () => data.Scale.y, (val) => data.Scale.y = val, DrivenTransformProperties.ScaleY, Styles.Y);
            EditorGUI.EndProperty();


            EditorGUI.BeginProperty(rectX, Styles.Z, scaleProp.FindPropertyRelative("z"));
            this.FloatField(rectZ, () => data.Scale.z, (val) => data.Scale.z = val, DrivenTransformProperties.ScaleZ, Styles.Z);
            EditorGUI.EndProperty();

            EditorGUIUtility.labelWidth = lblW;
            EditorGUI.indentLevel = ident;
        }

        private void RotationField(SerializedProperty rotationProp, RectTransformData data)
        {
            Rect controlRect = EditorGUILayout.GetControlRect(new GUILayoutOption[0]);
            controlRect.x += 10;
            controlRect.width -= 10;

            EditorGUI.PrefixLabel(controlRect, -1, Styles.transformRotationContent);
            float lblW = EditorGUIUtility.labelWidth;
            int ident = EditorGUI.indentLevel;
            Rect rectX = this.GetColumnRect(controlRect, 0);
            Rect rectY = this.GetColumnRect(controlRect, 1);
            Rect rectZ = this.GetColumnRect(controlRect, 2);
            EditorGUIUtility.labelWidth = 13f;
            EditorGUI.indentLevel = 0;

            EditorGUI.BeginProperty(controlRect, GUIContent.none, rotationProp);
            Vector3 euler = data.Rotation.eulerAngles;

            this.FloatField(rectX, () => data.Rotation.eulerAngles.x, (val) => data.Rotation.eulerAngles = new Vector3(val, euler.y, euler.z), DrivenTransformProperties.Rotation, Styles.X);

            this.FloatField(rectY, () => data.Rotation.eulerAngles.y, (val) => data.Rotation.eulerAngles = new Vector3(euler.x, val, euler.z), DrivenTransformProperties.Rotation, Styles.Y);

            this.FloatField(rectZ, () => data.Rotation.eulerAngles.z, (val) => data.Rotation.eulerAngles = new Vector3(euler.x, euler.y, val), DrivenTransformProperties.Rotation, Styles.Z);

            EditorGUI.EndProperty();

            EditorGUIUtility.labelWidth = lblW;
            EditorGUI.indentLevel = ident;
        }

        private void FloatFieldLabelAbove(Rect position, Func<float> getter, Action<float> setter, DrivenTransformProperties driven, GUIContent label)
        {
            FloatField(position, getter, setter, driven, label, true);
        }

        private void FloatField(Rect position, Func<float> getter, Action<float> setter, DrivenTransformProperties driven, GUIContent label, bool labelAbove = false)
        {
            float val = getter();

            EditorGUI.BeginChangeCheck();

            float newVal;
            if (labelAbove)
            {
                int controlID = GUIUtility.GetControlID(BetterLocatorEditor.floatFieldHash, FocusType.Keyboard, position);

                Rect rect = new Rect(position.x, position.y, position.width, EditorGUIUtility.singleLineHeight);
                Rect rect1 = new Rect(position.x, position.y + EditorGUIUtility.singleLineHeight, position.width, EditorGUIUtility.singleLineHeight);
                EditorGUI.HandlePrefixLabel(position, rect, label, controlID);
                
                EditorGUI.PrefixLabel(rect, label);
                newVal = EditorGUI.FloatField(rect1, val);
            }
            else
            {
                newVal = EditorGUI.FloatField(position, label, val);
            }


            if (EditorGUI.EndChangeCheck())
            {
                Undo.RecordObject(base.target, "Inspector");
                setter(newVal);
            }
        }

        [MenuItem("CONTEXT/RectTransform/♠ Add Better Locator", false)]
        public static void AddBetterLocator(MenuCommand command)
        {
            var ctx = command.context as RectTransform;
            var locator = ctx.gameObject.AddComponent<BetterLocator>();

            while(UnityEditorInternal.ComponentUtility.MoveComponentUp(locator))
            { }
        }

        [MenuItem("CONTEXT/RectTransform/♠ Add Better Locator", true)]
        public static bool CheckBetterLocator(MenuCommand command)
        {
            var ctx = command.context as RectTransform;
            return ctx.gameObject.GetComponent<BetterLocator>() == null;
        }
    }
    
}
