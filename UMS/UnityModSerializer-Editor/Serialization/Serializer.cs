using Ionic.Zip;
using UMS.Core;
using UMS.Editor;
using UnityEngine;
using System.Collections.Generic;
using UnityEditor;

namespace UMS.Serialization
{
    public static class Serializer
    {
        private static Dictionary<string, string> _filesToWrite;        
        private static HashSet<string> _usedNames;
        private static string _currentPath;

        public static int AddObject(object obj)
        {
            return IDManager.AddObject(obj);
        }
        public static void CreateManifest()
        {
            Manifest.Instance = new Manifest();

            _filesToWrite = new Dictionary<string, string>();
            _usedNames = new HashSet<string>();

            foreach (KeyValuePair<int, object> keyValuePair in IDManager.Data)
            {
                if (keyValuePair.Value is IModEntry entry)
                {
                    string preferredName = string.Format("{0}/{1}", entry.FolderName, entry.FileName);

                    string json = Json.ToJson(keyValuePair.Value);
                    string name = Utility.GetValidName(preferredName, entry.Extension, _usedNames);
                    string extension = Utility.SanitizeExtension(entry.Extension);

                    AddToManifest(keyValuePair.Key, name);

                    _usedNames.Add(name);

                    _filesToWrite.Add(name, json);
                }
                else
                {
                    UnityEngine.Debug.LogWarning("Tried to add " + keyValuePair.Value + " as a reference type, but it doesn't implement IModEntry");
                }
            }
        }
        private static void AddToManifest(int id, string name)
        {
            Manifest.Instance.Add(id, name, KeyManager.Get(id));
        }
        public static void Serialize(string path)
        {
            _filesToWrite.Add(Utility.MANIFEST_NAME, Json.ToJson(Manifest.Instance));

            using (ZipFile zip = new ZipFile())
            {
                foreach (KeyValuePair<string, string> file in _filesToWrite)
                {
                    zip.AddEntry(file.Key, file.Value);
                }

                foreach (KeyValuePair<string, byte[]> file in StaticObjects.ExtraFiles)
                {
                    zip.AddEntry(file.Key, file.Value);
                }

                zip.Save(path);
            }
        }
        public static void SerializePackage(ModPackage package, string pathToFolder)
        {
            if (package == null)
                throw new System.NullReferenceException();
            
            _currentPath = pathToFolder;
            
            SerializePackages(package);
        }
        [MenuItem(itemName: EditorUtilities.MENU_ITEM_ROOT + "/Serialize All", priority = EditorUtilities.MENU_ITEM_PRIORITY)]
        private static void SerializeAll()
        {
            _currentPath = System.Environment.GetFolderPath(System.Environment.SpecialFolder.Desktop);

            List<ModPackage> packages = EditorUtilities.GetAllPackages();

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

            KeyManager.Add(toSerialize, entry.Key);

            object serialized = Converter.SerializeObject(toSerialize);
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
            CreateManifest();
            Serialize(string.Format(@"{0}/{1}.mod", _currentPath, package.name));
        }
    }
}