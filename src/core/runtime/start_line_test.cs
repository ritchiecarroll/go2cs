// Copyright 2022 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.
namespace go;

using fmt = fmt_package;
using testenv = @internal.testenv_package;
using Δruntime = runtime_package;
using testing = testing_package;
using @internal;
using MethodImplAttribute = global::System.Runtime.CompilerServices.MethodImplAttribute;
using MethodImplOptions = global::System.Runtime.CompilerServices.MethodImplOptions;
using static global::go.runtime_internal_test_package;

partial class runtime_test_package {

// The tests in this file test the function start line metadata included in
// _func and inlinedCall. TestStartLine hard-codes the start lines of functions
// in this file. If code moves, the test will need to be updated.
//
// The "start line" of a function should be the line containing the func
// keyword.
internal static partial nint normalFunc() {
    return callerStartLine(false);
}

internal static partial nint multilineDeclarationFunc() {
    return multilineDeclarationFunc1(0, 0, 0);
}

//go:noinline
internal static partial nint multilineDeclarationFunc1(nint a, nint b, nint c) {
    return callerStartLine(false);
}

internal static partial nint blankLinesFunc() {
    // Some
    // lines
    // without
    // code
    return callerStartLine(false);
}

internal static partial nint inlineFunc() {
    return inlineFunc1();
}

internal static partial nint inlineFunc1() {
    return callerStartLine(true);
}

internal static Func<nint> closureFn;

internal static nint normalClosure() {
    // Assign to global to ensure this isn't inlined.
    closureFn = [MethodImpl(MethodImplOptions.NoInlining)] () => callerStartLine(false);
    return closureFn();
}

internal static nint inlineClosure() {
    return ((Func<nint>)([MethodImpl(MethodImplOptions.NoInlining)] () => {
        return callerStartLine(true);
    }))();
}

internal partial struct TestStartLine_testCases /*dyn*/ {
    internal @string name;
    internal Func<nint> fn;
    internal nint want;
}

public static void TestStartLine(ж<testing.T> Ꮡt) {
    ref var t = ref Ꮡt.DerefOrNull();

    // We test inlined vs non-inlined variants. We can't do that if
    // optimizations are disabled.
    testenv.SkipIfOptimizationOff(new runtime_test_package.testing_TжTB(Ꮡt));
    var testCases = new TestStartLine_testCases[]{
        new(
            name: "normal"u8,
            fn: normalFunc,
            want: 21
        ),
        new(
            name: "multiline-declaration"u8,
            fn: multilineDeclarationFunc,
            want: 30
        ),
        new(
            name: "blank-lines"u8,
            fn: blankLinesFunc,
            want: 35
        ),
        new(
            name: "inline"u8,
            fn: inlineFunc,
            want: 49
        ),
        new(
            name: "normal-closure"u8,
            fn: normalClosure,
            want: 57
        ),
        new(
            name: "inline-closure"u8,
            fn: inlineClosure,
            want: 64
        )
    }.slice();
    foreach (var (_, vᴛ1) in testCases) {
        ref var tc = ref heap(new TestStartLine_testCases(), out var Ꮡtc);
        tc = vᴛ1;

        var tcʗ1 = tc;
        Ꮡt.Run(tc.name, (ж<testing.T> tΔ1) => {
            nint got = tcʗ1.fn();
            if (got != tcʗ1.want) {
                tΔ1.Errorf("start line got %d want %d"u8, got, tcʗ1.want);
            }
        });
    }
}

//go:noinline
internal static partial nint callerStartLine(bool wantInlined) {
    array<uintptr> pcs = new(1);
    nint n = Δruntime.Callers(2, pcs[..]);
    if (n != 1) {
        throw panic(fmt.Sprintf("no caller of callerStartLine? n = %d"u8, n));
    }
    var frames = Δruntime.CallersFrames(pcs[..]);
    ref var frame = ref heap<Δruntime.Frame>(out var Ꮡframe);
    (frame, _) = frames.Next();
    var inlined = frame.Func == nil; // Func always set to nil for inlined frames
    if (wantInlined != inlined) {
        throw panic(fmt.Sprintf("caller %s inlined got %v want %v"u8, frame.Function, inlined, wantInlined));
    }
    return runtime_internal_test_package.FrameStartLine(Ꮡframe);
}

} // end runtime_test_package
