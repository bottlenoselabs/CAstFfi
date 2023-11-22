// Copyright (c) Bottlenose Labs Inc. (https://github.com/bottlenoselabs). All rights reserved.
// Licensed under the MIT license. See LICENSE file in the Git repository root directory for full license information.

using System.CommandLine;

namespace CAstFfi.Features.Extract;

public sealed class ExtractAbstractSyntaxTreeCommand : Command
{
    private readonly ExtractAbstractSyntaxTreeTool _tool;

    public ExtractAbstractSyntaxTreeCommand(ExtractAbstractSyntaxTreeTool tool)
        : base(
            "extract",
            "Extract the FFI (foreign function interface) of a C `.h` file to one or more abstract syntax tree (AST) `.json` files per target platform.")
    {
        _tool = tool;

        var configurationFilePathOption = new Option<string>(
#pragma warning disable CA1861
            new[] {"--config", "--configFilePath" }, "The file path to configure extraction of abstract syntax tree `.json` files.");
#pragma warning restore CA1861
        configurationFilePathOption.SetDefaultValue("config.json");

        AddOption(configurationFilePathOption);

        var clangFilePathOption = new Option<string>(
#pragma warning disable CA1861
            new[] { "--clang", "--clangFilePath" }, "The file path to the native libclang library.");
#pragma warning restore CA1861
        clangFilePathOption.SetDefaultValue(string.Empty);

        this.SetHandler(Main, configurationFilePathOption, clangFilePathOption);
    }

    private void Main(string configurationFilePath, string clangFilePath)
    {
        _tool.Run(configurationFilePath, clangFilePath);
    }
}
