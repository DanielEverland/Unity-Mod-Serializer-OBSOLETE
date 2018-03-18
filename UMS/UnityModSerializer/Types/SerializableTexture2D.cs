using Newtonsoft.Json;
using UMS.Core;
using UnityEngine;

namespace UMS.Types
{
    public class SerializableTexture2D : SerializableTexture<Texture2D, SerializableTexture2D>
    {
        public override string Extension => "texture";

        public SerializableTexture2D() : base() { }
        /// <summary>
        /// Creates a serializable texture
        /// </summary>
        /// <param name="texture">Source</param>
        public SerializableTexture2D(Texture2D texture) : base(texture)
        {
            _imageFilePath = "Textures/" + texture.name + ".png";

            byte[] textureData = Utility.EncodeToPNG(texture);

            _imageFilePath = StaticObjects.Add(_imageFilePath, textureData);

            _alphaIsTransparency = texture.alphaIsTransparency;
            _format = (int)texture.format;
            _mipMapCount = texture.mipmapCount;
        }

#pragma warning disable 0649
        [JsonProperty]
        private bool _alphaIsTransparency;
        [JsonProperty]
        private int _format;
        [JsonProperty]
        private int _mipMapCount;
        [JsonProperty]
        private string _imageFilePath;
#pragma warning restore

        public override Texture2D Deserialize()
        {
            Texture2D texture = new Texture2D(_width, _height, (TextureFormat)_format, _mipMapCount > 1);

            Deserialize(texture);

            texture.wrapMode = (TextureWrapMode)_wrapMode;
            texture.wrapModeW = (TextureWrapMode)_wrapModeW;
            texture.wrapModeV = (TextureWrapMode)_wrapModeV;
            texture.wrapModeU = (TextureWrapMode)_wrapModeU;

            texture.anisoLevel = _anisoLevel;
            texture.filterMode = (FilterMode)_filterMode;
            texture.mipMapBias = _mipMapBias;

            texture.alphaIsTransparency = _alphaIsTransparency;

            byte[] data = StaticObjects.GetObject(_imageFilePath);

            texture.LoadImage(data, false);
            texture.Apply();

            return texture;
        }
        public static SerializableTexture2D Serialize(Texture2D obj)
        {
            return new SerializableTexture2D(obj);
        }
    }
}