using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Unity.Properties;
using UnityEditor;
using UnityEditor.Graphs;
using UnityEngine;
using WanFramework.Base;
using WanFramework.SM;
using Object = UnityEngine.Object;

namespace WanFramework.Editor.Debugger
{
    internal class StateTypeCache
    {
        public readonly Dictionary<string, int> gameStateKeyIdMap = new();
        public IList<string> keys = new List<string>();
        public IList<Type> types = new List<Type>();
        public UnityEditor.Editor[] stateEditor;
        
        public UnityEditor.Editor stateMachineEditor;
        public void Freeze()
        {
            keys = keys.ToArray();
            types = types.ToArray();
            stateEditor = new UnityEditor.Editor[types.Count];
        }
    }
    public class GameStateMachine : EditorWindow
    {
        private Dictionary<Type, StateTypeCache> _stateTypeCaches = new();
        private MethodInfo _enterStateMethod;

        private Vector2 _scroll;
        
        [MenuItem("Window/WanFramework/GameStateMachineWindow")]
        [MenuItem("WanFramework/Window/GameStateMachineWindow")]
        private static void ShowWindow()
        {
            var wnd = GetWindow<GameStateMachine>();
            wnd.titleContent = new GUIContent("Game State Machine Window");
        }
        private void OnEnable()
        {
            foreach (var state in TypeCache.GetTypesDerivedFrom<IState>())
            {
                if (state.IsAbstract) continue;
                var stateBaseType = state;
                while (stateBaseType != null &&
                       (!stateBaseType.IsGenericType || stateBaseType.GetGenericTypeDefinition() != typeof(StateBehaviour<>)))
                    stateBaseType = stateBaseType.BaseType;
                if (stateBaseType == null) continue;
                var stateMachineType = stateBaseType.GetGenericArguments()[0];
                if (!_stateTypeCaches.TryGetValue(stateMachineType, out var stateCache))
                {
                    stateCache = new();
                    _stateTypeCaches[stateMachineType] = stateCache;
                }
                stateCache.keys.Add(state.Name);
                stateCache.types.Add(state);
                stateCache.gameStateKeyIdMap[state.Name] = stateCache.keys.Count - 1;
            }
            foreach (var val in _stateTypeCaches.Values)
                val.Freeze();
            _enterStateMethod = typeof(IStateMachine).GetMethod("EnterState");
        }

        private void OnGUI()
        {
            if (!Application.isPlaying)
            {
                EditorGUILayout.HelpBox("游戏跑起来才能切", MessageType.Warning);
                return;
            }

            _scroll = GUILayout.BeginScrollView(_scroll);
            foreach (var machine in StateMachineSystem.Instance.GetMachines())
            {
                if (!_stateTypeCaches.TryGetValue(machine.GetType(), out var stateCache))
                    continue;
                if (machine.CurrentState == null) continue;
                GUILayout.BeginHorizontal();
                var cur = machine.CurrentState.GetType().Name;
                if (!stateCache.gameStateKeyIdMap.TryGetValue(cur, out var id))
                {
                    GUILayout.Label($"Current state {cur} unknown");
                    return;
                }
                var newId = EditorGUILayout.Popup((machine as MonoBehaviour)?.name ?? "???", id, stateCache.keys as string[]);
                var reenter = GUILayout.Button("Re-enter current state");
                if (newId != id || reenter)
                {
                    var newStateType = stateCache.types[newId];
                    _enterStateMethod.MakeGenericMethod(newStateType)
                        .Invoke(machine, Array.Empty<object>());
                }
                GUILayout.EndHorizontal();
                //DrawStateMachineEditor(stateCache, machine as MonoBehaviour);
                if (machine.CurrentState is MonoBehaviour stateBehaviour)
                    DrawStateEditor(stateCache, stateBehaviour, newId);
                EditorGUILayout.LabelField("_____________________________________________________________");
            }
            GUILayout.EndScrollView();
        }

        private void DrawStateMachineEditor(StateTypeCache stateCache, MonoBehaviour sm)
        {
            UnityEditor.Editor.CreateCachedEditor(sm, null, ref stateCache.stateMachineEditor);
            EditorGUILayout.Space();
            EditorGUILayout.BeginVertical(Styles.graphBackground);
            EditorGUILayout.InspectorTitlebar(false, stateCache.stateMachineEditor);
            EditorGUI.indentLevel += 2;
            stateCache.stateMachineEditor.OnInspectorGUI();
            EditorGUI.indentLevel -= 2;
            EditorGUILayout.EndVertical();
        }
        
        private void DrawStateEditor(StateTypeCache stateCache, MonoBehaviour state, int id)
        {
            UnityEditor.Editor.CreateCachedEditor(state, null, ref stateCache.stateEditor[id]);
            EditorGUILayout.Space();
            EditorGUILayout.BeginVertical(Styles.graphBackground);
            EditorGUILayout.InspectorTitlebar(false, stateCache.stateEditor[id]);
            EditorGUI.indentLevel += 2;
            stateCache.stateEditor[id].OnInspectorGUI();
            EditorGUI.indentLevel -= 2;
            EditorGUILayout.EndVertical();
        }
    }
}