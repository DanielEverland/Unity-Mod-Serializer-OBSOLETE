﻿using Ionic.Zip;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using UMS.Behaviour;
using UMS.Core;
using UnityEngine;

namespace UMS.Deserialization
{
    public static class Deserializer
    {
        public static event Action OnHasDeserialized;
        public static IDictionary<string, UnityEngine.Object> Objects { get { return _keyLookup; } }
        public static IDictionary<string, byte[]> SerializedData { get { return _serializedData; } }
        public static bool HasDeserialized { get; private set; }
        public static bool HasInitialized { get; private set; }

        private static Dictionary<string, byte[]> _serializedData;
        private static Dictionary<int, ObjectEntry> _objectReferences;
        private static Dictionary<string, UnityEngine.Object> _keyLookup;
        
        private static Dictionary<int, List<ActionInstance>> _serializedObjectQueue;
        private static Dictionary<int, List<ActionInstance>> _deserializedObjectQueue;

        private static List<JsonConverter> _converters;
        private static HashSet<int> _locks;
        private static System.Random _random;

        public static void Initialize()
        {
            if (HasInitialized)
                return;

            _random = new System.Random();
            _keyLookup = new Dictionary<string, UnityEngine.Object>();
            _serializedData = new Dictionary<string, byte[]>();
            _objectReferences = new Dictionary<int, ObjectEntry>();
            _serializedObjectQueue = new Dictionary<int, List<ActionInstance>>();
            _deserializedObjectQueue = new Dictionary<int, List<ActionInstance>>();
            _locks = new HashSet<int>();
            _converters = new List<JsonConverter>();

            BehaviourManager.OnBehaviourLoadedWithContext += BehaviourLoaded;

            CoreManager.Initialize();

            HasInitialized = true;
        }
        public static int CreateLock()
        {
            int id = _random.Next();

            _locks.Add(id);

            return id;
        }
        public static void RemoveLock(int id)
        {
            _locks.Remove(id);

            CheckIfFinished();
        }
        public static bool ContainsStaticObject(string localPath)
        {
            return StaticObjects.Contains(localPath);
        }
        public static byte[] GetStaticObject(string localPath)
        {
            return StaticObjects.GetObject(localPath);
        }
        public static void DeserializePackage(string path)
        {
            using (ZipFile zip = ZipFile.Read(path))
            {
                foreach (ZipEntry entry in zip)
                {
                    using (MemoryStream stream = new MemoryStream())
                    {
                        entry.Extract(stream);
                        
                        if(!_serializedData.ContainsKey(entry.FileName))
                            _serializedData.Add(entry.FileName, stream.ToArray());
                    }
                }
            }

            if (!_serializedData.ContainsKey(Utility.MANIFEST_NAME))
                throw new NullReferenceException("No manifest file in " + path);

            Manifest.Instance = Json.ToObject<Manifest>(Utility.ToString(_serializedData[Utility.MANIFEST_NAME]));
            _serializedData.Remove(Utility.MANIFEST_NAME);

            foreach (KeyValuePair<string, byte[]> file in _serializedData)
            {
                if (Manifest.Instance.data.Any(x => x.localPath == file.Key))
                {
                    Manifest.Data data = Manifest.Instance.data.Find(x => x.localPath == file.Key);

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

            CheckForCachedActions();
            CoreManager.FinishedSerialization();
            
            Debug.Log("Deserialized " + Path.GetFileNameWithoutExtension(path));
        }
        /// <summary>
        /// Used to load object entries from mod packages using the AssetDatabase during edit time
        /// </summary>
        public static void AddObject(string key, UnityEngine.Object obj)
        {
            if (_keyLookup.ContainsKey(key))
                return;

            _keyLookup.Add(key, obj);
        }
        public static bool KeyExists(string key)
        {
            if (!HasDeserialized)
                throw new InvalidOperationException("Deserialization hasn't finished");

            return _keyLookup.ContainsKey(key);
        }
        public static T GetObject<T>(string key) where T : UnityEngine.Object
        {
            if (!HasDeserialized)
                throw new InvalidOperationException("Deserialization hasn't finished");

            return (T)_keyLookup[key];
        }
        public static UnityEngine.Object GetObject(string key)
        {
            if (!HasDeserialized)
                throw new InvalidOperationException("Deserialization hasn't finished");

            return _keyLookup[key];
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
        public static IList<Newtonsoft.Json.JsonConverter> GetConverters()
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
            if (behaviour is CustomConstructorAttribute constructor && info is MethodInfo method)
            {
                CustomConstructorLoaded(constructor, method);
            }
        }
        private static void CustomConstructorLoaded(CustomConstructorAttribute customConstructor, MethodInfo info)
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
                    instance.Execute(obj);
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
        private static void CheckIfFinished()
        {
            if (_locks.Count == 0 && HasInitialized)
                FinishedDeserializing();
        }
        private static void FinishedDeserializing()
        {
            HasDeserialized = true;

            OnHasDeserialized?.Invoke();
        }

        private class ActionInstance
        {
            public ActionInstance(Action<object> action, Type expectedType)
            {
                _action = action;
                _lockID = CreateLock();

                ExpectedType = expectedType;
            }

            public Type ExpectedType { get; set; }

            private readonly Action<object> _action;
            private readonly int _lockID;

            public void Execute(object obj)
            {
                RemoveLock(_lockID);

                if(_action != null)
                    _action.Invoke(obj);
            }
        }
        private class ObjectEntry
        {
            public ObjectEntry(string json, Manifest.Data data)
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
                SerializedObject = Json.ToObject(JSON);
            }
            private void CreateDeserializedObject()
            {
                if (Converter.CanDeserialize(SerializedObject.GetType()))
                {
                    Converter.DeserializeObject(SerializedObject, AssignDeserializedObject);
                }
                else
                {
                    Debug.LogWarning("CANNOT DESERIALIZE " + ID);
                }
            }
            public void AssignDeserializedObject(object obj)
            {
                DeserializedObject = obj;

                ExecuteID(ID);

                if (Key != null)
                    _keyLookup.Set(Key, (UnityEngine.Object)DeserializedObject);

                CheckIfFinished();
            }
            public void Deserialize()
            {
                CreateSerializedObject();
                CreateDeserializedObject();
            }
        }
        private class CustomConstructorConverter : Newtonsoft.Json.JsonConverter
        {
            private CustomConstructorConverter() { }
            public CustomConstructorConverter(CustomConstructorAttribute customConstructor)
            {
                _customConstructor = customConstructor;
            }

            private readonly CustomConstructorAttribute _customConstructor;

            public override bool CanConvert(Type objectType)
            {
                return _customConstructor.TargetType.IsAssignableFrom(objectType);
            }
            public override bool CanWrite => false;

            public override object ReadJson(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer)
            {
                if (reader.TokenType == JsonToken.StartObject)
                    JObject.Load(reader);

                return _customConstructor.Method.Invoke(null, new object[1] { objectType });
            }

            public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer)
            {
                throw new NotImplementedException();
            }
        }
    }
}