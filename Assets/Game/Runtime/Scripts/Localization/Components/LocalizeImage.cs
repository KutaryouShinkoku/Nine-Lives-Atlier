using System;
using System.Collections.Generic;
using Game.Data;
using UnityEngine;
using UnityEngine.UI;
#if UNITY_EDITOR
using UnityEditor;
#endif

namespace Game.Localization.Components
{
    [RequireComponent(typeof(Image))]
    public class LocalizeImage : MonoBehaviour, ILocalizeComponent
    {
        [SerializeField]
        private Sprite[] sprites;
        private Image _image;
        void Awake() => _image = GetComponent<Image>();
        void Start() => UpdateImage();
        public void OnLanguageChanged() => UpdateImage();
        private void UpdateImage() => _image.sprite = sprites[(int)LocalizeSystem.Instance.Current];
        private void OnValidate()
        {
            var languageCount = Enum.GetValues(typeof(Language)).Length;
            if (sprites == null)
                sprites = new Sprite[languageCount];
            else if (sprites.Length != languageCount)
            {
                var newSprites = new Sprite[languageCount];
                for (var i = 0; i < Math.Min(languageCount, sprites.Length); i++)
                    newSprites[i] = sprites[i];
                sprites = newSprites;
            }
        }
    }
}