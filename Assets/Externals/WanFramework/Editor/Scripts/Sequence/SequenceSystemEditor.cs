//    ▄▀▀▀▄▄▄▄▄▄▄▀▀▀▄
//    █▒▒░░░░░░░░░▒▒█    SequenceSystemEditor.cs
//     █░░█░░░░░█░░█     Created by WanNeng
//  ▄▄  █░░░▀█▀░░░█  ▄▄  Created   01/14/2024 20:39
// █░░█ ▀▄░░░░░░░▄▀ █░░█

using System.Globalization;
using UnityEditor;
using UnityEngine;
using WanFramework.Sequence;

namespace WanFramework.Editor.Sequence
{
    [CustomEditor(typeof(SequenceSystem))]
    public class SequenceSystemEditor : UnityEditor.Editor
    {
        public override void OnInspectorGUI()
        {
            base.OnInspectorGUI();
            var sequenceSystem = target as SequenceSystem;
            if (!sequenceSystem) return;
            if (!Application.isPlaying) return;
            var sequenceInfo = new SequenceInfo();
            var i = 0;
            while (sequenceSystem.GetSequenceInfo(i++, ref sequenceInfo))
                GUILayout.Label($"[{sequenceInfo.Owner.name}] {sequenceInfo.Name} ({sequenceInfo.Percent})");
        }
    }
}