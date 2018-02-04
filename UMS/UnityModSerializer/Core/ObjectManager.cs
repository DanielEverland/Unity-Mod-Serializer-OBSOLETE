using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace UMS.Core
{
    public static class ObjectManager
    {
        public static IEnumerable<ObjectData> Data { get { return _data; } }

        private static HashSet<ObjectData> _data;

        public static void Initialize()
        {
            _data = new HashSet<ObjectData>();
        }
        public static int Add(object obj)
        {
            if (obj == null)
                throw new NullReferenceException("Object cannot be null");

            ObjectData data = new ObjectData(obj);

            if (!_data.Contains(data))
            {
                _data.Add(data);
            }

            return data.ID;
        }
    }
    public class ObjectData
    {
        private ObjectData() { }
        public ObjectData(object obj)
        {
            _id = Utility.GetID(obj);
            _obj = obj;
        }

        public int ID { get { return _id; } }
        public object Object { get { return _obj; } }

        private readonly int _id;
        private readonly object _obj;

        public override bool Equals(object obj)
        {
            if (obj == null)
                return false;

            if (obj is ObjectData data)
            {
                return data.ID == this.ID;
            }

            return false;
        }
        public override int GetHashCode()
        {
            return _id.GetHashCode();
        }
        public override string ToString()
        {
            return string.Format("{0}: {1}", _id, _obj);
        }
    }
}
