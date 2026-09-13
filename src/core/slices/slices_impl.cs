// slices_impl.cs - Gbtc
// Copyright © 2026 The go2cs Authors. All rights reserved.
//
// SPDX-License-Identifier: BSD-3-Clause
// Use of this source code is governed by a BSD-style license
// that can be found in the LICENSE file.

// overlaps has the same four-take address-ordering shape as crypto/internal/alias.AnyOverlap (plus the
// `elemSize - 1` arithmetic), and the same race: each `(uintptr)Ꮡ(…)` take pins its backing only until
// the box that took it is finalized, so a collection between two takes compares two heap layouts (the
// measurement and the mechanism are stated in crypto/internal/alias/alias_impl.cs). Its callers are
// Insert and Replace, which read `if !overlaps(…) { copy in place }` and otherwise take the hard-case
// rotation, where startIdx PANICS `needle not found` for a source that does not actually alias — the
// same death class as the crypto guard, one panic text over.
//
// Two facts measured on the way, both 2026-09-03. The converted Insert/Replace COPY their variadic
// source at the seam (`vʗp.slice()` is a CopyOf), so the second defect read from the address form —
// for an ARRAY element type `Ꮡ(a, i).Value` is an `array<T>` and the operator's fixed-array arm
// answers the element's INNER storage, a spurious FALSE — has no observer in the corpus; and a
// spurious TRUE from the race is observable exactly as the panic above. The converter drops the auto
// form of this declaration (manualConversionFuncs["slices"] in go2cs/manualTypeOperations.go),
// leaving a placeholder comment at the site; the body keeps Go's two early-outs verbatim and answers
// the rest through golib slice<T>.Overlaps (canonical backing identity + absolute index range, with
// native-address and zero-size arms), which is correct in both directions and for every element type.

using go;
using @unsafe = go.unsafe_package;

[module: go.GoManualConversion]

namespace go;

partial class slices_package
{
    // overlaps reports whether the memory ranges a[0:len(a)] and b[0:len(b)] overlap.
    internal static bool overlaps<E>(slice<E> a, slice<E> b)
    {
        if (len(a) == 0 || len(b) == 0)
        {
            return false;
        }

        var elemSize = @unsafe.Sizeof(a[0]);

        if (elemSize == 0)
        {
            return false;
        }

        return a.Overlaps(b);
    }
}
