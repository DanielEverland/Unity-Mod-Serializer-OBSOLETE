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
    public class SerializableComponent : SerializableObject<Component, SerializableComponent>, IDeserializer<Component>
    {
        public SerializableComponent() { }
        public SerializableComponent(Component obj) : base(obj)
        {
            _type = obj.GetType();
            _fileName = string.Format("{0} - {1}", obj.gameObject.name, obj.GetType().Name);
            _members = new List<SerializableMember>();

            AddMembers(obj);
        }

        public override string Extension { get { return "component"; } }
        public override string FileName { get { return _fileName; } }

        public Type ComponentType { get { return _type; } }
        public IList<SerializableMember> Members { get { return _members; } }

        [JsonProperty]
        private Type _type;
        [JsonProperty]
        private List<SerializableMember> _members;

        [JsonIgnore]
        private string _fileName;

        private void AddMembers(Component comp)
        {
            foreach (PropertyInfo property in ComponentType.GetProperties(BindingFlags.Instance | BindingFlags.Public))
            {
                try
                {
                    if (IsValid(property))
                    {
                        object propertyValue = property.GetValue(comp, null);

                        if (propertyValue != null)
                        {
                            _members.Add(new SerializableMember(property, propertyValue));
                        }
                    }

                }
                catch (NotSupportedException)
                {
                    throw new NotSupportedException(Utility.GetObjectMemberName(property.DeclaringType.Name, property.Name) + " is not supported. Please block");
                }
                catch (Exception)
                {
                    throw;
                }
            }
        }
        private bool IsValid(PropertyInfo info)
        {
            if (info.GetSetMethod() == null)
                return false;

            return IsValid((MemberInfo)info);
        }
        private bool IsValid(MemberInfo info)
        {
            string objectMemberValue = Utility.GetObjectMemberName(info.DeclaringType.Name, info.Name);

            return !BlockedTypes.IsBlocked(objectMemberValue);
        }
        public void Deserialize(Component target)
        {
            if (target == null)
                throw new System.NullReferenceException("Given component is null!");

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
