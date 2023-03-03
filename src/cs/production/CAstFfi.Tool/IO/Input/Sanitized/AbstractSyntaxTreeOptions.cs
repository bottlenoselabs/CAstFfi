// Copyright (c) Bottlenose Labs Inc. (https://github.com/bottlenoselabs). All rights reserved.
// Licensed under the MIT license. See LICENSE file in the Git repository root directory for full license information.

using CAstFfi.IO.Output;
using CAstFfi.Native;

namespace CAstFfi.IO.Input.Sanitized;

public sealed class AbstractSyntaxTreeOptions
{
    public string OutputFilePath { get; init; } = string.Empty;

    public CTargetPlatform TargetPlatform { get; init; } = CTargetPlatform.Unknown;

    public ExploreOptions ExplorerOptions { get; init; } = null!;

    public ParseOptions ParseOptions { get; init; } = null!;
}
