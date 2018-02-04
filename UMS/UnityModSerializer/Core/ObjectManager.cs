using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UMS.Serialization;

namespace UMS.Core
{
    public static class ObjectManager
    {
        public static IDictionary<int, object> Data { get { return _data; } }

        private static Dictionary<int, object> _data;

        public static void Initialize()
        {
            _data = new Dictionary<int, object>();
        }
        public static int Add(object obj)
        {
            if (obj == null)
                throw new NullReferenceException("Object cannot be null");

            if (obj is IModSerializer serialized)
                return serialized.ID;

            int id = Utility.GetID(obj);

            if (!_data.ContainsKey(id))
            {
                _data.Add(id, null);

                _data[id] = CustomSerializers.SerializeObject(obj);
            }

            return id;
        }
    }
}
