using System;
using System.Linq;
using System.Reflection;
using System.Collections.Generic;
using Newtonsoft.Json;
using UnityEngine;
using UMS.Core.Types;
using UMS.Serialization;

namespace UMS.Core
{
    public class ComponentMemberCollection
    {
        private ComponentMemberCollection() { }
        private ComponentMemberCollection(Component component)
        {
            _members = new List<SerializableMember>();
            _type = component.GetType();

            SerializeProperties(component);
            SerializeFields(component);
        }

        public static ComponentMemberCollection Create<T>(T component) where T : Component
        {
            return new ComponentMemberCollection(component);
        }

        private Type _type;

        public IList<SerializableMember> Members { get { return _members; } }

        [JsonProperty]
        private List<SerializableMember> _members;
        
        public void Deserialize(Component component)
        {
            foreach (SerializableMember member in _members)
            {
                member.Deserialize(component);
            }
        }
        private void SerializeFields(Component comp)
        {
            foreach (FieldInfo field in _type.GetFields(BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic))
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
            foreach (PropertyInfo property in _type.GetProperties(BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic))
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

            SerializableMember member = new SerializableMember(info, value);

            if (!member.IsJSONEmpty())
                _members.Add(member);
        }
    }
}
