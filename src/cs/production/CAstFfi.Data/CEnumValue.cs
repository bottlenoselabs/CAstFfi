// Copyright (c) Bottlenose Labs Inc. (https://github.com/bottlenoselabs). All rights reserved.
// Licensed under the MIT license. See LICENSE file in the Git repository root directory for full license information.

using System.Diagnostics.CodeAnalysis;
using System.Text.Json.Serialization;

namespace CAstFfi.Data;

// NOTE: Properties are required for System.Text.Json serialization
public class CEnumValue : CNode
{
    [JsonPropertyName("name")]
    public new string Name
    {
        get => base.Name;
        set => base.Name = value;
    }

    [JsonPropertyName("value")]
    public long Value { get; set; }

    [ExcludeFromCodeCoverage]
    public override string ToString()
    {
        return $"EnumValue '{Name}' = {Value}";
    }

    public override bool Equals(CNode? other)
    {
        if (!base.Equals(other) || other is not CEnumValue other2)
        {
            return false;
        }

        return Value == other2.Value;
    }

    public override int GetHashCode()
    {
        var baseHashCode = base.GetHashCode();

        var hashCode = default(HashCode);
        hashCode.Add(baseHashCode);

        // ReSharper disable NonReadonlyMemberInGetHashCode
        hashCode.Add(Value);

        // ReSharper restore NonReadonlyMemberInGetHashCode

        return hashCode.ToHashCode();
    }
}
