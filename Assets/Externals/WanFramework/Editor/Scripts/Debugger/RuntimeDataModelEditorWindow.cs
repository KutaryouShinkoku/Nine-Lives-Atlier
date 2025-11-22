//    ▄▀▀▀▄▄▄▄▄▄▄▀▀▀▄
//    █▒▒░░░░░░░░░▒▒█    DataModelEditor.cs
//     █░░█░░░░░█░░█     Created by WanNeng
//  ▄▄  █░░░▀█▀░░░█  ▄▄  Created   03/09/2024 14:20
// █░░█ ▀▄░░░░░░░▄▀ █░░█

using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using UnityEditor;
using UnityEngine;
using WanFramework.UI.DataComponent;

namespace WanFramework.Editor.Debugger
{
    public class RuntimeDataModelEditor : EditorWindow
    {
        private List<Type> _dataModelTypes = new();
        private string[] _dataModelNames;

        private int _currentIndex = 0;
        private DataModelBase _current;

        private Vector2 _scroll;
        
        [MenuItem("Window/WanFramework/RuntimeDataModelEditor")]
        [MenuItem("WanFramework/Window/RuntimeDataModelEditor")]
        private static void ShowWindow()
        {
            var wnd = GetWindow<RuntimeDataModelEditor>();
            wnd.titleContent = new GUIContent("数据模型修改器");
        }

        private void OnEnable()
        {
            var nameCache = new List<string>();
            _dataModelTypes.Clear();
            _dataModelTypes.Add(null);
            nameCache.Add("<NULL>");
            _currentIndex = 0;
            foreach (var asm in AppDomain.CurrentDomain.GetAssemblies())
                foreach (var type in asm.GetTypes())
                    if (typeof(DataModelBase).IsAssignableFrom(type) &&
                        !type.IsAbstract &&
                        !type.IsGenericType &&
                        type.GetCustomAttribute<ShowInDataModelEditorAttribute>() != null)
                    {
                        _dataModelTypes.Add(type);
                        nameCache.Add(type.Name);
                    }
            _dataModelNames = nameCache.ToArray();
        }

        public void OnGUI()
        {
            if (!Application.isPlaying)
            {
                EditorGUILayout.HelpBox("游戏跑起来才能改", MessageType.Warning);
                return;
            }
            var newIndex = EditorGUILayout.Popup(_currentIndex, _dataModelNames);
            if (newIndex != _currentIndex)
            {
                if (_dataModelTypes[newIndex] == null)
                    _current = null;
                else
                {
                    var instanceType = typeof(DataModel<>).MakeGenericType(_dataModelTypes[newIndex]);
                    _current = instanceType.GetProperty("Instance")?.GetValue(null) as DataModelBase;
                }
                _currentIndex = newIndex;
            }

            if (_current != null)
            {
                try
                {
                    _scroll = GUILayout.BeginScrollView(_scroll);
                    ShowObjectEditor(_current, out _);
                    GUILayout.EndScrollView();
                }
                catch (Exception e)
                {
                    Debug.LogError($"数据模型{_current}暂不支持编辑");
                    Debug.LogException(e);
                }
            }
            GUILayout.BeginHorizontal();
            if (GUILayout.Button("保存"))
                Save();
            if (GUILayout.Button("读取"))
                Load();
            GUILayout.EndHorizontal();
        }

        private void Save()
        {
            var savePath = EditorUtility.SaveFilePanel("存哪？", "", "Save", "json");
            if (string.IsNullOrEmpty(savePath)) return;
            var result = _current.Serialize(true);
            File.WriteAllText(savePath, result);
            Debug.Log("存了");
        }
        private void Load()
        {
            var loadPath = EditorUtility.OpenFilePanel("读哪？", "", "json");
            if (string.IsNullOrEmpty(loadPath)) return;
            _current.Deserialize(File.ReadAllText(loadPath));
            Debug.Log("读了");
        }
        private object ShowArrayEditor(Array obj, out bool isDirty)
        {
            isDirty = false;
            for (var i = 0; i < obj.Length; ++i)
            {
                GUILayout.BeginHorizontal();
                GUILayout.Label($"{i}: ");
                ShowValueEditor(obj.GetValue(i), out var isElemDirty);
                if (isElemDirty) isDirty = true;
                GUILayout.EndHorizontal();
            }

            return obj;
        }
        
        private object ShowListEditor(IList obj, out bool isDirty)
        {
            isDirty = false;
            var removedIndex = -1;
            var insertIndex = -1;
            var elementType = obj.GetType().GetGenericArguments().FirstOrDefault();
            GUILayout.BeginVertical();
            for (var i = 0; i < obj.Count; ++i)
            {
                GUILayout.BeginHorizontal();
                GUILayout.Label($"{i}: ");
                var newVal = ShowValueEditor(obj[i], out var isElemDirty);
                if (isElemDirty)
                {
                    if (!Equals(obj[i], newVal))
                        obj[i] = newVal;
                    isDirty = true;
                }
                if (GUILayout.Button("Insert", GUILayout.Width(50)))
                    insertIndex = i;
                if (GUILayout.Button("Remove", GUILayout.Width(50)))
                    removedIndex = i;
                GUILayout.EndHorizontal();
            }
            if (GUILayout.Button("Insert", GUILayout.Width(50)))
                insertIndex = obj.Count;
            GUILayout.EndVertical();
            if (removedIndex != -1)
                obj.RemoveAt(removedIndex);
            if (insertIndex != -1 && elementType != null)
                obj.Insert(insertIndex, Activator.CreateInstance(elementType));

            return obj;
        }

        private object ShowValueEditor(object val, out bool isDirty)
        {
            isDirty = false;
            GUILayout.BeginVertical();
            var newVal = val switch
            {
                int v => EditorGUILayout.IntField(v),
                uint v => EditorGUILayout.IntField((int)v),
                short v => EditorGUILayout.IntField(v),
                ushort v => EditorGUILayout.IntField(v),
                long v => EditorGUILayout.IntField((int)v),
                ulong v => EditorGUILayout.IntField((int)v),
                float v => EditorGUILayout.FloatField(v),
                double v => EditorGUILayout.FloatField((float)v),
                decimal v => EditorGUILayout.FloatField((float)v),
                string v => EditorGUILayout.TextField(v),
                bool v => EditorGUILayout.Toggle(v),
                Enum v => EditorGUILayout.EnumPopup(v),
                Array v => ShowArrayEditor(v, out isDirty),
                IList v => ShowListEditor(v, out isDirty),
                UnityEngine.Object v => EditorGUILayout.ObjectField(v, v.GetType(), true),
                _ => ShowObjectEditor(val, out isDirty)
            };
            GUILayout.EndVertical();
            if (!isDirty) isDirty = !newVal.Equals(val);
            return newVal;
        }
        private object ShowObjectEditor(object obj, out bool isDirty)
        {
            isDirty = false;
            foreach (var property in obj.GetType().GetProperties())
            {
                GUILayout.BeginHorizontal();
                GUILayout.Label(property.Name);
                GUILayout.BeginVertical();
                var val = property.GetValue(obj);
                var newVal = ShowValueEditor(val, out var isFieldDirty);
                if (isFieldDirty && property.SetMethod != null)
                {
                    property.SetValue(obj, newVal);
                    isDirty = true;
                }
                GUILayout.EndVertical();
                GUILayout.EndHorizontal();
            }

            return obj;
        }
    }
}