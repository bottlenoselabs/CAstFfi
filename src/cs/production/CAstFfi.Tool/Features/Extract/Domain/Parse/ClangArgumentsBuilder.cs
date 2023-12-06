// Copyright (c) Bottlenose Labs Inc. (https://github.com/bottlenoselabs). All rights reserved.
// Licensed under the MIT license. See LICENSE file in the Git repository root directory for full license information.

using System.Collections.Immutable;
using System.IO.Abstractions;
using bottlenoselabs.Common;
using CAstFfi.Features.Extract.Input.Sanitized;
using Microsoft.Extensions.Logging;

namespace CAstFfi.Features.Extract.Domain.Parse;

public sealed class ClangArgumentsBuilder
{
    private readonly ILogger<ClangArgumentsBuilder> _logger;
    private readonly IFileSystem _fileSystem;
    private readonly ClangSystemIncludeDirectoriesProvider _systemIncludeDirectoriesProvider;

    public ClangArgumentsBuilder(
        ILogger<ClangArgumentsBuilder> logger,
        IFileSystem fileSystem,
        ClangSystemIncludeDirectoriesProvider systemIncludeDirectoriesProvider)
    {
        _logger = logger;
        _fileSystem = fileSystem;
        _systemIncludeDirectoriesProvider = systemIncludeDirectoriesProvider;
    }

    public ClangArgumentsBuilderResult Build(
        TargetPlatform targetPlatform,
        ExtractParseOptions options,
        bool isCPlusPlus,
        bool ignoreWarnings)
    {
        var args = ImmutableArray.CreateBuilder<string>();

        AddDefaults(args, targetPlatform, isCPlusPlus, ignoreWarnings);
        AddUserIncludeDirectories(args, options.UserIncludeDirectories);
        AddDefines(args, options.MacroObjectDefines);
        AddTargetTriple(args, targetPlatform);
        AddAdditionalArgs(args, options.AdditionalArguments);

        var systemIncludeDirectories = _systemIncludeDirectoriesProvider
            .GetSystemIncludeDirectories(
                targetPlatform,
                options.SystemIncludeDirectories,
                options.IsEnabledFindSystemHeaders);
        AddSystemIncludeDirectories(args, systemIncludeDirectories);

        var arguments = args.ToImmutable();
        var result = new ClangArgumentsBuilderResult(arguments, systemIncludeDirectories);
        return result;
    }

    private void AddTargetTriple(ImmutableArray<string>.Builder args, TargetPlatform platform)
    {
        var targetTripleString = $"--target={platform}";
        args.Add(targetTripleString);
    }

    private static void AddDefaults(
        ImmutableArray<string>.Builder args,
        TargetPlatform platform,
        bool isCPlusPlus,
        bool ignoreWarnings)
    {
        if (isCPlusPlus)
        {
            args.Add("--language=c++");

            if (platform.OperatingSystem == NativeOperatingSystem.Linux)
            {
                args.Add("--std=gnu++11");
            }
            else
            {
                args.Add("--std=c++11");
            }
        }
        else
        {
            args.Add("--language=c");

            if (platform.OperatingSystem == NativeOperatingSystem.Linux)
            {
                args.Add("--std=gnu11");
            }
            else
            {
                args.Add("--std=c11");
            }

            args.Add("-fblocks");
            args.Add("-Wno-pragma-once-outside-header");
            args.Add("-fparse-all-comments");
        }

        if (ignoreWarnings)
        {
            args.Add("-Wno-everything");
        }
    }

    private static void AddUserIncludeDirectories(
        ImmutableArray<string>.Builder args, ImmutableArray<string> includeDirectories)
    {
        if (includeDirectories.IsDefaultOrEmpty)
        {
            return;
        }

        foreach (var searchDirectory in includeDirectories)
        {
            var commandLineArg = "--include-directory=" + searchDirectory;
            args.Add(commandLineArg);
        }
    }

    private static void AddDefines(ImmutableArray<string>.Builder args, ImmutableArray<string> defines)
    {
        if (defines.IsDefaultOrEmpty)
        {
            return;
        }

        foreach (var defineMacro in defines)
        {
            var commandLineArg = "--define-macro=" + defineMacro;
            args.Add(commandLineArg);
        }
    }

    private static void AddAdditionalArgs(ImmutableArray<string>.Builder args, ImmutableArray<string> additionalArgs)
    {
        if (additionalArgs.IsDefaultOrEmpty)
        {
            return;
        }

        foreach (var arg in additionalArgs)
        {
            args.Add(arg);
        }
    }

    private void AddSystemIncludeDirectories(
        ImmutableArray<string>.Builder args,
        ImmutableArray<string> systemIncludeDirectories)
    {
        if (systemIncludeDirectories.IsDefaultOrEmpty)
        {
            return;
        }

        foreach (var directory in systemIncludeDirectories)
        {
            args.Add($"-isystem{directory}");
        }
    }
}
