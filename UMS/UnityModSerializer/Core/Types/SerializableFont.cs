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
            _material = Reference.Create(font.material);
        }
        
        [JsonProperty]
        private Reference _material;

        public override Font Deserialize(SerializableFont serializable)
        {
            Font font = new Font();

            serializable.Deserialize(font);            

            Deserializer.GetDeserializedObject<Material>(serializable._material.ID, material =>
            {
                font.material = material;
            });

            return font;
        }
        public override SerializableFont Serialize(Font obj)
        {
            return new SerializableFont(obj);
        }
    }
}
