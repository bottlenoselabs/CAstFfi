// Copyright (c) Bottlenose Labs Inc. (https://github.com/bottlenoselabs). All rights reserved.
// Licensed under the MIT license. See LICENSE file in the Git repository root directory for full license information.

using CAstFfi.Extract.Domain.Explore;
using CAstFfi.Extract.Domain.Explore.Handlers;
using CAstFfi.Extract.Domain.Parse;
using CAstFfi.Extract.Input;
using Microsoft.Extensions.DependencyInjection;

namespace CAstFfi.Extract;

public static class Startup
{
    public static void ConfigureServices(IServiceCollection services)
    {
        services.AddSingleton<ExtractInputSanitizer>();

        services.AddSingleton<ClangInstaller>();
        services.AddSingleton<ClangArgumentsBuilder>();
        services.AddSingleton<Parser>();

        services.AddSingleton<Explorer>();
        services.AddSingleton<ArrayExplorer>();
        services.AddSingleton<EnumConstantExplorer>();
        services.AddSingleton<EnumExplorer>();
        services.AddSingleton<FunctionExplorer>();
        services.AddSingleton<FunctionPointerExplorer>();
        services.AddSingleton<OpaqueTypeExplorer>();
        services.AddSingleton<PointerExplorer>();
        services.AddSingleton<PrimitiveExplorer>();
        services.AddSingleton<StructExplorer>();
        services.AddSingleton<UnionExplorer>();
        services.AddSingleton<TypeAliasExplorer>();
        services.AddSingleton<VariableExplorer>();

        services.AddSingleton<ExtractAbstractSyntaxTreeCommand>();
        services.AddSingleton<ExtractAbstractSyntaxTreeTool>();
    }
}
