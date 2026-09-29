// malloc_impl.cs - Gbtc
// Copyright © 2026 The go2cs Authors. All rights reserved.
//
// SPDX-License-Identifier: BSD-3-Clause
// Use of this source code is governed by a BSD-style license
// that can be found in the LICENSE file.

// go2cs HAND-OWNED companion to malloc.go.
//
// ZEROBASE. manualConversionVars["runtime"] displaces `var zerobase uintptr` ("base address for all
// 0-byte allocations"), and this file declares it over golib's GoZeroBase, the one box every zero-size
// allocation golib makes already names. So mallocgc's `&zerobase`, arena.go's and slice.go's, and
// export_test.go's ZeroBase are the same pointer as new(struct{}), &s[i] of a []struct{} and the data
// word of make([]T, 0): equal, one hash, one order token, one address, as Go measures them. Go takes
// its address, so the heap box `Ꮡzerobase` is declared here too; the value member is a ref over it.
//
// TEST SEAM. GolibTests is not in runtime's InternalsVisibleTo grant, so the Go-prefixed PUBLIC
// helper below exposes runtime's zerobase for the zero-base identity guard (pinner_impl.cs's
// pattern: one public helper per operation).
[module: go.GoManualConversion]

namespace go;

using @unsafe = unsafe_package;

public static partial class runtime_package
{
    // base address for all 0-byte allocations
    internal static ж<uintptr> Ꮡzerobase => GoZeroBase.Box;

    internal static ref uintptr zerobase => ref Ꮡzerobase.Value;

    /// <summary>
    /// <c>unsafe.Pointer(&amp;zerobase)</c> — the address Go's mallocgc answers for every zero-byte
    /// allocation, as export_test.go's <c>ZeroBase</c> reads it.
    /// </summary>
    public static @unsafe.Pointer GoZeroBaseProbe() => @unsafe.Pointer.FromBox(Ꮡzerobase);
}
