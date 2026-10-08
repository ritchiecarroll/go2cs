// Copyright 2024 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.

// Package synctest provides support for testing concurrent code.
//
// See the testing/synctest package for function documentation.
namespace go.@internal;

// blank import: unsafe_package (side effects only; no using emitted — a `using _` alias hijacks C# discards) // for go:linkname

partial class synctest_package {

//go:linkname Run
public static partial void Run(Action f);

//go:linkname Wait
public static partial void Wait();

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
