// Copyright 2009 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.
using go;

// Hand-finished conversion (joins sync's Mutex/RWMutex/WaitGroup/Pool/Cond hand-owns). The one
// deviation from the auto conversion is Do's fast path: the emitted `Ꮡo.of(Once.Ꮡdone).Load()`
// minted a fresh ж field box per CALL — with the box Load itself minted, 2 objects/216 B on a path
// Go runs allocation-free, measured by net/textproto's want-zero TestCommonHeaders (L11). Do now
// reads the flag through atomic.Bool's `[GoRecv] ref` receiver form (`o.done.Load()`), which is
// zero-allocation and reads exactly like the Go source. doSlow keeps the emitted shape — it runs
// once per Once instance (plus contended racers), where the field-box cost is irrelevant.
[module: GoManualConversion]

namespace go;

using atomic = sync.atomic_package;
using sync;

partial class sync_package {

// Once is an object that will perform exactly one action.
//
// A Once must not be copied after first use.
//
// In the terminology of [the Go memory model],
// the return from f “synchronizes before”
// the return from any call of once.Do(f).
//
// [the Go memory model]: https://go.dev/ref/mem
[GoType] partial struct Once {
    internal noCopy _;
    // done indicates whether the action has been performed.
    // It is first in the struct because it is used in the hot path.
    // The hot path is inlined at every call site.
    // Placing done first allows more compact instructions on some architectures (amd64/386),
    // and fewer instructions (to calculate offset) on other architectures.
    internal atomic.Bool done;
    internal Mutex m;
}

// Do calls the function f if and only if Do is being called for the
// first time for this instance of [Once]. In other words, given
//
//	var once Once
//
// if once.Do(f) is called multiple times, only the first call will invoke f,
// even if f has a different value in each invocation. A new instance of
// Once is required for each function to execute.
//
// Do is intended for initialization that must be run exactly once. Since f
// is niladic, it may be necessary to use a function literal to capture the
// arguments to a function to be invoked by Do:
//
//	config.once.Do(func() { config.init(filename) })
//
// Because no call to Do returns until the one call to f returns, if f causes
// Do to be called, it will deadlock.
//
// If f panics, Do considers it to have returned; future calls of Do return
// without calling f.
public static void Do(this ж<Once> Ꮡo, Action f) {
    ref var o = ref Ꮡo.DerefOrNull();

    // Note: Here is an incorrect implementation of Do:
    //
    //	if o.done.CompareAndSwap(false, true) {
    //		f()
    //	}
    //
    // Do guarantees that when it returns, f has finished.
    // This implementation would not implement that guarantee:
    // given two simultaneous calls, the winner of the cas would
    // call f, and the second would return immediately, without
    // waiting for the first's call to f to complete.
    // This is why the slow path falls back to a mutex, and why
    // the o.done.Store must be delayed until after f returns.
    if (!o.done.Load()) {
        // Outlined slow-path to allow inlining of the fast-path.
        Ꮡo.doSlow(f);
    }
}

internal static void doSlow(this ж<Once> Ꮡo, Action f) {
    GoFrame ᒐ = default;
    try {
        Ꮡo.of(Once.Ꮡm).Lock();
        defer(Ꮡo.of(Once.Ꮡm).Unlock, ref ᒐ);
        if (!Ꮡo.of(Once.Ꮡdone).Load()) {
            defer(Ꮡo.of(Once.Ꮡdone).Store, (bool)true, ref ᒐ);
            f();
        }
    }
    catch (Exception ᒐex) when (GoFrame.IsPanic(ᒐex, out PanicException? ᒐp)) { GoFrame.Capture(ᒐp); }
    finally { ᒐ.Run(); }
}

} // end sync_package
