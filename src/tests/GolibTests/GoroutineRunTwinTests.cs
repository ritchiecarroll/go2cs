// GoroutineRunTwinTests.cs - Gbtc
// Copyright © 2026 The go2cs Authors. All rights reserved.
//
// Use of this source code is governed by an MIT-style license
// that can be found in the LICENSE file.

#nullable enable
#pragma warning disable CS1998 // the twins here need no await: what is measured is where they run

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Threading;
using System.Threading.Tasks;
using go;
using go.golib;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace GolibTests;

// Goroutine.RunTwin, the converted test host's twin entry (docs/phase4/DESIGN-mn-test-twin.md). It is body() exactly
// when the scheduler is off, when the calling thread holds no goroutine, when that goroutine is locked to its thread, or
// when it is already multiplexed; otherwise the goroutine leaves its thread, runs the twin multiplexed, and comes back.
// GO2CS_MN is read once per process, so each mode runs in a CHILD. Every row prints its own line, and the parent
// requires each line by name: a child that stopped early, or a row that never ran, fails here, not silently.
[TestClass]
public class GoroutineRunTwinTests
{
    // "mn" or "default": which scheduler the parent started the child under.
    private const string ChildVariable = "GO2CS_TEST_RUNTWIN_CHILD";

    private static readonly string[] s_mnRows = ["twin-runs", "panic-carried", "no-goroutine", "locked", "already-multiplexed"];
    private static readonly string[] s_defaultRows = ["scheduler-off"];

    // The CHILD. Run on its own (no variable) it does nothing.
    [TestMethod]
    public void ChildRunTwinRows()
    {
        string? mode = Environment.GetEnvironmentVariable(ChildVariable);

        if (mode is not ("mn" or "default"))
            return;

        Assert.AreEqual(mode == "mn", GoScheduler.Enabled, $"the child runs in {mode} mode");

        if (mode == "default")
        {
            // Scheduler off: body() on the calling goroutine, and the twin is never asked for.
            OnGoroutineThread(entered =>
            {
                (bool body, bool twin) = Run();
                Assert.IsTrue(body && !twin, "scheduler off: body ran, twin did not");
                Assert.AreSame(entered, Goroutine.Current, "scheduler off: the goroutine never left");
            });
            Row("scheduler-off");
            return;
        }

        // The positive control: a goroutine on its own thread leaves it, runs the twin on a carrier, and comes back.
        OnGoroutineThread(entered =>
        {
            int thread = Environment.CurrentManagedThreadId;
            Goroutine? inTwin = null;
            int twinThread = 0;
            bool body = false;

            Goroutine.RunTwin(() => body = true, async () =>
            {
                inTwin = Goroutine.Current;
                twinThread = Environment.CurrentManagedThreadId;
            });

            Assert.IsFalse(body, "the twin path does not run body()");
            Assert.AreSame(entered, inTwin, "the twin runs as the same goroutine");
            Assert.AreNotEqual(thread, twinThread, "the twin runs on a carrier, not on the calling thread");
            Assert.AreSame(entered, Goroutine.Current, "the calling thread holds the goroutine again");
        });
        Row("twin-runs");

        // What leaves the twin leaves RunTwin, on the calling thread: the same exception object.
        OnGoroutineThread(entered =>
        {
            InvalidOperationException thrown = new("from the twin");
            Exception? caught = null;

            try
            {
                Goroutine.RunTwin(() => { }, async () => throw thrown);
            }
            catch (Exception ex)
            {
                caught = ex;
            }

            Assert.AreSame(thrown, caught, "the twin's exception is rethrown from RunTwin");
            Assert.AreSame(entered, Goroutine.Current, "the goroutine is back after a throw");
        });
        Row("panic-carried");

        // A thread that holds no goroutine has no goroutine to move: body().
        OnThread(() =>
        {
            Assert.IsNull(Goroutine.Current, "the thread holds no goroutine");
            (bool body, bool twin) = Run();
            Assert.IsTrue(body && !twin, "no goroutine: body ran, twin did not");
        });
        Row("no-goroutine");

        // A goroutine locked to its thread (runtime.LockOSThread) stays on it: body().
        OnGoroutineThread(entered =>
        {
            runtime_package.LockOSThread();

            try
            {
                (bool body, bool twin) = Run();
                Assert.IsTrue(body && !twin, "locked: body ran, twin did not");
                Assert.AreSame(entered, Goroutine.Current, "locked: the goroutine never left");
            }
            finally
            {
                runtime_package.UnlockOSThread();
            }
        });
        Row("locked");

        // Inside a twin the goroutine is already multiplexed: there is no thread to leave, so body().
        OnGoroutineThread(_ =>
        {
            (bool body, bool twin) inner = default;
            Goroutine.RunTwin(() => { }, async () => inner = Run());
            Assert.IsTrue(inner.body && !inner.twin, "already multiplexed: body ran, twin did not");
        });
        Row("already-multiplexed");
    }

    [TestMethod]
    public void RunTwinRowsUnderTheScheduler()
    {
        RunChild("mn", s_mnRows);
    }

    [TestMethod]
    public void RunTwinRowsInDefaultMode()
    {
        RunChild("default", s_defaultRows);
    }

    private static (bool Body, bool Twin) Run()
    {
        bool body = false, twin = false;
        Goroutine.RunTwin(() => body = true, async () => twin = true);
        return (body, twin);
    }

    private static void Row(string name) =>
        Console.WriteLine($"RUNTWIN ROW {name} ok");

    private static void OnGoroutineThread(Action<Goroutine> action) =>
        OnThread(() =>
        {
            using Goroutine.Scope scope = Goroutine.Enter();
            action(Goroutine.Current ?? throw new InvalidOperationException("Goroutine.Enter left no current goroutine"));
        });

    private static void OnThread(Action action)
    {
        Exception? failure = null;

        Thread thread = new(() =>
        {
            try
            {
                action();
            }
            catch (Exception ex)
            {
                failure = ex;
            }
        });

        thread.Start();

        if (!thread.Join(60_000))
            Assert.Fail("a RunTwin row did not finish in 60 s");

        if (failure is not null)
            System.Runtime.ExceptionServices.ExceptionDispatchInfo.Capture(failure).Throw();
    }

    private static void RunChild(string mode, IReadOnlyList<string> rows)
    {
        string filter = $"FullyQualifiedName={typeof(GoroutineRunTwinTests).FullName}.{nameof(ChildRunTwinRows)}";

        ProcessStartInfo start = new("dotnet", ["test", typeof(GoroutineRunTwinTests).Assembly.Location, "--filter", filter, "--logger", "console;verbosity=detailed"])
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
            Assert.Fail("the RunTwin child did not finish in 180 s");
        }

        string child = stdout.Result + stderr.Result;

        Assert.AreEqual(0, process.ExitCode, $"the child failed:\n{child}");
        Assert.IsTrue(child.Contains("Passed!", StringComparison.Ordinal) || child.Contains("Passed:", StringComparison.Ordinal), $"the child test ran and passed:\n{child}");

        foreach (string row in rows)
            Assert.IsTrue(child.Contains($"RUNTWIN ROW {row} ok", StringComparison.Ordinal), $"the child ran row {row}:\n{child}");
    }
}
