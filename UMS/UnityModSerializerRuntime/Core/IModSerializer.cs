using System;

namespace UMS.Runtime.Core
{
    public interface IModSerializer
    {
        int Priority { get; }

        Type NonSerializableType { get; }
        Type SerializedType { get; }
    }
    public interface ICustomSerializer { }

    public interface IModSerializer<TObject, TSerializable> : IModSerializer
    {
        TSerializable Serialize(TObject obj);
        TObject Deserialize(TSerializable serializable);
    }
    public interface ISerializer<TObject, TSerializable> : ICustomSerializer
    {
        TSerializable Serialize(TObject obj);
    }
    public interface ISerializer<TObject> : ICustomSerializer
    {
        void Serialize(TObject obj);
    }
}