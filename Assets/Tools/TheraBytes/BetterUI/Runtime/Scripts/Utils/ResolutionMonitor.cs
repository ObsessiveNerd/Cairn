using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Serialization;

#pragma warning disable 0618 // disable "never assigned" warnings

namespace TheraBytes.BetterUi
{
    public class ResolutionMonitor : SingletonScriptableObject<ResolutionMonitor>
    {
        static string FilePath { get { return "TheraBytes/Resources/ResolutionMonitor"; } }

        #region Obsolete
        [Obsolete("Use 'GetOptimizedResolution()' instead.")]
        public static Vector2 OptimizedResolution
        {
            get { return ResolutionMonitor.Instance.optimizedResolutionFallback; }
            set
            {
                if (ResolutionMonitor.Instance.optimizedResolutionFallback == value)
                    return;

                ResolutionMonitor.Instance.optimizedResolutionFallback = value;
                CallResolutionChanged();
            }
        }

        [Obsolete("Use 'GetOptimizedDpi()' instead.")]
        public static float OptimizedDpi
        {
            get { return ResolutionMonitor.Instance.optimizedDpiFallback; }
            set
            {
                if (ResolutionMonitor.Instance.optimizedDpiFallback == value)
                    return;
                
                ResolutionMonitor.Instance.optimizedDpiFallback = value;
                CallResolutionChanged();
            }
        }
        #endregion

        public static Vector2 CurrentResolution
        {
            get
            {
                if (lastScreenResolution == Vector2.zero)
                {
                    lastScreenResolution = new Vector2(Screen.width, Screen.height);
                }

                return lastScreenResolution;
            }
        }

        public static float CurrentDpi
        {
            get
            {
                if (lastDpi == 0)
                {
                    lastDpi = Instance.dpiManager.GetDpi();
                }

                return lastDpi;
            }
        }

        public string FallbackName { get { return fallbackName; } set { fallbackName = value; } }

        public static Vector2 OptimizedResolutionFallback { get { return ResolutionMonitor.Instance.optimizedResolutionFallback; } }
        public static float OptimizedDpiFallback { get { return ResolutionMonitor.Instance.optimizedDpiFallback; } }

        [FormerlySerializedAs("optimizedResolution")]
        [SerializeField]
        Vector2 optimizedResolutionFallback = new Vector2(1080, 1920);

        [FormerlySerializedAs("optimizedDpi")]
        [SerializeField]
        float optimizedDpiFallback = 96;

        [SerializeField]
        string fallbackName = "Portrait";

        [SerializeField]
        DpiManager dpiManager = new DpiManager();

        ScreenTypeConditions currentScreenConfig;

        [SerializeField]
        List<ScreenTypeConditions> optimizedScreens = new List<ScreenTypeConditions>()
        {
            new ScreenTypeConditions("Landscape", optimizedScreenInfo: true, orientation: true),
        };

        public List<ScreenTypeConditions> OptimizedScreens { get { return optimizedScreens; } }

        static Dictionary<string, ScreenTypeConditions> lookUpScreens = new Dictionary<string, ScreenTypeConditions>();

        public static ScreenTypeConditions CurrentScreenConfiguration
        {
            get
            {
#if UNITY_EDITOR
                if(simulatedScreenConfig != null)
                {
                    return simulatedScreenConfig;
                }
#endif
                return ResolutionMonitor.Instance.currentScreenConfig;
            }
        }

        public static ScreenTypeConditions GetConfig(string name)
        {
            if(lookUpScreens.Count == 0)
            {
                foreach(var config in ResolutionMonitor.Instance.optimizedScreens)
                {
                    lookUpScreens.Add(config.Name, config);
                }
            }

            if(!(lookUpScreens.ContainsKey(name)))
            {
                Debug.LogErrorFormat("Couldn't find Screen Config with name '{0}'", name);
                return null;
            }
            
            return lookUpScreens[name];
        }

        public static ScreenInfo GetOpimizedScreenInfo(string name)
        {
            if(string.IsNullOrEmpty(name))
            {
                return new ScreenInfo(OptimizedResolutionFallback, OptimizedDpiFallback);
            }

            return GetConfig(name).OptimizedScreenInfo;
        }


        public static IEnumerable<ScreenTypeConditions> GetCurrentScreenConfigurations()
        {
            foreach(ScreenTypeConditions config in ResolutionMonitor.Instance.optimizedScreens)
            {
                if (config.IsActive)
                    yield return config;
            }
        }


        static Vector2 lastScreenResolution;
        static float lastDpi;

        static bool isDirty;

#if UNITY_EDITOR
        static Type gameViewType = null;
        static UnityEditor.EditorWindow gameViewWindow = null;
        static Version unityVersion;

        static ScreenTypeConditions simulatedScreenConfig;
        public static ScreenTypeConditions SimulatedScreenConfig
        {
            get { return simulatedScreenConfig; }
            set
            {
                if (simulatedScreenConfig != value)
                    isDirty = true;

                simulatedScreenConfig = value;
            }
        }
        

        void OnEnable()
        {
            unityVersion = UnityEditorInternal.InternalEditorUtility.GetUnityVersion();

            if (HasInstance)
            {
                ResolutionChanged();
            }
            else
            {
                isDirty = true;
            }

            UnityEditor.EditorApplication.update += Update;

#if UNITY_5_6_OR_NEWER
            UnityEditor.SceneManagement.EditorSceneManager.sceneOpened += SceneOpened;
#endif

#if UNITY_2018_0_OR_NEWER
            UnityEditor.EditorApplication.playModeStateChanged += PlayModeStateChanged;
#else
            UnityEditor.EditorApplication.playmodeStateChanged += PlayModeStateChanged;
#endif
        }



        void OnDisable()
        {
            UnityEditor.EditorApplication.update -= Update;

#if UNITY_5_6_OR_NEWER
            UnityEditor.SceneManagement.EditorSceneManager.sceneOpened -= SceneOpened;
#endif

#if UNITY_2018_0_OR_NEWER
            UnityEditor.EditorApplication.playModeStateChanged -= PlayModeStateChanged;
#else
            UnityEditor.EditorApplication.playmodeStateChanged -= PlayModeStateChanged;
#endif
        }


#if UNITY_5_6_OR_NEWER
        private void SceneOpened(UnityEngine.SceneManagement.Scene scene, UnityEditor.SceneManagement.OpenSceneMode mode)
        {
            isDirty = true;
            Update();
        }
#endif


        private void PlayModeStateChanged()
        {
            ResolutionChanged();
        }
#endif


        public static new void SetDirty()
        {
            ResolutionMonitor.isDirty = true;
        }  

        public static float GetOptimizedDpi(string screenName)
        {
            if (string.IsNullOrEmpty(screenName) || screenName == Instance.fallbackName)
                return OptimizedDpiFallback;

            var s = Instance.optimizedScreens.FirstOrDefault(o => o.Name == screenName);
            if(s == null)
            {
                Debug.LogError("Screen Config with name " + screenName + " could not be found.");
                return OptimizedDpiFallback;
            }

            return s.OptimizedDpi;
        }


        public static Vector2 GetOptimizedResolution(string screenName)
        {
            if (string.IsNullOrEmpty(screenName) || screenName == Instance.fallbackName)
                return OptimizedResolutionFallback;

            var s = GetConfig(screenName);
            if (s == null)
                return OptimizedResolutionFallback;

            return s.OptimizedResolution;
        }

        public static bool IsOptimizedResolution(int width, int height)
        {
            if ((int)OptimizedResolutionFallback.x == width && (int)OptimizedResolutionFallback.y == height)
                return true;

            foreach(var config in Instance.optimizedScreens)
            {
                ScreenInfo si = config.OptimizedScreenInfo;
                if (si != null && (int)si.Resolution.x == width && (int)si.Resolution.y == height)
                    return true;
            }

            return false;
        }

        public static void Update()
        {
            isDirty = isDirty
#if UNITY_EDITOR // should never change in reality...
                || (GetCurrentDpi() != lastDpi)
#endif
                || GetCurrentResolution() != lastScreenResolution;

            if (isDirty)
            {
                CallResolutionChanged();
                isDirty = false;
            }
        }

        public static void CallResolutionChanged()
        {
            Instance.ResolutionChanged();
        }

        public void ResolutionChanged()
        { 
            lastScreenResolution = GetCurrentResolution();
            lastDpi = GetCurrentDpi();

            currentScreenConfig = null;

            bool foundConfig = false;
            foreach (var config in optimizedScreens)
            {
                if(config.IsScreenType() && !(foundConfig))
                {
                    currentScreenConfig = config;
                    foundConfig = true;
                }
            }

            foreach (IResolutionDependency rd in AllResolutionDependencies())
            {
                rd.OnResolutionChanged();
            }

//#if UNITY_EDITOR
//            if (IsZoomPossible())
//            {
//                EnsureGameViewReference();
                
//                var method = gameViewType.GetMethod("UpdateZoomAreaAndParent",
//                System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.NonPublic);

//                try
//                {
//                    if(method != null && gameViewWindow != null)
//                        method.Invoke(gameViewWindow, null);
//                }
//                catch (Exception) { }
//            }
//#endif
        }

        static IEnumerable<IResolutionDependency> AllResolutionDependencies()
        {
            var allObjects = GameObject.FindObjectsOfType<GameObject>();
            foreach (GameObject go in allObjects)
            {
                var resDeps = go.GetComponents<Component>().OfType<IResolutionDependency>();
                foreach (IResolutionDependency comp in resDeps)
                {
                    yield return comp;
                }
            }
        }


        static Vector2 GetCurrentResolution()
        {
//#if UNITY_EDITOR
//            EnsureGameViewReference();

//            System.Reflection.MethodInfo GetSizeOfMainGameView = gameViewType.GetMethod("GetSizeOfMainGameView",
//                System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Static);

//            object res = GetSizeOfMainGameView.Invoke(null, null);
//            return (Vector2)res;
//#else
            return new Vector2(Screen.width, Screen.height);
//#endif
        }

        static float GetCurrentDpi()
        {
//#if UNITY_EDITOR
            
//            if (IsZoomPossible())
//            {
//                EnsureGameViewReference();

//                var zoomArea = gameViewType.GetField("m_ZoomArea",
//                    System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.NonPublic)
//                    .GetValue(gameViewWindow);

//                Vector2 scale = (Vector2)zoomArea.GetType().GetField("m_Scale",
//                    System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.NonPublic)
//                    .GetValue(zoomArea);

//                return Screen.dpi / scale.y;
//            }
//#endif
            return Instance.dpiManager.GetDpi();
        }

#if UNITY_EDITOR
        //static void EnsureGameViewReference()
        //{
        //    if(gameViewType == null)
        //    {
        //        gameViewType = Type.GetType("UnityEditor.GameView,UnityEditor");
        //    }

        //    if(gameViewWindow == null)
        //    {                
        //        gameViewWindow = UnityEditor.EditorWindow.GetWindowWithRect(gameViewType, new Rect(0,0, 500, 500));
        //    }
        //}

        public static bool IsZoomPossible()
        {
            return unityVersion.Major > 5
                || (unityVersion.Major == 5 && unityVersion.Minor >= 4);
        }
#endif
    }
}
