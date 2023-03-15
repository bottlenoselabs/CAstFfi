// Copyright (c) Bottlenose Labs Inc. (https://github.com/bottlenoselabs). All rights reserved.
// Licensed under the MIT license. See LICENSE file in the Git repository root directory for full license information.

using System.Collections.Immutable;
using System.CommandLine;
using System.IO.Abstractions;
using System.Reflection;
using System.Resources;
using CAstFfi.Common;
using CAstFfi.Extract.Input;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace CAstFfi;

public static class Startup
{
    public static IHost CreateHost(string[] args)
    {
        return new HostBuilder()
            .ConfigureDefaults(args)
            .UseConsoleLifetime()
            .ConfigureHostCommon()
            .Build();
    }

    public static IHostBuilder ConfigureHostCommon(this IHostBuilder builder)
    {
        return builder
            .ConfigureAppConfiguration(ConfigureAppConfiguration)
            .ConfigureLogging(ConfigureLogging)
            .ConfigureServices(ConfigureServices);
    }

    private static void ConfigureAppConfiguration(IConfigurationBuilder builder)
    {
        AddDefaultAppConfiguration(builder);
    }

    private static void AddDefaultAppConfiguration(IConfigurationBuilder builder)
    {
        var sources = builder.Sources.ToImmutableArray();
        builder.Sources.Clear();

        var filePath = Path.Combine(AppContext.BaseDirectory, "appsettings.json");
        if (File.Exists(filePath))
        {
            builder.AddJsonFile(filePath);
        }
        else
        {
            var appSettingsResourceName = Assembly.GetExecutingAssembly().GetManifestResourceNames()
                .FirstOrDefault(x => x.EndsWith("appsettings.json", StringComparison.InvariantCulture));
            if (string.IsNullOrEmpty(appSettingsResourceName))
            {
                throw new MissingManifestResourceException("Missing appsettings.json embedded resource.");
            }

            var jsonStream = Assembly.GetExecutingAssembly().GetManifestResourceStream(appSettingsResourceName)!;
            builder.AddJsonStream(jsonStream);
        }

        foreach (var originalSource in sources)
        {
            builder.Add(originalSource);
        }
    }

    private static void ConfigureLogging(HostBuilderContext context, ILoggingBuilder builder)
    {
        builder.ClearProviders();
        builder.AddSimpleConsole();
        builder.AddConfiguration(context.Configuration.GetSection("Logging"));
    }

    private static void ConfigureServices(IServiceCollection services)
    {
        services.AddSingleton<IFileSystem, FileSystem>();
        services.AddHostedService<CommandLineHost>();
        services.AddSingleton<RootCommand, CommandLineInterface>();
        services.AddSingleton<ProgramInputSanitizer>();

        Extract.Startup.ConfigureServices(services);
        Merge.Startup.ConfigureServices(services);
    }
}
