﻿using System;
using System.Reflection;
using UMS.Behaviour;

namespace UMS.Deserialization
{
    [AttributeUsage(AttributeTargets.Method)]
    public class CustomConstructorAttribute : BehaviourBase
    {
        private CustomConstructorAttribute() { }
        /// <summary>
        /// Useful for creating an instance of a class you don't have access to, such as a Unity class, using a custom defined constructor.
        /// </summary>
        /// <param name="targetType">Type to create an instance of. Your method return type must match this</param>
        public CustomConstructorAttribute(Type targetType)
        {
            TargetType = targetType;
        }

        public Type TargetType { get; private set; }

        public MethodInfo Method { get; private set; }

        public void AssignMethod(MethodInfo info)
        {
            if (!info.IsStatic)
                throw new ArgumentException(info + " must be static");

            if (info.ReturnType != TargetType)
                throw new ArgumentException(info + " return type must be " + TargetType);

            if (!ValidParameters(info))
                throw new ArgumentException(info + " cannot have any parameters");

            Method = info;
        }
        private bool ValidParameters(MethodInfo info)
        {
            return info.GetParameters().Length == 0;
        }
    }
}