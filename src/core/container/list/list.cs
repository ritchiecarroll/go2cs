// Copyright 2009 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.

// Package list implements a doubly linked list.
//
// To iterate over a list (where l is a *List):
//
//	for e := l.Front(); e != nil; e = e.Next() {
//		// do something with e.Value
//	}
namespace go.container;

partial class list_package {

// Element is an element of a linked list.
partial struct Element {
    // Next and previous pointers in the doubly-linked list of elements.
    // To simplify the implementation, internally a list l is implemented
    // as a ring, such that &l.root is both the next element of the last
    // list element (l.Back()) and the previous element of the first list
    // element (l.Front()).
    internal ж<Element> next, prev;
    // The list to which this element belongs.
    internal ж<List> list;
    // The value stored with this element.
    public any Value;
}

// Next returns the next list element or nil.
public static ж<Element> Next(this ref Element e) {
    {
        var p = e.next; if (e.list != nil && p != e.list.of(List.Ꮡroot)) {
            return p;
        }
    }
    return default!;
}

// Prev returns the previous list element or nil.
public static ж<Element> Prev(this ref Element e) {
    {
        var p = e.prev; if (e.list != nil && p != e.list.of(List.Ꮡroot)) {
            return p;
        }
    }
    return default!;
}

// List represents a doubly linked list.
// The zero value for List is an empty list ready to use.
partial struct List {
    internal Element root; // sentinel list element, only &root, root.prev, and root.next are used
    internal nint len;    // current list length excluding (this) sentinel element
}

// Init initializes or clears list l.
public static ж<List> Init(this ж<List> Ꮡl) {
    ref var l = ref Ꮡl.DerefOrNull();

    l.root.next = Ꮡl.of(List.Ꮡroot);
    l.root.prev = Ꮡl.of(List.Ꮡroot);
    l.len = 0;
    return Ꮡl;
}

// New returns an initialized list.
public static ж<List> New() {
    return @new<List>().Init();
}

// Len returns the number of elements of list l.
// The complexity is O(1).
public static nint Len(this ref List l) {
    return l.len;
}

// Front returns the first element of list l or nil if the list is empty.
public static ж<Element> Front(this ref List l) {
    if (l.len == 0) {
        return default!;
    }
    return l.root.next;
}

// Back returns the last element of list l or nil if the list is empty.
public static ж<Element> Back(this ref List l) {
    if (l.len == 0) {
        return default!;
    }
    return l.root.prev;
}

// lazyInit lazily initializes a zero List value.
internal static void lazyInit(this ж<List> Ꮡl) {
    ref var l = ref Ꮡl.DerefOrNull();

    if (l.root.next == nil) {
        Ꮡl.Init();
    }
}

// insert inserts e after at, increments l.len, and returns e.
internal static ж<Element> insert(this ж<List> Ꮡl, ж<Element> Ꮡe, ж<Element> Ꮡat) {
    ref var l = ref Ꮡl.DerefOrNull();
    ref var e = ref Ꮡe.DerefOrNull();
    ref var at = ref Ꮡat.DerefOrNull();

    e.prev = Ꮡat;
    e.next = at.next;
    e.prev.Value.next = Ꮡe;
    e.next.Value.prev = Ꮡe;
    e.list = Ꮡl;
    l.len++;
    return Ꮡe;
}

// insertValue is a convenience wrapper for insert(&Element{Value: v}, at).
internal static ж<Element> insertValue(this ж<List> Ꮡl, any v, ж<Element> Ꮡat) {
    return Ꮡl.insert(Ꮡ(new Element(Value: v)), Ꮡat);
}

// remove removes e from its list, decrements l.len
internal static void remove(this ref List l, ж<Element> Ꮡe) {
    ref var e = ref Ꮡe.DerefOrNull();

    e.prev.Value.next = e.next;
    e.next.Value.prev = e.prev;
    e.next = default!; // avoid memory leaks
    e.prev = default!; // avoid memory leaks
    e.list = default!;
    l.len--;
}

// move moves e to next to at.
internal static void move(this ref List l, ж<Element> Ꮡe, ж<Element> Ꮡat) {
    ref var e = ref Ꮡe.DerefOrNull();
    ref var at = ref Ꮡat.DerefOrNull();

    if (Ꮡe == Ꮡat) {
        return;
    }
    e.prev.Value.next = e.next;
    e.next.Value.prev = e.prev;
    e.prev = Ꮡat;
    e.next = at.next;
    e.prev.Value.next = Ꮡe;
    e.next.Value.prev = Ꮡe;
}

// Remove removes e from l if e is an element of list l.
// It returns the element value e.Value.
// The element must not be nil.
public static any Remove(this ж<List> Ꮡl, ж<Element> Ꮡe) {
    ref var l = ref Ꮡl.DerefOrNull();
    ref var e = ref Ꮡe.DerefOrNull();

    if (e.list == Ꮡl) {
        // if e.list == l, l must have been initialized when e was inserted
        // in l or l == nil (e is a zero Element) and l.remove will crash
        l.remove(Ꮡe);
    }
    return e.Value;
}

// PushFront inserts a new element e with value v at the front of list l and returns e.
public static ж<Element> PushFront(this ж<List> Ꮡl, any v) {
    Ꮡl.lazyInit();
    return Ꮡl.insertValue(v, Ꮡl.of(List.Ꮡroot));
}

// PushBack inserts a new element e with value v at the back of list l and returns e.
public static ж<Element> PushBack(this ж<List> Ꮡl, any v) {
    ref var l = ref Ꮡl.DerefOrNull();

    Ꮡl.lazyInit();
    return Ꮡl.insertValue(v, l.root.prev);
}

// InsertBefore inserts a new element e with value v immediately before mark and returns e.
// If mark is not an element of l, the list is not modified.
// The mark must not be nil.
public static ж<Element> InsertBefore(this ж<List> Ꮡl, any v, ж<Element> Ꮡmark) {
    ref var l = ref Ꮡl.DerefOrNull();
    ref var mark = ref Ꮡmark.DerefOrNull();

    if (mark.list != Ꮡl) {
        return default!;
    }
    // see comment in List.Remove about initialization of l
    return Ꮡl.insertValue(v, mark.prev);
}

// InsertAfter inserts a new element e with value v immediately after mark and returns e.
// If mark is not an element of l, the list is not modified.
// The mark must not be nil.
public static ж<Element> InsertAfter(this ж<List> Ꮡl, any v, ж<Element> Ꮡmark) {
    ref var l = ref Ꮡl.DerefOrNull();
    ref var mark = ref Ꮡmark.DerefOrNull();

    if (mark.list != Ꮡl) {
        return default!;
    }
    // see comment in List.Remove about initialization of l
    return Ꮡl.insertValue(v, Ꮡmark);
}

// MoveToFront moves element e to the front of list l.
// If e is not an element of l, the list is not modified.
// The element must not be nil.
public static void MoveToFront(this ж<List> Ꮡl, ж<Element> Ꮡe) {
    ref var l = ref Ꮡl.DerefOrNull();
    ref var e = ref Ꮡe.DerefOrNull();

    if (e.list != Ꮡl || l.root.next == Ꮡe) {
        return;
    }
    // see comment in List.Remove about initialization of l
    l.move(Ꮡe, Ꮡl.of(List.Ꮡroot));
}

// MoveToBack moves element e to the back of list l.
// If e is not an element of l, the list is not modified.
// The element must not be nil.
public static void MoveToBack(this ж<List> Ꮡl, ж<Element> Ꮡe) {
    ref var l = ref Ꮡl.DerefOrNull();
    ref var e = ref Ꮡe.DerefOrNull();

    if (e.list != Ꮡl || l.root.prev == Ꮡe) {
        return;
    }
    // see comment in List.Remove about initialization of l
    l.move(Ꮡe, l.root.prev);
}

// MoveBefore moves element e to its new position before mark.
// If e or mark is not an element of l, or e == mark, the list is not modified.
// The element and mark must not be nil.
public static void MoveBefore(this ж<List> Ꮡl, ж<Element> Ꮡe, ж<Element> Ꮡmark) {
    ref var l = ref Ꮡl.DerefOrNull();
    ref var e = ref Ꮡe.DerefOrNull();
    ref var mark = ref Ꮡmark.DerefOrNull();

    if (e.list != Ꮡl || Ꮡe == Ꮡmark || mark.list != Ꮡl) {
        return;
    }
    l.move(Ꮡe, mark.prev);
}

// MoveAfter moves element e to its new position after mark.
// If e or mark is not an element of l, or e == mark, the list is not modified.
// The element and mark must not be nil.
public static void MoveAfter(this ж<List> Ꮡl, ж<Element> Ꮡe, ж<Element> Ꮡmark) {
    ref var l = ref Ꮡl.DerefOrNull();
    ref var e = ref Ꮡe.DerefOrNull();
    ref var mark = ref Ꮡmark.DerefOrNull();

    if (e.list != Ꮡl || Ꮡe == Ꮡmark || mark.list != Ꮡl) {
        return;
    }
    l.move(Ꮡe, Ꮡmark);
}

// PushBackList inserts a copy of another list at the back of list l.
// The lists l and other may be the same. They must not be nil.
public static void PushBackList(this ж<List> Ꮡl, ж<List> Ꮡother) {
    ref var l = ref Ꮡl.DerefOrNull();
    ref var other = ref Ꮡother.DerefOrNull();

    Ꮡl.lazyInit();
    for ((nint i, var e) = (other.Len(), other.Front()); i > 0; (i, e) = (i - 1, e.Next())) {
        Ꮡl.insertValue((~e).Value, l.root.prev);
    }
}

// PushFrontList inserts a copy of another list at the front of list l.
// The lists l and other may be the same. They must not be nil.
public static void PushFrontList(this ж<List> Ꮡl, ж<List> Ꮡother) {
    ref var other = ref Ꮡother.DerefOrNull();

    Ꮡl.lazyInit();
    for ((nint i, var e) = (other.Len(), other.Back()); i > 0; (i, e) = (i - 1, e.Prev())) {
        Ꮡl.insertValue((~e).Value, Ꮡl.of(List.Ꮡroot));
    }
}

} // end list_package
