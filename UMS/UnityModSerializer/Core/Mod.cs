using System;
using System.IO;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using Ionic.Zip;
using UMS.Serialization;

namespace UMS.Core
{
    [System.Serializable]
    public class Mod
    {
        public Mod(string name)
        {
            _data = new List<ModData>();
            _objects = new Dictionary<int, object>();
            _entries = new List<ModEntry>();

            this.name = name;
        }

        [Newtonsoft.Json.JsonIgnore]
        private List<ModEntry> _entries;
        [Newtonsoft.Json.JsonIgnore]
        private Dictionary<int, object> _objects;
        [Newtonsoft.Json.JsonIgnore]
        public IEnumerable<object> Objects { get { return _objects.Values; } }
        
        public string name;
        public List<ModData> _data;

        private const string CONFIG_NAME = "config";

        public int Add(object obj, string name, string extension)
        {
            int id = _data.Count + 1;

            ModEntry entry = new ModEntry()
            {
                json = JsonSerializer.ToJson(obj),
                name = name,
                extension = extension,
            };

            _entries.Add(entry);
            _objects.Add(id, obj);
            _data.Add(new ModData() { name = name, ID = id, type = obj.GetType() });

            return _entries.Count;
        }
        public object Get(int id)
        {
            return _objects[id];
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

                mod._objects.Add(data.ID, obj);
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
