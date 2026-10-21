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
/// A minimal execution tracer: Go's v2 trace format (the version the go1.26 runtime writes,
/// <c>go 1.26 trace</c>), carrying exactly the goroutine facts the managed runtime observes and the
/// annotations the program states.
/// </summary>
/// <remarks>
/// <para>
/// Design: docs/phase4/DESIGN-managed-execution-tracer.md (Q28), its 2026-09-27 amendment and its
/// 2026-10-21 amendment (generations, for runtime/trace's flight recorder). The runtime's hand-owned
/// <c>StartTrace</c>/<c>StopTrace</c>/<c>ReadTrace</c> (runtime/trace_impl.cs) drive it, and so do
/// runtime/trace's flight-recorder linknames (traceAdvance, traceClockNow, traceClockUnitsPerSecond)
/// and the runtime's user-annotation bodies (trace_userTaskCreate and its siblings).
/// <see cref="Goroutine"/> calls it at its five lifecycle points -- create, start, block, unblock,
/// destroy (and resume, the wakee's half) -- behind one volatile read, which is the tracer's whole cost
/// while no trace runs.
/// </para>
/// <para>
/// The model, stated because a reader of the trace must know it: an M is the goroutine's OS thread (a
/// goroutine owns its thread for life), and each M holds ONE P whose id is the M's. There is no
/// scheduler to describe, so no P ever moves between Ms and no steal, GC, heap, syscall or CPU-sample
/// event is ever written -- those would describe a runtime that is not running. Block reasons are Go's
/// trace vocabulary (traceBlockReasonStrings), not the traceback text. Stacks are not recorded (stack
/// ID 0). User tasks, regions and logs ARE written: they are facts the program states, not facts about
/// a scheduler, and trace.Log is how a program puts them in its trace.
/// </para>
/// <para>
/// GENERATIONS, as Go's runtime writes them. A trace is a header, then one generation after another.
/// Each generation opens with a sync batch (its frequency and a clock snapshot), carries per-M event
/// batches (at most 64 KiB each) and one strings batch, and ends with Go 1.26's in-band end-of-generation
/// signal, a one-byte chunk the reader sees in band. <see cref="Advance"/> ends one generation and
/// opens the next; Go's runtime does that every second (defaultTraceAdvancePeriod) and so does this
/// tracer, and the flight recorder advances on demand. Every generation STATES every live goroutine
/// and every P it touches before their first event in it, because a reader may start at any
/// generation: the flight recorder keeps only the latest, and Go's parser refuses a goroutine status
/// it has never seen after the first generation it reads. The prologue states each goroutine from what
/// the trace last said about it (a running one in its own M's batch), and a goroutine the trace has
/// never named is stated from the registry.
/// </para>
/// <para>
/// Every event is written under one lock with a timestamp taken under it, so the events are globally
/// ordered and every batch is monotonic. The data is released to the reader as batches seal and at the
/// end of each generation. An advance returns only after the reader has come back for more after the
/// generation's end signal, which is Go's doneSema contract: the flight recorder's WriteTo reads its
/// ring right after it advances, and relies on the reader having filed the generation it just ended.
/// </para>
/// </remarks>
public static class ExecutionTracer
{
    // ---- the wire format (tracev2 event numbers, internal/trace/tracev2/events.go at the pin) ----

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
    private const byte EvUserTaskBegin = 40;
    private const byte EvUserTaskEnd = 41;
    private const byte EvUserRegionBegin = 42;
    private const byte EvUserRegionEnd = 43;
    private const byte EvUserLog = 44;
    private const byte EvSync = 50;
    private const byte EvClockSnapshot = 51;
    private const byte EvEndOfGeneration = 52;

    private const ulong ProcRunning = 1;
    private const ulong GoRunnable = 1;
    private const ulong GoRunning = 2;
    private const ulong GoWaiting = 4;

    private const ulong NoThread = ulong.MaxValue;
    private const int MaxBatchPayload = 64 << 10;

    // The largest event this tracer writes: a type byte and five uvarints (dt and UserLog's four args).
    private const int MaxEventSize = 1 + 5 * 10;

    private static readonly byte[] s_header = Encoding.ASCII.GetBytes("go 1.26 trace\0\0\0");

    // Go's defaultTraceAdvancePeriod (runtime/trace.go): the approximate period between generations.
    private static readonly TimeSpan s_advancePeriod = TimeSpan.FromSeconds(1);

    // ---- state ----

    private static volatile bool s_enabled;

    /// <summary>Whether a trace is running: the one read every lifecycle hook makes first.</summary>
    public static bool Enabled => s_enabled;

    // Serializes Start and every advance, the stop's included: Go's traceAdvanceSema.
    private static readonly object s_advanceLock = new();

    private static readonly object s_lock = new();

    // Guarded by s_lock.
    private static ulong s_generation;
    private static long s_lastTimestamp;
    private static Dictionary<int, MachineState> s_machines = new();
    private static Dictionary<long, GoroutineState> s_goroutines = new();
    private static Dictionary<string, ulong> s_strings = new(StringComparer.Ordinal);

    // The reader's queue, guarded by s_readLock: chunks in order, then null-and-drained at the end.
    private static readonly object s_readLock = new();
    private static readonly Queue<byte[]> s_chunks = new();
    private static bool s_stopping;
    private static bool s_drained = true;

    // Guarded by s_readLock: the advance contract. The reader holds an end signal from the moment Read
    // returns one until it calls Read again; that return is when its generation is DONE.
    private static bool s_endSignalOut;
    private static ulong s_generationsDone;

    // The periodic advancer of the running trace, and the trace it belongs to (guarded by s_advanceLock).
    private static Thread? s_advancer;
    private static ManualResetEventSlim? s_advancerStop;
    private static long s_epoch;

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

    // What the trace has said about one goroutine. Status persists across generations; Stated and
    // Seq are the current generation's.
    private sealed class GoroutineState(long id)
    {
        public readonly ulong Id = (ulong)id;
        public Status Status;
        public bool Stated;
        public ulong Seq;
        public MachineState? Machine;
    }

    // ---- the runtime's entry points ----

    /// <summary>
    /// Starts a trace, as <c>runtime.StartTrace</c>: false (Go's "tracing is already enabled") when one
    /// is running or still being read.
    /// </summary>
    public static bool Start()
    {
        lock (s_advanceLock)
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
                    s_endSignalOut = false;
                    s_generationsDone = 0;
                    s_chunks.Enqueue((byte[])s_header.Clone());
                }

                s_machines = new Dictionary<int, MachineState>();
                s_goroutines = new Dictionary<long, GoroutineState>();
                s_strings = new Dictionary<string, ulong>(StringComparer.Ordinal);
                s_lastTimestamp = 0;
                s_generation = 1;
                s_enabled = true;

                beginGeneration();
            }

            startAdvancer();
        }

        return true;
    }

    /// <summary>
    /// Stops the trace, as <c>runtime.StopTrace</c> (Go's <c>traceAdvance(true)</c>): the last
    /// generation is released to the reader, and the call returns only after the reader has taken the
    /// last of it (Go: "only returns after all the reads for the trace have completed"). A stop with no
    /// trace running is a no-op.
    /// </summary>
    public static void Stop() => Advance(stop: true);

    /// <summary>
    /// Ends the current generation and, unless <paramref name="stop"/>, opens the next, as Go's
    /// <c>traceAdvance</c>: the generation's batches, its strings and its end-of-generation signal go
    /// to the reader, and the call returns only after the reader has come back for more after that
    /// signal (with <paramref name="stop"/>, after the reader has drained the trace). A call with no
    /// trace running returns at once.
    /// </summary>
    public static void Advance(bool stop) => advance(stop, epoch: 0);

    // The epoch is the periodic advancer's: an advance it started for one trace must not touch the next.
    private static void advance(bool stop, long epoch)
    {
        Thread? advancer = null;

        lock (s_advanceLock)
        {
            ulong ended;

            lock (s_lock)
            {
                if (!s_enabled || epoch != 0 && epoch != s_epoch)
                    return;

                ended = s_generation;
                List<byte[]> tail = endGeneration();

                if (stop)
                {
                    s_enabled = false;
                }
                else
                {
                    s_generation++;
                    resetGeneration();
                }

                lock (s_readLock)
                {
                    foreach (byte[] chunk in tail)
                        s_chunks.Enqueue(chunk);

                    s_stopping = stop;
                    Monitor.PulseAll(s_readLock);
                }

                if (!stop)
                    beginGeneration();
            }

            if (stop)
            {
                advancer = s_advancer;
                s_advancer = null;
                s_advancerStop?.Set();
                s_epoch++;
            }

            lock (s_readLock)
            {
                while (stop ? !s_drained : s_generationsDone < ended && !s_drained)
                    Monitor.Wait(s_readLock);
            }
        }

        // Outside the advance lock: the advancer may be waiting for it, to find the trace gone.
        if (advancer is not null && advancer != Thread.CurrentThread)
            advancer.Join();
    }

    /// <summary>
    /// The next chunk of trace data, as <c>runtime.ReadTrace</c>: blocks until data is available, and
    /// returns <c>null</c> once the trace has stopped and every byte has been read. Calling it again
    /// after an end-of-generation chunk is what tells an advance that its generation was filed.
    /// </summary>
    public static byte[]? Read()
    {
        lock (s_readLock)
        {
            if (s_endSignalOut)
            {
                s_endSignalOut = false;
                s_generationsDone++;
                Monitor.PulseAll(s_readLock);
            }

            while (true)
            {
                if (s_chunks.TryDequeue(out byte[]? chunk))
                {
                    // An event batch is never one byte long, so a one-byte chunk is the end signal.
                    if (chunk is [EvEndOfGeneration])
                        s_endSignalOut = true;

                    return chunk;
                }

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

    /// <summary>
    /// Trace clock units per second, as <c>runtime.traceClockUnitsPerSecond</c>: the timestamps are
    /// <see cref="Stopwatch"/> ticks.
    /// </summary>
    public static ulong ClockUnitsPerSecond => (ulong)Stopwatch.Frequency;

    /// <summary>The trace clock now, as <c>runtime.traceClockNow</c>.</summary>
    public static ulong Now() => (ulong)Stopwatch.GetTimestamp();

    // ---- the user annotations (runtime's trace_userTaskCreate and its siblings) ----

    // Each is written on the calling goroutine, which must be running on an M holding a P, as Go's
    // eventWriter(GoRunning, ProcRunning) states it. A thread running no goroutine has no G to carry
    // the event, so it writes nothing (Go always has one).

    /// <summary>A task begins: <c>trace.NewTask</c> (EvUserTaskBegin).</summary>
    public static void OnUserTaskBegin(ulong id, ulong parentId, ReadOnlySpan<byte> name)
    {
        if (!s_enabled || Goroutine.Current is not { } g)
            return;

        lock (s_lock)
        {
            if (running(g) is { } m)
                emit(m, EvUserTaskBegin, id, parentId, intern(name), 0);
        }
    }

    /// <summary>A task ends: <c>Task.End</c> (EvUserTaskEnd).</summary>
    public static void OnUserTaskEnd(ulong id)
    {
        if (!s_enabled || Goroutine.Current is not { } g)
            return;

        lock (s_lock)
        {
            if (running(g) is { } m)
                emit(m, EvUserTaskEnd, id, 0);
        }
    }

    /// <summary>A region begins (mode 0) or ends (mode 1): <c>trace.StartRegion</c> and <c>Region.End</c>.</summary>
    public static void OnUserRegion(ulong id, ulong mode, ReadOnlySpan<byte> name)
    {
        byte type = mode switch
        {
            0 => EvUserRegionBegin,
            1 => EvUserRegionEnd,
            _ => 0
        };

        if (type == 0 || !s_enabled || Goroutine.Current is not { } g)
            return;

        lock (s_lock)
        {
            if (running(g) is { } m)
                emit(m, type, id, intern(name), 0);
        }
    }

    /// <summary>A log message: <c>trace.Log</c> (EvUserLog).</summary>
    public static void OnUserLog(ulong id, ReadOnlySpan<byte> category, ReadOnlySpan<byte> message)
    {
        if (!s_enabled || Goroutine.Current is not { } g)
            return;

        lock (s_lock)
        {
            if (running(g) is { } m)
                emit(m, EvUserLog, id, intern(category), intern(message), 0);
        }
    }

    // The calling thread's M with `g` running on it, or null when the trace stopped before the lock.
    private static MachineState? running(Goroutine g)
    {
        if (!s_enabled)
            return null;

        MachineState m = machine();
        ensureProc(m);
        ensureRunning(m, goroutine(g));
        return m;
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
            state.Stated = true;
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
            state.Machine = null;
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

            // A target this generation has not stated yet is stated where it is first named: Waiting
            // when the trace last saw it park, or when the trace never saw it (it parked before the
            // trace started, or between the prologue's snapshot and this call).
            if (!state.Stated)
            {
                switch (state.Status)
                {
                    case Status.Unknown:
                    case Status.Waiting:
                        state.Status = Status.Waiting;
                        emit(m, EvGoStatus, state.Id, NoThread, GoWaiting);
                        break;
                    case Status.Runnable:
                        emit(m, EvGoStatus, state.Id, NoThread, GoRunnable);
                        break;
                    default:
                        return;
                }

                state.Stated = true;
                state.Seq = 0;
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
            state.Machine = null;
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

    // ---- generations (all under s_lock) ----

    // Opens the current generation: its sync batch, then its prologue. Go writes the sync batch first
    // ("as early in the trace as possible") and then states the goroutines.
    private static void beginGeneration()
    {
        release(syncBatch());

        MachineState here = machine();
        ensureProc(here);

        foreach (Goroutine g in Goroutine.Snapshot())
            stateGoroutine(here, g);
    }

    // States one goroutine in the generation now opening, from what the trace last said about it or,
    // for a goroutine the trace has never named, from the registry: parked is Waiting (Runnable once
    // readied), not yet on a thread is Runnable (created, not started), and anything else is Running on
    // its own thread, stated in THAT M's batch, where Go's parser binds a running goroutine.
    private static void stateGoroutine(MachineState here, Goroutine g)
    {
        GoroutineState state = goroutine(g);

        if (state.Stated || state.Status == Status.Dead)
            return;

        Status status = state.Status;

        if (status == Status.Unknown)
        {
            if (g.IsParked)
                status = g.IsReadied ? Status.Runnable : Status.Waiting;
            else
                status = g.ThreadId == 0 ? Status.Runnable : Status.Running;
        }

        switch (status)
        {
            case Status.Running:
                MachineState on = state.Machine ?? machine(g.ThreadId);
                ensureProc(on);
                emit(on, EvGoStatus, state.Id, on.Id, GoRunning);
                on.Current = state;
                state.Machine = on;
                break;
            case Status.Runnable:
                emit(here, EvGoStatus, state.Id, NoThread, GoRunnable);
                break;
            case Status.Waiting:
                emit(here, EvGoStatus, state.Id, NoThread, GoWaiting);
                break;
            default:
                return;
        }

        state.Status = status;
        state.Stated = true;
        state.Seq = 0;
    }

    // Ends the current generation: every M's open batch, the strings batch, and the end signal.
    private static List<byte[]> endGeneration()
    {
        List<byte[]> tail = [];

        foreach (MachineState m in s_machines.Values)
        {
            if (m.Payload.Count > 0)
                tail.Add(seal(m));
        }

        tail.Add(stringsBatch());
        tail.Add([EvEndOfGeneration]);
        return tail;
    }

    // What a new generation forgets: every P and goroutine statement, and the string table. What the
    // trace last said about each live goroutine is kept, for the prologue to state it again.
    private static void resetGeneration()
    {
        foreach (MachineState m in s_machines.Values)
        {
            m.ProcStated = false;
            m.Current = null;
        }

        List<long> dead = [];

        foreach ((long id, GoroutineState state) in s_goroutines)
        {
            if (state.Status == Status.Dead)
            {
                dead.Add(id);
                continue;
            }

            state.Stated = false;
            state.Seq = 0;
        }

        foreach (long id in dead)
            s_goroutines.Remove(id);

        s_strings = new Dictionary<string, ulong>(StringComparer.Ordinal);
    }

    // Go's traceAdvancer: a generation a second while the trace runs.
    private static void startAdvancer()
    {
        ManualResetEventSlim stop = new();
        long epoch = ++s_epoch;

        Thread advancer = new(() =>
        {
            while (!stop.Wait(s_advancePeriod))
                advance(stop: false, epoch);
        })
        {
            IsBackground = true,
            Name = "go2cs trace advancer"
        };

        s_advancerStop = stop;
        s_advancer = advancer;
        advancer.Start();
    }

    // ---- the per-M and per-G state machine (all under s_lock) ----

    private static MachineState machine() => machine(Environment.CurrentManagedThreadId);

    private static MachineState machine(int id)
    {
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

    // The M's one P, stated Running before the M's first event of the generation that needs it.
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
        if (m.Current == state && state.Status == Status.Running && state.Stated)
            return;

        if (!state.Stated)
        {
            switch (state.Status)
            {
                case Status.Unknown:
                case Status.Running:
                    // Never named in this generation, and running here now: stated where it runs.
                    emit(m, EvGoStatus, state.Id, m.Id, GoRunning);
                    state.Stated = true;
                    state.Seq = 0;
                    putOn(m, state);
                    return;
                case Status.Runnable:
                    emit(m, EvGoStatus, state.Id, NoThread, GoRunnable);
                    break;
                case Status.Waiting:
                    emit(m, EvGoStatus, state.Id, NoThread, GoWaiting);
                    break;
                default:
                    return;
            }

            state.Stated = true;
            state.Seq = 0;
        }

        switch (state.Status)
        {
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

        putOn(m, state);
    }

    private static void putOn(MachineState m, GoroutineState state)
    {
        if (state.Machine is { } previous && previous != m && previous.Current == state)
            previous.Current = null;

        state.Status = Status.Running;
        state.Machine = m;
        m.Current = state;
    }

    // The generation's string table. Keys are Latin-1 strings, one char per byte, so a Go string's
    // bytes round-trip exactly, whatever their encoding.
    private static ulong intern(string text)
    {
        if (!s_strings.TryGetValue(text, out ulong id))
            s_strings[text] = id = (ulong)s_strings.Count + 1;

        return id;
    }

    private static ulong intern(ReadOnlySpan<byte> text) => intern(Encoding.Latin1.GetString(text));

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
            byte[] bytes = Encoding.Latin1.GetBytes(text);
            payload.Add(EvString);
            uvarint(payload, id);
            uvarint(payload, (ulong)bytes.Length);
            payload.AddRange(bytes);
        }

        return batchOf(NoThread, (ulong)s_lastTimestamp, payload);
    }

    // Go's traceSyncBatch (runtime/tracetime.go): EvSync, the frequency, and a clock snapshot of the
    // trace clock, the monotonic clock and the wall clock, in a batch of no M.
    private static byte[] syncBatch()
    {
        DateTimeOffset wall = DateTimeOffset.UtcNow;
        long mono = MonotonicClock.Nanoseconds();
        long now = timestamp();

        List<byte> payload = [EvSync, EvFrequency];
        uvarint(payload, (ulong)Stopwatch.Frequency);
        payload.Add(EvClockSnapshot);
        uvarint(payload, 0);
        uvarint(payload, (ulong)mono);
        uvarint(payload, (ulong)wall.ToUnixTimeSeconds());
        uvarint(payload, (ulong)(wall.UtcTicks % TimeSpan.TicksPerSecond * 100));

        return batchOf(NoThread, (ulong)now, payload);
    }

    private static byte[] batchOf(ulong m, ulong timestamp, List<byte> payload)
    {
        List<byte> batch = new(payload.Count + 32) { EvEventBatch };
        uvarint(batch, s_generation);
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
