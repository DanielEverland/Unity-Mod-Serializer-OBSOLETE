using Newtonsoft.Json;
using UnityEngine;

namespace UMS.Types
{
    public abstract class SerializableTexture<TFrom, TTo> : SerializableObject<TFrom, TTo> where TFrom : UnityEngine.Object
    {
        public SerializableTexture() { }
        public SerializableTexture(Texture texture) : base(texture)
        {
            _mipMapBias = texture.mipMapBias;
            _wrapModeW = (int)texture.wrapModeW;
            _wrapModeV = (int)texture.wrapModeV;
            _wrapModeU = (int)texture.wrapModeU;
            _wrapMode = (int)texture.wrapMode;
            _anisoLevel = texture.anisoLevel;
            _height = texture.height;
            _width = texture.width;
            _filterMode = (int)texture.filterMode;
        }

        [JsonProperty]
        protected float _mipMapBias;
        [JsonProperty]
        protected int _wrapModeW;
        [JsonProperty]
        protected int _wrapModeV;
        [JsonProperty]
        protected int _wrapModeU;
        [JsonProperty]
        protected int _wrapMode;
        [JsonProperty]
        protected int _anisoLevel;
        [JsonProperty]
        protected int _height;
        [JsonProperty]
        protected int _width;
        [JsonProperty]
        protected int _filterMode;
    }
}