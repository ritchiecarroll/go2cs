// GoexitException.cs - Gbtc
// Copyright © 2026 The go2cs Authors. All rights reserved.
//
// Use of this source code is governed by an MIT-style license
// that can be found in the LICENSE file.

using System;
using System.Diagnostics;

namespace go;

/// <summary>
/// Unwinds the calling goroutine for <c>runtime.Goexit</c>.
/// </summary>
/// <remarks>
/// <para>
/// Deliberately NOT a <see cref="PanicException"/>. Go specifies that <c>recover()</c> returns
/// <c>nil</c> inside the deferred calls a Goexit runs — a defer cannot cancel a Goexit the way it
/// can cancel a panic — and golib's recover path captures only panic-convertible exceptions
/// (<c>GoFunc.Execute</c>'s filter, <c>RuntimeErrorPanic.TryAsPanic</c>). Because this type fails
/// that filter, it is invisible to <c>recover()</c> BY CONSTRUCTION: the recover path needs no
/// knowledge of Goexit at all.
/// </para>
/// <para>
/// The deferred calls still run. <c>GoFunc.HandleFinally</c> sits in a <c>finally</c>, so an
/// unwinding <see cref="GoexitException"/> pops the defer stack in the usual order exactly as a
/// panic does.
/// </para>
/// <para>
/// The goroutine root (<c>golib.Goroutine.Run</c>) swallows it, ending THAT goroutine and no other;
/// a <see cref="PanicException"/> reaching the same point keeps its fatal-crash path unchanged.
/// </para>
/// </remarks>
[DebuggerNonUserCode]
public class GoexitException : Exception
{
    public GoexitException() : this(fromMain: false)
    {
    }

    /// <summary>
    /// Unwinds for <c>runtime.Goexit</c>; <paramref name="fromMain"/> when the MAIN goroutine called it.
    /// </summary>
    /// <remarks>
    /// A goroutine's Goexit ends at its root (<c>golib.Goroutine.Run</c>). Main's has no root: a converted
    /// <c>main</c> is the CLR entry point, and an exception nothing catches ends the process WITHOUT running a
    /// single <c>finally</c>, so main's deferred calls would never run. So every deferring frame catches a
    /// from-main Goexit (<see cref="GoFrame.IsPanic"/>, through a marker <c>recover()</c> cannot read), runs its
    /// deferred calls, and raises it again at its end; past the outermost one, golib parks the main thread until
    /// no goroutine can run (<see cref="golib.MainGoexit"/>). docs/DESIGN-goexit-main.md.
    /// </remarks>
    public GoexitException(bool fromMain) : base("runtime.Goexit")
    {
        FromMain = fromMain;

        // Nothing stops a Goexit, so once one is raised this goroutine ends on it. GoFrame.Run reads the
        // mark: Go shows runtime.Goexit beneath a deferred call that a Goexit is running, and that frame
        // is not modelled, so a panic such a call raises is left unowned (a missing splice, never a
        // wrong one). Cleared with the goroutine's other thread state (GoFuncRoot.ResetThread).
        MarkGoroutineExiting();
    }

    /// <summary>Whether the MAIN goroutine called <c>runtime.Goexit</c>.</summary>
    public bool FromMain { get; }

    [ThreadStatic] private static bool t_started;

    /// <summary>Whether a Goexit has been raised on this thread's goroutine.</summary>
    internal static bool Started => t_started;

    /// <summary>
    /// Marks this thread's goroutine as running a Goexit. The constructor calls it; so does every other
    /// Goexit that unwinds by a different exception or on a different thread than the one that raised it:
    /// the test host's FailNow/SkipNow (TestAbortException, its Goexit on the test's own thread), and the
    /// range-over-func adapter, which re-raises a seq's Goexit on the ranging goroutine.
    /// </summary>
    public static void MarkGoroutineExiting() => t_started = true;

    internal static void ResetThread() => t_started = false;
}
