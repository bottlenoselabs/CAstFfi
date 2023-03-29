// Copyright (c) Bottlenose Labs Inc. (https://github.com/bottlenoselabs). All rights reserved.
// Licensed under the MIT license. See LICENSE file in the Git repository root directory for full license information.

using System.Collections.Immutable;
using System.IO.Abstractions;
using System.Text.Json;
using CAstFfi.Common;
using CAstFfi.Data;
using CAstFfi.Extract.Input.Sanitized;
using CAstFfi.Extract.Input.Unsanitized;
using JsonSerializer = System.Text.Json.JsonSerializer;

namespace CAstFfi.Extract.Input;

public sealed class ExtractInputSanitizer
{
    private readonly IFileSystem _fileSystem;

    public ExtractInputSanitizer(IFileSystem fileSystem)
    {
        _fileSystem = fileSystem;
    }

    public ExtractOptions SanitizeFromFile(string filePath)
    {
        var fullFilePath = _fileSystem.Path.GetFullPath(filePath);
        if (!_fileSystem.File.Exists(fullFilePath))
        {
            throw new InputSanitizationException($"The extract options file '{filePath}' does not exist.");
        }

        var fileContents = _fileSystem.File.ReadAllText(fullFilePath);
        if (string.IsNullOrEmpty(fileContents))
        {
            throw new InputSanitizationException($"The extract options file '{filePath}' is empty.");
        }

        var serializerOptions = new JsonSerializerOptions
        {
            AllowTrailingCommas = true
        };
        var unsanitizedProgramOptions = JsonSerializer.Deserialize<UnsanitizedExtractOptions>(fileContents, serializerOptions);
        if (unsanitizedProgramOptions == null)
        {
            throw new InputSanitizationException("The extract options file is null.");
        }

        var previousCurrentDirectory = Environment.CurrentDirectory;
        Environment.CurrentDirectory = Path.GetDirectoryName(filePath)!;
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
        var abstractSyntaxTreeOptionsBuilder = ImmutableArray.CreateBuilder<ExtractAbstractSyntaxTreeOptions>();
        if (extractOptions.Platforms == null)
        {
            var abstractSyntaxTreeRequests = new Dictionary<string, UnsanitizedExtractOptionsTargetPlatform>();
            var targetPlatform = NativeUtility.HostPlatform.ToString();
            abstractSyntaxTreeRequests.Add(targetPlatform, new UnsanitizedExtractOptionsTargetPlatform());
            extractOptions.Platforms = abstractSyntaxTreeRequests.ToImmutableDictionary();
        }

        foreach (var (targetPlatformString, targetPlatformOptions) in extractOptions.Platforms)
        {
            var abstractSyntaxTreeOptions = SanitizeOptions(extractOptions, targetPlatformString, targetPlatformOptions, inputFilePath);
            abstractSyntaxTreeOptionsBuilder.Add(abstractSyntaxTreeOptions);
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
            VerifySystemDirectoriesPlatform(targetPlatformOptions?.SystemIncludeDirectories, systemIncludeDirectories);

        var userIncludeDirectories = VerifyIncludeDirectories(options.UserIncludeDirectories, inputFilePath);
        var userIncludeDirectoriesPlatform =
            VerifyIncludeDirectoriesPlatform(
                targetPlatformOptions?.UserIncludeDirectories,
                inputFilePath,
                userIncludeDirectories);

        var frameworks = VerifyImmutableArray(options.AppleFrameworks);
        var frameworksPlatform = VerifyFrameworks(targetPlatformOptions?.AppleFrameworks, frameworks);

        var outputFilePath = VerifyOutputFilePath(options.OutputDirectory, targetPlatformString);

        var clangDefines = VerifyImmutableArray(targetPlatformOptions?.Defines);
        var clangArguments = VerifyImmutableArray(targetPlatformOptions?.ClangArguments);

        var targetPlatform = new CTargetPlatform(targetPlatformString);

        var inputAbstractSyntaxTree = new ExtractAbstractSyntaxTreeOptions
        {
            TargetPlatform = targetPlatform,
            OutputFilePath = outputFilePath,
            ExplorerOptions = new ExtractExploreOptions
            {
                IsEnabledSystemDeclarations = options.IsEnabledSystemDeclarations ?? false,
                IsEnabledOnlyExternalTopLevelCursors = options.IsEnabledOnlyExternalTopLevelCursors ?? true,
            },
            ParseOptions = new ExtractParseOptions
            {
                UserIncludeDirectories = userIncludeDirectoriesPlatform,
                SystemIncludeDirectories = systemIncludeDirectoriesPlatform,
                MacroObjectDefines = clangDefines,
                AdditionalArguments = clangArguments,
                IsEnabledFindSystemHeaders = options.IsEnabledAutomaticallyFindSystemHeaders ?? true,
                AppleFrameworks = frameworksPlatform,
                IsEnabledSystemDeclarations = options.IsEnabledSystemDeclarations ?? false,
                IsEnabledSingleHeader = options.IsEnabledParseAsSingleHeader ?? true
            }
        };

        return inputAbstractSyntaxTree;
    }

    private string VerifyInputHeaderFilePath(string? inputFilePath)
    {
        if (string.IsNullOrEmpty(inputFilePath))
        {
            throw new InputSanitizationException("The C input file can not be null, empty, or whitespace.");
        }

        var filePath = _fileSystem.Path.GetFullPath(inputFilePath);

        if (!_fileSystem.File.Exists(filePath))
        {
            throw new InputSanitizationException($"The C input file does not exist: `{filePath}`.");
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

    private ImmutableArray<string> VerifyIncludeDirectories(
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
                throw new InputSanitizationException($"The include directory does not exist: `{includeDirectory}`.");
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

        var directoriesPlatform = VerifyIncludeDirectories(includeDirectoriesPlatform, inputFilePath);
        var result = directoriesPlatform.AddRange(includeDirectoriesNonPlatform);
        return result;
    }

    private ImmutableArray<string> VerifySystemDirectoriesPlatform(
        ImmutableArray<string>? includeDirectoriesPlatform,
        ImmutableArray<string> includeDirectoriesNonPlatform)
    {
        var directoriesPlatform = VerifyImmutableArray(includeDirectoriesPlatform);
        var result = directoriesPlatform.AddRange(includeDirectoriesNonPlatform);
        return result;
    }

    private ImmutableArray<string> VerifyFrameworks(
        ImmutableArray<string>? platformFrameworks,
        ImmutableArray<string> frameworksNonPlatform)
    {
        var directoriesPlatform = VerifyImmutableArray(platformFrameworks);
        var result = directoriesPlatform.AddRange(frameworksNonPlatform);
        return result;
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
