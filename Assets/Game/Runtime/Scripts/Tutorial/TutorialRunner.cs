using System;
using System.Threading;
using Cysharp.Threading.Tasks;
using Game.BattleAnim;
using Game.Data;
using Game.Model;
using Game.Model.InGameSubModel;
using Game.UI.InGame;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.Utilities;
using WanFramework.UI;
using WanFramework.UI.DataComponent;

namespace Game.Tutorial
{
    public partial class TutorialRunner
    {
        private CancellationToken _ct;
        
        private TutorialIds _cur;
        private TutorialIds _next;
        private bool _hasDrawCard = false;
        private int _lastUseCardIndex = -1;
        private int _lastSacrificeCardIndex = -1;

        public UnityEvent OnFinished = new();
        
        public bool IsRunning { get; private set; }
        
        public void OnPlayerDrawCard() => _hasDrawCard = true;
        public void OnPlayerUseCard(int index)
        {
            if (_lastUseCardIndex == -1) _lastUseCardIndex = index;
        }
        public void OnPlayerSacrificeCard(int index)
        {
            if (_lastSacrificeCardIndex == -1) _lastSacrificeCardIndex = index;
        }
        public void Run(TutorialIds beg, CancellationToken token) => RunAsync(beg, token).Forget();
        public async UniTask RunAsync(TutorialIds beg, CancellationToken token)
        {
            Debug.Log($"Running Tutorial {beg}");
            try
            {
                if (IsRunning) throw new Exception("教学在跑了在跑了，你先Cancel");
                _cur = beg;
                _next = _cur + 1;
                while (true)
                {
                    _cur = _next;
                    _next = _cur + 1;
                    if (_cur.Data().IsEnd)
                    {
                        IsRunning = false;
                        break;
                    }
                    var tutorial = _cur.Data();
                    await ShowMessage(tutorial.Message, tutorial.MessageSlot, tutorial.VfxPrefab, token);
                    if (tutorial.ActionDelay > 0)
                        await UniTask.WaitForSeconds(tutorial.ActionDelay, cancellationToken: token);
                    await HandleAction(token, tutorial.Action, tutorial.ActionArgs);
                    ShowMessageTail();
                    BeginHandleCondition(tutorial.Condition, tutorial.ConditionArgs);
                    await HandleCondition(token, tutorial.Condition, tutorial.ConditionArgs);
                }
                var tutorialView = UISystem.Instance.GetUI<InGameUITutorialCover>("InGame/UITutorialCover");
                tutorialView.ShowVfx(null);
                UISystem.Instance.Hide(tutorialView);
                OnFinished?.Invoke();
            }
            catch (OperationCanceledException)
            {
                Debug.LogWarning($"Cancelled running Tutorial {beg}");
                var tutorialView = UISystem.Instance.GetUI<InGameUITutorialCover>("InGame/UITutorialCover");
                tutorialView.ShowVfx(null);
                UISystem.Instance.Hide(tutorialView);
            }
            catch (Exception e)
            {
                Debug.LogException(e);
            }
        }
        
        private async UniTask ShowMessage(LocalizeIds msg, int slot, string vfxPath, CancellationToken token)
        {
            if (msg == LocalizeIds.Empty) 
                UISystem.Instance.Hide("InGame/UITutorialCover");
            else
            {
                var tutorialView = UISystem.Instance.ShowUI<InGameUITutorialCover>("InGame/UITutorialCover");
                tutorialView.SetTailVisible(false);
                tutorialView.SetMessage(msg, slot);
                tutorialView.ShowVfx(vfxPath);
                await tutorialView.PlayMessagePrinterAnim(token);
            }
        }

        private void ShowMessageTail()
        {
            if (UISystem.Instance.IsShowing("InGame/UITutorialCover"))
                UISystem.Instance.GetUI<InGameUITutorialCover>("InGame/UITutorialCover").SetTailVisible(true);
        }
    }
}