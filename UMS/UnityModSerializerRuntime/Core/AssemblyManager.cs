using System;
using System.Collections.Generic;
using System.Reflection;
using UnityEngine;

namespace UMS.Runtime.Core
{
    public static class AssemblyManager
    {
        public static event Action<Type> OnLoadType;
        public static event Action OnFinishedReflection;

        public static IEnumerable<Assembly> LoadedAssemblies { get { return _loadedAssemblies; } }

        private static List<Assembly> _loadedAssemblies;

        #region BlockedAssemblies
        //Obviously not an ideal solution, but I prefer this over getting false positives using RegEx.
        private static readonly HashSet<string> _blockedAssemblies = new HashSet<string>()
        {
            "pdb2mdb",
            "UnityScript.Lang",
            "Boo.Lang.Parser",
            "Boo.Lang",
            "Boo.Lang.Compiler",
            "Mono.Data.Tds",
            "System.Transactions",
            "System.Data",
            "System.Runtime.Serialization",
            "SyntaxTree.VisualStudio.Unity.Messaging",
            "UnityEditor.iOS.Extensions.Xcode",
            "Unity.IvyParser",
            "Unity.Legacy.NRefactory",
            "UnityScript",
            "Unity.SerializationLogic",
            "Unity.Cecil",
            "System.Xml.Linq",
            "ExCSS.Unity",
            "UnityEngine.StandardEvents",
            "UnityEditor.Purchasing",
            "UnityEngine.Purchasing",
            "UnityEditor.Analytics",
            "UnityEngine.Analytics",
            "UnityEditor.Advertisements",
            "UnityEngine.Advertisements",
            "SyntaxTree.VisualStudio.Unity.Bridge",
            "UnityEditor.WindowsStandalone.Extensions",
            "UnityEditor.Graphs",
            "UnityEditor.VR",
            "UnityEngine.SpatialTracking",
            "UnityEditor.SpatialTracking",
            "UnityEngine.HoloLens",
            "UnityEditor.HoloLens",
            "UnityEngine.GoogleAudioSpatializer",
            "UnityEditor.GoogleAudioSpatializer",
            "UnityEditor.UIAutomation",
            "UnityEngine.UIAutomation",
            "UnityEditor.TreeEditor",
            "UnityEditor.Timeline",
            "UnityEngine.Timeline",
            "nunit.framework",
            "UnityEngine.TestRunner",
            "UnityEditor.TestRunner",
            "UnityEditor.Networking",
            "UnityEngine.Networking",
            "UnityEditor.UI",
            "UnityEngine.UI",
            "Unity.PackageManager",
            "Unity.DataContract",
            "System.Core",
            "Mono.Security",
            "System.Xml",
            "System.Configuration",
            "System",
            "I18N.West",
            "I18N",
            "Unity.Locator",
            "UnityEditor",
            "UnityEngine.WindModule",
            "UnityEngine.VideoModule",
            "UnityEngine.UnityWebRequestWWWModule",
            "UnityEngine.UnityWebRequestTextureModule",
            "UnityEngine.UnityWebRequestAudioModule",
            "UnityEngine.UnityWebRequestModule",
            "UnityEngine.TilemapModule",
            "UnityEngine.TerrainModule",
            "UnityEngine.SpriteShapeModule",
            "UnityEngine.SpriteMaskModule",
            "UnityEngine.SharedInternalsModule",
            "UnityEngine.ScreenCaptureModule",
            "UnityEngine.Physics2DModule",
            "UnityEngine.ParticlesLegacyModule",
            "UnityEngine.JSONSerializeModule",
            "UnityEngine.InputModule",
            "UnityEngine.ImageConversionModule",
            "UnityEngine.GridModule",
            "UnityEngine.GameCenterModule",
            "UnityEngine.CrashReportingModule",
            "UnityEngine.AudioModule",
            "UnityEngine.AssetBundleModule",
            "UnityEngine.StyleSheetsModule",
            "UnityEngine.UIElementsModule",
            "UnityEngine.VRModule",
            "UnityEngine.ARModule",
            "UnityEngine.WebModule",
            "UnityEngine.UnityConnectModule",
            "UnityEngine.PerformanceReportingModule",
            "UnityEngine.UnityAnalyticsModule",
            "UnityEngine.DirectorModule",
            "UnityEngine.UNETModule",
            "UnityEngine.ClusterRendererModule",
            "UnityEngine.ClusterInputModule",
            "UnityEngine.IMGUIModule",
            "UnityEngine.TerrainPhysicsModule",
            "UnityEngine.UIModule",
            "UnityEngine.TextRenderingModule",
            "UnityEngine.AnimationModule",
            "UnityEngine.AIModule",
            "UnityEngine.ClothModule",
            "UnityEngine.VehiclesModule",
            "UnityEngine.PhysicsModule",
            "UnityEngine.ParticleSystemModule",
            "UnityEngine.AccessibilityModule",
            "UnityEngine.CoreModule",
            "UnityEngine",
            "mscorlib",
        };
        #endregion
        
        public static void Initialize()
        {
            GetAssemblies();
        }
        public static void ExecuteReflection()
        {
            if (OnLoadType == null)
                throw new NullReferenceException("No reflection analyzers loaded!");
            
            foreach (Assembly assembly in LoadedAssemblies)
            {
                try
                {
                    foreach (Type type in assembly.GetTypes())
                    {
                        OnLoadType(type);
                    }
                }
                catch (ReflectionTypeLoadException)
                {
                    Debug.LogError("Cannot load types in " + assembly);
                    throw;
                }
                catch (Exception)
                {
                    throw;
                }
            }

            OnFinishedReflection?.Invoke();
        }
        private static void GetAssemblies()
        {
            _loadedAssemblies = new List<Assembly>();

            foreach (Assembly assembly in AppDomain.CurrentDomain.GetAssemblies())
            {
                if (IsBlocked(assembly))
                    continue;

                _loadedAssemblies.Add(assembly);
            }
        }
        private static bool IsBlocked(Assembly assembly)
        {
            return _blockedAssemblies.Contains(assembly.GetName().Name);
        }
    }
}