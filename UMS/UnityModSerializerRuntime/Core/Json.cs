using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Newtonsoft.Json.Serialization;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using UMS.Runtime.Deserialization;

namespace UMS.Runtime.Core
{
    public class Json
    {
        private static int errorIndex;
        private static JsonSerializerSettings SerializeSettings
        {
            get
            {
                if (_serializeSettings == null)
                    CreateSerializeSettings();

                return _serializeSettings;
            }
        }
        private static JsonSerializerSettings _serializeSettings;

        private static JsonSerializerSettings DeserializeSettings
        {
            get
            {
                if (_deserializeSettings == null)
                    CreateDeserializeSettings();

                return _deserializeSettings;
            }
        }
        private static JsonSerializerSettings _deserializeSettings;

        ///------------------------------SERIALIZE SETTINGS------------------------------
        private static void CreateSerializeSettings()
        {
            _serializeSettings = new JsonSerializerSettings();

            _serializeSettings.ReferenceLoopHandling = ReferenceLoopHandling.Ignore;
            _serializeSettings.NullValueHandling = NullValueHandling.Ignore;
            _serializeSettings.TypeNameHandling = TypeNameHandling.All;
            _serializeSettings.TypeNameAssemblyFormatHandling = TypeNameAssemblyFormatHandling.Simple;
            _serializeSettings.TraceWriter = new Debugging.EG_TraceLogger(true);
            _serializeSettings.ContractResolver = new OnlyFieldsContractResolver();
        }
        ///------------------------------DESERIALIZE SETTINGS------------------------------
        private static void CreateDeserializeSettings()
        {
            _deserializeSettings = new JsonSerializerSettings();

            _deserializeSettings.NullValueHandling = NullValueHandling.Ignore;
            _deserializeSettings.TypeNameHandling = TypeNameHandling.All;
            _deserializeSettings.TypeNameAssemblyFormatHandling = TypeNameAssemblyFormatHandling.Simple;
            _deserializeSettings.TraceWriter = new Debugging.EG_TraceLogger(true);
            _deserializeSettings.ConstructorHandling = ConstructorHandling.AllowNonPublicDefaultConstructor;
            _deserializeSettings.Converters = GetDeserializationConverters();
        }
        private static IList<Newtonsoft.Json.JsonConverter> GetDeserializationConverters()
        {
            IList<Newtonsoft.Json.JsonConverter> converters = Deserializer.GetConverters();

            converters.Add(new NumberConverter());

            return converters;
        }
        public static string ToJson(object obj)
        {
            return JsonConvert.SerializeObject(obj, Formatting.Indented, SerializeSettings);
        }
        public static object ToObject(string json)
        {
            JObject jobject = ToObject<JObject>(json);
            Type type = jobject["$type"].ToObject<Type>();

            return ToObject(json, type);
        }
        public static T ToObject<T>(string json)
        {
            return (T)ToObject(json, typeof(T));
        }
        public static object ToObject(string json, Type type)
        {
            try
            {
                return JsonConvert.DeserializeObject(json, type, DeserializeSettings);
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
    public class NumberConverter : Newtonsoft.Json.JsonConverter
    {
        public override bool CanConvert(Type objectType)
        {
            return objectType == typeof(int) || objectType == typeof(long) || objectType == typeof(uint) || objectType == typeof(ulong)
                || objectType == typeof(float) || objectType == typeof(double) || objectType.IsEnum;
        }

        public override object ReadJson(JsonReader reader, Type objectType, object existingValue, Newtonsoft.Json.JsonSerializer serializer)
        {
            if (reader.TokenType == JsonToken.Integer)
            {
                string stringValue = reader.Value.ToString();
                int intValue;
                uint uIntValue;
                long longValue;
                ulong ulongValue;

                if (int.TryParse(stringValue, out intValue))
                {
                    return intValue;
                }
                else if (uint.TryParse(stringValue, out uIntValue))
                {
                    return uIntValue;
                }
                else if (long.TryParse(stringValue, out longValue))
                {
                    return longValue;
                }
                else if (ulong.TryParse(stringValue, out ulongValue))
                {
                    return ulongValue;
                }
                else
                {
                    UnityEngine.Debug.LogWarning("Couldn't convert " + stringValue + " to integer");
                }
            }
            else if (reader.TokenType == JsonToken.Float)
            {
                string stringValue = reader.Value.ToString();

                float floatValue = -1;
                double doubleValue = -1;

                if (float.TryParse(stringValue, out floatValue))
                {
                    return floatValue;
                }
                else if (double.TryParse(stringValue, out doubleValue))
                {
                    return doubleValue;
                }
            }

            UnityEngine.Debug.LogWarning("Couldn't convert " + objectType);
            return null;
        }

        public override bool CanWrite => false;
        public override void WriteJson(JsonWriter writer, object value, Newtonsoft.Json.JsonSerializer serializer)
        {
            throw new NotImplementedException();
        }
    }
    public class OnlyFieldsContractResolver : DefaultContractResolver
    {
        /// <summary>
        /// Defines attributes that allow private fields to be serialized
        /// </summary>
        private static HashSet<Type> PrivateFieldOverrideAttributes = new HashSet<Type>()
        {
            typeof(UnityEngine.SerializeField),
            typeof(JsonPropertyAttribute),
        };

        /// <summary>
        /// Defines attributes that forces a field not to be serialized
        /// </summary>
        private static HashSet<Type> FieldSerializationBlockerAttributes = new HashSet<Type>()
        {
            typeof(JsonIgnoreAttribute),
            typeof(NonSerializedAttribute)
        };

        protected override IList<JsonProperty> CreateProperties(Type type, MemberSerialization memberSerialization)
        {
            List<JsonProperty> properties = new List<JsonProperty>();
            Type toCheck = type;

            while (toCheck != null && toCheck != typeof(object))
            {
                properties.AddRange(GetFields(toCheck).Select(x => base.CreateProperty(x, memberSerialization)));

                toCheck = toCheck.BaseType;
            }

            properties.ForEach(x =>
            {
                x.Writable = true;
                x.Readable = true;
            });

            return properties;
        }
        protected static IEnumerable<FieldInfo> GetFields(Type type)
        {
            return type.GetFields(BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.DeclaredOnly).Where(x => IsSerializable(x));
        }
        protected static bool IsSerializable(FieldInfo field)
        {
            if (IsBlocked(field))
                return false;

            if (!field.IsPublic)
            {
                if (!ContainsPrivateSerializerField(field))
                    return false;
            }

            return true;
        }
        protected static bool ContainsPrivateSerializerField(FieldInfo field)
        {
            return field.GetCustomAttributes(false).Any(x => PrivateFieldOverrideAttributes.Contains(x.GetType()));
        }
        protected static bool IsBlocked(FieldInfo field)
        {
            return field.GetCustomAttributes(false).Any(x => FieldSerializationBlockerAttributes.Contains(x.GetType()));
        }
    }
}
