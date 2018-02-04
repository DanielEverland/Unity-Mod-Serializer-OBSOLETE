using Ionic.Zip;
using System.Collections.Generic;
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

            foreach (ObjectData data in ObjectManager.Data)
            {
                if (data.Object is IModEntry entry)
                {
                    string json = JsonSerializer.ToJson(data.Object);
                    string name = Utility.GetValidName(entry.FileName, usedNames);
                    string extension = Utility.SanitizeExtension(entry.Extension);
                    string selectedName = string.Format("{0}.{1}", name, extension);

                    AddToConfig(data.ID, selectedName);

                    usedNames.Add(name);

                    filesToWrite.Add(selectedName, json);
                }
                else
                {
                    UnityEngine.Debug.LogWarning("Tried to add " + data + " as a reference type, but it doesn't implement IModEntry");
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
