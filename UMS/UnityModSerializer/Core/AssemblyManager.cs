using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using System.Text.RegularExpressions;
using UnityEngine;

namespace UMS.Core
{
    public static class AssemblyManager
    {
        public static event Action<Type> OnLoadType;
        public static event Action OnFinishedReflection;

        public static IEnumerable<Assembly> LoadedAssemblies { get { return _loadedAssemblies; } }

        private static List<Assembly> _loadedAssemblies;
        private static Regex _blockedAssemblies = new Regex(REGEX);

        private const string REGEX = "^(Unity|System|Boo|pdb|I18N|Mono|nunit|ExCSS|Syntax|mscorlib)";

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
                if (_blockedAssemblies.IsMatch(assembly.FullName))
                    continue;
                    
                _loadedAssemblies.Add(assembly);
            }
        }
    }
}