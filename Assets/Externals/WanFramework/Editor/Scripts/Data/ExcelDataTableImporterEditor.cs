//    ▄▀▀▀▄▄▄▄▄▄▄▀▀▀▄
//    █▒▒░░░░░░░░░▒▒█    ExcelDataTableImporterEditor.cs
//     █░░█░░░░░█░░█     Created by WanNeng
//  ▄▄  █░░░▀█▀░░░█  ▄▄  Created   01/01/2024 21:19
// █░░█ ▀▄░░░░░░░▄▀ █░░█

using System;
using System.IO;
using UnityEditor;
using UnityEditor.AssetImporters;
using UnityEngine;
using UnityEngine.UIElements;
using WanFramework.Data;
using WanFramework.Editor.Utils;
using Object = UnityEngine.Object;

namespace WanFramework.Editor.Data
{
    [CustomEditor(typeof(ExcelDataTableImporter))]
    public class ExcelDataTableImporterEditor : ScriptedImporterEditor
    {
        private SerializedProperty _namespace;
        private SerializedProperty _script;

        private Texture2D _icon;
        private UnityEditor.Editor _tableEditor;
        
        private static Texture2D GetIcon()
        {
            var editorRoot = WanFrameworkEditorUtils.GetEditorRoot();
            var excelIconPath = Path.Combine(editorRoot, "Arts", "Icon", "ExcelIcon.png");
            return AssetDatabase.LoadAssetAtPath<Texture2D>(excelIconPath);
        }
        public override void OnEnable()
        {
            base.OnEnable();
            _icon = GetIcon();
            if (target != null)
                SetupSerializedProperty();
        }

        private void SetupSerializedProperty()
        {
            _namespace = serializedObject.FindProperty("namespace");
            _script = serializedObject.FindProperty("dataTableScript");
        }
        
        public override void OnInspectorGUI()
        {
            EditorGUILayout.PropertyField(_namespace);
            EditorGUILayout.HelpBox(
                "Generate code and write to the following script. All existed code will be override when the data table changed", 
                MessageType.Warning);
            EditorGUILayout.PropertyField(_script);
            serializedObject.ApplyModifiedProperties();
            GUILayout.Label("DataTable preview");
            ApplyRevertGUI();
        }
        
        public override Texture2D RenderStaticPreview(string assetPath, Object[] subAssets, int width, int height)
        {
            return _icon;
        }
    }
}