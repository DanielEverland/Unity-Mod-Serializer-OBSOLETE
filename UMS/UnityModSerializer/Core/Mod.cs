using Ionic.Zip;
using System.Collections.Generic;
using System.IO;
using UMS.Serialization;

namespace UMS.Core
{
    public static class Mod
    {
        private const string CONFIG_NAME = "config";
        
        private static Config configFile;
        private static Dictionary<string, string> filesToWrite;
        private static HashSet<string> usedNames;
             
        public static void Initialize()
        {
            configFile = new Config();
            filesToWrite = new Dictionary<string, string>();
            usedNames = new HashSet<string>();

            foreach (KeyValuePair<int, object> keyValuePair in ObjectManager.Data)
            {
                if (keyValuePair.Value is IModEntry entry)
                {
                    string json = JsonSerializer.ToJson(keyValuePair.Value);
                    string name = Utility.GetValidName(entry.FileName, usedNames);
                    string extension = Utility.SanitizeExtension(entry.Extension);
                    string selectedName = string.Format("{0}.{1}", name, extension);

                    AddToConfig(keyValuePair.Key, selectedName);

                    usedNames.Add(name);

                    filesToWrite.Add(selectedName, json);
                }
                else
                {
                    UnityEngine.Debug.LogWarning("Tried to add " + keyValuePair.Value + " as a reference type, but it doesn't implement IModEntry");
                }
            }
        }
        private static void AddToConfig(int id, string name)
        {
            configFile.Add(id, name);
        }
        public static void Serialize(string path)
        {
            filesToWrite.Add(CONFIG_NAME, JsonSerializer.ToJson(configFile));

            using (ZipFile zip = new ZipFile())
            {
                foreach (KeyValuePair<string, string> file in filesToWrite)
                {
                    zip.AddEntry(file.Key, file.Value);
                }

                zip.Save(path);
            }
        }
        public static Dictionary<int, object> Deserialize(string path)
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

                        files.Add(entry.FileName, sr.ReadToEnd());
                    }
                }
            }

            if (!files.ContainsKey(CONFIG_NAME))
                throw new System.NullReferenceException("No config file in " + path);

            Config config = JsonSerializer.ToObject<Config>(files[CONFIG_NAME]);
            files.Remove(CONFIG_NAME);

            Dictionary<int, object> deserializedObjects = new Dictionary<int, object>();
            foreach (KeyValuePair<string, string> file in files)
            {
                int id = -1;

                try
                {
                    id = config.data.Find(x => x.localPath == file.Key).id;
                }
                catch (System.Exception)
                {
                    UnityEngine.Debug.LogError("Config file didn't contain " + file.Key);
                    throw;
                }
                
                object obj = JsonSerializer.ToObject(file.Value);

                UnityEngine.Debug.Log(id + " - " + file.Value.Length);
                deserializedObjects.Add(id, obj);
            }

            return deserializedObjects;
        }
    }
}
