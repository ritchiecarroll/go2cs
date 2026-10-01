// Copyright 2023 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.
namespace go;

using abi = @internal.abi_package;
using windows = @internal.syscall.windows_package;
using Δruntime = runtime_package;
using slices = slices_package;
using testing = testing_package;
using @unsafe = unsafe_package;
using @internal;
using @internal.syscall;
using System.Runtime.CompilerServices;
using static global::go.runtime_internal_test_package;

partial class runtime_test_package {

internal static nint sehf1() {
    return sehf1();
}

internal static void sehf2() {
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static readonly object skippingAmd64OnlyTestˢ = (@string)"skipping amd64-only test"u8;

[GoType("dyn")] internal partial struct TestSehLookupFunctionEntry_tests {
    internal @string name;
    internal uintptr pc;
    internal bool hasframe;
}

public static void TestSehLookupFunctionEntry(ж<testing.T> Ꮡt) {
    ref var t = ref Ꮡt.DerefOrNull();

    if (Δruntime.GOARCH != "amd64"u8) {
        Ꮡt.Skip(skippingAmd64OnlyTestˢ);
    }
    // This test checks that Win32 is able to retrieve
    // function metadata stored in the .pdata section
    // by the Go linker.
    // Win32 unwinding will fail if this test fails,
    // as RtlUnwindEx uses RtlLookupFunctionEntry internally.
    // If that's the case, don't bother investigating further,
    // first fix the .pdata generation.
    var sehf1pc = abi.FuncPCABIInternal(sehf1);
    ref var fnwithframe = ref heap<Action>(out var Ꮡfnwithframe);
    fnwithframe = () => {
        Ꮡfnwithframe.ValueSlot();
    };
    var fnwithoutframe = () => {
    };
    var tests = new TestSehLookupFunctionEntry_tests[]{
        new("no frame func"u8, abi.FuncPCABIInternal(sehf2), false),
        new("no func"u8, sehf1pc - 1, false),
        new("func at entry"u8, sehf1pc, true),
        new("func in prologue"u8, sehf1pc + 1, true),
        new("anonymous func with frame"u8, abi.FuncPCABIInternal((fnwithframe).OrTypedNilFunc()), true),
        new("anonymous func without frame"u8, abi.FuncPCABIInternal((fnwithoutframe).OrTypedNilFunc()), false),
        new("pc at func body"u8, (~runtime_internal_test_package.NewContextStub()).GetPC(), true)
    }.slice();
    foreach (var (_, tt) in tests) {
        ref var @base = ref heap(new uintptr(), out var Ꮡbase);
        var fn = windows.RtlLookupFunctionEntry(tt.pc, Ꮡbase, nil);
        if (!tt.hasframe) {
            if (fn != 0) {
                Ꮡt.Errorf("%s: unexpected frame"u8, tt.name);
            }
            continue;
        }
        if (fn == 0) {
            Ꮡt.Errorf("%s: missing frame"u8, tt.name);
        }
    }
}

internal static slice<uintptr> sehCallers() {
    // We don't need a real context,
    // RtlVirtualUnwind just needs a context with
    // valid a pc, sp and fp (aka bp).
    var ctx = runtime_internal_test_package.NewContextStub();
    var pcs = new slice<uintptr>(15);
    ref var @base = ref heap(new uintptr(), out var Ꮡbase);
    ref var frame = ref heap(new uintptr(), out var Ꮡframe);
    nint n = default!;
    for (nint i = 0; i < len(pcs); i++) {
        var fn = windows.RtlLookupFunctionEntry((~ctx).GetPC(), Ꮡbase, nil);
        if (fn == 0) {
            break;
        }
        pcs[i] = (~ctx).GetPC();
        n++;
        windows.RtlVirtualUnwind(0, @base, (~ctx).GetPC(), fn, (uintptr)ctx, nil, Ꮡframe, nil);
    }
    return pcs.slice(0, n);
}

// SEH unwinding does not report inlined frames.
//
//go:noinline
[MethodImpl(MethodImplOptions.NoInlining)] internal static slice<uintptr> sehf3(bool pan) {
    return sehf4(pan);
}

//go:noinline
[MethodImpl(MethodImplOptions.NoInlining)] internal static slice<uintptr> sehf4(bool pan) {
    slice<uintptr> pcs = default!;
    if (pan) {
        throw panic("sehf4");
    }
    pcs = sehCallers();
    return pcs;
}

internal static void testSehCallersEqual(ж<testing.T> Ꮡt, slice<uintptr> pcs, slice<@string> want) {
    Ꮡt.Helper();
    var got = new slice<@string>(0, len(want));
    foreach (var (_, pc) in pcs) {
        var fn = Δruntime.FuncForPC(pc);
        if (fn == nil || len(got) >= len(want)) {
            break;
        }
        @string name = fn.Name();
        var exprᴛ1 = name;
        if (exprᴛ1 == "runtime.panicmem"u8) {
            continue;
        }

        // These functions are skipped as they appear inconsistently depending
        // whether inlining is on or off.
        got = append(got, name);
    }
    if (!slices.Equal<slice<@string>, @string>(want, got)) {
        Ꮡt.Fatalf("wanted %v, got %v"u8, want, got);
    }
}

public static void TestSehUnwind(ж<testing.T> Ꮡt) {
    if (Δruntime.GOARCH != "amd64"u8) {
        Ꮡt.Skip(skippingAmd64OnlyTestˢ);
    }
    var pcs = sehf3(false);
    testSehCallersEqual(Ꮡt, pcs, new @string[]{"runtime_test.sehCallers"u8, "runtime_test.sehf4"u8,
        "runtime_test.sehf3"u8, "runtime_test.TestSehUnwind"u8}.slice());
}

public static void TestSehUnwindPanic(ж<testing.T> Ꮡt) {
    GoFrame ᒐ = default;
    try {
        if (Δruntime.GOARCH != "amd64"u8) {
            Ꮡt.Skip(skippingAmd64OnlyTestˢ);
        }
        var want = new @string[]{"runtime_test.sehCallers"u8, "runtime_test.TestSehUnwindPanic.func1"u8, "runtime.gopanic"u8,
            "runtime_test.sehf4"u8, "runtime_test.sehf3"u8, "runtime_test.TestSehUnwindPanic"u8}.slice();
        var wantʗ1 = want;
        defer(() => {
            {
                var r = recover(); if (r == default!) {
                    Ꮡt.Fatal(didNotPanicˢ);
                }
            }
            var pcs = sehCallers();
            testSehCallersEqual(Ꮡt, pcs, wantʗ1);
        }, ref ᒐ);
        sehf3(true);
    }
    catch (Exception ᒐex) when (GoFrame.IsPanic(ᒐex, out PanicException? ᒐp)) { GoFrame.Capture(ᒐp); }
    finally { ᒐ.Run(); }
}

public static void TestSehUnwindDoublePanic(ж<testing.T> Ꮡt) {
    GoFrame ᒐ = default;
    try {
        if (Δruntime.GOARCH != "amd64"u8) {
            Ꮡt.Skip(skippingAmd64OnlyTestˢ);
        }
        var want = new @string[]{"runtime_test.sehCallers"u8, "runtime_test.TestSehUnwindDoublePanic.func1.1"u8, "runtime.gopanic"u8,
            "runtime_test.TestSehUnwindDoublePanic.func1"u8, "runtime.gopanic"u8, "runtime_test.TestSehUnwindDoublePanic"u8}.slice();
        var wantʗ1 = want;
        defer(() => {
            GoFrame ᒐ = default;
            try {
                var wantʗ2 = wantʗ1;
                defer(() => {
                    if (recover() == default!) {
                        Ꮡt.Fatal(didNotPanicˢ);
                    }
                    var pcs = sehCallers();
                    testSehCallersEqual(Ꮡt, pcs, wantʗ2);
                }, ref ᒐ);
                if (recover() == default!) {
                    Ꮡt.Fatal(didNotPanicˢ);
                }
                throw panic((nint)(2));
            }
            catch (Exception ᒐex) when (GoFrame.IsPanic(ᒐex, out PanicException? ᒐp)) { GoFrame.Capture(ᒐp); }
            finally { ᒐ.Run(); }
        }, ref ᒐ);
        throw panic((nint)(1));
    }
    catch (Exception ᒐex) when (GoFrame.IsPanic(ᒐex, out PanicException? ᒐp)) { GoFrame.Capture(ᒐp); }
    finally { ᒐ.Run(); }
}

public static void TestSehUnwindNilPointerPanic(ж<testing.T> Ꮡt) {
    GoFrame ᒐ = default;
    try {
        if (Δruntime.GOARCH != "amd64"u8) {
            Ꮡt.Skip(skippingAmd64OnlyTestˢ);
        }
        var want = new @string[]{"runtime_test.sehCallers"u8, "runtime_test.TestSehUnwindNilPointerPanic.func1"u8, "runtime.gopanic"u8,
            "runtime.sigpanic"u8, "runtime_test.TestSehUnwindNilPointerPanic"u8}.slice();
        var wantʗ1 = want;
        defer(() => {
            {
                var r = recover(); if (r == default!) {
                    Ꮡt.Fatal(didNotPanicˢ);
                }
            }
            var pcs = sehCallers();
            testSehCallersEqual(Ꮡt, pcs, wantʗ1);
        }, ref ᒐ);
        ж<nint> p = default!;
        if (p.Value == 3) {
            Ꮡt.Fatal(didNotSeeNilPointerPanicˢ);
        }
    }
    catch (Exception ᒐex) when (GoFrame.IsPanic(ᒐex, out PanicException? ᒐp)) { GoFrame.Capture(ᒐp); }
    finally { ᒐ.Run(); }
}

} // end runtime_test_package
