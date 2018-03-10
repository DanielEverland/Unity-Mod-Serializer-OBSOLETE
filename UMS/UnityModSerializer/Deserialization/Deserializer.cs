using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using UMS.Behaviour;
using UMS.Core;
using UnityEditor;
using UnityEngine;

namespace UMS.Deserialization
{
    public static class Deserializer
    {
        public static IDictionary<string, byte[]> SerializedData { get { return _serializedData; } }
        public static bool HasDeserialized { get { return _hasFinished; } }

        private static Dictionary<string, byte[]> _serializedData;
        private static Dictionary<int, ObjectEntry> _objectReferences;
        private static Dictionary<string, ObjectEntry> _keyLookup;

        private static Dictionary<int, List<ActionInstance>> _serializedObjectQueue;
        private static Dictionary<int, List<ActionInstance>> _deserializedObjectQueue;

        private static List<JsonConverter> _converters;
        private static bool _hasFinished;

        [MenuItem(itemName: Utility.MENU_ITEM_ROOT + "/Deserialize", priority = Utility.MENU_ITEM_PRIORITY)]
        private static void Deserialize()
        {
            Initialize();
            ExecuteDeserialization();

            CheckForCachedActions();
            CoreManager.FinishedSerialization();

            _hasFinished = true;
            Debug.Log("Finished deserializing");
        }
        private static void Initialize()
        {
            _keyLookup = new Dictionary<string, ObjectEntry>();
            _serializedObjectQueue = new Dictionary<int, List<ActionInstance>>();
            _deserializedObjectQueue = new Dictionary<int, List<ActionInstance>>();
            _converters = new List<JsonConverter>();

            BehaviourManager.OnBehaviourLoadedWithContext += BehaviourLoaded;

            CoreManager.Initialize();
        }
        public static bool KeyExists(string key)
        {
            if (!HasDeserialized)
                throw new InvalidOperationException("Deserialization hasn't finished");

            return _keyLookup.ContainsKey(key);
        }
        public static T GetObject<T>(string key)
        {
            if (!HasDeserialized)
                throw new InvalidOperationException("Deserialization hasn't finished");

            return (T)_keyLookup[key].DeserializedObject;
        }
        public static object GetObject(string key)
        {
            if (!HasDeserialized)
                throw new InvalidOperationException("Deserialization hasn't finished");

            return _keyLookup[key].DeserializedObject;
        }
        private static void CheckForCachedActions()
        {
            foreach (KeyValuePair<int, List<ActionInstance>> queueObject in _deserializedObjectQueue.Union(_serializedObjectQueue))
            {
                if (_objectReferences.ContainsKey(queueObject.Key))
                {
                    ObjectEntry entry = _objectReferences[queueObject.Key];

                    if (_serializedObjectQueue.ContainsKey(queueObject.Key))
                    {
                        if (entry.SerializedObject != null)
                        {
                            Debugging.Info("Executing serialized object queue for object " + entry.ID);

                            ExecuteActions(_serializedObjectQueue[entry.ID], entry.SerializedObject);
                        }
                        else
                        {
                            Debugging.Warning("Serialized object for " + entry.ID + " has not been added");
                        }
                    }
                    if (_deserializedObjectQueue.ContainsKey(queueObject.Key))
                    {
                        if (entry.DeserializedObject != null)
                        {
                            Debugging.Info("Executing deserialized object queue for object " + entry.ID);

                            ExecuteActions(_deserializedObjectQueue[entry.ID], entry.DeserializedObject);
                        }
                        else
                        {
                            Debugging.Warning("Deserialized object for " + entry.ID + " has not been added");
                        }
                    }
                }
                else
                {
                    Debugging.Warning("ID " + queueObject.Key + " has not been deserialized. Objects waiting for execution: " + queueObject.Value);
                }
            }
        }
        public static IList<JsonConverter> GetConverters()
        {
            return _converters;
        }
        /// <summary>
        /// Used for objects that deserialize using ICustomDeserializer
        /// </summary>
        public static void AssignDeserializedObject(int ID, object obj)
        {
            _objectReferences[ID].AssignDeserializedObject(obj);
        }
        private static void BehaviourLoaded(BehaviourBase behaviour, MemberInfo info)
        {
            if (behaviour is CustomConstructor constructor && info is MethodInfo method)
            {
                CustomConstructorLoaded(constructor, method);
            }
        }
        private static void CustomConstructorLoaded(CustomConstructor customConstructor, MethodInfo info)
        {
            customConstructor.AssignMethod(info);

            _converters.Add(new CustomConstructorConverter(customConstructor));
        }
        private static void ExecuteID(int id)
        {
            ObjectEntry entry = _objectReferences[id];

            if (_serializedObjectQueue.ContainsKey(id))
            {
                ExecuteActions(_serializedObjectQueue[id], entry.SerializedObject);

                _serializedObjectQueue.Remove(id);
            }

            if (_deserializedObjectQueue.ContainsKey(id) && entry.DeserializedObject != null)
            {
                ExecuteActions(_deserializedObjectQueue[id], entry.DeserializedObject);

                _deserializedObjectQueue.Remove(id);
            }
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

            if (_objectReferences[id].SerializedObject == null)
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
            GetDeserializedObject(id, typeof(T), new Action<object>(x => action((T)x)));
        }
        public static void GetDeserializedObject(int id, Type type, Action<object> action)
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

            foreach (KeyValuePair<string, byte[]> file in _serializedData)
            {
                if (Mod.ConfigFile.data.Any(x => x.localPath == file.Key))
                {
                    Config.Data data = Mod.ConfigFile.data.Find(x => x.localPath == file.Key);

                    if (!_objectReferences.ContainsKey(data.id))
                    {
                        ObjectEntry entry = new ObjectEntry(Utility.ToString(file.Value), data);

                        _objectReferences.Add(data.id, entry);
                    }
                }
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
            public ObjectEntry(string json, Config.Data data)
            {
                JSON = json;

                this.ID = data.id;
                this.Key = data.key;
            }

            public string JSON { get; private set; }

            public string Key { get; private set; }
            public object SerializedObject { get; set; }
            public object DeserializedObject { get; set; }
            public int ID { get; set; }

            private void CreateSerializedObject()
            {
                SerializedObject = Serialization.JsonSerializer.ToObject(JSON);
            }
            private void CreateDeserializedObject()
            {
                if (Serialization.CustomSerializers.CanDeserialize(SerializedObject.GetType()))
                {
                    Serialization.CustomSerializers.DeserializeObject(SerializedObject, AssignDeserializedObject);
                }
            }
            public void AssignDeserializedObject(object obj)
            {
                DeserializedObject = obj;

                ExecuteID(ID);

                if (Key != null)
                    _keyLookup.Set(Key, this);
            }
            public void Deserialize()
            {
                CreateSerializedObject();
                CreateDeserializedObject();
            }
        }
        private class CustomConstructorConverter : JsonConverter
        {
            private CustomConstructorConverter() { }
            public CustomConstructorConverter(CustomConstructor customConstructor)
            {
                _customConstructor = customConstructor;
            }

            private readonly CustomConstructor _customConstructor;

            public override bool CanConvert(Type objectType)
            {
                return _customConstructor.TargetType.IsAssignableFrom(objectType);
            }
            public override bool CanWrite => false;

            public override object ReadJson(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer)
            {
                if (reader.TokenType == JsonToken.StartObject)
                    JObject.Load(reader);

                return _customConstructor.Method.Invoke(null, null);
            }

            public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer)
            {
                throw new NotImplementedException();
            }
        }
    }
}