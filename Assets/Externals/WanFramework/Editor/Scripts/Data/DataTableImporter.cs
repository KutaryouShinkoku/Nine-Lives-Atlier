//    ▄▀▀▀▄▄▄▄▄▄▄▀▀▀▄
//    █▒▒░░░░░░░░░▒▒█    DataTableImporter.cs
//     █░░█░░░░░█░░█     Created by WanNeng
//  ▄▄  █░░░▀█▀░░░█  ▄▄  Created   01/01/2024 19:07
// █░░█ ▀▄░░░░░░░▄▀ █░░█

using System;
using System.CodeDom;
using System.CodeDom.Compiler;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using ExcelDataReader;
using Microsoft.CSharp;
using UnityEditor;
using UnityEngine;
using WanFramework.Data;

namespace WanFramework.Editor.Data
{
    public class DataTableImportException : Exception
    {
        public DataTableImportException(string message, ref DataTableRaw dataTableRaw, ref DataTableRaw.DataBlock block, Exception inner)
            : base($"At {dataTableRaw.Path}({block.Row}:{block.Column}). {message}", inner)
        {
        }

        public DataTableImportException(string message, ref DataTableRaw dataTableRaw, Exception inner)
            : this(message, ref dataTableRaw, ref DataTableRaw.DataBlock.Empty, inner)
        {
        }
        
        public DataTableImportException(ref DataTableRaw dataTableRaw, Exception inner)
            : this("", ref dataTableRaw, ref DataTableRaw.DataBlock.Empty, inner)
        {
        }
        
        public DataTableImportException(string message, ref DataTableRaw dataTableRaw, ref DataTableRaw.DataBlock block)
            : base($"At {dataTableRaw.Path}({block.Row}:{block.Column}). {message}")
        {
        }

        public DataTableImportException(string message, ref DataTableRaw dataTableRaw)
            : this(message, ref dataTableRaw, ref DataTableRaw.DataBlock.Empty)
        {
        }
        
        public DataTableImportException(ref DataTableRaw dataTableRaw)
            : this("", ref dataTableRaw, ref DataTableRaw.DataBlock.Empty)
        {
        }
    }
    public class DataTableRaw
    {
        public string Path { get; private set; }

        public struct DataBlock
        {
            public static DataBlock Empty = new DataBlock("", 0, 0);
            public string Data;
            public int Row;
            public int Column;

            public DataBlock(string data, int row, int column)
            {
                Data = data;
                Row = row;
                Column = column;
            }
        }

        private DataBlock[][] _data;

        public DataBlock[] GetRow(int rowId)
        {
            return _data.Length <= rowId
                ? Array.Empty<DataBlock>() : _data[rowId];
        }

        public int GetRowCount()
        {
            return _data.Length;
        }

        public bool OnValid()
        {
            var name = GetRow(0);
            var type = GetRow(1);
            if (name.Length != type.Length) return false;
            for (var i = 0; i < name.Length; ++i)
                if (name[i].Data == "Id" && 
                    (type[i].Data == "string" || type[i].Data == "System.String"))
                    return true;
            return false;
        }
        
        public static DataTableRaw FromExcel(string path)
        {
            using var stream = File.Open(path, FileMode.Open, FileAccess.Read, FileShare.ReadWrite);
            using var reader = ExcelReaderFactory.CreateOpenXmlReader(stream);

            var dataInRow = new List<DataBlock[]>();
            var rawRowIdx = 0;
            do
            {
                while (reader.Read())
                {
                    ++rawRowIdx;
                    var fieldCount = reader.FieldCount;
                    // 空行跳过
                    if (fieldCount == 0) continue;
                    var first = reader.GetString(0);
                    if (string.IsNullOrEmpty(first)) continue;
                    // 注释行和空行跳过
                    if (first.StartsWith("#") || string.IsNullOrEmpty(first)) continue;
                    // 开始读row
                    var row = new DataBlock[fieldCount];
                    dataInRow.Add(row);
                    for (var i = 0; i < fieldCount; ++i)
                    {
                        var data = reader.GetValue(i)?.ToString() ?? "";
                        row[i] = new DataBlock(data, rawRowIdx, i + 1);
                    }
                }
            } while (reader.NextResult());

            return new DataTableRaw
            {
                _data = dataInRow.ToArray(),
                Path = path
            };
        }
    }
    
    /// <summary>
    /// 代码生成器
    /// </summary>
    public class DataTableAssetSourceGenerator
    {
        public static Type FindTypeByName(string typeName)
        {
            if (string.IsNullOrEmpty(typeName))
            {
                Debug.LogError("Empty type is not allowed");
                return null;
            }
            // 对Array则需要递归展开
            var isArray = typeName.EndsWith("[]");
            if (isArray)
            {
                var innerTypeName = typeName[..^2];
                var innerType = FindTypeByName(innerTypeName);
                return innerType?.MakeArrayType();
            }
            
            // 内置类型关键字
            Type type;
            switch (typeName)
            {
                // System内置
                case "string":
                    type = typeof(string);
                    break;
                case "sbyte":
                    type = typeof(sbyte);
                    break;
                case "byte":
                    type = typeof(byte);
                    break;
                case "short":
                    type = typeof(short);
                    break;
                case "ushort":
                    type = typeof(ushort);
                    break;
                case "int":
                    type = typeof(int);
                    break;
                case "uint":
                    type = typeof(uint);
                    break;
                case "long":
                    type = typeof(long);
                    break;
                case "ulong":
                    type = typeof(ulong);
                    break;
                case "float":
                    type = typeof(float);
                    break;
                case "double":
                    type = typeof(double);
                    break;
                case "decimal":
                    type = typeof(decimal);
                    break;
                case "bool":
                    type = typeof(bool);
                    break;
                // Unity内置别名
                case "Vector2":
                    type = typeof(Vector2);
                    break;
                case "Vector3":
                    type = typeof(Vector3);
                    break;
                case "Vector4":
                    type = typeof(Vector4);
                    break;
                case "Vector2Int":
                    type = typeof(Vector2Int);
                    break;
                case "Vector3Int":
                    type = typeof(Vector3Int);
                    break;
                case "Color":
                    type = typeof(Color);
                    break;
                default:
                    type = null;
                    break;
            }
            if (type != null) return type;
            // 查找当前程序集中的类型
            type = Type.GetType(typeName);
            if (type != null) return type;
            // 从其他程序集中查找
            foreach (var asm in AppDomain.CurrentDomain.GetAssemblies())
            {
                type = asm.GetType(typeName);
                if (type != null) return type;
            }
            // 没找到
            return null;
        }

        /// <summary>
        /// 插入表头
        /// </summary>
        /// <param name="namespace"></param>
        /// <param name="dataTable"></param>
        public string Generate(string @namespace, ref DataTableRaw dataTable)
        {
            var headerArray = dataTable.GetRow(0);
            var typeArray = dataTable.GetRow(1);
            if (typeArray.Length != headerArray.Length)
                throw new DataTableImportException("Table header count is not equal to type count.", ref dataTable);
            var fieldCount = 0;
            for (var i = 0; i < headerArray.Length; ++i)
            {
                if (string.IsNullOrEmpty(headerArray[i].Data)) break;
                ++fieldCount;
            }
            var fields = new (Type type, string name)[fieldCount];
            for (var i = 0; i < fieldCount; ++i)
            {
                fields[i].name = headerArray[i].Data;
                var typeName = typeArray[i].Data;
                var type = FindTypeByName(typeName);
                if (type == null)
                    throw new DataTableImportException($"Type {typeName} not found.", ref dataTable);
                fields[i].type = type;
            }
            
            // 表名
            var tableName = Path.GetFileNameWithoutExtension(dataTable.Path);

            // 所有Key，保证每个Key的唯一性
            var keyColumnId = -1;
            for (var i = 0; i < fieldCount; ++i)
                if (headerArray[i].Data == "Id")
                {
                    keyColumnId = i;
                    break;
                }
            if (keyColumnId == -1)
                throw new DataTableImportException($"Column \"Id\" not found.", ref dataTable);
            var keys = new string[dataTable.GetRowCount() - 2];
            var keyHashSet = new HashSet<string>();
            for (var i = 0; i < dataTable.GetRowCount() - 2; ++i)
            {
                var block = dataTable.GetRow(i + 2)[keyColumnId];
                var key = block.Data;
                keys[i] = key;
                if (keyHashSet.Contains(key))
                    throw new DataTableImportException($"Name \"{key}\" already exists.", ref dataTable, ref block);
                keyHashSet.Add(key);
            }
            
            // 生成代码
            return Generate(@namespace, tableName, fields, keys);
        }

        public static string ToFieldName(string name)
        {
            return name[..1].ToLower() + name[1..];
        }
        
        public static string ToPropertyName(string name)
        {
            return name[..1].ToUpper() + name[1..];
        }

        public static string ToEnumTypeName(string assetTypeName)
        {
            return assetTypeName.EndsWith("Table") ? $"{assetTypeName[..^5]}Ids" : $"{assetTypeName}Ids";
        }
        private void GenerateAssetKeyEnum(CodeNamespace @namespace, string assetTypeName, string[] keys)
        {
            var enumTypeName = ToEnumTypeName(assetTypeName);
            var enumType = new CodeTypeDeclaration(enumTypeName)
            {
                IsEnum = true
            };
            for (var i = 0; i < keys.Length; ++i)
                enumType.Members.Add(
                    new CodeMemberField(enumTypeName, keys[i])
                    {
                        InitExpression = new CodeSnippetExpression(i.ToString())
                    });
            @namespace.Types.Add(enumType);
        }

        private void GenerateAssetKeyEnumExtension(CodeNamespace @namespace, string assetTypeName)
        {
            var enumTypeName = ToEnumTypeName(assetTypeName);
            var enumExtensionType = new CodeTypeDeclaration($"{enumTypeName}Extensions")
            {
                Attributes = MemberAttributes.Public
            };
            var extensionMethodCode = $"\t\tpublic static {assetTypeName}.Entry Data(this {enumTypeName} id) => global::WanFramework.Data.DataSystem.Instance.Load<{assetTypeName}>().Get(id);";
            var extensionByTablePathMethodCode = $"\t\tpublic static {assetTypeName}.Entry DataIn(this {enumTypeName} id, string tablePath) => global::WanFramework.Data.DataSystem.Instance.Load<{assetTypeName}>(tablePath).Get(id);";
            var extensionMethod = new CodeSnippetTypeMember(extensionMethodCode);
            var extensionByTablePathMethod = new CodeSnippetTypeMember(extensionByTablePathMethodCode);
            enumExtensionType.Members.Add(extensionMethod);
            enumExtensionType.Members.Add(extensionByTablePathMethod);
            enumExtensionType.StartDirectives.Add(
                new CodeRegionDirective(CodeRegionMode.Start, "\nstatic"));
            enumExtensionType.EndDirectives.Add(
                new CodeRegionDirective(CodeRegionMode.End, String.Empty));
            @namespace.Types.Add(enumExtensionType);
        }
        
        private void GenerateAssetType(CodeNamespace @namespace, string assetTypeName, (Type type, string name)[] fields)
        {
            var serializeFieldAttribute
                = new CodeAttributeDeclaration(new CodeTypeReference(typeof(SerializeField)));
            var serializableAttribute
                = new CodeAttributeDeclaration(new CodeTypeReference(typeof(SerializableAttribute)));
            
            // DataTableType::Entry
            var codeAssetEntryType = new CodeTypeDeclaration("Entry")
            {
                Attributes = MemberAttributes.Public,
                BaseTypes = { new CodeTypeReference(typeof(DataEntry)) },
                CustomAttributes = { serializableAttribute }
            };
            foreach (var (type, name) in fields)
            {
                codeAssetEntryType.Members.Add(new CodeMemberField(type, ToFieldName(name))
                {
                    Attributes = MemberAttributes.Private,
                    CustomAttributes = { serializeFieldAttribute }
                });
                codeAssetEntryType.Members.Add(new CodeMemberProperty
                {
                    Attributes = MemberAttributes.Final | MemberAttributes.Public,
                    Name = ToPropertyName(name),
                    Type = new CodeTypeReference(type),
                    GetStatements =
                    {
                        new CodeMethodReturnStatement(
                            new CodeFieldReferenceExpression(
                                new CodeThisReferenceExpression(), 
                                ToFieldName(name)))
                    }
                });
            }
            var serializeStatement = GenerateEntryDeserializeStatement(@namespace, assetTypeName, "this", new CodeVariableReferenceExpression("reader"), fields);
            var serializeMethod = new CodeMemberMethod
            {
                Attributes = MemberAttributes.Public,
                Parameters = { new CodeParameterDeclarationExpression(new CodeTypeReference(typeof(BinaryReader)), "reader") },
                Name = "LoadFrom"
            };
            codeAssetEntryType.Members.Add(serializeMethod);
            serializeMethod.Statements.AddRange(serializeStatement);
            // DataTable::Entry end
            
            var codeAssetNameProperty = new CodeMemberProperty
            {
                Attributes = MemberAttributes.Override | MemberAttributes.Public,
                Name = "TableName",
                Type = new CodeTypeReference(typeof(string)),
                GetStatements =
                {
                    new CodeMethodReturnStatement(new CodePrimitiveExpression(assetTypeName))
                }
            };
            var codeAssetLoadMethod = GenerateTableLoader(@namespace, assetTypeName, fields);
            var codeAssetGetMethod = new CodeMemberMethod()
            {
                Attributes = MemberAttributes.Final | MemberAttributes.Public,
                Name = "Get",
                ReturnType = new CodeTypeReference($"{@namespace.Name}.{assetTypeName}.Entry"),
                Parameters = { new CodeParameterDeclarationExpression($"{@namespace.Name}.{ToEnumTypeName(assetTypeName)}", "id")},
                Statements = 
                { 
                    new CodeMethodReturnStatement(
                        new CodeCastExpression($"{@namespace.Name}.{assetTypeName}.Entry",
                            new CodeMethodInvokeExpression(
                                new CodeMethodReferenceExpression(new CodeThisReferenceExpression(), "Get"), 
                                new CodeCastExpression("System.Int32", new CodeArgumentReferenceExpression("id"))
                                )
                            )
                        )
                }
            };
            var codeAssetType = new CodeTypeDeclaration($"{assetTypeName}")
            {
                Attributes = MemberAttributes.Public,
                BaseTypes = { new CodeTypeReference(typeof(DataTable<>).FullName, new CodeTypeReference($"{assetTypeName}.Entry")) },
                Members =
                {
                    codeAssetNameProperty,
                    codeAssetEntryType,
                    codeAssetLoadMethod,
                    codeAssetGetMethod
                },
                CustomAttributes = { serializableAttribute }
            };
            @namespace.Types.Add(codeAssetType);
        }
        private CodeStatement[] GenerateTypeDeserializeStatement(string varName, CodeVariableReferenceExpression reader, Type type)
        {
            CodeStatement[] codeStatements;
            if (type.IsArray)
            {
                var arraySizeVarName = $"array_size_{Math.Abs(varName.GetHashCode())}";
                var arrayLoopVarName = $"{arraySizeVarName}_i";
                codeStatements = new CodeStatement[]
                {
                    new CodeCommentStatement($"{type} {varName}"),
                    new CodeVariableDeclarationStatement(
                        typeof(int),
                        arraySizeVarName,
                        new CodeMethodInvokeExpression(
                            new CodeTypeReferenceExpression(typeof(DataTableBinarySerializer)), "DeserializeInt32",
                            new CodeVariableReferenceExpression("reader"))),
                    new CodeAssignStatement(new CodeSnippetExpression(varName), new CodeArrayCreateExpression(type, new CodeVariableReferenceExpression(arraySizeVarName))),
                    // for (int i=0;i<array_size;++i) 
                    new CodeIterationStatement(
                        new CodeVariableDeclarationStatement(typeof(int), arrayLoopVarName, new CodePrimitiveExpression(0)),
                        new CodeBinaryOperatorExpression(
                            new CodeVariableReferenceExpression(arrayLoopVarName),
                            CodeBinaryOperatorType.LessThan,
                            new CodeVariableReferenceExpression(arraySizeVarName)
                            ),
                        new CodeAssignStatement(
                            new CodeVariableReferenceExpression(arrayLoopVarName),
                            new CodeBinaryOperatorExpression(
                                new CodeVariableReferenceExpression(arrayLoopVarName),
                                CodeBinaryOperatorType.Add,
                                new CodePrimitiveExpression(1)
                                )
                            ),
                        GenerateTypeDeserializeStatement($"{varName}[{arrayLoopVarName}]", new CodeVariableReferenceExpression("reader"), type.GetElementType())
                        )
                };
            }
            else if (type.IsEnum)
            {
                var enumStrVarName = $"enum_str_{Math.Abs(varName.GetHashCode())}";
                codeStatements = new CodeStatement[]
                {
                    new CodeCommentStatement($"{type} {varName}"),
                    new CodeVariableDeclarationStatement(
                        typeof(string),
                        enumStrVarName,
                        new CodeMethodInvokeExpression(
                            new CodeTypeReferenceExpression(typeof(DataTableBinarySerializer)), "DeserializeString",
                            new CodeVariableReferenceExpression("reader"))),
                    new CodeAssignStatement(
                        new CodeSnippetExpression(varName),
                        new CodeMethodInvokeExpression(
                            new CodeMethodReferenceExpression(new CodeTypeReferenceExpression(typeof(Enum)), "Parse", new CodeTypeReference(type)),
                            new CodeVariableReferenceExpression(enumStrVarName)))
                };
            }
            else
            {
                codeStatements = new CodeStatement[]
                {
                    new CodeCommentStatement($"{type} {varName}"),
                    new CodeAssignStatement(
                        new CodeSnippetExpression(varName),
                        new CodeMethodInvokeExpression(
                            new CodeTypeReferenceExpression(typeof(DataTableBinarySerializer)), DataTableBinarySerializer.GetSerializeInfo(type).Deserializer.Name,
                            new CodeVariableReferenceExpression("reader")))
                };
            }
            return codeStatements;
        }
        private CodeStatement[] GenerateEntryDeserializeStatement(CodeNamespace @namespace, string assetTypeName, string entryVarName, CodeVariableReferenceExpression reader, (Type type, string name)[] fields)
        {
            //return new CodeSnippetStatement("return;");
            var codeStatements = new List<CodeStatement>();
            foreach (var field in fields)
                codeStatements.AddRange(GenerateTypeDeserializeStatement($"{entryVarName}.{ToFieldName(field.name)}", reader, field.type));
            return codeStatements.ToArray();
        }
        
        private CodeMemberMethod GenerateTableLoader(CodeNamespace @namespace, string assetTypeName, (Type type, string name)[] fields)
        {
            var tableLoaderMethod = new CodeMemberMethod()
            {
                Attributes = MemberAttributes.Public | MemberAttributes.Override,
                Name = "LoadFrom",
                Parameters = { new CodeParameterDeclarationExpression($"global::WanFramework.Data.DataTableRawAsset", "rawAsset")},
                Statements = 
                { 
                    // using var ms = new MemoryStream(rawAsset.GetData());
                    new CodeVariableDeclarationStatement(
                        $"using {typeof(MemoryStream).FullName}", 
                        "ms",
                        new CodeObjectCreateExpression(
                            new CodeTypeReference(typeof(MemoryStream)), 
                            new CodeMethodInvokeExpression(
                                new CodeVariableReferenceExpression("rawAsset"),"GetData"))),
                    // using var reader = new BinaryReader(ms);
                    new CodeVariableDeclarationStatement(
                        $"using {typeof(BinaryReader).FullName}", 
                        "reader",
                        new CodeObjectCreateExpression(
                            new CodeTypeReference(typeof(BinaryReader)), 
                            new CodeVariableReferenceExpression("ms"))),
                    // var entryCount = DataTableBinarySerializer.DeserializeInt32(ms);
                    new CodeVariableDeclarationStatement(
                        typeof(int), 
                        "entryCount",
                        new CodeMethodInvokeExpression(
                            new CodeTypeReferenceExpression(typeof(DataTableBinarySerializer)),"DeserializeInt32", new CodeVariableReferenceExpression("reader"))),
                    // data = new Entry[entryCount];
                    new CodeVariableDeclarationStatement(
                        new CodeTypeReference($"{@namespace.Name}.{assetTypeName}.Entry[]"),
                        "tmpData", new CodeArrayCreateExpression(new CodeTypeReference($"{@namespace.Name}.{assetTypeName}.Entry"), new CodeVariableReferenceExpression("entryCount"))),
                    // for (int i=0;i<entryCount;++i) 
                    new CodeIterationStatement(
                        new CodeVariableDeclarationStatement(typeof(int), "i", new CodePrimitiveExpression(0)),
                        new CodeBinaryOperatorExpression(
                            new CodeVariableReferenceExpression("i"),
                            CodeBinaryOperatorType.LessThan,
                            new CodeVariableReferenceExpression("entryCount")
                            ),
                        new CodeAssignStatement(
                            new CodeVariableReferenceExpression("i"),
                            new CodeBinaryOperatorExpression(
                                new CodeVariableReferenceExpression("i"),
                                CodeBinaryOperatorType.Add,
                                new CodePrimitiveExpression(1)
                                )
                            ),
                        new CodeStatement[] {
                            // tmpData[i] = new Entry()
                            new CodeAssignStatement(new CodeSnippetExpression("tmpData[i]"), new CodeObjectCreateExpression(new CodeTypeReference($"{@namespace.Name}.{assetTypeName}.Entry"))),
                            new CodeExpressionStatement(new CodeMethodInvokeExpression(new CodeSnippetExpression("tmpData[i]"), "LoadFrom", new CodeVariableReferenceExpression("reader")))
                            ////////////////////////
                        }
                    ),
                    // if (ms.Length != ms.Position) throw Exception("Failed to load table {TableName}");
                    new CodeConditionStatement(new CodeBinaryOperatorExpression(
                        new CodeSnippetExpression("ms.Length"), 
                        CodeBinaryOperatorType.GreaterThan,
                        new CodeSnippetExpression("ms.Position")), new CodeThrowExceptionStatement(new CodeObjectCreateExpression(typeof(Exception), new CodePrimitiveExpression("Failed to load binary table")))),
                    // this.data = tmpData
                    new CodeAssignStatement(
                        new CodeFieldReferenceExpression(new CodeThisReferenceExpression(), "data"),
                        new CodeVariableReferenceExpression("tmpData"))
                }
            };
            return tableLoaderMethod;
        }
        private string Generate(string @namespace, string assetTypeName, (Type type, string name)[] fields, string[] keys)
        {
            var codeNamespace = new CodeNamespace(@namespace);
            GenerateAssetKeyEnum(codeNamespace, assetTypeName, keys);
            GenerateAssetKeyEnumExtension(codeNamespace, assetTypeName);
            GenerateAssetType(codeNamespace, assetTypeName, fields);
            var compileUnit = new CodeCompileUnit()
            {
                Namespaces =
                {
                    codeNamespace
                }
            };
            using var provider = new CSharpCodeProvider();
            using var writer = new StringWriter();
            provider.GenerateCodeFromCompileUnit(compileUnit, writer, new CodeGeneratorOptions()
            {
                
            });
            return writer.ToString();
        }
    }

    public class DataTableRawAssetConverter
    {
        public DataTableRawAsset ToBinaryTableAsset(ref DataTableRaw dataTableRaw)
        {
            var names = dataTableRaw.GetRow(0);
            var types = dataTableRaw.GetRow(1);
            var tableRaw = ScriptableObject.CreateInstance<DataTableRawAsset>();
            var fieldCount = 0;
            for (var col = 0; col < types.Length; ++col)
            {
                if (string.IsNullOrEmpty(names[col].Data)) break;
                ++fieldCount;
            }
            var fieldTypes = new Type[fieldCount];
            for (var col = 0; col < fieldTypes.Length; ++col)
            {
                var fieldType = DataTableAssetSourceGenerator.FindTypeByName(types[col].Data);
                fieldTypes[col] = fieldType ?? throw new DataTableImportException($"Failed to find type {types[col].Data}", ref dataTableRaw);
            }
            using var ms = new MemoryStream();
            using var writer = new BinaryWriter(ms);
            var entryCount = dataTableRaw.GetRowCount() - 2;
            DataTableBinarySerializer.SerializeInt32(writer, entryCount);
            for (var row = 0; row < entryCount; ++row)
            {
                var rowData =  dataTableRaw.GetRow(row + 2);
                for (var col = 0; col < fieldTypes.Length; ++col)
                {
                    object value;
                    try
                    {
                        value = ConvertTo(rowData[col].Data, fieldTypes[col]);
                    }
                    catch (Exception e)
                    {
                        throw new DataTableImportException($"Failed to convert data", ref dataTableRaw, e);
                    }
                    DataTableBinarySerializer.Serialize(writer, value);
                }
            }
            tableRaw.SetData(ms.ToArray());
            return tableRaw;
        }
        
        private object ConvertTo(string data, Type type)
        {
            // Unity object
            if (typeof(UnityEngine.Object).IsAssignableFrom(type))
            {
                if (!File.Exists(data)) return null;
                var ret = AssetDatabase.LoadAssetAtPath(data, type);
                if (ret == null)
                {
                    AssetDatabase.ImportAsset(data);
                    ret = AssetDatabase.LoadAssetAtPath(data, type);
                }
                return ret;
            }

            // Unity vector
            if (type == typeof(Vector2))
            {
                data = data.Trim();
                var args = data.Split(',');
                if (!data.StartsWith('(') || !data.EndsWith(')') || args.Length != 2)
                    throw new Exception($"Vector2 should be written in (x, y) format but get {data}");
                return new Vector2(float.Parse(args[0][1..]), float.Parse(args[1][..^1]));
            }
            if (type == typeof(Vector3))
            {
                data = data.Trim();
                var args = data.Split(',');
                if (!data.StartsWith('(') || !data.EndsWith(')') || args.Length != 3)
                    throw new Exception($"Vector3 should be written in (x, y, z) format but get {data}");
                return new Vector3(float.Parse(args[0][1..]), float.Parse(args[1]), float.Parse(args[2][..^1]));
            }
            if (type == typeof(Vector4))
            {
                data = data.Trim();
                var args = data.Split(',');
                if (!data.StartsWith('(') || !data.EndsWith(')') || args.Length != 4)
                    throw new Exception($"Vector4 should be written in (x, y, z, w) format but get {data}");
                return new Vector4(float.Parse(args[0][1..]), float.Parse(args[1]), float.Parse(args[2]), float.Parse(args[3][..^1]));
            }
            if (type == typeof(Vector2Int))
            {
                data = data.Trim();
                var args = data.Split(',');
                if (!data.StartsWith('(') || !data.EndsWith(')') || args.Length != 2)
                    throw new Exception($"Vector2Int should be written in (x, y) format but get {data}");
                return new Vector2Int(int.Parse(args[0][1..]), int.Parse(args[1][..^1]));
            }
            if (type == typeof(Vector3Int))
            {
                data = data.Trim();
                var args = data.Split(',');
                if (!data.StartsWith('(') || !data.EndsWith(')') || args.Length != 3)
                    throw new Exception($"Vector3Int should be written in (x, y, z) format but get {data}");
                return new Vector3(int.Parse(args[0][1..]), int.Parse(args[1]), int.Parse(args[2][..^1]));
            }
            
            // Color
            if (type == typeof(Color))
            {
                data = data.Trim();
                var args = data.Split(',');
                if (!data.StartsWith('(') || !data.EndsWith(')') || args.Length != 3)
                    throw new Exception($"Color should be written in (r, g, b) (range from 0-255)format but get {data}");
                return new Color(float.Parse(args[0][1..]) / 255, float.Parse(args[1]) / 255, float.Parse(args[2][..^1]) / 255);
            }
            
            // Enum
            if (type.IsEnum)
            {
                if (string.IsNullOrEmpty(data))
                    return Enum.GetValues(type).GetValue(0);
                if (Enum.TryParse(type, data, out var result))
                    return result;
                throw new Exception($"Failed to convert {data} to enum type {type}");
            }

            // Array
            if (type.IsArray)
            {
                var elementType = type.GetElementType()!;
                var depth = 0;
                var beginIndex = 1;
                if (data == "[]" || string.IsNullOrEmpty(data)) return Array.CreateInstance(elementType, 0);
                var list = new List<object>();
                for (var i = 0; i < data.Length; ++i)
                {
                    if (data[i] == '[')
                        ++depth;
                    if (data[i] == ',' || data[i] == ']' && depth == 1)
                    {
                        var subData = data.Substring(beginIndex, i - beginIndex);
                        var element = ConvertTo(subData, elementType);
                        list.Add(element);
                        beginIndex = i + 1;
                    }
                    if (data[i] == ']')
                        --depth;
                    
                    if (depth <= 0) break;
                }

                var array = Array.CreateInstance(elementType, list.Count);
                for (var i = 0; i < list.Count; ++i)
                    array.SetValue(list[i], i);
                return array;
            }
            return string.IsNullOrEmpty(data) ? 
                (type == typeof(string) ? "" : Activator.CreateInstance(type)) : 
                Convert.ChangeType(data, type);
        }
    }
}