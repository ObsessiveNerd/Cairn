using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEditor;
using UnityEngine;
using UnityEngine.UI;

namespace TheraBytes.BetterUi.Editor
{
    [CustomEditor(typeof(BetterVerticalLayoutGroup))]
    public class BetterVerticalLayoutGroupEditor
        : BetterHorizontalOrVerticalLayoutGroupEditor<VerticalLayoutGroup, BetterVerticalLayoutGroup>
    {
        public override void OnInspectorGUI()
        {
            base.DrawObsoleteWarning();
            base.OnInspectorGUI();
        }
    }
}
