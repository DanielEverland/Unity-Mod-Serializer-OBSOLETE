using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace UMS.Core.Types
{
    [Serializable]
    public class Reference : Serializable<object, Reference>
    {
        public Reference() { }
        public Reference(object obj)
        {
            if (obj == null)
                throw new NullReferenceException("Object cannot be null");

            ID = ObjectManager.Add(obj);
        }

        public override Reference Serialize(object obj)
        {
            return new Reference(obj);
        }
    }
}
