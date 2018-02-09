using System.Reflection;

namespace UMS.Behaviour
{
    public interface IBehaviourLoader<T> where T : MemberInfo
    {
        void Load(T type);
    }
}