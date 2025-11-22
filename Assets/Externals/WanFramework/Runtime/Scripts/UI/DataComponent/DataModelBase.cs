//    ▄▀▀▀▄▄▄▄▄▄▄▀▀▀▄
//    █▒▒░░░░░░░░░▒▒█    DataModel.cs
//     █░░█░░░░░█░░█     Created by WanNeng
//  ▄▄  █░░░▀█▀░░░█  ▄▄  Created   01/06/2024 14:03
// █░░█ ▀▄░░░░░░░▄▀ █░░█

using System;
using System.Runtime.CompilerServices;
using Newtonsoft.Json;
using UnityEngine;
using UnityEngine.Events;

namespace WanFramework.UI.DataComponent
{
    /// <summary>
    /// 数据模型抽象类
    /// </summary>
    [Serializable]
    public abstract class DataModelBase
    {
        [JsonIgnore]
        public UnityEvent<DataModelBase, string> onPropertyChanged = new();

        protected void RaisePropertyChangedEvent([CallerMemberName] string propertyName = "")
        {
            onPropertyChanged?.Invoke(this, propertyName);
        }

        protected static T[] CreateModelArray<T>(int count) where T : DataModelBase, new()
        {
            var array = new T[count];
            for (var i = 0; i < count; ++i) array[i] = new T();
            return array;
        }
        public virtual void Reset()
        {
        }
    }

    public static class DataModelExtensions
    {
        public static T As<T>(this DataModelBase modelBase) where T : DataModelBase
        {
            return modelBase as T;
        }
    }
    
    /// <summary>
    /// 数据模型单例
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public static class DataModel<T> where T : DataModelBase, new()
    {
        public static T Instance { get; } = new();
    }
    
    [AttributeUsage(AttributeTargets.Field | AttributeTargets.Property)]
    [System.Diagnostics.Conditional("AutoNotifyGenerator_DEBUG")]
    public sealed class AutoNotifyAttribute : Attribute
    {
        public AutoNotifyAttribute()
        {
        }
        public string PropertyName { get; set; }
    }
    
    [AttributeUsage(AttributeTargets.Class)]
    public class ShowInDataModelEditorAttribute : Attribute
    {
    }
}