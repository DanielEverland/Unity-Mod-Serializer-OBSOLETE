using System;
using System.Reflection;
using Newtonsoft.Json.Linq;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UMS.Serialization;

namespace UMS.Behaviour
{
    /// <summary>
    /// Defines a function that returns a serializable object. Must return object, be static, and contain a single parameter of same type as defined in constructor
    /// </summary>
    [AttributeUsage(AttributeTargets.Method, AllowMultiple = true, Inherited = false)]
    public class TypeSerializer : TypeBehaviour, IBehaviourLoader<MethodInfo>
    {
        public TypeSerializer(Type type, int priority = 0) : base(type, priority)
        {
        }

        public bool IsValid(MethodInfo info)
        {
            ParameterInfo[] parameters = info.GetParameters();
            bool validParameters = false;

            if (parameters.Length == 1)
            {
                validParameters = parameters[0].ParameterType == AttributeType;
            }

            if (!validParameters)
            {
                throw BehaviourException.Generate(this, "Method must contain a single parameter of type " + AttributeType);
            }

            bool validReturnType = info.ReturnType == typeof(object);

            if (!validReturnType)
            {
                throw BehaviourException.Generate(this, "Return type must be object");
            }

            bool isStatic = info.IsStatic;

            if (!isStatic)
            {
                throw BehaviourException.Generate(this, "Method must be static");
            }

            return validReturnType && validParameters && isStatic;

        }

        public void Load(MethodInfo type)
        {
            if (IsValid(type))
            {
                SetMethod(type);
            }
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
    }
}
