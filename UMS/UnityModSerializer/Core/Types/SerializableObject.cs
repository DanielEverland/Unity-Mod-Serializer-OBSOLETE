using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

namespace UMS.Core.Types
{
    [Serializable]
    [JsonObject(MemberSerialization = MemberSerialization.OptIn)]
    public abstract class SerializableObject<TNonSerializable, TSerializable> : Serializable<TNonSerializable, TSerializable>,
                                                                                            IModEntry where TNonSerializable : UnityEngine.Object
    {
        public SerializableObject() { }
        public SerializableObject(UnityEngine.Object obj) : base((TNonSerializable)obj)
        {
            if (obj is null)
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
    }
}
