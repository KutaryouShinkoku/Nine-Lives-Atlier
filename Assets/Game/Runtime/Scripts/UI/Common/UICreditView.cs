using UnityEngine;
using UnityEngine.UI;
using WanFramework.UI;

namespace Game.UI.Common
{
    public class UICreditView : UIRootView
    {
        [SerializeField]
        private Button btnBack;
        protected override void InitComponents()
        {
            base.InitComponents();
            btnBack.onClick.AddListener(OnBack);
        }
        private void OnBack() => UISystem.Instance.Hide(this);
    }
}