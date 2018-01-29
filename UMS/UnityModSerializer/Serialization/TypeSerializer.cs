using System;
using System.Reflection;
using Newtonsoft.Json.Linq;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace UMS.Serialization
{
    /// <summary>
    /// Defines a function that returns a serializable object. Must return object, be static, and contain a single parameter of same type as defined in constructor
    /// </summary>
    [AttributeUsage(AttributeTargets.Method, AllowMultiple = true, Inherited = false)]
    public class TypeSerializer : Attribute
    {
        private TypeSerializer() { }
        public TypeSerializer(Type type, int priority = 0)
        {
            this.type = type;
            this.priority = priority;
        }

        public Type ReturnType { get { return method.ReturnType; } }
        
        public readonly Type type;
        public readonly int priority;

        private MethodInfo method;

        public bool IsValid(MethodInfo info)
        {
            ParameterInfo[] parameters = info.GetParameters();
            bool validParameters = false;

            if (parameters.Length == 1)
            {
                validParameters = parameters[0].ParameterType == type;
            }

            if (!validParameters)
            {
                UnityEngine.Debug.LogError("Detected improper usage of TypeSerailizer. Method must contain a single parameter of type " + type);
            }

            bool validReturnType = info.ReturnType == typeof(object);

            if (!validReturnType)
            {
                UnityEngine.Debug.LogError("Detected improper usage of TypeSerializer. Return type must be object");
            }

            bool isStatic = info.IsStatic;

            if (!isStatic)
            {
                UnityEngine.Debug.LogError("Detected improper usage of TypeSerializer. Method must be static");
            }

            return validReturnType && validParameters && isStatic;
        }
        public bool Serialize(object input, out object output)
        {
            output = method.Invoke(null, new object[1] { input });

            if (JsonSerializer.CanSerialize(output))
            {
                return true;
            }
            else
            {
                UnityEngine.Debug.LogError("Detected improper usage of TypeSerializer. Returned object must implement System.Serializable attribute");
            }

            output = null;
            return false;
        }
        public void SetMethod(MethodInfo method)
        {
            if (IsValid(method))
            {
                this.method = method;
            }            
        }
    }
}
