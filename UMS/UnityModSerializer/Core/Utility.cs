﻿using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text.RegularExpressions;
using UnityEngine;
using UMS.Behaviour;

namespace UMS.Core
{
    public static class Utility
    {
        /// <summary>
        /// Name used for the manifest file
        /// </summary>
        public const string MANIFEST_NAME = "manifest";

        private static Regex _endNumberParanthesis = new Regex(@"\(\d+\)$");

        public static Type GetCommonBaseClass(IEnumerable enumerable)
        {
            List<Type> types = new List<Type>();

            foreach (object obj in enumerable)
            {
                types.Add(obj.GetType());
            }

            return GetCommonBaseClassUsingParams(types.ToArray());
        }
        public static Type GetCommonBaseClassUsingParams(params Type[] types)
        {
            if (types.Length == 0)
                return typeof(object);

            Type ret = types[0];

            for (int i = 1; i < types.Length; ++i)
            {
                if (types[i].IsAssignableFrom(ret))
                    ret = types[i];
                else
                {
                    // This will always terminate when ret == typeof(object)
                    while (!ret.IsAssignableFrom(types[i]))
                        ret = ret.BaseType;
                }
            }

            return ret;
        }
        public static byte[] EncodeToPNG(Texture2D texture)
        {
            // Create a temporary RenderTexture of the same size as the texture
            RenderTexture temporaryTexture = RenderTexture.GetTemporary(
                                texture.width,
                                texture.height,
                                0,
                                RenderTextureFormat.Default,
                                RenderTextureReadWrite.Linear);

            Graphics.Blit(texture, temporaryTexture);

            // Backup the currently set RenderTexture
            RenderTexture previous = RenderTexture.active;

            RenderTexture.active = temporaryTexture;

            Texture2D toReturn = new Texture2D(texture.width, texture.height);

            toReturn.ReadPixels(new Rect(0, 0, temporaryTexture.width, temporaryTexture.height), 0, 0);
            toReturn.Apply();

            // Reset the active RenderTexture
            RenderTexture.active = previous;
            // Release the temporary RenderTexture
            RenderTexture.ReleaseTemporary(temporaryTexture);

            return toReturn.EncodeToPNG();
        }
        public static string ToString(byte[] array)
        {
            using (MemoryStream stream = new MemoryStream())
            {
                stream.Write(array, 0, array.Length);

                stream.Position = 0;
                StreamReader reader = new StreamReader(stream);

                return reader.ReadToEnd();
            }
        }
        public static string GetGameObjectFolderName(GameObject gameObject)
        {
            string toReturn = gameObject.name;

            Transform currentParent = gameObject.transform.parent;

            while (currentParent != null)
            {
                toReturn = currentParent.gameObject.name + "/" + toReturn;

                currentParent = currentParent.parent;
            }

            return toReturn;
        }
        public static bool IsNull(object obj)
        {
            if (obj == null)
                return true;

            if (obj.ToString() == "null")
                return true;

            return !CustomValidityComparers.IsValid(obj);
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

            if (mostInherited.Count() > 1)
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
                if (_endNumberParanthesis.IsMatch(preferredName))
                {
                    Match match = _endNumberParanthesis.Match(preferredName);

                    int index = 0;
                    string strippedString = preferredName.Replace(match.Value, string.Empty);

                    foreach (Capture capture in match.Captures)
                    {
                        if (int.TryParse(capture.Value.Trim(')', '('), out int value))
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