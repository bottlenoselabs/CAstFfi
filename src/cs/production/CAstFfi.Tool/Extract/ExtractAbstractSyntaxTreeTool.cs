// Copyright (c) Bottlenose Labs Inc. (https://github.com/bottlenoselabs). All rights reserved.
// Licensed under the MIT license. See LICENSE file in the Git repository root directory for full license information.

using System.Collections.Immutable;
using CAstFfi.Data;
using CAstFfi.Data.Serialization;
using CAstFfi.Extract.Explore;
using CAstFfi.Extract.Input;
using CAstFfi.Extract.Parse;
using Microsoft.Extensions.Logging;

namespace CAstFfi.Extract;

public sealed partial class ExtractAbstractSyntaxTreeTool
{
    private readonly ILogger<ExtractAbstractSyntaxTreeTool> _logger;

    private readonly ProgramInputSanitizer _programInputSanitizer;
    private readonly ClangInstaller _clangInstaller;
    private readonly Explorer _explorer;

    public ExtractAbstractSyntaxTreeTool(
        ILogger<ExtractAbstractSyntaxTreeTool> logger,
        ProgramInputSanitizer programInputSanitizer,
        ClangInstaller clangInstaller,
        Explorer explorer)
    {
        _logger = logger;
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

            try
            {
                CJsonSerializer.WriteAbstractSyntaxTreeTargetPlatform(abstractSyntaxTree, options.OutputFilePath);
            }
#pragma warning disable CA1031
            catch (Exception e)
#pragma warning restore CA1031
            {
                LocalFailure(e, abstractSyntaxTree.PlatformRequested, options.OutputFilePath);
                return;
            }

            LocalSuccess(abstractSyntaxTree.PlatformRequested, options.OutputFilePath);
        }

        var targetPlatforms = programOptions.AbstractSyntaxTreeOptions.Select(x => x.TargetPlatform).ToImmutableArray();
        TotalSuccess(targetPlatforms);
    }

    [LoggerMessage(0, LogLevel.Information, "Success. Extracted abstract syntax tree for the target platform '{TargetPlatform}': {FilePath}")]
    private partial void LocalSuccess(CTargetPlatform targetPlatform, string filePath);

    [LoggerMessage(1, LogLevel.Error, "Failed to extract abstract syntax tree for the target platform '{TargetPlatform}': {FilePath}")]
    private partial void LocalFailure(Exception exception, CTargetPlatform targetPlatform, string filePath);

    [LoggerMessage(2, LogLevel.Information, "Success. Extracted abstract syntax trees for the target platforms '{TargetPlatforms}'.")]
    private partial void TotalSuccess(ImmutableArray<CTargetPlatform> targetPlatforms);
}
