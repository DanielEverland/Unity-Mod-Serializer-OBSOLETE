using System;
using System.IO;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEditor;
using UMS.Core;
using UMS.Serialization;

namespace UMS.Deserialization
{
    public class Deserializer
    {
        [MenuItem(Utility.MENU_ITEM_ROOT + "/Deserialize")]
        private static void DeserializeTest()
        {
            DeserializeObject(@"C:\Users\Daniel\Desktop\Test Mod.mod");
        }
        public static void DeserializeObject(string path)
        {
            Mod mod = Mod.Deserialize(path);

            foreach (Core.Object obj in mod.Objects.Where(x => x.GetType().IsAssignableFrom(typeof(Core.Object))))
            {
                ObjectCreator.CreateObject(obj);
            }
        }
    }
}
