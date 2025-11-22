//    ▄▀▀▀▄▄▄▄▄▄▄▀▀▀▄
//    █▒▒░░░░░░░░░▒▒█    DataTable.cs
//     █░░█░░░░░█░░█     Created by WanNeng
//  ▄▄  █░░░▀█▀░░░█  ▄▄  Created   12/24/2023 10:43
// █░░█ ▀▄░░░░░░░▄▀ █░░█

using System;
using System.Collections;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Linq;
using System.Runtime.Serialization.Formatters.Binary;
using UnityEngine;

namespace WanFramework.Data
{
    public interface IDataTable
    {
        public string TableName { get; }
        public DataEntry Get(int id);
        public void LoadFrom(DataTableRawAsset rawAsset);
        public int Length { get; }
    }
    
    public interface IDataTable<out T> where T : DataEntry
    {
        public string TableName { get; }
        public T Get(int id);
    }
    
    /// <summary>
    /// 数据表泛型，源生成器生成的表资源均继承于此
    /// </summary>
    [Serializable]
    public abstract class DataTable<T> : IDataTable, IDataTable<T> where T : DataEntry
    {
        [SerializeField] protected T[] data;
        public abstract string TableName { get; }
        T IDataTable<T>.Get(int id) => data[id];
        public DataEntry Get(int id) => data[id];
        public int Length => data.Length;
        public void SetData(T[] d) => data = d;
        public abstract void LoadFrom(DataTableRawAsset rawAsset);
    }

    [Serializable]
    public abstract class DataEntry
    {
    }
}