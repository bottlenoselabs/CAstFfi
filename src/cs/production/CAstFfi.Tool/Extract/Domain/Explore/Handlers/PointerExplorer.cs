// Copyright (c) Bottlenose Labs Inc. (https://github.com/bottlenoselabs). All rights reserved.
// Licensed under the MIT license. See LICENSE file in the Git repository root directory for full license information.

using CAstFfi.Data;
using JetBrains.Annotations;
using Microsoft.Extensions.Logging;
using static bottlenoselabs.clang;

namespace CAstFfi.Extract.Domain.Explore.Handlers;

[UsedImplicitly]
public sealed class PointerExplorer : ExploreNodeHandler<CPointer>
{
    public PointerExplorer(ILogger<PointerExplorer> logger)
        : base(logger, false)
    {
    }

    protected override ExploreKindCursors ExpectedCursors => ExploreKindCursors.Any;

    protected override ExploreKindTypes ExpectedTypes { get; } = ExploreKindTypes.Is(CXTypeKind.CXType_Pointer);

    protected override CPointer Explore(ExploreContext context, ExploreInfoNode info)
    {
        var pointer = Pointer(context, info);
        return pointer;
    }

    private static CPointer Pointer(ExploreContext context, ExploreInfoNode info)
    {
        var type = clang_getPointeeType(info.Type);
        var typeInfo = context.VisitType(type, info)!;
        var comment = context.Comment(info.Cursor);

        var cursorLocation = clang_getCursorLocation(info.Cursor);
        var isSystemCursor = clang_Location_isInSystemHeader(cursorLocation) > 0;

        var result = new CPointer
        {
            Name = info.Name,
            TypeInfo = typeInfo,
            Comment = comment,
            IsSystem = isSystemCursor
        };

        return result;
    }
}
