using System;
using UnityEditor;
using UnityEngine;
using WanFramework.Utils;

namespace WanFramework.Editor.Utils
{
    [CustomPropertyDrawer(typeof(EnumStringAttribute), true)]
    public class EnumStringDrawer : PropertyDrawer
    {
        private bool _isTextInputMode = false;
        
        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            // 多选时禁用
            if (property.serializedObject.isEditingMultipleObjects)
            {
                EditorGUI.LabelField(position, label.text, "老大，多选时编辑这一项会出问题！");
                return;
            }
            var enumStringAttr = attribute as EnumStringAttribute;
            if (enumStringAttr == null) return;
            if (property.propertyType == SerializedPropertyType.String)
            {
                position.width -= 50;
                if (_isTextInputMode)
                {
                    property.stringValue = EditorGUI.TextField(position, label, property.stringValue);
                }
                else
                {
                    if (!Enum.TryParse(enumStringAttr.EnumType, property.stringValue, out var current))
                        current = Enum.GetValues(enumStringAttr.EnumType).GetValue(0);
                    current = EditorGUI.EnumPopup(position, label, current as Enum);
                    var newVal = Enum.GetName(enumStringAttr.EnumType, current);
                    property.stringValue = newVal;
                }

                var buttonRect = new Rect(position.x + position.width, position.y, 50, position.height);
                if (GUI.Button(buttonRect,  _isTextInputMode ? "枚举" : "文本"))
                    _isTextInputMode = !_isTextInputMode;
            }
            else
                EditorGUI.LabelField(position, label.text, "Use EnumStringAttribute with string.");
        }
    }
}