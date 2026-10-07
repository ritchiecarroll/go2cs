// Copyright 2009 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.
namespace go;

using Δruntime = runtime_package;
using strings = strings_package;
using testing = testing_package;
using @unsafe = unsafe_package;
using MethodImplAttribute = global::System.Runtime.CompilerServices.MethodImplAttribute;
using MethodImplOptions = global::System.Runtime.CompilerServices.MethodImplOptions;
using static global::go.runtime_internal_test_package;

partial class runtime_test_package {

public static partial void TestCaller(ж<testing.T> Ꮡt) {
    GoFrame ᒐ = default;
    try {
        nint procs = Δruntime.GOMAXPROCS(-1);
        var c = new channel<bool>(procs);
        for (nint p = 0; p < procs; p++) {
            var cʗ1 = c;
            goǃ(() => {
                for (nint i = 0; i < 1000; i++) {
                    testCallerFoo(Ꮡt);
                }
                cʗ1.ᐸꟷ(true);
            });
            var cʗ2 = c;
            defer(() => {
                ᐸꟷ(cʗ2);
            }, ref ᒐ);
        }
    }
    catch (Exception ᒐex) when (GoFrame.IsPanic(ᒐex, out PanicException? ᒐp)) { GoFrame.Capture(ᒐp); }
    finally { ᒐ.Run(); }
}

// These are marked noinline so that we can use FuncForPC
// in testCallerBar.
//
//go:noinline
internal static partial void testCallerFoo(ж<testing.T> Ꮡt) {
    testCallerBar(Ꮡt);
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static readonly @string symtabTestGoˢ = "symtab_test.go"u8;
internal static readonly @string testCallerBarˢ = "testCallerBar"u8;
internal static readonly @string testCallerFooˢ = "testCallerFoo"u8;

//go:noinline
internal static partial void testCallerBar(ж<testing.T> Ꮡt) {
    ref var t = ref Ꮡt.DerefOrNull();

    for (nint i = 0; i < 2; i++) {
        var (pc, @file, line, ok) = Δruntime.Caller(i);
        var f = Δruntime.FuncForPC(pc);
        if (!ok || !strings.HasSuffix(@file, symtabTestGoˢ) || (i == 0 && !strings.HasSuffix(f.Name(), testCallerBarˢ)) || (i == 1 && !strings.HasSuffix(f.Name(), testCallerFooˢ)) || line < 5 || line > 1000 || f.Entry() >= pc) {
            Ꮡt.Errorf("incorrect symbol info %d: %t %d %d %s %s %d"u8,
                i, ok, f.Entry(), pc, f.Name(), @file, line);
        }
    }
}

internal static partial nint lineNumber() {
    var (_, _, line, _) = Δruntime.Caller(1);
    return line; // return 0 for error
}

// Do not add/remove lines in this block without updating the line numbers.
internal static nint firstLine = lineNumber(); // 0

// 1
internal static nint lineVar1 = lineNumber();  // 2
internal static nint lineVar2a = lineNumber();  // 3
internal static nint lineVar2b = lineNumber();

// 4
// 5
// 7
// 8
// 9
// 10
// 11
// 12
// 13
// 14
// 15
// 16
// 17
// 18

// 19
[GoType("dyn")] partial struct compLitᴛ1 {
    internal nint lineA, lineB; // 6
}
internal static slice<compLitᴛ1> compLit = new compLitᴛ1[]{
    new(
        lineNumber(), lineNumber()
    ),
    new(
        lineNumber(),
        lineNumber()
    ),
    new(
        lineB: lineNumber(),
        lineA: lineNumber()
    )
}.slice();

// 20
// 21
// 22
internal static array<nint> arrayLit = new nint[]{lineNumber(),
    lineNumber(), lineNumber(),
    lineNumber()
}.array();                            // 23

// 24
// 25
// 26
internal static slice<nint> sliceLit = new nint[]{lineNumber(),
    lineNumber(), lineNumber(),
    lineNumber()
}.slice();                   // 27

// 28
// 29
// 30
// 31
// 32
internal static map<nint, nint> mapLit = new map<nint, nint>{
    [29] = lineNumber(),
    [30] = lineNumber(),
    [lineNumber()] = 31,
    [lineNumber()] = 32
};                         // 33

// 34
// 35
internal static nint intLit = lineNumber() + lineNumber() + lineNumber(); // 36

internal static void trythis() {
    // 37
    recordLines(lineNumber(), // 38

        lineNumber(), // 39

        lineNumber()); // 40
}

// Modifications below this line are okay.
internal static nint l38;
internal static nint l39;
internal static nint l40;

internal static void recordLines(nint a, nint b, nint c) {
    l38 = a;
    l39 = b;
    l40 = c;
}

[GoType("dyn")] internal partial struct TestLineNumber_type {
    internal @string name;
    internal nint val;
    internal nint want;
}

public static void TestLineNumber(ж<testing.T> Ꮡt) {
    trythis();
    foreach (var (_, test) in new TestLineNumber_type[]{
        new("firstLine"u8, firstLine, 0),
        new("lineVar1"u8, lineVar1, 2),
        new("lineVar2a"u8, lineVar2a, 3),
        new("lineVar2b"u8, lineVar2b, 3),
        new("compLit[0].lineA"u8, compLit[0].lineA, 9),
        new("compLit[0].lineB"u8, compLit[0].lineB, 9),
        new("compLit[1].lineA"u8, compLit[1].lineA, 12),
        new("compLit[1].lineB"u8, compLit[1].lineB, 13),
        new("compLit[2].lineA"u8, compLit[2].lineA, 17),
        new("compLit[2].lineB"u8, compLit[2].lineB, 16),
        new("arrayLit[0]"u8, arrayLit[0], 20),
        new("arrayLit[1]"u8, arrayLit[1], 21),
        new("arrayLit[2]"u8, arrayLit[2], 21),
        new("arrayLit[3]"u8, arrayLit[3], 22),
        new("sliceLit[0]"u8, sliceLit[0], 24),
        new("sliceLit[1]"u8, sliceLit[1], 25),
        new("sliceLit[2]"u8, sliceLit[2], 25),
        new("sliceLit[3]"u8, sliceLit[3], 26),
        new("mapLit[29]"u8, mapLit[29], 29),
        new("mapLit[30]"u8, mapLit[30], 30),
        new("mapLit[31]"u8, mapLit[31 + firstLine] + firstLine, 31), // nb it's the key not the value

        new("mapLit[32]"u8, mapLit[32 + firstLine] + firstLine, 32), // nb it's the key not the value

        new("intLit"u8, intLit - 2 * firstLine, 34 + 35 + 36),
        new("l38"u8, l38, 38),
        new("l39"u8, l39, 39),
        new("l40"u8, l40, 40)
    }.slice()) {
        {
            nint got = test.val - firstLine; if (got != test.want) {
                Ꮡt.Errorf("%s on firstLine+%d want firstLine+%d (firstLine=%d, val=%d)"u8,
                    test.name, got, test.want, firstLine, test.val);
            }
        }
    }
}

public static void TestNilName(ж<testing.T> Ꮡt) {
    GoFrame ᒐ = default;
    try {
        defer(() => {
            {
                var ex = recover(); if (ex != default!) {
                    Ꮡt.Fatalf("expected no nil panic, got=%v"u8, ex);
                }
            }
        }, ref ᒐ);
        {
            @string got = (((ж<Δruntime.Func>)nil)).Name(); if (got != ""u8) {
                Ꮡt.Errorf("Name() = %q, want %q"u8, got, (@string)""u8);
            }
        }
    }
    catch (Exception ᒐex) when (GoFrame.IsPanic(ᒐex, out PanicException? ᒐp)) { GoFrame.Capture(ᒐp); }
    finally { ᒐ.Run(); }
}

internal static nint dummy;

internal static void inlined() {
    // Side effect to prevent elimination of this entire function.
    dummy = 42;
}

// A function with an InlTree. Returns a PC within the function body.
//
// No inline to ensure this complete function appears in output.
//
//go:noinline
internal static partial uintptr tracebackFunc(ж<testing.T> Ꮡt) {
    // This body must be more complex than a single call to inlined to get
    // an inline tree.
    inlined();
    inlined();
    // Acquire a PC in this function.
    var (pc, _, _, ok) = Δruntime.Caller(0);
    if (!ok) {
        Ꮡt.Fatalf("Caller(0) got ok false, want true"u8);
    }
    return pc;
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static readonly @string tracebackFuncˢ = "tracebackFunc"u8;

// Test that CallersFrames handles PCs in the alignment region between
// functions (int 3 on amd64) without crashing.
//
// Go will never generate a stack trace containing such an address, as it is
// not a valid call site. However, the cgo traceback function passed to
// runtime.SetCgoTraceback may not be completely accurate and may incorrect
// provide PCs in Go code or the alignment region between functions.
//
// Go obviously doesn't easily expose the problematic PCs to running programs,
// so this test is a bit fragile. Some details:
//
//   - tracebackFunc is our target function. We want to get a PC in the
//     alignment region following this function. This function also has other
//     functions inlined into it to ensure it has an InlTree (this was the source
//     of the bug in issue 44971).
//
//   - We acquire a PC in tracebackFunc, walking forwards until FuncForPC says
//     we're in a new function. The last PC of the function according to FuncForPC
//     should be in the alignment region (assuming the function isn't already
//     perfectly aligned).
//
// This is a regression test for issue 44971.
public static void TestFunctionAlignmentTraceback(ж<testing.T> Ꮡt) {
    ref var t = ref Ꮡt.DerefOrNull();

    var pc = tracebackFunc(Ꮡt);
    // Double-check we got the right PC.
    var f = Δruntime.FuncForPC(pc);
    if (!strings.HasSuffix(f.Name(), tracebackFuncˢ)) {
        Ꮡt.Fatalf("Caller(0) = %+v, want tracebackFunc"u8, f.OrTypedNil());
    }
    // Iterate forward until we find a different function. Back up one
    // instruction is (hopefully) an alignment instruction.
    while (Δruntime.FuncForPC(pc) == f) {
        pc++;
    }
    pc--;
    // Is this an alignment region filler instruction? We only check this
    // on amd64 for simplicity. If this function has no filler, then we may
    // get a false negative, but will never get a false positive.
    if (Δruntime.GOARCH == "amd64"u8) {
        var code = ~(ж<uint8>)(uintptr)((@unsafe.Pointer)pc);
        if (code != 0xcc) {
            // INT $3
            Ꮡt.Errorf("PC %v code got %#x want 0xcc"u8, pc, code);
        }
    }
    // Finally ensure that Frames.Next doesn't crash when processing this
    // PC.
    var frames = Δruntime.CallersFrames(new uintptr[]{pc}.slice());
    var (frame, _) = frames.Next();
    if (frame.Func != f) {
        Ꮡt.Errorf("frames.Next() got %+v want %+v"u8, frame.Func.OrTypedNil(), f.OrTypedNil());
    }
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static readonly object failedToLookUpPcˢ = (@string)"failed to look up PC"u8;
internal static readonly @string nameˢ = "Name"u8;
internal static readonly @string entryˢ = "Entry"u8;
internal static readonly object zeroPcˢ = (@string)"zero PC"u8;
internal static readonly @string fileLineˢ = "FileLine"u8;

public static partial void BenchmarkFunc(ж<testing.B> Ꮡb) {
    var (pc, _, _, ok) = Δruntime.Caller(0);
    if (!ok) {
        Ꮡb.Fatal(failedToLookUpPcˢ);
    }
    var f = Δruntime.FuncForPC(pc);
    var fʗ1 = f;
    Ꮡb.Run(nameˢ, (ж<testing.B> bΔ1) => {
        for (nint i = 0; i < (~bΔ1).N; i++) {
            @string name = fʗ1.Name();
            if (name != "runtime_test.BenchmarkFunc"u8) {
                bΔ1.Fatalf("unexpected name %q"u8, name);
            }
        }
    });
    var fʗ2 = f;
    Ꮡb.Run(entryˢ, (ж<testing.B> bΔ2) => {
        for (nint i = 0; i < (~bΔ2).N; i++) {
            var pcΔ1 = fʗ2.Entry();
            if (pcΔ1 == 0) {
                bΔ2.Fatal(zeroPcˢ);
            }
        }
    });
    var fʗ3 = f;
    Ꮡb.Run(fileLineˢ, (ж<testing.B> bΔ3) => {
        for (nint i = 0; i < (~bΔ3).N; i++) {
            var (@file, line) = fʗ3.FileLine(pc);
            if (!strings.HasSuffix(@file, symtabTestGoˢ) || line == 0) {
                bΔ3.Fatalf("unexpected file/line %q:%d"u8, @file, line);
            }
        }
    });
}

} // end runtime_test_package
