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
    // NoInlining is a CONTRACT here, not a heuristic: runtime's captureCallers pairs each Run frame it
    // walks with this thread's panic-sequence entries (see GoThreadState.SequenceDepth), so an inlined Run would leave
    // no frame to pair and shift every splice by one sequence.
    //
    // DebuggerNonUserCode, not DebuggerStepThrough: this frame CALLS the user's deferred functions, so
    // under Just My Code a step into the function's exit lands in the deferred call rather than here. It
    // changes only the debugger's view; StackTrace (and so captureCallers' pairing) still sees the frame.
    [MethodImpl(MethodImplOptions.NoInlining)]
    [System.Diagnostics.DebuggerNonUserCode]
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
        // This thread's panic and defer state, fetched ONCE: every slot below is a field of it (the
        // defer-cost cut; see GoThreadState).
        GoThreadState state = GoThreadState.Current;
        PanicException? owned = GoFuncRoot.ClaimPanic(state);

        if (m_count > 0)
        {
            // The panic this deferred sequence is handling, if any. It stays observable through
            // InFlightPanic for the whole sequence — recover() clears the captured panic, but Go's
            // traceback keeps showing the panicking frames until the panic completes. Strictly
            // save/restore scoped, so it cannot outlive the sequence.
            PanicException? handling = owned;
            PanicException? outer = state.HandledPanic;

            state.HandledPanic = handling ?? outer;

            // What a recover() in THIS sequence's deferred calls may stop: the panic being handled,
            // or nothing for a normal-return sequence — NOT the outer sequence's panic, even when this
            // frame is itself a deferred call that panic is running (Go's direct-call rule; runtime's
            // TestRecoverMatching). Written only when it changes, and restored on exit below.
            PanicException? outerRecoverable = state.RecoverablePanic;

            if (!ReferenceEquals(outerRecoverable, handling))
                state.RecoverablePanic = handling;

            // This sequence's slot for runtime's captureCallers. EVERY deferring Run takes one, since a
            // normal-return Run can sit between a Callers and a panicking one ([P2-1]), but a normal
            // return pays only this depth: the entry itself (the panic and the activation) is written
            // once a panic is running in, or raised into, the sequence. An unwritten slot reads as a
            // normal-return entry (SequenceFromTop).
            int sequence = state.SequenceDepth++;
            bool recorded = handling is not null;

            if (recorded)
                SetSequence(state, sequence, handling);

            // The panic this frame's own catch caught FIRST has its site ending at this frame, so this
            // activation owns it. Owned by ACTIVATION, not by method: a recursive activation of the
            // same function is a different owner (review C1 of the cut).
            if (owned is { Catches: 1, SiteOwner: 0 })
            {
                owned.SiteOwner = state.Sequences![sequence].Activation;
                owned.SiteOwnerThread = ThreadToken;
            }

            // The delegate the loop last invoked: the one accepted re-raise is recognised by its OWN method
            // (ReRaisedByTheDeferredDelegate), never by frames missing from a trace.
            Action? deferred = null;

            // Whether a recovery completed in this sequence. Go then runs the frame's remaining deferred calls
            // from runtime.deferreturn, so a nil deferred func faulting after it shows deferreturn beneath
            // sigpanic, which is not modelled (COORD's second verification, item 3).
            bool recoveryCompleted = false;

            try
            {
                while (m_count > 0)
                {
                    try
                    {
                        // THE NORMAL-RETURN LOOP, at base's per-call cost: no delegate kept live into the
                        // catch and no recovery check. The slot is cleared only AFTER the call returns, so
                        // when the call panics, the catch below re-reads the raising delegate from its slot.
                        // A panic makes `handling` non-null for good, so every later call takes the loop below.
                        if (handling is null)
                        {
                            int top = --m_count;
                            Slot(top)();
                            ClearSlot(top);
                            continue;
                        }

                        deferred = Pop();
                        deferred();

                        // A deferred call that recovered this sequence's panic has RETURNED: Go's recovery
                        // completes here and the panicking frames leave the stack, so a later deferred
                        // call's Callers no longer shows them (runtime's TestCallersAfterRecovery).
                        if (handling.Recovered)
                        {
                            SetSequence(state, sequence, null);
                            recoveryCompleted = true;
                        }
                    }
                    catch (Exception ex) when (IsPanic(ex, out PanicException? raised))
                    {
                        // Raised from the normal-return loop: its delegate is still in its slot.
                        if (handling is null)
                        {
                            deferred = Slot(m_count);
                            ClearSlot(m_count);
                        }

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
                            state.InFlightForeign is { } preservedForeign)
                        {
                            state.InFlightForeign = null;
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

                        // The panic this sequence was running when the deferred call raised: Go keeps it
                        // beneath the new one until a recovery completes, because the deferred call was
                        // called by its gopanic. A recovery that already completed nulled the entry.
                        PanicException? running = recorded ? state.Sequences![sequence].Panic : null;

                        // The entry (and its activation) exists from here on.
                        SetSequence(state, sequence, running);
                        recorded = true;

                        // Whether this activation OWNS the new panic's site (review B1, B2, C3). It does
                        // when this catch is the panic's FIRST: the deferred call raised it with no
                        // deferring frame of its own, so the site ends here. It also does in the one
                        // accepted re-raise: the deferred delegate IS the frame that caught the panic
                        // first, and that frame's Run re-raised it straight here (`defer D()` where D
                        // defers and panics). Anything else leaves the site unowned here, so it splices
                        // nothing rather than a list with frames missing. A panic raised while a Goexit
                        // runs this sequence is left unowned too: Go shows runtime.Goexit beneath the
                        // deferred call, and that frame is not modelled. (runtime's captureCallers
                        // accepts only some owned ends-at-Run sites: see splicePanic.)
                        bool firstHere = raised is { SiteOwner: 0, Catches: 1 };
                        bool reRaised = !firstHere && ReRaisedByTheDeferredDelegate(deferred, raised);

                        // The nil-func thunk faulting with no newer panic running, after this sequence's recovery
                        // completed: Go's frames there include runtime.deferreturn beneath sigpanic (only when the
                        // nil call itself faults), so it is left unowned too.
                        bool afterCompletedRecovery = running is null && recoveryCompleted && ReferenceEquals(deferred, s_nilDeferredCall);

                        if ((firstHere || reRaised) && !(running is null && GoexitException.Started) && !afterCompletedRecovery)
                        {
                            raised.SiteOwner = state.Sequences![sequence].Activation;
                            raised.SiteOwnerThread = ThreadToken;
                            raised.SiteEndsAtRun = true;
                            raised.SiteIsTheDeferredCall = reRaised;

                            if (running is not null && !ReferenceEquals(running, raised))
                                raised.Beneath = running;
                        }

                        state.CapturedPanic = raised;
                        state.HandledPanic = raised;
                        state.RecoverablePanic = raised;
                        handling = raised;

                        // The new panic REPLACES the one this sequence was running, so the sequence's
                        // later deferred calls see it (the TestCallersAbortedPanic shape).
                        SetSequence(state, sequence, raised);

                        // A panic raised by THIS frame's own deferred call is this frame's to
                        // continue, whether or not the frame was already panicking — so the tail
                        // re-raises it even when nothing was claimed on entry.
                        owned = raised;
                    }
                }
            }
            finally
            {
                // Only an entry this activation wrote is cleared (so no stale panic outlives it); the depth
                // is the normal return's one store.
                if (recorded)
                    state.Sequences![sequence] = default;

                state.SequenceDepth = sequence;
                state.HandledPanic = outer;

                if (!ReferenceEquals(state.RecoverablePanic, outerRecoverable))
                    state.RecoverablePanic = outerRecoverable;
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

    // THE PANIC SEQUENCES (docs/phase4/DESIGN-panic-stack-frames.md §3.B). Go runs a panic's deferred
    // calls ON the panicking stack, so a Callers from one still finds runtime.gopanic and the frames it
    // panicked through. Here the CLR has already unwound those frames, and runtime's captureCallers
    // splices them back from the panic's SiteTrace. To know WHICH panic a Run frame on the live stack
    // is running, every deferring Run takes one slot: its own panic, or null. The k-th Run frame met
    // walking down from the top pairs with the k-th slot from the top. Per thread, as the other panic
    // slots are (GoFuncRoot), and the array is allocated once per thread, on the first panic, and reused.
    // A normal return pays the depth alone (Go's commonest idiom is `mu.Lock(); defer mu.Unlock()`):
    // a slot's entry is written only while a panic is running in it, and cleared by whoever wrote it.
    //
    // Each entry also carries its ACTIVATION: a per-thread number no other Run activation on the thread
    // ever shares (a slot index is reused as soon as its Run returns, so it cannot tell two activations
    // apart). A panic's SiteOwner names the activation its site provably ends at, and the splice
    // requires it on every link.
    internal struct Sequence
    {
        internal PanicException? Panic;
        internal long Activation;
    }

    [ThreadStatic] private static long t_lastActivation;

    // Writes a sequence's entry on first need: grows the array and numbers the activation lazily, once.
    private static void SetSequence(GoThreadState state, int index, PanicException? panic)
    {
        Sequence[] entries = state.Sequences ??= new Sequence[16];

        if (index >= entries.Length)
        {
            Array.Resize(ref state.Sequences, Math.Max(index + 1, entries.Length * 2));
            entries = state.Sequences;
        }

        ref Sequence entry = ref entries[index];
        entry.Panic = panic;

        if (entry.Activation == 0)
            entry.Activation = ++t_lastActivation;
    }


    // The one re-raise an activation accepts as its own site (see Run's catch): the panic was caught FIRST
    // by an emitted frame (its site ends at that frame, not at a Run), caught exactly once since, here, and
    // the delegate this Run just invoked IS that frame's method, so no Go frame lies between. Decided by the
    // delegate's own method: a trace cannot be read for "no frame between", because the JIT's implicit
    // tail calls remove exactly those frames (COORD's verification of the re-cut, B1). A catcher whose Run
    // registered no defer (an unreached conditional defer) left the site unstamped, and passes too.
    private static bool ReRaisedByTheDeferredDelegate(Action? deferred, PanicException raised)
    {
        if (deferred is null || raised.Catches != 2 || raised.SiteEndsAtRun || raised.SiteTrace?.GetFrames() is not { Length: > 0 } site)
            return false;

        System.Reflection.MethodBase? catcher = site[^1].GetMethod();

        return catcher is not null && catcher != RunMethod && deferred.Method == catcher;
    }

    // Identifies this thread for SiteOwnerThread: an object, so no two threads ever share one (a managed
    // thread id can be reused once a thread ends). Read only on the panic path.
    [ThreadStatic] private static object? t_threadToken;

    internal static object ThreadToken => t_threadToken ??= new object();

    /// <summary>The zero-argument nil-deferred-func thunk's method: the one site-less ends-at-Run site runtime's captureCallers accepts.</summary>
    internal static System.Reflection.MethodInfo NilDeferredCallMethod => s_nilDeferredCall.Method;

    // A range-over-func seq resumed with its loop STOPPED (a break, a panic or a Goexit in the loop body,
    // which the adapter cannot tell apart) runs the rest of seq on its coro's thread, where Go would show
    // the body's frames beneath it. Every splice on that thread is refused from then on (a missing splice,
    // never a partial one); the pooled thread is reset with the rest of its state.
    [ThreadStatic] private static bool t_splicesRefused;

    internal static void RefuseSplicesOnThisThread() => t_splicesRefused = true;

    internal static bool SplicesRefused => t_splicesRefused;

    // A goroutine's thread starts with no sequences and no refusal (GoFuncRoot.ResetThread): the entries and the
    // depth are cleared with the rest of GoThreadState. Run's finally always pops, so this only matters for a
    // thread reused after an abnormal end.
    internal static void ResetSequences()
    {
        t_splicesRefused = false;
    }

    /// <summary>The <see cref="Run"/> method, for matching its frames by identity rather than by name ([P2-4]).</summary>
    internal static readonly System.Reflection.MethodBase RunMethod = typeof(GoFrame).GetMethod(nameof(Run))!;

    /// <summary>
    /// The panic the <paramref name="fromTop"/>-th deferring <see cref="Run"/> frame below the caller
    /// is running (0 is the innermost), or null for a normal-return sequence or no such frame, with that
    /// Run's activation.
    /// </summary>
    internal static (PanicException? Panic, long Activation) SequenceFromTop(int fromTop)
    {
        GoThreadState state = GoThreadState.Current;
        int index = state.SequenceDepth - 1 - fromTop;
        return index >= 0 && state.Sequences is { } entries && index < entries.Length ? (entries[index].Panic, entries[index].Activation) : (null, 0);
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

    // The normal-return loop's halves of Pop: read a slot, and clear it once its call has returned (Run).
    private readonly Action Slot(int index) => index switch
    {
        0 => m_d0!,
        1 => m_d1!,
        2 => m_d2!,
        3 => m_d3!,
        _ => m_overflow![index - 4]
    };

    private void ClearSlot(int index)
    {
        switch (index)
        {
            case 0:
                m_d0 = null;
                break;
            case 1:
                m_d1 = null;
                break;
            case 2:
                m_d2 = null;
                break;
            case 3:
                m_d3 = null;
                break;
            default:
                m_overflow!.RemoveAt(index - 4);
                break;
        }
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
