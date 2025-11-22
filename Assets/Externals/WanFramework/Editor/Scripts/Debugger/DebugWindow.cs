//    ▄▀▀▀▄▄▄▄▄▄▄▀▀▀▄
//    █▒▒░░░░░░░░░▒▒█    DebugWindow.cs
//     █░░█░░░░░█░░█     Created by WanNeng
//  ▄▄  █░░░▀█▀░░░█  ▄▄  Created   12/24/2023 15:07
// █░░█ ▀▄░░░░░░░▄▀ █░░█

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using DynamicExpresso.Exceptions;
using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;

namespace WanFramework.Editor.Debugger
{
    public class DebugWindow : EditorWindow
    {
        private TextField _codeField;
        private TextField _codeHistoryField;
        
        private CodeRunner _codeRunner;

        private readonly List<string> _commandHistory = new();
        private int _historyIndex;
        
        [MenuItem("Window/WanFramework/DebugWindow")]
        [MenuItem("WanFramework/Window/DebugWindow")]
        private static void ShowWindow()
        {
            var wnd = GetWindow<DebugWindow>();
            wnd.titleContent = new GUIContent("Debug Window");
        }

        public void CreateGUI()
        {
            var monoScript = MonoScript.FromScriptableObject(this);
            var scriptPath = AssetDatabase.GetAssetPath(monoScript);
            var path = Path.GetDirectoryName(scriptPath);
            var debugWindowTree = AssetDatabase.LoadAssetAtPath<VisualTreeAsset>(Path.Combine(path ?? "", "DebugWindow.CodeRunner.uxml"));
            debugWindowTree.CloneTree(rootVisualElement);
            _codeField = rootVisualElement.Q<TextField>("CodeText");
            _codeHistoryField = rootVisualElement.Q<TextField>("CodeHistoryText");
            _codeField.RegisterCallback<KeyDownEvent>(OnCodeFieldKeyDown);
            _commandHistory.Clear();
            _historyIndex = 0;
        }

        
        private void OnCodeFieldHistoryUp()
        {
            _historyIndex += 1;
            if (_historyIndex > _commandHistory.Count) _historyIndex = _commandHistory.Count;
            OnCodeFieldHistoryChanged();
        }
        
        private void OnCodeFieldHistoryDown()
        {
            _historyIndex -= 1;
            if (_historyIndex < 0) _historyIndex = 0;
            OnCodeFieldHistoryChanged();
        }

        private void OnCodeFieldHistoryChanged()
        {
            if (_commandHistory.Count == 0)
                return;
            _codeField.value = _historyIndex == 0 ? "" : _commandHistory[_historyIndex - 1];
        }

        private void AddHistory(string str)
        {
            _commandHistory.Insert(0, str);
            _historyIndex = 0;
        }
        
        private void OnCodeFieldEnter()
        {
            _codeField.delegatesFocus = true;
            _codeField.Focus();
            _codeRunner ??= new();
            var command = _codeField.text;
            _codeField.value = "";
            _codeHistoryField.value = _codeHistoryField.text + $"\n> {command}";
            AddHistory(command);
            try
            {
                var result = _codeRunner.Run(command);
                _codeHistoryField.value = _codeHistoryField.text + "\n" + result;
                UnityEngine.Debug.Log(result);
            }
            catch (ParseException ex)
            {
                _codeHistoryField.value = _codeHistoryField.text + $"\n{ex.Message}";
                var formatCommand = command.Insert(ex.Position, "!!HERE!!");
                _codeHistoryField.value = _codeHistoryField.text + $"\n{formatCommand}";
                UnityEngine.Debug.LogWarning(ex);
            }
            catch (Exception ex)
            {
                _codeHistoryField.value = _codeHistoryField.text + $"\n{ex.Message}";
                UnityEngine.Debug.LogWarning(ex);
            }
        }
        
        private void OnCodeFieldKeyDown(KeyDownEvent e)
        {
            switch (e.keyCode)
            {
                case KeyCode.UpArrow:
                    OnCodeFieldHistoryUp();
                    break;
                case KeyCode.DownArrow:
                    OnCodeFieldHistoryDown();
                    break;
                case KeyCode.Return:
                    OnCodeFieldEnter();
                    break;
            }
        }
    }
}