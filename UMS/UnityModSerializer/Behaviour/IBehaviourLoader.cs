using System.Reflection;

namespace UMS.Behaviour
{
    public interface IBehaviourLoader
    {
        void Load();
    }
    public interface IBehaviourLoader<T> where T : MemberInfo
    {
        void Load(T type);
    }
}