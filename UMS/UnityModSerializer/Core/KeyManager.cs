using System.Collections.Generic;
using UMS.Deserialization;

namespace UMS.Core
{
    public static class KeyManager
    {
        public static IDictionary<int, string> Keys
        {
            get
            {
                if (_idKeys == null)
                    _idKeys = new Dictionary<int, string>();

                return _idKeys;
            }
        }

        private static Dictionary<int, string> _idKeys;

        public static T GetObject<T>(string key) where T : UnityEngine.Object
        {
            return Deserializer.GetObject<T>(key);
        }
        public static object GetObject(string key)
        {
            return Deserializer.GetObject(key);
        }
        public static bool Contains(object obj)
        {
            return Contains(IDManager.GetID(obj));
        }
        public static bool Contains(int id)
        {
            return Keys.ContainsKey(id);
        }
        public static string Get(object obj)
        {
            return Get(IDManager.GetID(obj));
        }
        public static string Get(int id)
        {
            if (Contains(id))
                return Keys[id];
            else
                return null;
        }
        public static void Add(object obj, string key)
        {
            Add(IDManager.GetID(obj), key);
        }
        public static void Add(int ID, string key)
        {
            if (!Keys.ContainsKey(ID))
            {
                Keys.Add(ID, key);
            }
        }
    }
}
