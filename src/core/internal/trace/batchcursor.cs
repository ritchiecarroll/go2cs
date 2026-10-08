// Copyright 2023 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.
namespace go.@internal;

using cmp = cmp_package;
using binary = encoding.binary_package;
using fmt = fmt_package;
using @event = go.@internal.trace.event_package;
using go122 = go.@internal.trace.@event.go122_package;
using encoding;
using go.@internal.trace;
using go.@internal.trace.@event;

partial class trace_package {

partial struct batchCursor {
    internal ThreadID m;
    internal ΔTime lastTs;
    internal nint idx;      // next index into []batch
    internal nint dataOff;      // next index into batch.data
    internal baseEvent ev; // last read event
}

internal static (bool ok, error err) nextEvent(this ж<batchCursor> Ꮡb, slice<batch> batches, frequency freq) {
    error err = default!;

    ref var b = ref Ꮡb.DerefOrNull();
    // Batches should generally always have at least one event,
    // but let's be defensive about that and accept empty batches.
    while (b.idx < len(batches) && len(batches[b.idx].data) == b.dataOff) {
        b.idx++;
        b.dataOff = 0;
        b.lastTs = 0;
    }
    // Have we reached the end of the batches?
    if (b.idx == len(batches)) {
        return (false, default!);
    }
    // Initialize lastTs if it hasn't been yet.
    if (b.lastTs == 0) {
        b.lastTs = freq.mul(batches[b.idx].time);
    }
    // Read an event out.
    (var n, var tsdiff, err) = readTimedBaseEvent(batches[b.idx].data.slice(b.dataOff), ref nonnil(ref b).ev);
    if (err != default!) {
        return (false, err);
    }
    // Complete the timestamp from the cursor's last timestamp.
    b.ev.time = freq.mul(tsdiff) + b.lastTs;
    // Move the cursor's timestamp forward.
    b.lastTs = b.ev.time;
    // Move the cursor forward.
    b.dataOff += n;
    return (true, default!);
}

internal static nint compare(this ref batchCursor b, ж<batchCursor> Ꮡa) {
    ref var a = ref Ꮡa.DerefOrNull();

    return cmp.Compare(b.ev.time, a.ev.time);
}

// readTimedBaseEvent reads out the raw event data from b
// into e. It does not try to interpret the arguments
// but it does validate that the event is a regular
// event with a timestamp (vs. a structural event).
//
// It requires that the event its reading be timed, which must
// be the case for every event in a plain EventBatch.
internal static (nint, timestamp, error) readTimedBaseEvent(slice<byte> b, ref baseEvent e) {
    // Get the event type.
    var typ = ((@event.Type)b[0]);
    var specs = go122.Specs();
    if ((nint)(uint8)typ >= len(specs)) {
        return (0, 0, fmt.Errorf("found invalid event type: %v"u8, typ));
    }
    e.typ = typ;
    // Get spec.
    var spec = Ꮡ(specs, typ);
    if (len((~spec).Args) == 0 || !(~spec).IsTimedEvent) {
        return (0, 0, fmt.Errorf("found event without a timestamp: type=%v"u8, typ));
    }
    nint n = 1;
    // Read timestamp diff.
    var (ts, nb) = binary.Uvarint(b.slice(n));
    if (nb <= 0) {
        return (0, 0, fmt.Errorf("found invalid uvarint for timestamp"u8));
    }
    n += nb;
    // Read the rest of the arguments.
    for (nint i = 0; i < len((~spec).Args) - 1; i++) {
        var (arg, nbΔ1) = binary.Uvarint(b.slice(n));
        if (nbΔ1 <= 0) {
            return (0, 0, fmt.Errorf("found invalid uvarint"u8));
        }
        e.args[i] = arg;
        n += nbΔ1;
    }
    return (n, ((timestamp)ts), default!);
}

internal static slice<ж<batchCursor>> heapInsert(slice<ж<batchCursor>> heap, ж<batchCursor> Ꮡbc) {
    // Add the cursor to the end of the heap.
    heap = append(heap, Ꮡbc);
    // Sift the new entry up to the right place.
    heapSiftUp(heap, len(heap) - 1);
    return heap;
}

internal static void heapUpdate(slice<ж<batchCursor>> heap, nint i) {
    // Try to sift up.
    if (heapSiftUp(heap, i) != i) {
        return;
    }
    // Try to sift down, if sifting up failed.
    heapSiftDown(heap, i);
}

internal static slice<ж<batchCursor>> heapRemove(slice<ж<batchCursor>> heap, nint i) {
    // Sift index i up to the root, ignoring actual values.
    while (i > 0) {
        (heap[(i - 1) / 2], heap[i]) = (heap[i], heap[(i - 1) / 2]);
        i = (i - 1) / 2;
    }
    // Swap the root with the last element, then remove it.
    (heap[0], heap[len(heap) - 1]) = (heap[len(heap) - 1], heap[0]);
    heap = heap.slice(0, len(heap) - 1);
    // Sift the root down.
    heapSiftDown(heap, 0);
    return heap;
}

internal static nint heapSiftUp(slice<ж<batchCursor>> heap, nint i) {
    while (i > 0 && (~heap[(i - 1) / 2]).ev.time > (~heap[i]).ev.time) {
        (heap[(i - 1) / 2], heap[i]) = (heap[i], heap[(i - 1) / 2]);
        i = (i - 1) / 2;
    }
    return i;
}

internal static nint heapSiftDown(slice<ж<batchCursor>> heap, nint i) {
    while (ᐧ) {
        nint m = min3(heap, i, 2 * i + 1, 2 * i + 2);
        if (m == i) {
            // Heap invariant already applies.
            break;
        }
        (heap[i], heap[m]) = (heap[m], heap[i]);
        i = m;
    }
    return i;
}

internal static nint min3(slice<ж<batchCursor>> b, nint i0, nint i1, nint i2) {
    nint minIdx = i0;
    var minT = maxTime;
    if (i0 < len(b)) {
        minT = b[i0].Value.ev.time;
    }
    if (i1 < len(b)) {
        {
            var t = b[i1].Value.ev.time; if (t < minT) {
                minT = t;
                minIdx = i1;
            }
        }
    }
    if (i2 < len(b)) {
        {
            var t = b[i2].Value.ev.time; if (t < minT) {
                minT = t;
                minIdx = i2;
            }
        }
    }
    return minIdx;
}

} // end trace_package
