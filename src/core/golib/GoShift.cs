// GoShift.cs - Gbtc
// Copyright © 2026 The go2cs Authors. All rights reserved.
//
// Use of this source code is governed by an MIT-style license
// that can be found in the LICENSE file.

// ReSharper disable InconsistentNaming
// ReSharper disable BuiltInTypeReferenceStyle

using System.Runtime.CompilerServices;
using go.golib;

namespace go;

/// <summary>
/// Go-semantics shift helpers used by the converter when a shift's count is NOT provably within the
/// operand's bit width.
/// </summary>
/// <remarks>
/// Go and C# disagree when a shift count reaches or exceeds the operand's width:
/// <list type="bullet">
/// <item>Go: <c>x &gt;&gt; n</c> / <c>x &lt;&lt; n</c> with <c>n &gt;= width</c> yields 0 for an UNSIGNED
/// or LEFT shift; a SIGNED right shift sign-extends (0 for a non-negative value, -1 for a negative one).</item>
/// <item>C#: the native <c>&gt;&gt;</c>/<c>&lt;&lt;</c> operators MASK the count — <c>n &amp; 63</c> for a
/// 64-bit operand, <c>n &amp; 31</c> for a 32-bit operand, and sub-<c>int</c> operands promote to <c>int</c>
/// so they also mask by <c>&amp; 31</c>. So a native shift silently produces the wrong value once the count
/// can reach the width.</item>
/// </list>
/// The count is taken as a WIDE UNSIGNED <see cref="uint64"/> so its FULL magnitude is compared against the
/// width BEFORE any narrowing — a computed count such as <c>64 - n</c> that unsigned-wraps to a huge value
/// is then correctly seen as <c>&gt;= width</c>. A SIGNED count is widened to <see cref="int64"/> instead, and
/// its overloads raise Go's <c>runtime error: negative shift amount</c> panic for a count below zero, as
/// Go's <c>panicshift</c> does.
/// </remarks>
public static class GoShift
{
    // ---- 64-bit: uint64 / int64 ----------------------------------------------------------------------

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static uint64 Rsh(this uint64 x, uint64 n) => n >= 64 ? 0UL : x >> (int)n;

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static uint64 Lsh(this uint64 x, uint64 n) => n >= 64 ? 0UL : x << (int)n;

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static int64 Rsh(this int64 x, uint64 n) => n >= 64 ? x >> 63 : x >> (int)n;

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static int64 Lsh(this int64 x, uint64 n) => n >= 64 ? 0L : x << (int)n;

    // ---- native word: nuint / nint (64-bit on this target) -------------------------------------------

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static nuint Rsh(this nuint x, uint64 n) => n >= 64 ? (nuint)0 : x >> (int)n;

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static nuint Lsh(this nuint x, uint64 n) => n >= 64 ? (nuint)0 : x << (int)n;

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static nint Rsh(this nint x, uint64 n) => n >= 64 ? x >> 63 : x >> (int)n;

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static nint Lsh(this nint x, uint64 n) => n >= 64 ? (nint)0 : x << (int)n;

    // ---- uintptr (nuint-backed struct, 64-bit on this target) ----------------------------------------

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static uintptr Rsh(this uintptr x, uint64 n) => n >= 64 ? default(uintptr) : x >> (int)n;

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static uintptr Lsh(this uintptr x, uint64 n) => n >= 64 ? default(uintptr) : x << (int)n;

    // ---- 32-bit: uint32 / int32 ----------------------------------------------------------------------

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static uint32 Rsh(this uint32 x, uint64 n) => n >= 32 ? 0U : x >> (int)n;

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static uint32 Lsh(this uint32 x, uint64 n) => n >= 32 ? 0U : x << (int)n;

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static int32 Rsh(this int32 x, uint64 n) => n >= 32 ? x >> 31 : x >> (int)n;

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static int32 Lsh(this int32 x, uint64 n) => n >= 32 ? 0 : x << (int)n;

    // ---- sub-int: uint16 / int16 (promote to int in the C# shift, so cast back at the operand width) --

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static uint16 Rsh(this uint16 x, uint64 n) => n >= 16 ? (uint16)0 : (uint16)(x >> (int)n);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static uint16 Lsh(this uint16 x, uint64 n) => n >= 16 ? (uint16)0 : (uint16)(x << (int)n);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static int16 Rsh(this int16 x, uint64 n) => n >= 16 ? (int16)(x >> 15) : (int16)(x >> (int)n);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static int16 Lsh(this int16 x, uint64 n) => n >= 16 ? (int16)0 : (int16)(x << (int)n);

    // ---- sub-int: uint8 / int8 -----------------------------------------------------------------------

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static uint8 Rsh(this uint8 x, uint64 n) => n >= 8 ? (uint8)0 : (uint8)(x >> (int)n);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static uint8 Lsh(this uint8 x, uint64 n) => n >= 8 ? (uint8)0 : (uint8)(x << (int)n);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static int8 Rsh(this int8 x, uint64 n) => n >= 8 ? (int8)(x >> 7) : (int8)(x >> (int)n);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static int8 Lsh(this int8 x, uint64 n) => n >= 8 ? (int8)0 : (int8)(x << (int)n);

    // ---- COMPOUND ASSIGNMENT: `x >>= n` / `x <<= n` ----------------------------------------------------
    //
    // The same guard for Go's compound shift-assign, whose native C# form (`x >>= (int)n`) masks the count
    // exactly as the binary operator does -- runtime's softfloat fadd64 `gm >>= shift` with an exponent gap
    // of 664 shifted by 24 (664 & 63) and TestFloat64 read "-1 + 1e-200 = sw -0.9999999543755939, hw -1".
    // `ref this` so the converter emits `x.RshAssign(n)` and the target is evaluated ONCE: a field, a local,
    // and golib's ref-returning element indexers all bind, where a rewrite to `x = x.Rsh(n)` would evaluate
    // an indexed or selected target twice. Each delegates to its value twin above, so the two forms cannot
    // disagree about Go's rule.

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static void RshAssign(ref this uint64 x, uint64 n) => x = x.Rsh(n);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static void LshAssign(ref this uint64 x, uint64 n) => x = x.Lsh(n);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static void RshAssign(ref this int64 x, uint64 n) => x = x.Rsh(n);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static void LshAssign(ref this int64 x, uint64 n) => x = x.Lsh(n);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static void RshAssign(ref this nuint x, uint64 n) => x = x.Rsh(n);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static void LshAssign(ref this nuint x, uint64 n) => x = x.Lsh(n);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static void RshAssign(ref this nint x, uint64 n) => x = x.Rsh(n);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static void LshAssign(ref this nint x, uint64 n) => x = x.Lsh(n);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static void RshAssign(ref this uintptr x, uint64 n) => x = x.Rsh(n);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static void LshAssign(ref this uintptr x, uint64 n) => x = x.Lsh(n);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static void RshAssign(ref this uint32 x, uint64 n) => x = x.Rsh(n);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static void LshAssign(ref this uint32 x, uint64 n) => x = x.Lsh(n);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static void RshAssign(ref this int32 x, uint64 n) => x = x.Rsh(n);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static void LshAssign(ref this int32 x, uint64 n) => x = x.Lsh(n);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static void RshAssign(ref this uint16 x, uint64 n) => x = x.Rsh(n);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static void LshAssign(ref this uint16 x, uint64 n) => x = x.Lsh(n);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static void RshAssign(ref this int16 x, uint64 n) => x = x.Rsh(n);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static void LshAssign(ref this int16 x, uint64 n) => x = x.Lsh(n);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static void RshAssign(ref this uint8 x, uint64 n) => x = x.Rsh(n);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static void LshAssign(ref this uint8 x, uint64 n) => x = x.Lsh(n);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static void RshAssign(ref this int8 x, uint64 n) => x = x.Rsh(n);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static void LshAssign(ref this int8 x, uint64 n) => x = x.Lsh(n);

    // ---- a SIGNED count --------------------------------------------------------------------------------
    //
    // Go panics with the runtime.Error "negative shift amount" (runtime.panicshift) when a shift's count
    // is a signed integer below zero at run time. The converter widens a signed count to int64, never to
    // uint64 (where a negative count would read as a huge one and yield 0), so these overloads bind: a
    // negative count raises the panic, and any other defers to the unsigned guard above.

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static uint64 Rsh(this uint64 x, int64 n) => n < 0 ? throw RuntimeErrorPanic.NegativeShiftAmount() : x.Rsh((uint64)n);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static uint64 Lsh(this uint64 x, int64 n) => n < 0 ? throw RuntimeErrorPanic.NegativeShiftAmount() : x.Lsh((uint64)n);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static int64 Rsh(this int64 x, int64 n) => n < 0 ? throw RuntimeErrorPanic.NegativeShiftAmount() : x.Rsh((uint64)n);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static int64 Lsh(this int64 x, int64 n) => n < 0 ? throw RuntimeErrorPanic.NegativeShiftAmount() : x.Lsh((uint64)n);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static nuint Rsh(this nuint x, int64 n) => n < 0 ? throw RuntimeErrorPanic.NegativeShiftAmount() : x.Rsh((uint64)n);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static nuint Lsh(this nuint x, int64 n) => n < 0 ? throw RuntimeErrorPanic.NegativeShiftAmount() : x.Lsh((uint64)n);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static nint Rsh(this nint x, int64 n) => n < 0 ? throw RuntimeErrorPanic.NegativeShiftAmount() : x.Rsh((uint64)n);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static nint Lsh(this nint x, int64 n) => n < 0 ? throw RuntimeErrorPanic.NegativeShiftAmount() : x.Lsh((uint64)n);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static uintptr Rsh(this uintptr x, int64 n) => n < 0 ? throw RuntimeErrorPanic.NegativeShiftAmount() : x.Rsh((uint64)n);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static uintptr Lsh(this uintptr x, int64 n) => n < 0 ? throw RuntimeErrorPanic.NegativeShiftAmount() : x.Lsh((uint64)n);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static uint32 Rsh(this uint32 x, int64 n) => n < 0 ? throw RuntimeErrorPanic.NegativeShiftAmount() : x.Rsh((uint64)n);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static uint32 Lsh(this uint32 x, int64 n) => n < 0 ? throw RuntimeErrorPanic.NegativeShiftAmount() : x.Lsh((uint64)n);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static int32 Rsh(this int32 x, int64 n) => n < 0 ? throw RuntimeErrorPanic.NegativeShiftAmount() : x.Rsh((uint64)n);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static int32 Lsh(this int32 x, int64 n) => n < 0 ? throw RuntimeErrorPanic.NegativeShiftAmount() : x.Lsh((uint64)n);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static uint16 Rsh(this uint16 x, int64 n) => n < 0 ? throw RuntimeErrorPanic.NegativeShiftAmount() : x.Rsh((uint64)n);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static uint16 Lsh(this uint16 x, int64 n) => n < 0 ? throw RuntimeErrorPanic.NegativeShiftAmount() : x.Lsh((uint64)n);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static int16 Rsh(this int16 x, int64 n) => n < 0 ? throw RuntimeErrorPanic.NegativeShiftAmount() : x.Rsh((uint64)n);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static int16 Lsh(this int16 x, int64 n) => n < 0 ? throw RuntimeErrorPanic.NegativeShiftAmount() : x.Lsh((uint64)n);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static uint8 Rsh(this uint8 x, int64 n) => n < 0 ? throw RuntimeErrorPanic.NegativeShiftAmount() : x.Rsh((uint64)n);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static uint8 Lsh(this uint8 x, int64 n) => n < 0 ? throw RuntimeErrorPanic.NegativeShiftAmount() : x.Lsh((uint64)n);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static int8 Rsh(this int8 x, int64 n) => n < 0 ? throw RuntimeErrorPanic.NegativeShiftAmount() : x.Rsh((uint64)n);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static int8 Lsh(this int8 x, int64 n) => n < 0 ? throw RuntimeErrorPanic.NegativeShiftAmount() : x.Lsh((uint64)n);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static void RshAssign(ref this uint64 x, int64 n) => x = x.Rsh(n);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static void LshAssign(ref this uint64 x, int64 n) => x = x.Lsh(n);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static void RshAssign(ref this int64 x, int64 n) => x = x.Rsh(n);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static void LshAssign(ref this int64 x, int64 n) => x = x.Lsh(n);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static void RshAssign(ref this nuint x, int64 n) => x = x.Rsh(n);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static void LshAssign(ref this nuint x, int64 n) => x = x.Lsh(n);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static void RshAssign(ref this nint x, int64 n) => x = x.Rsh(n);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static void LshAssign(ref this nint x, int64 n) => x = x.Lsh(n);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static void RshAssign(ref this uintptr x, int64 n) => x = x.Rsh(n);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static void LshAssign(ref this uintptr x, int64 n) => x = x.Lsh(n);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static void RshAssign(ref this uint32 x, int64 n) => x = x.Rsh(n);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static void LshAssign(ref this uint32 x, int64 n) => x = x.Lsh(n);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static void RshAssign(ref this int32 x, int64 n) => x = x.Rsh(n);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static void LshAssign(ref this int32 x, int64 n) => x = x.Lsh(n);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static void RshAssign(ref this uint16 x, int64 n) => x = x.Rsh(n);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static void LshAssign(ref this uint16 x, int64 n) => x = x.Lsh(n);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static void RshAssign(ref this int16 x, int64 n) => x = x.Rsh(n);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static void LshAssign(ref this int16 x, int64 n) => x = x.Lsh(n);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static void RshAssign(ref this uint8 x, int64 n) => x = x.Rsh(n);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static void LshAssign(ref this uint8 x, int64 n) => x = x.Lsh(n);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static void RshAssign(ref this int8 x, int64 n) => x = x.Rsh(n);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static void LshAssign(ref this int8 x, int64 n) => x = x.Lsh(n);
}
