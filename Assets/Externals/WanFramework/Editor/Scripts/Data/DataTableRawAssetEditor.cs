using System;
using System.Collections.Generic;
using System.Reflection;
using UnityEditor;
using UnityEngine;
using WanFramework.Data;

namespace WanFramework.Editor.Data
{
    [CustomEditor(typeof(DataTableRawAsset))]
    public class DataTableRawAssetEditor : UnityEditor.Editor
    {
        private IDataTable _dataTable;
        private DataTableRawAsset _dataTableRawAsset;
        public DataTableRawAsset DataTableRawAsset
        {
            get => _dataTableRawAsset;
            set
            {
                if (value != _dataTableRawAsset)
                    OnSetDataAsset(value);
                _dataTableRawAsset = value;
            }
        }

        private List<Action<int, IDataTable>> _filedGUI = new();
    
        void OnSetDataAsset(DataTableRawAsset rawAsset)
        {
            Type type = null;
            if (string.IsNullOrEmpty(rawAsset?.name)) throw new Exception("Failed to load table");
            foreach (var asm in AppDomain.CurrentDomain.GetAssemblies())
            {
                type = asm.GetType(rawAsset.GetTypeName());
                if (type != null) break;
            }
            if (type == null) throw new Exception("Failed to load table");
            if (Activator.CreateInstance(type) is not IDataTable instance) throw new Exception("Failed to load table");
            instance.LoadFrom(rawAsset);
            _dataTable = instance;
            _filedGUI.Clear();
            var entryType = type.GetNestedType("Entry");
            if (entryType == null) throw new Exception("Failed to find entry type");
            foreach (var property in entryType.GetProperties())
            {
                _filedGUI.Add((i, table) =>
                {
                    if (i < 0)
                        GUILayout.Label(property.Name, EditorStyles.boldLabel);
                    else
                        ShowProperty(property.GetValue(table.Get(i)));
                });
            }
        }

        private void ShowProperty(object obj)
        {
            if (obj.GetType().IsArray)
            {
                GUILayout.BeginVertical();
                if (((Array)obj).Length == 0)
                    GUILayout.Label("[EMPTY]");
                else
                    foreach (var subObj in (Array)obj)
                    {
                        ShowProperty(subObj);
                        GUILayout.FlexibleSpace();
                    }
                GUILayout.EndVertical();
            }
            else
            {
                var str = obj.ToString();
                if (string.IsNullOrEmpty(str)) str = "[EMPTY]";
                GUILayout.Label(str);
            }
        }
        
        public override void OnInspectorGUI()
        {
            DataTableRawAsset = target as DataTableRawAsset;
            if (_dataTable == null)
            {
                GUILayout.Label("No table selected");
                return;
            }
            GUILayout.BeginVertical();
            var width = EditorGUIUtility.currentViewWidth / _filedGUI.Count * 0.75f;
            for (var i = -1; i < _dataTable.Length; ++i)
            {
                GUILayout.BeginHorizontal(GUI.skin.box);
                foreach (var fieldGUI in _filedGUI)
                {
                    GUILayout.BeginVertical(GUILayout.Width(width));
                    fieldGUI(i, _dataTable);
                    GUILayout.EndHorizontal();
                    GUILayout.FlexibleSpace();
                }
                GUILayout.EndHorizontal();
            }
            GUILayout.EndVertical();
        }
    }
}
