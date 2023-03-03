// Copyright (c) Bottlenose Labs Inc. (https://github.com/bottlenoselabs). All rights reserved.
// Licensed under the MIT license. See LICENSE file in the Git repository root directory for full license information.

using System.Collections.Immutable;
using System.Text.Json.Serialization;
using JetBrains.Annotations;

namespace CAstFfi.IO.Input.Unsanitized;

// NOTE: This class is considered un-sanitized input; all strings and other types could be null.
[PublicAPI]
public sealed class UnsanitizedTargetPlatformOptions
{
    /// <summary>
    ///     The directories to search for non-system header files specific to the target platform.
    /// </summary>
    [JsonPropertyName("userIncludeDirectories")]
    public ImmutableArray<string>? UserIncludeDirectories { get; set; }

    /// <summary>
    ///     The directories to search for system header files of the target platform.
    /// </summary>
    [JsonPropertyName("systemIncludeDirectories")]
    public ImmutableArray<string>? SystemIncludeDirectories { get; set; }

    /// <summary>
    ///     The object-like macros to use when parsing C code.
    /// </summary>
    [JsonPropertyName("defines")]
    public ImmutableArray<string>? Defines { get; set; }

    /// <summary>
    ///     The additional Clang arguments to use when parsing C code.
    /// </summary>
    [JsonPropertyName("clangArguments")]
    public ImmutableArray<string>? ClangArguments { get; set; }

    /// <summary>
    ///     The names of libraries and/or interfaces for macOS, iOS, tvOS or watchOS.
    /// </summary>
    [JsonPropertyName("appleFrameworks")]
    public ImmutableArray<string>? AppleFrameworks { get; set; }
}
