using System.Collections.Generic;
using UMS.Serialization;

namespace UMS.Core
{
    public static class Extensions
    {
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
            return JsonSerializer.ToJson(obj);
        }
        public static T ToObject<T>(this string json)
        {
            return JsonSerializer.ToObject<T>(json);
        }
        public static object ToObject(this string json)
        {
            return JsonSerializer.ToObject<object>(json);
        }
    }
}