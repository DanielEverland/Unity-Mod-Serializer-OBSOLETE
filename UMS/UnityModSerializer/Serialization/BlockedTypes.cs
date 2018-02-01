using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UMS.Behaviour;
using UnityEngine;

namespace UMS.Serialization
{
    public class BlockedTypes
    {
        [TypeBlocker()]
        public static readonly List<Type> Types = new List<Type>()
        {
            typeof(GameObject),
            typeof(Transform),
        };

        [MemberBlocker()]
        public static readonly List<string> Members = new List<string>()
        {
            "MeshFilter.mesh",
            "Renderer.material",
            "Renderer.materials",
        };
    }
}
