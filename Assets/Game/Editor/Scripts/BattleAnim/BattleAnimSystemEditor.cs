using System;
using System.CodeDom;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Threading;
using Cysharp.Threading.Tasks;
using Game.BattleAnim;
using UnityEditor;
using UnityEditorInternal;
using UnityEngine;

namespace Game.Editor.BattleAnim
{
    [CustomEditor(typeof(BattleAnimSystem))]
    public class BattleAnimSystemEditor : UnityEditor.Editor
    {
        private MethodInfo[] _queueMethods;
        private string[] _queueMethodNames;
        private int _selectedQueueMethod;

        private readonly List<object> _cachedMethodArgs = new();
        
        private void Awake()
        {
            _queueMethods = typeof(BattleAnimSystem).GetMethods().Where(m => m.Name.StartsWith("Queue")).ToArray();
            _queueMethodNames = _queueMethods.Select(m => m.Name.Replace("Queue", "")).ToArray();
            _selectedQueueMethod = -1;
        }
        public override void OnInspectorGUI()
        {
            base.OnInspectorGUI();
            if (!EditorApplication.isPlaying) return;
            DrawAnimQueueTool();
        }
        private void DrawAnimQueueTool()
        {
            GUILayout.Label("==动画队列小工具==");
            GUILayout.BeginVertical();
            var newSelection = EditorGUILayout.Popup("动画名", _selectedQueueMethod, _queueMethodNames);
            if (_selectedQueueMethod != newSelection)
            {
                _cachedMethodArgs.Clear();
                for (var i=0; i<_queueMethods[newSelection].GetParameters().Length; i++)
                    _cachedMethodArgs.Add(null);
            }
            _selectedQueueMethod = newSelection;
            if (_selectedQueueMethod != -1)
            {
                var method = _queueMethods[_selectedQueueMethod];
                var methodParams = method.GetParameters();
                for (var i = 0; i < methodParams.Length; i++)
                {
                    if (methodParams[i].ParameterType == typeof(float))
                        _cachedMethodArgs[i] = EditorGUILayout.FloatField(methodParams[i].Name,
                            _cachedMethodArgs[i] != null ? (float)_cachedMethodArgs[i] : 0.0f);
                    else if (methodParams[i].ParameterType == typeof(int))
                        _cachedMethodArgs[i] =
                            EditorGUILayout.IntField(methodParams[i].Name, _cachedMethodArgs[i] != null ? (int)_cachedMethodArgs[i] : 0);
                    else if (methodParams[i].ParameterType == typeof(bool))
                        _cachedMethodArgs[i] =
                            EditorGUILayout.Toggle(methodParams[i].Name, _cachedMethodArgs[i] != null && (bool)_cachedMethodArgs[i]);
                    else
                        GUILayout.Label($"{methodParams[i].Name} 参数暂不支持");
                }
                if (GUILayout.Button("压新动画"))
                    method.Invoke(target, _cachedMethodArgs.ToArray());
            }
            if (GUILayout.Button("播放动画队列"))
                ((BattleAnimSystem)target).PlayAllQueuedAnim(CancellationToken.None).Forget();
            GUILayout.EndHorizontal();
        }
    }
}