using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Reflection;
using UMS.Serialization;

namespace UMS.Core
{
    [System.Serializable]
    public class Component
    {
        public Component(UnityEngine.Component component)
        {
            if (component == null)
                return;

            _componentType = component.GetType();
            _members = Utility.GetMembers(component);
        }

        public Type _componentType;
        public List<Member> _members;
    }
}
