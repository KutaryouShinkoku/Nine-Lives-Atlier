using System.Threading;
using Cysharp.Threading.Tasks;
using Game.Audio;
using Game.Data;
using Game.Model.InGameSubModel;
using Game.SM.InGameState.ShopState;
using Game.UI.Common;
using Game.UI.Common.Components;
using TMPro;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.Serialization;
using UnityEngine.UI;
using WanFramework.UI;
using WanFramework.Utils;

namespace Game.UI.InGame.Shop
{
    public class InGameUIUpgradeCardView : UIRootView
    {
        public UnityEvent onDelete;
        public UnityEvent onCancel;
        public UnityEvent onConfirm;
        public UnityEvent onAdd;
        public UnityEvent onMinus;
        // 从1开始！和property name里的123对应！
        public UnityEvent<int> onChooseEffectVal;

        [FormerlySerializedAs("confirmAnim")]
        [SerializeField]
        private UniAnimation upgradeAnim;
        [SerializeField]
        private UniAnimation deleteAnim;
        [SerializeField]
        private CommonCardUIView cardView;
        [SerializeField]
        private Button deleteButton;
        [SerializeField]
        private Button addButton;
        [SerializeField]
        private Button minusButton;
        [SerializeField]
        private Button confirmButton;
        [SerializeField]
        private Button cancelButton;
        [SerializeField]
        private Button selectEffectVal1Button;
        [SerializeField]
        private GameObject selectEffectVal1Display;
        [SerializeField]
        private Button selectEffectVal2Button;
        [SerializeField]
        private GameObject selectEffectVal2Display;
        [SerializeField]
        private Button selectEffectVal3Button;
        [SerializeField]
        private GameObject selectEffectVal3Display;
        [SerializeField]
        private TMP_Text costText;

        [SerializeField]
        private GameObject deleteCardVfx;
        [SerializeField]
        private GameObject upgradeCardVfx;
        
        protected override void InitComponents()
        {
            base.InitComponents();
            deleteButton.onClick.AddListener(() => onDelete?.Invoke());
            confirmButton.onClick.AddListener(() => onConfirm?.Invoke());
            cancelButton.onClick.AddListener(() => onCancel?.Invoke());
            addButton.onClick.AddListener(() => onAdd?.Invoke());
            minusButton.onClick.AddListener(() => onMinus?.Invoke());
            selectEffectVal1Button.onClick.AddListener(() => onChooseEffectVal?.Invoke(1));
            selectEffectVal2Button.onClick.AddListener(() => onChooseEffectVal?.Invoke(2));
            selectEffectVal3Button.onClick.AddListener(() => onChooseEffectVal?.Invoke(3));
        }

        public void SetSelectedEffectVal(int index)
        {
            switch (index)
            {
                case 1:
                    selectEffectVal1Display.SetActive(true);
                    selectEffectVal2Display.SetActive(false);
                    selectEffectVal3Display.SetActive(false);
                    break;
                case 2:
                    selectEffectVal1Display.SetActive(false);
                    selectEffectVal2Display.SetActive(true);
                    selectEffectVal3Display.SetActive(false);
                    break;
                case 3:
                    selectEffectVal1Display.SetActive(false);
                    selectEffectVal2Display.SetActive(false);
                    selectEffectVal3Display.SetActive(true);
                    break;
                default:
                    selectEffectVal1Display.SetActive(false);
                    selectEffectVal2Display.SetActive(false);
                    selectEffectVal3Display.SetActive(false);
                    break;
            }
        }
        public void SetCard(CardModel card) => cardView.DataModel = card;
        public void SetCost(int cost) => costText.text = cost.ToString();
        public void SetEffectVals(int val1, int val2, int val3) => cardView.SetCardEffectValues(val1, val2, val3);
        public void SetCurrentAction(ShopCardAction action)
        {
            switch (action)
            {
                case ShopCardAction.None:
                    deleteCardVfx.SetActive(false);
                    upgradeCardVfx.SetActive(false);
                    break;
                case ShopCardAction.Delete:
                    deleteCardVfx.SetActive(true);
                    upgradeCardVfx.SetActive(false);
                    break;
                case ShopCardAction.Upgrade:
                    deleteCardVfx.SetActive(false);
                    upgradeCardVfx.SetActive(true);
                    break;
            }
        }
        public async UniTask PlayUpgradeAnimation(CancellationToken token)
        {
            AudioSystem.Instance.SendEvent(AudioIds.Effect_Shop_Upgrade);
            await upgradeAnim.Play(token); 
        }
        public async UniTask PlayDeleteAnimation(CancellationToken token)
        {
            AudioSystem.Instance.SendEvent(AudioIds.Effect_Shop_Delete);
            await deleteAnim.Play(token);
        }
        public void SetConfirmButtonEnable(bool enable) => confirmButton.gameObject.SetActive(enable);
        
        public void SetCancelButtonEnable(bool enable) => cancelButton.gameObject.SetActive(enable);
    }
}