using UMS.Behaviour;
using UMS.Core;
using UnityEngine;

namespace UMS.Serialization
{
    public static class CustomIDGeneratorCollection
    {
        [CustomIDGenerator(typeof(Material))]
        public static int GetMaterialID(Material material)
        {
            if (material == null)
                return -1;

            if (material.shader == null)
                return material.GetHashCode();

            int id = 17;
            
            return id;
        }
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