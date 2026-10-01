// Copyright 2022 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.
namespace go;

using goarch = @internal.goarch_package;
using atomic = @internal.runtime.atomic_package;
using reflect = reflect_package;
using static runtime_package;
using Δdebug = global::go.runtime.debug_package;
using testing = testing_package;
using time = time_package;
using @unsafe = unsafe_package;
using @internal;
using @internal.runtime;
using global::go.runtime;
using static global::go.runtime_internal_test_package;
using Δruntime = runtime_package;

partial class runtime_test_package {

[GoType] partial struct smallScalar {
    public uintptr X;
}

[GoType] public partial struct smallPointer {
    public ж<smallPointer> X;
}

[GoType] partial struct smallPointerMix {
    public ж<smallPointer> A;
    public byte B;
    public ж<smallPointer> C;
    public array<byte> D = new(11);
}

[GoType("[8192]byte")] partial struct mediumScalarEven;

[GoType("[3321]byte")] partial struct mediumScalarOdd;

[GoType("[1024]ж<smallPointer>")] partial struct mediumPointerEven;

[GoType("[1023]ж<smallPointer>")] partial struct mediumPointerOdd;

[GoType("[4194305]byte")] /* [runtime_internal_test_package.UserArenaChunkBytes + 1]byte */
partial struct largeScalar;

[GoType("[524289]ж<smallPointer>")] /* [runtime_internal_test_package.UserArenaChunkBytes /  unsafe.Sizeof(&smallPointer{})  (uintptr)8 + 1]ж<smallPointer> */
partial struct largePointer;

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static readonly @string allocˢ = "Alloc"u8;
internal static readonly @string structˢ = "struct{}"u8;
internal static readonly @string structˢ2 = "[]struct{}"u8;
internal static readonly @string intCap0ˢ = "[]int (cap 0)"u8;

public static void TestUserArena(ж<testing.T> Ꮡt) {
    GoFrame ᒐ = default;
    try {
        ref var t = ref Ꮡt.DerefOrNull();

        // Set GOMAXPROCS to 2 so we don't run too many of these
        // tests in parallel.
        defer(GOMAXPROCS, GOMAXPROCS(2), ref ᒐ);
        // Start a subtest so that we can clean up after any parallel tests within.
        Ꮡt.Run(allocˢ, (ж<testing.T> tΔ1) => {
            var ss = Ꮡ(new smallScalar(5));
            runSubTestUserArenaNew(tΔ1, ss, true);
            var sp = Ꮡ(new smallPointer(@new<smallPointer>()));
            runSubTestUserArenaNew(tΔ1, sp, true);
            var spm = Ꮡ(new smallPointerMix(sp, 5, nil, new byte[]{1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 11}.array()));
            runSubTestUserArenaNew(tΔ1, spm, true);
            var mse = @new<mediumScalarEven>();
            foreach (var (i, _) in mse.Value) {
                mse.Value[i] = 121;
            }
            runSubTestUserArenaNew(tΔ1, mse, true);
            var mso = @new<mediumScalarOdd>();
            foreach (var (i, _) in mso.Value) {
                mso.Value[i] = 122;
            }
            runSubTestUserArenaNew(tΔ1, mso, true);
            var mpe = @new<mediumPointerEven>();
            foreach (var (i, _) in mpe.Value) {
                mpe.Value[i] = sp;
            }
            runSubTestUserArenaNew(tΔ1, mpe, true);
            var mpo = @new<mediumPointerOdd>();
            foreach (var (i, _) in mpo.Value) {
                mpo.Value[i] = sp;
            }
            runSubTestUserArenaNew(tΔ1, mpo, true);
            var ls = @new<largeScalar>();
            foreach (var (i, _) in ls.Value) {
                ls.Value[i] = 123;
            }
            // Not in parallel because we don't want to hold this large allocation live.
            runSubTestUserArenaNew(tΔ1, ls, false);
            var lp = @new<largePointer>();
            foreach (var (i, _) in lp.Value) {
                lp.Value[i] = sp;
            }
            // Not in parallel because we don't want to hold this large allocation live.
            runSubTestUserArenaNew(tΔ1, lp, false);
            var sss = new slice<smallScalar>(25);
            foreach (var (i, _) in sss) {
                sss[i] = new smallScalar(12);
            }
            runSubTestUserArenaSlice(tΔ1, sss, true);
            var mpos = GoReflect.WithElemDims(new slice<mediumPointerOdd>(5), 1023);
            foreach (var (i, _) in mpos) {
                mpos[i] = mpo.Value.Clone();
            }
            runSubTestUserArenaSlice(tΔ1, mpos, true);
            var sps = new slice<smallPointer>((nint)(runtime_internal_test_package.UserArenaChunkBytes / /* unsafe.Sizeof(smallPointer{}) */ (uintptr)8 + 1));
            foreach (var (i, _) in sps) {
                sps[i] = sp.Value;
            }
            // Not in parallel because we don't want to hold this large allocation live.
            runSubTestUserArenaSlice(tΔ1, sps, false);
            // Test zero-sized types.
            tΔ1.Run(structˢ, (ж<testing.T> tΔ2) => {
                var arena = runtime_internal_test_package.NewUserArena();
                ref var x = ref heap<any>(out var Ꮡx);
                x = ((ж<EmptyStruct>)nil);
                arena.New(Ꮡx);
                {
                    @unsafe.Pointer v = @unsafe.Pointer.FromPinnedBox(x._<ж<EmptyStruct>>()); if (v != runtime_internal_test_package.ZeroBase) {
                        tΔ2.Errorf("expected zero-sized type to be allocated as zerobase: got %x, want %x"u8, @unsafe.Pointer.OrTypedNil(v), @unsafe.Pointer.OrTypedNil(runtime_internal_test_package.ZeroBase));
                    }
                }
                arena.Free();
            });
            tΔ1.Run(structˢ2, (ж<testing.T> tΔ3) => {
                var arena = runtime_internal_test_package.NewUserArena();
                ref var sl = ref heap<slice<EmptyStruct>>(out var Ꮡsl);
                arena.Slice(Ꮡsl, 10);
                {
                    @unsafe.Pointer v = @unsafe.Pointer.FromPinnedBox(Ꮡ(sl, 0)); if (v != runtime_internal_test_package.ZeroBase) {
                        tΔ3.Errorf("expected zero-sized type to be allocated as zerobase: got %x, want %x"u8, @unsafe.Pointer.OrTypedNil(v), @unsafe.Pointer.OrTypedNil(runtime_internal_test_package.ZeroBase));
                    }
                }
                arena.Free();
            });
            tΔ1.Run(intCap0ˢ, (ж<testing.T> tΔ4) => {
                var arena = runtime_internal_test_package.NewUserArena();
                ref var sl = ref heap<slice<nint>>(out var Ꮡsl);
                arena.Slice(Ꮡsl, 0);
                if (len(sl) != 0) {
                    tΔ4.Errorf("expected requested zero-sized slice to still have zero length: got %x, want 0"u8, len(sl));
                }
                arena.Free();
            });
        });
        // Run a GC cycle to get any arenas off the quarantine list.
        GC();
        {
            nint n = runtime_internal_test_package.GlobalWaitingArenaChunks(); if (n != 0) {
                Ꮡt.Errorf("expected zero waiting arena chunks, found %d"u8, n);
            }
        }
    }
    catch (Exception ᒐex) when (GoFrame.IsPanic(ᒐex, out PanicException? ᒐp)) { GoFrame.Capture(ᒐp); }
    finally { ᒐ.Run(); }
}

internal static void runSubTestUserArenaNew<S>(ж<testing.T> Ꮡt, ж<S> Ꮡvalue, bool parallel) {
    Ꮡt.Run(reflect.TypeOf(Ꮡvalue.OrTypedNil()).Elem().Name(), (ж<testing.T> tΔ1) => {
        if (parallel) {
            tΔ1.Parallel();
        }
        // Allocate and write data, enough to exhaust the arena.
        //
        // This is an underestimate, likely leaving some space in the arena. That's a good thing,
        // because it gives us coverage of boundary cases.
        nint n = (nint)(runtime_internal_test_package.UserArenaChunkBytes / @unsafe.Sizeof(Ꮡvalue.ValueSlot));
        if (n == 0) {
            n = 1;
        }
        // Create a new arena and do a bunch of operations on it.
        var arena = runtime_internal_test_package.NewUserArena();
        var arenaValues = new slice<ж<S>>(0, n);
        for (nint j = 0; j < n; j++) {
            ref var x = ref heap<any>(out var Ꮡx);
            x = ((ж<S>)nil);
            arena.New(Ꮡx);
            var s = x._<ж<S>>();
            s.ValueSlot = Ꮡvalue.ValueSlot;
            arenaValues = append(arenaValues, s);
        }
        // Check integrity of allocated data.
        foreach (var (_, s) in arenaValues) {
            if (!AreEqual(s.ValueSlot, Ꮡvalue.ValueSlot)) {
                tΔ1.Errorf("failed integrity check: got %#v, want %#v"u8, s.ValueSlot, Ꮡvalue.ValueSlot);
            }
        }
        // Release the arena.
        arena.Free();
    });
}

internal static void runSubTestUserArenaSlice<S>(ж<testing.T> Ꮡt, slice<S> value, bool parallel) {
    var valueʗ1 = value;
    Ꮡt.Run("[]"u8 + reflect.TypeOf(value).Elem().Name(), (ж<testing.T> tΔ1) => {
        if (parallel) {
            tΔ1.Parallel();
        }
        // Allocate and write data, enough to exhaust the arena.
        //
        // This is an underestimate, likely leaving some space in the arena. That's a good thing,
        // because it gives us coverage of boundary cases.
        nint n = (nint)(runtime_internal_test_package.UserArenaChunkBytes / (@unsafe.Sizeof(@new<S>().ValueSlot) * (uintptr)cap(valueʗ1)));
        if (n == 0) {
            n = 1;
        }
        // Create a new arena and do a bunch of operations on it.
        var arena = runtime_internal_test_package.NewUserArena();
        var arenaValues = new slice<slice<S>>(0, n);
        for (nint j = 0; j < n; j++) {
            ref var sl = ref heap<slice<S>>(out var Ꮡsl);
            arena.Slice(Ꮡsl, cap(valueʗ1));
            copy(sl, valueʗ1);
            arenaValues = append(arenaValues, sl);
        }
        // Check integrity of allocated data.
        foreach (var (_, sl) in arenaValues) {
            foreach (var (i, _) in sl) {
                var got = sl[i];
                var want = valueʗ1[i];
                if (!AreEqual(got, want)) {
                    tΔ1.Errorf("failed integrity check: got %#v, want %#v at index %d"u8, got, want, i);
                }
            }
        }
        // Release the arena.
        arena.Free();
    });
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static readonly @string freeˢ = "Free"u8;
internal static readonly @string finalizerˢ = "Finalizer"u8;

public static void TestUserArenaLiveness(ж<testing.T> Ꮡt) {
    Ꮡt.Run(freeˢ, (ж<testing.T> tΔ1) => {
        testUserArenaLiveness(tΔ1, false);
    });
    Ꮡt.Run(finalizerˢ, (ж<testing.T> tΔ2) => {
        testUserArenaLiveness(tΔ2, true);
    });
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static readonly object finalizedArenaReferencedˢ = (@string)"finalized arena-referenced object unexpectedly"u8;
internal static readonly object finalizerQueueWasNeverˢ = (@string)"finalizer queue was never emptied"u8;
internal static readonly object expectedArenaReferencedˢ = (@string)"expected arena-referenced object to be finalized"u8;

internal static void testUserArenaLiveness(ж<testing.T> Ꮡt, bool useArenaFinalizer) {
    GoFrame ᒐ = default;
    try {
        // Disable the GC so that there's zero chance we try doing anything arena related *during*
        // a mark phase, since otherwise a bunch of arenas could end up on the fault list.
        defer(Δdebug.SetGCPercent, Δdebug.SetGCPercent(-1), ref ᒐ);
        // Defensively ensure that any full arena chunks leftover from previous tests have been cleared.
        GC();
        GC();
        var arena = runtime_internal_test_package.NewUserArena();
        // Allocate a few pointer-ful but un-initialized objects so that later we can
        // place a reference to heap object at a more interesting location.
        for (nint i = 0; i < 3; i++) {
            ref var xΔ1 = ref heap<any>(out var ᏑxΔ1);
            xΔ1 = ж<mediumPointerOdd>.NilBoxOfDims(1023L);
            arena.New(ᏑxΔ1);
        }
        ref var x = ref heap<any>(out var Ꮡx);
        x = ((ж<smallPointerMix>)nil);
        arena.New(Ꮡx);
        var v = x._<ж<smallPointerMix>>();
        ref var safeToFinalize = ref heap(new atomic.Bool(), out var ᏑsafeToFinalize);
        ref var finalized = ref heap(new atomic.Bool(), out var Ꮡfinalized);
        v.Value.C = @new<smallPointer>();
        SetFinalizer((~v).C.OrTypedNil(), (ж<smallPointer> _) => {
            if (!ᏑsafeToFinalize.Load()) {
                Ꮡt.Error(finalizedArenaReferencedˢ);
            }
            Ꮡfinalized.Store(true);
        });
        // Make sure it stays alive.
        GC();
        GC();
        // In order to ensure the object can be freed, we now need to make sure to use
        // the entire arena. Exhaust the rest of the arena.
        for (nint i = 0; i < (nint)(runtime_internal_test_package.UserArenaChunkBytes / /* unsafe.Sizeof(mediumScalarEven{}) */ (uintptr)8192); i++) {
            ref var xΔ2 = ref heap<any>(out var ᏑxΔ2);
            xΔ2 = ж<mediumScalarEven>.NilBoxOfDims(8192L);
            arena.New(ᏑxΔ2);
        }
        // Make sure it stays alive again.
        GC();
        GC();
        v = default!;
        ᏑsafeToFinalize.Store(true);
        if (useArenaFinalizer){
            arena = default!;
            // Try to queue the arena finalizer.
            GC();
            GC();
            // In order for the finalizer we actually want to run to execute,
            // we need to make sure this one runs first.
            if (!runtime_internal_test_package.BlockUntilEmptyFinalizerQueue((int64)(2 * time.ΔSecond))) {
                Ꮡt.Fatal(finalizerQueueWasNeverˢ);
            }
        } else {
            // Free the arena explicitly.
            arena.Free();
        }
        // Try to queue the object's finalizer that we set earlier.
        GC();
        GC();
        if (!runtime_internal_test_package.BlockUntilEmptyFinalizerQueue((int64)(2 * time.ΔSecond))) {
            Ꮡt.Fatal(finalizerQueueWasNeverˢ);
        }
        if (!Ꮡfinalized.Load()) {
            Ꮡt.Error(expectedArenaReferencedˢ);
        }
    }
    catch (Exception ᒐex) when (GoFrame.IsPanic(ᒐex, out PanicException? ᒐp)) { GoFrame.Capture(ᒐp); }
    finally { ᒐ.Run(); }
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static readonly object heapAllocationKeptAliveˢ = (@string)"heap allocation kept alive through non-pointer reference"u8;

public static void TestUserArenaClearsPointerBits(ж<testing.T> Ꮡt) {
    ref var t = ref Ꮡt.DerefOrNull();

    // This is a regression test for a serious issue wherein if pointer bits
    // aren't properly cleared, it's possible to allocate scalar data down
    // into a previously pointer-ful area, causing misinterpretation by the GC.
    // Create a large object, grab a pointer into it, and free it.
    var x = Ꮡ(new array<byte>(8388608));
    var xp = (uintptr)x.at<byte>(124);
    ref var finalized = ref heap(new atomic.Bool(), out var Ꮡfinalized);
    SetFinalizer(x.OrTypedNil(), ([GoArrayDims(8388608)] ж<array<byte>> _) => {
        Ꮡfinalized.Store(true);
    });
    // Write three chunks worth of pointer data. Three gives us a
    // high likelihood that when we write 2 later, we'll get the behavior
    // we want.
    var a = runtime_internal_test_package.NewUserArena();
    for (nint i = 0; i < (nint)((uintptr)(runtime_internal_test_package.UserArenaChunkBytes / (uintptr)goarch.PtrSize) * 3); i++) {
        ref var xΔ1 = ref heap<any>(out var ᏑxΔ1);
        xΔ1 = ((ж<smallPointer>)nil);
        a.New(ᏑxΔ1);
    }
    a.Free();
    // Recycle the arena chunks.
    GC();
    GC();
    a = runtime_internal_test_package.NewUserArena();
    for (nint i = 0; i < (nint)((uintptr)(runtime_internal_test_package.UserArenaChunkBytes / (uintptr)goarch.PtrSize) * 2); i++) {
        ref var xΔ2 = ref heap<any>(out var ᏑxΔ2);
        xΔ2 = ((ж<smallScalar>)nil);
        a.New(ᏑxΔ2);
        var v = xΔ2._<ж<smallScalar>>();
        // Write a pointer that should not keep x alive.
        v.Value = new smallScalar(xp);
    }
    KeepAlive(x.OrTypedNil());
    x = ж<array<byte>>.NilBoxOfDims(8388608L);
    // Try to free x.
    GC();
    GC();
    if (!runtime_internal_test_package.BlockUntilEmptyFinalizerQueue((int64)(2 * time.ΔSecond))) {
        Ꮡt.Fatal(finalizerQueueWasNeverˢ);
    }
    if (!Ꮡfinalized.Load()) {
        Ꮡt.Fatal(heapAllocationKeptAliveˢ);
    }
    // Clean up the arena.
    a.Free();
    GC();
    GC();
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static readonly @string abcdefghijˢ = "abcdefghij"u8;
internal static readonly object cloneDidNotMakeACopyˢ = (@string)"Clone did not make a copy"u8;
internal static readonly object cloneShouldNotHaveMadeAˢ = (@string)"Clone should not have made a copy"u8;

public static void TestUserArenaCloneString(ж<testing.T> Ꮡt) {
    ref var t = ref Ꮡt.DerefOrNull();

    var a = runtime_internal_test_package.NewUserArena();
    // A static string (not on heap or arena)
    @string s = abcdefghijˢ;
    // Create a byte slice in the arena, initialize it with s
    ref var b = ref heap<slice<byte>>(out var Ꮡb);
    a.Slice(Ꮡb, len(s));
    copy(b, s);
    // Create a string as using the same memory as the byte slice, hence in
    // the arena. This could be an arena API, but hasn't really been needed
    // yet.
    @string @as = @unsafe.String(Ꮡ(b, 0), len(b));
    // Clone should make a copy of as, since it is in the arena.
    @string asCopy = runtime_internal_test_package.UserArenaClone(@as);
    if (@unsafe.StringData(@as) == @unsafe.StringData(asCopy)) {
        Ꮡt.Error(cloneDidNotMakeACopyˢ);
    }
    // Clone should make a copy of subAs, since subAs is just part of as and so is in the arena.
    @string subAs = @as[1..3];
    @string subAsCopy = runtime_internal_test_package.UserArenaClone(subAs);
    if (@unsafe.StringData(subAs) == @unsafe.StringData(subAsCopy)) {
        Ꮡt.Error(cloneDidNotMakeACopyˢ);
    }
    if (len(subAs) != len(subAsCopy)){
        Ꮡt.Errorf("Clone made an incorrect copy (bad length): %d -> %d"u8, len(subAs), len(subAsCopy));
    } else {
        foreach (var (i, _) in subAs) {
            if (subAs[i] != subAsCopy[i]) {
                Ꮡt.Errorf("Clone made an incorrect copy (data at index %d): %d -> %d"u8, i, subAs[i], subAs[i]);
            }
        }
    }
    // Clone should not make a copy of doubleAs, since doubleAs will be on the heap.
    @string doubleAs = @as + @as;
    @string doubleAsCopy = runtime_internal_test_package.UserArenaClone(doubleAs);
    if (@unsafe.StringData(doubleAs) != @unsafe.StringData(doubleAsCopy)) {
        Ꮡt.Error(cloneShouldNotHaveMadeAˢ);
    }
    // Clone should not make a copy of s, since s is a static string.
    @string sCopy = runtime_internal_test_package.UserArenaClone(s);
    if (@unsafe.StringData(s) != @unsafe.StringData(sCopy)) {
        Ꮡt.Error(cloneShouldNotHaveMadeAˢ);
    }
    a.Free();
}

public static void TestUserArenaClonePointer(ж<testing.T> Ꮡt) {
    var a = runtime_internal_test_package.NewUserArena();
    // Clone should not make a copy of a heap-allocated smallScalar.
    var x = runtime_internal_test_package.Escape(@new<smallScalar>());
    var xCopy = runtime_internal_test_package.UserArenaClone(x);
    if (@unsafe.Pointer.FromPinnedBox(x) != @unsafe.Pointer.FromPinnedBox(xCopy)) {
        Ꮡt.Errorf("Clone should not have made a copy: %#v -> %#v"u8, x.OrTypedNil(), xCopy.OrTypedNil());
    }
    // Clone should make a copy of an arena-allocated smallScalar.
    ref var i = ref heap<any>(out var Ꮡi);
    i = ((ж<smallScalar>)nil);
    a.New(Ꮡi);
    var xArena = i._<ж<smallScalar>>();
    var xArenaCopy = runtime_internal_test_package.UserArenaClone(xArena);
    if (@unsafe.Pointer.FromPinnedBox(xArena) == @unsafe.Pointer.FromPinnedBox(xArenaCopy)) {
        Ꮡt.Errorf("Clone should have made a copy: %#v -> %#v"u8, xArena.OrTypedNil(), xArenaCopy.OrTypedNil());
    }
    if (xArena.Value != xArenaCopy.Value) {
        Ꮡt.Errorf("Clone made an incorrect copy copy: %#v -> %#v"u8, xArena.Value, xArenaCopy.Value);
    }
    a.Free();
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static readonly @string klmnopqrstuvˢ = "klmnopqrstuv"u8;

public static void TestUserArenaCloneSlice(ж<testing.T> Ꮡt) {
    var a = runtime_internal_test_package.NewUserArena();
    // A static string (not on heap or arena)
    @string s = klmnopqrstuvˢ;
    // Create a byte slice in the arena, initialize it with s
    ref var b = ref heap<slice<byte>>(out var Ꮡb);
    a.Slice(Ꮡb, len(s));
    copy(b, s);
    // Clone should make a copy of b, since it is in the arena.
    var bCopy = runtime_internal_test_package.UserArenaClone(b);
    if (@unsafe.Pointer.FromPinnedBox(Ꮡ(b, 0)) == @unsafe.Pointer.FromPinnedBox(Ꮡ(bCopy, 0))) {
        Ꮡt.Errorf("Clone did not make a copy: %#v -> %#v"u8, b, bCopy);
    }
    if (len(b) != len(bCopy)){
        Ꮡt.Errorf("Clone made an incorrect copy (bad length): %d -> %d"u8, len(b), len(bCopy));
    } else {
        foreach (var (i, _) in b) {
            if (b[i] != bCopy[i]) {
                Ꮡt.Errorf("Clone made an incorrect copy (data at index %d): %d -> %d"u8, i, b[i], bCopy[i]);
            }
        }
    }
    // Clone should make a copy of bSub, since bSub is just part of b and so is in the arena.
    var bSub = b[1..3];
    var bSubCopy = runtime_internal_test_package.UserArenaClone(bSub);
    if (@unsafe.Pointer.FromPinnedBox(Ꮡ(bSub, 0)) == @unsafe.Pointer.FromPinnedBox(Ꮡ(bSubCopy, 0))) {
        Ꮡt.Errorf("Clone did not make a copy: %#v -> %#v"u8, bSub, bSubCopy);
    }
    if (len(bSub) != len(bSubCopy)){
        Ꮡt.Errorf("Clone made an incorrect copy (bad length): %d -> %d"u8, len(bSub), len(bSubCopy));
    } else {
        foreach (var (i, _) in bSub) {
            if (bSub[i] != bSubCopy[i]) {
                Ꮡt.Errorf("Clone made an incorrect copy (data at index %d): %d -> %d"u8, i, bSub[i], bSubCopy[i]);
            }
        }
    }
    // Clone should not make a copy of bNotArena, since it will not be in an arena.
    var bNotArena = new slice<byte>(len(s));
    copy(bNotArena, s);
    var bNotArenaCopy = runtime_internal_test_package.UserArenaClone(bNotArena);
    if (@unsafe.Pointer.FromPinnedBox(Ꮡ(bNotArena, 0)) != @unsafe.Pointer.FromPinnedBox(Ꮡ(bNotArenaCopy, 0))) {
        Ꮡt.Error(cloneShouldNotHaveMadeAˢ);
    }
    a.Free();
}

public static void TestUserArenaClonePanic(ж<testing.T> Ꮡt) {
    @string s = default!;
    ((Action)(() => {
        GoFrame ᒐ = default;
        try {
            var x = new smallScalar(2);
            defer(() => {
                {
                    var v = recover(); if (v != default!) {
                        s = v._<@string>();
                    }
                }
            }, ref ᒐ);
            runtime_internal_test_package.UserArenaClone(x);
        }
        catch (Exception ᒐex) when (GoFrame.IsPanic(ᒐex, out PanicException? ᒐp)) { GoFrame.Capture(ᒐp); }
        finally { ᒐ.Run(); }
    }))();
    if (s == ""u8) {
        Ꮡt.Errorf("expected panic from Clone"u8);
    }
}

} // end runtime_test_package
