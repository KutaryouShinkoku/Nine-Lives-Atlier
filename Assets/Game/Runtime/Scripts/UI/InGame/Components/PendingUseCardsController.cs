using System;
using System.Collections.Generic;
using Game.SM.InGameState.BattleState;
using Game.UI.Common.Components;
using UnityEngine;
using WanFramework.UI;

namespace Game.UI.InGame.Components
{
    public class PendingUseCardsController : MonoBehaviour
    {
        // 使用卡牌的起始位置和偏移
        [SerializeField] private Transform useOrigin;
        [SerializeField] private Vector3 useOffset = new(3, -3, 0);

        // 献祭卡牌的起始位置和偏移
        [SerializeField] private Transform sacrificeOrigin;
        [SerializeField] private Vector3 sacrificeOffset = new(-3, -3, 0);

        private readonly List<int> _useCardIndices = new();
        private readonly List<int> _sacrificeCardIndices = new();

        public void AddCardView(int index, PlayerInteractionType interactionType)
        {
            switch (interactionType)
            {
                case PlayerInteractionType.UseCard:
                    _useCardIndices.Add(index);
                    break;
                case PlayerInteractionType.SacrificeCard:
                    _sacrificeCardIndices.Add(index);
                    break;
            }
        }

        public void RemoveCardView(int index)
        {
            _useCardIndices.Remove(index);
            _sacrificeCardIndices.Remove(index);
        }

        public void RemoveAll()
        {
            _useCardIndices.Clear();
            _sacrificeCardIndices.Clear();
        }

        private void Update()
        {
            var handAreaView = UISystem.Instance.GetCommonView<InGameHandAreaView>("InGame/HandAreaView");

            // 处理使用卡牌队列
            for (var i = 0; i < _useCardIndices.Count; i++)
            {
                var index = _useCardIndices[i];
                handAreaView.SetTargetPosition(
                    index,
                    useOrigin.position + useOffset * i
                );
                handAreaView.GetCardView(index).transform.SetSiblingIndex(0);
            }

            // 处理献祭卡牌队列
            for (var i = 0; i < _sacrificeCardIndices.Count; i++)
            {
                var index = _sacrificeCardIndices[i];
                handAreaView.SetTargetPosition(
                    index,
                    sacrificeOrigin.position + sacrificeOffset * i
                );
                handAreaView.GetCardView(index).transform.SetSiblingIndex(0);
            }
        }

        private void OnDrawGizmosSelected()
        {
            const float size = 80;
            // 绘制使用卡牌区域
            DrawGizmosForType(useOrigin, useOffset);
            // 绘制献祭卡牌区域
            DrawGizmosForType(sacrificeOrigin, sacrificeOffset);

            void DrawGizmosForType(Transform origin, Vector3 offset)
            {
                if (origin == null) return;
                Gizmos.DrawWireCube(origin.position, new Vector3(size, 100, 0));
                Gizmos.DrawWireCube(origin.position + offset, new Vector3(size, 100, 0));
                Gizmos.DrawWireCube(origin.position + offset * 2, new Vector3(size, 100, 0));
            }
        }
    }
}