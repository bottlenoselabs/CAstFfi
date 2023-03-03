// Copyright (c) Bottlenose Labs Inc. (https://github.com/bottlenoselabs). All rights reserved.
// Licensed under the MIT license. See LICENSE file in the Git repository root directory for full license information.

using System;
using Microsoft.Extensions.Hosting;

namespace CAstFfi.Tests.Foundation;

public static class TestHost
{
    private static readonly IHost Host = GetHostBuilder().Build();

    public static IServiceProvider Services => Host.Services;

    private static IHostBuilder GetHostBuilder()
    {
        var result = new HostBuilder()
            .ConfigureHostCommon()
            .ConfigureServices(Startup.ConfigureServices);

        return result;
    }
}
