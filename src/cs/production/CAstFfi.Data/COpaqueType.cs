// Copyright (c) Bottlenose Labs Inc. (https://github.com/bottlenoselabs). All rights reserved.
// Licensed under the MIT license. See LICENSE file in the Git repository root directory for full license information.

using System.Diagnostics.CodeAnalysis;
using System.Text.Json.Serialization;

namespace CAstFfi.Data;

public class COpaqueType : CNodeWithLocation
{
    [JsonPropertyName("sizeOf")]
    public int SizeOf { get; set; }

    [ExcludeFromCodeCoverage]
    public override string ToString()
    {
        return $"OpaqueType '{Name}' @ {Location}";
    }

    public override bool Equals(CNode? other)
    {
        if (!base.Equals(other) || other is not COpaqueType other2)
        {
            return false;
        }

        return SizeOf == other2.SizeOf;
    }

    public override int GetHashCode()
    {
        var baseHashCode = base.GetHashCode();

        var hashCode = default(HashCode);
        hashCode.Add(baseHashCode);

        // ReSharper disable NonReadonlyMemberInGetHashCode
        hashCode.Add(SizeOf);

        // ReSharper restore NonReadonlyMemberInGetHashCode

        return hashCode.ToHashCode();
    }
}
