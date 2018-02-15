using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;
using Newtonsoft.Json;
using UMS.Deserialization;

namespace UMS.Core.Types
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

        public override Font Deserialize(SerializableFont serializable)
        {
            Font font = Font.CreateDynamicFontFromOSFont(serializable._fontNames, serializable._fontSize);
            
            serializable.Deserialize(font);

            return font;
        }
        public override SerializableFont Serialize(Font obj)
        {
            return new SerializableFont(obj);
        }
    }
}
