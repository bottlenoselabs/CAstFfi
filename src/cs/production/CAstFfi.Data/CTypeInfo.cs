// Copyright (c) Bottlenose Labs Inc. (https://github.com/bottlenoselabs). All rights reserved.
// Licensed under the MIT license. See LICENSE file in the Git repository root directory for full license information.

using System.Diagnostics.CodeAnalysis;
using System.Text.Json.Serialization;
using CAstFfi.Data.Serialization;

namespace CAstFfi.Data;

// NOTE: Properties are required for System.Text.Json serialization
public class CTypeInfo : IEquatable<CTypeInfo>
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
    public CLocation? Location { get; set; }

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
        Location = null,
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
            Location = null,
            IsAnonymous = null,
            InnerTypeInfo = Void
        };
    }
#pragma warning restore CA2211

    public bool Equals(CTypeInfo? other)
    {
        if (other is null)
        {
            return false;
        }

        if (ReferenceEquals(this, other))
        {
            return true;
        }

        return Kind == other.Kind &&
               SizeOf == other.SizeOf &&
               AlignOf == other.AlignOf &&
               ElementSize == other.ElementSize &&
               ArraySizeOf == other.ArraySizeOf &&
               IsAnonymous == other.IsAnonymous &&
               IsConst == other.IsConst;
    }

    public override bool Equals(object? obj)
    {
        if (ReferenceEquals(null, obj))
        {
            return false;
        }

        if (ReferenceEquals(this, obj))
        {
            return true;
        }

        if (obj.GetType() != GetType())
        {
            return false;
        }

        return Equals((CTypeInfo)obj);
    }

    public override int GetHashCode()
    {
        var hashCode = default(HashCode);
        // ReSharper disable NonReadonlyMemberInGetHashCode
        hashCode.Add(Name);
        hashCode.Add((int)Kind);
        hashCode.Add(SizeOf);
        hashCode.Add(AlignOf);
        hashCode.Add(ElementSize);
        hashCode.Add(ArraySizeOf);
        hashCode.Add(IsAnonymous);
        hashCode.Add(IsConst);
        hashCode.Add(Location);
        hashCode.Add(InnerTypeInfo);
        return hashCode.ToHashCode();
        // ReSharper restore NonReadonlyMemberInGetHashCode
    }
}
