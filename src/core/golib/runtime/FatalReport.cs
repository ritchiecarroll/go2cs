// FatalReport.cs - Gbtc
// Copyright © 2026 The go2cs Authors. All rights reserved.
//
// Use of this source code is governed by an MIT-style license
// that can be found in the LICENSE file.

using System;
using System.Diagnostics.CodeAnalysis;
using System.Runtime.CompilerServices;

namespace go.golib;

/// <summary>
/// Go's report for a FATAL error — <c>runtime.throw</c> and <c>runtime.fatal</c>, which no
/// <c>recover()</c> can see and which always take the process down with status 2.
/// </summary>
/// <remarks>
/// <para>
/// The sibling of <see cref="CrashReport"/> and deliberately shaped like it. That one owns the
/// PANIC path — a value nobody recovered — and this one owns the path Go's runtime takes when it
/// has decided the program cannot continue at all. The two are different in Go and are different
/// here: a panic is a value that travelled and can be caught until it reaches the top, while a
/// fatal is unrecoverable from the first instruction.
/// </para>
/// <para>
/// Go writes, to standard error and to the descriptor <c>debug.SetCrashOutput</c> configured:
/// <c>fatal error: </c> and the text, a BLANK line, the failing goroutine's header and frames, then
/// every other goroutine's block; and then it exits 2. The shape is measured at the corpus pin in
/// <c>docs/phase4/DESIGN-fatal-path.md</c> §1 rather than described from memory.
/// </para>
/// <para>
/// This lives in golib for the reason <see cref="CrashReport"/> states for itself, plus one more:
/// golib is the only assembly BELOW every converted package, and the fatal shims are declared in
/// three of them — <c>runtime</c> (<c>throw</c>, <c>fatal</c>), <c>sync</c>, and at Go 1.24 the new
/// <c>internal/sync</c>. A primitive in <c>runtime</c> could not serve the other two: neither
/// references it, and <c>internal/sync</c> referencing <c>sync</c> is the project-reference CYCLE
/// <c>check-solution-integrity</c>'s per-GOOS assertion exists to catch (the W1 class). One
/// primitive here, one-line forwards there.
/// </para>
/// <para>
/// golib cannot spell a Go frame name or map a converted <c>.cs</c> line back to its Go position:
/// that machinery is <c>core/runtime</c>'s, which sits above this layer. The dependency inverts
/// exactly as it does for the crash report and for the divide-by-zero panic VALUE — golib declares
/// the hook, the runtime package fills it from its own module initializer. With nothing registered
/// the report is <c>fatal error: &lt;text&gt;</c> and nothing more, which is strictly better than
/// what preceded this (Go's text followed by a .NET exception dump naming <c>getcallerpc</c>): an
/// uninstalled renderer costs the traceback and can never produce a wrong one.
/// </para>
/// </remarks>
[EditorBrowsable(EditorBrowsableState.Never)] public static class FatalReport
{
    /// <summary>
    /// Renders the failing goroutine's header and Go-spelled frames, and every other goroutine's
    /// block beneath them. Registered by the converted <c>runtime</c> package;
    /// <see langword="null"/> until it is.
    /// </summary>
    /// <remarks>
    /// <para>
    /// The <see cref="bool"/> is Go's <c>throwType</c> axis: <see langword="true"/> for
    /// <c>fatal</c> (<c>throwTypeUser</c> — user code is at fault, so Go omits system goroutines
    /// and runtime frames unless <c>GOTRACEBACK</c> is <c>system</c> or higher), and
    /// <see langword="false"/> for <c>throw</c> (<c>throwTypeRuntime</c> — the runtime itself is at
    /// fault, and Go's <c>gotraceback</c> raises the level so that they ARE shown). The renderer
    /// owns that decision because <c>GOTRACEBACK</c> is read on the runtime side.
    /// </para>
    /// <para>
    /// It renders the goroutine blocks only. The <c>fatal error:</c> line and the blank line above
    /// the first header are <see cref="Format"/>'s, so a renderer can never move them.
    /// </para>
    /// </remarks>
    public static Func<bool, string>? TracebackRenderer { get; set; }

    /// <summary>
    /// Renders a deadlock report's goroutine blocks: every user goroutine in goid order, each with the
    /// header and the frames it captured as it entered its forever wait. Registered by the converted
    /// <c>runtime</c> package; <see langword="null"/> until it is.
    /// </summary>
    /// <remarks>
    /// Separate from <see cref="TracebackRenderer"/> because a deadlock has no failing goroutine: Go's
    /// checkdead reports from the scheduler, so no block is the "current" one with live frames, and every
    /// goroutine is parked. The thread that reports here is merely the first poller to see that.
    /// </remarks>
    public static Func<string>? DeadlockRenderer { get; set; }

    /// <summary>
    /// The report text: <c>fatal error: &lt;text&gt;</c>, a blank line, and the goroutine blocks.
    /// </summary>
    /// <remarks>
    /// Defensive in the same place and for the same reason <see cref="CrashReport.Format"/> is: a
    /// traceback is diagnostic output and must never be the thing that takes the report down. A
    /// report ABOUT the reporter is worse than the divergence it would describe, and the operator
    /// still gets the line that says what happened.
    /// </remarks>
    public static string Format(string text, bool userFault)
    {
        if (TracebackRenderer is null)
            CrashReport.EnsureRuntimeInitialized();

        Func<bool, string>? renderer = TracebackRenderer;

        return Format(text, renderer is null ? null : () => renderer(userFault));
    }

    // The report text over any renderer of the goroutine blocks: the fatal line, then a blank line and the
    // blocks when the renderer produced any.
    private static string Format(string text, Func<string>? renderer)
    {
        string report = $"fatal error: {text}\n";

        if (renderer is null)
            return report;

        try
        {
            string traceback = renderer();

            if (!string.IsNullOrEmpty(traceback))
                report = $"{report}\n{traceback}";
        }
        catch (Exception)
        {
        }

        return report;
    }

    /// <summary>
    /// Go's <c>runtime.throw</c> / <c>runtime.fatal</c>: write the report and terminate the process
    /// with status 2. Never returns, and is not recoverable by any means.
    /// </summary>
    /// <remarks>
    /// <para>
    /// <c>NoInlining</c> is load-bearing rather than defensive. The registered renderer locates the
    /// frame it should start rendering from by IDENTITY — the first frame above this class's own —
    /// which is only a boundary while this method HAS a frame. It is the same rule, for the same
    /// reason, that <c>runtime.Stack</c> records above its own attribute.
    /// </para>
    /// <para>
    /// stderr goes FIRST and is guarded on its own, then the crash descriptor: a failure teeing to
    /// the crash file must not cost the report the operator will actually read, and neither write
    /// may throw out of here, because the exit that follows must happen on every path. Go's
    /// <c>debug.SetCrashOutput</c> documents itself as covering "unhandled panics and other fatal
    /// errors", so the fatal path tees to the same descriptor the panic path does — which is why
    /// this reuses <see cref="CrashReport"/>'s writer rather than growing a second one.
    /// </para>
    /// </remarks>
    [DoesNotReturn]
    [MethodImpl(MethodImplOptions.NoInlining)]
    public static void Fatal(string text, bool userFault)
    {
        // Go's startpanic_m sets panicking for a throw too: a main that returns meanwhile waits for this report.
        MainExit.BeginFatal(2);

        Report(Format(text, userFault));
    }

    /// <summary>
    /// Go's checkdead deadlock report, <c>fatal error: all goroutines are asleep - deadlock!</c>: the line, a
    /// blank line, and a block for every user goroutine with the frames of its forever wait; then exit status 2.
    /// Never returns.
    /// </summary>
    /// <remarks>
    /// A Debug golib throws instead, as <c>builtin.fatal</c> has always done there, so a debugger stops at
    /// the report.
    /// </remarks>
    [DoesNotReturn]
    public static void Deadlock(string text)
    {
    #if DEBUG
        throw new InvalidOperationException($"fatal error: {text} [2]");
    #else
        // Every goroutine in a forever wait polls, so each one finds the deadlock. The first claims the
        // report (Go's paniclk) and the others wait behind it and never print a second one.
        MainExit.AwaitReportOwner();
        MainExit.BeginFatal(2);

        if (DeadlockRenderer is null)
            CrashReport.EnsureRuntimeInitialized();

        Report(Format(text, DeadlockRenderer));
    #endif
    }

    // stderr first and guarded on its own, then the crash descriptor, then the exit, on every path (see Fatal).
    [DoesNotReturn]
    private static void Report(string report)
    {
        try
        {
            Console.Error.Write(report);
            Console.Error.Flush();
        }
        catch (Exception)
        {
        }

        CrashReport.WriteToCrashOutput(report);

        MainExit.FatalReported();
        MainExit.Exit(2);
    }
}
