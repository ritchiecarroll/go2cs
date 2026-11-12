// MainThreadTests.cs - Gbtc
// Copyright © 2026 The go2cs Authors. All rights reserved.
//
// Use of this source code is governed by an MIT-style license
// that can be found in the LICENSE file.

using System;
using System.Runtime.CompilerServices;
using System.Threading;
using go.golib;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace GolibTests;

// C46: a converted program's main runs as the main goroutine on a thread reserving Go's main-goroutine headroom, except
// when an init function locked the main goroutine to the startup thread, where Go invokes main on that thread.
[TestClass]
public class MainThreadTests
{
    // Each level of Recurse holds a frame of its own; 1,000,000 levels need far more than any OS's default thread stack
    // (1 MiB on Windows, 8 MiB on linux and macOS) and far less than 1 GiB.
    private const int Levels = 1_000_000;

    [MethodImpl(MethodImplOptions.NoInlining)]
    private static int Recurse(int n) => n == 0 ? 0 : Recurse(n - 1) + 1;

    [TestMethod]
    public void MainRunsAsGoroutineOneOnAnotherThread()
    {
        int caller = Environment.CurrentManagedThreadId;
        int ran = 0;
        Goroutine? identity = null;

        MainThread.Run(() =>
        {
            ran = Environment.CurrentManagedThreadId;
            identity = Goroutine.Current;
        });

        Assert.AreNotEqual(0, ran, "main did not run");
        Assert.AreNotEqual(caller, ran, "main ran on the calling thread");
        Assert.IsNotNull(identity, "main ran with no goroutine identity");
        Assert.IsTrue(identity.IsMain, "main ran as a goroutine other than goroutine 1");
        Assert.AreEqual(1L, identity.Id, "the main goroutine's id");
    }

    [TestMethod]
    public void MainRecursesFarPastTheDefaultThreadStack()
    {
        int depth = 0;

        MainThread.Run(() => depth = Recurse(Levels));

        Assert.AreEqual(Levels, depth);
    }

    // Start is the converted-test host's form: TestMain runs as goroutine 1 on the deep thread while the startup thread
    // waits on the package deadline.
    [TestMethod]
    public void StartRunsMainAsGoroutineOneOnTheDeepThreadAndCompletesWithItsResult()
    {
        int caller = Environment.CurrentManagedThreadId;
        int ran = 0;
        Goroutine? identity = null;

        System.Threading.Tasks.Task<long> run = MainThread.Start(() =>
        {
            ran = Environment.CurrentManagedThreadId;
            identity = Goroutine.Current;
            return (long)Recurse(Levels);
        });

        Assert.IsTrue(run.Wait(TimeSpan.FromSeconds(60)), "main did not finish");
        Assert.AreEqual((long)Levels, run.Result);
        Assert.AreNotEqual(caller, ran, "main ran on the calling thread");
        Assert.IsNotNull(identity);
        Assert.IsTrue(identity.IsMain, "main ran as a goroutine other than goroutine 1");
    }

    [TestMethod]
    public void StartFaultsWithWhatEscapedMain()
    {
        System.Threading.Tasks.Task<int> run = MainThread.Start<int>(() => throw new InvalidOperationException("escaped main"));

        AggregateException wrapped = Assert.ThrowsException<AggregateException>(() => run.Wait(TimeSpan.FromSeconds(60)));
        Assert.IsInstanceOfType(wrapped.InnerException, typeof(InvalidOperationException));
        Assert.AreEqual("escaped main", wrapped.InnerException!.Message);
    }

    [TestMethod]
    public void MainLockedByInitRunsOnTheStartupThread()
    {
        // Stand in for the startup thread: a thread that carries goroutine 1, as golib's module initializer leaves the
        // process's startup thread, and on which an init function called runtime.LockOSThread.
        int startup = 0, ran = 0;
        Exception? failure = null;

        Thread thread = new(() =>
        {
            try
            {
                using Goroutine.Scope scope = Goroutine.EnterAsMain();
                startup = Environment.CurrentManagedThreadId;
                Goroutine.NoteLockOSThread();

                try
                {
                    MainThread.Run(() => ran = Environment.CurrentManagedThreadId);
                }
                finally
                {
                    Goroutine.NoteUnlockOSThread();
                }
            }
            catch (Exception ex)
            {
                failure = ex;
            }
        });

        thread.Start();
        thread.Join();

        Assert.IsNull(failure, failure?.ToString());
        Assert.AreNotEqual(0, ran, "main did not run");
        Assert.AreEqual(startup, ran, "main left the thread an init function locked it to");
    }

    [TestMethod]
    public void AnUnlockedMainLeavesTheStartupThread()
    {
        // The control for the arm above: the same stand-in, locked and then unlocked before main, hands main over.
        int startup = 0, ran = 0;
        Exception? failure = null;

        Thread thread = new(() =>
        {
            try
            {
                using Goroutine.Scope scope = Goroutine.EnterAsMain();
                startup = Environment.CurrentManagedThreadId;
                Goroutine.NoteLockOSThread();
                Goroutine.NoteUnlockOSThread();
                Goroutine.NoteUnlockOSThread(); // an unmatched unlock does nothing, as in Go

                MainThread.Run(() => ran = Environment.CurrentManagedThreadId);
            }
            catch (Exception ex)
            {
                failure = ex;
            }
        });

        thread.Start();
        thread.Join();

        Assert.IsNull(failure, failure?.ToString());
        Assert.AreNotEqual(0, ran, "main did not run");
        Assert.AreNotEqual(startup, ran, "an unlocked main stayed on the startup thread");
    }
}
