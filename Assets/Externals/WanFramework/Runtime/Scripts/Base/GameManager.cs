using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Runtime.CompilerServices;
using Cysharp.Threading.Tasks;
using JetBrains.Annotations;
using TMPro;
using UnityEngine;
using UnityEngine.LowLevel;
using WanFramework.SM;
using WanFramework.Utils;

namespace WanFramework.Base
{
    /// <summary>
    /// 游戏定义
    /// </summary>
    public sealed class GameManager : StateMachine<GameManager>
    {
        public static GameManager Current;

        private Application.LogCallback _crashHandle;
        
        [SerializeField]
        [CanBeNull]
        private InitCover initCover;
        [SerializeField]
        [CanBeNull]
        private CrashCover crashCover;
        [SerializeField]
        private Camera mainCamera;

        public InitCover InitCover => initCover;
        public Camera MainCamera => mainCamera;

        public bool IsCrash { get; private set; } = false;
        
        private ISystem[] _systems;
        
        private void Awake()
        {
            if (mainCamera == null)
                mainCamera = Camera.main;
            Current = this;
            SetupExceptionCatch();
            RunGame().Forget();
        }
        private void OnCrash(string msg, string trace)
        {
            try
            {
                crashCover?.gameObject.SetActive(true);
                if (!IsCrash)
                    crashCover?.AddCrashLog("STOP ALL GAME SYSTEM AND RESET PLAYER LOOP");
                crashCover?.AddCrashLog(msg, trace);
                if (!IsCrash)
                {
                    IsCrash = true;
                    var playerLoop = PlayerLoop.GetDefaultPlayerLoop();
                    PlayerLoop.SetPlayerLoop(playerLoop);
                    foreach (var system in _systems)
                        if (system is Component component)
                            component.gameObject.SetActive(false);
                }
            }
            catch (Exception e)
            {
                Debug.LogException(e);
            }
        }
        private void SetupExceptionCatch()
        {
            _crashHandle = (msg, trace, type) =>
            {
                if (type == LogType.Exception)
                    OnCrash(msg, trace);
            };
            if (crashCover == null) return;
            Application.logMessageReceived += _crashHandle;
        }
        private void DestroyExceptionCatch()
        {
            Application.logMessageReceived -= _crashHandle;
        }
        /// <summary>
        /// 系统初始化
        /// </summary>
        private async UniTask SetupSystem()
        {
            initCover?.SetCurrentState("Setup Game Systems");
            // load all system
            _systems = GetComponentsInChildren<ISystem>();
            Array.Sort(_systems, (a, b) =>
            {
                var aAttr = a.GetType().GetCustomAttributes(typeof(SystemPriorityAttribute)).FirstOrDefault();
                var bAttr = b.GetType().GetCustomAttributes(typeof(SystemPriorityAttribute)).FirstOrDefault();
                var aPriority = (aAttr as SystemPriorityAttribute)?.priority ?? 0;
                var bPriority = (bAttr as SystemPriorityAttribute)?.priority ?? 0;
                return bPriority - aPriority;
            });
            // init all system
            foreach (var system in _systems)
            {
                initCover?.SetCurrentState($"Setup System {system.GetType().Name}");
                await system.Init();
            }
        }

        private async UniTask RunGame()
        {
            initCover?.gameObject.SetActive(true);
            await SetupSystem();
            initCover?.SetCurrentState("Run Game");
            Current?.InitCover?.SetProgress(1.0f);
#if UNITY_ANDROID && !UNITY_EDITOR
            Application.targetFrameRate = (int)Screen.currentResolution.refreshRateRatio.value;
#endif
            if (GameEntryPoint.Instance == null)
            {
                Debug.LogError("A game entry point is needed to start the game");
                return;
            }
            try
            {
                initCover?.SetCurrentState("Enter Game Main");
                await GameEntryPoint.Instance.MainAsync();
            }
            catch (Exception e)
            {
                Debug.LogException(e);
            }
            initCover?.gameObject.SetActive(false);
        }
        private void OnDestroy()
        {
            DestroyExceptionCatch();
        }
    }
    
    /// <summary>
    /// 程序入口点
    /// </summary>
    public abstract class GameEntryPoint : SingletonBehaviour<GameEntryPoint>
    {
        public abstract UniTask MainAsync();
    }

    /// <summary>
    /// 游戏状态
    /// </summary>
    public class GameState : StateBehaviour<GameManager>
    {
    }
}
