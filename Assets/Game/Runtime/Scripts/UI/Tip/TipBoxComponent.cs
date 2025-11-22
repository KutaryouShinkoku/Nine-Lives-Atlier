using System;
using System.Collections.Generic;
using System.Text;
using Game.Data;
using Game.Localization;
using Game.Localization.Components;
using JetBrains.Annotations;
using UnityEngine;
using UnityEngine.UI;

namespace Game.UI.Tip
{
    [Serializable]
    public struct TipBoxData : IEquatable<TipBoxData>, ISerializationCallbackReceiver
    {
        #if UNITY_EDITOR
        [WanFramework.Utils.EnumString(typeof(LocalizeIds))]
        #endif
        [SerializeField]
        private string title;
        #if UNITY_EDITOR
        [WanFramework.Utils.EnumString(typeof(LocalizeIds))]
        #endif
        [SerializeField]
        private string content;
        [SerializeField]
        private Color color;
        public LocalizeIds Title { get; private set; }
        public LocalizeIds Content { get; private set; }
        public Color Color => color;
        [CanBeNull]
        private IReadOnlyDictionary<string, Func<string>> _tipParams;
        public IReadOnlyDictionary<string, Func<string>> TipParams => _tipParams;
        public TipBoxData(LocalizeIds title, LocalizeIds content, Color color, IReadOnlyDictionary<string, Func<string>> tipParams = null)
        {
            Title = title;
            Content = content;
            this.title = string.Empty;
            this.content = string.Empty;
            this.color = color;
            _tipParams = tipParams;
        }
        public TipBoxData(TipIds tipId)
        {
            var data = tipId.Data();
            Title = data.Title;
            Content = data.Content;
            title = string.Empty;
            content = string.Empty;
            color = data.Color;
            _tipParams = null;
        }
        public bool Equals(TipBoxData other) => Title == other.Title && Content == other.Content;
        public override bool Equals(object obj) => obj is TipBoxData other && Equals(other);
        public override int GetHashCode() => HashCode.Combine((int)Title, (int)Content);
        public void OnBeforeSerialize()
        {
            title = Title.ToString();
            content = Content.ToString();
        }
        public void OnAfterDeserialize()
        {
            Title = Enum.Parse<LocalizeIds>(title);
            Content = Enum.Parse<LocalizeIds>(content);
        }
    }
    public class TipBoxComponent : MonoBehaviour
    {
        [SerializeField]
        private LocalizeText textName;
        [SerializeField]
        private LocalizeText textDesc;
        [SerializeField]
        private Image bgImg;
        
        private TipBoxData _tipData;
        private bool _isTitleDynamic;
        private bool _isDescDynamic;
        
        private void Update()
        {
            if (_tipData.TipParams == null ||
                _tipData.TipParams.Count == 0)
                return;
            if (_isTitleDynamic) UpdateTipTitle();
            if (_isDescDynamic) UpdateTipContent();
        }
        private void UpdateTipTitle() =>
            textName.SetRawText(
                $"<b><color=#{ColorUtility.ToHtmlStringRGBA(_tipData.Color)}>{Format(_tipData.Title.Local(), _tipData.TipParams, out _isTitleDynamic)}</color></b>");
        private void UpdateTipContent() => 
            textDesc.SetRawText(Format(_tipData.Content.Local(), _tipData.TipParams, out _isDescDynamic));
        public void SetTip(TipBoxData tip)
        {
            _tipData = tip;
            UpdateTipTitle();
            UpdateTipContent();
        }
        private string Format(string str, [CanBeNull] IReadOnlyDictionary<string, Func<string>> tipParams, out bool isDynamic)
        {
            isDynamic = false;
            if (tipParams == null || tipParams.Count == 0)
                return str;
            var span = str.AsSpan();
            var sb = new StringBuilder();
            for (var lp = 0; lp < span.Length; )
            {
                if (span[lp] != '{')
                {
                    sb.Append(span[lp++]);
                    continue;
                }
                var rp = lp + 1;
                while (rp < span.Length && span[rp] != '}') rp++;
                if (rp == span.Length || span[rp] != '}')
                {
                    sb.Append(span[lp..]);
                    break;
                }
                var keyword = span.Slice(lp + 1, rp - lp - 1);
                if (keyword.Length != 0 && tipParams.TryGetValue(keyword.ToString(), out var getter))
                {
                    sb.Append(getter());
                    isDynamic = true;
                }
                else
                    sb.Append(span.Slice(lp, rp - lp));
                lp = rp + 1;
            }
            return sb.ToString();
        }
    }
}