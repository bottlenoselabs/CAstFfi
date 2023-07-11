// Copyright (c) Bottlenose Labs Inc. (https://github.com/bottlenoselabs). All rights reserved.
// Licensed under the MIT license. See LICENSE file in the Git repository root directory for full license information.

using System.Globalization;
using CAstFfi.Data;
using Microsoft.Extensions.Logging;
using static bottlenoselabs.clang;

namespace CAstFfi.Extract.Domain.Explore.Handlers;

public class EnumConstantExplorer : ExploreNodeHandler<CEnumConstant>
{
    public EnumConstantExplorer(
        ILogger<EnumConstantExplorer> logger)
        : base(logger, false)
    {
    }

    protected override ExploreKindCursors ExpectedCursors { get; } =
        ExploreKindCursors.Is(CXCursorKind.CXCursor_EnumConstantDecl);

    protected override ExploreKindTypes ExpectedTypes { get; } =
        ExploreKindTypes.Either(CXTypeKind.CXType_Int, CXTypeKind.CXType_UInt, CXTypeKind.CXType_ULong);

    protected override CEnumConstant Explore(ExploreContext context, ExploreInfoNode info)
    {
        var enumConstant = EnumConstant(context, info);
        return enumConstant;
    }

    private CEnumConstant EnumConstant(ExploreContext context, ExploreInfoNode info)
    {
        var typeInfo = context.VisitType(info.Type, info)!;
        var value = clang_getEnumConstantDeclValue(info.Cursor).ToString(CultureInfo.InvariantCulture);
        var comment = context.Comment(info.Cursor);

        var cursorLocation = clang_getCursorLocation(info.Cursor);
        var isSystemCursor = clang_Location_isInSystemHeader(cursorLocation) > 0;

        var result = new CEnumConstant
        {
            Name = info.Name,
            Location = info.Location ?? info.Parent!.Location,
            TypeInfo = typeInfo,
            Value = value,
            Comment = comment,
            IsSystem = isSystemCursor
        };

        return result;
    }
}
