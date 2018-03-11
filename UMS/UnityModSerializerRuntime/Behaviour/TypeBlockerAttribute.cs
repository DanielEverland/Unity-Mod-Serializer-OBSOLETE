using System;
using System.Collections.Generic;
using System.Reflection;

namespace UMS.Runtime.Behaviour
{
    [AttributeUsage(AttributeTargets.Property | AttributeTargets.Method | AttributeTargets.Field, AllowMultiple = true, Inherited = true)]
    public class TypeBlockerAttribute : BehaviourBase, IBehaviourMemberLoader<PropertyInfo>, IBehaviourMemberLoader<MethodInfo>, IBehaviourMemberLoader<FieldInfo>
    {
        public Func<IEnumerable<Type>> TypeFunction;

        public void Load(PropertyInfo type)
        {
            TypeFunction = () =>
            {
                return (IEnumerable<Type>)type.GetGetMethod().Invoke(null, null);
            };
        }
        public void Load(MethodInfo type)
        {
            TypeFunction = () =>
            {
                return (IEnumerable<Type>)type.Invoke(null, null);
            };
        }
        public void Load(FieldInfo type)
        {
            TypeFunction = () =>
            {
                return (IEnumerable<Type>)type.GetValue(null);
            };
        }

        public override int GetHashCode()
        {
            int id = 17;

            unchecked
            {
                id += TypeFunction.GetHashCode() * 11;
                id += typeof(TypeBlockerAttribute).GetHashCode() * 7;
            }

            return id;
        }
    }
}