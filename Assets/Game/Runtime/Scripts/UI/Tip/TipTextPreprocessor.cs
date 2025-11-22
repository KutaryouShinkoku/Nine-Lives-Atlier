using System;
using System.Text;
using Game.Data;
using Game.Localization;
using TMPro;
using UnityEngine;
using UnityEngine.Pool;

namespace Game.UI.Tip
{
    public class TipTextPreprocessor : ITextPreprocessor
    {
        private readonly StringBuilder _sb = new();
        
        public string PreprocessText(string text)
        {
            _sb.Clear();
            var list = ListPool<TipTextSeg>.Get();
            try
            {
                TipUtils.GetTipSeg(text, list);
                if (list.Count == 0) return text;
                var pos = 0;
                var span = text.AsSpan();
                for (var i = 0; i < list.Count; i++)
                {
                    _sb.Append(span.Slice(pos, list[i].Begin - pos));
                    FormatTipsToRichText(list[i].TipId, _sb);
                    pos = list[i].Back + 1;
                }
                if (span.Length != pos)
                    _sb.Append(span.Slice(pos, span.Length - pos));
                return _sb.ToString();
            }
            finally
            {
                ListPool<TipTextSeg>.Release(list);
            }
        }
        
        private static void FormatTipsToRichText(TipIds tipsName, StringBuilder sb)
        {
            var tip = tipsName.Data();
            sb.Append($"<b><color=#{ColorUtility.ToHtmlStringRGBA(tip.Color)}>{tip.Title.Local()}</color></b>");
        }
    }
}