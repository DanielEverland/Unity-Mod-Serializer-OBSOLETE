using System;
using System.Reflection;

namespace UMS.Behaviour
{
    [AttributeUsage(AttributeTargets.Method)]
    public class CustomIDGeneratorAttribute : BehaviourBase, IBehaviourMemberLoader<MethodInfo>
    {
        private CustomIDGeneratorAttribute() { }
        public CustomIDGeneratorAttribute(Type type, int priority = (int)Core.Priority.Medium) : base(priority)
        {
            _type = type;
        }

        public Type Type { get { return _type; } }

        private readonly Type _type;
        private MethodInfo _method;

        public void Load(MethodInfo type)
        {
            _method = type;
        }
        public int GetID(object obj)
        {
            if (obj.GetType() != Type)
                throw new ArgumentException();

            return (int)_method.Invoke(null, new object[1] { obj });
        }

        public override int GetHashCode()
        {
            int hash = 17;

            unchecked
            {
                hash += _type.GetHashCode() * 7;
                hash += _method.GetHashCode() * 11;
                hash += typeof(CustomIDGeneratorAttribute).GetHashCode() * 13;
            }

            return hash;
        }
    }
}