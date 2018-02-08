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
        private Reference(object obj)
        {
            ID = ObjectManager.Add(obj);
        }

        public override Reference Serialize(object obj)
        {
            return new Reference(obj);
        }
        public static Reference Create(object obj)
        {
            if (obj == null)
                return null;

            return new Reference(obj);
        }
    }
}
