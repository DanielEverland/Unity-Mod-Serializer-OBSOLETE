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
            if (Selection.objects.Length == 0)
            {
                Debug.LogWarning("No objects selected");
                return;
            }               

            StartNewSerializer();

            for (int i = 0; i < Selection.objects.Length; i++)
            {
                Mod.Current.Serialize(Selection.objects[i]);
            }

            Mod.Current.Serialize(@"C:\Users\Daniel\Desktop");

            Debug.Log("Finished serializing");
        }

        private static void StartNewSerializer()
        {
            _instance = new Serializer();

            new Mod("Test Mod");
        }


        public Serializer()
        {
            _typeSerializers = new Dictionary<Type, TypeSerializer>();
            _blockedTypes = new Dictionary<Type, TypeBlocker>();
            _blockedMembers = new HashSet<string>();

            OnBehaviourLoaded += BehaviourLoaded;
            
            InitializeBehaviours();
        }

        public static event Action<BehaviourBase> OnBehaviourLoaded;
        
        private static Dictionary<Type, TypeSerializer> Serializers { get { return _instance._typeSerializers; } }
        private static Dictionary<Type, TypeBlocker> BlockedTypes { get { return _instance._blockedTypes; } }
        private static HashSet<string> BlockedMembers { get { return _instance._blockedMembers; } }

        private Dictionary<Type, TypeSerializer> _typeSerializers;
        private Dictionary<Type, TypeBlocker> _blockedTypes;
        private HashSet<string> _blockedMembers;
        
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
        public static bool IsBlocked(string memberObjectName)
        {
            return BlockedMembers.Contains(memberObjectName);
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
            foreach (Assembly assembly in GetAssemblies())
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
        private IEnumerable<Assembly> GetAssemblies()
        {
            HashSet<Assembly> _toReturn = new HashSet<Assembly>()
            {
                Assembly.GetExecutingAssembly(),
            };

            Queue<string> toCheck = new Queue<string>();
            toCheck.Enqueue(Application.dataPath);

            while (toCheck.Count > 0)
            {
                string current = toCheck.Dequeue();

                if(Path.GetFileNameWithoutExtension(current) == "Plugins")
                {
                    GetAssemblies(current, _toReturn);
                }
                else
                {
                    foreach (string subFolder in Directory.GetDirectories(current))
                    {
                        toCheck.Enqueue(subFolder);
                    }
                }                
            }

            return _toReturn;
        }
        private void GetAssemblies(string path, HashSet<Assembly> collection)
        {
            foreach (string file in Directory.GetFiles(path))
            {
                if (Path.GetExtension(file) == ".dll")
                {
                    Assembly assembly = Assembly.LoadFile(file);

                    if(!collection.Contains(assembly))
                        collection.Add(assembly);
                }                    
            }

            foreach (string subDirectory in Directory.GetDirectories(path))
            {
                GetAssemblies(subDirectory, collection);
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
            else if(behaviour is MemberBlocker)
            {
                MemberBlocker blocker = behaviour as MemberBlocker;

                foreach (string memberObjectName in blocker.MemberFunction())
                {
                    if (!_blockedMembers.Contains(memberObjectName))
                    {
                        _blockedMembers.Add(memberObjectName);
                    }
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
