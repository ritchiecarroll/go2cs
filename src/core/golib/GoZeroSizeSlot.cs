// GoZeroSizeSlot.cs - Gbtc
// Copyright © 2026 The go2cs Authors. All rights reserved.
//
// Use of this source code is governed by an MIT-style license
// that can be found in the LICENSE file.

// ReSharper disable CheckNamespace
// ReSharper disable StaticMemberInGenericType

namespace go;

/// <summary>
/// The ref TARGET go2cs-gen's <c>Ꮡ&lt;field&gt;</c> accessor answers for a Go ZERO-SIZE field laid out
/// readonly at Go's offset (A17, COORD ruling 2026-09-28): one shared slot per zero-size type.
/// </summary>
/// <remarks>
/// <para>
/// WHY. Under the zero-size-field layout arc (src/go2cs/zeroSizeFieldLayout.go) a zero-size field shares
/// its offset with the next field and is READONLY, so its one C# byte can never be written over that
/// neighbour. A NAMED zero-size field (<c>noCopy noCopy</c>, <c>pad struct{}</c>) is addressable in Go,
/// and a writable ref to a readonly field does not compile (CS8160). A zero-size value carries no state,
/// so a write through its address stores NOTHING in Go: pointing the ref at this shared slot instead of
/// the field is faithful, and the field's own byte stays untouched.
/// </para>
/// <para>
/// ONLY THE TARGET, NEVER THE IDENTITY. A pointer to the field is still a FieldRefBox naming (containing
/// allocation, field); for a Go zero-size element type its <c>uintptr</c> is the box's order token (base +
/// Go field offset), never this slot's address, so <c>&amp;x.f != &amp;y.f</c> for distinct x and y, as in
/// Go (C2's Z2 field-stays-distinct rule).
/// </para>
/// <para>
/// Public because the accessors are generated into every converted assembly, and golib's own
/// <see cref="GoZeroSizeFacts{T}"/> is internal.
/// </para>
/// </remarks>
/// <typeparam name="T">A Go zero-size type.</typeparam>
public static class GoZeroSizeSlot<T>
{
    private static T s_slot = default!;

    /// <summary>The shared slot every readonly zero-size field of <typeparamref name="T"/> answers as its ref target.</summary>
    public static ref T Ref => ref s_slot;
}
