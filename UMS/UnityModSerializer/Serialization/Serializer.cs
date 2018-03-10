using System.Collections.Generic;
using UMS.Core;
using UnityEditor;
using UnityEngine;

namespace UMS.Serialization
{
    public static class Serializer
    {
        public static IDictionary<string, byte[]> ExtraFiles { get { return extraFiles; } }

        private static Dictionary<string, byte[]> extraFiles = new Dictionary<string, byte[]>();

        public static void SerializePackage(ModPackage package)
        {
            if (package == null)
                throw new System.NullReferenceException();

            SerializePackages(package);
        }

        [MenuItem(itemName: Utility.MENU_ITEM_ROOT + "/Serialize All", priority = Utility.MENU_ITEM_PRIORITY)]
        private static void SerializeAll()
        {
            List<ModPackage> packages = Utility.GetAllPackages();

            if (packages.Count == 0)
            {
                Debug.LogWarning("Found no packages");
                return;
            }

            SerializePackages(packages.ToArray());
        }

        private static void SerializePackages(params ModPackage[] packages)
        {
            foreach (ModPackage package in packages)
            {
                Initialize();

                foreach (ModPackage.ObjectEntry entry in package.ObjectEntries)
                {
                    AddEntry(entry);
                }

                Complete(package);
            }
        }
        private static void AddEntry(ModPackage.ObjectEntry entry)
        {
            Object toSerialize = CloneManager.GetClone(entry.Object);

            ObjectManager.AddKey(toSerialize, entry.Key);

            object serialized = CustomSerializers.SerializeObject(toSerialize);
        }
        public static void AddExtraFile(string filePath, byte[] data)
        {
            if (!extraFiles.ContainsKey(filePath))
                extraFiles.Add(filePath, null);

            extraFiles[filePath] = data;
        }
        private static void Complete(ModPackage package)
        {
            InitializeSerialization(package);

            CoreManager.FinishedSerialization();
            Debug.Log("Serialized " + package.name);
        }
        private static void Initialize()
        {
            CoreManager.Initialize();
        }
        private static void InitializeSerialization(ModPackage package)
        {
            Mod.Initialize();
            Mod.Serialize(string.Format(@"{0}/{1}.mod", System.Environment.GetFolderPath(System.Environment.SpecialFolder.Desktop), package.name));
        }
    }
}