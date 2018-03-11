using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using UMS.Runtime.Core;

namespace UMS.Runtime.Deserialization
{
    public class JsonDeserializer
    {
        private static int errorIndex;

        private static JsonSerializerSettings Settings
        {
            get
            {
                if (_settings == null)
                    CreateDeserializeSettings();

                return _settings;
            }
        }
        private static JsonSerializerSettings _settings;

        private static void CreateDeserializeSettings()
        {
            _settings = new JsonSerializerSettings();

            _settings.NullValueHandling = NullValueHandling.Ignore;
            _settings.TypeNameHandling = TypeNameHandling.All;
            _settings.TypeNameAssemblyFormatHandling = TypeNameAssemblyFormatHandling.Simple;
            _settings.TraceWriter = new Debugging.EG_TraceLogger(true);
            _settings.ConstructorHandling = ConstructorHandling.AllowNonPublicDefaultConstructor;
            _settings.Converters = GetDeserializationConverters();
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
                return JsonConvert.DeserializeObject(json, type, Settings);
            }
            catch (Exception)
            {
                UnityEngine.Debug.Log("Couldn't deserialize object. Putting JSON in desktop");

                PasteJSONToDesktop(json);

                throw;
            }
        }
        private static IList<JsonConverter> GetDeserializationConverters()
        {
            IList<JsonConverter> converters = Deserializer.GetConverters();

            converters.Add(new NumberConverter());

            return converters;
        }
        private static void PasteJSONToDesktop(string json)
        {
            string directory = Environment.GetFolderPath(Environment.SpecialFolder.Desktop);
            string fileName = "/ERROR" + errorIndex++ + ".txt";
            string fullPath = directory + fileName;

            System.IO.File.WriteAllText(fullPath, json);
        }
        public class NumberConverter : JsonConverter
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
    }
}
