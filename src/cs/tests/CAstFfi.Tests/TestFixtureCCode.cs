// Copyright (c) Bottlenose Labs Inc. (https://github.com/bottlenoselabs). All rights reserved.
// Licensed under the MIT license. See LICENSE file in the Git repository root directory for full license information.

using System;
using System.Collections.Immutable;
using System.IO;
using CAstFfi.Data;
using CAstFfi.Data.Serialization;
using CAstFfi.Features.Extract;
using CAstFfi.Tests.Data.Models;
using Microsoft.Extensions.DependencyInjection;
using Xunit;

namespace CAstFfi.Tests;

public sealed class TestFixtureCCode : IDisposable
{
    private readonly ExtractAbstractSyntaxTreeTool _tool;

    public ImmutableArray<TestCCodeAbstractSyntaxTree> AbstractSyntaxTrees { get; }

    public TestFixtureCCode()
    {
        var services = TestHost.Services;

        _tool = services.GetService<ExtractAbstractSyntaxTreeTool>()!;
        AbstractSyntaxTrees = GetAbstractSyntaxTrees();
    }

    public void Dispose()
    {
        foreach (var ast in AbstractSyntaxTrees)
        {
            ast.AssertNodesAreTested();
        }
    }

    private ImmutableArray<TestCCodeAbstractSyntaxTree> GetAbstractSyntaxTrees()
    {
        const string configurationFileName = "config.json";
        var gitRepositoryDirectoryPath = GetGitRepositoryDirectoryPath();
        var sourceDirectoryPath = Path.Combine(
            gitRepositoryDirectoryPath,
            "src",
            "cs",
            "tests",
            "CAstFfi.Tests");

        var x = Environment.CurrentDirectory;
        var configurationFilePath = Path.Combine(sourceDirectoryPath, configurationFileName);
        _tool.Run(configurationFilePath, string.Empty);

        var abstractSyntaxTreesDirectory = Path.Combine(sourceDirectoryPath, "ast");
        var abstractSyntaxTreeFilePaths = Directory.EnumerateFiles(abstractSyntaxTreesDirectory);

        var builder = ImmutableArray.CreateBuilder<TestCCodeAbstractSyntaxTree>();

        foreach (var filePath in abstractSyntaxTreeFilePaths)
        {
            var abstractSyntaxTree = GetAbstractSyntaxTree(filePath);
            builder.Add(abstractSyntaxTree);
        }

        var abstractSyntaxTrees = builder.ToImmutable();
        Assert.True(abstractSyntaxTrees.Length > 0, "Failed to read C code.");

        return abstractSyntaxTrees;
    }

    private static string GetGitRepositoryDirectoryPath()
    {
        var baseDirectory = AppContext.BaseDirectory;
        var directoryInfo = new DirectoryInfo(baseDirectory);
        while (true)
        {
            var files = directoryInfo.GetFiles(".gitignore", SearchOption.TopDirectoryOnly);
            if (files.Length > 0)
            {
                return directoryInfo.FullName;
            }

            directoryInfo = directoryInfo.Parent;
            if (directoryInfo == null)
            {
                return string.Empty;
            }
        }
    }

    private TestCCodeAbstractSyntaxTree GetAbstractSyntaxTree(string filePath)
    {
        var ast = CJsonSerializer.ReadAbstractSyntaxTreeTargetPlatform(filePath);
        var functions = CreateTestFunctions(ast);
        var enums = CreateTestEnums(ast);
        var structs = CreateTestRecords(ast);
        var macroObjects = CreateTestMacroObjects(ast);
        var typeAliases = CreateTestTypeAliases(ast);
        var functionPointers = CreateTestFunctionPointers(ast);
        var opaqueDataTypes = CreateTestOpaqueTypes(ast);

        var data = new TestCCodeAbstractSyntaxTree(
            ast.PlatformRequested,
            ast.PlatformActual,
            functions,
            enums,
            structs,
            macroObjects,
            typeAliases,
            functionPointers,
            opaqueDataTypes);
        return data;
    }

    private static ImmutableDictionary<string, CTestFunction> CreateTestFunctions(CAbstractSyntaxTreeTargetPlatform ast)
    {
        var builder = ImmutableDictionary.CreateBuilder<string, CTestFunction>();

        foreach (var function in ast.Functions.Values)
        {
            var result = CreateTestFunction(function);
            builder.Add(result.Name, result);
        }

        return builder.ToImmutable();
    }

    private static CTestFunction CreateTestFunction(CFunction value)
    {
        var parameters = CreateTestFunctionParameters(value.Parameters);

        var result = new CTestFunction
        {
            Name = value.Name,
#pragma warning disable CA1308
            CallingConvention = value.CallingConvention.ToString().ToLowerInvariant(),
#pragma warning restore CA1308
            ReturnTypeName = value.ReturnTypeInfo.Name,
            Parameters = parameters,
            Comment = value.Comment
        };
        return result;
    }

    private static ImmutableArray<CTestFunctionParameter> CreateTestFunctionParameters(
        ImmutableArray<CFunctionParameter> values)
    {
        var builder = ImmutableArray.CreateBuilder<CTestFunctionParameter>();

        foreach (var value in values)
        {
            var result = CreateTestFunctionParameter(value);
            builder.Add(result);
        }

        return builder.ToImmutable();
    }

    private static CTestFunctionParameter CreateTestFunctionParameter(CFunctionParameter value)
    {
        var result = new CTestFunctionParameter
        {
            Name = value.Name,
            TypeName = value.TypeInfo.Name
        };

        return result;
    }

    private static ImmutableDictionary<string, CTestEnum> CreateTestEnums(CAbstractSyntaxTreeTargetPlatform ast)
    {
        var builder = ImmutableDictionary.CreateBuilder<string, CTestEnum>();

        foreach (var @enum in ast.Enums.Values)
        {
            var result = CreateTestEnum(@enum);
            builder.Add(result.Name, result);
        }

        return builder.ToImmutable();
    }

    private static CTestEnum CreateTestEnum(CEnum value)
    {
        var values = CreateTestEnumValues(value.Values);

        var result = new CTestEnum
        {
            Name = value.Name,
            IntegerType = value.IntegerTypeInfo.Name,
            Values = values
        };
        return result;
    }

    private static ImmutableArray<CTestEnumValue> CreateTestEnumValues(ImmutableArray<CEnumValue> values)
    {
        var builder = ImmutableArray.CreateBuilder<CTestEnumValue>();

        foreach (var value in values)
        {
            var result = CreateTestEnumValue(value);
            builder.Add(result);
        }

        return builder.ToImmutable();
    }

    private static CTestEnumValue CreateTestEnumValue(CEnumValue value)
    {
        var result = new CTestEnumValue
        {
            Name = value.Name,
            Value = value.Value
        };
        return result;
    }

    private static ImmutableDictionary<string, CTestRecord> CreateTestRecords(CAbstractSyntaxTreeTargetPlatform ast)
    {
        var builder = ImmutableDictionary.CreateBuilder<string, CTestRecord>();

        foreach (var value in ast.Records.Values)
        {
            var result = CreateTestRecord(value);
            builder.Add(result.Name, result);
        }

        return builder.ToImmutable();
    }

    private static CTestRecord CreateTestRecord(CRecord value)
    {
        var name = value.Name;
        var fields = CreateTestRecordFields(value.Fields);

        var result = new CTestRecord
        {
            Name = name,
            SizeOf = value.SizeOf,
            AlignOf = value.AlignOf,
            Fields = fields,
            IsUnion = false
        };

        return result;
    }

    private static ImmutableArray<CTestRecordField> CreateTestRecordFields(ImmutableArray<CRecordField> values)
    {
        var builder = ImmutableArray.CreateBuilder<CTestRecordField>();

        foreach (var value in values)
        {
            var result = CreateTestRecordField(value);
            builder.Add(result);
        }

        return builder.ToImmutable();
    }

    private static CTestRecordField CreateTestRecordField(CRecordField value)
    {
        var result = new CTestRecordField
        {
            Name = value.Name,
            TypeName = value.TypeInfo.Name,
            OffsetOf = value.OffsetOf,
            SizeOf = value.TypeInfo.SizeOf
        };

        return result;
    }

    private ImmutableDictionary<string, CTestMacroObject> CreateTestMacroObjects(CAbstractSyntaxTreeTargetPlatform ast)
    {
        var builder = ImmutableDictionary.CreateBuilder<string, CTestMacroObject>();

        foreach (var value in ast.MacroObjects.Values)
        {
            var result = CreateMacroObject(value);
            builder.Add(result.Name, result);
        }

        return builder.ToImmutable();
    }

    private CTestMacroObject CreateMacroObject(CMacroObject value)
    {
        var result = new CTestMacroObject
        {
            Name = value.Name,
            TypeName = value.TypeInfo.Name,
            Value = value.Value
        };

        return result;
    }

    private ImmutableDictionary<string, CTestTypeAlias> CreateTestTypeAliases(CAbstractSyntaxTreeTargetPlatform ast)
    {
        var builder = ImmutableDictionary.CreateBuilder<string, CTestTypeAlias>();

        foreach (var value in ast.TypeAliases.Values)
        {
            var result = CreateTestTypeAlias(value);
            builder.Add(result.Name, result);
        }

        return builder.ToImmutable();
    }

    private CTestTypeAlias CreateTestTypeAlias(CTypeAlias value)
    {
        var result = new CTestTypeAlias
        {
            Name = value.Name,
            UnderlyingName = value.UnderlyingTypeInfo.Name,
            UnderlyingKind = value.UnderlyingTypeInfo.Kind
        };

        return result;
    }

    private ImmutableDictionary<string, CTestFunctionPointer> CreateTestFunctionPointers(CAbstractSyntaxTreeTargetPlatform ast)
    {
        var builder = ImmutableDictionary.CreateBuilder<string, CTestFunctionPointer>();

        foreach (var value in ast.FunctionPointers.Values)
        {
            var result = CreateTestFunctionPointer(value);
            builder.Add(result.Name, result);
        }

        return builder.ToImmutable();
    }

    private CTestFunctionPointer CreateTestFunctionPointer(CFunctionPointer value)
    {
        var parameters = CreateTestFunctionPointerParameters(value.Parameters);

        var result = new CTestFunctionPointer
        {
            Name = value.Name,
            CallingConvention = "todo",
            ReturnTypeName = value.ReturnTypeInfo.Name,
            Parameters = parameters
        };

        return result;
    }

    private static ImmutableArray<CTestFunctionPointerParameter> CreateTestFunctionPointerParameters(
        ImmutableArray<CFunctionPointerParameter> values)
    {
        var builder = ImmutableArray.CreateBuilder<CTestFunctionPointerParameter>();

        foreach (var value in values)
        {
            var result = CreateTestFunctionPointerParameter(value);
            builder.Add(result);
        }

        return builder.ToImmutable();
    }

    private static CTestFunctionPointerParameter CreateTestFunctionPointerParameter(CFunctionPointerParameter value)
    {
        var result = new CTestFunctionPointerParameter
        {
            Name = value.Name,
            TypeName = value.TypeInfo.Name
        };

        return result;
    }

    private static ImmutableDictionary<string, CTestOpaqueType> CreateTestOpaqueTypes(
        CAbstractSyntaxTreeTargetPlatform ast)
    {
        var builder = ImmutableDictionary.CreateBuilder<string, CTestOpaqueType>();

        foreach (var value in ast.OpaqueTypes.Values)
        {
            var result = CreateTestOpaqueType(value);
            builder.Add(result.Name, result);
        }

        return builder.ToImmutable();
    }

    private static CTestOpaqueType CreateTestOpaqueType(COpaqueType value)
    {
        var result = new CTestOpaqueType
        {
            Name = value.Name,
            SizeOf = value.SizeOf
        };

        return result;
    }
}
