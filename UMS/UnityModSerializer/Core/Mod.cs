using System;
using System.IO;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Ionic.Zip;
using UMS.Serialization;

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

            this.name = name;
        }

        [Newtonsoft.Json.JsonIgnore]
        public static Mod Current { get { return Serializer.CurrentMod; } }

        [Newtonsoft.Json.JsonIgnore]
        private List<ModEntry> _entries;
        [Newtonsoft.Json.JsonIgnore]
        private Dictionary<int, object> _idToObject;
        [Newtonsoft.Json.JsonIgnore]
        public IEnumerable<object> Objects { get { return _idToObject.Values; } }
        [Newtonsoft.Json.JsonIgnore]
        private Dictionary<int, int> _hashToID;

        public string name;
        public List<ModData> _data;

        private const string CONFIG_NAME = "config";

        public int Add(object obj, int hash, string name, string extension)
        {
            extension = Utility.SanitizeExtension(extension);
            
            if (!_hashToID.ContainsKey(hash))
            {
                int id = _data.Count + 1;

                ModEntry entry = new ModEntry()
                {
                    json = JsonSerializer.ToJson(obj),
                    name = name,
                    extension = extension,
                };

                _entries.Add(entry);
                _idToObject.Add(id, obj);
                _data.Add(new ModData() { name = name, ID = id, type = obj.GetType() });

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
                    zip.AddEntry(entry.name + "." + entry.extension, entry.json);
                }

                zip.Save(string.Format(@"{0}\{1}.mod", path, name));
            }
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
