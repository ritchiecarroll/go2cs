// EventPipeSamplerHostProbe.cs - Gbtc
// Copyright © 2026 The go2cs Authors. All rights reserved.
//
// Use of this source code is governed by an MIT-style license
// that can be found in the LICENSE file.

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Runtime.InteropServices;
using System.Threading;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using go;
using go.CpuProfiler;
using go.golib;

namespace GolibTests;

// A MEASUREMENT, not a guard (C1, 2026-11-09, the post-.3 queue's item 2).
// EventPipeSamplerTests.AGoHogIsSampledAsItsSyntheticPCAndThinnedToTheRate reads, on the macs, "managed samples 329,
// written 20413, hog as leaf 8" (osx-arm64) and 3 of 23403 (osx-x64), where linux keeps about 200 for two 1 s hogs at
// 100 Hz. EventPipeSampler keys its per-thread CPU readings on ProcessThread.Id and its samples on EventPipe's ThreadID,
// and takes both to be the OS thread id. This probe reads the two keyings on this host. Two hog threads record their
// own native id (pthread_threadid_np on darwin, gettid on linux) and their CPU time from the OS (getrusage
// RUSAGE_THREAD on linux, thread_info on darwin through ThreadCpuTime). After the sampler stops, the probe prints, as
// READING lines: the sampler's counts; each hog's native id, own CPU time, and the sampler's CPU reading and written
// count under that id; and the largest per-thread CPU readings and written counts, with their sums against the
// process's own CPU time over the session. It asserts only that a session opened.
[TestClass]
[DoNotParallelize]
public class EventPipeSamplerHostProbe
{
    [TestMethod]
    public void TheSamplersThreadKeysAndCpuReadingsAreMeasured()
    {
        const int hz = 100;
        const int hogMilliseconds = 1000;
        const int threads = 2;

        var sampler = new EventPipeSampler(EventPipeSampler.OpenInProcessSession);
        int written = 0;
        int leafHog = 0;
        uintptr hogPC = GoSyntheticPC.Of(typeof(cpusamplerprobe_package).GetMethod(nameof(cpusamplerprobe_package.cpuHog1))!);

        var leaves = new Dictionary<string, int>();

        void Collect(int64 nanotime, slice<uintptr> stack, unsafe_package.Pointer tag)
        {
            written++;

            if (stack.Length > 0 && stack[0] == hogPC)
                leafHog++;

            // Which Go function each written sample has as its leaf: on the macs the hog leaf reads 0-10 of about 200
            // (run 42609377638), so where the hog's CPU lands instead is the reading.
            string leaf = stack.Length > 0 ? GoSyntheticPC.NameOf(stack[0]) ?? $"pc {(ulong)stack[0]:x}" : "(empty)";
            leaves[leaf] = leaves.GetValueOrDefault(leaf) + 1;
        }

        using Process self = Process.GetCurrentProcess();
        TimeSpan processBefore = self.TotalProcessorTime;
        Stopwatch wall = Stopwatch.StartNew();

        sampler.Start(hz);
        Assert.IsTrue(sampler.LastSessionOpened, "a SampleProfiler session must open on this process");

        var ids = new ulong[threads];
        var ownCpu = new TimeSpan[threads];
        var hogs = new Thread[threads];
        var thrown = new Exception?[threads];

        for (int i = 0; i < threads; i++)
        {
            int index = i;

            // The hog is a thread of its own, so what it throws is caught here and reported on the test's thread below:
            // an exception left unhandled on it ends the test host and every test after this one (a refused P/Invoke
            // did, on linux-arm64, while the GolibTests assembly carried bindings converted for linux/amd64).
            hogs[i] = new Thread(() =>
            {
                try
                {
                    ids[index] = NativeThreadId();
                    long token = ThreadCpuTime.CurrentThreadToken();
                    using ThreadCpuReader? reader = token != 0 ? ThreadCpuTime.Open(token) : null;
                    TimeSpan start = reader is not null && reader.TryRead(out TimeSpan before) ? before : TimeSpan.Zero;
                    cpusamplerprobe_package.cpuHogger(hogMilliseconds);
                    ownCpu[index] = reader is not null && reader.TryRead(out TimeSpan after) ? after - start : TimeSpan.MinValue;
                }
                catch (Exception exception)
                {
                    thrown[index] = exception;
                }
            });

            hogs[i].Start();
        }

        foreach (Thread hog in hogs)
            hog.Join();

        if (Array.FindIndex(thrown, exception => exception is not null) is int failed and >= 0)
        {
            sampler.Stop((_, _, _) => { });
            Assert.Fail($"hog {failed} threw on its own thread: {thrown[failed]}");
        }

        sampler.Stop(Collect);
        wall.Stop();
        self.Refresh();
        TimeSpan processCpu = self.TotalProcessorTime - processBefore;

        string host = $"{RuntimeInformation.OSDescription.Trim()}, {RuntimeInformation.ProcessArchitecture}";
        List<string> lines =
        [
            $"counts: managed samples {sampler.LastManagedSamples}, written {written} (sampler says {sampler.LastSamplesWritten}), " +
            $"external {sampler.LastExternalCodeSamples}, hog as leaf {leafHog}; session wall {wall.Elapsed.TotalMilliseconds:F0} ms, " +
            $"process CPU {processCpu.TotalMilliseconds:F0} ms"
        ];

        for (int i = 0; i < threads; i++)
        {
            int key = unchecked((int)ids[i]);
            string cpu = sampler.LastCpuByThread.TryGetValue(key, out TimeSpan reading) ? $"{reading.TotalMilliseconds:F0} ms" : "absent";
            string samples = sampler.LastWrittenByThread.TryGetValue(key, out int count) ? count.ToString() : "absent";
            lines.Add($"hog {i}: native id {ids[i]} (as int {key}), own CPU {ownCpu[i].TotalMilliseconds:F0} ms; sampler CPU reading {cpu}, written {samples}");
        }

        lines.Add($"leaf frames of the written samples: " +
            string.Join(", ", leaves.OrderByDescending(pair => pair.Value).Take(6).Select(pair => $"{pair.Key}={pair.Value}")));
        lines.Add($"CPU readings: {sampler.LastCpuByThread.Count} threads, sum {sampler.LastCpuByThread.Values.Sum(t => t.TotalMilliseconds):F0} ms; largest " +
            string.Join(", ", sampler.LastCpuByThread.OrderByDescending(pair => pair.Value).Take(6).Select(pair => $"{pair.Key}={pair.Value.TotalMilliseconds:F0}ms")));
        lines.Add($"written by thread: {sampler.LastWrittenByThread.Count} threads, sum {sampler.LastWrittenByThread.Values.Sum()}; largest " +
            string.Join(", ", sampler.LastWrittenByThread.OrderByDescending(pair => pair.Value).Take(6).Select(pair => $"{pair.Key}={pair.Value}")));

        foreach (string line in lines)
            Console.WriteLine($"READING EventPipe sampler keys: {line} ({host})");
    }

    // The calling thread's OS id in the spelling the runtime's diagnostics use: pthread_threadid_np on darwin (the
    // 64-bit system-wide id), gettid on linux. 0 elsewhere.
    private static ulong NativeThreadId()
    {
        if (OperatingSystem.IsMacOS())
            return pthread_threadid_np(0, out ulong id) == 0 ? id : 0;

        if (OperatingSystem.IsLinux())
            return (ulong)gettid();

        return 0;
    }

    [DllImport("libc")]
    private static extern int pthread_threadid_np(nint thread, out ulong id);

    [DllImport("libc")]
    private static extern int gettid();
}
