// GoFrame.cs - Gbtc
// Copyright © 2026 The go2cs Authors. All rights reserved.
//
// Use of this source code is governed by an MIT-style license
// that can be found in the LICENSE file.

// ReSharper disable CheckNamespace
// ReSharper disable InconsistentNaming

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Runtime.CompilerServices;
using go.golib;

namespace go;

/// <summary>
/// The per-call defer list of a converted Go function — a Go stack frame's deferred-call
/// records, and nothing else.
/// </summary>
/// <remarks>
/// <para>
/// A Go function that defers or recovers is emitted with its body INLINE inside
/// <c>try</c>/<c>catch</c>/<c>finally</c> and one of these declared beside it:
/// </para>
/// <code>
/// //  Go:   func f() { defer g(); … }
/// GoFrame ᒐ = default;
/// try
/// {
///     defer(g, ref ᒐ);
///     …
/// }
/// catch (Exception ᒐex) when (GoFrame.IsPanic(ᒐex, out PanicException? ᒐp)) { GoFrame.Capture(ᒐp); }
/// finally { ᒐ.Run(); }
/// </code>
/// <para>
/// That replaces the <c>func&lt;T&gt;((defer, recover) =&gt; …)</c> execution context, which
/// modelled the same three things — a catch, a finally, and a defer list — as an OBJECT that owned
/// the body. Owning the body forced the body to be a delegate, which forced a display class for
/// everything it touched, which forced the <c>GoFunc&lt;TRef1…TRef16&gt;</c> ladder for everything
/// a delegate cannot capture. None of that is needed: <c>try</c>/<c>catch</c>/<c>finally</c> are
/// STATEMENTS, and <c>recover()</c> reads a static thread-local slot rather than a handle on the
/// frame — so a deferred closure never needs to reach the frame, and only the defer LIST is
/// genuinely per-call. A <c>ref struct</c> can hold it: it lives in the caller's own stack frame,
/// the JIT can enregister the slots, and it allocates nothing.
/// </para>
/// <para>
/// Design: <c>docs/Phase4/DESIGN-closure-emission.md</c> §4.
/// </para>
/// </remarks>
public ref struct GoFrame
{
    // Inline slots for the common defer arities; m_overflow is the correctness tail past them.
    // FOUR comes from a census of the Go standard library, not from a guess: of its 1,454 deferring
    // scopes (defer statements counted per function or literal, the way Go scopes them), 85.7%
    // register one, 96.3% two or fewer, and 99.2% four or fewer — one scope reaches sixteen. So the
    // overflow list exists for correctness and is allocated by a vanishing fraction of frames.
    // A defer inside a LOOP registers once per iteration and is what actually reaches it, which is
    // also why the count cannot be a purely syntactic property (see docs/Phase4/DESIGN-closure-emission.md §4.2).
    // What a nil deferred func does when it is called: Go's nil-dereference runtime error (panicmem).
    private static readonly Action s_nilDeferredCall = static () => throw RuntimeErrorPanic.NilPointerDereference();

    private Action? m_d0, m_d1, m_d2, m_d3;
    private List<Action>? m_overflow;
    private int m_count;
    private System.Runtime.ExceptionServices.ExceptionDispatchInfo? m_foreignRethrow;

    /// <summary>
    /// Gets the number of deferred calls registered in this frame and not yet run.
    /// </summary>
    public readonly int Count => m_count;

    /// <summary>
    /// Registers a deferred call, i.e. Go's <c>defer</c> statement.
    /// </summary>
    /// <param name="deferred">Deferred call to register; a null (Go's nil func) is registered too, and
    /// raises the nil-dereference runtime error when it is called at function exit.</param>
    /// <remarks>
    /// Go evaluates a deferred call's ARGUMENTS at the <c>defer</c> statement and runs the call on
    /// function exit, so the emission captures the arguments here — see the <c>defer</c> arity
    /// ladder in <c>builtin.DeferRegistrations.cs</c>, which is what closes over them.
    /// <para>
    /// Go's spec: a deferred function value that is nil panics when the function is INVOKED, not when
    /// the defer statement runs. A null registration used to be IGNORED, so `var f func(); defer f()`
    /// returned normally (runtime's TestCallersDeferNilFuncPanic and its loop form). The argument-taking
    /// rungs already wrap the call in a closure and fault correctly; the zero-argument rung hands the
    /// delegate straight here.
    /// </para>
    /// </remarks>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void Push(Action? deferred)
    {
        deferred ??= s_nilDeferredCall;

        switch (m_count)
        {
            case 0:
                m_d0 = deferred;
                break;
            case 1:
                m_d1 = deferred;
                break;
            case 2:
                m_d2 = deferred;
                break;
            case 3:
                m_d3 = deferred;
                break;
            default:
                (m_overflow ??= new List<Action>()).Add(deferred);
                break;
        }

        m_count++;
    }

    /// <summary>
    /// Runs this frame's deferred calls, last registered first, then re-raises an unrecovered panic.
    /// </summary>
    /// <remarks>
    /// <para>
    /// Called from the emitted <c>finally</c>, so it runs on EVERY exit path — normal return,
    /// panic, <c>runtime.Goexit</c>, or a mapped runtime fault — which is Go's whole guarantee for
    /// <c>defer</c>. It runs AFTER the panic has been parked by <see cref="Capture"/>, which is
    /// what lets a deferred call recover the panic raised by the body it was registered in.
    /// </para>
    /// <para>
    /// This is <c>GoFunc.HandleFinally</c>'s logic — the <c>HandledPanic</c> save/restore, the
    /// re-panic <c>InheritThrowSite</c> rule, and the final re-throw of an unrecovered panic — with
    /// TWO corrections. First: a panic raised BY a deferred call now becomes the frame's in-flight
    /// panic and the sequence CONTINUES, instead of aborting it. See the catch below. Second: the
    /// re-throw is the OWNING frame's, claimed from <see cref="Capture"/>, rather than a read of the
    /// thread's captured-panic slot — a frame called from another frame's deferred sequence must not
    /// re-raise a panic it never caught. See the claim below.
    /// </para>
    /// </remarks>
    public void Run()
    {
        // The panic THIS frame is responsible for continuing, if any: the one its own catch just
        // captured. Claiming it here — rather than reading the thread's captured-panic slot at the
        // tail — is what keeps the re-raise frame-owned. A frame that caught nothing claims null and
        // must leave an in-flight panic exactly where it is, because it is being CALLED from some
        // other frame's deferred sequence and that frame's Run is the one that continues the panic.
        //
        // Reading the slot instead made every such callee re-raise the caller's panic from its own
        // finally, aborting the rest of the deferred cleanup at an arbitrary point: database/sql's
        // Conn.Raw panic path left the connection open because withLock — three calls below the
        // deferred release, panicking nothing itself — threw the parked panic on the way out and
        // Conn.close never reached `c.dc = nil`.
        PanicException? owned = GoFuncRoot.ClaimPanic();

        if (m_count > 0)
        {
            // The panic this deferred sequence is handling, if any. It stays observable through
            // InFlightPanic for the whole sequence — recover() clears the captured panic, but Go's
            // traceback keeps showing the panicking frames until the panic completes. Strictly
            // save/restore scoped, so it cannot outlive the sequence.
            PanicException? handling = owned;
            PanicException? outer = GoFuncRoot.HandledPanicValue;

            GoFuncRoot.HandledPanicValue = handling ?? outer;

            // What a recover() in THIS sequence's deferred calls may stop: the panic being handled,
            // or nothing for a normal-return sequence — NOT the outer sequence's panic, even when this
            // frame is itself a deferred call that panic is running (Go's direct-call rule; runtime's
            // TestRecoverMatching). Written only when it changes, and restored on exit below.
            PanicException? outerRecoverable = GoFuncRoot.RecoverablePanicValue;

            if (!ReferenceEquals(outerRecoverable, handling))
                GoFuncRoot.RecoverablePanicValue = handling;

            try
            {
                while (m_count > 0)
                {
                    try
                    {
                        Pop()();
                    }
                    catch (Exception ex) when (IsPanic(ex, out PanicException? raised))
                    {
                        // A deferred call PANICKED. Go does not stop the sequence here: the new
                        // panic joins the one already unwinding (replacing it as the value a
                        // recover() answers), and this frame's REMAINING deferred calls still run —
                        // which is exactly what lets an earlier-registered deferred recover() catch
                        // a panic that a later-registered one raised. `defer panic(v)` is the
                        // smallest case (guarded by the DeferPanicArg behavioral test): the panic
                        // thunk runs first, the recover thunk second. Parking the panic where
                        // recover() reads it and continuing the loop is the whole correction; if
                        // nothing recovers it, the tail below re-raises it to the caller, unchanged.
                        //
                        // Before this, the catch was `when (handling is not null)` + `throw;`, so a
                        // deferred panic in a NON-panicking frame escaped the loop uncaught (the
                        // remaining defers were skipped and no recover() ever saw it), and one in a
                        // panicking frame aborted the rest of the sequence.
                        //
                        // The filter is IsPanic, matching the emitted frame's own catch, so a
                        // runtime fault in a deferred call is recoverable here exactly as it is in
                        // the body. GoexitException deliberately FAILS that filter, so a
                        // runtime.Goexit still unwinds through this frame as before.
                        //
                        // The FOREIGN-UNWIND correction (exec-wall design OQ-6, ratified
                        // 2026-08-22): when this frame is unwinding on a foreign (.NET) exception —
                        // owned/handling are null, because IsPanic rightly refused to adopt it —
                        // a deferred `panic(recover())` re-panics NIL, since recover() sees no Go
                        // panic. Adopting that nil panic here would REPLACE the original defect at
                        // the tail, which is exactly how every OnceValue-guarded probe reported
                        // `panic: nil` instead of naming the NotImplementedException underneath.
                        // The original is preserved (IsPanic's filter pass captured it with its
                        // stack); consume it, let the remaining defers run per Go's sequence rules,
                        // and the tail continues the ORIGINAL unwind. A later real panic from
                        // another deferred call still supersedes it — Go's own replacement rule —
                        // because the tail prefers `owned`.
                        if (handling is null && raised.State is null &&
                            GoFuncRoot.InFlightForeignException is { } preservedForeign)
                        {
                            GoFuncRoot.InFlightForeignException = null;
                            m_foreignRethrow = preservedForeign;
                            continue;
                        }

                        if (handling is not null)
                        {
                            // Go's re-panic idiom (`defer func(){ panic(recover()) }()`, which is how
                            // sync.OnceFunc replays a panic on every call) raises a NEW panic from the
                            // deferred frame. Go's traceback still shows the original panic's frames, so
                            // the new panic adopts the origin rather than starting a fresh, shallower one.
                            raised.InheritThrowSite(handling);
                        }

                        GoFuncRoot.CapturedPanicValue = raised;
                        GoFuncRoot.HandledPanicValue = raised;
                        GoFuncRoot.RecoverablePanicValue = raised;
                        handling = raised;

                        // A panic raised by THIS frame's own deferred call is this frame's to
                        // continue, whether or not the frame was already panicking — so the tail
                        // re-raises it even when nothing was claimed on entry.
                        owned = raised;
                    }
                }
            }
            finally
            {
                GoFuncRoot.HandledPanicValue = outer;

                if (!ReferenceEquals(GoFuncRoot.RecoverablePanicValue, outerRecoverable))
                    GoFuncRoot.RecoverablePanicValue = outerRecoverable;
            }
        }

        // The owned panic continues exactly when nothing recovered IT. This used to read the thread's
        // captured-panic slot instead, which a NESTED panic overwrites and its recover() clears — so a
        // panic recovered inside one of this frame's deferred calls silently swallowed the panic this
        // frame was running (runtime's TestIssue43921), or left an outer recover() reading nil
        // (TestIssue43920). `owned` and the sequence's `handling` are always the same panic here.
        if (owned is { Recovered: false })
            throw owned;

        // The foreign-unwind correction's second half: no real panic superseded the sequence, so
        // the ORIGINAL foreign exception continues unwinding with its stack intact — instead of
        // the nil re-panic that used to replace it here.
        if (m_foreignRethrow is { } foreignRethrow)
        {
            m_foreignRethrow = null;
            foreignRethrow.Throw();
        }
    }

    // LIFO removal. The slot is cleared on the way out so a frame that outlives its drain (it
    // cannot, but the JIT does not know that) holds no reference to a run delegate.
    private Action Pop()
    {
        m_count--;

        if (m_count > 3)
        {
            int index = m_count - 4;
            Action overflowed = m_overflow![index];
            m_overflow.RemoveAt(index);
            return overflowed;
        }

        Action deferred;

        switch (m_count)
        {
            case 0:
                deferred = m_d0!;
                m_d0 = null;
                break;
            case 1:
                deferred = m_d1!;
                m_d1 = null;
                break;
            case 2:
                deferred = m_d2!;
                m_d2 = null;
                break;
            default:
                deferred = m_d3!;
                m_d3 = null;
                break;
        }

        return deferred;
    }

    /// <summary>
    /// The emitted <c>catch</c> FILTER: reports whether an exception is (or maps to) a Go panic.
    /// </summary>
    /// <param name="ex">Exception to inspect.</param>
    /// <param name="panic">Resulting panic when the exception maps to a Go panic.</param>
    /// <returns><c>true</c> if <paramref name="ex"/> is (or maps to) a Go panic; otherwise <c>false</c>.</returns>
    /// <remarks>
    /// A pure forward to <see cref="RuntimeErrorPanic.TryAsPanic"/> — which is the ONE adoption
    /// point where a .NET exception becomes a Go panic and where the panic's origin is snapshotted.
    /// It exists so an emitted <c>catch</c> filter can name it without the converted file having to
    /// import <c>go.golib</c>; a non-panic exception fails the filter and propagates unchanged,
    /// exactly as it does past <c>GoFunc.Execute</c>. <see cref="GoexitException"/> fails it by
    /// design, so a <c>runtime.Goexit</c> unwinds through the frame while the <c>finally</c> still
    /// runs the defers.
    /// </remarks>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static bool IsPanic(Exception ex, [NotNullWhen(true)] out PanicException? panic)
    {
        if (RuntimeErrorPanic.TryAsPanic(ex, out panic))
            return true;

        // A non-panic exception fails the filter and propagates unchanged — but PRESERVE it first:
        // the filter's first pass is the one reliable point every foreign exception crosses before
        // any finally runs its defers, and a deferred `panic(recover())` in those defers would
        // otherwise replace this original with `panic: nil` (Run's foreign-unwind correction,
        // exec-wall design OQ-6). GoexitException is deliberate control flow, not a defect, and is
        // excluded exactly as it is from panic adoption.
        if (ex is not GoexitException)
            GoFuncRoot.InFlightForeignException = System.Runtime.ExceptionServices.ExceptionDispatchInfo.Capture(ex);

        return false;
    }

    /// <summary>
    /// The emitted <c>catch</c> BODY: parks a caught panic where <c>recover()</c> can read it.
    /// </summary>
    /// <param name="panic">Panic caught by the emitted frame.</param>
    /// <remarks>
    /// Deliberately separate from <see cref="IsPanic"/> rather than folded into the filter: an
    /// exception filter runs during the FIRST pass of managed exception handling, before any
    /// intervening <c>finally</c>, and parking the panic is what the <c>finally</c> then observes.
    /// Keeping it in the catch BODY preserves <c>GoFunc.Execute</c>'s ordering exactly. The origin
    /// snapshot is not repeated here — <see cref="RuntimeErrorPanic.TryAsPanic"/> already took it
    /// at the adoption point, and it is once-only.
    /// <para>
    /// It also ARMS the re-raise claim, which is what tells the <c>finally</c>'s <see cref="Run"/>
    /// that the parked panic is this frame's to continue. Arming here rather than in the filter is
    /// deliberate for the same ordering reason, and it needs no change to the emitted frame: a catch
    /// body and its finally are adjacent, so the next <see cref="Run"/> on the thread is always this
    /// frame's.
    /// </para>
    /// </remarks>
    [MethodImpl(MethodImplOptions.AggressiveInlining), DebuggerStepperBoundary]
    public static void Capture(PanicException panic)
    {
        GoFuncRoot.InFlightForeignException = null; // a REAL panic supersedes any preserved foreign unwind
        panic.Recovered = false; // a panic arriving at a catch is in flight: unrecovered by definition
        GoFuncRoot.CapturedPanicValue = panic;
        GoFuncRoot.ArmPanicClaim(panic);
    }
}
