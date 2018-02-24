using Newtonsoft.Json;
using System;
using UnityEngine;

namespace UMS.Core.Types
{
    public abstract class SerializableObject<TNonSerializable, TSerializable> : Serializable<TNonSerializable, TSerializable>,
                                                                                            IModEntry where TNonSerializable : UnityEngine.Object
    {
        public SerializableObject() { }
        public SerializableObject(UnityEngine.Object obj)
        {
            if (obj == null)
                throw new NullReferenceException("Object cannot be null");

            _name = obj.name;
            _hideFlags = obj.hideFlags;
            _id = ObjectManager.Add(obj);
        }

        public abstract string Extension { get; }
        public virtual string FileName { get { return Name; } }
        public virtual string FolderName { get { return "Static Objects"; } }

        public string Name { get { return _name; } }
        public HideFlags HideFlags { get { return _hideFlags; } }
        public int ID { get { return _id; } protected set { _id = value; } }
                        
        [JsonProperty]
        private string _name;
        [JsonProperty]
        private HideFlags _hideFlags;
        [JsonProperty]
        private int _id;

        protected void Deserialize(UnityEngine.Object obj)
        {
            obj.name = Name;
            obj.hideFlags = HideFlags;
        }
    }
}