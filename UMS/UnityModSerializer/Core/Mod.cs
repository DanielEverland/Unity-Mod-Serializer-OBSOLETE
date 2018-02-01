using System;
using System.IO;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Ionic.Zip;
using UMS.Serialization;
using UnityEditor;
using System.Text.RegularExpressions;
using static UMS.Serialization.CustomSerializers;

namespace UMS.Core
{
    [System.Serializable]
    public class Mod
    {
        public Mod(string name)
        {
            _entries = new Dictionary<int, ModEntry>();
            _usedFileNames = new HashSet<string>();
            _data = new List<ModData>();

            this.name = name;

            Current = this;
        }

        [Newtonsoft.Json.JsonIgnore]
        public static Mod Current { get; private set; }
        
        public string name;
        public List<ModData> _data;

        [Newtonsoft.Json.JsonIgnore]
        private static Dictionary<int, ModEntry> _entries;
        [Newtonsoft.Json.JsonIgnore]
        private static HashSet<string> _usedFileNames;

        //Default(int) must be an invalid ID. Therefore we start at 1
        private static int CurrentID = 1;
        private static Regex EndNumberParanthesis = new Regex(@"\(\d\)$");
        private const string CONFIG_NAME = "config";
        
        public void Serialize(UnityEngine.Object obj)
        {
            Serialize<SerializableObject>(obj);
        }
        public T Serialize<T>(UnityEngine.Object obj) where T : SerializableObject
        {
            if (Serializer.ContainsSerializer(obj.GetType()))
            {
                return (T)Serializer.SerializeCustom(obj);
            }
            else
            {
                UnityEngine.Debug.LogError("Cannot serialize " + obj);
                return null;
            }
        }
        public int Add(SerializableObject obj)
        {
            if (_entries.ContainsKey(obj.InstanceID))
            {
                if(_entries[obj.InstanceID].HasObject)
                    return _entries[obj.InstanceID].ID;
            }

            return CreateEntry(obj);
        }
        public static bool Contains(int instanceID)
        {
            return _entries.ContainsKey(instanceID);
        }
        public static int Get(int instanceID)
        {
            if(_entries.ContainsKey(instanceID))
            {
                return _entries[instanceID].ID;
            }
            else
            {
                ModEntry entry = new ModEntry();

                _entries.Add(instanceID, entry);

                return entry.ID;
            }
        }
        private int CreateEntry(SerializableObject obj)
        {
            ModEntry entry = new ModEntry(obj);

            if (_entries.ContainsKey(obj.InstanceID))
            {
                _entries[obj.InstanceID].SetObject(obj);
            }
            else
            {
                _entries.Add(obj.InstanceID, entry);
            }            

            return entry.ID;
        }
        public void Serialize(string path)
        {
            using (ZipFile zip = new ZipFile())
            {
                foreach (KeyValuePair<int, ModEntry> keyValuePair in _entries)
                {
                    ModEntry entry = keyValuePair.Value;

                    if (!entry.HasObject)
                    {
                        Object obj = EditorUtility.InstanceIDToObject(keyValuePair.Key);

                        UnityEngine.Debug.LogWarning("Skipping " + entry.ID + " which is " + obj);
                        continue;
                    }

                    string fileNameWithoutExtension = GetValidName(entry.Name, _usedFileNames);
                    string fileName = fileNameWithoutExtension + "." + entry.Extension;

                    UnityEngine.Debug.Log("Creating file " + fileName);
                    zip.AddEntry(fileName, entry.JSON);

                    _usedFileNames.Add(fileNameWithoutExtension);
                    _data.Add(new ModData(entry));
                }

                zip.AddEntry(CONFIG_NAME, JsonSerializer.ToJson(this));

                zip.Save(string.Format(@"{0}\{1}.mod", path, name));
            }
        }
        private static string GetValidName(string preferredName, HashSet<string> blackList)
        {
            if (blackList.Contains(preferredName))
            {
                if (EndNumberParanthesis.IsMatch(preferredName))
                {
                    Match match = EndNumberParanthesis.Match(preferredName);

                    int index = 0;
                    string strippedString = preferredName.Replace(match.Value, string.Empty);

                    
                    foreach (Capture capture in match.Captures)
                    {
                        int value = 0;
                        if(int.TryParse(capture.Value.Trim(')', '('), out value))
                        {
                            index = value;
                            break;
                        }
                    }
                    
                    return GetValidName(strippedString + "(" + (index + 1) + ")", blackList);                    
                }
                else
                {
                    return GetValidName(preferredName + " (1)", blackList);
                }                
            }

            return preferredName;
        }
        public static Mod Deserialize(string path)
        {
            Dictionary<string, string> files = new Dictionary<string, string>();
            
            using (ZipFile zip = ZipFile.Read(path))
            {
                foreach (ZipEntry entry in zip)
                {
                    using (MemoryStream stream = new MemoryStream())
                    {
                        entry.Extract(stream);

                        stream.Position = 0;
                        StreamReader sr = new StreamReader(stream);

                        files.Add(Path.GetFileNameWithoutExtension(entry.FileName), sr.ReadToEnd());
                    }
                }
            }
            
            if (!files.ContainsKey(CONFIG_NAME))
                throw new NullReferenceException("No config file in " + path);

            Mod mod = JsonSerializer.ToObject<Mod>(files[CONFIG_NAME]);
            files.Remove(CONFIG_NAME);

            Current = mod;
            
            return mod;
        }

        [Serializable]
        public struct ModData
        {
            public ModData(ModEntry entry)
            {
                name = entry.Name;
                ID = entry.ID;
                type = entry.Type;
            }

            public string name;
            public int ID;
            public Type type;
        }
        public class ModEntry
        {
            public ModEntry()
            {
                ID = CurrentID++;
            }
            public ModEntry(SerializableObject obj)
            {
                ID = CurrentID++;

                SetObject(obj);
            }

            public void SetObject(SerializableObject obj)
            {
                if (HasObject || obj == null)
                    return;

                HasObject = true;
                _obj = obj;
                Extension = obj.Extension;
                Type = obj.GetType();
            }
        
            public string JSON
            {
                get
                {
                    return JsonSerializer.ToJson(_obj);
                }
            }
            private SerializableObject _obj;

            public string Name
            {
                get
                {
                    return _obj.ToString();
                }
            }

            public bool HasObject { get; private set; }
            public string Extension { get; private set; }
            public int ID { get; private set; }
            public Type Type { get; private set; }
        }
    }
}
