// //    ▄▀▀▀▄▄▄▄▄▄▄▀▀▀▄
// //    █▒▒░░░░░░░░░▒▒█
// //     █░░█░░░░░█░░█     Created by WanNeng
// //  ▄▄  █░░░▀█▀░░░█  ▄▄  Created   02/08/2025 22:02
// // █░░█ ▀▄░░░░░░░▄▀ █░░█

using System;
using System.Threading;
using Cysharp.Threading.Tasks;
using Game.Audio;
using Game.BattleAnim;
using Game.Data;
using Game.Model.InGameSubModel;
using Game.UI.Common.Components;
using TMPro;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;
using WanFramework.UI;
using WanFramework.Utils;

namespace Game.UI.InGame
{
    public class InGameUIBattleRewardView : UIRootView
    {
        [SerializeField]
        private CardRewardView[] rewardCardViews;

        [SerializeField]
        private GameObject normalTitleRoot;
        [SerializeField]
        private GameObject rareTitleRoot;

        [SerializeField]
        private Button skipButton;
        [SerializeField]
        private TMP_Text maxRewardText;
        [SerializeField]
        private TMP_Text curRewardText;
        [SerializeField]
        private TMP_Text skipRewardText;
        private int _oldCount = 0;
        public UnityEvent<int> onSelectReward;

        protected override void InitComponents()
        {
            base.InitComponents();
            foreach (var view in rewardCardViews)
            {
                view.gameObject.SetActive(false);
                view.onCardEnter.AddListener(OnCardEnter);
                view.onCardExit.AddListener(OnCardExit);
                view.onCardClick.AddListener(OnCardClick);
            }
            skipButton.onClick.AddListener(OnSkipClick);
            normalTitleRoot.SetActive(false);
            rareTitleRoot.SetActive(false);
        }
        private void OnCardEnter(CardRewardView cardView)
        {
            cardView.PlaySelectRewardAnim(CancellationToken.None).Forget();
        }
        private void OnCardExit(CardRewardView cardView)
        {
            cardView.PlayNormalAnim(CancellationToken.None).Forget();
        }
        private void OnSkipClick() 
        {
            AudioSystem.Instance.SendEvent(AudioIds.Effect_Reward_Skip);
            onSelectReward?.Invoke(3);
        }
        private void OnCardClick(CardRewardView cardView) 
        {
            AudioSystem.Instance.SendEvent(AudioIds.Effect_Reward_Selected);
            onSelectReward?.Invoke(Array.IndexOf(rewardCardViews, cardView)); 
        } 
        public void SetReward(int index, CardModel card)
        {
            if (index < 0 || index >= rewardCardViews.Length)
            {
                Debug.LogError($"index {index} is out of range");
                return;
            }
            rewardCardViews[index].gameObject.SetActive(card != null);
            if (card != null) rewardCardViews[index].SetRootDataModel(card);
        }
        public void SetSkipReward(int count)
        {
            skipRewardText.text = count.ToString();
        }
        public void SetMaxReward(int count)
        {
            maxRewardText.text = count.ToString();
        }
        public void SetCurReward(int count, bool isRare)
        {
            _oldCount = count;
            curRewardText.text = count.ToString();
            normalTitleRoot.SetActive(!isRare);
            rareTitleRoot.SetActive(isRare);
        }
        public async UniTask PlaySetCurRewardAnim(int count, bool isRare, CancellationToken token)
        {
            if (count == 0)
            {
                _oldCount = 0;
                return;
            }
            var old = _oldCount;
            _oldCount = count;
            normalTitleRoot.SetActive(!isRare);
            rareTitleRoot.SetActive(isRare);
            await BattleAnimSystem.PlayNumberChangeAnim(token, curRewardText, old, count);
        }
        public async UniTask PlayShowRewardAnimAsync(CancellationToken token)
        {
            foreach (var view in rewardCardViews)
                view.PlayShowRewardAnim(token).Forget();
            await UniTask.WaitForSeconds(0.1f, cancellationToken: token);
        }
        public async UniTask PlayHideRewardAnimAsync(CancellationToken token, int selected = -1)
        {
            for (var i = 0; i < rewardCardViews.Length; i++)
            {
                if (i == selected) rewardCardViews[i].PlayHideChosenRewardAnim(token).Forget();
                else rewardCardViews[i].PlayHideRewardAnim(token).Forget();
            }
            await UniTask.WaitForSeconds(0.1f, cancellationToken: token);
        }
    }
}