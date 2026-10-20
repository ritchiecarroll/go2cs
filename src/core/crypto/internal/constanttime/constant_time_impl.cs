// constant_time_impl.cs - Gbtc
// Copyright © 2026 The go2cs Authors. All rights reserved.
//
// SPDX-License-Identifier: BSD-3-Clause
// Use of this source code is governed by a BSD-style license
// that can be found in the LICENSE file.

// boolToUint8 is a Go COMPILER INTRINSIC: the gc compiler replaces every call with a flag-to-register
// move (SETcc on amd64), and the body Go writes is only `panic("unreachable; must be intrinsicified")`.
// Converted faithfully, that body panics for real -- and every helper of this package goes through it
// (Select, ByteEq, Eq, LessOrEq), which crypto/subtle exposes and GCM's tag check reaches: Go 1.26's
// new package made EVERY converted GCM Open panic in Release (GolibTests
// ConvertedGcmOpenNeverPanicsWithOverlapUnderStress, 48 of 48; net/http's TLS read path). A Debug run
// never showed it because that test is Inconclusive on a JIT-optimizer-disabled build.
//
// The converter's intrinsic rule (compilerIntrinsic.go) recognizes Go's `panic("intrinsic")` convention,
// whose meaning is a no-op or a zero value. This body's text differs and its result MEANS something --
// 1 for true, 0 for false -- which no rule can read out of a panic message, so it is hand-owned here
// (manualConversionFuncs["crypto/internal/constanttime"]). The body is BRANCHLESS, as the intrinsic is:
// a .NET bool is one byte holding exactly 0 or 1, so reinterpreting it is the SETcc result itself, with
// no data-dependent branch for a timing side channel to read.

using System.Runtime.CompilerServices;
using go;

[module: go.GoManualConversion]

namespace go.crypto.@internal;

partial class constanttime_package
{
    // boolToUint8 is a compiler intrinsic.
    // It returns 1 for true and 0 for false.
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    internal static uint8 boolToUint8(bool b) {
        return Unsafe.As<bool, byte>(ref b);
    }
}
