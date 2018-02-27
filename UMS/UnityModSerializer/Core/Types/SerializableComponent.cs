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
    public sealed class SerializableComponent : SerializableComponentBase<Component, SerializableComponent>
    {
        public SerializableComponent() { }
        public SerializableComponent(Component obj) : base(obj)
        {
            _memberCollection = ComponentMemberCollection.Create(obj);
        }

        [JsonProperty]
        private ComponentMemberCollection _memberCollection;
        
        public override void OnDeserialize(Component target)
        {
            if (target == null)
                throw new System.NullReferenceException("Given component is null!");
            
            _memberCollection.Deserialize(target);
        }
        public override SerializableComponent Serialize(Component obj)
        {
            return new SerializableComponent(obj);
        }
    }
}