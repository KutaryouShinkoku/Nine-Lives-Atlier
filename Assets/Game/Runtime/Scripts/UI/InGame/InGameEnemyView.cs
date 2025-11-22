using System.Threading;
using Cysharp.Threading.Tasks;
using Game.Data;
using Game.Localization.Components;
using Game.Model;
using Game.Audio;
using Game.Logic;
using Game.Model.InGameSubModel;
using Game.UI.Common;
using Game.UI.Common.Components;
using Game.UI.InGame.Components;
using JetBrains.Annotations;
using TMPro;
using UnityEngine;
using UnityEngine.Serialization;
using UnityEngine.TextCore.Text;
using UnityEngine.UI;
using WanFramework.Resource;
using WanFramework.UI;
using WanFramework.UI.Components;
using WanFramework.UI.DataComponent;
using WanFramework.Utils;

namespace Game.UI.InGame
{
    public class InGameEnemyView : RootView
    {
        [SerializeField]
        private TMP_Text textAttack;
        [SerializeField]
        private TMP_Text textHealth;
        [SerializeField]
        private TMP_Text textReward;

        [SerializeField]
        private UniAnimation animAttack;

        [SerializeField]
        private UniAnimation animTakeDamage;

        [SerializeField]
        private UniAnimation animTakeBuffDamage;

        [SerializeField]
        private UniAnimation animDeath;

        [SerializeField]
        private UIBuffCollectionView enemyBuffCollectionView;

        public TMP_Text GetAttackText() => textAttack;

        public TMP_Text GetHealthText() => textHealth;

        public TMP_Text GetRewardText() => textReward;

        public UIBuffCollectionView GetBuffCollectionView() => enemyBuffCollectionView;


        [CanBeNull]
        private GameObject _enemyObj;

        public CommonUIBuffView GetEnemyBuff()
        {
            var views = enemyBuffCollectionView.GetComponentsInChildren<CommonUIBuffView>();
            return views.Length > 0 ? views[0] : null;
        }

        protected override void InitComponents()
        {
            base.InitComponents();
            Bind(nameof(EnemyModel.EnemyId), m => SetEnemy(m.As<EnemyModel>().EnemyId));
            Bind(nameof(EnemyModel.Health), m => SetHealth(m.As<EnemyModel>().Health));
            Bind(nameof(EnemyModel.Attack), m => SetAttack(m.As<EnemyModel>().Attack));
            Bind(nameof(EnemyModel.Reward), m => SetReward(m.As<EnemyModel>().Reward));
        }

        protected override void OnDataModelChanged(DataModelBase dataModel)
        {
            base.OnDataModelChanged(dataModel);
            enemyBuffCollectionView.ItemSource = dataModel?.As<EnemyModel>()?.enemyBuffs;
        }

        public void SetEnemy(EnemyIds enemy)
        {
            if (_enemyObj != null) Destroy(_enemyObj);

            _enemyObj = Instantiate(ResourceSystem.Instance.LoadPrefab(enemy.Data().Prefab), this.transform);
            var animator = _enemyObj.GetComponent<Animator>();
            animAttack.SetAnimator(animator);
            animTakeDamage.SetAnimator(animator);
            animTakeBuffDamage.SetAnimator(animator);
            animDeath.SetAnimator(animator);

            var entry = enemy.Data();
            if (entry.EnemyAttributes.Length < 3) return;
            SetAttack(entry.EnemyAttributes[0]);
            SetHealth(entry.EnemyAttributes[1]);
            SetReward(entry.EnemyAttributes[2]);
        }

        public void SetHealth(int health)
        {
            textHealth.SetText(health.ToString());
        }

        public void SetAttack(int attack)
        {
            textAttack.SetText(attack.ToString());
        }

        public void SetReward(int reward)
        {
            textReward.SetText(reward.ToString());
        }

        public async UniTask PlayAttackAnim(CancellationToken token) 
        {
            AudioSystem.Instance.SendEvent(AudioIds.Effect_Enemy_Attack);
            await animAttack.Play(token);
        }

        public async UniTask PlayTakeDamageAnim(CancellationToken token)
        {
            Vector3 enemyRoot = _enemyObj?.transform.position ?? Vector3.zero;
            EffectView.ShowEffect("Arts/Effects/Damage", 1,enemyRoot - new Vector3(25,25,0), enemyRoot + new Vector3(25, 25, 0), token).Forget();
            AudioSystem.Instance.SendEvent(AudioIds.Effect_Enemy_Damaged_S);
            await animTakeDamage.Play(token);
        }

        public async UniTask PlayTakeBuffDamageAnim(CancellationToken token)
        {
            await animTakeBuffDamage.Play(token);
        }

        public async UniTask PlayDeathAnim(CancellationToken token)
        {
            AudioSystem.Instance.SendEvent(AudioIds.Effect_Enemy_Die);
            await animDeath.Play(token); 
        } 
    }
}