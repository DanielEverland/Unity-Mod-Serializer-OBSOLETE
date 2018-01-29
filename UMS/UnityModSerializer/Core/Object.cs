using System;
using System.Collections.Generic;
using System.Reflection;
using System.Linq;
using System.Text;

namespace UMS.Core
{
    [System.Serializable]
    public class Object
    {
        public Object(UnityEngine.GameObject obj)
        {
            if (obj == null)
                return;

            _components = new List<Component>();

            foreach (UnityEngine.Component comp in obj.GetComponents<UnityEngine.Component>())
            {
                _components.Add(new Component(comp));
            }

            _members = Utility.GetMembers(obj);
        }

        public List<Member> _members;
        public List<Component> _components;
    }
}
