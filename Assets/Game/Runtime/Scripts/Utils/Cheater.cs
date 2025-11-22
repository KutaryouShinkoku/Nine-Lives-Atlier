using System;
using Game.Data;
using Game.Logic;
using Game.Model;
using Game.Model.InGameSubModel;
using UnityEngine;
using UnityEngine.InputSystem;
using WanFramework.UI.DataComponent;

namespace Game.Utils
{
    public class Cheater : MonoBehaviour
    {
        public CardIds cheatCardID;

        private static readonly Key[] CheatCodes =
        {
            Key.B, Key.L, Key.A, Key.C, Key.K,
            Key.C, Key.A, Key.T
        };
        private int _curCodeState;
        
        private Rect _windowRect = new(20, 20, 360, 200);

        private void Start()
        {
            #if UNITY_EDITOR
            _curCodeState = CheatCodes.Length;
            #endif
        }
        private void Update()
        {
            if (_curCodeState >= CheatCodes.Length) return;
            if (!Keyboard.current.anyKey.wasPressedThisFrame) return;
            if (Keyboard.current[CheatCodes[_curCodeState]].wasPressedThisFrame) ++_curCodeState;
            else _curCodeState = 0;
        }
        private void OnGUI()
        {
            if (_curCodeState < CheatCodes.Length) return;
            var scale = Screen.height / 600.0f;
            GUI.matrix = Matrix4x4.TRS(Vector3.zero, Quaternion.identity, new Vector3
                (scale, scale, 1.0f));
            _windowRect = GUILayout.Window(0, _windowRect, DrawWindow, "I AM CHEATER!");
        }
        private void DrawWindow(int windowID)
        {
            GUI.DragWindow(new Rect(0, 0, 10000, 20));
            GUILayout.BeginVertical();
            #if UNITY_EDITOR
            GUILayout.Label("哦，在编辑器里作弊，那没事了");
            #endif
            if (GUILayout.Button("Kill them all!", new GUIStyle(GUI.skin.button)))
                KillEnemy();
            if (GUILayout.Button("Give me money!", new GUIStyle(GUI.skin.button)))
                GiveMoney();
            if (GUILayout.Button("Give me resources!", new GUIStyle(GUI.skin.button)))
                GiveResources();
            if (GUILayout.Button("Give me card!", new GUIStyle(GUI.skin.button)))
                GiveCard();
            if (GUILayout.Button("Discard my cards!", new GUIStyle(GUI.skin.button)))
                DiscardCards();
            if (GUILayout.Button("Goodbye~", new GUIStyle(GUI.skin.button)))
                CloseCheater();
            GUILayout.EndVertical();
        }

        private void KillEnemy()
        {
            DataModel<BattleModel>.Instance.EnemyModel.Health = 0;
        }
        private void GiveMoney()
        {
            DataModel<InGameModel>.Instance.Gold += 1000;
        }
        private void GiveResources()
        {
            DataModel<BattleModel>.Instance.PlayerModel.Resources = new CardCost()
            {
                Fire = 10,
                Water = 10,
                Earth = 10,
                Air = 10,
            };
        }

        private void GiveCard()
        {
            DataModel<BattleModel>.Instance.HandCards.Add(new CardModel
                    {
                        Id = cheatCardID,
                        AdditionEffectVal1 = 0,
                        AdditionEffectVal2 = 0,
                        AdditionEffectVal3 = 0,
                        FeeChange = new CardCost
                        {
                            Air = 0,
                            Earth = 0,
                            Fire = 0,
                            Water = 0,
                        }
                    });
        }

        private void DiscardCards()
        {
            EffectLogic.DiscardCards(1);
        }

        private void CloseCheater()
        {
            _curCodeState = 0;
        }
    }
}