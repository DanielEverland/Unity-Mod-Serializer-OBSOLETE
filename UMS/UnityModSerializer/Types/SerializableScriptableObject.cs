using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using UMS.Core;
using UnityEngine;

namespace UMS.Types
{
    public class SerializableScriptableObject : SerializableObject<ScriptableObject, SerializableScriptableObject>
    {
        public SerializableScriptableObject() { }
        public SerializableScriptableObject(ScriptableObject obj) : base(obj)
        {
            _memberCollection = new MemberCollection<ScriptableObject>(obj);
        }

        public override string Extension => "scriptableObject";

        private UnityEngine.Object _obj;
        
        [JsonProperty]
        private MemberCollection<ScriptableObject> _memberCollection;
        [JsonProperty]
        private Type _type;

        public override ScriptableObject Deserialize()
        {
            ScriptableObject obj = ScriptableObject.CreateInstance(_type);

            _memberCollection.Deserialize(obj);

            return obj;
        }
        public static SerializableScriptableObject Serialize(ScriptableObject obj)
        {
            return new SerializableScriptableObject(obj);
        }
    }
}
