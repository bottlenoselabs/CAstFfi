// Copyright (c) Bottlenose Labs Inc. (https://github.com/bottlenoselabs). All rights reserved.
// Licensed under the MIT license. See LICENSE file in the Git repository root directory for full license information.

using System;
using JetBrains.Annotations;
using Microsoft.Extensions.DependencyInjection;
using Xunit;

namespace CAstFfi.Tests;

[PublicAPI]
public class TestCCode : TestBase
{
    [Fact]
    public void Reads()
    {
        Assert.True(_fixture.AbstractSyntaxTrees.Length != 0, "Failed to read C code.");
    }

    public static TheoryData<string> EnumNames() => new()
    {
        "Enum_Force_SInt8",
        "Enum_Force_SInt16",
        "Enum_Force_SInt32",
        "Enum_Force_SInt64",
        "Enum_Force_UInt8",
        "Enum_Force_UInt16",
        "Enum_Force_UInt32",
        "Enum_Force_UInt64"
    };

    [Theory]
    [MemberData(nameof(EnumNames))]
    public void Enum(string name)
    {
        foreach (var ast in _fixture.AbstractSyntaxTrees)
        {
            var value = ast.GetEnum(name);
            AssertValue(name, value, $"{ast.TargetPlatformRequested}/Enums");
        }
    }

    public static TheoryData<string> FunctionPointerNames() => new()
    {
        "TypeDef_FunctionPointer_ReturnVoid_ArgsVoid",
    };

    [Theory]
    [MemberData(nameof(FunctionPointerNames))]
    public void FunctionPointer(string name)
    {
        foreach (var ast in _fixture.AbstractSyntaxTrees)
        {
            var typeAlias = ast.GetTypeAlias(name);
            AssertValue(name, typeAlias, $"{ast.TargetPlatformRequested}/TypeAliases");

            var functionPointer = ast.GetFunctionPointer(typeAlias.UnderlyingName);
            var typeAliasFileName = typeAlias.UnderlyingName
                .Replace("*", "+", StringComparison.InvariantCulture)
                .Replace(" ", string.Empty, StringComparison.InvariantCulture);
            AssertValue(typeAliasFileName, functionPointer, $"{ast.TargetPlatformRequested}/FunctionPointers");
        }
    }

    public static TheoryData<string> OpaqueTypesNames() => new()
    {
        "OpaqueType_PlatformSpecificSize",
    };

    [Theory]
    [MemberData(nameof(OpaqueTypesNames))]
    public void OpaqueType(string name)
    {
        foreach (var ast in _fixture.AbstractSyntaxTrees)
        {
            var opaqueType = ast.GetOpaqueType(name);
            AssertValue(name, opaqueType, $"{ast.TargetPlatformRequested}/OpaqueTypes");
        }
    }

    public static TheoryData<string> MacroObjectNames() => new()
    {
        "FFI_PLATFORM_NAME",
        "MACRO_OBJECT_INT_VALUE"
    };

    [Theory]
    [MemberData(nameof(MacroObjectNames))]
    public void MacroObject(string name)
    {
        foreach (var ast in _fixture.AbstractSyntaxTrees)
        {
            var value = ast.GetMacroObject(name);
            AssertValue(name, value, $"{ast.TargetPlatformRequested}/MacroObjects");
        }
    }

    private readonly TestFixtureCCode _fixture;

    public TestCCode()
        : base("Data/Values", true)
    {
        var services = TestHost.Services;
        _fixture = services.GetService<TestFixtureCCode>()!;
    }
}
