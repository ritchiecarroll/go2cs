// Copyright 2021 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.
namespace go.@internal;

partial class fuzz_package {

// queue holds a growable sequence of inputs for fuzzing and minimization.
//
// For now, this is a simple ring buffer
// (https://en.wikipedia.org/wiki/Circular_buffer).
//
// TODO(golang.org/issue/46224): use a prioritization algorithm based on input
// size, previous duration, coverage, and any other metrics that seem useful.
[GoType] partial struct queue {
    // elems holds a ring buffer.
    // The queue is empty when begin = end.
    // The queue is full (until grow is called) when end = begin + N - 1 (mod N)
    // where N = cap(elems).
    internal slice<any> elems;
    internal nint head, len;
}

[GoRecv] internal static nint cap(this ref queue q) {
    return len(q.elems);
}

[GoRecv] internal static void grow(this ref queue q) {
    nint oldCap = q.cap();
    nint newCap = oldCap * 2;
    if (newCap == 0) {
        newCap = 8;
    }
    var newElems = new slice<any>(newCap);
    nint oldLen = q.len;
    for (nint i = 0; i < oldLen; i++) {
        newElems[i] = q.elems[rem((q.head + i), oldCap)];
    }
    q.elems = newElems;
    q.head = 0;
}

[GoRecv] internal static void enqueue(this ref queue q, any e) {
    if (q.len + 1 > q.cap()) {
        q.grow();
    }
    nint i = rem((q.head + q.len), q.cap());
    q.elems[i] = e;
    q.len++;
}

[GoRecv] internal static (any, bool) dequeue(this ref queue q) {
    if (q.len == 0) {
        return (default!, false);
    }
    var e = q.elems[q.head];
    q.elems[q.head] = default!;
    q.head = rem((q.head + 1), q.cap());
    q.len--;
    return (e, true);
}

[GoRecv] internal static (any, bool) peek(this ref queue q) {
    if (q.len == 0) {
        return (default!, false);
    }
    return (q.elems[q.head], true);
}

[GoRecv] internal static void clear(this ref queue q) {
    q = new queue(nil);
}

} // end fuzz_package
