using System.Collections.Generic;
using System.Threading;
using Cysharp.Threading.Tasks;
using Game.Data;
using Game.Audio;
using Game.Localization.Components;
using JetBrains.Annotations;
using UnityEngine;
using WanFramework.Resource;
using WanFramework.UI;

namespace Game.UI.InGame
{
    public class InGameUITutorialCover : UIRootView
    {
        [SerializeField]
        [Tooltip("黑猫可能的位置列表")]
        private List<Transform> dialogSlot;
        
        [SerializeField]
        [Tooltip("黑猫对话框")]
        private Transform dialog;

        [SerializeField]
        [Tooltip("消息框")]
        private LocalizeText textMessage;

        [SerializeField]
        private GameObject messageTail;
        
        [SerializeField]
        private Transform vfxRoot;

        [CanBeNull]
        private GameObject _currentVfx;
        
        public void ShowVfx(string vfxPath)
        {
            if (_currentVfx != null)
                Destroy(_currentVfx);
            if (string.IsNullOrEmpty(vfxPath)) return;
            _currentVfx = Instantiate(ResourceSystem.Instance.LoadPrefab(vfxPath), vfxRoot, false)!;
            _currentVfx!.transform.localPosition = Vector3.zero;
            AudioSystem.Instance.SendEvent(AudioIds.UI_AVG_Swich);
        }
        
        public void SetMessage(LocalizeIds message, int slot)
        {
            textMessage.SetText(message);
            dialog.transform.SetParent(dialogSlot[slot], false);
            dialog.transform.localPosition = Vector3.zero;
        }
        public void SetTailVisible(bool isTailVisible) => messageTail.SetActive(isTailVisible);
        /// <summary>
        /// 播放蹦字特效
        /// </summary>
        /// <returns></returns>
        public UniTask PlayMessagePrinterAnim(CancellationToken token)
        {
            return UniTask.WaitForSeconds(0.5f, cancellationToken: token);
        }
    }
}