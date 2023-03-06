// Copyright (c) Bottlenose Labs Inc. (https://github.com/bottlenoselabs). All rights reserved.
// Licensed under the MIT license. See LICENSE file in the Git repository root directory for full license information.

using CAstFfi.Data;
using JetBrains.Annotations;
using Microsoft.Extensions.Logging;
using static bottlenoselabs.clang;

namespace CAstFfi.Extract.Explore.Handlers;

[UsedImplicitly]
public class TypeAliasExplorer : ExploreNodeHandler<CTypeAlias>
{
    public TypeAliasExplorer(ILogger<TypeAliasExplorer> logger)
        : base(logger, false)
    {
    }

    protected override ExploreKindCursors ExpectedCursors { get; } =
        ExploreKindCursors.Is(CXCursorKind.CXCursor_TypedefDecl);

    protected override ExploreKindTypes ExpectedTypes { get; } = ExploreKindTypes.Is(CXTypeKind.CXType_Typedef);

    protected override CTypeAlias Explore(ExploreContext context, ExploreInfoNode info)
    {
        var typeAlias = TypeAlias(context, info);
        return typeAlias;
    }

    private static CTypeAlias TypeAlias(ExploreContext context, ExploreInfoNode info)
    {
        var aliasType = clang_getTypedefDeclUnderlyingType(info.Cursor);
        var aliasTypeInfo = context.VisitType(aliasType, info)!;
        var comment = context.Comment(info.Cursor);

        var typedef = new CTypeAlias
        {
            Name = info.Name,
            Location = info.Location,
            UnderlyingTypeInfo = aliasTypeInfo,
            Comment = comment
        };
        return typedef;
    }
}
