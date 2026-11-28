using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Threading;
using System.Threading.Tasks;
using go;
using go.golib;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace GolibTests;

// Door 1 (docs/phase4/DESIGN-cgo-struct-array-element.md), P3's F1 row in both scheduler modes: while the element's
// mirror is out (C "blocked" in the call), another goroutine writes a different element of the same Go array, and the
// copy-back, which writes only the elements whose mirror bytes C changed, leaves that write standing. GO2CS_MN is read
// once per process, so each mode runs in a CHILD (the GoroutineRunTwinTests pattern), and the parent requires the row's
// line by name: a child that stopped early, or a row that never ran, fails here, not silently.
// Negative control: CopyBack writing every element (the snapshot compare removed) -> the row reads 1 where 42 is due.
[TestClass]
public unsafe class GoCgoElementsSchedulerTests
{
    private const string ChildVariable = "GO2CS_TEST_CGO_ELEMENTS_CHILD";

    private struct rec
    {
        public int32 A;
        public int32 B;
    }

    [TestMethod]
    public void ChildElementsRows()
    {
        string? mode = Environment.GetEnvironmentVariable(ChildVariable);

        if (mode is not ("mn" or "default"))
            return;

        Assert.AreEqual(mode == "mn", GoScheduler.Enabled, $"the child runs in {mode} mode");

        Exception? failure = null;

        Thread thread = new(() =>
        {
            try
            {
                using Goroutine.Scope scope = Goroutine.Enter();
                AConcurrentGoWriteSurvivesTheCopyBack();
            }
            catch (Exception exception)
            {
                failure = exception;
            }
        });

        thread.Start();

        if (!thread.Join(60_000))
            Assert.Fail("the elements row did not finish in 60 s");

        if (failure is not null)
            System.Runtime.ExceptionServices.ExceptionDispatchInfo.Capture(failure).Throw();

        Console.WriteLine("CGO ELEMENTS ROW concurrent-write ok");
    }

    private static void AConcurrentGoWriteSurvivesTheCopyBack()
    {
        array<rec> records = new(3);

        for (int i = 0; i < 3; i++)
            records[i] = new rec { A = i + 1, B = 10 * (i + 1) };

        rec[] backing = records.m_array;
        GoCgoElements<rec, rec> elements = new(backing, 1, default, static (ref rec m, in rec v, GoCgoCheck check) => m = v);
        GoCgoPin pin = elements.Pin;
        using ManualResetEventSlim written = new();

        fixed (byte* pinned = pin)
        {
            rec* element = (rec*)pin.Address(pinned);
            element->A = 99; // C writes the element it was handed

            // Another goroutine writes element 0 while the call is out.
            builtin.goǃ(() =>
            {
                backing[0].A = 42;
                written.Set();
            });

            Assert.IsTrue(written.Wait(30_000), "the goroutine ran while the call was out");
        }

        elements.CopyBack(static (ref rec v, in rec m, in rec s, ReadOnlySpan<GoCgoStackCopy> copies, string function) => v = m, default, "slow");

        Assert.AreEqual(42, backing[0].A, "the goroutine's write to an element C left alone survives the copy-back");
        Assert.AreEqual(99, backing[1].A, "C's write comes back");
        Assert.AreEqual(3, backing[2].A, "an element nobody wrote is unchanged");
    }

    [TestMethod]
    public void AConcurrentGoWriteSurvivesUnderTheScheduler() => RunChild("mn");

    [TestMethod]
    public void AConcurrentGoWriteSurvivesInDefaultMode() => RunChild("default");

    private static void RunChild(string mode)
    {
        string filter = $"FullyQualifiedName={typeof(GoCgoElementsSchedulerTests).FullName}.{nameof(ChildElementsRows)}";

        ProcessStartInfo start = new("dotnet", ["test", typeof(GoCgoElementsSchedulerTests).Assembly.Location, "--filter", filter, "--logger", "console;verbosity=detailed"])
        {
            RedirectStandardOutput = true,
            RedirectStandardError = true,
            UseShellExecute = false
        };

        start.Environment[ChildVariable] = mode;

        if (mode == "mn")
            start.Environment["GO2CS_MN"] = "1";
        else
            start.Environment.Remove("GO2CS_MN");

        using Process process = Process.Start(start)!;
        Task<string> stdout = process.StandardOutput.ReadToEndAsync();
        Task<string> stderr = process.StandardError.ReadToEndAsync();

        if (!process.WaitForExit(180_000))
        {
            process.Kill(entireProcessTree: true);
            Assert.Fail("the elements child did not finish in 180 s");
        }

        string child = stdout.Result + stderr.Result;

        Assert.AreEqual(0, process.ExitCode, $"the child failed:\n{child}");
        Assert.IsTrue(child.Contains("CGO ELEMENTS ROW concurrent-write ok", StringComparison.Ordinal), $"the child ran the row:\n{child}");
    }
}
