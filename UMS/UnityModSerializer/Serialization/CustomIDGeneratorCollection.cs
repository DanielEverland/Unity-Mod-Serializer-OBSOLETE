using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;
using UnityEditor;
using UMS.Core;
using UMS.Behaviour;

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

            unchecked
            {
                int propertyCount = ShaderUtil.GetPropertyCount(material.shader);

                for (int i = 0; i < propertyCount; i++)
                {
                    if(material.HasProperty(i))
                    {
                        object obj = GetValue(material, i);

                        if(obj != null)
                            id += obj.GetHashCode() * 11;
                    }                        
                }
            }

            return id;
        }
        private static object GetValue(Material material, int index)
        {
            switch (ShaderUtil.GetPropertyType(material.shader, index))
            {
                case ShaderUtil.ShaderPropertyType.Color:
                    return material.GetColor(index);
                case ShaderUtil.ShaderPropertyType.Vector:
                    return material.GetVector(index);
                case ShaderUtil.ShaderPropertyType.TexEnv:
                    return material.GetTexture(index);
                default:
                    return null;
            }
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
