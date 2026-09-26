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
// THIS PIECE OWNS THE SESSION'S LIFETIME ONLY. Stop drains the event stream and writes no sample yet:
// resolving the sampled frames to Go PCs is the next piece (frames, section 11.6), so an opted-in
// profile still has zero samples. It never throws into runtime: where a session cannot open
// (DOTNET_EnableDiagnostics=0, a runtime without EventPipe) Start records the failure and Stop writes
// nothing, which is the zero-sample profile every ordinary program gets.

namespace go.CpuProfiler;

using System;
using System.Collections.Generic;
using System.Diagnostics.Tracing;
using System.IO;
using System.Threading.Tasks;
using Microsoft.Diagnostics.NETCore.Client;

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

    private static readonly TimeSpan DrainBound = TimeSpan.FromSeconds(30);

    private readonly Func<ISession> m_open;
    private ISession? m_session;
    private MemoryStream? m_trace;
    private Task? m_drain;

    public EventPipeSampler(Func<ISession> open) => m_open = open;

    /// <summary>Registers an in-process sampler with runtime's seam. Called by the module initializer
    /// GoCpuProfiler.targets links into an opted-in program.</summary>
    public static void Register() => runtime_package.GoRegisterCpuSampler(new EventPipeSampler(OpenInProcessSession));

    /// <summary>Opens a SampleProfiler session on this process's own diagnostic port.</summary>
    public static ISession OpenInProcessSession()
    {
        List<EventPipeProvider> providers = [new("Microsoft-DotNETCore-SampleProfiler", EventLevel.Informational)];
        return new InProcessSession(new DiagnosticsClient(Environment.ProcessId).StartEventPipeSession(providers, requestRundown: true));
    }

    /// <summary>Whether the last <see cref="Start"/> opened a session.</summary>
    public bool LastSessionOpened { get; private set; }

    /// <summary>Bytes of trace the last <see cref="Stop"/> drained (0 when no session opened).</summary>
    public long LastTraceBytes { get; private set; }

    public void Start(int hz)
    {
        LastSessionOpened = false;
        LastTraceBytes = 0;
    }

    public void Stop(GoCpuSampleWriter write)
    {
        ISession? session = m_session;
        m_session = null;

        if (session is null)
            return;

        try
        {
            session.Stop();
            m_drain?.Wait(DrainBound);
        }
        catch (Exception)
        {
            // A session that fails while stopping leaves the samples it could not deliver out.
        }
        finally
        {
            session.Dispose();
        }

        LastTraceBytes = m_trace?.Length ?? 0;
        m_trace = null;
        m_drain = null;
    }
}
