using System.Linq;
using System.Reflection;
using System.Collections.Generic;
using UnityEngine;
using UMS.Core;
using static UMS.Serialization.CustomSerializers;

namespace UMS.Serialization
{
    [System.Serializable]
    public class Reference
    {
        //public Reference()
        //{

        //}
        //public Reference(Object obj, System.Type type)
        //{
        //    if(obj == null)
        //    {
        //        throw new System.NullReferenceException("Object cannot be null!");
        //    }
        //    if (!(typeof(SerializableObject).IsAssignableFrom(type)))
        //    {
        //        throw new System.ArgumentException("Type must be assignable from SerializableObject. " + type + " is not.");
        //    }

        //    _id = Mod.Current.GetID(obj);
        //    _type = type;

        //    Mod.Current.AddDependency(_id, obj);
        //}
        
        //public int _id;
        //public System.Type _type;
        
        //public SerializableObject Serialize(Object obj)
        //{
        //    try
        //    {
        //        if(obj is Component comp)
        //        {
        //            return new SerializableComponent(comp);
        //        }
        //        else
        //        {
        //            return (SerializableObject)System.Activator.CreateInstance(_type, new object[1] { obj, });
        //        }                
        //    }
        //    catch (System.MissingMethodException)
        //    {
        //        Debug.LogError("Missing constructor with type " + obj.GetType() + " in " + _type);
        //        throw;
        //    }
        //    catch (System.Exception)
        //    {
        //        throw;
        //    }            
        //}
        //public static Reference Create<T>(Object obj) where T : SerializableObject
        //{
        //    return new Reference(obj, typeof(T));
        //}
        //public static Reference Create(Object obj)
        //{
        //    IEnumerable<ConstructorInfo> validConstructors = Serializer.SerializableConstructors
        //        .Where(x => IsValidConstructor(x, obj));

        //    if(validConstructors.Count() == 0)
        //    {
        //        throw new System.NotImplementedException("No constructor for " + obj);
        //    }
        //    else
        //    {
        //        ConstructorInfo constructor = GetMostInherited(validConstructors);
                
        //        return new Reference(obj, constructor.DeclaringType);
        //    }            
        //}
        //private static ConstructorInfo GetMostInherited(IEnumerable<ConstructorInfo> constructors)
        //{
        //    int max = constructors.Max(x => Utility.GetInheritanceCount(x.DeclaringType));

        //    return constructors.First(x => Utility.GetInheritanceCount(x.DeclaringType) == max);
        //}
        //private static bool IsValidConstructor(ConstructorInfo info, Object obj)
        //{
        //    if (!typeof(SerializableObject).IsAssignableFrom(info.DeclaringType))
        //        return false;

        //    ParameterInfo[] parameters = info.GetParameters();

        //    if (parameters.Length > 1 || parameters.Length == 0)
        //        return false;

        //    return parameters[0].ParameterType.IsAssignableFrom(obj.GetType());
        //}
    }
}
