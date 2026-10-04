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
internal static void dump(ж<global::go.container.ring_package.Ring> Ꮡr) {
    if (Ꮡr == nil) {
        fmt.Println(emptyˢ);
        return;
    }
    nint i = 0;
    nint n = Ꮡr.Len();
    for (var p = Ꮡr; i < n; p = p.Value.next) {
        fmt.Printf("%4d: %p = {<- %p | %p ->}\n"u8, i, p.OrTypedNil(), (~p).prev.OrTypedNil(), (~p).next.OrTypedNil());
        i++;
    }
    fmt.Println();
}

internal static void verify(ж<testing.T> Ꮡt, ж<global::go.container.ring_package.Ring> Ꮡr, nint N, nint sum) {
    ref var r = ref Ꮡr.DerefOrNull();

    // Len
    nint n = Ꮡr.Len();
    if (n != N) {
        Ꮡt.Errorf("r.Len() == %d; expected %d"u8, n, N);
    }
    // iteration
    n = 0;
    nint s = 0;
    Ꮡr.Do((any p) => {
        n++;
        if (p != default!) {
            s += p._<nint>();
        }
    });
    if (n != N) {
        Ꮡt.Errorf("number of forward iterations == %d; expected %d"u8, n, N);
    }
    if (sum >= 0 && s != sum) {
        Ꮡt.Errorf("forward ring sum = %d; expected %d"u8, s, sum);
    }
    if (Ꮡr == nil) {
        return;
    }
    // connections
    if (r.next != nil) {
        ж<global::go.container.ring_package.Ring> p = default!;                                             // previous element
        for (var q = Ꮡr; p == nil || q != Ꮡr; q = q.Value.next) {
            if (p != nil && p != (~q).prev) {
                Ꮡt.Errorf("prev = %p, expected q.prev = %p\n"u8, p.OrTypedNil(), (~q).prev.OrTypedNil());
            }
            p = q;
        }
        if (p != r.prev) {
            Ꮡt.Errorf("prev = %p, expected r.prev = %p\n"u8, p.OrTypedNil(), r.prev.OrTypedNil());
        }
    }
    // Next, Prev
    if (Ꮡr.Next() != r.next) {
        Ꮡt.Errorf("r.Next() != r.next"u8);
    }
    if (Ꮡr.Prev() != r.prev) {
        Ꮡt.Errorf("r.Prev() != r.prev"u8);
    }
    // Move
    if (Ꮡr.Move(0) != Ꮡr) {
        Ꮡt.Errorf("r.Move(0) != r"u8);
    }
    if (Ꮡr.Move(N) != Ꮡr) {
        Ꮡt.Errorf("r.Move(%d) != r"u8, N);
    }
    if (Ꮡr.Move(-N) != Ꮡr) {
        Ꮡt.Errorf("r.Move(%d) != r"u8, -N);
    }
    for (nint i = 0; i < 10; i++) {
        nint ni = N + i;
        nint mi = rem(ni, N);
        if (Ꮡr.Move(ni) != Ꮡr.Move(mi)) {
            Ꮡt.Errorf("r.Move(%d) != r.Move(%d)"u8, ni, mi);
        }
        if (Ꮡr.Move(-ni) != Ꮡr.Move(-mi)) {
            Ꮡt.Errorf("r.Move(%d) != r.Move(%d)"u8, -ni, -mi);
        }
    }
}

public static void TestCornerCases(ж<testing.T> Ꮡt) {
    ж<global::go.container.ring_package.Ring> r0 = default!;
    ref var r1 = ref heap(new global::go.container.ring_package.Ring(), out var Ꮡr1);
    // Basics
    verify(Ꮡt, r0, 0, 0);
    verify(Ꮡt, Ꮡr1, 1, 0);
    // Insert
    Ꮡr1.Link(r0);
    verify(Ꮡt, r0, 0, 0);
    verify(Ꮡt, Ꮡr1, 1, 0);
    // Insert
    Ꮡr1.Link(r0);
    verify(Ꮡt, r0, 0, 0);
    verify(Ꮡt, Ꮡr1, 1, 0);
    // Unlink
    Ꮡr1.Unlink(0);
    verify(Ꮡt, Ꮡr1, 1, 0);
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

public static void TestNew(ж<testing.T> Ꮡt) {
    for (nint i = 0; i < 10; i++) {
        var r = New(i);
        verify(Ꮡt, r, i, -1);
    }
    for (nint i = 0; i < 10; i++) {
        var r = makeN(i);
        verify(Ꮡt, r, i, sumN(i));
    }
}

public static void TestLink1(ж<testing.T> Ꮡt) {
    var r1a = makeN(1);
    ref var r1b = ref heap(new global::go.container.ring_package.Ring(), out var Ꮡr1b);
    var r2a = r1a.Link(Ꮡr1b);
    verify(Ꮡt, r2a, 2, 1);
    if (r2a != r1a) {
        Ꮡt.Errorf("a) 2-element link failed"u8);
    }
    var r2b = r2a.Link(r2a.Next());
    verify(Ꮡt, r2b, 2, 1);
    if (r2b != r2a.Next()) {
        Ꮡt.Errorf("b) 2-element link failed"u8);
    }
    var r1c = r2b.Link(r2b);
    verify(Ꮡt, r1c, 1, 1);
    verify(Ꮡt, r2b, 1, 0);
}

public static void TestLink2(ж<testing.T> Ꮡt) {
    ж<global::go.container.ring_package.Ring> r0 = default!;
    var r1a = Ꮡ(new Ring(Value: (nint)(42)));
    var r1b = Ꮡ(new Ring(Value: (nint)(77)));
    var r10 = makeN(10);
    r1a.Link(r0);
    verify(Ꮡt, r1a, 1, 42);
    r1a.Link(r1b);
    verify(Ꮡt, r1a, 2, 42 + 77);
    r10.Link(r0);
    verify(Ꮡt, r10, 10, sumN(10));
    r10.Link(r1a);
    verify(Ꮡt, r10, 12, sumN(10) + 42 + 77);
}

public static void TestLink3(ж<testing.T> Ꮡt) {
    ref var r = ref heap(new global::go.container.ring_package.Ring(), out var Ꮡr);
    nint n = 1;
    for (nint i = 1; i < 10; i++) {
        n += i;
        verify(Ꮡt, Ꮡr.Link(New(i)), n, -1);
    }
}

public static void TestUnlink(ж<testing.T> Ꮡt) {
    var r10 = makeN(10);
    var s10 = r10.Move(6);
    nint sum10 = sumN(10);
    verify(Ꮡt, r10, 10, sum10);
    verify(Ꮡt, s10, 10, sum10);
    var r0 = r10.Unlink(0);
    verify(Ꮡt, r0, 0, 0);
    var r1 = r10.Unlink(1);
    verify(Ꮡt, r1, 1, 2);
    verify(Ꮡt, r10, 9, sum10 - 2);
    var r9 = r10.Unlink(9);
    verify(Ꮡt, r9, 9, sum10 - 2);
    verify(Ꮡt, r10, 9, sum10 - 2);
}

public static void TestLinkUnlink(ж<testing.T> Ꮡt) {
    for (nint i = 1; i < 4; i++) {
        var ri = New(i);
        for (nint j = 0; j < i; j++) {
            var rj = ri.Unlink(j);
            verify(Ꮡt, rj, j, -1);
            verify(Ꮡt, ri, i - j, -1);
            ri.Link(rj);
            verify(Ꮡt, ri, i, -1);
        }
    }
}

// Test that calling Move() on an empty Ring initializes it.
public static void TestMoveEmptyRing(ж<testing.T> Ꮡt) {
    ref var r = ref heap(new global::go.container.ring_package.Ring(), out var Ꮡr);
    Ꮡr.Move(1);
    verify(Ꮡt, Ꮡr, 1, 0);
}

} // end ring_internal_test_package
