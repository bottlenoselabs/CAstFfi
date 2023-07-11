// Copyright (c) Bottlenose Labs Inc. (https://github.com/bottlenoselabs). All rights reserved.
// Licensed under the MIT license. See LICENSE file in the Git repository root directory for full license information.

using System.Text.Json.Serialization;

namespace CAstFfi.Data;

// NOTE: Properties are required for System.Text.Json serialization
public abstract class CNode : IComparable<CNode>, IEquatable<CNode>
{
    [JsonPropertyName("comment")]
    public string? Comment { get; set; }

    [JsonPropertyName("isSystem")]
    public bool IsSystem { get; set; }

    [JsonIgnore]
    public string Name { get; set; } = string.Empty;

    [JsonIgnore]
    public CKind Kind => GetKind(this);

    public int CompareTo(CNode? other)
    {
        if (ReferenceEquals(this, other))
        {
            return 0;
        }

        if (ReferenceEquals(null, other))
        {
            return 1;
        }

        var result = CompareToInternal(other);
        return result;
    }

    protected virtual int CompareToInternal(CNode? other)
    {
        if (other == null)
        {
            return 0;
        }

        return string.Compare(Name, other.Name, StringComparison.Ordinal);
    }

    private CKind GetKind(CNode node)
    {
        return this switch
        {
            CEnum => CKind.Enum,
            CEnumValue => CKind.EnumValue,
            CFunction => CKind.Function,
            CFunctionParameter => CKind.FunctionParameter,
            CFunctionPointer => CKind.FunctionPointer,
            CFunctionPointerParameter => CKind.FunctionPointerParameter,
            COpaqueType => CKind.OpaqueType,
            CRecord => ((CRecord)node).RecordKind == CRecordKind.Struct ? CKind.Struct : CKind.Union,
            CTypeAlias => CKind.TypeAlias,
            CVariable => CKind.Variable,
            CMacroObject => CKind.MacroObject,
            CRecordField => CKind.RecordField,
            CPrimitive => CKind.Primitive,
            CPointer => CKind.Pointer,
            CArray => CKind.Array,
            CEnumConstant => CKind.EnumConstant,
            _ => throw new NotImplementedException($"The kind of mapping for '{GetType()}' is not implemented.")
        };
    }

    public static bool operator <(CNode first, CNode second)
    {
        return first.CompareTo(second) < 0;
    }

    public static bool operator >(CNode first, CNode second)
    {
        return first.CompareTo(second) > 0;
    }

    public static bool operator >=(CNode first, CNode second)
    {
        return first.CompareTo(second) >= 0;
    }

    public static bool operator <=(CNode first, CNode second)
    {
        return first.CompareTo(second) <= 0;
    }

    public virtual bool Equals(CNode? other)
    {
        if (ReferenceEquals(null, other))
        {
            return false;
        }

        if (ReferenceEquals(this, other))
        {
            return true;
        }

        return Name == other.Name;
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

        return Equals((CNode)obj);
    }

    public override int GetHashCode()
    {
        return HashCode.Combine(Name, Comment);
    }

    public static bool operator ==(CNode? left, CNode? right)
    {
        if (left is null)
        {
            return right is null;
        }

        return left.Equals(right);
    }

    public static bool operator !=(CNode? left, CNode? right)
    {
        return !(left == right);
    }
}
