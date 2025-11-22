using System;
using System.Collections.Generic;
using Cysharp.Threading.Tasks;
using UnityEngine;
using WanFramework.Base;
using Object = UnityEngine.Object;

namespace Game.Tests
{
    public static class GameSystemHelper
    {
        private static Dictionary<Type, GameObject> _systems = new();
        
        public static async UniTask RequireSystem<T>() where T : SystemBase<T>
        {
            if (_systems.ContainsKey(typeof(T))) return;
            var go = new GameObject(typeof(T).Name, typeof(T));
            await go.GetComponent<T>().Init();
            _systems[typeof(T)] = go;
        }
        public static void DestroyAllSystems()
        {
            foreach (var kvp in _systems)
                Object.Destroy(kvp.Value);
        }
    }
}