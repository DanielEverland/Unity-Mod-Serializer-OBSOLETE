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

        [MenuItem(itemName: Utility.MENU_ITEM_ROOT + "/Serialize Selection", priority = Utility.MENU_ITEM_PRIORITY)]
        private static void SerializeSelection()
        {
            if (Selection.objects.Length == 0)
            {
                Debug.LogWarning("No objects selected");
                return;
            }

            Initialize();

            for (int i = 0; i < Selection.objects.Length; i++)
            {
                Object toSerialize = CloneManager.GetClone(Selection.objects[i]);

                CustomSerializers.SerializeObject(toSerialize);
            }

            InitializeSerialization();

            CoreManager.FinishedSerialization();
            Debug.Log("Finished serializing");
        }
        public static void AddExtraFile(string filePath, byte[] data)
        {
            if (!extraFiles.ContainsKey(filePath))
                extraFiles.Add(filePath, null);

            extraFiles[filePath] = data;
        }
        private static void Initialize()
        {
            CoreManager.Initialize();
        }
        private static void InitializeSerialization()
        {
            Mod.Initialize();
            Mod.Serialize(@"C:/Users/Daniel/Desktop/New Mod.mod");
        }
    }
}