// alias_purego_impl.cs - Gbtc
// Copyright © 2026 The go2cs Authors. All rights reserved.
//
// SPDX-License-Identifier: BSD-3-Clause
// Use of this source code is governed by a BSD-style license
// that can be found in the LICENSE file.

// The vendored purego twin of crypto/internal/alias.AnyOverlap — golang.org/x/crypto's copy of the memory-ALIASING
// predicate, reached by chacha20 (XORKeyStream) and chacha20poly1305 (Seal / Open) through InexactOverlap on every
// record the CHACHA20 cipher suites crypto/tls negotiates. Go writes it by ORDERING element addresses, one reflect
// call deeper than the crypto/internal twin:
//
//     return len(x) > 0 && len(y) > 0 &&
//         reflect.ValueOf(&x[0]).Pointer() <= reflect.ValueOf(&y[len(y)-1]).Pointer() &&
//         reflect.ValueOf(&y[0]).Pointer() <= reflect.ValueOf(&x[len(x)-1]).Pointer()
//
// and the converter emits that literally, as four `reflect.ValueOf(Ꮡ(…)).Pointer()` takes over MANAGED storage —
// the same four-take shape whose tear was measured on the crypto/internal twin (2026-09-03, Release, tiering off:
// TRUE for two distinct fresh arrays 9 s into a 16-thread stress; `crypto/aes: invalid buffer overlap` 27 s in; the
// panic that killed the banked net/http row on two host classes). Each take pins its backing through a finalizable
// holder on a box that is garbage the instant the take returns, so a collection landing between two takes relocates
// an operand whose earlier pin has already been finalized, and the ordering then compares two heap layouts. The
// race is inherited here by construction; its own measurement is the VendoredAnyOverlap* guards in
// src/tests/GolibTests/AliasOverlapRaceTests.cs (RED on the reflect-based form, GREEN on this body).
//
// The converter drops the auto form of this declaration (manualConversionFuncs["vendor/golang.org/x/crypto/internal/alias"]
// in go2cs/manualTypeOperations.go), leaving a placeholder comment at the site, and the body below answers by
// STRUCTURE — golib slice<T>.Overlaps: canonical backing identity + absolute index-range intersection, with
// native-address and zero-size arms — which cannot tear. InexactOverlap stays converted: its `Ꮡ(x, 0) == Ꮡ(y, 0)`
// early-out is ElemRefBox equality (storage identity + absolute index, never an address), so chacha20's in-place
// XORKeyStream(dst, dst) and chacha20poly1305's Open(payload[:0], …, payload) exact-alias contract was already
// structural. crypto/internal/alias/alias_impl.cs is the twin this mirrors, registration comment included.

using go;

[module: go.GoManualConversion]

namespace go.vendor.golang.org.x.crypto.@internal;

partial class alias_package
{
    // AnyOverlap reports whether x and y share memory at any (not necessarily
    // corresponding) index. The memory beyond the slice length is ignored.
    public static bool AnyOverlap(slice<byte> x, slice<byte> y)
    {
        return len(x) > 0 && len(y) > 0 && x.Overlaps(y);
    }
}
