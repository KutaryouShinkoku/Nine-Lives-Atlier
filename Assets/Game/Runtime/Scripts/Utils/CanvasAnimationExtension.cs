using System;
using UnityEngine;

namespace Game.Utils
{
    [RequireComponent(typeof(Canvas))]
    [ExecuteAlways]
    public class CanvasAnimationExtension : MonoBehaviour
    {
        [SerializeField]
        private int sortingOrder;
        private Canvas _canvas;
        
        void Awake()
        {
            _canvas = GetComponent<Canvas>();
            sortingOrder = _canvas.sortingOrder;
        }
        private void Update()
        {
            sortingOrder = _canvas.sortingOrder;
        }

        private void OnDidApplyAnimationProperties()
        {
            _canvas.sortingOrder = sortingOrder;
        }
    }
}