// Copyright (c) Bottlenose Labs Inc. (https://github.com/bottlenoselabs). All rights reserved.
// Licensed under the MIT license. See LICENSE file in the Git repository root directory for full license information.

using System.Collections.Immutable;
using CAstFfi.Data;
using CAstFfi.Data.Serialization;
using CAstFfi.Extract.Domain.Explore;
using CAstFfi.Extract.Domain.Parse;
using CAstFfi.Extract.Input;
using Microsoft.Extensions.Logging;

namespace CAstFfi.Extract;

public sealed partial class ExtractAbstractSyntaxTreeTool
{
    private readonly ILogger<ExtractAbstractSyntaxTreeTool> _logger;

    private readonly ExtractInputSanitizer _extractInputSanitizer;
    private readonly ClangInstaller _clangInstaller;
    private readonly Explorer _explorer;

    public ExtractAbstractSyntaxTreeTool(
        ILogger<ExtractAbstractSyntaxTreeTool> logger,
        ExtractInputSanitizer extractInputSanitizer,
        ClangInstaller clangInstaller,
        Explorer explorer)
    {
        _logger = logger;
        _extractInputSanitizer = extractInputSanitizer;
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

        var options = _extractInputSanitizer.SanitizeFromFile(configurationFilePath);
        foreach (var astOptions in options.AbstractSyntaxTreeOptions)
        {
            var abstractSyntaxTree = _explorer.GetAbstractSyntaxTree(
                options.InputFilePath,
                astOptions.TargetPlatform,
                astOptions.ParseOptions,
                astOptions.ExplorerOptions);

            try
            {
                CJsonSerializer.WriteAbstractSyntaxTreeTargetPlatform(abstractSyntaxTree, astOptions.OutputFilePath);
            }
#pragma warning disable CA1031
            catch (Exception e)
#pragma warning restore CA1031
            {
                LogWriteAbstractSyntaxTreeTargetPlatformFailure(e, abstractSyntaxTree.PlatformRequested, astOptions.OutputFilePath);
                return;
            }

            LogWriteAbstractSyntaxTreeTargetPlatformSuccess(abstractSyntaxTree.PlatformRequested, astOptions.OutputFilePath);
        }

        var targetPlatforms = options.AbstractSyntaxTreeOptions.Select(x => x.TargetPlatform).ToImmutableArray();
        LogSuccess(targetPlatforms);
    }

    [LoggerMessage(0, LogLevel.Information, "Success. Extracted abstract syntax tree for the target platform '{TargetPlatform}': {FilePath}")]
    private partial void LogWriteAbstractSyntaxTreeTargetPlatformSuccess(
        CTargetPlatform targetPlatform,
        string filePath);

    [LoggerMessage(1, LogLevel.Error, "Failed to extract abstract syntax tree for the target platform '{TargetPlatform}': {FilePath}")]
    private partial void LogWriteAbstractSyntaxTreeTargetPlatformFailure(Exception exception,
        CTargetPlatform targetPlatform,
        string filePath);

    [LoggerMessage(2, LogLevel.Information, "Success. Extracted abstract syntax trees for the target platforms '{TargetPlatforms}'.")]
    private partial void LogSuccess(ImmutableArray<CTargetPlatform> targetPlatforms);
}
