//    ▄▀▀▀▄▄▄▄▄▄▄▀▀▀▄
//    █▒▒░░░░░░░░░▒▒█    ValueBar.cs
//     █░░█░░░░░█░░█     Created by WanNeng
//  ▄▄  █░░░▀█▀░░░█  ▄▄  Created   03/09/2024 12:56
// █░░█ ▀▄░░░░░░░▄▀ █░░█

using System;
using System.Text;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using WanFramework.Sequence;
using WanFramework.Utils;

namespace WanFramework.UI.Components
{
    /// <summary>
    /// 血条平滑过度序列
    /// </summary>
    [Serializable]
    internal class SliderBarSmoothlySequence : SequencePlaying<SliderBarSmoothlySequence>
    {
        public float newValue;
        public Slider slider;
        private float _velocity;
        public SliderBarSmoothlySequence() : base(
            new[]
            {
                SmoothlyChangeHealthValue(c => c.slider, c => c.newValue)
            })
        {
        }

        private static IPlaying<SliderBarSmoothlySequence> SmoothlyChangeHealthValue(Getter<Slider> slider, Getter<float> newValue)
        {
            return Playing(Update);
            void Update(SliderBarSmoothlySequence c, out bool isFinished)
            {
                var s = slider(c);
                var v = newValue(c);
                s.value = Mathf.SmoothDamp(s.value, v, ref c._velocity, 0.1f);
                isFinished = Mathf.Abs(v - s.value) < 0.1f;
            }
        }

        public override void OnExit()
        {
            base.OnExit();
            slider.value = newValue;
        }
    }
    
    /// <summary>
    /// 数值条
    /// </summary>
    [RequireComponent(typeof(Slider))]
    public class ValueBar : MonoBehaviour
    {
        [SerializeField]
        private float max;

        [SerializeField]
        private float current;

        [SerializeField]
        private TMP_Text currentText;
        
        [SerializeField]
        private TMP_Text maxText;
        
        [SerializeField]
        private SliderBarSmoothlySequence sliderBarSmoothlySequence;
        
        private Slider _slider;
        
        private void Awake()
        {
            _slider = GetComponent<Slider>();
            sliderBarSmoothlySequence.slider = _slider;
        }
        
        public void SetValue(float val)
        {
            current = val;
            sliderBarSmoothlySequence.newValue = val;
            if (!isActiveAndEnabled)
                return;
            using var str = ((int)current).ToStringNoGC();
            currentText.SetText(str);
            // 由于采用了平滑slider序列，不需要手动设置值了
            //_slider.value = val;
            if (!sliderBarSmoothlySequence.IsPlaying)
                sliderBarSmoothlySequence.Play(this);
        }
        public float GetValue() => current;
        public void SetMaxValue(float maxValue)
        {
            max = maxValue;
            if (!isActiveAndEnabled) return;
            using var str = ((int)max).ToStringNoGC();
            _slider.maxValue = max;
            maxText.SetText(str);
        }
        private void OnEnable()
        {
            _slider.maxValue = max;
            _slider.value = current;
            using var curStr = ((int)current).ToStringNoGC();
            currentText.SetText(curStr);
            using var maxStr = ((int)max).ToStringNoGC();
            maxText.SetText(maxStr);
        }

        private void OnDisable()
        {
            this.StopAllSequence();
        }
    }
}