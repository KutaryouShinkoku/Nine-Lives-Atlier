//    ▄▀▀▀▄▄▄▄▄▄▄▀▀▀▄
//    █▒▒░░░░░░░░░▒▒█    StateMachineSystem.cs
//     █░░█░░░░░█░░█     Created by WanNeng
//  ▄▄  █░░░▀█▀░░░█  ▄▄  Created   01/11/2024 16:51
// █░░█ ▀▄░░░░░░░▄▀ █░░█

using System;
using System.Collections.Generic;
using Cysharp.Threading.Tasks;
using WanFramework.Base;

namespace WanFramework.SM
{
    [SystemPriority(SystemPriorities.StateMachine)]
    public class StateMachineSystem : SystemBase<StateMachineSystem>
    {
        private int _currentStateId;
        private readonly Dictionary<int, IState> _stateDict = new();
        private readonly List<IStateMachine> _stateMachines = new();

        public IEnumerable<IStateMachine> GetMachines()
        {
            return _stateMachines;
        }
        
        public IEnumerable<IState> GetStates()
        {
            return _stateDict.Values;
        }
        
        /// <summary>
        /// 请求state id
        /// </summary>
        /// <returns></returns>
        public IState CreateState<T>() where T : IState
        {
            var id = ++_currentStateId;
            var state = (IState)gameObject.AddComponent(typeof(T));
            state.Id = id;
            _stateDict[id] = state;
            return state;
        }

        public IState GetStateById(int id)
        {
            return id <= 0 ? null : _stateDict[id];
        }

        public void AddStateMachine(IStateMachine machine)
        {
            _stateMachines.Add(machine);
            machine.StateMachineInit();
        }
        
        public void RemoveStateMachine(IStateMachine machine)
        {
            _stateMachines.Remove(machine);
            machine.StateMachineDestroy();
        }

        private void Update()
        {
            foreach (var stateMachine in _stateMachines)
            {
                var currentState = stateMachine.CurrentState;
                currentState?.OnUpdate(stateMachine);
            }
        }

        public override UniTask Init()
        {
            return base.Init();
        }
    }
}