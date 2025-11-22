using System;
using System.Linq;
using System.Text;
using System.Threading;
using Cysharp.Threading.Tasks;
using Game.Data;
using Game.Localization;
using Game.Localization.Components;
using Game.Logic;
using Game.Model.InGameSubModel;
using Game.UI.Tip;
using JetBrains.Annotations;
using TMPro;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using WanFramework.Resource;
using WanFramework.UI;
using WanFramework.UI.DataComponent;
using WanFramework.Utils;

namespace Game.UI.Common.Components
{
    public class CommonCardUIView : UISubView
    {
        [CanBeNull]
        private CardTable.Entry _current;
        
        [CanBeNull]
        [SerializeField]
        private TipProviderRegion tipRegion;

        [SerializeField]
        private LocalizeText textName;

        [SerializeField]
        private LocalizeText textDesc;

        [SerializeField]
        private TMP_Text cardDesc;

        [SerializeField]
        private TMP_Text textCardEffectVal1;

        [SerializeField]
        private TMP_Text textCardEffectVal2;

        [SerializeField]
        private TMP_Text textCardEffectVal3;


        [Header("稀有度边框")]
        [SerializeField]
        private Image basicImage;

        [SerializeField]
        private Image rarityImage;

        [SerializeField] 
        private Image shopImage;

        [SerializeField]
        private GameObject[] goCosts;
        
        [SerializeField]
        private TMP_Text[] textCosts;
        
        [SerializeField]
        private RectTransform root;
        [CanBeNull]
        private GameObject _obj;

        [SerializeField]
        private Image[] cardLabelImg;
        
        [Header("卡牌背景")]
        [SerializeField]
        private GameObject cardBgFire;
        [SerializeField]
        private GameObject cardBgWater;
        [SerializeField]
        private GameObject cardBgEarth;
        [SerializeField]
        private GameObject cardBgAir;
        [SerializeField]
        private GameObject cardBgFireWater;
        [SerializeField]
        private GameObject cardBgFireEarth;
        [SerializeField]
        private GameObject cardBgFireAir;
        [SerializeField]
        private GameObject cardBgWaterEarth;
        [SerializeField]
        private GameObject cardBgWaterAir;
        [SerializeField]
        private GameObject cardBgEarthAir;
        
        
        // 添加宽度控制相关变量
        private float[] _originalWidths = new float[4];

        private bool _needUpdateDescNextFrame = false;

        [Header("描述的数字颜色")]
        [SerializeField]
        private Color32 PositiveColor = new Color32(44, 160, 35, 255);
        [SerializeField]
        private Color32 NegativeColor = new Color32(166, 62, 58, 255);
        [Header("炼制的数字颜色")]
        [SerializeField]
        private Color32 PositiveColor2 = new Color32();
        [SerializeField]
        private Color32 NegativeColor2 = new Color32();

        protected override void InitComponents()
        {
            base.InitComponents();
            SetCard(CardIds.Unknown);
            Bind(nameof(CardModel.Id), m =>
            {
                SetCard(m.As<CardModel>().Id);
                OnAdditionValueChanged(m);
            });
            Bind(nameof(CardModel.AdditionEffectVal1), OnAdditionValueChanged);
            Bind(nameof(CardModel.AdditionEffectVal2), OnAdditionValueChanged);
            Bind(nameof(CardModel.AdditionEffectVal3), OnAdditionValueChanged);
            textDesc.onLanguageChanged.AddListener(() =>
            {
                if (_current == null) return;
                if (DataModel is CardModel cardModel)
                    SetCardDesc(_current.Effects, _current.EffectValue, cardModel.AdditionEffectVal1, cardModel.AdditionEffectVal2, cardModel.AdditionEffectVal3);
                else
                    SetCardDesc(_current.Effects, _current.EffectValue);
            });

            // 记录原始宽度
            for (int i = 0; i < goCosts.Length; i++)
            {
                if (goCosts[i] != null)
                {
                    var rt = goCosts[i].GetComponent<RectTransform>();
                    _originalWidths[i] = rt.rect.width;
                }
            }
        }
        protected override void OnDataModelChanged(DataModelBase dataModel)
        {
            base.OnDataModelChanged(dataModel);
            if (gameObject.activeInHierarchy)
                LateUpdateEffectValAndDesc();
        }
        private void OnAdditionValueChanged(DataModelBase m) => _needUpdateDescNextFrame = true;

        /// <summary>
        /// 设置并绑定卡牌模型
        /// </summary>
        /// <param name="id">卡牌ID</param>
        public void SetCard(CardIds id)
        {
            if (_obj != null) Destroy(_obj);
            _obj = Instantiate(ResourceSystem.Instance.LoadPrefab(id.Data().Icon), root);
            if (tipRegion) tipRegion.ClearTip();
            var card = id.Data();
            _current = card;
            textName.SetText(card.Name);
            if (tipRegion) TipUtils.GetTipsAndAddToRegion(card.Name.Local(), tipRegion);
            SetBaseTypeImage(card.Type);
            SetCardRarity(card.Rarity);
            if (DataModel is CardModel cardModel)
                SetCardDesc(card.Effects, card.EffectValue, cardModel.AdditionEffectVal1, cardModel.AdditionEffectVal2, cardModel.AdditionEffectVal3);
            else
                SetCardDesc(card.Effects, card.EffectValue);
            var costData = BattleLogic.ConvertCost(card.Cost);
            SetCardCost(costData);
            for (var i = 0; i < cardLabelImg.Length; i++)
                cardLabelImg[i].gameObject.SetActive(card.Label.Contains((CardLabel)i));
        }
        public void SetCardEffectValues(int additionValue1, int additionValue2, int additionValue3)
        {
            // 效果值1
            var total1 = additionValue1 + (_current?.EffectValue[0] ?? 0);
            var color1 = GetEffectValueColor(_current?.EffectValue[0] ?? 0, additionValue1, textCardEffectVal1);
            using var str1 = total1.ToColoredString(color1);
            textCardEffectVal1.SetText(str1);

            // 效果值2
            var total2 = additionValue2 + (_current?.EffectValue[1] ?? 0);
            var color2 = GetEffectValueColor(_current?.EffectValue[1] ?? 0, additionValue2, textCardEffectVal2);
            using var str2 = total2.ToColoredString(color2);
            textCardEffectVal2.SetText(str2);

            // 效果值3
            var total3 = additionValue3 + (_current?.EffectValue[2] ?? 0);
            var color3 = GetEffectValueColor(_current?.EffectValue[2] ?? 0, additionValue3, textCardEffectVal3);
            using var str3 = total3.ToColoredString(color3);
            textCardEffectVal3.SetText(str3);

            if (DataModel is not CardModel cardModel) return;
            var card = cardModel.Id.Data();
            SetCardDesc(card.Effects, card.EffectValue, additionValue1, additionValue2, additionValue3);
        }
        private void SetCardDesc(EffectIds[] effectIds, int[] effectArgs) => SetCardDesc(effectIds, effectArgs, 0, 0, 0);
        private void SetCardDesc(EffectIds[] effectIds, int[] effectArgs, int additionVal1, int additionVal2, int additionVal3)
        {
            var sb = new StringBuilder();
            Span<int> additionVals = stackalloc int[] { additionVal1, additionVal2, additionVal3 };

            for (var i = 0; i < Math.Min(effectIds.Length, effectArgs.Length); i++)
            {
                var effectDesc = LocalizeSystem.Instance.GetLocalText(effectIds[i].Data().Desc);
                var total = effectArgs[i] + additionVals[i];

                var color = GetDescColor(effectArgs[i], additionVals[i]);
                using var effectArgStr = total.ToColoredString(color);

                sb.Append(string.Format(effectDesc, effectArgStr.ToString()));
                sb.AppendLine(LocalizeIds.Card_Separator.Local());
            }

            textDesc.SetRawText(sb.ToString());
            if (tipRegion) TipUtils.GetTipsAndAddToRegion(sb.ToString(), tipRegion);
        }

        private void SetCardRarity(CardRarity cardRarity)
        {
            basicImage.gameObject.SetActive(cardRarity == CardRarity.Basic);
            rarityImage.gameObject.SetActive(cardRarity == CardRarity.Rare);
            shopImage.gameObject.SetActive(cardRarity == CardRarity.Shop);
        }

        /// <summary>
        /// 设置卡牌底色，根据卡牌类型
        /// </summary>
        /// <param name="type">卡牌类型</param>
        private void SetBaseTypeImage(CardBaseType type)
        {
            if (cardBgFire) cardBgFire.SetActive(type == CardBaseType.Fire);
            if (cardBgWater) cardBgWater.SetActive(type == CardBaseType.Water);
            if (cardBgEarth) cardBgEarth.SetActive(type == CardBaseType.Earth);
            if (cardBgAir) cardBgAir.SetActive(type == CardBaseType.Air);
            if (cardBgFireWater) cardBgFireWater.SetActive(type == CardBaseType.Fire_Water);
            if (cardBgFireEarth) cardBgFireEarth.SetActive(type == CardBaseType.Fire_Earth);
            if (cardBgFireAir) cardBgFireAir.SetActive(type == CardBaseType.Fire_Air);
            if (cardBgWaterEarth) cardBgWaterEarth.SetActive(type == CardBaseType.Water_Earth);
            if (cardBgWaterAir) cardBgWaterAir.SetActive(type == CardBaseType.Water_Air);
            if (cardBgEarthAir) cardBgEarthAir.SetActive(type == CardBaseType.Earth_Air);
        }

        /// <summary>
        /// 根据 CardCost 数据动态生成费用图标视图
        /// 火🔥 水💦 土⛰ 风💨
        /// </summary>
        private void SetCardCost(CardCost cost)
        {
            // 火费用
            UpdateCostElement(
                type: CardBaseType.Fire,
                costValue: cost.Fire,
                symbol: "🔥"
            );

            // 水费用
            UpdateCostElement(
                type: CardBaseType.Water,
                costValue: cost.Water,
                symbol: "💦"
            );

            // 土费用
            UpdateCostElement(
                type: CardBaseType.Earth,
                costValue: cost.Earth,
                symbol: "⛰"
            );

            // 风费用
            UpdateCostElement(
                type: CardBaseType.Air,
                costValue: cost.Air,
                symbol: "💨"
            );
        }

        /// <summary>
        /// 统一更新费用元素显示逻辑
        /// </summary>
        /// <param name="type">费用类型</param>
        /// <param name="costValue">费用数值</param>
        /// <param name="symbol">显示符号</param>
        private void UpdateCostElement(CardBaseType type, int costValue, string symbol)
        {
            int index = (int)type;

            // 控制显隐
            bool shouldShow = costValue > 0;
            goCosts[index].SetActive(shouldShow);

            // 更新文本
            textCosts[index].text = shouldShow ?
                string.Concat(Enumerable.Repeat(symbol, costValue)) :
                string.Empty;

            // 调整容器宽度
            if (shouldShow)
            {
                var rt = goCosts[index].GetComponent<RectTransform>();
                rt.SetSizeWithCurrentAnchors(
                    axis: RectTransform.Axis.Horizontal,
                    size: _originalWidths[index] * costValue
                );
            }
        }

        private void LateUpdate()
        {
            LateUpdateEffectValAndDesc();
        }
        // 确保每帧只刷新一次描述的屎山，不然初始化卡牌会重复生成三次描述
        private void LateUpdateEffectValAndDesc()
        {
            if (!_needUpdateDescNextFrame) return;
            if (DataModel.As<CardModel>() != null)
                SetCardEffectValues(
                    DataModel.As<CardModel>().AdditionEffectVal1,
                    DataModel.As<CardModel>().AdditionEffectVal2,
                    DataModel.As<CardModel>().AdditionEffectVal3);
            _needUpdateDescNextFrame = false;
        }

        private Color32 GetEffectValueColor(int original, int addition, TMP_Text targetText)
        {
            if (addition > 0) return PositiveColor2;
            if (addition < 0) return NegativeColor2;
            return targetText.color;
        }

        private Color32 GetDescColor(int original, int addition)
        {
            if (addition > 0) return PositiveColor;
            if (addition < 0) return NegativeColor;
            return cardDesc.color;
        }
    }
}