using System;
using Game.Audio;
using Game.Data;
using Game.Localization;
using Game.Model;
using Game.SM.InGameState;
using UnityEngine;
using UnityEngine.UI;
using WanFramework.Base;
using WanFramework.UI;
using WanFramework.UI.DataComponent;

namespace Game.UI.MainMenu
{
    public class UIMainMenuView : UIRootView
    {
        [SerializeField]
        private Button buttonTutorial;
        [SerializeField]
        private Button buttonNewGame;
        [SerializeField]
        private Button buttonExit;
        [SerializeField]
        private Button buttonContinue;
        [SerializeField]
        private Button buttonSettings;
        [SerializeField]
        private Button buttonCredits;
        [SerializeField]
        private Button buttonSwitchLanguage;

        protected override void InitComponents()
        {
            base.InitComponents();
            buttonTutorial.onClick.AddListener(OnButtonTutorial);
            buttonNewGame.onClick.AddListener(OnButtonNewGame);
            buttonExit.onClick.AddListener(OnButtonExit);
            buttonContinue.onClick.AddListener(OnButtonContinue);
            buttonSettings.onClick.AddListener(OnButtonSettings);
            buttonCredits.onClick.AddListener(OnButtonCredits);
            buttonSwitchLanguage.onClick.AddListener(OnButtonSwitchLanguage);
        }
        private void OnButtonTutorial()
        {
            DataModel<InGameModel>.Instance.Reset();
            GameManager.Current.EnterState<InGameTutorialState>();
            AudioSystem.Instance.SendEvent(AudioIds.UI_Button_Click);
        }
        private void OnButtonNewGame()
        {
            DataModel<InGameModel>.Instance.Reset();
            GameManager.Current.EnterState<InGameChooseCharacterState>();
            AudioSystem.Instance.SendEvent(AudioIds.UI_Button_Click);
        }
        private void OnButtonExit()
        {
            AudioSystem.Instance.SendEvent(AudioIds.UI_Button_Click);
            #if UNITY_EDITOR
            UnityEditor.EditorApplication.isPlaying = false;
            #else
            Application.Quit();
            #endif
        }
        private void OnButtonContinue()
        {
            if (!SaveUtils.IsGameSaveExist())
            {
                Debug.Log("默认存档不存在");
                return;
            }
            SaveUtils.LoadGame();
            GameManager.Current.EnterState<InGameChooseLevelState>();
            AudioSystem.Instance.SendEvent(AudioIds.UI_Button_Click);
        }
        private void OnButtonSettings()
        {
            UISystem.Instance.ShowUI("Common/UISetting");
            AudioSystem.Instance.SendEvent(AudioIds.UI_Button_Click);
        }
        private void OnButtonCredits()
        {
            UISystem.Instance.ShowUI("Common/UICredit");
            AudioSystem.Instance.SendEvent(AudioIds.UI_Button_Click);
        }
        private void OnButtonSwitchLanguage()
        {
            LocalizeSystem.Instance.SwitchNextLanguage();
            AudioSystem.Instance.SendEvent(AudioIds.UI_Button_Click);
        }
    }
}