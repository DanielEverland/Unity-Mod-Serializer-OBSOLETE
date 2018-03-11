using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Newtonsoft.Json.Serialization;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using UMS.Runtime.Deserialization;
using UMS.Runtime.Core;

namespace UMS.Serialization
{
    public partial class JsonSerializer
    {
        private static JsonSerializerSettings Settings
        {
            get
            {
                if (_settings == null)
                    CreateSettings();

                return _settings;
            }
        }
        private static JsonSerializerSettings _settings;
        
        private static void CreateSettings()
        {
            _settings = new JsonSerializerSettings();

            _settings.ReferenceLoopHandling = ReferenceLoopHandling.Ignore;
            _settings.NullValueHandling = NullValueHandling.Ignore;
            _settings.TypeNameHandling = TypeNameHandling.All;
            _settings.TypeNameAssemblyFormatHandling = TypeNameAssemblyFormatHandling.Simple;
            _settings.TraceWriter = new Debugging.EG_TraceLogger(true);
            _settings.ContractResolver = new OnlyFieldsContractResolver();
        }
        public static string ToJson(object obj)
        {
            return JsonConvert.SerializeObject(obj, Formatting.Indented, Settings);
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