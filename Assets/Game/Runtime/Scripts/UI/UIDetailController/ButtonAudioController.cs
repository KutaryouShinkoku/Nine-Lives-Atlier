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

public class ButtonAudioController : MonoBehaviour, IPointerEnterHandler, IPointerClickHandler
{
    [SerializeField] private bool enableHighlightAudio = true;
    [SerializeField] private bool enableClickAudio = true;
    public void OnPointerEnter(PointerEventData eventData)
    {
        if(enableHighlightAudio)
        AudioSystem.Instance.SendEvent(AudioIds.UI_Button_Touch);
    }
    public void OnPointerClick(PointerEventData eventData)
    {
        if(enableClickAudio)
        AudioSystem.Instance.SendEvent(AudioIds.UI_Button_Click);
    }

}

