using System.Security.Cryptography;
using System.Threading;
using Cysharp.Threading.Tasks;
using Game.Data;
using Game.Localization.Components;
using Game.Model;
using Game.Model.InGameSubModel;
using Game.UI.Common.Components;
using Game.UI.InGame.Components;
using TMPro;
using UnityEngine;
using UnityEngine.Serialization;
using UnityEngine.TextCore.Text;
using UnityEngine.UI;
using WanFramework.UI;
using WanFramework.UI.Components;
using WanFramework.UI.DataComponent;
using WanFramework.Utils;

namespace Game.UI.InGame
{
    public class InGamePlayerView : RootView
    {
        [SerializeField]
        private ValueBar barWater;
        [SerializeField]
        private ValueBar barFire;
        [SerializeField]
        private ValueBar barEarth;
        [SerializeField]
        private ValueBar barAir;

        [SerializeField]
        private TMP_Text textHealth;

        [SerializeField]
        private GameObject tailwindGauge;
        [SerializeField]
        private TMP_Text txtTailwind;

        [SerializeField]
        private UIBuffCollectionView buffCollectionView;

        [SerializeField]
        private UniAnimation animResourceShake;
        
        public TMP_Text GetHealthText() => textHealth;

        public UIBuffCollectionView GetBuffCollectionView() => buffCollectionView;

        public CommonUIBuffView GetPlayerBuff()
        {
            var views = buffCollectionView.GetComponentsInChildren<CommonUIBuffView>();
            return views.Length > 0 ? views[0] : null;
        }

        protected override void InitComponents()
        {
            base.InitComponents();
            Bind(nameof(PlayerModel.CharacterId), m => SetCharacter(m.As<PlayerModel>().CharacterId));
            Bind(nameof(PlayerModel.Resources), m => SetResources(m.As<PlayerModel>().Resources));
            Bind(nameof(PlayerModel.Health), m => SetHealth(m.As<PlayerModel>().Health));
        }
        protected override void OnDataModelChanged(DataModelBase dataModel)
        {
            base.OnDataModelChanged(dataModel);
            buffCollectionView.ItemSource = dataModel?.As<PlayerModel>()?.Buffs;
        }
        public void SetCharacter(CharacterIds character)
        {
            var entry = character.Data();
            if (entry.ResMax.Length < 4) return;
            SetPlayerElements(entry.CharElemental);
            barFire.SetMaxValue(entry.ResMax[(int)CardBaseType.Fire]);
            barWater.SetMaxValue(entry.ResMax[(int)CardBaseType.Water]);
            barEarth.SetMaxValue(entry.ResMax[(int)CardBaseType.Earth]);
            barAir.SetMaxValue(entry.ResMax[(int)CardBaseType.Air]);
            SetGauges(character);
        }

        public void SetPlayerElements(bool[] elements)
        {
            if (elements.Length < 4) return;
            barFire.transform.parent.gameObject.SetActive(elements[0]);
            barWater.transform.parent.gameObject.SetActive(elements[1]);
            barEarth.transform.parent.gameObject.SetActive(elements[2]);
            barAir.transform.parent.gameObject.SetActive(elements[3]);
        }

        public void SetResources(CardCost res)
        {
            barWater.SetValue(res.Water);
            barFire.SetValue(res.Fire);
            barEarth.SetValue(res.Earth);
            barAir.SetValue(res.Air);
        }

        public void SetGauges(CharacterIds character)
        {
            var entry = character.Data();
            //如果选择里包含气，则额外有顺风量表
            tailwindGauge.SetActive(entry.ResMax[(int)CardBaseType.Air] != 0);
            //特定组合会有额外量表，如果以后要做的话，就把这个做成数组或者list吧。
        }

        public void ChangeGaugeValue(int value)
        {
            txtTailwind.SetText(value.ToString());
        }

        public void SetHealth(int health)
        {
            textHealth.SetText(health.ToString());
        }

        public void PlayResourceBarValChangeAnim(CardCost res) => SetResources(res);
        
        //暂时只有一个量表，先史山一下
        public void PlayGaugesValueChangeAnim(int value) => ChangeGaugeValue(value);
        public async UniTask PlayResourceShakeAnim(CancellationToken token) => await animResourceShake.Play(token);
    }
}