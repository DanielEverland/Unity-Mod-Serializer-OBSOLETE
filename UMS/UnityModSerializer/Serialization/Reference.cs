using System.Collections.Generic;
using System.Linq;
using System.Text;
using UMS.Core;
using UnityEngine;
using static UMS.Serialization.CustomSerializers;

namespace UMS.Serialization
{
    [System.Serializable]
    public class Reference
    {
        public Reference(Object obj)
        {
            _id = Mod.Get(obj.GetInstanceID());         
        }

        public int _id;

        public static implicit operator Object(Reference reference)
        {
            throw new System.NotImplementedException();
        }
    }
}
