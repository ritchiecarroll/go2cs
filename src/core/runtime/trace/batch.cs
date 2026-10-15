// Copyright 2025 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.
namespace go.runtime;

using fmt = fmt_package;
using tracev2 = @internal.trace.tracev2_package;
using @internal.trace;

partial class trace_package {

// timestamp is an unprocessed timestamp.
partial struct timestamp /*num:uint64*/;

partial struct batch {
    internal timestamp time;
    internal uint64 gen;
    internal slice<byte> data;
}

// readBatch copies b and parses the trace batch header inside.
// Returns the batch, bytes read, and an error.
internal static (batch, uint64, error) readBatch(slice<byte> b) {
    if (len(b) == 0) {
        return (new batch(nil), 0, fmt.Errorf("batch is empty"u8));
    }
    var data = new slice<byte>(len(b));
    copy(data, b);

    // Read batch header byte.
    {
        var typ = ((tracev2.EventType)b[0]); if (typ == tracev2.EvEndOfGeneration) {
            if (len(b) != 1) {
                return (new batch(nil), 1, fmt.Errorf("unexpected end of generation in batch of size >1"u8));
            }
            return (new batch(data: data), 1, default!);
        }
    }
    {
        var typ = ((tracev2.EventType)b[0]); if (typ != tracev2.EvEventBatch && typ != tracev2.EvExperimentalBatch) {
            return (new batch(nil), 1, fmt.Errorf("expected batch event, got event %d"u8, typ));
        }
    }
    nint total = 1;
    b = b[1..];

    // Read the generation
    var (gen, n, err) = readUvarint(b);
    if (err != default!) {
        return (new batch(nil), (uint64)(total + n), fmt.Errorf("error reading batch gen: %w"u8, err));
    }
    total += n;
    b = b.slice(n);

    // Read the M (discard it).
    (_, n, err) = readUvarint(b);
    if (err != default!) {
        return (new batch(nil), (uint64)(total + n), fmt.Errorf("error reading batch M ID: %w"u8, err));
    }
    total += n;
    b = b.slice(n);

    // Read the timestamp.
    (var ts, n, err) = readUvarint(b);
    if (err != default!) {
        return (new batch(nil), (uint64)(total + n), fmt.Errorf("error reading batch timestamp: %w"u8, err));
    }
    total += n;
    b = b.slice(n);

    // Read the size of the batch to follow.
    (var size, n, err) = readUvarint(b);
    if (err != default!) {
        return (new batch(nil), (uint64)(total + n), fmt.Errorf("error reading batch size: %w"u8, err));
    }
    if (size > tracev2.MaxBatchSize) {
        return (new batch(nil), (uint64)(total + n), fmt.Errorf("invalid batch size %d, maximum is %d"u8, size, (nint)(tracev2.MaxBatchSize)));
    }
    total += n;
    total += (nint)size;
    if (total != len(data)) {
        return (new batch(nil), (uint64)total, fmt.Errorf("expected complete batch"u8));
    }
    data = data.slice(0, total);

    // Return the batch.
    return (new batch(
        gen: gen,
        time: ((timestamp)ts),
        data: data
    ), (uint64)total, default!);
}

} // end trace_package
