using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace UMS.Behaviour
{
    public interface IBehaviourClassLoader
    {
        void Load(Type type);
    }
}
