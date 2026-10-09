// Copyright 2014 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.
//go:build !js
namespace go.runtime;

using bytes = bytes_package;
using fmt = fmt_package;
using asan = @internal.asan_package;
using profile = @internal.profile_package;
using reflect = reflect_package;
using regexp = regexp_package;
using runtime = runtime_package;
using testing = testing_package;
using @unsafe = unsafe_package;
using @internal;
using MethodImplAttribute = global::System.Runtime.CompilerServices.MethodImplAttribute;
using MethodImplOptions = global::System.Runtime.CompilerServices.MethodImplOptions;
using io = io_package;
using static go.runtime.pprof_package;

partial class pprof_internal_test_package {

internal static any memSink;

internal partial struct allocateTransient1M_type /*dyn*/ {
    internal array<byte> x = new(1024);
}

internal static void allocateTransient1M() {
    for (nint i = 0; i < 1024; i++) {
        memSink = Ꮡ(new allocateTransient1M_type());
    }
}

//go:noinline
internal static partial void allocateTransient2M() {
    memSink = new slice<byte>((2 << (int)(20)));
}

internal static partial void allocateTransient2MInline() {
    memSink = new slice<byte>((2 << (int)(20)));
}

public partial struct Obj32 {
    internal ж<Obj32> link;
    internal array<byte> pad = new(32 - /* unsafe.Sizeof(uintptr(0)) */ (uintptr)8);
}

internal static ж<Obj32> persistentMemSink;

internal static void allocatePersistent1K() {
    for (nint i = 0; i < 32; i++) {
        // Can't use slice because that will introduce implicit allocations.
        var obj = Ꮡ(new Obj32(link: persistentMemSink));
        persistentMemSink = obj;
    }
}

// Allocate transient memory using reflect.Call.
internal static partial void allocateReflectTransient() {
    memSink = new slice<byte>((2 << (int)(20)));
}

internal static void allocateReflect() {
    var rv = reflect.ValueOf(allocateReflectTransient);
    rv.Call(default!);
}

internal static nint memoryProfilerRun = 0;

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static readonly object extraAllocationsWithAsanˢ = (@string)"extra allocations with -asan throw off the test; see #70079"u8;
internal static readonly @string debug1ˢ = "debug=1"u8;
internal static readonly @string heapˢ = "heap"u8;
internal static readonly @string protoˢ = "proto"u8;

internal partial struct TestMemoryProfiler_tests /*dyn*/ {
    internal slice<@string> stk;
    internal @string legacy;
}

public static void TestMemoryProfiler(ж<testing.T> Ꮡt) {
    GoFrame ᒐ = default;
    try {
        ref var t = ref Ꮡt.DerefOrNull();

        if (asan.Enabled) {
            Ꮡt.Skip(extraAllocationsWithAsanˢ);
        }
        // Disable sampling, otherwise it's difficult to assert anything.
        nint oldRate = runtime.MemProfileRate;
        runtime.MemProfileRate = 1;
        defer(() => {
            runtime.MemProfileRate = oldRate;
        }, ref ᒐ);
        // Allocate a meg to ensure that mcache.nextSample is updated to 1.
        for (nint i = 0; i < 1024; i++) {
            memSink = new slice<byte>(1024);
        }
        // Do the interesting allocations.
        allocateTransient1M();
        allocateTransient2M();
        allocateTransient2MInline();
        allocatePersistent1K();
        allocateReflect();
        memSink = default!;
        runtime.GC(); // materialize stats
        memoryProfilerRun++;
        var tests = new TestMemoryProfiler_tests[]{new(
            stk: new @string[]{"runtime/pprof.allocatePersistent1K"u8, "runtime/pprof.TestMemoryProfiler"u8}.slice(),
            legacy: fmt.Sprintf("""
%v: %v \[%v: %v\] @ 0x[0-9,a-f]+ 0x[0-9,a-f]+ 0x[0-9,a-f]+ 0x[0-9,a-f]+
#	0x[0-9,a-f]+	runtime/pprof\.allocatePersistent1K\+0x[0-9,a-f]+	.*runtime/pprof/mprof_test\.go:48
#	0x[0-9,a-f]+	runtime/pprof\.TestMemoryProfiler\+0x[0-9,a-f]+	.*runtime/pprof/mprof_test\.go:87

"""u8, 32 * memoryProfilerRun, 1024 * memoryProfilerRun, 32 * memoryProfilerRun, 1024 * memoryProfilerRun)
        ), new(
            stk: new @string[]{"runtime/pprof.allocateTransient1M"u8, "runtime/pprof.TestMemoryProfiler"u8}.slice(),
            legacy: fmt.Sprintf("""
0: 0 \[%v: %v\] @ 0x[0-9,a-f]+ 0x[0-9,a-f]+ 0x[0-9,a-f]+ 0x[0-9,a-f]+
#	0x[0-9,a-f]+	runtime/pprof\.allocateTransient1M\+0x[0-9,a-f]+	.*runtime/pprof/mprof_test.go:25
#	0x[0-9,a-f]+	runtime/pprof\.TestMemoryProfiler\+0x[0-9,a-f]+	.*runtime/pprof/mprof_test.go:84

"""u8, ((1 << (int)(10))) * memoryProfilerRun, ((1 << (int)(20))) * memoryProfilerRun)
        ), new(
            stk: new @string[]{"runtime/pprof.allocateTransient2M"u8, "runtime/pprof.TestMemoryProfiler"u8}.slice(),
            legacy: fmt.Sprintf("""
0: 0 \[%v: %v\] @ 0x[0-9,a-f]+ 0x[0-9,a-f]+ 0x[0-9,a-f]+ 0x[0-9,a-f]+
#	0x[0-9,a-f]+	runtime/pprof\.allocateTransient2M\+0x[0-9,a-f]+	.*runtime/pprof/mprof_test.go:31
#	0x[0-9,a-f]+	runtime/pprof\.TestMemoryProfiler\+0x[0-9,a-f]+	.*runtime/pprof/mprof_test.go:85

"""u8, memoryProfilerRun, ((2 << (int)(20))) * memoryProfilerRun)
        ), new(
            stk: new @string[]{"runtime/pprof.allocateTransient2MInline"u8, "runtime/pprof.TestMemoryProfiler"u8}.slice(),
            legacy: fmt.Sprintf("""
0: 0 \[%v: %v\] @ 0x[0-9,a-f]+ 0x[0-9,a-f]+ 0x[0-9,a-f]+ 0x[0-9,a-f]+
#	0x[0-9,a-f]+	runtime/pprof\.allocateTransient2MInline\+0x[0-9,a-f]+	.*runtime/pprof/mprof_test.go:35
#	0x[0-9,a-f]+	runtime/pprof\.TestMemoryProfiler\+0x[0-9,a-f]+	.*runtime/pprof/mprof_test.go:86

"""u8, memoryProfilerRun, ((2 << (int)(20))) * memoryProfilerRun)
        ), new(
            stk: new @string[]{"runtime/pprof.allocateReflectTransient"u8}.slice(),
            legacy: fmt.Sprintf("""
0: 0 \[%v: %v\] @( 0x[0-9,a-f]+)+
#	0x[0-9,a-f]+	runtime/pprof\.allocateReflectTransient\+0x[0-9,a-f]+	.*runtime/pprof/mprof_test.go:56

"""u8, memoryProfilerRun, ((2 << (int)(20))) * memoryProfilerRun)
        )
        }.slice();
        var testsʗ1 = tests;
        Ꮡt.Run(debug1ˢ, (ж<testing.T> tΔ1) => {
            ref var buf = ref heap(new bytes.Buffer(), out var Ꮡbuf);
            {
                var err = Lookup(heapˢ).WriteTo(new pprof_internal_test_package.bytes_BufferжWriter(Ꮡbuf), 1); if (err != default!) {
                    tΔ1.Fatalf("failed to write heap profile: %v"u8, err);
                }
            }
            foreach (var (_, test) in testsʗ1) {
                if (!regexp.MustCompile(test.legacy).Match(buf.Bytes())) {
                    tΔ1.Fatalf("The entry did not match:\n%v\n\nProfile:\n%v\n"u8, test.legacy, Ꮡbuf.String());
                }
            }
        });
        var testsʗ2 = tests;
        Ꮡt.Run(protoˢ, (ж<testing.T> tΔ2) => {
            ref var buf = ref heap(new bytes.Buffer(), out var Ꮡbuf);
            {
                var errΔ1 = Lookup(heapˢ).WriteTo(new pprof_internal_test_package.bytes_BufferжWriter(Ꮡbuf), 0); if (errΔ1 != default!) {
                    tΔ2.Fatalf("failed to write heap profile: %v"u8, errΔ1);
                }
            }
            var (p, err) = profile.Parse(new pprof_internal_test_package.bytes_BufferжReader(Ꮡbuf));
            if (err != default!) {
                tΔ2.Fatalf("failed to parse heap profile: %v"u8, err);
            }
            tΔ2.Logf("Profile = %v"u8, p.OrTypedNil());
            var stks = profileStacks(p);
            foreach (var (_, test) in testsʗ2) {
                if (!containsStack(stks, test.stk)) {
                    tΔ2.Fatalf("No matching stack entry for %q\n\nProfile:\n%v\n"u8, test.stk, p.OrTypedNil());
                }
            }
            if (!containsInlinedCall(TestMemoryProfiler, (4 << (int)(10)))) {
                tΔ2.Logf("Can't determine whether allocateTransient2MInline was inlined into TestMemoryProfiler."u8);
                return;
            }
            // Check the inlined function location is encoded correctly.
            foreach (var (_, loc) in (~p).Location) {
                var (inlinedCaller, inlinedCallee) = (false, false);
                foreach (var (_, line) in (~loc).Line) {
                    if ((~line.Function).Name == "runtime/pprof.allocateTransient2MInline"u8) {
                        inlinedCallee = true;
                    }
                    if (inlinedCallee && (~line.Function).Name == "runtime/pprof.TestMemoryProfiler"u8) {
                        inlinedCaller = true;
                    }
                }
                if (inlinedCallee != inlinedCaller) {
                    tΔ2.Errorf("want allocateTransient2MInline after TestMemoryProfiler in one location, got separate location entries:\n%v"u8, loc.OrTypedNil());
                }
            }
        });
    }
    catch (Exception ᒐex) when (GoFrame.IsPanic(ᒐex, out PanicException? ᒐp)) { GoFrame.Capture(ᒐp); }
    finally { ᒐ.Run(); }
}

} // end pprof_internal_test_package
