// Copyright 2013 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.
namespace go;

using rand = crypto.rand_package;
using binary = encoding.binary_package;
using fmt = fmt_package;
using race = @internal.race_package;
using testenv = @internal.testenv_package;
using static runtime_package;
using atomic = global::go.sync.atomic_package;
using testing = testing_package;
using @unsafe = unsafe_package;
using @internal;
using crypto;
using encoding;
using global::go.sync;
using static global::go.runtime_internal_test_package;

partial class runtime_test_package {

public static void TestMemmove(ж<testing.T> Ꮡt) {
    if (flagQuick.Value) {
        Ꮡt.Skip(quickˢ);
    }
    Ꮡt.Parallel();
    nint size = 256;
    if (testing.Short()) {
        size = 128 + 16;
    }
    var src = new slice<byte>(size);
    var dst = new slice<byte>(size);
    for (nint i = 0; i < size; i++) {
        src[i] = (byte)(128 + ((nint)(i & 127)));
    }
    for (nint i = 0; i < size; i++) {
        dst[i] = (byte)((nint)(i & 127));
    }
    for (nint n = 0; n <= size; n++) {
        for (nint x = 0; x <= size - n; x++) {
            // offset in src
            for (nint y = 0; y <= size - n; y++) {
                // offset in dst
                copy(dst.slice(y, y + n), src.slice(x, x + n));
                for (nint i = 0; i < y; i++) {
                    if (dst[i] != (byte)((nint)(i & 127))) {
                        Ꮡt.Fatalf("prefix dst[%d] = %d"u8, i, dst[i]);
                    }
                }
                for (nint i = y; i < y + n; i++) {
                    if (dst[i] != (byte)(128 + ((nint)((i - y + x) & 127)))) {
                        Ꮡt.Fatalf("copied dst[%d] = %d"u8, i, dst[i]);
                    }
                    dst[i] = (byte)((nint)(i & 127)); // reset dst
                }
                for (nint i = y + n; i < size; i++) {
                    if (dst[i] != (byte)((nint)(i & 127))) {
                        Ꮡt.Fatalf("suffix dst[%d] = %d"u8, i, dst[i]);
                    }
                }
            }
        }
    }
}

public static void TestMemmoveAlias(ж<testing.T> Ꮡt) {
    if (flagQuick.Value) {
        Ꮡt.Skip(quickˢ);
    }
    Ꮡt.Parallel();
    nint size = 256;
    if (testing.Short()) {
        size = 128 + 16;
    }
    var buf = new slice<byte>(size);
    for (nint i = 0; i < size; i++) {
        buf[i] = (byte)i;
    }
    for (nint n = 0; n <= size; n++) {
        for (nint x = 0; x <= size - n; x++) {
            // src offset
            for (nint y = 0; y <= size - n; y++) {
                // dst offset
                copy(buf.slice(y, y + n), buf.slice(x, x + n));
                for (nint i = 0; i < y; i++) {
                    if (buf[i] != (byte)i) {
                        Ꮡt.Fatalf("prefix buf[%d] = %d"u8, i, buf[i]);
                    }
                }
                for (nint i = y; i < y + n; i++) {
                    if (buf[i] != (byte)(i - y + x)) {
                        Ꮡt.Fatalf("copied buf[%d] = %d"u8, i, buf[i]);
                    }
                    buf[i] = (byte)i; // reset buf
                }
                for (nint i = y + n; i < size; i++) {
                    if (buf[i] != (byte)i) {
                        Ꮡt.Fatalf("suffix buf[%d] = %d"u8, i, buf[i]);
                    }
                }
            }
        }
    }
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static readonly object shortˢ = (@string)"-short"u8;
internal static readonly object skippingLargeMemmoveTestˢ = (@string)"skipping large memmove test under race detector"u8;

public static void TestMemmoveLarge0x180000(ж<testing.T> Ꮡt) {
    if (testing.Short() && testenv.Builder() == ""u8) {
        Ꮡt.Skip(shortˢ);
    }
    Ꮡt.Parallel();
    if (race.Enabled) {
        Ꮡt.Skip(skippingLargeMemmoveTestˢ);
    }
    testSize(Ꮡt, 0x180000);
}

public static void TestMemmoveOverlapLarge0x120000(ж<testing.T> Ꮡt) {
    if (testing.Short() && testenv.Builder() == ""u8) {
        Ꮡt.Skip(shortˢ);
    }
    Ꮡt.Parallel();
    if (race.Enabled) {
        Ꮡt.Skip(skippingLargeMemmoveTestˢ);
    }
    testOverlap(Ꮡt, 0x120000);
}

internal static void testSize(ж<testing.T> Ꮡt, nint size) {
    var src = new slice<byte>(size);
    var dst = new slice<byte>(size);
    (_, _) = rand.Read(src);
    (_, _) = rand.Read(dst);
    var @ref = new slice<byte>(size);
    copyref(@ref, dst);
    for (nint n = size - 50; n > 1; n >>= (int)(1)) {
        for (nint x = 0; x <= size - n; x = x * 7 + 1) {
            // offset in src
            for (nint y = 0; y <= size - n; y = y * 9 + 1) {
                // offset in dst
                copy(dst.slice(y, y + n), src.slice(x, x + n));
                copyref(@ref.slice(y, y + n), src.slice(x, x + n));
                nint p = cmpb(dst, @ref);
                if (p >= 0) {
                    Ꮡt.Fatalf("Copy failed, copying from src[%d:%d] to dst[%d:%d].\nOffset %d is different, %v != %v"u8, x, x + n, y, y + n, p, dst[p], @ref[p]);
                }
            }
        }
    }
}

internal static void testOverlap(ж<testing.T> Ꮡt, nint size) {
    var src = new slice<byte>(size);
    var test = new slice<byte>(size);
    var @ref = new slice<byte>(size);
    (_, _) = rand.Read(src);
    for (nint n = size - 50; n > 1; n >>= (int)(1)) {
        for (nint x = 0; x <= size - n; x = x * 7 + 1) {
            // offset in src
            for (nint y = 0; y <= size - n; y = y * 9 + 1) {
                // offset in dst
                // Reset input
                copyref(test, src);
                copyref(@ref, src);
                copy(test.slice(y, y + n), test.slice(x, x + n));
                if (y <= x){
                    copyref(@ref.slice(y, y + n), @ref.slice(x, x + n));
                } else {
                    copybw(@ref.slice(y, y + n), @ref.slice(x, x + n));
                }
                nint p = cmpb(test, @ref);
                if (p >= 0) {
                    Ꮡt.Fatalf("Copy failed, copying from src[%d:%d] to dst[%d:%d].\nOffset %d is different, %v != %v"u8, x, x + n, y, y + n, p, test[p], @ref[p]);
                }
            }
        }
    }
}

// Forward copy.
internal static void copyref(slice<byte> dst, slice<byte> src) {
    foreach (var (i, v) in src) {
        dst[i] = v;
    }
}

// Backwards copy
internal static void copybw(slice<byte> dst, slice<byte> src) {
    if (len(src) == 0) {
        return;
    }
    for (nint i = len(src) - 1; i >= 0; i--) {
        dst[i] = src[i];
    }
}

// Returns offset of difference
internal static nint matchLen(slice<byte> a, slice<byte> b, nint max) {
    a = a.slice(0, max);
    b = b.slice(0, max);
    foreach (var (i, av) in a) {
        if (b[i] != av) {
            return i;
        }
    }
    return max;
}

internal static nint cmpb(slice<byte> a, slice<byte> b) {
    nint l = matchLen(a, b, len(a));
    if (l == len(a)) {
        return -1;
    }
    return l;
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static readonly object skipUnderTheRaceDetectorˢ = (@string)"skip under the race detector -- this test is intentionally racy"u8;

// Ensure that memmove writes pointers atomically, so the GC won't
// observe a partially updated pointer.
public static void TestMemmoveAtomicity(ж<testing.T> Ꮡt) {
    if (race.Enabled) {
        Ꮡt.Skip(skipUnderTheRaceDetectorˢ);
    }
    ref var x = ref heap(new nint(), out var Ꮡx);
    foreach (var (_, backward) in new bool[]{true, false}.slice()) {
        foreach (var (_, n) in new nint[]{3, 4, 5, 6, 7, 8, 9, 10, 15, 25, 49}.slice()) {
            nint nΔ1 = n;
            // test copying [N]*int.
            var sz = (uintptr)(nΔ1 * (nint)runtime_internal_test_package.PtrSize);
            @string name = fmt.Sprint(sz);
            if (backward){
                name += "-backward"u8;
            } else {
                name += "-forward"u8;
            }
            Ꮡt.Run(name, (ж<testing.T> tΔ1) => {
                // Use overlapping src and dst to force forward/backward copy.
                array<ж<nint>> s = new(100);
                ref var src = ref heap<slice<ж<nint>>>(out var Ꮡsrc);
                Ꮡsrc.ValueSlot = s.slice(nΔ1 - 1, 2 * nΔ1 - 1);
                ref var dst = ref heap<slice<ж<nint>>>(out var Ꮡdst);
                Ꮡdst.ValueSlot = s.slice(0, nΔ1);
                if (backward) {
                    (Ꮡsrc.ValueSlot, Ꮡdst.ValueSlot) = (Ꮡdst.ValueSlot, Ꮡsrc.ValueSlot);
                }
                foreach (var (i, _) in Ꮡsrc.ValueSlot) {
                    Ꮡsrc.ValueSlot[i] = Ꮡx;
                }
                builtin.clear(Ꮡdst.ValueSlot);
                ref var ready = ref heap(new atomic.Uint32(), out var Ꮡready);
                goǃ(() => {
                    @unsafe.Pointer sp = @unsafe.Pointer.FromBox(Ꮡ(Ꮡsrc.ValueSlot, 0));
                    @unsafe.Pointer dp = @unsafe.Pointer.FromBox(Ꮡ(Ꮡdst.ValueSlot, 0));
                    Ꮡready.Store(1);
                    for (nint i = 0; i < 10000; i++) {
                        runtime_internal_test_package.Memmove(dp, sp, sz);
                        runtime_internal_test_package.MemclrNoHeapPointers(dp, sz);
                    }
                    Ꮡready.Store(2);
                });
                while (Ꮡready.Load() == 0) {
                    Gosched();
                }
                while (Ꮡready.Load() != 2) {
                    foreach (var (i, _) in Ꮡdst.ValueSlot) {
                        var p = Ꮡdst.ValueSlot[i];
                        if (p != nil && p != Ꮡx) {
                            tΔ1.Fatalf("got partially updated pointer %p at dst[%d], want either nil or %p"u8, p.OrTypedNil(), i, Ꮡx);
                        }
                    }
                }
            });
        }
    }
}

internal static void benchmarkSizes(ж<testing.B> Ꮡb, slice<nint> sizes, Action<ж<testing.B>, nint> fn) {
    foreach (var (_, n) in sizes) {
        Ꮡb.Run(fmt.Sprint(n), (ж<testing.B> bΔ1) => {
            bΔ1.SetBytes((int64)n);
            fn(bΔ1, n);
        });
    }
}

internal static slice<nint> bufSizes = new nint[]{
    0, 1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 11, 12, 13, 14, 15, 16,
    32, 64, 128, 256, 512, 1024, 2048, 4096
}.slice();

internal static slice<nint> bufSizesOverlap = new nint[]{
    32, 64, 128, 256, 512, 1024, 2048, 4096
}.slice();

public static void BenchmarkMemmove(ж<testing.B> Ꮡb) {
    benchmarkSizes(Ꮡb, bufSizes, (ж<testing.B> bΔ1, nint n) => {
        var x = new slice<byte>(n);
        var y = new slice<byte>(n);
        for (nint i = 0; i < (~bΔ1).N; i++) {
            copy(x, y);
        }
    });
}

public static void BenchmarkMemmoveOverlap(ж<testing.B> Ꮡb) {
    benchmarkSizes(Ꮡb, bufSizesOverlap, (ж<testing.B> bΔ1, nint n) => {
        var x = new slice<byte>(n + 16);
        for (nint i = 0; i < (~bΔ1).N; i++) {
            copy(x.slice(16, n + 16), x.slice(0, n));
        }
    });
}

public static void BenchmarkMemmoveUnalignedDst(ж<testing.B> Ꮡb) {
    benchmarkSizes(Ꮡb, bufSizes, (ж<testing.B> bΔ1, nint n) => {
        var x = new slice<byte>(n + 1);
        var y = new slice<byte>(n);
        for (nint i = 0; i < (~bΔ1).N; i++) {
            copy(x[1..], y);
        }
    });
}

public static void BenchmarkMemmoveUnalignedDstOverlap(ж<testing.B> Ꮡb) {
    benchmarkSizes(Ꮡb, bufSizesOverlap, (ж<testing.B> bΔ1, nint n) => {
        var x = new slice<byte>(n + 16);
        for (nint i = 0; i < (~bΔ1).N; i++) {
            copy(x.slice(16, n + 16), x.slice(1, n + 1));
        }
    });
}

public static void BenchmarkMemmoveUnalignedSrc(ж<testing.B> Ꮡb) {
    benchmarkSizes(Ꮡb, bufSizes, (ж<testing.B> bΔ1, nint n) => {
        var x = new slice<byte>(n);
        var y = new slice<byte>(n + 1);
        for (nint i = 0; i < (~bΔ1).N; i++) {
            copy(x, y[1..]);
        }
    });
}

public static void BenchmarkMemmoveUnalignedSrcDst(ж<testing.B> Ꮡb) {
    foreach (var (_, n) in new nint[]{16, 64, 256, 4096, 65536}.slice()) {
        var buf = new slice<byte>((n + 8) * 2);
        var x = buf.slice(0, len(buf) / 2);
        var y = buf.slice(len(buf) / 2);
        foreach (var (_, off) in new nint[]{0, 1, 4, 7}.slice()) {
            var xʗ1 = x;
            var yʗ1 = y;
            Ꮡb.Run(fmt.Sprint((@string)"f_"u8, n, off), (ж<testing.B> bΔ1) => {
                bΔ1.SetBytes((int64)n);
                for (nint i = 0; i < (~bΔ1).N; i++) {
                    copy(xʗ1.slice(off, n + off), yʗ1.slice(off, n + off));
                }
            });
            var xʗ2 = x;
            var yʗ2 = y;
            Ꮡb.Run(fmt.Sprint((@string)"b_"u8, n, off), (ж<testing.B> bΔ2) => {
                bΔ2.SetBytes((int64)n);
                for (nint i = 0; i < (~bΔ2).N; i++) {
                    copy(yʗ2.slice(off, n + off), xʗ2.slice(off, n + off));
                }
            });
        }
    }
}

public static void BenchmarkMemmoveUnalignedSrcOverlap(ж<testing.B> Ꮡb) {
    benchmarkSizes(Ꮡb, bufSizesOverlap, (ж<testing.B> bΔ1, nint n) => {
        var x = new slice<byte>(n + 1);
        for (nint i = 0; i < (~bΔ1).N; i++) {
            copy(x.slice(1, n + 1), x.slice(0, n));
        }
    });
}

public static void TestMemclr(ж<testing.T> Ꮡt) {
    nint size = 512;
    if (testing.Short()) {
        size = 128 + 16;
    }
    var mem = new slice<byte>(size);
    for (nint i = 0; i < size; i++) {
        mem[i] = 0xee;
    }
    for (nint n = 0; n < size; n++) {
        for (nint x = 0; x <= size - n; x++) {
            // offset in mem
            runtime_internal_test_package.MemclrBytes(mem.slice(x, x + n));
            for (nint i = 0; i < x; i++) {
                if (mem[i] != 0xee) {
                    Ꮡt.Fatalf("overwrite prefix mem[%d] = %d"u8, i, mem[i]);
                }
            }
            for (nint i = x; i < x + n; i++) {
                if (mem[i] != 0) {
                    Ꮡt.Fatalf("failed clear mem[%d] = %d"u8, i, mem[i]);
                }
                mem[i] = 0xee;
            }
            for (nint i = x + n; i < size; i++) {
                if (mem[i] != 0xee) {
                    Ꮡt.Fatalf("overwrite suffix mem[%d] = %d"u8, i, mem[i]);
                }
            }
        }
    }
}

public static void BenchmarkMemclr(ж<testing.B> Ꮡb) {
    foreach (var (_, n) in new nint[]{5, 16, 64, 256, 4096, 65536}.slice()) {
        var x = new slice<byte>(n);
        var xʗ1 = x;
        Ꮡb.Run(fmt.Sprint(n), (ж<testing.B> bΔ1) => {
            bΔ1.SetBytes((int64)n);
            for (nint i = 0; i < (~bΔ1).N; i++) {
                runtime_internal_test_package.MemclrBytes(xʗ1);
            }
        });
    }
    foreach (var (_, m) in new nint[]{1, 4, 8, 16, 64}.slice()) {
        var x = new slice<byte>((m << (int)(20)));
        var xʗ2 = x;
        Ꮡb.Run(fmt.Sprint(m, (@string)"M"u8), (ж<testing.B> bΔ2) => {
            bΔ2.SetBytes((int64)((m << (int)(20))));
            for (nint i = 0; i < (~bΔ2).N; i++) {
                runtime_internal_test_package.MemclrBytes(xʗ2);
            }
        });
    }
}

public static void BenchmarkMemclrUnaligned(ж<testing.B> Ꮡb) {
    foreach (var (_, off) in new nint[]{0, 1, 4, 7}.slice()) {
        foreach (var (_, n) in new nint[]{5, 16, 64, 256, 4096, 65536}.slice()) {
            var x = new slice<byte>(n + off);
            var xʗ1 = x;
            Ꮡb.Run(fmt.Sprint(off, n), (ж<testing.B> bΔ1) => {
                bΔ1.SetBytes((int64)n);
                for (nint i = 0; i < (~bΔ1).N; i++) {
                    runtime_internal_test_package.MemclrBytes(xʗ1.slice(off));
                }
            });
        }
    }
    foreach (var (_, off) in new nint[]{0, 1, 4, 7}.slice()) {
        foreach (var (_, m) in new nint[]{1, 4, 8, 16, 64}.slice()) {
            var x = new slice<byte>(((m << (int)(20))) + off);
            var xʗ2 = x;
            Ꮡb.Run(fmt.Sprint(off, m, (@string)"M"u8), (ж<testing.B> bΔ2) => {
                bΔ2.SetBytes((int64)((m << (int)(20))));
                for (nint i = 0; i < (~bΔ2).N; i++) {
                    runtime_internal_test_package.MemclrBytes(xʗ2.slice(off));
                }
            });
        }
    }
}

public static void BenchmarkGoMemclr(ж<testing.B> Ꮡb) {
    benchmarkSizes(Ꮡb, new nint[]{5, 16, 64, 256}.slice(), (ж<testing.B> bΔ1, nint n) => {
        var x = new slice<byte>(n);
        for (nint i = 0; i < (~bΔ1).N; i++) {
            builtin.clear(x);
        }
    });
}

[GoType("dyn")] internal partial struct BenchmarkMemclrRange_RunData {
    internal slice<nint> data;
}

public static void BenchmarkMemclrRange(ж<testing.B> Ꮡb) {
    var benchSizes = new BenchmarkMemclrRange_RunData[]{
        new(new nint[]{1043, 1078, 1894, 1582, 1044, 1165, 1467, 1100, 1919, 1562, 1932, 1645,
            1412, 1038, 1576, 1200, 1029, 1336, 1095, 1494, 1350, 1025, 1502, 1548, 1316, 1296,
            1868, 1639, 1546, 1626, 1642, 1308, 1726, 1665, 1678, 1187, 1515, 1598, 1353, 1237,
            1977, 1452, 2012, 1914, 1514, 1136, 1975, 1618, 1536, 1695, 1600, 1733, 1392, 1099,
            1358, 1996, 1224, 1783, 1197, 1838, 1460, 1556, 1554, 2020}.slice()), // 1kb-2kb

        new(new nint[]{3964, 5139, 6573, 7775, 6553, 2413, 3466, 5394, 2469, 7336, 7091, 6745,
            4028, 5643, 6164, 3475, 4138, 6908, 7559, 3335, 5660, 4122, 3945, 2082, 7564, 6584,
            5111, 2288, 6789, 2797, 4928, 7986, 5163, 5447, 2999, 4968, 3174, 3202, 7908, 8137,
            4735, 6161, 4646, 7592, 3083, 5329, 3687, 2754, 3599, 7231, 6455, 2549, 8063, 2189,
            7121, 5048, 4277, 6626, 6306, 2815, 7473, 3963, 7549, 7255}.slice()), // 2kb-8kb

        new(new nint[]{16304, 15936, 15760, 4736, 9136, 11184, 10160, 5952, 14560, 15744,
            6624, 5872, 13088, 14656, 14192, 10304, 4112, 10384, 9344, 4496, 11392, 7024,
            5200, 10064, 14784, 5808, 13504, 10480, 8512, 4896, 13264, 5600}.slice()), // 4kb-16kb

        new(new nint[]{164576, 233136, 220224, 183280, 214112, 217248, 228560, 201728}.slice())
    }.slice();
    // 128kb-256kb
    foreach (var (_, vᴛ1) in benchSizes) {
        ref var t = ref heap(new BenchmarkMemclrRange_RunData(), out var Ꮡt);
        t = vᴛ1;

        nint total = 0;
        nint minLen = 0;
        nint maxLen = 0;
        foreach (var (_, clrLen) in t.data) {
            maxLen = builtin.max(maxLen, clrLen);
            if (clrLen < minLen || minLen == 0) {
                minLen = clrLen;
            }
            total += clrLen;
        }
        var buffer = new slice<byte>(maxLen);
        @string text = ""u8;
        if (minLen >= ((1 << (int)(20)))){
            text = fmt.Sprint((minLen >> (int)(20)), (@string)"M "u8, ((maxLen + ((1 << (int)(20)) - 1)) >> (int)(20)), (@string)"M"u8);
        } else 
        if (minLen >= ((1 << (int)(10)))){
            text = fmt.Sprint((minLen >> (int)(10)), (@string)"K "u8, ((maxLen + ((1 << (int)(10)) - 1)) >> (int)(10)), (@string)"K"u8);
        } else {
            text = fmt.Sprint(minLen, (@string)" "u8, maxLen);
        }
        var bufferʗ1 = buffer;
        var tʗ1 = t;
        Ꮡb.Run(text, (ж<testing.B> bΔ1) => {
            bΔ1.SetBytes((int64)total);
            for (nint i = 0; i < (~bΔ1).N; i++) {
                foreach (var (_, clrLen) in tʗ1.data) {
                    runtime_internal_test_package.MemclrBytes(bufferʗ1.slice(0, clrLen));
                }
            }
        });
    }
}

public static void BenchmarkClearFat7(ж<testing.B> Ꮡb) {
    ref var b = ref Ꮡb.DerefOrNull();

    var p = Ꮡ(new array<byte>(7));
    runtime_internal_test_package.Escape(p);
    b.ResetTimer();
    for (nint i = 0; i < b.N; i++) {
        p.Value = new byte[]{}.array(7);
    }
}

public static void BenchmarkClearFat8(ж<testing.B> Ꮡb) {
    ref var b = ref Ꮡb.DerefOrNull();

    var p = Ꮡ(new array<uint32>(2));
    runtime_internal_test_package.Escape(p);
    b.ResetTimer();
    for (nint i = 0; i < b.N; i++) {
        p.Value = new uint32[]{}.array(2);
    }
}

public static void BenchmarkClearFat11(ж<testing.B> Ꮡb) {
    ref var b = ref Ꮡb.DerefOrNull();

    var p = Ꮡ(new array<byte>(11));
    runtime_internal_test_package.Escape(p);
    b.ResetTimer();
    for (nint i = 0; i < b.N; i++) {
        p.Value = new byte[]{}.array(11);
    }
}

public static void BenchmarkClearFat12(ж<testing.B> Ꮡb) {
    ref var b = ref Ꮡb.DerefOrNull();

    var p = Ꮡ(new array<uint32>(3));
    runtime_internal_test_package.Escape(p);
    b.ResetTimer();
    for (nint i = 0; i < b.N; i++) {
        p.Value = new uint32[]{}.array(3);
    }
}

public static void BenchmarkClearFat13(ж<testing.B> Ꮡb) {
    ref var b = ref Ꮡb.DerefOrNull();

    var p = Ꮡ(new array<byte>(13));
    runtime_internal_test_package.Escape(p);
    b.ResetTimer();
    for (nint i = 0; i < b.N; i++) {
        p.Value = new byte[]{}.array(13);
    }
}

public static void BenchmarkClearFat14(ж<testing.B> Ꮡb) {
    ref var b = ref Ꮡb.DerefOrNull();

    var p = Ꮡ(new array<byte>(14));
    runtime_internal_test_package.Escape(p);
    b.ResetTimer();
    for (nint i = 0; i < b.N; i++) {
        p.Value = new byte[]{}.array(14);
    }
}

public static void BenchmarkClearFat15(ж<testing.B> Ꮡb) {
    ref var b = ref Ꮡb.DerefOrNull();

    var p = Ꮡ(new array<byte>(15));
    runtime_internal_test_package.Escape(p);
    b.ResetTimer();
    for (nint i = 0; i < b.N; i++) {
        p.Value = new byte[]{}.array(15);
    }
}

public static void BenchmarkClearFat16(ж<testing.B> Ꮡb) {
    ref var b = ref Ꮡb.DerefOrNull();

    var p = Ꮡ(new array<uint32>(4));
    runtime_internal_test_package.Escape(p);
    b.ResetTimer();
    for (nint i = 0; i < b.N; i++) {
        p.Value = new uint32[]{}.array(4);
    }
}

public static void BenchmarkClearFat24(ж<testing.B> Ꮡb) {
    ref var b = ref Ꮡb.DerefOrNull();

    var p = Ꮡ(new array<uint32>(6));
    runtime_internal_test_package.Escape(p);
    b.ResetTimer();
    for (nint i = 0; i < b.N; i++) {
        p.Value = new uint32[]{}.array(6);
    }
}

public static void BenchmarkClearFat32(ж<testing.B> Ꮡb) {
    ref var b = ref Ꮡb.DerefOrNull();

    var p = Ꮡ(new array<uint32>(8));
    runtime_internal_test_package.Escape(p);
    b.ResetTimer();
    for (nint i = 0; i < b.N; i++) {
        p.Value = new uint32[]{}.array(8);
    }
}

public static void BenchmarkClearFat40(ж<testing.B> Ꮡb) {
    ref var b = ref Ꮡb.DerefOrNull();

    var p = Ꮡ(new array<uint32>(10));
    runtime_internal_test_package.Escape(p);
    b.ResetTimer();
    for (nint i = 0; i < b.N; i++) {
        p.Value = new uint32[]{}.array(10);
    }
}

public static void BenchmarkClearFat48(ж<testing.B> Ꮡb) {
    ref var b = ref Ꮡb.DerefOrNull();

    var p = Ꮡ(new array<uint32>(12));
    runtime_internal_test_package.Escape(p);
    b.ResetTimer();
    for (nint i = 0; i < b.N; i++) {
        p.Value = new uint32[]{}.array(12);
    }
}

public static void BenchmarkClearFat56(ж<testing.B> Ꮡb) {
    ref var b = ref Ꮡb.DerefOrNull();

    var p = Ꮡ(new array<uint32>(14));
    runtime_internal_test_package.Escape(p);
    b.ResetTimer();
    for (nint i = 0; i < b.N; i++) {
        p.Value = new uint32[]{}.array(14);
    }
}

public static void BenchmarkClearFat64(ж<testing.B> Ꮡb) {
    ref var b = ref Ꮡb.DerefOrNull();

    var p = Ꮡ(new array<uint32>(16));
    runtime_internal_test_package.Escape(p);
    b.ResetTimer();
    for (nint i = 0; i < b.N; i++) {
        p.Value = new uint32[]{}.array(16);
    }
}

public static void BenchmarkClearFat72(ж<testing.B> Ꮡb) {
    ref var b = ref Ꮡb.DerefOrNull();

    var p = Ꮡ(new array<uint32>(18));
    runtime_internal_test_package.Escape(p);
    b.ResetTimer();
    for (nint i = 0; i < b.N; i++) {
        p.Value = new uint32[]{}.array(18);
    }
}

public static void BenchmarkClearFat128(ж<testing.B> Ꮡb) {
    ref var b = ref Ꮡb.DerefOrNull();

    var p = Ꮡ(new array<uint32>(32));
    runtime_internal_test_package.Escape(p);
    b.ResetTimer();
    for (nint i = 0; i < b.N; i++) {
        p.Value = new uint32[]{}.array(32);
    }
}

public static void BenchmarkClearFat256(ж<testing.B> Ꮡb) {
    ref var b = ref Ꮡb.DerefOrNull();

    var p = Ꮡ(new array<uint32>(64));
    runtime_internal_test_package.Escape(p);
    b.ResetTimer();
    for (nint i = 0; i < b.N; i++) {
        p.Value = new uint32[]{}.array(64);
    }
}

public static void BenchmarkClearFat512(ж<testing.B> Ꮡb) {
    ref var b = ref Ꮡb.DerefOrNull();

    var p = Ꮡ(new array<uint32>(128));
    runtime_internal_test_package.Escape(p);
    b.ResetTimer();
    for (nint i = 0; i < b.N; i++) {
        p.Value = new uint32[]{}.array(128);
    }
}

public static void BenchmarkClearFat1024(ж<testing.B> Ꮡb) {
    ref var b = ref Ꮡb.DerefOrNull();

    var p = Ꮡ(new array<uint32>(256));
    runtime_internal_test_package.Escape(p);
    b.ResetTimer();
    for (nint i = 0; i < b.N; i++) {
        p.Value = new uint32[]{}.array(256);
    }
}

public static void BenchmarkClearFat1032(ж<testing.B> Ꮡb) {
    ref var b = ref Ꮡb.DerefOrNull();

    var p = Ꮡ(new array<uint32>(258));
    runtime_internal_test_package.Escape(p);
    b.ResetTimer();
    for (nint i = 0; i < b.N; i++) {
        p.Value = new uint32[]{}.array(258);
    }
}

public static void BenchmarkClearFat1040(ж<testing.B> Ꮡb) {
    ref var b = ref Ꮡb.DerefOrNull();

    var p = Ꮡ(new array<uint32>(260));
    runtime_internal_test_package.Escape(p);
    b.ResetTimer();
    for (nint i = 0; i < b.N; i++) {
        p.Value = new uint32[]{}.array(260);
    }
}

public static void BenchmarkCopyFat7(ж<testing.B> Ꮡb) {
    ref var b = ref Ꮡb.DerefOrNull();

    array<byte> x = new(7);
    var p = Ꮡ(new array<byte>(7));
    runtime_internal_test_package.Escape(p);
    b.ResetTimer();
    for (nint i = 0; i < b.N; i++) {
        p.Value = x.Clone();
    }
}

public static void BenchmarkCopyFat8(ж<testing.B> Ꮡb) {
    ref var b = ref Ꮡb.DerefOrNull();

    array<uint32> x = new(2); /* 8 / 4 */
    var p = Ꮡ(new array<uint32>(2));
    runtime_internal_test_package.Escape(p);
    b.ResetTimer();
    for (nint i = 0; i < b.N; i++) {
        p.Value = x.Clone();
    }
}

public static void BenchmarkCopyFat11(ж<testing.B> Ꮡb) {
    ref var b = ref Ꮡb.DerefOrNull();

    array<byte> x = new(11);
    var p = Ꮡ(new array<byte>(11));
    runtime_internal_test_package.Escape(p);
    b.ResetTimer();
    for (nint i = 0; i < b.N; i++) {
        p.Value = x.Clone();
    }
}

public static void BenchmarkCopyFat12(ж<testing.B> Ꮡb) {
    ref var b = ref Ꮡb.DerefOrNull();

    array<uint32> x = new(3); /* 12 / 4 */
    var p = Ꮡ(new array<uint32>(3));
    runtime_internal_test_package.Escape(p);
    b.ResetTimer();
    for (nint i = 0; i < b.N; i++) {
        p.Value = x.Clone();
    }
}

public static void BenchmarkCopyFat13(ж<testing.B> Ꮡb) {
    ref var b = ref Ꮡb.DerefOrNull();

    array<byte> x = new(13);
    var p = Ꮡ(new array<byte>(13));
    runtime_internal_test_package.Escape(p);
    b.ResetTimer();
    for (nint i = 0; i < b.N; i++) {
        p.Value = x.Clone();
    }
}

public static void BenchmarkCopyFat14(ж<testing.B> Ꮡb) {
    ref var b = ref Ꮡb.DerefOrNull();

    array<byte> x = new(14);
    var p = Ꮡ(new array<byte>(14));
    runtime_internal_test_package.Escape(p);
    b.ResetTimer();
    for (nint i = 0; i < b.N; i++) {
        p.Value = x.Clone();
    }
}

public static void BenchmarkCopyFat15(ж<testing.B> Ꮡb) {
    ref var b = ref Ꮡb.DerefOrNull();

    array<byte> x = new(15);
    var p = Ꮡ(new array<byte>(15));
    runtime_internal_test_package.Escape(p);
    b.ResetTimer();
    for (nint i = 0; i < b.N; i++) {
        p.Value = x.Clone();
    }
}

public static void BenchmarkCopyFat16(ж<testing.B> Ꮡb) {
    ref var b = ref Ꮡb.DerefOrNull();

    array<uint32> x = new(4); /* 16 / 4 */
    var p = Ꮡ(new array<uint32>(4));
    runtime_internal_test_package.Escape(p);
    b.ResetTimer();
    for (nint i = 0; i < b.N; i++) {
        p.Value = x.Clone();
    }
}

public static void BenchmarkCopyFat24(ж<testing.B> Ꮡb) {
    ref var b = ref Ꮡb.DerefOrNull();

    array<uint32> x = new(6); /* 24 / 4 */
    var p = Ꮡ(new array<uint32>(6));
    runtime_internal_test_package.Escape(p);
    b.ResetTimer();
    for (nint i = 0; i < b.N; i++) {
        p.Value = x.Clone();
    }
}

public static void BenchmarkCopyFat32(ж<testing.B> Ꮡb) {
    ref var b = ref Ꮡb.DerefOrNull();

    array<uint32> x = new(8); /* 32 / 4 */
    var p = Ꮡ(new array<uint32>(8));
    runtime_internal_test_package.Escape(p);
    b.ResetTimer();
    for (nint i = 0; i < b.N; i++) {
        p.Value = x.Clone();
    }
}

public static void BenchmarkCopyFat64(ж<testing.B> Ꮡb) {
    ref var b = ref Ꮡb.DerefOrNull();

    array<uint32> x = new(16); /* 64 / 4 */
    var p = Ꮡ(new array<uint32>(16));
    runtime_internal_test_package.Escape(p);
    b.ResetTimer();
    for (nint i = 0; i < b.N; i++) {
        p.Value = x.Clone();
    }
}

public static void BenchmarkCopyFat72(ж<testing.B> Ꮡb) {
    ref var b = ref Ꮡb.DerefOrNull();

    array<uint32> x = new(18); /* 72 / 4 */
    var p = Ꮡ(new array<uint32>(18));
    runtime_internal_test_package.Escape(p);
    b.ResetTimer();
    for (nint i = 0; i < b.N; i++) {
        p.Value = x.Clone();
    }
}

public static void BenchmarkCopyFat128(ж<testing.B> Ꮡb) {
    ref var b = ref Ꮡb.DerefOrNull();

    array<uint32> x = new(32); /* 128 / 4 */
    var p = Ꮡ(new array<uint32>(32));
    runtime_internal_test_package.Escape(p);
    b.ResetTimer();
    for (nint i = 0; i < b.N; i++) {
        p.Value = x.Clone();
    }
}

public static void BenchmarkCopyFat256(ж<testing.B> Ꮡb) {
    ref var b = ref Ꮡb.DerefOrNull();

    array<uint32> x = new(64); /* 256 / 4 */
    var p = Ꮡ(new array<uint32>(64));
    runtime_internal_test_package.Escape(p);
    b.ResetTimer();
    for (nint i = 0; i < b.N; i++) {
        p.Value = x.Clone();
    }
}

public static void BenchmarkCopyFat512(ж<testing.B> Ꮡb) {
    ref var b = ref Ꮡb.DerefOrNull();

    array<uint32> x = new(128); /* 512 / 4 */
    var p = Ꮡ(new array<uint32>(128));
    runtime_internal_test_package.Escape(p);
    b.ResetTimer();
    for (nint i = 0; i < b.N; i++) {
        p.Value = x.Clone();
    }
}

public static void BenchmarkCopyFat520(ж<testing.B> Ꮡb) {
    ref var b = ref Ꮡb.DerefOrNull();

    array<uint32> x = new(130); /* 520 / 4 */
    var p = Ꮡ(new array<uint32>(130));
    runtime_internal_test_package.Escape(p);
    b.ResetTimer();
    for (nint i = 0; i < b.N; i++) {
        p.Value = x.Clone();
    }
}

public static void BenchmarkCopyFat1024(ж<testing.B> Ꮡb) {
    ref var b = ref Ꮡb.DerefOrNull();

    array<uint32> x = new(256); /* 1024 / 4 */
    var p = Ꮡ(new array<uint32>(256));
    runtime_internal_test_package.Escape(p);
    b.ResetTimer();
    for (nint i = 0; i < b.N; i++) {
        p.Value = x.Clone();
    }
}

public static void BenchmarkCopyFat1032(ж<testing.B> Ꮡb) {
    ref var b = ref Ꮡb.DerefOrNull();

    array<uint32> x = new(258); /* 1032 / 4 */
    var p = Ꮡ(new array<uint32>(258));
    runtime_internal_test_package.Escape(p);
    b.ResetTimer();
    for (nint i = 0; i < b.N; i++) {
        p.Value = x.Clone();
    }
}

public static void BenchmarkCopyFat1040(ж<testing.B> Ꮡb) {
    ref var b = ref Ꮡb.DerefOrNull();

    array<uint32> x = new(260); /* 1040 / 4 */
    var p = Ꮡ(new array<uint32>(260));
    runtime_internal_test_package.Escape(p);
    b.ResetTimer();
    for (nint i = 0; i < b.N; i++) {
        p.Value = x.Clone();
    }
}

[GoType("dyn")] internal partial struct BenchmarkIssue18740_benchmarks {
    internal @string name;
    internal nint nbyte;
    internal Func<slice<byte>, uint64> f;
}

// BenchmarkIssue18740 ensures that memmove uses 4 and 8 byte load/store to move 4 and 8 bytes.
// It used to do 2 2-byte load/stores, which leads to a pipeline stall
// when we try to read the result with one 4-byte load.
public static void BenchmarkIssue18740(ж<testing.B> Ꮡb) {
    var benchmarks = new BenchmarkIssue18740_benchmarks[]{
        new("2byte"u8, 2, (slice<byte> buf) => (uint64)binary.LittleEndian.Uint16(buf)),
        new("4byte"u8, 4, (slice<byte> buf) => (uint64)binary.LittleEndian.Uint32(buf)),
        new("8byte"u8, 8, (slice<byte> buf) => binary.LittleEndian.Uint64(buf))
    }.slice();
    ref var g = ref heap(new array<byte>(4096), out var Ꮡg);
    foreach (var (_, vᴛ1) in benchmarks) {
        ref var bm = ref heap(new BenchmarkIssue18740_benchmarks(), out var Ꮡbm);
        bm = vᴛ1;

        var buf = new slice<byte>(bm.nbyte);
        var bmʗ1 = bm;
        var bufʗ1 = buf;
        var gʗ1 = g;
        Ꮡb.Run(bm.name, (ж<testing.B> bΔ1) => {
            for (nint j = 0; j < (~bΔ1).N; j++) {
                for (nint i = 0; i < 4096; i += bmʗ1.nbyte) {
                    copy(bufʗ1[..], gʗ1.slice(i));
                    sink += bmʗ1.f(bufʗ1[..]);
                }
            }
        });
    }
}

internal static slice<int8> memclrSink;

public static void BenchmarkMemclrKnownSize1(ж<testing.B> Ꮡb) {
    ref var b = ref Ꮡb.DerefOrNull();

    array<int8> x = new(1);
    b.SetBytes(1);
    for (nint i = 0; i < b.N; i++) {
        foreach (var (a, _) in x) {
            x[a] = 0;
        }
    }
    memclrSink = x[..];
}

public static void BenchmarkMemclrKnownSize2(ж<testing.B> Ꮡb) {
    ref var b = ref Ꮡb.DerefOrNull();

    array<int8> x = new(2);
    b.SetBytes(2);
    for (nint i = 0; i < b.N; i++) {
        foreach (var (a, _) in x) {
            x[a] = 0;
        }
    }
    memclrSink = x[..];
}

public static void BenchmarkMemclrKnownSize4(ж<testing.B> Ꮡb) {
    ref var b = ref Ꮡb.DerefOrNull();

    array<int8> x = new(4);
    b.SetBytes(4);
    for (nint i = 0; i < b.N; i++) {
        foreach (var (a, _) in x) {
            x[a] = 0;
        }
    }
    memclrSink = x[..];
}

public static void BenchmarkMemclrKnownSize8(ж<testing.B> Ꮡb) {
    ref var b = ref Ꮡb.DerefOrNull();

    array<int8> x = new(8);
    b.SetBytes(8);
    for (nint i = 0; i < b.N; i++) {
        foreach (var (a, _) in x) {
            x[a] = 0;
        }
    }
    memclrSink = x[..];
}

public static void BenchmarkMemclrKnownSize16(ж<testing.B> Ꮡb) {
    ref var b = ref Ꮡb.DerefOrNull();

    array<int8> x = new(16);
    b.SetBytes(16);
    for (nint i = 0; i < b.N; i++) {
        foreach (var (a, _) in x) {
            x[a] = 0;
        }
    }
    memclrSink = x[..];
}

public static void BenchmarkMemclrKnownSize32(ж<testing.B> Ꮡb) {
    ref var b = ref Ꮡb.DerefOrNull();

    array<int8> x = new(32);
    b.SetBytes(32);
    for (nint i = 0; i < b.N; i++) {
        foreach (var (a, _) in x) {
            x[a] = 0;
        }
    }
    memclrSink = x[..];
}

public static void BenchmarkMemclrKnownSize64(ж<testing.B> Ꮡb) {
    ref var b = ref Ꮡb.DerefOrNull();

    array<int8> x = new(64);
    b.SetBytes(64);
    for (nint i = 0; i < b.N; i++) {
        foreach (var (a, _) in x) {
            x[a] = 0;
        }
    }
    memclrSink = x[..];
}

public static void BenchmarkMemclrKnownSize112(ж<testing.B> Ꮡb) {
    ref var b = ref Ꮡb.DerefOrNull();

    array<int8> x = new(112);
    b.SetBytes(112);
    for (nint i = 0; i < b.N; i++) {
        foreach (var (a, _) in x) {
            x[a] = 0;
        }
    }
    memclrSink = x[..];
}

public static void BenchmarkMemclrKnownSize128(ж<testing.B> Ꮡb) {
    ref var b = ref Ꮡb.DerefOrNull();

    array<int8> x = new(128);
    b.SetBytes(128);
    for (nint i = 0; i < b.N; i++) {
        foreach (var (a, _) in x) {
            x[a] = 0;
        }
    }
    memclrSink = x[..];
}

public static void BenchmarkMemclrKnownSize192(ж<testing.B> Ꮡb) {
    ref var b = ref Ꮡb.DerefOrNull();

    array<int8> x = new(192);
    b.SetBytes(192);
    for (nint i = 0; i < b.N; i++) {
        foreach (var (a, _) in x) {
            x[a] = 0;
        }
    }
    memclrSink = x[..];
}

public static void BenchmarkMemclrKnownSize248(ж<testing.B> Ꮡb) {
    ref var b = ref Ꮡb.DerefOrNull();

    array<int8> x = new(248);
    b.SetBytes(248);
    for (nint i = 0; i < b.N; i++) {
        foreach (var (a, _) in x) {
            x[a] = 0;
        }
    }
    memclrSink = x[..];
}

public static void BenchmarkMemclrKnownSize256(ж<testing.B> Ꮡb) {
    ref var b = ref Ꮡb.DerefOrNull();

    array<int8> x = new(256);
    b.SetBytes(256);
    for (nint i = 0; i < b.N; i++) {
        foreach (var (a, _) in x) {
            x[a] = 0;
        }
    }
    memclrSink = x[..];
}

public static void BenchmarkMemclrKnownSize512(ж<testing.B> Ꮡb) {
    ref var b = ref Ꮡb.DerefOrNull();

    array<int8> x = new(512);
    b.SetBytes(512);
    for (nint i = 0; i < b.N; i++) {
        foreach (var (a, _) in x) {
            x[a] = 0;
        }
    }
    memclrSink = x[..];
}

public static void BenchmarkMemclrKnownSize1024(ж<testing.B> Ꮡb) {
    ref var b = ref Ꮡb.DerefOrNull();

    array<int8> x = new(1024);
    b.SetBytes(1024);
    for (nint i = 0; i < b.N; i++) {
        foreach (var (a, _) in x) {
            x[a] = 0;
        }
    }
    memclrSink = x[..];
}

public static void BenchmarkMemclrKnownSize4096(ж<testing.B> Ꮡb) {
    ref var b = ref Ꮡb.DerefOrNull();

    array<int8> x = new(4096);
    b.SetBytes(4096);
    for (nint i = 0; i < b.N; i++) {
        foreach (var (a, _) in x) {
            x[a] = 0;
        }
    }
    memclrSink = x[..];
}

public static void BenchmarkMemclrKnownSize512KiB(ж<testing.B> Ꮡb) {
    ref var b = ref Ꮡb.DerefOrNull();

    array<int8> x = new(524288);
    b.SetBytes(524288);
    for (nint i = 0; i < b.N; i++) {
        foreach (var (a, _) in x) {
            x[a] = 0;
        }
    }
    memclrSink = x[..];
}

} // end runtime_test_package
