using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UMS.Core;
using UMS.Editor;
using UnityEditor;

namespace UMS.Deserialization
{
    /// <summary>
    /// Implements functions that can be performed to fake deserialization in the Unity Editor
    /// </summary>
    public static class EditorSession
    {
        public static void Load()
        {
            Deserializer.Initialize();

            string[] guids = AssetDatabase.FindAssets("t:ModPackage");

            foreach (string id in guids)
            {
                string path = AssetDatabase.GUIDToAssetPath(id);
                ModPackage package = AssetDatabase.LoadAssetAtPath<ModPackage>(path);

                foreach (ModPackage.ObjectEntry entry in package.ObjectEntries)
                {
                    Deserializer.AddObject(entry.Key, entry.Object);
                }
            }
        }
    }
}