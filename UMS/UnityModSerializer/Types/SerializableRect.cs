﻿using Newtonsoft.Json;
using UnityEngine;

namespace UMS.Types
{
    public class SerializableRect : Serializable<Rect, SerializableRect>
    {
        public SerializableRect() { }
        public SerializableRect(Rect rect)
        {
            _x = rect.x;
            _y = rect.y;
            _width = rect.width;
            _height = rect.height;
        }

        [JsonProperty]
        private float _x;
        [JsonProperty]
        private float _y;
        [JsonProperty]
        private float _width;
        [JsonProperty]
        private float _height;

        public override Rect Deserialize()
        {
            return new Rect(_x, _y, _width, _height);
        }
        public static SerializableRect Serialize(Rect obj)
        {
            return new SerializableRect(obj);
        }

        public static implicit operator SerializableRect(Rect rect)
        {
            return new SerializableRect(rect);
        }
        public static implicit operator Rect(SerializableRect serialized)
        {
            return serialized.Deserialize();
        }
    }
}