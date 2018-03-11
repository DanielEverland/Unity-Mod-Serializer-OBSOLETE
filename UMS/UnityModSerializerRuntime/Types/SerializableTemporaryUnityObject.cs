using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using UnityEngine;

namespace UMS.Runtime.Types
{
    /// <summary>
    /// Class that will serialize Unity objects using the reference system. Only meant for temporary use, all Unity object types should use their own class.
    /// Inherit SerializableObject when doing so, not this class.
    /// </summary>
    public sealed class SerializableTemporaryUnityObject : SerializableObject<UnityEngine.Object, SerializableTemporaryUnityObject>
    {
        public SerializableTemporaryUnityObject() { }
        public SerializableTemporaryUnityObject(UnityEngine.Object obj) : base(obj)
        {
            this.obj = obj;

            _type = obj.GetType();

            bool validType = false;
            foreach (Initializer initalizer in initializers)
            {
                if (initalizer.IsValid(_type))
                {
                    validType = true;
                    break;
                }
            }

            if (!validType)
                throw new ArgumentException("Cannot create new instances of " + _type);

            GetAllMembers();

            Debug.LogWarning("Serializing " + obj.GetType() + " using a temporary serializer");
        }

        private static readonly List<Initializer> initializers = new List<Initializer>()
        {
            new EmptyConstructorInitializer(),
            new ConstructorWithArgumentsInitializer(),
            new StaticFunctionInitializer(),
        };

        public override string Extension => "unityObject";

        private UnityEngine.Object obj;

        [JsonProperty]
        private List<SerializableMember> _members;
        [JsonProperty]
        private Type _type;

        private void GetAllMembers()
        {
            _members = new List<SerializableMember>();

            AssignProperties();
            AssignFields();
        }
        private void AssignProperties()
        {
            foreach (PropertyInfo property in obj.GetType().GetProperties(BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.Public))
            {
                if (IsValid(property))
                {
                    Add(property.GetValue(obj, null), property);
                }
            }
        }
        private void AssignFields()
        {
            foreach (FieldInfo field in obj.GetType().GetFields(BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.Public))
            {
                if (IsValid(field))
                {
                    Add(field.GetValue(obj), field);
                }
            }
        }
        private void Add(object obj, MemberInfo info)
        {
            SerializableMember member = new SerializableMember(info, obj);

            if (!member.IsJSONEmpty())
                _members.Add(member);
        }

        public override SerializableTemporaryUnityObject Serialize(UnityEngine.Object obj)
        {
            return new SerializableTemporaryUnityObject(obj);
        }
        public override UnityEngine.Object Deserialize(SerializableTemporaryUnityObject serializable)
        {
            object obj = null;

            foreach (Initializer initializer in initializers)
            {
                if (initializer.IsValid(serializable._type))
                {
                    obj = initializer.Initialize(serializable._type, _members);
                    break;
                }
            }

            if (obj == null)
                throw new NullReferenceException();

            UnityEngine.Object unityObject = (UnityEngine.Object)obj;

            serializable.Deserialize(unityObject);

            return unityObject;
        }

        private bool IsValid(MemberInfo info)
        {
            if (info is PropertyInfo property)
            {
                return IsValid(property);
            }
            else if (info is FieldInfo field)
            {
                return IsValid(field);
            }

            throw new NotImplementedException();
        }
        private bool IsValid(PropertyInfo property)
        {
            if (property.GetSetMethod() == null)
                return false;

            return true;
        }
        private bool IsValid(FieldInfo field)
        {
            return true;
        }

        private sealed class StaticFunctionInitializer : Initializer
        {
            public override object Initialize(Type type, List<SerializableMember> members)
            {
                return CreateObjectWithParameters(type.GetMethods(BindingFlags.Static | BindingFlags.Public).Where(x => x.ReflectedType == type).ToArray(), members);
            }
            public override bool IsValid(Type type)
            {
                return type.GetMethods(BindingFlags.Static | BindingFlags.Public).Any(x => x.ReturnType == type);
            }
        }
        private sealed class ConstructorWithArgumentsInitializer : Initializer
        {
            public override object Initialize(Type type, List<SerializableMember> members)
            {
                return CreateObjectWithParameters(type.GetConstructors(BindingFlags.Instance | BindingFlags.Public), members);
            }

            public override bool IsValid(Type type)
            {
                return type.GetConstructors(BindingFlags.Instance | BindingFlags.Public).Any(x => x.GetParameters().Length > 0);
            }
        }
        private sealed class EmptyConstructorInitializer : Initializer
        {
            public override object Initialize(Type type, List<SerializableMember> members)
            {
                return Activator.CreateInstance(type);
            }

            public override bool IsValid(Type type)
            {
                return type.GetConstructors(BindingFlags.Instance | BindingFlags.Public).Any(x => x.GetParameters().Length == 0);
            }
        }
        private abstract class Initializer
        {
            public abstract object Initialize(Type type, List<SerializableMember> members);
            public abstract bool IsValid(Type type);

            protected object CreateObjectWithParameters(MethodBase[] methods, List<SerializableMember> members)
            {
                foreach (MethodBase method in methods)
                {
                    object[] parameterObjects = GetParameterObjects(method.GetParameters(), members);

                    if (!parameterObjects.Any(x => x == null))
                    {
                        return method.Invoke(null, parameterObjects);
                    }
                }

                throw new InvalidOperationException();
            }
            protected object[] GetParameterObjects(ParameterInfo[] parameters, List<SerializableMember> members)
            {
                List<object> memberObjects = new List<object>(members.Select(x => x.Value));
                List<object> parameterObjects = new List<object>();

                foreach (ParameterInfo info in parameters)
                {
                    if (memberObjects.Any(x => x.GetType() == info.ParameterType))
                    {
                        parameterObjects.Add(memberObjects.First(x => x.GetType() == info.ParameterType));
                    }
                    else
                    {
                        parameterObjects.Add(null);
                    }
                }

                return parameterObjects.ToArray();
            }
        }
    }
}