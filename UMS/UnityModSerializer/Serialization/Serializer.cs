using System;
using System.IO;
using System.Linq;
using System.Text;
using System.Reflection;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using UMS.Core;

namespace UMS.Serialization
{
    public class Serializer
    {
        private static Serializer _instance;

        [MenuItem(itemName: Utility.MENU_ITEM_ROOT + "/Serialize Selection", priority = Utility.MENU_ITEM_PRIORITY)]
        private static void SerializeSelection()
        {
            if (Selection.gameObjects.Length == 0)
            {
                Debug.LogWarning("No objects selected");
                return;
            }               

            StartNewSerializer();

            for (int i = 0; i < Selection.gameObjects.Length; i++)
            {
                SerializeObject(Selection.gameObjects[i]);
            }
        }

        private static void StartNewSerializer()
        {
            _instance = new Serializer();
        }
        private static void SerializeObject(GameObject gameObject)
        {
            Mod mod = new Mod("Test Mod");
            
            Core.Object obj = new Core.Object(gameObject);

            mod.Add(obj, gameObject.name, "prefab");

            mod.Serialize(@"C:\Users\Daniel\Desktop");

            UnityEngine.Debug.Log("Finished serializing");
        }


        public Serializer()
        {
            _typeSerializers = new Dictionary<Type, TypeSerializer>();

            InitializeSerializers();
        }

        private static Dictionary<Type, TypeSerializer> Serializers { get { return _instance._typeSerializers; } }

        private Dictionary<Type, TypeSerializer> _typeSerializers;
        public static object SerializeCustom(object value)
        {
            if (ContainsSerializer(value.GetType()))
            {
                TypeSerializer serializer = GetSerializer(value.GetType());
                object output;

                if(serializer.Serialize(value, out output))
                {
                    return output;
                }
            }

            UnityEngine.Debug.LogError("Couldn't serializer " + value.GetType() + value);

            return null;
        }
        public static bool ContainsSerializer(Type type)
        {
            return Serializers.ContainsKey(type);
        }
        public static TypeSerializer GetSerializer(Type type)
        {
            return Serializers[type];
        }
        private void InitializeSerializers()
        {
            foreach (Assembly assembly in System.AppDomain.CurrentDomain.GetAssemblies())
            {
                foreach (Type type in assembly.GetTypes())
                {
                    foreach (MethodInfo method in type.GetMethods())
                    {
                        IEnumerable<TypeSerializer> attributes = method.GetCustomAttributes(false)
                            .Where(x => x.GetType() == typeof(TypeSerializer)).Select(x => (TypeSerializer)x);

                        foreach (TypeSerializer serializer in attributes)
                        {
                            serializer.SetMethod(method);

                            if (_typeSerializers.ContainsKey(serializer.type))
                            {
                                if(serializer.priority > _typeSerializers[serializer.type].priority)
                                {
                                    _typeSerializers[serializer.type] = serializer;
                                }
                            }
                            else
                            {
                                _typeSerializers.Add(serializer.type, serializer);
                            }
                        }                                                                    
                    }
                }
            }
        }
    }
}
