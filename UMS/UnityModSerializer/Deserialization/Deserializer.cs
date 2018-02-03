using System;
using System.IO;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;
using UnityEditor;
using UMS.Core;
using UMS.Serialization;
using static UMS.Serialization.CustomSerializers;

namespace UMS.Deserialization
{
    public class Deserializer
    {
        //public Deserializer()
        //{
        //    _deserializedObjects = new Dictionary<int, object>();
        //}

        //private static Deserializer _current;

        //private static Dictionary<int, object> _deserializedObjects;

        //[MenuItem(Utility.MENU_ITEM_ROOT + "/Deserialize")]
        //private static void DeserializeTest()
        //{
        //    CreateInstance();
        //    DeserializeObject(@"C:\Users\Daniel\Desktop\Test Mod.mod");
        //}
        //private static void CreateInstance()
        //{
        //    _current = new Deserializer();
        //}
        //public static bool Contains(int id)
        //{
        //    return _deserializedObjects.ContainsKey(id);
        //}
        //public static void DeserializeObject(string path)
        //{
        //    Mod mod = Mod.Deserialize(path);

        //    foreach (KeyValuePair<string, string> keyValuePair in mod.files)
        //    {
        //        Mod.ModData data = mod._data.Find(x => x.name == keyValuePair.Key);

        //        Debug.Log(keyValuePair.Key + " - " + keyValuePair.Value);
        //        if (_deserializedObjects.ContainsKey(data.ID))
        //            continue;

        //        object obj = JsonSerializer.ToObject(keyValuePair.Value, data.type);

        //        _deserializedObjects.Add(data.ID, obj);
        //    }

        //    foreach (SerializableGameObject go in _deserializedObjects.Values.Where(x => typeof(SerializableGameObject).IsAssignableFrom(x.GetType())))
        //    {
        //        GameObject gameObject = (GameObject)go;
        //    }
        //}
    }
}
