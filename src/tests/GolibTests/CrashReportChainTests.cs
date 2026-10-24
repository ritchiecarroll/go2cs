#nullable enable

using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using go;
using go.golib;
using static go.builtin;

namespace GolibTests;

/// <summary>
/// The panic CHAIN at the head of Go's crash report (runtime/panic.go's preprintpanics and printpanics): every panic
/// still linked beneath the one that escaped, oldest first, a TAB before every line after the first, <c> [recovered]</c>
/// on a panic a recover() stopped, and <c> [recovered, repanicked]</c> on one that was re-raised with the same interface
/// words, whose re-raise is then not printed again. Every expected text below is go1.26.10's own output for the same
/// program (linux/amd64, measured 2026-10-24); the first six are runtime's crash_test.go cases by name.
/// </summary>
[TestClass]
public class CrashReportChainTests
{
    private delegate void FrameBody(ref GoFrame frame);

    private Func<PanicException, Exception, string>? m_savedRenderer;

    [TestInitialize]
    public void NoTraceback()
    {
        // The chain is the whole report when no traceback renderer is registered.
        m_savedRenderer = CrashReport.TracebackRenderer;
        CrashReport.TracebackRenderer = null;
    }

    [TestCleanup]
    public void Restore()
    {
        CrashReport.TracebackRenderer = m_savedRenderer;
        Assert.IsNull(GoFuncRoot.InFlightPanic, "a panic outlived its arm");
        Assert.IsNull(recover(), "a recoverable panic outlived its sequence");
    }

    // One emitted frame, as RecoverModelTests drives it; returns the panic that escaped it, or null.
    private static PanicException? RunInFrame(FrameBody body)
    {
        try
        {
            GoFrame frame = default;

            try
            {
                body(ref frame);
            }
            catch (Exception ex) when (GoFrame.IsPanic(ex, out PanicException? p))
            {
                GoFrame.Capture(p);
            }
            finally
            {
                frame.Run();
            }
        }
        catch (PanicException escaped)
        {
            GoFuncRoot.CapturedPanicValue = null;
            return escaped;
        }

        return null;
    }

    private static void Call(FrameBody body)
    {
        PanicException? escaped = RunInFrame(body);

        if (escaped is not null)
            throw escaped;
    }

    private static string Report(FrameBody body)
    {
        PanicException? escaped = RunInFrame(body);

        Assert.IsNotNull(escaped, "the program must crash");
        return CrashReport.Format(escaped!, escaped!);
    }

    [TestMethod]
    public void RepanickedPanic()
    {
        // defer func() { panic(recover()) }(); panic("message")
        string report = Report((ref GoFrame frame) =>
        {
            frame.Push(() => throw panic(recover()));
            throw panic("message");
        });

        Assert.AreEqual("panic: message [recovered, repanicked]\n", report);
    }

    [TestMethod]
    public void RepanickedMiddlePanic()
    {
        string report = Report((ref GoFrame frame) =>
        {
            frame.Push(() => { recover(); throw panic("outer"); });

            Call((ref GoFrame f2) =>
            {
                f2.Push(() => throw panic(recover()));

                Call((ref GoFrame f3) =>
                {
                    f3.Push(() => { recover(); throw panic("middle"); });
                    throw panic("inner");
                });
            });
        });

        Assert.AreEqual("panic: inner [recovered]\n\tpanic: middle [recovered, repanicked]\n\tpanic: outer\n", report);
    }

    [TestMethod]
    public void RepanickedPanicSandwich()
    {
        // The outer value is re-raised two links up, not by the panic directly above it, so nothing is marked.
        object? outer = null;

        string report = Report((ref GoFrame frame) =>
        {
            frame.Push(() => { recover(); throw panic(outer); });

            Call((ref GoFrame f2) =>
            {
                f2.Push(() => { outer = recover(); throw panic("inner"); });
                throw panic("outer");
            });
        });

        Assert.AreEqual("panic: outer [recovered]\n\tpanic: inner [recovered]\n\tpanic: outer\n", report);
    }

    [TestMethod]
    public void DoublePanicWithSameValue()
    {
        // Issue 76099: the same value panicked twice and never recovered prints once, with no suffix.
        object e = (@string)"message";

        string report = Report((ref GoFrame frame) =>
        {
            frame.Push(() => throw panic(e));
            throw panic(e);
        });

        Assert.AreEqual("panic: message\n", report);
    }

    [TestMethod]
    public void RecursivePanic4()
    {
        string report = Report((ref GoFrame frame) =>
        {
            frame.Push(() => { recover(); throw panic("second panic"); });
            throw panic("first panic");
        });

        Assert.AreEqual("panic: first panic [recovered]\n\tpanic: second panic\n", report);
    }

    [TestMethod]
    public void APanicRaisedInACalleeOfADeferredCallLinksToThePanicItInterrupted()
    {
        // defer func() { func() { defer func() {}(); panic("second") }() }(); panic("first")
        // The callee's own frame catches "second" first, so the deferring frame's catch is not its first.
        string report = Report((ref GoFrame frame) =>
        {
            frame.Push(() => Call((ref GoFrame callee) =>
            {
                callee.Push(() => { });
                throw panic("second");
            }));

            throw panic("first");
        });

        Assert.AreEqual("panic: first\n\tpanic: second\n", report);
    }

    // preprintpanics compares INTERFACE WORDS, not values: the runtime's static data makes some separately
    // converted values identical words, and an allocation never is.

    private static string RecoverThenPanic(Func<object?> first, Func<object?> second)
    {
        return Report((ref GoFrame frame) =>
        {
            frame.Push(() => { recover(); throw panic(second()); });
            throw panic(first());
        });
    }

    [TestMethod]
    public void ASmallIntegerIsTheSameStaticWord()
    {
        // convT64 answers a value below 256 from staticuint64s, computed or not.
        Assert.AreEqual("panic: 1 [recovered, repanicked]\n", RecoverThenPanic(static () => (nint)1, static () => (nint)1));
    }

    [TestMethod]
    public void AComputedLargeIntegerIsAnAllocationEachTime()
    {
        nint n = Environment.ProcessorCount > 0 ? 3 : 0;

        Assert.AreEqual("panic: 1001 [recovered]\n\tpanic: 1001\n", RecoverThenPanic(() => 998 + n, () => 998 + n));
    }

    [TestMethod]
    public void DifferentValuesOrTypesAreNeverTheSameWord()
    {
        Assert.AreEqual("panic: 2 [recovered]\n\tpanic: 1\n", RecoverThenPanic(static () => (nint)2, static () => (nint)1));
        Assert.AreEqual("panic: 1 [recovered]\n\tpanic: 1\n", RecoverThenPanic(static () => (nint)1, static () => 1));
    }

    [TestMethod]
    public void ABoolOrSingleByteIsTheSameStaticWord()
    {
        // Go prints `true`; the value's own text is PanicText's (it prints `True`), so only the chain is pinned here.
        AssertOneRepanickedLine(RecoverThenPanic(static () => true, static () => true));
        Assert.AreEqual("panic: 200 [recovered, repanicked]\n", RecoverThenPanic(static () => (byte)200, static () => (byte)200));
    }

    [TestMethod]
    public void AStringLiteralIsOneReadOnlyWord()
    {
        // Go's linker deduplicates a constant's read-only data; a C# string literal is interned.
        Assert.AreEqual("panic: x [recovered, repanicked]\n", RecoverThenPanic(static () => "x", static () => "x"));
    }

    [TestMethod]
    public void AComputedStringIsAnAllocationEachTime()
    {
        Assert.AreEqual("panic: yyy [recovered]\n\tpanic: yyy\n",
            RecoverThenPanic(static () => Repeat("y", 3), static () => Repeat("y", 3)));

        // strings.Repeat: a string built at run time, already a Go string when it reaches panic.
        static @string Repeat(string s, int count) => string.Concat(System.Linq.Enumerable.Repeat(s, count));
    }

    [TestMethod]
    public void TheEmptyStringIsTheZeroValueWord()
    {
        Assert.AreEqual("panic:  [recovered, repanicked]\n", RecoverThenPanic(static () => "", static () => new @string(string.Empty)));
    }

    [TestMethod]
    public void TheSameErrorValueIsTheSameWord()
    {
        // err := errors.New("e") re-raised is one pointer; two errors.New calls are two.
        error err = errors_package.New("e");

        Assert.AreEqual("panic: e [recovered, repanicked]\n", RecoverThenPanic(() => err, () => err));
        Assert.AreEqual("panic: e [recovered]\n\tpanic: e\n", RecoverThenPanic(static () => errors_package.New("e"), static () => errors_package.New("e")));
    }

    [TestMethod]
    public void PointerShapedValuesAreTheirOwnWord()
    {
        // A pointer, a map and a nil slice: Go prints the address, so only the suffix and the line count are pinned.
        ж<nint> p = @new<nint>();
        map<nint, nint> m = new(0);

        AssertOneRepanickedLine(RecoverThenPanic(() => p, () => p));
        AssertOneRepanickedLine(RecoverThenPanic(() => m, () => m));
        AssertOneRepanickedLine(RecoverThenPanic(static () => default(slice<nint>), static () => default(slice<nint>)));
        AssertOneRepanickedLine(RecoverThenPanic(static () => new ZeroSize(), static () => new ZeroSize()));
    }

    private struct ZeroSize;

    private static void AssertOneRepanickedLine(string report)
    {
        Assert.IsTrue(report.StartsWith("panic: ", StringComparison.Ordinal) && report.EndsWith(" [recovered, repanicked]\n", StringComparison.Ordinal) &&
            report.IndexOf('\n') == report.Length - 1, report);
    }
}
