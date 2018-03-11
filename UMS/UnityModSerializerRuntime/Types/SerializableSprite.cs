using Newtonsoft.Json;
using System;
using UMS.Deserialization;
using UnityEngine;

namespace UMS.Runtime.Types
{
    public class SerializableSprite : SerializableObject<Sprite, SerializableSprite>, IAsynchronousDeserializer<SerializableSprite>
    {
        public override string Extension => "sprite";

        public SerializableSprite() { }
        public SerializableSprite(Sprite sprite) : base(sprite)
        {
            _texture = Reference.Create(sprite.texture);
            _pivot = sprite.pivot;
            _pixelsPerUnit = sprite.pixelsPerUnit;
            _border = sprite.border;
            _spriteRect = sprite.rect;
        }

        [JsonProperty]
        private Reference _texture;
        [JsonProperty]
        private Vector2 _pivot;
        [JsonProperty]
        private float _pixelsPerUnit;
        [JsonProperty]
        private Vector4 _border;
        [JsonProperty]
        private SerializableRect _spriteRect;

        public void AsynchronousDeserialization(Action<object> action, SerializableSprite serialized)
        {
            Deserializer.GetDeserializedObject<Texture2D>(serialized._texture.ID, texture =>
            {
                action(CreateSprite(serialized, texture));
            });
        }
        private Sprite CreateSprite(SerializableSprite serialized, Texture2D texture)
        {
            Sprite sprite = Sprite.Create(texture, serialized._spriteRect, serialized._pivot, serialized._pixelsPerUnit, 0, SpriteMeshType.Tight, serialized._border);

            serialized.Deserialize(sprite);

            return sprite;
        }
        public override SerializableSprite Serialize(Sprite obj)
        {
            return new SerializableSprite(obj);
        }
    }
}