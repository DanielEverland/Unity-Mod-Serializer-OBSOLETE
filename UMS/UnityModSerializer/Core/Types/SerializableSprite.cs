using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;
using Newtonsoft.Json;
using UMS.Deserialization;

namespace UMS.Core.Types
{
    public class SerializableSprite : SerializableObject<Sprite, SerializableSprite>, IAsynchronousDeserializer<SerializableSprite>
    {
        public override string Extension => "sprite";
        
        public SerializableSprite() { }
        public SerializableSprite(Sprite sprite) : base(sprite)
        {
            _texture = Reference.Create(sprite.texture);
            _rect = sprite.rect;
            _pivot = sprite.pivot;
            _pixelsPerUnit = sprite.pixelsPerUnit;
            _border = sprite.border;
        }

        [JsonProperty]
        private Reference _texture;
        [JsonProperty]
        private SerializableRect _rect;
        [JsonProperty]
        private Vector2 _pivot;
        [JsonProperty]
        private float _pixelsPerUnit;
        [JsonProperty]
        private Vector4 _border;
        
        public void AsynchronousDeserialization(Action<object> action, SerializableSprite serialized)
        {
            Deserializer.GetDeserializedObject<Texture2D>(serialized._texture.ID, texture =>
            {
                action(CreateSprite(serialized, texture));
            });
        }
        private Sprite CreateSprite(SerializableSprite serialized, Texture2D texture)
        {
            Sprite sprite = Sprite.Create(texture, serialized._rect, serialized._pivot, serialized._pixelsPerUnit, 0, SpriteMeshType.Tight, serialized._border);
            
            serialized.Deserialize(sprite);

            return sprite;
        }
        public override SerializableSprite Serialize(Sprite obj)
        {
            return new SerializableSprite(obj);
        }
    }
}
