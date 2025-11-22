using System.Threading;
using Game.UI.InGame.Components;
using UnityEditor;
using UnityEngine;

namespace Game.Editor.UI.InGame.Components
{
    [CustomEditor(typeof(HandCardsCollectionView))]
    public class HandCardsCollectionViewEditor : UnityEditor.Editor
    {
        private bool _showWaveShake = false;
        private float _waveShakeTime = 5;
        private float _waveShakeAmplitude = 15;
        private float _waveShakeFrequency = 0.25f;
        private float _waveShakeTScale = 10;
        private bool _waveShakeShrink = true;
        private float _waveShakeCenter = 0.5f;
        
        private bool _showExplosion = false;
        private float _explosionTime = 0.25f;
        private float _explosionPower = 5f;
        private Vector3 _explosionCenter = new(0, 10, 0);
        private float _explosionMaxDistance = 200;
        
        public override void OnInspectorGUI()
        {
            base.OnInspectorGUI();
            GUILayout.Label("动效测试");
            _showWaveShake = EditorGUILayout.Foldout(_showWaveShake, "Wave Shake");
            if (_showWaveShake) WaveShakeAnimGUI();
            _showExplosion = EditorGUILayout.Foldout(_showExplosion, "Explosion");
            if (_showExplosion) ExplosionAnimGUI();
        }

        private void WaveShakeAnimGUI()
        {
            GUILayout.Label("Wave Shake");
            GUILayout.BeginVertical();
            _waveShakeTime = EditorGUILayout.FloatField("时间", _waveShakeTime);
            _waveShakeAmplitude = EditorGUILayout.FloatField("幅值", _waveShakeAmplitude);
            _waveShakeFrequency = EditorGUILayout.FloatField("频率", _waveShakeFrequency);
            _waveShakeTScale = EditorGUILayout.FloatField("时间缩放", _waveShakeTScale);
            _waveShakeTScale = EditorGUILayout.FloatField("时间缩放", _waveShakeTScale);
            _waveShakeShrink = EditorGUILayout.Toggle("跟随时间缩放", _waveShakeShrink);
            _waveShakeCenter = EditorGUILayout.FloatField("中心", _waveShakeCenter);
            if (GUILayout.Button("Play"))
                (target as HandCardsCollectionView)?.PlayWaveShakeCardsAnim(
                    CancellationToken.None, _waveShakeTime, _waveShakeAmplitude,
                    _waveShakeFrequency, _waveShakeTScale, _waveShakeShrink, _waveShakeCenter);
            GUILayout.EndVertical();
        }
        private void ExplosionAnimGUI()
        {
            GUILayout.Label("Explosion");
            GUILayout.BeginVertical();
            _explosionTime = EditorGUILayout.FloatField("时间", _explosionTime);
            _explosionCenter = EditorGUILayout.Vector3Field("爆炸中心", _explosionCenter);
            _explosionPower = EditorGUILayout.FloatField("爆炸强度", _explosionPower);
            _explosionMaxDistance = EditorGUILayout.FloatField("最大距离", _explosionMaxDistance);
            if (GUILayout.Button("Play"))
                (target as HandCardsCollectionView)?.PlayExplosionAnim(
                    CancellationToken.None, _explosionCenter, _explosionTime, _explosionPower, _explosionMaxDistance);
            GUILayout.EndVertical();
        }
    }
}