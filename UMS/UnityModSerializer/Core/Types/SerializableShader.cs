using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;
using Newtonsoft.Json;

namespace UMS.Core.Types
{
    public class SerializableShader : SerializableObject<Shader, SerializableShader>
    {
        public SerializableShader() { }
        public SerializableShader(Shader shader) : base(shader)
        {
        }

        public override string Extension => "shader";

        public override SerializableShader Serialize(Shader obj)
        {
            return new SerializableShader(obj);
        }
        public override Shader Deserialize(SerializableShader serializable)
        {
            return Shader.Find(serializable.Name);
        }
    }
}
