using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using UnityEngine;
using UnityEditor;
using UnityEditor.Callbacks;
using UMS.Serialization;

namespace UMS.Editor
{
    public static class BuildHandler
    {
        private static string _pathToRootBuildFolder;
        private static string _pathToRootModsFolder;
        private static string _pathToCoreMods;

        [PostProcessBuild()]
        private static void PostBuild(BuildTarget target, string pathToBuiltProject)
        {
            _pathToRootBuildFolder = string.Format("{0}/{1}_Data", Path.GetDirectoryName(pathToBuiltProject), Path.GetFileNameWithoutExtension(pathToBuiltProject));

            BuildMods();
        }
        private static void BuildMods()
        {
            CreateModsDirectory();
            BuildCoreMods();
        }
        private static void BuildCoreMods()
        {
            CreateCoreModsFolder();

            string[] guids = AssetDatabase.FindAssets("t:ModPackage");

            for (int i = 0; i < guids.Length; i++)
            {
                string path = AssetDatabase.GUIDToAssetPath(guids[i]);

                Object obj = AssetDatabase.LoadAssetAtPath<Object>(path);

                if(obj is ModPackage package)
                {
                    BuildMod(package);
                }
            }
        }
        private static void BuildMod(ModPackage package)
        {
            try
            {
                Serializer.SerializePackage(package, _pathToCoreMods);
            }
            catch (System.Exception)
            {
                Debug.LogWarning("Failed to serialize " + package);
            }
            
        }
        private static void CreateCoreModsFolder()
        {
            _pathToCoreMods = string.Format("{0}/{1}", _pathToRootModsFolder, Settings.CoreFolderName);

            Directory.CreateDirectory(_pathToCoreMods);
        }
        private static void CreateModsDirectory()
        {
            if(Settings.BuildModFolderLocation != "")
            {
                _pathToRootModsFolder = string.Format("{0}/{1}/{2}", _pathToRootBuildFolder, Settings.BuildModFolderLocation, Settings.FolderName);
            }
            else
            {
                _pathToRootModsFolder = string.Format("{0}/{1}", _pathToRootBuildFolder, Settings.FolderName);
            }

            Directory.CreateDirectory(_pathToRootModsFolder);
        }
    }
}