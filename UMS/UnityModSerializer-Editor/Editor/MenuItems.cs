using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using UMS.Core;
using UMS.Deserialization;
using UnityEditor;
using UnityEngine;

namespace UMS.Editor
{
    public static class MenuItems
    {
        [MenuItem(EditorUtilities.MENU_ITEM_ROOT + "/Deserialize Desktop", priority = EditorUtilities.MENU_ITEM_PRIORITY)]
        private static void DeserializeDesktop()
        {
            string[] desktopFiles = Directory.GetFiles(Environment.GetFolderPath(Environment.SpecialFolder.Desktop));
            IEnumerable<string> modFiles = desktopFiles.Where(x => Path.GetExtension(x) == ".mod");

            if (modFiles.Count() == 0)
                Debug.LogWarning("No mod files on desktop");

            Deserializer.Initialize();

            foreach (string path in modFiles)
            {
                Deserializer.DeserializePackage(path);
            }
        }
    }
}
