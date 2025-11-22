//    ▄▀▀▀▄▄▄▄▄▄▄▀▀▀▄
//    █▒▒░░░░░░░░░▒▒█    DataSystem.cs
//     █░░█░░░░░█░░█     Created by WanNeng
//  ▄▄  █░░░▀█▀░░░█  ▄▄  Created   12/24/2023 10:43
// █░░█ ▀▄░░░░░░░▄▀ █░░█

using System;
using System.Collections.Generic;
using Cysharp.Threading.Tasks;
using UnityEngine;
using WanFramework.Base;
using WanFramework.Resource;

namespace WanFramework.Data
{
    [SystemPriority(SystemPriorities.DataSystem)]
    public class DataSystem : SystemBase<DataSystem>
    {
        private readonly Dictionary<string, IDataTable> _dataCache = new();

        public T Load<T>() where T : class, IDataTable, new()
            => Load<T>(typeof(T).Name);
        public T Load<T>(string tableName) where T : class, IDataTable, new()
        {
            if (_dataCache.TryGetValue(tableName, out var value))
                return value as T;
            var tableAsset = new T();
            _dataCache[tableName] = tableAsset;
            var tableRaw = ResourceSystem.Instance.Load<DataTableRawAsset>($"Content/Data/{tableName}.xlsx");
            tableAsset.LoadFrom(tableRaw);
            return tableAsset;
        }

        public override async UniTask Init()
        {
            await base.Init();
            await ResourceSystem.Instance.PreloadAssetByLabelAsync(ResourceLabels.Table);
            GameManager.Current?.InitCover?.SetProgress(0.6f);
        }
    }
}