// Copyright (c) Bottlenose Labs Inc. (https://github.com/bottlenoselabs). All rights reserved.
// Licensed under the MIT license. See LICENSE file in the Git repository root directory for full license information.

using CAstFfi.Tests.Foundation.CMake;
using Microsoft.Extensions.DependencyInjection;

namespace CAstFfi.Tests.Foundation;

public static class Startup
{
    public static void ConfigureServices(IServiceCollection services)
    {
        services.AddSingleton<CMakeLibraryBuilder>();
        services.AddSingleton<TestCCode>();
        services.AddSingleton<TestFixtureCCode>();
    }
}
