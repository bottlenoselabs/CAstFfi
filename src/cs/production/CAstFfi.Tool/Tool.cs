// Copyright (c) Bottlenose Labs Inc. (https://github.com/bottlenoselabs). All rights reserved.
// Licensed under the MIT license. See LICENSE file in the Git repository root directory for full license information.

using CAstFfi.Domain.Explore;
using CAstFfi.Domain.Parse;
using CAstFfi.IO.Input;
using CAstFfi.IO.Output.Serialization;

namespace CAstFfi;

public sealed class Tool
{
    private readonly ProgramInputSanitizer _programInputSanitizer;
    private readonly ClangInstaller _clangInstaller;
    private readonly Explorer _explorer;
    private readonly CJsonSerializer _serializer;

    public Tool(
        ProgramInputSanitizer programInputSanitizer,
        ClangInstaller clangInstaller,
        Explorer explorer,
        CJsonSerializer serializer)
    {
        _programInputSanitizer = programInputSanitizer;
        _clangInstaller = clangInstaller;
        _explorer = explorer;
        _serializer = serializer;
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

            _serializer.Write(abstractSyntaxTree, options.OutputFilePath);
        }
    }
}
