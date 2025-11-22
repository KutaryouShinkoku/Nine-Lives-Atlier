using System;
using Game.Audio;
using Game.Data;
using JetBrains.Annotations;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.Controls;
using UnityEngine.InputSystem.LowLevel;
using UnityEngine.InputSystem.Utilities;

namespace Game.Utils
{
    public static class MouseUtils
    {
        [CanBeNull]
        private static InputAction _pointerPressAction;
        private static AudioIds _mouseClickAudio;
        private static void PlayPointerClickAudio() => AudioSystem.Instance.SendEvent(_mouseClickAudio);
        private static void OnPointerDownAction(InputAction.CallbackContext context) => PlayPointerClickAudio();
        public static void SetMouseClickAudio(AudioIds eventName)
        {
            if (_pointerPressAction == null)
            {
                _pointerPressAction = new InputAction(type: InputActionType.Button, binding: "<Pointer>/press");
                _pointerPressAction.performed += OnPointerDownAction;
                _pointerPressAction.Enable();
            }
            _mouseClickAudio = eventName;
        }
    }
}