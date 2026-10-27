// RuntimePanicCheck.cs - Gbtc
// Copyright © 2026 The go2cs Authors. All rights reserved.
//
// Use of this source code is governed by an MIT-style license
// that can be found in the LICENSE file.

using System;
using System.Diagnostics;

namespace go.golib;

/// <summary>
/// Go's <c>panicCheck1</c>: a bounds or shift panic raised by code IN package <c>runtime</c> is a fatal
/// error, not a panic. <c>recover()</c> never sees it, and the process exits 2.
/// </summary>
/// <remarks>
/// <para>
/// Go's index, slice, slice3 and slice-convert bounds checks and its negative-shift check reach
/// <c>panicCheck1(callerpc, msg)</c>, which throws when the faulting function's name has the
/// <c>"runtime."</c> prefix: the runtime faulting on its own data is a runtime bug, never a value a
/// program should catch. The divide and nil-dereference checks are <c>panicCheck2</c>, which throws only
/// while mallocing, so they stay recoverable here and are never tagged.
/// </para>
/// <para>
/// DECIDED WHERE THE PANIC IS RECOVERED OR REPORTED, NOT WHERE IT IS RAISED. <see cref="RuntimeErrorPanic"/>'s
/// factories for those checks only tag the panic with Go's throw text
/// (<see cref="PanicException.RuntimeThrowText"/>). <c>recover()</c> and the unrecovered-panic report
/// then read the frames the panic already carries (<see cref="PanicException.SiteTrace"/>, the throw
/// site's trace snapshotted once at its first catch), so no stack is walked on any path. A walk at the
/// raise cost about 8 to 25 µs per bounds panic and, under tiered compilation with on-stack replacement,
/// made a .NET runtime fault in its stack walker about ten times as likely (measured, A8, 2026-09-29).
/// </para>
/// <para>
/// RESIDUAL, accepted: Go throws at the raise, so no deferred call runs. Here the deferred calls in the
/// frames between the raise and the recover (or the root) run first, and only then does the process end.
/// And a panic the test host contains (its per-test catch, which reads no recover()) fails that one
/// test as every contained panic does, where Go's test binary would end.
/// </para>
/// <para>
/// The marker is STRUCTURAL. The converter declares every member of package <c>runtime</c> on
/// <c>go.runtime_package</c>, and a <c>-tests</c> build puts package runtime's own <c>_test.go</c> files
/// (Go names them <c>runtime.*</c> too) on the internal-test bridge <c>go.runtime_internal_test_package</c>,
/// in both cases directly or on a type nested in the class. <c>runtime_test_package</c> (package
/// <c>runtime_test</c>) and <c>internal/runtime/*</c> are other classes, which matches Go's prefix
/// exactly. No message text is read and no assembly name is guessed.
/// </para>
/// <para>
/// The raising frame must still EXIST in the trace. A method the JIT inlines into a non-runtime caller
/// hides, and the panic then stays recoverable, which is never a false fatal. Go's <c>//go:noinline</c>
/// is emitted as <c>NoInlining</c>, which keeps the frames Go's own tests rely on.
/// </para>
/// </remarks>
internal static class RuntimePanicCheck
{
    /// <summary>
    /// Terminates the process with Go's fatal report when <paramref name="panic"/> is one of
    /// <c>panicCheck1</c>'s and was raised in package <c>runtime</c>, and returns otherwise.
    /// </summary>
    internal static void FatalIfRaisedInRuntime(PanicException panic)
    {
        if (panic.RuntimeThrowText is { } throwText && RaisedInRuntimePackage(panic.SiteTrace, out _))
            FatalReport.Fatal(throwText, userFault: false);
    }

    /// <summary>
    /// Reports whether the first frame of <paramref name="site"/> outside golib is declared on package
    /// <c>runtime</c>'s class, its internal-test bridge, or a type nested in either, and names that frame.
    /// </summary>
    internal static bool RaisedInRuntimePackage(StackTrace? site, out string frameName)
    {
        frameName = "";

        if (site is null)
            return false;

        string? golib = typeof(RuntimePanicCheck).Assembly.FullName;

        // Names only, through DiagnosticMethodInfo rather than StackFrame.GetMethod(): a frame's method name, its
        // declaring type's name and its assembly's are what a trimmed or Native AOT program keeps for its stack
        // traces, where a MethodBase needs the method's full reflection metadata (IL2026).
        for (int i = 0; i < site.FrameCount; i++)
        {
            if (site.GetFrame(i) is not { } frame ||
                DiagnosticMethodInfo.Create(frame) is not { DeclaringTypeName: { } typeName } method ||
                method.DeclaringAssemblyName == golib)
                continue;

            frameName = $"{typeName}.{method.Name}";
            return IsRuntimePackageType(typeName);
        }

        return false;
    }

    /// <summary>
    /// Reports whether the type whose full name is <paramref name="typeName"/> is package <c>runtime</c>'s
    /// class or its internal-test bridge, or is nested in either.
    /// </summary>
    internal static bool IsRuntimePackageType(string typeName)
    {
        // A nested type's full name is its outermost type's, then '+' and each nested name, so the outermost decides.
        int nested = typeName.IndexOf('+');

        return (nested < 0 ? typeName : typeName[..nested]) is "go.runtime_package" or "go.runtime_internal_test_package";
    }
}
