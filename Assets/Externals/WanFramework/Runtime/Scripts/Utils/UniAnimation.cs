using System;
using System.Threading;
using Cysharp.Threading.Tasks;
using UnityEngine;

namespace WanFramework.Utils
{
    [Serializable]
    public struct UniAnimation
    {
        [SerializeField]
        private string name;
        [SerializeField]
        private Animator animator;
        [SerializeField]
        private float waitTime;
        
        public async UniTask Play(CancellationToken token)
        {
            if (animator)
                animator.SetTrigger(name);
            else
                Debug.LogWarning("Animator is null");
            if (waitTime == 0) return;
            await UniTask.WaitForSeconds(waitTime, cancellationToken: token, cancelImmediately: true);
        }

        public void SetAnimator(Animator animator)
        {
            this.animator = animator;
        }

        public void Cancel() => animator.ResetTrigger(name);
    }
}