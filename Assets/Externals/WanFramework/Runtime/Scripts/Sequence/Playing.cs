//    ▄▀▀▀▄▄▄▄▄▄▄▀▀▀▄
//    █▒▒░░░░░░░░░▒▒█    Playing.cs
//     █░░█░░░░░█░░█     Created by WanNeng
//  ▄▄  █░░░▀█▀░░░█  ▄▄  Created   01/14/2024 21:12
// █░░█ ▀▄░░░░░░░▄▀ █░░█

using System;
using UnityEngine;

namespace WanFramework.Sequence
{
    /// <summary>
    /// 可播放对象
    /// </summary>
    /// <typeparam name="T">数据上下文</typeparam>
    public interface IPlaying<in T>
    {
        public void OnUpdate(T context, out bool isFinished);
        public void OnEnter(T context);
        public void OnExit(T context);
    }

    public sealed class Playing<T> : IPlaying<T>
    {
        public delegate void OnUpdateHandle(T context, out bool isFinished);
        public delegate void OnEnterHandle(T context);
        public delegate void OnLeaveHandle(T context);

        void IPlaying<T>.OnUpdate(T context, out bool isFinished)
        {
            if (OnUpdate == null)
                isFinished = true;
            else
                OnUpdate.Invoke(context, out isFinished);
        }

        void IPlaying<T>.OnEnter(T context)
        {
            OnEnter?.Invoke(context);
        }
        
        void IPlaying<T>.OnExit(T context)
        {
            OnExit?.Invoke(context);
        }
        
        public OnUpdateHandle OnUpdate { get; }
        public OnEnterHandle OnEnter { get; }
        public OnLeaveHandle OnExit { get; }
        
        public Playing(OnUpdateHandle update, OnEnterHandle enter, OnLeaveHandle exit)
        {
            OnUpdate = update;
            OnEnter = enter;
            OnExit = exit;
        }
    }
}