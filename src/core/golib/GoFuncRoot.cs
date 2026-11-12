// GoFuncRoot.cs - Gbtc
// Copyright © 2026 The go2cs Authors. All rights reserved.
//
// Use of this source code is governed by an MIT-style license
// that can be found in the LICENSE file.

// ReSharper disable CheckNamespace

using System;

namespace go;

/// <summary>
/// Represents the root execution context for all Go functions.
/// </summary>
/// <remarks>
/// The panic slots below live in this thread's <see cref="GoThreadState"/>, one holder per thread, so a
/// deferring <see cref="GoFrame.Run"/> pays one thread-local lookup rather than one per slot. They were
/// five <c>ThreadLocal&lt;T&gt;</c> fields here; the meaning of each is unchanged:
/// <list type="bullet">
/// <item>CAPTURED: the panic a frame's catch captured, shared between all frames on the thread.</item>
/// <item>HANDLED: the panic whose deferred calls are RUNNING on this thread. The captured slot is cleared
/// by recover(), but Go's traceback keeps showing the panicking frames for the rest of the deferred
/// sequence, so the panic being handled is tracked separately, strictly scoped to GoFrame.Run (saved and
/// restored), which is what keeps it from ever going stale.</item>
/// <item>UNCLAIMED: the panic a frame's own catch has just captured and that no GoFrame.Run has CLAIMED
/// yet. This is what makes the re-raise of an unrecovered panic frame-OWNED instead of thread-global:
/// GoFrame.Capture arms this slot and the very next GoFrame.Run on the thread claims it, which is always
/// that same frame's finally, because nothing runs between an emitted catch body and its finally. A frame
/// that caught nothing therefore claims null and leaves an in-flight panic alone, rather than re-raising
/// another frame's panic from the middle of that frame's deferred sequence (see GoFrame.Run).</item>
/// <item>IN-FLIGHT FOREIGN: the most recent FOREIGN (.NET, non-panic, non-Goexit) exception seen
/// unwinding through an emitted frame's IsPanic filter on this thread, preserved with its stack. It exists
/// for one consumer: GoFrame.Run's foreign-unwind correction (exec-wall design OQ-6, ratified 2026-08-22).
/// A deferred `panic(recover())` during a foreign unwind re-panics NIL, because recover() rightly sees no
/// Go panic, and without this slot that nil panic REPLACES the original defect (sync.OnceFunc/OnceValue's
/// guard is the canonical shape: every exec-wall residual behind a OnceValue-guarded probe reported
/// `panic: nil` instead of naming the NotImplementedException underneath). Overwritten by each newer
/// foreign exception, cleared when consumed and when a REAL panic is captured (GoFrame.Capture): a genuine
/// Go panic superseding the unwind is Go's own replacement rule.</item>
/// <item>RECOVERABLE: the panic the deferred sequence RUNNING on this thread may recover, Go's rule that
/// recover() succeeds only in a deferred call the panic sequence itself invoked. GoFrame.Run sets it once
/// per sequence (to the panic it is handling, or null for a normal-return sequence), updates it when a
/// deferred call's panic replaces the one being handled, and restores the outer value on exit. So a
/// recover() inside the defers of a deferred function's OWN normal return reads null, exactly as Go's
/// does (runtime's TestRecoverMatching), and the outer panic is still there for the outer sequence
/// afterwards. docs/phase4/DESIGN-recover-model.md.</item>
/// </list>
/// </remarks>
[EditorBrowsable(EditorBrowsableState.Never)] public class GoFuncRoot
{
    /// <summary>
    /// Clears this thread's panic slots before a pooled thread runs its next goroutine. Each is
    /// frame-scoped and normally empty when a goroutine ends; a goroutine that ends on a Goexit or an
    /// unrecovered unwind can leave one set, and the next goroutine must not see another's panic.
    /// </summary>
    internal static void ResetThread()
    {
        GoThreadState.ResetThread();
        GoFrame.ResetSequences();
        GoexitException.ResetThread();
        PanicException.ResetThread();
    }

    internal static System.Runtime.ExceptionServices.ExceptionDispatchInfo? InFlightForeignException
    {
        get => GoThreadState.Current.InFlightForeign;
        set => GoThreadState.Current.InFlightForeign = value;
    }

    /// <summary>
    /// Gets the panic whose traceback a <c>runtime.Stack</c>/<c>debug.Stack</c> call on this thread
    /// should report — the one being handled by an enclosing deferred sequence, else one caught and
    /// not yet recovered. Null when no panic is in flight.
    /// </summary>
    /// <remarks>
    /// Go keeps a panicking goroutine's frames on the stack until the panic completes, so a
    /// traceback taken from a deferred function shows the panic site; the CLR has already unwound
    /// them. Consumers append <see cref="PanicException.PanicTrace"/> to the live managed trace to
    /// recover Go's observable output — see runtime's Stack.
    /// </remarks>
    public static PanicException? InFlightPanic
    {
        get
        {
            GoThreadState state = GoThreadState.Current;
            return state.HandledPanic ?? state.CapturedPanic;
        }
    }

    // The slots, reachable by the golib members that read and write them: GoFrame's catch/finally
    // pair (all of them) and builtin.recover() (the recoverable one, and the captured one it clears).
    internal static PanicException? CapturedPanicValue
    {
        get => GoThreadState.Current.CapturedPanic;
        set => GoThreadState.Current.CapturedPanic = value;
    }

    internal static PanicException? HandledPanicValue
    {
        get => GoThreadState.Current.HandledPanic;
        set => GoThreadState.Current.HandledPanic = value;
    }

    internal static PanicException? RecoverablePanicValue
    {
        get => GoThreadState.Current.RecoverablePanic;
        set => GoThreadState.Current.RecoverablePanic = value;
    }

    // Arms the re-raise claim for a panic a frame's catch just captured.
    internal static void ArmPanicClaim(PanicException panic)
    {
        GoThreadState.Current.UnclaimedPanic = panic;
    }

    // Claims the armed panic, if any, and disarms the slot: the caller — one GoFrame.Run — becomes
    // the single frame responsible for continuing that panic once its deferred sequence has run.
    // Returns null for a frame that caught nothing, which is the whole point.
    internal static PanicException? ClaimPanic() => ClaimPanic(GoThreadState.Current);

    internal static PanicException? ClaimPanic(GoThreadState state)
    {
        PanicException? claimed = state.UnclaimedPanic;

        if (claimed is not null)
            state.UnclaimedPanic = null;

        return claimed;
    }
}
