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
//     1/hz. Samples are thinned per thread to one per 1/hz of elapsed time (section 9.2 step 4). How
//     close that comes to the process's CPU time is the magnitude piece's question, not this one's.
// Labels (tag) are nil until the labels piece.
//
// It never throws into runtime: where a session cannot open (DOTNET_EnableDiagnostics=0, a runtime
// without EventPipe) Start records the failure and Stop writes nothing, which is the zero-sample profile
// every ordinary program gets. A trace that cannot be parsed writes nothing the same way.

namespace go.CpuProfiler;

using System;
using System.Collections.Generic;
using System.Diagnostics.Tracing;
using System.IO;
using System.Reflection;
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

    // ThreadSample's Type payload: the thread was running managed code (1 is External, 0 an error).
    private const int ManagedThreadSample = 2;

    private static readonly TimeSpan DrainBound = TimeSpan.FromSeconds(30);

    private readonly Func<ISession> m_open;
    private ISession? m_session;
    private string? m_tracePath;
    private Task? m_drain;
    private int m_hz;

    public EventPipeSampler(Func<ISession> open) => m_open = open;

    /// <summary>Registers an in-process sampler with runtime's seam. Called by the module initializer
    /// GoCpuProfiler.targets links into an opted-in program.</summary>
    public static void Register() => runtime_package.GoRegisterCpuSampler(new EventPipeSampler(OpenInProcessSession));

    /// <summary>Opens a SampleProfiler session on this process's own diagnostic port. The runtime
    /// provider's JIT and loader events, with rundown, name the method behind each sampled frame.</summary>
    public static ISession OpenInProcessSession()
    {
        List<EventPipeProvider> providers =
        [
            new(SampleProfilerProvider, EventLevel.Informational),
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

    public void Start(int hz)
    {
        LastSessionOpened = false;
        LastTraceBytes = 0;
        LastManagedSamples = 0;
        LastSamplesWritten = 0;
        m_hz = hz;

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
            TryDelete(path);

            if (etlx is not null)
                TryDelete(etlx);
        }
    }

    private void WriteSamples(string etlx, GoCpuSampleWriter write)
    {
        using TraceLog log = new(etlx);

        double periodMSec = 1000.0 / (m_hz > 0 ? m_hz : 100);
        Dictionary<int, double> nextDue = [];
        FrameResolver frames = new();
        List<uintptr> stack = [];

        foreach (TraceEvent sample in log.Events)
        {
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

            double at = sample.TimeStampRelativeMSec;

            if (nextDue.TryGetValue(sample.ThreadID, out double due) && at < due)
                continue;

            // Keep the thread's samples on a 1/hz grid; a thread idle past its next slot restarts from now.
            nextDue[sample.ThreadID] = at < due + periodMSec ? due + periodMSec : at + periodMSec;

            write((int64)(at * 1_000_000.0), stack.ToArray().slice(), nil);
            LastSamplesWritten++;
        }
    }

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
