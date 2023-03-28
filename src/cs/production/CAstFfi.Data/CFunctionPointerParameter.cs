// Copyright (c) Bottlenose Labs Inc. (https://github.com/bottlenoselabs). All rights reserved.
// Licensed under the MIT license. See LICENSE file in the Git repository root directory for full license information.

using System.Diagnostics.CodeAnalysis;
using System.Text.Json.Serialization;

namespace CAstFfi.Data;

// NOTE: Properties are required for System.Text.Json serialization
public class CFunctionPointerParameter : CNode
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

    public override bool Equals(CNode? other)
    {
        if (!base.Equals(other) || other is not CFunctionPointerParameter other2)
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
