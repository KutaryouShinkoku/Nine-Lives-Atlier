//    ▄▀▀▀▄▄▄▄▄▄▄▀▀▀▄
//    █▒▒░░░░░░░░░▒▒█    Sequence.Playing.cs
//     █░░█░░░░░█░░█     Created by WanNeng
//  ▄▄  █░░░▀█▀░░░█  ▄▄  Created   03/09/2024 12:06
// █░░█ ▀▄░░░░░░░▄▀ █░░█

using System;
using UnityEngine;

namespace WanFramework.Sequence
{
    /// <summary>
    /// 序列Action内置刷新函数
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public partial class SequencePlaying<T>
    {
        public delegate TReturn Getter<out TReturn>(T context);
        public delegate void SequenceCallback(T context);
        
        private static void DefaultUpdateFunc(T context, out bool isFinished)
        {
            isFinished = true;
        }

        private static void DefaultEnterFunc(T context)
        {
        }
        
        private static void DefaultLeaveFunc(T context)
        {
        }

        /// <summary>
        /// 记录等待时间的Playing上下文
        /// </summary>
        private class WaitForSecondPlaying : IPlaying<T>
        {
            private readonly Getter<float> _secondGetter;
            private float _countdown = 0;
            public WaitForSecondPlaying(Getter<float> second)
            {
                _secondGetter = second;
            }
            
            public void OnUpdate(T context, out bool isFinished)
            {
                _countdown -= Time.deltaTime;
                isFinished = _countdown <= 0;
            }

            public void OnEnter(T context)
            {
                _countdown = _secondGetter(context) + Time.deltaTime;
            }

            public void OnExit(T context)
            {
            }
        }
        /// <summary>
        /// 等待指定时间
        /// </summary>
        /// <param name="second"></param>
        /// <returns></returns>
        protected static IPlaying<T> WaitForSecond(Getter<float> second)
        {
            return new WaitForSecondPlaying(second);
        }
        
        private class WaitForCallbackPlaying : IPlaying<T>
        {
            private readonly Action<T, Action> _func;
            private readonly Action _callback;
            private bool _shouldExit;
            public WaitForCallbackPlaying(Action<T, Action> func)
            {
                _func = func;
                _callback = () => _shouldExit = true;
            }
            
            public void OnUpdate(T context, out bool isFinished)
            {
                isFinished = _shouldExit;
            }

            public void OnEnter(T context)
            {
                _shouldExit = false;
                _func?.Invoke(context, _callback);
            }

            public void OnExit(T context)
            {
            }
        }
        /// <summary>
        /// 等待回调
        /// </summary>
        /// <param name="callback"></param>
        /// <returns></returns>
        protected static IPlaying<T> WaitForCallback(Action<T, Action> func)
        {
            return new WaitForCallbackPlaying(func);
        }
        
        /// <summary>
        /// 运行回调
        /// </summary>
        /// <param name="callback"></param>
        /// <returns></returns>
        protected static Playing<T> Run(SequenceCallback callback)
        {
            return new Playing<T>(
                DefaultUpdateFunc,
                EnterFunc,
                DefaultLeaveFunc);
            
            void EnterFunc(T context)
            {
                callback(context);
            }
        }
        
        /// <summary>
        /// 运行回调
        /// </summary>
        /// <param name="key"></param>
        /// <returns></returns>
        protected static Playing<T> WaitKey(KeyCode key)
        {
            return new Playing<T>(
                UpdateFunc,
                DefaultEnterFunc,
                DefaultLeaveFunc);
            
            void UpdateFunc(T context, out bool isFinished)
            {
                isFinished = Input.GetKeyDown(key);
            }
        }
        
        protected static Playing<T> WaitAnyKey()
        {
            return new Playing<T>(
                UpdateFunc,
                DefaultEnterFunc,
                DefaultLeaveFunc);
            
            void UpdateFunc(T context, out bool isFinished)
            {
                isFinished = Input.anyKeyDown;
            }
        }

        /// <summary>
        /// 可播放队列
        /// </summary>
        /// <param name="playings"></param>
        /// <returns></returns>
        protected static IPlaying<T> Queue(IPlaying<T>[] playings)
        {
            return new QueuePlaying<T>(playings);
        }
        
        /// <summary>
        /// UI淡入
        /// </summary>
        /// <param name="canvasGroup"></param>
        /// <param name="fadingSpeed"></param>
        /// <returns></returns>
        protected static IPlaying<T> FadeIn(Getter<CanvasGroup> canvasGroup, Getter<float> fadingSpeed)
        {
            return new Playing<T>(
                UpdateFunc,
                EnterFunc,
                DefaultLeaveFunc);

            void UpdateFunc(T context, out bool isFinished)
            {
                var currentCanvasGroup = canvasGroup(context);
                var currentAlpha = currentCanvasGroup.alpha;
                currentAlpha += Time.deltaTime * fadingSpeed(context);
                currentCanvasGroup.alpha = currentAlpha;
                isFinished = currentAlpha >= 1;
            }

            void EnterFunc(T context)
            {
                canvasGroup(context).alpha = 0;
            }
        }
        
        /// <summary>
        /// UI淡出
        /// </summary>
        /// <param name="canvasGroup"></param>
        /// <param name="fadingSpeed"></param>
        /// <returns></returns>
        protected static IPlaying<T> FadeOut(Getter<CanvasGroup> canvasGroup, Getter<float> fadingSpeed)
        {
            return new Playing<T>(
                UpdateFunc,
                EnterFunc,
                DefaultLeaveFunc);

            void UpdateFunc(T context, out bool isFinished)
            {
                var currentCanvasGroup = canvasGroup(context);
                var currentAlpha = currentCanvasGroup.alpha;
                currentAlpha -= Time.deltaTime * fadingSpeed(context);
                currentCanvasGroup.alpha = currentAlpha;
                isFinished = currentAlpha <= 0;
            }

            void EnterFunc(T context)
            {
                canvasGroup(context).alpha = 1;
            }
        }

        /// <summary>
        /// 播放动画
        /// </summary>
        /// <param name="anim"></param>
        /// <param name="animName"></param>
        /// <param name="lazePlayFrames"></param>
        /// <returns></returns>
        protected static IPlaying<T> PlayAnimation(Getter<Animation> anim, Getter<string> animName, Getter<int> lazePlayFrames = null)
        {
            var lazyFrame = -1;
            return new Playing<T>(
                UpdateFunc,
                EnterFunc,
                DefaultLeaveFunc);

            void UpdateFunc(T context, out bool isFinished)
            {
                if (lazyFrame > 0)
                {
                    --lazyFrame;
                    isFinished = false;
                    return;
                }
                var animInstance = anim(context);
                if (lazyFrame == 0)
                {
                    animInstance.Stop();
                    animInstance.Play(animName(context));
                    --lazyFrame;
                }
                isFinished = !animInstance.isPlaying;
            }

            void EnterFunc(T context)
            {
                var animInstance = anim(context);
                if (lazePlayFrames != null)
                {
                    lazyFrame = lazePlayFrames.Invoke(context);
                    if (lazyFrame >= 0)
                    {
                        animInstance.Stop();
                        animInstance.GetClip(animName(context)).SampleAnimation(animInstance.gameObject, 0);
                        return;
                    }
                }
                lazyFrame = -1;
                animInstance.Stop();
                animInstance.Play(animName(context));
            }
        }

        protected static IPlaying<T> Playing(
            Playing<T>.OnUpdateHandle update = null,
            Playing<T>.OnEnterHandle enter = null, 
            Playing<T>.OnLeaveHandle leave = null)
        {
            return new Playing<T>(update, enter, leave);
        }
    }

    /// <summary>
    /// 可播放队列
    /// </summary>
    /// <typeparam name="T"></typeparam>
    class QueuePlaying<T> : IPlaying<T> where T : SequencePlaying<T>
    {
        private IPlaying<T>[] _playings;
        private int _current;
        public QueuePlaying(IPlaying<T>[] playings)
        {
            _playings = playings;
        }
        public void OnUpdate(T context, out bool isFinished)
        {
            if (_playings.Length == 0)
            {
                isFinished = true;
                return;
            }
            isFinished = false;
            _playings[_current].OnUpdate(context, out var isSubFinished);
            if (!isSubFinished) return;
            _playings[_current].OnExit(context);
            ++_current;
            if (_current < _playings.Length)
                _playings[_current].OnEnter(context);
            else isFinished = true;
        }

        public void OnEnter(T context)
        {
            _current = 0;
            if (_playings.Length != 0)
                _playings[_current].OnEnter(context);
        }

        public void OnExit(T context)
        {
        }
    }
}