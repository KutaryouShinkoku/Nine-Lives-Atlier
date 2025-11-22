
using UnityEngine;
#if ENABLE_INPUT_SYSTEM
using UnityEngine.InputSystem;
#endif

namespace WanFramework.Base
{
    public static class InputHelper
    {
        public static PointerType GetPointerType()
        {
#if ENABLE_INPUT_SYSTEM
            switch (Pointer.current)
            {
                case Touchscreen:
                    return PointerType.Touch;
                case Mouse:
                    return PointerType.Mouse;
                case Pen:
                    return PointerType.Pen;
                default:
                    return PointerType.Mouse;
            }
#else
            return PointerType.Mouse;
#endif
        }
        public static bool AnyKeyDown() =>
            Keyboard.current.anyKey.wasPressedThisFrame ||
            Pointer.current.press.wasPressedThisFrame;
        public static bool AnyKeyUp() =>
            Keyboard.current.anyKey.wasReleasedThisFrame ||
            Pointer.current.press.wasReleasedThisFrame;
    }
}