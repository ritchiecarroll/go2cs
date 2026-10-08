// GoMemberRecordAttribute.cs - Gbtc
// Copyright © 2026 The go2cs Authors. All rights reserved.
//
// Use of this source code is governed by an MIT-style license
// that can be found in the LICENSE file.

using System;

namespace go;

/// <summary>
/// The Go facts a member record can carry.
/// </summary>
public enum GoMemberFact : byte
{
    /// <summary>The named field is a Go EMBEDDED field (what <see cref="GoEmbeddedAttribute"/> says).</summary>
    Embedded = 1,

    /// <summary>
    /// The named field, or the partial property that declares an embedded field, carries the Go struct tag in
    /// <see cref="GoMemberRecordAttribute.Value"/> (what <c>[GoTag]</c> says).
    /// </summary>
    Tag = 2,

    /// <summary>
    /// The named field carries Go array dims in <see cref="GoMemberRecordAttribute.Dims"/>: what
    /// <see cref="GoArrayDimsAttribute"/> says on a field (the dims its pointer or map hop hands down).
    /// </summary>
    Dims = 3,

    /// <summary>
    /// The named field's Go type is a defined type over an interface that the emission erased to a <c>using</c>
    /// alias, and <see cref="GoMemberRecordAttribute.Carrier"/> is its DESCRIPTOR CARRIER: what
    /// <see cref="GoDescriptorTypeAttribute"/>'s <c>Self</c> says on a field.
    /// </summary>
    Descriptor = 4,

    /// <summary>
    /// The named map field's KEY carries Go array dims in <see cref="GoMemberRecordAttribute.Dims"/>: what
    /// <see cref="GoMapKeyDimsAttribute"/> says on a field (the dims <c>reflect.Type.Key()</c> hands down).
    /// </summary>
    KeyDims = 5
}

/// <summary>
/// Carries a Go fact about one MEMBER of the decorated type, which converted code states in a comment
/// beside the member rather than in an attribute on it (docs/PLAN-marker-comment-parity.md, sections 5.4-5.6).
/// </summary>
/// <remarks>
/// <para>
/// go2cs-gen's <c>MemberRecordGenerator</c> reads the comment and emits this on a GENERATED partial of the
/// member's declaring type, since a generated file can add an attribute to a type but not to a field, so
/// converted code reads as the comment alone. The member is named by its C# name.
/// </para>
/// <para>
/// A <see cref="GoMemberFact.Descriptor"/> fact has no comment: the field is already spelled with the Go
/// type's name, and only the converter knows the carrier that name stands for. The converter writes that
/// record on the type's accessibility declaration in the package's metadata file (<c>package_info.cs</c>'s
/// <c>TypeAccessibility</c> section), out of the converted code (docs/PLAN-marker-comment-parity.md,
/// section 11).
/// </para>
/// <para>
/// golib reads the member attribute OR this record: hand-written files keep the attribute. A record whose
/// member is missing, or cannot hold the fact, is refused BY NAME when the type's records are first read,
/// never applied to a member it does not describe.
/// </para>
/// </remarks>
[AttributeUsage(AttributeTargets.Class | AttributeTargets.Struct | AttributeTargets.Interface, AllowMultiple = true, Inherited = false)]
public sealed class GoMemberRecordAttribute : Attribute
{
    /// <summary>Records a fact that carries no value.</summary>
    public GoMemberRecordAttribute(string member, GoMemberFact fact)
    {
        Member = member;
        Fact = fact;
    }

    /// <summary>Records a fact with its value.</summary>
    public GoMemberRecordAttribute(string member, GoMemberFact fact, string value) : this(member, fact)
    {
        Value = value;
    }

    /// <summary>Records a fact whose value is Go array dims, outermost first.</summary>
    public GoMemberRecordAttribute(string member, GoMemberFact fact, params long[] dims) : this(member, fact)
    {
        Dims = dims;
    }

    /// <summary>Records a fact whose value is a type: a <see cref="GoMemberFact.Descriptor"/> fact's carrier.</summary>
    public GoMemberRecordAttribute(string member, GoMemberFact fact, Type carrier) : this(member, fact)
    {
        Carrier = carrier;
    }

    /// <summary>The C# name of the member the fact is about.</summary>
    public string Member { get; }

    /// <summary>The fact.</summary>
    public GoMemberFact Fact { get; }

    /// <summary>The fact's value (a <see cref="GoMemberFact.Tag"/>'s tag), or null for a fact that has none.</summary>
    public string? Value { get; }

    /// <summary>A <see cref="GoMemberFact.Dims"/> or <see cref="GoMemberFact.KeyDims"/> fact's dims, outermost first, or null for any other fact.</summary>
    public long[]? Dims { get; }

    /// <summary>A <see cref="GoMemberFact.Descriptor"/> fact's carrier interface, or null for any other fact.</summary>
    public Type? Carrier { get; }
}
