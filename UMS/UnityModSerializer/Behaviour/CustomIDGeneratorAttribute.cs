using System;
using System.Reflection;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace UMS.Behaviour
{
    [AttributeUsage(AttributeTargets.Method)]
    public class CustomIDGeneratorAttribute : BehaviourBase, IBehaviourLoader<MethodInfo>
    {
        private CustomIDGeneratorAttribute() { }
        public CustomIDGeneratorAttribute(Type type, int priority = (int)Core.Priority.Medium) : base(priority)
        {
            _type = type;
        }

        public Type Type { get { return _type; } }

        private readonly Type _type;
        private MethodInfo _method;

        public void Load(MethodInfo type)
        {
            _method = type;
        }
        public int GetID(object obj)
        {
            if (obj.GetType() != Type)
                throw new ArgumentException();

            return (int)_method.Invoke(null, new object[1] { obj });
        }
    }
}
