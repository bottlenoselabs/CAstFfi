// Copyright (c) Bottlenose Labs Inc. (https://github.com/bottlenoselabs). All rights reserved.
// Licensed under the MIT license. See LICENSE file in the Git repository root directory for full license information.

using CAstFfi.Data;
using CAstFfi.Data.Serialization;
using CAstFfi.Extract.Explore;
using CAstFfi.Extract.Input;
using CAstFfi.Extract.Parse;

namespace CAstFfi.Extract;

public sealed class ExtractAbstractSyntaxTreeTool
{
    private readonly ProgramInputSanitizer _programInputSanitizer;
    private readonly ClangInstaller _clangInstaller;
    private readonly Explorer _explorer;

    public ExtractAbstractSyntaxTreeTool(
        ProgramInputSanitizer programInputSanitizer,
        ClangInstaller clangInstaller,
        Explorer explorer)
    {
        _programInputSanitizer = programInputSanitizer;
        _clangInstaller = clangInstaller;
        _explorer = explorer;
    }

    public void Run(string configurationFilePath)
    {
        var isClangInstalled = _clangInstaller.Install();
        if (!isClangInstalled)
        {
            return;
        }

        var programOptions = _programInputSanitizer.SanitizeFromFile(configurationFilePath);
        foreach (var options in programOptions.AbstractSyntaxTreeOptions)
        {
            var abstractSyntaxTree = _explorer.GetAbstractSyntaxTree(
                programOptions.InputFilePath,
                options.TargetPlatform,
                options.ParseOptions,
                options.ExplorerOptions);

            CJsonSerializer.WriteAbstractSyntaxTreeTargetPlatform(abstractSyntaxTree, options.OutputFilePath);
        }
    }
}
