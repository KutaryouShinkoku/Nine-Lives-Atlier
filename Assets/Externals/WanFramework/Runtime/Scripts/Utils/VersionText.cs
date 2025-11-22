using TMPro;
using UnityEngine;

namespace WanFramework.Utils
{
    [RequireComponent(typeof(TMP_Text))]
    public class VersionText : MonoBehaviour
    {
        [SerializeField]
        private TMP_Text text;
        private void Awake() => text = GetComponent<TMP_Text>();
        private void Start() => text.text = $"Ver.{Application.version} ({GitInfo.GetRevisionHash()[..10]})";
    }
}