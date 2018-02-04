using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace UMS.Serialization
{
    public interface IModSerializer
    {
        int Priority { get; }

        Type NonSerializableType { get; }
        Type SerializedType { get; }
    }
    public interface IModSerializer<TObject, TSerializable> : IModSerializer
    {
        TSerializable Serialize(TObject obj);
        TObject Deserialize(TSerializable serializable);
    }
    public interface ISerializer<TObject, TSerializable>
    {
        TSerializable Serialize(TObject obj);
    }
    public interface IDeserializer<TObject, TSerializable>
    {
        TObject Deserialize(TSerializable serializable);
    }
    public interface IDeserializer<TSerializable>
    {
        void Deserialize(TSerializable serializable);
    }
    public interface ISerializer<TObject>
    {
        void Serialize(TObject obj);
    }
}
