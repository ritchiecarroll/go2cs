// mbitmap_impl.cs - Gbtc
// Copyright © 2026 The go2cs Authors. All rights reserved.
//
// SPDX-License-Identifier: BSD-3-Clause
// Use of this source code is governed by a BSD-style license
// that can be found in the LICENSE file.

// The GC pointer bitmap, answered from the TYPE rather than from the heap.
//
// Go's getgcmask reads the collector's own metadata to answer: findObject and the span's
// typePointersOfUnchecked iterator for a heap object, activeModules' data/bss bitmaps for a global,
// the frame's locals map for a stack slot. None of those exist in a managed runtime, and the
// converted body does not fail cheaply when it tries — reflect's TestGCBits reported
// infrastructure-error rather than a failure, because the process does not survive the walk.
//
// The datum itself is not lost, which is why this is hand-owned rather than disclosed. The bitmap is
// a TYPE-level property: which words of the object hold pointers the collector must scan. golib's
// layout walk already enumerates exactly those words — it is where PtrBytes comes from, PtrBytes
// being the same enumeration reported at coarser resolution (where the LAST pointer word ends,
// rather than WHICH words they are). GoReflect.GoGCMaskOf answers from that walk, so this reports
// the same truth at finer grain rather than substituting a plausible one.
//
// What is deliberately NOT reproduced: Go's cross-check of the heap-derived mask against the
// type-derived one ("found two different masks from two different methods"), and its zeroed-tail
// assertion. Both compare the ALLOCATOR's view with the TYPE's view, and there is only one view
// here — asserting agreement between an answer and itself is not a check, it is a decoration.
//
// Hand-owned: registered in manualConversionFuncs["runtime"]["getgcmask"], so a -stdlib reconvert
// emits the declaration as a placeholder and never regenerates the body over this file.

using System;

[module: go.GoManualConversion]

namespace go;

partial class runtime_package
{
    // Returns GC type info for the pointer stored in ep for testing.
    //
    // Go requires the argument to be a POINTER to the value type whose mask is wanted — reflect's
    // verifyGCBits calls it as GCBits(New(typ).Interface()) — and panics with its own text when it
    // is not. That contract is kept exactly: the managed argument is the ж<T> box standing for
    // *T, and T is what gets walked.
    //
    // The result is one entry per POINTER WORD, 0 or 1, from the object's base upward. That
    // granularity is Go's own, read off getgcmask's construction (`make([]byte, n/goarch.PtrSize)`,
    // indexed `[i/goarch.PtrSize]`) and NOT off reflect's doc comment, which says "one entry per
    // byte" about the bitmap's storage. verifyGCBits compares by PREFIX — it forgives a mask longer
    // than expected, because Go's iterator runs out to the size class, and forgives nothing that is
    // shifted — so a byte-vs-word transposition would fail everywhere while an over-long answer
    // passes.
    internal static slice<byte> /*mask*/ getgcmask(any epʗp)
    {
        // The pointer contract is enforced by the same subsumption test every other value-side
        // descent in golib uses, rather than by a second rule written here: PointeeTypeOfValue
        // takes the interface- and named-pointer-adapter hops a boxed `any` may carry and answers
        // null for anything that is not a Go pointer — which is exactly Go's `Kind_ != Pointer`.
        Type? elem = GoReflect.PointeeTypeOfValue(epʗp);

        if (elem is null)
        {
            // Go's own text, verbatim. It is spelled here rather than referenced because the
            // converter hoists a body's string literals WITH the body: displacing getgcmask
            // removes badArgumentToGetgcmaskˢ from mbitmap.cs along with the two literals only
            // the checks this hand-own deliberately drops were using. The seam owns its own
            // constant, so nothing here depends on a hoist the displacement removes.
            @throw("bad argument to getgcmask: expected type to be a pointer to the value type whose mask is being queried"u8);
            return default!;
        }

        // An array's Go length lives in the VALUE here, not in the managed type, so the dims come
        // off the pointee when there is one to read. That is not the zero-instance route this whole
        // hand-own exists to avoid: the caller already allocated the object and handed us a pointer
        // to it, so reading it costs nothing and invents nothing. PointeeArrayDims answers null for
        // every non-array pointee, which is the "no dims" case GoGCMaskOf already takes.
        byte[]? mask = GoReflect.GoGCMaskOf(elem, GoReflect.PointeeArrayDims(epʗp));

        // A type whose layout is not derivable is reported as Go reports a noscan span: no mask at
        // all. An answer that is WRONG passes no prefix check, so a short or invented one would be
        // worse than none.
        return mask is null ? default! : new slice<byte>(mask);
    }
}
