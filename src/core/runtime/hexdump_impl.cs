// hexdump_impl.cs - Gbtc
// Copyright © 2026 The go2cs Authors. All rights reserved.
//
// SPDX-License-Identifier: BSD-3-Clause
// Use of this source code is governed by a BSD-style license
// that can be found in the LICENSE file.

// HAND-OWNED companion to hexdump.go -- hexdumpWords, with its symbolizer on the caller records.
//
// Go's hexdumpWords annotates every aligned word of a dump that is the pc of a function:
//
//     fn := findfunc(val)
//     if fn.valid() { hm.start(); print("<", funcname(fn), "+", hex(val-fn.entry()), ">\n") }
//
// findfunc, funcname and funcInfo.entry read the linker-built pclntab, which this host does not have
// (symtab.cs's firstmoduledata is an empty stub), so fn.valid() was always false and the annotation line
// was never written: TestHexdumpWords read the dump without its two "^ <runtime_test.TestHexdumpWords+0x0>"
// lines. A pc here is an opaque token (managed_impl.cs), so the word is symbolized through the same records
// FuncForPC and runtime.Callers read -- the function's Go spelling and the entry of the span holding the
// token, as printDebugLogPC does (debuglog_impl.cs). A word that names no function this host minted prints
// no annotation, as Go's does for a pc in no function.
//
// Everything else is the converted body: hexdumper, hexdumpMarker and their methods stay in hexdump.cs.
[module: go.GoManualConversion]

namespace go;

using @unsafe = unsafe_package;
using @internal;

partial class runtime_package {

// hexdumpWords prints a word-oriented hex dump of [p, p+len).
//
// If mark != nil, it will be passed to hexdumper.mark.
internal static void hexdumpWords(uintptr Δp, uintptr len, Action<uintptr, hexdumpMarker> mark) {
    printlock();

    // Provide a default annotation
    var symMark = (uintptr u, hexdumpMarker hm) => {
        if (mark != default!) {
            mark(u, hm);
        }

        // Can we symbolize this value?
        var val = ~(ж<uintptr>)(uintptr)((@unsafe.Pointer)u);
        if (managedFuncName(val) is { Length: > 0 } name) {
            hm.start();
            print((@string)"<"u8, (@string)name, (@string)"+"u8, ((Δhex)(uint64)(val - frameEntry(val))), (@string)">\n"u8);
        }
    };

    var h = new hexdumper(addr: Δp, mark: symMark);
    h.write(@unsafe.Slice((ж<byte>)(uintptr)((@unsafe.Pointer)Δp), len));
    h.close();
    printunlock();
}

} // end runtime_package
