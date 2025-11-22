using System;
using System.Linq;
using System.Threading;
using Cysharp.Threading.Tasks;
using UnityEngine;
using WanFramework.Utils;

namespace Game.UI.InGame.Components
{
    public partial class HandCardsCollectionView
    {
        /// <summary>
        /// 波形震动动画
        /// </summary>
        /// <param name="token"></param>
        /// <param name="time">动画时长</param>
        /// <param name="amplitude">震动幅值</param>
        /// <param name="frequency">震动频率（指相邻卡牌之间的震动相位差）</param>
        /// <param name="tScale">时间缩放</param>
        /// <param name="shrink">是否按照时间收缩幅值</param>
        /// <param name="center">中心位置（归一化坐标）</param>
        public async UniTask PlayWaveShakeCardsAnim(CancellationToken token, float time, float amplitude, float frequency, float tScale, bool shrink, float center = 0.5f)
        {
            for (var t = 0f; t < time; t += Time.deltaTime)
            {
                var totalWidth = _layoutInfos.Where(layout => layout.View.IsVisible).Sum(layout => layout.ElementWidth);
                var curX = totalWidth * (center - 1);
                for (var i = 0; i < _layoutInfos.Count; ++i)
                {
                    if (_layoutInfos.GetRef(i).IgnoreLayout ||
                        _layoutInfos.GetRef(i).IgnoreAnim) continue;
                    if (i != 0)
                        curX += _layoutInfos[i - 1].ElementWidth / 2;
                    curX += _layoutInfos[i].ElementWidth / 2;
                    var yOffset = amplitude * MathF.Sin(MathF.Abs(2 * MathF.PI * frequency * curX) + t * tScale);
                    if (shrink)
                        yOffset *= (time - t) / time;
                    _layoutInfos.GetRef(i).TargetPositionAnimOffset.y = yOffset;
                }
                await UniTask.NextFrame(token);
            }
        }

        public async UniTask PlayExplosionAnim(CancellationToken token, Vector3 position, float time, float explosionPower, float maxDistance)
        {
            for (var t = 0f; t < time; t += Time.deltaTime)
            {
                for (var i = 0; i < _layoutInfos.Count; ++i)
                {
                    if (_layoutInfos.GetRef(i).IgnoreLayout ||
                        _layoutInfos.GetRef(i).IgnoreAnim) continue;
                    var offset = position - _layoutInfos[i].TargetPosition;
                    var distance = offset.magnitude;
                    var power = Mathf.Clamp(maxDistance - distance, 0, maxDistance) / maxDistance;
                    power *= explosionPower;
                    power *= (time - t) / time;
                    _layoutInfos.GetRef(i).TargetPositionAnimOffset -= offset * power;
                }
                await UniTask.NextFrame(token);
            }
        }
    }
}