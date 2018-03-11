using System;
using System.Collections.Generic;
using System.Reflection;

namespace UMS.Runtime.Behaviour
{
    [AttributeUsage(AttributeTargets.Property | AttributeTargets.Method | AttributeTargets.Field, AllowMultiple = true, Inherited = true)]
    public class MemberBlockerAttribute : BehaviourBase, IBehaviourMemberLoader<PropertyInfo>, IBehaviourMemberLoader<MethodInfo>, IBehaviourMemberLoader<FieldInfo>
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

        public override int GetHashCode()
        {
            int id = 17;

            unchecked
            {
                id += MemberFunction.GetHashCode() * 11;
                id += typeof(MemberBlockerAttribute).GetHashCode() * 7;
            }

            return id;
        }
    }
}