using System.Threading;
using Cysharp.Threading.Tasks;
using Cysharp.Threading.Tasks.Triggers;
using Game.Audio;
using Game.Data;
using Game.Localization.Components;
using Game.Model;
using Game.Model.InGameSubModel;
using Game.UI.Common;
using Game.UI.Common.Components;
using Game.UI.InGame.Components;
using JetBrains.Annotations;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.Serialization;
using UnityEngine.TextCore.Text;
using UnityEngine.UI;
using WanFramework.Resource;
using WanFramework.UI;
using WanFramework.UI.Components;
using WanFramework.UI.DataComponent;
using WanFramework.Utils;

public class ButtonImageController : MonoBehaviour, IScrollHandler
{
    [SerializeField] private Sprite normalImg;
    [SerializeField] private Sprite interactedImg;
    [SerializeField] private bool handleMode;

    private Image _buttonImg;


    private void Awake()
    {
        _buttonImg = GetComponent<Image>();
        _buttonImg.sprite = normalImg;
    }

    public void OnScroll(PointerEventData eventData)
    {
        _buttonImg.sprite = interactedImg;
    }

    //public void OnPointerDown(PointerEventData eventData)
    //{
    //    _buttonImg.sprite = interactedImg;
    //}
    //public void OnPointerExit(PointerEventData eventData)
    //{
    //   if (handleMode) { _buttonImg.sprite = normalImg; } 
    //}

    //public void OnPointerUp(PointerEventData eventData)
    //{
    //   _buttonImg.sprite = normalImg;
    //}

}

