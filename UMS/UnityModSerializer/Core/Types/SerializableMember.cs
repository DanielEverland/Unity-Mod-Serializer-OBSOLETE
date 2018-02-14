using Newtonsoft.Json;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using UMS.Deserialization;
using UMS.Serialization;
using UnityEngine;

namespace UMS.Core.Types
{
    public class SerializableMember : IDeserializer<object>
    {
        private SerializableMember() { }
        public SerializableMember(MemberInfo info, object value)
        {
            if (value == null)
                return;

            _value = new MemberObject();
            _memberName = info.Name;

            if (value.GetType().IsArray)
            {
                AssignAsArray(value);
            }
            else
            {
                AssignAsSingular(value);
            }
        }

        public string MemberName { get { return _memberName; } }
        public object Value
        {
            get
            {
                return _value.Object;
            }
            set
            {
                _value.Object = value;
            }
        }

        [JsonProperty]
        private string _memberName;
        [JsonProperty]
        private MemberObject _value;

        private void AssignAsSingular(object value)
        {
            Value = GetSerializableObject(value);
        }
        private void AssignAsArray(object value)
        {
            List<object> objects = new List<object>();

            foreach (object item in value as IEnumerable)
            {
                objects.Add(GetSerializableObject(item));
            }

            Value = objects.ToArray();
        }
        private object GetSerializableObject(object obj)
        {
            if (obj == null)
                return null;

            if (CustomSerializers.CanSerialize(obj.GetType()))
            {
                return Reference.Create(obj);
            }

            return obj;
        }
        public void Deserialize(object target)
        {
            if (target == null)
                throw new NullReferenceException("Given target for " + this + " is null!");

            Type declaredType = target.GetType();
            MemberInfo member = Utility.GetMember(declaredType, _memberName);

            if (member == null || Value == null)
                return;

            if (Value is Reference reference)
            {
                Deserializer.GetDeserializedObject(reference.ID, GetType(member), obj =>
                {
                    Value = obj;
                    Deserialize(target);
                });

                return;
            }

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
                Debug.Log("Data dump: " + _memberName + ", " + Value + ", " + target);
                throw;
            }
        }
        private Type GetType(MemberInfo member)
        {
            if (member is PropertyInfo property)
            {
                return property.PropertyType;
            }
            else if (member is FieldInfo field)
            {
                return field.FieldType;
            }
            else
            {
                throw new NotImplementedException("Cannot get type of " + member.GetType());
            }
        }
        private void AssignAsField(FieldInfo info, object target)
        {
            Type fieldType = info.FieldType;
            Type valueType = Value.GetType();

            if (fieldType.IsEnum)
            {
                if(!Enum.IsDefined(fieldType, Value))
                    throw new ArgumentException("Type mismatch for field " + info + " - " + this);
            }
            else if(!info.FieldType.IsAssignableFrom(valueType))
            {
                throw new ArgumentException("Type mismatch for field " + info + " - " + this);
            }                

            info.SetValue(target, Value);
        }
        private void AssignAsProperty(PropertyInfo info, object target)
        {
            MethodInfo setter = info.GetSetMethod();

            if (setter == null)
                throw new ArgumentException("No setter for property " + info + " - " + this);

            if (!ParamatersMatch(setter))
                throw new ArgumentException("Parameters don't match for " + info + " - Value: " + Value + "(" + Value.GetType() + ")");

            info.SetValue(target, Value, null);
        }
        private bool ParamatersMatch(MethodInfo info)
        {
            ParameterInfo[] parameters = info.GetParameters();

            if (parameters.Length != 1)
                return false;

            Type parameterType = parameters[0].ParameterType;

            if (parameterType.IsEnum)
            {
                if (!Enum.IsDefined(parameterType, Value))
                    return false;
            }
            else if(!parameters[0].ParameterType.IsAssignableFrom(Value.GetType()))
            {
                return false;
            }

            return true;
        }
        public override string ToString()
        {
            return string.Format("{0}: {1} ({2})", _memberName, Value, Value.GetType());
        }
        [JsonObject(MemberSerialization = MemberSerialization.OptIn)]
        private class MemberObject
        {
            public object Object
            {
                get
                {
                    if (_type.IsEnum)
                    {
                        return Enum.Parse(_type, _value.ToString());
                    }

                    return Convert.ChangeType(_value, _type);
                }
                set
                {
                    _value = value;
                    _type = value.GetType();
                }
            }

            [JsonProperty]
            private object _value;
            [JsonProperty]
            private Type _type;
            
        }
    }
}