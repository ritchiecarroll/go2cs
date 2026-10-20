// Copyright 2024 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.

// Package synctest provides support for testing concurrent code.
//
// See the testing/synctest package for function documentation.
namespace go.@internal;

using abi = go.@internal.abi_package;
using @unsafe = unsafe_package;
using go.@internal;

partial class synctest_package {

//go:linkname Run
public static partial void Run(Action f);

//go:linkname Wait
public static partial void Wait();

// IsInBubble reports whether the current goroutine is in a bubble.
//
//go:linkname IsInBubble
public static partial bool IsInBubble();

// Association is the state of a pointer's bubble association.
partial struct Association /*num:nint*/;

public static Association Unbubbled => /* Association(iota) */ 0; // not associated with any bubble
public static Association CurrentBubble => 1;   // associated with the current bubble
public static Association OtherBubble => 2;     // associated with a different bubble

// Associate attempts to associate p with the current bubble.
// It returns the new association status of p.
public static Association Associate<T>(ж<T> Ꮡp) {
    // Ensure p escapes to permit us to attach a special to it.
    var escapedP = abi.Escape(Ꮡp);
    return (Association)associate(@unsafe.Pointer.FromPinnedBox(escapedP));
}

//go:linkname associate
internal static partial nint associate(@unsafe.Pointer p);

// Disassociate disassociates p from any bubble.
public static void Disassociate<T>(ж<T> Ꮡp) {
    disassociate(@unsafe.Pointer.FromPinnedBox(Ꮡp));
}

//go:linkname disassociate
internal static partial void disassociate(@unsafe.Pointer b);

// IsAssociated reports whether p is associated with the current bubble.
public static bool IsAssociated<T>(ж<T> Ꮡp) {
    return isAssociated(@unsafe.Pointer.FromPinnedBox(Ꮡp));
}

//go:linkname isAssociated
internal static partial bool isAssociated(@unsafe.Pointer p);

//go:linkname acquire
internal static partial any acquire();

//go:linkname release
internal static partial void release(any _);

//go:linkname inBubble
internal static partial void inBubble(any _Δp0, Action _Δp1);

// A Bubble is a synctest bubble.
//
// Not a public API. Used by syscall/js to propagate bubble membership through syscalls.
partial struct Bubble {
    internal any b;
}

// Acquire returns a reference to the current goroutine's bubble.
// The bubble will not become idle until Release is called.
public static ж<Bubble> Acquire() {
    {
        var b = acquire(); if (b != default!) {
            return Ꮡ(new Bubble(b));
        }
    }
    return default!;
}

// Release releases the reference to the bubble,
// allowing it to become idle again.
public static void Release(this ж<Bubble> Ꮡb) {
    ref var b = ref Ꮡb.DerefOrNull();

    if (Ꮡb == nil) {
        return;
    }
    release(b.b);
    b.b = default!;
}

// Run executes f in the bubble.
// The current goroutine must not be part of a bubble.
public static void Run(this ж<Bubble> Ꮡb, Action f) {
    ref var b = ref Ꮡb.DerefOrNull();

    if (Ꮡb == nil){
        f();
    } else {
        inBubble(b.b, f);
    }
}

} // end synctest_package
