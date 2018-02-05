using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Newtonsoft.Json.Serialization;
using UMS.Core;

namespace UMS.Serialization
{
    public class JsonSerializer
    {
        private static int errorIndex;
        private static JsonSerializerSettings serializeSettings = new JsonSerializerSettings()
        {
            ReferenceLoopHandling = ReferenceLoopHandling.Ignore,
            NullValueHandling = NullValueHandling.Ignore,
            TypeNameHandling = TypeNameHandling.All,
            TypeNameAssemblyFormatHandling = TypeNameAssemblyFormatHandling.Simple,
            TraceWriter = new Debugging.EG_TraceLogger(),
            MetadataPropertyHandling = MetadataPropertyHandling.Ignore,
            ContractResolver = new OnlyFieldsContractResolver(),
        };

        private static JsonSerializerSettings deserializeSettings = new JsonSerializerSettings()
        {
            NullValueHandling = NullValueHandling.Ignore,
            TypeNameHandling = TypeNameHandling.All,
            TypeNameAssemblyFormatHandling = TypeNameAssemblyFormatHandling.Simple,
            TraceWriter = new Debugging.EG_TraceLogger(),
        };

        public static string ToJson(object obj)
        {
            return JsonConvert.SerializeObject(obj, Formatting.Indented, serializeSettings);
        }
        public static object ToObject(string json)
        {
            JObject jobject = ToObject<JObject>(json);
            Type type = jobject["$type"].ToObject<Type>();

            object obj = jobject.ToObject(type);

            if(obj is IModSerializer serializer)
            {
                try
                {
                    return CustomSerializers.DeserializeObject(obj);
                }
                catch (Exception)
                {
                    UnityEngine.Debug.LogError("Issue while deserializing object. Putting JSON on desktop");

                    PasteJSONToDesktop(json);
                    throw;
                }                
            }
            
            return obj;
        }
        public static T ToObject<T>(string json)
        {
            return (T)ToObject(json, typeof(T));
        }
        public static object ToObject(string json, Type type)
        {
            try
            {
                return JsonConvert.DeserializeObject(json, type, deserializeSettings);
            }
            catch (Exception)
            {
                UnityEngine.Debug.Log("Couldn't deserialize object. Putting JSON in desktop");

                PasteJSONToDesktop(json);

                throw;
            }
        }
        private static void PasteJSONToDesktop(string json)
        {
            string directory = Environment.GetFolderPath(Environment.SpecialFolder.Desktop);
            string fileName = "/ERROR" + errorIndex++ + ".txt";
            string fullPath = directory + fileName;

            System.IO.File.WriteAllText(fullPath, json);
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
    public class OnlyFieldsContractResolver : DefaultContractResolver
    {
        protected override IList<JsonProperty> CreateProperties(Type type, MemberSerialization memberSerialization)
        {
            List<JsonProperty> properties = new List<JsonProperty>();
            Type toCheck = type;

            while (toCheck != null && toCheck != typeof(object))
            {
                properties.AddRange(toCheck.GetFields(BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.DeclaredOnly)
                .Select(x => base.CreateProperty(x, memberSerialization)));

                toCheck = toCheck.BaseType;
            }

            return properties;
        }
    }
}
