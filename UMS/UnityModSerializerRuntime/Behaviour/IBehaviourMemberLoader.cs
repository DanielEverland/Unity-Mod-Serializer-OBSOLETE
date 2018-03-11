using System.Reflection;

namespace UMS.Runtime.Behaviour
{
    public interface IBehaviourMemberLoader
    {
        void Load();
    }
    public interface IBehaviourMemberLoader<T> where T : MemberInfo
    {
        void Load(T type);
    }
}