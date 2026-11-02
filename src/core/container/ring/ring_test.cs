// Copyright 2009 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.
namespace go.container;

using fmt = fmt_package;
using testing = testing_package;
using static go.container.ring_package;

partial class ring_internal_test_package {

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static readonly object emptyˢ = (@string)"empty"u8;

// For debugging - keep around.
internal static void dump(ж<global::go.container.ring_package.Ring> r) {
    if (r == nil) {
        fmt.Println(emptyˢ);
        return;
    }
    nint i = 0;
    nint n = r.Len();
    for (var p = r; i < n; p = p.next) {
        fmt.Printf("%4d: %p = {<- %p | %p ->}\n"u8, i, p.OrTypedNil(), p.prev.OrTypedNil(), p.next.OrTypedNil());
        i++;
    }
    fmt.Println();
}

internal static void verify(ж<testing.T> t, ж<global::go.container.ring_package.Ring> Ꮡr, nint N, nint sum) {
    ref var r = ref Ꮡr.DerefOrNull();

    // Len
    nint n = Ꮡr.Len();
    if (n != N) {
        t.Errorf("r.Len() == %d; expected %d", n, N);
    }

    // iteration
    n = 0;
    nint s = 0;
    Ꮡr.Do((any p) => {
        n++;
        if (p != nil) {
            s += p._<nint>();
        }
    });
    if (n != N) {
        t.Errorf("number of forward iterations == %d; expected %d", n, N);
    }
    if (sum >= 0 && s != sum) {
        t.Errorf("forward ring sum = %d; expected %d", s, sum);
    }

    if (Ꮡr == nil) {
        return;
    }

    // connections
    if (r.next != nil) {
        ж<global::go.container.ring_package.Ring> p = default; // previous element
        for (var q = Ꮡr; p == nil || q != Ꮡr; q = q.next) {
            if (p != nil && p != q.prev) {
                t.Errorf("prev = %p, expected q.prev = %p\n", p.OrTypedNil(), q.prev.OrTypedNil());
            }
            p = q;
        }
        if (p != r.prev) {
            t.Errorf("prev = %p, expected r.prev = %p\n", p.OrTypedNil(), r.prev.OrTypedNil());
        }
    }

    // Next, Prev
    if (Ꮡr.Next() != r.next) {
        t.Errorf("r.Next() != r.next");
    }
    if (Ꮡr.Prev() != r.prev) {
        t.Errorf("r.Prev() != r.prev");
    }

    // Move
    if (Ꮡr.Move(0) != Ꮡr) {
        t.Errorf("r.Move(0) != r");
    }
    if (Ꮡr.Move(N) != Ꮡr) {
        t.Errorf("r.Move(%d) != r", N);
    }
    if (Ꮡr.Move(-N) != Ꮡr) {
        t.Errorf("r.Move(%d) != r", -N);
    }
    for (nint i = 0; i < 10; i++) {
        nint ni = N + i;
        nint mi = rem(ni, N);
        if (Ꮡr.Move(ni) != Ꮡr.Move(mi)) {
            t.Errorf("r.Move(%d) != r.Move(%d)", ni, mi);
        }
        if (Ꮡr.Move(-ni) != Ꮡr.Move(-mi)) {
            t.Errorf("r.Move(%d) != r.Move(%d)", -ni, -mi);
        }
    }
}

public static void TestCornerCases(ж<testing.T> t) {
    ж<global::go.container.ring_package.Ring> r0 = default;
    ref var r1 = ref heap(new global::go.container.ring_package.Ring(), out var Ꮡr1);
    // Basics
    verify(t, r0, 0, 0);
    verify(t, Ꮡr1, 1, 0);
    // Insert
    Ꮡr1.Link(r0);
    verify(t, r0, 0, 0);
    verify(t, Ꮡr1, 1, 0);
    // Insert
    Ꮡr1.Link(r0);
    verify(t, r0, 0, 0);
    verify(t, Ꮡr1, 1, 0);
    // Unlink
    Ꮡr1.Unlink(0);
    verify(t, Ꮡr1, 1, 0);
}

internal static ж<global::go.container.ring_package.Ring> makeN(nint n) {
    var r = New(n);
    for (nint i = 1; i <= n; i++) {
        r.Value.Value = i;
        r = r.Next();
    }
    return r;
}

internal static nint sumN(nint n) {
    return (n * n + n) / 2;
}

public static void TestNew(ж<testing.T> t) {
    for (nint i = 0; i < 10; i++) {
        var r = New(i);
        verify(t, r, i, -1);
    }
    for (nint i = 0; i < 10; i++) {
        var r = makeN(i);
        verify(t, r, i, sumN(i));
    }
}

public static void TestLink1(ж<testing.T> t) {
    var r1a = makeN(1);
    ref var r1b = ref heap(new global::go.container.ring_package.Ring(), out var Ꮡr1b);
    var r2a = r1a.Link(Ꮡr1b);
    verify(t, r2a, 2, 1);
    if (r2a != r1a) {
        t.Errorf("a) 2-element link failed");
    }

    var r2b = r2a.Link(r2a.Next());
    verify(t, r2b, 2, 1);
    if (r2b != r2a.Next()) {
        t.Errorf("b) 2-element link failed");
    }

    var r1c = r2b.Link(r2b);
    verify(t, r1c, 1, 1);
    verify(t, r2b, 1, 0);
}

public static void TestLink2(ж<testing.T> t) {
    ж<global::go.container.ring_package.Ring> r0 = default;
    var r1a = Ꮡ(new Ring(Value: (nint)42));
    var r1b = Ꮡ(new Ring(Value: (nint)77));
    var r10 = makeN(10);

    r1a.Link(r0);
    verify(t, r1a, 1, 42);

    r1a.Link(r1b);
    verify(t, r1a, 2, 42 + 77);

    r10.Link(r0);
    verify(t, r10, 10, sumN(10));

    r10.Link(r1a);
    verify(t, r10, 12, sumN(10) + 42 + 77);
}

public static void TestLink3(ж<testing.T> t) {
    ref var r = ref heap(new global::go.container.ring_package.Ring(), out var Ꮡr);
    nint n = 1;
    for (nint i = 1; i < 10; i++) {
        n += i;
        verify(t, Ꮡr.Link(New(i)), n, -1);
    }
}

public static void TestUnlink(ж<testing.T> t) {
    var r10 = makeN(10);
    var s10 = r10.Move(6);

    nint sum10 = sumN(10);

    verify(t, r10, 10, sum10);
    verify(t, s10, 10, sum10);

    var r0 = r10.Unlink(0);
    verify(t, r0, 0, 0);

    var r1 = r10.Unlink(1);
    verify(t, r1, 1, 2);
    verify(t, r10, 9, sum10 - 2);

    var r9 = r10.Unlink(9);
    verify(t, r9, 9, sum10 - 2);
    verify(t, r10, 9, sum10 - 2);
}

public static void TestLinkUnlink(ж<testing.T> t) {
    for (nint i = 1; i < 4; i++) {
        var ri = New(i);
        for (nint j = 0; j < i; j++) {
            var rj = ri.Unlink(j);
            verify(t, rj, j, -1);
            verify(t, ri, i - j, -1);
            ri.Link(rj);
            verify(t, ri, i, -1);
        }
    }
}

// Test that calling Move() on an empty Ring initializes it.
public static void TestMoveEmptyRing(ж<testing.T> t) {
    ref var r = ref heap(new global::go.container.ring_package.Ring(), out var Ꮡr);

    Ꮡr.Move(1);
    verify(t, Ꮡr, 1, 0);
}

} // end ring_internal_test_package
