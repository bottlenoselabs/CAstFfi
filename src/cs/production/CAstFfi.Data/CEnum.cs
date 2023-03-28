// Copyright (c) Bottlenose Labs Inc. (https://github.com/bottlenoselabs). All rights reserved.
// Licensed under the MIT license. See LICENSE file in the Git repository root directory for full license information.

using System.Collections.Immutable;
using System.Diagnostics.CodeAnalysis;
using System.Text.Json.Serialization;

namespace CAstFfi.Data;

// NOTE: Properties are required for System.Text.Json serialization
public class CEnum : CNodeWithLocation
{
    [JsonPropertyName("typeInteger")]
    public CTypeInfo IntegerTypeInfo { get; set; } = null!;

    [JsonPropertyName("values")]
    public ImmutableArray<CEnumValue> Values { get; set; } = ImmutableArray<CEnumValue>.Empty;

    [ExcludeFromCodeCoverage]
    public override string ToString()
    {
        return $"Enum '{Name}': {IntegerTypeInfo} @ {Location}";
    }

    public override bool Equals(CNode? other)
    {
        if (!base.Equals(other) || other is not CEnum other2)
        {
            return false;
        }

        return IntegerTypeInfo.Equals(other2.IntegerTypeInfo) && Values.SequenceEqual(other2.Values);
    }

    public override int GetHashCode()
    {
        var baseHashCode = base.GetHashCode();

        var hashCode = default(HashCode);
        hashCode.Add(baseHashCode);

        // ReSharper disable NonReadonlyMemberInGetHashCode
        hashCode.Add(IntegerTypeInfo);

        foreach (var value in Values)
        {
            hashCode.Add(value);
        }

        // ReSharper restore NonReadonlyMemberInGetHashCode

        return hashCode.ToHashCode();
    }
}
