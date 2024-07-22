using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;
using UnityEngine.Serialization;

namespace TheraBytes.BetterUi
{
    [RequireComponent(typeof(RectTransform))]
    [AddComponentMenu("Better UI/Layout/Size Delta Sizer", 30)]
    public class SizeDeltaSizer : ResolutionSizer<Vector2>
    {
        [Serializable]
        public class Settings : IScreenConfigConnection
        {
            public bool ApplyWidth { get { return applyWidth; } set { applyWidth = value; } }
            public bool ApplyHeight { get { return applyHeight; } set { applyHeight = value; } }

            [SerializeField]
            bool applyWidth, applyHeight;
            
            [SerializeField]
            string screenConfigName;
            public string ScreenConfigName { get { return screenConfigName; } set { screenConfigName = value; } }
        }

        [Serializable]
        public class SettingsConfigCollection : SizeConfigCollection<Settings> { }
        public Settings CurrentSettings { get { return customSettings.GetCurrentItem(settingsFallback); } }

        [SerializeField]
        Settings settingsFallback = new Settings();

        [SerializeField]
        SettingsConfigCollection customSettings = new SettingsConfigCollection();


        public Vector2SizeModifier DeltaSizer { get { return customDeltaSizers.GetCurrentItem(deltaSizerFallback); } }


        protected override ScreenDependentSize<Vector2> sizer { get { return customDeltaSizers.GetCurrentItem(deltaSizerFallback); } }
        
        [SerializeField]
        Vector2SizeModifier deltaSizerFallback = new Vector2SizeModifier(100 * Vector2.one, Vector2.zero, 1000 * Vector2.one);

        [SerializeField]
        Vector2SizeConfigCollection customDeltaSizers = new Vector2SizeConfigCollection();


        protected override void ApplySize(Vector2 newSize)
        {
            RectTransform rt = (this.transform as RectTransform);
            Vector2 size = rt.sizeDelta;

            Settings settings = CurrentSettings;

            if(settings.ApplyWidth)
            {
                size.x = newSize.x;
            }

            if(settings.ApplyHeight)
            {
                size.y = newSize.y;
            }

            rt.sizeDelta = size;
        }
    }
}
