using System;
using System.IO;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Ionic.Zip;
using UMS.Serialization;
using System.Text.RegularExpressions;

namespace UMS.Core
{
    [System.Serializable]
    public class Mod
    {
        public Mod(string name)
        {
            _hashToID = new Dictionary<int, int>();
            _data = new List<ModData>();
            _idToObject = new Dictionary<int, object>();
            _entries = new List<ModEntry>();
            _usedFilenames = new HashSet<string>();

            this.name = name;

            Current = this;
        }

        [Newtonsoft.Json.JsonIgnore]
        public static Mod Current { get; private set; }

        [Newtonsoft.Json.JsonIgnore]
        private List<ModEntry> _entries;
        [Newtonsoft.Json.JsonIgnore]
        private Dictionary<int, object> _idToObject;
        [Newtonsoft.Json.JsonIgnore]
        public IEnumerable<object> Objects { get { return _idToObject.Values; } }
        [Newtonsoft.Json.JsonIgnore]
        private Dictionary<int, int> _hashToID;
        [Newtonsoft.Json.JsonIgnore]
        private HashSet<string> _usedFilenames;

        public string name;
        public List<ModData> _data;

        private static Regex EndNumberParanthesis = new Regex(@"\(\d\)$");
        private const string CONFIG_NAME = "config";

        public int Add(object obj, string name, string extension)
        {
            string serialized = JsonSerializer.ToJson(obj);
            int hash = serialized.GetHashCode();
            extension = Utility.SanitizeExtension(extension);
            
            if (!_hashToID.ContainsKey(hash))
            {
                int id = _data.Count + 1;

                string uniqueName = GetValidName(name, _usedFilenames);
                _usedFilenames.Add(uniqueName);
                
                ModEntry entry = new ModEntry()
                {
                    json = serialized,
                    name = uniqueName,
                    extension = extension,
                };

                ModData data = new ModData()
                {
                    name = uniqueName,
                    ID = id,
                    type = obj.GetType(),
                };
                
                _entries.Add(entry);
                _hashToID.Add(hash, id);
                _data.Add(data);

                return _entries.Count;
            }
            else
            {
                return _hashToID[hash];
            }
        }
        public T Get<T>(int id)
        {
            return (T)_idToObject[id];
        }
        public void Serialize(string path)
        {
            using (ZipFile zip = new ZipFile())
            {
                zip.AddEntry(CONFIG_NAME, JsonSerializer.ToJson(this));
                
                foreach (ModEntry entry in _entries)
                {
                    string fileName = entry.name + "." + entry.extension;
                    
                    UnityEngine.Debug.Log("Creating file " + fileName);
                    zip.AddEntry(fileName, entry.json);
                }

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

            foreach (ModData data in mod._data)
            {
                object obj = JsonSerializer.ToObject(files[data.name], data.type);

                mod._idToObject.Add(data.ID, obj);
            }

            return mod;
        }

        [System.Serializable]
        public struct ModData
        {
            public string name;
            public int ID;
            public Type type;
        }
        public struct ModEntry
        {
            public string json;
            public string name;
            public string extension;
        }
    }
}
