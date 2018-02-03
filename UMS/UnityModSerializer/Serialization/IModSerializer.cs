using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace UMS.Serialization
{
    public interface IModSerializer
    {
        Type NonSerializableType { get; }
        Type SerializedType { get; }
    }
    public interface IModSerializer<TObject, TSerializable> : IModSerializer
    {
        TSerializable Serialize(TObject obj);
        TObject Deserialize(TSerializable serializable);
    }
}
