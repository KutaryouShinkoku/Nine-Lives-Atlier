using System;
using Game.Audio;
using Game.Data;
using UnityEditor;
using UnityEngine;

namespace Game.Editor.Scripts.Audio
{
    public class AudioDebugEditor : EditorWindow
    {
        private AudioIds _selectedAudioEvent;
        
        [MenuItem("Window/WanFramework/AudioDebugEditor")]
        [MenuItem("WanFramework/Window/AudioDebugEditor")]
        private static void ShowWindow()
        {
            var wnd = GetWindow<AudioDebugEditor>();
            wnd.titleContent = new GUIContent("音效调试器");
        }
        
        public void OnGUI()
        {
            GUILayout.Label("音效测试");
            _selectedAudioEvent = (AudioIds)EditorGUILayout.EnumPopup(_selectedAudioEvent);
            if (!EditorApplication.isPlaying)
                GUILayout.Label("你先跑起来");
            else
            {
                if (GUILayout.Button("发送事件"))
                    AudioSystem.Instance.SendEvent(_selectedAudioEvent);
            }
        }
    }
}