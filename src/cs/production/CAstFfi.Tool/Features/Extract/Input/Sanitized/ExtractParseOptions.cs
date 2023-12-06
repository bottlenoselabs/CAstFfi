// Copyright (c) Bottlenose Labs Inc. (https://github.com/bottlenoselabs). All rights reserved.
// Licensed under the MIT license. See LICENSE file in the Git repository root directory for full license information.

using System.Collections.Immutable;

namespace CAstFfi.Features.Extract.Input.Sanitized;

public sealed class ExtractParseOptions
{
    public ImmutableArray<string> UserIncludeDirectories { get; init; } = ImmutableArray<string>.Empty;

    public ImmutableArray<string> SystemIncludeDirectories { get; init; } = ImmutableArray<string>.Empty;

    public ImmutableArray<string> IgnoredIncludeDirectories { get; init; } = ImmutableArray<string>.Empty;

    public ImmutableArray<string> MacroObjectDefines { get; init; } = ImmutableArray<string>.Empty;

    public ImmutableArray<string> AdditionalArguments { get; init; } = ImmutableArray<string>.Empty;

    public bool IsEnabledFindSystemHeaders { get; init; }

    public bool IsEnabledSystemDeclarations { get; init; }
}
