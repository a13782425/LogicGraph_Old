using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEditor;
using UnityEngine;

namespace Logic.Editor
{
    [CustomPropertyDrawer(typeof(CustomFieldNameAttribute))]
    public sealed class CustomFieldNameDrawer : PropertyDrawer
    {
        private GUIContent _label = null;
        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            if (_label == null)
            {
                _label = new GUIContent((attribute as CustomFieldNameAttribute).Name);
            }

            EditorGUI.PropertyField(position, property, _label);
        }
    }
}
