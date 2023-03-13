// Copyright (c) Bottlenose Labs Inc. (https://github.com/bottlenoselabs). All rights reserved.
// Licensed under the MIT license. See LICENSE file in the Git repository root directory for full license information.

using System.Collections.Immutable;
using System.IO.Abstractions;
using System.Text.Json;
using CAstFfi.Data;
using CAstFfi.Extract.Input.Sanitized;
using CAstFfi.Extract.Input.Unsanitized;
using JsonSerializer = System.Text.Json.JsonSerializer;

namespace CAstFfi.Extract.Input;

public sealed class ProgramInputSanitizer
{
    private readonly IFileSystem _fileSystem;

    public ProgramInputSanitizer(IFileSystem fileSystem)
    {
        _fileSystem = fileSystem;
    }

    public ProgramOptions SanitizeFromFile(string filePath)
    {
        if (!_fileSystem.File.Exists(filePath))
        {
            throw new ProgramInputSanitizationException($"The program options file '{filePath}' does not exist.");
        }

        var fileContents = _fileSystem.File.ReadAllText(filePath);
        if (string.IsNullOrEmpty(fileContents))
        {
            throw new ProgramInputSanitizationException($"The program options file '{filePath}' is empty.");
        }

        var serializerOptions = new JsonSerializerOptions
        {
            AllowTrailingCommas = true
        };
        var unsanitizedProgramOptions = JsonSerializer.Deserialize<UnsanitizedProgramOptions>(fileContents, serializerOptions);
        if (unsanitizedProgramOptions == null)
        {
            throw new ProgramInputSanitizationException("The program options file is null.");
        }

        var result = Sanitize(unsanitizedProgramOptions);
        return result;
    }

    private ProgramOptions Sanitize(UnsanitizedProgramOptions options)
    {
        var inputFilePath = VerifyInputHeaderFilePath(options.InputFilePath);
        var abstractSyntaxTreeOptions = VerifyAbstractSyntaxTreeOptions(options, inputFilePath);

        var result = new ProgramOptions
        {
            InputFilePath = inputFilePath,
            AbstractSyntaxTreeOptions = abstractSyntaxTreeOptions
        };

        return result;
    }

    private ImmutableArray<AbstractSyntaxTreeOptions> VerifyAbstractSyntaxTreeOptions(
        UnsanitizedProgramOptions programOptions,
        string inputFilePath)
    {
        var abstractSyntaxTreeOptionsBuilder = ImmutableArray.CreateBuilder<AbstractSyntaxTreeOptions>();
        if (programOptions.Platforms == null)
        {
            var abstractSyntaxTreeRequests = new Dictionary<string, UnsanitizedTargetPlatformOptions>();
            var targetPlatform = NativeUtility.HostPlatform.ToString();
            abstractSyntaxTreeRequests.Add(targetPlatform, new UnsanitizedTargetPlatformOptions());
            programOptions.Platforms = abstractSyntaxTreeRequests.ToImmutableDictionary();
        }

        foreach (var (targetPlatformString, targetPlatformOptions) in programOptions.Platforms)
        {
            var abstractSyntaxTreeOptions = SanitizeOptions(programOptions, targetPlatformString, targetPlatformOptions, inputFilePath);
            abstractSyntaxTreeOptionsBuilder.Add(abstractSyntaxTreeOptions);
        }

        return abstractSyntaxTreeOptionsBuilder.ToImmutable();
    }

    private AbstractSyntaxTreeOptions SanitizeOptions(
        UnsanitizedProgramOptions options,
        string targetPlatformString,
        UnsanitizedTargetPlatformOptions targetPlatformOptions,
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

        var inputAbstractSyntaxTree = new AbstractSyntaxTreeOptions
        {
            TargetPlatform = targetPlatform,
            OutputFilePath = outputFilePath,
            ExplorerOptions = new ExploreOptions
            {
                IsEnabledSystemDeclarations = options.IsEnabledSystemDeclarations ?? false,
                IsEnabledOnlyExternalTopLevelCursors = options.IsEnabledOnlyExternalTopLevelCursors ?? true,
            },
            ParseOptions = new ParseOptions
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
            throw new ProgramInputSanitizationException("The C input file can not be null, empty, or whitespace.");
        }

        var filePath = _fileSystem.Path.GetFullPath(inputFilePath);

        if (!_fileSystem.File.Exists(filePath))
        {
            throw new ProgramInputSanitizationException($"The C input file does not exist: `{filePath}`.");
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
                throw new ProgramInputSanitizationException($"The include directory does not exist: `{includeDirectory}`.");
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
