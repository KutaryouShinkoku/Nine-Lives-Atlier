using System;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using Unity.Collections.LowLevel.Unsafe;

namespace WanFramework.Utils
{
    public static class ListExtensions
    {
        private class ListPrivateFieldAccess<T>
        {
#pragma warning disable CS0649
#pragma warning disable CS8618
            internal T[] _items; // Do not rename (binary serialization)
#pragma warning restore CS8618
            internal int _size; // Do not rename (binary serialization)
            internal int _version; // Do not rename (binary serialization)
#pragma warning restore CS0649
        }
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        static ListPrivateFieldAccess<T> GetPrivateFieldsUnsafe<T>(this List<T> list)
            => UnsafeUtility.As<List<T>, ListPrivateFieldAccess<T>>(ref list);
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        static T[] GetInternalArrayUnsafe<T>(this List<T> list)
            => list.GetPrivateFieldsUnsafe()._items;
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        static Span<T> AsSpan<T>(this List<T> list)
            => list.GetInternalArrayUnsafe().AsSpan(0, list.Count);
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static ref T GetRef<T>(this List<T> list, int index)
            => ref AsSpan(list)[index];
    }
}