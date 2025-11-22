using System;
using UnityEngine;

namespace WanFramework.Utils
{
    public class DebugUtils : MonoBehaviour
    {
        private int _janky = 0;

        private GUIStyle _jankyTextStyle = new GUIStyle(GUIStyle.none);
        
        private void Awake()
        {
            if (!Debug.isDebugBuild && !Application.isEditor)
                Destroy(gameObject);
            _jankyTextStyle.normal.textColor = Color.red;
        }

        private void OnGUI()
        {
            if (!Debug.isDebugBuild && !Application.isEditor) return;
            GUILayout.BeginArea(new Rect(10, 10, 150, 150));
            GUILayout.BeginVertical();
            var fps = (int)(1 / Time.deltaTime);
            using var fpsStr = fps.ToStringNoGC();
            GUILayout.Label(fpsStr.ToString());
            var isJankyFrame = fps < 30;
            if (isJankyFrame) ++_janky;
            using var jankyStr = _janky.ToStringNoGC();
            if (isJankyFrame)
                GUILayout.Label(jankyStr.ToString(), _jankyTextStyle);
            else
                GUILayout.Label(jankyStr.ToString());
            GUILayout.EndVertical();
            GUILayout.EndArea();
        }
    }
}