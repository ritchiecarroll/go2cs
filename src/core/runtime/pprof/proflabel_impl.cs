// proflabel_impl.cs - Gbtc
// Copyright © 2026 The go2cs Authors. All rights reserved.
//
// SPDX-License-Identifier: BSD-3-Clause
// Use of this source code is governed by a BSD-style license
// that can be found in the LICENSE file.

// Per-goroutine profile labels — the managed realization of `getg().labels`.
//
// runtime/pprof.go declares the pair as linkname pulls from the runtime:
//
//     //go:linkname runtime_setProfLabel runtime/pprof.runtime_setProfLabel
//     //go:linkname runtime_getProfLabel runtime/pprof.runtime_getProfLabel
//
// so runtime/pprof/runtime.cs carries them as BODYLESS partials, which take throwing
// PartialStubGenerator stubs. Every caller — SetGoroutineLabels, Do, and the suite's own read-back
// — died there. The runtime-side definitions in runtime/proflabel.cs are faithful conversions and
// die one level deeper, in `getg()`.
//
// WHY THIS DID NOT IMPLEMENT getg() — AND WHERE getg() LIVES NOW (2026-09-05, Q47). This file once
// argued that giving getg() a managed body to reach two functions would be the wrong trade: 574
// referencing sites, a `g` modelling a fraction of what Go's carries, quiet partial behaviour in
// place of loud throws. The Linux runtime row's bill (2026-09-04) and Q40's reader census answered
// it on the numbers — the 574 is a token count over three per-GOOS folders (266 emitted call sites
// per flavour, 280 readers in Go), 202 of those 280 read `gp.m` FIRST so a bare `g` is loud and
// anonymous rather than quiet, and 47 of the row's 378 rows died on the stub — so getg() has a body
// since Q47: a `g` AND the `m` that is its thread, thread-static-cached, in runtime/stubs_impl.cs
// (DESIGN-managed-getg.md §6). The decision THIS file made is unchanged: the labels' storage stays
// here and in golib's mirror, and the managed `g.labels` READS the mirror on every call — it is the
// one H field programs mutate, and the mirror remains the source of truth.
//
// WHERE THE STORAGE LIVES, AND WHY IT MOVED (2026-09-04, Q27). It was an AsyncLocal on this class.
// The mechanism is unchanged and the reasoning below still holds in full -- it simply lives in
// golib now (`Goroutine.SetProfileLabels`/`GetProfileLabels`), because a GOROUTINE PROFILE reads
// every goroutine's labels from ONE thread and an AsyncLocal is readable only from the thread whose
// ExecutionContext holds it. golib keeps the same AsyncLocal as the authoritative per-goroutine
// storage AND mirrors the value onto the registry entry, seeded at Goroutine.Enter from the flowed
// context. These two functions became forwarders; their contract, and the guard that measures it,
// did not move.
//
// WHY AsyncLocal AND NOT [ThreadStatic]. golib gives every goroutine its own dedicated thread, so
// [ThreadStatic] looks exact here. It is wrong, and Go's own scheduler says why:
//
//     proc.go:5097   // Only user goroutines inherit pprof labels.
//     proc.go:5099   newg.labels = mp.curg.labels
//
// Labels are INHERITED at goroutine creation and independent thereafter. A [ThreadStatic] gives a
// spawned goroutine none, which is a silent wrong answer that passes every test that does not spawn
// under a label — and spawning under a label is the entire point of pprof.Do. AsyncLocal<T> under a
// flowing ExecutionContext expresses Go's rule exactly: the value is CAPTURED at thread start and
// later writes on either side do not cross, which is `newg.labels = mp.curg.labels` and then
// separate storage.
//
// That the flow actually happens is MEASURED, not read off Goroutine.Start's comment:
// GolibTests/GoroutineExecutionContextFlowTests covers inheritance through golib's real spawn
// primitive, independence of the child's writes, and — the control that makes the first mean
// anything — that a SUPPRESSED-flow spawn does NOT inherit. If golib's spawn path ever moves to
// Thread.UnsafeStart, ThreadPool.UnsafeQueueUserWorkItem, or wraps spawning in
// ExecutionContext.SuppressFlow, that guard goes red and these labels silently stop inheriting.
//
// SCOPE, stated because it is narrower than the Go original. Go stores labels on the g, where the
// SIGPROF handler also reads them (proc.go:5564, `tagPtr = &gp.m.curg.labels`) to tag CPU samples.
// This storage serves the pprof API surface — SetGoroutineLabels, Do, Labels, and the suite's
// read-back — and the managed corpus has no signal-based sampling profiler to read it from. When
// one exists it reads from here; nothing else changes.
//
// This file has no `<name>.go` counterpart, so a -stdlib reconvert never emits over it; the module
// marker states the ownership explicitly.

[module: go.GoManualConversion]

namespace go.runtime;

using @unsafe = unsafe_package;

partial class pprof_package {

// runtime_setProfLabel: `getg().labels = labels`, on managed storage.
//
// Go's body also takes a race edge on labelSync under raceenabled (proflabel.go), so that
// profBuf.read synchronizes with all prior setProfLabel operations. There is no race detector in
// this corpus and the converted runtime's raceenabled is constant false, so the edge has nothing to
// carry and is deliberately not modeled — the same reasoning syscall_linux_impl.cs's
// entersyscall/exitsyscall pair records.
//
// `unsafe.Pointer` is a CLASS in this corpus, so golib's `object?` slot holds it by reference: null
// is nil, nothing is boxed, and the reference keeps the pinned labelMap alive for as long as the
// goroutine is labelled — which is what `FromPinnedBox` needs and what a raw address could not
// promise.
internal static partial void runtime_setProfLabel(@unsafe.Pointer labels) {
    global::go.golib.Goroutine.SetProfileLabels(labels);
}

// runtime_getProfLabel: `return getg().labels`.
//
// An unset slot answers null, which is the converted nil unsafe.Pointer — Go returns
// unsafe.Pointer(nil) for a goroutine that has never been labelled, and pprof's read-back
// (`(*labelMap)(runtime_getProfLabel())`) is a nil-tolerant conversion on both sides.
internal static partial @unsafe.Pointer runtime_getProfLabel() {
    return (global::go.golib.Goroutine.GetProfileLabels() as @unsafe.Pointer)!;
}

} // end pprof_package
