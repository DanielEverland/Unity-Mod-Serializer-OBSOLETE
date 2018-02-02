using UMS.Core;
using UnityEngine;

namespace UMS.Serialization
{
    [System.Serializable]
    public class Reference
    {
        public Reference(Object obj)
        {
            if(obj == null)
            {
                throw new System.NullReferenceException("Object cannot be null!");
            }

            _id = Mod.Current.GetID(obj);
        }
        
        public int _id;
        
        public static implicit operator Object(Reference reference)
        {
            throw new System.NotImplementedException();
        }
    }
}
