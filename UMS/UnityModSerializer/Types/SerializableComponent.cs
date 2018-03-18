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
            _memberCollection = new MemberCollection<Component>(obj);
        }

        [JsonProperty]
        private MemberCollection<Component> _memberCollection;

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