// Copyright 2009 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.
namespace go.container;

using testing = testing_package;
using static go.container.list_package;

partial class list_internal_test_package {

internal static bool checkListLen(ж<testing.T> Ꮡt, ж<global::go.container.list_package.List> Ꮡl, nint len) {
    ref var l = ref Ꮡl.DerefOrNull();

    {
        nint n = l.Len(); if (n != len) {
            Ꮡt.Errorf("l.Len() = %d, want %d"u8, n, len);
            return false;
        }
    }
    return true;
}

internal static void checkListPointers(ж<testing.T> Ꮡt, ж<global::go.container.list_package.List> Ꮡl, slice<ж<global::go.container.list_package.Element>> es) {
    ref var l = ref Ꮡl.DerefOrNull();

    var root = Ꮡl.of(global::go.container.list_package.List.Ꮡroot);

    if (!checkListLen(Ꮡt, Ꮡl, len(es))) {
        return;
    }

    // zero length lists must be the zero value or properly initialized (sentinel circle)
    if (len(es) == 0) {
        if (l.root.next != nil && l.root.next != root || l.root.prev != nil && l.root.prev != root) {
            Ꮡt.Errorf("l.root.next = %p, l.root.prev = %p; both should both be nil or %p"u8, l.root.next.OrTypedNil(), l.root.prev.OrTypedNil(), root.OrTypedNil());
        }
        return;
    }
    // len(es) > 0

    // check internal and external prev/next connections
    foreach (var (i, e) in es) {
        var prev = root;
        var Prev = (ж<global::go.container.list_package.Element>)nil;
        if (i > 0) {
            prev = es[i - 1];
            Prev = prev;
        }
        {
            var p = e.Value.prev; if (p != prev) {
                Ꮡt.Errorf("elt[%d](%p).prev = %p, want %p"u8, i, e.OrTypedNil(), p.OrTypedNil(), prev.OrTypedNil());
            }
        }
        {
            var p = e.Prev(); if (p != Prev) {
                Ꮡt.Errorf("elt[%d](%p).Prev() = %p, want %p"u8, i, e.OrTypedNil(), p.OrTypedNil(), Prev.OrTypedNil());
            }
        }

        var next = root;
        var Next = (ж<global::go.container.list_package.Element>)nil;
        if (i < len(es) - 1) {
            next = es[i + 1];
            Next = next;
        }
        {
            var n = e.Value.next; if (n != next) {
                Ꮡt.Errorf("elt[%d](%p).next = %p, want %p"u8, i, e.OrTypedNil(), n.OrTypedNil(), next.OrTypedNil());
            }
        }
        {
            var n = e.Next(); if (n != Next) {
                Ꮡt.Errorf("elt[%d](%p).Next() = %p, want %p"u8, i, e.OrTypedNil(), n.OrTypedNil(), Next.OrTypedNil());
            }
        }
    }
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static readonly object bananaˢ = (@string)"banana"u8;

public static void TestList(ж<testing.T> Ꮡt) {
    var l = New();
    checkListPointers(Ꮡt, l, new ж<global::go.container.list_package.Element>[]{}.slice());

    // Single element list
    var e = l.PushFront((@string)"a"u8);
    checkListPointers(Ꮡt, l, new ж<global::go.container.list_package.Element>[]{e}.slice());
    l.MoveToFront(e);
    checkListPointers(Ꮡt, l, new ж<global::go.container.list_package.Element>[]{e}.slice());
    l.MoveToBack(e);
    checkListPointers(Ꮡt, l, new ж<global::go.container.list_package.Element>[]{e}.slice());
    l.Remove(e);
    checkListPointers(Ꮡt, l, new ж<global::go.container.list_package.Element>[]{}.slice());

    // Bigger list
    var e2 = l.PushFront((nint)2);
    var e1 = l.PushFront((nint)1);
    var e3 = l.PushBack((nint)3);
    var e4 = l.PushBack(bananaˢ);
    checkListPointers(Ꮡt, l, new ж<global::go.container.list_package.Element>[]{e1, e2, e3, e4}.slice());

    l.Remove(e2);
    checkListPointers(Ꮡt, l, new ж<global::go.container.list_package.Element>[]{e1, e3, e4}.slice());

    l.MoveToFront(e3); // move from middle
    checkListPointers(Ꮡt, l, new ж<global::go.container.list_package.Element>[]{e3, e1, e4}.slice());

    l.MoveToFront(e1);
    l.MoveToBack(e3); // move from middle
    checkListPointers(Ꮡt, l, new ж<global::go.container.list_package.Element>[]{e1, e4, e3}.slice());

    l.MoveToFront(e3); // move from back
    checkListPointers(Ꮡt, l, new ж<global::go.container.list_package.Element>[]{e3, e1, e4}.slice());
    l.MoveToFront(e3); // should be no-op
    checkListPointers(Ꮡt, l, new ж<global::go.container.list_package.Element>[]{e3, e1, e4}.slice());

    l.MoveToBack(e3); // move from front
    checkListPointers(Ꮡt, l, new ж<global::go.container.list_package.Element>[]{e1, e4, e3}.slice());
    l.MoveToBack(e3); // should be no-op
    checkListPointers(Ꮡt, l, new ж<global::go.container.list_package.Element>[]{e1, e4, e3}.slice());

    e2 = l.InsertBefore((nint)2, e1); // insert before front
    checkListPointers(Ꮡt, l, new ж<global::go.container.list_package.Element>[]{e2, e1, e4, e3}.slice());
    l.Remove(e2);
    e2 = l.InsertBefore((nint)2, e4); // insert before middle
    checkListPointers(Ꮡt, l, new ж<global::go.container.list_package.Element>[]{e1, e2, e4, e3}.slice());
    l.Remove(e2);
    e2 = l.InsertBefore((nint)2, e3); // insert before back
    checkListPointers(Ꮡt, l, new ж<global::go.container.list_package.Element>[]{e1, e4, e2, e3}.slice());
    l.Remove(e2);

    e2 = l.InsertAfter((nint)2, e1); // insert after front
    checkListPointers(Ꮡt, l, new ж<global::go.container.list_package.Element>[]{e1, e2, e4, e3}.slice());
    l.Remove(e2);
    e2 = l.InsertAfter((nint)2, e4); // insert after middle
    checkListPointers(Ꮡt, l, new ж<global::go.container.list_package.Element>[]{e1, e4, e2, e3}.slice());
    l.Remove(e2);
    e2 = l.InsertAfter((nint)2, e3); // insert after back
    checkListPointers(Ꮡt, l, new ж<global::go.container.list_package.Element>[]{e1, e4, e3, e2}.slice());
    l.Remove(e2);

    // Check standard iteration.
    nint sum = 0;
    for (var eΔ1 = l.Front(); eΔ1 != nil; eΔ1 = eΔ1.Next()) {
        {
            var (i, ok) = (~eΔ1).Value._<nint>(ᐧ); if (ok) {
                sum += i;
            }
        }
    }
    if (sum != 4) {
        Ꮡt.Errorf("sum over l = %d, want 4"u8, sum);
    }

    // Clear all elements by iterating
    ж<global::go.container.list_package.Element> next = default!;
    for (var eΔ2 = l.Front(); eΔ2 != nil; eΔ2 = next) {
        next = eΔ2.Next();
        l.Remove(eΔ2);
    }
    checkListPointers(Ꮡt, l, new ж<global::go.container.list_package.Element>[]{}.slice());
}

internal static void checkList(ж<testing.T> Ꮡt, ж<global::go.container.list_package.List> Ꮡl, slice<any> es) {
    ref var l = ref Ꮡl.DerefOrNull();

    if (!checkListLen(Ꮡt, Ꮡl, len(es))) {
        return;
    }

    nint i = 0;
    for (var e = l.Front(); e != nil; e = e.Next()) {
        nint le = (~e).Value._<nint>();
        if (!AreEqual(le, es[i])) {
            Ꮡt.Errorf("elt[%d].Value = %v, want %v"u8, i, le, es[i]);
        }
        i++;
    }
}

public static void TestExtending(ж<testing.T> Ꮡt) {
    var l1 = New();
    var l2 = New();

    l1.PushBack((nint)1);
    l1.PushBack((nint)2);
    l1.PushBack((nint)3);

    l2.PushBack((nint)4);
    l2.PushBack((nint)5);

    var l3 = New();
    l3.PushBackList(l1);
    checkList(Ꮡt, l3, new any[]{(nint)1, (nint)2, (nint)3}.slice());
    l3.PushBackList(l2);
    checkList(Ꮡt, l3, new any[]{(nint)1, (nint)2, (nint)3, (nint)4, (nint)5}.slice());

    l3 = New();
    l3.PushFrontList(l2);
    checkList(Ꮡt, l3, new any[]{(nint)4, (nint)5}.slice());
    l3.PushFrontList(l1);
    checkList(Ꮡt, l3, new any[]{(nint)1, (nint)2, (nint)3, (nint)4, (nint)5}.slice());

    checkList(Ꮡt, l1, new any[]{(nint)1, (nint)2, (nint)3}.slice());
    checkList(Ꮡt, l2, new any[]{(nint)4, (nint)5}.slice());

    l3 = New();
    l3.PushBackList(l1);
    checkList(Ꮡt, l3, new any[]{(nint)1, (nint)2, (nint)3}.slice());
    l3.PushBackList(l3);
    checkList(Ꮡt, l3, new any[]{(nint)1, (nint)2, (nint)3, (nint)1, (nint)2, (nint)3}.slice());

    l3 = New();
    l3.PushFrontList(l1);
    checkList(Ꮡt, l3, new any[]{(nint)1, (nint)2, (nint)3}.slice());
    l3.PushFrontList(l3);
    checkList(Ꮡt, l3, new any[]{(nint)1, (nint)2, (nint)3, (nint)1, (nint)2, (nint)3}.slice());

    l3 = New();
    l1.PushBackList(l3);
    checkList(Ꮡt, l1, new any[]{(nint)1, (nint)2, (nint)3}.slice());
    l1.PushFrontList(l3);
    checkList(Ꮡt, l1, new any[]{(nint)1, (nint)2, (nint)3}.slice());
}

public static void TestRemove(ж<testing.T> Ꮡt) {
    var l = New();
    var e1 = l.PushBack((nint)1);
    var e2 = l.PushBack((nint)2);
    checkListPointers(Ꮡt, l, new ж<global::go.container.list_package.Element>[]{e1, e2}.slice());
    var e = l.Front();
    l.Remove(e);
    checkListPointers(Ꮡt, l, new ж<global::go.container.list_package.Element>[]{e2}.slice());
    l.Remove(e);
    checkListPointers(Ꮡt, l, new ж<global::go.container.list_package.Element>[]{e2}.slice());
}

public static void TestIssue4103(ж<testing.T> Ꮡt) {
    var l1 = New();
    l1.PushBack((nint)1);
    l1.PushBack((nint)2);

    var l2 = New();
    l2.PushBack((nint)3);
    l2.PushBack((nint)4);

    var e = l1.Front();
    l2.Remove(e); // l2 should not change because e is not an element of l2
    {
        nint n = l2.Len(); if (n != 2) {
            Ꮡt.Errorf("l2.Len() = %d, want 2"u8, n);
        }
    }

    l1.InsertBefore((nint)8, e);
    {
        nint n = l1.Len(); if (n != 3) {
            Ꮡt.Errorf("l1.Len() = %d, want 3"u8, n);
        }
    }
}

public static void TestIssue6349(ж<testing.T> Ꮡt) {
    var l = New();
    l.PushBack((nint)1);
    l.PushBack((nint)2);

    var e = l.Front();
    l.Remove(e);
    if (!AreEqual((~e).Value, (nint)1)) {
        Ꮡt.Errorf("e.value = %d, want 1"u8, (~e).Value);
    }
    if (e.Next() != nil) {
        Ꮡt.Errorf("e.Next() != nil"u8);
    }
    if (e.Prev() != nil) {
        Ꮡt.Errorf("e.Prev() != nil"u8);
    }
}

public static void TestMove(ж<testing.T> Ꮡt) {
    var l = New();
    var e1 = l.PushBack((nint)1);
    var e2 = l.PushBack((nint)2);
    var e3 = l.PushBack((nint)3);
    var e4 = l.PushBack((nint)4);

    l.MoveAfter(e3, e3);
    checkListPointers(Ꮡt, l, new ж<global::go.container.list_package.Element>[]{e1, e2, e3, e4}.slice());
    l.MoveBefore(e2, e2);
    checkListPointers(Ꮡt, l, new ж<global::go.container.list_package.Element>[]{e1, e2, e3, e4}.slice());

    l.MoveAfter(e3, e2);
    checkListPointers(Ꮡt, l, new ж<global::go.container.list_package.Element>[]{e1, e2, e3, e4}.slice());
    l.MoveBefore(e2, e3);
    checkListPointers(Ꮡt, l, new ж<global::go.container.list_package.Element>[]{e1, e2, e3, e4}.slice());

    l.MoveBefore(e2, e4);
    checkListPointers(Ꮡt, l, new ж<global::go.container.list_package.Element>[]{e1, e3, e2, e4}.slice());
    (e2, e3) = (e3, e2);

    l.MoveBefore(e4, e1);
    checkListPointers(Ꮡt, l, new ж<global::go.container.list_package.Element>[]{e4, e1, e2, e3}.slice());
    (e1, e2, e3, e4) = (e4, e1, e2, e3);

    l.MoveAfter(e4, e1);
    checkListPointers(Ꮡt, l, new ж<global::go.container.list_package.Element>[]{e1, e4, e2, e3}.slice());
    (e2, e3, e4) = (e4, e2, e3);

    l.MoveAfter(e2, e3);
    checkListPointers(Ꮡt, l, new ж<global::go.container.list_package.Element>[]{e1, e3, e2, e4}.slice());
}

// Test PushFront, PushBack, PushFrontList, PushBackList with uninitialized List
public static void TestZeroList(ж<testing.T> Ꮡt) {
    ж<global::go.container.list_package.List> l1 = @new<global::go.container.list_package.List>();
    l1.PushFront((nint)1);
    checkList(Ꮡt, l1, new any[]{(nint)1}.slice());

    ж<global::go.container.list_package.List> l2 = @new<global::go.container.list_package.List>();
    l2.PushBack((nint)1);
    checkList(Ꮡt, l2, new any[]{(nint)1}.slice());

    ж<global::go.container.list_package.List> l3 = @new<global::go.container.list_package.List>();
    l3.PushFrontList(l1);
    checkList(Ꮡt, l3, new any[]{(nint)1}.slice());

    ж<global::go.container.list_package.List> l4 = @new<global::go.container.list_package.List>();
    l4.PushBackList(l2);
    checkList(Ꮡt, l4, new any[]{(nint)1}.slice());
}

// Test that a list l is not modified when calling InsertBefore with a mark that is not an element of l.
public static void TestInsertBeforeUnknownMark(ж<testing.T> Ꮡt) {
    ref var l = ref heap(new global::go.container.list_package.List(), out var Ꮡl);
    Ꮡl.PushBack((nint)1);
    Ꮡl.PushBack((nint)2);
    Ꮡl.PushBack((nint)3);
    Ꮡl.InsertBefore((nint)1, @new<global::go.container.list_package.Element>());
    checkList(Ꮡt, Ꮡl, new any[]{(nint)1, (nint)2, (nint)3}.slice());
}

// Test that a list l is not modified when calling InsertAfter with a mark that is not an element of l.
public static void TestInsertAfterUnknownMark(ж<testing.T> Ꮡt) {
    ref var l = ref heap(new global::go.container.list_package.List(), out var Ꮡl);
    Ꮡl.PushBack((nint)1);
    Ꮡl.PushBack((nint)2);
    Ꮡl.PushBack((nint)3);
    Ꮡl.InsertAfter((nint)1, @new<global::go.container.list_package.Element>());
    checkList(Ꮡt, Ꮡl, new any[]{(nint)1, (nint)2, (nint)3}.slice());
}

// Test that a list l is not modified when calling MoveAfter or MoveBefore with a mark that is not an element of l.
public static void TestMoveUnknownMark(ж<testing.T> Ꮡt) {
    ref var l1 = ref heap(new global::go.container.list_package.List(), out var Ꮡl1);
    var e1 = Ꮡl1.PushBack((nint)1);

    ref var l2 = ref heap(new global::go.container.list_package.List(), out var Ꮡl2);
    var e2 = Ꮡl2.PushBack((nint)2);

    Ꮡl1.MoveAfter(e1, e2);
    checkList(Ꮡt, Ꮡl1, new any[]{(nint)1}.slice());
    checkList(Ꮡt, Ꮡl2, new any[]{(nint)2}.slice());

    Ꮡl1.MoveBefore(e1, e2);
    checkList(Ꮡt, Ꮡl1, new any[]{(nint)1}.slice());
    checkList(Ꮡt, Ꮡl2, new any[]{(nint)2}.slice());
}

} // end list_internal_test_package
