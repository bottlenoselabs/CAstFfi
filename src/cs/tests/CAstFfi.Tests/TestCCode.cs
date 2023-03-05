// Copyright (c) Bottlenose Labs Inc. (https://github.com/bottlenoselabs). All rights reserved.
// Licensed under the MIT license. See LICENSE file in the Git repository root directory for full license information.

using System;
using CAstFfi.Tests.Foundation;
using JetBrains.Annotations;
using Microsoft.Extensions.DependencyInjection;
using Xunit;

namespace CAstFfi.Tests;

[PublicAPI]
public class TestCCode : TestBase
{
    public static TheoryData<string> Enums() => new()
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

    public static TheoryData<string> NamedFunctionPointers() => new()
    {
        "TypeDef_FunctionPointer_ReturnVoid_ArgsVoid",
    };

    [Fact]
    public void Reads()
    {
        Assert.True(_fixture.AbstractSyntaxTrees.Length != 0, "Failed to read C code.");
    }

    [Theory]
    [MemberData(nameof(Enums))]
    public void Enum(string name)
    {
        foreach (var ast in _fixture.AbstractSyntaxTrees)
        {
            var value = ast.GetEnum(name);
            AssertValue(name, value, $"{ast.TargetPlatformRequested}/Enums");
        }
    }

    [Theory]
    [MemberData(nameof(NamedFunctionPointers))]
    public void NamedFunctionPointer(string name)
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

    private readonly TestFixtureCCode _fixture;

    public TestCCode()
        : base("Data/Values", false)
    {
        var services = TestHost.Services;
        _fixture = services.GetService<TestFixtureCCode>()!;
    }
}
