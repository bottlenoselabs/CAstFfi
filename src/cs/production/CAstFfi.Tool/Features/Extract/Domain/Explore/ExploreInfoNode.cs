// Copyright (c) Bottlenose Labs Inc. (https://github.com/bottlenoselabs). All rights reserved.
// Licensed under the MIT license. See LICENSE file in the Git repository root directory for full license information.

using CAstFfi.Data;
using static bottlenoselabs.clang;

namespace CAstFfi.Features.Extract.Domain.Explore;

#pragma warning disable CA1815
public class ExploreInfoNode
#pragma warning restore CA1815
{
    public CKind Kind { get; init; }

    public string Name { get; init; } = string.Empty;

    public string TypeName { get; init; } = string.Empty;

    public CXCursor Cursor { get; init; }

    public CXType Type { get; init; }

    public CLocation? Location { get; init; }

    public int SizeOf { get; init; }

    public int? AlignOf { get; init; }

    public ExploreInfoNode? Parent { get; init; }

    public override string ToString()
    {
        return Name;
    }
}
