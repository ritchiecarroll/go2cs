// PanicException.cs - Gbtc
// Copyright © 2026 The go2cs Authors. All rights reserved.
//
// Use of this source code is governed by an MIT-style license
// that can be found in the LICENSE file.

using System;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using System.Runtime.ExceptionServices;
using go.golib;

namespace go;

/// <summary>
/// Represents an exception for the "panic" keyword.
/// </summary>
[DebuggerNonUserCode]
public class PanicException(object? state, Exception? innerException = null) :
    Exception(null, innerException)
{
    [DebuggerBrowsable(DebuggerBrowsableState.Never)]
    public object? State { get; } = state;

    /// <summary>
    /// Whether a <c>recover()</c> has stopped this panic — Go's <c>_panic.recovered</c>. Set by
    /// <see cref="builtin.recover"/>; cleared by <see cref="GoFrame.Capture"/>, because a panic
    /// arriving at a frame's catch is in flight and therefore unrecovered by definition. The deferred
    /// sequence that owns the panic re-raises it at its tail exactly when this is still false, so a
    /// NESTED panic recovered inside one of its deferred calls can no longer make it disappear
    /// (runtime's TestIssue43920/TestIssue43921; docs/phase4/DESIGN-recover-model.md).
    /// </summary>
    [DebuggerBrowsable(DebuggerBrowsableState.Never)]
    internal bool Recovered { get; set; }

    [DebuggerBrowsable(DebuggerBrowsableState.Never)]
    private string? m_message;

    /// <summary>
    /// The panic value as Go's runtime reports it — <see cref="PanicText"/> of <see cref="State"/>.
    /// </summary>
    /// <remarks>
    /// Computed on FIRST READ, not at construction, because that is when Go computes it: Go runs
    /// <c>preprintpanics</c> only once a panic has gone unrecovered and is about to be printed. A
    /// recovered panic — <c>fmt</c>'s catchPanic, <c>text/template</c>'s errRecover, and every
    /// <c>defer func(){ recover() }()</c> in the corpus — therefore never calls a user
    /// <c>Error()</c>/<c>String()</c> at all, exactly as in Go, and pays nothing for the rule.
    /// </remarks>
    public override string Message => m_message ??= PanicText(State);

    /// <summary>
    /// Renders a panic VALUE the way Go's runtime prints it.
    /// </summary>
    /// <remarks>
    /// <para>
    /// Go's <c>preprintpanics</c> (runtime/panic.go) substitutes before anything is printed: an
    /// <c>error</c> panic value becomes its <c>Error()</c>, a <c>Stringer</c> its <c>String()</c>.
    /// Without that rule a converted <c>panic(err)</c> renders the managed value itself — for a
    /// pointer-held error, its ADDRESS (<c>panic: 0x211163e3340</c>) — which is not merely
    /// unlike Go, it destroys the one piece of information a panic report exists to carry. It cost
    /// the row-harvest-2 lane a diagnostic round-trip on the only defect it was chasing.
    /// </para>
    /// <para>
    /// Asking that question of the managed type system alone is not enough, and the gap is the whole
    /// corpus: a CONVERTED Go error does not implement golib's <c>error</c> interface — the converter
    /// emits <c>Error</c> as an extension method over its receiver — so <c>state is error</c> matches
    /// only golib's own carrier. <c>reflect</c>'s <c>panic(&amp;ValueError{...})</c> is the measured
    /// case: it arrives as a ж box, which implements nothing, carries no <c>String</c>, and answers
    /// <c>ToString()</c> with its address. It printed <c>panic: 0x2668d34e960</c> and cost two
    /// investigations their time. A value-held converted error was no better off — it printed the
    /// .NET type name. Both are why the method set is probed, not merely the interface.
    /// </para>
    /// <para>
    /// The <c>Stringer</c> arm is not redundant with <c>ToString()</c>. A Go named type's generated
    /// <c>ToString()</c> forwards to its UNDERLYING value (go2cs-gen's InheritedTypeTemplate), so
    /// <c>panic(2 * time.Second)</c> would print <c>2000000000</c> where Go prints <c>2s</c>. The
    /// method is found the way golib's <c>error&lt;T&gt;</c> finds <c>Error</c> — through the
    /// extension-method registry, which is where a converted Go method lives.
    /// </para>
    /// <para>
    /// Go throws a fatal <c>"panic while printing panic value"</c> if that substitution itself
    /// panics. Reproducing the FATALITY from a <see cref="Exception.Message"/> getter would be
    /// worse than the divergence it reports, so the text is returned instead of thrown.
    /// </para>
    /// </remarks>
    public static string PanicText(object? state)
    {
        switch (state)
        {
            case null:
                return "nil";
            // Ahead of the method probes: this is the commonest panic value by far, and its own
            // ToString is already the Go rendering.
            case @string text:
                return text.ToString();
        }

        try
        {
            if (state is error goError)
                return goError.Error().ToString();

            // The `error` arm above catches only what golib's own carrier holds. A CONVERTED Go
            // error implements no managed interface at all — the converter emits a Go method as an
            // extension method over its receiver — so the question has to be asked of the Go method
            // set, exactly as Go's `case error:` asks it of the dynamic type's.
            if (TryGoMethod(state, nameof(error.Error), out string errorText))
                return errorText;

            if (TryGoMethod(state, "String", out string stringer))
                return stringer;
        }
        catch (Exception ex) when (ex is not GoexitException)
        {
            return "panic while printing panic value";
        }

        return state.ToString() ?? "nil";
    }

    // Go's `error`/`stringer` are the structural `interface{ Error() string }` and
    // `interface{ String() string }`, so this asks the same question of the managed value: does its
    // Go method set carry the method? A converted Go method is an extension method over its
    // receiver, so the method set is the extension registry and the RECEIVER is what decides.
    //
    // Deciding on the receiver is what keeps Go's method SET honest, which matters most for `Error`: a
    // pointer-receiver `func (e *T) Error() string` converts to a `[GoRecv] this ref T` primary
    // plus go2cs-gen's ж<T> overload, and a `T&` parameter can never be an instance of a `T` value —
    // so the value shape declines and prints as a plain value, exactly as in Go, while the ж box
    // that a `panic(&T{...})` actually carries binds the pointer overload and prints its text.
    private static bool TryGoMethod(object state, string name, out string text)
    {
        text = "";

        // Every candidate is considered and the RECEIVER decides, rather than asking
        // GetExtensionMethod for its single best pick. For a ж box the registry answers the whole
        // ж<> family — every `Error(this ж<X>)` in every loaded assembly — and the precedence
        // comparer's winner is routinely some other X, which a receiver re-check can only reject,
        // never repair. Rejecting it is what left `panic(&ValueError{...})` printing its address
        // after the interface arm had already been written. Exactly one candidate can pass the
        // instance test, so the choice is unambiguous rather than merely first.
        MethodInfo? method = state.GetType().GetExtensionMethods().FirstOrDefault(candidate =>
            candidate.Name == name &&
            candidate.ReturnType == typeof(@string) &&
            candidate.GetParameters() is { Length: 1 } parameters &&
            parameters[0].ParameterType.IsInstanceOfType(state));

        if (method is null)
            return false;

        try
        {
            text = ((@string)method.Invoke(null, [state])!).ToString();
        }
        catch (TargetInvocationException ex) when (ex.InnerException is not null)
        {
            // A reflective call BOXES whatever the method threw. The caller's filter has to read the
            // real exception's type — a Goexit is the goroutine ending and must keep unwinding,
            // where anything else is only a failed substitution to report — and the wrapper hides
            // it, so the original is rethrown with its stack intact.
            ExceptionDispatchInfo.Capture(ex.InnerException).Throw();
        }

        return true;
    }

    /// <summary>
    /// Gets the stack trace of the site where this panic ORIGINALLY started, as
    /// <c>runtime.Stack</c>/<c>debug.Stack</c> must report it while the panic is being handled.
    /// </summary>
    /// <remarks>
    /// <para>
    /// Go keeps the panicking frames physically on the stack until the panic completes, so a
    /// traceback taken inside a deferred function shows the panic site. A CLR exception has already
    /// unwound those frames by the time a <c>finally</c>-based defer runs, and worse, both of the
    /// ways a panic travels DESTROY the trace: re-raising the same instance (<c>throw ex</c>) resets
    /// <see cref="Exception.StackTrace"/> to the re-raise point, and Go's own re-panic idiom
    /// (<c>defer func(){ p := recover(); panic(p) }()</c> — exactly what <c>sync.OnceFunc</c> does)
    /// creates a brand new panic in the deferred frame. This property is snapshotted ONCE, at the
    /// first catch, and inherited by any panic raised while handling this one, so the origin
    /// survives both.
    /// </para>
    /// <para>
    /// Cost is zero on the non-panicking path: the CLR fills <see cref="Exception.StackTrace"/> at
    /// throw time regardless, and nothing here is computed unless a panic is actually caught.
    /// </para>
    /// </remarks>
    [DebuggerBrowsable(DebuggerBrowsableState.Never)]
    public StackTrace? PanicTrace { get; private set; }

    /// <summary>
    /// Reports the panic SITE first, then the frames the panic unwound through to reach the reader —
    /// the closest a CLR exception can come to Go's single, uninterrupted panic traceback.
    /// </summary>
    /// <remarks>
    /// <para>
    /// The base property cannot answer this on its own. A panic reaches its reporter by being
    /// re-raised from <c>GoFunc.HandleFinally</c> (<c>throw CapturedPanic.Value</c>, after the
    /// deferred sequence declined to recover it), and re-raising a stored instance RESETS
    /// <see cref="Exception.StackTrace"/> to the re-raise point. Worse, a runtime-error panic — a nil
    /// dereference, a divide by zero — is SYNTHESIZED by <see cref="golib.RuntimeErrorPanic"/> from
    /// the .NET exception and was never thrown at the fault site at all, so its base trace can only
    /// ever name the re-raise. Both cases print the same useless frame: every panic in the corpus
    /// appears to originate inside the defer machinery, and the real fault site — the whole reason a
    /// traceback is read — is gone.
    /// </para>
    /// <para>
    /// <see cref="PanicTrace"/> already holds the origin, snapshotted once at the first catch, and is
    /// how <c>runtime.Stack</c> reproduces Go's traceback. This makes every OTHER reader — the
    /// Phase-4 test host, an unhandled-exception dump, a debugger — see it too, without any of them
    /// having to know the panic machinery exists.
    /// </para>
    /// </remarks>
    public override string? StackTrace
    {
        get
        {
            string? unwound = base.StackTrace;

            if (PanicTrace is null || PanicTrace.FrameCount == 0)
                return unwound;

            // StackTrace.ToString() terminates its final frame with a newline; Exception.StackTrace
            // does not — so the origin concatenates onto the unwind cleanly, and trims to match the
            // base property's shape when there is no unwind to append.
            string origin = PanicTrace.ToString();

            return unwound is null ? origin.TrimEnd() : origin + unwound;
        }
    }

    /// <summary>
    /// Gets this panic's OWN site: the trace of the raised exception as its first catching frame saw
    /// it, from the throw site to that frame. Unlike <see cref="PanicTrace"/>, a re-panic's inheritance
    /// never overwrites it.
    /// </summary>
    /// <remarks>
    /// runtime's <c>captureCallers</c> splices it beneath <c>runtime.gopanic</c> when a
    /// <c>Callers</c> runs inside this panic's deferred sequence, which is where Go's unwinder still
    /// finds the panicking frames (docs/phase4/DESIGN-panic-stack-frames.md §3.B, [P2-2]).
    /// </remarks>
    [DebuggerBrowsable(DebuggerBrowsableState.Never)]
    internal StackTrace? SiteTrace { get; private set; }

    /// <summary>
    /// Gets the runtime frames Go's unwinder shows between <c>runtime.gopanic</c> and the panic site:
    /// a hardware fault adds <c>panicmem</c> and <c>sigpanic</c>, an integer divide adds
    /// <c>panicdivide</c>, and an explicit <c>panic(v)</c> adds none. Tagged where the panic is RAISED
    /// (builtin.panic, and RuntimeErrorPanic's nil-dereference and divide factories); every other panic
    /// golib raises keeps the default, <see cref="PanicFaultKind.Unmodelled"/>, which runtime's
    /// captureCallers refuses to splice.
    /// </summary>
    [DebuggerBrowsable(DebuggerBrowsableState.Never)]
    internal PanicFaultKind FaultKind { get; init; }

    /// <summary>
    /// Gets Go's throw text when this panic is one of <c>panicCheck1</c>'s -- an index, slice, slice3 or
    /// slice-convert bounds check, or a negative shift -- and <see langword="null"/> otherwise. Raised by
    /// code in package <c>runtime</c>, such a panic is Go's FATAL error with this text; anywhere else it is
    /// an ordinary panic. Tagged where the panic is raised (<see cref="golib.RuntimeErrorPanic"/>'s
    /// factories), and read where the panic is recovered or reported (<see cref="golib.RuntimePanicCheck"/>).
    /// </summary>
    [DebuggerBrowsable(DebuggerBrowsableState.Never)]
    internal string? RuntimeThrowText { get; init; }

    /// <summary>
    /// Gets the panic whose deferred sequence raised this one, when a deferred call panicked while that
    /// panic was being handled. Go's stack still holds it beneath: the deferred call's frame sits on
    /// the older panic's <c>runtime.gopanic</c>, until a recovery completes. Set by
    /// <see cref="GoFrame.Run"/>'s own catch; runtime's <c>captureCallers</c> splices down the chain.
    /// </summary>
    [DebuggerBrowsable(DebuggerBrowsableState.Never)]
    internal PanicException? Beneath { get; set; }

    /// <summary>
    /// Gets how many catches have adopted this panic (each <c>IsPanic</c> filter that caught it). One
    /// means the catch now running is the FIRST, so <see cref="SiteTrace"/> ends at it.
    /// </summary>
    [DebuggerBrowsable(DebuggerBrowsableState.Never)]
    internal int Catches { get; private set; }

    /// <summary>
    /// Gets the deferring <see cref="GoFrame.Run"/> ACTIVATION that owns this panic's site, or 0 when
    /// none provably does. It is stamped only at the first catch: by the Run of the frame whose catch
    /// that was, or by the Run whose own catch caught a deferred call's panic. A panic re-raised past
    /// its first catcher keeps its stamp, so a later Run never matches it: its site does not reach that
    /// Run, and runtime's <c>captureCallers</c> then splices nothing rather than a wrong list.
    /// </summary>
    [DebuggerBrowsable(DebuggerBrowsableState.Never)]
    internal long SiteOwner { get; set; }

    /// <summary>
    /// Gets whether the site ends at the owning <see cref="GoFrame.Run"/>, not at the deferring
    /// function: a deferred call raised this panic with no deferring frame of its own. Every Go frame
    /// of the site then stays on Go's stack, above the deferring function.
    /// </summary>
    [DebuggerBrowsable(DebuggerBrowsableState.Never)]
    internal bool SiteEndsAtRun { get; set; }

    /// <summary>
    /// Gets the THREAD whose Run activation <see cref="SiteOwner"/> numbers: activation numbers are unique
    /// only within a thread, and a panic can cross threads (range-over-func re-raises a seq's panic on the
    /// ranging goroutine). runtime's captureCallers requires both to match.
    /// </summary>
    [DebuggerBrowsable(DebuggerBrowsableState.Never)]
    internal object? SiteOwnerThread { get; set; }

    /// <summary>
    /// Gets whether this panic's site ends at a Run because the deferred delegate that Run invoked IS the
    /// frame that caught the panic first, re-raised straight to it (checked by the popped delegate's own
    /// method, never inferred from missing stack frames).
    /// </summary>
    [DebuggerBrowsable(DebuggerBrowsableState.Never)]
    internal bool SiteIsTheDeferredCall { get; set; }

    /// <summary>
    /// Gets whether a converter-emitted method-expression WRAPPER raised this panic itself, on a nil
    /// receiver (builtin.wrapperRecv / builtin.panicwrapRecv). It is the emitter's marker that the
    /// wrapper's frame is the panic site: runtime's captureCallers keeps a wrapper frame only then,
    /// because Go keeps one only when its callee is the panic machinery.
    /// </summary>
    [DebuggerBrowsable(DebuggerBrowsableState.Never)]
    internal bool RaisedByWrapper { get; init; }

    /// <summary>
    /// Gets whether this panic unwound through a range-over-func loop that stopped early: the loop BODY's
    /// frame (Go's rangefunc closure) and seq's lie between the site and the ranging function, and are
    /// not modelled, so runtime's captureCallers splices nothing for it.
    /// </summary>
    [DebuggerBrowsable(DebuggerBrowsableState.Never)]
    internal bool CrossedRangeFunc { get; set; }

    // The panic this thread's catches adopted last (CaptureThrowSite): the one a range-over-func loop's
    // early stop is unwinding for, when it is unwinding for one. Read only on that path.
    [ThreadStatic] private static PanicException? t_lastAdopted;

    // A range-over-func loop stopped early. If it stopped for a panic, the filters already adopted it
    // (first pass) before the loop's Dispose runs (second pass), so the last adopted panic is it; a break
    // marks an older panic instead, which only refuses that panic's splices (the safe direction).
    internal static void MarkLastAdoptedCrossedRangeFunc()
    {
        if (t_lastAdopted is { } panic)
            panic.CrossedRangeFunc = true;
    }

    internal static void ResetThread() => t_lastAdopted = null;

    // Snapshot the throw site the first time this panic is caught. `thrown` is the exception that
    // actually travelled: for a mapped .NET runtime error (nil deref, divide by zero) THIS instance
    // was synthesized by RuntimeErrorPanic and was never thrown, so only the original carries frames.
    // One capture serves both properties: the first catch is the panic's own site, and PanicTrace
    // takes the same snapshot unless a re-panic's inheritance has already set it.
    internal void CaptureThrowSite(Exception thrown)
    {
        Catches++;
        t_lastAdopted = this;

        if (SiteTrace is not null)
            return;

        SiteTrace = new StackTrace(thrown, fNeedFileInfo: false);
        PanicTrace ??= SiteTrace;
    }

    // Adopt the origin of the panic being handled when this one is raised from a deferred call —
    // Go's traceback there still shows the original panic's frames. Deliberately UNCONDITIONAL,
    // overriding CaptureThrowSite's first-adoption snapshot: the drain's own IsPanic filter adopts
    // the new panic (at the deferred call's site) before the drain can express the inheritance, and
    // an explicit re-panic inheritance is the stronger semantic at this method's one call site.
    internal void InheritThrowSite(PanicException origin)
    {
        PanicTrace = origin.PanicTrace ?? PanicTrace;
    }
}

/// <summary>
/// The runtime frames between <c>runtime.gopanic</c> and a panic's site in Go's traceback: a hardware
/// fault reaches gopanic through <c>sigpanic</c> and <c>panicmem</c>, an integer divide through
/// <c>panicdivide</c>, and an explicit <c>panic(v)</c> directly.
/// </summary>
internal enum PanicFaultKind
{
    /// <summary>The default: a panic raised through a runtime frame that is not modelled (goPanicIndex, panicdottypeE, mapassign, closechan, ...).</summary>
    Unmodelled,

    /// <summary>An explicit <c>panic(v)</c> (builtin.panic): no runtime frame between gopanic and the site.</summary>
    Explicit,

    /// <summary>A nil dereference: sigpanic and panicmem.</summary>
    Memory,

    /// <summary>An integer divide by zero: panicdivide.</summary>
    Divide,

    /// <summary>A value method called through a nil *T by Go's autogenerated `(*T).M` wrapper: panicwrap.</summary>
    Panicwrap
}
