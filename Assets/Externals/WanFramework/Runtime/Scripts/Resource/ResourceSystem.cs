//    ▄▀▀▀▄▄▄▄▄▄▄▀▀▀▄
//    █▒▒░░░░░░░░░▒▒█    ResourceSystem.cs
//     █░░█░░░░░█░░█     Created by WanNeng
//  ▄▄  █░░░▀█▀░░░█  ▄▄  Created   12/24/2023 10:52
// █░░█ ▀▄░░░░░░░▄▀ █░░█

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using Cysharp.Threading.Tasks;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.Pool;
using UnityEngine.ResourceManagement.AsyncOperations;
using UnityEngine.ResourceManagement.ResourceProviders;
using UnityEngine.SceneManagement;
using UnityEngine.Serialization;
using WanFramework.Base;

namespace WanFramework.Resource
{
    public static class ResourceLabels
    {
        public const string Prefab = "Prefab";
        public const string UI = "UI";
        public const string Table = "Table";
    }
    [SystemPriority(SystemPriorities.Resource)]
    public class ResourceSystem : SystemBase<ResourceSystem>
    {
        private readonly Dictionary<string, AsyncOperationHandle<UnityEngine.Object>> _resourceCache = new();
        private readonly Dictionary<string, AsyncOperationHandle<SceneInstance>> _sceneCache = new();
        public async UniTask LoadSceneAsync(string scenePath)
        {
            if (_sceneCache.TryGetValue(scenePath, out var sceneOp))
                return;
            var loadOp = Addressables.LoadSceneAsync(scenePath, LoadSceneMode.Additive);
            _sceneCache[scenePath] = loadOp;
            await loadOp.Task;
        }
        public async UniTask UnloadScene(string scenePath)
        {
            if (_sceneCache.TryGetValue(scenePath, out var sceneOp))
                await Addressables.UnloadSceneAsync(sceneOp);
            else
                Debug.LogWarning($"Scene '{scenePath}' not found");
        }
        public T Load<T>(string path) where T : UnityEngine.Object
        {
            if (_resourceCache.TryGetValue(path, out var cached))
                return (T)cached.Result;
            #if UNITY_WEBGL
            Debug.LogError($"WebGL下同步加载需要提前预载资源，资源{path}未预载，调用ResourceSystem.Instance.LoadAssetByLabelAsync来预载标签资源");
            #endif
            var asset = LoadAsync<T>(path);
            asset.WaitForCompletion();
            var result = asset.Result;
            if (result == null)
                Debug.LogError($"{path} not found");
            return result;
        }
        public GameObject LoadPrefab(string prefabName)
            => Load<GameObject>($"Content/Prefab/{prefabName}.prefab");
        public AsyncOperationHandle<T> LoadAsync<T>(string path) where T : UnityEngine.Object
            => Addressables.LoadAssetAsync<T>(path);
        public async UniTask<T> LoadAsyncUniTask<T>(string path, CancellationToken token) where T : UnityEngine.Object
        {
            var op = Addressables.LoadAssetAsync<T>(path);
            await op.WithCancellation(token);
            return op.Result;
        }
        public async UniTask PreloadAssetByLabelAsync(string label)
        {
            GameManager.Current?.InitCover?.PrintResourceLog($"Preloading resource {label}");
            GameManager.Current?.InitCover?.SetSubProgress(0);
            var loadResourceLocationsHandle = Addressables.LoadResourceLocationsAsync(label);
            await loadResourceLocationsHandle;
            var loadingTasks
                = loadResourceLocationsHandle.Result.Select(
                    async l =>
                    {
                        GameManager.Current?.InitCover?.PrintResourceLog($"{l.PrimaryKey}");
                        var task = Addressables.LoadAssetAsync<UnityEngine.Object>(l);
                        while (!task.IsDone)
                        {
                            GameManager.Current?.InitCover?.SetSubProgress(task.PercentComplete);
                            await UniTask.Yield();
                        }
                        await task;
                        _resourceCache[l.PrimaryKey] = task;
                    });
            await UniTask.WhenAll(loadingTasks);
        }

        public override async UniTask Init()
        {
            await base.Init();
            await PreloadAssetByLabelAsync(ResourceLabels.Prefab);
            GameManager.Current?.InitCover?.SetProgress(0.5f);
        }
    }
}