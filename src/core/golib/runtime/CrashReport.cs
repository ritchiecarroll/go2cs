// CrashReport.cs - Gbtc
// Copyright © 2026 The go2cs Authors. All rights reserved.
//
// Use of this source code is governed by an MIT-style license
// that can be found in the LICENSE file.

using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.Reflection;
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
    /// <para>
    /// The first lines are the panic CHAIN (Go's <c>printpanics</c>): every panic still linked beneath this one
    /// (<see cref="PanicException.Link"/>), oldest first, each after the first on a TAB, with <c> [recovered]</c> on a
    /// panic a recover() stopped. A panic re-raised with the same interface words as the panic directly beneath it is
    /// printed once, on the older line, as <c> [recovered, repanicked]</c> when that one was recovered.
    /// </para>
    /// </remarks>
    public static string Format(PanicException panic, Exception thrown)
    {
        string report = PanicChain(panic);
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

    // Go's preprintpanics, then printpanics (runtime/panic.go).
    private static string PanicChain(PanicException panic)
    {
        // Newest first, as the links run. A link is only ever set to an older panic, so the walk ends; the
        // set is a guard for a report that must be written whatever state it finds.
        List<PanicException> chain = [];
        HashSet<PanicException> seen = new(ReferenceEqualityComparer.Instance);

        for (PanicException? link = panic; link is not null && seen.Add(link); link = link.Link)
            chain.Add(link);

        // preprintpanics, newest first: a panic holding the same interface words as the one beneath it marks that
        // one repanicked and is not rendered; every other value is rendered now, Error() or String() included.
        bool[] repanicked = new bool[chain.Count];
        string?[] values = new string?[chain.Count];

        for (int i = 0; i < chain.Count; i++)
        {
            if (i + 1 < chain.Count && SameInterfaceWords(chain[i], chain[i + 1]))
            {
                repanicked[i + 1] = true;
                continue;
            }

            try
            {
                values[i] = chain[i].Message;
            }
            catch (Exception)
            {
                values[i] = "panic while printing panic value";
            }
        }

        // printpanics, oldest first: a panic whose older neighbour it re-raised is not printed again.
        StringBuilder text = new();

        for (int i = chain.Count - 1; i >= 0; i--)
        {
            if (i + 1 < chain.Count)
            {
                if (repanicked[i + 1])
                    continue;

                text.Append('\t');
            }

            text.Append("panic: ").Append(values[i]);

            if (chain[i].Recovered)
                text.Append(repanicked[i] ? " [recovered, repanicked]" : " [recovered]");

            text.Append('\n');
        }

        return text.ToString();
    }

    // Go compares the two panic values' interface WORDS (`*efaceOf(&p.link.arg) == *efaceOf(&p.arg)`), not the
    // values. The same interface value re-raised is one word pair (recover()'s result, a variable holding it), and so
    // is any value the runtime answers from static data instead of an allocation.
    private static bool SameInterfaceWords(PanicException newer, PanicException older)
    {
        if (ReferenceEquals(newer.State, older.State))
            return true;

        // A Go string constant is one deduplicated read-only symbol; the C# string literal it was emitted as is interned.
        if (newer.StringSource is { } newerText && older.StringSource is { } olderText && ReferenceEquals(newerText, olderText))
            return true;

        return newer.State is { } left && older.State is { } right && left.GetType() == right.GetType() && SameDataWord(left, right);
    }

    // Whether two values of one dynamic type convert to the same interface data word (walk/convert.go's dataWord and
    // runtime/iface.go's convT*): a pointer-shaped value is its own word; a zero-size value is &zerobase; a bool or
    // one-byte integer, and a 2-, 4- or 8-byte scalar below 256, are staticuint64s; "" and a nil slice are &zeroVal;
    // a struct or array of one element converts as that element. Anything else is a fresh allocation every time.
    [UnconditionalSuppressMessage("Trimming", "IL2072", Justification = "A Go struct's fields are kept by the generated metadata GoFieldMetadata reads.")]
    private static bool SameDataWord(object left, object right)
    {
        switch (left)
        {
            case bool or byte or sbyte:
                return left.Equals(right);
            case short value:
                return SmallScalar((ushort)value, (ushort)(short)right);
            case ushort value:
                return SmallScalar(value, (ushort)right);
            case int value:
                return SmallScalar((uint)value, (uint)(int)right);
            case uint value:
                return SmallScalar(value, (uint)right);
            case long value:
                return SmallScalar((ulong)value, (ulong)(long)right);
            case ulong value:
                return SmallScalar(value, (ulong)right);
            case nint value:
                return SmallScalar((ulong)value, (ulong)(nint)right);
            case nuint value:
                return SmallScalar(value, (nuint)right);
            case float value:
                return SmallScalar(BitConverter.SingleToUInt32Bits(value), BitConverter.SingleToUInt32Bits((float)right));
            case double value:
                return SmallScalar(BitConverter.DoubleToUInt64Bits(value), BitConverter.DoubleToUInt64Bits((double)right));
            case @string value:
                return value.Length == 0 && ((@string)right).Length == 0;
            case ISlice value:
                return value.IsNil && ((ISlice)right).IsNil;
            case IMap or Delegate:
                return left.Equals(right);
            case INilPointer or IChannel when left is IGoEquatable pointer:
                return pointer.GoEquals(right);
            case IArray value:
                return value.Length == 0 || value.Length == 1 && SameElement(value[0], ((IArray)right)[0]);
        }

        Type type = left.GetType();

        if (!type.IsValueType || type.IsPrimitive || type.IsEnum)
            return false;

        if (GoZeroSizeFacts.Classify(type))
            return true;

        FieldInfo[] fields = GoFieldMetadata.InstanceFields(type);

        // An interface field is two words, so its struct is allocated like any other.
        return fields.Length == 1 && !fields[0].FieldType.IsInterface && SameElement(fields[0].GetValue(left), fields[0].GetValue(right));

        static bool SmallScalar(ulong left, ulong right) => left == right && left < 256;

        static bool SameElement(object? left, object? right) =>
            left is null ? right is null : right is not null && left.GetType() == right.GetType() && SameDataWord(left, right);
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
        // Go's panicCheck1: an unrecovered bounds or shift panic raised in package runtime reports as
        // the fatal error it is, not as a panic.
        RuntimePanicCheck.FatalIfRaisedInRuntime(panic);

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
