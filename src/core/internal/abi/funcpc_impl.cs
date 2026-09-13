// funcpc_impl.cs - Gbtc
// Copyright © 2026 The go2cs Authors. All rights reserved.
//
// SPDX-License-Identifier: BSD-3-Clause
// Use of this source code is governed by a BSD-style license
// that can be found in the LICENSE file.

using System;
using System.Reflection;

namespace go.@internal;

// ---------------------------------------------------------------------------------------------
// THE TWO ENTRY POINTS INTO THE SYNTHETIC-PC REGISTRY.
// Design record: docs/phase4/DESIGN-synthetic-pc-registry.md.
//
// WHAT THESE USED TO DO, AND WHY IT WAS THE WORST POSSIBLE ANSWER
//   Both bodies were `return default` — every function's PC was 0. That compiles, returns a
//   plausible value, and is wrong, which is exactly why the hole stayed invisible for the life of
//   the corpus: `internal/abi`'s own TestFuncPC compared FuncPCABI0(FuncPCTestFn) against
//   FuncPCTestFnAddr and PASSED, because both were zero. Go passes the same test for the opposite
//   reason — a real assembly address on both arms — so the verdicts matched and the row banked. A
//   silent wrong answer that agrees with the oracle by coincidence is the failure mode this file
//   now exists to prevent.
//
// HOW THE ARGUMENT ARRIVES
//   `any` is System.Object and every call site passes a bare method group — `FuncPCABI0(clone)`,
//   `FuncPCABIInternal(chansend)`. Since C# 10 that is a NATURAL DELEGATE TYPE conversion (the
//   compiler reports CS8974 at each site), so `f` is a real System.Delegate and `f.Method` is the
//   target's MethodInfo. That is what lets the whole mechanism live in a body rather than needing
//   a converter change.
//
// THE THREE CLASSES, AND WHY ONLY ONE OF THEM GETS A NUMBER
//   A — a converted Go function with a managed body. It exists, it can be symbolized, and a
//       synthetic PC is exactly the right answer. This is the population the registry serves.
//   B — darwin's dylib trampolines, whose result is CALLED. They need a real code address from
//       NativeLibrary.GetExport, not a token; a token would be fatal the moment it is invoked.
//   C — Go's own assembly routines (goexit, mstart, sigtramp, methodValueCall, ...). There is no
//       dylib to resolve them from and no managed equivalent to point at. Nothing honest exists.
//
//   B and C share the only property visible here — no managed body — so this file cannot tell them
//   apart, and deliberately does not try: it refuses both, loudly, and darwin's resolution arm slots
//   in later keyed on the cgo_import_dynamic pragma map. A number returned to either would be the
//   `return default` defect wearing better clothes.
// ---------------------------------------------------------------------------------------------

partial class abi_package
{
    // Implementation of FuncPCABI0
    public static partial uintptr FuncPCABI0(any f) => FuncPC(f, nameof(FuncPCABI0));

    // Implementation of FuncPCABIInternal
    public static partial uintptr FuncPCABIInternal(any f) => FuncPC(f, nameof(FuncPCABIInternal));

    private static uintptr FuncPC(any f, string entryPoint)
    {
        // This arm stays a plain exception on purpose, and the asymmetry with the panic below is
        // the point: a call site handing FuncPC something that is not a func is a CONVERTER defect,
        // which is exactly what `infrastructure-error` is for. A missing body is not.
        if (f is not Delegate target)
        {
            throw new ArgumentException(
                $"{entryPoint}: expected a func value, got {(f is null ? "nil" : f.GetType().FullName)}", nameof(f));
        }

        MethodInfo method = target.Method;

        // A PANIC, and not a plain exception, for two independent reasons — the second is the one
        // that nearly went the other way.
        //
        // Mechanically: the test host classifies a non-panic exception escaping a test body as
        // `infrastructure-error` (TestExecution.Execute's last arm), and a disclosure absorbs a
        // verdict of exactly `fail` (testConversion.go, matchTerminalStatuses). An
        // infrastructure-error is therefore unbankable — it cannot be disclosed, only left as a
        // mismatch.
        //
        // Honestly: that classification would ALSO be a lie. `InfrastructureFailed` means "a host
        // defect" — the comment above that arm says so — and there is no host defect here. The
        // host is fine; this corpus simply has no code address for a function written in assembly.
        // That is a property of the port, which is what a panic reports and what the
        // `runtime-capability` disclosure class records. The convenient answer and the correct one
        // agree, but they were checked separately.
        //
        // Go itself has no runtime behaviour to model: a bad FuncPC argument is a COMPILE error
        // there. So this is not "what Go does" — it is the honest report of a limit Go never has.
        //
        // The message names the function, because it is the signature a disclosure matches on.
        // Class B slots in AHEAD of the refusal, exactly where the header says it would: a darwin dylib
        // trampoline IS an external stub (no managed body), so the discriminator is not the marker
        // but the `//go:cgo_import_dynamic` record the class-B pass emitted for it. When one exists
        // the answer is the exported symbol's REAL address — dereferenced by design, since the
        // keystone jumps to it — and a token here would be the `return default` defect at the first
        // call. No record means class C, and the refusal below is the honest answer. A record whose
        // library or symbol does not resolve throws from TryResolve, naming both: never a zero.
        if (GoCgoDynamicImports.TryResolve(method, out nint exportAddress))
        {
            return (nuint)exportAddress;
        }

        if (method.IsDefined(typeof(GoExternalStubAttribute), inherit: false))
        {
            throw panic(
                $"{entryPoint}: no program counter exists for {GoSyntheticPC.GoNameOf(method)} — " +
                "it is an external (assembly or cgo) function with no managed body in this corpus");
        }

        return GoSyntheticPC.Of(method);
    }
}
