using System;
using System.Text;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using WanFramework.Utils;

namespace WanFramework.Base
{
    public class CrashCover : MonoBehaviour
    {
        [SerializeField]
        private TMP_Text applicationInfoText;
        [SerializeField]
        private TMP_Text crashLogText;
        [SerializeField]
        private Button copyButton;
        [SerializeField]
        private Button exitButton;
        [SerializeField]
        private Button hideButton;
        [SerializeField]
        private GameObject coverRoot;
        [SerializeField]
        private int maxCrashLog = 20;

        private string _appInfo;
        private string _crashLog;
        private int _crashIndex;
        
        void Awake()
        {
            copyButton.onClick.AddListener(() =>
            {
                GUIUtility.systemCopyBuffer = _crashLog;
            });
            exitButton.onClick.AddListener(() =>
            {
                #if UNITY_EDITOR
                UnityEditor.EditorApplication.isPlaying = false;
                #else
                Application.Quit();
                #endif
            });
            hideButton.onClick.AddListener(() =>
            {
                coverRoot.SetActive(!coverRoot.activeSelf);
            });
        }
        void OnEnable()
        {
            const string title = "Oh no! Our code tripped over its own feet. We're helping it up — please take a screenshot and send it to us!";
            var gitInfo = GitInfo.GetRevisionHash();
            var stateInfo = GameManager.Current.CurrentState?.GetType().FullName ?? "Unknown State";
            _appInfo = $"{title}\n{gitInfo}\n{stateInfo}";
            applicationInfoText.text = _appInfo;
        }
        public void AddCrashLog(string message)
        {
            if (_crashIndex > maxCrashLog) return;
            _crashLog += $"{message}\n";
            crashLogText.text = _crashLog;
            ++_crashIndex;
        }
        public void AddCrashLog(string message, string trace)
        {
            if (_crashIndex > maxCrashLog) return;
            var sb = new StringBuilder();
            sb.Append($"[{_crashIndex}] ====={message}=====");
            var traceLines = trace.Split('\n');
            foreach (var traceLine in traceLines)
                sb.Append($"\n[{_crashIndex}] \t{traceLine}");
            sb.Append("\n");
            _crashLog += sb.ToString();
            crashLogText.text = _crashLog;
            ++_crashIndex;
            var pos = traceLines.Length > 0 ? traceLines[0] : "Unknown Position";
            applicationInfoText.text = $"{_appInfo}\n{pos}";
        }
    }
}