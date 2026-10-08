// GoDescriptorTypeAttribute.cs - Gbtc
// Copyright © 2026 The go2cs Authors. All rights reserved.
//
// Use of this source code is governed by an MIT-style license
// that can be found in the LICENSE file.

using System;

namespace go;

/// <summary>
/// Names the DESCRIPTOR CARRIER for a position whose emitted C# type has lost the Go type's name.
/// </summary>
/// <remarks>
/// <para>
/// A Go DEFINED type over a NAMED interface — <c>type Token any</c>, <c>type Reader io.Reader</c> —
/// is emitted as a <c>global using</c> alias rather than a nested type, because it has exactly that
/// interface's method set and can carry no methods of its own (see <c>visitTypeSpec</c>). A C#
/// <c>using</c> alias is a COMPILE-TIME construct: it leaves no metadata, so by the time the
/// reflection bridge sees the position the type is plain <c>System.Object</c> (or the target
/// interface), and <c>reflect.Type.Name()</c> answers <c>""</c> where Go answers <c>Token</c> —
/// or, for the non-empty case, answers the TARGET's name, which is a different type's.
/// </para>
/// <para>
/// The alias is not the defect; it is what makes Go's universal assignability to <c>any</c> fall
/// out of C# assignment for free, and no C# type carries a name AND keeps that. So the VALUE stays
/// as it is and only the DESCRIPTOR gains an identity: the converter emits an uninhabited carrier
/// interface beside the alias — <c>[GoLocalName("Token")] internal interface ΔTokenᴅ { }</c>, which
/// nothing implements and no value is ever of — and stamps this attribute wherever a descriptor is
/// minted from that static Go type. The bridge substitutes the carrier's <c>System.Type</c> when
/// synthesizing the descriptor, so golib's existing naming reconstruction (<c>GoTypeName</c>,
/// <c>HasGoName</c>, <c>GoPackagePath</c>) answers Go's name with no change of its own. The
/// carrier's <c>Kind</c> is <c>Interface</c>, identical to the erased type's, so nothing else moves.
/// </para>
/// <para>
/// This is the <see cref="GoArrayDimsAttribute"/> pattern for a different lost datum, at the same
/// finite set of positions and for the same reason: the managed type cannot hold it, so it travels
/// as a converter stamp. On a struct FIELD the converter no longer writes the attribute: it records
/// the carrier on the field's declaring type as <c>[GoMemberRecord(field, GoMemberFact.Descriptor,
/// typeof(carrier))]</c>, in the package's metadata file, so converted code carries neither
/// (docs/PLAN-marker-comment-parity.md, section 11). A hand-written field keeps the attribute, and
/// golib reads either. <see cref="Self"/> is the position's own type. <see cref="Elem"/> and
/// <see cref="Key"/> are declared for the hops <c>Elem()</c> and <c>Key()</c> hand down and are
/// NOT read yet — a slice/pointer/map element's carrier has to ride on the DESCRIPTOR rather than
/// be re-read per access, which is a descriptor-shape change deliberately sequenced after this one.
/// They exist here so the attribute's shape does not change when that lands.
/// </para>
/// </remarks>
[AttributeUsage(AttributeTargets.Field | AttributeTargets.Parameter | AttributeTargets.ReturnValue)]
public sealed class GoDescriptorTypeAttribute : Attribute
{
    /// <summary>The carrier for the position's OWN Go type.</summary>
    public Type? Self { get; set; }

    /// <summary>The carrier for what this position's <c>Elem()</c> hands down. Not read yet.</summary>
    public Type? Elem { get; set; }

    /// <summary>The carrier for what this position's <c>Key()</c> hands down. Not read yet.</summary>
    public Type? Key { get; set; }
}
