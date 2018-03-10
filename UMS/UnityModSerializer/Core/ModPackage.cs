using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;
using UnityEditor;

namespace UMS.Core
{
    [Serializable]
    [CreateAssetMenu(fileName = "ModPackage.asset", menuName = "Modding/Package", order = Utility.MENU_ITEM_PRIORITY)]
    public class ModPackage : ScriptableObject
    {
        public IEnumerable<ObjectEntry> ObjectEntries { get { return _objectEntries; } }

        [SerializeField]
        private List<ObjectEntry> _objectEntries;

        [Serializable]
        public class ObjectEntry
        {
            public UnityEngine.Object Object { get { return _object; } set { _object = value; } }
            public string Key { get { return _key; } set { _key = value; } }

            [SerializeField]
            private string _key;
            [SerializeField]
            private UnityEngine.Object _object;
        }
    }
}
