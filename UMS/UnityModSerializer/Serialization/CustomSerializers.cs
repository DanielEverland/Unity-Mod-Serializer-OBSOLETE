using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using UMS.Core;
using UnityEngine;

namespace UMS.Serialization
{
    public static class CustomSerializers
    {
        private static List<IModSerializer> serializers;

        public static void Initialize()
        {
            serializers = new List<IModSerializer>();

            AssemblyManager.OnLoadType += Analyze;
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

                if (serializers.Any(x => Compare(x, serializer)))
                {
                    IModSerializer other = serializers.Find(x => Compare(x, serializer));

                    if (other.Priority < serializer.Priority)
                    {
                        serializers.Remove(other);
                        serializers.Add(serializer);
                    }
                }
                else
                {
                    serializers.Add(serializer);
                }
            }
        }
        private static bool ParametersMatch(ParameterInfo[] parameters, Type[] types)
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
        private static MethodInfo GetMethod(Type fromType, Type returnType, params Type[] parameters)
        {
            foreach (MethodInfo method in fromType.GetMethods(BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic))
            {
                if (method.ReturnType != returnType)
                    continue;

                ParameterInfo[] methodParameters = method.GetParameters();

                if (methodParameters.Length != parameters.Length)
                    continue;

                if (!ParametersMatch(methodParameters, parameters))
                    continue;

                Debugging.Verbose("Returning method " + method);

                return method;
            }

            throw new NullReferenceException();
        }
        private static MethodInfo Query(Func<IModSerializer, bool> predicate, Func<IModSerializer, Type> toType, Func<IModSerializer, Type> returnType, out object instance)
        {
            if (serializers == null)
                throw new NullReferenceException("Serializers not initialized");

            List<IModSerializer> validSerializers = new List<IModSerializer>(serializers.Where(predicate));

            if (validSerializers.Count == 0)
                throw new NotImplementedException("Couldn't find any valid serializers ");

            IModSerializer selectedSerializer = Utility.GetMostInherited(validSerializers);
            instance = selectedSerializer;

            Debugging.Verbose("Found top-most serializer " + selectedSerializer);

            return GetMethod(selectedSerializer.GetType(), toType(selectedSerializer), returnType(selectedSerializer));
        }
        private static MethodInfo QueryForSerialization(Func<IModSerializer, bool> predicate, out object instance)
        {
            Debugging.Info("Processing Serialization Query for " + predicate);

            return Query(predicate, x => { return x.SerializedType; }, x => { return x.NonSerializableType; }, out instance);
        }
        private static MethodInfo QueryForDeserialization(Func<IModSerializer, bool> predicate, out object instance)
        {
            Debugging.Info("Processing Deserialization Query for " + predicate);

            return Query(predicate, x => { return x.NonSerializableType; }, x => { return x.SerializedType; }, out instance);
        }
        public static T SerializeObject<T>(object fromObject)
        {
            if (fromObject == null)
                return default(T);

            Debugging.Info("Serializing " + fromObject);

            object instance;
            MethodInfo info = QueryForSerialization(x => x.NonSerializableType.IsAssignableFrom(fromObject.GetType()) && x.SerializedType.IsAssignableFrom(typeof(T)), out instance);

            return (T)info.Invoke(instance, new object[1] { fromObject });
        }
        public static object SerializeObject(object fromObject)
        {
            if (fromObject == null)
                return null;

            Debugging.Info("Serializing " + fromObject);

            object instance;
            MethodInfo info = QueryForSerialization(x => x.NonSerializableType.IsAssignableFrom(fromObject.GetType()), out instance);

            return info.Invoke(instance, new object[1] { fromObject });
        }
        public static T DeserializeObject<T>(object serialized)
        {
            if (serialized == null)
                return default(T);

            if (!CanDeserialize(serialized.GetType()))
                throw new InvalidOperationException("Cannot deserialize " + serialized + ", because it implements a custom deserializer");

            try
            {
                Debugging.Info("Deserializing " + serialized);

                object instance;
                MethodInfo info = QueryForDeserialization(x => x.SerializedType.IsAssignableFrom(serialized.GetType()) && x.NonSerializableType.IsAssignableFrom(typeof(T)), out instance);
                return (T)info.Invoke(instance, new object[1] { serialized });
            }
            catch (Exception)
            {
                Debug.Log(string.Format("Datadump: ({0}){1} - (2)", serialized.GetType(), serialized, typeof(T)));
                throw;
            }
        }
        public static object DeserializeObject(object serialized)
        {
            if (serialized == null)
                return null;

            if (!CanDeserialize(serialized.GetType()))
                throw new InvalidOperationException("Cannot deserialize " + serialized + ", because it implements a custom deserializer");

            try
            {
                Debugging.Info("Deserializing " + serialized);

                object instance;
                MethodInfo info = QueryForDeserialization(x => x.SerializedType.IsAssignableFrom(serialized.GetType()), out instance);

                return info.Invoke(instance, new object[1] { serialized });
            }
            catch (Exception)
            {
                Debug.Log(string.Format("Datadump: ({0}){1} - (2)", serialized.GetType(), serialized));
                throw;
            }
        }
        public static bool CanDeserialize(Type type)
        {
            return !type.GetInterfaces().Any(x => x == typeof(ICustomDeserializer));
        }
        public static bool CanSerialize(Type type)
        {
            if (type.GetInterfaces().Any(x => x == typeof(ICustomSerializer)))
                return false;

            return serializers.Any(x => x.NonSerializableType.IsAssignableFrom(type) && !IsPrimitive(x.NonSerializableType));
        }
        private static bool IsPrimitive(Type type)
        {
            return type.IsPrimitive || type == typeof(System.Object);
        }
    }
}