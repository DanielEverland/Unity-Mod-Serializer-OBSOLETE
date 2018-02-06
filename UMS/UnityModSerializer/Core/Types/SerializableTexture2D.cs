using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Newtonsoft.Json;
using UnityEngine;

namespace UMS.Core.Types
{
    [Serializable]
    [JsonObject(MemberSerialization = MemberSerialization.OptIn)]
    public class SerializableTexture2D : SerializableTexture<Texture2D, SerializableTexture2D>
    {
        public override string Extension => "texture";

        public SerializableTexture2D() : base() { } 
        public SerializableTexture2D(Texture2D texture) : base(texture)
        {
            _rawData = texture.GetRawTextureData();
            _alphaIsTransparency = texture.alphaIsTransparency;
            _format = (int)texture.format;
            _mipMapCount = texture.mipmapCount;
        }

        [JsonProperty]
        private byte[] _rawData;
        [JsonProperty]
        private bool _alphaIsTransparency;
        [JsonProperty]
        private int _format;
        [JsonProperty]
        private int _mipMapCount;

        public override Texture2D Deserialize(SerializableTexture2D serializable)
        {
            Texture2D texture = new Texture2D(serializable._width, serializable._height, (TextureFormat)serializable._format, serializable._mipMapCount > 0);

            texture.wrapMode = (TextureWrapMode)serializable._wrapMode;
            texture.wrapModeW = (TextureWrapMode)serializable._wrapModeW;
            texture.wrapModeV = (TextureWrapMode)serializable._wrapModeV;
            texture.wrapModeU = (TextureWrapMode)serializable._wrapModeU;

            texture.anisoLevel = serializable._anisoLevel;
            texture.filterMode = (FilterMode)serializable._filterMode;
            texture.mipMapBias = serializable._mipMapBias;

            texture.alphaIsTransparency = serializable._alphaIsTransparency;
            texture.LoadRawTextureData(serializable._rawData);

            texture.Apply();

            return texture;
        }
        public override SerializableTexture2D Serialize(Texture2D obj)
        {
            return new SerializableTexture2D(obj);
        }
    }
}
