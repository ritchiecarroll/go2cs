using System;
using System.Collections.Generic;
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

    // Runs `window` between Start and Stop with a reader draining, and returns every byte read.
    private static byte[] Trace(Action window)
    {
        MemoryStream bytes = new();

        Thread reader = new(() =>
        {
            while (ExecutionTracer.Read() is { } chunk)
                bytes.Write(chunk);
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

        Assert.AreEqual("go 1.23 trace\0\0\0", Encoding.ASCII.GetString(trace, 0, 16), "the header the go1.24.13 runtime writes");
        Assert.IsTrue(trace.Length > 16, "a stopped trace carries at least its frequency batch after the header");
    }

    [TestMethod]
    public void OneGenerationWithExactlyOneFrequencyAndOneStringsBatch()
    {
        List<Batch> batches = Parse(Trace(() => { }));

        Assert.IsTrue(batches.All(b => b.Gen == 1), "the whole window is one generation");
        Assert.AreEqual(1, batches.Count(b => b.Payload[0] == EvFrequency), "exactly one frequency batch, as the parser requires");
        Assert.AreEqual(1, batches.Count(b => b.Payload[0] == EvStrings), "one strings batch");
        Assert.IsTrue(batches.All(b => b.Payload.Length <= 64 << 10), "no batch payload over 64 KiB");
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

        List<Batch> batches = Parse(trace);
        Dictionary<ulong, string> strings = Strings(batches);
        List<(byte Type, ulong[] Args)> events = batches.Where(b => b.Payload[0] is not (EvFrequency or EvStrings)).SelectMany(Events).ToList();
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

    internal static List<Batch> Parse(byte[] trace)
    {
        List<Batch> batches = [];
        int at = 16;

        while (at < trace.Length)
        {
            Assert.AreEqual(EvEventBatch, trace[at++], "every batch opens with EvEventBatch");
            ulong gen = UVarint(trace, ref at), m = UVarint(trace, ref at), ts = UVarint(trace, ref at);
            int length = (int)UVarint(trace, ref at);
            batches.Add(new Batch(gen, m, ts, trace[at..(at + length)]));
            at += length;
        }

        return batches;
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
