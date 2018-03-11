﻿using Ionic.Zip;
using System.Collections.Generic;
using UMS.Runtime.Core;
using UMS.Runtime.Types;
using UnityEditor;
using UnityEngine;

namespace UMS.Serialization
{
    public static class Serializer
    {
        public static IDictionary<string, byte[]> ExtraFiles { get { return _extraFiles; } }
                
        private static Dictionary<string, byte[]> _extraFiles = new Dictionary<string, byte[]>();
        private static Dictionary<string, string> _filesToWrite;        
        private static HashSet<string> _usedNames;

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

                    string json = JsonSerializer.ToJson(keyValuePair.Value);
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
            _filesToWrite.Add(Utility.MANIFEST_NAME, JsonSerializer.ToJson(Manifest.Instance));

            using (ZipFile zip = new ZipFile())
            {
                foreach (KeyValuePair<string, string> file in _filesToWrite)
                {
                    zip.AddEntry(file.Key, file.Value);
                }

                foreach (KeyValuePair<string, byte[]> file in Serializer.ExtraFiles)
                {
                    zip.AddEntry(file.Key, file.Value);
                }

                zip.Save(path);
            }
        }
        public static void SerializePackage(ModPackage package)
        {
            if (package == null)
                throw new System.NullReferenceException();

            SerializePackages(package);
        }

        [MenuItem(itemName: Utility.MENU_ITEM_ROOT + "/Serialize All", priority = Utility.MENU_ITEM_PRIORITY)]
        private static void SerializeAll()
        {
            List<ModPackage> packages = SerializationUtility.GetAllPackages();

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
        public static void AddExtraFile(string filePath, byte[] data)
        {
            if (!_extraFiles.ContainsKey(filePath))
                _extraFiles.Add(filePath, null);

            _extraFiles[filePath] = data;
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
            Serialize(string.Format(@"{0}/{1}.mod", System.Environment.GetFolderPath(System.Environment.SpecialFolder.Desktop), package.name));
        }
        /// <summary>
        /// Adds a file to the Static Objects folder. Must be called during serialization
        /// </summary>
        /// <param name="path">Path relative to Static Objects folder</param>
        public static string AddStaticObject(string path, byte[] data)
        {
            string fullPath = StaticObjects.FolderPath + path;

            AddExtraFile(fullPath, data);

            return fullPath;
        }
    }
}