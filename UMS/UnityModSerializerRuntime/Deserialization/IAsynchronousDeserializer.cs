using System;

namespace UMS.Runtime.Deserialization
{
    public interface IAsynchronousDeserializer
    {
    }
    public interface IAsynchronousDeserializer<T> : IAsynchronousDeserializer
    {
        void AsynchronousDeserialization(Action<object> action, T serialized);
    }
}