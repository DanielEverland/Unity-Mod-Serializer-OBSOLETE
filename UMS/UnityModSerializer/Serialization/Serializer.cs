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

                Serialize(toSerialize);
            }

            //Mod.Serialize(@"C:\Users\Daniel\Desktop");

            Debug.Log("Finished serializing");
        }
        [MenuItem(Utility.MENU_ITEM_ROOT + "/Deserialize")]
        private static void Deserialize()
        {
            Initialize();
            
            string text = File.ReadAllText(@"C:/Users/Daniel/Desktop/JSON.txt");

            object obj = JsonSerializer.ToObject(text);

            Debug.Log(obj.GetType());
        }
        //private static void StartNewSerializer()
        //{


        //    new Mod("Test Mod");
        //}

        private static void Initialize()
        {
            AssemblyManager.Initialize();

            BehaviourManager.Initialize();
            CustomSerializers.Initialize();

            AssemblyManager.ExecuteReflection();
        }
        
        private static void Serialize(object obj)
        {
            object serialized = CustomSerializers.SerializeObject(obj);

            string json = JsonSerializer.ToJson(serialized);

            File.WriteAllText(@"C:/Users/Daniel/Desktop/JSON.txt", json);
        }
    }
}
