// Copyright (c) Bottlenose Labs Inc. (https://github.com/bottlenoselabs). All rights reserved.
// Licensed under the MIT license. See LICENSE file in the Git repository root directory for full license information.

using CAstFfi.Data;
using JetBrains.Annotations;
using Microsoft.Extensions.Logging;
using static bottlenoselabs.clang;

namespace CAstFfi.Features.Extract.Domain.Explore.Handlers;

[UsedImplicitly]
public sealed class PrimitiveExplorer : ExploreNodeHandler<CPrimitive>
{
    public PrimitiveExplorer(ILogger<PrimitiveExplorer> logger)
        : base(logger, false)
    {
    }

    protected override ExploreKindCursors ExpectedCursors => ExploreKindCursors.Any;

    protected override ExploreKindTypes ExpectedTypes { get; } = ExploreKindTypes.Either(
        CXTypeKind.CXType_Void,
        CXTypeKind.CXType_Bool,
        CXTypeKind.CXType_Char_U,
        CXTypeKind.CXType_UChar,
        CXTypeKind.CXType_Char16,
        CXTypeKind.CXType_Char32,
        CXTypeKind.CXType_UShort,
        CXTypeKind.CXType_UInt,
        CXTypeKind.CXType_ULong,
        CXTypeKind.CXType_ULongLong,
        CXTypeKind.CXType_UInt128,
        CXTypeKind.CXType_Char_S,
        CXTypeKind.CXType_SChar,
        CXTypeKind.CXType_WChar,
        CXTypeKind.CXType_Short,
        CXTypeKind.CXType_Int,
        CXTypeKind.CXType_Long,
        CXTypeKind.CXType_LongLong,
        CXTypeKind.CXType_Int128,
        CXTypeKind.CXType_Float,
        CXTypeKind.CXType_Double,
        CXTypeKind.CXType_LongDouble);

    protected override CPrimitive Explore(ExploreContext context, ExploreInfoNode info)
    {
        var result = Primitive(context, info);
        return result;
    }

    private static CPrimitive Primitive(ExploreContext context, ExploreInfoNode info)
    {
        var typeInfo = context.VisitType(info.Type, info)!;
        var comment = context.Comment(info.Cursor);

        var cursorLocation = clang_getCursorLocation(info.Cursor);
        var isSystemCursor = clang_Location_isInSystemHeader(cursorLocation) > 0;

        var result = new CPrimitive
        {
            Name = info.Name,
            TypeInfo = typeInfo,
            Comment = comment,
            IsSystem = isSystemCursor
        };
        return result;
    }
}
