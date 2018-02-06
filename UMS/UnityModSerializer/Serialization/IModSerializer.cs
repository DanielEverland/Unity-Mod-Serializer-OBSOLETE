using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace UMS.Serialization
{
    public interface IModSerializer
    {
        int ID { get; }
        int Priority { get; }

        Type NonSerializableType { get; }
        Type SerializedType { get; }
    }
    public interface ICustomSerializer { }
    public interface ICustomDeserializer { }
    public interface IModSerializer<TObject, TSerializable> : IModSerializer
    {
        TSerializable Serialize(TObject obj);
        TObject Deserialize(TSerializable serializable);
    }
    public interface ISerializer<TObject, TSerializable> : ICustomSerializer
    {
        TSerializable Serialize(TObject obj);
    }
    public interface IDeserializer<TObject, TSerializable> : ICustomDeserializer
    {
        TObject Deserialize(TSerializable serializable);
    }
    public interface IDeserializer<TSerializable> : ICustomDeserializer
    {
        void Deserialize(TSerializable serializable);
    }
    public interface ISerializer<TObject> : ICustomSerializer
    {
        void Serialize(TObject obj);
    }
}
