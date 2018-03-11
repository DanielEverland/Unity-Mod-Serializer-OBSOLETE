using Newtonsoft.Json;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using UMS.Deserialization;
using UMS.Core;
using UMS.Deserialization;
using UnityEngine;

namespace UMS.Types
{
    public class SerializableMember : IDeserializer<object>
    {
        private SerializableMember() { }
        public SerializableMember(MemberInfo info, object value)
        {
            if (Utility.IsNull(value))
                throw new NullReferenceException();

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
                return DeserializedValue;
            }
        }
        private object DeserializedValue
        {
            get
            {
                if (_value == null)
                    return null;

                return _value.DeserializedObject;
            }
            set
            {
                _value.DeserializedObject = value;
            }
        }
        private object SerializedValue
        {
            get
            {
                if (_value == null)
                    return null;

                return _value.SerializedObject;
            }
            set
            {
                _value.SerializedObject = value;
            }
        }

        [JsonProperty]
        private string _memberName;
        [JsonProperty]
        private MemberObject _value;

        public bool IsJSONEmpty()
        {
            string json = Json.ToJson(_value.SerializedObject);

            string[] lines = json.Split('\n');

            if (lines.Length == 3)
            {
                return json.StartsWith("{") && json.EndsWith("}") && json.Contains("$type");
            }

            return false;
        }
        private void AssignAsSingular(object value)
        {
            SerializedValue = GetSerializableObject(value);
        }
        private void AssignAsArray(object value)
        {
            List<object> objects = new List<object>();

            foreach (object item in value as IEnumerable)
            {
                objects.Add(GetSerializableObject(item));
            }

            SerializedValue = objects.ToArray();
        }
        private object GetSerializableObject(object obj)
        {
            if (obj == null)
                return null;

            if (Converter.CanSerialize(obj.GetType()))
            {
                if (obj is UnityEngine.Object)
                {
                    return Reference.Create(obj);
                }
                else
                {
                    return Converter.SerializeObject(obj);
                }
            }

            return obj;
        }
        public void Deserialize(object target)
        {
            if (target == null)
                throw new NullReferenceException("Given target for " + this + " is null!");

            Type declaredType = target.GetType();
            MemberInfo member = Utility.GetMember(declaredType, _memberName);

            if (IsNull(member, DeserializedValue))
                return;

            if (ValueIsReference(member, target))
                return;

            if (DeserializedValue is IModSerializer serializer)
            {
                Converter.DeserializeObject(serializer, x =>
                {
                    DeserializedValue = x;

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
                Debug.Log("Data dump: " + _memberName + ", " + DeserializedValue + ", " + target);
                throw;
            }
        }
        private bool IsNull(MemberInfo info, object value)
        {
            if (info == null)
            {
                Debugging.Warning("MemberInfo is null on " + _memberName);
                return true;
            }

            if (value == null)
            {
                Debugging.Warning("Value is null on " + _memberName);
                return true;
            }

            if (value.GetType().IsArray)
            {
                Array array = value as Array;

                for (int i = 0; i < array.Length; i++)
                {
                    if (array.GetValue(i) != null)
                        return false;
                }

                return true;
            }

            return false;
        }
        private bool ValueIsReference(MemberInfo member, object target)
        {
            if (DeserializedValue.GetType().IsArray)
            {
                Array array = (Array)DeserializedValue;

                if (array.Length > 0 && array != null)
                {
                    Type elementType = array.GetType().GetElementType();

                    if (elementType == typeof(Reference))
                    {
                        ArrayDeserializationHandler.Create(array, obj =>
                        {
                            DeserializedValue = obj;
                            Deserialize(target);
                        });

                        return true;
                    }
                }
            }
            else if (DeserializedValue is Reference reference)
            {
                Deserializer.GetDeserializedObject(reference.ID, GetType(member), obj =>
                {
                    DeserializedValue = obj;
                    Deserialize(target);
                });

                return true;
            }

            return false;
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
            Type valueType = DeserializedValue.GetType();

            if (fieldType.IsEnum)
            {
                if (!Enum.IsDefined(fieldType, DeserializedValue))
                    throw new ArgumentException("Type mismatch for field " + info + " - " + this);
            }
            else if (!info.FieldType.IsAssignableFrom(valueType))
            {
                throw new ArgumentException("Type mismatch for field " + info + " - " + this);
            }

            info.SetValue(target, DeserializedValue);
        }
        private void AssignAsProperty(PropertyInfo info, object target)
        {
            MethodInfo setter = info.GetSetMethod();

            if (setter == null)
                throw new ArgumentException("No setter for property " + info + " - " + this);

            if (!ParamatersMatch(setter))
                throw new ArgumentException("Parameters don't match for " + info + " - Value: " + DeserializedValue + "(" + DeserializedValue.GetType() + ")");

            info.SetValue(target, DeserializedValue, null);
        }
        private bool ParamatersMatch(MethodInfo info)
        {
            ParameterInfo[] parameters = info.GetParameters();

            if (parameters.Length != 1)
                return false;

            return ParametersMatch(parameters[0].ParameterType);
        }
        private bool ParametersMatch(Type type)
        {
            if (type.IsArray)
            {
                return ParametersMatch(type.GetElementType());
            }
            else if (type.IsEnum)
            {
                if (!Enum.IsDefined(type, DeserializedValue))
                    return false;
            }
            else
            {
                Type deserializedType = DeserializedValue.GetType();

                if (deserializedType.IsArray)
                {
                    if (!type.IsAssignableFrom(deserializedType.GetElementType()))
                    {
                        return false;
                    }
                }
                else if (!type.IsAssignableFrom(DeserializedValue.GetType()))
                {
                    return false;
                }
            }

            return true;
        }
        public override string ToString()
        {
            return string.Format("{0} - SerializedValue: {1} - DeserializedValue: {2}", _memberName, ValueString(SerializedValue), ValueString(DeserializedValue));
        }
        private string ValueString(object value)
        {
            if (value != null)
            {
                return string.Format("{0} ({1})", value, value.GetType());
            }
            else
            {
                return null;
            }
        }
        private class ArrayDeserializationHandler
        {
            private ArrayDeserializationHandler(Array array, Action<object> onDeserialized)
            {
                this.onDeserialized = onDeserialized;
                deserializedObjects = new List<object>();

                for (int i = 0; i < array.Length; i++)
                {
                    object obj = array.GetValue(i);

                    if (obj != null && obj is Reference reference)
                    {
                        targetObjectCount++;

                        Deserializer.GetDeserializedObject(reference.ID, reference.NonSerializableType, x => AddObject(x));
                    }
                }

                if (targetObjectCount == 0)
                    return;

                finishedInitializing = true;
                CheckIfDone();
            }

            private Action<object> onDeserialized;
            private List<object> deserializedObjects;
            private bool finishedInitializing;
            private int targetObjectCount;

            private void AddObject(object obj)
            {
                deserializedObjects.Add(obj);

                CheckIfDone();
            }

            private void CheckIfDone()
            {
                if (!finishedInitializing)
                    return;

                if (deserializedObjects.Count == targetObjectCount)
                {
                    onDeserialized?.Invoke(CreateArray());
                }
            }
            private Array CreateArray()
            {
                if (deserializedObjects.Count == 0)
                    throw new NullReferenceException();

                Type targetType = deserializedObjects[0].GetType();

                if (deserializedObjects.Any(x => x.GetType() != targetType))
                    throw new InvalidCastException();

                Array array = Array.CreateInstance(targetType, deserializedObjects.Count);

                for (int i = 0; i < deserializedObjects.Count; i++)
                {
                    array.SetValue(deserializedObjects[i], i);
                }

                return array;
            }

            public static ArrayDeserializationHandler Create(Array array, Action<object> onDeserialized)
            {
                return new ArrayDeserializationHandler(array, onDeserialized);
            }
        }
        [JsonObject(MemberSerialization = MemberSerialization.OptIn)]
        private class MemberObject
        {
            public object DeserializedObject
            {
                get
                {
                    if (_deserializedValue == null)
                    {
                        Deserialize();
                    }

                    return _deserializedValue;
                }
                set
                {
                    _deserializedValue = value;
                }
            }
            public object SerializedObject
            {
                get
                {
                    return _serializedValue;
                }
                set
                {
                    Assign(value);
                }
            }

            [JsonIgnore]
            private object _deserializedValue;
            [JsonProperty]
            private object _serializedValue;

            private void Deserialize()
            {
                if (_serializedValue.GetType().IsArray)
                {
                    Array serializedArray = _serializedValue as Array;
                    Array deserializedArray = Array.CreateInstance(GetType(serializedArray), serializedArray.Length);

                    for (int i = 0; i < serializedArray.Length; i++)
                    {
                        deserializedArray.SetValue(GetValue(serializedArray, i), i);
                    }

                    _deserializedValue = deserializedArray;
                }
                else if (_serializedValue is Value value)
                {
                    _deserializedValue = value.Object;
                }
                else
                {
                    throw new NotImplementedException();
                }
            }
            private object GetValue(Array serializedArray, int index)
            {
                if (serializedArray.Length < index)
                    throw new ArgumentOutOfRangeException();

                Value value = serializedArray.GetValue(index) as Value;

                if (value == null)
                    return serializedArray.GetValue(index);

                return value.Object;
            }
            private Type GetType(Array array)
            {
                Type type = null;

                foreach (object obj in array)
                {
                    if (obj == null)
                        continue;

                    if (type == null)
                    {
                        type = obj.GetType();
                    }
                    else if (type != obj.GetType())
                    {
                        return typeof(object);
                    }
                }

                return type ?? typeof(object);
            }
            private void Assign(object value)
            {
                _deserializedValue = value;

                if (value.GetType().IsArray)
                {
                    AssignAsArray(value);
                }
                else
                {
                    AssignAsSingular(value);
                }
            }
            private void AssignAsSingular(object value)
            {
                _serializedValue = new Value(value);
            }
            private void AssignAsArray(object value)
            {
                Array existingArray = value as Array;
                Array newArray = Array.CreateInstance(typeof(Value), existingArray.Length);

                for (int i = 0; i < existingArray.Length; i++)
                {
                    newArray.SetValue(new Value(existingArray.GetValue(i)), i);
                }

                _serializedValue = existingArray;
            }

            [Serializable]
            private class Value
            {
                private Value() { }
                public Value(object value)
                {
                    Object = value;
                }

                public object Object
                {
                    get
                    {
                        if (_value == null)
                            return null;

                        if (_type.IsEnum)
                        {
                            return Enum.Parse(_type, _value.ToString());
                        }

                        return Convert.ChangeType(_value, _type);
                    }
                    set
                    {
                        _value = value;

                        if (value != null)
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
}