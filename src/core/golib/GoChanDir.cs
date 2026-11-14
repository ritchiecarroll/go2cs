// GoChanDir.cs - Gbtc
// Copyright © 2026 The go2cs Authors. All rights reserved.
//
// Use of this source code is governed by an MIT-style license
// that can be found in the LICENSE file.

namespace go;

/// <summary>
/// The DIRECTION half of a Go channel type — the part the managed emission cannot express in the
/// type itself, carried on the <see cref="channel{T}"/> value instead and read back by the
/// reflection bridge as descriptor cargo.
/// </summary>
/// <remarks>
/// <para>
/// A Go channel's direction belongs to its TYPE (<c>chan int</c>, <c>chan&lt;- int</c> and
/// <c>&lt;-chan int</c> are three distinct types), and it is the one part <c>channel&lt;T&gt;</c>
/// cannot hold: all three emit as one managed generic type. The bridge therefore recovers it from
/// a live source the way it recovers a fixed-size array's length — see
/// <see cref="GoArrayDimsAttribute"/> for the same rule at the same finite set of positions.
/// </para>
/// <para>
/// The numeric values are <c>internal/abi.ChanDir</c>'s own, so the cargo crosses into the
/// descriptor with no translation: <c>RecvDir = 1</c>, <c>SendDir = 2</c>, <c>BothDir = 3</c>,
/// <c>InvalidDir = 0</c>. <see cref="Unstamped"/> shares abi's zero deliberately — a channel whose
/// direction no source stamped is answered <see cref="Both"/>, which is what the bridge has always
/// reported and remains the honest answer for a type it cannot otherwise distinguish.
/// </para>
/// </remarks>
[EditorBrowsable(EditorBrowsableState.Never)] public enum GoChanDir : byte
{
    /// <summary>
    /// No source stamped a direction; the bridge answers <see cref="Both"/> (abi's
    /// <c>InvalidDir</c> value, never reported as such for a channel).
    /// </summary>
    Unstamped = 0,

    /// <summary>Receive-only — Go's <c>&lt;-chan T</c> (abi's <c>RecvDir</c>).</summary>
    Recv = 1,

    /// <summary>Send-only — Go's <c>chan&lt;- T</c> (abi's <c>SendDir</c>).</summary>
    Send = 2,

    /// <summary>Bidirectional — Go's <c>chan T</c> (abi's <c>BothDir</c>).</summary>
    Both = 3
}
