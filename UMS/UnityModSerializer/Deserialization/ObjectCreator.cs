using System;
using System.Reflection;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;
using UMS.Core;

namespace UMS.Deserialization
{
    public static class ObjectCreator
    {
        public static GameObject CreateObject(Core.Object obj)
        {
            GameObject gameObject = new GameObject();

            LoadMembers(gameObject, obj._members);

            foreach (Core.Component comp in obj._components)
            {
                UnityEngine.Component unityComponent = null;

                if(comp._componentType == typeof(Transform))
                {
                    LoadMembers(gameObject.GetComponent<Transform>(), comp._members);
                }
                else if(TryAdd(comp._componentType, gameObject, out unityComponent))
                {
                    LoadMembers(unityComponent, comp._members);
                }
            }

            return gameObject;
        }
        private static void LoadMembers(object target, IEnumerable<Member> members)
        {
            if (target == null || members == null)
                return;

            foreach (Member member in members)
            {
                if (member._value == null)
                    continue;
                
                member.AssignValue(target);
            }
        }
        private static bool TryGet(Type type, GameObject gameobject, out UnityEngine.Component component)
        {
            try
            {
                component = gameobject.GetComponent(type);
                return true;
            }
            catch (Exception)
            {
                component = null;
                return false;
            }
        }
        private static bool TryAdd(Type type, GameObject gameobject, out UnityEngine.Component component)
        {
            try
            {
                component = gameobject.AddComponent(type);
                return true;
            }
            catch (Exception)
            {
                component = null;
                return false;
            }
        }
    }
}
