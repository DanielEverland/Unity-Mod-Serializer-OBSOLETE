using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using UMS.Core;

namespace UMS.Behaviour
{
    public static class BehaviourManager
    {
        public static event Action<BehaviourBase, MemberInfo> OnBehaviourLoadedWithContext;
        public static event Action<BehaviourBase> OnBehaviourLoaded;
        public static event Action<BehaviourBase> OnBehaviourAdded;
        public static event Action OnFinishedInitializing;

        public static bool HasInitialized { get; private set; }

        private static Dictionary<int, BehaviourBase> _loadedBehaviours;

        public static void Initialize()
        {
            _loadedBehaviours = new Dictionary<int, BehaviourBase>();

            AssemblyManager.OnLoadType += Analyze;

            AssemblyManager.OnFinishedReflection += () =>
            {
                HasInitialized = true;
                OnFinishedInitializing?.Invoke();
            };
        }
        public static IEnumerable<BehaviourBase> GetBehaviours<T>() where T : BehaviourBase
        {
            return _loadedBehaviours.Values.Where(x => (typeof(T).IsAssignableFrom(x.GetType())));
        }
        private static void Analyze(Type type)
        {
            foreach (Attribute attribute in type.GetCustomAttributes(false))
            {
                if (!(attribute is BehaviourBase))
                    continue;

                if(attribute is IBehaviourClassLoader loader)
                {
                    loader.Load(type);
                }

                AddBehaviour(attribute as BehaviourBase);
            }

            foreach (PropertyInfo property in type.GetProperties())
            {
                Analyze(property);
            }
            foreach (FieldInfo field in type.GetFields())
            {
                Analyze(field);
            }
            foreach (MethodInfo method in type.GetMethods())
            {
                Analyze(method);
            }
        }
        private static void Analyze<T>(T targetObj) where T : MemberInfo
        {
            foreach (Attribute attribute in targetObj.GetCustomAttributes(false))
            {
                Analyze(attribute, targetObj);
            }
        }
        private static void Analyze<T>(Attribute attribute, T targetObj) where T : MemberInfo
        {
            if (!(attribute is BehaviourBase))
                return;
            
            if (attribute is IBehaviourLoader<T>)
            {
                (attribute as IBehaviourLoader<T>).Load(targetObj);
            }
            if(attribute is IBehaviourLoader)
            {
                (attribute as IBehaviourLoader).Load();
            }

            AddBehaviour(attribute as BehaviourBase, targetObj);
        }
        private static void AddBehaviour<T>(BehaviourBase behaviour, T targetObject) where T : MemberInfo
        {
            if (behaviour == null)
                return;

            AddBehaviour(behaviour);

            if (OnBehaviourLoadedWithContext != null && behaviour != null)
                OnBehaviourLoadedWithContext(behaviour, targetObject);
        }
        private static void AddBehaviour(BehaviourBase behaviour)
        {
            if (behaviour == null)
                return;
            
            bool added = AddBehaviour(_loadedBehaviours, behaviour.GetHashCode(), behaviour);
            
            if (OnBehaviourLoaded != null && behaviour != null)
                OnBehaviourLoaded(behaviour);

            if (added && behaviour != null && OnBehaviourAdded != null)
                OnBehaviourAdded(behaviour);
        }

        /// <summary>
        /// Adds a behaviour to a dictionary using a key. If there's any collision it will use the behaviour with the highest priority, or the one that was added first.
        /// </summary>
        /// <typeparam name="TKey">The type of key to add</typeparam>
        /// <typeparam name="TValue">The type of behaviour to add</typeparam>
        /// <param name="dictionary">The dictionary to add the behaviour to using <paramref name="key"/></param>
        /// <param name="key">The key to use as an index. Two colliding behaviours must share the same key</param>
        /// <param name="behaviour">The behaviour to add</param>
        /// <returns></returns>
        public static bool AddBehaviour<TKey, TValue>(Dictionary<TKey, TValue> dictionary, TKey key, TValue behaviour) where TValue : BehaviourBase
        {
            if (key == null || behaviour == null)
                return false;

            int priority = behaviour.Priority;

            if (dictionary.ContainsKey(key))
            {
                if (dictionary[key].Priority < priority)
                {
                    dictionary[key] = behaviour;
                }                    
            }
            else
            {
                dictionary.Add(key, behaviour);

                return true;
            }

            return false;
        }
    }
}