using System;
using UnityEngine.Events;

namespace WanFramework.UI
{
    public static class EventCenter<T> where T : struct
    {
        private static readonly UnityEvent<T> Evt = new();
        
        public static void AddListener(UnityAction<T> listener) => Evt.AddListener(listener);
        public static void RemoveListener(UnityAction<T> listener) => Evt.RemoveListener(listener);
        public static void Raise(T args) => Evt.Invoke(args);
    }
}