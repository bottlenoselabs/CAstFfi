// Copyright (c) Bottlenose Labs Inc. (https://github.com/bottlenoselabs). All rights reserved.
// Licensed under the MIT license. See LICENSE file in the Git repository root directory for full license information.

using System.CommandLine;
using CAstFfi.Features.Extract;
using CAstFfi.Features.Merge;

namespace CAstFfi;

public sealed class CommandLineInterface : RootCommand
{
    public CommandLineInterface(
        ExtractAbstractSyntaxTreeCommand extractCommand,
        MergeAbstractSyntaxTreesCommand mergeCommand)
        : base("Convert a cross-platform C header `.h` to a FFI (foreign function interface) abstract syntax tree (AST) `.json` for the purposes of generating bindings to other languages.")
    {
        AddCommand(extractCommand);
        AddCommand(mergeCommand);
    }
}
