// Copyright (c) Bottlenose Labs Inc. (https://github.com/bottlenoselabs). All rights reserved.
// Licensed under the MIT license. See LICENSE file in the Git repository root directory for full license information.

using CAstFfi.Features.Extract.Domain.Explore.Handlers;
using CAstFfi.Features.Extract.Domain.Parse;
using CAstFfi.Features.Extract.Input;
using Microsoft.Extensions.DependencyInjection;
using ClangArgumentsBuilder = CAstFfi.Features.Extract.Domain.Parse.ClangArgumentsBuilder;
using ClangInstaller = CAstFfi.Features.Extract.Domain.Parse.ClangInstaller;
using Explorer = CAstFfi.Features.Extract.Domain.Explore.Explorer;
using Parser = CAstFfi.Features.Extract.Domain.Parse.Parser;

namespace CAstFfi.Features.Extract;

public static class Startup
{
    public static void ConfigureServices(IServiceCollection services)
    {
        services.AddSingleton<ExtractInputSanitizer>();

        services.AddSingleton<ClangInstaller>();
        services.AddSingleton<ClangArgumentsBuilder>();
        services.AddSingleton<ClangSystemIncludeDirectoriesProvider>();
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
