using System;
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
        public Member(MemberInfo info, object value, bool isProperty)
        {
            _currentMember = info;

            _isProperty = isProperty;
            _memberName = info.Name;
            
            if (value == null)
                return;
            
            if (value.GetType().IsArray)
            {
                Type valueType = value.GetType().GetElementType();

                if (Serializer.IsBlocked(valueType))
                    return;

                SerializeArray((Array)value);
            }
            else
            {
                if (Serializer.IsBlocked(value.GetType()))
                    return;

                SerializeSingular(value);
            }
        }
        
        public string _memberName;
        public bool _isProperty;
        public object _value;
        public Type _type;

        private static MemberInfo _currentMember;

        private void SerializeArray(Array array)
        {
            if (array == null)
                return;

            object[] newArray = new object[array.Length];

            for (int i = 0; i < array.Length; i++)
            {
                object arrayValue = array.GetValue(i);

                if (arrayValue == null)
                    continue;

                object obj = SerializeObject(array.GetValue(i));

                if (obj != null)
                    _type = obj.GetType();
                
                newArray[i] = obj;
            }
            
            _value = newArray;

            if (_type == null)
                _value = null;
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
                UnityEngine.Debug.LogError(string.Format("Cannot serialize {0} {1} on {2}", value.GetType(), value,
                    Utility.GetObjectMemberName(_currentMember.DeclaringType.Name, _currentMember.Name)));
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