using Newtonsoft.Json;
using System;

namespace UMS.Core.Types
{
    public class Reference : Serializable<object, Reference>
    {
        public Reference() { }
        private Reference(object obj)
        {
            if (obj == null)
            {
                UnityEngine.Debug.LogWarning("Obj is null");
                throw new NullReferenceException();
            }

            ID = ObjectManager.Add(obj);
        }

        public int ID { get { return _id; } private set { _id = value; } }

        [JsonProperty]
        private int _id;

        public override Reference Serialize(object obj)
        {
            return new Reference(obj);
        }
        public static Reference Create(object obj)
        {
            if (obj == null)
                return null;

            return new Reference(obj);
        }
    }
}