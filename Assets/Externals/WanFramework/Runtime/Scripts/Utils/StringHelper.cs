using System;
using System.Buffers;
using TMPro;
using UnityEngine;

namespace WanFramework.Utils
{
    /// <summary>
    /// 主要负责实现从数字到字符串的0GC转换
    /// </summary>
    public static class StringHelper
    {
        public static DisposableString ToStringNoGC(this int num)
        {
            var str = new DisposableString(16);
            ConvertFrom(ref str, num);
            return str;
        }
        
        public static void ConvertFrom(ref DisposableString str, int num)
        {
            var array = str.GetArray();
            var i = 0;
            var negative = false;
            if (num < 0)
            {
                negative = true;
                num = -num;
            }
            if (num == 0)
            {
                array[i++] = '0';
            }
            else
            {
                while (num > 0)
                {
                    array[i++] = (char)('0' + num % 10);
                    num /= 10;
                }
                if (negative) array[i++] = '-';
                Array.Reverse(array, 0, i);
            }
            str.Length = i;
        }
    }

    public static class TMPTextExtensions
    {
        public static void SetText(this TMP_Text text, DisposableString str)
        {
            text.SetText(str.GetArray(), 0, str.Length);
        }
    }
    public struct DisposableString : IDisposable
    {
        private static ArrayPool<char> _pool = ArrayPool<char>.Create();
        private readonly char[] _array;
        
        public int Length;
        
        public DisposableString(int capacity)
        {
            _array = _pool.Rent(capacity);
            Length = 0;
        }
        public char[] GetArray() => _array;
        public void Dispose()
        {
            if (_array != null)
                _pool.Return(_array);
        }

        public override string ToString()
        {
            var span = new ReadOnlySpan<char>(_array, 0, Length);
            return span.ToString();
        }
    }

    public static class ColorStringHelper
    {
        private const int MinBufferSize = 64;

        public static DisposableString ToColoredString(this int num, Color32 color)
        {
            var str = new DisposableString(MinBufferSize);
            var buffer = str.GetArray();
            int index = 0;

            // 写入开始颜色标签
            index = WriteColorStartTag(buffer, index, color);

            // 转换数字部分
            index = ConvertNumber(buffer, index, num);

            // 写入结束标签
            index = WriteColorEndTag(buffer, index);

            str.Length = index;
            return str;
        }

        private static int WriteColorStartTag(char[] buffer, int index, Color32 color)
        {
            const string prefix = "<color=#";
            foreach (var c in prefix)
            {
                if (index >= buffer.Length) break;
                buffer[index++] = c;
            }

            // 写入RGBA
            WriteHexByte(buffer, ref index, color.r);
            WriteHexByte(buffer, ref index, color.g);
            WriteHexByte(buffer, ref index, color.b);
            WriteHexByte(buffer, ref index, color.a);

            // 闭合标签
            if (index < buffer.Length) buffer[index++] = '>';
            return index;
        }

        private static int WriteColorEndTag(char[] buffer, int index)
        {
            const string endTag = "</color>";
            foreach (var c in endTag)
            {
                if (index >= buffer.Length) break;
                buffer[index++] = c;
            }
            return index;
        }

        private static int ConvertNumber(char[] buffer, int startIndex, int num)
        {
            int index = startIndex;
            bool negative = false;

            if (num < 0)
            {
                negative = true;
                num = -num;
            }

            Span<char> temp = stackalloc char[16];
            int tempIndex = 0;

            do
            {
                temp[tempIndex++] = (char)('0' + num % 10);
                num /= 10;
            } while (num > 0);

            if (negative)
            {
                temp[tempIndex++] = '-';
            }

            for (int i = tempIndex - 1; i >= 0; i--)
            {
                if (index >= buffer.Length) break;
                buffer[index++] = temp[i];
            }

            return index;
        }

        private static void WriteHexByte(char[] buffer, ref int index, byte value)
        {
            buffer[index] = NibbleToHex((byte)(value >> 4));
            if (++index >= buffer.Length) return;

            buffer[index] = NibbleToHex((byte)(value & 0xF));
            if (++index >= buffer.Length) return;
        }

        private static char NibbleToHex(byte nibble)
        {
            return (char)(nibble < 10 ?
                '0' + nibble :
                'A' + (nibble - 10));
        }

    }
}