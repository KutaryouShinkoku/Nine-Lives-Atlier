//    ▄▀▀▀▄▄▄▄▄▄▄▀▀▀▄
//    █▒▒░░░░░░░░░▒▒█    SequencePlayingSystem.cs
//     █░░█░░░░░█░░█     Created by WanNeng
//  ▄▄  █░░░▀█▀░░░█  ▄▄  Created   01/14/2024 19:15
// █░░█ ▀▄░░░░░░░▄▀ █░░█

using System;
using System.Collections.Generic;
using UnityEngine;
using WanFramework.Base;

namespace WanFramework.Sequence
{
    public struct SequenceInfo
    {
        public string Name;
        public ISequencePlaying Playing;
        public Behaviour Owner;
        public float Percent;
        public Action Callback;
    }
    
    /// <summary>
    /// Sequence是一个简单的序列播放系统，可支持多个动画的顺序播放、事件执行与等待、计时等
    /// </summary>
    public class SequenceSystem : SystemBase<SequenceSystem>
    {
        private const int DefaultCapacity = 100;

        /// <summary>
        /// 当前序列集合
        /// </summary>
        private readonly HashSet<ISequencePlaying> _currentSequenceSet = new(DefaultCapacity);
        
        /// <summary>
        /// 当前正在播放的序列
        /// </summary>
        private readonly List<ISequencePlaying> _currentSequence = new(DefaultCapacity);

        /// <summary>
        /// 当前播放sequence的回调函数
        /// </summary>
        private readonly List<Action> _currentSequenceCallback = new(DefaultCapacity);
        
        /// <summary>
        /// 记录所属Behaviour
        /// </summary>
        private readonly List<Behaviour> _currentSequenceOwner = new(DefaultCapacity);

        public bool GetSequenceInfo(int i, ref SequenceInfo info)
        {
            if (i >= _currentSequence.Count)
            {
                info.Name = null;
                info.Playing = null;                
                info.Owner = null;
                info.Callback = null;
                info.Percent = 0;
                return false;
            }
            info.Name = $"Sequence {i}";
            info.Playing = _currentSequence[i];                
            info.Owner = _currentSequenceOwner[i];
            info.Callback = _currentSequenceCallback[i];
            info.Percent = _currentSequence[i].Runner.Percent;
            return true;
        }
        private bool TryAddSequence(ISequencePlaying sequence, Action callback, Behaviour owner)
        {
            if (!_currentSequenceSet.Add(sequence))
                return false;
            _currentSequence.Add(sequence);
            _currentSequenceCallback.Add(callback);
            _currentSequenceOwner.Add(owner);
            return true;
        }
        
        private void RemoveSequenceAt(int index)
        {
            _currentSequence.RemoveAt(index);
            _currentSequenceCallback.RemoveAt(index);
            _currentSequenceOwner.RemoveAt(index);
        }

        private void SetSequenceInactivateAt(int index)
        {
            // 已经被设置为Inactive了就不需要再次设置
            if (_currentSequence[index] == null) return;
            _currentSequence[index].OnExit();
            _currentSequence[index].Runner.Reset();
            _currentSequenceSet.Remove(_currentSequence[index]);
            _currentSequence[index] = null;
        }
        
        // ReSharper disable Unity.PerformanceAnalysis
        /// <summary>
        /// 播放序列对象
        /// </summary>
        /// <param name="sequencePlaying"></param>
        /// <param name="callback"></param>
        /// <param name="owner"></param>
        public void Play(ISequencePlaying sequencePlaying, Action callback, Behaviour owner)
        {
            if (sequencePlaying.IsPlaying)
            {
#if UNITY_EDITOR
                Debug.LogWarning("Sequence is already playing. Reset it.");
#endif
                sequencePlaying.OnExit();
                sequencePlaying.Runner.Reset();
            }
            sequencePlaying.OnStart();
            if (!TryAddSequence(sequencePlaying, callback, owner))
            {
#if UNITY_EDITOR
                Debug.LogWarning("Sequence is already exists. New callback will not be apply.");
#endif
            }
        }
        
        // ReSharper disable Unity.PerformanceAnalysis
        /// <summary>
        /// 停止序列对象
        /// </summary>
        /// <param name="sequencePlaying"></param>
        /// <param name="ignoreCallback"></param>
        public void Stop(ISequencePlaying sequencePlaying, bool ignoreCallback = false)
        {
            var index = _currentSequence.IndexOf(sequencePlaying);
            if (index == -1)
            {
                Debug.LogWarning("Sequence is already stopped");
                return;
            }
            var callback = _currentSequenceCallback[index];
            _currentSequenceCallback[index] = null;
            if (!ignoreCallback)
                callback?.Invoke();
            SetSequenceInactivateAt(index);
        }

        /// <summary>
        /// 停止所有属于Behaviour的序列
        /// </summary>
        /// <param name="owner"></param>
        /// <param name="ignoreCallback"></param>
        public void StopAll(Behaviour owner, bool ignoreCallback = false)
        {
            for (var i = 0; i < _currentSequenceOwner.Count; ++i)
                if (_currentSequenceOwner[i] == owner)
                {
                    // 是否调用回调待定，目前调用
                    var callback = _currentSequenceCallback[i];
                    _currentSequenceCallback[i] = null;
                    if (!ignoreCallback)
                        callback?.Invoke();
                    SetSequenceInactivateAt(i);
                }
        }
        
        /// <summary>
        /// 停止所有序列
        /// </summary>
        public void StopAll()
        {
            for (var i = 0; i < _currentSequenceOwner.Count; ++i)
                SetSequenceInactivateAt(i);
        }
        
        private void Update()
        {
            UpdateAllSequence();
        }
        
        private void UpdateAllSequence()
        {
            // 执行update并使停止的sequence失效
            for (var i = 0; i < _currentSequence.Count; ++i)
            {
                // 如果所属behaviour处于非enable状态，不更新
                var currentOwner = _currentSequenceOwner[i];
                var currentSequence = _currentSequence[i];
                if (currentOwner && currentOwner.isActiveAndEnabled)
                {
                    // 尝试播放（即MoveNext）
                    if (currentSequence != null &&
                        currentSequence.Runner.MoveNext())
                        continue;
                }
                
                var isActivateSequence = currentSequence != null;
                // 如果是不是无效的sequence（已经被移除），则需要调用回调，并且设置为无效队列
                if (isActivateSequence)
                {
                    var callback = _currentSequenceCallback[i];
                    _currentSequenceCallback[i] = null;
                    callback?.Invoke();
                    SetSequenceInactivateAt(i);
                }
            }
            // 移除所有null（失效的sequence）
            for (var i = 0; i < _currentSequence.Count; )
                if (_currentSequence[i] == null)
                    RemoveSequenceAt(i);
                else
                    ++i;
        }
    }

    public static class SequencePlayingExtension
    {
        [Obsolete("To play a sequence, an owner must be specified", true)]
        public static void Play(this ISequencePlaying sequencePlaying)
        {
            SequenceSystem.Instance.Play(sequencePlaying, null, null);
        }
        
        public static void Play(this ISequencePlaying sequencePlaying, Action callback, Behaviour owner)
        {
            SequenceSystem.Instance.Play(sequencePlaying, callback, owner);
        }
        
        public static void Play(this ISequencePlaying sequencePlaying, Behaviour owner)
        {
            SequenceSystem.Instance.Play(sequencePlaying, null, owner);
        }
        
        public static void Stop(this ISequencePlaying sequencePlaying, bool ignoreCallback = false)
        {
            SequenceSystem.Instance.Stop(sequencePlaying, ignoreCallback);
        }
        
        public static void PlaySequence(this Behaviour behaviour, ISequencePlaying sequencePlaying, Action callback)
        {
            SequenceSystem.Instance.Play(sequencePlaying, callback, behaviour);
        }
        
        public static void PlaySequence(this Behaviour behaviour, ISequencePlaying sequencePlaying)
        {
            SequenceSystem.Instance.Play(sequencePlaying, null, behaviour);
        }
        
        public static void StopAllSequence(this Behaviour behaviour, bool ignoreCallback = false)
        {
            SequenceSystem.Instance.StopAll(behaviour, ignoreCallback);
        }
    }
}