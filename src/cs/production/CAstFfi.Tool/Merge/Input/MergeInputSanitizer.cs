// Copyright (c) Bottlenose Labs Inc. (https://github.com/bottlenoselabs). All rights reserved.
// Licensed under the MIT license. See LICENSE file in the Git repository root directory for full license information.

using System.Collections.Immutable;
using System.IO.Abstractions;
using CAstFfi.Common;
using CAstFfi.Data;
using CAstFfi.Merge.Input.Sanitized;
using CAstFfi.Merge.Input.Unsanitized;

namespace CAstFfi.Merge.Input;

public sealed class MergeInputSanitizer
{
    private readonly IFileSystem _fileSystem;

    public MergeInputSanitizer(IFileSystem fileSystem)
    {
        _fileSystem = fileSystem;
    }

    public MergeOptions Sanitize(UnsanitizedMergeOptions unsanitizedOptions)
    {
        var directoryPath = _fileSystem.Path.GetFullPath(unsanitizedOptions.InputDirectoryPath);
        if (!_fileSystem.Directory.Exists(directoryPath))
        {
            throw new InputSanitizationException($"The directory '{directoryPath}' does not exist.");
        }

        var filePaths = _fileSystem.Directory.GetFiles(directoryPath, "*.json").ToImmutableArray();

        if (filePaths.IsDefaultOrEmpty)
        {
            throw new InputSanitizationException($"The directory '{directoryPath}' does not contain any abstract syntax tree `.json` files.");
        }

        var outputFilePath = _fileSystem.Path.GetFullPath(unsanitizedOptions.OutputFilePath);

        var result = new MergeOptions
        {
            OutputFilePath = outputFilePath,
            InputFilePaths = filePaths
        };

        return result;
    }
}
