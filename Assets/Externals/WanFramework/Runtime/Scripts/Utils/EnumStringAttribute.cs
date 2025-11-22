using System;
using UnityEngine;

namespace WanFramework.Utils
{
    [AttributeUsage(AttributeTargets.Field)]
    public class EnumStringAttribute : PropertyAttribute
    {
        public readonly Type EnumType;

        public EnumStringAttribute(Type enumType)
        {
            EnumType = enumType;
        }
    }
}