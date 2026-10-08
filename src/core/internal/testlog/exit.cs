// Copyright 2020 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.
namespace go.@internal;

using sync = go.sync_package;
// blank import: unsafe_package (side effects only; no using emitted — a `using _` alias hijacks C# discards) // for linkname
using go;

partial class testlog_package {

// PanicOnExit0 reports whether to panic on a call to os.Exit(0).
// This is in the testlog package because, like other definitions in
// package testlog, it is a hook between the testing package and the
// os package. This is used to ensure that an early call to os.Exit(0)
// does not cause a test to pass.
public static bool PanicOnExit0() {
    GoFrame ᒐ = default;
    try {
        ᏑpanicOnExit0.of(panicOnExit0ᴛ1.Ꮡmu).Lock();
        defer(ᏑpanicOnExit0.of(panicOnExit0ᴛ1.Ꮡmu).Unlock, ref ᒐ);
        return panicOnExit0.val;
    }
    catch (Exception ᒐex) when (GoFrame.IsPanic(ᒐex, out PanicException? ᒐp)) { GoFrame.Capture(ᒐp); return default!; }
    finally { ᒐ.Run(); }
}

// panicOnExit0 is the flag used for PanicOnExit0. This uses a lock
// because the value can be cleared via a timer call that may race
// with calls to os.Exit

partial struct panicOnExit0ᴛ1 /*dyn*/ {
    internal sync.Mutex mu;
    internal bool val;
}
internal static ж<panicOnExit0ᴛ1> ᏑpanicOnExit0 = new StandardBox<panicOnExit0ᴛ1>(default(panicOnExit0ᴛ1));
internal static ref panicOnExit0ᴛ1 panicOnExit0 => ref ᏑpanicOnExit0.Value;

// SetPanicOnExit0 sets panicOnExit0 to v.
//
// SetPanicOnExit0 should be an internal detail,
// but alternate implementations of go test in other
// build systems may need to access it using linkname.
//
// Do not remove or change the type signature.
// See go.dev/issue/67401.
//
//go:linkname SetPanicOnExit0
public static void SetPanicOnExit0(bool v) {
    GoFrame ᒐ = default;
    try {
        ᏑpanicOnExit0.of(panicOnExit0ᴛ1.Ꮡmu).Lock();
        defer(ᏑpanicOnExit0.of(panicOnExit0ᴛ1.Ꮡmu).Unlock, ref ᒐ);
        panicOnExit0.val = v;
    }
    catch (Exception ᒐex) when (GoFrame.IsPanic(ᒐex, out PanicException? ᒐp)) { GoFrame.Capture(ᒐp); }
    finally { ᒐ.Run(); }
}

} // end testlog_package
