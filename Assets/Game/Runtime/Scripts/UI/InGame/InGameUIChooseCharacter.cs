using System;
using System.Diagnostics;
using System.Linq;
using System.Threading;
using Cysharp.Threading.Tasks;
using Game.Audio;
using Game.Data;
using Game.Model;
using Game.Model.InGameSubModel;
using Game.UI.Common;
using Game.UI.Common.Components;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.Serialization;
using UnityEngine.TextCore.Text;
using UnityEngine.UI;
using WanFramework.Data;
using WanFramework.UI;
using WanFramework.UI.DataComponent;
using WanFramework.Utils;

namespace Game.UI.InGame
{
    public class InGameUIChooseCharacter : UIRootView
    {
        [SerializeField]
        private Toggle toggleFireElem;
        [SerializeField]
        private Toggle toggleAirElem;
        [SerializeField]
        private Toggle toggleEarthElem;
        [SerializeField]
        private Toggle toggleWaterElem;

        [SerializeField]
        private Button checkDeckButton;
        [SerializeField]
        private Button buttonConfirm;

        [SerializeField]
        private UniAnimation showAnim;
        [SerializeField]
        private UniAnimation hideAnim;
        
        [FormerlySerializedAs("characterInfoUIView")]
        [FormerlySerializedAs("characterUIView")]
        [SerializeField]
        private CommonUICharacterInfoView uiCharacterInfoView;
        
        private readonly bool[] _toggleState = new bool[4];

        private CharacterIds _selectedCharacterId;
        public UnityEvent<CharacterIds> onConfirmCharacter;

        private bool _canInteract = true;
        public bool CanInteract
        {
            get => _canInteract;
            set
            {
                _canInteract = value;
                toggleFireElem.enabled = _canInteract;
                toggleAirElem.enabled = _canInteract;
                toggleEarthElem.enabled = _canInteract;
                toggleWaterElem.enabled = _canInteract;
                checkDeckButton.enabled = _canInteract;
                buttonConfirm.enabled = _canInteract;
            }
        }
        
        protected override void InitComponents()
        {
            base.InitComponents();
            buttonConfirm.onClick.AddListener(OnConfirm);
            toggleFireElem.onValueChanged.AddListener(state => OnElemToggleChanged(CardBaseType.Fire, state));
            toggleAirElem.onValueChanged.AddListener(state => OnElemToggleChanged(CardBaseType.Air, state));
            toggleEarthElem.onValueChanged.AddListener(state => OnElemToggleChanged(CardBaseType.Earth, state));
            toggleWaterElem.onValueChanged.AddListener(state => OnElemToggleChanged(CardBaseType.Water, state));

            checkDeckButton.onClick.AddListener(OnCardDeckButton);
        }
        public override void OnShow()
        {
            base.OnShow();
            for (var i = 0; i < 4; ++i) _toggleState[i] = false;
            toggleFireElem.SetIsOnWithoutNotify(false);
            toggleAirElem.SetIsOnWithoutNotify(false);
            toggleEarthElem.SetIsOnWithoutNotify(false);
            toggleWaterElem.SetIsOnWithoutNotify(false);
            buttonConfirm.enabled = false;
            _selectedCharacterId = CharacterIds.Empty;
            uiCharacterInfoView.SetCharacter(_selectedCharacterId);
            checkDeckButton.gameObject.SetActive(false);
            buttonConfirm.gameObject.SetActive(false);
        }
        private void OnElemToggleChanged(CardBaseType elem, bool state)
        {
            if (!CanInteract) return;
            _toggleState[(int)elem] = state;
            
            var table = DataSystem.Instance.Load<CharacterTable>();
            var curCharacterId = 1;
            for (; curCharacterId < table.Length; ++curCharacterId)
            {
                var entry = table.Get((CharacterIds)curCharacterId);
                if (entry.CharElemental.Length != _toggleState.Length) continue;
                var isSame = !_toggleState.Where((t, j) => entry.CharElemental[j] != t).Any();
                if (isSame)
                    break;
            }
            if (curCharacterId == table.Length)
                _selectedCharacterId = CharacterIds.Unknown;
            else
                _selectedCharacterId = (CharacterIds)curCharacterId;
            uiCharacterInfoView.SetCharacter(_selectedCharacterId);
            buttonConfirm.enabled = 
                _selectedCharacterId != CharacterIds.Unknown && 
                _selectedCharacterId != CharacterIds.Empty;

            //设置一下牌组预览
            if (_selectedCharacterId != CharacterIds.Empty)
            {
                var m = DataModel<InGameModel>.Instance;
                m.CharacterId = _selectedCharacterId;
                m.CardDeck.Clear();
                var characterData = _selectedCharacterId.Data();
                foreach (var cardId in characterData.InitialCards)
                    DataModel<InGameModel>.Instance.CardDeck.Add(new CardModel
                    {
                        Id = cardId,
                        AdditionEffectVal1 = 0,
                        AdditionEffectVal2 = 0,
                        AdditionEffectVal3 = 0,
                        FeeChange = new CardCost
                        {
                            Air = 0,
                            Earth = 0,
                            Fire = 0,
                            Water = 0,
                        }
                    });
            }

            //点击属性之后更新一下检查卡组按钮
            checkDeckButton.gameObject.SetActive(_selectedCharacterId != CharacterIds.Empty);
            buttonConfirm.gameObject.SetActive(_selectedCharacterId != CharacterIds.Empty);
        }
        private void OnCardDeckButton()
        {
            if (!CanInteract) return;
            var cardDeckView = UISystem.Instance.ShowUI<UICardDeckView>("Common/UICardDeck");
            cardDeckView.SetCardDeckViewType(CardDeckViewType.CardDeck);
        }

        private void OnConfirm()
        {
            if (!CanInteract) return;
            onConfirmCharacter?.Invoke(_selectedCharacterId);
        }

        public void playAudio(string audioId)
        {
           if (Enum.TryParse(typeof(AudioIds), audioId, out var id))
           AudioSystem.Instance.SendEvent((AudioIds)id);
        }

        public UniTask PlayHide(CancellationToken token) => hideAnim.Play(token);
        public UniTask PlayShow(CancellationToken token) => showAnim.Play(token);
    }
}