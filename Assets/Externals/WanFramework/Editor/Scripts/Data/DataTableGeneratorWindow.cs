//    ▄▀▀▀▄▄▄▄▄▄▄▀▀▀▄
//    █▒▒░░░░░░░░░▒▒█    DataTableGenerator.cs
//     █░░█░░░░░█░░█     Created by WanNeng
//  ▄▄  █░░░▀█▀░░░█  ▄▄  Created   01/01/2024 14:30
// █░░█ ▀▄░░░░░░░▄▀ █░░█

using System.CodeDom;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;
using WanFramework.Data;
using WanFramework.Editor.Utils;

namespace WanFramework.Editor.Data
{
    public class DataTableGeneratorWindow : EditorWindow
    {
        private TextField _tableNameText;
        private Button _createButton;
        private Label _pathLabel;
        
        private string _currentSelectedPath;

        private string CurrentSelectedPath
        {
            get => _currentSelectedPath;
            set
            {
                _currentSelectedPath = value;
                OnOutputPathChanged();
            }
        }

        [MenuItem("Assets/Create/WanFramework/DataTable")]
        public static void CreateDataTable()
        {
            var wnd = GetWindow<DataTableGeneratorWindow>();
            wnd.titleContent = new GUIContent("Data Table Create Window");
            if (Selection.assetGUIDs.Length == 0)
                wnd.CurrentSelectedPath = "Assets";
            else
                wnd.CurrentSelectedPath = AssetDatabase.GUIDToAssetPath(Selection.assetGUIDs[0]);
        }

        public void CreateGUI()
        {
            var monoScript = MonoScript.FromScriptableObject(this);
            var scriptPath = AssetDatabase.GetAssetPath(monoScript);
            var path = Path.GetDirectoryName(scriptPath);
            var debugWindowTree = AssetDatabase.LoadAssetAtPath<VisualTreeAsset>(Path.Combine(path ?? "", "DataTableGeneratorWindow.uxml"));
            debugWindowTree.CloneTree(rootVisualElement);
            _tableNameText = rootVisualElement.Q<TextField>("TableNameText");
            _createButton = rootVisualElement.Q<Button>("CreateButton");
            _pathLabel = rootVisualElement.Q<Label>("PathLabel");
            _createButton.clicked += OnCreateButtonClicked;
            _tableNameText.RegisterValueChangedCallback(OnTableNameChanged);
            _pathLabel.text = GetOutputInfo(_tableNameText.value);
        }

        private string GetAssetOutputPath(string tableName)
        {
            if (string.IsNullOrEmpty(tableName)) tableName = "NewDataTable";
            var outputPath = Path.Join(CurrentSelectedPath, $"{tableName}.xlsx");
            return outputPath;
        }
        
        private string GetCSharpOutputPath(string tableName)
        {
            if (string.IsNullOrEmpty(tableName)) tableName = "NewDataTable";
            var outputPath = Path.Join(CurrentSelectedPath, $"{tableName}.cs");
            return outputPath;
        }

        private string GetOutputInfo(string tableName)
        {
            return $"{GetAssetOutputPath(tableName)}\n{GetCSharpOutputPath(tableName)}";
        }
        private void OnOutputPathChanged()
        {
            _pathLabel.text = GetOutputInfo(_tableNameText.value);
        }
        private void OnTableNameChanged(ChangeEvent<string> evt)
        {
            _pathLabel.text = GetOutputInfo(evt.newValue);
        }
        private void OnCreateButtonClicked()
        {
            GenerateSource(_tableNameText.value);
        }

        private static bool ValidateTableName(string tableName)
        {
            return Regex.Match(tableName, "^[a-zA-Z_][a-zA-Z0-9_]*$").Success;
        }
        
        private void GenerateSource(string tableName)
        {
            if (!ValidateTableName(tableName))
            {
                EditorUtility.DisplayDialog("DataTableGenerator", $"Not a valid table name {tableName}", "ok");
                return;
            }
            var csharpPath = GetCSharpOutputPath(tableName);
            var outputPath = GetAssetOutputPath(tableName);
            if (File.Exists(csharpPath) || File.Exists(outputPath))
            {
                EditorUtility.DisplayDialog("DataTableGenerator", $"{csharpPath} or {outputPath} already exists", "ok");
                return;
            }
            // 创建excel表格
            var editorRoot = WanFrameworkEditorUtils.GetEditorRoot();
            var excelTemplatePath = Path.Combine(editorRoot, "Data", "_Template.xlsx");
            File.Copy(excelTemplatePath, outputPath);
            // 创建数据表代码
            AssetDatabase.ImportAsset(outputPath);
            var importer = AssetImporter.GetAtPath(outputPath);
            if (importer is not ExcelDataTableImporter dataTableImporter)
            {
                EditorUtility.DisplayDialog("DataTableGenerator",
                    $"Importer required to be {typeof(ExcelDataTableImporter)} but get {(importer == null ? "<NULL>" : importer.GetType())}", "ok");
                File.Delete(outputPath);
                File.Delete(csharpPath);
                AssetDatabase.Refresh();
                return;
            }
            File.Create(csharpPath).Dispose();
            AssetDatabase.ImportAsset(csharpPath);
            var monoScript = AssetDatabase.LoadAssetAtPath<MonoScript>(csharpPath);
            dataTableImporter.dataTableScript = monoScript;
            EditorUtility.SetDirty(dataTableImporter);
            dataTableImporter.SaveAndReimport();
        }
    }
}