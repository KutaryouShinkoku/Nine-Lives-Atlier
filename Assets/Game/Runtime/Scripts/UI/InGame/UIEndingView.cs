using System;
using System.Linq;
using Game.Data;
using Game.Localization.Components;
using Game.Logic;
using TMPro;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;
using WanFramework.UI;
using Random = UnityEngine.Random;

namespace Game.UI.InGame
{
    public class UIEndingView : UIRootView, ISerializationCallbackReceiver
    {
        [SerializeField]
        private Button backButton;
        [SerializeField]
        private Button infinityButton;
        [SerializeField]
        private GameObject winRoot;
        [SerializeField]
        private GameObject lossRoot;
        [SerializeField]
        private LocalizeText loseText;
        [SerializeField]
        #if UNITY_EDITOR
        [WanFramework.Utils.EnumString(typeof(LocalizeIds))]
        #endif
        private string[] loseMessages;
        private LocalizeIds[] _loseMessages;
        public void OnBeforeSerialize()
        {
            loseMessages = _loseMessages?.Select(id => id.ToString()).ToArray() ?? Array.Empty<string>();
        }
        public void OnAfterDeserialize()
        {
            _loseMessages = loseMessages?
                .Select(id => Enum.TryParse<LocalizeIds>(id, out var val) ? val : LocalizeIds.Empty)
                .ToArray() ?? Array.Empty<LocalizeIds>();
        }
        public UnityEvent onBackPressed;
        public UnityEvent onInfinityPressed;

        protected override void InitComponents()
        {
            base.InitComponents();
            backButton.onClick.AddListener(() => onBackPressed?.Invoke());
            infinityButton.onClick.AddListener(() => onInfinityPressed?.Invoke());
            SetResult(false);
        }
        public void SetResult(bool isWin)
        {
            winRoot.SetActive(isWin);
            lossRoot.SetActive(!isWin);
            var rnd = Random.Range(0, _loseMessages.Length);
            loseText.SetText(_loseMessages[rnd]);
        }
    }
}