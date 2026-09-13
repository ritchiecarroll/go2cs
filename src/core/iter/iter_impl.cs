// iter_impl.cs - Gbtc
// Copyright © 2026 The go2cs Authors. All rights reserved.
//
// SPDX-License-Identifier: BSD-3-Clause
// Use of this source code is governed by a BSD-style license
// that can be found in the LICENSE file.

// Hand-written implementations of iter's two //go:linkname runtime primitives, newcoro and
// coroswitch. In Go these are provided by the runtime (coro.go); go2cs emits them as bodyless
// `partial` methods, and without a body here the PartialStubGenerator fills each with a throwing
// stub — so iter.Pull and iter.Pull2 raise NotImplementedException on first use, taking every
// pull-style iterator in the corpus with them.
//
// EVERYTHING ELSE IN iter CONVERTS FAITHFULLY, and that is the whole reason this file is four
// methods rather than a rewrite of the package. Pull's real content — the yield closure, the
// yieldNext handshake, the done latch, the deferred recover that turns a panic or a Goexit in the
// sequence function into a panicValue the pulling side re-raises — is ordinary Go, and the
// conversion of it in iter.cs is line-for-line faithful, panic texts included. Only the control
// TRANSFER underneath it has no managed counterpart. So the seam is drawn exactly there: supply the
// transfer, change nothing else, and let the converted code keep being the specification.
//
// The transfer itself is golib's (go.golib.Coro) rather than this package's, because it is a
// runtime capability and not an iter concept — Go declares it in runtime/coro.go, and the corpus
// carries the mechanically converted, permanently dead counterpart at runtime/coro.cs, whose body
// bottoms out in mcall/getg/newproc1/gogo stubs that throw. Read Coro.cs for the model; this file is
// only the binding.
//
// WHY A SIDE TABLE. iter declares `type coro struct{}` — an EMPTY struct, deliberately opaque, whose
// only job is to be a token the two linkname'd functions agree on. There is nowhere in it to put a
// managed rendezvous, and widening it is the wrong move twice over: the converted `[GoType] partial
// struct coro` mirrors Go's field set (which is empty), and a Go zero-size type is a shape golib
// classifies and treats specially (GoZeroSizeFacts). Keying on the BOX instead leaves the converted
// type exactly as Go declares it. `new(coro)` mints a fresh StandardBox per call (builtin.@new), and
// ж<T> boxes compare by IDENTITY — the same property sync's semaphore table relies on for ж<uint32>
// keys — so one Pull's token can never collide with another's. A ConditionalWeakTable rather than a
// ConcurrentDictionary because the entry must not outlive the token: a program that creates pull
// iterators in a loop would otherwise leak one rendezvous per iteration for the life of the process,
// which is the bounded leak sync/runtime_impl.cs documents and this has no reason to repeat.
//
// Hand-owned: there is no iter_impl.go, so a -stdlib reconvert never regenerates this file. Nothing
// is registered in manualConversionFuncs for it either — newcoro and coroswitch have no Go BODY to
// suppress (they are linkname declarations), so the converter already emits them as bodyless
// partials and this file simply supplies the implementing halves.

using System;
using System.Runtime.CompilerServices;
using go.golib;

[module: go.GoManualConversion]

namespace go;

partial class iter_package
{
    // The token → rendezvous binding. See "WHY A SIDE TABLE" above.
    private static readonly ConditionalWeakTable<ж<coro>, Coro> coroTable = new();

    // newcoro creates a new coro containing a goroutine blocked waiting to run f, and returns that
    // coro. The goroutine EXISTS when this returns — registered and counted — but has not started:
    // Coro.Start performs the creation handshake before returning, and iter's own tests pin both
    // halves of that (one extra goroutine on the statement after Pull returns; a sequence function
    // that panics on entry must not panic until something pulls from it).
    internal static partial ж<coro> newcoro(Action<ж<coro>> f)
    {
        ж<coro> c = @new<coro>();

        // f is handed the same token this returns, exactly as Go hands the coro to its own body —
        // that is how Pull's yield closure reaches coroswitch for the return trip.
        coroTable.Add(c, Coro.Start(() => f(c)));

        return c;
    }

    // coroswitch switches to the goroutine blocked on c and then blocks the current goroutine on c.
    internal static partial void coroswitch(ж<coro> c)
    {
        if (!coroTable.TryGetValue(c, out Coro? coro))
        {
            // Unreachable from iter: every token reaching here came from newcoro above, and the
            // table entry lives as long as the token does. A coro value that never passed through
            // newcoro is the caller's error, and Go's own coroswitch is equally unforgiving about
            // it (it throws on a nil c.gp).
            throw new InvalidOperationException("coro: coroswitch on a coro that was never created");
        }

        coro.Switch();
    }
}
