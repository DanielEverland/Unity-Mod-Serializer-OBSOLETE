using Newtonsoft.Json;
using System;
using UnityEngine;

namespace UMS.Core.Types
{
    public abstract class SerializableObject<TNonSerializable, TSerializable> : Serializable<TNonSerializable, TSerializable>,
                                                                                            IModEntry where TNonSerializable : UnityEngine.Object
    {
        public SerializableObject() { }
        public SerializableObject(UnityEngine.Object obj) : base((TNonSerializable)obj)
        {
            if (obj == null)
                throw new NullReferenceException("Object cannot be null");

            _name = obj.name;
            _hideFlags = obj.hideFlags;
        }

        public abstract string Extension { get; }
        public virtual string FileName { get { return Name; } }

        public string Name { get { return _name; } }
        public HideFlags HideFlags { get { return _hideFlags; } }

        [JsonProperty]
        private string _name;
        [JsonProperty]
        private HideFlags _hideFlags;
        
        protected void Deserialize(UnityEngine.Object obj)
        {
            obj.name = Name;
            obj.hideFlags = HideFlags;
        }
    }
}