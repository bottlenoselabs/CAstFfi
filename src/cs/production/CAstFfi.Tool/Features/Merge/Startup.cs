// Copyright (c) Bottlenose Labs Inc. (https://github.com/bottlenoselabs). All rights reserved.
// Licensed under the MIT license. See LICENSE file in the Git repository root directory for full license information.

using CAstFfi.Features.Merge.Input;
using Microsoft.Extensions.DependencyInjection;

namespace CAstFfi.Features.Merge;

public static class Startup
{
    public static void ConfigureServices(IServiceCollection services)
    {
        services.AddSingleton<MergeAbstractSyntaxTreesCommand>();
        services.AddSingleton<Features.Merge.MergeAbstractSyntaxTreesTool>();
        services.AddSingleton<MergeInputSanitizer>();
    }
}
