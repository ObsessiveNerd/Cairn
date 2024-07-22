using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using UnityEngine;

namespace TheraBytes.BetterUi
{
    [Serializable]
    public class ScreenTypeConditions
    {
        [SerializeField]
        string name = "Screen";

        public string Name
        {
            get { return name; }
            set { this.name = value; }
        }

        [SerializeField]
        IsCertainScreenOrientation checkOrientation;

        [SerializeField]
        IsScreenOfCertainSize checkScreenSize;

        [SerializeField]
        IsCertainAspectRatio checkAspectRatio;

        [SerializeField]
        IsScreenOfCertainDeviceInfo checkDeviceType;

        [SerializeField]
        ScreenInfo optimizedScreenInfo;

        [SerializeField]
        List<string> fallbacks = new List<string>();

        public bool IsActive { get; private set; }

        public List<string> Fallbacks { get { return fallbacks; } }

        public Vector2 OptimizedResolution { get { return (optimizedScreenInfo != null) ? optimizedScreenInfo.Resolution : ResolutionMonitor.OptimizedResolutionFallback; } }

        public int OptimizedWidth { get { return (int)OptimizedResolution.x; } }
        public int OptimizedHeight { get { return (int)OptimizedResolution.y; } }

        public float OptimizedDpi { get { return (optimizedScreenInfo != null) ? optimizedScreenInfo.Dpi : ResolutionMonitor.OptimizedDpiFallback; } }

        public IsCertainScreenOrientation CheckOrientation
        {
            get { return checkOrientation; }
        }

        public IsScreenOfCertainSize CheckScreenSize
        {
            get { return checkScreenSize; }
        }

        public IsCertainAspectRatio CheckAspectRatio
        {
            get { return checkAspectRatio; }
        }

        public IsScreenOfCertainDeviceInfo CheckDeviceType
        {
            get { return checkDeviceType; }
        }

        public ScreenInfo OptimizedScreenInfo
        {
            get { return optimizedScreenInfo; }
        }

        public ScreenTypeConditions(string displayName,
            bool optimizedScreenInfo = false,
            bool orientation = false,
            bool bigger = false,
            bool smaller = false,
            bool touch = false,
            bool vr = false,
            bool deviceType = false)
        { 
            this.name = displayName;
            this.optimizedScreenInfo = new ScreenInfo(new Vector2(1920, 1080), 96);

            this.checkOrientation = new IsCertainScreenOrientation(IsCertainScreenOrientation.Orientation.Landscape)
            { IsActive = orientation };

            this.checkScreenSize = new IsScreenOfCertainSize()
            { IsActive = bigger };

            this.checkAspectRatio = new IsCertainAspectRatio()
            { IsActive = bigger };

            this.checkDeviceType = new IsScreenOfCertainDeviceInfo()
            { IsActive = deviceType };
        }

        public bool IsScreenType()
        {
            IsActive = (!(checkOrientation.IsActive)        || checkOrientation.IsScreenType())
                && (!(checkScreenSize.IsActive)         || checkScreenSize.IsScreenType())
                && (!(checkAspectRatio.IsActive)        || checkAspectRatio.IsScreenType())
                && (!(checkDeviceType.IsActive)         || checkDeviceType.IsScreenType());

            return IsActive;
        }
    }
}
