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

        [JsonProperty]
        private bool _alphaIsTransparency;
        [JsonProperty]
        private int _format;
        [JsonProperty]
        private int _mipMapCount;
        [JsonProperty]
        private string _imageFilePath;

        public override Texture2D Deserialize(SerializableTexture2D serializable)
        {
            Texture2D texture = new Texture2D(serializable._width, serializable._height, (TextureFormat)serializable._format, serializable._mipMapCount > 1);

            serializable.Deserialize(texture);

            texture.wrapMode = (TextureWrapMode)serializable._wrapMode;
            texture.wrapModeW = (TextureWrapMode)serializable._wrapModeW;
            texture.wrapModeV = (TextureWrapMode)serializable._wrapModeV;
            texture.wrapModeU = (TextureWrapMode)serializable._wrapModeU;

            texture.anisoLevel = serializable._anisoLevel;
            texture.filterMode = (FilterMode)serializable._filterMode;
            texture.mipMapBias = serializable._mipMapBias;

            texture.alphaIsTransparency = serializable._alphaIsTransparency;

            byte[] data = StaticObjects.GetObject(serializable._imageFilePath);

            texture.LoadImage(data, false);
            texture.Apply();

            return texture;
        }
        public override SerializableTexture2D Serialize(Texture2D obj)
        {
            return new SerializableTexture2D(obj);
        }
    }
}