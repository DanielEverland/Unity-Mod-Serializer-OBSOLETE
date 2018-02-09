using System;
using System.Collections.Generic;
using System.Reflection;

namespace UMS.Behaviour
{
    [AttributeUsage(AttributeTargets.Property | AttributeTargets.Method | AttributeTargets.Field, AllowMultiple = true, Inherited = true)]
    public class TypeBlocker : BehaviourBase, IBehaviourLoader<PropertyInfo>, IBehaviourLoader<MethodInfo>, IBehaviourLoader<FieldInfo>
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
    }
}