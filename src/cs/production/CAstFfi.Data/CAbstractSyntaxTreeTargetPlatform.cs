// Copyright (c) Bottlenose Labs Inc. (https://github.com/bottlenoselabs). All rights reserved.
// Licensed under the MIT license. See LICENSE file in the Git repository root directory for full license information.

using System.Collections.Immutable;
using System.Text.Json.Serialization;
using bottlenoselabs.Common;

namespace CAstFfi.Data;

// NOTE: Properties are required for System.Text.Json serialization
public record CAbstractSyntaxTreeTargetPlatform
{
    [JsonPropertyName("fileName")]
    public string FileName { get; set; } = string.Empty;

    [JsonPropertyName("platformRequested")]
    public TargetPlatform PlatformRequested { get; set; } = TargetPlatform.Unknown;

    [JsonPropertyName("platformActual")]
    public TargetPlatform PlatformActual { get; set; } = TargetPlatform.Unknown;

    [JsonPropertyName("macroObjects")]
    public ImmutableDictionary<string, CMacroObject> MacroObjects { get; set; } =
        ImmutableDictionary<string, CMacroObject>.Empty;

    [JsonPropertyName("variables")]
    public ImmutableDictionary<string, CVariable> Variables { get; set; } =
        ImmutableDictionary<string, CVariable>.Empty;

    [JsonPropertyName("functions")]
    public ImmutableDictionary<string, CFunction> Functions { get; set; } =
        ImmutableDictionary<string, CFunction>.Empty;

    [JsonPropertyName("records")]
    public ImmutableDictionary<string, CRecord> Records { get; set; } = ImmutableDictionary<string, CRecord>.Empty;

    [JsonPropertyName("enums")]
    public ImmutableDictionary<string, CEnum> Enums { get; set; } = ImmutableDictionary<string, CEnum>.Empty;

    [JsonPropertyName("typeAliases")]
    public ImmutableDictionary<string, CTypeAlias> TypeAliases { get; set; } =
        ImmutableDictionary<string, CTypeAlias>.Empty;

    [JsonPropertyName("opaqueTypes")]
    public ImmutableDictionary<string, COpaqueType> OpaqueTypes { get; set; } =
        ImmutableDictionary<string, COpaqueType>.Empty;

    [JsonPropertyName("functionPointers")]
    public ImmutableDictionary<string, CFunctionPointer> FunctionPointers { get; set; } =
        ImmutableDictionary<string, CFunctionPointer>.Empty;

    [JsonPropertyName("enumConstants")]
    public ImmutableDictionary<string, CEnumConstant> EnumConstants { get; set; } =
        ImmutableDictionary<string, CEnumConstant>.Empty;
}
