using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using UMS.Serialization;

namespace UMS.Core
{
    public static class Utility
    {
        public const int MENU_ITEM_PRIORITY = 100;

        public const string MENU_ITEM_ROOT = "Modding";
        public const string MENU_SERIALIZATION = "Serialization";
        
        public static string GetObjectMemberName(string objName, string memberName)
        {
            return string.Format("{0}.{1}", objName, memberName);
        }
        public static bool ContainsAttribute(MemberInfo member, Type type)
        {
            return member.GetCustomAttributes(false).Any(x => type.IsAssignableFrom(x.GetType()));
        }
        public static bool IsPrefab(UnityEngine.GameObject obj)
        {
            return UnityEditor.PrefabUtility.GetPrefabParent(obj) == null && UnityEditor.PrefabUtility.GetPrefabObject(obj) != null;
        }
        public static string SanitizeExtension(string extension)
        {
            return extension.TrimStart('.');
        }
        public static List<Member> GetMembers(object obj)
        {
            List<Member> members = new List<Member>();

            foreach (FieldInfo field in obj.GetType().GetFields(BindingFlags.Public | BindingFlags.Static | BindingFlags.Instance))
            {
                if (Serializer.IsBlocked(GetObjectMemberName(field.DeclaringType.Name, field.Name)))
                    continue;

                if (!CanAccessMember(field))
                    continue;
                
                try
                {
                    members.Add(new Member(field.Name, field.GetValue(obj), false));
                }
                catch (Exception)
                {
                    UnityEngine.Debug.LogError("Issue getting value of " + GetObjectMemberName(field.DeclaringType.Name, field.Name));
                    throw;
                }
            }

            foreach (PropertyInfo property in obj.GetType().GetProperties(BindingFlags.Public | BindingFlags.Static | BindingFlags.Instance))
            {
                if (Serializer.IsBlocked(GetObjectMemberName(property.DeclaringType.Name, property.Name)))
                    continue;
                                
                MethodInfo method = property.GetGetMethod();
                
                if (method == null)
                    continue;

                if (!CanAccessMember(property))
                    continue;

                try
                {
                    members.Add(new Member(property.Name, property.GetValue(obj, null), true));
                }
                catch (Exception)
                {
                    UnityEngine.Debug.LogError("Issue getting value of " + GetObjectMemberName(property.DeclaringType.Name, property.Name));
                    throw;
                }             
            }

            return members.Where(x => x._value != null).ToList();
        }
        private static bool CanAccessMember(MemberInfo member)
        {
            return !member.GetCustomAttributes(false).Any(x => x.GetType() == typeof(ObsoleteAttribute));
        }
        public static T[] Copy<T>(Array from)
        {
            T[] array = new T[from.Length];
            Type arrayElementType = from.GetType().GetElementType();

            for (int i = 0; i < from.Length; i++)
            {
                array[i] = (T)ConvertObject(from.GetValue(i), arrayElementType, typeof(T));                
            }

            return array;
        }
        public static object ConvertObject(object obj, Type fromType, Type toType)
        {
            if (fromType == null)
            {
                try
                {
                    return System.Convert.ChangeType(obj, toType);
                }
                catch (Exception)
                {
                    return null;
                }
            }
            else if (toType.IsArray)
            {
                Array objd = ConvertAsArray(obj, fromType, toType.GetElementType());
                
                return objd;
            }
            else
            {
                return Convert(obj, fromType, toType);
            }            
        }
        private static Array ConvertAsArray(object obj, Type fromType, Type toType)
        {            
            if(obj is Array)
            {
                return ((object[])obj).Select(x => Convert(x, fromType, toType)).ToArray();
            }
            else if(obj is JArray)
            {
                object[] objArray = ((JArray)obj).Select(x => Convert(x, fromType, toType)).ToArray();
                Array array = Array.CreateInstance(toType, objArray.Length);

                for (int i = 0; i < objArray.Length; i++)
                {
                    array.SetValue(objArray[i], i);
                }
                
                return array;
            }
            else
            {
                throw new NotImplementedException("Array type " + obj.GetType() + " not recognized");
            }
        }
        private static object Convert(object obj, Type fromType, Type toType)
        {
            object toReturn;

            if (CanConvert(obj, fromType, toType, out toReturn))
            {
                return toReturn;
            }

            if (toType != null)
                throw new System.InvalidCastException("Cannot cast from " + fromType + " to " + toType);
            else
                throw new NullReferenceException("ToType is null");
        }
        private static bool CanConvert(object obj, Type fromType, Type toType, out object returnObject)
        {
            returnObject = null;

            foreach (MethodInfo info in fromType.GetMethods().Union(toType.GetMethods()))
            {
                ParameterInfo[] parameters = info.GetParameters();

                if (parameters.Length != 1)
                    continue;
                
                if (info.Name == "op_Implicit" && info.ReturnType == toType)
                {
                    if (obj.GetType() == typeof(JObject))
                    {
                        JObject jobj = obj as JObject;

                        if (parameters[0].GetType().IsAssignableFrom(fromType))
                            continue;

                        returnObject = info.Invoke(null, new object[1] { jobj.ToObject(fromType) });
                        return true;
                    }
                    else
                    {
                        if (parameters[0].GetType().IsAssignableFrom(obj.GetType()))
                            continue;

                        returnObject = info.Invoke(null, new object[1] { obj });
                        return true;
                    }
                }
            }
            
            return false;
        }
    }
}
