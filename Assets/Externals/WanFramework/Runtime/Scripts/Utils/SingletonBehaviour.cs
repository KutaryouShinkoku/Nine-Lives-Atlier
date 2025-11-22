//    ▄▀▀▀▄▄▄▄▄▄▄▀▀▀▄
//    █▒▒░░░░░░░░░▒▒█    SingletonBehaviour.cs
//     █░░█░░░░░█░░█     Created by WanNeng
//  ▄▄  █░░░▀█▀░░░█  ▄▄  Created   01/11/2024 17:46
// █░░█ ▀▄░░░░░░░▄▀ █░░█

using UnityEngine;

namespace WanFramework.Utils
{
    public class SingletonBehaviour<T> : MonoBehaviour where T : SingletonBehaviour<T>
    {
        public static T Instance { get; private set; }
        
        protected SingletonBehaviour()
        {
            if (Instance == null)
                Instance = this as T;
            else
                Debug.LogError($"Singleton {typeof(T)} already created");
        }
    }
}