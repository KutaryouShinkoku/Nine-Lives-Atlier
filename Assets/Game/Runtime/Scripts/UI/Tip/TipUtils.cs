using System;
using System.Collections.Generic;
using Game.Data;
using UnityEngine;
using UnityEngine.Pool;
using WanFramework.UI;

namespace Game.UI.Tip
{
    public struct TipTextSeg
    {
        public readonly int Begin;
        public readonly int Back;
        public readonly TipIds TipId;
        public TipTextSeg(int begin, int back, TipIds tipId)
        {
            Begin = begin;
            Back = back;
            TipId = tipId;
        }
    }
    public static class TipUtils
    {
        public static TipTextPreprocessor TipTextPreprocessor { get; } = new();
        private static TipProviderRegion _providerRegion;
        
        public static TipProviderRegion CurrentProviderRegion => _providerRegion;
        public static Vector3 TipActivePos { get; private set; }
        /// <summary>
        /// 获取文本中所有Tip并加入到交互区域当中
        /// </summary>
        /// <param name="text"></param>
        /// <param name="region"></param>
        public static void GetTipsAndAddToRegion(string text, TipProviderRegion region)
        {
            var segList = ListPool<TipTextSeg>.Get();
            try
            {
                GetTipSeg(text, segList);
                for (var i = 0; i < segList.Count; i++)
                    region.AddTip(new TipBoxData(segList[i].TipId));
            }
            finally
            {
                ListPool<TipTextSeg>.Release(segList);
            }
        }
        
        /// <summary>
        /// 获取文本中包含的所有Tips引用
        /// </summary>
        /// <param name="text"></param>
        /// <param name="tips">Tips引用输出</param>
        public static void GetTipSeg(string text, List<TipTextSeg> tips)
        {
            tips.Clear();
            var lBrackets = -1;
            var textSpan = text.AsSpan();
            // 最小匹配
            for (var i = 0; i < textSpan.Length; i++)
            {
                if (text[i] == '[')
                    lBrackets = i;
                else if (text[i] == ']' && lBrackets != -1)
                {
                    var key = textSpan.Slice(lBrackets + 1, i - lBrackets - 1);
                    if (!Enum.TryParse<TipIds>(key.ToString(), out var tipId))
                    {
                        lBrackets = -1;
                        continue;
                    }
                    tips.Add(new TipTextSeg(lBrackets, i, tipId));
                }
            }
        }
        public static void ActivateTipProvider(TipProviderRegion tipProviderRegion)
        {
            if (_providerRegion != tipProviderRegion)
                TipActivePos = UISystem.Instance.UICamera.WorldToScreenPoint(tipProviderRegion.transform.position);
            _providerRegion = tipProviderRegion;
            var view = UISystem.Instance.ShowUI<TipView>("Tip/TipView");
            view.SetTips(_providerRegion.GetTips());
        }
        public static void DeactivateTipProvider(TipProviderRegion tipProviderRegion)
        {
            if (tipProviderRegion != _providerRegion) return;
            _providerRegion = null;
            var view = UISystem.Instance.GetUI<TipView>("Tip/TipView");
            if (view)
            {
                UISystem.Instance.Hide(view);
                view.ClearTip();
            }
        }
    }
}