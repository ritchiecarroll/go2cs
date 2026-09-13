// alias_impl.cs - Gbtc
// Copyright © 2026 The go2cs Authors. All rights reserved.
//
// SPDX-License-Identifier: BSD-3-Clause
// Use of this source code is governed by a BSD-style license
// that can be found in the LICENSE file.

// AnyOverlap is the corpus's memory-ALIASING predicate. Go writes it by ORDERING element addresses:
//
//     return len(x) > 0 && len(y) > 0 &&
//         uintptr(unsafe.Pointer(&x[0])) <= uintptr(unsafe.Pointer(&y[len(y)-1])) &&
//         uintptr(unsafe.Pointer(&y[0])) <= uintptr(unsafe.Pointer(&x[len(x)-1]))
//
// and the converter emits that literally, as four `(uintptr)Ꮡ(…)` takes over MANAGED storage. Each take
// pins its backing through a finalizable holder on a box that is garbage the instant the take returns,
// so the pin is released by the FINALIZER, not by the next take; a collection landing between two takes
// relocates an operand whose earlier pin has already been finalized, and the ordering then compares two
// heap layouts. Measured 2026-09-03 (Release, tiering off, 4 cores, 16 threads + allocation churn): the
// mirrored four-take predicate tore FIVE threads on one collection 17 s in — every quadruple four real
// heap addresses with exactly one array's pair inconsistent (x relocated between take 1 and take 4 by
// 3.6–43.9 MB, or y between take 2 and take 3), the re-take consistent and FALSE; the converted
// AnyOverlap answered TRUE for two distinct fresh arrays 9 s in; the converted GCM Open raised
// `crypto/aes: invalid buffer overlap` 27 s in through crypto/aes Encrypt's InexactOverlap guard on
// counterCrypt's Encrypt(mask[:], counter[:]) — the panic that killed the banked net/http row on two
// host classes (four deaths at 286 / 373 / 656 / 2,124 s). A Debug build of the same probe reads clean:
// a non-optimizing frame roots its temporaries for the method's life, so all four pins hold there.
//
// The converter drops the auto form of this declaration (manualConversionFuncs["crypto/internal/alias"]
// in go2cs/manualTypeOperations.go), leaving a placeholder comment at the site, and the body below
// answers by STRUCTURE — golib slice<T>.Overlaps: canonical backing identity + absolute index-range
// intersection, with native-address and zero-size arms — which cannot tear. InexactOverlap stays
// converted: its `Ꮡ(x, 0) == Ꮡ(y, 0)` early-out is ElemRefBox equality (storage identity + absolute
// index, never an address), so the exact-alias contract crypto/tls's in-place record decrypt and
// newGCM's Encrypt(key[:], key[:]) lean on was already structural. The seven contract assertions in
// src/tests/GolibTests/AliasOverlapTests.cs bind this body; AliasOverlapRaceTests.cs holds the
// mechanism and the stress guard that is RED at the address-ordering form and GREEN here.

using go;

[module: go.GoManualConversion]

namespace go.crypto.@internal;

partial class alias_package
{
    // AnyOverlap reports whether x and y share memory at any (not necessarily
    // corresponding) index. The memory beyond the slice length is ignored.
    public static bool AnyOverlap(slice<byte> x, slice<byte> y)
    {
        return len(x) > 0 && len(y) > 0 && x.Overlaps(y);
    }
}
