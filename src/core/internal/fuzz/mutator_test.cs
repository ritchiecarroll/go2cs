// Copyright 2021 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.
namespace go.@internal;

using bytes = bytes_package;
using fmt = fmt_package;
using os = os_package;
using strconv = strconv_package;
using testing = testing_package;
using static global::go.@internal.fuzz_package;

partial class fuzz_internal_test_package {

public static void BenchmarkMutatorBytes(ж<testing.B> Ꮡb) {
    GoFrame ᒐ = default;
    try {
        @string origEnv = os.Getenv(godebugˢ);
        defer(() => {
            os.Setenv(godebugˢ, origEnv);
        }, ref ᒐ);
        os.Setenv(godebugˢ, fmt.Sprintf("%s,fuzzseed=123"u8, origEnv));
        var m = newMutator();
        foreach (var (_, size) in new nint[]{
            1,
            10,
            100,
            1000,
            10000,
            100000
        }.slice()) {
            var mʗ1 = m;
            Ꮡb.Run(strconv.Itoa(size), (ж<testing.B> bΔ1) => {
                var buf = new slice<byte>(size);
                bΔ1.ResetTimer();
                for (nint i = 0; i < (~bΔ1).N; i++) {
                    // resize buffer to the correct shape and reset the PCG
                    buf = buf.slice(0, size);
                    mʗ1.Value.r = new global::go.@internal.fuzz_package.pcgRandжmutatorRand(newPcgRand());
                    mʗ1.mutate(new any[]{buf}.slice(), workerSharedMemSize);
                }
            });
        }
    }
    catch (Exception ᒐex) when (GoFrame.IsPanic(ᒐex, out PanicException? ᒐp)) { GoFrame.Capture(ᒐp); }
    finally { ᒐ.Run(); }
}

public static void BenchmarkMutatorString(ж<testing.B> Ꮡb) {
    GoFrame ᒐ = default;
    try {
        @string origEnv = os.Getenv(godebugˢ);
        defer(() => {
            os.Setenv(godebugˢ, origEnv);
        }, ref ᒐ);
        os.Setenv(godebugˢ, fmt.Sprintf("%s,fuzzseed=123"u8, origEnv));
        var m = newMutator();
        foreach (var (_, size) in new nint[]{
            1,
            10,
            100,
            1000,
            10000,
            100000
        }.slice()) {
            var mʗ1 = m;
            Ꮡb.Run(strconv.Itoa(size), (ж<testing.B> bΔ1) => {
                var buf = new slice<byte>(size);
                bΔ1.ResetTimer();
                for (nint i = 0; i < (~bΔ1).N; i++) {
                    // resize buffer to the correct shape and reset the PCG
                    buf = buf.slice(0, size);
                    mʗ1.Value.r = new global::go.@internal.fuzz_package.pcgRandжmutatorRand(newPcgRand());
                    mʗ1.mutate(new any[]{((@string)buf)}.slice(), workerSharedMemSize);
                }
            });
        }
    }
    catch (Exception ᒐex) when (GoFrame.IsPanic(ᒐex, out PanicException? ᒐp)) { GoFrame.Capture(ᒐp); }
    finally { ᒐ.Run(); }
}

public static void BenchmarkMutatorAllBasicTypes(ж<testing.B> Ꮡb) {
    GoFrame ᒐ = default;
    try {
        @string origEnv = os.Getenv(godebugˢ);
        defer(() => {
            os.Setenv(godebugˢ, origEnv);
        }, ref ᒐ);
        os.Setenv(godebugˢ, fmt.Sprintf("%s,fuzzseed=123"u8, origEnv));
        var m = newMutator();
        var types = new any[]{
            slice<byte>(""u8),
            ((@string)""u8),
            false,
            (float32)0F,
            (float64)0D,
            (nint)0,
            (int8)0,
            (int16)0,
            (int32)0,
            (int64)0,
            (uint8)0,
            (uint16)0,
            (uint32)0,
            (uint64)0
        }.slice();
        foreach (var (_, t) in types) {
            var mʗ1 = m;
            var tʗ1 = t;
            Ꮡb.Run(fmt.Sprintf("%T"u8, t), (ж<testing.B> bΔ1) => {
                for (nint i = 0; i < (~bΔ1).N; i++) {
                    mʗ1.Value.r = new global::go.@internal.fuzz_package.pcgRandжmutatorRand(newPcgRand());
                    mʗ1.mutate(new any[]{tʗ1}.slice(), workerSharedMemSize);
                }
            });
        }
    }
    catch (Exception ᒐex) when (GoFrame.IsPanic(ᒐex, out PanicException? ᒐp)) { GoFrame.Capture(ᒐp); }
    finally { ᒐ.Run(); }
}

public static void TestStringImmutability(ж<testing.T> Ꮡt) {
    var v = new any[]{(@string)"hello"u8}.slice();
    var m = newMutator();
    m.mutate(v, 1024);
    @string original = v[0]._<@string>();
    var originalCopy = new slice<byte>(len(original));
    copy(originalCopy, slice<byte>(original));
    for (nint i = 0; i < 25; i++) {
        m.mutate(v, 1024);
    }
    if (!bytes.Equal(slice<byte>(original), originalCopy)) {
        Ꮡt.Fatalf("string was mutated: got %x, want %x"u8, slice<byte>(original), originalCopy);
    }
}

} // end fuzz_internal_test_package
