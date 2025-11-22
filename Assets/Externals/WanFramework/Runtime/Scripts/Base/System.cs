//    ▄▀▀▀▄▄▄▄▄▄▄▀▀▀▄
//    █▒▒░░░░░░░░░▒▒█    System.cs
//     █░░█░░░░░█░░█     Created by WanNeng
//  ▄▄  █░░░▀█▀░░░█  ▄▄  Created   12/24/2023 10:31
// █░░█ ▀▄░░░░░░░▄▀ █░░█

using System;
using Cysharp.Threading.Tasks;
using UnityEngine;
using WanFramework.Utils;

namespace WanFramework.Base
{
    public interface ISystem
    {
        /// <summary>
        /// 初始化系统，在游戏Awake后调用
        /// </summary>
        public UniTask Init();
    }
    
    /// <summary>
    /// 游戏由多个System组成
    /// </summary>
    public abstract class SystemBase<T> : SingletonBehaviour<T>, ISystem where T : SystemBase<T>
    {
        public virtual UniTask Init()
        {
            return UniTask.CompletedTask;
        }
    }

    /// <summary>
    /// 系统的刷新优先度，默认为0，越高越先执行
    /// </summary>
    [AttributeUsage(AttributeTargets.Class)]
    public class SystemPriorityAttribute : Attribute
    {
        public int priority;
        public SystemPriorityAttribute(int p)
        {
            priority = p;
        }
    }

    public static class SystemPriorities
    {
        public const int Resource = 4000;
        public const int DataSystem = 3000;
        public const int StateMachine = 2000;
        public const int UI = 1000;
    }
}