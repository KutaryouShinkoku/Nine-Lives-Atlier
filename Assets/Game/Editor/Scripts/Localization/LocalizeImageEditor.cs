using System;
using Game.Localization;
using Game.Localization.Components;
using UnityEditor;
using UnityEngine;

namespace Game.Editor.Scripts.Localization
{
    [CustomEditor(typeof(LocalizeImage))]
    class LocalizeImageEditor : UnityEditor.Editor
    {
        private SerializedProperty _spritesProperty;
        private void OnEnable()
        {
            _spritesProperty = serializedObject.FindProperty("sprites");
        }
        public override void OnInspectorGUI()
        {
            var languageCount = Enum.GetValues(typeof(Language)).Length;
            if (_spritesProperty.arraySize != languageCount)
            {
                _spritesProperty.arraySize = languageCount;
                serializedObject.ApplyModifiedPropertiesWithoutUndo();
            }
            GUILayout.BeginVertical();
            GUILayout.Label("不同语言下的Sprite~");
            for (var i = 0; i < languageCount; i++)
            {
                var curElement = _spritesProperty.GetArrayElementAtIndex(i);
                curElement.objectReferenceValue = EditorGUILayout.ObjectField(((Language)i).ToString(), curElement.objectReferenceValue, typeof(Sprite),
                    false) as Sprite;
            }
            GUILayout.EndVertical();
            serializedObject.ApplyModifiedProperties();
            serializedObject.Update();
        }
    }
}