using System;
using System.Collections.Generic;
using System.Threading;
using Cysharp.Threading.Tasks;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using WanFramework.UI;
using Game.Data;
using Game.UI.Tip;
using JetBrains.Annotations;
using WanFramework.Resource;
using WanFramework.Utils;
using Game.Model.InGameSubModel;
using WanFramework.UI.DataComponent;

namespace Game.UI.Common.Components
{
    public class CommonUIBuffView : SubView
    {
        [SerializeField]
        private CanvasGroup _canvasGroup;

        [SerializeField]
        private UniAnimation buffAddStack;

        [SerializeField]
        private UniAnimation buffMinusStack;

        [SerializeField]
        private UniAnimation buffEffect;

        [SerializeField]
        private UniAnimation buffShow;

        [SerializeField]
        private TMP_Text textStack;
        [SerializeField]
        private RectTransform root;
        [SerializeField]
        [CanBeNull]
        private TipProviderRegion tipRegion;

        public TMP_Text TextStack => textStack;


        [CanBeNull]
        private GameObject _obj;

        private Dictionary<string, Func<string>> _tipParams = new();
        
        public UniTask PlayBuffStackAddAnim(CancellationToken token) => buffAddStack.Play(token);
        public UniTask PlayBuffStackMinusAnim(CancellationToken token) => buffMinusStack.Play(token);
        public UniTask PlayBuffEffect(CancellationToken token) => buffEffect.Play(token);
        public UniTask PlayBuffShow(CancellationToken token) => buffShow.Play(token);

        protected override void InitComponents()
        {
            base.InitComponents();
            Bind(nameof(BuffModel.Stack), m => SetStack(m.As<BuffModel>()?.Stack ?? 0));
            Bind(nameof(BuffModel.Id), m => SetBuff(m.As<BuffModel>()?.Id ?? BuffIds.Unknown));
            _tipParams["buffStack"] = () => textStack?.text ?? "0";
        }
        public void SetStack(int stack)
        {
            textStack.SetText(stack.ToString());
        }
        
        public void SetBuff(BuffIds buffId)
        {
            if (_obj != null) Destroy(_obj);
            _obj = Instantiate(ResourceSystem.Instance.LoadPrefab(buffId.Data().Prefab), root);
            if (tipRegion)
            {
                tipRegion.ClearTip();
                var buffData = buffId.Data();
                tipRegion.AddTip(new TipBoxData(buffData.Name, buffData.Desc, Color.red, _tipParams));
            }
        }

    }
}
