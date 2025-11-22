using System;
using System.Collections.Generic;
using System.Linq;

namespace Game.Logic
{
    public static class Algorithm
    {
        public static void Roulette(ReadOnlySpan<int> weights, int count, int outCount, IList<int> outList)
        {
            while (outList.Count < Math.Min(outCount, count))
            {
                var totalWeight = 0;
                for (var i = 0; i < Math.Min(weights.Length, count); ++i)
                {
                    if (outList.Contains(i)) continue;
                    totalWeight += weights[i];
                }
                var curWeight = 0;
                var skillRand = UnityEngine.Random.Range(0, totalWeight) + 1;
                var hasAdd = false;
                for (var i = 0; i < Math.Min(weights.Length, count); ++i)
                {
                    if (outList.Contains(i)) continue;
                    curWeight += weights[i];
                    if (curWeight < skillRand) continue;
                    outList.Add(i);
                    hasAdd = true;
                    break;
                }
                if (!hasAdd) outList.Add(Math.Min(weights.Length, count));
            }
        }
        /// <summary>
        /// 轮盘赌算法
        /// </summary>
        /// <param name="weights">每一项的权重</param>
        /// <param name="count">总数</param>
        /// <returns>随机索引</returns>
        public static int Roulette(ReadOnlySpan<int> weights, int count)
        {
            var totalWeight = 0;
            for (var i = 0; i < Math.Min(weights.Length, count); ++i)
                totalWeight += weights[i];
            var curWeight = 0;
            var skillRand = UnityEngine.Random.Range(0, totalWeight) + 1;
            for (var i = 0; i < Math.Min(weights.Length, count); ++i)
            {
                curWeight += weights[i];
                if (curWeight < skillRand) continue;
                return i;
            }
            return Math.Min(weights.Length, count);
        }
        /// <summary>
        /// 轮盘赌算法
        /// </summary>
        /// <param name="weights">每一项的权重</param>
        /// <param name="count">总数</param>
        /// <returns>随机索引</returns>
        public static int Roulette(IReadOnlyList<int> weights, int count)
        {
            var totalWeight = 0;
            for (var i = 0; i < Math.Min(weights.Count, count); ++i)
                totalWeight += weights[i];
            var curWeight = 0;
            var skillRand = UnityEngine.Random.Range(0, totalWeight) + 1;
            for (var i = 0; i < Math.Min(weights.Count, count); ++i)
            {
                curWeight += weights[i];
                if (curWeight < skillRand) continue;
                return i;
            }
            return Math.Min(weights.Count, count);
        }
        public static T Roulette<T>(ReadOnlySpan<T> array, ReadOnlySpan<int> weight)
            => array[Roulette(weight, array.Length)];
        public static T Roulette<T>(IReadOnlyList<T> array, IReadOnlyList<int> weight)
            => array[Roulette(weight, array.Count)];
    }
}