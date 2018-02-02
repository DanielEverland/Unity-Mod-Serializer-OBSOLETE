using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UMS.Behaviour;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

namespace UMS.Serialization
{
    public class BlockedTypes
    {
        [TypeBlocker()]
        public static readonly List<Type> Types = new List<Type>()
        {
            typeof(Scene),
        };

        [MemberBlocker()]
        public static readonly List<string> Members = new List<string>()
        {
            "MeshFilter.mesh",
            "Renderer.material",
            "Renderer.materials",
            "Canvas.rootCanvas",
            "Graphic.canvas",
        };
    }
}
