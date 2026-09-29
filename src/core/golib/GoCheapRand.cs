// GoCheapRand.cs - Gbtc
// Copyright © 2026 The go2cs Authors. All rights reserved.
//
// Use of this source code is governed by an MIT-style license
// that can be found in the LICENSE file.

using System;
using System.Runtime.CompilerServices;

namespace go;

/// <summary>
/// Go's <c>runtime.cheaprand</c>/<c>cheaprandn</c>: a fast, per-thread, NON-cryptographic random
/// number for the runtime's own randomization, such as where each range over a map starts.
/// </summary>
/// <remarks>
/// Go keeps the state on the M, which is a thread; here it is thread-static. The step is Go's (a
/// wyrand add and a 64x64 multiply folded to 32 bits), and <see cref="Next(uint)"/> bounds it by
/// multiply-shift as <c>cheaprandn</c> does, with no division and no loop. The state seeds from
/// <see cref="Random.Shared"/> on a thread's first use, so no two threads share a sequence.
/// </remarks>
internal static class GoCheapRand
{
    [ThreadStatic]
    private static ulong t_state;

    /// <summary>A random 32-bit value.</summary>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    internal static uint Next()
    {
        ulong state = t_state;

        if (state == 0)
            state = (ulong)Random.Shared.NextInt64() | 1;

        state += 0xa0761d6478bd642f;
        t_state = state;

        ulong high = Math.BigMul(state, state ^ 0xe7037ed1a0b428db, out ulong low);
        return (uint)(high ^ low);
    }

    /// <summary>A random value in [0, <paramref name="n"/>).</summary>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    internal static uint Next(uint n) => (uint)(((ulong)Next() * n) >> 32);
}
