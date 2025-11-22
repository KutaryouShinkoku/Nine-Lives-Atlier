//    ▄▀▀▀▄▄▄▄▄▄▄▀▀▀▄
//    █▒▒░░░░░░░░░▒▒█    IStateMachine.cs
//     █░░█░░░░░█░░█     Created by WanNeng
//  ▄▄  █░░░▀█▀░░░█  ▄▄  Created   01/11/2024 16:49
// █░░█ ▀▄░░░░░░░▄▀ █░░█

using System;
using System.Collections.Generic;
using UnityEngine;

namespace WanFramework.SM
{
    /// <summary>
    /// 状态机
    /// </summary>
    public interface IStateMachine
    {
        public IState CurrentState { get; }

        /// <summary>
        /// 进入状态
        /// </summary>
        public void EnterState<T>() where T : IState;

        /// <summary>
        /// 初始化状态机，每次加入到状态机系统时调用
        /// </summary>
        public void StateMachineInit();
        
        /// <summary>
        /// 销毁状态机，非销毁对象，每次从状态机系统中移除时调用
        /// </summary>
        public void StateMachineDestroy();
    }
    
    /// <summary>
    /// 状态机
    /// </summary>
    public abstract class StateMachine<T> : MonoBehaviour, IStateMachine
        where T : StateMachine<T>
    {
        // 记录已经进入的状态
        private readonly HashSet<int> _enteredStateSet = new();
        
        // 当前状态Id
        private int _currentStateId;
        private IState _currentState;

        public IState CurrentState
        {
            get
            {
                if (_currentStateId == 0)
                    _currentState = null;
                else if (_currentState == null || _currentState.Id != _currentStateId)
                    _currentState = StateMachineSystem.Instance.GetStateById(_currentStateId);
                return _currentState;
            }
        }
        void IStateMachine.EnterState<TState>()
        {
            if (_currentStateId > 0)
                CurrentState.OnExit(this, State<TState>.Instance);
            if (_enteredStateSet.Add(State<TState>.Id))
                State<TState>.OnInit(this);
            var previousState = CurrentState;
            _currentStateId = State<TState>.Id;
            State<TState>.OnEnter(this, previousState);
        }

        public void EnterState<TState>() where TState : StateBehaviour<T>
        {
            ((IStateMachine)this).EnterState<TState>();
        }

        public virtual void StateMachineInit()
        {
        }
        
        public virtual void StateMachineDestroy()
        {
        }
        
        protected virtual void OnEnable()
        {
            StateMachineSystem.Instance.AddStateMachine(this);
        }

        protected virtual void OnDisable()
        {
            StateMachineSystem.Instance.RemoveStateMachine(this);
        }
    }
}