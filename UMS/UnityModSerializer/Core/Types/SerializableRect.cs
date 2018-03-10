using Newtonsoft.Json;
using UnityEngine;

namespace UMS.Core.Types
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

        public override Rect Deserialize(SerializableRect serializable)
        {
            return new Rect(serializable._x, serializable._y, serializable._width, serializable._height);
        }
        public override SerializableRect Serialize(Rect obj)
        {
            return new SerializableRect(obj);
        }

        public static implicit operator SerializableRect(Rect rect)
        {
            return new SerializableRect(rect);
        }
        public static implicit operator Rect(SerializableRect serialized)
        {
            return serialized.Deserialize(serialized);
        }
    }
}