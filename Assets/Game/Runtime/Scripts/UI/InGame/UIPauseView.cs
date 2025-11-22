using Game.SM;
using UnityEngine;
using UnityEngine.UI;
using WanFramework.Base;
using WanFramework.UI;

namespace Game.UI.InGame
{
    public class UIPauseView : UIRootView
    {
        [SerializeField]
        private Button btnContinue;
        [SerializeField]
        private Button btnLeave;
        [SerializeField]
        private Button btnSetting;
        
        protected override void InitComponents()
        {
            base.InitComponents();
            btnContinue.onClick.AddListener(OnContinue);
            btnLeave.onClick.AddListener(OnLeave);
            btnSetting.onClick.AddListener(OnSetting);
        }
        private void OnContinue() => UISystem.Instance.Hide(this);
        private void OnLeave() => GameManager.Current.EnterState<MainMenuState>();
        private void OnSetting() => UISystem.Instance.ShowUI("Common/UISetting");
        public override void OnShow()
        {
            // 屎山1
            base.OnShow();
            Time.timeScale = 0;
        }
        public override void OnHide()
        {
            // 屎山2
            base.OnHide();
            Time.timeScale = 1;
        }
    }
}