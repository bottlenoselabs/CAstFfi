// Copyright (c) Bottlenose Labs Inc. (https://github.com/bottlenoselabs). All rights reserved.
// Licensed under the MIT license. See LICENSE file in the Git repository root directory for full license information.

using System.Collections.Immutable;

namespace CAstFfi.Extract.Input.Sanitized;

public sealed class ExtractExploreOptions
{
    public bool IsEnabledSystemDeclarations { get; init; }

    public bool IsEnabledOnlyExternalTopLevelCursors { get; init; }

    public ImmutableHashSet<string> OpaqueTypeNames { get; init; } = ImmutableHashSet<string>.Empty;
}
