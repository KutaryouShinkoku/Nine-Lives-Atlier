//    ▄▀▀▀▄▄▄▄▄▄▄▀▀▀▄
//    █▒▒░░░░░░░░░▒▒█    SequencePlayingPropertyDrawer.cs
//     █░░█░░░░░█░░█     Created by WanNeng
//  ▄▄  █░░░▀█▀░░░█  ▄▄  Created   01/14/2024 22:11
// █░░█ ▀▄░░░░░░░▄▀ █░░█

using System.Reflection;
using UnityEditor;
using UnityEditor.Graphs;
using UnityEngine;
using WanFramework.Sequence;

namespace WanFramework.Editor.Sequence
{
    [CustomPropertyDrawer(typeof(ISequencePlaying), true)]
    public class SequencePlayingPropertyDrawer : PropertyDrawer
    {
        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            GUI.BeginGroup(position, Styles.graphBackground);
            var propertyRect = new Rect(
                0, 
                0, 
                position.width - 50 - 6, 
                position.height);
            var buttonRect = new Rect(
                0 + propertyRect.width + 3, 
                0 + propertyRect.height - 20, 
                50, 
                20);
            EditorGUI.PropertyField(propertyRect, property, label, true);
            var sequence = GetSequence(property);
            if (sequence != null)
            {
                var isButtonClicked = GUI.Button(buttonRect, sequence.IsPlaying ? "Stop" : "Play");
                if (isButtonClicked)
                    SwitchSequenceState(property.serializedObject.targetObject as Behaviour, sequence);
            }

            GUI.EndGroup();
        }
        
        public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
        {
            return EditorGUI.GetPropertyHeight(property);
        }

        private ISequencePlaying GetSequence(SerializedProperty property)
        {
            // 使用 property.boxedValue 的话，会创建新的obj实例，无法得到唯一的SequencePlaying对象，所以此处使用了反射
            var behaviour = property.serializedObject.targetObject;
            var reflectProperty = behaviour.GetType().GetField(
                property.propertyPath, 
                BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
            if (reflectProperty != null)
                return reflectProperty.GetValue(behaviour) as ISequencePlaying;
            return null;
        }
        
        private void SwitchSequenceState(Behaviour owner, ISequencePlaying sequence)
        {
            if (sequence == null) return;
            if (!Application.isPlaying)
            {
                EditorUtility.DisplayDialog("WanFramework", "Run the game first!", "I know");
                return;
            }
            if (!owner) owner = SequenceSystem.Instance;
            if (sequence.IsPlaying)
                SequenceSystem.Instance.Stop(sequence);
            else
                SequenceSystem.Instance.Play(sequence, null, owner);
        }
    }
}