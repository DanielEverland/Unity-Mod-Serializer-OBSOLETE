using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using UMS.Deserialization;
using UMS.Serialization;
using UnityEngine;

namespace UMS.Core.Types
{
    public class SerializableComponent : SerializableComponentBase<Component, SerializableComponent>
    {
        public SerializableComponent() { }
        public SerializableComponent(Component obj) : base(obj)
        {
            _members = new List<SerializableMember>();

            AddMembers(obj);
        }
        
        public IList<SerializableMember> Members { get { return _members; } }
        
        [JsonProperty]
        private List<SerializableMember> _members;

        private void AddMembers(Component comp)
        {
            SerializeProperties(comp);
            SerializeFields(comp);
        }
        private void SerializeFields(Component comp)
        {
            foreach (FieldInfo field in ComponentType.GetFields(BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic))
            {
                try
                {
                    Debugging.Verbose("Found field " + Utility.GetObjectMemberName(field));

                    if (IsValid(field))
                    {
                        object fieldValue = field.GetValue(comp);

                        if (!Utility.IsNull(fieldValue))
                        {
                            Add(field, fieldValue);
                        }
                    }
                    else
                    {
                        Debugging.Verbose("Skipping " + Utility.GetObjectMemberName(field) + " because it's invalid");
                    }
                }
                catch (Exception)
                {
                    throw;
                }
            }
        }
        private void SerializeProperties(Component comp)
        {
            foreach (PropertyInfo property in ComponentType.GetProperties(BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic))
            {
                try
                {
                    Debugging.Verbose("Found property " + Utility.GetObjectMemberName(property));

                    if (IsValid(property))
                    {
                        object propertyValue = property.GetValue(comp, null);

                        if (!Utility.IsNull(propertyValue))
                        {
                            Add(property, propertyValue);
                        }
                    }
                    else
                    {
                        Debugging.Verbose("Skipping " + Utility.GetObjectMemberName(property) + " because it's invalid");
                    }
                }
                catch (NotSupportedException)
                {
                    throw new NotSupportedException(Utility.GetObjectMemberName(property) + " is not supported. Please block");
                }
                catch (Exception)
                {
                    throw;
                }
            }
        }
        private bool IsValid(FieldInfo info)
        {
            if (!info.IsPublic)
            {
                if (!info.GetCustomAttributes(true).Any(x => x.GetType() == typeof(SerializeField)))
                    return false;
            }

            return IsValid((MemberInfo)info);
        }
        private bool IsValid(PropertyInfo info)
        {
            if (info.GetSetMethod() == null)
                return false;

            return IsValid((MemberInfo)info);
        }
        private bool IsValid(MemberInfo info)
        {
            string objectMemberValue = Utility.GetObjectMemberName(info);

            return !BlockedTypes.IsBlocked(objectMemberValue);
        }
        private void Add(MemberInfo info, object value)
        {
            if (Utility.IsNull(value))
                throw new NullReferenceException();

            Debugging.Verbose("Adding member " + Utility.GetObjectMemberName(info) + " with value " + value + " IsNull: " + (value == null));
                 
            _members.Add(new SerializableMember(info, value));
        }
        public override void OnDeserialize(Component target)
        {
            if (target == null)
                throw new System.NullReferenceException("Given component is null!");
            
            Deserialize(target as UnityEngine.Object);

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
}