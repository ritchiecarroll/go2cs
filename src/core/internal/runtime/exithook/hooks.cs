// Copyright 2024 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.

// Package exithook provides limited support for on-exit cleanup.
//
// CAREFUL! The expectation is that Add should only be called
// from a safe context (e.g. not an error/panic path or signal
// handler, preemption enabled, allocation allowed, write barriers
// allowed, etc), and that the exit function F will be invoked under
// similar circumstances. That is the say, we are expecting that F
// uses normal / high-level Go code as opposed to one of the more
// restricted dialects used for the trickier parts of the runtime.
namespace go.@internal.runtime;

using atomic = go.@internal.runtime.atomic_package;
// blank import: unsafe_package (side effects only; no using emitted — a `using _` alias hijacks C# discards) // for linkname
using go.@internal.runtime;

partial class exithook_package {

// A Hook is a function to be run at program termination
// (when someone invokes os.Exit, or when main.main returns).
// Hooks are run in reverse order of registration:
// the first hook added is the last one run.
[GoType] partial struct Hook {
    public Action F; // func to run
    public bool RunOnFailure;   // whether to run on non-zero exit code
}

internal static ж<atomic.Int32> Ꮡlocked = new StandardBox<atomic.Int32>(default(atomic.Int32));
internal static ref atomic.Int32 locked => ref Ꮡlocked.Value;
internal static ж<atomic.Uint64> ᏑrunGoid = new StandardBox<atomic.Uint64>(default(atomic.Uint64));
internal static ref atomic.Uint64 runGoid => ref ᏑrunGoid.Value;
internal static slice<Hook> hooks;
internal static bool running;
public static Action Gosched;
public static Func<uint64> Goid;
public static Action<@string> Throw;

// Add adds a new exit hook.
public static void Add(Hook h) {
    while (!Ꮡlocked.CompareAndSwap(0, 1)) {
        Gosched();
    }
    hooks = append(hooks, h);
    Ꮡlocked.Store(0);
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
private static readonly @string exitHookInvokedExitˢ = "exit hook invoked exit"u8;
private static readonly @string exitHookInvokedPanicˢ = "exit hook invoked panic"u8;

// Run runs the exit hooks.
//
// If an exit hook panics, Run will throw with the panic on the stack.
// If an exit hook invokes exit in the same goroutine, the goroutine will throw.
// If an exit hook invokes exit in another goroutine, that exit will block.
public static void Run(nint code) {
    GoFrame ᒐ = default;
    try {
        while (!Ꮡlocked.CompareAndSwap(0, 1)) {
            if (Goid() == ᏑrunGoid.Load()) {
                Throw(exitHookInvokedExitˢ);
            }
            Gosched();
        }
        defer(Ꮡlocked.Store, (int32)(0), ref ᒐ);
        ᏑrunGoid.Store(Goid());
        defer(ᏑrunGoid.Store, (uint64)(0), ref ᒐ);
        defer(() => {
            {
                var e = recover(); if (e != default!) {
                    Throw(exitHookInvokedPanicˢ);
                }
            }
        }, ref ᒐ);
        while (len(hooks) > 0) {
            var h = hooks[len(hooks) - 1];
            hooks = hooks.slice(0, len(hooks) - 1);
            if (code != 0 && !h.RunOnFailure) {
                continue;
            }
            h.F();
        }
    }
    catch (Exception ᒐex) when (GoFrame.IsPanic(ᒐex, out PanicException? ᒐp)) { GoFrame.Capture(ᒐp); }
    finally { ᒐ.Run(); }
}

[GoType("@string")] partial struct exitError;

internal static @string Error(this exitError e) {
    return ((@string)e);
}

} // end exithook_package
