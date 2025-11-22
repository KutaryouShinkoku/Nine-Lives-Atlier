//    ▄▀▀▀▄▄▄▄▄▄▄▀▀▀▄
//    █▒▒░░░░░░░░░▒▒█    DataTableImporter.cs
//     █░░█░░░░░█░░█     Created by WanNeng
//  ▄▄  █░░░▀█▀░░░█  ▄▄  Created   12/24/2023 11:44
// █░░█ ▀▄░░░░░░░▄▀ █░░█

using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Linq;
using System.Reflection;
using ExcelDataReader;
using UnityEditor;
using UnityEditor.AssetImporters;
using UnityEditor.Compilation;
using UnityEngine;
using WanFramework.Data;

namespace WanFramework.Editor.Data
{
    [ScriptedImporter(1, "xlsx")]
    public class ExcelDataTableImporter : ScriptedImporter
    {
        public MonoScript dataTableScript;
        public string @namespace = "Game.Data";
        
        private void RegenerateScript(DataTableRaw tableRaw)
        {
            // Generate source code
            var sourceGenerator = new DataTableAssetSourceGenerator();
            var scriptCode = sourceGenerator.Generate(@namespace, ref tableRaw);
            if (scriptCode == dataTableScript.text)
                return;
            var scriptPath = AssetDatabase.GetAssetPath(dataTableScript);
            File.WriteAllText(scriptPath, scriptCode);
            AssetDatabase.ImportAsset(scriptPath);
            EditorUtility.RequestScriptReload();
        }

        private DataTableRawAsset LoadDataTableAsset(DataTableRaw tableRaw)
        {
            // Create data table asset
            var converter = new DataTableRawAssetConverter();
            var asset = converter.ToBinaryTableAsset(ref tableRaw);
            asset.SetTypeName($"{@namespace}.{Path.GetFileNameWithoutExtension(assetPath)}");
            return asset;
        }

        public override void OnImportAsset(AssetImportContext ctx)
        {
            var assetFileName = Path.GetFileName(assetPath);
            if (assetFileName.StartsWith("~$"))
                return;
            if (dataTableScript == null)
                return;
            var tableName = Path.GetFileNameWithoutExtension(assetPath);
            if (!tableName?.EndsWith("Table") ?? false)
                Debug.LogWarning($"表文件名推荐统一以Table为结尾，建议修改{assetPath}");
            // ctx.DependsOnArtifact(AssetDatabase.GetAssetPath(dataTableScript));
            // Load raw table
            var tableRaw = DataTableRaw.FromExcel(ctx.assetPath);
            if (!tableRaw.OnValid())
                throw new DataTableImportException(
                    "Require Id column in data table and name column should be \"string\"", ref tableRaw);
            
            var asset = LoadDataTableAsset(tableRaw);
            if (asset != null)
            {
                ctx.AddObjectToAsset("DataTable", asset);
                ctx.SetMainObject(asset);
            }
            RegenerateScript(tableRaw);
            //else ctx.DependsOnArtifact(AssetDatabase.GetAssetPath(dataTableScript));
        }
    }
}