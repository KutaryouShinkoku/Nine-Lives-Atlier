//    ▄▀▀▀▄▄▄▄▄▄▄▀▀▀▄
//    █▒▒░░░░░░░░░▒▒█    BindingObjectEditor.cs
//     █░░█░░░░░█░░█     Created by WanNeng
//  ▄▄  █░░░▀█▀░░░█  ▄▄  Created   01/03/2024 21:23
// █░░█ ▀▄░░░░░░░▄▀ █░░█

using System;
using UnityEditor;
using UnityEngine;
using WanFramework.UI;

namespace WanFramework.Editor.UI
{
    [CustomEditor(typeof(UIRootView), true)]
    public class UIViewScriptEditor : UnityEditor.Editor
    {
        private UIRootView _bObj;
        protected void OnEnable()
        {
            _bObj = target as UIRootView;
        }

        public override void OnInspectorGUI()
        {
            base.OnInspectorGUI();
        }
    }
}