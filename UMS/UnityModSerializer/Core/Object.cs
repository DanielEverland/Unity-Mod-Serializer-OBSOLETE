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

            //We have to create a clone in the scene because HideFlags is used to hide prefab hiearchies.
            //This means if you serialize a prefab directly, it won't be visible in the hierarchy upon deserialization.
            bool createdClone = false;
            if (Utility.IsPrefab(obj))
            {
                createdClone = true;
                obj = UnityEngine.GameObject.Instantiate(obj);
            }

            _components = new List<Component>();

            foreach (UnityEngine.Component comp in obj.GetComponents<UnityEngine.Component>())
            {
                _components.Add(new Component(comp));
            }

            _members = Utility.GetMembers(obj);

            _children = new List<Object>();
            foreach (UnityEngine.Transform child in obj.transform)
            {
                _children.Add(new Object(child.gameObject));
            }

            if(createdClone)
            {
                UnityEngine.GameObject.DestroyImmediate(obj);
            }
        }

        public List<Member> _members;
        public List<Component> _components;
        public List<Object> _children;
    }
}
