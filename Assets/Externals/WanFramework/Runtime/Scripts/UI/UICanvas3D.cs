using UnityEngine;

namespace WanFramework.UI
{
    [RequireComponent(typeof(Canvas))]
    public class UICanvas3D : MonoBehaviour
    {
        private void Awake() => GetComponent<Canvas>().worldCamera = UISystem.Instance.UICamera;
    }
}