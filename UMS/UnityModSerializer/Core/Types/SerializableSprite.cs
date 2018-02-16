using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;
using Newtonsoft.Json;
using UMS.Deserialization;

namespace UMS.Core.Types
{
    public class SerializableSprite : SerializableObject<Sprite, SerializableSprite>
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
        private Rect _rect;
        [JsonProperty]
        private Vector2 _pivot;
        [JsonProperty]
        private float _pixelsPerUnit;
        [JsonProperty]
        private Vector4 _border;

        public override Sprite Deserialize(SerializableSprite serializable)
        {
            Sprite sprite = new Sprite();

            Deserializer.GetDeserializedObject<Texture2D>(serializable._texture.ID, texture =>
            {
                sprite = Sprite.Create(texture, serializable._rect, serializable._pivot, serializable._pixelsPerUnit, 0, SpriteMeshType.Tight, serializable._border);
            });

            return sprite;
        }
        public override SerializableSprite Serialize(Sprite obj)
        {
            return new SerializableSprite(obj);
        }
    }
}
