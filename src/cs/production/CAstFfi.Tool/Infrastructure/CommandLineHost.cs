// Copyright (c) Bottlenose Labs Inc. (https://github.com/bottlenoselabs). All rights reserved.
// Licensed under the MIT license. See LICENSE file in the Git repository root directory for full license information.

using System.CommandLine;
using Microsoft.Extensions.Hosting;

namespace CAstFfi.Infrastructure;

public sealed class CommandLineHost : IHostedService
{
    private readonly IHostApplicationLifetime _applicationLifetime;
    private readonly RootCommand _rootCommand;

    public CommandLineHost(
        IHostApplicationLifetime applicationLifetime,
        RootCommand rootCommand)
    {
        _applicationLifetime = applicationLifetime;
        _rootCommand = rootCommand;
    }

    public Task StartAsync(CancellationToken cancellationToken)
    {
        _applicationLifetime.ApplicationStarted.Register(() => Task.Run(Main, cancellationToken));
        return Task.CompletedTask;
    }

    public Task StopAsync(CancellationToken cancellationToken)
    {
        return Task.CompletedTask;
    }

    private void Main()
    {
        var commandLineArguments = Environment.GetCommandLineArgs();
        Environment.ExitCode = _rootCommand.Invoke(commandLineArguments);
        _applicationLifetime.StopApplication();
    }
}
