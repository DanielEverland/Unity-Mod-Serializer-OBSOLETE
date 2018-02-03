
using System.Collections.Generic;

namespace UMS.Core
{
    public static class Mod
    {
        private const string CONFIG_NAME = "config";

        private static List<object> _objectsToSerialize;

        public static void Initialize()
        {
            _objectsToSerialize = new List<object>();
        }
        public static void Add(object obj)
        {

        }

        //public void Serialize(string path)
        //{
        //    while (toSerialize.Count > 0)
        //    {
        //        UnityEngine.Object obj = toSerialize.Dequeue();
        //        int id = GetID(obj);

        //        //UnityEngine.Debug.Log(id);
        //        UnityEngine.Debug.Log(_entries.Count);

        //        if (!_entries.ContainsKey(id))
        //        {
        //            Reference reference = Reference.Create(obj);
        //            SerializableObject serialized = reference.Serialize(obj);
        //            ModEntry entry = new ModEntry(serialized);

        //            _entries.Add(id, entry);
        //        }
        //    }

        //    using (ZipFile zip = new ZipFile())
        //    {
        //        foreach (KeyValuePair<int, ModEntry> keyValuePair in _entries)
        //        {
        //            ModEntry entry = keyValuePair.Value;

        //            string fileNameWithoutExtension = GetValidName(entry.Name, _usedFileNames);
        //            string fileName = fileNameWithoutExtension + "." + entry.Extension;

        //            zip.AddEntry(fileName, entry.JSON);

        //            _usedFileNames.Add(fileNameWithoutExtension);
        //            _data.Add(new ModData(entry));
        //        }

        //        zip.AddEntry(CONFIG_NAME, JsonSerializer.ToJson(this));

        //        zip.Save(string.Format(@"{0}\{1}.mod", path, name));
        //    }
        //}

        //public static Mod Deserialize(string path)
        //{
        //    Dictionary<string, string> files = new Dictionary<string, string>();

        //    using (ZipFile zip = ZipFile.Read(path))
        //    {
        //        foreach (ZipEntry entry in zip)
        //        {
        //            using (MemoryStream stream = new MemoryStream())
        //            {
        //                entry.Extract(stream);

        //                stream.Position = 0;
        //                StreamReader sr = new StreamReader(stream);

        //                files.Add(Path.GetFileNameWithoutExtension(entry.FileName), sr.ReadToEnd());
        //            }
        //        }
        //    }

        //    if (!files.ContainsKey(CONFIG_NAME))
        //        throw new NullReferenceException("No config file in " + path);

        //    Mod mod = JsonSerializer.ToObject<Mod>(files[CONFIG_NAME]);
        //    files.Remove(CONFIG_NAME);

        //    return mod;
        //}
    }
}
