// GoNativeCall.cs - Gbtc
// Copyright © 2026 The go2cs Authors. All rights reserved.
//
// Use of this source code is governed by an MIT-style license
// that can be found in the LICENSE file.

using System.Runtime.CompilerServices;

namespace go.golib;

/// <summary>How a native call is expected to behave, for the scheduler's handoff (Go's entersyscall forms).</summary>
public enum NativeCallHint
{
    /// <summary>Go's <c>entersyscall</c>: the call may block; a long one has its P retaken.</summary>
    MayBlock,

    /// <summary>Go's <c>entersyscallblock</c>: the call is known to block; its P is handed off at once.</summary>
    Blocks
}

/// <summary>
/// The one native-call transition seam: Go's <c>entersyscall</c> / <c>exitsyscall</c>, and <c>cgocallbackg</c>'s pair
/// for a callback, for every native call converted code makes (docs/phase4/DESIGN-native-call-seam.md).
/// </summary>
/// <remarks>
/// <para>
/// <b>Who calls it.</b> A native-call funnel's bottom (or the runtime's entersyscall family, for a funnel whose Go
/// original goes through it) calls <see cref="Enter"/> immediately before the native call and <see cref="Exit"/> in a
/// <c>finally</c> after it. A native-to-managed entry point calls <see cref="ReenterManaged"/> before any Go code runs
/// and <see cref="ReturnToNative"/> in a <c>finally</c> before control returns. A call that cannot block and cannot
/// call back (Go's <c>RawSyscall</c> class, every <c>[SuppressGCTransition]</c> binding) never calls it (§7.1).
/// </para>
/// <para>
/// <b>errno.</b> The caller reads the error the call left (a <c>SetLastError</c> stub, <c>GetLastSystemError</c>)
/// BETWEEN the native return and <see cref="Exit"/>. Exit is free to run managed code, and under M:N to wait for a P
/// (§4.3).
/// </para>
/// <para>
/// <b>Exactly once per transition.</b> Each goroutine keeps one depth: even while it runs managed code, odd while it
/// is inside a native call. <see cref="Enter"/> and <see cref="ReenterManaged"/> add one, <see cref="Exit"/> and
/// <see cref="ReturnToNative"/> subtract one, and each refuses the parity it does not expect. So a transition entered
/// twice (a funnel and the runtime family both entering) fails by name instead of double-counting (COORD, Q2). The
/// depth lives on the goroutine, never in a thread-static (§12.1), and a thread with no goroutine identity is inert.
/// </para>
/// <para>
/// <b>Under today's 1:1 model</b> there is no P to release, so a transition does golib's blocked accounting
/// (the marks <see cref="Goroutine.EnterBlocked"/> / <see cref="Goroutine.ExitBlocked"/> make, which it COMPOSES over:
/// those also mark the execution tracer's waits, which are not native calls) and nothing else. It reads the current
/// goroutine once per transition. The <paramref name="hint"/> of
/// <see cref="Enter"/> is carried for the M:N scheduler and read by nothing yet.
/// </para>
/// </remarks>
public static class GoNativeCall
{
    /// <summary>The calling goroutine enters a native call: Go's <c>entersyscall</c>.</summary>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static void Enter(NativeCallHint hint = NativeCallHint.MayBlock)
    {
        if (Goroutine.Current is not { } goroutine)
            return;

        if ((goroutine.m_nativeDepth & 1) != 0)
            throw OutOfOrder(goroutine, nameof(Enter), "already inside a native call");

        goroutine.m_nativeDepth++;
        goroutine.EnterBlockedOn();
    }

    /// <summary>The native call <see cref="Enter"/> opened has returned: Go's <c>exitsyscall</c>.</summary>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static void Exit()
    {
        if (Goroutine.Current is not { } goroutine)
            return;

        if ((goroutine.m_nativeDepth & 1) == 0)
            throw OutOfOrder(goroutine, nameof(Exit), "not inside a native call");

        goroutine.m_nativeDepth--;
        goroutine.ExitBlockedOn();
    }

    /// <summary>Native code calls back into Go: <c>cgocallbackg</c>'s <c>exitsyscall</c>.</summary>
    public static void ReenterManaged()
    {
        if (Goroutine.Current is not { } goroutine)
            return;

        if ((goroutine.m_nativeDepth & 1) == 0)
            throw OutOfOrder(goroutine, nameof(ReenterManaged), "not inside a native call");

        goroutine.m_nativeDepth++;
        goroutine.ExitBlockedOn();
    }

    /// <summary>The callback <see cref="ReenterManaged"/> opened returns to native code: <c>cgocallbackg</c>'s <c>reentersyscall</c>.</summary>
    public static void ReturnToNative()
    {
        if (Goroutine.Current is not { } goroutine)
            return;

        if ((goroutine.m_nativeDepth & 1) != 0 || goroutine.m_nativeDepth == 0)
            throw OutOfOrder(goroutine, nameof(ReturnToNative), "not inside a callback from a native call");

        goroutine.m_nativeDepth--;
        goroutine.EnterBlockedOn();
    }

    /// <summary>The calling goroutine's transition depth (§12.3), or -1 on a thread with no goroutine identity.</summary>
    public static int TransitionDepth => Goroutine.Current is { } goroutine ? goroutine.m_nativeDepth : -1;

    [MethodImpl(MethodImplOptions.NoInlining)]
    private static PanicException OutOfOrder(Goroutine goroutine, string operation, string state) =>
        new($"go2cs: GoNativeCall.{operation} on goroutine {goroutine.Id}, which is {state} (transition depth {goroutine.m_nativeDepth})");
}
