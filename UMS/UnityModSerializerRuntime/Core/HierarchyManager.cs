using System;
using System.Collections.Generic;
using UnityEngine;

namespace UMS.Runtime.Core
{
    public static class HierarchyManager
    {
        private static HashSet<RectTransform> _executedTransforms;
        private static Dictionary<RectTransform, List<Action<RectTransform>>> _queuedActions;

        public static void Initialize()
        {
            _executedTransforms = new HashSet<RectTransform>();
            _queuedActions = new Dictionary<RectTransform, List<Action<RectTransform>>>();
        }
        public static void ExecuteTransform(Transform transform)
        {
            if (transform is RectTransform rectTransform)
            {
                ExecuteTransform(rectTransform);
            }
        }
        private static void ExecuteTransform(RectTransform transform)
        {
            if (!HasExecuted(transform))
                _executedTransforms.Add(transform);

            if (IsQueued(transform))
            {
                ExecuteQueue(transform);
            }
        }
        private static void ExecuteQueue(RectTransform transform)
        {
            foreach (Action<RectTransform> callback in _queuedActions[transform])
            {
                callback(transform);
            }

            _queuedActions.Remove(transform);
        }
        private static bool IsQueued(RectTransform transform)
        {
            return _queuedActions.ContainsKey(transform);
        }
        private static bool HasExecuted(RectTransform transform)
        {
            return _executedTransforms.Contains(transform);
        }
        private static void Queue(RectTransform transform, Action<RectTransform> callback)
        {
            if (IsQueued(transform))
            {
                _queuedActions[transform].Add(callback);
            }
            else
            {
                _queuedActions.Add(transform, new List<Action<RectTransform>>() { callback, });
            }
        }
        public static void Subscribe(RectTransform transform, Action<RectTransform> callback)
        {
            if (HasExecuted(transform))
            {
                callback(transform);
            }
            else
            {
                Queue(transform, callback);
            }
        }
    }
}