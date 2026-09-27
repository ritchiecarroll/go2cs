// ExecutionTracer.cs - Gbtc
// Copyright © 2026 The go2cs Authors. All rights reserved.
//
// Use of this source code is governed by an MIT-style license
// that can be found in the LICENSE file.

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text;
using System.Threading;

namespace go.golib;

/// <summary>
/// A minimal execution tracer: Go's v2 trace format (the version the go1.24.13 runtime writes,
/// <c>go 1.23 trace</c>), carrying exactly the goroutine facts the managed runtime observes.
/// </summary>
/// <remarks>
/// <para>
/// Design: docs/phase4/DESIGN-managed-execution-tracer.md (Q28) and its 2026-09-27 amendment. The
/// runtime's hand-owned <c>StartTrace</c>/<c>StopTrace</c>/<c>ReadTrace</c> (runtime/{windows,linux}/
/// trace_impl.cs) drive it; <see cref="Goroutine"/> calls it at its five lifecycle points -- create,
/// start, block, unblock, destroy (and resume, the wakee's half) -- behind one volatile read, which is
/// the tracer's whole cost while no trace runs.
/// </para>
/// <para>
/// The model, stated because a reader of the trace must know it: an M is the goroutine's OS thread (a
/// goroutine owns its thread for life), and each M holds ONE P whose id is the M's. There is no
/// scheduler to describe, so no P ever moves between Ms and no steal, GC, heap, syscall or CPU-sample
/// event is ever written -- those would describe a runtime that is not running. A goroutine's status is
/// stated lazily, before its first event in the trace, exactly as Go's runtime does per generation; the
/// start prologue states every goroutine parked at that moment, so a goroutine that never wakes inside
/// the window is still named. Block reasons are Go's trace vocabulary (traceBlockReasonStrings), not the
/// traceback text. Stacks are not recorded (stack ID 0).
/// </para>
/// <para>
/// One generation spans the whole trace: a header, then per-M event batches (at most 64 KiB each), then
/// one strings batch and one frequency batch; the trace ends at EOF. Every event is written under one
/// lock with a timestamp taken under it, so the events are globally ordered and every batch is
/// monotonic. The data is released to the reader as batches seal and all at once at stop.
/// </para>
/// </remarks>
public static class ExecutionTracer
{
    // ---- the wire format (go122 event numbers, internal/trace/event/go122/event.go at the pin) ----

    private const byte EvEventBatch = 1;
    private const byte EvStrings = 4;
    private const byte EvString = 5;
    private const byte EvFrequency = 8;
    private const byte EvProcStatus = 13;
    private const byte EvGoCreate = 14;
    private const byte EvGoStart = 16;
    private const byte EvGoDestroy = 17;
    private const byte EvGoBlock = 20;
    private const byte EvGoUnblock = 21;
    private const byte EvGoStatus = 25;

    private const ulong ProcRunning = 1;
    private const ulong GoRunnable = 1;
    private const ulong GoRunning = 2;
    private const ulong GoWaiting = 4;

    private const ulong Generation = 1;
    private const ulong NoThread = ulong.MaxValue;
    private const int MaxBatchPayload = 64 << 10;

    // The largest event this tracer writes: a type byte and four uvarints.
    private const int MaxEventSize = 1 + 4 * 10;

    private static readonly byte[] s_header = Encoding.ASCII.GetBytes("go 1.23 trace\0\0\0");

    // ---- state ----

    private static volatile bool s_enabled;

    /// <summary>Whether a trace is running: the one read every lifecycle hook makes first.</summary>
    public static bool Enabled => s_enabled;

    private static readonly object s_lock = new();

    // Guarded by s_lock.
    private static long s_lastTimestamp;
    private static Dictionary<int, MachineState> s_machines = new();
    private static Dictionary<long, GoroutineState> s_goroutines = new();
    private static Dictionary<string, ulong> s_strings = new(StringComparer.Ordinal);

    // The reader's queue, guarded by s_readLock: chunks in order, then null-and-drained at the end.
    private static readonly object s_readLock = new();
    private static readonly Queue<byte[]> s_chunks = new();
    private static bool s_stopping;
    private static bool s_drained = true;

    private sealed class MachineState(int id)
    {
        public readonly ulong Id = (ulong)id;
        public bool ProcStated;
        public GoroutineState? Current;
        public readonly List<byte> Payload = new(4096);
        public long BaseTimestamp;
        public long LastTimestamp;
    }

    private enum Status
    {
        Unknown,
        Runnable,
        Running,
        Waiting,
        Dead
    }

    private sealed class GoroutineState(long id)
    {
        public readonly ulong Id = (ulong)id;
        public Status Status;
        public ulong Seq;
    }

    // ---- the runtime's entry points ----

    /// <summary>
    /// Starts a trace, as <c>runtime.StartTrace</c>: false (Go's "tracing is already enabled") when one
    /// is running or still being read.
    /// </summary>
    public static bool Start()
    {
        lock (s_lock)
        {
            lock (s_readLock)
            {
                if (s_enabled || !s_drained)
                    return false;

                s_chunks.Clear();
                s_stopping = false;
                s_drained = false;
                s_chunks.Enqueue((byte[])s_header.Clone());
            }

            s_machines = new Dictionary<int, MachineState>();
            s_goroutines = new Dictionary<long, GoroutineState>();
            s_strings = new Dictionary<string, ulong>(StringComparer.Ordinal);
            s_lastTimestamp = 0;
            s_enabled = true;

            // The prologue: every goroutine PARKED now is stated, so one that never wakes inside the
            // window is still in the trace. A running goroutine is stated lazily on its own thread,
            // the only place that can put it on an M.
            MachineState m = machine();
            ensureProc(m);

            foreach (Goroutine g in Goroutine.Snapshot())
            {
                if (!g.IsParked || ReferenceEquals(g, Goroutine.Current))
                    continue;

                GoroutineState state = goroutine(g);
                state.Status = g.IsReadied ? Status.Runnable : Status.Waiting;
                state.Seq = 0;
                emit(m, EvGoStatus, state.Id, NoThread, state.Status == Status.Runnable ? GoRunnable : GoWaiting);
            }
        }

        return true;
    }

    /// <summary>
    /// Stops the trace, as <c>runtime.StopTrace</c>: every batch is released to the reader, and the
    /// call returns only after the reader has taken the last of it (Go: "only returns after all the
    /// reads for the trace have completed"). A stop with no trace running is a no-op.
    /// </summary>
    public static void Stop()
    {
        lock (s_lock)
        {
            if (!s_enabled)
                return;

            s_enabled = false;

            List<byte[]> tail = [];

            foreach (MachineState m in s_machines.Values)
            {
                if (m.Payload.Count > 0)
                    tail.Add(seal(m));
            }

            tail.Add(stringsBatch());
            tail.Add(frequencyBatch());

            lock (s_readLock)
            {
                foreach (byte[] chunk in tail)
                    s_chunks.Enqueue(chunk);

                s_stopping = true;
                Monitor.PulseAll(s_readLock);
            }
        }

        lock (s_readLock)
        {
            while (!s_drained)
                Monitor.Wait(s_readLock);
        }
    }

    /// <summary>
    /// The next chunk of trace data, as <c>runtime.ReadTrace</c>: blocks until data is available, and
    /// returns <c>null</c> once the trace has stopped and every byte has been read.
    /// </summary>
    public static byte[]? Read()
    {
        lock (s_readLock)
        {
            while (true)
            {
                if (s_chunks.TryDequeue(out byte[]? chunk))
                    return chunk;

                if (s_stopping)
                {
                    s_drained = true;
                    Monitor.PulseAll(s_readLock);
                    return null;
                }

                if (s_drained)
                    return null;

                Monitor.Wait(s_readLock);
            }
        }
    }

    // ---- the lifecycle points (golib's Goroutine calls these after reading Enabled) ----

    // Go's newproc: on the CREATING thread, before the new goroutine has run.
    internal static void OnCreate(Goroutine created)
    {
        lock (s_lock)
        {
            if (!s_enabled)
                return;

            MachineState m = machine();
            ensureProc(m);

            if (Goroutine.Current is { } creator)
                ensureRunning(m, goroutine(creator));

            GoroutineState state = goroutine(created);
            state.Status = Status.Runnable;
            state.Seq = 0;
            emit(m, EvGoCreate, state.Id, 0, 0);
        }
    }

    // The new goroutine's first instruction, on its own thread.
    internal static void OnStart(Goroutine g) => OnResume(g);

    // The wakee's half: a goroutine running again on its own thread, after a park or at its start.
    internal static void OnResume(Goroutine g)
    {
        lock (s_lock)
        {
            if (!s_enabled)
                return;

            MachineState m = machine();
            ensureProc(m);
            ensureRunning(m, goroutine(g));
        }
    }

    // Go's gopark: the goroutine blocks on its own thread, with its reason.
    internal static void OnBlock(Goroutine g, WaitReason reason)
    {
        lock (s_lock)
        {
            if (!s_enabled)
                return;

            MachineState m = machine();
            ensureProc(m);

            GoroutineState state = goroutine(g);
            ensureRunning(m, state);

            emit(m, EvGoBlock, intern(BlockReason(reason)), 0);
            state.Status = Status.Waiting;
            m.Current = null;
        }
    }

    // Go's goready: on the WAKER's thread (any thread), before the primitive is signalled.
    internal static void OnUnblock(Goroutine target)
    {
        lock (s_lock)
        {
            if (!s_enabled)
                return;

            MachineState m = machine();
            GoroutineState state = goroutine(target);

            // A target parked before the trace started and missed by the prologue (it parked between
            // the snapshot and this call) is stated where it is first named.
            if (state.Status == Status.Unknown)
            {
                state.Status = Status.Waiting;
                state.Seq = 0;
                emit(m, EvGoStatus, state.Id, NoThread, GoWaiting);
            }

            if (state.Status != Status.Waiting)
                return;

            state.Seq++;
            emit(m, EvGoUnblock, state.Id, state.Seq, 0);
            state.Status = Status.Runnable;
        }
    }

    // Go's goexit: the goroutine ends on its own thread.
    internal static void OnDestroy(Goroutine g)
    {
        lock (s_lock)
        {
            if (!s_enabled)
                return;

            MachineState m = machine();
            ensureProc(m);

            GoroutineState state = goroutine(g);
            ensureRunning(m, state);

            emit(m, EvGoDestroy);
            state.Status = Status.Dead;
            m.Current = null;
        }
    }

    // ---- the trace vocabulary ----

    /// <summary>
    /// Go's TRACE block reason for a wait reason -- runtime/traceruntime.go's traceBlockReasonStrings, as
    /// the pin's gopark call sites pair them -- not the traceback text <see cref="WaitReasons.Text"/>
    /// prints. A coroutine switch, which Go traces with its own switch events, is stated as a block
    /// ("unspecified") and an unblock: a named difference of this model.
    /// </summary>
    internal static string BlockReason(WaitReason reason) => reason switch
    {
        WaitReason.ChanReceive => "chan receive",
        WaitReason.ChanSend => "chan send",
        WaitReason.ChanReceiveNilChan or WaitReason.ChanSendNilChan or WaitReason.SelectNoCases => "forever",
        WaitReason.Select => "select",
        WaitReason.Semacquire or WaitReason.SyncMutexLock or WaitReason.SyncRWMutexRLock or
            WaitReason.SyncRWMutexLock or WaitReason.SyncWaitGroupWait => "sync",
        WaitReason.SyncCondWait => "sync.(*Cond).Wait",
        WaitReason.Sleep => "sleep",
        WaitReason.IOWait => "network",
        WaitReason.SynctestRun or WaitReason.SynctestWait or WaitReason.SynctestChanReceive or
            WaitReason.SynctestChanSend or WaitReason.SynctestSelect => "synctest",
        _ => "unspecified",
    };

    // ---- the per-M and per-G state machine (all under s_lock) ----

    private static MachineState machine()
    {
        int id = Environment.CurrentManagedThreadId;

        if (!s_machines.TryGetValue(id, out MachineState? m))
            s_machines[id] = m = new MachineState(id);

        return m;
    }

    private static GoroutineState goroutine(Goroutine g)
    {
        if (!s_goroutines.TryGetValue(g.Id, out GoroutineState? state))
            s_goroutines[g.Id] = state = new GoroutineState(g.Id);

        return state;
    }

    // The M's one P, stated Running before the M's first event that needs it.
    private static void ensureProc(MachineState m)
    {
        if (m.ProcStated)
            return;

        emit(m, EvProcStatus, m.Id, ProcRunning);
        m.ProcStated = true;
    }

    // Puts `state` on `m` as its running goroutine, from whatever the trace last said about it.
    private static void ensureRunning(MachineState m, GoroutineState state)
    {
        if (m.Current == state && state.Status == Status.Running)
            return;

        switch (state.Status)
        {
            case Status.Unknown:
                // Never named in this trace: it has been running since before the trace began.
                emit(m, EvGoStatus, state.Id, m.Id, GoRunning);
                state.Seq = 0;
                break;
            case Status.Waiting:
                // Woken without a waker naming it (a timeout, a sleep's timer): the unblock is its own.
                state.Seq++;
                emit(m, EvGoUnblock, state.Id, state.Seq, 0);
                goto case Status.Runnable;
            case Status.Runnable:
                state.Seq++;
                emit(m, EvGoStart, state.Id, state.Seq);
                break;
            case Status.Running:
                // Running on another M in the trace's account: restated here.
                emit(m, EvGoStatus, state.Id, m.Id, GoRunning);
                state.Seq = 0;
                break;
            default:
                return;
        }

        state.Status = Status.Running;
        m.Current = state;
    }

    private static ulong intern(string text)
    {
        if (!s_strings.TryGetValue(text, out ulong id))
            s_strings[text] = id = (ulong)s_strings.Count + 1;

        return id;
    }

    // ---- encoding ----

    private static long timestamp()
    {
        long now = Stopwatch.GetTimestamp();

        if (now <= s_lastTimestamp)
            now = s_lastTimestamp + 1;

        s_lastTimestamp = now;
        return now;
    }

    private static void emit(MachineState m, byte type, params ReadOnlySpan<ulong> args)
    {
        if (m.Payload.Count + MaxEventSize > MaxBatchPayload)
            release(seal(m));

        long now = timestamp();

        if (m.Payload.Count == 0)
            m.BaseTimestamp = m.LastTimestamp = now;

        m.Payload.Add(type);
        uvarint(m.Payload, (ulong)(now - m.LastTimestamp));
        m.LastTimestamp = now;

        foreach (ulong arg in args)
            uvarint(m.Payload, arg);
    }

    private static byte[] seal(MachineState m)
    {
        byte[] batch = batchOf(m.Id, (ulong)m.BaseTimestamp, m.Payload);
        m.Payload.Clear();
        return batch;
    }

    private static byte[] stringsBatch()
    {
        List<byte> payload = [EvStrings];

        foreach ((string text, ulong id) in s_strings)
        {
            byte[] bytes = Encoding.UTF8.GetBytes(text);
            payload.Add(EvString);
            uvarint(payload, id);
            uvarint(payload, (ulong)bytes.Length);
            payload.AddRange(bytes);
        }

        return batchOf(NoThread, (ulong)s_lastTimestamp, payload);
    }

    private static byte[] frequencyBatch()
    {
        List<byte> payload = [EvFrequency];
        uvarint(payload, (ulong)Stopwatch.Frequency);
        return batchOf(NoThread, (ulong)s_lastTimestamp, payload);
    }

    private static byte[] batchOf(ulong m, ulong timestamp, List<byte> payload)
    {
        List<byte> batch = new(payload.Count + 32) { EvEventBatch };
        uvarint(batch, Generation);
        uvarint(batch, m);
        uvarint(batch, timestamp);
        uvarint(batch, (ulong)payload.Count);
        batch.AddRange(payload);
        return [.. batch];
    }

    private static void release(byte[] chunk)
    {
        lock (s_readLock)
        {
            s_chunks.Enqueue(chunk);
            Monitor.PulseAll(s_readLock);
        }
    }

    private static void uvarint(List<byte> buffer, ulong value)
    {
        while (value >= 0x80)
        {
            buffer.Add((byte)(value | 0x80));
            value >>= 7;
        }

        buffer.Add((byte)value);
    }
}
