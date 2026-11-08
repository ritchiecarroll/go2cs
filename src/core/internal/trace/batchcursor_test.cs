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

public static void TestHeap(ж<testing.T> t) {
    var removedᴷ = new stackAppend<ж<global::go.@internal.trace_package.batchCursor>>();

    slice<ж<global::go.@internal.trace_package.batchCursor>> heap = default;

    // Insert a bunch of values into the heap.
    checkHeap(t, heap);
    heap = heapInsert(heap, makeBatchCursor(5));
    checkHeap(t, heap);
    for (var i = (int64)(-20); i < 20; i++) {
        heap = heapInsert(heap, makeBatchCursor(i));
        checkHeap(t, heap);
    }

    // Update an element in the middle to be the new minimum.
    foreach (var i in range(len(heap))) {
        if (heap[i].ev.time == 5) {
            heap[i].ev.time = -21;
            heapUpdate(heap, i);
            break;
        }
    }
    checkHeap(t, heap);
    if (heap[0].ev.time != -21) {
        t.Fatalf("heap update failed, expected %d as heap min: %s", (nint)(-21), heapDebugString(heap));
    }

    // Update the minimum element to be smaller. There should be no change.
    heap[0].ev.time = -22;
    heapUpdate(heap, 0);
    checkHeap(t, heap);
    if (heap[0].ev.time != -22) {
        t.Fatalf("heap update failed, expected %d as heap min: %s", (nint)(-22), heapDebugString(heap));
    }

    // Update the last element to be larger. There should be no change.
    heap[len(heap) - 1].ev.time = 21;
    heapUpdate(heap, len(heap) - 1);
    checkHeap(t, heap);
    if (heap[len(heap) - 1].ev.time != 21) {
        t.Fatalf("heap update failed, expected %d as heap min: %s", (nint)21, heapDebugString(heap));
    }

    // Update the last element to be smaller.
    heap[len(heap) - 1].ev.time = 7;
    heapUpdate(heap, len(heap) - 1);
    checkHeap(t, heap);
    if (heap[len(heap) - 1].ev.time == 21) {
        t.Fatalf("heap update failed, unexpected %d as heap min: %s", (nint)21, heapDebugString(heap));
    }

    // Remove an element in the middle.
    foreach (var i in range(len(heap))) {
        if (heap[i].ev.time == 5) {
            heap = heapRemove(heap, i);
            break;
        }
    }
    checkHeap(t, heap);
    foreach (var i in range(len(heap))) {
        if (heap[i].ev.time == 5) {
            t.Fatalf("failed to remove heap elem with time %d: %s", (nint)5, heapDebugString(heap));
        }
    }

    // Remove tail.
    heap = heapRemove(heap, len(heap) - 1);
    checkHeap(t, heap);

    // Remove from the head, and make sure the result is sorted.
    nint l = len(heap);
    slice<ж<global::go.@internal.trace_package.batchCursor>> removed = default;
    for (nint i = 0; i < l; i++) {
        removed = builtin.append(removedᴷ, removed, heap[0]);
        heap = heapRemove(heap, 0);
        checkHeap(t, heap);
    }
    if (!slices.IsSortedFunc<slice<ж<global::go.@internal.trace_package.batchCursor>>, ж<global::go.@internal.trace_package.batchCursor>>(removed, ((Func<ж<global::go.@internal.trace_package.batchCursor>, ж<global::go.@internal.trace_package.batchCursor>, nint>)(global::go.@internal.trace_package.compare)))) {
        t.Fatalf("heap elements not removed in sorted order, got: %s", heapDebugString(removed));
    }
}

internal static ж<global::go.@internal.trace_package.batchCursor> makeBatchCursor(int64 v) {
    return Ꮡ(new batchCursor(ev: new baseEvent(time: ((global::go.@internal.trace_package.ΔTime)v))));
}

internal static @string heapDebugString(slice<ж<global::go.@internal.trace_package.batchCursor>> heap) {
    ref var sb = ref builtin.heap(new strings.Builder(), out var Ꮡsb);
    fmt.Fprintf(new trace_test_package.strings_BuilderжWriter(Ꮡsb), "["u8);
    foreach (var i in range(len(heap))) {
        if (i != 0) {
            fmt.Fprintf(new trace_test_package.strings_BuilderжWriter(Ꮡsb), ", "u8);
        }
        fmt.Fprintf(new trace_test_package.strings_BuilderжWriter(Ꮡsb), "%d"u8, heap[i].ev.time);
    }
    fmt.Fprintf(new trace_test_package.strings_BuilderжWriter(Ꮡsb), "]"u8);
    return sb.String();
}

internal static void checkHeap(ж<testing.T> t, slice<ж<global::go.@internal.trace_package.batchCursor>> heap) {
    t.Helper();

    foreach (var i in range(len(heap))) {
        if (i == 0) {
            continue;
        }
        if (heap[(i - 1) / 2].compare(heap[i]) > 0) {
            t.Errorf("heap invariant not maintained between index %d and parent %d: %s", i, i / 2, heapDebugString(heap));
        }
    }
    if (t.Failed()) {
        t.FailNow();
    }
}

} // end trace_internal_test_package
