﻿using UMS.Core;
using UMS.Serialization;
using UnityEngine;
using UnityEditor;

namespace UMS.Editor
{
    [CustomEditor(typeof(ModPackage))]
    public class ModPackageEditor : UnityEditor.Editor
    {
        protected const float SERIALIZE_BUTTON_WIDTH = 200;

        protected ModPackage Target { get { return (ModPackage)target; } }

        protected ModPackageReorderableList _objectEntryList;

        protected virtual void OnEnable()
        {
            _objectEntryList = CreateList("_objectEntries");
        }
        public override void OnInspectorGUI()
        {
            GUI.changed = false;

            EditorGUILayout.Space();
            _objectEntryList.Draw();
            EditorGUILayout.Space();

            EditorGUILayout.Space();
            DrawSerializeButton();
            EditorGUILayout.Space();

            if (GUI.changed)
                EditorUtility.SetDirty(target);
        }
        protected virtual void DrawSerializeButton()
        {
            GUIContent buttonText = new GUIContent("Serialize", "Serialize this package. To serialize all packages use Modding/Serialize All");
            GUIStyle buttonStyle = EditorStyles.largeLabel;

            Rect rect = GUILayoutUtility.GetRect(buttonText, buttonStyle);
            rect.x = (rect.width - SERIALIZE_BUTTON_WIDTH) / 2;
            rect.width = SERIALIZE_BUTTON_WIDTH;

            if (GUI.Button(rect, buttonText))
            {
                Serializer.SerializePackage(Target, System.Environment.GetFolderPath(System.Environment.SpecialFolder.Desktop));
            }
        }
        protected virtual ModPackageReorderableList CreateList(string propertyName)
        {
            return new ModPackageReorderableList(Target, serializedObject, serializedObject.FindProperty(propertyName));
        }
    }
}