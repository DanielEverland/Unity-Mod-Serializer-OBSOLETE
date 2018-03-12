using System;

namespace UMS.Core
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
        TObject Deserialize();
    }
    public interface ISerializer<TObject> : ICustomSerializer
    {
        void Serialize(TObject obj);
    }
}