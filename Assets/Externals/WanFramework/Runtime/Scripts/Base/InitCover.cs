using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using TMPro;
using UnityEngine;
using UnityEngine.Serialization;
using UnityEngine.UI;

namespace WanFramework.Base
{
    public class InitCover : MonoBehaviour
    {
        [SerializeField]
        private TMP_Text loadingStateText;
        private string _currentLoadingState;
        private bool _isLoadingStateChanged = false;
        
        [SerializeField]
        private Slider progressBar;
        [SerializeField]
        private Slider subProgressBar;
        
        [SerializeField]
        private TMP_Text resourceProcessingText;
        private readonly List<string> _resLogLines = new();
        private readonly StringBuilder _resLogStringBuilder = new();
        private bool _isResourceLogChanged = false;
        [SerializeField]
        private int maxResLogLine = 4;
        public void SetCurrentState(string state)
        {
            _currentLoadingState = state;
            _isLoadingStateChanged = true;
        }
        public void PrintResourceLog(string resourceLog)
        {
            _resLogLines.Add(resourceLog);
            while (_resLogLines.Count > maxResLogLine)
                _resLogLines.RemoveAt(0);
            _isResourceLogChanged = true;
        }
        public void SetProgress(float progress)
            => progressBar.value = progress;
        public void SetSubProgress(float progress)
            => subProgressBar.value = progress;
        public void Update()
        {
            UpdateLoadingState();
            UpdateResourceLog();
        }
        private void UpdateLoadingState()
        {
            if (!_isLoadingStateChanged) return;
            _isLoadingStateChanged = false;
            loadingStateText.SetText(_currentLoadingState);
        }
        private void UpdateResourceLog()
        {
            if (!_isResourceLogChanged) return;
            _isResourceLogChanged = false;
            _resLogStringBuilder.Clear();
            for (var i = 0; i < _resLogLines.Count; i++)
            {
                var a = (1 - (_resLogLines.Count - i - 1.0f) / _resLogLines.Count);
                var color = new Color(1, 1, 1, a);
                _resLogStringBuilder.Append($"<color=#{ColorUtility.ToHtmlStringRGBA(color)}>{_resLogLines[i]}</color>");
                if (i != _resLogLines.Count - 1) _resLogStringBuilder.Append("\n");
            }
            resourceProcessingText.SetText(_resLogStringBuilder);
        }
        public void OnEnable()
        {
            resourceProcessingText.SetText("");
            loadingStateText.SetText("");
        }
        public void OnDisable()
        {
            _resLogLines.Clear();
            _resLogStringBuilder.Clear();
            _currentLoadingState = "";
            resourceProcessingText.SetText("");
            loadingStateText.SetText("");
        }
    }
}