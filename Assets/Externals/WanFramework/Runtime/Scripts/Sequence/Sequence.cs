//    ▄▀▀▀▄▄▄▄▄▄▄▀▀▀▄
//    █▒▒░░░░░░░░░▒▒█    SequencePlaying.cs
//     █░░█░░░░░█░░█     Created by WanNeng
//  ▄▄  █░░░▀█▀░░░█  ▄▄  Created   01/14/2024 19:16
// █░░█ ▀▄░░░░░░░▄▀ █░░█

using System;
using System.Collections;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.Serialization;

namespace WanFramework.Sequence
{
    /// <summary>
    /// 运行可播放队列的迭代器，每次Update调用MoveNext，类似协程但是需要显示创建类，并支持Reset
    /// </summary>
    internal interface ISequencePlayingRunner : IEnumerator
    {
        /// <summary>
        /// 播放百分比
        /// </summary>
        public float Percent { get; }
        
        /// <summary>
        /// 是否正在播放
        /// </summary>
        public bool IsPlaying { get; }
    }
    
    internal class SequencePlayingRunner<T> : ISequencePlayingRunner where T : SequencePlaying<T>
    {
        private bool _hasEnter;
        private int _currentStep;
        private readonly IPlaying<T>[] _playingArray;
        private readonly SequencePlaying<T> _playing;

        public float Percent => (float)_currentStep / _playingArray.Length;
        public bool IsPlaying { get; private set; }
        
        public SequencePlayingRunner(SequencePlaying<T> playing)
        {
            _playingArray = playing.GetPlayingArray();
            _playing = playing;
        }

        private bool InnerMoveNext()
        {
            if (_currentStep >= _playingArray.Length) return false;
            if (!_hasEnter)
            {
                _playingArray[_currentStep].OnEnter(_playing as T);
                _hasEnter = true;
            }
            _playingArray[_currentStep].OnUpdate(_playing as T, out var isFinished);
            if (isFinished)
            {
                _playingArray[_currentStep].OnExit(_playing as T);
                ++_currentStep;
                _hasEnter = false;
            }
            return _currentStep < _playingArray.Length;
        }

        public bool MoveNext()
        {
            IsPlaying = true;
            var result = false;
            try
            {
                result = InnerMoveNext();
            }
            catch (Exception e)
            {
                Debug.LogException(e);
                Reset();
            }

            if (!result)
                IsPlaying = false;
            return result;
        }
        
        public void Reset()
        {
            _currentStep = 0;
            _hasEnter = false;
            IsPlaying = false;
        }

        object IEnumerator.Current => null;
    }

    public interface ISequencePlaying
    {
        internal ISequencePlayingRunner Runner { get; }
        public bool IsPlaying { get; }

        /// <summary>
        /// 当退出播放调用，通常用于复位序列初始状态，注意无论是否正常结束序列，都会调用OnExit
        /// </summary>
        public void OnExit();

        /// <summary>
        /// 序列开始播放调用
        /// </summary>
        public void OnStart();
    }
    
    /// <summary>
    /// 序列播放，连接多个可播放对象，并对其进行控制，可被序列系统播放，同时为Playing对象提供数据上下文
    /// 此类型为可序列化类型，支持在编辑器中配置序列播放行为
    /// </summary>
    [Serializable]
    public abstract partial class SequencePlaying<T> : ISequencePlaying where T : SequencePlaying<T>
    {
        public bool IsPlaying => _runner.IsPlaying;
        private readonly IPlaying<T>[] _playingArray;
        private readonly SequencePlayingRunner<T> _runner;
        
        ISequencePlayingRunner ISequencePlaying.Runner => _runner;
        
        internal IPlaying<T>[] GetPlayingArray()
        {
            return _playingArray;
        }
        
        protected SequencePlaying(IPlaying<T>[] playing)
        {
            _playingArray = playing;
            _runner = new SequencePlayingRunner<T>(this);
        }

        public virtual void OnExit()
        {
        }
        public virtual void OnStart()
        {
        }
    }
    
    /// <summary>
    /// 播放动画序列
    /// </summary>
    [Serializable]
    public class PlayAnimationSequence : SequencePlaying<PlayAnimationSequence>
    {
        public Animation anim;
        public string animName;
        [Tooltip("在多少帧结束后播放动画")]
        public bool lazyLoad = false;
        
        public PlayAnimationSequence()
            : this(null, "")
        {
        }

        public PlayAnimationSequence(Animation anim, string animName) 
            : base(new[]
            {
                PlayAnimation(c => c.anim, c => c.animName, c => c.lazyLoad ? 1 : 0)
            })
        {
            this.anim = anim;
            this.animName = animName;
        }

        public override void OnExit()
        {
            base.OnExit();
            anim.Stop(animName);
            var clip = anim[animName].clip;
            clip.SampleAnimation(anim.gameObject, clip.length);
        }

        public void SampleFirstFrame()
        {
            var clip = anim[animName].clip;
            clip.SampleAnimation(anim.gameObject, 0);
        }
        
        public void SampleLastFrame()
        {
            var clip = anim[animName].clip;
            clip.SampleAnimation(anim.gameObject, clip.length);
        }
    }
    
    /// <summary>
    /// 播放动画序列
    /// </summary>
    [Serializable]
    public class WaitForSecondSequence : SequencePlaying<WaitForSecondSequence>
    {
        public float second;
        
        public WaitForSecondSequence()
            : this(0)
        {
        }
        
        public WaitForSecondSequence(float second) 
            : base(new[]
            {
                WaitForSecond(c => c.second)
            })
        {
            this.second = second;
        }
    }
}