﻿using Newtonsoft.Json;
using UMS.Deserialization;
using UMS.Core;
using UnityEngine;
using UnityEngine.UI;

namespace UMS.Types
{
    /// <summary>
    /// Implemented custom class because it has a tendency to throw exceptions if Content isn't assigned first
    /// </summary>
    public sealed class SerializableScrollRect : SerializableComponentBase<ScrollRect, SerializableScrollRect>
    {
        public SerializableScrollRect() { }
        public SerializableScrollRect(ScrollRect obj) : base(obj)
        {
            if (obj.content != null)
                _content = Reference.Create(obj.content);

            _members = new MemberCollection<ScrollRect>(obj);
        }

        [JsonProperty]
        private Reference _content;
        [JsonProperty]
        private MemberCollection<ScrollRect> _members;

        public override void OnDeserialize(ScrollRect component)
        {
            Deserializer.GetDeserializedObject<RectTransform>(_content.ID, content =>
            {
                component.content = content;

                _members.Deserialize(component);
            });
        }
        public static SerializableScrollRect Serialize(ScrollRect obj)
        {
            return new SerializableScrollRect(obj);
        }
    }
}