using System;
using System.Buffers;
using System.Text;
using Game.Data;
using JetBrains.Annotations;
using TMPro;
using UnityEngine;
using UnityEngine.Events;
using WanFramework.Data;

namespace Game.Localization.Components
{
    [RequireComponent(typeof(TMP_Text))]
    public class LocalizeText : MonoBehaviour, ILocalizeComponent
    {
        private TMP_Text _text;
        
        // 只使用LocalizeText
        #if UNITY_EDITOR
        [WanFramework.Utils.EnumString(typeof(LocalizeIds))]
        #endif
        [SerializeField]
        private string localizeText;
        
        [CanBeNull] private LocalizeIds? _localizeText;
        private bool _isRaw = false;
        private object[] _formatParams = Array.Empty<object>();

        public UnityEvent onLanguageChanged;
        
        private void Awake()
        {
            _text = GetComponent<TMP_Text>();
        }

        private void Start()
        {
            if (_localizeText != null) return;
            if (_isRaw) return;
            if (Enum.TryParse(typeof(LocalizeIds), localizeText, out var l))
                SetText((LocalizeIds)l);
            else
                SetText(LocalizeIds.Unknown);
        }

        private void OnEnable()
        {
            if (!_isRaw) UpdateText();
        }
        public void SetText(LocalizeIds l, params object[] args)
        {
            _isRaw = false;
            _localizeText = l;
            _formatParams = args;
            UpdateText();
        }
        public void OnLanguageChanged()
        {
            UpdateText();
            onLanguageChanged?.Invoke();
        }
        public void SetRawText(StringBuilder sb)
        {
            _isRaw = true;
            _localizeText = LocalizeIds.Empty;
            _formatParams = Array.Empty<object>();
            if (!_text) TryGetComponent(out _text);
            _text.SetText(sb);
        }
        public void SetRawText(string str)
        {
            _isRaw = true;
            _localizeText = LocalizeIds.Empty;
            _formatParams = Array.Empty<object>();
            if (!_text) TryGetComponent(out _text);
            _text.SetText(str);
        }
        private void UpdateText()
        {
            if (_localizeText == null) return;
            var str = LocalizeSystem.Instance.GetLocalText(_localizeText.Value);
            if (!_text) TryGetComponent(out _text);
            _text.text = string.Format(str ?? string.Empty, _formatParams);
        }
    }
}