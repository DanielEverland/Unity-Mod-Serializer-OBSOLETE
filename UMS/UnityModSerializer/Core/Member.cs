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
        public Member(string memberName, object value, bool isProperty)
        {
            _isProperty = isProperty;
            _memberName = memberName;

            if (value == null)
                return;
            
            if (JsonSerializer.CanSerialize(value))
            {
                _value = value;
            }
            else if(Serializer.ContainsSerializer(value.GetType()))
            {
                _value = Serializer.SerializeCustom(value);
                _type = _value.GetType();
            }
            else
            {
                UnityEngine.Debug.LogWarning("Cannot serialize object " + value.GetType() + " " + value);
            }
        }
        
        public string _memberName;
        public bool _isProperty;
        public object _value;
        public Type _type;

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

            object convertedObject = Utility.ConvertObject(_value, _type, property.PropertyType);

            if (convertedObject == null)
                return;

            property.SetValue(obj, convertedObject, null);
        }
        private void AssignAsField(object obj)
        {
            FieldInfo field = obj.GetType().GetField(_memberName);

            if (field == null)
                return;

            object convertedObject = Utility.ConvertObject(_value, _type, field.FieldType);

            if (convertedObject == null)
                return;

            field.SetValue(obj, convertedObject);
        }
    }
}
