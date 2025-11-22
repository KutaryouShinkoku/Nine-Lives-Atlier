//    ▄▀▀▀▄▄▄▄▄▄▄▀▀▀▄
//    █▒▒░░░░░░░░░▒▒█    GameEditor.cs
//     █░░█░░░░░█░░█     Created by WanNeng
//  ▄▄  █░░░▀█▀░░░█  ▄▄  Created   01/11/2024 13:47
// █░░█ ▀▄░░░░░░░▄▀ █░░█

using System;
using System.Linq;
using UnityEditor;
using UnityEditor.Graphs;
using UnityEngine;
using UnityEngine.Assertions.Must;
using WanFramework.Base;

namespace WanFramework.Editor.Base
{
    [CustomEditor(typeof(GameManager))]
    public class GameEditor : UnityEditor.Editor
    {
        private MonoBehaviour[] _systems;
        private UnityEditor.Editor[] _systemEditor;
        private bool[] _systemEditorFoldout;
        
        private void OnEnable()
        {
            RefreshEditor();
        }

        private void RefreshEditor()
        {
            _systems = ((GameManager)target).transform
                .GetComponentsInChildren<Transform>()
                .Select(t => t.GetComponent<ISystem>() as MonoBehaviour)
                .Where(s => s != null)
                .ToArray();
            _systemEditor = new UnityEditor.Editor[_systems.Length];
            _systemEditorFoldout = new bool[_systems.Length];
        }

        private void SetupSystems()
        {
            GameHelper.SetupSystems(target as GameManager);
            RefreshEditor();
        }
        public override void OnInspectorGUI()
        {
            base.OnInspectorGUI();
            if (GameEntryPoint.Instance == null)
                EditorGUILayout.HelpBox("A entry point behaviour should be add to game object", MessageType.Error);
            if (GUILayout.Button("Setup systems"))
                SetupSystems();

            for (var i = 0; i < _systems.Length; ++i)
            {
                var system = _systems[i];
                if (system == null) continue;
                CreateCachedEditor(system, null, ref _systemEditor[i]);
                EditorGUILayout.Space();
                EditorGUILayout.BeginVertical(Styles.graphBackground);
                _systemEditorFoldout[i] = EditorGUILayout.InspectorTitlebar(_systemEditorFoldout[i], _systemEditor[i]);
                EditorGUI.indentLevel += 2;
                if (_systemEditorFoldout[i])
                    _systemEditor[i].OnInspectorGUI();
                EditorGUI.indentLevel -= 2;
                EditorGUILayout.EndVertical();
            }
        }
        
    }

    internal static class GameHelper
    {
        public static void SetupSystems(GameManager gameManager)
        {
            var systemTypes =
                AppDomain.CurrentDomain.GetAssemblies()
                    .SelectMany(asm => asm.DefinedTypes)
                    .Where(t => typeof(ISystem).IsAssignableFrom(t))
                    .Where(t => !t.IsAbstract && !t.IsGenericType);
            var children = gameManager.transform.GetComponentsInChildren<Transform>();
            foreach (var systemType in systemTypes)
            {
                var childTransform = children.FirstOrDefault(t => t.gameObject.name == systemType.Name);
                if (childTransform && childTransform.TryGetComponent(typeof(ISystem), out var childSystem))
                    continue;
                if (childTransform == null)
                {
                    childTransform = new GameObject(systemType.Name, systemType).transform;
                    childTransform.SetParent(gameManager.transform);
                }
                else childTransform.gameObject.AddComponent(systemType);
                Debug.Log($"Create {systemType.Name}");
            }
        }
    }
}