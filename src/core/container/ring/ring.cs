// Copyright 2009 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.

// Package ring implements operations on circular lists.
namespace go.container;

using static ring_package;

partial class ring_package {

// A Ring is an element of a circular list, or ring.
// Rings do not have a beginning or end; a pointer to any ring element
// serves as reference to the entire ring. Empty rings are represented
// as nil Ring pointers. The zero value for a Ring is a one-element
// ring with a nil Value.
partial struct Ring {
    internal ж<Ring> next, prev;
    public any Value; // for use by client; untouched by this library
}

} partial class ring_package {

[GoMethodBody("init")] internal static ж<Ring> initᴍ(ж<Ring> Ꮡr) {
    ref var r = ref Ꮡr.DerefOrNull();

    r.next = Ꮡr;
    r.prev = Ꮡr;
    return Ꮡr;
}

// Next returns the next ring element. r must not be empty.
[GoMethodBody("Next")] internal static ж<Ring> Nextᴍ(ж<Ring> Ꮡr) {
    ref var r = ref Ꮡr.DerefOrNull();

    if (r.next == nil) {
        return Ꮡr.init();
    }
    return r.next;
}

// Prev returns the previous ring element. r must not be empty.
[GoMethodBody("Prev")] internal static ж<Ring> Prevᴍ(ж<Ring> Ꮡr) {
    ref var r = ref Ꮡr.DerefOrNull();

    if (r.next == nil) {
        return Ꮡr.init();
    }
    return r.prev;
}

// Move moves n % r.Len() elements backward (n < 0) or forward (n >= 0)
// in the ring and returns that ring element. r must not be empty.
[GoMethodBody("Move")] internal static ж<Ring> Moveᴍ(ж<Ring> Ꮡr, nint n) {
    ref var r = ref Ꮡr.DerefOrNull();

    if (r.next == nil) {
        return Ꮡr.init();
    }
    switch (ᐧ) {
    case {} when n is < 0: {
        for (; n < 0; n++) {
            Ꮡr = r.prev; r = ref Ꮡr.DerefOrNull();
        }
        break;
    }
    case {} when n is > 0: {
        for (; n > 0; n--) {
            Ꮡr = r.next; r = ref Ꮡr.DerefOrNull();
        }
        break;
    }}
    return Ꮡr;
}

} partial class ring_package {

// New creates a ring of n elements.
public static ж<Ring> New(nint n) {
    if (n <= 0) {
        return default;
    }
    var r = @new<Ring>();
    var p = r;
    for (nint i = 1; i < n; i++) {
        p.next = Ꮡ(new Ring(prev: p));
        p = p.next;
    }
    p.next = r;
    r.prev = p;
    return r;
}

} partial class ring_package {

// Link connects ring r with ring s such that r.Next()
// becomes s and returns the original value for r.Next().
// r must not be empty.
//
// If r and s point to the same ring, linking
// them removes the elements between r and s from the ring.
// The removed elements form a subring and the result is a
// reference to that subring (if no elements were removed,
// the result is still the original value for r.Next(),
// and not nil).
//
// If r and s point to different rings, linking
// them creates a single ring with the elements of s inserted
// after r. The result points to the element following the
// last element of s after insertion.
[GoMethodBody("Link")] internal static ж<Ring> Linkᴍ(ж<Ring> Ꮡr, ж<Ring> Ꮡs) {
    ref var r = ref Ꮡr.DerefOrNull();
    ref var s = ref Ꮡs.DerefOrNull();

    var n = Ꮡr.Next();
    if (Ꮡs != nil) {
        var p = Ꮡs.Prev();
        // Note: Cannot use multiple assignment because
        // evaluation order of LHS is not specified.
        r.next = Ꮡs;
        s.prev = Ꮡr;
        n.prev = p;
        p.next = n;
    }
    return n;
}

// Unlink removes n % r.Len() elements from the ring r, starting
// at r.Next(). If n % r.Len() == 0, r remains unchanged.
// The result is the removed subring. r must not be empty.
[GoMethodBody("Unlink")] internal static ж<Ring> Unlinkᴍ(ж<Ring> Ꮡr, nint n) {
    if (n <= 0) {
        return default;
    }
    return Ꮡr.Link(Ꮡr.Move(n + 1));
}

// Len computes the number of elements in ring r.
// It executes in time proportional to the number of elements.
[GoMethodBody("Len")] internal static nint Lenᴍ(ж<Ring> Ꮡr) {
    nint n = 0;
    if (Ꮡr != nil) {
        n = 1;
        for (var p = Ꮡr.Next(); p != Ꮡr; p = p.next) {
            n++;
        }
    }
    return n;
}

// Do calls function f on each element of the ring, in forward order.
// The behavior of Do is undefined if f changes *r.
[GoMethodBody("Do")] internal static void Doᴍ(ж<Ring> Ꮡr, Action<any> f) {
    ref var r = ref Ꮡr.DerefOrNull();

    if (Ꮡr != nil) {
        f(r.Value);
        for (var p = Ꮡr.Next(); p != Ꮡr; p = p.next) {
            f((~p).Value);
        }
    }
}

}
