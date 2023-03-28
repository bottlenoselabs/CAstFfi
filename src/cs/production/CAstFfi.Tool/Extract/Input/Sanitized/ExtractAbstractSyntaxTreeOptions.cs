// Copyright (c) Bottlenose Labs Inc. (https://github.com/bottlenoselabs). All rights reserved.
// Licensed under the MIT license. See LICENSE file in the Git repository root directory for full license information.

using CAstFfi.Data;

namespace CAstFfi.Extract.Input.Sanitized;

public sealed class ExtractAbstractSyntaxTreeOptions
{
    public string OutputFilePath { get; init; } = string.Empty;

    public CTargetPlatform TargetPlatform { get; init; } = CTargetPlatform.Unknown;

    public ExtractExploreOptions ExplorerOptions { get; init; } = null!;

    public ExtractParseOptions ParseOptions { get; init; } = null!;
}
