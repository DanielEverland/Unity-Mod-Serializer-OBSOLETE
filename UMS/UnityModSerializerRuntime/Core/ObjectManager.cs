using System;
using System.Collections.Generic;
using UMS.Core.Types;
using UMS.Serialization;

namespace UMS.Runtime.Core
{
    public static class ObjectManager
    {
        public static IDictionary<int, object> Data { get { return _data; } }

        private static Dictionary<int, object> _data;
        private static Dictionary<int, string> _idKeys;

        public static void Initialize()
        {
            _data = new Dictionary<int, object>();
            _idKeys = new Dictionary<int, string>();
        }
        public static string GetKey(int ID)
        {
            if (_idKeys.ContainsKey(ID))
            {
                return _idKeys[ID];
            }

            return null;
        }
        public static void AddKey(object obj, string key)
        {
            AddKey(IDManager.GetID(obj), key);
        }
        public static void AddKey(int ID, string key)
        {
            if (!_idKeys.ContainsKey(ID))
            {
                _idKeys.Add(ID, key);
            }
        }
        public static int Add(object obj)
        {
            if (obj == null)
                throw new NullReferenceException("Object cannot be null");

            if (obj is Reference reference)
                return reference.ID;

            int id = Utility.GetID(obj);

            if (!_data.ContainsKey(id))
            {
                //The two lines cannot be merged!
                //We add the ID before the value because SerializeObject might call constructors that checks whether an ID exists
                //This is done to prevent infinite loops. Once an object ID has been added, we only want to serialize it once, and use its ID as a reference
                _data.Add(id, null);

                _data[id] = CustomSerializers.SerializeObject(obj);

                if (!(_data[id] is IModEntry))
                    throw new ArgumentException("Tried to add " + _data[id].GetType() + " as a reference type, but it does not implement IModEntry");
            }

            return id;
        }
    }
}