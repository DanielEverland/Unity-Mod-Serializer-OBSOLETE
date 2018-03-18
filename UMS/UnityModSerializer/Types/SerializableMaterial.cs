using Newtonsoft.Json;
using UMS.Deserialization;
using UnityEngine;

namespace UMS.Types
{
    public class SerializableMaterial : SerializableObject<Material, SerializableMaterial>
    {
        public override string Extension => "material";

        public SerializableMaterial() { }
        public SerializableMaterial(Material material) : base(material)
        {
            _shader = Reference.Create(material.shader);
            _globalIlluminationFlags = (int)material.globalIlluminationFlags;
            _shaderKeywords = material.shaderKeywords;
            _renderQueue = material.renderQueue;
            _mainTextureScale = material.mainTextureScale;
            _mainTextureOffset = material.mainTextureOffset;
            _mainTexture = Reference.Create(material.mainTexture);
            _enableInstancing = material.enableInstancing;
            _doubleSidedGI = material.doubleSidedGI;
        }

        [JsonProperty]
        private Reference _shader;
        [JsonProperty]
        private int _globalIlluminationFlags;
        [JsonProperty]
        private string[] _shaderKeywords;
        [JsonProperty]
        private int _renderQueue;
        [JsonProperty]
        private Vector2 _mainTextureScale;
        [JsonProperty]
        private Vector2 _mainTextureOffset;
        [JsonProperty]
        private Reference _mainTexture;
        [JsonProperty]
        private bool _enableInstancing;
        [JsonProperty]
        private bool _doubleSidedGI;

        public override Material Deserialize()
        {
            Material material = new Material(Shader.Find("Diffuse"));

            Deserialize(material);

            material.globalIlluminationFlags = (MaterialGlobalIlluminationFlags)_globalIlluminationFlags;
            material.shaderKeywords = _shaderKeywords;
            material.renderQueue = _renderQueue;
            material.mainTextureScale = _mainTextureScale;
            material.mainTextureOffset = _mainTextureOffset;
            material.enableInstancing = _enableInstancing;
            material.doubleSidedGI = _doubleSidedGI;

            if (_shader != null)
            {
                Deserializer.GetDeserializedObject<Shader>(_shader.ID, shader =>
                {
                    material.shader = shader;
                });
            }

            if (_mainTexture != null)
            {
                Deserializer.GetDeserializedObject<Texture>(_mainTexture.ID, texture =>
                {
                    material.mainTexture = texture;
                });
            }

            return material;
        }
        public static SerializableMaterial Serialize(Material obj)
        {
            return new SerializableMaterial(obj);
        }
        [CustomConstructor(typeof(Material))]
        public static Material CustomConstructor(System.Type type)
        {
            return new Material(Shader.Find("Diffuse"));
        }
    }
}