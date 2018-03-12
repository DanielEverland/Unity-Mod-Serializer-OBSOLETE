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

                if (attribute is IBehaviourClassLoader loader)
                {
                    loader.Load(type);
                }

                AddBehaviour(attribute as BehaviourBase);
            }

            foreach (PropertyInfo property in type.GetProperties(BindingFlags.DeclaredOnly | BindingFlags.Instance | BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic))
            {
                Analyze(property);
            }
            foreach (FieldInfo field in type.GetFields(BindingFlags.DeclaredOnly | BindingFlags.Instance | BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic))
            {
                Analyze(field);
            }
            foreach (MethodInfo method in type.GetMethods(BindingFlags.DeclaredOnly | BindingFlags.Instance | BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic))
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

            if (attribute is IBehaviourMemberLoader<T>)
            {
                (attribute as IBehaviourMemberLoader<T>).Load(targetObj);
            }
            if (attribute is IBehaviourMemberLoader)
            {
                (attribute as IBehaviourMemberLoader).Load();
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

            bool added = AddBehaviour(behaviour.GetHashCode(), behaviour);

            if (OnBehaviourLoaded != null && behaviour != null)
                OnBehaviourLoaded(behaviour);

            if (added && behaviour != null && OnBehaviourAdded != null)
                OnBehaviourAdded(behaviour);
        }

        public static bool AddBehaviour(int key, BehaviourBase behaviour)
        {
            if (behaviour == null)
                return false;

            int priority = behaviour.Priority;

            if (_loadedBehaviours.ContainsKey(key))
            {
                if (_loadedBehaviours[key].Priority < priority)
                {
                    _loadedBehaviours[key] = behaviour;
                }
            }
            else
            {
                _loadedBehaviours.Add(key, behaviour);

                return true;
            }

            return false;
        }
    }
}