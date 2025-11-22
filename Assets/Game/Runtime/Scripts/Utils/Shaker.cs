using UnityEngine;

namespace Game.Utils
{
    public class Shaker : MonoBehaviour
    {
        [SerializeField]
        [Tooltip("幅度")]
        private float amplify = 1.0f;
        
        private Vector3 _originalPos;
        private float _curShakeTime;
        private float _shakeTotalTime;

        public void Shake(float time)
        {
            _curShakeTime = 0;
            _shakeTotalTime = time;
        }
        private void Awake() => _originalPos = transform.position;
        private void Update()
        {
            if (_curShakeTime >= _shakeTotalTime) return;
            _curShakeTime += Time.deltaTime;
            var offset = new Vector3(Random.value, Random.value, Random.value) * amplify * (_shakeTotalTime - _curShakeTime) / _shakeTotalTime;
            transform.position = _originalPos + offset;
        }
    }
}