using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using go.golib;

namespace GolibTests;

/// <summary>
/// The managed execution tracer (Q28, docs/phase4/DESIGN-managed-execution-tracer.md): its state
/// machine, its framing, and the goroutine events it writes.
/// </summary>
/// <remarks>
/// <para>
/// Red first: until 2026-09-27 runtime.StartTrace answered "tracing is not supported" and runtime/trace's
/// two tests failed on it. These arms were made to fail by forcing <see cref="ExecutionTracer.Start"/> to
/// refuse, then pass with it restored byte-identical. The acceptance oracle for the stream's validity --
/// Go's own parser -- is C-3's arm, not these; these decode the stream only as far as the framing and the
/// event types the tracer promises.
/// </para>
/// <para>
/// Tracing is process-global, so each arm owns a whole Start..Stop window and drains it on a reader
/// thread, which is what runtime/trace.Start's goroutine does: StopTrace returns only after the reader
/// has taken the last byte. MSTest runs this assembly serially.
/// </para>
/// </remarks>
[TestClass]
public class ExecutionTracerTests
{
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

    // Runs `window` between Start and Stop with a reader draining, and returns every byte read.
    private static byte[] Trace(Action window) => Trace(window, onChunk: null);

    // The same, with `onChunk` called on the reader thread after each chunk it takes, before it reads
    // again: the place a subscriber does its work, which the advance contract orders against.
    private static byte[] Trace(Action window, Action<byte[]>? onChunk)
    {
        MemoryStream bytes = new();

        Thread reader = new(() =>
        {
            while (ExecutionTracer.Read() is { } chunk)
            {
                lock (bytes)
                    bytes.Write(chunk);

                onChunk?.Invoke(chunk);
            }
        });

        Assert.IsTrue(ExecutionTracer.Start(), "no trace may be running when an arm starts one");
        reader.Start();

        try
        {
            window();
        }
        finally
        {
            ExecutionTracer.Stop();
            reader.Join();
        }

        return bytes.ToArray();
    }

    [TestMethod]
    public void ASecondStartIsRefusedAndStopIsIdempotent()
    {
        byte[] trace = Trace(() => Assert.IsFalse(ExecutionTracer.Start(), "a second Start while tracing must fail, as Go's does"));

        ExecutionTracer.Stop();
        ExecutionTracer.Stop();

        Assert.AreEqual("go 1.26 trace\0\0\0", Encoding.ASCII.GetString(trace, 0, 16), "the header the go1.26 runtime writes");
        Assert.IsTrue(trace.Length > 16, "a stopped trace carries at least its sync batch after the header");
    }

    [TestMethod]
    public void EveryGenerationHasOneSyncBatchAndEndsWithItsEndOfGenerationSignal()
    {
        // Two advances inside the window, and the stop's own: three generations.
        List<Batch> items = Parse(Trace(() =>
        {
            ExecutionTracer.Advance(false);
            ExecutionTracer.Advance(false);
        }));

        List<List<Batch>> generations = Generations(items);
        Assert.AreEqual(3, generations.Count, "two advances and a stop end three generations, each with its in-band end signal");
        Assert.AreEqual(EvEndOfGeneration, items[^1].Payload[0], "the stream ends with an end-of-generation signal (Go 1.26's parser refuses a truncated generation)");

        for (int i = 0; i < generations.Count; i++)
        {
            List<Batch> generation = generations[i];
            ulong gen = (ulong)i + 1;

            Assert.IsTrue(generation.All(b => b.Gen == gen), $"generation {gen}'s batches all carry its number");

            Batch sync = generation.Single(b => b.Payload[0] == EvSync);
            Assert.AreEqual(ulong.MaxValue, sync.M, "the sync batch belongs to no M, as Go's unsafeTraceWriter(gen, nil) writes it");
            CollectionAssert.AreEqual(new[] { EvFrequency, EvClockSnapshot }, SyncEvents(sync).ToArray(), "the sync batch: one frequency, one clock snapshot");

            Assert.AreEqual(1, generation.Count(b => b.Payload[0] == EvStrings), $"generation {gen} has one strings batch");
            Assert.IsTrue(generation.All(b => b.Payload.Length <= 64 << 10), "no batch payload over 64 KiB");
        }
    }

    [TestMethod]
    public void AdvanceReturnsOnlyAfterTheReaderHasTakenTheGeneration()
    {
        // flightrecorder.go:127: "traceAdvance blocks until the tracer goroutine has actually written
        // everything out", which WriteTo's read of the ring depends on. Go's reader releases the
        // advance (doneSema) when it comes back to ReadTrace after the end-of-generation chunk.
        // The reader here dawdles after that chunk; Advance must not return before it is back.
        long readerBack = 0;
        long advanceReturned = 0;
        int endSignals = 0;

        Trace(() =>
        {
            ExecutionTracer.Advance(false);
            Interlocked.Exchange(ref advanceReturned, Stopwatch.GetTimestamp());
        },
        chunk =>
        {
            if (chunk is [EvEndOfGeneration] && Interlocked.Increment(ref endSignals) == 1)
            {
                Thread.Sleep(300);
                Interlocked.Exchange(ref readerBack, Stopwatch.GetTimestamp());
            }
        });

        Assert.IsTrue(Volatile.Read(ref endSignals) >= 1, "the advance must send its end-of-generation signal to the reader");
        Assert.IsTrue(Volatile.Read(ref readerBack) != 0 && Volatile.Read(ref advanceReturned) >= Volatile.Read(ref readerBack),
            "Advance(false) returned before the reader had finished with the generation it ended");
    }

    [TestMethod]
    public void UserAnnotationsAreTracedOnTheCallersGoroutine()
    {
        using ManualResetEventSlim done = new();
        Goroutine? annotator = null;

        byte[] trace = Trace(() =>
        {
            Goroutine.Start(() =>
            {
                annotator = Goroutine.Current;
                ExecutionTracer.OnUserTaskBegin(7, 0, "task"u8);
                ExecutionTracer.OnUserRegion(7, 0, "region"u8);
                ExecutionTracer.OnUserLog(7, "message"u8, "hello"u8);
                ExecutionTracer.OnUserRegion(7, 1, "region"u8);
                ExecutionTracer.OnUserTaskEnd(7);
                done.Set();
            });

            Assert.IsTrue(done.Wait(TimeSpan.FromSeconds(10)));
        });

        List<Batch> batches = Parse(trace).Where(b => b.Payload[0] != EvEndOfGeneration).ToList();
        Dictionary<ulong, string> strings = Strings(batches);
        List<(byte Type, ulong[] Args)> events = batches.Where(b => b.Payload[0] is not (EvSync or EvStrings)).SelectMany(Events).ToList();

        (byte _, ulong[] begin) = events.Single(e => e.Type == EvUserTaskBegin);
        Assert.AreEqual(7UL, begin[0]);
        Assert.AreEqual("task", strings[begin[2]]);

        (byte _, ulong[] log) = events.Single(e => e.Type == EvUserLog);
        Assert.AreEqual(7UL, log[0]);
        Assert.AreEqual("message", strings[log[1]]);
        Assert.AreEqual("hello", strings[log[2]]);

        Assert.AreEqual("region", strings[events.Single(e => e.Type == EvUserRegionBegin).Args[1]]);
        Assert.AreEqual("region", strings[events.Single(e => e.Type == EvUserRegionEnd).Args[1]]);
        Assert.AreEqual(7UL, events.Single(e => e.Type == EvUserTaskEnd).Args[0]);
        Assert.IsNotNull(annotator);
    }

    [TestMethod]
    public void AGoroutinesLifeIsTracedWithGosTraceVocabulary()
    {
        Goroutine? child = null;
        using SemaphoreSlim gate = new(0, 1);
        using ManualResetEventSlim parked = new();

        byte[] trace = Trace(() =>
        {
            Goroutine.Start(() =>
            {
                child = Goroutine.Current;

                using (Goroutine.Park(WaitReason.ChanReceive))
                {
                    parked.Set();
                    gate.Wait();
                }
            });

            Assert.IsTrue(parked.Wait(TimeSpan.FromSeconds(10)), "the goroutine must park");
            Goroutine.Ready(child);
            gate.Release();

            // The destroy is traced as the goroutine's thread retires it; wait for that before Stop.
            SpinWait.SpinUntil(() => Goroutine.FromId(child!.Id) is null, TimeSpan.FromSeconds(10));
        });

        List<Batch> batches = Parse(trace).Where(b => b.Payload[0] != EvEndOfGeneration).ToList();
        Dictionary<ulong, string> strings = Strings(batches);
        List<(byte Type, ulong[] Args)> events = batches.Where(b => b.Payload[0] is not (EvSync or EvStrings)).SelectMany(Events).ToList();
        ulong id = (ulong)child!.Id;

        Assert.IsTrue(events.Any(e => e.Type == EvGoCreate && e.Args[0] == id), "GoCreate for the child, on the creating thread");
        Assert.IsTrue(events.Any(e => e.Type == EvGoStart && e.Args[0] == id), "GoStart on its own thread");

        (byte _, ulong[] block) = events.Single(e => e.Type == EvGoBlock);
        Assert.AreEqual("chan receive", strings[block[0]], "Go's trace block reason, not the traceback text");

        Assert.IsTrue(events.Any(e => e.Type == EvGoUnblock && e.Args[0] == id), "GoUnblock on the waker's side");
        Assert.IsTrue(events.Any(e => e.Type == EvGoDestroy), "GoDestroy as it exits");
        Assert.IsTrue(events.Any(e => e.Type == EvProcStatus), "every M states its one P");
    }

    // ---- a decoder for exactly the framing and events the tracer writes ----

    internal sealed record Batch(ulong Gen, ulong M, ulong Timestamp, byte[] Payload);

    // Every batch in stream order, with each in-band end-of-generation signal as an item of its own
    // (Gen 0, M 0, its one byte as the payload), where Go 1.26's reader meets it.
    internal static List<Batch> Parse(byte[] trace)
    {
        List<Batch> batches = [];
        int at = 16;

        while (at < trace.Length)
        {
            if (trace[at] == EvEndOfGeneration)
            {
                batches.Add(new Batch(0, 0, 0, [trace[at++]]));
                continue;
            }

            Assert.AreEqual(EvEventBatch, trace[at++], "every batch opens with EvEventBatch");
            ulong gen = UVarint(trace, ref at), m = UVarint(trace, ref at), ts = UVarint(trace, ref at);
            int length = (int)UVarint(trace, ref at);
            batches.Add(new Batch(gen, m, ts, trace[at..(at + length)]));
            at += length;
        }

        return batches;
    }

    // The batches of each generation, split at its end-of-generation signal. A generation without one
    // at the end of the stream is returned too, so an arm can see it is missing its signal.
    private static List<List<Batch>> Generations(List<Batch> items)
    {
        List<List<Batch>> generations = [];
        List<Batch> current = [];

        foreach (Batch item in items)
        {
            if (item.Payload[0] == EvEndOfGeneration)
            {
                generations.Add(current);
                current = [];
                continue;
            }

            current.Add(item);
        }

        if (current.Count > 0)
            generations.Add(current);

        return generations;
    }

    // The event types inside a sync batch, after its EvSync header.
    private static IEnumerable<byte> SyncEvents(Batch sync)
    {
        Assert.AreEqual(EvSync, sync.Payload[0]);
        int at = 1;

        while (at < sync.Payload.Length)
        {
            byte type = sync.Payload[at++];
            int args = type switch
            {
                EvFrequency => 1,
                EvClockSnapshot => 4,
                _ => throw new AssertFailedException($"a sync batch carries only frequency and clock snapshot events, not {type}")
            };

            for (int i = 0; i < args; i++)
                UVarint(sync.Payload, ref at);

            yield return type;
        }
    }

    private static Dictionary<ulong, string> Strings(List<Batch> batches)
    {
        Dictionary<ulong, string> strings = new();
        byte[] payload = batches.Single(b => b.Payload[0] == EvStrings).Payload;
        int at = 1;

        while (at < payload.Length)
        {
            Assert.AreEqual(EvString, payload[at++]);
            ulong id = UVarint(payload, ref at);
            int length = (int)UVarint(payload, ref at);
            strings[id] = Encoding.UTF8.GetString(payload, at, length);
            at += length;
        }

        return strings;
    }

    // Argument counts after dt, from go122's specs, for the events this tracer writes.
    private static int Arity(byte type) => type switch
    {
        EvProcStatus => 2,
        EvGoCreate => 3,
        EvGoStart => 2,
        EvGoDestroy => 0,
        EvGoBlock => 2,
        EvGoUnblock => 3,
        EvGoStatus => 3,
        EvUserTaskBegin => 4,
        EvUserTaskEnd => 2,
        EvUserRegionBegin => 3,
        EvUserRegionEnd => 3,
        EvUserLog => 4,
        _ => throw new AssertFailedException($"the tracer wrote event type {type}, which it never promises")
    };

    private static IEnumerable<(byte, ulong[])> Events(Batch batch)
    {
        List<(byte, ulong[])> events = [];
        int at = 0;

        while (at < batch.Payload.Length)
        {
            byte type = batch.Payload[at++];
            UVarint(batch.Payload, ref at);
            ulong[] args = new ulong[Arity(type)];

            for (int i = 0; i < args.Length; i++)
                args[i] = UVarint(batch.Payload, ref at);

            events.Add((type, args));
        }

        return events;
    }

    private static ulong UVarint(byte[] data, ref int at)
    {
        ulong value = 0;

        for (int shift = 0; ; shift += 7)
        {
            byte b = data[at++];
            value |= (ulong)(b & 0x7F) << shift;

            if (b < 0x80)
                return value;
        }
    }
}
