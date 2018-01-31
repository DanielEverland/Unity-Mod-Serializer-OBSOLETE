using System;
using System.IO;
using System.Linq;
using System.Text;
using System.Reflection;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using UMS.Core;
using UMS.Behaviour;

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

            mod.Add(obj, obj.GetHashCode(), gameObject.name, "prefab");

            mod.Serialize(@"C:\Users\Daniel\Desktop");
            
            Debug.Log("Finished serializing");
        }


        public Serializer()
        {
            _typeSerializers = new Dictionary<Type, TypeSerializer>();
            _blockedTypes = new Dictionary<Type, TypeBlocker>();

            OnBehaviourLoaded += BehaviourLoaded;
            
            CreateAnalyzers();
            InitializeBehaviours();
        }

        public static event Action<BehaviourBase> OnBehaviourLoaded;
        
        private static Dictionary<Type, TypeSerializer> Serializers { get { return _instance._typeSerializers; } }
        private static Dictionary<Type, TypeBlocker> BlockedTypes { get { return _instance._blockedTypes; } }

        private Dictionary<Type, TypeSerializer> _typeSerializers;
        private Dictionary<Type, TypeBlocker> _blockedTypes;

        private void CreateAnalyzers()
        {
              
        }
        public static object SerializeCustom(object value)
        {
            Type type = value.GetType();

            if (ContainsSerializer(type) && !IsBlocked(type))
            {
                TypeSerializer serializer = GetSerializer(type);
                object output;

                if(serializer.Serialize(value, out output))
                {
                    return output;
                }
            }

            Debug.LogError("Couldn't serialize " + type + value);

            return null;
        }
        public static bool IsBlocked(Type type)
        {
            return BlockedTypes.ContainsKey(type);
        }
        public static bool ContainsSerializer(Type type)
        {
            return Serializers.ContainsKey(type);
        }
        public static TypeBlocker GetBlocker(Type type)
        {
            return BlockedTypes[type];
        }
        public static TypeSerializer GetSerializer(Type type)
        {
            return Serializers[type];
        }
        private void InitializeBehaviours()
        {
            foreach (Assembly assembly in AppDomain.CurrentDomain.GetAssemblies())
            {
                foreach (Type type in assembly.GetTypes())
                {
                    foreach (PropertyInfo property in type.GetProperties())
                    {
                        Analyze(property);
                    }
                    
                    foreach (FieldInfo field in type.GetFields())
                    {
                        Analyze(field);
                    }
                                        
                    foreach (MethodInfo method in type.GetMethods())
                    {
                        Analyze(method);
                    }
                }
            }
        }
        private void Analyze<T>(T targetObj) where T : MemberInfo
        {
            foreach (Attribute attribute in targetObj.GetCustomAttributes(false))
            {
                Analyze(attribute, targetObj);
            }
        }
        private void Analyze<T>(Attribute attribute, T targetObj) where T : MemberInfo
        {
            if (!(attribute is BehaviourBase))
                return;

            if(attribute is IBehaviourLoader<T> loader)
            {
                loader.Load(targetObj);
            }

            AddBehaviour(attribute as BehaviourBase);
        }
        private void AddBehaviour(BehaviourBase behaviour)
        {
            if (OnBehaviourLoaded != null && behaviour != null)
                OnBehaviourLoaded.Invoke(behaviour);
        }
        private void BehaviourLoaded(BehaviourBase behaviour)
        {
            if(behaviour is TypeSerializer)
            {
                TypeSerializer serializer = behaviour as TypeSerializer;
                
                AddBehaviour(_typeSerializers, serializer.AttributeType, serializer);
            }
            else if(behaviour is TypeBlocker)
            {
                TypeBlocker blocker = behaviour as TypeBlocker;

                foreach (Type type in blocker.TypeFunction())
                {
                    AddBehaviour(_blockedTypes, type, blocker);
                }
            }
        }
        private void AddBehaviour<TKey, TValue>(Dictionary<TKey, TValue> dictionary, TKey key, TValue behaviour) where TValue : BehaviourBase
        {
            if (key == null || behaviour == null)
                return;

            int priority = behaviour.Priority;

            if (dictionary.ContainsKey(key))
            {
                if (dictionary[key].Priority < priority)
                    dictionary[key] = behaviour;
            }
            else
            {
                dictionary.Add(key, behaviour);
            }
        }
    }
}
