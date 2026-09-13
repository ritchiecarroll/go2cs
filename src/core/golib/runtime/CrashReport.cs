// CrashReport.cs - Gbtc
// Copyright © 2026 The go2cs Authors. All rights reserved.
//
// Use of this source code is governed by an MIT-style license
// that can be found in the LICENSE file.

using System;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.Text;
using Microsoft.Win32.SafeHandles;

namespace go.golib;

/// <summary>
/// Go's crash report for a panic that reached the top with nobody to recover it — the text, and
/// the two places it goes.
/// </summary>
/// <remarks>
/// <para>
/// Go's runtime writes four elements to standard error and dies with status 2: <c>panic: </c> and
/// the panic value, a BLANK line, <c>goroutine N [running]:</c>, and the traceback. The shape is
/// recorded verbatim in Go's own <c>runtime/debug/stack_test.go</c>, above the read-back in
/// <c>TestSetCrashOutput</c>, and nothing here invents any part of it —
/// <c>docs/phase4/DESIGN-crash-report.md</c>.
/// </para>
/// <para>
/// This lives in golib because golib is the only assembly both the Phase-4 test host and every
/// converted program share, and both of them decide what an escaped panic prints: golib's own
/// <c>AppDomain.UnhandledException</c> backstop (<c>builtin.InitializeGoLib</c>) for a converted
/// program, and <c>TestHost.Run</c>'s outer catch for the host. A RECOVERED panic never reaches
/// here.
/// </para>
/// <para>
/// golib cannot spell a Go frame name or map a converted <c>.cs</c> line back to its Go position:
/// that machinery is <c>core/runtime</c>'s, which sits above this layer. The dependency is
/// therefore inverted exactly as <see cref="RuntimeErrorPanic.IntegerDivideByZeroValue"/> already
/// inverts it for the divide-by-zero panic VALUE — golib declares the hook, the runtime package
/// fills it from its own module initializer. With nothing registered the report is
/// <c>panic: &lt;value&gt;</c> and nothing more, which is byte-identical to what golib printed
/// before this existed: an uninstalled renderer costs the traceback and can never produce a wrong
/// report.
/// </para>
/// </remarks>
public static class CrashReport
{
    /// <summary>
    /// Go's <c>^uintptr(0)</c> — "no additional crash output is configured".
    /// </summary>
    public static readonly nuint NoCrashOutput = ~(nuint)0;

    private static readonly object s_crashOutputLock = new();
    private static nuint s_crashOutputFd = NoCrashOutput;

    /// <summary>
    /// Renders the <c>goroutine N [running]:</c> header and the Go-spelled traceback beneath it, as
    /// one block, for a panic that is about to be reported. Registered by the converted
    /// <c>runtime</c> package; <see langword="null"/> until it is.
    /// </summary>
    /// <remarks>
    /// Takes BOTH the panic and the exception that actually travelled, because they are not always
    /// the same object: a runtime-error panic (nil dereference, divide by zero) is synthesized by
    /// <see cref="RuntimeErrorPanic"/> from the .NET exception and was never thrown, so only the
    /// original carries frames.
    /// </remarks>
    public static Func<PanicException, Exception, string>? TracebackRenderer { get; set; }

    /// <summary>
    /// The additional file descriptor a crash report is copied to, or <see cref="NoCrashOutput"/>.
    /// </summary>
    public static nuint CrashOutputFd
    {
        get
        {
            lock (s_crashOutputLock)
                return s_crashOutputFd;
        }
    }

    /// <summary>
    /// Sets the additional crash-output descriptor and returns the previous one — Go's
    /// <c>runtime.setCrashFD</c> contract, which <c>runtime/debug.SetCrashOutput</c> relies on to
    /// know whether it owns a descriptor to close.
    /// </summary>
    /// <remarks>
    /// The slot lives here rather than in <c>runtime/debug</c> because that is where Go keeps it:
    /// <c>runtime.crashFD</c>, reached by <c>//go:linkname runtime_setCrashFD runtime.setCrashFD</c>.
    /// golib is this project's runtime library, and it is also where the writer is.
    /// </remarks>
    public static nuint SetCrashOutputFd(nuint fd)
    {
        lock (s_crashOutputLock)
        {
            nuint previous = s_crashOutputFd;
            s_crashOutputFd = fd;
            return previous;
        }
    }

    /// <summary>
    /// Finds the Go panic inside an exception that has travelled through machinery which WRAPS —
    /// <c>Task.Wait</c>'s <see cref="AggregateException"/>, reflection's
    /// <c>TargetInvocationException</c> — and hands back the exception that carries the throw site
    /// along with it.
    /// </summary>
    /// <remarks>
    /// A Go panic is a Go panic whatever wrapped it in transit, and reporting the wrapper instead
    /// is precisely the defect this type exists to remove: the test host's escaped-panic catch
    /// printed <c>System.AggregateException: One or more errors occurred. (oops) ---&gt;
    /// go.PanicException: oops</c> over a CLR frame list where Go writes a crash report.
    /// </remarks>
    public static bool TryUnwrapPanic(Exception? exception, [NotNullWhen(true)] out PanicException? panic, [NotNullWhen(true)] out Exception? thrown)
    {
        if (exception is not null)
        {
            if (RuntimeErrorPanic.TryAsPanic(exception, out panic))
            {
                thrown = exception;
                return true;
            }

            if (exception is AggregateException aggregate)
            {
                foreach (Exception inner in aggregate.InnerExceptions)
                {
                    if (TryUnwrapPanic(inner, out panic, out thrown))
                        return true;
                }
            }
            else if (exception.InnerException is { } single)
            {
                return TryUnwrapPanic(single, out panic, out thrown);
            }
        }

        panic = null;
        thrown = null;
        return false;
    }

    /// <summary>
    /// Composes the report Go would print for this panic.
    /// </summary>
    /// <remarks>
    /// <para>
    /// Newlines are <c>\n</c>, Go's own, not the platform's — this is a report a Go program can be
    /// asked to read, not console decoration.
    /// </para>
    /// <para>
    /// The panic VALUE is <see cref="PanicException.Message"/>, which is Go's <c>preprintpanics</c>
    /// rule (an <c>error</c> prints its <c>Error()</c>, a <c>Stringer</c> its <c>String()</c>)
    /// computed once, lazily, at exactly the moment Go computes it. It is read rather than
    /// reimplemented, and it is read defensively: Go itself answers
    /// <c>"panic while printing panic value"</c> when the substitution faults, and a report about
    /// the reporter is worse than the divergence it would describe.
    /// </para>
    /// </remarks>
    public static string Format(PanicException panic, Exception thrown)
    {
        string value;

        try
        {
            value = panic.Message;
        }
        catch (Exception)
        {
            value = "panic while printing panic value";
        }

        string report = $"panic: {value}\n";
        Func<PanicException, Exception, string>? renderer = TracebackRenderer;

        if (renderer is null)
            return report;

        try
        {
            string traceback = renderer(panic, thrown);

            if (!string.IsNullOrEmpty(traceback))
                report = $"{report}\n{traceback}";
        }
        catch (Exception)
        {
            // A traceback is diagnostic output and must never be the thing that takes the report
            // down — the same posture the position-map reader takes for an unreadable assembly.
        }

        return report;
    }

    /// <summary>
    /// Writes the report to standard error and, when <c>debug.SetCrashOutput</c> has configured
    /// one, to the additional descriptor as well.
    /// </summary>
    /// <remarks>
    /// <para>
    /// stderr goes FIRST and is guarded on its own: a failure teeing to the crash file must not
    /// cost the report the operator will actually read. Neither write can throw out of here — the
    /// process is already dying, and the exit that follows must happen on every path.
    /// </para>
    /// <para>
    /// The asymmetry <c>TestSetCrashOutput</c> pins — stderr carries the program's own output AND
    /// the report, the crash file carries only the report — needs no rule: program output reaches
    /// stderr through <c>println</c>/<c>os.Stderr</c> and never through here, so the descriptor
    /// only ever receives what this method hands it.
    /// </para>
    /// </remarks>
    public static void Report(PanicException panic, Exception thrown)
    {
        string report = Format(panic, thrown);

        try
        {
            Console.Error.Write(report);
            Console.Error.Flush();
        }
        catch (Exception)
        {
        }

        WriteToCrashOutput(report);
    }

    // internal rather than private because the FATAL path tees to the same descriptor: Go's
    // debug.SetCrashOutput documents itself as covering "unhandled panics and other fatal errors",
    // and one writer for both is what keeps the two from drifting apart. FatalReport.Fatal is the
    // only other caller.
    internal static void WriteToCrashOutput(string report)
    {
        nuint fd = CrashOutputFd;

        if (fd == NoCrashOutput)
            return;

        try
        {
            // The descriptor is a real OS handle on both platform shapes — a DuplicateHandle result
            // on Windows, a dup'd file descriptor elsewhere — which is exactly what SafeFileHandle
            // means on each. It is held NON-OWNING: debug.SetCrashOutput duplicated it precisely so
            // it would outlive the caller's *os.File, and closing it here would defeat that.
            using SafeFileHandle handle = new((nint)fd, ownsHandle: false);
            using FileStream output = new(handle, FileAccess.Write);
            byte[] bytes = Encoding.UTF8.GetBytes(report);

            output.Write(bytes, 0, bytes.Length);
            output.Flush();
        }
        catch (Exception)
        {
        }
    }
}
