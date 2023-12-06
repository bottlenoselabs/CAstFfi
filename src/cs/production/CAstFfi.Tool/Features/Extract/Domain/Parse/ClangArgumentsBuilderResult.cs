// Copyright (c) Bottlenose Labs Inc. (https://github.com/bottlenoselabs). All rights reserved.
// Licensed under the MIT license. See LICENSE file in the Git repository root directory for full license information.

using System.Collections.Immutable;

namespace CAstFfi.Features.Extract.Domain.Parse;

public class ClangArgumentsBuilderResult
{
    public readonly ImmutableArray<string> Arguments;
    public readonly ImmutableArray<string> SystemIncludeDirectories;

    public ClangArgumentsBuilderResult(
        ImmutableArray<string> arguments,
        ImmutableArray<string> systemIncludeDirectories)
    {
        Arguments = arguments;
        SystemIncludeDirectories = systemIncludeDirectories;
    }
}
