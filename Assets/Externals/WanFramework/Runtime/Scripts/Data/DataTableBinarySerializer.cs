using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using System.Runtime.CompilerServices;
using UnityEngine;

namespace WanFramework.Data
{
    /// <summary>
    /// 序列化辅助类
    /// </summary>
    public static partial class DataTableBinarySerializer
    {
        public class SerializeInfo
        {
            public MethodInfo Serializer;
            public MethodInfo Deserializer;
            public SerializeInfo(string serializer, string deserializer)
            {
                Serializer = typeof(DataTableBinarySerializer).GetMethod(serializer) ?? throw new MissingMethodException(serializer);
                Deserializer = typeof(DataTableBinarySerializer).GetMethod(deserializer) ?? throw new MissingMethodException(deserializer);
            }
            public SerializeInfo(string typeName) : this($"Serialize{typeName}", $"Deserialize{typeName}")
            {
            }
        }
        public static Dictionary<Type, SerializeInfo> serializeInfos = new()
        {
            { typeof(byte), new SerializeInfo("Byte") },
            { typeof(short), new SerializeInfo("Int16") },
            { typeof(int), new SerializeInfo("Int32") },
            { typeof(long), new SerializeInfo("Int64") },
            { typeof(sbyte), new SerializeInfo("SByte") },
            { typeof(ushort), new SerializeInfo("UInt16") },
            { typeof(uint), new SerializeInfo("UInt32") },
            { typeof(ulong), new SerializeInfo("UInt64") },
            { typeof(bool), new SerializeInfo("Bool") },
            { typeof(decimal), new SerializeInfo("Decimal") },
            { typeof(float), new SerializeInfo("Single") },
            { typeof(double), new SerializeInfo("Double") },
            { typeof(string), new SerializeInfo("String") },
            { typeof(Color), new SerializeInfo("UnityColor") },
            { typeof(Vector2), new SerializeInfo("UnityVector2") },
            { typeof(Vector3), new SerializeInfo("UnityVector3") },
            { typeof(Vector4), new SerializeInfo("UnityVector4") },
            { typeof(Vector2Int), new SerializeInfo("UnityVector2Int") },
            { typeof(Vector3Int), new SerializeInfo("UnityVector3Int") },
        };
        public static void Serialize(BinaryWriter writer, object obj)
        {
            if (obj.GetType().IsArray)
                SerializeArray(writer, (Array)obj);
            else if (obj.GetType().IsEnum)
                SerializeEnum(writer, (Enum)obj);
            else
                GetSerializeInfo(obj.GetType()).Serializer.Invoke(null, new object[] { writer, obj });
        }
        public static object Deserialize(BinaryReader reader, Type type)
        {
            if (type.IsArray)
                return DeserializeArray(reader);
            if (type.IsEnum)
                return DeserializeEnum(reader, type);
            return GetSerializeInfo(type).Deserializer.Invoke(null, new object[] { reader });
        }
        public static T Deserialize<T>(BinaryReader reader) => (T)Deserialize(reader, typeof(T));
        public static SerializeInfo GetSerializeInfo(Type type)
        {
            if (!serializeInfos.TryGetValue(type, out var serializer))
                throw new Exception($"The serializer of type {type} not found");
            return serializer;
        }
    }
    /// <summary>
    /// 特殊内置类型序列化
    /// </summary>
    public static partial class DataTableBinarySerializer
    {
        public static void SerializeArray(BinaryWriter writer, Array objects)
        {
            writer.Write(objects.Length);
            foreach (var obj in objects)
                Serialize(writer, obj);
        }
        public static Array DeserializeArray(BinaryReader reader)
        {
            var len = reader.ReadInt32();
            var objects = new object[len];
            for (var i = 0; i < len; ++i)
                objects[i] = Deserialize(reader, typeof(object));
            return objects;
        }
        public static void SerializeEnum(BinaryWriter writer, Enum val)
        {
            var enumName = Enum.GetName(val.GetType(), val) ?? throw new MissingMemberException(val.ToString());
            writer.Write(enumName);
        }
        public static Enum DeserializeEnum(BinaryReader reader, Type enumType)
        {
            var str = reader.ReadString();
            return Enum.Parse(enumType, str) as Enum;
        }
    }
    /// <summary>
    /// CSharp内置类型
    /// </summary>
    public static partial class DataTableBinarySerializer
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void SerializeByte(BinaryWriter writer, byte val) => writer.Write(val);
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void SerializeInt16(BinaryWriter writer, short val) => writer.Write(val);
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void SerializeInt32(BinaryWriter writer, int val) => writer.Write(val);
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void SerializeInt64(BinaryWriter writer, long val) => writer.Write(val);
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void SerializeSByte(BinaryWriter writer, sbyte val) => writer.Write(val);
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void SerializeUInt16(BinaryWriter writer, ushort val) => writer.Write(val);
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void SerializeUInt32(BinaryWriter writer, uint val) => writer.Write(val);
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void SerializeUInt64(BinaryWriter writer, ulong val) => writer.Write(val);
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void SerializeBool(BinaryWriter writer, bool val) => writer.Write(val);
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void SerializeDecimal(BinaryWriter writer, decimal val) => writer.Write(val);
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void SerializeSingle(BinaryWriter writer, float val) => writer.Write(val);
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void SerializeDouble(BinaryWriter writer, double val) => writer.Write(val);
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void SerializeString(BinaryWriter writer, string val) => writer.Write(val);
        
        public static byte DeserializeByte(BinaryReader reader) => reader.ReadByte();
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static short DeserializeInt16(BinaryReader reader) => reader.ReadInt16();
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static int DeserializeInt32(BinaryReader reader) => reader.ReadInt32();
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static long DeserializeInt64(BinaryReader reader) => reader.ReadInt64();
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static sbyte DeserializeSByte(BinaryReader reader) => reader.ReadSByte();
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static ushort DeserializeUInt16(BinaryReader reader) => reader.ReadUInt16();
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static uint DeserializeUInt32(BinaryReader reader) => reader.ReadUInt32();
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static ulong DeserializeUInt64(BinaryReader reader) => reader.ReadUInt64();
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool DeserializeBool(BinaryReader reader) => reader.ReadBoolean();
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static decimal DeserializeDecimal(BinaryReader reader) => reader.ReadDecimal();
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static float DeserializeSingle(BinaryReader reader) => reader.ReadSingle();
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static double DeserializeDouble(BinaryReader reader) => reader.ReadDouble();
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static string DeserializeString(BinaryReader reader) => reader.ReadString();
    }

    /// <summary>
    /// Unity类型
    /// </summary>
    public static partial class DataTableBinarySerializer
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void SerializeUnityColor(BinaryWriter writer, Color val)
        {
            writer.Write(val.r);
            writer.Write(val.g);
            writer.Write(val.b);
            writer.Write(val.a);
        }
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void SerializeUnityVector2(BinaryWriter writer, Vector2 val)
        {
            writer.Write(val.x);
            writer.Write(val.y);
        }
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void SerializeUnityVector3(BinaryWriter writer, Vector3 val)
        {
            writer.Write(val.x);
            writer.Write(val.y);
            writer.Write(val.z);
        }
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void SerializeUnityVector4(BinaryWriter writer, Vector4 val)
        {
            writer.Write(val.x);
            writer.Write(val.y);
            writer.Write(val.z);
            writer.Write(val.w);
        }
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void SerializeUnityVector2Int(BinaryWriter writer, Vector2Int val)
        {
            writer.Write(val.x);
            writer.Write(val.y);
        }
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void SerializeUnityVector3Int(BinaryWriter writer, Vector3Int val)
        {
            writer.Write(val.x);
            writer.Write(val.y);
            writer.Write(val.z);
        }
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Color DeserializeUnityColor(BinaryReader reader)
            => new Color(reader.ReadSingle(), reader.ReadSingle(), reader.ReadSingle(), reader.ReadSingle());
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Vector2 DeserializeUnityVector2(BinaryReader reader)
            => new Vector2(reader.ReadSingle(), reader.ReadSingle());
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Vector3 DeserializeUnityVector3(BinaryReader reader)
            => new Vector3(reader.ReadSingle(), reader.ReadSingle(), reader.ReadSingle());
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Vector4 DeserializeUnityVector4(BinaryReader reader)
            => new Vector4(reader.ReadSingle(), reader.ReadSingle(), reader.ReadSingle(), reader.ReadSingle());
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Vector2Int DeserializeUnityVector2Int(BinaryReader reader)
            => new Vector2Int(reader.ReadInt32(), reader.ReadInt32());
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Vector3Int DeserializeUnityVector3Int(BinaryReader reader)
            => new Vector3Int(reader.ReadInt32(), reader.ReadInt32(), reader.ReadInt32());
    }
}