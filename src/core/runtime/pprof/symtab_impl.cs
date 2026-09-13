// symtab_impl.cs - Gbtc
// Copyright © 2026 The go2cs Authors. All rights reserved.
//
// SPDX-License-Identifier: BSD-3-Clause
// Use of this source code is governed by a BSD-style license
// that can be found in the LICENSE file.

// runtime_expandFinalInlineFrame — the linkname destination runtime/symtab.go pushes into
// runtime/pprof, standing in for a push this corpus does not perform.
//
// WHY IT NEEDED A BODY AT ALL
//   Every linkname destination declared in this package is a bodyless partial (runtime.cs's five,
//   pprof.cs's eight), so PartialStubGenerator gives each one a throwing stub and nothing forwards.
//   That is tolerable for a function nothing calls. This is not one: appendLocsForStack calls it on
//   the FIRST line of every stack it encodes (proto.cs:396), so the stub takes down every test that
//   builds a profile at all — reported as `infrastructure-error`, which is not even a verdict.
//
// WHY `return stk` IS GO'S OWN ANSWER AND NOT AN APPROXIMATION
//   Read runtime's implementation (symtab.cs) and it has three early returns before it expands
//   anything:
//
//       if len(stk) == 0            -> return stk
//       f := findfunc(tracepc); if !f.valid()  -> return stk   // "Not a Go function."
//       if !u.isInlined(uf)         -> return stk   // "Nothing inline at tracepc."
//
//   In this corpus the SECOND one fires for every pc that will ever be passed. findfunc walks the
//   module list and skips any module whose pclntable is empty (symtab.cs:268); the only moduledata
//   here is a permanent empty stub, so the search cannot succeed and f is never valid. Running
//   runtime's code would therefore return stk — this body returns the same value by the same
//   reasoning, without depending on a cross-assembly `internal` it cannot reach.
//
// AND IT STAYS RIGHT WHEN THE REGISTRY'S READ-BACK LANDS — which is the reason it is a LOCAL body
// rather than a forward. Once findfunc resolves synthetic PCs, forwarding would become actively
// dangerous: f would validate, and runtime's code would walk on to newInlineUnwinder to read an
// inline tree that does not exist. A synthetic PC has none BY CONSTRUCTION — the registry answers
// "which function", never "which instruction", and DESIGN-synthetic-pc-registry.md §8 refuses
// inlining trees explicitly — so "nothing inline at tracepc" is permanently true for every pc this
// package can hand us, real or synthetic. The third early return is the one that would fire then,
// and it returns stk as well.
//
// WHAT THIS DOES NOT CLAIM
//   Not that inline expansion works. It does not, and cannot without a pclntab. A profile built
//   here attributes a sample to the physical frame rather than to the inlined callers Go would
//   name — a fidelity limit, stated here so it is not rediscovered as a defect.

[module: go.GoManualConversion]

namespace go.runtime;

using runtime = runtime_package;

partial class pprof_package
{
    // Expands the final pc in stk to include all "callers" if pc is inline. Nothing is inline here,
    // for the reasons above, so the stack is returned unchanged — Go's own "Not a Go function" and
    // "Nothing inline at tracepc" branches, which are the two that can fire in this corpus.
    internal static partial slice<uintptr> runtime_expandFinalInlineFrame(slice<uintptr> stk) => stk;

    // ---- The two symbolization destinations, and both are Go's own branch ----

    // Go's body is:
    //     if !f.funcInfo.valid() { return f.Function }
    //     u, uf := newInlineUnwinder(f.funcInfo, f.PC); return u.srcFunc(uf).name()
    // The inline half exists to give a generic function its unsubstituted symbol name. `funcInfo` is
    // never valid in this corpus — findfunc skips every module with an empty pclntable and the only
    // moduledata is a permanent empty stub — so the FIRST branch is the one that fires, exactly as it
    // does for runtime_expandFinalInlineFrame above. Returning f.Function is Go's answer here, not an
    // approximation of it.
    internal static partial @string runtime_FrameSymbolName(ж<runtime.Frame> f) => f.Value.Function;

    // Go's body is `return f.startLine`, unconditionally. Two facts make 0 the EXACT answer rather
    // than a stand-in: `Frame.startLine` is `internal` to the runtime assembly, so this one cannot
    // read it across the boundary at all; and nothing in the corpus ever ASSIGNS it — a census of
    // `startLine` writes finds exactly one occurrence corpus-wide and it is this function's own
    // caller reading the value back (pprof/proto.cs:633). The field is therefore always its zero
    // value, and 0 is what Go's body would return.
    //
    // It is honest for the same reason the frame carries no file or line: a synthetic PC knows WHICH
    // FUNCTION and not which instruction, so there is no start line to know. When a consumer measures
    // a need for one, the converter's per-method record is the route — see DESIGN-pc-readback.md §3.1.
    internal static partial nint runtime_FrameStartLine(ж<runtime.Frame> f) => 0;
}
