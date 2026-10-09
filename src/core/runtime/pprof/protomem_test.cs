// Copyright 2016 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.
namespace go.runtime;

using bytes = bytes_package;
using fmt = fmt_package;
using asan = @internal.asan_package;
using profile = @internal.profile_package;
using profilerecord = @internal.profilerecord_package;
using testenv = @internal.testenv_package;
using runtime = runtime_package;
using slices = slices_package;
using strings = strings_package;
using testing = testing_package;
using @internal;
using MethodImplAttribute = global::System.Runtime.CompilerServices.MethodImplAttribute;
using MethodImplOptions = global::System.Runtime.CompilerServices.MethodImplOptions;
using io = io_package;
using static go.runtime.pprof_package;

partial class pprof_internal_test_package {

internal partial struct TestConvertMemProfile_type /*dyn*/ {
    internal @string name;
    internal @string defaultSampleType;
}

public static void TestConvertMemProfile(ж<testing.T> Ꮡt) {
    var (addr1, addr2, map1, map2) = testPCs(Ꮡt);
    // MemProfileRecord stacks are return PCs, so add one to the
    // addresses recorded in the "profile". The proto profile
    // locations are call PCs, so conversion will subtract one
    // from these and get back to addr1 and addr2.
    var (a1, a2) = ((uintptr)addr1 + 1, (uintptr)addr2 + 1);
    var rate = (int64)(512 * 1024);
    var rec = new profilerecord.MemProfileRecord[]{
        new(AllocBytes: 4096, FreeBytes: 1024, AllocObjects: 4, FreeObjects: 1, Stack: new uintptr[]{a1, a2}.slice()),
        new(AllocBytes: 512 * 1024, FreeBytes: 0, AllocObjects: 1, FreeObjects: 0, Stack: new uintptr[]{a2 + 1, a2 + 2}.slice()),
        new(AllocBytes: 512 * 1024, FreeBytes: 512 * 1024, AllocObjects: 1, FreeObjects: 1, Stack: new uintptr[]{a1 + 1, a1 + 2, a2 + 3}.slice())
    }.slice();
    var periodType = Ꮡ(new profile.ValueType(Type: "space"u8, Unit: "bytes"u8));
    var sampleType = new ж<profile.ValueType>[]{
        Ꮡ(new profile.ValueType(Type: "alloc_objects"u8, Unit: "count"u8)),
        Ꮡ(new profile.ValueType(Type: "alloc_space"u8, Unit: "bytes"u8)),
        Ꮡ(new profile.ValueType(Type: "inuse_objects"u8, Unit: "count"u8)),
        Ꮡ(new profile.ValueType(Type: "inuse_space"u8, Unit: "bytes"u8))
    }.slice();
    var samples = new ж<profile.Sample>[]{
        Ꮡ(new profile.Sample(
            Value: new int64[]{2050, 2099200, 1537, 1574400}.slice(),
            Location: new ж<profile.Location>[]{
                Ꮡ(new profile.Location(ID: 1, Mapping: map1, Address: addr1)),
                Ꮡ(new profile.Location(ID: 2, Mapping: map2, Address: addr2))
            }.slice(),
            NumLabel: new map<@string, slice<int64>>{["bytes"u8] = new int64[]{1024}.slice()})),
        Ꮡ(new profile.Sample(
            Value: new int64[]{1, 829411, 1, 829411}.slice(),
            Location: new ж<profile.Location>[]{
                Ꮡ(new profile.Location(ID: 3, Mapping: map2, Address: addr2 + 1)),
                Ꮡ(new profile.Location(ID: 4, Mapping: map2, Address: addr2 + 2))
            }.slice(),
            NumLabel: new map<@string, slice<int64>>{["bytes"u8] = new int64[]{512 * 1024}.slice()})),
        Ꮡ(new profile.Sample(
            Value: new int64[]{1, 829411, 0, 0}.slice(),
            Location: new ж<profile.Location>[]{
                Ꮡ(new profile.Location(ID: 5, Mapping: map1, Address: addr1 + 1)),
                Ꮡ(new profile.Location(ID: 6, Mapping: map1, Address: addr1 + 2)),
                Ꮡ(new profile.Location(ID: 7, Mapping: map2, Address: addr2 + 3))
            }.slice(),
            NumLabel: new map<@string, slice<int64>>{["bytes"u8] = new int64[]{512 * 1024}.slice()}))
    }.slice();
    foreach (var (_, vᴛ1) in new TestConvertMemProfile_type[]{
        new("heap"u8, ""u8),
        new("allocs"u8, "alloc_space"u8)
    }.slice()) {
        ref var tc = ref heap(new TestConvertMemProfile_type(), out var Ꮡtc);
        tc = vᴛ1;

        var periodTypeʗ1 = periodType;
        var recʗ1 = rec;
        var sampleTypeʗ1 = sampleType;
        var samplesʗ1 = samples;
        var tcʗ1 = tc;
        Ꮡt.Run(tc.name, (ж<testing.T> tΔ1) => {
            ref var buf = ref heap(new bytes.Buffer(), out var Ꮡbuf);
            {
                var errΔ1 = writeHeapProto(new pprof_internal_test_package.bytes_BufferжWriter(Ꮡbuf), recʗ1, rate, tcʗ1.defaultSampleType); if (errΔ1 != default!) {
                    tΔ1.Fatalf("writing profile: %v"u8, errΔ1);
                }
            }
            var (p, err) = profile.Parse(new pprof_internal_test_package.bytes_BufferжReader(Ꮡbuf));
            if (err != default!) {
                tΔ1.Fatalf("profile.Parse: %v"u8, err);
            }
            checkProfile(tΔ1, p, rate, periodTypeʗ1, sampleTypeʗ1, samplesʗ1, tcʗ1.defaultSampleType);
        });
    }
}

internal static partial slice<T> genericAllocFunc<T>(nint n)
    where T : /* interface{uint32 | uint64} */ IAdditionOperators<T, T, T>, ISubtractionOperators<T, T, T>, IMultiplyOperators<T, T, T>, IDivisionOperators<T, T, T>, IIncrementOperators<T>, IDecrementOperators<T>, IUnaryNegationOperators<T, T>, IModulusOperators<T, T, T>, IBitwiseOperators<T, T, T>, IShiftOperators<T, int, T>, IEqualityOperators<T, T, bool>, IComparisonOperators<T, T, bool>, new()
{
    return new slice<T>(n);
}

internal static slice<@string> profileToStrings(ж<profile.Profile> Ꮡp) {
    ref var p = ref Ꮡp.DerefOrNull();

    slice<@string> res = default!;
    foreach (var (_, s) in p.Sample) {
        res = append(res, sampleToString(s));
    }
    return res;
}

internal static @string sampleToString(ж<profile.Sample> Ꮡs) {
    ref var s = ref Ꮡs.DerefOrNull();

    slice<@string> funcs = default!;
    for (nint i = len(s.Location) - 1; i >= 0; i--) {
        var loc = s.Location[i];
        funcs = locationToStrings(loc, funcs);
    }
    return fmt.Sprintf("%s %v"u8, strings_package.Join(funcs, ";"u8), s.Value);
}

internal static slice<@string> locationToStrings(ж<profile.Location> Ꮡloc, slice<@string> funcs) {
    ref var loc = ref Ꮡloc.DerefOrNull();

    foreach (var (j, _) in loc.Line) {
        var line = loc.Line[len(loc.Line) - 1 - j];
        funcs = append(funcs, (~line.Function).Name);
    }
    return funcs;
}

// This is a regression test for https://go.dev/issue/64528.
public static void TestGenericsHashKeyInPprofBuilder(ж<testing.T> Ꮡt) {
    GoFrame ᒐ = default;
    try {
        if (asan.Enabled) {
            Ꮡt.Skip(extraAllocationsWithAsanˢ);
        }
        nint previousRate = runtime.MemProfileRate;
        runtime.MemProfileRate = 1;
        defer(() => {
            runtime.MemProfileRate = previousRate;
        }, ref ᒐ);
        foreach (var (_, sz) in new nint[]{128, 256}.slice()) {
            genericAllocFunc<uint32>(sz / 4);
        }
        foreach (var (_, sz) in new nint[]{32, 64}.slice()) {
            genericAllocFunc<uint64>(sz / 8);
        }
        runtime.GC();
        var buf = bytes.NewBuffer(default!);
        {
            var errΔ1 = WriteHeapProfile(new pprof_internal_test_package.bytes_BufferжWriter(buf)); if (errΔ1 != default!) {
                Ꮡt.Fatalf("writing profile: %v"u8, errΔ1);
            }
        }
        var (p, err) = profile.Parse(new pprof_internal_test_package.bytes_BufferжReader(buf));
        if (err != default!) {
            Ꮡt.Fatalf("profile.Parse: %v"u8, err);
        }
        var actual = profileToStrings(p);
        var expected = new @string[]{
            "testing.tRunner;runtime/pprof.TestGenericsHashKeyInPprofBuilder;runtime/pprof.genericAllocFunc[go.shape.uint32] [1 128 0 0]"u8,
            "testing.tRunner;runtime/pprof.TestGenericsHashKeyInPprofBuilder;runtime/pprof.genericAllocFunc[go.shape.uint32] [1 256 0 0]"u8,
            "testing.tRunner;runtime/pprof.TestGenericsHashKeyInPprofBuilder;runtime/pprof.genericAllocFunc[go.shape.uint64] [1 32 0 0]"u8,
            "testing.tRunner;runtime/pprof.TestGenericsHashKeyInPprofBuilder;runtime/pprof.genericAllocFunc[go.shape.uint64] [1 64 0 0]"u8
        }.slice();
        foreach (var (_, l) in expected) {
            if (!slices.Contains(actual, l)) {
                Ꮡt.Errorf("profile = %v\nwant = %v"u8, strings_package.Join(actual, "\n"u8), l);
            }
        }
    }
    catch (Exception ᒐex) when (GoFrame.IsPanic(ᒐex, out PanicException? ᒐp)) { GoFrame.Capture(ᒐp); }
    finally { ᒐ.Run(); }
}

internal partial struct opAlloc {
    internal array<byte> buf = new(128);
}

internal partial struct opCall {
}

internal static slice<byte> sink;

internal static partial void storeAlloc() {
    sink = new slice<byte>(16);
}

internal static partial void nonRecursiveGenericAllocFunction<CurrentOp, OtherOp>(bool alloc) {
    if (alloc){
        storeAlloc();
    } else {
        nonRecursiveGenericAllocFunction<OtherOp, CurrentOp>(true);
    }
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static readonly object skippingTestWithˢ = (@string)"skipping test with optimizations disabled"u8;

// Hoisted Go string constant (single allocation; Go keeps it in RODATA)
internal static readonly @string expectedSampleᶜ = "testing.tRunner;runtime/pprof.TestGenericsInlineLocations;runtime/pprof.nonRecursiveGenericAllocFunction[go.shape.struct {},go.shape.struct { runtime/pprof.buf [128]uint8 }];runtime/pprof.nonRecursiveGenericAllocFunction[go.shape.struct { runtime/pprof.buf [128]uint8 },go.shape.struct {}];runtime/pprof.storeAlloc [1 16 1 16]"u8;

// Hoisted Go string constant (single allocation; Go keeps it in RODATA)
internal static readonly @string expectedLocationᶜ = "runtime/pprof.nonRecursiveGenericAllocFunction[go.shape.struct {},go.shape.struct { runtime/pprof.buf [128]uint8 }];runtime/pprof.nonRecursiveGenericAllocFunction[go.shape.struct { runtime/pprof.buf [128]uint8 },go.shape.struct {}];runtime/pprof.storeAlloc"u8;

// Hoisted Go string constant (single allocation; Go keeps it in RODATA)
internal static readonly @string expectedLocationNewInlinerᶜ = "runtime/pprof.TestGenericsInlineLocations;runtime/pprof.nonRecursiveGenericAllocFunction[go.shape.struct {},go.shape.struct { runtime/pprof.buf [128]uint8 }];runtime/pprof.nonRecursiveGenericAllocFunction[go.shape.struct { runtime/pprof.buf [128]uint8 },go.shape.struct {}];runtime/pprof.storeAlloc";

public static void TestGenericsInlineLocations(ж<testing.T> Ꮡt) {
    GoFrame ᒐ = default;
    try {
        if (asan.Enabled) {
            Ꮡt.Skip(extraAllocationsWithAsanˢ);
        }
        if (testenv.OptimizationOff()) {
            Ꮡt.Skip(skippingTestWithˢ);
        }
        nint previousRate = runtime.MemProfileRate;
        runtime.MemProfileRate = 1;
        defer(() => {
            runtime.MemProfileRate = previousRate;
            sink = default!;
        }, ref ᒐ);
        nonRecursiveGenericAllocFunction<opAlloc, opCall>(true);
        nonRecursiveGenericAllocFunction<opCall, opAlloc>(false);
        runtime.GC();
        var buf = bytes.NewBuffer(default!);
        {
            var errΔ1 = WriteHeapProfile(new pprof_internal_test_package.bytes_BufferжWriter(buf)); if (errΔ1 != default!) {
                Ꮡt.Fatalf("writing profile: %v"u8, errΔ1);
            }
        }
        var (p, err) = profile.Parse(new pprof_internal_test_package.bytes_BufferжReader(buf));
        if (err != default!) {
            Ꮡt.Fatalf("profile.Parse: %v"u8, err);
        }
        @string expectedSample = expectedSampleᶜ;
        @string expectedLocation = expectedLocationᶜ;
        @string expectedLocationNewInliner = expectedLocationNewInlinerᶜ;
        ж<profile.Sample> s = default!;
        foreach (var (_, sample) in (~p).Sample) {
            if (sampleToString(sample) == expectedSample) {
                s = sample;
                break;
            }
        }
        if (s == nil) {
            Ꮡt.Fatalf("expected \n%s\ngot\n%s"u8, expectedSample, strings_package.Join(profileToStrings(p), "\n"u8));
        }
        var loc = (~s).Location[0];
        @string actual = strings_package.Join(locationToStrings(loc, default!), ";"u8);
        if (expectedLocation != actual && expectedLocationNewInliner != actual) {
            Ꮡt.Errorf("expected a location with at least 3 functions\n%s\ngot\n%s\n"u8, expectedLocation, actual);
        }
    }
    catch (Exception ᒐex) when (GoFrame.IsPanic(ᒐex, out PanicException? ᒐp)) { GoFrame.Capture(ᒐp); }
    finally { ᒐ.Run(); }
}

internal static void growMap() {
    var m = new map<nint, nint>();
    foreach (var i in range(512)) {
        m[i] = i;
    }
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static readonly @string runtimePprofGrowMapˢ = "runtime/pprof.growMap"u8;
internal static readonly @string runtimeInternalˢ = "runtime/internal/"u8;
internal static readonly @string mapassignˢ = "mapassign"u8;

// Runtime frames are hidden in heap profiles.
// This is a regression test for https://go.dev/issue/71174.
public static void TestHeapRuntimeFrames(ж<testing.T> Ꮡt) {
    GoFrame ᒐ = default;
    try {
        nint previousRate = runtime.MemProfileRate;
        runtime.MemProfileRate = 1;
        defer(() => {
            runtime.MemProfileRate = previousRate;
        }, ref ᒐ);
        growMap();
        runtime.GC();
        var buf = bytes.NewBuffer(default!);
        {
            var errΔ1 = WriteHeapProfile(new pprof_internal_test_package.bytes_BufferжWriter(buf)); if (errΔ1 != default!) {
                Ꮡt.Fatalf("writing profile: %v"u8, errΔ1);
            }
        }
        var (p, err) = profile.Parse(new pprof_internal_test_package.bytes_BufferжReader(buf));
        if (err != default!) {
            Ꮡt.Fatalf("profile.Parse: %v"u8, err);
        }
        var actual = profileToStrings(p);
        // We must see growMap at least once.
        var foundGrowMap = false;
        foreach (var (_, l) in actual) {
            if (!strings_package.Contains(l, runtimePprofGrowMapˢ)) {
                continue;
            }
            foundGrowMap = true;
            // Runtime frames like mapassign and map internals should be hidden.
            if (strings_package.Contains(l, runtimeˢ)) {
                Ꮡt.Errorf("Sample got %s, want no runtime frames"u8, l);
            }
            if (strings_package.Contains(l, internalRuntimeˢ)) {
                Ꮡt.Errorf("Sample got %s, want no runtime frames"u8, l);
            }
            if (strings_package.Contains(l, runtimeInternalˢ)) {
                Ꮡt.Errorf("Sample got %s, want no runtime frames"u8, l);
            }
            if (strings_package.Contains(l, mapassignˢ)) {
                // in case mapassign moves to a package not matching above paths.
                Ꮡt.Errorf("Sample got %s, want no mapassign frames"u8, l);
            }
        }
        if (!foundGrowMap) {
            Ꮡt.Errorf("Profile got:\n%s\nwant sample in runtime/pprof.growMap"u8, strings_package.Join(actual, "\n"u8));
        }
    }
    catch (Exception ᒐex) when (GoFrame.IsPanic(ᒐex, out PanicException? ᒐp)) { GoFrame.Capture(ᒐp); }
    finally { ᒐ.Run(); }
}

} // end pprof_internal_test_package
