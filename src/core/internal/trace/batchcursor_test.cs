// Copyright 2023 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.
namespace go.@internal;

using fmt = fmt_package;
using strings = strings_package;
using testing = testing_package;
using slices = slices_package;
using io = io_package;
using static go.@internal.trace_package;

partial class trace_internal_test_package {

public static void TestHeap(ж<testing.T> Ꮡt) {
    var removedᴷ = new stackAppend<ж<global::go.@internal.trace_package.batchCursor>>();

    slice<ж<global::go.@internal.trace_package.batchCursor>> heap = default!;

    // Insert a bunch of values into the heap.
    checkHeap(Ꮡt, heap);
    heap = heapInsert(heap, makeBatchCursor(5));
    checkHeap(Ꮡt, heap);
    for (var i = (int64)(-20); i < 20; i++) {
        heap = heapInsert(heap, makeBatchCursor(i));
        checkHeap(Ꮡt, heap);
    }

    // Update an element in the middle to be the new minimum.
    foreach (var i in range(heap)) {
        if ((~heap[i]).ev.time == 5) {
            heap[i].Value.ev.time = -21;
            heapUpdate(heap, i);
            break;
        }
    }
    checkHeap(Ꮡt, heap);
    if ((~heap[0]).ev.time != -21) {
        Ꮡt.Fatalf("heap update failed, expected %d as heap min: %s"u8, (nint)(-21), heapDebugString(heap));
    }

    // Update the minimum element to be smaller. There should be no change.
    heap[0].Value.ev.time = -22;
    heapUpdate(heap, 0);
    checkHeap(Ꮡt, heap);
    if ((~heap[0]).ev.time != -22) {
        Ꮡt.Fatalf("heap update failed, expected %d as heap min: %s"u8, (nint)(-22), heapDebugString(heap));
    }

    // Update the last element to be larger. There should be no change.
    heap[len(heap) - 1].Value.ev.time = 21;
    heapUpdate(heap, len(heap) - 1);
    checkHeap(Ꮡt, heap);
    if ((~heap[len(heap) - 1]).ev.time != 21) {
        Ꮡt.Fatalf("heap update failed, expected %d as heap min: %s"u8, (nint)21, heapDebugString(heap));
    }

    // Update the last element to be smaller.
    heap[len(heap) - 1].Value.ev.time = 7;
    heapUpdate(heap, len(heap) - 1);
    checkHeap(Ꮡt, heap);
    if ((~heap[len(heap) - 1]).ev.time == 21) {
        Ꮡt.Fatalf("heap update failed, unexpected %d as heap min: %s"u8, (nint)21, heapDebugString(heap));
    }

    // Remove an element in the middle.
    foreach (var i in range(heap)) {
        if ((~heap[i]).ev.time == 5) {
            heap = heapRemove(heap, i);
            break;
        }
    }
    checkHeap(Ꮡt, heap);
    foreach (var i in range(heap)) {
        if ((~heap[i]).ev.time == 5) {
            Ꮡt.Fatalf("failed to remove heap elem with time %d: %s"u8, (nint)5, heapDebugString(heap));
        }
    }

    // Remove tail.
    heap = heapRemove(heap, len(heap) - 1);
    checkHeap(Ꮡt, heap);

    // Remove from the head, and make sure the result is sorted.
    nint l = len(heap);
    slice<ж<global::go.@internal.trace_package.batchCursor>> removed = default!;
    for (nint i = 0; i < l; i++) {
        removed = builtin.append(removedᴷ, removed, heap[0]);
        heap = heapRemove(heap, 0);
        checkHeap(Ꮡt, heap);
    }
    if (!slices.IsSortedFunc<slice<ж<global::go.@internal.trace_package.batchCursor>>, ж<global::go.@internal.trace_package.batchCursor>>(removed, ((Func<ж<global::go.@internal.trace_package.batchCursor>, ж<global::go.@internal.trace_package.batchCursor>, nint>)(global::go.@internal.trace_package.compare)))) {
        Ꮡt.Fatalf("heap elements not removed in sorted order, got: %s"u8, heapDebugString(removed));
    }
}

internal static ж<global::go.@internal.trace_package.batchCursor> makeBatchCursor(int64 v) {
    return Ꮡ(new batchCursor(ev: new baseEvent(time: ((global::go.@internal.trace_package.ΔTime)v))));
}

internal static @string heapDebugString(slice<ж<global::go.@internal.trace_package.batchCursor>> heap) {
    ref var sb = ref builtin.heap(new strings.Builder(), out var Ꮡsb);
    fmt.Fprintf(new trace_test_package.strings_BuilderжWriter(Ꮡsb), "["u8);
    foreach (var i in range(heap)) {
        if (i != 0) {
            fmt.Fprintf(new trace_test_package.strings_BuilderжWriter(Ꮡsb), ", "u8);
        }
        fmt.Fprintf(new trace_test_package.strings_BuilderжWriter(Ꮡsb), "%d"u8, (~heap[i]).ev.time);
    }
    fmt.Fprintf(new trace_test_package.strings_BuilderжWriter(Ꮡsb), "]"u8);
    return sb.String();
}

internal static void checkHeap(ж<testing.T> Ꮡt, slice<ж<global::go.@internal.trace_package.batchCursor>> heap) {
    Ꮡt.Helper();

    foreach (var i in range(heap)) {
        if (i == 0) {
            continue;
        }
        if (heap[(i - 1) / 2].compare(heap[i]) > 0) {
            Ꮡt.Errorf("heap invariant not maintained between index %d and parent %d: %s"u8, i, i / 2, heapDebugString(heap));
        }
    }
    if (Ꮡt.Failed()) {
        Ꮡt.FailNow();
    }
}

} // end trace_internal_test_package
