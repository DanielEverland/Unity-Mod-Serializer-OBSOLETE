using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text.RegularExpressions;
using UMS.Serialization;

namespace UMS.Core
{
    public static class Utility
    {
        public const int MENU_ITEM_PRIORITY = 100;

        public const string MENU_ITEM_ROOT = "Modding";
        public const string MENU_SERIALIZATION = "Serialization";

        private static Regex EndNumberParanthesis = new Regex(@"\(\d+\)$");

        public static bool IsNull(object obj)
        {
            if (obj == null)
                return true;

            if (obj.ToString() == "null")
                return true;

            return !Serialization.CustomValidityComparers.IsValid(obj);
        }
        public static MemberInfo GetMember(Type type, string name)
        {
            Type toCheck = type;

            while (toCheck != null && toCheck != typeof(object))
            {
                foreach (MemberInfo info in type.GetMembers(BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.Public))
                {
                    if (info.Name == name)
                        return info;
                }

                toCheck = toCheck.BaseType;
            }

            return null;
        }
        public static string GetObjectMemberName(MemberInfo info)
        {
            return GetObjectMemberName(info.DeclaringType.Name, info.Name);
        }
        private static string GetObjectMemberName(string objName, string memberName)
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
        public static IModSerializer GetMostInherited(IEnumerable<IModSerializer> source)
        {
            int max = source.Max(x => GetInheritanceCount(x.GetType()));
            IEnumerable<IModSerializer> mostInherited = source.Where(x => GetInheritanceCount(x.GetType()) == max);
            
            if(mostInherited.Count() > 1)
            {
                int nonSerializedTypeInheritance = mostInherited.Max(x => GetInheritanceCount(x.NonSerializableType));

                return mostInherited.First(x => GetInheritanceCount(x.NonSerializableType) == nonSerializedTypeInheritance);
            }
            else
            {
                return mostInherited.ElementAt(0);
            }
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
        public static string GetValidName(string preferredName, string extension, HashSet<string> blackList)
        {
            string fullName = string.Format("{0}.{1}", preferredName, extension);

            if (blackList.Contains(fullName))
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

                    return GetValidName(strippedString + "(" + (index + 1) + ")", extension, blackList);
                }
                else
                {
                    return GetValidName(preferredName + " (1)", extension, blackList);
                }
            }

            return fullName;
        }
    }
}