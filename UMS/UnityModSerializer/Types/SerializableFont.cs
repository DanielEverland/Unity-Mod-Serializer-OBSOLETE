using Newtonsoft.Json;
using UnityEngine;

namespace UMS.Types
{
    public class SerializableFont : SerializableObject<Font, SerializableFont>
    {
        public override string Extension => "font";

        public SerializableFont() { }
        public SerializableFont(Font font) : base(font)
        {
            _fontNames = font.fontNames;
            _fontSize = font.fontSize;
        }

        [JsonProperty]
        private string[] _fontNames;
        [JsonProperty]
        private int _fontSize;

        public override Font Deserialize()
        {
            Font font = Font.CreateDynamicFontFromOSFont(_fontNames, _fontSize);

            Deserialize(font);

            return font;
        }
        public static SerializableFont Serialize(Font obj)
        {
            return new SerializableFont(obj);
        }
    }
}