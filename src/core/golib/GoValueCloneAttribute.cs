// GoValueCloneAttribute.cs - Gbtc
// Copyright © 2026 The go2cs Authors. All rights reserved.
//
// Use of this source code is governed by an MIT-style license
// that can be found in the LICENSE file.

using System;

namespace go;

/// <summary>
/// Names the fields of a converted Go struct that a by-value copy must DEEP-copy: the fields whose
/// type is a fixed-size array, or another struct that itself carries one. go2cs-gen turns this into
/// the struct's <see cref="IGoValueClone"/> implementation — a <c>Clone()</c> that copies the value
/// and then re-clones exactly these fields.
/// </summary>
/// <remarks>
/// The converter is the single source of truth here: only it has the Go type information that
/// decides which fields need the deep copy, and it emits the matching <c>.Clone()</c> at every Go
/// by-value copy site. An EMBEDDED (promoted) struct member needs no listing for its own sake: it is
/// an INLINE field of the enclosing struct, so the C# struct copy already copies it, exactly as Go
/// does. (It was held in a shared <c>ж&lt;T&gt;</c> box until 2026-08-14, which gave the embed
/// reference semantics that a value copy then aliased — the defect behind go/types' type-parameter
/// identity wall.) Listing an embed remains meaningful only when the embedded TYPE itself carries a
/// fixed array; the converter does not walk into embeds yet, so an array reached only THROUGH an
/// embed is the one named residue of this attribute's class.
/// </remarks>
[AttributeUsage(AttributeTargets.Struct)]
[EditorBrowsable(EditorBrowsableState.Never)] public sealed class GoValueCloneAttribute(params string[] fields) : Attribute
{
    /// <summary>Names of the fields a by-value copy must re-clone.</summary>
    public string[] Fields { get; } = fields;
}
