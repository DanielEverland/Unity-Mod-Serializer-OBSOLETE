using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using UMS.Deserialization;
using UnityEngine;

namespace UMS.Core
{
    public static class Converter
    {
        private static List<IModSerializer> _serializers;

        public static void Initialize()
        {
            _serializers = new List<IModSerializer>();

            AssemblyManager.OnLoadType += Analyze;
        }

        private static MethodInfo QueryForSerialization(Func<IModSerializer, bool> predicate)
        {
            Debugging.Info("Processing Serialization Query for " + predicate);

            return Query(predicate, x => { return x.SerializedType; }, x => { return x.NonSerializableType; });
        }
        private static MethodInfo QueryForDeserialization(Func<IModSerializer, bool> predicate)
        {
            Debugging.Info("Processing Deserialization Query for " + predicate);

            return Query(predicate, x => { return x.NonSerializableType; }, x => { return x.SerializedType; });
        }
        public static T SerializeObject<T>(object fromObject)
        {
            if (fromObject == null)
                return default(T);
            
            MethodInfo info = QueryForSerialization(x => x.NonSerializableType.IsAssignableFrom(fromObject.GetType()) && x.SerializedType.IsAssignableFrom(typeof(T)));

            info = Utility.ConvertMethodInfo(AssemblyManager.RuntimeAssembly, info);

            return (T)info.Invoke(null, new object[1] { fromObject });
        }
        public static object SerializeObject(object fromObject)
        {
            if (fromObject == null)
                return null;
            
            MethodInfo info = QueryForSerialization(x => x.NonSerializableType.IsAssignableFrom(fromObject.GetType()));

            info = Utility.ConvertMethodInfo(AssemblyManager.RuntimeAssembly, info);

            return info.Invoke(null, new object[1] { fromObject });
        }
        public static void DeserializeObject<T>(object serialized, Action<T> callback)
        {
            if (serialized == null)
            {
                callback(default(T));
                return;
            }

            if (!CanDeserialize(serialized.GetType()))
                throw new InvalidOperationException("Cannot deserialize " + serialized + ", because it implements a custom deserializer");

            try
            {
                Debugging.Info("Deserializing " + serialized);

                Deserialize(x => x.SerializedType.IsAssignableFrom(serialized.GetType()) && x.NonSerializableType.IsAssignableFrom(typeof(T)),
                                            serialized, x => { callback((T)x); });
            }
            catch (Exception)
            {
                Debug.Log(string.Format("Datadump: ({0}){1} - (2)", serialized.GetType(), serialized, typeof(T)));
                throw;
            }
        }
        public static void DeserializeObject(object serialized, Action<object> callback)
        {
            if (serialized == null)
            {
                callback(null);
                return;
            }

            if (!CanDeserialize(serialized.GetType()))
                throw new InvalidOperationException("Cannot deserialize " + serialized + ", because it implements a custom deserializer");

            try
            {
                Debugging.Info("Deserializing " + serialized);

                Deserialize(x => x.SerializedType.IsAssignableFrom(serialized.GetType()), serialized, callback);
            }
            catch (Exception)
            {
                Debug.Log(string.Format("Datadump: ({0}){1} - (2)", serialized.GetType(), serialized));
                throw;
            }
        }
        private static void Deserialize(Func<IModSerializer, bool> predicate, object serialized, Action<object> callback)
        {
            if (serialized is IAsynchronousDeserializer)
            {
                bool failed = !TryCallGenericInterface(callback, serialized);

                if (failed)
                {
                    Debugging.Error("Unsuccessfully attempted to asynchronously call " + serialized);
                }
            }
            else
            {
                MethodInfo info = QueryForDeserialization(predicate);

                callback(info.Invoke(serialized, null));
            }
        }
        private static bool Compare(IModSerializer a, IModSerializer b)
        {
            return a.NonSerializableType == b.NonSerializableType && a.SerializedType == b.SerializedType;
        }
        private static void Analyze(Type type)
        {
            if (typeof(IModSerializer).IsAssignableFrom(type) && !type.IsAbstract)
            {
                IModSerializer serializer = Activator.CreateInstance(type) as IModSerializer;

                if (_serializers.Any(x => Compare(x, serializer)))
                {
                    IModSerializer other = _serializers.Find(x => Compare(x, serializer));

                    if (other.Priority < serializer.Priority)
                    {
                        _serializers.Remove(other);
                        _serializers.Add(serializer);
                    }
                }
                else
                {
                    _serializers.Add(serializer);
                }
            }
        }
        public static bool ParametersMatch(ParameterInfo[] parameters, Type[] types)
        {
            List<Type> matchCollection = new List<Type>(types);

            foreach (ParameterInfo info in parameters)
            {
                if (matchCollection.Contains(info.ParameterType))
                {
                    matchCollection.Remove(info.ParameterType);
                }
                else
                {
                    return false;
                }
            }

            return matchCollection.Count == 0;
        }
        public static MethodInfo GetMethod(Type fromType, Type returnType)
        {
            foreach (MethodInfo method in fromType.GetMethods(BindingFlags.Static | BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic))
            {
                if (method.ReturnType != returnType)
                    continue;

                return method;
            }

            throw new NotImplementedException(string.Format("FromType: {0} - ReturnType: {1}", fromType, returnType));
        }
        public static MethodInfo Query(Func<IModSerializer, bool> predicate, Func<IModSerializer, Type> toType, Func<IModSerializer, Type> returnType)
        {
            if (_serializers == null)
                throw new NullReferenceException("Serializers not initialized");

            List<IModSerializer> validSerializers = new List<IModSerializer>(_serializers.Where(predicate));

            if (validSerializers.Count == 0)
                throw new NotImplementedException("Couldn't find any valid serializers ");

            IModSerializer selectedSerializer = Utility.GetMostInherited(validSerializers);

            Debugging.Verbose("Found top-most serializer " + selectedSerializer);

            return GetMethod(selectedSerializer.GetType(), toType(selectedSerializer));
        }
        public static bool TryCallGenericInterface(Action<object> callback, object target)
        {
            Type[] interfaces = target.GetType().GetInterfaces();

            Debugging.Verbose("Getting interfaces from " + target.GetType());

            for (int i = 0; i < interfaces.Length; i++)
            {
                Type currentInterface = interfaces[i];

                Debugging.Verbose("Checking interface " + currentInterface);

                if (typeof(IAsynchronousDeserializer).IsAssignableFrom(currentInterface) && currentInterface.IsGenericType)
                {
                    MethodInfo function = target.GetType().GetMethod("AsynchronousDeserialization");
                    ParameterInfo[] arguments = function.GetParameters();

                    Type firstParameterType = arguments[0].ParameterType;

                    Debugging.Verbose("Arg count: " + arguments.Length);
                    for (int x = 0; x < arguments.Length; x++)
                    {
                        Debugging.Verbose("Argument " + x + ": " + arguments[x]);
                    }

                    if (!callback.Method.GetParameters()[0].ParameterType.IsAssignableFrom(firstParameterType))
                    {
                        Debug.LogWarning("Cannot assign " + callback + " from " + firstParameterType);
                        break;
                    }

                    Debugging.Info("Successfully calling asynchronous function");
                    function.Invoke(target, new object[1] { callback });
                    return true;
                }
                else
                {
                    Debugging.Verbose("Skipping interface " + currentInterface + " IsGeneric: " + currentInterface.IsGenericType);
                }
            }

            return false;
        }
        public static bool CanDeserialize(Type type)
        {
            return !type.GetInterfaces().Any(x => x == typeof(ICustomDeserializer));
        }
        public static bool CanSerialize(Type type)
        {
            if (type.GetInterfaces().Any(x => x == typeof(ICustomSerializer)))
                return false;

            return _serializers.Any(x => x.NonSerializableType.IsAssignableFrom(type) && !IsPrimitive(x.NonSerializableType));
        }
        public static bool IsPrimitive(Type type)
        {
            return type.IsPrimitive || type == typeof(System.Object);
        }
    }
}
