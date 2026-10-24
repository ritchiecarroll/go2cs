// CrashChainProbe.cs - Gbtc
// Copyright © 2026 The go2cs Authors. All rights reserved.
//
// Use of this source code is governed by an MIT-style license
// that can be found in the LICENSE file.

using System;
using System.Runtime.CompilerServices;
using go.golib;
using static go.builtin;

namespace go;

// Converted-shaped functions for CrashReportTests' chain arms: a package class runtime reads as Go source, each body
// the converter's own emission for the Go beside it.
internal static partial class crashchainprobe_package
{
    internal static void noop() { }

    // func cleanup() { defer noop(); panic("from cleanup") }
    [MethodImpl(MethodImplOptions.NoInlining)]
    internal static void cleanup()
    {
        GoFrame ᒐ = default;
        try {
            defer(noop, ref ᒐ);
            throw panic("from cleanup");
        }
        catch (Exception ᒐex) when (GoFrame.IsPanic(ᒐex, out PanicException? ᒐp)) { GoFrame.Capture(ᒐp); }
        finally { ᒐ.Run(); }
    }

    // func deferNamed() { defer cleanup(); panic("first") } -- the ACCEPTED shape: the deferred function defers and
    // panics itself, so it catches its own panic first.
    [MethodImpl(MethodImplOptions.NoInlining)]
    internal static void deferNamed()
    {
        GoFrame ᒐ = default;
        try {
            defer(cleanup, ref ᒐ);
            throw panic("first");
        }
        catch (Exception ᒐex) when (GoFrame.IsPanic(ᒐex, out PanicException? ᒐp)) { GoFrame.Capture(ᒐp); }
        finally { ᒐ.Run(); }
    }

    // func deferClosure() { defer func() { r := recover(); panic(r) }(); panic("first") } -- C2's shape: the closure has
    // no frame of its own, so the deferring function's Run catches its panic first, and splicePanic refuses it (a
    // closure and a converter defer wrapper look alike at run time).
    [MethodImpl(MethodImplOptions.NoInlining)]
    internal static void deferClosure()
    {
        GoFrame ᒐ = default;
        try {
            defer(() => {
                var r = recover();
                throw panic(r);
            }, ref ᒐ);
            throw panic("first");
        }
        catch (Exception ᒐex) when (GoFrame.IsPanic(ᒐex, out PanicException? ᒐp)) { GoFrame.Capture(ᒐp); }
        finally { ᒐ.Run(); }
    }

    // ---- S1: the frames below the first deferring frame ----

    // func s1f() { defer noop(); panic("x") }
    [MethodImpl(MethodImplOptions.NoInlining)]
    internal static void s1f()
    {
        GoFrame ᒐ = default;
        try {
            defer(noop, ref ᒐ);
            throw panic("x");
        }
        catch (Exception ᒐex) when (GoFrame.IsPanic(ᒐex, out PanicException? ᒐp)) { GoFrame.Capture(ᒐp); }
        finally { ᒐ.Run(); }
    }

    // func s1One() { s1f() } -- ONE deferring frame on the path, so the crash shows the frames below it.
    [MethodImpl(MethodImplOptions.NoInlining)]
    internal static void s1One() => s1f();

    // func s1Mid() { s1f() }
    [MethodImpl(MethodImplOptions.NoInlining)]
    internal static void s1Mid() => s1f();

    // func s1Deep() { s1Mid() } -- one deferring frame, several plain frames below it.
    [MethodImpl(MethodImplOptions.NoInlining)]
    internal static void s1Deep() => s1Mid();

    // func s1g() { defer noop(); s1f() }
    [MethodImpl(MethodImplOptions.NoInlining)]
    internal static void s1g()
    {
        GoFrame ᒐ = default;
        try {
            defer(noop, ref ᒐ);
            s1f();
        }
        catch (Exception ᒐex) when (GoFrame.IsPanic(ᒐex, out PanicException? ᒐp)) { GoFrame.Capture(ᒐp); }
        finally { ᒐ.Run(); }
    }

    // func s1Two() { s1g() } -- TWO deferring frames: the frames between them were never recorded, so the crash keeps
    // its old output.
    [MethodImpl(MethodImplOptions.NoInlining)]
    internal static void s1Two() => s1g();

    // func s1r(n int) { defer noop(); if n == 0 { panic("x") }; s1r(n - 1) }
    [MethodImpl(MethodImplOptions.NoInlining)]
    internal static void s1r(nint n)
    {
        GoFrame ᒐ = default;
        try {
            defer(noop, ref ᒐ);
            if (n == 0) {
                throw panic("x");
            }
            s1r(n - 1);
        }
        catch (Exception ᒐex) when (GoFrame.IsPanic(ᒐex, out PanicException? ᒐp)) { GoFrame.Capture(ᒐp); }
        finally { ᒐ.Run(); }
    }

    // func s1Rec() { s1r(2) } -- three deferring activations of one function.
    [MethodImpl(MethodImplOptions.NoInlining)]
    internal static void s1Rec() => s1r(2);

    // func s1Chain() { deferNamed() } -- the accepted chain, under a caller that does not defer: Go prints cleanup,
    // panic, deferNamed, then s1Chain.
    [MethodImpl(MethodImplOptions.NoInlining)]
    internal static void s1Chain() => deferNamed();
}
