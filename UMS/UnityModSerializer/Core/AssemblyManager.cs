using System;
using System.IO;
using System.Collections.Generic;
using System.Reflection;
using UnityEngine;

namespace UMS.Core
{
    public static class AssemblyManager
    {
        public const string ASSEMBLY_EDITOR_NAME = "UMS_Editor";
        public const string ASSEMBLY_RUNTIME_NAME = "UMS_Runtime";

        public static Assembly EditorAssembly { get; private set; }
        public static Assembly RuntimeAssembly { get; private set; }

        public static event Action<Type> OnLoadType;
        public static event Action OnFinishedReflection;

        public static IEnumerable<Assembly> LoadedAssemblies { get { return _loadedAssemblies; } }

        private static List<Assembly> _loadedAssemblies;
        private static Dictionary<string, Type> _cachedTypes;

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
            "I18N",
            "I18N.CJK",
            "I18N.MidEast",
            "I18N.Other",
            "I18N.Rare",
            "I18N.West",
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
            _cachedTypes = new Dictionary<string, Type>();

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

            LoadAssemblies(AppDomain.CurrentDomain.GetAssemblies());
#if EDITOR
            LoadAssemblies(GetAssembliesInProject());
#endif
        }
        private static void LoadAssemblies(IEnumerable<Assembly> collection)
        {
            foreach (Assembly assembly in collection)
            {
                if (IsBlocked(assembly))
                    continue;

                if (assembly.GetName().Name == ASSEMBLY_RUNTIME_NAME)
                    RuntimeAssembly = assembly;

                if (assembly.GetName().Name == ASSEMBLY_EDITOR_NAME)
                    EditorAssembly = assembly;
                
                _loadedAssemblies.Add(assembly);
            }
        }
#if EDITOR
        private static IEnumerable<Assembly> GetAssembliesInProject()
        {
            LinkedList<Assembly> toReturn = new LinkedList<Assembly>();
            Queue<string> toCheck = new Queue<string>();
            toCheck.Enqueue(Application.dataPath);

            while (toCheck.Count > 0)
            {
                string currentFolder = toCheck.Dequeue();

                foreach (string file in Directory.GetFiles(currentFolder))
                {
                    if(Path.GetExtension(file) == ".dll")
                    {
                        toReturn.AddLast(Assembly.LoadFile(file));
                    }
                }

                foreach (string subFolder in Directory.GetDirectories(currentFolder))
                {
                    toCheck.Enqueue(subFolder);
                }
            }

            return toReturn;
        }
#endif
        private static bool IsBlocked(Assembly assembly)
        {
            return _blockedAssemblies.Contains(assembly.GetName().Name);
        }
        public static Type GetType(string typeName)
        {
            if (_cachedTypes.ContainsKey(typeName))
                return _cachedTypes[typeName];

            foreach (Assembly assembly in LoadedAssemblies)
            {
                foreach (Type type in assembly.GetTypes())
                {
                    if (type.Name == typeName)
                    {
                        _cachedTypes.Add(typeName, type);

                        return type;
                    }                        
                }
            }

            return null;
        }
    }
}