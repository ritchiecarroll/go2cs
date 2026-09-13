// cond_impl.cs - Gbtc
// Copyright © 2026 The go2cs Authors. All rights reserved.
//
// SPDX-License-Identifier: BSD-3-Clause
// Use of this source code is governed by a BSD-style license
// that can be found in the LICENSE file.

// Hand-written implementation of sync's copyChecker.check — the "a Cond must not be copied after
// first use" guard. Go implements it by having the checker store its OWN ADDRESS in itself:
//
//	if uintptr(*c) != uintptr(unsafe.Pointer(c)) &&
//	   !atomic.CompareAndSwapUintptr((*uintptr)(c), 0, uintptr(unsafe.Pointer(c))) &&
//	   uintptr(*c) != uintptr(unsafe.Pointer(c)) { panic("sync.Cond is copied") }
//
// Neither half survives conversion:
//   - Storing an ADDRESS is unsound on a moving collector. The GC relocates the box holding the
//     Cond, so a compaction between two Wait calls would leave a stale word in a Cond that was
//     never copied — a SPURIOUSLY panicking condition variable, far worse than no check at all.
//   - The auto body is inert: the address-of-self operand converts to `unsafe.Pointer.FromRef(ref c)`
//     while the CAS destination converts to `Ꮡ((uintptr)(c))`, which BOXES A COPY of the value, so
//     the compare-and-swap writes to a throwaway box and the checker is never initialized. It
//     therefore never panics no matter how often the Cond is copied (sync's TestCondCopy).
//
// The managed form asks the same question — "am I being used from the allocation I was initialized
// in?" — against the invariant the CLR does guarantee: REFERENCE IDENTITY. Every Cond method reaches
// its checker as `Ꮡc.of(Cond.Ꮡchecker)`, whose ReferentObject is the root allocation holding the Cond
// (golib resolves nested field-ref chains to the root, so a Cond embedded in a larger struct answers
// with the outer box, the same object on every access). The checker stores a token derived from that
// object's GC-stable identity hash; a struct copy carries the ORIGINATING allocation's token into a
// new one, which is exactly what `check` sees.
//
// Two deliberate differences from Go, both in the safe direction:
//   - No compare-and-swap. Go needs one because concurrent first-users race to publish the word;
//     here every racer computes the SAME token for the same allocation, so an aligned native-word
//     store cannot publish a value any racer would disagree with. (A torn read is impossible: nuint
//     is naturally aligned and word-sized on every supported platform.)
//   - Identity hashes are not unique, so two allocations can collide and a copy between exactly
//     those two would go unreported. That is a missed detection, never a false alarm — and Go's own
//     check is documented as best-effort ("must not be copied", enforced where it can be seen).
//
// Copies that Go detects and this does not: two elements of the same backing array (both resolve to
// that array), because an element pointer's identity is the storage plus an index the checker has no
// room to store. Recorded, not worked around.

using System.Runtime.CompilerServices;

// Hand-owned (no cond_impl.go exists, so a reconvert never regenerates it); marked for consistency
// with the other hand-owned sync files. The converter drops only `copyChecker.check` from cond.cs —
// see manualConversionFuncs in src/go2cs/manualTypeOperations.go.
[module: go.GoManualConversion]

namespace go;

partial class sync_package
{
    // The token identifying the allocation this checker lives in. Never 0 — 0 is Go's
    // "uninitialized" sentinel, and the top bit is what guarantees a real allocation can never
    // produce it.
    private static nuint copyCheckerToken(ж<copyChecker> Ꮡc)
    {
        object root = ((INilPointer)Ꮡc).ReferentObject;
        return unchecked((nuint)(uint)RuntimeHelpers.GetHashCode(root) | ((nuint)1 << (System.IntPtr.Size * 8 - 1)));
    }

    internal static void check(this ж<copyChecker> Ꮡc)
    {
        ref var c = ref Ꮡc.Value;
        nuint self = copyCheckerToken(Ꮡc);
        nuint stored = (uintptr)c;

        // Fast path: initialized, and initialized HERE.
        if (stored == self)
            return;

        // Not initialized yet — claim it for this allocation (see the header on why no CAS).
        if (stored == 0)
        {
            c = (copyChecker)(uintptr)self;
            return;
        }

        // Initialized somewhere else: this Cond's bytes came from another allocation.
        throw panic("sync.Cond is copied");
    }
}
