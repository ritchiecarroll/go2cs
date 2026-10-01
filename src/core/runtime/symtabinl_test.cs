// Copyright 2023 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.
namespace go;

using abi = @internal.abi_package;
using sys = @internal.runtime.sys_package;
using stringslite = @internal.stringslite_package;
using @internal;
using @internal.runtime;
using static global::go.runtime_package;

partial class runtime_internal_test_package {

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static readonly object skippingTestWithInliningˢ = (@string)"skipping test with inlining optimizations disabled"u8;
internal static readonly @string tiuˢ = "tiu"u8;

// Hoisted Go string constant (single allocation; Go keeps it in RODATA)
internal static readonly @string wantFileᶜ = "symtabinl_test.go"u8;

// Hoisted Go string constant (single allocation; Go keeps it in RODATA)
internal static readonly @string namePrefixᶜ = "runtime."u8;

public static void XTestInlineUnwinder(TestingT t) {
    if (TestenvOptimizationOff()) {
        t.Skip(skippingTestWithInliningˢ);
    }
    var pc1 = abi.FuncPCABIInternal(tiuTest);
    var f = findfunc(pc1);
    if (!f.valid()) {
        t.Fatalf("failed to resolve tiuTest at PC %#x"u8, pc1);
    }
    var want = new map<@string, nint>{
        ["tiuInlined1:3 tiuTest:10"u8] = 0,
        ["tiuInlined1:3 tiuInlined2:6 tiuTest:11"u8] = 0,
        ["tiuInlined2:7 tiuTest:11"u8] = 0,
        ["tiuTest:12"u8] = 0
    };
    var wantStart = new map<@string, nint>{
        ["tiuInlined1"u8] = 2,
        ["tiuInlined2"u8] = 5,
        ["tiuTest"u8] = 9
    };
    // Iterate over the PCs in tiuTest and walk the inline stack for each.
    @string prevStack = "x"u8;
    for (var pc = pc1; pc < pc1 + 1024 && findfunc(pc) == f; pc += sys.PCQuantum) {
        @string Δstack = ""u8;
        var (u, uf) = newInlineUnwinder(f, pc);
        {
            var (@file, _) = u.fileLine(uf); if (@file == "?"u8) {
                // We're probably in the trailing function padding, where findfunc
                // still returns f but there's no symbolic information. Just keep
                // going until we definitely hit the end. If we see a "?" in the
                // middle of unwinding, that's a real problem.
                //
                // TODO: If we ever have function end information, use that to make
                // this robust.
                continue;
            }
        }
        for (; uf.valid(); uf = u.next(uf)) {
            var (@file, line) = u.fileLine(uf);
            @string wantFile = wantFileᶜ;
            if (!stringslite.HasSuffix(@file, wantFile)) {
                t.Errorf("tiuTest+%#x: want file ...%s, got %s"u8, pc - pc1, wantFile, @file);
            }
            var sf = u.srcFunc(uf);
            @string name = sf.name();
            @string namePrefix = namePrefixᶜ;
            if (stringslite.HasPrefix(name, namePrefix)) {
                name = name[(int)(len(namePrefix))..];
            }
            if (!stringslite.HasPrefix(name, tiuˢ)) {
                t.Errorf("tiuTest+%#x: unexpected function %s"u8, pc - pc1, name);
            }
            nint start = (nint)sf.startLine - tiuStart;
            if (start != wantStart[name]) {
                t.Errorf("tiuTest+%#x: want startLine %d, got %d"u8, pc - pc1, wantStart[name], start);
            }
            if (sf.funcID != abi.FuncIDNormal) {
                t.Errorf("tiuTest+%#x: bad funcID %v"u8, pc - pc1, sf.funcID);
            }
            if (len(Δstack) > 0) {
                Δstack += " "u8;
            }
            Δstack += FmtSprintf("%s:%d"u8, name, line - tiuStart);
        }
        if (Δstack != prevStack) {
            prevStack = Δstack;
            t.Logf("tiuTest+%#x: %s"u8, pc - pc1, Δstack);
            {
                var (_, ok) = want[Δstack, ꟷ]; if (ok) {
                    want[Δstack]++;
                }
            }
        }
    }
    // Check that we got all the stacks we wanted.
    foreach (var (Δstack, count) in want) {
        if (count == 0) {
            t.Errorf("missing stack %s"u8, Δstack);
        }
    }
}

internal static nint lineNumber() {
    var (_, _, line, _) = Caller(1);
    return line; // return 0 for error
}

// Below here is the test data for XTestInlineUnwinder
internal static nint tiuStart;
internal static void initᴛtiuStart() { tiuStart = lineNumber(); } // +0

internal static nint tiu2;     // +1
internal static nint tiu3;

internal static void tiuInlined1(nint i) {
    // +2
    tiu1[i]++; // +3
}

// +4
internal static void tiuInlined2() {
    // +5
    tiuInlined1(1); // +6
    tiu2++; // +7
}

// +8
internal static void tiuTest() {
    // +9
    tiuInlined1(0); // +10
    tiuInlined2(); // +11
    tiu3++; // +12
}

// +13
internal static array<nint> tiu1 = new(2); // +14

} // end runtime_internal_test_package
