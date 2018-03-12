using Newtonsoft.Json;
using UMS.Core;
using UnityEngine;

namespace UMS.Types
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
        public static SerializableComponent Serialize(Component obj)
        {
            return new SerializableComponent(obj);
        }
    }
}