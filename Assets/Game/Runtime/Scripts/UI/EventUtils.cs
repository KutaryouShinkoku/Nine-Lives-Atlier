using UnityEngine;
using UnityEngine.EventSystems;

namespace Game.UI
{
    public static class EventUtils
    {
        /// <summary>
        /// 向上冒泡传递事件，如希望在某些情况下不响应某些事件，则使用该方法
        /// </summary>
        /// <param name="evtHandler">ExecuteEvents.xxxxxxx</param>
        /// <param name="eventData">EventData</param>
        /// <typeparam name="THandler"></typeparam>
        /// <typeparam name="TData"></typeparam>
        public static void BubblingEvent<THandler, TData>(GameObject go,  ExecuteEvents.EventFunction<THandler> evtHandler, TData eventData)
            where THandler : IEventSystemHandler
            where TData : BaseEventData
        {
            if (go.transform.parent == null) return;
            ExecuteEvents.ExecuteHierarchy(
                go.transform.parent.gameObject,
                eventData,
                evtHandler
                );
        }
    }
}