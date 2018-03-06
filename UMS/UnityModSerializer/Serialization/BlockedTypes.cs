using System;
using System.Collections.Generic;
using UMS.Behaviour;
using UnityEngine.SceneManagement;

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
            //Core
            "MeshFilter.mesh",
            "Renderer.material",
            "Renderer.materials",
            "Transform.root",
            "Component.rigidbody",

            //UI
            "Canvas.rootCanvas",
            "Graphic.canvas",
            "Graphic.material",
            "Image.material",
        };
        #endregion Definitions

        #region Manager
        public static void Initialize()
        {
            BehaviourManager.OnBehaviourLoaded += OnBehaviourLoaded;
        }
        private static void OnBehaviourLoaded(BehaviourBase behaviour)
        {
            if (behaviour is TypeBlockerAttribute typeBlocker)
            {
                foreach (Type type in typeBlocker.TypeFunction())
                {
                    if (!_blockedTypes.Contains(type))
                        _blockedTypes.Add(type);
                }
            }
            else if (behaviour is MemberBlockerAttribute memberBlocker)
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
        #endregion Manager
    }
}