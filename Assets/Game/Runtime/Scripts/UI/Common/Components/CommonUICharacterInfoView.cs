using Game.Data;
using Game.Localization.Components;
using UnityEngine;
using WanFramework.Data;

namespace Game.UI.Common.Components
{
    public class CommonUICharacterInfoView : MonoBehaviour
    {
        [SerializeField]
        private LocalizeText textName;
        [SerializeField]
        private LocalizeText textDesc;
        
        public void SetCharacter(CharacterIds id)
        {
            var data = DataSystem.Instance.Load<CharacterTable>().Get(id);
            textName.SetText(data.Name);
            textDesc.SetText(data.Desc);
        }
    }
}