// Copyright (c) Bottlenose Labs Inc. (https://github.com/bottlenoselabs). All rights reserved.
// Licensed under the MIT license. See LICENSE file in the Git repository root directory for full license information.

using CAstFfi.Data;
using JetBrains.Annotations;
using Microsoft.Extensions.Logging;
using static bottlenoselabs.clang;

namespace CAstFfi.Extract.Domain.Explore.Handlers;

[UsedImplicitly]
public sealed class VariableExplorer : ExploreNodeHandler<CVariable>
{
    public VariableExplorer(ILogger<VariableExplorer> logger)
        : base(logger, false)
    {
    }

    protected override ExploreKindCursors ExpectedCursors { get; } =
        ExploreKindCursors.Is(CXCursorKind.CXCursor_VarDecl);

    protected override ExploreKindTypes ExpectedTypes => ExploreKindTypes.Any;

    protected override CVariable Explore(ExploreContext context, ExploreInfoNode info)
    {
        var variable = Variable(context, info);
        return variable;
    }

    private static CVariable Variable(ExploreContext context, ExploreInfoNode info)
    {
        var comment = context.Comment(info.Cursor);

        var cursorLocation = clang_getCursorLocation(info.Cursor);
        var isSystemCursor = clang_Location_isInSystemHeader(cursorLocation) > 0;

        var result = new CVariable
        {
            Location = info.Location,
            Name = info.Name,
            Type = info.TypeName,
            Comment = comment,
            IsSystem = isSystemCursor
        };
        return result;
    }
}
