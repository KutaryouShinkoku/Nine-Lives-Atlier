using System.Collections;
using System.IO;
using System.Threading.Tasks;
using Cysharp.Threading.Tasks;
using Game.BattleAnim;
using Game.Logic;
using Game.Model;
using NUnit.Framework;
using UnityEditor;
using UnityEngine.TestTools;
using WanFramework.Data;
using WanFramework.Resource;
using WanFramework.Sequence;
using WanFramework.UI;
using WanFramework.UI.DataComponent;

namespace Game.Tests
{
    public class BattleTests
    {
        private const string TestRoot = "Assets/Game/Tests/BattleTests";
        private void LoadModel(BattleModel m, string jsonPath)
        {
            m.Deserialize(File.ReadAllText(Path.Join(TestRoot, jsonPath)));
        }
        [UnitySetUp]
        public IEnumerator SetUp()
        {
            yield return GameSystemHelper.RequireSystem<SequenceSystem>().ToCoroutine();
            yield return GameSystemHelper.RequireSystem<ResourceSystem>().ToCoroutine();
            yield return GameSystemHelper.RequireSystem<UISystem>().ToCoroutine();
            yield return GameSystemHelper.RequireSystem<DataSystem>().ToCoroutine();
            yield return GameSystemHelper.RequireSystem<BattleAnimSystem>().ToCoroutine();
        }

        [UnityTearDown]
        public void TearDown()
        {
            GameSystemHelper.DestroyAllSystems();
        }
        private bool IsBattleModelEqualTo(BattleModel a, BattleModel b)
            => a.Serialize() == b.Serialize();
        
        [Test]
        public void BattleExampleTests()
        {
            LoadModel(DataModel<BattleModel>.Instance, "ExampleTest.json");
            var correctBattleModel = new BattleModel();
            LoadModel(correctBattleModel, "ExampleTest.json");
            BattleLogic.UseCardFromHand(0); // 资源不足，没变化
            Assert.True(IsBattleModelEqualTo(correctBattleModel, DataModel<BattleModel>.Instance));
        }

        [Test]
        public void BattleTest2()
        {
            // 第二项测试
            Assert.Pass();
        }
    }
}
