// hand_impl.cs - Gbtc
// Copyright © 2026 The go2cs Authors. All rights reserved.
//
// Use of this source code is governed by an MIT-style license
// that can be found in the LICENSE file.

// The hand-written half of S2 arm 3 (see main.go). This file has no .go counterpart, so a transpile never
// regenerates it. It stands in for code outside Go that calls back into it on a thread of its own: a C# host
// (the M:N record's M3) or a C library's callback thread. The thread is a plain System.Threading.Thread, so the
// scheduler has never seen it: no carrier, no P, no goroutine. Under Go, main.go's fallback runs f on a
// goroutine locked to its thread instead, which is what a cgo callback runs on.
namespace go;

partial class main_package {

[System.Runtime.CompilerServices.ModuleInitializer]
internal static void ForeignInit() {
    foreign = f => {
        System.Threading.Thread thread = new(() => f()) { IsBackground = true, Name = "foreign" };
        thread.Start();
    };
}

} // end main_package
