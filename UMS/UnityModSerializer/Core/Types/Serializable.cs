using Newtonsoft.Json;
using System;
using UMS.Serialization;

namespace UMS.Core.Types
{
    [Serializable]
    [JsonObject(MemberSerialization = MemberSerialization.OptIn)]
    public abstract class Serializable<TFrom, TTo> : IModSerializer<TFrom, TTo>
    {
        public Type NonSerializableType => typeof(TFrom);
        public Type SerializedType => typeof(TTo);
        public virtual int Priority => (int)Core.Priority.Medium;

        public Serializable() { }
        public Serializable(TFrom obj)
        {
            _id = ObjectManager.Add(obj);
        }

        public int ID { get { return _id; } protected set { _id = value; } }

        [JsonProperty]
        private int _id;

        public virtual TFrom Deserialize(TTo serializable) { throw new NotImplementedException(GetType().ToString()); }
        public virtual TTo Serialize(TFrom obj) { throw new NotImplementedException(GetType().ToString()); }
    }
}