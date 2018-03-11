using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UMS.Runtime.Core;
using UnityEditor;

namespace UMS.Serialization
{
    public static class SerializationUtility
    {
        public static List<ModPackage> GetAllPackages()
        {
            return new List<ModPackage>(AssetDatabase.FindAssets("t:ModPackage").Select(x =>
            {
                return AssetDatabase.LoadAssetAtPath<ModPackage>(AssetDatabase.GUIDToAssetPath(x));
            }));
        }
    }
}
