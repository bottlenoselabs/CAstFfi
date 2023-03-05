// Copyright (c) Bottlenose Labs Inc. (https://github.com/bottlenoselabs). All rights reserved.
// Licensed under the MIT license. See LICENSE file in the Git repository root directory for full license information.

using System.Diagnostics.CodeAnalysis;
using System.Text.Json.Serialization;
using CAstFfi.Data.Serialization;

namespace CAstFfi.Data;

// NOTE: Properties are required for System.Text.Json serialization
public class CTypeInfo
{
    [JsonPropertyName("name")]
    public string Name { get; set; } = string.Empty;

    [JsonPropertyName("kind")]
    public CKind Kind { get; set; } = CKind.Unknown;

    [JsonPropertyName("sizeOf")]
    public int SizeOf { get; set; }

    [JsonPropertyName("alignOf")]
    public int? AlignOf { get; set; }

    [JsonPropertyName("sizeOfElement")]
    public int? ElementSize { get; set; }

    [JsonPropertyName("arraySize")]
    public int? ArraySizeOf { get; set; }

    [JsonPropertyName("isAnonymous")]
    public bool? IsAnonymous { get; set; }

    [JsonPropertyName("isConst")]
    public bool IsConst { get; set; }

    [JsonPropertyName("location")]
    [JsonConverter(typeof(CLocationJsonConverter))]
    public CLocation Location { get; set; }

    [JsonPropertyName("innerType")]
    public CTypeInfo? InnerTypeInfo { get; set; }

    [ExcludeFromCodeCoverage]
    public override string ToString()
    {
        return Name;
    }

#pragma warning disable CA2211
    private static readonly CTypeInfo Void = new()
    {
        Name = "void",
        Kind = CKind.Primitive,
        SizeOf = 0,
        AlignOf = null,
        ArraySizeOf = null,
        Location = CLocation.NoLocation,
        IsAnonymous = null
    };

    public static CTypeInfo VoidPointer(int pointerSize)
    {
        return new CTypeInfo
        {
            Name = "void*",
            Kind = CKind.Pointer,
            SizeOf = pointerSize,
            AlignOf = pointerSize,
            ElementSize = null,
            ArraySizeOf = null,
            Location = CLocation.NoLocation,
            IsAnonymous = null,
            InnerTypeInfo = Void
        };
    }
#pragma warning restore CA2211
}
