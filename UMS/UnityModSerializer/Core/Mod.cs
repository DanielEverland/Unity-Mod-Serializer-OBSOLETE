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
using System.Reflection;

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
            references = new Queue<Reference>();

            this.name = name;

            Current = this;
        }

        [Newtonsoft.Json.JsonIgnore]
        public static Mod Current { get; private set; }
        
        public string name;
        public List<ModData> _data;

        [Newtonsoft.Json.JsonIgnore]
        public Queue<Reference> references;

        [Newtonsoft.Json.JsonIgnore]
        private static Dictionary<int, ModEntry> _entries;
        [Newtonsoft.Json.JsonIgnore]
        private static HashSet<string> _usedFileNames;
        
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
        public void Add(SerializableObject obj, int id)
        {
            if (obj == null)
                return;

            if (!_entries.ContainsKey(id))
            {
                _entries.Add(id, new ModEntry(obj));
            }
        }
        public int GetID(UnityEngine.Object obj)
        {
            if (obj == null)
                return 0;
            
            unchecked
            {
                int i = 17;

                foreach (PropertyInfo info in obj.GetType().GetProperties(BindingFlags.Instance | BindingFlags.Public))
                {
                    if (Serializer.IsBlocked(Utility.GetObjectMemberName(info.DeclaringType.Name, info.Name)))
                        continue;

                    MethodInfo method = info.GetGetMethod();

                    if (method == null)
                        continue;

                    if (!Utility.CanAccessMember(info))
                        continue;

                    try
                    {
                        i = i + info.GetValue(obj, null).GetHashCode() * 13;
                    }
                    catch (System.Exception)
                    {
                    }
                }
                                
                return i;
            }
        }
        public void Serialize(string path)
        {
            using (ZipFile zip = new ZipFile())
            {
                foreach (KeyValuePair<int, ModEntry> keyValuePair in _entries)
                {
                    ModEntry entry = keyValuePair.Value;
                    
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
            public ModEntry(SerializableObject obj)
            {
                _obj = obj;
                Extension = obj.Extension;
                Type = obj.GetType();
                ID = obj.ID;
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
            
            public string Extension { get; private set; }
            public int ID { get; private set; }
            public Type Type { get; private set; }
        }
    }
}
