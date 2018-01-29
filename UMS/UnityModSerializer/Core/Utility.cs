using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;

namespace UMS.Core
{
    public static class Utility
    {
        public const int MENU_ITEM_PRIORITY = 100;

        public const string MENU_ITEM_ROOT = "Modding";
        public const string MENU_SERIALIZATION = "Serialization";

        public static string SanitizeExtension(string extension)
        {
            return extension.TrimStart('.');
        }
        public static List<Member> GetMembers(object obj)
        {
            List<Member> members = new List<Member>();

            foreach (FieldInfo field in obj.GetType().GetAllFields())
            {
                if (!CanAccessMember(field))
                    continue;

                members.Add(new Member(field.Name, field.GetValue(obj), false));
            }

            foreach (PropertyInfo property in obj.GetType().GetAllProperties())
            {
                MethodInfo method = property.GetGetMethod();

                if (method == null)
                    continue;

                if (!CanAccessMember(property))
                    continue;

                members.Add(new Member(property.Name, property.GetValue(obj, null), true));
            }

            return members;
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
            if(fromType == null)
            {
                try
                {
                    return Convert.ChangeType(obj, toType);
                }
                catch (Exception)
                {
                    return null;
                }                
            }

            object toReturn;

            if(CanConvert(obj, fromType, toType, out toReturn))
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
