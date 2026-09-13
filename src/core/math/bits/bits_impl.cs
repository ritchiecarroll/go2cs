// bits_impl.cs - Gbtc
// Copyright © 2026 The go2cs Authors. All rights reserved.
//
// SPDX-License-Identifier: BSD-3-Clause
// Use of this source code is governed by a BSD-style license
// that can be found in the LICENSE file.

// THE THREE WORD-SIZE LEAVES, AS ONE BCL CALL EACH.
//
// `math/big` calls `bits.Mul` and `bits.Add` -- the nuint WORD-SIZE wrappers -- from the innermost
// loop of Montgomery multiplication (`addMulVVW`), i.e. from every RSA private-key operation. The
// converter emits each of them faithfully as a TWO-LEVEL body:
//
//     public static (nuint hi, nuint lo) Mul(nuint x, nuint y) {
//         if (UintSize == 32) { ... Mul32 ... }
//         var (h, l) = Mul64((uint64)x, (uint64)y);      // second call, second tuple
//         return ((nuint)h, (nuint)l);
//     }
//
// Faithful, and it costs. Go's compiler intrinsifies `bits.Mul64` to a single MULQ at the call site
// (`ssagen/ssa.go:5022`) and aliases `math/big`'s own `mulWW` onto it (`:5113`), so Go has no call,
// no tuple, and no branch. Two mechanisms make the emitted form expensive, and BOTH are in this body:
//
//   1. `UintSize == 32` is a STRUCT COMPARISON, evaluated per call -- measured 2.72x. `bits.cs:21`
//      emits `public static UntypedInt UintSize => 64;`, a property returning the generated
//      `UntypedInt` struct; its `operator ==` is `left.Equals(right)` over a private `Compare` the
//      JIT compiles standalone at IL 141 and never inlines. Go folds this branch at compile time.
//   2. IL SIZE over the JIT's default inlining budget -- measured 1.32-1.42x. The two-level shape
//      puts `Mul` at IL 83 and `Add` at IL 87, and `DOTNET_JitDisasmSummary` shows both compiled as
//      standalone methods rather than inlined into the loop.
//
// Collapsing each to ONE BCL call removes both at once: no branch, no nested call, no inter-level
// tuple. `Mul64`/`Add64`/`Sub64` are deliberately LEFT as the converter emits them -- they are not on
// this path and displacing them bought a measured zero when it was tried (see the withdrawn
// `claude/g-mathbits-intrinsics`, whose registration took the 64-bit leaves and NOT these three,
// which is exactly why it moved nothing).
//
// MEASURED (Release + DOTNET_TieredCompilation=0, one host, one variable, records to distinct paths):
//
//     addMulVVW inner loop      22.4  -> 2.76-2.89 ns/word      8.1x
//     RSA-2048 PSS signature    68.5  -> 22.8 ms                3.0x  (-67%)
//     signature vs Go                    82x -> 27x
//
// The loop figure lands on variant D (`slice<nuint>` + inline `Math.BigMul`, 2.72-2.90 ns/word) within
// noise -- i.e. it is now bounded by golib's `slice<T>` indexing (1.47-1.50x), not by the call. What
// remains between it and a raw `Span<ulong>` loop is the container, and that is a separate question.
//
// ⚠ `Sub` IS NOT EXERCISED BY THAT LOOP. `addMulVVW` never calls it; it is rewritten here for
// consistency with its two siblings and is measured by nothing above. Its correctness rides on
// `math/bits`'s own banked row, which is what that row is for.

using System;

[module: go.GoManualConversion]

namespace go.math;

partial class bits_package
{
    // Mul returns the full-width product of x and y: (hi, lo) = x * y.
    //
    // No attribute: at one level this body is small enough that the JIT inlines it on its own --
    // `DOTNET_JitDisasmSummary` shows `bits_package:Mul` ABSENT from the compile list entirely once
    // it is written this way, where the two-level form appeared at IL 83 / code 141.
    public static (nuint hi, nuint lo) Mul(nuint x, nuint y)
    {
        uint64 hi = Math.BigMul((uint64)x, (uint64)y, out uint64 lo);
        return ((nuint)hi, (nuint)lo);
    }

    // Add returns the sum with carry of x, y and carry: sum = x + y + carry.
    // The carry input must be 0 or 1; otherwise the behavior is undefined.
    //
    // ⚠ THE ATTRIBUTE IS LOAD-BEARING AND IT IS MEASURED, not precautionary. Unlike `Mul`, this body
    // does NOT clear the default inlining budget on its own: at one level it is still IL 59, and
    // `DOTNET_JitDisasmSummary` showed it compiled standalone --
    //
    //     25: JIT compiled go.math.bits_package:Add(nuint,nuint,nuint) [FullOpts, IL size=59, code size=38]
    //
    // -- while `Mul` had already vanished from that list. `addMulVVW` calls `Add` TWICE per word, so
    // two surviving calls per word held the loop at 6.50-6.93 ns/word. Adding the attribute HERE and
    // nowhere else took it to 2.76-2.89 and emptied `bits_package` from the compile list. If a future
    // runtime inlines this body unaided, the attribute becomes redundant rather than wrong.
    [System.Runtime.CompilerServices.MethodImpl(System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining)]
    public static (nuint sum, nuint carryOut) Add(nuint x, nuint y, nuint carry)
    {
        UInt128 sum = (UInt128)(uint64)x + (uint64)y + (uint64)carry;
        return ((nuint)(uint64)sum, (nuint)(uint64)(sum >> 64));
    }

    // Sub returns the difference of x, y and borrow: diff = x - y - borrow.
    // The borrow input must be 0 or 1; otherwise the behavior is undefined.
    //
    // On borrow the 128-bit subtraction wraps and every high bit is set, so the low bit of the high
    // half is the borrow; without one the high half is zero.
    public static (nuint diff, nuint borrowOut) Sub(nuint x, nuint y, nuint borrow)
    {
        UInt128 diff = (UInt128)(uint64)x - (uint64)y - (uint64)borrow;
        return ((nuint)(uint64)diff, (nuint)((uint64)(diff >> 64) & 1));
    }
}
