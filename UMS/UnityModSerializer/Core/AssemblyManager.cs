using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using UnityEngine;
using UnityEditor;

namespace UMS.Core
{
    public static class AssemblyManager
    {
        public static event Action<Type> OnLoadType;
        public static event Action OnFinishedReflection;

        public static IEnumerable<Assembly> LoadedAssemblies { get { return _loadedAssemblies; } }

        private static List<Assembly> _loadedAssemblies;

        public static void Initialize()
        {
            _loadedAssemblies = new List<Assembly>(GetAssemblies());
        }
        public static void ExecuteReflection()
        {
            if (OnLoadType == null)
                throw new NullReferenceException("No reflection analyzers loaded!");
            
            foreach (Assembly assembly in LoadedAssemblies)
            {
                Debug.Log(assembly);

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
        private static IEnumerable<Assembly> GetAssemblies()
        {
            HashSet<Assembly> _toReturn = new HashSet<Assembly>();

            Assembly unityAssembly = GetUnityAssembly();

            if (unityAssembly != null)
                _toReturn.Add(unityAssembly);
            
            Queue<string> toCheck = new Queue<string>();
            toCheck.Enqueue(Application.dataPath);

            while (toCheck.Count > 0)
            {
                string current = toCheck.Dequeue();

                if (Path.GetFileNameWithoutExtension(current) == "Plugins")
                {
                    GetAssemblies(current, _toReturn);
                }
                else
                {
                    foreach (string subFolder in Directory.GetDirectories(current))
                    {
                        toCheck.Enqueue(subFolder);
                    }
                }
            }

            return _toReturn;
        }
        private static Assembly GetUnityAssembly()
        {
            string assetPath = Application.dataPath;
            string projectPath = Directory.GetParent(assetPath).FullName;
            string fullPath = projectPath + @"\Library\ScriptAssemblies\Assembly-CSharp.dll";

            AssetDatabase.Refresh();

            if (!File.Exists(fullPath))
                return null;

            return Assembly.LoadFile(fullPath);
        }
        private static void GetAssemblies(string path, HashSet<Assembly> collection)
        {
            foreach (string file in Directory.GetFiles(path))
            {
                if (Path.GetExtension(file) == ".dll")
                {
                    Assembly assembly = Assembly.LoadFile(file);

                    if (!collection.Contains(assembly))
                        collection.Add(assembly);
                }
            }

            foreach (string subDirectory in Directory.GetDirectories(path))
            {
                GetAssemblies(subDirectory, collection);
            }
        }
    }
}