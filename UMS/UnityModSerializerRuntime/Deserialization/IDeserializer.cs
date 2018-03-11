using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace UMS.Runtime.Deserialization
{
    public interface ICustomDeserializer { }
    public interface IDeserializer<TObject, TSerializable> : ICustomDeserializer
    {
        TObject Deserialize(TSerializable serializable);
    }
    public interface IDeserializer<TSerializable> : ICustomDeserializer
    {
        void Deserialize(TSerializable serializable);
    }
}
