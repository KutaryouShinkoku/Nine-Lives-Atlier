using System;
using System.Threading;
using Cysharp.Threading.Tasks;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Events;
using Game.Audio;
using Game.Data;
using Game.Localization.Components;
using Game.Logic;
using Game.Model;
using WanFramework.Data;
using WanFramework.UI;
using WanFramework.UI.DataComponent;
using WanFramework.Utils;
using System.Collections.Generic;

namespace Game.UI.InGame
{
    public class InGameUIChooseLevelView : UIRootView
    {
        [SerializeField] private LocalizeText textTitle;
        [SerializeField] private LocalizeText textDescription;

        [SerializeField] private GameObject[] rodeArrows;

        [SerializeField] private Button buttonExp;
        [SerializeField] private Button buttonEnlight;
        [SerializeField] private Button buttonBoss;

        [SerializeField] private Image lockExp;
        [SerializeField] private Image lockEnlight;
        [SerializeField] private Image lockBoss;

        [SerializeField] private Image hint1;
        [SerializeField] private Image hint2;
        [SerializeField] private Image hint3;

        [SerializeField] private LocalizeText textButtonTitle;
        [SerializeField] private LocalizeText textButtonAreaEffect;

        public UnityEvent onSelectLevel;

        [SerializeField]
        private List<UniAnimation> levelActiveAnim = new List<UniAnimation>();

        [SerializeField]
        private List<UniAnimation> levelNormalAnim = new List<UniAnimation>();

        private CancellationTokenSource _animCts;

        [Header("显示和隐藏选关界面动画")]
        [SerializeField]
        private UniAnimation showAnim;
        [SerializeField]
        private UniAnimation hideAnim;

        protected override void InitComponents()
        {
            base.InitComponents();
            Bind(nameof(InGameModel.CurrentLevel), m => OnLevelChanged(m.As<InGameModel>()));
            Bind(nameof(InGameModel.LevelInnerState), m => OnLevelChanged(m.As<InGameModel>()));
            buttonExp.onClick.AddListener(() => onSelectLevel?.Invoke());
            buttonEnlight.onClick.AddListener(() => onSelectLevel?.Invoke());
            buttonBoss.onClick.AddListener(() => onSelectLevel?.Invoke());
        }

        private void OnLevelChanged(InGameModel inGameModel)
        {
            if (inGameModel == null) return;
            var levelData = inGameModel.CurrentLevel.Data();
            textTitle.SetText(levelData.Name);
            textDescription.SetText(levelData.Desc);
            textButtonTitle.SetText(levelData.Name);
            textButtonAreaEffect.SetText(levelData.BossDesc);

            // 更新进度箭头显示
            for (var lp = 0; lp < rodeArrows.Length;)
            {
                if (!LevelLogic.HasState((LevelInnerState)lp))
                {
                    rodeArrows[lp++].SetActive(false);
                    continue;
                }
                var rp = lp + 1;
                while (!LevelLogic.HasState((LevelInnerState)rp) &&
                       rp <= rodeArrows.Length &&
                       (LevelInnerState)rp != LevelInnerState.NextLevel)
                {
                    if (rp < rodeArrows.Length) rodeArrows[rp].SetActive(false);
                    rp++;
                }
                rodeArrows[lp].SetActive((LevelInnerState)rp != LevelInnerState.NextLevel);
                lp = rp;
            }

            // 按钮及锁图像设置
            buttonExp.gameObject.SetActive(LevelLogic.HasState(LevelInnerState.Battle));
            buttonEnlight.gameObject.SetActive(LevelLogic.HasState(LevelInnerState.Shop));
            buttonBoss.gameObject.SetActive(LevelLogic.HasState(LevelInnerState.Boss));

            switch (inGameModel.LevelInnerState)
            {
                case LevelInnerState.Battle:
                    buttonExp.interactable = true;
                    buttonEnlight.interactable = false;
                    buttonBoss.interactable = false;
                    lockExp.gameObject.SetActive(false);
                    lockEnlight.gameObject.SetActive(true);
                    lockBoss.gameObject.SetActive(true);

                    //设置提示
                    hint1.gameObject.SetActive(true);
                    hint2.gameObject.SetActive(false);
                    hint3.gameObject.SetActive(false);
                    break;
                case LevelInnerState.Shop:
                    buttonExp.interactable = false;
                    buttonEnlight.interactable = true;
                    buttonBoss.interactable = false;
                    lockExp.gameObject.SetActive(true);
                    lockEnlight.gameObject.SetActive(false);
                    lockBoss.gameObject.SetActive(true);

                    //设置提示
                    hint1.gameObject.SetActive(false);
                    hint2.gameObject.SetActive(false);
                    hint3.gameObject.SetActive(true);
                    break;
                case LevelInnerState.Boss:
                    buttonExp.interactable = false;
                    buttonEnlight.interactable = false;
                    buttonBoss.interactable = true;
                    lockExp.gameObject.SetActive(true);
                    lockEnlight.gameObject.SetActive(true);
                    lockBoss.gameObject.SetActive(false);

                    //设置提示
                    hint1.gameObject.SetActive(false);
                    hint2.gameObject.SetActive(true);
                    hint3.gameObject.SetActive(false);
                    break;
                default:
                    buttonExp.interactable = false;
                    buttonEnlight.interactable = false;
                    buttonBoss.interactable = false;
                    lockExp.gameObject.SetActive(false);
                    lockEnlight.gameObject.SetActive(false);
                    lockBoss.gameObject.SetActive(false);
                    hint1.gameObject.SetActive(false);
                    hint2.gameObject.SetActive(false);
                    hint3.gameObject.SetActive(false);
                    break;
            }

            ChangeLevelActiveAnim(inGameModel).Forget();
        }

        private async UniTask ChangeLevelActiveAnim(InGameModel inGameModel)
        {
            // 取消之前的动画
            _animCts?.Cancel();
            _animCts = new CancellationTokenSource();
            var token = _animCts.Token;

            // 重置所有 trigger
            foreach (var anim in levelActiveAnim) anim.Cancel();
            foreach (var anim in levelNormalAnim) anim.Cancel();

            int activeIndex;
            switch (inGameModel.CurrentLevel)
            {
                case LevelIds.Level1:
                    activeIndex = 0;
                    break;
                case LevelIds.Level2:
                    activeIndex = 1;
                    break;
                case LevelIds.Level3:
                    activeIndex = 2;
                    break;
                case LevelIds.Level4:
                    activeIndex = 3;
                    break;
                case LevelIds.Level5:
                    activeIndex = 4;
                    break;
                default:
                    activeIndex = -1;
                    break;
            }

            // 给所有非当前关卡的图标播放 normal 动画
            for (int i = 0; i < levelNormalAnim.Count; i++)
            {
                if (i != activeIndex)
                    levelNormalAnim[i].Play(token).Forget();
            }

            // 播放当前关卡的 active 动画
            if (activeIndex >= 0 && activeIndex < levelActiveAnim.Count)
                await levelActiveAnim[activeIndex].Play(token);
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
