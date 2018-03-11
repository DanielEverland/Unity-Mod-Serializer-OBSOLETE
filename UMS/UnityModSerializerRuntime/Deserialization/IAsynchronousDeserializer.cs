using System;

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