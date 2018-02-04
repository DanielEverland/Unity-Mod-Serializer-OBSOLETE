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
    public static class Serializer
    {
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
                UnityEngine.Object toSerialize = UnityEngine.Object.Instantiate(Selection.objects[i]);

                CustomSerializers.SerializeObject(toSerialize);
            }

            InitializeSerialization();

            Debug.Log("Finished serializing");
        }

        private static void Initialize()
        {
            AssemblyManager.Initialize();

            ObjectManager.Initialize();
            BehaviourManager.Initialize();
            CustomSerializers.Initialize();

            AssemblyManager.ExecuteReflection();
        }
        private static void InitializeSerialization()
        {
            Mod.Initialize();
            Mod.Serialize(@"C:/Users/Daniel/Desktop/New Mod.mod");
        }
    }
}
