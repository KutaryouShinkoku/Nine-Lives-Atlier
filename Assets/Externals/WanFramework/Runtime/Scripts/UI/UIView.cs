//    ▄▀▀▀▄▄▄▄▄▄▄▀▀▀▄
//    █▒▒░░░░░░░░░▒▒█    UIView.cs
//     █░░█░░░░░█░░█     Created by WanNeng
//  ▄▄  █░░░▀█▀░░░█  ▄▄  Created   01/06/2024 14:21
// █░░█ ▀▄░░░░░░░▄▀ █░░█

using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Threading;
using JetBrains.Annotations;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.EventSystems;
using UnityEngine.Pool;
using UnityEngine.Serialization;
using WanFramework.UI.DataComponent;
using Object = UnityEngine.Object;

namespace WanFramework.UI
{
    /// <summary>
    /// 视图基类
    /// </summary>
    public abstract class ViewBase : MonoBehaviour
    {
        [Tooltip("当切换绑定的数据模型")]
        public UnityEvent<DataModelBase> onDataModelChanged;
        public bool RaiseEventOnStart { get; set; } = true;
        
        private readonly DataBindingWorker _worker = new();
        private DataModelBase _dataModel;
        public DataModelBase DataModel
        {
            get => _dataModel;
            set
            {
                _dataModel = value;
                OnDataModelChanged(_dataModel);
                onDataModelChanged?.Invoke(_dataModel);
                if (enabled)
                    _worker.SetupDataBinding(_dataModel, didStart);
            }
        }
        
        /// <summary>
        /// 绑定
        /// </summary>
        /// <param name="propertyName">监听的属性</param>
        /// <param name="toTarget">源发生变化后执行的绑定函数</param>
        /// <returns>绑定路径</returns>
        public BindingPath Bind(string propertyName, BindFunc toTarget)
        {
            return _worker.Bind(propertyName, toTarget);
        }

        public void Unbind(ref BindingPath path)
        {
            _worker.Unbind(path);
            path.ToTarget = null;
        }
        
        /// <summary>
        /// 初始化所有组件，在Awake时调用，仅初始化1次
        /// </summary>
        protected virtual void InitComponents()
        {
            
        }

        protected virtual void OnDataModelChanged(DataModelBase dataModel)
        {
            
        }
        protected void Awake()
        {
            InitComponents();
        }

        protected void Start()
        {
            _worker.Activate(RaiseEventOnStart);
        }

        protected virtual void OnDisable()
        {
            _worker.SetupDataBinding(null);
        }
        
        protected virtual void OnEnable()
        {
            onDataModelChanged?.Invoke(_dataModel);
            _worker.SetupDataBinding(_dataModel);
        }

        public void RaiseAllPropertyChanged() => _worker.RaiseAllPropertyChangedEvent();
    }

    /// <summary>
    /// 子视图
    /// </summary>
    public abstract class SubView : ViewBase
    {
        public virtual void OnShow()
        {
        }
        
        public virtual void OnHide()
        {
        }
    }

    /// <summary>
    /// 顶层视图
    /// </summary>
    public abstract class RootView : ViewBase
    {
        [CanBeNull]
        protected CancellationTokenSource UniTaskCts;
        public virtual void OnShow()
        {
            UniTaskCts = new CancellationTokenSource();
        }
        public virtual void OnHide()
        {
            UniTaskCts?.Cancel(true);
            UniTaskCts?.Dispose();
            UniTaskCts = null;
        }
    }
    
    /// <summary>
    /// 顶层UI视图
    /// </summary>
    [RequireComponent(typeof(RectTransform))]
    public abstract class UIRootView : RootView
    {
        [SerializeField]
        [Tooltip("自适应异形屏")]
        private bool enableSafeAreaAdaptation = false;
        
        public override void OnShow()
        {
            base.OnShow();
            if (enableSafeAreaAdaptation)
                CalculateSafeAreaRect();
        }

        private void CalculateSafeAreaRect()
        {
            var rect = GetComponent<RectTransform>();
            var safeArea = Screen.safeArea;
            var minRelv = safeArea.min / new Vector2(Screen.width, Screen.height);
            var maxRelv = safeArea.max / new Vector2(Screen.width, Screen.height);
            rect.anchorMin = new Vector2(
                Mathf.Max(rect.anchorMin.x, minRelv.x),
                Mathf.Max(rect.anchorMin.y, minRelv.y));
            rect.anchorMax = new Vector2(
                Mathf.Min(rect.anchorMax.x, maxRelv.x),
                Mathf.Min(rect.anchorMax.y, maxRelv.y));
        }
    }
    
    /// <summary>
    /// UI子视图
    /// </summary>
    public abstract class UISubView : SubView
    {
    }
    
    [Serializable]
    public class SubViewCollectionPool : IDisposable
    {
        [SerializeField]
        [Tooltip("子视图根节点")]
        internal Transform root;

        private ObjectPool<SubView> _subViewPool;
        /// <summary>
        /// 视图实例
        /// </summary>
        private readonly List<SubView> _subViews = new();
        
        [Tooltip("集合模板")]
        [SerializeField]
        private SubView template;
        public SubView Template => template;

        public SubViewCollectionPool()
        {
            _subViewPool = new ObjectPool<SubView>(Create, OnGet, OnRelease, OnViewDestroy);
            SubView Create()
            {
                return Object.Instantiate(Template, root);
            }

            void OnGet(SubView view)
            {
                view.gameObject.SetActive(true);
            }
            
            void OnRelease(SubView view)
            {
                view.DataModel = null;
                view.gameObject.SetActive(false);
            }
            
            void OnViewDestroy(SubView view)
            {
                Object.Destroy(view.gameObject);
            }
        }

        public void Clear()
        {
            foreach (var subView in _subViews)
                _subViewPool.Release(subView);
            _subViews.Clear();
        }

        public void Remove(int index)
        {
            var instance = _subViews[index];
            _subViews.RemoveAt(index);
            _subViewPool.Release(instance);
            instance.OnHide();
        }

        public SubView Insert(int newIndex)
        {
            var instance = _subViewPool.Get();
            instance.transform.SetSiblingIndex(newIndex);
            _subViews.Insert(newIndex, instance);
            instance.OnShow();
            return instance;
        }

        public SubView Get(int index)
        {
            return _subViews[index];
        }
        
        public int IndexOf(SubView view)
        {
            return _subViews.IndexOf(view);
        }

        public int Count => _subViews.Count;

        public void Dispose()
        {
            Clear();
            _subViewPool?.Dispose();
        }
    }
    
    /// <summary>
    /// 集合UI视图，负责自动根据集合创建子对象
    /// </summary>
    public abstract class CollectionView<T> : SubView
    {
        [FormerlySerializedAs("uiCollectionPool")]
        [SerializeField]
        private SubViewCollectionPool subViewCollectionPool;
        
        private readonly CollectionBindingWorker<T> _worker;
        private DataCollection<T> _itemSource;
        
        /// <summary>
        /// 数据源
        /// </summary>
        public DataCollection<T> ItemSource
        {
            get => _itemSource;
            set
            {
                _itemSource = value;
                if (enabled)
                    _worker.SetupCollectionBinding(_itemSource);
            }
        }
        public Transform Root => subViewCollectionPool.root;
        
        protected CollectionView()
        {
            _worker = new(OnCollectionItemRemove, OnCollectionItemInsert, OnCollectionReset);
        }

        private void OnCollectionReset()
        {
            for (var i = subViewCollectionPool.Count - 1; i >= 0; --i)
                OnElementRemoving(subViewCollectionPool.Get(i), i);
            subViewCollectionPool.Clear();
        }
        
        private void OnCollectionItemRemove(DataCollection<T> collection, int oldIndex)
        {
            OnElementRemoving(subViewCollectionPool.Get(oldIndex), oldIndex);
            subViewCollectionPool.Remove(oldIndex);
        }

        protected virtual void OnElementRemoving(SubView subView, int oldIndex)
        {
        }
        
        private void OnCollectionItemInsert(DataCollection<T> collection, int newIndex, T newElement)
        {
            var instance = subViewCollectionPool.Insert(newIndex);
            instance.DataModel = newElement as DataModelBase;
            OnElementAdding(instance, newElement, newIndex);
        }

        protected virtual void OnElementAdding(SubView subView, T newElement, int newIndex)
        {
        }

        public int GetSubViewCount()
        {
            return subViewCollectionPool.Count;
        }
        
        public SubView GetSubView(int index)
        {
            return subViewCollectionPool.Get(index);
        }

        public int IndexOf(SubView subView)
        {
            return subViewCollectionPool.IndexOf(subView);
        }
        
        protected override void OnDisable()
        {
            base.OnDisable();
            _worker.SetupCollectionBinding(null);
        }
        
        protected override void OnEnable()
        {
            base.OnEnable();
            _worker.SetupCollectionBinding(_itemSource);
        }

        protected virtual void OnDestroy()
        {
            subViewCollectionPool.Dispose();
        }

        private void OnValidate()
        {
            if (subViewCollectionPool != null && subViewCollectionPool.root == null)
                subViewCollectionPool.root = transform;
        }
    }
}