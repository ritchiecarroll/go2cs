// Copyright 2023 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.
namespace go.@internal.trace.@internal.testgen;

using bytes = bytes_package;
using binary = encoding.binary_package;
using fmt = fmt_package;
using os = os_package;
using regexp = regexp_package;
using strings = strings_package;
using trace = go.@internal.trace_package;
using @event = go.@internal.trace.event_package;
using go122 = go.@internal.trace.@event.go122_package;
using raw = go.@internal.trace.raw_package;
using version = go.@internal.trace.version_package;
using txtar = go.@internal.txtar_package;
using encoding;
using go.@internal;
using go.@internal.trace;
using go.@internal.trace.@event;
using io = io_package;
using ꓸꓸꓸany = Span<any>;
using ꓸꓸꓸuint64 = Span<uint64>;

partial class testkit_package {

public static void ΔMain(Action<ж<Trace>> f) {
    GoFrame ᒐ = default;
    try {
        // Create an output file.
        var (@out, err) = os.Create(os.Args[1]);
        if (err != default!) {
            throw panic(err.Error());
        }
        var outʗ1 = @out;
        defer(() => outʗ1.Close(), ref ᒐ);
        // Create a new trace.
        var trace = NewTrace();
        // Call the generator.
        f(trace);
        // Write out the generator's state.
        {
            var (_, errΔ1) = @out.Write(trace.Generate()); if (errΔ1 != default!) {
                throw panic(errΔ1.Error());
            }
        }
    }
    catch (Exception ᒐex) when (GoFrame.IsPanic(ᒐex, out PanicException? ᒐp)) { GoFrame.Capture(ᒐp); }
    finally { ᒐ.Run(); }
}

// Trace represents an execution trace for testing.
//
// It does a little bit of work to ensure that the produced trace is valid,
// just for convenience. It mainly tracks batches and batch sizes (so they're
// trivially correct), tracks strings and stacks, and makes sure emitted string
// and stack batches are valid. That last part can be controlled by a few options.
//
// Otherwise, it performs no validation on the trace at all.
[GoType] partial struct Trace {
    // Trace data state.
    internal version.Version ver;
    internal map<@string, @event.Type> names;
    internal slice<@event.Spec> specs;
    internal slice<raw.Event> events;
    internal slice<ж<ΔGeneration>> gens;
    internal bool validTimestamps;
    // Expectation state.
    internal bool bad;
    internal ж<regexp.Regexp> badMatch;
}

// NewTrace creates a new trace.
public static ж<Trace> NewTrace() {
    var ver = version.Go122;
    return Ꮡ(new Trace(
        names: @event.Names(ver.Specs()),
        specs: ver.Specs(),
        validTimestamps: true
    ));
}

// ExpectFailure writes down that the trace should be broken. The caller
// must provide a pattern matching the expected error produced by the parser.
[GoRecv] public static void ExpectFailure(this ref Trace t, @string pattern) {
    t.bad = true;
    t.badMatch = regexp.MustCompile(pattern);
}

// ExpectSuccess writes down that the trace should successfully parse.
[GoRecv] public static void ExpectSuccess(this ref Trace t) {
    t.bad = false;
}

// RawEvent emits an event into the trace. name must correspond to one
// of the names in Specs() result for the version that was passed to
// this trace.
[GoRecv] public static void RawEvent(this ref Trace t, @event.Type typ, slice<byte> data, params ꓸꓸꓸuint64 argsʗp) {
    var args = argsʗp.sslice();

    t.events = append(t.events, t.createEvent(typ, data, args.ꓸꓸꓸ));
}

// DisableTimestamps makes the timestamps for all events generated after
// this call zero. Raw events are exempted from this because the caller
// has to pass their own timestamp into those events anyway.
[GoRecv] public static void DisableTimestamps(this ref Trace t) {
    t.validTimestamps = false;
}

// Generation creates a new trace generation.
//
// This provides more structure than Event to allow for more easily
// creating complex traces that are mostly or completely correct.
public static ж<ΔGeneration> Generation(this ж<Trace> Ꮡt, uint64 gen) {
    ref var t = ref Ꮡt.DerefOrNull();

    var g = Ꮡ(new ΔGeneration(
        trace: Ꮡt,
        gen: gen,
        strings: new map<@string, uint64>(),
        stacks: new map<stack, uint64>()
    ));
    t.gens = append(t.gens, g);
    return g;
}

// Generate creates a test file for the trace.
[GoRecv] public static slice<byte> Generate(this ref Trace t) {
    // Trace file contents.
    ref var buf = ref heap(new bytes.Buffer(), out var Ꮡbuf);
    var (tw, err) = raw.NewTextWriter(new bytes_BufferжWriter(Ꮡbuf), version.Go122);
    if (err != default!) {
        throw panic(err.Error());
    }
    // Write raw top-level events.
    foreach (var (_, e) in t.events) {
        tw.WriteEvent(e);
    }
    // Write generations.
    foreach (var (_, g) in t.gens) {
        g.writeEventsTo(tw);
    }
    // Expectation file contents.
    var expect = slice<byte>("SUCCESS\n"u8);
    if (t.bad) {
        expect = slice<byte>(fmt.Sprintf("FAILURE %q\n"u8, t.badMatch.OrTypedNil()));
    }
    // Create the test file's contents.
    return txtar.Format(Ꮡ(new txtar.Archive(
        Files: new txtar.File[]{
            new(Name: "expect"u8, Data: expect),
            new(Name: "trace"u8, Data: buf.Bytes())
        }.slice()
    )));
}

[GoRecv] internal static raw.Event createEvent(this ref Trace t, @event.Type ev, slice<byte> data, params ꓸꓸꓸuint64 argsʗp) {
    var args = argsʗp.slice();

    var spec = t.specs[ev];
    if (ev != go122.EvStack) {
        {
            nint arity = len(spec.Args); if (len(args) != arity) {
                throw panic(fmt.Sprintf("expected %d args for %s, got %d"u8, arity, spec.Name, len(args)));
            }
        }
    }
    return new raw.Event(
        Version: version.Go122,
        Ev: ev,
        Args: args,
        Data: data
    );
}

[GoType] partial struct stack {
    internal array<trace.StackFrame> stk = new(32);
    internal nint len;
}

public static @string NoString = ""u8;
public static slice<trace.StackFrame> NoStack = new trace.StackFrame[]{}.slice();

// Generation represents a single generation in the trace.
[GoType] partial struct ΔGeneration {
    internal ж<Trace> trace;
    internal uint64 gen;
    internal slice<ж<ΔBatch>> batches;
    internal map<@string, uint64> strings;
    internal map<stack, uint64> stacks;
    // Options applied when Trace.Generate is called.
    internal bool ignoreStringBatchSizeLimit;
    internal bool ignoreStackBatchSizeLimit;
}

// Batch starts a new event batch in the trace data.
//
// This is convenience function for generating correct batches.
public static ж<ΔBatch> Batch(this ж<ΔGeneration> Ꮡg, trace.ThreadID thread, Time time) {
    ref var g = ref Ꮡg.DerefOrNull();

    if (!(~g.trace).validTimestamps) {
        time = 0;
    }
    var b = Ꮡ(new ΔBatch(
        gen: Ꮡg,
        thread: thread,
        timestamp: time
    ));
    g.batches = append(g.batches, b);
    return b;
}

// String registers a string with the trace.
//
// This is a convenience function for easily adding correct
// strings to traces.
[GoRecv] public static uint64 String(this ref ΔGeneration g, @string s) {
    if (len(s) == 0) {
        return 0;
    }
    {
        var (idΔ1, ok) = g.strings[s, ꟷ]; if (ok) {
            return idΔ1;
        }
    }
    var id = (uint64)(len(g.strings) + 1);
    g.strings[s] = id;
    return id;
}

// Stack registers a stack with the trace.
//
// This is a convenience function for easily adding correct
// stacks to traces.
[GoRecv] public static uint64 Stack(this ref ΔGeneration g, slice<trace.StackFrame> stk) {
    if (len(stk) == 0) {
        return 0;
    }
    if (len(stk) > 32) {
        throw panic("stack too big for test");
    }
    stack stkc = new();
    copy(stkc.stk[..], stk);
    stkc.len = len(stk);
    {
        var (idΔ1, ok) = g.stacks[stkc, ꟷ]; if (ok) {
            return idΔ1;
        }
    }
    var id = (uint64)(len(g.stacks) + 1);
    g.stacks[stkc] = id;
    return id;
}

// writeEventsTo emits event batches in the generation to tw.
internal static void writeEventsTo(this ж<ΔGeneration> Ꮡg, ж<raw.TextWriter> Ꮡtw) {
    ref var g = ref Ꮡg.DerefOrNull();

    // Write event batches for the generation.
    foreach (var (_, bΔ1) in g.batches) {
        bΔ1.writeEventsTo(Ꮡtw);
    }
    // Write frequency.
    var b = Ꮡg.newStructuralBatch();
    b.RawEvent(go122.EvFrequency, default!, 15625000);
    b.writeEventsTo(Ꮡtw);
    // Write stacks.
    b = Ꮡg.newStructuralBatch();
    b.RawEvent(go122.EvStacks, default!);
    foreach (var (kᴛ1, id) in g.stacks) {
        var stk = kᴛ1.ΔClone();

        var stkΔ1 = stk.stk.slice(0, stk.len);
        var args = new uint64[]{id}.slice();
        foreach (var (_, f) in stkΔ1) {
            args = append(args, f.PC, g.String(f.Func), g.String(f.File), f.Line);
        }
        b.RawEvent(go122.EvStack, default!, args.ꓸꓸꓸ);
        // Flush the batch if necessary.
        if (!g.ignoreStackBatchSizeLimit && (~b).size > (uint64)(go122.MaxBatchSize / 2)) {
            b.writeEventsTo(Ꮡtw);
            b = Ꮡg.newStructuralBatch();
        }
    }
    b.writeEventsTo(Ꮡtw);
    // Write strings.
    b = Ꮡg.newStructuralBatch();
    b.RawEvent(go122.EvStrings, default!);
    foreach (var (s, id) in g.strings) {
        b.RawEvent(go122.EvString, slice<byte>(s), id);
        // Flush the batch if necessary.
        if (!g.ignoreStringBatchSizeLimit && (~b).size > (uint64)(go122.MaxBatchSize / 2)) {
            b.writeEventsTo(Ꮡtw);
            b = Ꮡg.newStructuralBatch();
        }
    }
    b.writeEventsTo(Ꮡtw);
}

internal static ж<ΔBatch> newStructuralBatch(this ж<ΔGeneration> Ꮡg) {
    return Ꮡ(new ΔBatch(gen: Ꮡg, thread: trace.NoThread));
}

// Batch represents an event batch.
[GoType] partial struct ΔBatch {
    internal ж<ΔGeneration> gen;
    internal trace.ThreadID thread;
    internal Time timestamp;
    internal uint64 size;
    internal slice<raw.Event> events;
}

// Event emits an event into a batch. name must correspond to one
// of the names in Specs() result for the version that was passed to
// this trace. Callers must omit the timestamp delta.
[GoRecv] public static void Event(this ref ΔBatch b, @string name, params ꓸꓸꓸany argsʗp) {
    var args = argsʗp.sslice();

    var (ev, ok) = (~(~b.gen).trace).names[name, ꟷ];
    if (!ok) {
        throw panic(fmt.Sprintf("invalid or unknown event %s"u8, name));
    }
    slice<uint64> uintArgs = default!;
    nint argOff = 0;
    if ((~(~b.gen).trace).specs[ev].IsTimedEvent) {
        if ((~(~b.gen).trace).validTimestamps){
            uintArgs = new uint64[]{1}.slice();
        } else {
            uintArgs = new uint64[]{0}.slice();
        }
        argOff = 1;
    }
    var spec = (~(~b.gen).trace).specs[ev];
    {
        nint arity = len(spec.Args) - argOff; if (len(args) != arity) {
            throw panic(fmt.Sprintf("expected %d args for %s, got %d"u8, arity, spec.Name, len(args)));
        }
    }
    foreach (var (i, arg) in args) {
        uintArgs = append(uintArgs, b.uintArgFor(arg, spec.Args[i + argOff]));
    }
    b.RawEvent(ev, default!, uintArgs.ꓸꓸꓸ);
}

[GoRecv] internal static uint64 uintArgFor(this ref ΔBatch b, any arg, @string argSpec) {
    var components = strings.SplitN(argSpec, "_"u8, 2);
    @string typStr = components[0];
    if (len(components) == 2) {
        typStr = components[1];
    }
    uint64 u = default!;
    var exprᴛ1 = typStr;
    if (exprᴛ1 == "value"u8) {
        u = arg._<uint64>();
    }
    else if (exprᴛ1 == "stack"u8) {
        u = b.gen.Stack(arg._<slice<trace.StackFrame>>());
    }
    else if (exprᴛ1 == "seq"u8) {
        u = (uint64)(arg._<Seq>());
    }
    else if (exprᴛ1 == "pstatus"u8) {
        u = (uint64)(uint8)(arg._<go122.ProcStatus>());
    }
    else if (exprᴛ1 == "gstatus"u8) {
        u = (uint64)(uint8)(arg._<go122.GoStatus>());
    }
    else if (exprᴛ1 == "g"u8) {
        u = (uint64)(int64)(arg._<trace.GoID>());
    }
    else if (exprᴛ1 == "m"u8) {
        u = (uint64)(int64)(arg._<trace.ThreadID>());
    }
    else if (exprᴛ1 == "p"u8) {
        u = (uint64)(int64)(arg._<trace.ProcID>());
    }
    else if (exprᴛ1 == "string"u8) {
        u = b.gen.String(arg._<@string>());
    }
    else if (exprᴛ1 == "task"u8) {
        u = (uint64)(arg._<trace.TaskID>());
    }
    else { /* default: */
        throw panic(fmt.Sprintf("unsupported arg type %q for spec %q"u8, typStr, argSpec));
    }

    return u;
}

// RawEvent emits an event into a batch. name must correspond to one
// of the names in Specs() result for the version that was passed to
// this trace.
[GoRecv] public static void RawEvent(this ref ΔBatch b, @event.Type typ, slice<byte> data, params ꓸꓸꓸuint64 argsʗp) {
    var args = argsʗp.sslice();

    var ev = (~b.gen).trace.createEvent(typ, data, args.ꓸꓸꓸ);
    // Compute the size of the event and add it to the batch.
    b.size += 1; // One byte for the event header.
    array<byte> buf = new(10); /* binary.MaxVarintLen64 */
    foreach (var (_, arg) in args) {
        b.size += (uint64)binary.PutUvarint(buf[..], arg);
    }
    if (len(data) != 0) {
        b.size += (uint64)binary.PutUvarint(buf[..], (uint64)len(data));
        b.size += (uint64)len(data);
    }
    // Add the event.
    b.events = append(b.events, ev);
}

// writeEventsTo emits events in the batch, including the batch header, to tw.
[GoRecv] internal static void writeEventsTo(this ref ΔBatch b, ж<raw.TextWriter> Ꮡtw) {
    ref var tw = ref Ꮡtw.DerefOrNull();

    tw.WriteEvent(new raw.Event(
        Version: version.Go122,
        Ev: go122.EvEventBatch,
        Args: new uint64[]{(~b.gen).gen, (uint64)(int64)b.thread, (uint64)b.timestamp, b.size}.slice()
    ));
    foreach (var (_, e) in b.events) {
        tw.WriteEvent(e);
    }
}

[GoType("num:uint64")] partial struct Seq;

[GoType("num:uint64")] partial struct Time;

} // end testkit_package
