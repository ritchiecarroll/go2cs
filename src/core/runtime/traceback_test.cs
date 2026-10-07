// Copyright 2021 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.
namespace go;

using bytes = bytes_package;
using fmt = fmt_package;
using abi = @internal.abi_package;
using testenv = @internal.testenv_package;
using Δregexp = regexp_package;
using Δruntime = runtime_package;
using Δdebug = global::go.runtime.debug_package;
using strconv = strconv_package;
using strings = strings_package;
using Δsync = sync_package;
using testing = testing_package;
// blank import: unsafe_package (side effects only; no using emitted — a `using _` alias hijacks C# discards)
using @internal;
using MethodImplAttribute = global::System.Runtime.CompilerServices.MethodImplAttribute;
using MethodImplOptions = global::System.Runtime.CompilerServices.MethodImplOptions;
using global::go.runtime;
using static global::go.runtime_internal_test_package;
using ꓸꓸꓸany = Span<any>;
using ꓸꓸꓸstring = Span<@string>;

partial class runtime_test_package {

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static readonly @string endˢ2 = "<end>"u8;
internal static readonly @string runtimeTestTtiSimple3ˢ = "runtime_test.ttiSimple3(...)"u8;
internal static readonly @string runtimeTestTtiSimple2ˢ = "runtime_test.ttiSimple2(...)"u8;
internal static readonly @string runtimeTestTtiSimple1ˢ = "runtime_test.ttiSimple1()"u8;
internal static readonly @string runtimeTestTtiSigpanic1ˢ = "runtime_test.ttiSigpanic1.func1()"u8;
internal static readonly @string runtimeTestTtiSigpanic3ˢ = "runtime_test.ttiSigpanic3(...)"u8;
internal static readonly @string runtimeTestTtiSigpanic2ˢ = "runtime_test.ttiSigpanic2(...)"u8;
internal static readonly @string runtimeTestTtiSigpanic1ˢ2 = "runtime_test.ttiSigpanic1()"u8;
internal static readonly @string wrapperˢ = "wrapper"u8;
internal static readonly @string runtimeTestTtiWrapperM1ˢ = "runtime_test.ttiWrapper.m1(...)"u8;
internal static readonly @string runtimeTestTtiWrapper1ˢ = "runtime_test.ttiWrapper1()"u8;
internal static readonly @string excludedˢ = "excluded"u8;
internal static readonly @string runtimeTestTtiExcluded3ˢ = "runtime_test.ttiExcluded3(...)"u8;
internal static readonly @string runtimeTestTtiExcluded1ˢ = "runtime_test.ttiExcluded1()"u8;

// Test traceback printing of inlined frames.
public static void TestTracebackInlined(ж<testing.T> Ꮡt) {
    testenv.SkipIfOptimizationOff(new runtime_test_package.testing_TжTB(Ꮡt)); // This test requires inlining
    void check(ж<testing.T> tΔ1, ж<ttiResult> r, params ꓸꓸꓸstring funcsʗp) {
        var funcs = funcsʗp.sslice();
        tΔ1.Helper();
        // Check the printed traceback.
        var frames = parseTraceback1(tΔ1, (~r).printed).Value.frames;
        tΔ1.Log((~r).printed);
        // Find ttiLeaf
        while (len(frames) > 0 && (~frames[0]).funcName != "runtime_test.ttiLeaf"u8) {
            frames = frames[1..];
        }
        if (len(frames) == 0) {
            tΔ1.Errorf("missing runtime_test.ttiLeaf"u8);
            return;
        }
        frames = frames[1..];
        // Check the function sequence.
        foreach (var (i, want) in funcs) {
            @string got = endˢ2;
            if (i < len(frames)) {
                got = frames[i].Value.funcName;
                if (strings.HasSuffix(want, ")"u8)) {
                    got += "("u8 + (~frames[i]).args + ")"u8;
                }
            }
            if (got != want) {
                tΔ1.Errorf("got %s, want %s"u8, got, want);
                return;
            }
        }
    }
    var checkʗ1 = check;
    Ꮡt.Run(simpleˢ, (ж<testing.T> tΔ2) => {
        // Check a simple case of inlining
        var r = ttiSimple1();
        checkʗ1(tΔ2, r, runtimeTestTtiSimple3ˢ, runtimeTestTtiSimple2ˢ, runtimeTestTtiSimple1ˢ);
    });
    var checkʗ2 = check;
    Ꮡt.Run(sigpanicˢ, (ж<testing.T> tΔ3) => {
        // Check that sigpanic from an inlined function prints correctly
        var r = ttiSigpanic1();
        checkʗ2(tΔ3, r, runtimeTestTtiSigpanic1ˢ, panicˢ, runtimeTestTtiSigpanic3ˢ, runtimeTestTtiSigpanic2ˢ, runtimeTestTtiSigpanic1ˢ2);
    });
    var checkʗ3 = check;
    Ꮡt.Run(wrapperˢ, (ж<testing.T> tΔ4) => {
        // Check that a method inlined into a wrapper prints correctly
        var r = ttiWrapper1();
        checkʗ3(tΔ4, r, runtimeTestTtiWrapperM1ˢ, runtimeTestTtiWrapper1ˢ);
    });
    var checkʗ4 = check;
    Ꮡt.Run(excludedˢ, (ж<testing.T> tΔ5) => {
        // Check that when F -> G is inlined and F is excluded from stack
        // traces, G still appears.
        var r = ttiExcluded1();
        checkʗ4(tΔ5, r, runtimeTestTtiExcluded3ˢ, runtimeTestTtiExcluded1ˢ);
    });
}

[GoType] partial struct ttiResult {
    internal @string printed;
}

//go:noinline
internal static partial ж<ttiResult> ttiLeaf() {
    // Get a printed stack trace.
    ref var printed = ref heap<@string>(out var Ꮡprinted);
    printed = ((@string)Δdebug.Stack());
    return Ꮡ(new ttiResult(printed));
}

//go:noinline
internal static partial ж<ttiResult> ttiSimple1() {
    return ttiSimple2();
}

internal static partial ж<ttiResult> ttiSimple2() {
    return ttiSimple3();
}

internal static partial ж<ttiResult> ttiSimple3() {
    return ttiLeaf();
}

//go:noinline
internal static partial ж<ttiResult> /*res*/ ttiSigpanic1() {
    ж<ttiResult> res = default!;
    GoFrame ᒐ = default;
    try {
        defer(() => {
            res = ttiLeaf();
            recover();
        }, ref ᒐ);
        ttiSigpanic2();
        // without condition below the inliner might decide to de-prioritize
        // the callsite above (since it would be on an "always leads to panic"
        // path).
        if (alwaysTrue) {
            throw panic("did not panic");
        }
        res = default!;
    }
    catch (Exception ᒐex) when (GoFrame.IsPanic(ᒐex, out PanicException? ᒐp)) { GoFrame.Capture(ᒐp); }
    finally { ᒐ.Run(); }
    return res;
}

internal static void ttiSigpanic2() {
    ttiSigpanic3();
}

internal static void ttiSigpanic3() {
    ж<nint> p = default!;
    p.Value = 3;
}

internal static bool alwaysTrue = true;

//go:noinline
internal static partial ж<ttiResult> ttiWrapper1() {
    ref var w = ref heap(new ttiWrapper(), out var Ꮡw);
    var m = ((Func<ж<ttiWrapper>, ж<ttiResult>>)([GoWrapper("(*ttiWrapper).m1")] (p0) => m1(panicwrapRecv(p0, "value method runtime_test.ttiWrapper.m1 called using nil *ttiWrapper pointer").Value)));
    return m(Ꮡw);
}

[GoType] partial struct ttiWrapper {
}

internal static partial ж<ttiResult> m1(this ttiWrapper w) {
    return ttiLeaf();
}

//go:noinline
internal static partial ж<ttiResult> ttiExcluded1() {
    return ttiExcluded2();
}

// ttiExcluded2 should be excluded from tracebacks. There are
// various ways this could come up. Linking it to a "runtime." name is
// rather synthetic, but it's easy and reliable. See issue #42754 for
// one way this happened in real code.
//
//go:linkname ttiExcluded2 runtime.ttiExcluded2
//go:noinline
internal static partial ж<ttiResult> ttiExcluded2() {
    return ttiExcluded3();
}

internal static partial ж<ttiResult> ttiExcluded3() {
    return ttiLeaf();
}

internal static array<byte> testTracebackArgsBuf = new(1000);

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static readonly @string runtimeDebugStackˢ = "runtime/debug.Stack"u8;
internal static readonly @string runtimeTestTteStackˢ = "runtime_test.tteStack"u8;

public static void TestTracebackElision(ж<testing.T> Ꮡt) {
    ref var t = ref Ꮡt.DerefOrNull();

    // Test printing exactly the maximum number of frames to make sure we don't
    // print any "elided" message, eliding exactly 1 so we have to pick back up
    // in the paused physical frame, and eliding 10 so we have to advance the
    // physical frame forward.
    foreach (var (_, elided) in new nint[]{0, 1, 10}.slice()) {
        Ꮡt.Run(fmt.Sprintf("elided=%d"u8, elided), [MethodImpl(MethodImplOptions.NoInlining)] (ж<testing.T> tΔ1) => {
            nint n = elided + (nint)runtime_internal_test_package.TracebackInnerFrames + (nint)runtime_internal_test_package.TracebackOuterFrames;
            // Start a new goroutine so we have control over the whole stack.
            var stackChan = new channel<@string>(0);
            goǃ(tteStack, n, stackChan.WithDirection(GoChanDir.Send));
            @string stack = ᐸꟷ(stackChan);
            var tb = parseTraceback1(tΔ1, stack);
            // Check the traceback.
            nint i = 0;
            while (i < n) {
                if (len((~tb).frames) == 0) {
                    tΔ1.Errorf("traceback ended early"u8);
                    break;
                }
                var fr = (~tb).frames[0];
                if (i == runtime_internal_test_package.TracebackInnerFrames && elided > 0){
                    // This should be an "elided" frame.
                    if ((~fr).elided != elided) {
                        tΔ1.Errorf("want %d frames elided"u8, elided);
                        break;
                    }
                    i += fr.Value.elided;
                } else {
                    @string want = fmt.Sprintf("runtime_test.tte%d"u8, (i + 1) % 5);
                    if (i == 0){
                        want = runtimeDebugStackˢ;
                    } else 
                    if (i == n - 1) {
                        want = runtimeTestTteStackˢ;
                    }
                    if ((~fr).funcName != want) {
                        tΔ1.Errorf("want %s, got %s"u8, want, (~fr).funcName);
                        break;
                    }
                    i++;
                }
                tb.Value.frames = (~tb).frames[1..];
            }
            if (!tΔ1.Failed() && len((~tb).frames) > 0) {
                tΔ1.Errorf("got %d more frames than expected"u8, len((~tb).frames));
            }
            if (tΔ1.Failed()) {
                tΔ1.Logf("traceback diverged at frame %d"u8, i);
                nint off = len(stack);
                if (len((~tb).frames) > 0) {
                    off = (~tb).frames[0].Value.off;
                }
                tΔ1.Logf("traceback before error:\n%s"u8, stack.slice(0, off));
                tΔ1.Logf("traceback after error:\n%s"u8, stack.slice(off));
            }
        });
    }
}

// tteStack creates a stack of n logical frames and sends the traceback to
// stack. It cycles through 5 logical frames per physical frame to make it
// unlikely that any part of the traceback will end on a physical boundary.
internal static void tteStack(nint n, channel/*<-*/<@string> stack) {
    n--; // Account for this frame
    // This is basically a Duff's device for starting the inline stack in the
    // right place so we wind up at tteN when n%5=N.
    switch (n % 5) {
    case 0: {
        stack.ᐸꟷ(tte0(n));
        break;
    }
    case 1: {
        stack.ᐸꟷ(tte1(n));
        break;
    }
    case 2: {
        stack.ᐸꟷ(tte2(n));
        break;
    }
    case 3: {
        stack.ᐸꟷ(tte3(n));
        break;
    }
    case 4: {
        stack.ᐸꟷ(tte4(n));
        break;
    }
    default: {
        throw panic("unreachable");
        break;
    }}

}

internal static partial @string tte0(nint n) {
    return tte4(n - 1);
}

internal static partial @string tte1(nint n) {
    return tte0(n - 1);
}

internal static partial @string tte2(nint n) {
    // tte2 opens n%5 == 2 frames. It's also the base case of the recursion,
    // since we can open no fewer than two frames to call debug.Stack().
    if (n < 2) {
        throw panic("bad n");
    }
    if (n == 2) {
        return ((@string)Δdebug.Stack());
    }
    return tte1(n - 1);
}

internal static partial @string tte3(nint n) {
    return tte2(n - 1);
}

internal static partial @string tte4(nint n) {
    return tte3(n - 1);
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static readonly @string testTracebackArgs90x1ˢ = "testTracebackArgs9(0x1, 0xffffffff?, 0x3, 0xff?, {0x5, 0x6}, 0x7)"u8;
internal static readonly @string testTracebackArgs90x10x2ˢ = "testTracebackArgs9(0x1, 0x2, 0x3, 0x4, {0x5, 0x6}, 0x7)"u8;
internal static readonly @string testTracebackArgs10ˢ = "testTracebackArgs10(0xffffffff?, 0xffffffff?, 0xffffffff?, 0xffffffff?, 0xffffffff?)"u8;
internal static readonly @string testTracebackArgs100x1ˢ = "testTracebackArgs10(0x1, 0x2, 0x3, 0x4, 0x5)"u8;
internal static readonly @string testTracebackArgs11aˢ = "testTracebackArgs11a(0xffffffff?, 0xffffffff?, 0xffffffff?)"u8;
internal static readonly @string testTracebackArgs11a0x1ˢ = "testTracebackArgs11a(0x1, 0x2, 0x3)"u8;
internal static readonly @string testTracebackArgs11bˢ = "testTracebackArgs11b(0xffffffff?, 0xffffffff?, 0x3?, 0x4)"u8;
internal static readonly @string testTracebackArgs11b0x1ˢ = "testTracebackArgs11b(0x1, 0x2, 0x3, 0x4)"u8;

[GoType("dyn")] internal partial struct TestTracebackArgs_tests {
    internal Func<nint> fn;
    internal @string expect;
}

[GoType("dyn")] internal partial struct TestTracebackArgs_b {
    internal nint a, b, c;
    internal array<nint> x = new(2);
}

[GoType("dyn")] internal partial struct TestTracebackArgs_x {
    internal nint x;
    internal array<nint> y = new(0);
    internal array<array<nint>> z = new(2, () => new(0));
}

public static void TestTracebackArgs(ж<testing.T> Ꮡt) {
    if (flagQuick.Value) {
        Ꮡt.Skip(quickˢ);
    }
    var optimized = !testenv.OptimizationOff();
    @string abiSel(@string x, @string y) {
        // select expected output based on ABI
        // In noopt build we always spill arguments so the output is the same as stack ABI.
        if (optimized && abi.IntArgRegs > 0) {
            return x;
        }
        return y;
    }
    var tests = new TestTracebackArgs_tests[]{ // simple ints

        new(
            [MethodImpl(MethodImplOptions.NoInlining)] () => testTracebackArgs1(1, 2, 3, 4, 5),
            "testTracebackArgs1(0x1, 0x2, 0x3, 0x4, 0x5)"u8
        ), // some aggregates

        new(
            [MethodImpl(MethodImplOptions.NoInlining)] () => testTracebackArgs2(false, new TestTracebackArgs_b(1, 2, 3, new nint[]{4, 5}.array()), new nint[]{}.array(), new byte[]{6, 7, 8}.array()),
            "testTracebackArgs2(0x0, {0x1, 0x2, 0x3, {0x4, 0x5}}, {}, {0x6, 0x7, 0x8})"u8
        ),
        new(
            [MethodImpl(MethodImplOptions.NoInlining)] () => testTracebackArgs3(new byte[]{1, 2, 3}.array(), 4, 5, 6, new byte[]{7, 8, 9}.array()),
            "testTracebackArgs3({0x1, 0x2, 0x3}, 0x4, 0x5, 0x6, {0x7, 0x8, 0x9})"u8
        ), // too deeply nested type

        new(
            [MethodImpl(MethodImplOptions.NoInlining)] () => testTracebackArgs4(false, new array<array<array<array<array<array<array<array<array<nint>>>>>>>>>[]{}.array(1, () => new(1, () => new(1, () => new(1, () => new(1, () => new(1, () => new(1, () => new(1, () => new(1, () => new(1))))))))))),
            "testTracebackArgs4(0x0, {{{{{...}}}}})"u8
        ), // a lot of zero-sized type

        new(
            () => {
                var z = new nint[]{}.array();
                return testTracebackArgs5(false, new TestTracebackArgs_x(1, z.Clone(), new array<nint>[]{}.array(2, () => new(0))), z, z, z, z, z, z, z, z, z, z, z, z);
            },
            "testTracebackArgs5(0x0, {0x1, {}, {{}, {}}}, {}, {}, {}, {}, {}, ...)"u8
        ), // edge cases for ...
 // no ... for 10 args

        new(
            [MethodImpl(MethodImplOptions.NoInlining)] () => testTracebackArgs6a(1, 2, 3, 4, 5, 6, 7, 8, 9, 10),
            "testTracebackArgs6a(0x1, 0x2, 0x3, 0x4, 0x5, 0x6, 0x7, 0x8, 0x9, 0xa)"u8
        ), // has ... for 11 args

        new(
            [MethodImpl(MethodImplOptions.NoInlining)] () => testTracebackArgs6b(1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 11),
            "testTracebackArgs6b(0x1, 0x2, 0x3, 0x4, 0x5, 0x6, 0x7, 0x8, 0x9, 0xa, ...)"u8
        ), // no ... for aggregates with 10 words

        new(
            [MethodImpl(MethodImplOptions.NoInlining)] () => testTracebackArgs7a(new nint[]{1, 2, 3, 4, 5, 6, 7, 8, 9, 10}.array()),
            "testTracebackArgs7a({0x1, 0x2, 0x3, 0x4, 0x5, 0x6, 0x7, 0x8, 0x9, 0xa})"u8
        ), // has ... for aggregates with 11 words

        new(
            [MethodImpl(MethodImplOptions.NoInlining)] () => testTracebackArgs7b(new nint[]{1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 11}.array()),
            "testTracebackArgs7b({0x1, 0x2, 0x3, 0x4, 0x5, 0x6, 0x7, 0x8, 0x9, 0xa, ...})"u8
        ), // no ... for aggregates, but with more args

        new(
            [MethodImpl(MethodImplOptions.NoInlining)] () => testTracebackArgs7c(new nint[]{1, 2, 3, 4, 5, 6, 7, 8, 9, 10}.array(), 11),
            "testTracebackArgs7c({0x1, 0x2, 0x3, 0x4, 0x5, 0x6, 0x7, 0x8, 0x9, 0xa}, ...)"u8
        ), // has ... for aggregates and also for more args

        new(
            [MethodImpl(MethodImplOptions.NoInlining)] () => testTracebackArgs7d(new nint[]{1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 11}.array(), 12),
            "testTracebackArgs7d({0x1, 0x2, 0x3, 0x4, 0x5, 0x6, 0x7, 0x8, 0x9, 0xa, ...}, ...)"u8
        ), // nested aggregates, no ...

        new(
            [MethodImpl(MethodImplOptions.NoInlining)] () => testTracebackArgs8a(new testArgsType8a(1, 2, 3, 4, 5, 6, 7, 8, new nint[]{9, 10}.array())),
            "testTracebackArgs8a({0x1, 0x2, 0x3, 0x4, 0x5, 0x6, 0x7, 0x8, {0x9, 0xa}})"u8
        ), // nested aggregates, ... in inner but not outer

        new(
            [MethodImpl(MethodImplOptions.NoInlining)] () => testTracebackArgs8b(new testArgsType8b(1, 2, 3, 4, 5, 6, 7, 8, new nint[]{9, 10, 11}.array())),
            "testTracebackArgs8b({0x1, 0x2, 0x3, 0x4, 0x5, 0x6, 0x7, 0x8, {0x9, 0xa, ...}})"u8
        ), // nested aggregates, ... in outer but not inner

        new(
            [MethodImpl(MethodImplOptions.NoInlining)] () => testTracebackArgs8c(new testArgsType8c(1, 2, 3, 4, 5, 6, 7, 8, new nint[]{9, 10}.array(), 11)),
            "testTracebackArgs8c({0x1, 0x2, 0x3, 0x4, 0x5, 0x6, 0x7, 0x8, {0x9, 0xa}, ...})"u8
        ), // nested aggregates, ... in both inner and outer

        new(
            [MethodImpl(MethodImplOptions.NoInlining)] () => testTracebackArgs8d(new testArgsType8d(1, 2, 3, 4, 5, 6, 7, 8, new nint[]{9, 10, 11}.array(), 12)),
            "testTracebackArgs8d({0x1, 0x2, 0x3, 0x4, 0x5, 0x6, 0x7, 0x8, {0x9, 0xa, ...}, ...})"u8
        ), // Register argument liveness.
 // 1, 3 are used and live, 2, 4 are dead (in register ABI).
 // Address-taken (7) and stack ({5, 6}) args are always live.

        new(
            () => {
                poisonStack(); // poison arg area to make output deterministic
                return testTracebackArgs9(1, 2, 3, 4, new nint[]{5, 6}.array(), 7);
            },
            abiSel(
                testTracebackArgs90x1ˢ,
                testTracebackArgs90x10x2ˢ)
        ), // No live.
 // (Note: this assume at least 5 int registers if register ABI is used.)

        new(
            () => {
                poisonStack(); // poison arg area to make output deterministic
                return testTracebackArgs10(1, 2, 3, 4, 5);
            },
            abiSel(
                testTracebackArgs10ˢ,
                testTracebackArgs100x1ˢ)
        ), // Conditional spills.
 // Spill in conditional, not executed.

        new(
            () => {
                poisonStack(); // poison arg area to make output deterministic
                return testTracebackArgs11a(1, 2, 3);
            },
            abiSel(
                testTracebackArgs11aˢ,
                testTracebackArgs11a0x1ˢ)
        ), // 2 spills in conditional, not executed; 3 spills in conditional, executed, but not statically known.
 // So print 0x3?.

        new(
            () => {
                poisonStack(); // poison arg area to make output deterministic
                return testTracebackArgs11b(1, 2, 3, 4);
            },
            abiSel(
                testTracebackArgs11bˢ,
                testTracebackArgs11b0x1ˢ)
        ), // Make sure spilled slice data pointers are spilled to the right location
 // to ensure we see it listed without a ?.
 // See issue 64414.

        new(
            () => {
                poisonStack();
                return testTracebackArgsSlice(testTracebackArgsSliceBackingStore[..]);
            }, // Note: capacity of the slice might be junk, as it is not used.

            fmt.Sprintf("testTracebackArgsSlice({%p, 0x2, "u8, ᏑtestTracebackArgsSliceBackingStore.at<nint>(0))
        )
    }.slice();
    foreach (var (_, test) in tests) {
        nint n = test.fn();
        var got = testTracebackArgsBuf.slice(0, n);
        if (!bytes.Contains(got, slice<byte>(test.expect))) {
            Ꮡt.Errorf("traceback does not contain expected string: want %q, got\n%s"u8, test.expect, got);
        }
    }
}

//go:noinline
internal static partial nint testTracebackArgs1(nint a, nint b, nint c, nint d, nint e) {
    nint n = Δruntime.Stack(testTracebackArgsBuf[..], false);
    if (a < 0) {
        // use in-reg args to keep them alive
        return a + b + c + d + e;
    }
    return n;
}

//go:noinline
internal static partial nint testTracebackArgs2(bool a, TestTracebackArgs_b b, [GoArrayDims(0)] array<nint> _, [GoArrayDims(3)] array<byte> d) {
    b = b.ΔClone();
    d = d.Clone();

    nint n = Δruntime.Stack(testTracebackArgsBuf[..], false);
    if (a) {
        // use in-reg args to keep them alive
        return b.a + b.b + b.c + b.x[0] + b.x[1] + (nint)d[0] + (nint)d[1] + (nint)d[2];
    }
    return n;
}

//go:noinline
//go:registerparams
internal static partial nint testTracebackArgs3([GoArrayDims(3)] array<byte> x, nint a, nint b, nint c, [GoArrayDims(3)] array<byte> y) {
    x = x.Clone();
    y = y.Clone();

    nint n = Δruntime.Stack(testTracebackArgsBuf[..], false);
    if (a < 0) {
        // use in-reg args to keep them alive
        return (nint)x[0] + (nint)x[1] + (nint)x[2] + a + b + c + (nint)y[0] + (nint)y[1] + (nint)y[2];
    }
    return n;
}

//go:noinline
internal static partial nint testTracebackArgs4(bool a, [GoArrayDims(1, 1, 1, 1, 1, 1, 1, 1, 1, 1)] array<array<array<array<array<array<array<array<array<array<nint>>>>>>>>>> x) {
    x = x.Clone();

    nint n = Δruntime.Stack(testTracebackArgsBuf[..], false);
    if (a) {
        throw panic(x); // use args to keep them alive
    }
    return n;
}

//go:noinline
internal static partial nint testTracebackArgs5(bool a, TestTracebackArgs_x x, [GoArrayDims(0)] array<nint> _Δp2, [GoArrayDims(0)] array<nint> _Δp3, [GoArrayDims(0)] array<nint> _Δp4, [GoArrayDims(0)] array<nint> _Δp5, [GoArrayDims(0)] array<nint> _Δp6, [GoArrayDims(0)] array<nint> _Δp7, [GoArrayDims(0)] array<nint> _Δp8, [GoArrayDims(0)] array<nint> _Δp9, [GoArrayDims(0)] array<nint> _Δp10, [GoArrayDims(0)] array<nint> _Δp11, [GoArrayDims(0)] array<nint> _Δp12, [GoArrayDims(0)] array<nint> _Δp13) {
    x = x.ΔClone();

    nint n = Δruntime.Stack(testTracebackArgsBuf[..], false);
    if (a) {
        throw panic(x); // use args to keep them alive
    }
    return n;
}

//go:noinline
internal static partial nint testTracebackArgs6a(nint a, nint b, nint c, nint d, nint e, nint f, nint g, nint h, nint i, nint j) {
    nint n = Δruntime.Stack(testTracebackArgsBuf[..], false);
    if (a < 0) {
        // use in-reg args to keep them alive
        return a + b + c + d + e + f + g + h + i + j;
    }
    return n;
}

//go:noinline
internal static partial nint testTracebackArgs6b(nint a, nint b, nint c, nint d, nint e, nint f, nint g, nint h, nint i, nint j, nint k) {
    nint n = Δruntime.Stack(testTracebackArgsBuf[..], false);
    if (a < 0) {
        // use in-reg args to keep them alive
        return a + b + c + d + e + f + g + h + i + j + k;
    }
    return n;
}

//go:noinline
internal static partial nint testTracebackArgs7a([GoArrayDims(10)] array<nint> a) {
    a = a.Clone();

    nint n = Δruntime.Stack(testTracebackArgsBuf[..], false);
    if (a[0] < 0) {
        // use in-reg args to keep them alive
        return a[1] + a[2] + a[3] + a[4] + a[5] + a[6] + a[7] + a[8] + a[9];
    }
    return n;
}

//go:noinline
internal static partial nint testTracebackArgs7b([GoArrayDims(11)] array<nint> a) {
    a = a.Clone();

    nint n = Δruntime.Stack(testTracebackArgsBuf[..], false);
    if (a[0] < 0) {
        // use in-reg args to keep them alive
        return a[1] + a[2] + a[3] + a[4] + a[5] + a[6] + a[7] + a[8] + a[9] + a[10];
    }
    return n;
}

//go:noinline
internal static partial nint testTracebackArgs7c([GoArrayDims(10)] array<nint> a, nint b) {
    a = a.Clone();

    nint n = Δruntime.Stack(testTracebackArgsBuf[..], false);
    if (a[0] < 0) {
        // use in-reg args to keep them alive
        return a[1] + a[2] + a[3] + a[4] + a[5] + a[6] + a[7] + a[8] + a[9] + b;
    }
    return n;
}

//go:noinline
internal static partial nint testTracebackArgs7d([GoArrayDims(11)] array<nint> a, nint b) {
    a = a.Clone();

    nint n = Δruntime.Stack(testTracebackArgsBuf[..], false);
    if (a[0] < 0) {
        // use in-reg args to keep them alive
        return a[1] + a[2] + a[3] + a[4] + a[5] + a[6] + a[7] + a[8] + a[9] + a[10] + b;
    }
    return n;
}

[GoType] partial struct testArgsType8a {
    internal nint a, b, c, d, e, f, g, h;
    internal array<nint> i = new(2);
}

[GoType] partial struct testArgsType8b {
    internal nint a, b, c, d, e, f, g, h;
    internal array<nint> i = new(3);
}

[GoType] partial struct testArgsType8c {
    internal nint a, b, c, d, e, f, g, h;
    internal array<nint> i = new(2);
    internal nint j;
}

[GoType] partial struct testArgsType8d {
    internal nint a, b, c, d, e, f, g, h;
    internal array<nint> i = new(3);
    internal nint j;
}

//go:noinline
internal static partial nint testTracebackArgs8a(testArgsType8a a) {
    a = a.ΔClone();

    nint n = Δruntime.Stack(testTracebackArgsBuf[..], false);
    if (a.a < 0) {
        // use in-reg args to keep them alive
        return a.b + a.c + a.d + a.e + a.f + a.g + a.h + a.i[0] + a.i[1];
    }
    return n;
}

//go:noinline
internal static partial nint testTracebackArgs8b(testArgsType8b a) {
    a = a.ΔClone();

    nint n = Δruntime.Stack(testTracebackArgsBuf[..], false);
    if (a.a < 0) {
        // use in-reg args to keep them alive
        return a.b + a.c + a.d + a.e + a.f + a.g + a.h + a.i[0] + a.i[1] + a.i[2];
    }
    return n;
}

//go:noinline
internal static partial nint testTracebackArgs8c(testArgsType8c a) {
    a = a.ΔClone();

    nint n = Δruntime.Stack(testTracebackArgsBuf[..], false);
    if (a.a < 0) {
        // use in-reg args to keep them alive
        return a.b + a.c + a.d + a.e + a.f + a.g + a.h + a.i[0] + a.i[1] + a.j;
    }
    return n;
}

//go:noinline
internal static partial nint testTracebackArgs8d(testArgsType8d a) {
    a = a.ΔClone();

    nint n = Δruntime.Stack(testTracebackArgsBuf[..], false);
    if (a.a < 0) {
        // use in-reg args to keep them alive
        return a.b + a.c + a.d + a.e + a.f + a.g + a.h + a.i[0] + a.i[1] + a.i[2] + a.j;
    }
    return n;
}

// nosplit to avoid preemption or morestack spilling registers.
//
//go:nosplit
//go:noinline
internal static partial nint testTracebackArgs9(int64 a, int32 b, int16 c, int8 d, [GoArrayDims(2)] array<nint> x, nint yʗp) {
    x = x.Clone();

    ref var y = ref heap(yʗp, out var Ꮡy);
    if (a < 0) {
        println(Ꮡy); // take address, make y live, even if no longer used at traceback
    }
    nint n = Δruntime.Stack(testTracebackArgsBuf[..], false);
    if (a < 0) {
        // use half of in-reg args to keep them alive, the other half are dead
        return (nint)a + (nint)c;
    }
    return n;
}

// nosplit to avoid preemption or morestack spilling registers.
//
//go:nosplit
//go:noinline
internal static partial nint testTracebackArgs10(int32 a, int32 b, int32 c, int32 d, int32 e) {
    // no use of any args
    return Δruntime.Stack(testTracebackArgsBuf[..], false);
}

// norace to avoid race instrumentation changing spill locations.
// nosplit to avoid preemption or morestack spilling registers.
//
//go:norace
//go:nosplit
//go:noinline
internal static partial nint testTracebackArgs11a(int32 a, int32 b, int32 c) {
    if (a < 0) {
        println(a, b, c); // spill in a conditional, may not execute
    }
    if (b < 0) {
        return (nint)(a + b + c);
    }
    return Δruntime.Stack(testTracebackArgsBuf[..], false);
}

// norace to avoid race instrumentation changing spill locations.
// nosplit to avoid preemption or morestack spilling registers.
//
//go:norace
//go:nosplit
//go:noinline
internal static partial nint testTracebackArgs11b(int32 a, int32 b, int32 c, int32 d) {
    int32 x = default!;
    if (a < 0){
        builtin.print(); // spill b in a conditional
        x = b;
    } else {
        builtin.print(); // spill c in a conditional
        x = c;
    }
    if (d < 0) {
        // d is always needed
        return (nint)(x + d);
    }
    return Δruntime.Stack(testTracebackArgsBuf[..], false);
}

// norace to avoid race instrumentation changing spill locations.
// nosplit to avoid preemption or morestack spilling registers.
//
//go:norace
//go:nosplit
//go:noinline
internal static partial nint testTracebackArgsSlice(slice<nint> a) {
    nint n = Δruntime.Stack(testTracebackArgsBuf[..], false);
    return a[1] + n;
}

internal static ж<array<nint>> ᏑtestTracebackArgsSliceBackingStore = new StandardBox<array<nint>>(new array<nint>(2));
internal static ref array<nint> testTracebackArgsSliceBackingStore => ref ᏑtestTracebackArgsSliceBackingStore.Value;

// Poison the arg area with deterministic values.
//
//go:noinline
internal static partial array<nint> poisonStack() {
    return new nint[]{-1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1}.array();
}

public static partial void TestTracebackParentChildGoroutines(ж<testing.T> Ꮡt) {
    @string parent = fmt.Sprintf("goroutine %d"u8, runtime_internal_test_package.Goid());
    ref var wg = ref heap(new Δsync.WaitGroup(), out var Ꮡwg);
    Ꮡwg.Add(1);
    goǃ([MethodImpl(MethodImplOptions.NoInlining)] () => {
        GoFrame ᒐ = default;
        try {
            defer(Ꮡwg.Done, ref ᒐ);
            var buf = new slice<byte>((1 << (int)(10)));
            // We collect the stack only for this goroutine (by passing
            // false to runtime.Stack). We expect to see the current
            // goroutine ID, and the parent goroutine ID in a message like
            // "created by ... in goroutine N".
            @string stack = ((@string)(buf.slice(0, Δruntime.Stack(buf, false))));
            @string child = fmt.Sprintf("goroutine %d"u8, runtime_internal_test_package.Goid());
            if (!strings.Contains(stack, parent) || !strings.Contains(stack, child)) {
                Ꮡt.Errorf("did not see parent (%s) and child (%s) IDs in stack, got %s"u8, parent, child, stack);
            }
        }
        catch (Exception ᒐex) when (GoFrame.IsPanic(ᒐex, out PanicException? ᒐp)) { GoFrame.Capture(ᒐp); }
        finally { ᒐ.Run(); }
    });
    Ꮡwg.Wait();
}

[GoType] partial struct traceback {
    internal slice<ж<tbFrame>> frames;
    internal ж<tbFrame> createdBy; // no args
}

[GoType] partial struct tbFrame {
    internal @string funcName;
    internal @string args;
    internal bool inlined;
    // elided is set to the number of frames elided, and the other fields are
    // set to the zero value.
    internal nint elided;
    internal nint off; // byte offset in the traceback text of this frame
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static readonly @string missingSourceLineˢ = "missing source line"u8;
internal static readonly @string framesElidedˢ = @"^\.\.\.([0-9]+) frames elided\.\.\.$"u8;
internal static readonly @string unexpectedIndentˢ = "unexpected indent"u8;
internal static readonly @string missingˢ = "missing ("u8;

// parseTraceback parses a printed traceback to make it easier for tests to
// check the result.
internal static slice<ж<traceback>> parseTraceback(ж<testing.T> Ꮡt, @string tb) {
    ref var t = ref Ꮡt.DerefOrNull();

    //lines := strings.Split(tb, "\n")
    //nLines := len(lines)
    ref var off = ref heap<nint>(out var Ꮡoff);
    off = 0;
    nint lineNo = 0;
    void fatal(@string f, params ꓸꓸꓸany argsʗp) {
        var args = argsʗp.sslice();
        @string msg = fmt.Sprintf(f, args.ꓸꓸꓸ);
        Ꮡt.Fatalf("%s (line %d):\n%s"u8, msg, lineNo, tb);
    }
    var fatalʗ1 = fatal;
    ж<tbFrame> parseFrame(@string funcName, @string args) {
        // Consume file/line/etc
        if (!strings.HasPrefix(tb, "\t"u8)) {
            fatalʗ1(missingSourceLineˢ);
        }
        (_, tb, _) = strings.Cut(tb, "\n"u8);
        lineNo++;
        ref var inlined = ref heap<bool>(out var Ꮡinlined);
        inlined = args == "..."u8;
        return Ꮡ(new tbFrame(funcName: funcName, args: args, inlined: inlined, off: Ꮡoff.Value));
    }
    ж<Δregexp.Regexp> elidedRe = Δregexp.MustCompile(framesElidedˢ);
    slice<ж<traceback>> tbs = default!;
    ж<traceback> cur = default!;
    nint tbLen = len(tb);
    while (len(tb) > 0) {
        @string line = default!;
        off = tbLen - len(tb);
        (line, tb, _) = strings.Cut(tb, "\n"u8);
        lineNo++;
        switch (ᐧ) {
        case {} when strings.HasPrefix(line, goroutineˢ): {
            cur = Ꮡ(new traceback(nil));
            tbs = append(tbs, cur);
            break;
        }
        case {} when line == ""u8: {
            cur = default!;
            break;
        }
        case {} when line[0] is (rune)'\t': {
            fatal(unexpectedIndentˢ);
            break;
        }
        case {} when strings.HasPrefix(line, // Separator between goroutines
 createdByˢ): {
            @string funcName = line[(int)(len("created by "))..];
            cur.Value.createdBy = parseFrame(funcName, ""u8);
            break;
        }
        case {} when strings.HasSuffix(line, ")"u8): {
            line = line.slice(0, len(line) - 1); // Trim trailing ")"
            var (funcName, args, found) = strings.Cut(line, "("u8);
            if (!found) {
                fatal(missingˢ);
            }
            var frame = parseFrame(funcName, args);
            cur.Value.frames = append((~cur).frames, frame);
            break;
        }
        case {} when elidedRe.MatchString(line): {
            var nStr = elidedRe.FindStringSubmatch(line);
            ref var n = ref heap<nint>(out var Ꮡn);
            (n, _) = strconv.Atoi(nStr[1]);
            var frame = Ꮡ(new tbFrame( // "...N frames elided..."
elided: n));
            cur.Value.frames = append((~cur).frames, frame);
            break;
        }}

    }
    return tbs;
}

// parseTraceback1 is like parseTraceback, but expects tb to contain exactly one
// goroutine.
internal static ж<traceback> parseTraceback1(ж<testing.T> Ꮡt, @string tb) {
    var tbs = parseTraceback(Ꮡt, tb);
    if (len(tbs) != 1) {
        Ꮡt.Fatalf("want 1 goroutine, got %d:\n%s"u8, len(tbs), tb);
    }
    return tbs[0];
}

//go:noinline
internal static partial nint testTracebackGenericFn<T>(slice<byte> buf) {
    return Δruntime.Stack(buf[..], false);
}

internal static partial nint testTracebackGenericFnInlined<T>(slice<byte> buf) {
    return Δruntime.Stack(buf[..], false);
}

[GoType] partial struct testTracebackGenericTyp<P> {
    internal P x;
}

//go:noinline
internal static partial nint M<P>(this testTracebackGenericTyp<P> t, slice<byte> buf) {
    return Δruntime.Stack(buf[..], false);
}

internal static partial nint Inlined<P>(this testTracebackGenericTyp<P> t, slice<byte> buf) {
    return Δruntime.Stack(buf[..], false);
}

[GoType("dyn")] internal partial struct TestTracebackGeneric_tests {
    internal Func<slice<byte>, nint> fn;
    internal @string expect;
}

public static void TestTracebackGeneric(ж<testing.T> Ꮡt) {
    if (flagQuick.Value) {
        Ꮡt.Skip(quickˢ);
    }
    ref var x = ref heap(new testTracebackGenericTyp<nint>(), out var Ꮡx);
            var xʗ1 = x;

            var xʗ2 = x;
    var tests = new TestTracebackGeneric_tests[]{ // function, not inlined

        new(
            testTracebackGenericFn<nint>,
            "testTracebackGenericFn[...]("u8
        ), // function, inlined

        new(
            (slice<byte> bufΔ1) => testTracebackGenericFnInlined<nint>(bufΔ1),
            "testTracebackGenericFnInlined[...]("u8
        ), // method, not inlined

        new(
            (slice<byte> p1) => xʗ1.M(p1),
            "testTracebackGenericTyp[...].M("u8
        ), // method, inlined

        new(
            (slice<byte> bufΔ2) => xʗ2.Inlined(bufΔ2),
            "testTracebackGenericTyp[...].Inlined("u8
        )
    }.slice();
    array<byte> buf = new(1000);
    foreach (var (_, test) in tests) {
        nint n = test.fn(buf[..]);
        var got = buf.slice(0, n);
        if (!bytes.Contains(got, slice<byte>(test.expect))) {
            Ꮡt.Errorf("traceback does not contain expected string: want %q, got\n%s"u8, test.expect, got);
        }
        if (bytes.Contains(got, slice<byte>("shape"u8))) {
            // should not contain shape name
            Ꮡt.Errorf("traceback contains shape name: got\n%s"u8, got);
        }
    }
}

} // end runtime_test_package
