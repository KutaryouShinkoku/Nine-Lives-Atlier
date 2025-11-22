using Cysharp.Threading.Tasks;
using Game.Audio;
using Game.Data;
using Game.Localization;
using Game.Model;
using Game.SM;
using Game.UI.InGame;
using Game.UI.MainMenu;
using Game.Utils;
using UnityEngine;
using WanFramework.Base;
using WanFramework.Data;
using WanFramework.Resource;
using WanFramework.UI;
using WanFramework.UI.DataComponent;
using WanFramework.Utils;

namespace Game
{
    public class GameMain : GameEntryPoint
    {
        private readonly DataBindingWorker _settingBinds = new();
        private void SetupSettingBindings()
        {
            _settingBinds.Bind(nameof(SettingModel.Volume), m =>
            {
                AudioSystem.Instance.SetValue(
                    "MainVolume",
                    AudioSystem.ToDb(DataModel<SettingModel>.Instance.Volume));
            });
            _settingBinds.Bind(nameof(SettingModel.Language), m =>
            {
                LocalizeSystem.Instance.SetDisplayLanguage(m.As<SettingModel>().Language);
            });
            _settingBinds.SetupDataBinding(DataModel<SettingModel>.Instance);
        }
        private void Setup()
        {
            SaveUtils.RegisterSettingSaveListener();
            SetupSettingBindings();
            MouseUtils.SetMouseClickAudio(AudioIds.UI_Mouse_Click);
        }
        public override async UniTask MainAsync()
        {
            Debug.Log($"Version: {GitInfo.GetRevisionHash()}");
#if UNITY_ANDROID || UNITY_IOS
            Application.targetFrameRate = (int)Screen.currentResolution.refreshRateRatio.value;
#else
            Application.targetFrameRate = -1;
#endif
            Setup();
#if UNITY_WEBGL
            ResourceSystem.Instance.LoadSceneAsync("Content/Scene/BgRoomDefault.unity").Forget();
            await UniTask.CompletedTask;
#else
            await ResourceSystem.Instance.LoadSceneAsync("Content/Scene/BgRoomDefault.unity");
#endif
            GameManager.Current.EnterState<MainMenuState>();
            AudioSystem.Instance.SendEvent(AudioIds.BGM_Mainmenu);
            //Debug.Log(LocalizeSystem.Instance.GetLocalText(DataSystem.Instance.Load<CardTable>().Get(CardIds.Unknown).Name));
            //var handAreaView = UISystem.Instance.ShowCommonView<InGameHandAreaView>("InGame/HandAreaView");
            //handAreaView.DataModel = DataModel<InGameModel>.Instance;
        }
    }
}