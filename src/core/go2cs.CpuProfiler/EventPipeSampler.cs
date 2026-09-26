// EventPipeSampler.cs - Gbtc
// Copyright © 2026 The go2cs Authors. All rights reserved.
//
// SPDX-License-Identifier: BSD-3-Clause
// Use of this source code is governed by a BSD-style license
// that can be found in the LICENSE file.

// The opt-in CPU sampler: an EventPipe session on this process's own diagnostic port with the
// runtime's SampleProfiler provider, opened when Go turns CPU profiling on and closed when it turns
// it off (section 11 of docs/phase4/DESIGN-managed-profiling.md; measured feasible in section 9.1).
//
// THE FRAMES PIECE (section 11.6). Stop parses the drained trace with TraceEvent's TraceLog and writes
// one Go sample per kept SampleProfiler sample:
//   - Only samples of a thread running managed code count (thread-sample type Managed). A thread
//     blocked in a wait or in native code samples as External, and Go's profile holds no such time.
//   - Each frame's method, named by the session's JIT and rundown events as a metadata token and a
//     module, resolves through Module.ResolveMethod to the MethodBase, and runtime maps that to the
//     method's synthetic PC when it is a frame Go's unwinder reports (runtime.GoCpuSamplePC). Every
//     other frame (golib, the BCL, the test host) is dropped. A sample with no Go frame is dropped, which
//     drops the sampler's own threads too. The stack is written leaf first, as Go records it.
//   - The SampleProfiler samples each managed thread about every millisecond; Go values a record at
//     1/hz. Samples are thinned per thread to one per 1/hz of elapsed time (section 9.2 step 4) --
//     except as the magnitude piece below says.
//   - Each sample carries the labels its thread had when sampled: golib's ProfileLabelEvents writes
//     one event into the same trace per label change, and the latest one on the sample's thread
//     before the sample names the labels object, which is the tag (nil for none).
//
// THE MAGNITUDE PIECE. The SampleProfiler samples a thread whenever it is in managed code, whether or
// not it is on a CPU, so a runnable thread waiting for a core is sampled too: with more busy threads
// than cores, one sample per 1/hz of WALL time over-counts (measured: five 1,500 ms hogs on four cores
// each sampled for 1,500 ms while on a CPU for 748-1,169 ms). Go's SIGPROF counts CPU time. So while a
// session runs, every thread's CPU time is polled (System.Diagnostics.ProcessThread.TotalProcessorTime:
// GetThreadTimes on windows, /proc/self/task/<tid>/stat on linux) every CpuPollInterval, and at Stop a
// thread with a reading keeps round(its CPU time x hz) of its samples, spread evenly over them, in
// place of the wall-time grid. EventPipe's ThreadID is the OS thread id, the id ProcessThread reports.
// A thread that exits between two polls loses at most one interval of CPU time; a thread with no
// reading keeps the grid.
//
// It never throws into runtime: where a session cannot open (DOTNET_EnableDiagnostics=0, a runtime
// without EventPipe) Start records the failure and Stop writes nothing, which is the zero-sample profile
// every ordinary program gets. A trace that cannot be parsed writes nothing the same way.

namespace go.CpuProfiler;

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Diagnostics.Tracing;
using System.IO;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Diagnostics.NETCore.Client;
using Microsoft.Diagnostics.Tracing;
using Microsoft.Diagnostics.Tracing.Etlx;
using Microsoft.Diagnostics.Tracing.Parsers;

/// <summary>The EventPipe-backed <see cref="IGoCpuSampler"/>.</summary>
public sealed class EventPipeSampler : IGoCpuSampler
{
    /// <summary>One open sampling session: its event stream, and how to stop it.</summary>
    public interface ISession : IDisposable
    {
        Stream Events { get; }

        void Stop();
    }

    private sealed class InProcessSession(EventPipeSession session) : ISession
    {
        public Stream Events => session.EventStream;

        public void Stop() => session.Stop();

        public void Dispose() => session.Dispose();
    }

    private const string SampleProfilerProvider = "Microsoft-DotNETCore-SampleProfiler";

    private const string LabelsProvider = golib.ProfileLabelEvents.ProviderName;

    // ThreadSample's Type payload: the thread was running managed code (1 is External, 0 an error).
    private const int ManagedThreadSample = 2;

    private static readonly TimeSpan DrainBound = TimeSpan.FromSeconds(30);

    private readonly Func<ISession> m_open;
    private ISession? m_session;
    private string? m_tracePath;
    private Task? m_drain;
    private int m_hz;

    /// <summary>How often a running session reads every thread's CPU time. A thread that exits between two
    /// reads loses at most this much CPU time from its weighting.</summary>
    public static readonly TimeSpan CpuPollInterval = TimeSpan.FromMilliseconds(100);

    private readonly object m_cpuLock = new();
    private readonly Dictionary<int, TimeSpan> m_cpuAtStart = [];
    private readonly Dictionary<int, TimeSpan> m_cpuLatest = [];
    private Timer? m_cpuPoll;
    private int m_polling;

    public EventPipeSampler(Func<ISession> open) => m_open = open;

    /// <summary>Registers an in-process sampler with runtime's seam. Called by the module initializer
    /// GoCpuProfiler.targets links into an opted-in program.</summary>
    public static void Register() => runtime_package.GoRegisterCpuSampler(new EventPipeSampler(OpenInProcessSession));

    /// <summary>Opens a SampleProfiler session on this process's own diagnostic port. The runtime
    /// provider's JIT and loader events, with rundown, name the method behind each sampled frame, and
    /// golib's label events name each sample's labels.</summary>
    public static ISession OpenInProcessSession()
    {
        List<EventPipeProvider> providers =
        [
            new(SampleProfilerProvider, EventLevel.Informational),
            new(LabelsProvider, EventLevel.Informational),
            new("Microsoft-Windows-DotNETRuntime", EventLevel.Informational, (long)(ClrTraceEventParser.Keywords.Jit | ClrTraceEventParser.Keywords.Loader))
        ];

        return new InProcessSession(new DiagnosticsClient(Environment.ProcessId).StartEventPipeSession(providers, requestRundown: true, circularBufferMB: 256));
    }

    /// <summary>Whether the last <see cref="Start"/> opened a session.</summary>
    public bool LastSessionOpened { get; private set; }

    /// <summary>Bytes of trace the last <see cref="Stop"/> drained (0 when no session opened).</summary>
    public long LastTraceBytes { get; private set; }

    /// <summary>SampleProfiler samples of running managed code in the last trace, before any was dropped
    /// or thinned.</summary>
    public int LastManagedSamples { get; private set; }

    /// <summary>Samples the last <see cref="Stop"/> wrote.</summary>
    public int LastSamplesWritten { get; private set; }

    /// <summary>Samples the last <see cref="Stop"/> wrote, per OS thread id (EventPipe's ThreadID).</summary>
    public IReadOnlyDictionary<int, int> LastWrittenByThread => m_writtenByThread;

    /// <summary>The CPU time each OS thread used while the last session ran, as polled; the threads with no
    /// written samples here are the ones whose CPU no sample stands for.</summary>
    public IReadOnlyDictionary<int, TimeSpan> LastCpuByThread => m_cpuByThread;

    private readonly Dictionary<int, TimeSpan> m_cpuByThread = [];

    private readonly Dictionary<int, int> m_writtenByThread = [];

    public void Start(int hz)
    {
        LastSessionOpened = false;
        LastTraceBytes = 0;
        LastManagedSamples = 0;
        LastSamplesWritten = 0;
        m_writtenByThread.Clear();
        m_cpuByThread.Clear();
        m_hz = hz;
        golib.ProfileLabelEvents.Reset();

        lock (m_cpuLock)
        {
            m_cpuAtStart.Clear();
            m_cpuLatest.Clear();
        }

        try
        {
            ISession session = m_open();
            string path = Path.Combine(Path.GetTempPath(), $"go2cs-cpuprofile-{Environment.ProcessId}-{Guid.NewGuid():N}.nettrace");
            m_drain = Task.Run(() =>
            {
                using FileStream trace = File.Create(path);
                session.Events.CopyTo(trace);
            });
            m_tracePath = path;
            m_session = session;
            LastSessionOpened = true;

            PollThreadCpu(baseline: true);
            m_cpuPoll = new Timer(_ => PollThreadCpu(baseline: false), null, CpuPollInterval, CpuPollInterval);
        }
        catch (Exception)
        {
            // No session: the profile completes with zero samples (section 11.4).
            m_session = null;
        }
    }

    public void Stop(GoCpuSampleWriter write)
    {
        ISession? session = m_session;
        string? path = m_tracePath;
        m_session = null;
        m_tracePath = null;

        if (session is null)
            return;

        using (ManualResetEvent stopped = new(false))
        {
            if (m_cpuPoll?.Dispose(stopped) == true)
                stopped.WaitOne();
        }

        m_cpuPoll = null;
        PollThreadCpu(baseline: false);

        lock (m_cpuLock)
        {
            foreach (int thread in m_cpuLatest.Keys)
                m_cpuByThread[thread] = CpuTimeOf(thread)!.Value;
        }

        bool drained = false;

        try
        {
            session.Stop();
            drained = m_drain?.Wait(DrainBound) ?? false;
        }
        catch (Exception)
        {
            // A session that fails while stopping leaves the samples it could not deliver out.
        }
        finally
        {
            session.Dispose();
            m_drain = null;
        }

        if (path is null)
            return;

        string? etlx = null;

        try
        {
            LastTraceBytes = File.Exists(path) ? new FileInfo(path).Length : 0;

            if (drained && LastTraceBytes > 0)
            {
                etlx = TraceLog.CreateFromEventTraceLogFile(path);
                WriteSamples(etlx, write);
            }
        }
        catch (Exception)
        {
            // An unreadable trace writes what it wrote so far and no more.
        }
        finally
        {
            golib.ProfileLabelEvents.Reset();
            TryDelete(path);

            if (etlx is not null)
                TryDelete(etlx);
        }
    }

    private void WriteSamples(string etlx, GoCpuSampleWriter write)
    {
        using TraceLog log = new(etlx);

        int hz = m_hz > 0 ? m_hz : 100;
        double periodMSec = 1000.0 / hz;
        Dictionary<int, unsafe_package.Pointer> labels = [];
        Dictionary<int, List<Candidate>> byThread = [];
        FrameResolver frames = new();
        List<uintptr> stack = [];

        foreach (TraceEvent sample in log.Events)
        {
            if (sample.ProviderName == LabelsProvider)
            {
                // Events are in time order, so this is the thread's labels from here on.
                if (golib.ProfileLabelEvents.Resolve(LabelsId(sample)) is unsafe_package.Pointer set)
                    labels[sample.ThreadID] = set;
                else
                    labels.Remove(sample.ThreadID);

                continue;
            }

            if (sample.ProviderName != SampleProfilerProvider || !IsManagedSample(sample))
                continue;

            LastManagedSamples++;

            stack.Clear();

            for (TraceCallStack? frame = sample.CallStack(); frame is not null; frame = frame.Caller)
            {
                uintptr pc = frames.PCOf(frame.CodeAddress.Method);

                if (pc != 0)
                    stack.Add(pc);
            }

            if (stack.Count == 0)
                continue;

            if (!byThread.TryGetValue(sample.ThreadID, out List<Candidate>? candidates))
                byThread[sample.ThreadID] = candidates = [];

            candidates.Add(new Candidate(sample.ThreadID, sample.TimeStampRelativeMSec, stack.ToArray(), labels.TryGetValue(sample.ThreadID, out unsafe_package.Pointer? tag) ? tag : nil));
        }

        List<Candidate> kept = [];

        foreach ((int thread, List<Candidate> candidates) in byThread)
        {
            if (CpuTimeOf(thread) is TimeSpan used)
                KeepByCpuTime(candidates, (int)Math.Round(used.TotalSeconds * hz), kept);
            else
                KeepOnGrid(candidates, periodMSec, kept);
        }

        kept.Sort(static (left, right) => left.AtMSec.CompareTo(right.AtMSec));

        foreach (Candidate sample in kept)
        {
            write((int64)(sample.AtMSec * 1_000_000.0), sample.Stack.slice(), sample.Tag);
            LastSamplesWritten++;
            m_writtenByThread[sample.Thread] = m_writtenByThread.GetValueOrDefault(sample.Thread) + 1;
        }
    }

    // One SampleProfiler sample of a Go stack, before it is kept or dropped.
    private sealed record Candidate(int Thread, double AtMSec, uintptr[] Stack, unsafe_package.Pointer Tag);

    // A thread whose CPU time was read keeps `count` of its samples, spread evenly over them in time.
    private static void KeepByCpuTime(List<Candidate> candidates, int count, List<Candidate> kept)
    {
        count = Math.Min(count, candidates.Count);

        for (int k = 0; k < count; k++)
            kept.Add(candidates[(int)((k + 0.5) * candidates.Count / count)]);
    }

    // A thread with no CPU reading keeps its samples on a 1/hz grid of wall time; a thread idle past its
    // next slot restarts from now.
    private static void KeepOnGrid(List<Candidate> candidates, double periodMSec, List<Candidate> kept)
    {
        double due = double.NegativeInfinity;

        foreach (Candidate sample in candidates)
        {
            if (sample.AtMSec < due)
                continue;

            due = sample.AtMSec < due + periodMSec ? due + periodMSec : sample.AtMSec + periodMSec;
            kept.Add(sample);
        }
    }

    // The CPU time a thread used while the session ran: its last reading less its reading at Start (0 for
    // a thread that did not exist then). Null when it was never read.
    private TimeSpan? CpuTimeOf(int thread)
    {
        lock (m_cpuLock)
        {
            if (!m_cpuLatest.TryGetValue(thread, out TimeSpan latest))
                return null;

            TimeSpan used = latest - m_cpuAtStart.GetValueOrDefault(thread);

            return used < TimeSpan.Zero ? TimeSpan.Zero : used;
        }
    }

    // Reads every thread's CPU time. Timer callbacks can overlap on a busy machine; a late one skips.
    private void PollThreadCpu(bool baseline)
    {
        if (Interlocked.Exchange(ref m_polling, 1) != 0)
            return;

        try
        {
            using Process self = Process.GetCurrentProcess();

            foreach (ProcessThread thread in self.Threads)
            {
                using (thread)
                {
                    try
                    {
                        TimeSpan used = thread.TotalProcessorTime;

                        lock (m_cpuLock)
                        {
                            if (baseline)
                                m_cpuAtStart[thread.Id] = used;

                            m_cpuLatest[thread.Id] = used;
                        }
                    }
                    catch (Exception)
                    {
                        // The thread exited between the enumeration and the read.
                    }
                }
            }
        }
        catch (Exception)
        {
            // No reading this time: threads keep their previous one.
        }
        finally
        {
            Volatile.Write(ref m_polling, 0);
        }
    }

    private static long LabelsId(TraceEvent labelsSet) =>
        labelsSet.PayloadNames.Length > 0 && labelsSet.PayloadValue(0) is IConvertible id ? id.ToInt64(null) : 0;

    private static bool IsManagedSample(TraceEvent sample)
    {
        if (sample.PayloadNames.Length == 0)
            return false;

        return sample.PayloadValue(0) switch
        {
            string name => name == "Managed",
            IConvertible value => value.ToInt32(null) == ManagedThreadSample,
            _ => false
        };
    }

    private static void TryDelete(string path)
    {
        try
        {
            File.Delete(path);
        }
        catch (Exception)
        {
            // A temporary trace left behind is harmless.
        }
    }

    // A sampled frame's method, named by the trace as (module, metadata token), mapped once to its Go PC.
    private sealed class FrameResolver
    {
        private readonly Dictionary<string, Module> m_modules = new(StringComparer.OrdinalIgnoreCase);
        private readonly Dictionary<TraceMethod, uintptr> m_pcs = [];

        public FrameResolver()
        {
            foreach (Assembly assembly in AppDomain.CurrentDomain.GetAssemblies())
            {
                if (assembly.IsDynamic || assembly.GetName().Name is not { } name)
                    continue;

                m_modules.TryAdd(name, assembly.ManifestModule);
            }
        }

        public uintptr PCOf(TraceMethod? method)
        {
            if (method is null || method.MethodToken == 0)
                return 0;

            if (m_pcs.TryGetValue(method, out uintptr pc))
                return pc;

            pc = Resolve(method);
            m_pcs[method] = pc;
            return pc;
        }

        private uintptr Resolve(TraceMethod method)
        {
            TraceModuleFile? file = method.MethodModuleFile;

            if (file is null)
                return 0;

            string name = Path.GetFileNameWithoutExtension(file.FilePath is { Length: > 0 } filePath ? filePath : file.Name);

            if (!m_modules.TryGetValue(name, out Module? module))
                return 0;

            try
            {
                return module.ResolveMethod(method.MethodToken) is { } resolved ? runtime_package.GoCpuSamplePC(resolved) : 0;
            }
            catch (Exception)
            {
                // A token this module does not resolve names no Go frame.
                return 0;
            }
        }
    }
}
