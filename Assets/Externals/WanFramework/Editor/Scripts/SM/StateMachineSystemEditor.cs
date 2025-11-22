//    ▄▀▀▀▄▄▄▄▄▄▄▀▀▀▄
//    █▒▒░░░░░░░░░▒▒█    StateMachineSystemEditor.cs
//     █░░█░░░░░█░░█     Created by WanNeng
//  ▄▄  █░░░▀█▀░░░█  ▄▄  Created   01/11/2024 23:28
// █░░█ ▀▄░░░░░░░▄▀ █░░█

using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using WanFramework.SM;

namespace WanFramework.Editor.SM
{
    [CustomEditor(typeof(StateMachineSystem))]
    public class StateMachineSystemEditor : UnityEditor.Editor
    {
        public override void OnInspectorGUI()
        {
            base.OnInspectorGUI();
            if (Application.isPlaying)
            {
                var stateMachineSystem = target as StateMachineSystem;
                if (stateMachineSystem == null) return;
                DisplayStateMachine(stateMachineSystem);
                DisplayState(stateMachineSystem);
            }
            else
            {
                EditorGUILayout.HelpBox("Playing the game to display StateMachine info", MessageType.Info);
            }
        }

        private void DisplayState(StateMachineSystem stateMachineSystem)
        {
            EditorGUILayout.LabelField("States: ");
            EditorGUI.indentLevel++;
            foreach (var state in stateMachineSystem.GetStates())
                EditorGUILayout.LabelField($"{state.GetType().Name}({state.Id})");
            EditorGUI.indentLevel--;
        }
        
        private void DisplayStateMachine(StateMachineSystem stateMachineSystem)
        {
            EditorGUILayout.LabelField("Machines: ");
            EditorGUI.indentLevel++;
            foreach (var stateMachine in stateMachineSystem.GetMachines())
            {
                var stateMachineBehaviour = stateMachine as MonoBehaviour;
                if (stateMachineBehaviour == null) continue;
                EditorGUILayout.LabelField(stateMachineBehaviour.name);
                EditorGUI.indentLevel++;
                EditorGUILayout.ObjectField(stateMachineBehaviour, typeof(Object), true);
                EditorGUILayout.LabelField("State: ", $"{stateMachine.CurrentState?.GetType().Name ?? ""}({stateMachine.CurrentState?.Id ?? -1})");
                EditorGUI.indentLevel--;
            }
            EditorGUI.indentLevel--;
        }
    }
}