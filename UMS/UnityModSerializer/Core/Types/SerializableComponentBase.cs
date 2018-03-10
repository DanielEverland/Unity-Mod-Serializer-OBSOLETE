using Newtonsoft.Json;
using System;
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
            _fileName = component.GetType().Name;
            _folderName = Utility.GetGameObjectFolderName(component.gameObject);
        }

        public override string Extension { get { return "component"; } }
        public override string FileName { get { return _fileName; } }
        public override string FolderName { get { return _folderName; } }

        public Type ComponentType { get { return _componentType; } }

        [JsonProperty]
        private Type _componentType;

        [JsonIgnore]
        private string _fileName;
        [JsonIgnore]
        private string _folderName;

        public void Deserialize(Component component)
        {
            Deserialize(component as UnityEngine.Object);

            OnDeserialize((TNonSerializable)component);

            Deserializer.AssignDeserializedObject(ID, component);
        }
        public abstract void OnDeserialize(TNonSerializable component);
    }
    public interface ISerializableComponentBase : IDeserializer<Component>
    {
        Type ComponentType { get; }
        int ID { get; }
    }
}