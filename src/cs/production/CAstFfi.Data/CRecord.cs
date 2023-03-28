// Copyright (c) Bottlenose Labs Inc. (https://github.com/bottlenoselabs). All rights reserved.
// Licensed under the MIT license. See LICENSE file in the Git repository root directory for full license information.

using System.Collections.Immutable;
using System.Diagnostics.CodeAnalysis;
using System.Text.Json.Serialization;

namespace CAstFfi.Data;

// NOTE: Properties are required for System.Text.Json serialization
public class CRecord : CNodeWithLocation
{
    [JsonPropertyName("record_kind")]
    public CRecordKind RecordKind { get; set; }

    [JsonPropertyName("size_of")]
    public int SizeOf { get; set; }

    [JsonPropertyName("align_of")]
    public int AlignOf { get; set; }

    [JsonPropertyName("fields")]
    public ImmutableArray<CRecordField> Fields { get; set; } = ImmutableArray<CRecordField>.Empty;

    [ExcludeFromCodeCoverage]
    public override string ToString()
    {
        return $"{RecordKind} {Name} @ {Location}";
    }

    public override bool Equals(CNode? other)
    {
        if (!base.Equals(other) || other is not CRecord other2)
        {
            return false;
        }

        return RecordKind == other2.RecordKind &&
               SizeOf == other2.SizeOf &&
               AlignOf == other2.AlignOf &&
               Fields.SequenceEqual(other2.Fields);
    }

    public override int GetHashCode()
    {
        var baseHashCode = base.GetHashCode();

        var hashCode = default(HashCode);
        hashCode.Add(baseHashCode);

        // ReSharper disable NonReadonlyMemberInGetHashCode
        hashCode.Add(RecordKind);
        hashCode.Add(SizeOf);
        hashCode.Add(AlignOf);

        foreach (var field in Fields)
        {
            hashCode.Add(field);
        }

        // ReSharper restore NonReadonlyMemberInGetHashCode

        return hashCode.ToHashCode();
    }
}
