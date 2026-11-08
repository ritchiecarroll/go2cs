// Copyright 2009 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.
namespace go.container;

using rand = math.rand_package;
using testing = testing_package;
using math;
using static go.container.heap_package;

partial class heap_internal_test_package {

internal partial struct myHeap /*[]nint*/;

internal static bool Less(this ref myHeap h, nint i, nint j) {
    return (h)[i] < (h)[j];
}

internal static void Swap(this ref myHeap h, nint i, nint j) {
    ((h)[i], (h)[j]) = ((h)[j], (h)[i]);
}

internal static nint Len(this ref myHeap h) {
    return len(h);
}

internal static any /*v*/ Pop(this ref myHeap h) {
    any v = default;

    (h, v) = ((h).slice(0, h.Len() - 1), (h)[h.Len() - 1]);
    return v;
}

internal static void Push(this ref myHeap h, any v) {
    h = append(h, v._<nint>());
}

internal static void verify(this myHeap h, ж<testing.T> t, nint i) {
    t.Helper();
    nint n = h.Len();
    nint j1 = 2 * i + 1;
    nint j2 = 2 * i + 2;
    if (j1 < n) {
        if (h.Less(j1, i)) {
            t.Errorf("heap invariant invalidated [%d] = %d > [%d] = %d", i, h[i], j1, h[j1]);
            return;
        }
        h.verify(t, j1);
    }
    if (j2 < n) {
        if (h.Less(j2, i)) {
            t.Errorf("heap invariant invalidated [%d] = %d > [%d] = %d", i, h[i], j1, h[j2]);
            return;
        }
        h.verify(t, j2);
    }
}

public static void TestInit0(ж<testing.T> t) {
    var h = @new<myHeap>();
    for (nint i = 20; i > 0; i--) {
        h.Push((nint)0); // all elements are the same
    }
    Init(new heap_internal_test_package.myHeapжInterface(h));
    h.verify(t, 0);

    for (nint i = 1; h.Len() > 0; i++) {
        nint x = go.container.heap_package.Pop(new heap_internal_test_package.myHeapжInterface(h))._<nint>();
        h.verify(t, 0);
        if (x != 0) {
            t.Errorf("%d.th pop got %d; want %d", i, x, (nint)0);
        }
    }
}

public static void TestInit1(ж<testing.T> t) {
    var h = @new<myHeap>();
    for (nint i = 20; i > 0; i--) {
        h.Push(i); // all elements are different
    }
    Init(new heap_internal_test_package.myHeapжInterface(h));
    h.verify(t, 0);

    for (nint i = 1; h.Len() > 0; i++) {
        nint x = go.container.heap_package.Pop(new heap_internal_test_package.myHeapжInterface(h))._<nint>();
        h.verify(t, 0);
        if (x != i) {
            t.Errorf("%d.th pop got %d; want %d", i, x, i);
        }
    }
}

public static void Test(ж<testing.T> t) {
    var h = @new<myHeap>();
    h.verify(t, 0);

    for (nint i = 20; i > 10; i--) {
        h.Push(i);
    }
    Init(new heap_internal_test_package.myHeapжInterface(h));
    h.verify(t, 0);

    for (nint i = 10; i > 0; i--) {
        go.container.heap_package.Push(new heap_internal_test_package.myHeapжInterface(h), i);
        h.verify(t, 0);
    }

    for (nint i = 1; h.Len() > 0; i++) {
        nint x = go.container.heap_package.Pop(new heap_internal_test_package.myHeapжInterface(h))._<nint>();
        if (i < 20) {
            go.container.heap_package.Push(new heap_internal_test_package.myHeapжInterface(h), 20 + i);
        }
        h.verify(t, 0);
        if (x != i) {
            t.Errorf("%d.th pop got %d; want %d", i, x, i);
        }
    }
}

public static void TestRemove0(ж<testing.T> t) {
    var h = @new<myHeap>();
    for (nint i = 0; i < 10; i++) {
        h.Push(i);
    }
    h.verify(t, 0);

    while (h.Len() > 0) {
        nint i = h.Len() - 1;
        nint x = Remove(new heap_internal_test_package.myHeapжInterface(h), i)._<nint>();
        if (x != i) {
            t.Errorf("Remove(%d) got %d; want %d", i, x, i);
        }
        h.verify(t, 0);
    }
}

public static void TestRemove1(ж<testing.T> t) {
    var h = @new<myHeap>();
    for (nint i = 0; i < 10; i++) {
        h.Push(i);
    }
    h.verify(t, 0);

    for (nint i = 0; h.Len() > 0; i++) {
        nint x = Remove(new heap_internal_test_package.myHeapжInterface(h), 0)._<nint>();
        if (x != i) {
            t.Errorf("Remove(0) got %d; want %d", x, i);
        }
        h.verify(t, 0);
    }
}

public static void TestRemove2(ж<testing.T> t) {
    nint N = 10;

    var h = @new<myHeap>();
    for (nint i = 0; i < N; i++) {
        h.Push(i);
    }
    h.verify(t, 0);

    var m = new map<nint, bool>();
    while (h.Len() > 0) {
        m[Remove(new heap_internal_test_package.myHeapжInterface(h), (h.Len() - 1) / 2)._<nint>()] = true;
        h.verify(t, 0);
    }

    if (len(m) != N) {
        t.Errorf("len(m) = %d; want %d", len(m), N);
    }
    for (nint i = 0; i < len(m); i++) {
        if (!m[i]) {
            t.Errorf("m[%d] doesn't exist", i);
        }
    }
}

public static void BenchmarkDup(ж<testing.B> Ꮡb) {
    ref var b = ref Ꮡb.DerefOrNull();

    const nint n = 10000;
    ref var h = ref heap<myHeap>(new myHeap(0, n), out var Ꮡh);
    for (nint i = 0; i < b.N; i++) {
        for (nint j = 0; j < n; j++) {
            go.container.heap_package.Push(new heap_internal_test_package.myHeapжInterface(Ꮡh), (nint)0); // all elements are the same
        }
        while (h.Len() > 0) {
            go.container.heap_package.Pop(new heap_internal_test_package.myHeapжInterface(Ꮡh));
        }
    }
}

public static void TestFix(ж<testing.T> t) {
    var h = @new<myHeap>();
    h.verify(t, 0);

    for (nint i = 200; i > 0; i -= 10) {
        go.container.heap_package.Push(new heap_internal_test_package.myHeapжInterface(h), i);
    }
    h.verify(t, 0);

    if ((h.ValueSlot)[0] != 10) {
        t.Fatalf("Expected head to be 10, was %d", (h.ValueSlot)[0]);
    }
    (h.ValueSlot)[0] = 210;
    Fix(new heap_internal_test_package.myHeapжInterface(h), 0);
    h.verify(t, 0);

    for (nint i = 100; i > 0; i--) {
        nint elem = rand.Intn(h.Len());
        if ((i & 1) == 0) {
            (h.ValueSlot)[elem] *= 2;
        } else {
            (h.ValueSlot)[elem] /= 2;
        }
        Fix(new heap_internal_test_package.myHeapжInterface(h), elem);
        h.verify(t, 0);
    }
}

} // end heap_internal_test_package
