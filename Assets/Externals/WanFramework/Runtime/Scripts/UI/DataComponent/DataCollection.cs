//    ▄▀▀▀▄▄▄▄▄▄▄▀▀▀▄
//    █▒▒░░░░░░░░░▒▒█    IDataCollection.cs
//     █░░█░░░░░█░░█     Created by WanNeng
//  ▄▄  █░░░▀█▀░░░█  ▄▄  Created   01/06/2024 14:03
// █░░█ ▀▄░░░░░░░▄▀ █░░█

using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.Events;
using UnityEngine.Serialization;

namespace WanFramework.UI.DataComponent
{
    /// <summary>
    /// 数据集合
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public sealed class DataCollection<T> : DataModelBase, IList<T>, IList
    {
        private readonly UnityAction<DataModelBase, string> _itemPropertyChanged;
        public readonly UnityEvent<DataCollection<T>, int, T> onItemInsert = new();
        public readonly UnityEvent<DataCollection<T>, int> onItemRemove = new();

        private readonly List<T> _data = new();

        public int Count => _data.Count;
        public bool IsSynchronized => false;
        public object SyncRoot => new();
        public bool IsReadOnly => false;

        public bool IsFixedSize => false;

        public DataCollection()
        {
            _itemPropertyChanged = OnItemPropertyChanged;
        }
        
        public IEnumerator<T> GetEnumerator()
        {
            return ((IEnumerable<T>)_data).GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return ((IEnumerable)_data).GetEnumerator();
        }

        public bool Contains(T item)
        {
            return _data.Contains(item);
        }

        bool IList.Contains(object value)
        {
            return ((IList)_data).Contains(value);
        }

        public void CopyTo(T[] array, int arrayIndex)
        {
            _data.CopyTo(array, arrayIndex);
        }
        
        void ICollection.CopyTo(Array array, int index)
        {
            ((IList)_data).CopyTo(array, index);
        }
        
        public int IndexOf(T item)
        {
            return _data.IndexOf(item);
        }

        int IList.IndexOf(object value)
        {
            return ((IList)_data).IndexOf(value);
        }
        
        public void Add(T item)
        {
            Insert(_data.Count, item);
        }

        int IList.Add(object value)
        {
            if (value is not T tVal) return -1;
            Insert(_data.Count, tVal);
            return _data.Count - 1;
        }
        
        public void AddRange(IList<T> items)
        {
            for (var i = 0; i < items.Count; ++i)
                Add(items[i]);
        }

        public void Clear()
        {
            var oldCount = _data.Count;
            foreach (var item in _data)
                if (item is DataModelBase subModel) subModel.onPropertyChanged.RemoveListener(_itemPropertyChanged);
            _data.Clear();
            // 从后往前发送item移除事件
            for (var i = oldCount - 1; i >= 0; --i)
                onItemRemove?.Invoke(this, i);
            onPropertyChanged?.Invoke(this, "_data");
        }


        public bool Remove(T item)
        {
            var index = _data.IndexOf(item);
            if (index == -1) return false;
            RemoveAt(index);
            return true;
        }
        void IList.Remove(object value)
        {
            var index = ((IList)_data).IndexOf(value);
            if (index == -1) return;
            RemoveAt(index);
        }

        public T this[int index]
        {
            get => _data[index];
            set
            {
                RemoveAt(index);
                Insert(index, value);
            }
        }
        object IList.this[int index]
        {
            get => this[index];
            set => this[index] = (T)value;
        }

        public void RemoveAt(int index)
        {
            if (_data[index] is DataModelBase subModel) subModel.onPropertyChanged.RemoveListener(_itemPropertyChanged);
            _data.RemoveAt(index);
            onItemRemove?.Invoke(this, index);
            onPropertyChanged?.Invoke(this, "_data");
        }

        public void Insert(int index, T item)
        {
            if (item is DataModelBase subModel) subModel.onPropertyChanged.AddListener(_itemPropertyChanged);
            _data.Insert(index, item);
            onItemInsert?.Invoke(this, index, item);
            onPropertyChanged?.Invoke(this, "_data");
        }

        void IList.Insert(int index, object value)
        {
            Insert(index, (T)value);
        }

        public override void Reset()
        {
            Clear();
        }
        private void OnItemPropertyChanged(DataModelBase m, string p) => onPropertyChanged?.Invoke(this, "_data");
    }
}