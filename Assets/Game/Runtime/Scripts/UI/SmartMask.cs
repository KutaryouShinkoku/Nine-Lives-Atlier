using System;
using System.Buffers;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.Serialization;
using UnityEngine.UI;
using WanFramework.UI;

namespace Game.UI
{
    public enum SmartMaskInteraction
    {
        CanInteractInside,
        BlockAllInteraction,
        Keep
    }
    public class SmartMask : MonoBehaviour
    {
        [FormerlySerializedAs("referencePrefabPath")]
        [SerializeField]
        [Tooltip("全局路径")]
        private string referenceRectPath;
        [SerializeField]
        private SmartMaskInteraction interaction = SmartMaskInteraction.CanInteractInside;
        [SerializeField]
        private Image bg;
        [SerializeField]
        private Image mask2D;
        private CanvasRenderer _mask2DRenderer;
        [SerializeField]
        private RectTransform decorationRect;
        [SerializeField]
        private Vector2 maskScale = Vector2.one;
        [SerializeField]
        private Vector2 maskOffset = Vector2.zero;
        private Vector3 _decorationOffset = Vector3.zero;
        private RectTransform _referenceRect;
        private void Start()
        {
            if (decorationRect) _decorationOffset = decorationRect.position - mask2D.transform.position;
            mask2D.TryGetComponent(out _mask2DRenderer);
            if (!_referenceRect) return;
            SyncRectTransform(_referenceRect, mask2D.rectTransform, maskScale, maskOffset);
        }
        private void OnEnable()
        {
            if (string.IsNullOrEmpty(referenceRectPath))
            {
                mask2D.gameObject.SetActive(false);
                return;
            }
            var go = GameObject.Find(referenceRectPath);
            if (!go)
            {
                Debug.LogError($"{referenceRectPath} not found");
                mask2D.gameObject.SetActive(false);
            }
            if (!go.TryGetComponent(out _referenceRect))
            {
                Debug.LogError($"{referenceRectPath} is not a RectTransform");
                mask2D.gameObject.SetActive(false);
            }
            else mask2D.gameObject.SetActive(true);
        }
        private void Update()
        {
            if (!_referenceRect) return;
            SyncRectTransform(_referenceRect, mask2D.rectTransform, maskScale, maskOffset);
            if (decorationRect)
            {
                decorationRect.position = mask2D.transform.position + _decorationOffset;
            }
            if (interaction == SmartMaskInteraction.Keep) return;
            if (interaction == SmartMaskInteraction.CanInteractInside)
                bg.raycastTarget = !IsPointInside(Pointer.current.position.ReadValue());
            if (interaction == SmartMaskInteraction.BlockAllInteraction)
                bg.raycastTarget = true;
        }

        private static void SyncRectTransform(RectTransform rectA, RectTransform rectB, Vector2 scale, Vector2 offset)
        {
            var worldCorners = ArrayPool<Vector3>.Shared.Rent(4);
            try
            {
                rectA.GetWorldCorners(worldCorners);
                var uiCamera = UISystem.Instance.UICamera;
                var screenMin = uiCamera.WorldToScreenPoint(worldCorners[0]);
                var screenMax = uiCamera.WorldToScreenPoint(worldCorners[2]);
                
                RectTransformUtility.ScreenPointToLocalPointInRectangle(
                    rectB.parent as RectTransform, screenMin, uiCamera, out var localBottomLeft);
                RectTransformUtility.ScreenPointToLocalPointInRectangle(
                    rectB.parent as RectTransform, screenMax, uiCamera, out var localTopRight);
                
                var center = (localBottomLeft + localTopRight) / 2f;
                rectB.anchoredPosition = center + offset;
                
                var size = localTopRight - localBottomLeft;
                rectB.sizeDelta = new Vector2(Mathf.Abs(size.x), Mathf.Abs(size.y)) * scale;
            }
            finally
            {
                ArrayPool<Vector3>.Shared.Return(worldCorners);
            }
        }

        private bool IsPointInside(Vector2 screenPosition)
        {
            var mesh = _mask2DRenderer?.GetMesh();
            if (mesh == null) return false;
            var vertices = mesh.vertices;
            var triangles = mesh.triangles;
            for (var i = 0; i < triangles.Length - 3; i+=3)
            {
                var a = UISystem.Instance.UICamera.WorldToScreenPoint(mask2D.transform.TransformPoint(vertices[triangles[i]]));
                var b = UISystem.Instance.UICamera.WorldToScreenPoint(mask2D.transform.TransformPoint(vertices[triangles[i + 1]]));
                var c = UISystem.Instance.UICamera.WorldToScreenPoint(mask2D.transform.TransformPoint(vertices[triangles[i + 2]]));
                var pa = a - (Vector3)screenPosition;
                var pb = b - (Vector3)screenPosition;
                var pc = c - (Vector3)screenPosition;
                var t1 = Vector3.Cross(pa, pb).z;
                var t2 = Vector3.Cross(pb, pc).z;
                var t3 = Vector3.Cross(pc, pa).z;
                if (t1 * t2 >= 0 && t1 * t3 >= 0 && t2 * t3 >= 0) return true;
            }
            return false;
        }

        private void OnDrawGizmosSelected()
        {
            #if UNITY_EDITOR
            if (_mask2DRenderer == null)
                mask2D.TryGetComponent(out _mask2DRenderer);
            #endif
            var mesh = _mask2DRenderer?.GetMesh();
            if (mesh == null) return;
            var vertices = mesh.vertices;
            var triangles = mesh.triangles;
            Gizmos.color = Color.red;
            for (var i = 0; i < triangles.Length - 3; i+=3)
            {
                var a = mask2D.transform.TransformPoint(vertices[triangles[i]]);
                var b = mask2D.transform.TransformPoint(vertices[triangles[i + 1]]);
                var c = mask2D.transform.TransformPoint(vertices[triangles[i + 2]]);
                Gizmos.DrawLine(a, b);
                Gizmos.DrawLine(a, c);
                Gizmos.DrawLine(b, c);
            }
        }
    }
}