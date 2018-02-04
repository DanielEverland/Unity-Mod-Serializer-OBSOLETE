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

        public static void Initialize()
        {
            configFile = new Config();
            filesToWrite = new Dictionary<string, string>();

            foreach (ObjectData data in ObjectManager.Data)
            {
                if (data.Object is IModEntry entry)
                {
                    AddToConfig(entry, data.ID);

                    string json = JsonSerializer.ToJson(data.Object);

                    filesToWrite.Add(Utility.ModEntryToFullName(entry), json);
                }
            }
        }
        private static void AddToConfig(IModEntry entry, int id)
        {
            configFile.Add(id, Utility.ModEntryToFullName(entry));
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
