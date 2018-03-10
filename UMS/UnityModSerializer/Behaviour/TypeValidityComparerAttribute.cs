using System;
using System.Reflection;

namespace UMS.Behaviour
{
    /// <summary>
    /// Used to define whether an object can be serialized. Useful for Unity edge cases where a null check doesn't suffice
    /// </summary>
    [AttributeUsage(AttributeTargets.Class, AllowMultiple = true, Inherited = false)]
    public class TypeValidityComparerAttribute : BehaviourBase, IBehaviourClassLoader
    {
        private TypeValidityComparerAttribute() { }
        public TypeValidityComparerAttribute(Type type, int priority = (int)Core.Priority.Medium) : base(priority)
        {
            _type = type;
        }

        public Type Type { get { return _type; } }

        private Type _type;
        private MethodInfo _comparerMethod;
        private object _obj;

        public bool IsValid(object obj)
        {
            if (_type != obj.GetType())
                throw new ArgumentException();

            return (bool)_comparerMethod.Invoke(_obj, new object[1] { obj });
        }
        public void Load(Type type)
        {
            if (type == null)
                throw new NullReferenceException();

            foreach (Type interfaceType in type.GetInterfaces())
            {
                if (interfaceType.IsGenericType && interfaceType.GetGenericTypeDefinition() == typeof(IValidityComparer<>))
                {
                    Type comparerType = interfaceType.GetGenericArguments()[0];

                    if (comparerType != _type)
                    {
                        throw new ArgumentException("Equality comparer type must be " + _type + ". It is " + comparerType);
                    }
                    else
                    {
                        _comparerMethod = interfaceType.GetMethod("IsValid");
                        _obj = Activator.CreateInstance(type);
                    }
                }
            }

            if (_comparerMethod == null)
                throw new NullReferenceException("Couldn't find a valid IValidityComparer<" + _type + "> for type " + type);
        }
        public override int GetHashCode()
        {
            int id = 13;

            unchecked
            {
                id += _type.GetHashCode() * 11;
                id += _obj.GetHashCode() * 17;
                id += typeof(TypeValidityComparerAttribute).GetHashCode() * 7;
            }

            return id;
        }
    }
}