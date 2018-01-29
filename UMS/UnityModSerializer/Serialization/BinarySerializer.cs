using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.Serialization.Formatters.Binary;
using System.Text;
using UMS.Core;

namespace UMS.Serialization
{
    public static class BinarySerializer
    {
        public static byte[] ObjectToBytes(object obj)
        {
            if (CanSerialize(obj))
            {
                using (MemoryStream stream = new MemoryStream())
                {
                    BinaryFormatter formatter = new BinaryFormatter();
                    formatter.Serialize(stream, obj);

                    return stream.ToArray();
                }
            }
            else
            {
                UnityEngine.Debug.LogWarning("Cannot serialize object " + obj.GetType() + " " + obj);
            }

            throw new InvalidOperationException();
        }
        public static object BytesToObject(byte[] array)
        {
            using (MemoryStream stream = new MemoryStream(array))
            {
                BinaryFormatter formatter = new BinaryFormatter();
                return formatter.Deserialize(stream);
            }
        }
        public static bool CanSerialize(object obj)
        {
            return (obj.GetType().Attributes & TypeAttributes.Serializable) == TypeAttributes.Serializable;
        }
    }
}
