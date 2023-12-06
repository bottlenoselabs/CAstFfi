// Copyright (c) Bottlenose Labs Inc. (https://github.com/bottlenoselabs). All rights reserved.
// Licensed under the MIT license. See LICENSE file in the Git repository root directory for full license information.

using System.Collections.Immutable;
using System.IO.Abstractions;
using System.Text.Json;
using bottlenoselabs.Common;
using bottlenoselabs.Common.Tools;
using CAstFfi.Features.Extract.Input.Sanitized;
using CAstFfi.Features.Extract.Input.Unsanitized;
using JsonSerializer = System.Text.Json.JsonSerializer;

namespace CAstFfi.Features.Extract.Input;

public sealed class ExtractInputSanitizer
{
    private readonly IFileSystem _fileSystem;

    private readonly JsonSerializerOptions _jsonSerializerOptions;

    public ExtractInputSanitizer(IFileSystem fileSystem)
    {
        _fileSystem = fileSystem;
        _jsonSerializerOptions = new JsonSerializerOptions
        {
            AllowTrailingCommas = true
        };
    }

    public ExtractOptions SanitizeFromFile(string filePath)
    {
        var path = _fileSystem.Path;
        var fullFilePath = path.GetFullPath(filePath);
        if (!_fileSystem.File.Exists(fullFilePath))
        {
            throw new ToolInputSanitizationException($"The extract options file '{fullFilePath}' does not exist.");
        }

        var fileContents = _fileSystem.File.ReadAllText(fullFilePath);
        if (string.IsNullOrEmpty(fileContents))
        {
            throw new ToolInputSanitizationException($"The extract options file '{fullFilePath}' is empty.");
        }

        var unsanitizedProgramOptions = JsonSerializer.Deserialize<UnsanitizedExtractOptions>(
            fileContents, _jsonSerializerOptions);
        if (unsanitizedProgramOptions == null)
        {
            throw new ToolInputSanitizationException("The extract options file is null.");
        }

        var previousCurrentDirectory = Environment.CurrentDirectory;
        Environment.CurrentDirectory = path.GetDirectoryName(fullFilePath)!;
        var result = Sanitize(unsanitizedProgramOptions);
        Environment.CurrentDirectory = previousCurrentDirectory;

        return result;
    }

    private ExtractOptions Sanitize(UnsanitizedExtractOptions options)
    {
        var inputFilePath = VerifyInputHeaderFilePath(options.InputFilePath);
        var abstractSyntaxTreeOptions = VerifyAbstractSyntaxTreeOptions(options, inputFilePath);

        var result = new ExtractOptions
        {
            InputFilePath = inputFilePath,
            AbstractSyntaxTreeOptions = abstractSyntaxTreeOptions
        };

        return result;
    }

    private ImmutableArray<ExtractAbstractSyntaxTreeOptions> VerifyAbstractSyntaxTreeOptions(
        UnsanitizedExtractOptions extractOptions,
        string inputFilePath)
    {
        var hostOperatingSystemString = Native.OperatingSystem.ToString().ToUpperInvariant();

        var abstractSyntaxTreeOptionsBuilder = ImmutableArray.CreateBuilder<ExtractAbstractSyntaxTreeOptions>();
        if (extractOptions.TargetPlatforms == null)
        {
            var extractOptionsByTargetPlatformString = new Dictionary<string, UnsanitizedExtractOptionsTargetPlatform>();
            var targetPlatformString = Native.Platform.ToString();
            extractOptionsByTargetPlatformString.Add(targetPlatformString, new UnsanitizedExtractOptionsTargetPlatform());

            var targetPlatformStringsByOperatingSystemString = new Dictionary<string, ImmutableDictionary<string, UnsanitizedExtractOptionsTargetPlatform>>();
            targetPlatformStringsByOperatingSystemString.Add(hostOperatingSystemString, extractOptionsByTargetPlatformString.ToImmutableDictionary());
            extractOptions.TargetPlatforms = targetPlatformStringsByOperatingSystemString.ToImmutableDictionary();
        }

        var isHandled = false;
        foreach (var (operatingSystemString, extractOptionsByTargetPlatformString) in extractOptions.TargetPlatforms)
        {
            if (!operatingSystemString.Equals(hostOperatingSystemString, StringComparison.OrdinalIgnoreCase))
            {
                continue;
            }

            isHandled = true;

            foreach (var (targetPlatformString, targetPlatformExtractOptions) in extractOptionsByTargetPlatformString)
            {
                var abstractSyntaxTreeOptions = SanitizeOptions(extractOptions, targetPlatformString, targetPlatformExtractOptions, inputFilePath);
                abstractSyntaxTreeOptionsBuilder.Add(abstractSyntaxTreeOptions);
            }
        }

        if (!isHandled)
        {
#pragma warning disable CA1308
            var operatingSystemString = Native.OperatingSystem.ToString().ToLowerInvariant();
#pragma warning restore CA1308
            throw new ToolInputSanitizationException(
                $"The current operating system '{operatingSystemString}' was not specified in the extraction options for target platforms.");
        }

        return abstractSyntaxTreeOptionsBuilder.ToImmutable();
    }

    private ExtractAbstractSyntaxTreeOptions SanitizeOptions(
        UnsanitizedExtractOptions options,
        string targetPlatformString,
        UnsanitizedExtractOptionsTargetPlatform targetPlatformOptions,
        string inputFilePath)
    {
        var systemIncludeDirectories = VerifyImmutableArray(options.SystemIncludeDirectories);
        var systemIncludeDirectoriesPlatform =
            VerifySystemIncludeDirectoriesPlatform(targetPlatformOptions.SystemIncludeDirectories, systemIncludeDirectories);

        var userIncludeDirectories = VerifyUserIncludeDirectories(options.UserIncludeDirectories, inputFilePath);
        var userIncludeDirectoriesPlatform =
            VerifyIncludeDirectoriesPlatform(
                targetPlatformOptions.UserIncludeDirectories,
                inputFilePath,
                userIncludeDirectories);

        var ignoredIncludeDirectories = VerifyImmutableArray(options.IgnoredIncludeDirectories);
        var ignoredIncludeDirectoriesPlatform =
            VerifyImmutableArrayFilePaths(targetPlatformOptions.IgnoredIncludeDirectories, ignoredIncludeDirectories);

        var outputFilePath = VerifyOutputFilePath(options.OutputDirectory, targetPlatformString);

        var clangDefines = VerifyImmutableArray(targetPlatformOptions?.Defines);
        var clangArguments = VerifyImmutableArray(targetPlatformOptions?.ClangArguments);

        var targetPlatform = new TargetPlatform(targetPlatformString);

        var inputAbstractSyntaxTree = new ExtractAbstractSyntaxTreeOptions
        {
            TargetPlatform = targetPlatform,
            OutputFilePath = outputFilePath,
            ExplorerOptions = new ExtractExploreOptions
            {
                IsEnabledSystemDeclarations = options.IsEnabledSystemDeclarations ?? false,
                IsEnabledOnlyExternalTopLevelCursors = options.IsEnabledOnlyExternalTopLevelCursors ?? true,
                OpaqueTypeNames = VerifyImmutableArray(options.OpaqueTypeNames).ToImmutableHashSet(),
                IgnoredIncludeDirectories = ignoredIncludeDirectoriesPlatform
            },
            ParseOptions = new ExtractParseOptions
            {
                UserIncludeDirectories = userIncludeDirectoriesPlatform,
                SystemIncludeDirectories = systemIncludeDirectoriesPlatform,
                IgnoredIncludeDirectories = ignoredIncludeDirectoriesPlatform,
                MacroObjectDefines = clangDefines,
                AdditionalArguments = clangArguments,
                IsEnabledFindSystemHeaders = options.IsEnabledAutomaticallyFindSystemHeaders ?? true,
                IsEnabledSystemDeclarations = options.IsEnabledSystemDeclarations ?? false,
            }
        };

        return inputAbstractSyntaxTree;
    }

    private string VerifyInputHeaderFilePath(string? inputFilePath)
    {
        if (string.IsNullOrEmpty(inputFilePath))
        {
            throw new ToolInputSanitizationException("The C input file can not be null, empty, or whitespace.");
        }

        var filePath = _fileSystem.Path.GetFullPath(inputFilePath);

        if (!_fileSystem.File.Exists(filePath))
        {
            throw new ToolInputSanitizationException($"The C input file does not exist: `{filePath}`.");
        }

        return filePath;
    }

    private string VerifyOutputFilePath(
        string? outputFileDirectory,
        string targetPlatformString)
    {
        string directoryPath;
        // ReSharper disable once ConvertIfStatementToConditionalTernaryExpression
        if (string.IsNullOrEmpty(outputFileDirectory))
        {
            directoryPath = _fileSystem.Path.Combine(Environment.CurrentDirectory, "ast");
        }
        else
        {
            directoryPath = _fileSystem.Path.GetFullPath(outputFileDirectory);
        }

        var defaultFilePath = _fileSystem.Path.Combine(directoryPath, targetPlatformString + ".json");
        return defaultFilePath;
    }

    private ImmutableArray<string> VerifyUserIncludeDirectories(
        ImmutableArray<string>? includeDirectories,
        string inputFilePath)
    {
        var result = VerifyImmutableArray(includeDirectories);

        if (result.IsDefaultOrEmpty)
        {
            var directoryPath = _fileSystem.Path.GetDirectoryName(inputFilePath)!;
            if (string.IsNullOrEmpty(directoryPath))
            {
                directoryPath = Environment.CurrentDirectory;
            }

            result = new[]
            {
                _fileSystem.Path.GetFullPath(directoryPath)
            }.ToImmutableArray();
        }
        else
        {
            result = result.Select(_fileSystem.Path.GetFullPath).ToImmutableArray();
        }

        foreach (var includeDirectory in result)
        {
            if (!_fileSystem.Directory.Exists(includeDirectory))
            {
                throw new ToolInputSanitizationException($"The include directory does not exist: `{includeDirectory}`.");
            }
        }

        return result;
    }

    private ImmutableArray<string> VerifyIncludeDirectoriesPlatform(
        ImmutableArray<string>? includeDirectoriesPlatform,
        string inputFilePath,
        ImmutableArray<string> includeDirectoriesNonPlatform)
    {
        if (includeDirectoriesPlatform == null || includeDirectoriesPlatform.Value.IsDefaultOrEmpty)
        {
            return includeDirectoriesNonPlatform;
        }

        var directoriesPlatform = VerifyUserIncludeDirectories(includeDirectoriesPlatform, inputFilePath);
        var result = directoriesPlatform.AddRange(includeDirectoriesNonPlatform);
        return result;
    }

    private ImmutableArray<string> VerifySystemIncludeDirectoriesPlatform(
        ImmutableArray<string>? includeDirectoriesPlatform,
        ImmutableArray<string> includeDirectoriesNonPlatform)
    {
        var directoriesPlatform = VerifyImmutableArray(includeDirectoriesPlatform);
        var result = directoriesPlatform.AddRange(includeDirectoriesNonPlatform);
        return result;
    }

    private ImmutableArray<string> VerifyImmutableArrayFilePaths(
        ImmutableArray<string>? ignoredIncludeDirectoriesPlatform,
        ImmutableArray<string> ignoredIncludeDirectoriesNonPlatform)
    {
        var directoriesPlatform = VerifyImmutableArray(ignoredIncludeDirectoriesPlatform);
        var directoryPaths = directoriesPlatform.AddRange(ignoredIncludeDirectoriesNonPlatform);

        var builder = ImmutableArray.CreateBuilder<string>();
        var path = _fileSystem.Path;
        foreach (var directoryPath in directoryPaths)
        {
            var fullDirectoryPath = path.GetFullPath(directoryPath);
            builder.Add(fullDirectoryPath);
        }

        return builder.ToImmutable();
    }

    private static ImmutableArray<string> VerifyImmutableArray(ImmutableArray<string>? array)
    {
        if (array == null || array.Value.IsDefaultOrEmpty)
        {
            return ImmutableArray<string>.Empty;
        }

        var result = array.Value
            .Where(x => !string.IsNullOrEmpty(x)).ToImmutableArray();
        return result;
    }
}
