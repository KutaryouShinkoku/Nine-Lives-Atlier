using System;
using System.Linq;
using System.Threading;
using Cysharp.Threading.Tasks;
using Game.Data;
using Game.Model;
using Game.UI.InGame;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.Utilities;
using WanFramework.Base;
using WanFramework.UI;
using WanFramework.UI.DataComponent;

namespace Game.Tutorial
{
    public partial class TutorialRunner
    {
        private void BeginHandleCondition(TutorialCondition condition, string[] args)
        {
            switch (condition)
            {
                case TutorialCondition.WaitForDrawCard:
                    _hasDrawCard = false;
                    break;
                case TutorialCondition.WaitForUseCard:
                    _lastUseCardIndex = -1;
                    break;
                case TutorialCondition.WaitForSacrificeCard:
                    _lastSacrificeCardIndex = -1;
                    break;
            }
        }

        private bool CheckIsCardSame(int index, CardIds cardId)
        {
            if (index == -1 || index >= DataModel<BattleModel>.Instance.HandCards.Count)
                return false;
            return cardId == DataModel<BattleModel>.Instance.HandCards[index].Id;
        }
        private bool HandleCondition(TutorialCondition condition, string[] args)
        {
            switch (condition)
            {
                case TutorialCondition.None:
                    return true;
                case TutorialCondition.AnyKey:
                    return InputHelper.AnyKeyUp();
                case TutorialCondition.WaitForDrawCard:
                    return _hasDrawCard;
                case TutorialCondition.WaitForUseCard:
                    if (_lastUseCardIndex == -1) return false;
                    var handAreaView = UISystem.Instance.GetCommonView<InGameHandAreaView>("InGame/HandAreaView");
                    handAreaView.SetIgnoreLayout(_lastUseCardIndex, true);
                    handAreaView.SetCardInteractable(_lastUseCardIndex, false);
                    return true;
                case TutorialCondition.WaitForSacrificeCard:
                    if (_lastSacrificeCardIndex == -1) return false;
                    handAreaView = UISystem.Instance.GetCommonView<InGameHandAreaView>("InGame/HandAreaView");
                    handAreaView.SetIgnoreLayout(_lastSacrificeCardIndex, true);
                    handAreaView.SetCardInteractable(_lastSacrificeCardIndex, false);
                    return true;
                case TutorialCondition.CheckSacrificeCardId:
                    if (args.Length != 1)
                    {
                        Debug.LogError("CheckSacrificeCardId需要1个卡牌Id参数");
                        return false;
                    }
                    if (!Enum.TryParse<CardIds>(args[0], out var sactificeCardId))
                    {
                        Debug.LogError($"{args[0]} 不是一个有效的卡牌Id");
                        return false;
                    }
                    return CheckIsCardSame(_lastSacrificeCardIndex, sactificeCardId);
                case TutorialCondition.CheckUseCardId:
                    if (args.Length != 1)
                    {
                        Debug.LogError("CheckUseCardId需要1个卡牌Id参数");
                        return false;
                    }
                    if (!Enum.TryParse<CardIds>(args[0], out var useCardId))
                    {
                        Debug.LogError($"{args[0]} 不是一个有效的卡牌Id");
                        return false;
                    }
                    return CheckIsCardSame(_lastUseCardIndex, useCardId);
                case TutorialCondition.CheckSacrificeCardIndex:
                    if (args.Length != 1)
                    {
                        Debug.LogError("CheckSacrificeCardIndex需要1个手牌Index参数");
                        return false;
                    }
                    return int.Parse(args[0]) == _lastSacrificeCardIndex;
                case TutorialCondition.CheckUseCardIndex:
                    if (args.Length != 1)
                    {
                        Debug.LogError("CheckUseCardIndex需要1个手牌Index参数");
                        return false;
                    }
                    return int.Parse(args[0]) == _lastUseCardIndex;
                default:
                    throw new ArgumentOutOfRangeException(nameof(condition), condition, null);
            }
        }
        
        private async UniTask HandleCondition(CancellationToken token, TutorialCondition condition, string[] args)
        {
            while (!HandleCondition(condition, args)) await UniTask.Yield(token);
            if (token.IsCancellationRequested) throw new OperationCanceledException();
        }
    }
}