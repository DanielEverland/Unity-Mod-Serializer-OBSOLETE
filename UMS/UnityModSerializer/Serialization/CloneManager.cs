using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using UMS.Core;

namespace UMS.Serialization
{
    public class CloneManager
    {
        public CloneManager()
        {
            _clones = new List<Object>();

            CoreManager.OnSerializationStarted += () => _isSerializing = true;
            CoreManager.OnSerializationCompleted += () => _isSerializing = false;

            EditorApplication.update += ClearClones;
        }

        public static CloneManager Instance
        {
            get
            {
                if (_instance == null)
                    _instance = new CloneManager();

                return _instance;
            }
        }
        private static CloneManager _instance;

        private List<Object> _clones;
        private bool _isSerializing;

        public static Object GetClone(Object obj)
        {
            return Instance.Create(obj);
        }
        private Object Create(Object obj)
        {
            Object clone = Object.Instantiate(obj);
            clone.name = obj.name;

            _clones.Add(clone);

            return clone;
        }
        private void ClearClones()
        {
            if (_isSerializing)
                return;

            while (_clones.Count > 0)
            {
                Object.DestroyImmediate(_clones.GetAndRemove(0));
            }
        }
    }
}