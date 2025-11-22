//    ▄▀▀▀▄▄▄▄▄▄▄▀▀▀▄
//    █▒▒░░░░░░░░░▒▒█    DataContext.cs
//     █░░█░░░░░█░░█     Created by WanNeng
//  ▄▄  █░░░▀█▀░░░█  ▄▄  Created   01/10/2024 18:26
// █░░█ ▀▄░░░░░░░▄▀ █░░█

using System;
using System.Collections.Generic;
using JetBrains.Annotations;
using UnityEngine;
using WanFramework.UI.DataComponent;

namespace WanFramework.UI
{
    public delegate void BindFunc(DataModelBase m);

    /// <summary>
    /// 绑定路径
    /// </summary>
    public struct BindingPath
    {
        internal readonly string PropertyName;
        internal BindFunc ToTarget{ get; set; }
        
        public BindingPath(string name, BindFunc toTarget)
        {
            PropertyName = name;
            ToTarget = toTarget;
        }
    }

    internal class PropertyChangedEventRoute
    {
        private readonly Dictionary<string, List<BindFunc>> _dict = new();

        public void Add(string propertyName, BindFunc func)
        {
            if (!_dict.TryGetValue(propertyName, out var list))
            {
                list = new();
                _dict[propertyName] = list;
            }
            list.Add(func);
        }

        public void Remove(string propertyName, BindFunc func)
        {
            if (!_dict.TryGetValue(propertyName, out var list))
                return;
            list.Remove(func);
            if (list.Count == 0)
                _dict.Remove(propertyName);
        }

        internal void RaisePropertyChangedEvent(DataModelBase modelBase, string propertyName)
        {
            if (!_dict.TryGetValue(propertyName, out var list))
                return;
            foreach (var func in list)
            {
                try
                {
                    func.Invoke(modelBase);
                }
                catch (Exception e)
                {
                   Debug.LogException(e);
                }
            }
        }

        internal void RaiseAllPropertyChangedEvent(DataModelBase modelBase)
        {
            if (modelBase == null) return;
            foreach (var list in _dict.Values)
                foreach (var func in list)
                    func?.Invoke(modelBase);
        }
    }
    
    public class DataBindingWorker
    {
        private readonly PropertyChangedEventRoute _propertyChangedEventRoute = new();
        [CanBeNull] private DataModelBase _current;

        private bool isActivated = false;
        
        public void SetupDataBinding(DataModelBase modelBase, bool raiseEvent = true)
        {
            if (_current == modelBase) return;
            _current?.onPropertyChanged.RemoveListener(OnPropertyChanged);
            _current = modelBase;
            _current?.onPropertyChanged.AddListener(OnPropertyChanged);
            if (raiseEvent)
                _propertyChangedEventRoute.RaiseAllPropertyChangedEvent(_current);
        }

        private void OnPropertyChanged(DataModelBase modelBase, string property)
        {
            _propertyChangedEventRoute.RaisePropertyChangedEvent(modelBase, property);
        }

        public BindingPath Bind(string propertyName, BindFunc toTarget)
        {
            var path = new BindingPath(propertyName, toTarget);
            _propertyChangedEventRoute.Add(path.PropertyName, path.ToTarget);
            if (_current == null || !isActivated) 
                return path;
            try
            {
                toTarget?.Invoke(_current);
            }
            catch (Exception e)
            {
                Debug.LogException(e);
            }
            return path;
        }

        public void Unbind(BindingPath path)
        {
            _propertyChangedEventRoute.Remove(path.PropertyName, path.ToTarget);
        }

        internal void Activate(bool forceRaiseAllEvents = true)
        {
            if (isActivated) return;
            isActivated = true;
            if (_current != null && forceRaiseAllEvents)
                _propertyChangedEventRoute.RaiseAllPropertyChangedEvent(_current);
        }
        
        internal void RaiseAllPropertyChangedEvent()
            => _propertyChangedEventRoute.RaiseAllPropertyChangedEvent(_current);
    }

    public class CollectionBindingWorker<T>
    {
        [CanBeNull] private DataCollection<T> _current;

        private readonly Action<DataCollection<T>, int> _onCollectionItemRemove;
        private readonly Action<DataCollection<T>, int, T> _onCollectionItemInsert;
        private readonly Action _onCollectionReset;
        
        public CollectionBindingWorker(Action<DataCollection<T>, int> remove, Action<DataCollection<T>, int, T> insert, Action reset)
        {
            _onCollectionItemRemove = remove;
            _onCollectionItemInsert = insert;
            _onCollectionReset = reset;
        }
        
        public void SetupCollectionBinding(DataCollection<T> model)
        {
            if (_current == model) return;
            _current?.onItemInsert.RemoveListener(OnCollectionItemInsert);
            _current?.onItemRemove.RemoveListener(OnCollectionItemRemove);
            _current = model;
            OnCollectionItemReset();
            _current?.onItemInsert.AddListener(OnCollectionItemInsert);
            _current?.onItemRemove.AddListener(OnCollectionItemRemove);
            if (_current != null)
                RaiseCollectionRebindEvent(_current);
        }

        private void RaiseCollectionRebindEvent(DataCollection<T> collection)
        {
            for (var i = 0; i < collection.Count; ++i)
                OnCollectionItemInsert(collection, i, collection[i]);
        }
        
        private void OnCollectionItemRemove(DataCollection<T> collection, int oldIndex)
        {
            _onCollectionItemRemove?.Invoke(collection, oldIndex);
        }
        
        private void OnCollectionItemInsert(DataCollection<T> collection, int newIndex, T newElement)
        {
            _onCollectionItemInsert?.Invoke(collection, newIndex, newElement);
        }
        
        private void OnCollectionItemReset()
        {
            _onCollectionReset?.Invoke();
        }
    }
}