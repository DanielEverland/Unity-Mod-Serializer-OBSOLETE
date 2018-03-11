using System;
using System.Collections.Generic;

namespace UMS.Runtime.Behaviour
{
    public static class CustomValidityComparers
    {
        private static ValidityContainer Container
        {
            get
            {
                if (_container == null)
                    _container = new ValidityContainer();

                return _container;
            }
        }
        private static ValidityContainer _container;

        public static bool IsValid(object obj)
        {
            if (obj == null)
                return false;

            return Container.IsValid(obj);
        }

        private class ValidityContainer
        {
            public ValidityContainer()
            {
                if (BehaviourManager.HasInitialized)
                {
                    Load();
                }
                else
                {
                    BehaviourManager.OnFinishedInitializing += Load;
                }
            }

            private Dictionary<Type, TypeValidityComparerAttribute> _typeValidityComparers;

            private void Load()
            {
                _typeValidityComparers = new Dictionary<Type, TypeValidityComparerAttribute>();

                foreach (TypeValidityComparerAttribute comparer in BehaviourManager.GetBehaviours<TypeValidityComparerAttribute>())
                {
                    _typeValidityComparers.Add(comparer.Type, comparer);
                }
            }
            public bool IsValid(object obj)
            {
                if (_typeValidityComparers == null)
                    throw new NullReferenceException();

                Type type = obj.GetType();

                if (_typeValidityComparers.ContainsKey(type))
                {
                    return _typeValidityComparers[type].IsValid(obj);
                }

                return true;
            }
        }
    }
}