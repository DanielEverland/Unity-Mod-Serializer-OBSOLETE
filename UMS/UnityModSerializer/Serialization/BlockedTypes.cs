using System;
using System.Reflection;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UMS.Behaviour;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using UMS.Core;

namespace UMS.Serialization
{
    public static class BlockedTypes
    {
        #region Definitions
        [TypeBlocker()]
        public static readonly HashSet<Type> Types = new HashSet<Type>()
        {
            typeof(Scene),
        };

        [MemberBlocker()]
        public static readonly HashSet<string> Members = new HashSet<string>()
        {
            "MeshFilter.mesh",
            "Renderer.material",
            "Renderer.materials",
            "Canvas.rootCanvas",
            "Graphic.canvas",
            "Transform.root",
            "Component.rigidbody",
        };
        #endregion

        #region Manager
        public static void Initialize()
        {
            BehaviourManager.OnBehaviourLoaded += OnBehaviourLoaded;
        }
        private static void OnBehaviourLoaded(BehaviourBase behaviour)
        {
            if(behaviour is TypeBlocker typeBlocker)
            {
                foreach (Type type in typeBlocker.TypeFunction())
                {
                    if (!_blockedTypes.Contains(type))
                        _blockedTypes.Add(type);
                }
            }
            else if(behaviour is MemberBlocker memberBlocker)
            {
                foreach (string member in memberBlocker.MemberFunction())
                {
                    if (!_blockedMembers.Contains(member))
                        _blockedMembers.Add(member);
                }
            }
        }

        private static HashSet<Type> _blockedTypes = new HashSet<Type>();
        private static HashSet<string> _blockedMembers = new HashSet<string>();

        public static bool IsBlocked(Type type)
        {
            return _blockedTypes.Contains(type);
        }
        public static bool IsBlocked(string member)
        {
            return _blockedMembers.Contains(member);
        }
        #endregion
    }
}
