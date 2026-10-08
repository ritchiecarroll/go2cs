// Copyright 2023 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.
namespace go.@internal;

using bufio = bufio_package;
using bytes = bytes_package;
using cmp = cmp_package;
using binary = encoding.binary_package;
using fmt = fmt_package;
using io = io_package;
using slices = slices_package;
using strings = strings_package;
using @event = go.@internal.trace.event_package;
using go122 = go.@internal.trace.@event.go122_package;
using encoding;
using go.@internal.trace;
using go.@internal.trace.@event;

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
    internal partial ref ж<evTable> evTable { get; }
}

// spilledBatch represents a batch that was read out for the next generation,
// while reading the previous one. It's passed on when parsing the next
// generation.
partial struct spilledBatch {
    internal uint64 gen;
    internal partial ref ж<batch> batch { get; }
}

// readGeneration buffers and decodes the structural elements of a trace generation
// out of r. spill is the first batch of the new generation (already buffered and
// parsed from reading the last generation). Returns the generation and the first
// batch read of the next generation, if any.
//
// If gen is non-nil, it is valid and must be processed before handling the returned
// error.
internal static (ж<generation>, ж<spilledBatch>, error) readGeneration(ж<bufio.Reader> Ꮡr, ж<spilledBatch> Ꮡspill) {
    ref var spill = ref Ꮡspill.DerefOrNull();

    var g = Ꮡ(new generation(
        evTable: Ꮡ(new evTable(
            pcs: new map<uint64, frame>()
        )),
        batches: new map<ThreadID, slice<batch>>()
    ));
    // Process the spilled batch.
    if (Ꮡspill != nil) {
        g.Value.gen = spill.gen;
        {
            var err = processBatch(g, spill.batch.Value); if (err != default!) {
                return (default!, default!, err);
            }
        }
        Ꮡspill = default!; spill = ref Ꮡspill.DerefOrNull();
    }
    // Read batches one at a time until we either hit EOF or
    // the next generation.
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
                // This is an error reading the first batch of the next generation.
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
            // TODO: advance this the same way the runtime does.
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
        {
            var errΔ1 = processBatch(g, b); if (errΔ1 != default!) {
                return (default!, default!, errΔ1);
            }
        }
    }
    // Check some invariants.
    if ((~g).freq == 0D) {
        return (default!, default!, fmt.Errorf("no frequency event found"u8));
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
    // Fix up the CPU sample timestamps, now that we have freq.
    foreach (var (i, _) in (~g).cpuSamples) {
        var s = Ꮡ((~g).cpuSamples, i);
        s.Value.time = (~g).freq.mul(((timestamp)(uint64)(int64)(~s).time));
    }
    // Sort the CPU samples.
    slices.SortFunc((~g).cpuSamples, (cpuSample a, cpuSample b) => cmp.Compare(a.time, b.time));
    return (g, Ꮡspill, spillErr);
}

// processBatch adds the batch to the generation.
internal static error processBatch(ж<generation> Ꮡg, batch b) {
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
    case {} when b.isFreqBatch(): {
        var (freq, err) = parseFreq(b);
        if (err != default!) {
            return err;
        }
        if (g.freq != 0D) {
            return fmt.Errorf("found multiple frequency events"u8);
        }
        g.freq = freq;
        break;
    }
    case {} when b.exp != @event.NoExperiment: {
        if (g.expData == default!) {
            g.expData = new map<@event.Experiment, ж<ExperimentalData>>();
        }
        {
            var err = addExperimentalData(g.expData, b); if (err != default!) {
                return err;
            }
        }
        break;
    }
    default: {
        {
            var (_, ok) = g.batches[b.m, ꟷ]; if (!ok) {
                g.batchMs = append(g.batchMs, b.m);
            }
        }
        g.batches[b.m] = append(g.batches[b.m], b);
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
    if (err != default! || ((@event.Type)hdr) != go122.EvStrings) {
        return fmt.Errorf("missing strings batch header"u8);
    }
    ref var sb = ref heap(new strings.Builder(), out var Ꮡsb);
    while (r.Len() != 0) {
        // Read the header.
        var (ev, errΔ1) = r.ReadByte();
        if (errΔ1 != default!) {
            return errΔ1;
        }
        if (((@event.Type)ev) != go122.EvString) {
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
        if (len > go122.MaxStringSize) {
            return fmt.Errorf("invalid string size %d, maximum is %d"u8, len, (nint)(go122.MaxStringSize));
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
            var errΔ2 = stringTable.insert(((stringID)id), s); if (errΔ2 != default!) {
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
    if (err != default! || ((@event.Type)hdr) != go122.EvStacks) {
        return fmt.Errorf("missing stacks batch header"u8);
    }
    while (r.Len() != 0) {
        // Read the header.
        var (ev, errΔ1) = r.ReadByte();
        if (errΔ1 != default!) {
            return errΔ1;
        }
        if (((@event.Type)ev) != go122.EvStack) {
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
        if (nFrames > go122.MaxFramesPerStack) {
            return fmt.Errorf("invalid stack size %d, maximum is %d"u8, nFrames, (nint)(go122.MaxFramesPerStack));
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
            frames = append(frames, pc);
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
            var errΔ3 = stackTable.insert(((stackID)id), new stack(pcs: frames)); if (errΔ3 != default!) {
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
    if (err != default! || ((@event.Type)hdr) != go122.EvCPUSamples) {
        return (default!, fmt.Errorf("missing CPU samples batch header"u8));
    }
    while (r.Len() != 0) {
        // Read the header.
        var (ev, errΔ1) = r.ReadByte();
        if (errΔ1 != default!) {
            return (default!, errΔ1);
        }
        if (((@event.Type)ev) != go122.EvCPUSample) {
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
        var mid = ((ThreadID)(int64)m);
        // Read the sample's P.
        (var p, errΔ1) = binary.ReadUvarint(new bytes_ReaderжByteReader(r));
        if (errΔ1 != default!) {
            return (default!, errΔ1);
        }
        var pid = ((ProcID)(int64)p);
        // Read the sample's G.
        (var g, errΔ1) = binary.ReadUvarint(new bytes_ReaderжByteReader(r));
        if (errΔ1 != default!) {
            return (default!, errΔ1);
        }
        var goid = ((GoID)(int64)g);
        if (g == 0) {
            goid = NoGoroutine;
        }
        // Read the sample's stack.
        (var s, errΔ1) = binary.ReadUvarint(new bytes_ReaderжByteReader(r));
        if (errΔ1 != default!) {
            return (default!, errΔ1);
        }
        // Add the sample to the slice.
        samples = append(samples, new cpuSample(
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

// parseFreq parses out a lone EvFrequency from a batch.
internal static (frequency, error) parseFreq(batch b) {
    if (!b.isFreqBatch()) {
        return (0D, fmt.Errorf("internal error: parseFreq called on non-frequency batch"u8));
    }
    var r = bytes.NewReader(b.data);
    r.ReadByte(); // Consume the EvFrequency byte.
    // Read the frequency. It'll come out as timestamp units per second.
    var (f, err) = binary.ReadUvarint(new bytes_ReaderжByteReader(r));
    if (err != default!) {
        return (0D, err);
    }
    // Convert to nanoseconds per timestamp unit.
    return (((frequency)(1.0D / ((float64)f / 1e9D))), default!);
}

// addExperimentalData takes an experimental batch and adds it to the ExperimentalData
// for the experiment its a part of.
internal static error addExperimentalData(map<@event.Experiment, ж<ExperimentalData>> expData, batch b) {
    if (b.exp == @event.NoExperiment) {
        return fmt.Errorf("internal error: addExperimentalData called on non-experimental batch"u8);
    }
    var (ed, ok) = expData[b.exp, ꟷ];
    if (!ok) {
        ed = @new<ExperimentalData>();
        expData[b.exp] = ed;
    }
    ed.Value.Batches = append((~ed).Batches, new ExperimentalBatch(
        Thread: b.m,
        Data: b.data
    ));
    return default!;
}

} // end trace_package
