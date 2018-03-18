using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.Serialization.Formatters.Binary;
using UMS.Deserialization;

namespace UMS.Core
{
    public static class Extensions
    {
        public static T ToObject<T>(this byte[] array)
        {
            MemoryStream memStream = new MemoryStream();
            BinaryFormatter binForm = new BinaryFormatter();

            memStream.Write(array, 0, array.Length);
            memStream.Seek(0, SeekOrigin.Begin);
            
            return (T)binForm.Deserialize(memStream);
        }
        public static byte[] ToByteArray(this object obj)
        {
            if (obj == null)
                return null;

            BinaryFormatter bf = new BinaryFormatter();
            using (MemoryStream ms = new MemoryStream())
            {
                bf.Serialize(ms, obj);
                return ms.ToArray();
            }

        }
        /// <summary>
        /// Sets the value of a key. If a key already exists, it is overridden.
        /// </summary>
        public static void Set<TKey, TValue>(this IDictionary<TKey, TValue> dictionary, TKey key, TValue value)
        {
            if (dictionary.ContainsKey(key))
            {
                dictionary[key] = value;
            }
            else
            {
                dictionary.Add(key, value);
            }
        }
        public static string CollectionToString<T>(this IEnumerable<T> collection)
        {
            string toReturn = "";

            if (collection == null)
                return toReturn;

            int length = collection.Count();

            for (int i = 0; i < length; i++)
            {
                toReturn += collection.ElementAt(i).ToString();

                if (i < length - 1)
                    toReturn += ", ";
            }

            return toReturn;
        }
        public static T GetAndRemove<T>(this IList<T> collection, int index)
        {
            T obj = collection[index];
            collection.Remove(obj);

            return obj;
        }
        public static void Output<T>(this IEnumerable<T> collection)
        {
            foreach (T obj in collection)
            {
                UnityEngine.Debug.Log(obj);
            }
        }
        public static string ToJson(this object obj)
        {
            return Json.ToJson(obj);
        }
        public static T ToObject<T>(this string json)
        {
            return Json.ToObject<T>(json);
        }
        public static object ToObject(this string json)
        {
            return Json.ToObject<object>(json);
        }
    }
}