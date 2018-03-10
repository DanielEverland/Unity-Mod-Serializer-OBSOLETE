using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEditor;
using UnityEditorInternal;
using UnityEngine;
using UMS.Core;

namespace UMS.Inspectors
{
    public class ModPackageReorderableList : ReorderableList
    {
        public ModPackageReorderableList(ModPackage package, SerializedObject serializedObject, SerializedProperty serializedProperty)
            : base(serializedObject, serializedProperty, true, true, true, true)
        {
            Package = package;
            SerializedObject = serializedObject;
            SerializedProperty = serializedProperty;
            
            drawHeaderCallback = DrawHeader;
            drawElementCallback = DrawElement;
        }

        public ModPackage Package { get; protected set; }
        public SerializedObject SerializedObject { get; protected set; }
        public SerializedProperty SerializedProperty { get; protected set; }

        protected const float SPACING = 5;

        public virtual void Draw()
        {
            SerializedObject.Update();

            DoLayoutList();

            SerializedObject.ApplyModifiedProperties();
        }
        protected virtual void DrawHeader(Rect rect)
        {
            EditorGUI.LabelField(rect, ObjectNames.NicifyVariableName(SerializedProperty.name));
        }
        protected virtual void DrawElement(Rect rect, int index, bool isActive, bool isFocused)
        {
            ModPackage.ObjectEntry entry = Package.ObjectEntries.ElementAt(index);

            #region Object & Key
            Rect element = GetElementRect(rect);
            Rect textRect = new Rect(element.x, element.y, element.width / 2 - SPACING / 2, element.height);
            Rect objectRect = new Rect(element.x + element.width / 2 + SPACING / 2, element.y, element.width / 2, element.height);

            GUIContent keyContent = new GUIContent("Key", "Used to reference the object during runtime");
            GUIContent objectContent = new GUIContent("Object");

            EditorGUIUtility.labelWidth = TextWidth(keyContent, EditorStyles.label) + 2;
            entry.Key = EditorGUI.TextField(textRect, keyContent, entry.Key);

            EditorGUIUtility.labelWidth = TextWidth(objectContent, EditorStyles.label) + 2;
            entry.Object = EditorGUI.ObjectField(objectRect, objectContent, entry.Object, typeof(UnityEngine.Object), false);
            #endregion

            rect.y += EditorGUIUtility.singleLineHeight;
        }
        protected virtual float TextWidth(GUIContent content, GUIStyle style)
        {
            return style.CalcSize(content).x;
        }
        protected virtual Rect GetElementRect(Rect source)
        {
            return new Rect(source.x, source.y + 2, source.width, EditorGUIUtility.singleLineHeight);
        }
    }
}
