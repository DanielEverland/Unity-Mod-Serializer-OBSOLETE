using Newtonsoft.Json;
using System;
using UnityEngine;

namespace UMS.Core.Types
{
    public sealed class SerializableScriptableObject : SerializableObject<ScriptableObject, SerializableScriptableObject>
    {
        public override string Extension => "scriptableObject";

        public SerializableScriptableObject() { }
        public SerializableScriptableObject(ScriptableObject scriptableObject) : base(scriptableObject)
        {
            _type = scriptableObject.GetType();
            _jsonString = JsonUtility.ToJson(scriptableObject, true);
        }

        [JsonProperty]
        private Type _type;
        [JsonProperty]
        private string _jsonString;

        public override ScriptableObject Deserialize(SerializableScriptableObject serializable)
        {
            ScriptableObject scriptableObject = ScriptableObject.CreateInstance(serializable._type);

            JsonUtility.FromJsonOverwrite(serializable._jsonString, scriptableObject);

            return scriptableObject;
        }
        public override SerializableScriptableObject Serialize(ScriptableObject obj)
        {
            return new SerializableScriptableObject(obj);
        }
    }
}