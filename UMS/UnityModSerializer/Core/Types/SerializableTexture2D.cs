using Newtonsoft.Json;
using UnityEngine;
using UMS.Serialization;
using UMS.Deserialization;
using System.IO;

namespace UMS.Core.Types
{
    public class SerializableTexture2D : SerializableTexture<Texture2D, SerializableTexture2D>
    {
        public override string Extension => "texture";

        public SerializableTexture2D() : base() { }
        /// <summary>
        /// Creates a serializable texture
        /// </summary>
        /// <param name="texture">Source</param>
        /// <param name="createTextureInstance">Determines whether we use raw data or an instance of the image file</param>
        public SerializableTexture2D(Texture2D texture, bool createTextureInstance = true) : base(texture)
        {
            if (createTextureInstance && Utility.IsReadable(texture))
            {
                _imageFilePath = "Textures/" + texture.name + ".png";

                byte[] textureData = texture.EncodeToPNG();

                _imageFilePath = StaticObjects.AddObject(_imageFilePath, textureData);
            }
            else
            {
                _rawData = texture.GetRawTextureData();
            }
            
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

            if(serializable._imageFilePath != null)
            {
                byte[] data = StaticObjects.GetObject(serializable._imageFilePath);

                texture.LoadImage(data, false);
            }
            else
            {
                texture.LoadRawTextureData(serializable._rawData);
            }            

            texture.Apply();

            return texture;
        }
        public override SerializableTexture2D Serialize(Texture2D obj)
        {
            return new SerializableTexture2D(obj);
        }
    }
}