using System;
using TMPro;
using UnityEngine;

namespace Game.UI.Tip
{
    /// <summary>
    /// 支持显示Tip的文本
    /// </summary>
    [RequireComponent(typeof(TMP_Text))]
    public class TipableText : MonoBehaviour
    {
        public void Awake() => GetComponent<TMP_Text>().textPreprocessor = TipUtils.TipTextPreprocessor;
    }
}