// Copyright 2022 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.
using System.Runtime.ExceptionServices;

[module: go.GoManualConversion]
namespace go;

partial class sync_package {

// Hand-owned (exec-wall design OQ-6, replay half — JOB-024, 2026-08-26). The converted OnceX
// wrappers guard f with Go's own `defer { p = recover(); if !valid { panic(p) } }`, and the
// frame-level foreign-unwind correction (GoFrame.Run, cc6454f5e) already preserves a FOREIGN
// (.NET) exception's identity through that defer on the FIRST call. But Go's contract replays the
// panic VALUE on every LATER call — `if !valid { panic(p) }` in the returned closure — and during
// a foreign unwind recover() stored nil into p, so every later caller re-panicked `panic: nil`
// with no trace of the original. JOB-024 measured that shape killing the os/exec host: 1.23.12's
// TestConcurrentExec fans callers out on goroutines, one gets the preserved original, every other
// goroutine gets the nil replay, and an unrecovered goroutine panic is process-fatal.
//
// This hand-own extends Go's replay contract to the interop reality, disturbing nothing else:
//
//   - The unwind classes are RECORDED by an exception FILTER that always declines the catch, so
//     the first call's propagation — Go panic, adopted runtime fault, or foreign exception — is
//     byte-for-byte what the caller saw before: same object, same stack, first pass uninterrupted.
//   - A GO panic replays exactly as the converted code replayed it: `throw panic(p)` with the
//     recovered value, on every later call.
//   - A FOREIGN exception replays as ITSELF: the captured ExceptionDispatchInfo re-throws the
//     original exception, identity intact, on every later call — where the converted shape
//     replayed `panic(nil)`.
//   - runtime.Goexit records nothing and unwinds freely, exactly as it fails GoFrame.IsPanic and
//     the foreign-preservation filter today; a later call after a Goexit-abandoned once replays
//     the nil panic the converted shape produced (Go itself leaves this corner undefined).
//
// Guarded by GolibTests/OnceForeignExceptionTests (the replay guards fail against the converted
// shape and pass against this one; the Go-panic control passes against both).
//
// f is cleared in a `finally`, the analogue of Go's defer, so it is released after the first call on
// the panicking path too: Go's `d.f = nil` sits in the deferred func. The filter still runs first (it
// runs in the first pass of the unwind, before any finally), so it still sees the panic. Clearing f
// only after it returned kept a panicking f alive for the life of the returned func: Go 1.26's sync
// TestOnceXGC/*_panic, and the DROP guards in the same test file.

// OnceFunc returns a function that invokes f only once. The returned function
// may be called concurrently.
//
// If f panics, the returned function will panic with the same value on every call.
public static Action OnceFunc(Action f) {
    ref var once = ref heap(new Once(), out var Ꮡonce);
    bool valid = false;
    object? p = null;
    ExceptionDispatchInfo? foreignUnwind = null;

    bool recordUnwind(Exception ex) {
        if (GoFrame.IsPanic(ex, out PanicException? gp)) {
            p = gp.State;
        }
        else if (ex is not GoexitException) {
            foreignUnwind = ExceptionDispatchInfo.Capture(ex);
        }
        return false; // never catch — the first call's unwind continues with its identity intact
    }

    // Construct the inner closure just once to reduce costs on the fast path.
    var g = () => {
        try {
            f();
            valid = true; // Set only if f does not panic.
        }
        catch (Exception ᒐex) when (recordUnwind(ᒐex)) { throw; } // unreachable: the filter declines
        finally {
            f = default!; // Do not keep f alive after invoking it. Go clears it in the defer, so on a panic too.
        }
    };
    var gʗ1 = g;
    return () => {
        Ꮡonce.Do(gʗ1);
        if (!valid) {
            if (foreignUnwind is { } fe) {
                fe.Throw(); // replay the ORIGINAL foreign exception, identity intact
            }
            throw panic(p!);
        }
    };
}

// OnceValue returns a function that invokes f only once and returns the value
// returned by f. The returned function may be called concurrently.
//
// If f panics, the returned function will panic with the same value on every call.
public static Func<T> OnceValue<T>(Func<T> f) {
    ref var once = ref heap(new Once(), out var Ꮡonce);
    bool valid = false;
    object? p = null;
    ExceptionDispatchInfo? foreignUnwind = null;
    T result = default!;

    bool recordUnwind(Exception ex) {
        if (GoFrame.IsPanic(ex, out PanicException? gp)) {
            p = gp.State;
        }
        else if (ex is not GoexitException) {
            foreignUnwind = ExceptionDispatchInfo.Capture(ex);
        }
        return false; // never catch — the first call's unwind continues with its identity intact
    }

    var g = () => {
        try {
            result = f();
            valid = true;
        }
        catch (Exception ᒐex) when (recordUnwind(ᒐex)) { throw; } // unreachable: the filter declines
        finally {
            f = default!;
        }
    };
    var gʗ1 = g;
    return () => {
        Ꮡonce.Do(gʗ1);
        if (!valid) {
            if (foreignUnwind is { } fe) {
                fe.Throw(); // replay the ORIGINAL foreign exception, identity intact
            }
            throw panic(p!);
        }
        return result;
    };
}

// OnceValues returns a function that invokes f only once and returns the values
// returned by f. The returned function may be called concurrently.
//
// If f panics, the returned function will panic with the same value on every call.
public static Func<(T1, T2)> OnceValues<T1, T2>(Func<(T1, T2)> f) {
    ref var once = ref heap(new Once(), out var Ꮡonce);
    bool valid = false;
    object? p = null;
    ExceptionDispatchInfo? foreignUnwind = null;
    T1 r1 = default!;
    T2 r2 = default!;

    bool recordUnwind(Exception ex) {
        if (GoFrame.IsPanic(ex, out PanicException? gp)) {
            p = gp.State;
        }
        else if (ex is not GoexitException) {
            foreignUnwind = ExceptionDispatchInfo.Capture(ex);
        }
        return false; // never catch — the first call's unwind continues with its identity intact
    }

    var g = () => {
        try {
            (r1, r2) = f();
            valid = true;
        }
        catch (Exception ᒐex) when (recordUnwind(ᒐex)) { throw; } // unreachable: the filter declines
        finally {
            f = default!;
        }
    };
    var gʗ1 = g;
    return () => {
        Ꮡonce.Do(gʗ1);
        if (!valid) {
            if (foreignUnwind is { } fe) {
                fe.Throw(); // replay the ORIGINAL foreign exception, identity intact
            }
            throw panic(p!);
        }
        return (r1, r2);
    };
}

} // end sync_package
