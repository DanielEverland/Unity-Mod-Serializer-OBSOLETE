using Newtonsoft.Json;
using UnityEngine;
using UnityEngine.UI;

namespace UMS.Types
{
    public class SerializableColorBlock : Serializable<ColorBlock, SerializableColorBlock>
    {
        public SerializableColorBlock() { }
        public SerializableColorBlock(ColorBlock obj)
        {
            _normalColor = obj.normalColor;
            _highlightedColor = obj.highlightedColor;
            _pressedColor = obj.pressedColor;
            _disabledColor = obj.disabledColor;
            _colorMultiplier = obj.colorMultiplier;
            _fadeDuration = obj.fadeDuration;
        }

        [JsonProperty]
        private Color _normalColor;
        [JsonProperty]
        private Color _highlightedColor;
        [JsonProperty]
        private Color _pressedColor;
        [JsonProperty]
        private Color _disabledColor;
        [JsonProperty]
        private float _colorMultiplier;
        [JsonProperty]
        private float _fadeDuration;

        public override ColorBlock Deserialize(SerializableColorBlock serializable)
        {
            return new ColorBlock()
            {
                normalColor = serializable._normalColor,
                highlightedColor = serializable._highlightedColor,
                pressedColor = serializable._pressedColor,
                disabledColor = serializable._disabledColor,
                colorMultiplier = serializable._colorMultiplier,
                fadeDuration = serializable._fadeDuration,
            };
        }
        public override SerializableColorBlock Serialize(ColorBlock obj)
        {
            return new SerializableColorBlock(obj);
        }
    }
}