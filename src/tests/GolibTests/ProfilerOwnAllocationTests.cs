// ProfilerOwnAllocationTests.cs - Gbtc
// Copyright © 2026 The go2cs Authors. All rights reserved.
//
// Use of this source code is governed by an MIT-style license
// that can be found in the LICENSE file.

using System;
using System.Diagnostics;
using System.IO;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using go;
using go.golib;
using static go.runtime_package;

namespace GolibTests;

// THE PROFILER'S OWN OBJECTS ARE NOT GO ALLOCATIONS. Go's profilers allocate their bookkeeping outside
// mallocgc: a bucket comes from persistentalloc (mprof.go newBucket), and a block or mutex event's stack is
// read into the M's profStack. The memory profiler therefore never samples either, and that is what lets
// stkbucket allocate a bucket while it holds profInsertLock.
//
// The hand-owned newBucket allocated through the profiled doors (make and new). With a block or mutex event
// recording a NEW stack, a bucket allocation the memory profiler sampled called the recorder under
// profInsertLock, and the recorder's own stkbucket then took profInsertLock again on the same thread. The
// runtime mutex is not reentrant: the thread spun for ever. At Go's default MemProfileRate the sample is a
// rare draw, so the hang showed once in a long battery; at MemProfileRate = 1 every allocation is sampled
// and the first block event on a new stack hangs every time.
//
// Arms:
//   1. a child process (this assembly run again by `dotnet test`, filtered to the child's half and told to
//      run by GO2CS_PROFILER_OWN_ALLOCATION_CHILD_RECORD, as GodebugUpdateHookTests does): MemProfileRate = 1
//      with runtime's own recorder, block profile rate 1, one block event. The child bounds the event itself
//      and records "hung" when it does not return; the parent bounds the child and kills it. A hang is a
//      FAILED test, never a hung host.
//   2. in process, with the recorder replaced by one that only counts (it takes no lock, so it cannot
//      hang): a block event on a new stack, the same event again, and a mutex event each sample nothing.
//      Before the fix they read 4, 1 and 4: the event's stack buffer, and for a new stack the bucket's
//      three objects.
[TestClass]
public class ProfilerOwnAllocationTests
{
    private const string RecordVariable = "GO2CS_PROFILER_OWN_ALLOCATION_CHILD_RECORD";
    private const string Returned = "returned";
    private const string Hung = "hung";
    private const int HungExitCode = 3;

    // The event itself takes microseconds; the bound is for a starved box.
    private const int ChildEventBoundMilliseconds = 30_000;
    private const int ChildBoundMilliseconds = 180_000;

    // Each event is recorded under a root frame no other test carries, so its stack is one no profile holds
    // yet and stkbucket must allocate a bucket for it.
    [GoStackRoot("golibtests.profilerOwnAllocationBlock", "golibtests/profiler_own_allocation.go", 1)]
    [MethodImpl(MethodImplOptions.NoInlining | MethodImplOptions.NoOptimization)]
    private static void BlockEventOnItsOwnStack() => GoBlockEventProbe(1_000_000, 1);

    [GoStackRoot("golibtests.profilerOwnAllocationMutex", "golibtests/profiler_own_allocation.go", 2)]
    [MethodImpl(MethodImplOptions.NoInlining | MethodImplOptions.NoOptimization)]
    private static void MutexEventOnItsOwnStack() => GoSyncMutexEvent(1_000_000);

    [TestMethod]
    public void ABlockEventReturnsWhenEveryAllocationIsSampled()
    {
        string record = Path.Combine(Path.GetTempPath(), "g2cs-profiler-own-" + Guid.NewGuid().ToString("N") + ".txt");
        string filter = $"FullyQualifiedName={typeof(ProfilerOwnAllocationTests).FullName}.{nameof(ChildRecordsABlockEventWithEveryAllocationSampled)}";

        ProcessStartInfo start = new("dotnet", ["test", typeof(ProfilerOwnAllocationTests).Assembly.Location, "--filter", filter])
        {
            RedirectStandardOutput = true,
            RedirectStandardError = true,
            UseShellExecute = false
        };

        start.Environment[RecordVariable] = record;

        try
        {
            using Process process = Process.Start(start)!;
            Task<string> stdout = process.StandardOutput.ReadToEndAsync();
            Task<string> stderr = process.StandardError.ReadToEndAsync();

            if (!process.WaitForExit(ChildBoundMilliseconds))
            {
                process.Kill(entireProcessTree: true);
                Assert.Fail($"the profiler child did not finish in {ChildBoundMilliseconds / 1000} s (its record: {(File.Exists(record) ? File.ReadAllText(record) : "none")})");
            }

            Assert.IsTrue(File.Exists(record), $"the profiler child wrote no record:\n{stdout.Result}{stderr.Result}");
            Assert.AreEqual(Returned, File.ReadAllText(record), "a block event on a new stack, with MemProfileRate = 1: a sampled bucket allocation re-enters stkbucket under profInsertLock");
            Assert.AreEqual(0, process.ExitCode, $"profiler child:\n{stdout.Result}{stderr.Result}");
        }
        finally
        {
            File.Delete(record);
        }
    }

    // The child's half. Outside a child it returns at once, as Go's helper processes do.
    [TestMethod]
    public void ChildRecordsABlockEventWithEveryAllocationSampled()
    {
        string? record = Environment.GetEnvironmentVariable(RecordVariable);

        if (string.IsNullOrEmpty(record))
            return;

        Thread worker = new(static () =>
        {
            SetBlockProfileRate(1);
            MemProfileRate = 1;
            BlockEventOnItsOwnStack();
        })
        {
            IsBackground = true
        };

        worker.Start();
        bool returned = worker.Join(ChildEventBoundMilliseconds);
        File.WriteAllText(record, returned ? Returned : Hung);

        // The worker still spins on the lock it holds: leave the process rather than the test.
        if (!returned)
            Environment.Exit(HungExitCode);
    }

    // Runs record with every allocation sampled and a recorder that only counts, and returns the samples
    // this thread took. The counting recorder takes no lock, so a sample under profInsertLock is counted
    // where runtime's own recorder would hang.
    private static int SamplesWhile(Action record)
    {
        // Reading runtime's MemProfileRate loads runtime, whose module initializer installs the recorder.
        nint previous = MemProfileRate;
        Action<object, nuint, bool, GoAllocEntry>? recorder = GoMemProfile.Recorder;
        Assert.IsNotNull(recorder, "runtime installs the memory profile's recorder");

        int thread = Environment.CurrentManagedThreadId;
        int samples = 0;

        GoMemProfile.Recorder = (_, _, _, _) =>
        {
            if (Environment.CurrentManagedThreadId == thread)
                samples++;
        };

        try
        {
            MemProfileRate = 1;
            record();
            return samples;
        }
        finally
        {
            MemProfileRate = previous;
            GoMemProfile.Recorder = recorder;
        }
    }

    private static nint BlockBuckets() => BlockProfile(default).n;

    private static nint MutexBuckets() => MutexProfile(default).n;

    [TestMethod]
    public void ABlockEventSamplesNoneOfTheProfilersOwnObjects()
    {
        SetBlockProfileRate(1);

        try
        {
            nint before = BlockBuckets();
            int first = SamplesWhile(BlockEventOnItsOwnStack);
            nint afterFirst = BlockBuckets();
            int repeat = SamplesWhile(BlockEventOnItsOwnStack);
            nint afterRepeat = BlockBuckets();

            // The controls: the first event did allocate a bucket, and the repeat found it.
            Assert.AreEqual(before + 1, afterFirst, "the first event records a stack the block profile did not hold");
            Assert.AreEqual(afterFirst, afterRepeat, "the same event again finds its bucket");

            Assert.AreEqual((0, 0), (first, repeat), "samples taken by (a block event on a new stack, the same event again): its stack buffer and its bucket are the profiler's own, which Go never samples");
        }
        finally
        {
            SetBlockProfileRate(0);
        }
    }

    [TestMethod]
    public void AMutexEventSamplesNoneOfTheProfilersOwnObjects()
    {
        nint previous = SetMutexProfileFraction(1);

        try
        {
            nint before = MutexBuckets();
            int first = SamplesWhile(MutexEventOnItsOwnStack);

            Assert.AreEqual(before + 1, MutexBuckets(), "the event records a stack the mutex profile did not hold");
            Assert.AreEqual(0, first, "samples taken by a mutex event on a new stack");
        }
        finally
        {
            SetMutexProfileFraction(previous);
        }
    }
}
