using System.Threading;
using Cysharp.Threading.Tasks;
using Game.BattleAnim;
using Game.Data;
using Game.Localization.Components;
using Game.Model;
using Game.UI.Common;
using JetBrains.Annotations;
using TMPro;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;
using WanFramework.Base;
using WanFramework.Data;
using WanFramework.UI;
using WanFramework.UI.DataComponent;

namespace Game.UI.InGame
{
    /// <summary>
    /// 局内UI覆盖层
    /// </summary>
    public class InGameUICoverView : UIRootView
    {
        [SerializeField]
        private Button buttonSetting;
        [SerializeField]
        private Button buttonCardDeck;
        [SerializeField]
        private TMP_Text textCardDeckCount;
        [SerializeField]
        private Button buttonDrawDeck;
        [SerializeField]
        private TMP_Text textDrawDeckCount;
        [SerializeField]
        private Button buttonDiscardCard;
        [SerializeField]
        private TMP_Text textDiscardCardCount;
        
        [SerializeField]
        private LocalizeText textChapter;
        
        [SerializeField]
        private TMP_Text goldText;
        
        public UnityEvent onSettingsClick;

        [CanBeNull]
        private CancellationTokenSource _goldAnimCts;
        
        protected override void InitComponents()
        {
            base.InitComponents();
            Bind(nameof(InGameModel.CurrentLevel), m => SetLevel(m.As<InGameModel>().CurrentLevel));
            Bind(nameof(InGameModel.Gold), m => PlayGoldChangeAnim(m.As<InGameModel>().Gold));
            buttonCardDeck.onClick.AddListener(OnCardDeckButton);
            buttonDrawDeck.onClick.AddListener(OnDrawDeckButton);
            buttonDiscardCard.onClick.AddListener(OnDiscardCardButton);
            buttonSetting.onClick.AddListener(OnSettings);
            SetCardDeckVisible(false);
            SetDrawCardVisible(false);
            SetDiscardCardVisible(false);
        }

        protected override void OnDataModelChanged(DataModelBase dataModel)
        {
            if (dataModel is InGameModel inGameModel)
                SetGold(inGameModel.Gold);
            base.OnDataModelChanged(dataModel);
        }
        private void SetLevel(LevelIds id)
        {
            var level = id.Data();
            textChapter.SetText(level.Name);
        }
        public void PlayGoldChangeAnim(int gold)
        {
            _goldAnimCts?.Cancel();
            _goldAnimCts?.Dispose();
            _goldAnimCts = new CancellationTokenSource();
            if (!int.TryParse(goldText.text, out var oldGold))
                oldGold = gold;

            var deltaGold = gold - oldGold;
            var screenPos = UISystem.Instance.UICamera.WorldToScreenPoint(goldText.transform.position);
            screenPos.z = 450;
            var worldPos = UISystem.Instance.UICamera.ScreenToWorldPoint(screenPos);
            if (deltaGold != 0)
            {
                BattleAnimSystem.PlayNumberChangeAnim(_goldAnimCts.Token, goldText, oldGold, gold).Forget();
                BattleAnimSystem.Instance.PlayDamagePopupAnimWithPos(_goldAnimCts.Token, worldPos, deltaGold).Forget();
            }
            else goldText.SetText(gold.ToString());
        }
        public void SetGold(int gold)
        {
            _goldAnimCts?.Cancel();
            _goldAnimCts?.Dispose();
            _goldAnimCts = null;
            goldText.SetText(gold.ToString());
        }
        public void SetCardDeckCount(int count)
        {
            textCardDeckCount.SetText(count.ToString());
        }
        public void SetCardDeckVisible(bool visible)
        {
            buttonCardDeck.gameObject.SetActive(visible);
        }
        public void SetDrawDeckCount(int count)
        {
            textDrawDeckCount.SetText(count.ToString());
        }
        public void SetDrawCardVisible(bool visible)
        {
            buttonDrawDeck.gameObject.SetActive(visible);
        }
        public void SetDiscardDeckCount(int count)
        {
            textDiscardCardCount.SetText(count.ToString());
        }
        public void SetDiscardCardVisible(bool visible)
        {
            buttonDiscardCard.gameObject.SetActive(visible);
        }
        private void OnCardDeckButton()
        {
            var cardDeckView = UISystem.Instance.ShowUI<UICardDeckView>("Common/UICardDeck");
            cardDeckView.SetCardDeckViewType(CardDeckViewType.CardDeck);
        }
        private void OnDrawDeckButton()
        {
            var cardDeckView = UISystem.Instance.ShowUI<UICardDeckView>("Common/UICardDeck");
            cardDeckView.SetCardDeckViewType(CardDeckViewType.RemainCards);
        }
        private void OnDiscardCardButton()
        {
            var cardDeckView = UISystem.Instance.ShowUI<UICardDeckView>("Common/UICardDeck");
            cardDeckView.SetCardDeckViewType(CardDeckViewType.DiscardCards);
        }
        private void OnSettings() => onSettingsClick?.Invoke();
        public override void OnHide()
        {
            base.OnHide();
            _goldAnimCts?.Cancel();
            _goldAnimCts?.Dispose();
            _goldAnimCts = null;
        }
    }
}