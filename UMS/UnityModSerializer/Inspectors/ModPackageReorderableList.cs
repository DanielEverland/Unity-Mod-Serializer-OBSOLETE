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

            entry.Object = EditorGUI.ObjectField(GetElementRect(rect), GUIContent.none, entry.Object, typeof(UnityEngine.Object), false);

            rect.y += EditorGUIUtility.singleLineHeight;
        }
        protected virtual Rect GetElementRect(Rect source)
        {
            return new Rect(source.x, source.y + 2, source.width, EditorGUIUtility.singleLineHeight);
        }
    }
}
