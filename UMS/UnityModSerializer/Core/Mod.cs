using Ionic.Zip;
using System.Collections.Generic;
using System.IO;
using UMS.Serialization;

namespace UMS.Core
{
    public static class Mod
    {
        public static Config ConfigFile { get; private set; }

        private const string CONFIG_NAME = "config";

        private static Dictionary<string, string> filesToWrite;
        private static HashSet<string> usedNames;

        public static void Initialize()
        {
            ConfigFile = new Config();
            filesToWrite = new Dictionary<string, string>();
            usedNames = new HashSet<string>();

            foreach (KeyValuePair<int, object> keyValuePair in ObjectManager.Data)
            {
                if (keyValuePair.Value is IModEntry entry)
                {
                    string json = JsonSerializer.ToJson(keyValuePair.Value);
                    string name = Utility.GetValidName(entry.FileName, entry.Extension, usedNames);
                    string extension = Utility.SanitizeExtension(entry.Extension);

                    AddToConfig(keyValuePair.Key, name);

                    usedNames.Add(name);

                    filesToWrite.Add(name, json);
                }
                else
                {
                    UnityEngine.Debug.LogWarning("Tried to add " + keyValuePair.Value + " as a reference type, but it doesn't implement IModEntry");
                }
            }
        }
        private static void AddToConfig(int id, string name)
        {
            ConfigFile.Add(id, name);
        }
        public static void Serialize(string path)
        {
            filesToWrite.Add(CONFIG_NAME, JsonSerializer.ToJson(ConfigFile));

            using (ZipFile zip = new ZipFile())
            {
                foreach (KeyValuePair<string, string> file in filesToWrite)
                {
                    zip.AddEntry(file.Key, file.Value);
                }

                zip.Save(path);
            }
        }
        public static Dictionary<string, string> Deserialize(string path)
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

            ConfigFile = JsonSerializer.ToObject<Config>(files[CONFIG_NAME]);
            files.Remove(CONFIG_NAME);

            return files;
        }
    }
}