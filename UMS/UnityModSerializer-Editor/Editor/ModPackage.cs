using System;
using System.Collections.Generic;
using UnityEngine;

namespace UMS.Editor
{
    [Serializable]
    [CreateAssetMenu(fileName = "ModPackage.asset", menuName = "Modding/Package", order = EditorUtilities.MENU_ITEM_PRIORITY)]
    public class ModPackage : ScriptableObject
    {
        public IEnumerable<ObjectEntry> ObjectEntries { get { return _objectEntries; } }

#pragma warning disable
        [SerializeField]
        private List<ObjectEntry> _objectEntries;
#pragma warning restore

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