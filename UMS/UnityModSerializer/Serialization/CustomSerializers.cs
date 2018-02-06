using System;
using System.Linq;
using System.Reflection;
using System.Collections.Generic;
using Newtonsoft.Json;
using UMS.Core;
using UnityEngine;
using UMS.Deserialization;

namespace UMS.Serialization
{
    public static class CustomSerializers
    {
        #region Serialization
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

                if(serializers.Any(x => Compare(x, serializer)))
                {
                    IModSerializer other = serializers.Find(x => Compare(x, serializer));

                    if(other.Priority < serializer.Priority)
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

            return GetMethod(selectedSerializer.GetType(), toType(selectedSerializer), returnType(selectedSerializer));
        }
        private static MethodInfo QueryForSerialization(Func<IModSerializer, bool> predicate, out object instance)
        {
            return Query(predicate, x => { return x.SerializedType; }, x => { return x.NonSerializableType; }, out instance);
        }
        private static MethodInfo QueryForDeserialization(Func<IModSerializer, bool> predicate, out object instance)
        {
            return Query(predicate, x => { return x.NonSerializableType; }, x => { return x.SerializedType; }, out instance);
        }
        public static T SerializeObject<T>(object fromObject)
        {
            if (fromObject == null)
                return default(T);

            object instance;
            MethodInfo info = QueryForSerialization(x => x.NonSerializableType.IsAssignableFrom(fromObject.GetType()) && x.SerializedType.IsAssignableFrom(typeof(T)), out instance);

            return (T)info.Invoke(instance, new object[1] { fromObject });
        }
        public static object SerializeObject(object fromObject)
        {
            if (fromObject == null)
                return null;

            object instance;
            MethodInfo info = QueryForSerialization(x => x.NonSerializableType.IsAssignableFrom(fromObject.GetType()), out instance);

            return info.Invoke(instance, new object[1] { fromObject});
        }
        public static T DeserializeObject<T>(object serialized)
        {
            if (serialized == null)
                return default(T);

            if (!CanDeserialize(serialized.GetType()))
                throw new InvalidOperationException("Cannot deserialize " + serialized + ", because it implements a custom deserializer");

            try
            {
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
        #endregion
    }
    #region Definitions

    ///------------------Template------------------//
    //[Serializable]
    //[JsonObject(MemberSerialization = MemberSerialization.OptIn)]
    //public class SerializableTYPE : Serializable<UNITY_TYPE, SERIALIZABLE_TYPE>
    //{
    //    public SerializableTYPE() { }
    //    public SerializableTYPE(UNITY_TYPE obj) : base(obj)
    //    {
    //    }

    //    public override UNITY_TYPE Deserialize(SerializableTYPE serialized)
    //    {
    //        return new UNITY_TYPE(serializable);
    //    }
    //    public override SerializableTYPE Serialize(UNITY_TYPE obj)
    //    {
    //        return new SerializableTYPE(obj);
    //    }
    //}
    //------------------------------------//

    ///------------------Serializable------------------//
    [Serializable]
    [JsonObject(MemberSerialization = MemberSerialization.OptIn)]
    public abstract class Serializable<TFrom, TTo> : IModSerializer<TFrom, TTo>
    {
        public Type NonSerializableType => typeof(TFrom);
        public Type SerializedType => typeof(TTo);
        public virtual int Priority => (int)Core.Priority.Medium;

        public Serializable() { }
        public Serializable(TFrom obj)
        {
            _id = ObjectManager.Add(obj);
        }

        public int ID { get { return _id; } protected set { _id = value; } }

        [JsonProperty]
        private int _id;

        public virtual TFrom Deserialize(TTo serializable) { throw new NotImplementedException(GetType().ToString()); }
        public virtual TTo Serialize(TFrom obj) { throw new NotImplementedException(GetType().ToString()); }
    }
    //------------------------------------//

    ///------------------UnityEngine.Object------------------//
    [Serializable]
    [JsonObject(MemberSerialization = MemberSerialization.OptIn)]
    public abstract class SerializableObject<TFrom, TTo> : Serializable<TFrom, TTo>, IModEntry where TFrom : UnityEngine.Object
    {
        public SerializableObject() { }
        public SerializableObject(UnityEngine.Object obj) : base((TFrom)obj)
        {
            if (obj is null)
                throw new NullReferenceException("Object cannot be null");
            
            _name = obj.name;
            _hideFlags = (int)obj.hideFlags;
        }

        public abstract string Extension { get; }
        public virtual string FileName { get { return Name; } }

        public string Name { get { return _name; } }
        public HideFlags HideFlags { get { return (HideFlags)_hideFlags; } }

        [JsonProperty]
        private string _name;
        [JsonProperty]
        private int _hideFlags;
    }
    //------------------------------------//

    ///------------------GameObject------------------//
    [Serializable]
    [JsonObject(MemberSerialization = MemberSerialization.OptIn)]
    public class SerializableGameObject : SerializableObject<GameObject, SerializableGameObject>
    {
        public SerializableGameObject() { }
        public SerializableGameObject(GameObject obj) : base(obj)
        {
            _components = new List<Reference>();
            foreach (Component comp in obj.GetComponents<Component>())
            {
                if(comp == null)
                    continue;

                _components.Add(new Reference(comp));
            }
        }

        public override string Extension => "gameObject";

        public IList<Reference> Components { get { return _components; } }

        [JsonProperty]
        private List<Reference> _components;

        public override GameObject Deserialize(SerializableGameObject serialized)
        {
            GameObject gameObject = new GameObject(serialized.Name);

            foreach (Reference reference in serialized.Components)
            {
                if (!Deserializer.ContainsObject(reference.ID))
                    continue;

                Deserializer.GetSerializedObject<SerializableComponent>(reference.ID, serializableComponent =>
                {
                    Component component = GetComponent(serializableComponent.ComponentType, gameObject);
                    serializableComponent.Deserialize(component);
                });
            }

            return gameObject;
        }
        private Component GetComponent(Type type, GameObject obj)
        {
            if(type == typeof(Transform))
            {
                return obj.GetComponent<Transform>();
            }
            else
            {
                return obj.AddComponent(type);
            }
        }
        public override SerializableGameObject Serialize(GameObject obj)
        {
            return new SerializableGameObject(obj);
        }
    }
    //------------------------------------//

    ///------------------Reference------------------//
    [Serializable]
    [JsonObject(MemberSerialization = MemberSerialization.OptIn)]
    public class Reference : Serializable<object, Reference>
    {
        public Reference() { }
        public Reference(object obj)
        {
            if (obj == null)
                throw new NullReferenceException("Object cannot be null");

            ID = ObjectManager.Add(obj);
        }
        
        public override Reference Serialize(object obj)
        {
            return new Reference(obj);
        }
    }
    //------------------------------------//

    ///------------------Component------------------//
    [Serializable]
    [JsonObject(MemberSerialization = MemberSerialization.OptIn)]
    public class SerializableComponent : SerializableObject<Component, SerializableComponent>, IDeserializer<Component>
    {
        public SerializableComponent() { }
        public SerializableComponent(Component obj) : base(obj)
        {
            _type = obj.GetType();
            _fileName = string.Format("{0} - {1}", obj.gameObject.name, obj.GetType().Name);
            _members = new List<SerializableMember>();

            AddMembers(obj);
        }

        public override string Extension { get { return "component"; } }
        public override string FileName { get { return _fileName; } }

        public Type ComponentType { get { return _type; } }
        public IList<SerializableMember> Members { get { return _members; } }
        
        [JsonProperty]
        private Type _type;
        [JsonProperty]
        private List<SerializableMember> _members;

        [JsonIgnore]
        private string _fileName;

        private void AddMembers(Component comp)
        {
            foreach (PropertyInfo property in ComponentType.GetProperties(BindingFlags.Instance | BindingFlags.Public))
            {
                try
                {
                    if (IsValid(property))
                    {
                        object propertyValue = property.GetValue(comp, null);

                        if(propertyValue != null)
                        {
                            _members.Add(new SerializableMember(property, propertyValue));
                        }                        
                    }
                        
                }
                catch (NotSupportedException)
                {
                    throw new NotSupportedException(Utility.GetObjectMemberName(property.DeclaringType.Name, property.Name) + " is not supported. Please block");
                }
                catch (Exception)
                {
                    throw;
                }                
            }
        }
        private bool IsValid(PropertyInfo info)
        {
            if (info.GetSetMethod() == null)
                return false;

            return IsValid((MemberInfo)info);
        }
        private bool IsValid(MemberInfo info)
        {
            string objectMemberValue = Utility.GetObjectMemberName(info.DeclaringType.Name, info.Name);

            return !BlockedTypes.IsBlocked(objectMemberValue);
        }
        public void Deserialize(Component target)
        {
            if (target == null)
                throw new System.NullReferenceException("Given component is null!");

            foreach (SerializableMember member in _members)
            {
                member.Deserialize(target);
            }
        }
        public override SerializableComponent Serialize(Component obj)
        {
            return new SerializableComponent(obj);
        }        
    }
    //------------------------------------//

    ///------------------Member------------------//
    [Serializable]
    [JsonObject(MemberSerialization = MemberSerialization.OptIn)]
    public class SerializableMember : IDeserializer<object>
    {
        private SerializableMember() { }
        public SerializableMember(MemberInfo info, object value)
        {
            if (value == null)
                return;

            _memberName = info.Name;

            if (CustomSerializers.CanSerialize(value.GetType()))
            {
                _value = new Reference(value);
            }
            else
            {
                _value = value;
            }            
        }

        public string MemberName { get { return _memberName; } }
        public object Value { get { return _value; } }

        [JsonProperty]
        private string _memberName;
        [JsonProperty]
        private object _value;
        
        public void Deserialize(object target)
        {
            if (target == null)
                throw new NullReferenceException("Given target for " + this + " is null!");
            
            Type declaredType = target.GetType();
            MemberInfo member = Utility.GetMember(declaredType, _memberName);
            
            try
            {
                if (member is PropertyInfo property)
                {
                    AssignAsProperty(property, target);
                    
                }
                else if (member is FieldInfo field)
                {
                    AssignAsField(field, target);
                }
            }
            catch (Exception)
            {
                Debug.Log("Data dump: " + _memberName + ", " + _value + ", " + target);
                throw;
            }            
        }
        private void AssignAsField(FieldInfo info, object target)
        {
            _value = Utility.CheckForConversion(_value, info.FieldType);

            if (!info.FieldType.IsAssignableFrom(_value.GetType()))
                throw new ArgumentException("Type mismatch for field " + info + " - " + this);

            info.SetValue(target, _value);
        }
        private void AssignAsProperty(PropertyInfo info, object target)
        {
            MethodInfo setter = info.GetSetMethod();

            _value = Utility.CheckForConversion(_value, info.PropertyType);

            if (setter == null)
                throw new ArgumentException("No setter for property " + info + " - " + this);

            if (!ParamatersMatch(setter))
                throw new ArgumentException("Parameters don't match for " + info + " - " + this + " - Object: " + _value);
            
            info.SetValue(target, _value, null);
        }
        private bool ParamatersMatch(MethodInfo info)
        {
            ParameterInfo[] parameters = info.GetParameters();

            if (parameters.Length != 1)
                return false;

            if (!parameters[0].ParameterType.IsAssignableFrom(_value.GetType()))
                return false;

            return true;
        }
        public override string ToString()
        {
            return string.Format("{0}: {1} ({2})", _memberName, _value, _value.GetType());
        }
    }
    //------------------------------------//
    #endregion
}
