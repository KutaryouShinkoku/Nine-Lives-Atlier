using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

namespace WanFramework.Utils
{
    /// <summary>
    /// Animator事件路由
    /// </summary>
    [RequireComponent(typeof(Animator))]
    public class AnimatorEventRoute : MonoBehaviour
    {
        private Animator _animator;
        private readonly Dictionary<string, List<Action>> _listenerDict = new();

        private void Awake()
        {
            _animator = GetComponent<Animator>();
            _animator.fireEvents = true;
        }
        
        private void OnAnimationEvent(string eventName)
        {
            if (_listenerDict.TryGetValue(eventName, out var listeners))
                foreach (var action in listeners)
                    action?.Invoke();
        }

        /// <summary>
        /// 注册动画事件监听器
        /// </summary>
        /// <param name="eventName"></param>
        /// <param name="action"></param>
        public void RegisterListener(string eventName, Action action)
        {
            if (!_listenerDict.TryGetValue(eventName, out var listeners))
            {
                listeners = new();
                _listenerDict[eventName] = listeners;
            }
            listeners.Add(action);
        }
        
        /// <summary>
        /// 解绑事件监听
        /// </summary>
        /// <param name="eventName"></param>
        /// <param name="action"></param>
        public void UnregisterListener(string eventName, Action action)
        {
            if (_listenerDict.TryGetValue(eventName, out var listeners))
                listeners.Remove(action);
        }
    }
}