using Game.Audio;
using Game.Localization;
using Game.Model;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using WanFramework.UI;
using WanFramework.UI.DataComponent;

namespace Game.UI.Common
{
    public class UISettingView : UIRootView
    {
        [SerializeField]
        private Button btnApply;
        [SerializeField]
        private Button btnCancel;
        [SerializeField]
        private Button btnRevert;
        [SerializeField]
        private Slider sliderVolume;
        [SerializeField]
        private TMP_Text textVolume;
        [SerializeField]
        private Button[] btnLanguages;
        [SerializeField]
        private Toggle toggleFastUse;
        [SerializeField]
        private Toggle toggleFullScreen;

        [SerializeField]
        private Slider sliderCameraShake;
        [SerializeField]
        private TMP_Text textCameraShake;


        protected override void InitComponents()
        {
            base.InitComponents();
            btnApply.onClick.AddListener(OnApply);
            btnCancel.onClick.AddListener(OnCancel);
            btnRevert.onClick.AddListener(OnRevert);
            for (var i = 0; i < btnLanguages.Length; ++i)
            {
                var lang = (Language)i;
                btnLanguages[i].onClick.AddListener(() => OnLanguageChanged(lang));
            }
            Bind(nameof(SettingModel.Volume), m => SetVolume(m.As<SettingModel>().Volume));
            Bind(nameof(SettingModel.FastUse), m => toggleFastUse.SetIsOnWithoutNotify(m.As<SettingModel>().FastUse));
            Bind(nameof(SettingModel.CameraShakeSensitivity), m => SetCameraShakeSensitivity(m.As<SettingModel>().CameraShakeSensitivity));
            sliderCameraShake.onValueChanged.AddListener(v => DataModel<SettingModel>.Instance.CameraShakeSensitivity = v);

            DataModel = DataModel<SettingModel>.Instance;
            sliderVolume.onValueChanged.AddListener(v => DataModel<SettingModel>.Instance.Volume = v);
            toggleFastUse.onValueChanged.AddListener(b => DataModel<SettingModel>.Instance.FastUse = b);
        }
        public override void OnShow()
        {
            base.OnShow();
            SaveUtils.EnableSettingAutoSave = false;
            // 全屏按钮特殊处理，用PlayerPrefs
            toggleFullScreen.isOn = Screen.fullScreen;
        }
        public override void OnHide()
        {
            base.OnHide();
            SaveUtils.EnableSettingAutoSave = true;
            if (Screen.fullScreen != toggleFullScreen.isOn)
            {
                if (toggleFullScreen.isOn)
                {
                    var screenWidth = Screen.currentResolution.width;
                    var screenHeight = Screen.currentResolution.height;
                    Screen.SetResolution(screenWidth, screenHeight, FullScreenMode.FullScreenWindow);
                }
                else
                {
                    const int screenWidth = 960;
                    const int screenHeight = 540;
                    Screen.SetResolution(screenWidth, screenHeight, FullScreenMode.Windowed);
                }
            }
        }
        private void OnApply() => UISystem.Instance.Hide(this);
        private void OnCancel()
        {
            SaveUtils.LoadSetting();
            UISystem.Instance.Hide(this);
        }
        private void OnRevert() => SaveUtils.LoadSetting();
        private void OnLanguageChanged(Language lang) => LocalizeSystem.Instance.SetDisplayLanguage(lang);
        private void SetVolume(float val)
        {
            textVolume.text = ((int)(val * 100)).ToString();
            sliderVolume.SetValueWithoutNotify(val);
        }

        private void SetCameraShakeSensitivity(float val)
        {
            textCameraShake.text = $"{Mathf.RoundToInt(val * 100)}";
            sliderCameraShake.SetValueWithoutNotify(val);
        }
    }
}