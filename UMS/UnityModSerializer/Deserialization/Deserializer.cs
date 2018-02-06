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
    public static class Deserializer
    {
        private static Dictionary<string, string> _serializedData;
        private static Dictionary<int, ObjectEntry> _objectReferences;

        private static Dictionary<int, List<ActionInstance>> _serializedObjectQueue;
        private static Dictionary<int, List<ActionInstance>> _deserializedObjectQueue;

        [MenuItem(itemName: Utility.MENU_ITEM_ROOT + "/Deserialize", priority = Utility.MENU_ITEM_PRIORITY)]
        private static void Deserialize()
        {
            Initialize();
            ExecuteDeserialization();

            Debug.Log("Finished deserializing");
        }
        private static void Initialize()
        {
            _serializedObjectQueue = new Dictionary<int, List<ActionInstance>>();
            _deserializedObjectQueue = new Dictionary<int, List<ActionInstance>>();

            CoreManager.Initialize();
        }
        private static void ExecuteID(int id)
        {
            ObjectEntry entry = _objectReferences[id];

            if(_serializedObjectQueue.ContainsKey(id))
                ExecuteActions(_serializedObjectQueue[id], entry.SerializedObject);

            if(_deserializedObjectQueue.ContainsKey(id))
                ExecuteActions(_deserializedObjectQueue[id], entry.DeserializedObject);
        }
        private static void ExecuteActions(IList<ActionInstance> actions, object obj)
        {
            foreach (ActionInstance instance in actions)
            {
                if (instance.ExpectedType.IsAssignableFrom(obj.GetType()))
                {
                    instance.Action(obj);
                }
                else
                {
                    throw new InvalidCastException(string.Format("Cannot cast from {0} to {1} using {2}", instance.ExpectedType, obj.GetType(), obj));
                }
            }
        }
        private static void Subscribe(int id, ActionInstance instance, Dictionary<int, List<ActionInstance>> dictionary)
        {
            if (!dictionary.ContainsKey(id))
            {
                dictionary.Add(id, new List<ActionInstance>());
            }

            dictionary[id].Add(instance);
        }
        public static bool ContainsObject(int id)
        {
            return _objectReferences.ContainsKey(id);
        }
        public static void GetSerializedObject<T>(int id, Action<T> action)
        {
            GetSerializedObject(id, new Action<object>(x => action((T)x)), typeof(T));
        }
        public static void GetSerializedObject(int id, Action<object> action, Type type)
        {
            if (!_objectReferences.ContainsKey(id))
            {
                throw new NullReferenceException("ID " + id + " wasn't recognized by the config file");
            }
            
            if(_objectReferences[id].SerializedObject == null)
            {
                Subscribe(id, new ActionInstance(action, type), _serializedObjectQueue);
            }
            else
            {
                action(_objectReferences[id].SerializedObject);
            }
        }
        public static void GetDeserializedObject<T>(int id, Action<T> action)
        {
            GetDeserializedObject(id, new Action<object>(x => action((T)x)), typeof(T));
        }
        public static void GetDeserializedObject(int id, Action<object> action, Type type)
        {
            if (!_objectReferences.ContainsKey(id))
            {
                throw new NullReferenceException("ID " + id + " wasn't recognized by the config file");
            }

            if (_objectReferences[id].DeserializedObject == null)
            {
                Subscribe(id, new ActionInstance(action, type), _deserializedObjectQueue);
            }
            else
            {
                action(_objectReferences[id].DeserializedObject);
            }
        }
        private static void ExecuteDeserialization()
        {
            _objectReferences = new Dictionary<int, ObjectEntry>();
            _serializedData = Mod.Deserialize(@"C:/Users/Daniel/Desktop/New Mod.mod");
            
            foreach (KeyValuePair<string, string> file in _serializedData)
            {
                int id = -1;

                try
                {
                    id = Mod.ConfigFile.data.Find(x => x.localPath == file.Key).id;
                }
                catch (System.Exception)
                {
                    Debug.LogError("Config file didn't contain " + file.Key);
                    throw;
                }

                if(!_objectReferences.ContainsKey(id))
                    _objectReferences.Add(id, new ObjectEntry(file.Value));
            }

            foreach (KeyValuePair<int, ObjectEntry> keyValuePair in _objectReferences)
            {
                keyValuePair.Value.Deserialize();

                ExecuteID(keyValuePair.Key);
            }
        }
        private class ActionInstance
        {
            public ActionInstance(Action<object> action, Type expectedType)
            {
                Action = action;
                ExpectedType = expectedType;
            }

            public Action<object> Action { get; set; }
            public Type ExpectedType { get; set; }
        }
        private class ObjectEntry
        {
            public ObjectEntry(string json)
            {
                JSON = json;
            }
            
            public string JSON { get; private set; }

            public object SerializedObject { get; set; }
            public object DeserializedObject { get; set; }

            private void CreateSerializedObject()
            {
                SerializedObject = JsonSerializer.ToObject(JSON);
            }
            private void CreateDeserializedObject()
            {
                if(CustomSerializers.CanDeserialize(SerializedObject.GetType()))
                    DeserializedObject = CustomSerializers.DeserializeObject(SerializedObject);
            }
            public void Deserialize()
            {
                CreateSerializedObject();
                CreateDeserializedObject();
            }
        }
    }
}
