using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;

namespace UMS.Behaviour
{
    /// <summary>
    /// Custom attribute used for defining type relationships during serializiation
    /// </summary>
    public abstract class TypeBehaviour : BehaviourBase
    {
        private TypeBehaviour() { }
        public TypeBehaviour(Type type, int priority = (int)Core.Priority.Medium) : base(priority)
        {
            this._type = type;
        }

        public Type ReturnType
        {
            get
            {
                if (method == null)
                    throw new NullReferenceException();

                return method.ReturnType;
            }
        }
        public Type AttributeType { get { return _type; } }

        private readonly Type _type;

        protected MethodInfo method;

        public void SetMethod(MethodInfo method)
        {
            this.method = method;
        }
    }
}
