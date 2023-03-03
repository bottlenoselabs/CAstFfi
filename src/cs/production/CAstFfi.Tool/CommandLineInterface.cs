// Copyright (c) Bottlenose Labs Inc. (https://github.com/bottlenoselabs). All rights reserved.
// Licensed under the MIT license. See LICENSE file in the Git repository root directory for full license information.

using System.CommandLine;
using CAstFfi.Domain.Explore;
using CAstFfi.Domain.Parse;
using CAstFfi.IO.Input;
using CAstFfi.IO.Output.Serialization;

namespace CAstFfi;

public sealed class CommandLineInterface : RootCommand
{
    private readonly Tool _tool;

    public CommandLineInterface(Tool tool)
        : base("Convert a cross-platform C header `.h` to to a FFI (foreign function interface) abstract syntax tree (AST) `.json` for the purposes of generating bindings to other languages.")
    {
        _tool = tool;

        var configurationFilePathOption = new Option<string>("config", "The file path to the program configuration `.json` file.");
        configurationFilePathOption.SetDefaultValue("config.json");
        AddGlobalOption(configurationFilePathOption);

        this.SetHandler(Main, configurationFilePathOption);
    }

    private void Main(string configurationFilePath)
    {
        _tool.Run(configurationFilePath);
    }
}
