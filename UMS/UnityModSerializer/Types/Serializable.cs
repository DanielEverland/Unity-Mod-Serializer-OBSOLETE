using Newtonsoft.Json;
using UMS.Core;
using System;

namespace UMS.Types
{
    [Serializable]
    [JsonObject(MemberSerialization = MemberSerialization.OptIn)]
    public abstract class Serializable<TFrom, TTo> : IModSerializer<TFrom, TTo>
    {
        public Type NonSerializableType => typeof(TFrom);
        public Type SerializedType => typeof(TTo);
        public virtual int Priority => (int)Core.Priority.Medium;

        public virtual TFrom Deserialize() { throw new NotImplementedException(GetType().ToString()); }
    }
}