using Newtonsoft.Json;
using System;
using UnityEngine;
using UMS.Core;

namespace UMS.Types
{
    public sealed class SerializableScriptableObject : SerializableObject<ScriptableObject, SerializableScriptableObject>
    {
        public override string Extension => "scriptableObject";

        private string objectPath
        {
            get
            {
                return "ScriptableObjects/" + Name + ".txt";
            }
        }

        public SerializableScriptableObject() { }
        public SerializableScriptableObject(ScriptableObject scriptableObject) : base(scriptableObject)
        {
            _type = scriptableObject.GetType();
#if EDITOR
            StaticObjects.Add(objectPath, JsonUtility.ToJson(scriptableObject, true).ToByteArray());
#endif
        }

        [JsonProperty]
        private Type _type;

        public override ScriptableObject Deserialize()
        {
            ScriptableObject scriptableObject = ScriptableObject.CreateInstance(_type);
            string json = StaticObjects.GetObject(objectPath).ToObject<string>();

            JsonUtility.FromJsonOverwrite(json, scriptableObject);

            return scriptableObject;
        }
        public static SerializableScriptableObject Serialize(ScriptableObject obj)
        {
            return new SerializableScriptableObject(obj);
        }
    }
}