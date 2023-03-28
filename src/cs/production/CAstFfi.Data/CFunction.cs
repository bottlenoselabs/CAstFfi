// Copyright (c) Bottlenose Labs Inc. (https://github.com/bottlenoselabs). All rights reserved.
// Licensed under the MIT license. See LICENSE file in the Git repository root directory for full license information.

using System.Collections.Immutable;
using System.Diagnostics.CodeAnalysis;
using System.Text.Json.Serialization;

namespace CAstFfi.Data;

// NOTE: Properties are required for System.Text.Json serialization
public class CFunction : CNodeWithLocation
{
    [JsonPropertyName("callingConvention")]
    public CFunctionCallingConvention CallingConvention { get; set; } = CFunctionCallingConvention.Cdecl;

    [JsonPropertyName("returnType")]
    public CTypeInfo ReturnTypeInfo { get; set; } = null!;

    [JsonPropertyName("parameters")]
    public ImmutableArray<CFunctionParameter> Parameters { get; set; } = ImmutableArray<CFunctionParameter>.Empty;

    [ExcludeFromCodeCoverage]
    public override string ToString()
    {
        return $"FunctionExtern '{Name}' @ {Location}";
    }

    public override bool Equals(CNode? other)
    {
        if (!base.Equals(other) || other is not CFunction other2)
        {
            return false;
        }

        return CallingConvention == other2.CallingConvention &&
               ReturnTypeInfo.Equals(other2.ReturnTypeInfo) &&
               Parameters.SequenceEqual(other2.Parameters);
    }

    public override int GetHashCode()
    {
        var baseHashCode = base.GetHashCode();

        var hashCode = default(HashCode);
        hashCode.Add(baseHashCode);

        // ReSharper disable NonReadonlyMemberInGetHashCode
        hashCode.Add(CallingConvention);
        hashCode.Add(ReturnTypeInfo);

        foreach (var parameter in Parameters)
        {
            hashCode.Add(parameter);
        }

        // ReSharper restore NonReadonlyMemberInGetHashCode

        return hashCode.ToHashCode();
    }
}
