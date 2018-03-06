using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine.UI;
using UnityEngine;
using UMS.Deserialization;
using UMS.Serialization;
using Newtonsoft.Json;

namespace UMS.Core.Types
{
    public class SerializableRectTransform : SerializableComponentBase<RectTransform, SerializableRectTransform>
    {
        public SerializableRectTransform() { }
        public SerializableRectTransform(RectTransform rectTransform) : base(rectTransform)
        {
            _pivot = rectTransform.pivot;
            _offsetMax = rectTransform.offsetMax;
            _offsetMin = rectTransform.offsetMin;
            _anchoredPosition3D = rectTransform.anchoredPosition3D;
            _anchorMin = rectTransform.anchorMin;
            _anchorMax = rectTransform.anchorMax;
            _sizeDelta = rectTransform.sizeDelta;
        }
        
        [JsonProperty]
        private Vector2 _pivot;
        [JsonProperty]
        private Vector2 _offsetMax;
        [JsonProperty]
        private Vector2 _offsetMin;
        [JsonProperty]
        private Vector3 _anchoredPosition3D;
        [JsonProperty]
        private Vector2 _anchorMin;
        [JsonProperty]
        private Vector3 _anchorMax;
        [JsonProperty]
        private Vector2 _sizeDelta;

        public override void OnDeserialize(RectTransform rectTransform)
        {
            HierarchyManager.Subscribe(rectTransform, transform =>
            {
                transform.pivot = _pivot;
                transform.anchorMin = _anchorMin;
                transform.anchorMax = _anchorMax;
                transform.anchoredPosition3D = _anchoredPosition3D;
                transform.offsetMin = _offsetMin;
                transform.offsetMax = _offsetMax;
                transform.sizeDelta = _sizeDelta;
            });            
        }
        public override SerializableRectTransform Serialize(RectTransform obj)
        {
            return new SerializableRectTransform(obj);
        }
    }
}
