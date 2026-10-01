// Copyright 2023 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.
namespace go;

using Δruntime = runtime_package;
using testing = testing_package;
using time = time_package;
using @unsafe = unsafe_package;
using static global::go.runtime_internal_test_package;

partial class runtime_test_package {

[GoType] partial struct obj {
    internal int64 x;
    internal int64 y;
    internal int64 z;
}

[GoType] partial struct objWith<T> {
    internal int64 x;
    internal int64 y;
    internal int64 z;
    internal T o;
}

internal static uintptr globalUintptr;
internal static ж<obj> globalPtrToObj = Ꮡ(new obj(nil));
internal static ж<objWith<ж<uintptr>>> globalPtrToObjWithPtr = Ꮡ(new objWith<ж<uintptr>>(nil));
internal static ж<obj> globalPtrToRuntimeObj = ((Func<ж<obj>>)(() => {
    return Ꮡ(new obj(nil));
}))();
internal static ж<objWith<ж<uintptr>>> globalPtrToRuntimeObjWithPtr = ((Func<ж<objWith<ж<uintptr>>>>)(() => {
    return Ꮡ(new objWith<ж<uintptr>>(nil));
}))();

internal static void assertDidPanic(ж<testing.T> Ꮡt) {
    GoFrame ᒐ = default;
    try {
        if (recover() == default!) {
            Ꮡt.Fatal(didNotPanicˢ);
        }
    }
    catch (Exception ᒐex) when (GoFrame.IsPanic(ᒐex, out PanicException? ᒐp)) { GoFrame.Capture(ᒐp); }
    finally { ᒐ.Run(); }
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static readonly object cgoCheckPointerDidNotˢ = (@string)"cgoCheckPointer() did not panic, make sure the tests run with cgocheck=1"u8;

internal static void assertCgoCheckPanics(ж<testing.T> Ꮡt, any p) {
    GoFrame ᒐ = default;
    try {
        defer(() => {
            if (recover() == default!) {
                Ꮡt.Fatal(cgoCheckPointerDidNotˢ);
            }
        }, ref ᒐ);
        runtime_internal_test_package.CgoCheckPointer(p, true);
    }
    catch (Exception ᒐex) when (GoFrame.IsPanic(ᒐex, out PanicException? ᒐp)) { GoFrame.Capture(ᒐp); }
    finally { ᒐ.Run(); }
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static readonly object alreadyMarkedAsPinnedˢ = (@string)"already marked as pinned"u8;
internal static readonly object notMarkedAsPinnedˢ = (@string)"not marked as pinned"u8;
internal static readonly object pinCounterShouldNotExistˢ = (@string)"pin counter should not exist"u8;
internal static readonly object stillMarkedAsPinnedˢ = (@string)"still marked as pinned"u8;

public static void TestPinnerSimple(ж<testing.T> Ꮡt) {
    Δruntime.Pinner pinner = new(nil);
    var p = @new<obj>();
    @unsafe.Pointer addr = @unsafe.Pointer.FromPinnedBox(p);
    if (runtime_internal_test_package.IsPinned(addr)) {
        Ꮡt.Fatal(alreadyMarkedAsPinnedˢ);
    }
    pinner.Pin(p.OrTypedNil());
    if (!runtime_internal_test_package.IsPinned(addr)) {
        Ꮡt.Fatal(notMarkedAsPinnedˢ);
    }
    if (runtime_internal_test_package.GetPinCounter(addr) != nil) {
        Ꮡt.Fatal(pinCounterShouldNotExistˢ);
    }
    pinner.Unpin();
    if (runtime_internal_test_package.IsPinned(addr)) {
        Ꮡt.Fatal(stillMarkedAsPinnedˢ);
    }
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static readonly object pinDidnTKeepObjectAliveˢ = (@string)"Pin() didn't keep object alive"u8;
internal static readonly object unpinDidnTReleaseObjectˢ = (@string)"Unpin() didn't release object"u8;

public static void TestPinnerPinKeepsAliveAndReleases(ж<testing.T> Ꮡt) {
    Δruntime.Pinner pinner = new(nil);
    var p = @new<obj>();
    var done = new channel<EmptyStruct>(0);
    var doneʗ1 = done;
    Δruntime.SetFinalizer(p.OrTypedNil(), (any _) => {
        doneʗ1.ᐸꟷ(new EmptyStruct());
    });
    pinner.Pin(p.OrTypedNil());
    p = default!;
    Δruntime.GC();
    Δruntime.GC();
    var selᴛ74 = done;
    var selᴛ75 = time.After(time.Millisecond * 10);
    switch (select(ᐸꟷ(selᴛ74, ꓸꓸꓸ), ᐸꟷ(selᴛ75, ꓸꓸꓸ))) {
    case 0 when selᴛ74.ꟷᐳ(out _): {
        Ꮡt.Fatal(pinDidnTKeepObjectAliveˢ);
        break;
    }
    case 1 when selᴛ75.ꟷᐳ(out _): {
        break;
        break;
    }}
    pinner.Unpin();
    Δruntime.GC();
    Δruntime.GC();
    var selᴛ76 = done;
    var selᴛ77 = time.After(time.ΔSecond);
    switch (select(ᐸꟷ(selᴛ76, ꓸꓸꓸ), ᐸꟷ(selᴛ77, ꓸꓸꓸ))) {
    case 0 when selᴛ76.ꟷᐳ(out _): {
        break;
        break;
    }
    case 1 when selᴛ77.ꟷᐳ(out _): {
        Ꮡt.Fatal(unpinDidnTReleaseObjectˢ);
        break;
    }}
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static readonly object pinCounterWasNotDeletedˢ = (@string)"pin counter was not deleted"u8;

public static void TestPinnerMultiplePinsSame(ж<testing.T> Ꮡt) {
    UntypedInt N = 100;
    Δruntime.Pinner pinner = new(nil);
    var p = @new<obj>();
    @unsafe.Pointer addr = @unsafe.Pointer.FromPinnedBox(p);
    if (runtime_internal_test_package.IsPinned(addr)) {
        Ꮡt.Fatal(alreadyMarkedAsPinnedˢ);
    }
    for (nint i = 0; i < N; i++) {
        pinner.Pin(p.OrTypedNil());
    }
    if (!runtime_internal_test_package.IsPinned(addr)) {
        Ꮡt.Fatal(notMarkedAsPinnedˢ);
    }
    {
        var cnt = runtime_internal_test_package.GetPinCounter(addr); if (cnt == nil || cnt.Value != (uintptr)(N - 1)) {
            Ꮡt.Fatalf("pin counter incorrect: %d"u8, cnt.Value);
        }
    }
    pinner.Unpin();
    if (runtime_internal_test_package.IsPinned(addr)) {
        Ꮡt.Fatal(stillMarkedAsPinnedˢ);
    }
    if (runtime_internal_test_package.GetPinCounter(addr) != nil) {
        Ꮡt.Fatal(pinCounterWasNotDeletedˢ);
    }
}

public static void TestPinnerTwoPinner(ж<testing.T> Ꮡt) {
    Δruntime.Pinner pinner1 = new(nil);
    Δruntime.Pinner pinner2 = new(nil);
    var p = @new<obj>();
    @unsafe.Pointer addr = @unsafe.Pointer.FromPinnedBox(p);
    if (runtime_internal_test_package.IsPinned(addr)) {
        Ꮡt.Fatal(alreadyMarkedAsPinnedˢ);
    }
    pinner1.Pin(p.OrTypedNil());
    if (!runtime_internal_test_package.IsPinned(addr)) {
        Ꮡt.Fatal(notMarkedAsPinnedˢ);
    }
    if (runtime_internal_test_package.GetPinCounter(addr) != nil) {
        Ꮡt.Fatal(pinCounterShouldNotExistˢ);
    }
    pinner2.Pin(p.OrTypedNil());
    if (!runtime_internal_test_package.IsPinned(addr)) {
        Ꮡt.Fatal(notMarkedAsPinnedˢ);
    }
    {
        var cnt = runtime_internal_test_package.GetPinCounter(addr); if (cnt == nil || cnt.Value != 1) {
            Ꮡt.Fatalf("pin counter incorrect: %d"u8, cnt.Value);
        }
    }
    pinner1.Unpin();
    if (!runtime_internal_test_package.IsPinned(addr)) {
        Ꮡt.Fatal(notMarkedAsPinnedˢ);
    }
    if (runtime_internal_test_package.GetPinCounter(addr) != nil) {
        Ꮡt.Fatal(pinCounterShouldNotExistˢ);
    }
    pinner2.Unpin();
    if (runtime_internal_test_package.IsPinned(addr)) {
        Ꮡt.Fatal(stillMarkedAsPinnedˢ);
    }
    if (runtime_internal_test_package.GetPinCounter(addr) != nil) {
        Ꮡt.Fatal(pinCounterWasNotDeletedˢ);
    }
}

public static void TestPinnerPinZerosizeObj(ж<testing.T> Ꮡt) {
    GoFrame ᒐ = default;
    try {
        ref var pinner = ref heap(new Δruntime.Pinner(), out var Ꮡpinner);
        defer(Ꮡpinner.Unpin, ref ᒐ);
        var p = @new<EmptyStruct>();
        pinner.Pin(p.OrTypedNil());
        if (!runtime_internal_test_package.IsPinned(@unsafe.Pointer.FromPinnedBox(p))) {
            Ꮡt.Fatal(notMarkedAsPinnedˢ);
        }
    }
    catch (Exception ᒐex) when (GoFrame.IsPanic(ᒐex, out PanicException? ᒐp)) { GoFrame.Capture(ᒐp); }
    finally { ᒐ.Run(); }
}

public static void TestPinnerPinGlobalPtr(ж<testing.T> Ꮡt) {
    GoFrame ᒐ = default;
    try {
        ref var pinner = ref heap(new Δruntime.Pinner(), out var Ꮡpinner);
        defer(Ꮡpinner.Unpin, ref ᒐ);
        pinner.Pin(globalPtrToObj.OrTypedNil());
        pinner.Pin(globalPtrToObjWithPtr.OrTypedNil());
        pinner.Pin(globalPtrToRuntimeObj.OrTypedNil());
        pinner.Pin(globalPtrToRuntimeObjWithPtr.OrTypedNil());
    }
    catch (Exception ᒐex) when (GoFrame.IsPanic(ᒐex, out PanicException? ᒐp)) { GoFrame.Capture(ᒐp); }
    finally { ᒐ.Run(); }
}

public static void TestPinnerPinTinyObj(ж<testing.T> Ꮡt) {
    Δruntime.Pinner pinner = new(nil);
    UntypedInt N = 64;
    array<@unsafe.Pointer> addr = new(64); /* N */
    for (nint i = 0; i < N; i++) {
        var p = @new<bool>();
        addr[i] = @unsafe.Pointer.FromPinnedBox(p);
        pinner.Pin(p.OrTypedNil());
        pinner.Pin(p.OrTypedNil());
        if (!runtime_internal_test_package.IsPinned(addr[i])) {
            Ꮡt.Fatalf("not marked as pinned: %d"u8, i);
        }
        {
            var cnt = runtime_internal_test_package.GetPinCounter(addr[i]); if (cnt == nil || cnt.Value == 0) {
                Ꮡt.Fatalf("pin counter incorrect: %d, %d"u8, cnt.Value, i);
            }
        }
    }
    pinner.Unpin();
    for (nint i = 0; i < N; i++) {
        if (runtime_internal_test_package.IsPinned(addr[i])) {
            Ꮡt.Fatal(stillMarkedAsPinnedˢ);
        }
        if (runtime_internal_test_package.GetPinCounter(addr[i]) != nil) {
            Ꮡt.Fatal(pinCounterShouldNotExistˢ);
        }
    }
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static readonly object markedAsPinnedˢ = (@string)"marked as pinned"u8;

public static void TestPinnerInterface(ж<testing.T> Ꮡt) {
    Δruntime.Pinner pinner = new(nil);
    var o = @new<obj>();
    ref var ifc = ref heap<any>(out var Ꮡifc);
    ifc = ((any)o.OrTypedNil());
    pinner.Pin(Ꮡifc);
    if (!runtime_internal_test_package.IsPinned(@unsafe.Pointer.FromPinnedBox(Ꮡifc))) {
        Ꮡt.Fatal(notMarkedAsPinnedˢ);
    }
    if (runtime_internal_test_package.IsPinned(@unsafe.Pointer.FromPinnedBox(o))) {
        Ꮡt.Fatal(markedAsPinnedˢ);
    }
    pinner.Unpin();
    pinner.Pin(ifc);
    if (!runtime_internal_test_package.IsPinned(@unsafe.Pointer.FromPinnedBox(o))) {
        Ꮡt.Fatal(notMarkedAsPinnedˢ);
    }
    if (runtime_internal_test_package.IsPinned(@unsafe.Pointer.FromPinnedBox(Ꮡifc))) {
        Ꮡt.Fatal(markedAsPinnedˢ);
    }
    pinner.Unpin();
}

public static void TestPinnerPinNonPtrPanics(ж<testing.T> Ꮡt) {
    GoFrame ᒐ = default;
    try {
        ref var pinner = ref heap(new Δruntime.Pinner(), out var Ꮡpinner);
        defer(Ꮡpinner.Unpin, ref ᒐ);
        nint i = default!;
        defer(assertDidPanic, Ꮡt, ref ᒐ);
        pinner.Pin(i);
    }
    catch (Exception ᒐex) when (GoFrame.IsPanic(ᒐex, out PanicException? ᒐp)) { GoFrame.Capture(ᒐp); }
    finally { ᒐ.Run(); }
}

public static void TestPinnerReuse(ж<testing.T> Ꮡt) {
    Δruntime.Pinner pinner = new(nil);
    ref var p = ref heap<ж<obj>>(out var Ꮡp);
    p = @new<obj>();
    var p2 = Ꮡp;
    assertCgoCheckPanics(Ꮡt, p2.OrTypedNil());
    pinner.Pin(p.OrTypedNil());
    runtime_internal_test_package.CgoCheckPointer(p2.OrTypedNil(), true);
    pinner.Unpin();
    assertCgoCheckPanics(Ꮡt, p2.OrTypedNil());
    pinner.Pin(p.OrTypedNil());
    runtime_internal_test_package.CgoCheckPointer(p2.OrTypedNil(), true);
    pinner.Unpin();
}

public static void TestPinnerEmptyUnpin(ж<testing.T> Ꮡt) {
    Δruntime.Pinner pinner = new(nil);
    pinner.Unpin();
    pinner.Unpin();
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static readonly object leakDidnTMakeGcToPanicˢ = (@string)"leak didn't make GC to panic"u8;

public static void TestPinnerLeakPanics(ж<testing.T> Ꮡt) {
    var old = runtime_internal_test_package.GetPinnerLeakPanic();
    var oldʗ1 = old;
    ((Action)(() => {
        GoFrame ᒐ = default;
        try {
            defer(assertDidPanic, Ꮡt, ref ᒐ);
            oldʗ1();
        }
        catch (Exception ᒐex) when (GoFrame.IsPanic(ᒐex, out PanicException? ᒐp)) { GoFrame.Capture(ᒐp); }
        finally { ᒐ.Run(); }
    }))();
    var done = new channel<EmptyStruct>(0);
    var doneʗ1 = done;
    runtime_internal_test_package.SetPinnerLeakPanic(() => {
        doneʗ1.ᐸꟷ(new EmptyStruct());
    });
    ((Action)(() => {
        Δruntime.Pinner pinner = new(nil);
        var p = @new<obj>();
        pinner.Pin(p.OrTypedNil());
    }))();
    Δruntime.GC();
    Δruntime.GC();
    var selᴛ78 = done;
    var selᴛ79 = time.After(time.ΔSecond);
    switch (select(ᐸꟷ(selᴛ78, ꓸꓸꓸ), ᐸꟷ(selᴛ79, ꓸꓸꓸ))) {
    case 0 when selᴛ78.ꟷᐳ(out _): {
        break;
        break;
    }
    case 1 when selᴛ79.ꟷᐳ(out _): {
        Ꮡt.Fatal(leakDidnTMakeGcToPanicˢ);
        break;
    }}
    runtime_internal_test_package.SetPinnerLeakPanic(old);
}

public static void TestPinnerCgoCheckPtr2Ptr(ж<testing.T> Ꮡt) {
    GoFrame ᒐ = default;
    try {
        ref var pinner = ref heap(new Δruntime.Pinner(), out var Ꮡpinner);
        defer(Ꮡpinner.Unpin, ref ᒐ);
        var p = @new<obj>();
        var p2 = Ꮡ(new objWith<ж<obj>>(o: p));
        assertCgoCheckPanics(Ꮡt, p2.OrTypedNil());
        pinner.Pin(p.OrTypedNil());
        runtime_internal_test_package.CgoCheckPointer(p2.OrTypedNil(), true);
    }
    catch (Exception ᒐex) when (GoFrame.IsPanic(ᒐex, out PanicException? ᒐp)) { GoFrame.Capture(ᒐp); }
    finally { ᒐ.Run(); }
}

public static void TestPinnerCgoCheckPtr2UnsafePtr(ж<testing.T> Ꮡt) {
    GoFrame ᒐ = default;
    try {
        ref var pinner = ref heap(new Δruntime.Pinner(), out var Ꮡpinner);
        defer(Ꮡpinner.Unpin, ref ᒐ);
        ref var p = ref heap<@unsafe.Pointer>(out var Ꮡp);
        p = @unsafe.Pointer.FromPinnedBox(@new<obj>());
        var p2 = Ꮡ(new objWith<@unsafe.Pointer>(o: p));
        assertCgoCheckPanics(Ꮡt, p2.OrTypedNil());
        pinner.Pin(@unsafe.Pointer.OrTypedNil(p));
        runtime_internal_test_package.CgoCheckPointer(p2.OrTypedNil(), true);
    }
    catch (Exception ᒐex) when (GoFrame.IsPanic(ᒐex, out PanicException? ᒐp)) { GoFrame.Capture(ᒐp); }
    finally { ᒐ.Run(); }
}

public static void TestPinnerCgoCheckPtr2UnknownPtr(ж<testing.T> Ꮡt) {
    GoFrame ᒐ = default;
    try {
        ref var pinner = ref heap(new Δruntime.Pinner(), out var Ꮡpinner);
        defer(Ꮡpinner.Unpin, ref ᒐ);
        ref var p = ref heap<@unsafe.Pointer>(out var Ꮡp);
        p = @unsafe.Pointer.FromPinnedBox(@new<obj>());
        var p2 = Ꮡp;
        var p2ʗ1 = p2;
        ((Action)(() => {
            GoFrame ᒐ = default;
            try {
                defer(assertDidPanic, Ꮡt, ref ᒐ);
                runtime_internal_test_package.CgoCheckPointer(p2ʗ1.OrTypedNil(), default!);
            }
            catch (Exception ᒐex) when (GoFrame.IsPanic(ᒐex, out PanicException? ᒐp)) { GoFrame.Capture(ᒐp); }
            finally { ᒐ.Run(); }
        }))();
        pinner.Pin(@unsafe.Pointer.OrTypedNil(p));
        runtime_internal_test_package.CgoCheckPointer(p2.OrTypedNil(), default!);
    }
    catch (Exception ᒐex) when (GoFrame.IsPanic(ᒐex, out PanicException? ᒐp)) { GoFrame.Capture(ᒐp); }
    finally { ᒐ.Run(); }
}

public static void TestPinnerCgoCheckInterface(ж<testing.T> Ꮡt) {
    GoFrame ᒐ = default;
    try {
        ref var pinner = ref heap(new Δruntime.Pinner(), out var Ꮡpinner);
        defer(Ꮡpinner.Unpin, ref ᒐ);
        ref var ifc = ref heap<any>(out var Ꮡifc);
        ref var o = ref heap(new obj(), out var Ꮡo);
        ifc = Ꮡo;
        var p = Ꮡifc;
        assertCgoCheckPanics(Ꮡt, p.OrTypedNil());
        pinner.Pin(Ꮡo);
        runtime_internal_test_package.CgoCheckPointer(p.OrTypedNil(), true);
    }
    catch (Exception ᒐex) when (GoFrame.IsPanic(ᒐex, out PanicException? ᒐp)) { GoFrame.Capture(ᒐp); }
    finally { ᒐ.Run(); }
}

public static void TestPinnerCgoCheckSlice(ж<testing.T> Ꮡt) {
    GoFrame ᒐ = default;
    try {
        ref var pinner = ref heap(new Δruntime.Pinner(), out var Ꮡpinner);
        defer(Ꮡpinner.Unpin, ref ᒐ);
        ref var sl = ref heap<slice<nint>>(out var Ꮡsl);
        sl = new nint[]{1, 2, 3}.slice();
        assertCgoCheckPanics(Ꮡt, Ꮡsl);
        pinner.Pin(Ꮡ(sl, 0));
        runtime_internal_test_package.CgoCheckPointer(Ꮡsl, true);
    }
    catch (Exception ᒐex) when (GoFrame.IsPanic(ᒐex, out PanicException? ᒐp)) { GoFrame.Capture(ᒐp); }
    finally { ᒐ.Run(); }
}

public static void TestPinnerCgoCheckString(ж<testing.T> Ꮡt) {
    GoFrame ᒐ = default;
    try {
        ref var pinner = ref heap(new Δruntime.Pinner(), out var Ꮡpinner);
        defer(Ꮡpinner.Unpin, ref ᒐ);
        var b = slice<byte>("foobar"u8);
        ref var str = ref heap<@string>(out var Ꮡstr);
        str = @unsafe.String(Ꮡ(b, 0), 6);
        assertCgoCheckPanics(Ꮡt, Ꮡstr);
        pinner.Pin(Ꮡ(b, 0));
        runtime_internal_test_package.CgoCheckPointer(Ꮡstr, true);
    }
    catch (Exception ᒐex) when (GoFrame.IsPanic(ᒐex, out PanicException? ᒐp)) { GoFrame.Capture(ᒐp); }
    finally { ᒐ.Run(); }
}

public static void TestPinnerCgoCheckPinned2UnpinnedPanics(ж<testing.T> Ꮡt) {
    GoFrame ᒐ = default;
    try {
        ref var pinner = ref heap(new Δruntime.Pinner(), out var Ꮡpinner);
        defer(Ꮡpinner.Unpin, ref ᒐ);
        var p = @new<obj>();
        var p2 = Ꮡ(new objWith<ж<obj>>(o: p));
        assertCgoCheckPanics(Ꮡt, p2.OrTypedNil());
        pinner.Pin(p2.OrTypedNil());
        assertCgoCheckPanics(Ꮡt, p2.OrTypedNil());
    }
    catch (Exception ᒐex) when (GoFrame.IsPanic(ᒐex, out PanicException? ᒐp)) { GoFrame.Capture(ᒐp); }
    finally { ᒐ.Run(); }
}

public static void TestPinnerCgoCheckPtr2Pinned2Unpinned(ж<testing.T> Ꮡt) {
    GoFrame ᒐ = default;
    try {
        ref var pinner = ref heap(new Δruntime.Pinner(), out var Ꮡpinner);
        defer(Ꮡpinner.Unpin, ref ᒐ);
        var p = @new<obj>();
        var p2 = Ꮡ(new objWith<ж<obj>>(o: p));
        var p3 = Ꮡ(new objWith<ж<objWith<ж<obj>>>>(o: p2));
        assertCgoCheckPanics(Ꮡt, p2.OrTypedNil());
        assertCgoCheckPanics(Ꮡt, p3.OrTypedNil());
        pinner.Pin(p2.OrTypedNil());
        assertCgoCheckPanics(Ꮡt, p2.OrTypedNil());
        assertCgoCheckPanics(Ꮡt, p3.OrTypedNil());
        pinner.Pin(p.OrTypedNil());
        runtime_internal_test_package.CgoCheckPointer(p2.OrTypedNil(), true);
        runtime_internal_test_package.CgoCheckPointer(p3.OrTypedNil(), true);
    }
    catch (Exception ᒐex) when (GoFrame.IsPanic(ᒐex, out PanicException? ᒐp)) { GoFrame.Capture(ᒐp); }
    finally { ᒐ.Run(); }
}

public static void BenchmarkPinnerPinUnpinBatch(ж<testing.B> Ꮡb) {
    ref var b = ref Ꮡb.DerefOrNull();

    UntypedInt Batch = 1000;
    array<ж<obj>> data = new(1000); /* Batch */
    for (nint i = 0; i < Batch; i++) {
        data[i] = @new<obj>();
    }
    b.ResetTimer();
    for (nint n = 0; n < b.N; n++) {
        Δruntime.Pinner pinner = new(nil);
        for (nint i = 0; i < Batch; i++) {
            pinner.Pin(data[i].OrTypedNil());
        }
        pinner.Unpin();
    }
}

public static void BenchmarkPinnerPinUnpinBatchDouble(ж<testing.B> Ꮡb) {
    ref var b = ref Ꮡb.DerefOrNull();

    UntypedInt Batch = 1000;
    array<ж<obj>> data = new(1000); /* Batch */
    for (nint i = 0; i < Batch; i++) {
        data[i] = @new<obj>();
    }
    b.ResetTimer();
    for (nint n = 0; n < b.N; n++) {
        Δruntime.Pinner pinner = new(nil);
        for (nint i = 0; i < Batch; i++) {
            pinner.Pin(data[i].OrTypedNil());
            pinner.Pin(data[i].OrTypedNil());
        }
        pinner.Unpin();
    }
}

public static void BenchmarkPinnerPinUnpinBatchTiny(ж<testing.B> Ꮡb) {
    ref var b = ref Ꮡb.DerefOrNull();

    UntypedInt Batch = 1000;
    array<ж<bool>> data = new(1000); /* Batch */
    for (nint i = 0; i < Batch; i++) {
        data[i] = @new<bool>();
    }
    b.ResetTimer();
    for (nint n = 0; n < b.N; n++) {
        Δruntime.Pinner pinner = new(nil);
        for (nint i = 0; i < Batch; i++) {
            pinner.Pin(data[i].OrTypedNil());
        }
        pinner.Unpin();
    }
}

public static void BenchmarkPinnerPinUnpin(ж<testing.B> Ꮡb) {
    ref var b = ref Ꮡb.DerefOrNull();

    var p = @new<obj>();
    for (nint n = 0; n < b.N; n++) {
        Δruntime.Pinner pinner = new(nil);
        pinner.Pin(p.OrTypedNil());
        pinner.Unpin();
    }
}

public static void BenchmarkPinnerPinUnpinTiny(ж<testing.B> Ꮡb) {
    ref var b = ref Ꮡb.DerefOrNull();

    var p = @new<bool>();
    for (nint n = 0; n < b.N; n++) {
        Δruntime.Pinner pinner = new(nil);
        pinner.Pin(p.OrTypedNil());
        pinner.Unpin();
    }
}

public static void BenchmarkPinnerPinUnpinDouble(ж<testing.B> Ꮡb) {
    ref var b = ref Ꮡb.DerefOrNull();

    var p = @new<obj>();
    for (nint n = 0; n < b.N; n++) {
        Δruntime.Pinner pinner = new(nil);
        pinner.Pin(p.OrTypedNil());
        pinner.Pin(p.OrTypedNil());
        pinner.Unpin();
    }
}

public static void BenchmarkPinnerPinUnpinParallel(ж<testing.B> Ꮡb) {
    Ꮡb.RunParallel((ж<testing.PB> pb) => {
        var p = @new<obj>();
        while (pb.Next()) {
            Δruntime.Pinner pinner = new(nil);
            pinner.Pin(p.OrTypedNil());
            pinner.Unpin();
        }
    });
}

public static void BenchmarkPinnerPinUnpinParallelTiny(ж<testing.B> Ꮡb) {
    Ꮡb.RunParallel((ж<testing.PB> pb) => {
        var p = @new<bool>();
        while (pb.Next()) {
            Δruntime.Pinner pinner = new(nil);
            pinner.Pin(p.OrTypedNil());
            pinner.Unpin();
        }
    });
}

public static void BenchmarkPinnerPinUnpinParallelDouble(ж<testing.B> Ꮡb) {
    Ꮡb.RunParallel((ж<testing.PB> pb) => {
        var p = @new<obj>();
        while (pb.Next()) {
            Δruntime.Pinner pinner = new(nil);
            pinner.Pin(p.OrTypedNil());
            pinner.Pin(p.OrTypedNil());
            pinner.Unpin();
        }
    });
}

public static void BenchmarkPinnerIsPinnedOnPinned(ж<testing.B> Ꮡb) {
    ref var b = ref Ꮡb.DerefOrNull();

    Δruntime.Pinner pinner = new(nil);
    var ptr = @new<obj>();
    pinner.Pin(ptr.OrTypedNil());
    b.ResetTimer();
    for (nint n = 0; n < b.N; n++) {
        runtime_internal_test_package.IsPinned(@unsafe.Pointer.FromPinnedBox(ptr));
    }
    pinner.Unpin();
}

public static void BenchmarkPinnerIsPinnedOnUnpinned(ж<testing.B> Ꮡb) {
    ref var b = ref Ꮡb.DerefOrNull();

    var ptr = @new<obj>();
    b.ResetTimer();
    for (nint n = 0; n < b.N; n++) {
        runtime_internal_test_package.IsPinned(@unsafe.Pointer.FromPinnedBox(ptr));
    }
}

public static void BenchmarkPinnerIsPinnedOnPinnedParallel(ж<testing.B> Ꮡb) {
    ref var b = ref Ꮡb.DerefOrNull();

    Δruntime.Pinner pinner = new(nil);
    var ptr = @new<obj>();
    pinner.Pin(ptr.OrTypedNil());
    b.ResetTimer();
    var ptrʗ1 = ptr;
    Ꮡb.RunParallel((ж<testing.PB> pb) => {
        while (pb.Next()) {
            runtime_internal_test_package.IsPinned(@unsafe.Pointer.FromPinnedBox(ptrʗ1));
        }
    });
    pinner.Unpin();
}

public static void BenchmarkPinnerIsPinnedOnUnpinnedParallel(ж<testing.B> Ꮡb) {
    ref var b = ref Ꮡb.DerefOrNull();

    var ptr = @new<obj>();
    b.ResetTimer();
    var ptrʗ1 = ptr;
    Ꮡb.RunParallel((ж<testing.PB> pb) => {
        while (pb.Next()) {
            runtime_internal_test_package.IsPinned(@unsafe.Pointer.FromPinnedBox(ptrʗ1));
        }
    });
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static readonly @string testConstStringˢ = "test-const-string"u8;

// const string data is not in span.
public static void TestPinnerConstStringData(ж<testing.T> Ꮡt) {
    Δruntime.Pinner pinner = new(nil);
    @string str = testConstStringˢ;
    var p = @unsafe.StringData(str);
    @unsafe.Pointer addr = @unsafe.Pointer.FromPinnedBox(p);
    if (!runtime_internal_test_package.IsPinned(addr)) {
        Ꮡt.Fatal(notMarkedAsPinnedˢ);
    }
    pinner.Pin(p.OrTypedNil());
    pinner.Unpin();
    if (!runtime_internal_test_package.IsPinned(addr)) {
        Ꮡt.Fatal(notMarkedAsPinnedˢ);
    }
}

} // end runtime_test_package
