// mgc_impl.cs - Gbtc
// Copyright © 2026 The go2cs Authors. All rights reserved.
//
// SPDX-License-Identifier: BSD-3-Clause
// Use of this source code is governed by a BSD-style license
// that can be found in the LICENSE file.

// go2cs HAND-OWNED companion — runtime.gcTestIsReachable over the CLR heap (C1-2).
// Coordinator routing: mailbox 1ef59adad (option (c)); the bisect that motivated it is i9's
// 9f00b7059 and the three-layer reading is C1's d79dbb317.
//
// WHAT THIS FILE OWNS. One body manualConversionFuncs["runtime"] displaces: gcTestIsReachable.
// Everything the converted body reached underneath — mheap_.specialReachableAlloc, addspecial,
// the specialReachable records and the sweeper arm that sets them — stays converted in mgc.cs /
// mheap.cs / mgcsweep.cs and is DEAD behind this one function, the same shape pinner_impl.cs
// records for its five.
//
// WHY IT COULD NOT STAY CONVERTED, and the cost of leaving it. The converted body needs THREE
// things this host does not have, in this order:
//
//   1. mheap_.specialReachableAlloc.alloc() — a fixalloc whose init is emitted at
//      <goos>/mheap.cs:669 but is reached only from mallocinit <- schedinit, and NOTHING CALLS
//      schedinit here. So f.size == 0 and mfixalloc.cs:81 prints "use of FixAlloc_Alloc before
//      FixAlloc_Init" and throws. (The same silently-unreached init cputicks_impl.cs, goargs_impl.cs,
//      goenvs_impl.cs and hash_impl.cs each name in their own headers.)
//   2. addspecial(p, ...) — needs spanOf(p) to find a real mspan; the managed host allocates no
//      spans.
//   3. s.done / s.reachable — written ONLY by the sweeper (mgcsweep.cs:607), which never runs.
//
// Initialising (1) alone therefore does not fix the row: it moves the throw to (3)'s
// "IsReachable failed" arm (mgc.cs:1722). That is why this is a replacement and not a repair.
//
// THE COST OF LEAVING IT was measured rather than assumed: since seat 3's fatal path landed
// (7d3d03284), a runtime.throw is an Environment.Exit(2) through FatalReport that no catch, no
// recover and no deferred function can intercept — so this one test ENDED THE TEST HOST and the
// runtime row lost 57 verdicts behind it, 185 -> 128 (i9's bisect, three probes, the 128-set
// measured a prefix of the 185-set).
//
// ⚠ WHAT THIS BODY CAN AND CANNOT ANSWER — read this before reading a FAIL here as a defect.
// Go's test (gc_test.go:269) allocates 16 objects, passes all 16, KeepAlives the even 8, and
// asserts (a) every kept object is reported reachable and (b) AT MOST ONE unintentionally-retained
// dead object. In this port (b) cannot hold, for a reason that is the port working correctly: an
// address is only ever minted by the ж<T> conversions, which PIN the storage for the box's whole
// life (EnsureStableAddress), and FromPinnedBox additionally RETAINS the box in the Pointer itself
// (unsafe.cs:480 — `new Pointer((uintptr)box, box)`, cut 2026-09-04 because "a pin whose holder is
// unreachable is a pin the finalizer releases while the address is still in flight"). Every object
// whose unsafe.Pointer the caller still holds is therefore rooted BY THAT POINTER. Taking an
// object's address here is closer to Go's runtime.KeepAlive than to Go's unsafe.Pointer.
//
// So the honest expected reading is ALL BITS SET: assertion (a) passes, (b) fails with 8, and the
// verdict at this one name becomes an ordinary FAIL instead of a host kill. That is the whole
// purpose — 57 verdicts recovered, one divergence disclosed with a structural reason.
//
// WHY THIS MEASURES RATHER THAN ASSERTS. Returning a hardcoded all-ones would produce the same
// number today and would be a lie the moment the mint changes: a bare `new @unsafe.Pointer(box)`
// retains nothing (unsafe.cs:237), and if the converter ever emits that form here the objects
// become collectable and the true answer stops being all-ones. A WeakReference observation is
// correct under BOTH models and needs no edit when one replaces the other — the fake-but-plausible
// shape hashtriemap.cs's header forbids, avoided the way pinner_impl.cs avoids it.
//
// REFERENT RESOLUTION is not re-derived here: goReferentOfUnsafePointer is pinner_impl.cs's, in
// this same partial class, and is the corpus's one answer to "the Go allocation this pointer
// names" (retained source first, else the provenance/token record; null for nil and for a native
// alias). SetFinalizer keys its ConditionalWeakTable on the same notion, so all three mechanisms
// agree on what "the object" is. If that helper ever moves, this file moves with it.
[module: go.GoManualConversion]

namespace go;

using System;
// The two file-scoped aliases the DISPLACED body needed. A using alias is file-scoped, and the
// displacement takes the alias with the body: once mgc.cs stops emitting gcTestIsReachable it
// stops emitting `using ꓸꓸꓸunsafeꓸPointer` too (it had no other user there), so this file is now
// the only declaration of it in the package and must carry it verbatim or the parameter type
// does not resolve. Measured: 0 occurrences left in mgc.cs after the displacement.
using @unsafe = unsafe_package;
using ꓸꓸꓸunsafeꓸPointer = Span<unsafe_package.Pointer>;

partial class runtime_package {

// gcTestIsReachable performs a GC and returns a bit set where bit i
// is set if ptrs[i] is reachable.
//
// Managed replacement (see the file header): the reachability question is asked of the CLR rather
// than of a span/special/sweep machinery this host does not have.
internal static uint64 /*mask*/ gcTestIsReachable(params ꓸꓸꓸunsafeꓸPointer ptrsʗp) {
    uint64 mask = default!;
    var ptrs = ptrsʗp.sslice();

    // Go's own guard, kept verbatim: the result is a 64-bit mask.
    if (len(ptrs) > 64) {
        throw panic("too many pointers for uint64 mask");
    }

    int n = (int)len(ptrs);

    // One short weak reference per pointer, over the Go allocation it names. A pointer that
    // resolves to no allocation (nil, or a native alias — Go's "not a Go pointer") gets no
    // observer and reports NOT reachable: this body declines to answer rather than guessing, and
    // Go's own body would have thrown at addspecial for the same argument.
    var observers = new WeakReference?[n];

    for (int i = 0; i < n; i++) {
        object? referent = goReferentOfUnsafePointer(ptrs[i]);
        observers[i] = referent is null ? null : new WeakReference(referent, trackResurrection: false);
    }

    // Go: "Make sure we don't retain ptrs." Kept, and it is not a formality — it is the only
    // retention this function is able to drop. Everything the CALLER still holds (its own slice,
    // and the pin/retention each unsafe.Pointer carries) stays, which is the header's ⚠.
    for (int i = 0; i < n; i++) {
        ptrs[i] = default!;
    }

    // Go forces a full GC and sweep. The two-pass collect with the finalizer drain between them is
    // the CLR's equivalent: pass one queues anything finalizable, the drain runs it, pass two
    // reclaims what the drain released. Without the second pass an object whose only reference was
    // released by a finalizer would still read alive.
    // ⚠ `global::System.GC` in full, and it is not pedantry: this class DECLARES Go's own
    // `runtime.GC()` (managed_impl.cs:292), so inside runtime_package the bare name `GC`
    // resolves to that method and `GC.Collect()` is CS0119 — 'GC() is a method, which is not
    // valid in the given context', three of them, measured on the os-matrix census before this
    // line was written this way. A `using System;` cannot win against a member of the
    // enclosing type. Do not simplify.
    global::System.GC.Collect();
    global::System.GC.WaitForPendingFinalizers();
    global::System.GC.Collect();

    for (int i = 0; i < n; i++) {
        if (observers[i] is {} observer && observer.IsAlive) {
            mask |= (uint64)(((uint64)1).Lsh((uint64)(i)));
        }
    }

    return mask;
}

} // end runtime_package
