// Copyright (c) Bottlenose Labs Inc. (https://github.com/bottlenoselabs). All rights reserved.
// Licensed under the MIT license. See LICENSE file in the Git repository root directory for full license information.

using System.Diagnostics.CodeAnalysis;
using System.Text.Json.Serialization;

namespace CAstFfi.Data;

// NOTE: Properties are required for System.Text.Json serialization
public class CTypeAlias : CNodeWithLocation
{
    [JsonPropertyName("underlyingType")]
    public CTypeInfo UnderlyingTypeInfo { get; set; } = null!;

    [ExcludeFromCodeCoverage]
    public override string ToString()
    {
        return $"Typedef '{Name}': {UnderlyingTypeInfo} @ {Location}";
    }

    public override bool Equals(CNode? other)
    {
        if (!base.Equals(other) || other is not CTypeAlias other2)
        {
            return false;
        }

        return UnderlyingTypeInfo.Equals(other2.UnderlyingTypeInfo);
    }

    public override int GetHashCode()
    {
        var baseHashCode = base.GetHashCode();

        var hashCode = default(HashCode);
        hashCode.Add(baseHashCode);

        // ReSharper disable NonReadonlyMemberInGetHashCode
        hashCode.Add(UnderlyingTypeInfo);

        // ReSharper restore NonReadonlyMemberInGetHashCode

        return hashCode.ToHashCode();
    }
}
