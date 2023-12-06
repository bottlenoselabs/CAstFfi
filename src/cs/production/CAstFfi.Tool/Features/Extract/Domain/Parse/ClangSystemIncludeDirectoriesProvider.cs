// Copyright (c) Bottlenose Labs Inc. (https://github.com/bottlenoselabs). All rights reserved.
// Licensed under the MIT license. See LICENSE file in the Git repository root directory for full license information.

using System.Collections.Immutable;
using System.IO.Abstractions;
using bottlenoselabs.Common;
using Microsoft.Extensions.Logging;

namespace CAstFfi.Features.Extract.Domain.Parse;

public sealed partial class ClangSystemIncludeDirectoriesProvider
{
    private readonly ILogger<ClangSystemIncludeDirectoriesProvider> _logger;
    private readonly IFileSystem _fileSystem;

    public ClangSystemIncludeDirectoriesProvider(
        ILogger<ClangSystemIncludeDirectoriesProvider> logger,
        IFileSystem fileSystem)
    {
        _fileSystem = fileSystem;
        _logger = logger;
    }

    public ImmutableArray<string> GetSystemIncludeDirectories(
        TargetPlatform targetPlatform,
        ImmutableArray<string> userProvidedSystemIncludeDirectories,
        bool isEnabledFindSystemHeaders = true)
    {
        ImmutableArray<string> systemIncludeDirectories;
        if (isEnabledFindSystemHeaders)
        {
            systemIncludeDirectories = FindSystemIncludeDirectories(targetPlatform, userProvidedSystemIncludeDirectories);
        }
        else
        {
            systemIncludeDirectories = userProvidedSystemIncludeDirectories;
        }

        var builder = ImmutableArray.CreateBuilder<string>();
        foreach (var directory in systemIncludeDirectories)
        {
            if (_fileSystem.Directory.Exists(directory))
            {
                builder.Add(directory);
            }
            else
            {
                LogMissingSystemIncludeDirectory(directory);
            }
        }

        return builder.ToImmutable();
    }

    private ImmutableArray<string> FindSystemIncludeDirectories(
        TargetPlatform targetPlatform,
        ImmutableArray<string> userProvidedSystemIncludeDirectories)
    {
        var hostOperatingSystem = Native.OperatingSystem;
        var hostArchitecture = Native.Architecture;
        var targetOperatingSystem = targetPlatform.OperatingSystem;
        var directories = ImmutableArray.CreateBuilder<string>();

        switch (hostOperatingSystem)
        {
            case NativeOperatingSystem.Windows:
            {
                FindSystemIncludeDirectoriesHostWindows(targetOperatingSystem, directories);
                break;
            }

            case NativeOperatingSystem.macOS:
            {
                FindSystemIncludeDirectoriesHostMac(targetOperatingSystem, directories);
                break;
            }

            case NativeOperatingSystem.Linux:
            {
                FindSystemIncludeDirectoriesHostLinux(targetPlatform, targetOperatingSystem, hostArchitecture, directories);
                break;
            }
        }

        directories.AddRange(userProvidedSystemIncludeDirectories);

        var result = directories.Distinct().ToImmutableArray();
        return result;
    }

    private void FindSystemIncludeDirectoriesHostWindows(
        NativeOperatingSystem targetSystem,
        ImmutableArray<string>.Builder directories)
    {
        if (targetSystem == NativeOperatingSystem.Windows)
        {
            FindSystemIncludeDirectoriesTargetWindows(directories);
        }
    }

    private void FindSystemIncludeDirectoriesHostMac(
        NativeOperatingSystem targetSystem,
        ImmutableArray<string>.Builder directories)
    {
        if (targetSystem == NativeOperatingSystem.macOS)
        {
            FindSystemIncludesDirectoriesTargetMac(directories);
        }
        else if (targetSystem == NativeOperatingSystem.iOS)
        {
            FindSystemIncludesDirectoriesTargetIPhone(directories);
        }
    }

    private static void FindSystemIncludeDirectoriesHostLinux(
        TargetPlatform targetPlatform,
        NativeOperatingSystem targetSystem,
        NativeArchitecture hostArchitecture,
        ImmutableArray<string>.Builder directories)
    {
        if (targetSystem == NativeOperatingSystem.Linux)
        {
            FindSystemIncludeDirectoriesTargetLinux(hostArchitecture, targetPlatform.Architecture, directories);
        }
    }

    private void FindSystemIncludeDirectoriesTargetWindows(ImmutableArray<string>.Builder directories)
    {
        var sdkDirectoryPath =
            Environment.ExpandEnvironmentVariables(@"%ProgramFiles(x86)%\Windows Kits\10\Include");
        if (!string.IsNullOrEmpty(sdkDirectoryPath) && !_fileSystem.Directory.Exists(sdkDirectoryPath))
        {
            throw new ClangException(
                "Please install the software development kit (SDK) for Windows 10: https://developer.microsoft.com/en-us/windows/downloads/windows-10-sdk/");
        }

        var sdkHighestVersionDirectoryPath = GetHighestVersionDirectoryPathFrom(sdkDirectoryPath);
        if (string.IsNullOrEmpty(sdkHighestVersionDirectoryPath))
        {
            throw new ClangException(
                $"Unable to find a Windows SDK version. Expected a Windows SDK version at '{sdkDirectoryPath}'. Do you need install the a software development kit for Windows? https://developer.microsoft.com/en-us/windows/downloads/windows-10-sdk/");
        }

        var systemIncludeCommandLineArgSdk = $@"{sdkHighestVersionDirectoryPath}\ucrt";
        directories.Add(systemIncludeCommandLineArgSdk);

        var vsWhereFilePath =
            Environment.ExpandEnvironmentVariables(
                @"%ProgramFiles(x86)%\Microsoft Visual Studio\Installer\vswhere.exe");
        var shellOutput = "-latest -property installationPath".ExecuteShellCommand(fileName: vsWhereFilePath);
        var visualStudioInstallationDirectoryPath = shellOutput.Output.Trim();
        if (!_fileSystem.File.Exists(vsWhereFilePath) || string.IsNullOrEmpty(visualStudioInstallationDirectoryPath))
        {
            throw new ClangException(
                "Please install Visual Studio 2017 or later (community, professional, or enterprise).");
        }

        var mscvVersionsDirectoryPath = _fileSystem.Path.Combine(visualStudioInstallationDirectoryPath, @"VC\Tools\MSVC");
        if (!_fileSystem.Directory.Exists(mscvVersionsDirectoryPath))
        {
            throw new ClangException(
                $"Please install the Microsoft Visual C++ (MSVC) build tools for Visual Studio ({visualStudioInstallationDirectoryPath}).");
        }

        var mscvHighestVersionDirectoryPath = GetHighestVersionDirectoryPathFrom(mscvVersionsDirectoryPath);
        if (string.IsNullOrEmpty(mscvHighestVersionDirectoryPath))
        {
            throw new ClangException(
                $"Unable to find a version for Microsoft Visual C++ (MSVC) build tools for Visual Studio ({visualStudioInstallationDirectoryPath}).");
        }

        var mscvIncludeDirectoryPath = _fileSystem.Path.Combine(mscvHighestVersionDirectoryPath, "include");
        if (!_fileSystem.Directory.Exists(mscvIncludeDirectoryPath))
        {
            throw new ClangException(
                $"Please install Microsoft Visual C++ (MSVC) build tools for Visual Studio ({visualStudioInstallationDirectoryPath}).");
        }

        directories.Add(mscvIncludeDirectoryPath);
    }

    private void FindSystemIncludesDirectoriesTargetMac(ImmutableArray<string>.Builder directories)
    {
        var shellOutput = "xcrun --sdk macosx --show-sdk-path".ExecuteShellCommand();
        var sdkPath = shellOutput.Output.Trim();
        if (!_fileSystem.Directory.Exists(sdkPath))
        {
            throw new ClangException(
                "Please install XCode or CommandLineTools for macOS. This will install the software development kit (SDK) for macOS which gives access to common C/C++/ObjC headers.");
        }

        directories.Add($"{sdkPath}/usr/include");
    }

    private void FindSystemIncludesDirectoriesTargetIPhone(ImmutableArray<string>.Builder directories)
    {
        var shellOutput = "xcrun --sdk iphoneos --show-sdk-path".ExecuteShellCommand();
        var sdkPath = shellOutput.Output.Trim();
        if (!_fileSystem.Directory.Exists(sdkPath))
        {
            throw new ClangException(
                "Please install XCode for macOS. This will install the software development kit (SDK) for iOS which gives access to common C/C++/ObjC headers.");
        }

        directories.Add($"{sdkPath}/usr/include");
    }

    private static void FindSystemIncludeDirectoriesTargetLinux(
        NativeArchitecture hostArchitecture,
        NativeArchitecture targetArchitecture,
        ImmutableArray<string>.Builder directories)
    {
        // Cross platform headers are in: /usr/[ARCH]-linux-gnu/include
        //  For Ubuntu, cross platform toolchain (includes headers) are installed via packages:
        //  - gcc-x86-64-linux-gnu (ARCH = x86_64)
        //  - gcc-aarch64-linux-gnu (ARCH = aarch64)
        //  - gcc-i686-linux-gnu (ARCH = i686)
        // Host headers are in /usr/include/[ARCH]-linux-gnu

        if (targetArchitecture == hostArchitecture)
        {
            if (targetArchitecture == NativeArchitecture.X64)
            {
                directories.Add("/usr/include/x86_64-linux-gnu");
            }
            else if (targetArchitecture == NativeArchitecture.ARM64)
            {
                directories.Add("/usr/include/aarch64-linux-gnu");
            }
            else if (targetArchitecture == NativeArchitecture.X86)
            {
                directories.Add("/usr/include/i686-linux-gnu");
            }
        }
        else
        {
            if (targetArchitecture == NativeArchitecture.X64)
            {
                directories.Add("/usr/x86_64-linux-gnu/include");
            }
            else if (targetArchitecture == NativeArchitecture.ARM64)
            {
                directories.Add("/usr/aarch64-linux-gnu/include");
            }
            else if (targetArchitecture == NativeArchitecture.X86)
            {
                directories.Add("/usr/i686-linux-gnu/include");
            }
        }
    }

    private string GetHighestVersionDirectoryPathFrom(string sdkDirectoryPath)
    {
        var versionDirectoryPaths = _fileSystem.Directory.EnumerateDirectories(sdkDirectoryPath);
        var result = string.Empty;
        var highestVersion = Version.Parse("0.0.0");

        foreach (var directoryPath in versionDirectoryPaths)
        {
            var versionStringIndex = directoryPath.LastIndexOf(_fileSystem.Path.DirectorySeparatorChar);
            var versionString = directoryPath[(versionStringIndex + 1)..];
            if (!Version.TryParse(versionString, out var version))
            {
                continue;
            }

            if (version < highestVersion)
            {
                continue;
            }

            highestVersion = version;
            result = directoryPath;
        }

        return result;
    }

    [LoggerMessage(0, LogLevel.Warning, "- Could not find system include directory: {DirectoryPath}")]
    private partial void LogMissingSystemIncludeDirectory(string directoryPath);
}
