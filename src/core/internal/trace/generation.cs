// Copyright 2023 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.
namespace go.@internal;

using bufio = bufio_package;
using bytes = bytes_package;
using cmp = cmp_package;
using binary = encoding.binary_package;
using errors = errors_package;
using fmt = fmt_package;
using io = io_package;
using slices = slices_package;
using strings = strings_package;
using time = time_package;
using tracev2 = go.@internal.trace.tracev2_package;
using version = go.@internal.trace.version_package;
using encoding;
using go.@internal.trace;

partial class trace_package {

// generation contains all the trace data for a single
// trace generation. It is purely data: it does not
// track any parse state nor does it contain a cursor
// into the generation.
partial struct generation {
    internal uint64 gen;
    internal map<ThreadID, slice<batch>> batches;
    internal slice<ThreadID> batchMs;
    internal slice<cpuSample> cpuSamples;
    internal timestamp minTs;
    /*embed*/ internal ж<evTable> evTable;
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static readonly @string internalErrorˢ = "internal error: readGeneration called for <1.26 trace"u8;
internal static readonly @string incompleteGenerationˢ = "incomplete generation found; trace likely truncated"u8;

// readGeneration buffers and decodes the structural elements of a trace generation
// out of r.
internal static (ж<generation>, error) readGeneration(ж<bufio.Reader> Ꮡr, version.Version ver) {
    if (ver < version.Go126) {
        return (default!, errors.New(internalErrorˢ));
    }
    var g = Ꮡ(new generation(
        evTable: Ꮡ(new evTable(
            pcs: new map<uint64, frame>()
        )),
        batches: new map<ThreadID, slice<batch>>()
    ));

    // Read batches one at a time until we either hit the next generation.
    while (ᐧ) {
        var (b, gen, err) = readBatch(new bufio_ReaderжreadBatch_r(Ꮡr));
        if (AreEqual(err, io.EOF)) {
            if (len((~g).batches) != 0) {
                return (default!, errors.New(incompleteGenerationˢ));
            }
            return (default!, default!); // All done.
        }
        if (err != default!) {
            return (default!, err);
        }
        if ((~g).gen == 0) {
            // Initialize gen.
            g.Value.gen = gen;
        }
        if (b.isEndOfGeneration()) {
            break;
        }
        if (gen == 0) {
            // 0 is a sentinel used by the runtime, so we'll never see it.
            return (default!, fmt.Errorf("invalid generation number %d"u8, gen));
        }
        if (gen != (~g).gen) {
            return (default!, fmt.Errorf("broken trace: missing end-of-generation event, or generations are interleaved"u8));
        }
        if ((~g).minTs == 0 || b.time < (~g).minTs) {
            g.Value.minTs = b.time;
        }
        {
            var errΔ1 = processBatch(g, b, ver); if (errΔ1 != default!) {
                return (default!, errΔ1);
            }
        }
    }

    // Check some invariants.
    if ((~g).freq == 0D) {
        return (default!, fmt.Errorf("no frequency event found"u8));
    }
    if (!(~g).hasClockSnapshot) {
        return (default!, fmt.Errorf("no clock snapshot event found"u8));
    }

    // N.B. Trust that the batch order is correct. We can't validate the batch order
    // by timestamp because the timestamps could just be plain wrong. The source of
    // truth is the order things appear in the trace and the partial order sequence
    // numbers on certain events. If it turns out the batch order is actually incorrect
    // we'll very likely fail to advance a partial order from the frontier.

    // Compactify stacks and strings for better lookup performance later.
    g.of(generation.Ꮡstacks).compactify();
    g.of(generation.Ꮡstrings).compactify();

    // Validate stacks.
    {
        var err = validateStackStrings(g.of(generation.Ꮡstacks), g.of(generation.Ꮡstrings), (~g).pcs); if (err != default!) {
            return (default!, err);
        }
    }

    // Now that we have the frequency, fix up CPU samples.
    fixUpCPUSamples((~g).cpuSamples, (~g).freq);
    return (g, default!);
}

// spilledBatch represents a batch that was read out for the next generation,
// while reading the previous one. It's passed on when parsing the next
// generation.
//
// Used only for trace versions < Go126.
partial struct spilledBatch {
    internal uint64 gen;
    /*embed*/ internal ж<batch> batch;
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static readonly @string internalErrorˢ2 = "internal error: readGenerationWithSpill called for Go 1.26+ trace"u8;

// readGenerationWithSpill buffers and decodes the structural elements of a trace generation
// out of r. spill is the first batch of the new generation (already buffered and
// parsed from reading the last generation). Returns the generation and the first
// batch read of the next generation, if any.
//
// If gen is non-nil, it is valid and must be processed before handling the returned
// error.
internal static (ж<generation>, ж<spilledBatch>, error) readGenerationWithSpill(ж<bufio.Reader> Ꮡr, ж<spilledBatch> Ꮡspill, version.Version ver) {
    ref var spill = ref Ꮡspill.DerefOrNull();

    if (ver >= version.Go126) {
        return (default!, default!, errors.New(internalErrorˢ2));
    }
    var g = Ꮡ(new generation(
        evTable: Ꮡ(new evTable(
            pcs: new map<uint64, frame>()
        )),
        batches: new map<ThreadID, slice<batch>>()
    ));
    // Process the spilled batch.
    if (Ꮡspill != nil) {
        // Process the spilled batch, which contains real data.
        g.Value.gen = spill.gen;
        g.Value.minTs = spill.batch.Value.time;
        {
            var err = processBatch(g, spill.batch.Value, ver); if (err != default!) {
                return (default!, default!, err);
            }
        }
        Ꮡspill = default!; spill = ref Ꮡspill.DerefOrNull();
    }
    // Read batches one at a time until we either hit the next generation.
    error spillErr = default!;
    while (ᐧ) {
        ref var b = ref heap<batch>(out var Ꮡb);
        ref var gen = ref heap<uint64>(out var Ꮡgen);
        (b, gen, var err) = readBatch(new bufio_ReaderжreadBatch_r(Ꮡr));
        if (AreEqual(err, io.EOF)) {
            break;
        }
        if (err != default!) {
            if ((~g).gen != 0) {
                // This may be an error reading the first batch of the next generation.
                // This is fine. Let's forge ahead assuming that what we've got so
                // far is fine.
                spillErr = err;
                break;
            }
            return (default!, default!, err);
        }
        if (gen == 0) {
            // 0 is a sentinel used by the runtime, so we'll never see it.
            return (default!, default!, fmt.Errorf("invalid generation number %d"u8, gen));
        }
        if ((~g).gen == 0) {
            // Initialize gen.
            g.Value.gen = gen;
        }
        if (gen == (~g).gen + 1) {
            // TODO: Increment the generation with wraparound the same way the runtime does.
            Ꮡspill = Ꮡ(new spilledBatch(gen: gen, batch: Ꮡb)); spill = ref Ꮡspill.DerefOrNull();
            break;
        }
        if (gen != (~g).gen) {
            // N.B. Fail as fast as possible if we see this. At first it
            // may seem prudent to be fault-tolerant and assume we have a
            // complete generation, parsing and returning that first. However,
            // if the batches are mixed across generations then it's likely
            // we won't be able to parse this generation correctly at all.
            // Rather than return a cryptic error in that case, indicate the
            // problem as soon as we see it.
            return (default!, default!, fmt.Errorf("generations out of order"u8));
        }
        if ((~g).minTs == 0 || b.time < (~g).minTs) {
            g.Value.minTs = b.time;
        }
        {
            var errΔ1 = processBatch(g, b, ver); if (errΔ1 != default!) {
                return (default!, default!, errΔ1);
            }
        }
    }

    // Check some invariants.
    if ((~g).freq == 0D) {
        return (default!, default!, fmt.Errorf("no frequency event found"u8));
    }
    if (ver >= version.Go125 && !(~g).hasClockSnapshot) {
        return (default!, default!, fmt.Errorf("no clock snapshot event found"u8));
    }

    // N.B. Trust that the batch order is correct. We can't validate the batch order
    // by timestamp because the timestamps could just be plain wrong. The source of
    // truth is the order things appear in the trace and the partial order sequence
    // numbers on certain events. If it turns out the batch order is actually incorrect
    // we'll very likely fail to advance a partial order from the frontier.

    // Compactify stacks and strings for better lookup performance later.
    g.of(generation.Ꮡstacks).compactify();
    g.of(generation.Ꮡstrings).compactify();

    // Validate stacks.
    {
        var err = validateStackStrings(g.of(generation.Ꮡstacks), g.of(generation.Ꮡstrings), (~g).pcs); if (err != default!) {
            return (default!, default!, err);
        }
    }

    // Now that we have the frequency, fix up CPU samples.
    fixUpCPUSamples((~g).cpuSamples, (~g).freq);
    return (g, Ꮡspill, spillErr);
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static readonly @string internalErrorˢ3 = "internal error: unexpectedly processing EndOfGeneration; broken trace?"u8;

// processBatch adds the batch to the generation.
internal static error processBatch(ж<generation> Ꮡg, batch b, version.Version ver) {
    ref var g = ref Ꮡg.DerefOrNull();

    switch (ᐧ) {
    case {} when b.isStringsBatch(): {
        {
            var err = addStrings(Ꮡg.of(generation.Ꮡstrings), b); if (err != default!) {
                return err;
            }
        }
        break;
    }
    case {} when b.isStacksBatch(): {
        {
            var err = addStacks(Ꮡg.of(generation.Ꮡstacks), g.pcs, b); if (err != default!) {
                return err;
            }
        }
        break;
    }
    case {} when b.isCPUSamplesBatch(): {
        var (samples, err) = addCPUSamples(g.cpuSamples, b);
        if (err != default!) {
            return err;
        }
        g.cpuSamples = samples;
        break;
    }
    case {} when b.isSyncBatch(ver): {
        {
            var err = setSyncBatch(ref nonnil(ref g).sync, b, ver); if (err != default!) {
                return err;
            }
        }
        break;
    }
    case {} when b.exp != tracev2.NoExperiment: {
        if (g.expBatches == default!) {
            g.expBatches = new map<tracev2.Experiment, slice<ExperimentalBatch>>();
        }
        {
            var err = addExperimentalBatch(g.expBatches, b); if (err != default!) {
                return err;
            }
        }
        break;
    }
    case {} when b.isEndOfGeneration(): {
        return errors.New(internalErrorˢ3);
    }
    default: {
        {
            var (_, ok) = g.batches[b.m, ꟷ]; if (!ok) {
                g.batchMs = builtin.append(g.batchMs, b.m);
            }
        }
        g.batches[b.m] = builtin.append(g.batches[b.m], b);
        break;
    }}
    return default!;
}

// validateStackStrings makes sure all the string references in
// the stack table are present in the string table.
internal static error validateStackStrings(ж<dataTable<stackID, stack>> Ꮡstacks, ж<dataTable<stringID, @string>> Ꮡstrings, map<uint64, frame> frames) {
    ref var stacks = ref Ꮡstacks.DerefOrNull();

    ref var err = ref heap<error>(out var Ꮡerr);
    var framesʗ1 = frames;
    stacks.forEach((stackID id, stack stk) => {
        foreach (var (_, pc) in stk.pcs) {
            var (frame, ok) = framesʗ1[pc, ꟷ];
            if (!ok) {
                Ꮡerr.ValueSlot = fmt.Errorf("found unknown pc %x for stack %d"u8, pc, id);
                return false;
            }
            (_, ok) = Ꮡstrings.Value.get(frame.funcID);
            if (!ok) {
                Ꮡerr.ValueSlot = fmt.Errorf("found invalid func string ID %d for stack %d"u8, frame.funcID, id);
                return false;
            }
            (_, ok) = Ꮡstrings.Value.get(frame.fileID);
            if (!ok) {
                Ꮡerr.ValueSlot = fmt.Errorf("found invalid file string ID %d for stack %d"u8, frame.fileID, id);
                return false;
            }
        }
        return true;
    });
    return err;
}

// addStrings takes a batch whose first byte is an EvStrings event
// (indicating that the batch contains only strings) and adds each
// string contained therein to the provided strings map.
internal static error addStrings(ж<dataTable<stringID, @string>> ᏑstringTable, batch b) {
    ref var stringTable = ref ᏑstringTable.DerefOrNull();

    if (!b.isStringsBatch()) {
        return fmt.Errorf("internal error: addStrings called on non-string batch"u8);
    }
    var r = bytes.NewReader(b.data);
    var (hdr, err) = r.ReadByte(); // Consume the EvStrings byte.
    if (err != default! || ((tracev2.EventType)hdr) != tracev2.EvStrings) {
        return fmt.Errorf("missing strings batch header"u8);
    }

    ref var sb = ref heap(new strings.Builder(), out var Ꮡsb);
    while (r.Len() != 0) {
        // Read the header.
        var (ev, errΔ1) = r.ReadByte();
        if (errΔ1 != default!) {
            return errΔ1;
        }
        if (((tracev2.EventType)ev) != tracev2.EvString) {
            return fmt.Errorf("expected string event, got %d"u8, ev);
        }

        // Read the string's ID.
        (var id, errΔ1) = binary.ReadUvarint(new bytes_ReaderжByteReader(r));
        if (errΔ1 != default!) {
            return errΔ1;
        }

        // Read the string's length.
        (var len, errΔ1) = binary.ReadUvarint(new bytes_ReaderжByteReader(r));
        if (errΔ1 != default!) {
            return errΔ1;
        }
        if (len > tracev2.MaxEventTrailerDataSize) {
            return fmt.Errorf("invalid string size %d, maximum is %d"u8, len, (nint)(tracev2.MaxEventTrailerDataSize));
        }

        // Copy out the string.
        (var n, errΔ1) = io.CopyN(new strings_BuilderжWriter(Ꮡsb), new bytes_ReaderжReader(r), (int64)len);
        if (n != (int64)len) {
            return fmt.Errorf("failed to read full string: read %d but wanted %d"u8, n, len);
        }
        if (errΔ1 != default!) {
            return fmt.Errorf("copying string data: %w"u8, errΔ1);
        }

        // Add the string to the map.
        @string s = sb.String();
        sb.Reset();
        {
            var errΔ2 = stringTable.insert((stringID)id, s); if (errΔ2 != default!) {
                return errΔ2;
            }
        }
    }
    return default!;
}

// addStacks takes a batch whose first byte is an EvStacks event
// (indicating that the batch contains only stacks) and adds each
// string contained therein to the provided stacks map.
internal static error addStacks(ж<dataTable<stackID, stack>> ᏑstackTable, map<uint64, frame> pcs, batch b) {
    ref var stackTable = ref ᏑstackTable.DerefOrNull();

    if (!b.isStacksBatch()) {
        return fmt.Errorf("internal error: addStacks called on non-stacks batch"u8);
    }
    var r = bytes.NewReader(b.data);
    var (hdr, err) = r.ReadByte(); // Consume the EvStacks byte.
    if (err != default! || ((tracev2.EventType)hdr) != tracev2.EvStacks) {
        return fmt.Errorf("missing stacks batch header"u8);
    }

    while (r.Len() != 0) {
        // Read the header.
        var (ev, errΔ1) = r.ReadByte();
        if (errΔ1 != default!) {
            return errΔ1;
        }
        if (((tracev2.EventType)ev) != tracev2.EvStack) {
            return fmt.Errorf("expected stack event, got %d"u8, ev);
        }

        // Read the stack's ID.
        (var id, errΔ1) = binary.ReadUvarint(new bytes_ReaderжByteReader(r));
        if (errΔ1 != default!) {
            return errΔ1;
        }

        // Read how many frames are in each stack.
        (var nFrames, errΔ1) = binary.ReadUvarint(new bytes_ReaderжByteReader(r));
        if (errΔ1 != default!) {
            return errΔ1;
        }
        if (nFrames > tracev2.MaxFramesPerStack) {
            return fmt.Errorf("invalid stack size %d, maximum is %d"u8, nFrames, (nint)(tracev2.MaxFramesPerStack));
        }

        // Each frame consists of 4 fields: pc, funcID (string), fileID (string), line.
        var frames = new slice<uint64>(0, (nint)(nFrames));
        for (var i = (uint64)0; i < nFrames; i++) {
            // Read the frame data.
            var (pc, errΔ2) = binary.ReadUvarint(new bytes_ReaderжByteReader(r));
            if (errΔ2 != default!) {
                return fmt.Errorf("reading frame %d's PC for stack %d: %w"u8, i + 1, id, errΔ2);
            }
            (var funcID, errΔ2) = binary.ReadUvarint(new bytes_ReaderжByteReader(r));
            if (errΔ2 != default!) {
                return fmt.Errorf("reading frame %d's funcID for stack %d: %w"u8, i + 1, id, errΔ2);
            }
            (var fileID, errΔ2) = binary.ReadUvarint(new bytes_ReaderжByteReader(r));
            if (errΔ2 != default!) {
                return fmt.Errorf("reading frame %d's fileID for stack %d: %w"u8, i + 1, id, errΔ2);
            }
            (var line, errΔ2) = binary.ReadUvarint(new bytes_ReaderжByteReader(r));
            if (errΔ2 != default!) {
                return fmt.Errorf("reading frame %d's line for stack %d: %w"u8, i + 1, id, errΔ2);
            }
            frames = builtin.append(frames, pc);

            {
                var (_, ok) = pcs[pc, ꟷ]; if (!ok) {
                    pcs[pc] = new frame(
                        pc: pc,
                        funcID: ((stringID)funcID),
                        fileID: ((stringID)fileID),
                        line: line
                    );
                }
            }
        }

        // Add the stack to the map.
        {
            var errΔ3 = stackTable.insert((stackID)id, new stack(pcs: frames)); if (errΔ3 != default!) {
                return errΔ3;
            }
        }
    }
    return default!;
}

// addCPUSamples takes a batch whose first byte is an EvCPUSamples event
// (indicating that the batch contains only CPU samples) and adds each
// sample contained therein to the provided samples list.
internal static (slice<cpuSample>, error) addCPUSamples(slice<cpuSample> samples, batch b) {
    if (!b.isCPUSamplesBatch()) {
        return (default!, fmt.Errorf("internal error: addCPUSamples called on non-CPU-sample batch"u8));
    }
    var r = bytes.NewReader(b.data);
    var (hdr, err) = r.ReadByte(); // Consume the EvCPUSamples byte.
    if (err != default! || ((tracev2.EventType)hdr) != tracev2.EvCPUSamples) {
        return (default!, fmt.Errorf("missing CPU samples batch header"u8));
    }

    while (r.Len() != 0) {
        // Read the header.
        var (ev, errΔ1) = r.ReadByte();
        if (errΔ1 != default!) {
            return (default!, errΔ1);
        }
        if (((tracev2.EventType)ev) != tracev2.EvCPUSample) {
            return (default!, fmt.Errorf("expected CPU sample event, got %d"u8, ev));
        }

        // Read the sample's timestamp.
        (var ts, errΔ1) = binary.ReadUvarint(new bytes_ReaderжByteReader(r));
        if (errΔ1 != default!) {
            return (default!, errΔ1);
        }

        // Read the sample's M.
        (var m, errΔ1) = binary.ReadUvarint(new bytes_ReaderжByteReader(r));
        if (errΔ1 != default!) {
            return (default!, errΔ1);
        }
        var mid = (ThreadID)(int64)m;

        // Read the sample's P.
        (var p, errΔ1) = binary.ReadUvarint(new bytes_ReaderжByteReader(r));
        if (errΔ1 != default!) {
            return (default!, errΔ1);
        }
        var pid = (ProcID)(int64)p;

        // Read the sample's G.
        (var g, errΔ1) = binary.ReadUvarint(new bytes_ReaderжByteReader(r));
        if (errΔ1 != default!) {
            return (default!, errΔ1);
        }
        var goid = (GoID)(int64)g;
        if (g == 0) {
            goid = NoGoroutine;
        }

        // Read the sample's stack.
        (var s, errΔ1) = binary.ReadUvarint(new bytes_ReaderжByteReader(r));
        if (errΔ1 != default!) {
            return (default!, errΔ1);
        }

        // Add the sample to the slice.
        samples = builtin.append(samples, new cpuSample(
            schedCtx: new schedCtx(
                M: mid,
                P: pid,
                G: goid
            ),
            time: ((ΔTime)(int64)ts), // N.B. this is really a "timestamp," not a Time.
            stack: ((stackID)s)
        ));
    }
    return (samples, default!);
}

// sync holds the per-generation sync data.
partial struct sync {
    internal frequency freq;
    internal bool hasClockSnapshot;
    internal timestamp snapTime;
    internal uint64 snapMono;
    internal time.Time snapWall;
}

internal static error setSyncBatch(ref sync s, batch b, version.Version ver) {
    if (!b.isSyncBatch(ver)) {
        return fmt.Errorf("internal error: setSyncBatch called on non-sync batch"u8);
    }
    var r = bytes.NewReader(b.data);
    if (ver >= version.Go125) {
        var (hdr, err) = r.ReadByte(); // Consume the EvSync byte.
        if (err != default! || ((tracev2.EventType)hdr) != tracev2.EvSync) {
            return fmt.Errorf("missing sync batch header"u8);
        }
    }

    var lastTs = b.time;
    while (r.Len() != 0) {
        // Read the header
        var (ev, err) = r.ReadByte();
        if (err != default!) {
            return err;
        }
        var et = (tracev2.EventType)ev;
        switch (ᐧ) {
        case {} when et == tracev2.EvFrequency: {
            if (s.freq != 0D) {
                return fmt.Errorf("found multiple frequency events"u8);
            }
            // Read the frequency. It'll come out as timestamp units per second.
            var (f, errΔ2) = binary.ReadUvarint(new bytes_ReaderжByteReader(r));
            if (errΔ2 != default!) {
                return errΔ2;
            }
            // Convert to nanoseconds per timestamp unit.
            s.freq = (frequency)(1.0D / ((float64)f / 1e9D));
            break;
        }
        case {} when et == tracev2.EvClockSnapshot && ver >= version.Go125: {
            if (s.hasClockSnapshot) {
                return fmt.Errorf("found multiple clock snapshot events"u8);
            }
            s.hasClockSnapshot = true;
            // Read the EvClockSnapshot arguments.
            var (tdiff, errΔ3) = binary.ReadUvarint(new bytes_ReaderжByteReader(r));
            if (errΔ3 != default!) {
                return errΔ3;
            }
            lastTs += ((timestamp)tdiff);
            s.snapTime = lastTs;
            (var mono, errΔ3) = binary.ReadUvarint(new bytes_ReaderжByteReader(r));
            if (errΔ3 != default!) {
                return errΔ3;
            }
            s.snapMono = mono;
            (var sec, errΔ3) = binary.ReadUvarint(new bytes_ReaderжByteReader(r));
            if (errΔ3 != default!) {
                return errΔ3;
            }
            (var nsec, errΔ3) = binary.ReadUvarint(new bytes_ReaderжByteReader(r));
            if (errΔ3 != default!) {
                return errΔ3;
            }
            // TODO(felixge): In theory we could inject s.snapMono into the time
            // value below to make it comparable. But there is no API for this
            // in the time package right now.
            s.snapWall = time.Unix((int64)sec, (int64)nsec);
            break;
        }
        default: {
            return fmt.Errorf("expected frequency or clock snapshot event, got %d"u8, ev);
        }}
    }
    return default!;
}

// addExperimentalBatch takes an experimental batch and adds it to the list of experimental
// batches for the experiment its a part of.
internal static error addExperimentalBatch(map<tracev2.Experiment, slice<ExperimentalBatch>> expBatches, batch b) {
    if (b.exp == tracev2.NoExperiment) {
        return fmt.Errorf("internal error: addExperimentalBatch called on non-experimental batch"u8);
    }
    expBatches[b.exp] = builtin.append(expBatches[b.exp], new ExperimentalBatch(
        Thread: b.m,
        Data: b.data
    ));
    return default!;
}

internal static void fixUpCPUSamples(slice<cpuSample> samples, frequency freq) {
    // Fix up the CPU sample timestamps.
    foreach (var i in range(samples)) {
        var s = Ꮡ(samples, i);
        s.Value.time = freq.mul((timestamp)(uint64)(int64)(~s).time);
    }
    // Sort the CPU samples.
    slices.SortFunc(samples, (cpuSample a, cpuSample b) => cmp.Compare(a.time, b.time));
}

} // end trace_package
