using System;
using System.IO;
using System.Linq;
using System.Text;
using System.Reflection;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using UMS.Core;
using UMS.Behaviour;

namespace UMS.Serialization
{
    public static class Deserializer
    {
        [MenuItem(itemName: Utility.MENU_ITEM_ROOT + "/Deserialize", priority = Utility.MENU_ITEM_PRIORITY)]
        private static void Deserialize()
        {
            Initialize();
            InitializeDeserialization();

            Debug.Log("Finished deserializing");
        }

        private static void Initialize()
        {
            CoreManager.Initialize();
        }
        private static void InitializeDeserialization()
        {
            Mod.Deserialize(@"C:/Users/Daniel/Desktop/New Mod.mod");
        }
    }
}
