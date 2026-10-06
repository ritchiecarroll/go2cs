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
    Embedded = 1
}

/// <summary>
/// Carries a Go fact about one MEMBER of the decorated type, which converted code states in a comment
/// beside the member rather than in an attribute on it (docs/PLAN-marker-comment-parity.md, sections 5.4-5.6).
/// </summary>
/// <remarks>
/// <para>
/// Never written by the converter. go2cs-gen's <c>MemberRecordGenerator</c> reads the comment and emits
/// this on a GENERATED partial of the member's declaring type, since a generated file can add an attribute
/// to a type but not to a field, so converted code reads as the comment alone. The member is named by its
/// C# name.
/// </para>
/// <para>
/// golib reads the member attribute OR this record: hand-written files keep the attribute. A record whose
/// member is missing, or cannot hold the fact, is refused BY NAME when the type's records are first read,
/// never applied to a member it does not describe.
/// </para>
/// </remarks>
[AttributeUsage(AttributeTargets.Class | AttributeTargets.Struct | AttributeTargets.Interface, AllowMultiple = true, Inherited = false)]
public sealed class GoMemberRecordAttribute(string member, GoMemberFact fact) : Attribute
{
    /// <summary>The C# name of the member the fact is about.</summary>
    public string Member { get; } = member;

    /// <summary>The fact.</summary>
    public GoMemberFact Fact { get; } = fact;
}
