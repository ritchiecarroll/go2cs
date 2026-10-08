// Copyright 2023 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.
namespace go.@internal;

using bytes = bytes_package;
using binary = encoding.binary_package;
using fmt = fmt_package;
using io = io_package;
using @event = go.@internal.trace.event_package;
using go122 = go.@internal.trace.@event.go122_package;
using encoding;
using go.@internal.trace;
using go.@internal.trace.@event;

partial class trace_package {

partial struct timestamp /*num:uint64*/;

// batch represents a batch of trace events.
// It is unparsed except for its header.
partial struct batch {
    internal ThreadID m;
    internal timestamp time;
    internal slice<byte> data;
    internal @event.Experiment exp;
}

internal static bool isStringsBatch(this ref batch b) {
    return b.exp == @event.NoExperiment && len(b.data) > 0 && ((@event.Type)b.data[0]) == go122.EvStrings;
}

internal static bool isStacksBatch(this ref batch b) {
    return b.exp == @event.NoExperiment && len(b.data) > 0 && ((@event.Type)b.data[0]) == go122.EvStacks;
}

internal static bool isCPUSamplesBatch(this ref batch b) {
    return b.exp == @event.NoExperiment && len(b.data) > 0 && ((@event.Type)b.data[0]) == go122.EvCPUSamples;
}

internal static bool isFreqBatch(this ref batch b) {
    return b.exp == @event.NoExperiment && len(b.data) > 0 && ((@event.Type)b.data[0]) == go122.EvFrequency;
}

internal partial interface readBatch_r /*dyn*/ :
    io.Reader,
    io.ByteReader
{
}

// readBatch reads the next full batch from r.
internal static (batch, uint64, error) readBatch(readBatch_r r) {
    // Read batch header byte.
    var (b, err) = r.ReadByte();
    if (err != default!) {
        return (new batch(nil), 0, err);
    }
    {
        var typ = ((@event.Type)b); if (typ != go122.EvEventBatch && typ != go122.EvExperimentalBatch) {
            return (new batch(nil), 0, fmt.Errorf("expected batch event, got %s"u8, go122.EventString(typ)));
        }
    }
    // Read the experiment of we have one.
    @event.Experiment exp = @event.NoExperiment;
    if (((@event.Type)b) == go122.EvExperimentalBatch) {
        var (e, errΔ1) = r.ReadByte();
        if (errΔ1 != default!) {
            return (new batch(nil), 0, errΔ1);
        }
        exp = ((@event.Experiment)(nuint)e);
    }
    // Read the batch header: gen (generation), thread (M) ID, base timestamp
    // for the batch.
    (var gen, err) = binary.ReadUvarint(r);
    if (err != default!) {
        return (new batch(nil), gen, fmt.Errorf("error reading batch gen: %w"u8, err));
    }
    (var m, err) = binary.ReadUvarint(r);
    if (err != default!) {
        return (new batch(nil), gen, fmt.Errorf("error reading batch M ID: %w"u8, err));
    }
    (var ts, err) = binary.ReadUvarint(r);
    if (err != default!) {
        return (new batch(nil), gen, fmt.Errorf("error reading batch timestamp: %w"u8, err));
    }
    // Read in the size of the batch to follow.
    (var size, err) = binary.ReadUvarint(r);
    if (err != default!) {
        return (new batch(nil), gen, fmt.Errorf("error reading batch size: %w"u8, err));
    }
    if (size > go122.MaxBatchSize) {
        return (new batch(nil), gen, fmt.Errorf("invalid batch size %d, maximum is %d"u8, size, (nint)(go122.MaxBatchSize)));
    }
    // Copy out the batch for later processing.
    ref var data = ref heap(new bytes.Buffer(), out var Ꮡdata);
    data.Grow((nint)size);
    (var n, err) = io.CopyN(new bytes_BufferжWriter(Ꮡdata), r, (int64)size);
    if (n != (int64)size) {
        return (new batch(nil), gen, fmt.Errorf("failed to read full batch: read %d but wanted %d"u8, n, size));
    }
    if (err != default!) {
        return (new batch(nil), gen, fmt.Errorf("copying batch data: %w"u8, err));
    }
    // Return the batch.
    return (new batch(
        m: ((ThreadID)(int64)m),
        time: ((timestamp)ts),
        data: data.Bytes(),
        exp: exp
    ), gen, default!);
}

} // end trace_package
