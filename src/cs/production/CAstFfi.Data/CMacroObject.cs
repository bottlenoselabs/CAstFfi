// Copyright (c) Bottlenose Labs Inc. (https://github.com/bottlenoselabs). All rights reserved.
// Licensed under the MIT license. See LICENSE file in the Git repository root directory for full license information.

using System.Diagnostics.CodeAnalysis;
using System.Text.Json.Serialization;

namespace CAstFfi.Data;

public class CMacroObject : CNodeWithLocation
{
    [JsonPropertyName("type")]
    public CTypeInfo TypeInfo { get; set; } = null!;

    [JsonPropertyName("value")]
    public string Value { get; set; } = string.Empty;

    [ExcludeFromCodeCoverage]
    public override string ToString()
    {
        return $"Macro '{Name}' : {Value} @ {Location}";
    }

    public override bool Equals(CNode? other)
    {
        if (!base.Equals(other) || other is not CMacroObject other2)
        {
            return false;
        }

        return TypeInfo.Equals(other2.TypeInfo) && Value == other2.Value;
    }

    public override int GetHashCode()
    {
        var baseHashCode = base.GetHashCode();

        var hashCode = default(HashCode);
        hashCode.Add(baseHashCode);

        // ReSharper disable NonReadonlyMemberInGetHashCode
        hashCode.Add(TypeInfo);
        hashCode.Add(Value);

        // ReSharper restore NonReadonlyMemberInGetHashCode

        return hashCode.ToHashCode();
    }
}
