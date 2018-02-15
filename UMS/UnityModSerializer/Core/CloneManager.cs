using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

namespace UMS.Core
{
    public static class CloneManager
    {
        private static List<Object> _clones = new List<Object>();

        public static Object GetClone(Object obj)
        {
            EnsureCallbackExists();

            Object clone = Object.Instantiate(obj);
            clone.name = obj.name;


            _clones.Add(clone);

            return clone;
        }
        private static void ClearClones()
        {
            while (_clones.Count > 0)
            {
                Object.DestroyImmediate(_clones.GetAndRemove(0));
            }
        }
        private static void EnsureCallbackExists()
        {
            //Bit of a hack. First line is ignored if ClearClones is not in delegate
            CoreManager.OnSerializationCompleted -= ClearClones;
            CoreManager.OnSerializationCompleted += ClearClones;
        }
    }
}
