using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Text.RegularExpressions;
using UMS.Serialization;
using static UMS.Serialization.CustomSerializers;

namespace UMS.Core
{
    public static class Utility
    {
        public const int MENU_ITEM_PRIORITY = 100;

        public const string MENU_ITEM_ROOT = "Modding";
        public const string MENU_SERIALIZATION = "Serialization";

        private static Regex EndNumberParanthesis = new Regex(@"\(\d+\)$");

        private static readonly Dictionary<Type, Func<object, object>> converters = new Dictionary<Type, Func<object, object>>()
        {
            { typeof(Int32), ConvertToInt32 },
        };

        /// <summary>
        /// Used to compensate for Json.NET deserializing certain values as a higher-bit version than what is expected.
        /// For instance, this will occur when deserializing Int32 values - they'll be deserialized as Int64 types
        /// </summary>
        public static object CheckForConversion(object fromValue, Type toType)
        {
            if (fromValue == null)
                return null;

            if (converters.ContainsKey(toType))
            {
                return converters[toType].Invoke(fromValue);
            }
            
            return fromValue;
        }
        private static object ConvertToInt32(object fromValue)
        {
            string stringValue = fromValue.ToString();
            int intValue = -1;

            if(int.TryParse(stringValue, out intValue))
            {
                return intValue;
            }

            return fromValue;
        }
        public static MemberInfo GetMember(Type type, string name)
        {
            Type toCheck = type;

            while (toCheck != null && toCheck != typeof(object))
            {
                foreach (MemberInfo info in type.GetMembers(BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.Public | BindingFlags.DeclaredOnly))
                {
                    if (info.Name == name)
                        return info;
                }

                toCheck = toCheck.BaseType;
            }

            return null;
        }
        public static string GetObjectMemberName(string objName, string memberName)
        {
            return string.Format("{0}.{1}", objName, memberName);
        }
        public static string SanitizeExtension(string extension)
        {
            return extension.TrimStart('.');
        }
        public static bool CanAccessMember(MemberInfo member)
        {
            return !member.GetCustomAttributes(false).Any(x => x.GetType() == typeof(ObsoleteAttribute));
        }
        public static T GetMostInherited<T>(IEnumerable<T> source)
        {
            int max = source.Max(x => GetInheritanceCount(x.GetType()));
            return source.First(x => GetInheritanceCount(x.GetType()) == max);
        }
        public static Type GetMostInherited(IEnumerable<Type> source)
        {
            if (source.Count() == 0)
                return null;

            int max = source.Max(x => GetInheritanceCount(x));
            return source.First(x => GetInheritanceCount(x) == max);
        }
        public static int GetInheritanceCount(Type type)
        {
            Type baseType = type.BaseType;
            int i = 0;

            while (baseType != null)
            {
                baseType = baseType.BaseType;
                i++;
            }

            return i;
        }
        public static int GetID(object obj)
        {
            return IDManager.GetID(obj);
        }
        public static string GetValidName(string preferredName, HashSet<string> blackList)
        {
            if (blackList.Contains(preferredName))
            {
                if (EndNumberParanthesis.IsMatch(preferredName))
                {
                    Match match = EndNumberParanthesis.Match(preferredName);

                    int index = 0;
                    string strippedString = preferredName.Replace(match.Value, string.Empty);


                    foreach (Capture capture in match.Captures)
                    {
                        int value = 0;
                        if (int.TryParse(capture.Value.Trim(')', '('), out value))
                        {
                            index = value;
                            break;
                        }
                    }

                    return GetValidName(strippedString + "(" + (index + 1) + ")", blackList);
                }
                else
                {
                    return GetValidName(preferredName + " (1)", blackList);
                }
            }

            return preferredName;
        }
    }
}
