using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;

namespace UMS.Behaviour
{
    public interface IBehaviourLoader<T> where T : MemberInfo
    {
        void Load(T type);
    }
}
