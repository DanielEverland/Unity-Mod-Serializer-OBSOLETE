using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using UMS.Core;

namespace UMS.Behaviour
{
    public static class BehaviourManager
    {
        public static event Action<BehaviourBase, MemberInfo> OnBehaviourLoadedWithContext;
        public static event Action<BehaviourBase> OnBehaviourLoaded;
        public static event Action OnFinishedInitializing;

        public static bool HasInitialized { get; private set; }

        private static List<BehaviourBase> _loadedBehaviours;


        public static void Initialize()
        {
            _loadedBehaviours = new List<BehaviourBase>();
            
            AssemblyManager.OnLoadType += type =>
            {
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
            };            
            
            AssemblyManager.OnFinishedReflection += () =>
            {
                HasInitialized = true;
                OnFinishedInitializing?.Invoke();
            };
        }
        public static IEnumerable<BehaviourBase> GetBehaviours<T>() where T : BehaviourBase
        {
            return _loadedBehaviours.Where(x => (typeof(T).IsAssignableFrom(x.GetType())));
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

            if (attribute is IBehaviourLoader<T> loader)
            {
                loader.Load(targetObj);
            }

            AddBehaviour(attribute as BehaviourBase, targetObj);
        }
        private static void AddBehaviour<T>(BehaviourBase behaviour, T targetObject) where T : MemberInfo
        {
            if (behaviour == null)
                return;

            _loadedBehaviours.Add(behaviour);

            if (OnBehaviourLoaded != null && behaviour != null)
                OnBehaviourLoaded.Invoke(behaviour);

            if (OnBehaviourLoadedWithContext != null && behaviour != null)
                OnBehaviourLoadedWithContext.Invoke(behaviour, targetObject);
        }
        private static void AddBehaviour<TKey, TValue>(Dictionary<TKey, TValue> dictionary, TKey key, TValue behaviour) where TValue : BehaviourBase
        {
            if (key == null || behaviour == null)
                return;

            int priority = behaviour.Priority;

            if (dictionary.ContainsKey(key))
            {
                if (dictionary[key].Priority < priority)
                    dictionary[key] = behaviour;
            }
            else
            {
                dictionary.Add(key, behaviour);
            }
        }
    }
}
