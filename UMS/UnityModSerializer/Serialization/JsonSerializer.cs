using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using UMS.Core;

namespace UMS.Serialization
{
    public class JsonSerializer
    {
        private static JsonSerializerSettings settings = new JsonSerializerSettings()
        {
            ReferenceLoopHandling = ReferenceLoopHandling.Error,
            NullValueHandling = NullValueHandling.Ignore,
            TypeNameHandling = TypeNameHandling.All,
            TypeNameAssemblyFormatHandling = TypeNameAssemblyFormatHandling.Simple,
            TraceWriter = new Debugging.EG_TraceLogger(),
            MetadataPropertyHandling = MetadataPropertyHandling.Ignore,
        };

        public static string ToJson(object obj)
        {
            return JsonConvert.SerializeObject(obj, Formatting.Indented, settings);
        }
        public static object ToObject(string json)
        {
            JObject jobject = JsonConvert.DeserializeObject<JObject>(json);
            Type type = jobject["$type"].ToObject<Type>();

            object obj = jobject.ToObject(type);

            if(obj is IModSerializer serializer)
            {
                return CustomSerializers.DeserializeObject(obj);
            }

            return obj;
        }
        public static T ToObject<T>(string json)
        {
            return JsonConvert.DeserializeObject<T>(json, settings);
        }
        public static object ToObject(string json, Type type)
        {
            return JsonConvert.DeserializeObject(json, type, settings);
        }
        public static bool CanSerialize(Type type)
        {
            if (type == null)
                return false;

            return (type.Attributes & TypeAttributes.Serializable) == TypeAttributes.Serializable;
        }
        public static bool CanSerialize(object obj)
        {
            if (obj == null)
                return false;

            return (obj.GetType().Attributes & TypeAttributes.Serializable) == TypeAttributes.Serializable;
        }
    }
}
