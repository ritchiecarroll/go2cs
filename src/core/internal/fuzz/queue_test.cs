// Copyright 2021 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.
namespace go.@internal;

using testing = testing_package;
using static global::go.@internal.fuzz_package;

partial class fuzz_internal_test_package {

public static void TestQueue(ж<testing.T> t) {
    var sᴷ = new stackAppend<nint>();

    // Zero valued queue should have 0 length and capacity.
    global::go.@internal.fuzz_package.queue q = default;
    {
        nint n = q.len; if (n != 0) {
            t.Fatalf("empty queue has len %d; want 0", n);
        }
    }
    {
        nint n = q.cap(); if (n != 0) {
            t.Fatalf("empty queue has cap %d; want 0", n);
        }
    }

    // As we add elements, len should grow.
    nint N = 32;
    for (nint i = 0; i < N; i++) {
        q.enqueue(i);
        nint n = q.len; if (n != i + 1) {
            t.Fatalf("after adding %d elements, queue has len %d", i, n);
        }
        var (v, ok) = q.peek(); if (!ok) {
            t.Fatalf("couldn't peek after adding %d elements", i);
        } else if (v._<nint>() != 0) {
            t.Fatalf("after adding %d elements, peek is %d; want 0", i, v);
        }
    }

    // As we remove and add elements, len should shrink and grow.
    // We should also remove elements in the same order they were added.
    nint want = 0;
    foreach (var (_, r) in range(slice<nint>([1, 2, 3, 5, 8, 13, 21]))) {
        var s = new slice<nint>(0, r);
        for (nint i = 0; i < r; i++) {
            var (got, ok) = q.dequeue(); if (!ok) {
                t.Fatalf("after removing %d of %d elements, could not dequeue", i + 1, r);
            } else if (!AreEqual(got, want)) {
                t.Fatalf("after removing %d of %d elements, got %d; want %d", i + 1, r, got, want);
            } else {
                s = append(sᴷ, s, got._<nint>());
            }
            want = rem((want + 1), N);
            nint n = q.len; if (n != N - i - 1) {
                t.Fatalf("after removing %d of %d elements, len is %d; want %d", i + 1, r, n, N - i - 1);
            }
        }
        foreach (var (i, v) in range(s)) {
            q.enqueue(v);
            nint n = q.len; if (n != N - r + i + 1) {
                t.Fatalf("after adding back %d of %d elements, len is %d; want %d", i + 1, r, n, n - r + i + 1);
            }
        }
    }
}

} // end fuzz_internal_test_package
