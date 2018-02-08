﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;
using Newtonsoft.Json;
using UMS.Deserialization;

namespace UMS.Core.Types
{
    [Serializable]
    [JsonObject(MemberSerialization = MemberSerialization.OptIn)]
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
            _color = material.color;
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
        private Color _color;
        [JsonProperty]
        private bool _enableInstancing;
        [JsonProperty]
        private bool _doubleSidedGI;

        public override Material Deserialize(SerializableMaterial serializable)
        {
            Material material = new Material(Shader.Find("Standard"));

            material.globalIlluminationFlags = (MaterialGlobalIlluminationFlags)serializable._globalIlluminationFlags;
            material.shaderKeywords = serializable._shaderKeywords;
            material.renderQueue = serializable._renderQueue;
            material.mainTextureScale = serializable._mainTextureScale;
            material.mainTextureOffset = serializable._mainTextureOffset;
            material.color = serializable._color;
            material.enableInstancing = serializable._enableInstancing;
            material.doubleSidedGI = serializable._doubleSidedGI;

            Deserializer.GetDeserializedObject<Shader>(_shader.ID, shader =>
            {
                material.shader = shader;
            });
            Deserializer.GetDeserializedObject<Texture>(_mainTexture.ID, texture =>
            {
                material.mainTexture = texture;
            });

            return material;
        }
        public override SerializableMaterial Serialize(Material obj)
        {
            return new SerializableMaterial(obj);
        }
        [CustomConstructor(typeof(Material))]
        public static Material CustomConstructor()
        {
            return new Material(Shader.Find("Standard"));
        }
    }
}
