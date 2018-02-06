﻿using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using UMS.Serialization;
using UnityEngine;

namespace UMS.Core.Types
{
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
}
