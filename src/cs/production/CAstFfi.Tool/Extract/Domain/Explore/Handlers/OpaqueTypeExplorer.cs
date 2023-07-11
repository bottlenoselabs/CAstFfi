// Copyright (c) Bottlenose Labs Inc. (https://github.com/bottlenoselabs). All rights reserved.
// Licensed under the MIT license. See LICENSE file in the Git repository root directory for full license information.

using CAstFfi.Data;
using JetBrains.Annotations;
using Microsoft.Extensions.Logging;
using static bottlenoselabs.clang;

namespace CAstFfi.Extract.Domain.Explore.Handlers;

[UsedImplicitly]
public sealed class OpaqueTypeExplorer : ExploreNodeHandler<COpaqueType>
{
    public OpaqueTypeExplorer(ILogger<OpaqueTypeExplorer> logger)
        : base(logger, false)
    {
    }

    protected override ExploreKindCursors ExpectedCursors => ExploreKindCursors.Any;

    protected override ExploreKindTypes ExpectedTypes => ExploreKindTypes.Any;

    protected override COpaqueType Explore(ExploreContext context, ExploreInfoNode info)
    {
        var opaqueDataType = OpaqueDataType(context, info);
        return opaqueDataType;
    }

    private static COpaqueType OpaqueDataType(ExploreContext context, ExploreInfoNode info)
    {
        var comment = context.Comment(info.Cursor);

        var cursorLocation = clang_getCursorLocation(info.Cursor);
        var isSystemCursor = clang_Location_isInSystemHeader(cursorLocation) > 0;

        var result = new COpaqueType
        {
            Name = info.Name,
            Location = info.Location,
            SizeOf = info.SizeOf,
            Comment = comment,
            IsSystem = isSystemCursor
        };

        return result;
    }
}
