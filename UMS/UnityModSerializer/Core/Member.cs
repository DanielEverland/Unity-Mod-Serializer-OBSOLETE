﻿using System;
using System.Reflection;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UMS.Serialization;
using Newtonsoft.Json.Linq;

namespace UMS.Core
{
    [System.Serializable]
    public class Member
    {
        public Member(string memberName, object value, bool isProperty)
        {
            _isProperty = isProperty;
            _memberName = memberName;

            if (value == null)
                return;

            if (value.GetType().IsArray)
            {
                SerializeArray((Array)value, value.GetType().GetElementType());
            }
            else
            {
                SerializeSingular(value);
            }            
        }
        
        public string _memberName;
        public bool _isProperty;
        public object _value;
        public Type _type;

        private void SerializeArray(Array array, Type elementType)
        {
            object[] newArray = new object[array.Length];

            for (int i = 0; i < array.Length; i++)
            {
                object obj = SerializeObject(array.GetValue(i));

                _type = obj.GetType();

                newArray[i] = obj;
            }
            
            _value = newArray;
        }
        private void SerializeSingular(object value)
        {
            _value = SerializeObject(value);

            if (Serializer.ContainsSerializer(value.GetType()))
            {
                _type = _value.GetType();
            }
        }
        private object SerializeObject(object value)
        {
            if (JsonSerializer.CanSerialize(value))
            {
                return value;
            }
            else if (Serializer.ContainsSerializer(value.GetType()))
            {
                return Serializer.SerializeCustom(value);
            }
            else
            {
                UnityEngine.Debug.LogWarning("Cannot serialize object " + value.GetType() + " " + value);
                return null;
            }
        }
        public void AssignValue(object obj)
        {
            if(_isProperty)
            {
                AssignAsProperty(obj);
            }
            else
            {
                AssignAsField(obj);
            }
        }
        private void AssignAsProperty(object obj)
        {
            PropertyInfo property = obj.GetType().GetProperty(_memberName);

            if (property == null)
                return;
            
            MethodInfo info = property.GetSetMethod();

            if (info == null)
                return;

            object convertedObject = null;

            try
            {
                convertedObject = Utility.ConvertObject(_value, _type, property.PropertyType);

                if (convertedObject == null)
                    return;

                property.SetValue(obj, convertedObject, null);
            }
            catch (Exception)
            {
                throw new InvalidCastException(string.Format("Couldn't assign {0} to property {1}. ConvertedObject: {2}", _value, property, convertedObject));
            }
        }
        private void AssignAsField(object obj)
        {
            FieldInfo field = obj.GetType().GetField(_memberName);

            if (field == null)
                return;

            object convertedObject = null;

            try
            {
                convertedObject = Utility.ConvertObject(_value, _type, field.FieldType);

                if (convertedObject == null)
                    return;

                field.SetValue(obj, convertedObject);
            }
            catch (Exception)
            {
                throw new InvalidCastException(string.Format("Couldn't assign {0} to field {1}. ConvertedObject: {2}", _value, field, convertedObject));
            }            
        }
        public override string ToString()
        {
            return string.Format("MemberName: {0}, IsProperty: {1}, Value: {2}, Type: {3}", _memberName, _isProperty, _value, _type);
        }
    }
}