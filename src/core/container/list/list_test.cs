// Copyright 2009 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.
namespace go.container;

using testing = testing_package;
using static go.container.list_package;

partial class list_internal_test_package {

internal static bool checkListLen(ж<testing.T> t, ж<global::go.container.list_package.List> Ꮡl, nint len) {
    ref var l = ref Ꮡl.DerefOrNull();

    {
        nint n = l.Len(); if (n != len) {
            t.Errorf("l.Len() = %d, want %d", n, len);
            return false;
        }
    }
    return true;
}

internal static void checkListPointers(ж<testing.T> t, ж<global::go.container.list_package.List> Ꮡl, slice<ж<global::go.container.list_package.Element>> es) {
    ref var l = ref Ꮡl.DerefOrNull();

    var root = Ꮡl.Ꮡroot;

    if (!checkListLen(t, Ꮡl, len(es))) {
        return;
    }

    // zero length lists must be the zero value or properly initialized (sentinel circle)
    if (len(es) == 0) {
        if (l.root.next != nil && l.root.next != root || l.root.prev != nil && l.root.prev != root) {
            t.Errorf("l.root.next = %p, l.root.prev = %p; both should both be nil or %p", l.root.next.OrTypedNil(), l.root.prev.OrTypedNil(), root.OrTypedNil());
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
            var p = e.prev; if (p != prev) {
                t.Errorf("elt[%d](%p).prev = %p, want %p", i, e.OrTypedNil(), p.OrTypedNil(), prev.OrTypedNil());
            }
        }
        {
            var p = e.Prev(); if (p != Prev) {
                t.Errorf("elt[%d](%p).Prev() = %p, want %p", i, e.OrTypedNil(), p.OrTypedNil(), Prev.OrTypedNil());
            }
        }

        var next = root;
        var Next = (ж<global::go.container.list_package.Element>)nil;
        if (i < len(es) - 1) {
            next = es[i + 1];
            Next = next;
        }
        {
            var n = e.next; if (n != next) {
                t.Errorf("elt[%d](%p).next = %p, want %p", i, e.OrTypedNil(), n.OrTypedNil(), next.OrTypedNil());
            }
        }
        {
            var n = e.Next(); if (n != Next) {
                t.Errorf("elt[%d](%p).Next() = %p, want %p", i, e.OrTypedNil(), n.OrTypedNil(), Next.OrTypedNil());
            }
        }
    }
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static readonly object bananaˢ = (@string)"banana"u8;

public static void TestList(ж<testing.T> t) {
    var l = New();
    checkListPointers(t, l, slice<ж<global::go.container.list_package.Element>>([]));

    // Single element list
    var e = l.PushFront((@string)"a");
    checkListPointers(t, l, slice<ж<global::go.container.list_package.Element>>([e]));
    l.MoveToFront(e);
    checkListPointers(t, l, slice<ж<global::go.container.list_package.Element>>([e]));
    l.MoveToBack(e);
    checkListPointers(t, l, slice<ж<global::go.container.list_package.Element>>([e]));
    l.Remove(e);
    checkListPointers(t, l, slice<ж<global::go.container.list_package.Element>>([]));

    // Bigger list
    var e2 = l.PushFront((nint)2);
    var e1 = l.PushFront((nint)1);
    var e3 = l.PushBack((nint)3);
    var e4 = l.PushBack(bananaˢ);
    checkListPointers(t, l, slice<ж<global::go.container.list_package.Element>>([e1, e2, e3, e4]));

    l.Remove(e2);
    checkListPointers(t, l, slice<ж<global::go.container.list_package.Element>>([e1, e3, e4]));

    l.MoveToFront(e3); // move from middle
    checkListPointers(t, l, slice<ж<global::go.container.list_package.Element>>([e3, e1, e4]));

    l.MoveToFront(e1);
    l.MoveToBack(e3); // move from middle
    checkListPointers(t, l, slice<ж<global::go.container.list_package.Element>>([e1, e4, e3]));

    l.MoveToFront(e3); // move from back
    checkListPointers(t, l, slice<ж<global::go.container.list_package.Element>>([e3, e1, e4]));
    l.MoveToFront(e3); // should be no-op
    checkListPointers(t, l, slice<ж<global::go.container.list_package.Element>>([e3, e1, e4]));

    l.MoveToBack(e3); // move from front
    checkListPointers(t, l, slice<ж<global::go.container.list_package.Element>>([e1, e4, e3]));
    l.MoveToBack(e3); // should be no-op
    checkListPointers(t, l, slice<ж<global::go.container.list_package.Element>>([e1, e4, e3]));

    e2 = l.InsertBefore((nint)2, e1); // insert before front
    checkListPointers(t, l, slice<ж<global::go.container.list_package.Element>>([e2, e1, e4, e3]));
    l.Remove(e2);
    e2 = l.InsertBefore((nint)2, e4); // insert before middle
    checkListPointers(t, l, slice<ж<global::go.container.list_package.Element>>([e1, e2, e4, e3]));
    l.Remove(e2);
    e2 = l.InsertBefore((nint)2, e3); // insert before back
    checkListPointers(t, l, slice<ж<global::go.container.list_package.Element>>([e1, e4, e2, e3]));
    l.Remove(e2);

    e2 = l.InsertAfter((nint)2, e1); // insert after front
    checkListPointers(t, l, slice<ж<global::go.container.list_package.Element>>([e1, e2, e4, e3]));
    l.Remove(e2);
    e2 = l.InsertAfter((nint)2, e4); // insert after middle
    checkListPointers(t, l, slice<ж<global::go.container.list_package.Element>>([e1, e4, e2, e3]));
    l.Remove(e2);
    e2 = l.InsertAfter((nint)2, e3); // insert after back
    checkListPointers(t, l, slice<ж<global::go.container.list_package.Element>>([e1, e4, e3, e2]));
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
        t.Errorf("sum over l = %d, want 4", sum);
    }

    // Clear all elements by iterating
    ж<global::go.container.list_package.Element> next = default;
    for (var eΔ2 = l.Front(); eΔ2 != nil; eΔ2 = next) {
        next = eΔ2.Next();
        l.Remove(eΔ2);
    }
    checkListPointers(t, l, slice<ж<global::go.container.list_package.Element>>([]));
}

internal static void checkList(ж<testing.T> t, ж<global::go.container.list_package.List> Ꮡl, slice<any> es) {
    ref var l = ref Ꮡl.DerefOrNull();

    if (!checkListLen(t, Ꮡl, len(es))) {
        return;
    }

    nint i = 0;
    for (var e = l.Front(); e != nil; e = e.Next()) {
        nint le = (~e).Value._<nint>();
        if (!AreEqual(le, es[i])) {
            t.Errorf("elt[%d].Value = %v, want %v", i, le, es[i]);
        }
        i++;
    }
}

public static void TestExtending(ж<testing.T> t) {
    var l1 = New();
    var l2 = New();

    l1.PushBack((nint)1);
    l1.PushBack((nint)2);
    l1.PushBack((nint)3);

    l2.PushBack((nint)4);
    l2.PushBack((nint)5);

    var l3 = New();
    l3.PushBackList(l1);
    checkList(t, l3, slice<any>([(nint)1, (nint)2, (nint)3]));
    l3.PushBackList(l2);
    checkList(t, l3, slice<any>([(nint)1, (nint)2, (nint)3, (nint)4, (nint)5]));

    l3 = New();
    l3.PushFrontList(l2);
    checkList(t, l3, slice<any>([(nint)4, (nint)5]));
    l3.PushFrontList(l1);
    checkList(t, l3, slice<any>([(nint)1, (nint)2, (nint)3, (nint)4, (nint)5]));

    checkList(t, l1, slice<any>([(nint)1, (nint)2, (nint)3]));
    checkList(t, l2, slice<any>([(nint)4, (nint)5]));

    l3 = New();
    l3.PushBackList(l1);
    checkList(t, l3, slice<any>([(nint)1, (nint)2, (nint)3]));
    l3.PushBackList(l3);
    checkList(t, l3, slice<any>([(nint)1, (nint)2, (nint)3, (nint)1, (nint)2, (nint)3]));

    l3 = New();
    l3.PushFrontList(l1);
    checkList(t, l3, slice<any>([(nint)1, (nint)2, (nint)3]));
    l3.PushFrontList(l3);
    checkList(t, l3, slice<any>([(nint)1, (nint)2, (nint)3, (nint)1, (nint)2, (nint)3]));

    l3 = New();
    l1.PushBackList(l3);
    checkList(t, l1, slice<any>([(nint)1, (nint)2, (nint)3]));
    l1.PushFrontList(l3);
    checkList(t, l1, slice<any>([(nint)1, (nint)2, (nint)3]));
}

public static void TestRemove(ж<testing.T> t) {
    var l = New();
    var e1 = l.PushBack((nint)1);
    var e2 = l.PushBack((nint)2);
    checkListPointers(t, l, slice<ж<global::go.container.list_package.Element>>([e1, e2]));
    var e = l.Front();
    l.Remove(e);
    checkListPointers(t, l, slice<ж<global::go.container.list_package.Element>>([e2]));
    l.Remove(e);
    checkListPointers(t, l, slice<ж<global::go.container.list_package.Element>>([e2]));
}

public static void TestIssue4103(ж<testing.T> t) {
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
            t.Errorf("l2.Len() = %d, want 2", n);
        }
    }

    l1.InsertBefore((nint)8, e);
    {
        nint n = l1.Len(); if (n != 3) {
            t.Errorf("l1.Len() = %d, want 3", n);
        }
    }
}

public static void TestIssue6349(ж<testing.T> t) {
    var l = New();
    l.PushBack((nint)1);
    l.PushBack((nint)2);

    var e = l.Front();
    l.Remove(e);
    if (!AreEqual((~e).Value, (nint)1)) {
        t.Errorf("e.value = %d, want 1", (~e).Value);
    }
    if (e.Next() != nil) {
        t.Errorf("e.Next() != nil");
    }
    if (e.Prev() != nil) {
        t.Errorf("e.Prev() != nil");
    }
}

public static void TestMove(ж<testing.T> t) {
    var l = New();
    var e1 = l.PushBack((nint)1);
    var e2 = l.PushBack((nint)2);
    var e3 = l.PushBack((nint)3);
    var e4 = l.PushBack((nint)4);

    l.MoveAfter(e3, e3);
    checkListPointers(t, l, slice<ж<global::go.container.list_package.Element>>([e1, e2, e3, e4]));
    l.MoveBefore(e2, e2);
    checkListPointers(t, l, slice<ж<global::go.container.list_package.Element>>([e1, e2, e3, e4]));

    l.MoveAfter(e3, e2);
    checkListPointers(t, l, slice<ж<global::go.container.list_package.Element>>([e1, e2, e3, e4]));
    l.MoveBefore(e2, e3);
    checkListPointers(t, l, slice<ж<global::go.container.list_package.Element>>([e1, e2, e3, e4]));

    l.MoveBefore(e2, e4);
    checkListPointers(t, l, slice<ж<global::go.container.list_package.Element>>([e1, e3, e2, e4]));
    (e2, e3) = (e3, e2);

    l.MoveBefore(e4, e1);
    checkListPointers(t, l, slice<ж<global::go.container.list_package.Element>>([e4, e1, e2, e3]));
    (e1, e2, e3, e4) = (e4, e1, e2, e3);

    l.MoveAfter(e4, e1);
    checkListPointers(t, l, slice<ж<global::go.container.list_package.Element>>([e1, e4, e2, e3]));
    (e2, e3, e4) = (e4, e2, e3);

    l.MoveAfter(e2, e3);
    checkListPointers(t, l, slice<ж<global::go.container.list_package.Element>>([e1, e3, e2, e4]));
}

// Test PushFront, PushBack, PushFrontList, PushBackList with uninitialized List
public static void TestZeroList(ж<testing.T> t) {
    ж<global::go.container.list_package.List> l1 = @new<global::go.container.list_package.List>();
    l1.PushFront((nint)1);
    checkList(t, l1, slice<any>([(nint)1]));

    ж<global::go.container.list_package.List> l2 = @new<global::go.container.list_package.List>();
    l2.PushBack((nint)1);
    checkList(t, l2, slice<any>([(nint)1]));

    ж<global::go.container.list_package.List> l3 = @new<global::go.container.list_package.List>();
    l3.PushFrontList(l1);
    checkList(t, l3, slice<any>([(nint)1]));

    ж<global::go.container.list_package.List> l4 = @new<global::go.container.list_package.List>();
    l4.PushBackList(l2);
    checkList(t, l4, slice<any>([(nint)1]));
}

// Test that a list l is not modified when calling InsertBefore with a mark that is not an element of l.
public static void TestInsertBeforeUnknownMark(ж<testing.T> t) {
    ref var l = ref heap(new global::go.container.list_package.List(), out var Ꮡl);
    Ꮡl.PushBack((nint)1);
    Ꮡl.PushBack((nint)2);
    Ꮡl.PushBack((nint)3);
    Ꮡl.InsertBefore((nint)1, @new<global::go.container.list_package.Element>());
    checkList(t, Ꮡl, slice<any>([(nint)1, (nint)2, (nint)3]));
}

// Test that a list l is not modified when calling InsertAfter with a mark that is not an element of l.
public static void TestInsertAfterUnknownMark(ж<testing.T> t) {
    ref var l = ref heap(new global::go.container.list_package.List(), out var Ꮡl);
    Ꮡl.PushBack((nint)1);
    Ꮡl.PushBack((nint)2);
    Ꮡl.PushBack((nint)3);
    Ꮡl.InsertAfter((nint)1, @new<global::go.container.list_package.Element>());
    checkList(t, Ꮡl, slice<any>([(nint)1, (nint)2, (nint)3]));
}

// Test that a list l is not modified when calling MoveAfter or MoveBefore with a mark that is not an element of l.
public static void TestMoveUnknownMark(ж<testing.T> t) {
    ref var l1 = ref heap(new global::go.container.list_package.List(), out var Ꮡl1);
    var e1 = Ꮡl1.PushBack((nint)1);

    ref var l2 = ref heap(new global::go.container.list_package.List(), out var Ꮡl2);
    var e2 = Ꮡl2.PushBack((nint)2);

    Ꮡl1.MoveAfter(e1, e2);
    checkList(t, Ꮡl1, slice<any>([(nint)1]));
    checkList(t, Ꮡl2, slice<any>([(nint)2]));

    Ꮡl1.MoveBefore(e1, e2);
    checkList(t, Ꮡl1, slice<any>([(nint)1]));
    checkList(t, Ꮡl2, slice<any>([(nint)2]));
}

} // end list_internal_test_package
