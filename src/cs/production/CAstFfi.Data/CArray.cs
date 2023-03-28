// Copyright (c) Bottlenose Labs Inc. (https://github.com/bottlenoselabs). All rights reserved.
// Licensed under the MIT license. See LICENSE file in the Git repository root directory for full license information.

using System.Text.Json.Serialization;

namespace CAstFfi.Data;

public class CArray : CNode
{
    [JsonPropertyName("type")]
    public CTypeInfo TypeInfo { get; set; } = null!;

    public override bool Equals(CNode? other)
    {
        if (!base.Equals(other) || other is not CArray other2)
        {
            return false;
        }

        return TypeInfo.Equals(other2.TypeInfo);
    }

    public override int GetHashCode()
    {
        var baseHashCode = base.GetHashCode();

        var hashCode = default(HashCode);
        hashCode.Add(baseHashCode);

        // ReSharper disable NonReadonlyMemberInGetHashCode
        hashCode.Add(TypeInfo);

        // ReSharper restore NonReadonlyMemberInGetHashCode

        return hashCode.ToHashCode();
    }
}
