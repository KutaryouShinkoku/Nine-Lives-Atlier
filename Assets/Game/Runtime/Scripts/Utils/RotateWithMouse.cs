using Game.Model;
using System;
using UnityEngine;
using UnityEngine.InputSystem;
using WanFramework.Base;
using WanFramework.UI.DataComponent;
using Gyroscope = UnityEngine.InputSystem.Gyroscope;

namespace Game.Utils
{
    public class RotateWithMouse : MonoBehaviour
    {
        private Quaternion _originalRotation;
        [SerializeField]
        private float rotateScale = 0.01f;
        [SerializeField]
        private float mobileRotateScale = 1f;
        private Vector2 _smoothRotateVelocity;
        private Vector2 _smoothRotateOffset;
        private void Start()
        {
            _originalRotation = transform.localRotation;
        }
        private void Update()
        {
            if (InputHelper.GetPointerType() == PointerType.Mouse)
            {
                var screenSize = new Vector2(Screen.width, Screen.height);
                var refScreenSize = new Vector2(1920, 1080);
                var mousePosition = (Mouse.current.position.ReadValue() - screenSize / 2) / screenSize * refScreenSize;
                ApplyRotate(mousePosition * rotateScale);
                if (Gyroscope.current != null)
                    InputSystem.DisableDevice(Gyroscope.current);
            }
            else if (Gyroscope.current != null)
            {
                if (!Gyroscope.current.enabled)
                {
                    InputSystem.EnableDevice(Gyroscope.current);
                    Gyroscope.current.samplingFrequency = 16;
                }
                var acc = Gyroscope.current.angularVelocity.ReadValue();
                var offset = Vector2.ClampMagnitude(new Vector2(acc.y, acc.x), 1);
                ApplySmoothRotate(offset * mobileRotateScale);
            }
        }
        private void ApplySmoothRotate(Vector2 offset)
        {
            _smoothRotateOffset = Vector2.SmoothDamp(_smoothRotateOffset, offset, ref _smoothRotateVelocity, 0.2f);
            ApplyRotate(_smoothRotateOffset);
        }
        private void ApplyRotate(Vector2 offset)
        {
            float sensitivity = DataModel<SettingModel>.Instance.CameraShakeSensitivity;
            Vector2 adjustedOffset = offset * sensitivity;
            transform.localRotation = _originalRotation * Quaternion.Euler(-adjustedOffset.y, adjustedOffset.x, 0);
        }
    }
}