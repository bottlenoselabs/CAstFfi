// Copyright (c) Bottlenose Labs Inc. (https://github.com/bottlenoselabs). All rights reserved.
// Licensed under the MIT license. See LICENSE file in the Git repository root directory for full license information.

using System.Diagnostics.CodeAnalysis;
using System.Text.Json.Serialization;

namespace CAstFfi.IO.Output;

// NOTE: Properties are required for System.Text.Json serialization
public record CFunctionPointerParameter : CNode
{
    [JsonPropertyName("name")]
    public new string Name
    {
        get => base.Name;
        set => base.Name = value;
    }

    [JsonPropertyName("type")]
    public CTypeInfo TypeInfo { get; set; } = null!;

    [ExcludeFromCodeCoverage]
    public override string ToString()
    {
        return $"FunctionPointerParameter '{Name}': {TypeInfo}";
    }
}
