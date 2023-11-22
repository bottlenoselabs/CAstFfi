// Copyright (c) Bottlenose Labs Inc. (https://github.com/bottlenoselabs). All rights reserved.
// Licensed under the MIT license. See LICENSE file in the Git repository root directory for full license information.

using System.Collections.Immutable;
using System.Text.Json.Serialization;
using JetBrains.Annotations;

namespace CAstFfi.Features.Extract.Input.Unsanitized;

// NOTE: This class is considered un-sanitized input; all strings and other types could be null.
[PublicAPI]
public sealed class UnsanitizedExtractOptions
{
    /// <summary>
    ///     The path of the output abstract syntax tree directory.
    /// </summary>
    /// <remarks>
    ///     <para>
    ///         The directory will contain one or more generated abstract syntax tree `.json` files which each have a
    ///         file name of the target platform.
    ///     </para>
    /// </remarks>
    [JsonPropertyName("outputDirectory")]
    public string? OutputDirectory { get; set; } = "./ast";

    /// <summary>
    ///     The path of the input `.h` header file containing C code.
    /// </summary>
    [JsonPropertyName("inputFilePath")]
    public string? InputFilePath { get; set; }

    /// <summary>
    ///     The directories to search for non-system header files.
    /// </summary>
    [JsonPropertyName("userIncludeDirectories")]
    public ImmutableArray<string>? UserIncludeDirectories { get; set; }

    /// <summary>
    ///     The directories to search for system header files.
    /// </summary>
    [JsonPropertyName("systemIncludeDirectories")]
    public ImmutableArray<string>? SystemIncludeDirectories { get; set; }

    /// <summary>
    ///     Determines whether to show the the path of header code locations with full paths or relative paths.
    /// </summary>
    /// <remarks>
    ///     <para>
    ///         Default is <c>false</c>. Use <c>true</c> to use the full path for header locations. Use <c>false</c> to
    ///         show only relative file paths.
    ///     </para>
    /// </remarks>
    [JsonPropertyName("isEnabledLocationFullPaths")]
    public bool? IsEnabledLocationFullPaths { get; set; }

    /// <summary>
    ///     Determines whether to include or exclude declarations (functions, enums, structs, typedefs, etc) with a
    ///     prefixed underscore that are assumed to be 'non public'.
    /// </summary>
    /// <remarks>
    ///     <para>
    ///         Default is <c>false</c>. Use <c>true</c> to include declarations with a prefixed underscore. Use
    ///         <c>false</c> to exclude declarations with a prefixed underscore.
    ///     </para>
    /// </remarks>
    [JsonPropertyName("isEnabledAllowNamesWithPrefixedUnderscore")]
    public bool? IsEnabledAllowNamesWithPrefixedUnderscore { get; set; }

    /// <summary>
    ///     Determines whether to include or exclude system declarations (functions, enums, typedefs, records, etc).
    /// </summary>
    /// <remarks>
    ///     <para>
    ///         Default is `false`. Use <c>true</c> to include system declarations. Use `false` to exclude system
    ///         declarations.
    ///     </para>
    /// </remarks>
    [JsonPropertyName("isEnabledSystemDeclarations")]
    public bool? IsEnabledSystemDeclarations { get; set; }

    /// <summary>
    ///     Determines whether to automatically find and append the system headers for the target platform.
    /// </summary>
    /// <remarks>
    ///     <para>
    ///         Default is <c>true</c>. Use <c>true</c> to automatically find and append system headers for the target
    ///         platform. Use <c>false</c> to skip.
    ///     </para>
    /// </remarks>
    [JsonPropertyName("isEnabledAutomaticallyFindSystemHeaders")]
    public bool? IsEnabledAutomaticallyFindSystemHeaders { get; set; }

    /// <summary>
    ///     Determines whether to parse the main input header file and all transitive inclusive header files as if it
    ///     were a single translation unit.
    /// </summary>
    /// <remarks>
    ///     <para>
    ///         Default is <c>true</c>. Use <c>true</c> to parse the the main input header file and all transitive
    ///         inclusive headers as if it were a single translation unit. Use <c>false</c> to parse each translation
    ///         unit independently.
    ///     </para>
    /// </remarks>
    [JsonPropertyName("isEnabledParseAsSingleHeader")]
    public bool? IsEnabledParseAsSingleHeader { get; set; }

    /// <summary>
    ///     Determines whether to parse only the top-level cursors which are externally visible, or all top-level cursors.
    /// </summary>
    /// <para>
    ///     Default is <c>true</c>. Use <c>true</c> to parse only top-level cursors which are externally visible. Use
    ///     <c>false</c> to parse all top-level cursors whether or not they are externally visible.
    /// </para>
    [JsonPropertyName("isEnabledOnlyExternalTopLevelCursors")]
    public bool? IsEnabledOnlyExternalTopLevelCursors { get; set; }

    /// <summary>
    ///     The cursor names to be treated as opaque types.
    /// </summary>
    [JsonPropertyName("opaqueTypeNames")]
    public ImmutableArray<string>? OpaqueTypeNames { get; set; }

    /// <summary>
    ///     The target platform configurations for extracting the abstract syntax trees per desktop host operating system.
    /// </summary>
    [JsonPropertyName("targetPlatforms")]
    public ImmutableDictionary<string, ImmutableDictionary<string, UnsanitizedExtractOptionsTargetPlatform>>? TargetPlatforms { get; set; }

    /// <summary>
    ///     The names of libraries and/or interfaces for macOS, iOS, tvOS or watchOS.
    /// </summary>
    [JsonPropertyName("appleFrameworks")]
    public ImmutableArray<string>? AppleFrameworks { get; set; }
}
