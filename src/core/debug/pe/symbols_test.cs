// Copyright 2022 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.
namespace go.debug;

using fmt = fmt_package;
using testing = testing_package;
using static go.debug.pe_package;

partial class pe_internal_test_package {

internal partial struct testpoint {
    internal @string name;
    internal bool ok;
    internal @string err;
    internal @string auxstr;
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static ref readonly @string testdataLlvmMingwˢ => ref testdataLlvmMingwˢᵇ;
private static readonly @string testdataLlvmMingwˢᵇ = "testdata/llvm-mingw-20211002-msvcrt-x86_64-crt2"u8;

public static void TestReadCOFFSymbolAuxInfo(ж<testing.T> t) {
    GoFrame ᒐ = default;
    try {
        var testpoints = new map<nint, testpoint>{
            [39] = new testpoint(
                name: ".rdata$.refptr.__native_startup_lock",
                ok: true,
                auxstr: "{Size:8 NumRelocs:1 NumLineNumbers:0 Checksum:0 SecNum:16 Selection:2 _:[0 0 0]}"
            ),
            [81] = new testpoint(
                name: ".debug_line",
                ok: true,
                auxstr: "{Size:994 NumRelocs:1 NumLineNumbers:0 Checksum:1624223678 SecNum:32 Selection:0 _:[0 0 0]}"
            ),
            [155] = new testpoint(
                name: ".file",
                ok: false,
                err: "incorrect symbol storage class"
            )
        };

        // The testdata PE object file below was selected from a release
        // build from https://github.com/mstorsjo/llvm-mingw/releases; it
        // corresponds to the mingw "crt2.o" object. The object itself was
        // built using an x86_64 HOST=linux TARGET=windows clang cross
        // compiler based on LLVM 13. More build details can be found at
        // https://github.com/mstorsjo/llvm-mingw/releases.
        var (f, err) = Open(testdataLlvmMingwˢ);
        if (err != nil) {
            t.Errorf("open failed with %v", err);
        }
        var fʗ1 = f;
        defer(() => fʗ1.Close(), ref ᒐ);
        foreach (var k in range(f.COFFSymbols)) {
            var (tp, ok) = testpoints[k, ꟷ];
            if (!ok) {
                continue;
            }
            var sym = Ꮡ(f.COFFSymbols, k);
            if (sym.NumberOfAuxSymbols == 0) {
                t.Errorf("expected aux symbols for sym %d", k);
                continue;
            }
            var (name, nerr) = sym.FullName(f.StringTable);
            if (nerr != nil) {
                t.Errorf("FullName(%d) failed with %v", k, nerr);
                continue;
            }
            if (name != tp.name) {
                t.Errorf("name check for %d, got %s want %s", k, name, tp.name);
                continue;
            }
            var (ap, errΔ1) = f.COFFSymbolReadSectionDefAux(k);
            if (tp.ok) {
                if (errΔ1 != nil) {
                    t.Errorf("unexpected failure on %d, got error %v", k, errΔ1);
                    continue;
                }
                @string got = fmt.Sprintf("%+v"u8, ap.Value);
                if (got != tp.auxstr) {
                    t.Errorf("COFFSymbolReadSectionDefAux on %d bad return, got:\n%s\nwant:\n%s\n", k, got, tp.auxstr);
                    continue;
                }
            } else {
                if (errΔ1 == nil) {
                    t.Errorf("unexpected non-failure on %d", k);
                    continue;
                }
                @string got = fmt.Sprintf("%v"u8, errΔ1);
                if (got != tp.err) {
                    t.Errorf("COFFSymbolReadSectionDefAux %d wrong error, got %q want %q", k, got, tp.err);
                    continue;
                }
            }
        }
    }
    catch (Exception ᒐex) when (GoFrame.IsPanic(ᒐex, out PanicException? ᒐp)) { GoFrame.Capture(ᒐp); }
    finally { ᒐ.Run(); }
}

} // end pe_internal_test_package
