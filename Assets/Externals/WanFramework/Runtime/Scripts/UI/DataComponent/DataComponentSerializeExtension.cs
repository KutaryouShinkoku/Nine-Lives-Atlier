using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using Newtonsoft.Json;
using UnityEngine;
using UnityEngine.Events;

namespace WanFramework.UI.DataComponent
{
    public static class DataComponentSerializeExtension
    {
        private static readonly JsonSerializer Serializer;
        static DataComponentSerializeExtension()
        {
            Serializer = new JsonSerializer();
            Serializer.Converters.Add(new Newtonsoft.Json.Converters.StringEnumConverter());
        }
        public static string Serialize(this DataModelBase dataModel, bool format = false)
        {
            using var sw = new StringWriter();
            using var jw = new JsonTextWriter(sw);
            Serialize(dataModel, jw, format);
            return sw.ToString();
        }
        public static void Serialize(this DataModelBase dataModel, StreamWriter writer, bool format = false)
        {
            using var jw = new JsonTextWriter(writer);
            Serialize(dataModel, jw, format);
        }
        public static void Serialize(this DataModelBase dataModel, JsonTextWriter writer, bool format = false)
        {
            Serializer.Formatting = format ? Formatting.Indented : Formatting.None;
            Serializer.Serialize(writer, dataModel);
        }
        public static void Deserialize(this DataModelBase dataModel, string str)
        {
            using var sr = new StringReader(str);
            using var jr = new JsonTextReader(sr);
            Deserialize(dataModel, jr);
        }
        public static void Deserialize(this DataModelBase dataModel, StreamReader reader)
        {
            using var jr = new JsonTextReader(reader);
            Deserialize(dataModel, jr);
        }
        public static void Deserialize(this DataModelBase dataModel, JsonTextReader reader)
        {
            dataModel.Reset();
            Serializer.Populate(reader, dataModel);
        }
    }
}