// pprof_impl.cs - Gbtc
// Copyright © 2026 The go2cs Authors. All rights reserved.
//
// SPDX-License-Identifier: BSD-3-Clause
// Use of this source code is governed by a BSD-style license
// that can be found in the LICENSE file.

// pprof_goroutineProfileWithLabels -- the one linkname destination pprof.cs declares bodyless that this
// file answers itself rather than forwarding to the runtime's converted body.
//
// WHY IT IS ANSWERED HERE AND NOT FORWARDED
//   runtime HAS the function with a real body, and a forwarder across the runtime/pprof -> runtime edge
//   would cost no project-reference cycle (the converter's linknameForwardTargets forwards six siblings
//   that way). It stays here because its body deliberately WITHHOLDS the label slice: a label pointer
//   goes stale under GC, and printCountProfile then sizes a slice from a corrupt map -- an
//   OutOfMemoryException that kills the host and turns the row into an infrastructure-error rather
//   than a verdict. That is a judgement the forwarder registry cannot make.
//
// WHAT THIS FILE NO LONGER DOES (2026-09-27, class M piece M2b, COORD ruling)
//   It also answered pprof_memProfileInternal, with an honest (0, true): "this runtime keeps no
//   memory-profile records". That stopped being true when class M gave the runtime real records (M1
//   allocations sampled at golib's allocation doors under Go's MemProfileRate rule, M2 frees and GC
//   cycles), so the body was removed, runtime.pprof_memProfileInternal joined linknameForwardTargets,
//   and TestFakeMapping's capability gate -- which existed because the empty profile made that test
//   pass vacuously -- was retired in the same commit, as its own retirement clause required. The
//   header's earlier belief that "no forwarder can exist" (a runtime -> runtime/pprof cycle) was about
//   the push direction and never applied to this pull; the full text is in git history at efc9c093de.
//
// NOT COVERED, AND NOT AN OVERSIGHT
//   pprof_blockProfileInternal, pprof_mutexProfileInternal, pprof_threadCreateInternal,
//   pprof_fpunwindExpand and pprof_makeProfStack are converted forwarders now (linknameForwardTargets),
//   not bodies here.
//
// Hand-owned (no pprof_impl.go exists, so a reconvert never regenerates this file).
[module: go.GoManualConversion]

namespace go.runtime;

using profilerecord = go.@internal.profilerecord_package;
using @unsafe = unsafe_package;

partial class pprof_package {

// The GOROUTINE PROFILE, over golib's live goroutine registry. Design: the 2026-09-04 Q27 section
// of BOARD-next-validation-candidates.md.
//
// WHAT IS REPORTED, AND WHY EACH PART IS A FACT RATHER THAN A MODEL
//
//   THE POPULATION is Go's. `Goroutine.ProfileSnapshot()` walks the registry and returns the
//   goroutines Go's `gcount()` would count -- user goroutines -- plus the finalizer goroutine while
//   it is running a finalizer body, which is Go's own special case (`isSystemGoroutine` answers
//   false for `runfinq` exactly while `fingStatus&fingRunningFinalizer` is set, and
//   goroutineProfileWithLabelsConcurrent adds one to n for it by name).
//
//   THE LABELS are the same pointers the goroutines set. runtime_setProfLabel writes golib's
//   per-goroutine slot (proflabel_impl.cs) and this hands the value straight back; nothing is
//   interpreted in between.
//
//   THE STACK is one frame: the goroutine's START FUNCTION, Go's gp.startpc. This is the part worth
//   being precise about. Go's saveg records the whole traceback, and the managed runtime cannot
//   walk a foreign thread's stack -- runtime.Stack(all) states that same limit with its
//   ForeignStackPlaceholder. What it CAN state is the bottom Go frame of the very traceback saveg
//   would have recorded. So this is an INCOMPLETE stack, not an invented one, and the distinction
//   is the whole reason a body is admissible here at all: a deeper stack would have to be made up.
//
//   THE PC is not invented either. GoSyntheticPC mints stable process-lifetime tokens for exactly
//   this case -- a function whose address is taken without calling it -- and runtime's
//   syntheticFrameRecord resolves them back through CallersFrames to an import-path-qualified Go
//   name, so both consumers symbolize the result with no further work: printCountProfile's debug
//   renderer and profileBuilder's proto encoder.
//
// A goroutine with no start function -- the main goroutine, or a thread a host entered directly --
// reports an EMPTY stack rather than borrowing a frame from somewhere. Go has no such goroutine, so
// there is no Go behaviour to match; an empty stack is the answer that claims nothing.
//
// NOT ATOMIC, and Go's is not either once the world restarts. Go stops the world to count and then
// lets goroutines add themselves; this reads a snapshot. A goroutine created between the caller's
// sizing call and its filling call is simply absent, which is the tolerance Go documents for its
// own concurrent collection: "New goroutines may not be in this list, but we didn't want to know
// about them anyway."
internal static partial (nint n, bool ok) pprof_goroutineProfileWithLabels(slice<profilerecord.StackRecord> p, slice<@unsafe.Pointer> labels) {
    // Go's own guard (goroutineProfileWithLabels, mprof.go): a labels slice whose length does not
    // match p is dropped, and the concurrent collector then writes a label only when it holds a
    // slice. The same predicate, stated once -- a nil slice has length 0 and never matches a
    // non-empty p, so it never reaches the loop below.
    bool writeLabels = len(labels) == len(p);

    var snapshot = global::go.golib.Goroutine.ProfileSnapshot();
    nint n = ((nint)snapshot.Length);

    // Go answers (gcount(), false) for an empty slice without collecting anything -- "an empty
    // slice is obviously too small" -- and says false unconditionally, including when the count is
    // itself zero. Kept as its own arm so that edge reads the same here.
    if (len(p) == 0) {
        return (n, false);
    }

    // Per the contract of runtime.GoroutineProfile: when p cannot hold the whole profile we are not
    // allowed to write to it AT ALL, and must answer (n, false) so the caller can resize and retry.
    if (n > len(p)) {
        return (n, false);
    }

    for (nint i = 0; i < n; i++) {
        var entry = snapshot[((int)i)];

        p[i] = new profilerecord.StackRecord(
            Stack: entry.Function is null
                ? default!
                : new uintptr[]{ ((uintptr)global::go.GoSyntheticPC.Of(entry.Function)) }.slice());

        // THE LABEL is the very object the goroutine set: runtime_setProfLabel stores golib's slot
        // and ProfileSnapshot hands it back, so this is the unsafe.Pointer SetGoroutineLabels minted
        // with FromPinnedBox -- whose number, for a reference-bearing box such as labelMap, is the
        // box's registered order TOKEN (Q44), not an address. The block beneath this function
        // records why it was withheld before that and what re-entering it measured.
        if (writeLabels) {
            labels[i] = (entry.Labels as @unsafe.Pointer)!;
        }
    }

    return (n, true);
}

// WHY THE LABELS WERE WITHHELD FOR A DAY, AND WHAT RE-ENTERING THEM MEASURED (2026-09-04 -> 2026-09-05)
//
// The registry HAS every goroutine's labels and hands them to this function; what it could not do
// until Q44 was get them ACROSS this seam safely. The consumer reads a label the way
// runtimeProfile.Label does -- through the NUMBER: `(*labelMap)(p.labels[i])`, i.e.
// `(ж<labelMap>)(uintptr)`. `SetGoroutineLabels` produces that number with
// `unsafe.Pointer.FromPinnedBox(ctxLabels)`, and the reverse conversion used to recover a box that
// ALIASED the raw address (ж.cs). So a label survived the round trip exactly as long as the
// labelMap the address pointed at did not move.
//
// It moved. Instrumented on BOTH sides of the seam in one run of TestGoroutineCounts (2026-09-04,
// SUB-Q27): 192 entries, 101 unlabelled, 91 labelled. For 90 of the 91 the address still pointed
// at the right map when the profile read it (`len == 1`). For ONE -- the FINALIZER goroutine's,
// set from inside the finalizer body by pprof.Do -- the SAME number read `len == 1` at the instant
// runtime_setProfLabel stored it and `len == 1885431144` when the profile read it back, with two
// runtime.GC() calls in between. printCountProfile asks that map for its length to size a slice,
// so the process died with OutOfMemoryException inside labelMap.String, and the row was classified
// `infrastructure-error` -- a HOST DEFECT, not a verdict at all. This file's own header argues that
// making a row unmeasurable is the worse outcome, and that judgement withheld the labels: the
// profile reported the half it could guarantee.
//
// A REFUTED FIRST ATTEMPT, kept because it is the reason the mechanism above is stated so narrowly.
// The first version of this file tried to hand back only labels whose pointer still "resolved to
// managed storage", testing the recovered box's IsNative. That test rests on a false premise --
// IsNative was the NORMAL state for a pointer minted by FromPinnedBox, not evidence of a stale one
// -- and it dropped ALL 91 labels rather than the one bad one. The tell was the guard's own warning
// count: 182 drops (91 labelled goroutines across the two profile calls) where at most one was
// expected. So there was no cheap consumer-side test that separated a live address from a dead
// one; that was the whole difficulty, and it is why nothing is filtered here.
//
// WHAT FIXED IT, AND WHERE. Not this file: the address going stale under GC belonged to golib's
// address model. Q44 (DESIGN-managed-pointer-token.md) changed what `(uintptr)box` answers for a
// REFERENCE-BEARING pointee -- labelMap wraps a map, so its StandardBox has no pinnable slot -- from
// a movable field's address to the box's own registered order TOKEN, which `(ж<T>)(uintptr)`
// resolves back to that very box (ManagedPointerTokens.Resolve) for as long as anything holds it:
// here the goroutine's slot and this labels slice both hold the unsafe.Pointer, which retains the
// box. A token cannot go stale across a collection because it is not an address. With that in
// place the re-entry is the one line the 2026-09-04 note predicted: `labels[i]` filled from
// `entry.Labels`, under Go's own length guard.
//
// MEASURED AT THAT UNION (2026-09-05, gated `^(TestGoroutineCounts|TestGoroutineProfileLabelRace)$`,
// Release, tiering off, oracle go1.23.12 on linux): TestGoroutineCounts PASS in 10.7 s with its
// label half reached -- the same run that died in labelMap.String the day before -- and
// TestGoroutineProfileLabelRace, the host-fatal HANG whose /reset subtest spun until the package
// deadline (182 s) because the substring it waited for was a label, PASS in 58 ms (/reset 42 ms,
// /churn 6 ms); 4/4 verdicts matching. Its host-fatal disclosure entry is retired by that
// measurement (go2cs_test_disclosures.json keeps the record).

} // end pprof_package
