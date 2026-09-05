// GoChanDirAttribute.cs - Gbtc
// Copyright © 2026 The go2cs Authors. All rights reserved.
//
// Use of this source code is governed by an MIT-style license
// that can be found in the LICENSE file.

using System;

namespace go;

/// <summary>
/// Carries the Go channel DIRECTION chain of a DEFINED channel type, outermost first
/// (<c>type R &lt;-chan T</c> ⇒ <c>[GoChanDir(GoChanDir.Recv)]</c>; <c>type R &lt;-chan (chan&lt;- T)</c>
/// ⇒ <c>[GoChanDir(GoChanDir.Recv, GoChanDir.Send)]</c>) -- on the wrapper STRUCT the converter emits
/// for it, beside the <c>[GoType("chan T")]</c> marker that cannot spell it.
/// </summary>
/// <remarks>
/// <para>
/// An UNNAMED directional channel carries its direction on the VALUE (increment D's cargo:
/// <see cref="channel{T}.RecvOnly"/>, <see cref="ChanCargo"/>) and the descriptor reads it off the
/// live channel or the slot's initializer. A DEFINED channel type has neither: its wrapper's marker
/// is direction-agnostic (go2cs-gen dispatches the Channel template on the <c>chan </c> prefix), its
/// values are the wrapper struct rather than a stamped <c>channel&lt;T&gt;</c>, and so
/// <c>reflect.TypeOf(IntChanRecv(nil)).ChanDir()</c> answered bidirectional and TestConvert's
/// ConvertibleTo matrix read 24 named-channel lines wrong (increment E3 follow-up 7e-b).
/// </para>
/// <para>
/// This is the <see cref="GoArrayDimsAttribute"/> pattern for a different lost datum at a different
/// position: TYPE-level cargo the marker cannot carry, stamped by the converter and read back once per
/// type by <see cref="GoReflect.TypeStampedChanDirChain"/>, which <c>abi.synthType</c> consults for
/// every descriptor -- the STAMP DECIDES: a caller's chain that disagrees with it is refused by name,
/// never averaged -- so every route (a value, a slot, <c>Elem()</c>) interns to ONE descriptor carrying
/// the direction. Only the direction rides here; a defined channel type's ELEMENT dims stay a recorded
/// boundary.
/// </para>
/// </remarks>
[AttributeUsage(AttributeTargets.Struct, AllowMultiple = false, Inherited = false)]
public sealed class GoChanDirAttribute(params GoChanDir[] dirChain) : Attribute
{
    /// <summary>The direction chain, outermost first; never empty on an emitted stamp.</summary>
    public GoChanDir[] DirChain { get; } = dirChain;
}
