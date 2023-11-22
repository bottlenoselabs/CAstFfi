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
            "--config", "The file path to configure extraction of abstract syntax tree `.json` files.");
        configurationFilePathOption.SetDefaultValue("config.json");

        AddOption(configurationFilePathOption);
        this.SetHandler(Main, configurationFilePathOption);
    }

    private void Main(string configurationFilePath)
    {
        _tool.Run(configurationFilePath);
    }
}
