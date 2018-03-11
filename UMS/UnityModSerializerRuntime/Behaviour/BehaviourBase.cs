using System;

namespace UMS.Runtime.Behaviour
{
    /// <summary>
    /// Custom attribute used for defining ruleset during serializiation
    /// </summary>
    public abstract class BehaviourBase : Attribute
    {
        private BehaviourBase() { }
        public BehaviourBase(int priority = (int)Core.Priority.Medium)
        {
            _priority = priority;
        }

        public int Priority { get { return _priority; } }

        private readonly int _priority;
    }
}