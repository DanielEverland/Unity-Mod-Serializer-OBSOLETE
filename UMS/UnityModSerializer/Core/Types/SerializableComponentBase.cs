using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Newtonsoft.Json;
using UMS.Deserialization;
using UnityEngine;

namespace UMS.Core.Types
{
    public abstract class SerializableComponentBase<TNonSerializable, TSerializable> :
        SerializableObject<TNonSerializable, TSerializable>, ISerializableComponentBase where TNonSerializable : Component
    {
        public SerializableComponentBase() { }
        public SerializableComponentBase(TNonSerializable component) : base(component)
        {
            _componentType = component.GetType();
            _fileName = string.Format("{0} - {1}", component.gameObject.name, component.GetType().Name);
        }

        public override string Extension { get { return "component"; } }
        public override string FileName { get { return _fileName; } }

        public Type ComponentType { get { return _componentType; } }

        [JsonProperty]
        private Type _componentType;

        [JsonIgnore]
        private string _fileName;

        public void Deserialize(Component component)
        {
            OnDeserialize((TNonSerializable)component);
        }
        public abstract void OnDeserialize(TNonSerializable component);
    }
    public interface ISerializableComponentBase : IDeserializer<Component>
    {
        Type ComponentType { get; }    
    }
}
