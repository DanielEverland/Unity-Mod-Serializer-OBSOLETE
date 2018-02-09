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
        public object Value { get { return _value; } }

        [JsonProperty]
        private string _memberName;
        [JsonProperty]
        private object _value;

        private void AssignAsSingular(object value)
        {
            _value = GetSerializableObject(value);
        }
        private void AssignAsArray(object value)
        {
            List<object> objects = new List<object>();

            foreach (object item in value as IEnumerable)
            {
                objects.Add(GetSerializableObject(item));
            }

            _value = objects.ToArray();
        }
        private object GetSerializableObject(object obj)
        {
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

            if (member == null || _value == null)
                return;

            if (_value is Reference reference)
            {
                Deserializer.GetDeserializedObject(reference.ID, GetType(member), obj =>
                {
                    _value = obj;
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
                Debug.Log("Data dump: " + _memberName + ", " + _value + ", " + target);
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
            if (!info.FieldType.IsAssignableFrom(_value.GetType()))
                throw new ArgumentException("Type mismatch for field " + info + " - " + this);

            info.SetValue(target, _value);
        }
        private void AssignAsProperty(PropertyInfo info, object target)
        {
            MethodInfo setter = info.GetSetMethod();

            if (setter == null)
                throw new ArgumentException("No setter for property " + info + " - " + this);

            if (!ParamatersMatch(setter))
                throw new ArgumentException("Parameters don't match for " + info + " - Value: " + _value + "(" + _value.GetType() + ")");

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
}