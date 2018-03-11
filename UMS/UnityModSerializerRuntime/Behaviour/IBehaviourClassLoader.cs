using System;

namespace UMS.Runtime.Behaviour
{
    public interface IBehaviourClassLoader
    {
        void Load(Type type);
    }
}