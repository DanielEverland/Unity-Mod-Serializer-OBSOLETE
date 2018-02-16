using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace UMS.Deserialization
{
    public interface IAsynchronousDeserializer
    {
        
    }
    public interface IAsynchronousDeserializer<T> : IAsynchronousDeserializer
    {
        void AsynchronousDeserialization(Action<object> action, T serialized);
    }
}
