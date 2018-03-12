using UnityEngine;

namespace UMS.Types
{
    public class SerializableShader : SerializableObject<Shader, SerializableShader>
    {
        public SerializableShader() { }
        public SerializableShader(Shader shader) : base(shader)
        {
        }

        public override string Extension => "shader";

        public static SerializableShader Serialize(Shader obj)
        {
            return new SerializableShader(obj);
        }
        public override Shader Deserialize()
        {
            return Shader.Find(Name);
        }
    }
}