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
    any v = default!;

    (h, v) = ((h).slice(0, h.Len() - 1), (h)[h.Len() - 1]);
    return v;
}

internal static void Push(this ref myHeap h, any v) {
    h = append(h, v._<nint>());
}

internal static void verify(this myHeap h, ж<testing.T> Ꮡt, nint i) {
    Ꮡt.Helper();
    nint n = h.Len();
    nint j1 = 2 * i + 1;
    nint j2 = 2 * i + 2;
    if (j1 < n) {
        if (h.Less(j1, i)) {
            Ꮡt.Errorf("heap invariant invalidated [%d] = %d > [%d] = %d"u8, i, h[i], j1, h[j1]);
            return;
        }
        h.verify(Ꮡt, j1);
    }
    if (j2 < n) {
        if (h.Less(j2, i)) {
            Ꮡt.Errorf("heap invariant invalidated [%d] = %d > [%d] = %d"u8, i, h[i], j1, h[j2]);
            return;
        }
        h.verify(Ꮡt, j2);
    }
}

public static void TestInit0(ж<testing.T> Ꮡt) {
    var h = @new<myHeap>();
    for (nint i = 20; i > 0; i--) {
        h.Push((nint)(0)); // all elements are the same
    }
    Init(new heap_internal_test_package.myHeapжInterface(h));
    (~h).verify(Ꮡt, 0);
    for (nint i = 1; h.Len() > 0; i++) {
        nint x = go.container.heap_package.Pop(new heap_internal_test_package.myHeapжInterface(h))._<nint>();
        (~h).verify(Ꮡt, 0);
        if (x != 0) {
            Ꮡt.Errorf("%d.th pop got %d; want %d"u8, i, x, (nint)(0));
        }
    }
}

public static void TestInit1(ж<testing.T> Ꮡt) {
    var h = @new<myHeap>();
    for (nint i = 20; i > 0; i--) {
        h.Push(i); // all elements are different
    }
    Init(new heap_internal_test_package.myHeapжInterface(h));
    (~h).verify(Ꮡt, 0);
    for (nint i = 1; h.Len() > 0; i++) {
        nint x = go.container.heap_package.Pop(new heap_internal_test_package.myHeapжInterface(h))._<nint>();
        (~h).verify(Ꮡt, 0);
        if (x != i) {
            Ꮡt.Errorf("%d.th pop got %d; want %d"u8, i, x, i);
        }
    }
}

public static void Test(ж<testing.T> Ꮡt) {
    var h = @new<myHeap>();
    (~h).verify(Ꮡt, 0);
    for (nint i = 20; i > 10; i--) {
        h.Push(i);
    }
    Init(new heap_internal_test_package.myHeapжInterface(h));
    (~h).verify(Ꮡt, 0);
    for (nint i = 10; i > 0; i--) {
        go.container.heap_package.Push(new heap_internal_test_package.myHeapжInterface(h), i);
        (~h).verify(Ꮡt, 0);
    }
    for (nint i = 1; h.Len() > 0; i++) {
        nint x = go.container.heap_package.Pop(new heap_internal_test_package.myHeapжInterface(h))._<nint>();
        if (i < 20) {
            go.container.heap_package.Push(new heap_internal_test_package.myHeapжInterface(h), 20 + i);
        }
        (~h).verify(Ꮡt, 0);
        if (x != i) {
            Ꮡt.Errorf("%d.th pop got %d; want %d"u8, i, x, i);
        }
    }
}

public static void TestRemove0(ж<testing.T> Ꮡt) {
    var h = @new<myHeap>();
    for (nint i = 0; i < 10; i++) {
        h.Push(i);
    }
    (~h).verify(Ꮡt, 0);
    while (h.Len() > 0) {
        nint i = h.Len() - 1;
        nint x = Remove(new heap_internal_test_package.myHeapжInterface(h), i)._<nint>();
        if (x != i) {
            Ꮡt.Errorf("Remove(%d) got %d; want %d"u8, i, x, i);
        }
        (~h).verify(Ꮡt, 0);
    }
}

public static void TestRemove1(ж<testing.T> Ꮡt) {
    var h = @new<myHeap>();
    for (nint i = 0; i < 10; i++) {
        h.Push(i);
    }
    (~h).verify(Ꮡt, 0);
    for (nint i = 0; h.Len() > 0; i++) {
        nint x = Remove(new heap_internal_test_package.myHeapжInterface(h), 0)._<nint>();
        if (x != i) {
            Ꮡt.Errorf("Remove(0) got %d; want %d"u8, x, i);
        }
        (~h).verify(Ꮡt, 0);
    }
}

public static void TestRemove2(ж<testing.T> Ꮡt) {
    ref var t = ref Ꮡt.DerefOrNull();

    nint N = 10;
    var h = @new<myHeap>();
    for (nint i = 0; i < N; i++) {
        h.Push(i);
    }
    (~h).verify(Ꮡt, 0);
    var m = new map<nint, bool>();
    while (h.Len() > 0) {
        m[Remove(new heap_internal_test_package.myHeapжInterface(h), (h.Len() - 1) / 2)._<nint>()] = true;
        (~h).verify(Ꮡt, 0);
    }
    if (len(m) != N) {
        Ꮡt.Errorf("len(m) = %d; want %d"u8, len(m), N);
    }
    for (nint i = 0; i < len(m); i++) {
        if (!m[i]) {
            Ꮡt.Errorf("m[%d] doesn't exist"u8, i);
        }
    }
}

public static void BenchmarkDup(ж<testing.B> Ꮡb) {
    ref var b = ref Ꮡb.DerefOrNull();

    const nint n = 10000;
    ref var h = ref heap<myHeap>(out var Ꮡh);
    h = new myHeap(0, n);
    for (nint i = 0; i < b.N; i++) {
        for (nint j = 0; j < n; j++) {
            go.container.heap_package.Push(new heap_internal_test_package.myHeapжInterface(Ꮡh), (nint)(0)); // all elements are the same
        }
        while (h.Len() > 0) {
            go.container.heap_package.Pop(new heap_internal_test_package.myHeapжInterface(Ꮡh));
        }
    }
}

public static void TestFix(ж<testing.T> Ꮡt) {
    var h = @new<myHeap>();
    (~h).verify(Ꮡt, 0);
    for (nint i = 200; i > 0; i -= 10) {
        go.container.heap_package.Push(new heap_internal_test_package.myHeapжInterface(h), i);
    }
    (~h).verify(Ꮡt, 0);
    if ((h.ValueSlot)[0] != 10) {
        Ꮡt.Fatalf("Expected head to be 10, was %d"u8, (h.ValueSlot)[0]);
    }
    (h.ValueSlot)[0] = 210;
    Fix(new heap_internal_test_package.myHeapжInterface(h), 0);
    (~h).verify(Ꮡt, 0);
    for (nint i = 100; i > 0; i--) {
        nint elem = rand.Intn(h.Len());
        if ((nint)(i & 1) == 0){
            (h.ValueSlot)[elem] *= 2;
        } else {
            (h.ValueSlot)[elem] /= 2;
        }
        Fix(new heap_internal_test_package.myHeapжInterface(h), elem);
        (~h).verify(Ꮡt, 0);
    }
}

} // end heap_internal_test_package
