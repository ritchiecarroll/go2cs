// GoThreadState.cs - Gbtc
// Copyright © 2026 The go2cs Authors. All rights reserved.
//
// Use of this source code is governed by an MIT-style license
// that can be found in the LICENSE file.

using System;
using System.Runtime.CompilerServices;
using System.Runtime.ExceptionServices;

namespace go;

/// <summary>
/// One thread's panic and defer state: the panic slots <see cref="GoFuncRoot"/> exposes and the panic
/// sequences <see cref="GoFrame.Run"/> keeps for runtime's captureCallers, held in ONE per-thread object.
/// </summary>
/// <remarks>
/// <para>
/// WHY ONE HOLDER. Every deferring <see cref="GoFrame.Run"/> reads and writes this state, on the normal
/// return too: the claim, the handled and recoverable panics (saved and restored), and the sequence depth.
/// As five <c>ThreadLocal&lt;T&gt;</c> slots plus a <c>[ThreadStatic]</c> depth, each access paid its own
/// thread-local lookup, and <c>ThreadLocal.Value</c> is by far the costlier kind; the depth alone measured
/// about 2.3 ns per deferring call on <c>mu.Lock(); defer mu.Unlock()</c>, Go's commonest idiom. Here a
/// Run fetches the holder once and works on plain fields.
/// </para>
/// <para>
/// PER THREAD, exactly as the <c>ThreadLocal</c> slots were: a goroutine is a thread, a coroutine's body
/// runs on its own coro thread with its own holder, and a pooled thread's next goroutine starts clean
/// (<see cref="ResetThread"/>, from <see cref="GoFuncRoot.ResetThread"/>). A caller may keep the holder in
/// a local across calls it makes: synchronous code never changes threads under it.
/// </para>
/// </remarks>
internal sealed class GoThreadState
{
    [ThreadStatic] private static GoThreadState? t_current;

    /// <summary>This thread's holder, created on first use.</summary>
    internal static GoThreadState Current
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        get => t_current ?? Create();
    }

    [MethodImpl(MethodImplOptions.NoInlining)]
    private static GoThreadState Create() => t_current = new GoThreadState();

    // The panic slots (see GoFuncRoot for what each one means).
    internal PanicException? CapturedPanic;
    internal PanicException? HandledPanic;
    internal PanicException? UnclaimedPanic;
    internal ExceptionDispatchInfo? InFlightForeign;
    internal PanicException? RecoverablePanic;

    // Set just before a deferred call whose callee calls recover() runs, and taken (read and cleared) by that
    // callee's first line: Go's "called directly by the deferred function" rule (GoFrame.TakeDeferredCall).
    internal bool DeferredCallPending;

    // The panic sequences (see GoFrame): how many deferring Runs are live on this thread, and the entries a
    // panic writes, allocated on the first panic and reused.
    internal int SequenceDepth;
    internal GoFrame.Sequence[]? Sequences;

    /// <summary>Clears this thread's state before a pooled thread runs its next goroutine; a thread with no holder has nothing to clear.</summary>
    internal static void ResetThread() { if (t_current is { } state) state.Reset(); }

    private void Reset()
    {
        CapturedPanic = null;
        HandledPanic = null;
        UnclaimedPanic = null;
        InFlightForeign = null;
        RecoverablePanic = null;
        DeferredCallPending = false;
        SequenceDepth = 0;

        if (Sequences is { } entries)
            Array.Clear(entries);
    }
}
