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
}
