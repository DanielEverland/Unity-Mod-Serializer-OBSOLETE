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
        //public static bool ContainsAttribute(MemberInfo member, Type type)
        //{
        //    return member.GetCustomAttributes(false).Any(x => type.IsAssignableFrom(x.GetType()));
        //}
        //public static bool IsPrefab(UnityEngine.GameObject obj)
        //{
        //    return UnityEditor.PrefabUtility.GetPrefabParent(obj) == null && UnityEditor.PrefabUtility.GetPrefabObject(obj) != null;
        //}
        public static string SanitizeExtension(string extension)
        {
            return extension.TrimStart('.');
        }
        //public static List<Member> GetMembers(object obj)
        //{
        //    List<Member> members = new List<Member>();

        //    foreach (FieldInfo field in obj.GetType().GetFields(BindingFlags.Public | BindingFlags.Static | BindingFlags.Instance))
        //    {
        //        if (Serializer.IsBlocked(GetObjectMemberName(field.DeclaringType.Name, field.Name)))
        //            continue;

        //        if (!CanAccessMember(field))
        //            continue;

        //        try
        //        {
        //            members.Add(new Member(field, field.GetValue(obj), false));
        //        }
        //        catch (Exception)
        //        {
        //            UnityEngine.Debug.LogError("Issue getting value of " + GetObjectMemberName(field.DeclaringType.Name, field.Name));
        //            throw;
        //        }
        //    }

        //    foreach (PropertyInfo property in obj.GetType().GetProperties(BindingFlags.Public | BindingFlags.Static | BindingFlags.Instance))
        //    {
        //        if (Serializer.IsBlocked(GetObjectMemberName(property.DeclaringType.Name, property.Name)))
        //            continue;

        //        MethodInfo method = property.GetGetMethod();

        //        if (method == null)
        //            continue;

        //        if (!CanAccessMember(property))
        //            continue;

        //        try
        //        {
        //            members.Add(new Member(property, property.GetValue(obj, null), true));
        //        }
        //        catch (Exception)
        //        {
        //            UnityEngine.Debug.LogError("Issue getting value of " + GetObjectMemberName(property.DeclaringType.Name, property.Name));
        //            throw;
        //        }             
        //    }

        //    return members.Where(x => x._value != null).ToList();
        //}
        public static bool CanAccessMember(MemberInfo member)
        {
            return !member.GetCustomAttributes(false).Any(x => x.GetType() == typeof(ObsoleteAttribute));
        }
        //public static T[] Copy<T>(Array from)
        //{
        //    T[] array = new T[from.Length];
        //    Type arrayElementType = from.GetType().GetElementType();

        //    for (int i = 0; i < from.Length; i++)
        //    {
        //        array[i] = (T)ConvertObject(from.GetValue(i), arrayElementType, typeof(T));                
        //    }

        //    return array;
        //}
        //public static object ConvertObject(object obj, Type fromType, Type toType)
        //{
        //    if(typeof(Reference).IsAssignableFrom(fromType))
        //    {
        //        JObject jObj = obj as JObject;
        //        Reference reference = jObj.ToObject<Reference>();

        //        UnityEngine.Debug.Log(Deserialization.Deserializer.Contains(reference._id));

        //        return null;
        //    }
        //    else if (fromType == null)
        //    {
        //        try
        //        {
        //            return System.Convert.ChangeType(obj, toType);
        //        }
        //        catch (Exception)
        //        {
        //            return null;
        //        }
        //    }
        //    else if (toType.IsEnum)
        //    {
        //        return Enum.Parse(toType, obj.ToString());
        //    }
        //    else if (toType.IsArray)
        //    {
        //        Array objd = ConvertAsArray(obj, fromType, toType.GetElementType());

        //        return objd;
        //    }
        //    else
        //    {
        //        return Convert(obj, fromType, toType);
        //    }            
        //}
        //private static Array ConvertAsArray(object obj, Type fromType, Type toType)
        //{            
        //    if(obj is Array)
        //    {
        //        return ((object[])obj).Select(x => Convert(x, fromType, toType)).ToArray();
        //    }
        //    else if(obj is JArray)
        //    {
        //        object[] objArray = ((JArray)obj).Select(x => Convert(x, fromType, toType)).ToArray();
        //        Array array = Array.CreateInstance(toType, objArray.Length);

        //        for (int i = 0; i < objArray.Length; i++)
        //        {
        //            array.SetValue(objArray[i], i);
        //        }

        //        return array;
        //    }
        //    else
        //    {
        //        throw new NotImplementedException("Array type " + obj.GetType() + " not recognized");
        //    }
        //}
        //private static object Convert(object obj, Type fromType, Type toType)
        //{
        //    object toReturn;

        //    if (CanConvert(obj, fromType, toType, out toReturn))
        //    {
        //        return toReturn;
        //    }

        //    if (toType != null)
        //        throw new System.InvalidCastException("Cannot cast from " + fromType + " to " + toType);
        //    else
        //        throw new NullReferenceException("ToType is null");
        //}
        //private static bool CanConvert(object obj, Type fromType, Type toType, out object returnObject)
        //{
        //    returnObject = null;

        //    foreach (MethodInfo info in fromType.GetMethods().Union(toType.GetMethods()))
        //    {
        //        ParameterInfo[] parameters = info.GetParameters();

        //        if (parameters.Length != 1)
        //            continue;

        //        if (info.Name == "op_Implicit" && info.ReturnType == toType)
        //        {
        //            if (obj.GetType() == typeof(JObject))
        //            {
        //                JObject jobj = obj as JObject;

        //                if (parameters[0].GetType().IsAssignableFrom(fromType))
        //                    continue;

        //                returnObject = info.Invoke(null, new object[1] { jobj.ToObject(fromType) });
        //                return true;
        //            }
        //            else
        //            {
        //                if (parameters[0].GetType().IsAssignableFrom(obj.GetType()))
        //                    continue;

        //                returnObject = info.Invoke(null, new object[1] { obj });
        //                return true;
        //            }
        //        }
        //    }

        //    return false;
        //}
        //public static bool CanSerialize(Type type)
        //{
        //    return (type.Attributes & TypeAttributes.Serializable) == TypeAttributes.Serializable;
        //}
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
