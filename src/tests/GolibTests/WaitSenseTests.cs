// WaitSenseTests.cs - Gbtc
// Copyright © 2026 The go2cs Authors. All rights reserved.
//
// Use of this source code is governed by an MIT-style license
// that can be found in the LICENSE file.

using System;
using System.Diagnostics;
using System.IO.Pipes;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace GolibTests;

/// <summary>
/// The premise of N5's sensing (DESIGN-mn-n5-invisible-blocking.md, 3a and prediction I0): a thread blocked in a managed
/// wait reads <see cref="System.Threading.ThreadState.WaitSleepJoin"/> when ANOTHER thread reads its state, and a thread
/// that spins or blocks in native code does not. The M:N monitor relies on that read to retake the P of a carrier stuck
/// in a wait it cannot otherwise see, so this row must hold on every OS the scheduler runs on. It needs no scheduler.
/// </summary>
[TestClass]
public class WaitSenseTests
{
    private const int BlockMs = 1500;

    [DataTestMethod]
    [DataRow("Thread.Sleep")]
    [DataRow("Monitor.Enter")]
    [DataRow("Monitor.Wait")]
    [DataRow("Lock.Enter")]
    [DataRow("SemaphoreSlim.Wait")]
    [DataRow("ManualResetEventSlim.Wait")]
    [DataRow("ManualResetEvent.WaitOne")]
    [DataRow("Task.Wait")]
    public void AManagedWaitReadsWaitSleepJoinFromAnotherThread(string wait)
    {
        object monitor = new();
        Lock gate = new();
        using SemaphoreSlim semaphore = new(0);
        using ManualResetEventSlim slim = new(false);
        using ManualResetEvent manual = new(false);
        TaskCompletionSource source = new();

        Action body = wait switch
        {
            "Thread.Sleep" => () => Thread.Sleep(BlockMs),
            "Monitor.Enter" => () => { lock (monitor) { } },
            "Monitor.Wait" => () => { lock (monitor) Monitor.Wait(monitor, BlockMs); },
            "Lock.Enter" => () => { lock (gate) { } },
            "SemaphoreSlim.Wait" => () => semaphore.Wait(BlockMs),
            "ManualResetEventSlim.Wait" => () => slim.Wait(BlockMs),
            "ManualResetEvent.WaitOne" => () => manual.WaitOne(BlockMs),
            "Task.Wait" => () => source.Task.Wait(BlockMs),
            _ => throw new ArgumentOutOfRangeException(nameof(wait))
        };

        // The contended rows hold the lock on this thread while the other blocks on it.
        bool holdMonitor = wait == "Monitor.Enter";
        bool holdGate = wait == "Lock.Enter";

        if (holdMonitor)
            Monitor.Enter(monitor);

        if (holdGate)
            gate.Enter();

        Thread blocked = new(() => body()) { IsBackground = true, Name = "wait-sense " + wait };

        try
        {
            blocked.Start();

            // The wait may spin first (SemaphoreSlim, ManualResetEventSlim, Lock): read until it settles, then read again.
            Assert.IsTrue(SeenWithin(blocked, 1000), $"{wait}: the blocked thread never read WaitSleepJoin ({blocked.ThreadState})");
            Thread.Sleep(50);
            Assert.IsTrue((blocked.ThreadState & System.Threading.ThreadState.WaitSleepJoin) != 0, $"{wait}: the second read was {blocked.ThreadState}");
        }
        finally
        {
            if (holdMonitor)
                Monitor.Exit(monitor);

            if (holdGate)
                gate.Exit();

            semaphore.Release();
            slim.Set();
            manual.Set();
            source.TrySetResult();
            lock (monitor) Monitor.PulseAll(monitor);
            blocked.Join(BlockMs * 2);
        }
    }

    [DataTestMethod]
    [DataRow("spin")]
    [DataRow("pipe read")]
    public void ASpinOrANativeBlockDoesNotReadWaitSleepJoin(string kind)
    {
        using AnonymousPipeServerStream server = new(PipeDirection.Out);
        using AnonymousPipeClientStream client = new(PipeDirection.In, server.ClientSafePipeHandle);
        bool stop = false;

        Action body = kind switch
        {
            "spin" => () => { Stopwatch watch = Stopwatch.StartNew(); while (!Volatile.Read(ref stop) && watch.ElapsedMilliseconds < BlockMs) { } },
            "pipe read" => () => { byte[] one = new byte[1]; _ = client.Read(one, 0, 1); },
            _ => throw new ArgumentOutOfRangeException(nameof(kind))
        };

        Thread running = new(() => body()) { IsBackground = true, Name = "wait-sense " + kind };

        try
        {
            running.Start();
            Thread.Sleep(50);
            Assert.IsFalse(SeenWithin(running, 200), $"{kind}: read WaitSleepJoin, so the monitor would sense it ({running.ThreadState})");
        }
        finally
        {
            Volatile.Write(ref stop, true);
            server.WriteByte(1);
            running.Join(BlockMs * 2);
        }
    }

    private static bool SeenWithin(Thread thread, int ms)
    {
        Stopwatch watch = Stopwatch.StartNew();

        while (watch.ElapsedMilliseconds < ms)
        {
            if ((thread.ThreadState & System.Threading.ThreadState.WaitSleepJoin) != 0)
                return true;

            Thread.Sleep(1);
        }

        return false;
    }
}
