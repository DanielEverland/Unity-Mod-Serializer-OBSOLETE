using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;
using UMS.Core;
using UMS.Behaviour;

namespace UMS.Serialization
{
    public static class CustomIDGeneratorCollection
    {
        [CustomIDGenerator(typeof(Font))]
        public static int GetFontID(Font font)
        {
            int id = 17;

            unchecked
            {
                id += font.fontNames.CollectionToString().GetHashCode() * 61;
                id += font.name.GetHashCode() * 11;
                id += font.fontSize.GetHashCode() * 113;
            }

            return id;
        }
    }
}
