using System;
using System.Collections.Generic;
using System.Reflection;

namespace UMS.Behaviour
{
    [AttributeUsage(AttributeTargets.Property | AttributeTargets.Method | AttributeTargets.Field, AllowMultiple = true, Inherited = true)]
    public class MemberBlocker : BehaviourBase, IBehaviourLoader<PropertyInfo>, IBehaviourLoader<MethodInfo>, IBehaviourLoader<FieldInfo>
    {
        public Func<IEnumerable<string>> MemberFunction;

        public void Load(PropertyInfo type)
        {
            MemberFunction = () =>
            {
                return (IEnumerable<string>)type.GetGetMethod().Invoke(null, null);
            };
        }
        public void Load(MethodInfo type)
        {
            MemberFunction = () =>
            {
                return (IEnumerable<string>)type.Invoke(null, null);
            };
        }
        public void Load(FieldInfo type)
        {
            MemberFunction = () =>
            {
                return (IEnumerable<string>)type.GetValue(null);
            };
        }
    }
}