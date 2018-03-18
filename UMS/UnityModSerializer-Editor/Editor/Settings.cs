using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;
using UnityEditor;

namespace UMS.Editor
{
    public class Settings : ScriptableObject
    {
        public static string BuildModFolderLocation { get { return Instance._builtModFolderLocation; } }
        public static string FolderName { get { return Instance._folderName; } }
        public static string CoreFolderName { get { return Instance._coreFolderName; } }

        public static Settings Instance
        {
            get
            {
                if (_instance == null)
                    _instance = GetSettings();

                return _instance;
            }
        }
        private static Settings _instance;

        [SerializeField]
        [Tooltip("The path relative to the data folder where mods should be located")]
        private string _builtModFolderLocation = "";
        [SerializeField]
        [Tooltip("The name of the folder")]
        private string _folderName = "Mods";
        [SerializeField]
        [Tooltip("The name of the subfolder where the core files will reside")]
        private string _coreFolderName = "Core";

        private static Settings GetSettings()
        {
            Settings toReturn;

            if(SearchForSettings(out toReturn))
            {
                return toReturn;
            }
            else
            {
                return CreateNewSettings();
            }
        }
        private static bool SearchForSettings(out Settings settings)
        {
            string[] guids = AssetDatabase.FindAssets("t:Settings");

            for (int i = 0; i < guids.Length; i++)
            {
                string path = AssetDatabase.GUIDToAssetPath(guids[i]);
                Object obj = AssetDatabase.LoadAssetAtPath<Object>(path);

                if(obj is Settings)
                {
                    settings = obj as Settings;
                    return true;
                }
            }

            settings = null;
            return false;
        }
        private static Settings CreateNewSettings()
        {
            Settings newSettings = CreateInstance<Settings>();
            newSettings.name = "UMS Settings";

            Selection.activeObject = newSettings;

            AssetDatabase.CreateAsset(newSettings, "Assets/" + newSettings.name + ".asset");

            Debug.LogWarning("Couldn't find settings. Creating new object!", newSettings);
            Debug.Log("Feel free to move me somewhere else in your project", newSettings);

            return newSettings;
        }
    }
}