//    ▄▀▀▀▄▄▄▄▄▄▄▀▀▀▄
//    █▒▒░░░░░░░░░▒▒█    UISystem.cs
//     █░░█░░░░░█░░█     Created by WanNeng
//  ▄▄  █░░░▀█▀░░░█  ▄▄  Created   01/03/2024 19:28
// █░░█ ▀▄░░░░░░░▄▀ █░░█

using System;
using System.Collections.Generic;
using System.Linq;
using Cysharp.Threading.Tasks;
using UnityEngine;
using WanFramework.Base;
using WanFramework.Resource;
using WanFramework.Sequence;

namespace WanFramework.UI
{
    [SystemPriority(SystemPriorities.UI)]
    public class UISystem : SystemBase<UISystem>
    {
        public const string DefaultUIViewCategory = "UIView";
        
        [Tooltip("通用根节点（指3D View）")]
        public Transform commonViewRoot;
        
        [Tooltip("UI根节点")]
        public Transform uiRoot;
        
        [SerializeField]
        [Tooltip("UI相机")]
        private Camera uiCamera;
        public Camera UICamera => uiCamera;
        
        private readonly Dictionary<string, RootView> _viewCache = new();
        
        public RootView GetCommonView(string viewName, bool autoLoad = true)
        {
            if (_viewCache.TryGetValue(viewName, out var cached))
                return cached;
            if (autoLoad)
            {
                cached = LoadCommonView(viewName);
                cached.gameObject.SetActive(false);
            }
            else
                Debug.LogError($"View {viewName} not found in opened view cache");
            return cached;
        }
        public T GetCommonView<T>(string viewName) where T : RootView => GetCommonView(viewName) as T;
        public UIRootView GetUI(string viewName, bool autoLoad = true)
        {
            if (_viewCache.TryGetValue(viewName, out var cached))
                return (UIRootView)cached;
            if (autoLoad)
            {
                cached = LoadUI(viewName);
                cached.gameObject.SetActive(false);
            }
            else
                Debug.LogError($"View {viewName} not found in opened view cache");
            return (UIRootView)cached;
        }
        public T GetUI<T>(string viewName) where T : UIRootView => GetUI(viewName) as T;

        private static string GetPath(string viewName) => $"Content/UI/{viewName}.prefab";
        private RootView LoadCommonView(string viewName)
        {
            if (_viewCache.TryGetValue(viewName, out var cached))
                return cached;
            var prefab = ResourceSystem.Instance.Load<GameObject>(GetPath(viewName)).GetComponent<RootView>();
            if (prefab == null)
                throw new Exception($"Failed to load view {GetPath(viewName)}");
            var instance = Instantiate(prefab, commonViewRoot);
            instance.name = viewName.Replace("/", ".");
            _viewCache[viewName] = instance;
            return instance;
        }
        
        private RootView LoadUI(string viewName)
        {
            if (_viewCache.TryGetValue(viewName, out var cached))
                return cached;
            var prefab = ResourceSystem.Instance.Load<GameObject>(GetPath(viewName)).GetComponent<UIRootView>();
            var instance = Instantiate(prefab, uiRoot);
            instance.name = viewName.Replace("/", ".");
            _viewCache[viewName] = instance;
            return instance;
        }
        
        /// <summary>
        /// 弹出UI
        /// </summary>
        public UIRootView ShowUI(string viewName)
        {
            var instance = LoadUI(viewName) as UIRootView ?? throw new Exception($"Failed to load view {viewName}");
            /*#if UNITY_EDITOR
            if (!instance.gameObject.activeSelf)
                Debug.Log($"Show {viewName}");
            #endif*/
            instance.transform.SetAsLastSibling();
            instance.gameObject.SetActive(true);
            instance.OnShow();
            return instance;
        }

        public T ShowUI<T>(string viewName) where T: UIRootView
        {
            return ShowUI(viewName) as T;
        }

        public T ShowUI<T>(string viewName, Action<T> onShow = null) where T : UIRootView
        {
            var instance = ShowUI(viewName) as T;
            onShow?.Invoke(instance);
            return instance;
        }

        public void MoveToTop(string viewName)
        {
            var instance = LoadUI(viewName) ?? throw new Exception($"Failed to load view {viewName}");
            instance.transform.SetAsLastSibling();
        }
        
        /// <summary>
        /// 弹出Common View
        /// </summary>
        public RootView ShowCommonView(string viewName)
        {
            var instance = LoadCommonView(viewName) as RootView ?? throw new Exception($"Failed to load common view {viewName}");
            instance.transform.SetAsLastSibling();
            instance.gameObject.SetActive(true);
            instance.OnShow();
            return instance;
        }

        public T ShowCommonView<T>(string viewName) where T: RootView
        {
            return ShowCommonView(viewName) as T;
        }
        
        public bool IsShowing(string viewName)
            => _viewCache.TryGetValue(viewName, out var view) && view.isActiveAndEnabled;
        
        /// <summary>
        /// 隐藏UI
        /// </summary>
        public void Hide(RootView rootView)
        {
            rootView.OnHide();
            // TODO: 隐藏UI
            rootView.gameObject.SetActive(false);
            // 停止绑定在UI上的所有序列动画
            this.StopAllSequence();
        }
        
        public void Hide(string viewName)
        {
            if (_viewCache.TryGetValue(viewName, out var cached))
                Hide(cached);
            /*else
                Debug.LogWarning($"View {viewName} not found in cache while trying to fade");*/
        }
        
        public override async UniTask Init()
        {
            await base.Init();
            await ResourceSystem.Instance.PreloadAssetByLabelAsync(ResourceLabels.UI);
            GameManager.Current?.InitCover?.SetProgress(0.9f);
        }
        
        private void OnDisable()
        {
            var views = _viewCache.Values.ToArray();
            foreach (var view in views.Where(view => view))
                view.enabled = false;
        }
    }
}