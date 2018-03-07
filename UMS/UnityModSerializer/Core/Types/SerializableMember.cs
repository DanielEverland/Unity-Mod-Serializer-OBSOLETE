using Newtonsoft.Json;
using System;
using System.Linq;
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
                if (_value == null)
                    return null;

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

        public bool IsJSONEmpty()
        {
            string json = Serialization.JsonSerializer.ToJson(_value.Object);

            string[] lines = json.Split('\n');

            if(lines.Length == 3)
            {
                return json.StartsWith("{") && json.EndsWith("}") && json.Contains("$type");
            }

            return false;
        }
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
                if(obj is UnityEngine.Object)
                {
                    return Reference.Create(obj);
                }
                else
                {
                    return CustomSerializers.SerializeObject(obj);
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
            
            if (IsNull(member, Value))
                return;
            
            if (ValueIsReference(member, target))
                return;
            
            if (Value is IModSerializer serializer)
            {
                CustomSerializers.DeserializeObject(serializer, x =>
                {
                    Value = x;

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
        private bool IsNull(MemberInfo info, object value)
        {
            if(info == null)
            {
                Debugging.Warning("MemberInfo is null on " + _memberName);
                return true;
            }

            if(value == null)
            {
                Debugging.Warning("Value is null on " + _memberName);
                return true;
            }

            return false;
        }
        private bool ValueIsReference(MemberInfo member, object target)
        {
            if (Value.GetType().IsArray)
            {
                Array array = (Array)Value;

                if(array.Length > 0 && array != null)
                {
                    Type elementType = array.GetType().GetElementType();

                    if(elementType == typeof(Reference))
                    {
                        ArrayDeserializationHandler.Create(array, obj =>
                        {
                            Value = obj;
                            Deserialize(target);
                        });
                        
                        return true;
                    }
                }
            }
            else if(Value is Reference reference)
            {
                Deserializer.GetDeserializedObject(reference.ID, GetType(member), obj =>
                {
                    Value = obj;
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
                if (!Enum.IsDefined(type, Value))
                    return false;
            }
            else if (!type.IsAssignableFrom(Value.GetType()))
            {
                return false;
            }

            return true;
        }
        public override string ToString()
        {
            if(Value != null)
            {
                return string.Format("{0}: {1} ({2})", _memberName, Value, Value.GetType());
            }
            else
            {
                return string.Format("{0}: {1}", _memberName, Value);
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

                    if(obj != null && obj is Reference reference)
                    {
                        targetObjectCount++;

                        Deserializer.GetDeserializedObject(reference.ID, reference.NonSerializableType, x => AddObject(x));
                    }
                }

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

                if(deserializedObjects.Count == targetObjectCount)
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