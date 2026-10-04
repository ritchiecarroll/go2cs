// Copyright 2013 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.
namespace go;

using binary = encoding.binary_package;
using flag = flag_package;
using fmt = fmt_package;
using rand = global::go.math.rand_package;
using Δruntime = runtime_package;
using slices = slices_package;
using strconv = strconv_package;
using strings = strings_package;
using testing = testing_package;
using @unsafe = unsafe_package;
using MethodImplAttribute = global::System.Runtime.CompilerServices.MethodImplAttribute;
using MethodImplOptions = global::System.Runtime.CompilerServices.MethodImplOptions;
using encoding;
using global::go.math;
using static global::go.runtime_internal_test_package;

partial class runtime_test_package {

internal static ж<bool> mapbench = flag.Bool("mapbench"u8, false, "enable the full set of map benchmark variants"u8);

internal static UntypedInt size => 10;

public static void BenchmarkHashStringSpeed(ж<testing.B> Ꮡb) {
    ref var b = ref Ꮡb.DerefOrNull();

    var strings = new slice<@string>(size);
    for (nint i = 0; i < size; i++) {
        strings[i] = fmt.Sprintf("string#%d"u8, i);
    }
    nint sum = 0;
    var m = new map<@string, nint>(size);
    for (nint i = 0; i < size; i++) {
        m[strings[i]] = 0;
    }
    nint idx = 0;
    b.ResetTimer();
    for (nint i = 0; i < b.N; i++) {
        sum += m[strings[idx]];
        idx++;
        if (idx == size) {
            idx = 0;
        }
    }
}

[GoType("[17]byte")] partial struct chunk;

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static readonly object badMapEntryForChunkˢ = (@string)"bad map entry for chunk"u8;

public static void BenchmarkHashBytesSpeed(ж<testing.B> Ꮡb) {
    ref var b = ref Ꮡb.DerefOrNull();

    // a bunch of chunks, each with a different alignment mod 16
    array<chunk> chunks = new(10); /* size */
    // initialize each to a different value
    for (nint i = 0; i < size; i++) {
        chunks[i][0] = (byte)i;
    }
    // put into a map
    var m = new map<chunk, nint>(size);
    foreach (var (i, vᴛ1) in chunks.ΔRangeSnapshot()) {
        var c = vᴛ1.Clone();

        m[c] = i;
    }
    nint idx = 0;
    b.ResetTimer();
    for (nint i = 0; i < b.N; i++) {
        if (m[chunks[idx]] != idx) {
            Ꮡb.Error(badMapEntryForChunkˢ);
        }
        idx++;
        if (idx == size) {
            idx = 0;
        }
    }
}

public static void BenchmarkHashInt32Speed(ж<testing.B> Ꮡb) {
    ref var b = ref Ꮡb.DerefOrNull();

    var ints = new slice<int32>(size);
    for (nint i = 0; i < size; i++) {
        ints[i] = (int32)i;
    }
    nint sum = 0;
    var m = new map<int32, nint>(size);
    for (nint i = 0; i < size; i++) {
        m[ints[i]] = 0;
    }
    nint idx = 0;
    b.ResetTimer();
    for (nint i = 0; i < b.N; i++) {
        sum += m[ints[idx]];
        idx++;
        if (idx == size) {
            idx = 0;
        }
    }
}

public static void BenchmarkHashInt64Speed(ж<testing.B> Ꮡb) {
    ref var b = ref Ꮡb.DerefOrNull();

    var ints = new slice<int64>(size);
    for (nint i = 0; i < size; i++) {
        ints[i] = (int64)i;
    }
    nint sum = 0;
    var m = new map<int64, nint>(size);
    for (nint i = 0; i < size; i++) {
        m[ints[i]] = 0;
    }
    nint idx = 0;
    b.ResetTimer();
    for (nint i = 0; i < b.N; i++) {
        sum += m[ints[idx]];
        idx++;
        if (idx == size) {
            idx = 0;
        }
    }
}

public static void BenchmarkHashStringArraySpeed(ж<testing.B> Ꮡb) {
    ref var b = ref Ꮡb.DerefOrNull();

    var stringpairs = GoReflect.WithElemDims(new slice<array<@string>>(size, () => new(2)), 2);
    for (nint i = 0; i < size; i++) {
        for (nint j = 0; j < 2; j++) {
            stringpairs[i][j] = fmt.Sprintf("string#%d/%d"u8, i, j);
        }
    }
    nint sum = 0;
    var m = new map<array<@string>, nint>(size);
    for (nint i = 0; i < size; i++) {
        m[stringpairs[i]] = 0;
    }
    nint idx = 0;
    b.ResetTimer();
    for (nint i = 0; i < b.N; i++) {
        sum += m[stringpairs[idx]];
        idx++;
        if (idx == size) {
            idx = 0;
        }
    }
}

public static void BenchmarkMegMap(ж<testing.B> Ꮡb) {
    ref var b = ref Ꮡb.DerefOrNull();

    var m = new map<@string, bool>();
    for (var suffix = (rune)'A'; suffix <= (rune)'G'; suffix++) {
        m[strings.Repeat("X"u8, (1 << (int)(20)) - 1) + fmt.Sprint(suffix)] = true;
    }
    @string key = strings.Repeat("X"u8, (1 << (int)(20)) - 1) + "k"u8;
    b.ResetTimer();
    for (nint i = 0; i < b.N; i++) {
        (_, _) = m[key, ꟷ];
    }
}

public static void BenchmarkMegOneMap(ж<testing.B> Ꮡb) {
    ref var b = ref Ꮡb.DerefOrNull();

    var m = new map<@string, bool>();
    m[strings.Repeat("X"u8, (1 << (int)(20)))] = true;
    @string key = strings.Repeat("Y"u8, (1 << (int)(20)));
    b.ResetTimer();
    for (nint i = 0; i < b.N; i++) {
        (_, _) = m[key, ꟷ];
    }
}

public static void BenchmarkMegEqMap(ж<testing.B> Ꮡb) {
    ref var b = ref Ꮡb.DerefOrNull();

    var m = new map<@string, bool>();
    @string key1 = strings.Repeat("X"u8, (1 << (int)(20)));
    @string key2 = strings.Repeat("X"u8, (1 << (int)(20))); // equal but different instance
    m[key1] = true;
    b.ResetTimer();
    for (nint i = 0; i < b.N; i++) {
        (_, _) = m[key2, ꟷ];
    }
}

public static void BenchmarkMegEmptyMap(ж<testing.B> Ꮡb) {
    ref var b = ref Ꮡb.DerefOrNull();

    var m = new map<@string, bool>();
    @string key = strings.Repeat("X"u8, (1 << (int)(20)));
    b.ResetTimer();
    for (nint i = 0; i < b.N; i++) {
        (_, _) = m[key, ꟷ];
    }
}

public static void BenchmarkMegEmptyMapWithInterfaceKey(ж<testing.B> Ꮡb) {
    ref var b = ref Ꮡb.DerefOrNull();

    var m = new map<any, bool>();
    @string key = strings.Repeat("X"u8, (1 << (int)(20)));
    b.ResetTimer();
    for (nint i = 0; i < b.N; i++) {
        (_, _) = m[key, ꟷ];
    }
}

public static void BenchmarkSmallStrMap(ж<testing.B> Ꮡb) {
    ref var b = ref Ꮡb.DerefOrNull();

    var m = new map<@string, bool>();
    for (var suffix = (rune)'A'; suffix <= (rune)'G'; suffix++) {
        m[fmt.Sprint(suffix)] = true;
    }
    @string key = "k"u8;
    b.ResetTimer();
    for (nint i = 0; i < b.N; i++) {
        (_, _) = m[key, ꟷ];
    }
}

public static void BenchmarkMapStringKeysEight_16(ж<testing.B> Ꮡb) {
    benchmarkMapStringKeysEight(Ꮡb, 16);
}

public static void BenchmarkMapStringKeysEight_32(ж<testing.B> Ꮡb) {
    benchmarkMapStringKeysEight(Ꮡb, 32);
}

public static void BenchmarkMapStringKeysEight_64(ж<testing.B> Ꮡb) {
    benchmarkMapStringKeysEight(Ꮡb, 64);
}

public static void BenchmarkMapStringKeysEight_128(ж<testing.B> Ꮡb) {
    benchmarkMapStringKeysEight(Ꮡb, 128);
}

public static void BenchmarkMapStringKeysEight_256(ж<testing.B> Ꮡb) {
    benchmarkMapStringKeysEight(Ꮡb, 256);
}

public static void BenchmarkMapStringKeysEight_1M(ж<testing.B> Ꮡb) {
    benchmarkMapStringKeysEight(Ꮡb, (1 << (int)(20)));
}

internal static void benchmarkMapStringKeysEight(ж<testing.B> Ꮡb, nint keySize) {
    ref var b = ref Ꮡb.DerefOrNull();

    var m = new map<@string, bool>();
    for (nint i = 0; i < 8; i++) {
        m[strings.Repeat("K"u8, i + 1)] = true;
    }
    @string key = strings.Repeat("K"u8, keySize);
    b.ResetTimer();
    for (nint i = 0; i < b.N; i++) {
        _ = m[key];
    }
}

public static void BenchmarkMapFirst(ж<testing.B> Ꮡb) {
    for (nint nᴛ1 = 1; nᴛ1 <= 16; nᴛ1++) {
        var n = nᴛ1;
        Ꮡb.Run(fmt.Sprintf("%d"u8, n), (ж<testing.B> bΔ1) => {
            var m = new map<nint, bool>();
            for (nint i = 0; i < n; i++) {
                m[i] = true;
            }
            bΔ1.ResetTimer();
            for (nint i = 0; i < (~bΔ1).N; i++) {
                _ = m[0];
            }
        });
    }
}

public static void BenchmarkMapMid(ж<testing.B> Ꮡb) {
    for (nint nᴛ1 = 1; nᴛ1 <= 16; nᴛ1++) {
        var n = nᴛ1;
        Ꮡb.Run(fmt.Sprintf("%d"u8, n), (ж<testing.B> bΔ1) => {
            var m = new map<nint, bool>();
            for (nint i = 0; i < n; i++) {
                m[i] = true;
            }
            bΔ1.ResetTimer();
            for (nint i = 0; i < (~bΔ1).N; i++) {
                _ = m[(n >> (int)(1))];
            }
        });
    }
}

public static void BenchmarkMapLast(ж<testing.B> Ꮡb) {
    for (nint nᴛ1 = 1; nᴛ1 <= 16; nᴛ1++) {
        var n = nᴛ1;
        Ꮡb.Run(fmt.Sprintf("%d"u8, n), (ж<testing.B> bΔ1) => {
            var m = new map<nint, bool>();
            for (nint i = 0; i < n; i++) {
                m[i] = true;
            }
            bΔ1.ResetTimer();
            for (nint i = 0; i < (~bΔ1).N; i++) {
                _ = m[n - 1];
            }
        });
    }
}

internal static slice<nint> cyclicPermutation(nint n) {
    // From https://crypto.stackexchange.com/questions/51787/creating-single-cycle-permutations
    var p = rand.New(rand.NewSource(1)).Perm(n);
    var inc = new slice<nint>(n);
    var pInv = new slice<nint>(n);
    for (nint i = 0; i < n; i++) {
        inc[i] = rem((i + 1), n);
        pInv[p[i]] = i;
    }
    var res = new slice<nint>(n);
    for (nint i = 0; i < n; i++) {
        res[i] = pInv[inc[p[i]]];
    }
    // Test result.
    nint j = 0;
    for (nint i = 0; i < n - 1; i++) {
        j = res[j];
        if (j == 0) {
            throw panic("got back to 0 too early");
        }
    }
    j = res[j];
    if (j != 0) {
        throw panic("didn't get back to 0");
    }
    return res;
}

public static void BenchmarkMapCycle(ж<testing.B> Ꮡb) {
    ref var b = ref Ꮡb.DerefOrNull();

    // Arrange map entries to be a permutation, so that
    // we hit all entries, and one lookup is data dependent
    // on the previous lookup.
    const nint N = 3127;
    var p = cyclicPermutation(N);
    var m = new map<nint, nint>{};
    for (nint i = 0; i < N; i++) {
        m[i] = p[i];
    }
    b.ResetTimer();
    nint j = 0;
    for (nint i = 0; i < b.N; i++) {
        j = m[j];
    }
    sink = (uint64)j;
}

// Accessing the same keys in a row.
internal static void benchmarkRepeatedLookup(ж<testing.B> Ꮡb, nint lookupKeySize) {
    ref var b = ref Ꮡb.DerefOrNull();

    var m = new map<@string, bool>();
    // At least bigger than a single bucket:
    for (nint i = 0; i < 64; i++) {
        m[fmt.Sprintf("some key %d"u8, i)] = true;
    }
    @string @base = strings.Repeat("x"u8, lookupKeySize - 1);
    @string key1 = @base + "1"u8;
    @string key2 = @base + "2"u8;
    b.ResetTimer();
    for (nint i = 0; i < b.N / 4; i++) {
        _ = m[key1];
        _ = m[key1];
        _ = m[key2];
        _ = m[key2];
    }
}

public static void BenchmarkRepeatedLookupStrMapKey32(ж<testing.B> Ꮡb) {
    benchmarkRepeatedLookup(Ꮡb, 32);
}

public static void BenchmarkRepeatedLookupStrMapKey1M(ж<testing.B> Ꮡb) {
    benchmarkRepeatedLookup(Ꮡb, (1 << (int)(20)));
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static readonly @string byteByteˢ = "[Byte]Byte"u8;
internal static readonly @string intIntˢ = "[Int]Int"u8;

public static void BenchmarkMakeMap(ж<testing.B> Ꮡb) {
    Ꮡb.Run(byteByteˢ, (ж<testing.B> bΔ1) => {
        map<byte, byte> m = default!;
        for (nint i = 0; i < (~bΔ1).N; i++) {
            m = new map<byte, byte>(10);
        }
        hugeSink = m;
    });
    Ꮡb.Run(intIntˢ, (ж<testing.B> bΔ2) => {
        map<nint, nint> m = default!;
        for (nint i = 0; i < (~bΔ2).N; i++) {
            m = new map<nint, nint>(10);
        }
        hugeSink = m;
    });
}

public static void BenchmarkNewEmptyMap(ж<testing.B> Ꮡb) {
    ref var b = ref Ꮡb.DerefOrNull();

    b.ReportAllocs();
    for (nint i = 0; i < b.N; i++) {
        _ = new map<nint, nint>();
    }
}

public static void BenchmarkNewSmallMap(ж<testing.B> Ꮡb) {
    ref var b = ref Ꮡb.DerefOrNull();

    b.ReportAllocs();
    for (nint i = 0; i < b.N; i++) {
        var m = new map<nint, nint>();
        m[0] = 0;
        m[1] = 1;
    }
}

public static void BenchmarkSameLengthMap(ж<testing.B> Ꮡb) {
    ref var b = ref Ꮡb.DerefOrNull();

    // long strings, same length, differ in first few
    // and last few bytes.
    var m = new map<@string, bool>();
    @string s1 = "foo"u8 + strings.Repeat("-"u8, 100) + "bar"u8;
    @string s2 = "goo"u8 + strings.Repeat("-"u8, 100) + "ber"u8;
    m[s1] = true;
    m[s2] = true;
    b.ResetTimer();
    for (nint i = 0; i < b.N; i++) {
        _ = m[s1];
    }
}

public static void BenchmarkSmallKeyMap(ж<testing.B> Ꮡb) {
    ref var b = ref Ꮡb.DerefOrNull();

    var m = new map<int16, bool>();
    m[5] = true;
    for (nint i = 0; i < b.N; i++) {
        _ = m[5];
    }
}

public static void BenchmarkMapPopulate(ж<testing.B> Ꮡb) {
    for (nint sizeᴛ1 = 1; sizeᴛ1 < 1000000; sizeᴛ1 *= 10) {
        var size = sizeᴛ1;
        Ꮡb.Run(strconv.Itoa(size), (ж<testing.B> bΔ1) => {
            bΔ1.ReportAllocs();
            for (nint i = 0; i < (~bΔ1).N; i++) {
                var m = new map<nint, bool>();
                for (nint j = 0; j < size; j++) {
                    m[j] = true;
                }
            }
        });
    }
}

[GoType] partial struct ComplexAlgKey {
    internal int64 a, b, c;
    internal nint _;
    internal int32 d;
    internal nint __;
    internal @string e;
    internal nint ___;
    internal int64 f, g, h;
}

public static void BenchmarkComplexAlgMap(ж<testing.B> Ꮡb) {
    ref var b = ref Ꮡb.DerefOrNull();

    var m = new map<ComplexAlgKey, bool>();
    ComplexAlgKey k = default!;
    m[k] = true;
    for (nint i = 0; i < b.N; i++) {
        _ = m[k];
    }
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static readonly @string reflexiveˢ = "Reflexive"u8;
internal static readonly @string nonReflexiveˢ = "NonReflexive"u8;

public static void BenchmarkGoMapClear(ж<testing.B> Ꮡb) {
    Ꮡb.Run(reflexiveˢ, (ж<testing.B> bΔ1) => {
        for (nint sizeᴛ1 = 1; sizeᴛ1 < 100000; sizeᴛ1 *= 10) {
            var size = sizeᴛ1;
            bΔ1.Run(strconv.Itoa(size), (ж<testing.B> bΔ2) => {
                var m = new map<nint, nint>(size);
                for (nint i = 0; i < (~bΔ2).N; i++) {
                    m[0] = size; // Add one element so len(m) != 0 avoiding fast paths.
                    builtin.clear(m);
                }
            });
        }
    });
    Ꮡb.Run(nonReflexiveˢ, (ж<testing.B> bΔ3) => {
        for (nint sizeᴛ2 = 1; sizeᴛ2 < 100000; sizeᴛ2 *= 10) {
            var size = sizeᴛ2;
            bΔ3.Run(strconv.Itoa(size), (ж<testing.B> bΔ4) => {
                var m = new map<float64, nint>(size);
                for (nint i = 0; i < (~bΔ4).N; i++) {
                    m[1.0D] = size; // Add one element so len(m) != 0 avoiding fast paths.
                    builtin.clear(m);
                }
            });
        }
    });
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static readonly @string simpleˢ = "simple"u8;
internal static readonly @string structˢ4 = "struct"u8;
internal static readonly @string arrayˢ = "array"u8;

[GoType("dyn")] internal partial struct BenchmarkMapStringConversion_stringstruct {
    internal @string s;
}

[GoLocalName("stringarray")] [GoType("[1]@string")] internal partial struct BenchmarkMapStringConversion_stringarray;

public static void BenchmarkMapStringConversion(ж<testing.B> Ꮡb) {
    foreach (var (_, length) in new nint[]{32, 64}.slice()) {
        Ꮡb.Run(strconv.Itoa(length), (ж<testing.B> bΔ1) => {
            var bytes = new slice<byte>(length);
            var bytesʗ1 = bytes;
            bΔ1.Run(simpleˢ, (ж<testing.B> bΔ2) => {
                bΔ2.ReportAllocs();
                var m = new map<@string, nint>();
                m[((@string)bytesʗ1)] = 0;
                for (nint i = 0; i < (~bΔ2).N; i++) {
                    _ = m[tmpstring(bytesʗ1)];
                }
            });
            var bytesʗ2 = bytes;
            bΔ1.Run(structˢ4, (ж<testing.B> bΔ3) => {
                bΔ3.ReportAllocs();
                var m = new map<BenchmarkMapStringConversion_stringstruct, nint>();
                m[new BenchmarkMapStringConversion_stringstruct(((@string)bytesʗ2))] = 0;
                for (nint i = 0; i < (~bΔ3).N; i++) {
                    _ = m[new BenchmarkMapStringConversion_stringstruct(((@string)bytesʗ2))];
                }
            });
            var bytesʗ3 = bytes;
            bΔ1.Run(arrayˢ, (ж<testing.B> bΔ4) => {
                bΔ4.ReportAllocs();
                var m = new map<BenchmarkMapStringConversion_stringarray, nint>();
                m[new BenchmarkMapStringConversion_stringarray(new @string[]{((@string)bytesʗ3)}.array())] = 0;
                for (nint i = 0; i < (~bΔ4).N; i++) {
                    _ = m[new BenchmarkMapStringConversion_stringarray(new @string[]{((@string)bytesʗ3)}.array())];
                }
            });
        });
    }
}

public static bool BoolSink;

public static void BenchmarkMapInterfaceString(ж<testing.B> Ꮡb) {
    ref var b = ref Ꮡb.DerefOrNull();

    var m = new map<any, bool>{};
    for (nint i = 0; i < 100; i++) {
        m[fmt.Sprintf("%d"u8, i)] = true;
    }
    var key = ((any)(@string)("A"u8));
    b.ResetTimer();
    for (nint i = 0; i < b.N; i++) {
        BoolSink = m[key];
    }
}

public static void BenchmarkMapInterfacePtr(ж<testing.B> Ꮡb) {
    ref var b = ref Ꮡb.DerefOrNull();

    var m = new map<any, bool>{};
    for (nint i = 0; i < 100; i++) {
        ref var iΔ1 = ref heap<nint>(out var ᏑiΔ1);
        iΔ1 = i;
        m[ᏑiΔ1] = true;
    }
    var key = @new<nint>();
    b.ResetTimer();
    for (nint i = 0; i < b.N; i++) {
        BoolSink = m[key.OrTypedNil()];
    }
}

internal static nint hintLessThan8 = 7;
internal static nint hintGreaterThan8 = 32;

public static void BenchmarkNewEmptyMapHintLessThan8(ж<testing.B> Ꮡb) {
    ref var b = ref Ꮡb.DerefOrNull();

    b.ReportAllocs();
    for (nint i = 0; i < b.N; i++) {
        _ = new map<nint, nint>(hintLessThan8);
    }
}

public static void BenchmarkNewEmptyMapHintGreaterThan8(ж<testing.B> Ꮡb) {
    ref var b = ref Ꮡb.DerefOrNull();

    b.ReportAllocs();
    for (nint i = 0; i < b.N; i++) {
        _ = new map<nint, nint>(hintGreaterThan8);
    }
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static readonly object skippedBecauseMapbenchˢ = (@string)"Skipped because -mapbench=false"u8;

internal static Action<ж<testing.B>> benchSizes(Action<ж<testing.B>, nint> f) {
    slice<nint> cases = new nint[]{
        0,
        6,
        12,
        18,
        24,
        30,
        64,
        128,
        256,
        512,
        1024,
        2048,
        4096,
        8192,
        (1 << (int)(16)),
        (1 << (int)(18)),
        (1 << (int)(20)),
        (1 << (int)(22))
    }.slice();
    // Cases enabled by default. Set -mapbench for the remainder.
    //
    // With the other type combinations, there are literally thousands of
    // variations. It take too long to run all of these as part of
    // builders.
    var byDefault = new map<nint, bool>{
        [6] = true,
        [64] = true,
        [(1 << (int)(16))] = true
    };
    var byDefaultʗ1 = byDefault;
    var casesʗ1 = cases;
    return (ж<testing.B> b) => {
        foreach (var (_, n) in casesʗ1) {
            var byDefaultʗ2 = byDefaultʗ1;
            b.Run("len="u8 + strconv.Itoa(n), (ж<testing.B> bΔ1) => {
                if (!mapbench.Value && !byDefaultʗ2[n]) {
                    bΔ1.Skip(skippedBecauseMapbenchˢ);
                }
                f(bΔ1, n);
            });
        }
    };
}

internal static Action<ж<testing.B>> smallBenchSizes(Action<ж<testing.B>, nint> f) {
    return (ж<testing.B> b) => {
        for (nint nᴛ1 = 1; nᴛ1 <= 8; nᴛ1++) {
            var n = nᴛ1;
            b.Run("len="u8 + strconv.Itoa(n), (ж<testing.B> bΔ1) => {
                f(bΔ1, n);
            });
        }
    };
}

[GoType("[16]byte")] partial struct smallType;

[GoType("[512]byte")] /* [(1 << (int)(9))]byte */
partial struct mediumType;

[GoType("[4096]byte")] /* [(1 << (int)(12))]byte */
partial struct bigType;

[GoType] partial interface mapBenchmarkKeyType<ΔT> {
    //  Type constraints: int32 | int64 | string | smallType | mediumType | bigType | *int32
    // Derived operators: none
}

[GoType] partial interface mapBenchmarkElemType<ΔT> {
    //  Type constraints: mapBenchmarkKeyType | []int32
    // Derived operators: none
}

internal static slice<T> genIntValues<T>(nint start, nint end)
    where T : /* int | int32 | int64 */ IAdditionOperators<T, T, T>, ISubtractionOperators<T, T, T>, IMultiplyOperators<T, T, T>, IDivisionOperators<T, T, T>, IIncrementOperators<T>, IDecrementOperators<T>, IUnaryNegationOperators<T, T>, IModulusOperators<T, T, T>, IBitwiseOperators<T, T, T>, IShiftOperators<T, int, T>, IEqualityOperators<T, T, bool>, IComparisonOperators<T, T, bool>, new()
{
    var vals = new slice<T>(0, end - start);
    for (nint i = start; i < end; i++) {
        vals = append(vals, ConvertToType<T>(i));
    }
    return vals;
}

internal static slice<@string> genStringValues(nint start, nint end) {
    var vals = new slice<@string>(0, end - start);
    for (nint i = start; i < end; i++) {
        vals = append(vals, strconv.Itoa(i));
    }
    return vals;
}

internal static slice<smallType> genSmallValues(nint start, nint end) {
    var vals = GoReflect.WithElemDims(new slice<smallType>(0, end - start), 16);
    for (nint i = start; i < end; i++) {
        smallType v = default!;
        binary.NativeEndian.PutUint64(v[..], (uint64)i);
        vals = append(vals, v.Clone());
    }
    return vals;
}

internal static slice<mediumType> genMediumValues(nint start, nint end) {
    var vals = GoReflect.WithElemDims(new slice<mediumType>(0, end - start), 512);
    for (nint i = start; i < end; i++) {
        mediumType v = default!;
        binary.NativeEndian.PutUint64(v[..], (uint64)i);
        vals = append(vals, v.Clone());
    }
    return vals;
}

internal static slice<bigType> genBigValues(nint start, nint end) {
    var vals = GoReflect.WithElemDims(new slice<bigType>(0, end - start), 4096);
    for (nint i = start; i < end; i++) {
        bigType v = default!;
        binary.NativeEndian.PutUint64(v[..], (uint64)i);
        vals = append(vals, v.Clone());
    }
    return vals;
}

internal static slice<ж<T>> genPtrValues<T>(nint start, nint end) {
    // Start and end don't mean much. Each pointer by definition has a
    // unique identity.
    var vals = new slice<ж<T>>(0, end - start);
    for (nint i = start; i < end; i++) {
        var v = @new<T>();
        vals = append(vals, v);
    }
    return vals;
}

internal static slice<slice<T>> genIntSliceValues<T>(nint start, nint end)
    where T : /* int | int32 | int64 */ IAdditionOperators<T, T, T>, ISubtractionOperators<T, T, T>, IMultiplyOperators<T, T, T>, IDivisionOperators<T, T, T>, IIncrementOperators<T>, IDecrementOperators<T>, IUnaryNegationOperators<T, T>, IModulusOperators<T, T, T>, IBitwiseOperators<T, T, T>, IShiftOperators<T, int, T>, IEqualityOperators<T, T, bool>, IComparisonOperators<T, T, bool>, new()
{
    var vals = new slice<slice<T>>(0, end - start);
    for (nint i = start; i < end; i++) {
        vals = append(vals, new T[]{ConvertToType<T>(i)}.slice());
    }
    return vals;
}

internal static slice<T> genValues<T>(nint start, nint end) {
    T t = GoZero<T>();
    switch (((any)t).type()) {
    case int32: {
        return ((any)genIntValues<int32>(start, end))._<slice<T>>();
    }
    case int64: {
        return ((any)genIntValues<int64>(start, end))._<slice<T>>();
    }
    case @string: {
        return ((any)genStringValues(start, end))._<slice<T>>();
    }
    case smallType: {
        return ((any)genSmallValues(start, end))._<slice<T>>();
    }
    case mediumType: {
        return ((any)genMediumValues(start, end))._<slice<T>>();
    }
    case bigType: {
        return ((any)genBigValues(start, end))._<slice<T>>();
    }
    case ж<int32>: {
        return ((any)genPtrValues<int32>(start, end))._<slice<T>>();
    }
    case slice<int32>: {
        return ((any)genIntSliceValues<int32>(start, end))._<slice<T>>();
    }
    default: {
        throw panic("unreachable");
        break;
    }}

}

// Avoid inlining to force a heap allocation.
//
//go:noinline
[MethodImpl(MethodImplOptions.NoInlining)] internal static ж<T> newSink<T>() {
    return @new<T>();
}

// Return a new maps filled with keys and elems. Both slices must be the same length.
internal static map<K, E> fillMap<K, E>(slice<K> keys, slice<E> elems) {
    var m = new map<K, E>(len(keys));
    foreach (var (i, _) in keys) {
        m[keys[i]] = elems[i];
    }
    return m;
}

internal static nint iterCount(ж<testing.B> Ꮡb, nint n) {
    ref var b = ref Ꮡb.DerefOrNull();

    // Divide b.N by n so that the ns/op reports time per element,
    // not time per full map iteration. This makes benchmarks of
    // different map sizes more comparable.
    //
    // If size is zero we still need to do iterations.
    if (n == 0) {
        return b.N;
    }
    return quo(b.N, n);
}

internal static void checkAllocSize<K, E>(ж<testing.B> Ꮡb, nint n) {
    K k = GoZero<K>();
    var size = (uint64)n * (uint64)@unsafe.Sizeof(k);
    E e = GoZero<E>();
    size += (uint64)n * (uint64)@unsafe.Sizeof(e);
    if (size >= ((uint64)1 << (int)(30))) {
        Ꮡb.Skipf("Total key+elem size %d exceeds 1GiB"u8, size);
    }
}

[MethodImpl(MethodImplOptions.NoInlining)] internal static void benchmarkMapIter<K, E>(ж<testing.B> Ꮡb, nint n) {
    ref var b = ref Ꮡb.DerefOrNull();

    checkAllocSize<K, E>(Ꮡb, n);
    var k = genValues<K>(0, n);
    var e = genValues<E>(0, n);
    var m = fillMap(k, e);
    nint iterations = iterCount(Ꮡb, n);
    var sinkK = newSink<K>();
    var sinkE = newSink<E>();
    b.ResetTimer();
    for (nint i = 0; i < iterations; i++) {
        foreach (var (kΔ1, eΔ1) in m) {
            sinkK.ValueSlot = kΔ1;
            sinkE.ValueSlot = eΔ1;
        }
    }
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static readonly @string keyInt32ElemInt32ˢ = "Key=int32/Elem=int32"u8;
internal static readonly @string keyInt64ElemInt64ˢ = "Key=int64/Elem=int64"u8;
internal static readonly @string keyStringElemStringˢ = "Key=string/Elem=string"u8;
internal static readonly @string keySmallTypeElemInt32ˢ = "Key=smallType/Elem=int32"u8;
internal static readonly @string keyMediumTypeElemInt32ˢ = "Key=mediumType/Elem=int32"u8;
internal static readonly @string keyBigTypeElemInt32ˢ = "Key=bigType/Elem=int32"u8;
internal static readonly @string keyBigTypeElemBigTypeˢ = "Key=bigType/Elem=bigType"u8;
internal static readonly @string keyInt32ElemBigTypeˢ = "Key=int32/Elem=bigType"u8;
internal static readonly @string keyInt32ElemInt32ˢ2 = "Key=*int32/Elem=int32"u8;
internal static readonly @string keyInt32ElemInt32ˢ3 = "Key=int32/Elem=*int32"u8;

public static void BenchmarkMapIter(ж<testing.B> Ꮡb) {
    Ꮡb.Run(keyInt32ElemInt32ˢ, benchSizes(benchmarkMapIter<int32, int32>));
    Ꮡb.Run(keyInt64ElemInt64ˢ, benchSizes(benchmarkMapIter<int64, int64>));
    Ꮡb.Run(keyStringElemStringˢ, benchSizes(benchmarkMapIter<@string, @string>));
    Ꮡb.Run(keySmallTypeElemInt32ˢ, benchSizes(benchmarkMapIter<smallType, int32>));
    Ꮡb.Run(keyMediumTypeElemInt32ˢ, benchSizes(benchmarkMapIter<mediumType, int32>));
    Ꮡb.Run(keyBigTypeElemInt32ˢ, benchSizes(benchmarkMapIter<bigType, int32>));
    Ꮡb.Run(keyBigTypeElemBigTypeˢ, benchSizes(benchmarkMapIter<bigType, bigType>));
    Ꮡb.Run(keyInt32ElemBigTypeˢ, benchSizes(benchmarkMapIter<int32, bigType>));
    Ꮡb.Run(keyInt32ElemInt32ˢ2, benchSizes(benchmarkMapIter<ж<int32>, int32>));
    Ꮡb.Run(keyInt32ElemInt32ˢ3, benchSizes(benchmarkMapIter<int32, ж<int32>>));
}

[MethodImpl(MethodImplOptions.NoInlining)] internal static void benchmarkMapIterLowLoad<K, E>(ж<testing.B> Ꮡb, nint n) {
    ref var b = ref Ꮡb.DerefOrNull();

    // Only insert one entry regardless of map size.
    var k = genValues<K>(0, 1);
    var e = genValues<E>(0, 1);
    var m = new map<K, E>(n);
    foreach (var (i, _) in k) {
        m[k[i]] = e[i];
    }
    nint iterations = iterCount(Ꮡb, n);
    var sinkK = newSink<K>();
    var sinkE = newSink<E>();
    b.ResetTimer();
    for (nint i = 0; i < iterations; i++) {
        foreach (var (kΔ1, eΔ1) in m) {
            sinkK.ValueSlot = kΔ1;
            sinkE.ValueSlot = eΔ1;
        }
    }
}

public static void BenchmarkMapIterLowLoad(ж<testing.B> Ꮡb) {
    Ꮡb.Run(keyInt32ElemInt32ˢ, benchSizes(benchmarkMapIterLowLoad<int32, int32>));
    Ꮡb.Run(keyInt64ElemInt64ˢ, benchSizes(benchmarkMapIterLowLoad<int64, int64>));
    Ꮡb.Run(keyStringElemStringˢ, benchSizes(benchmarkMapIterLowLoad<@string, @string>));
    Ꮡb.Run(keySmallTypeElemInt32ˢ, benchSizes(benchmarkMapIterLowLoad<smallType, int32>));
    Ꮡb.Run(keyMediumTypeElemInt32ˢ, benchSizes(benchmarkMapIterLowLoad<mediumType, int32>));
    Ꮡb.Run(keyBigTypeElemInt32ˢ, benchSizes(benchmarkMapIterLowLoad<bigType, int32>));
    Ꮡb.Run(keyBigTypeElemBigTypeˢ, benchSizes(benchmarkMapIterLowLoad<bigType, bigType>));
    Ꮡb.Run(keyInt32ElemBigTypeˢ, benchSizes(benchmarkMapIterLowLoad<int32, bigType>));
    Ꮡb.Run(keyInt32ElemInt32ˢ2, benchSizes(benchmarkMapIterLowLoad<ж<int32>, int32>));
    Ꮡb.Run(keyInt32ElemInt32ˢ3, benchSizes(benchmarkMapIterLowLoad<int32, ж<int32>>));
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static readonly object canTAccessEmptyMapˢ = (@string)"can't access empty map"u8;

[MethodImpl(MethodImplOptions.NoInlining)] internal static void benchmarkMapAccessHit<K, E>(ж<testing.B> Ꮡb, nint n) {
    ref var b = ref Ꮡb.DerefOrNull();

    if (n == 0) {
        Ꮡb.Skip(canTAccessEmptyMapˢ);
    }
    checkAllocSize<K, E>(Ꮡb, n);
    var k = genValues<K>(0, n);
    var e = genValues<E>(0, n);
    var m = fillMap(k, e);
    var sink = newSink<E>();
    b.ResetTimer();
    for (nint i = 0; i < b.N; i++) {
        sink.ValueSlot = m[k[rem(i, n)]];
    }
}

public static void BenchmarkMapAccessHit(ж<testing.B> Ꮡb) {
    Ꮡb.Run(keyInt32ElemInt32ˢ, benchSizes(benchmarkMapAccessHit<int32, int32>));
    Ꮡb.Run(keyInt64ElemInt64ˢ, benchSizes(benchmarkMapAccessHit<int64, int64>));
    Ꮡb.Run(keyStringElemStringˢ, benchSizes(benchmarkMapAccessHit<@string, @string>));
    Ꮡb.Run(keySmallTypeElemInt32ˢ, benchSizes(benchmarkMapAccessHit<smallType, int32>));
    Ꮡb.Run(keyMediumTypeElemInt32ˢ, benchSizes(benchmarkMapAccessHit<mediumType, int32>));
    Ꮡb.Run(keyBigTypeElemInt32ˢ, benchSizes(benchmarkMapAccessHit<bigType, int32>));
    Ꮡb.Run(keyBigTypeElemBigTypeˢ, benchSizes(benchmarkMapAccessHit<bigType, bigType>));
    Ꮡb.Run(keyInt32ElemBigTypeˢ, benchSizes(benchmarkMapAccessHit<int32, bigType>));
    Ꮡb.Run(keyInt32ElemInt32ˢ2, benchSizes(benchmarkMapAccessHit<ж<int32>, int32>));
    Ꮡb.Run(keyInt32ElemInt32ˢ3, benchSizes(benchmarkMapAccessHit<int32, ж<int32>>));
}

internal static bool sinkOK;

internal static void benchmarkMapAccessMiss<K, E>(ж<testing.B> Ꮡb, nint n) {
    ref var b = ref Ꮡb.DerefOrNull();

    checkAllocSize<K, E>(Ꮡb, n);
    var k = genValues<K>(0, n);
    var e = genValues<E>(0, n);
    var m = fillMap(k, e);
    if (n == 0) {
        // Create a lookup values for empty maps.
        n = 1;
    }
    var w = genValues<K>(n, 2 * n);
    b.ResetTimer();
    bool ok = default!;
    for (nint i = 0; i < b.N; i++) {
        (_, ok) = m[w[rem(i, n)], ꟷ];
    }
    sinkOK = ok;
}

public static void BenchmarkMapAccessMiss(ж<testing.B> Ꮡb) {
    Ꮡb.Run(keyInt32ElemInt32ˢ, benchSizes(benchmarkMapAccessMiss<int32, int32>));
    Ꮡb.Run(keyInt64ElemInt64ˢ, benchSizes(benchmarkMapAccessMiss<int64, int64>));
    Ꮡb.Run(keyStringElemStringˢ, benchSizes(benchmarkMapAccessMiss<@string, @string>));
    Ꮡb.Run(keySmallTypeElemInt32ˢ, benchSizes(benchmarkMapAccessMiss<smallType, int32>));
    Ꮡb.Run(keyMediumTypeElemInt32ˢ, benchSizes(benchmarkMapAccessMiss<mediumType, int32>));
    Ꮡb.Run(keyBigTypeElemInt32ˢ, benchSizes(benchmarkMapAccessMiss<bigType, int32>));
    Ꮡb.Run(keyBigTypeElemBigTypeˢ, benchSizes(benchmarkMapAccessMiss<bigType, bigType>));
    Ꮡb.Run(keyInt32ElemBigTypeˢ, benchSizes(benchmarkMapAccessMiss<int32, bigType>));
    Ꮡb.Run(keyInt32ElemInt32ˢ2, benchSizes(benchmarkMapAccessMiss<ж<int32>, int32>));
    Ꮡb.Run(keyInt32ElemInt32ˢ3, benchSizes(benchmarkMapAccessMiss<int32, ж<int32>>));
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static readonly object canTAssignToExistingKeysˢ = (@string)"can't assign to existing keys in empty map"u8;

// Assign to a key that already exists.
internal static void benchmarkMapAssignExists<K, E>(ж<testing.B> Ꮡb, nint n) {
    ref var b = ref Ꮡb.DerefOrNull();

    if (n == 0) {
        Ꮡb.Skip(canTAssignToExistingKeysˢ);
    }
    checkAllocSize<K, E>(Ꮡb, n);
    var k = genValues<K>(0, n);
    var e = genValues<E>(0, n);
    var m = fillMap(k, e);
    b.ResetTimer();
    for (nint i = 0; i < b.N; i++) {
        m[k[rem(i, n)]] = e[rem(i, n)];
    }
}

public static void BenchmarkMapAssignExists(ж<testing.B> Ꮡb) {
    Ꮡb.Run(keyInt32ElemInt32ˢ, benchSizes(benchmarkMapAssignExists<int32, int32>));
    Ꮡb.Run(keyInt64ElemInt64ˢ, benchSizes(benchmarkMapAssignExists<int64, int64>));
    Ꮡb.Run(keyStringElemStringˢ, benchSizes(benchmarkMapAssignExists<@string, @string>));
    Ꮡb.Run(keySmallTypeElemInt32ˢ, benchSizes(benchmarkMapAssignExists<smallType, int32>));
    Ꮡb.Run(keyMediumTypeElemInt32ˢ, benchSizes(benchmarkMapAssignExists<mediumType, int32>));
    Ꮡb.Run(keyBigTypeElemInt32ˢ, benchSizes(benchmarkMapAssignExists<bigType, int32>));
    Ꮡb.Run(keyBigTypeElemBigTypeˢ, benchSizes(benchmarkMapAssignExists<bigType, bigType>));
    Ꮡb.Run(keyInt32ElemBigTypeˢ, benchSizes(benchmarkMapAssignExists<int32, bigType>));
    Ꮡb.Run(keyInt32ElemInt32ˢ2, benchSizes(benchmarkMapAssignExists<ж<int32>, int32>));
    Ꮡb.Run(keyInt32ElemInt32ˢ3, benchSizes(benchmarkMapAssignExists<int32, ж<int32>>));
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static readonly object canTCreateEmptyMapViaˢ = (@string)"can't create empty map via assignment"u8;

// Fill a map of size n with no hint. Time is per-key. A new map is created
// every n assignments.
//
// TODO(prattmic): Results don't make much sense if b.N < n.
// TODO(prattmic): Measure distribution of assign time to reveal the grow
// latency.
internal static void benchmarkMapAssignFillNoHint<K, E>(ж<testing.B> Ꮡb, nint n) {
    ref var b = ref Ꮡb.DerefOrNull();

    if (n == 0) {
        Ꮡb.Skip(canTCreateEmptyMapViaˢ);
    }
    checkAllocSize<K, E>(Ꮡb, n);
    var k = genValues<K>(0, n);
    var e = genValues<E>(0, n);
    b.ResetTimer();
    map<K, E> m = default!;
    for (nint i = 0; i < b.N; i++) {
        if (rem(i, n) == 0) {
            m = new map<K, E>();
        }
        m[k[rem(i, n)]] = e[rem(i, n)];
    }
}

public static void BenchmarkMapAssignFillNoHint(ж<testing.B> Ꮡb) {
    Ꮡb.Run(keyInt32ElemInt32ˢ, benchSizes(benchmarkMapAssignFillNoHint<int32, int32>));
    Ꮡb.Run(keyInt64ElemInt64ˢ, benchSizes(benchmarkMapAssignFillNoHint<int64, int64>));
    Ꮡb.Run(keyStringElemStringˢ, benchSizes(benchmarkMapAssignFillNoHint<@string, @string>));
    Ꮡb.Run(keySmallTypeElemInt32ˢ, benchSizes(benchmarkMapAssignFillNoHint<smallType, int32>));
    Ꮡb.Run(keyMediumTypeElemInt32ˢ, benchSizes(benchmarkMapAssignFillNoHint<mediumType, int32>));
    Ꮡb.Run(keyBigTypeElemInt32ˢ, benchSizes(benchmarkMapAssignFillNoHint<bigType, int32>));
    Ꮡb.Run(keyBigTypeElemBigTypeˢ, benchSizes(benchmarkMapAssignFillNoHint<bigType, bigType>));
    Ꮡb.Run(keyInt32ElemBigTypeˢ, benchSizes(benchmarkMapAssignFillNoHint<int32, bigType>));
    Ꮡb.Run(keyInt32ElemInt32ˢ2, benchSizes(benchmarkMapAssignFillNoHint<ж<int32>, int32>));
    Ꮡb.Run(keyInt32ElemInt32ˢ3, benchSizes(benchmarkMapAssignFillNoHint<int32, ж<int32>>));
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static readonly @string p50NsOpˢ = "p50-ns/op"u8;
internal static readonly @string p99NsOpˢ = "p99-ns/op"u8;
internal static readonly @string p999NsOpˢ = "p99.9-ns/op"u8;
internal static readonly @string p9999NsOpˢ = "p99.99-ns/op"u8;
internal static readonly @string p100NsOpˢ = "p100-ns/op"u8;

// Identical to benchmarkMapAssignFillNoHint, but additionally measures the
// latency of each mapassign to report tail latency due to map grow.
internal static void benchmarkMapAssignGrowLatency<K, E>(ж<testing.B> Ꮡb, nint n) {
    ref var b = ref Ꮡb.DerefOrNull();

    if (n == 0) {
        Ꮡb.Skip(canTCreateEmptyMapViaˢ);
    }
    checkAllocSize<K, E>(Ꮡb, n);
    var k = genValues<K>(0, n);
    var e = genValues<E>(0, n);
    // Store the run time of each mapassign. Keeping the full data rather
    // than a histogram provides higher precision. b.N tends to be <10M, so
    // the memory requirement isn't too bad.
    var sample = new slice<int64>(b.N);
    b.ResetTimer();
    map<K, E> m = default!;
    for (nint i = 0; i < b.N; i++) {
        if (rem(i, n) == 0) {
            m = new map<K, E>();
        }
        var start = runtime_internal_test_package.Nanotime();
        m[k[rem(i, n)]] = e[rem(i, n)];
        var end = runtime_internal_test_package.Nanotime();
        sample[i] = end - start;
    }
    b.StopTimer();
    slices.Sort<slice<int64>, int64>(sample);
    // TODO(prattmic): Grow is so rare that even p99.99 often doesn't
    // display a grow case. Switch to a more direct measure of grow cases
    // only?
    b.ReportMetric((float64)sample[(nint)((float64)len(sample) * 0.5D)], p50NsOpˢ);
    b.ReportMetric((float64)sample[(nint)((float64)len(sample) * 0.99D)], p99NsOpˢ);
    b.ReportMetric((float64)sample[(nint)((float64)len(sample) * 0.999D)], p999NsOpˢ);
    b.ReportMetric((float64)sample[(nint)((float64)len(sample) * 0.9999D)], p9999NsOpˢ);
    b.ReportMetric((float64)sample[len(sample) - 1], p100NsOpˢ);
}

public static void BenchmarkMapAssignGrowLatency(ж<testing.B> Ꮡb) {
    Ꮡb.Run(keyInt32ElemInt32ˢ, benchSizes(benchmarkMapAssignGrowLatency<int32, int32>));
    Ꮡb.Run(keyInt64ElemInt64ˢ, benchSizes(benchmarkMapAssignGrowLatency<int64, int64>));
    Ꮡb.Run(keyStringElemStringˢ, benchSizes(benchmarkMapAssignGrowLatency<@string, @string>));
    Ꮡb.Run(keySmallTypeElemInt32ˢ, benchSizes(benchmarkMapAssignGrowLatency<smallType, int32>));
    Ꮡb.Run(keyMediumTypeElemInt32ˢ, benchSizes(benchmarkMapAssignGrowLatency<mediumType, int32>));
    Ꮡb.Run(keyBigTypeElemInt32ˢ, benchSizes(benchmarkMapAssignGrowLatency<bigType, int32>));
    Ꮡb.Run(keyBigTypeElemBigTypeˢ, benchSizes(benchmarkMapAssignGrowLatency<bigType, bigType>));
    Ꮡb.Run(keyInt32ElemBigTypeˢ, benchSizes(benchmarkMapAssignGrowLatency<int32, bigType>));
    Ꮡb.Run(keyInt32ElemInt32ˢ2, benchSizes(benchmarkMapAssignGrowLatency<ж<int32>, int32>));
    Ꮡb.Run(keyInt32ElemInt32ˢ3, benchSizes(benchmarkMapAssignGrowLatency<int32, ж<int32>>));
}

// Fill a map of size n with size hint. Time is per-key. A new map is created
// every n assignments.
//
// TODO(prattmic): Results don't make much sense if b.N < n.
internal static void benchmarkMapAssignFillHint<K, E>(ж<testing.B> Ꮡb, nint n) {
    ref var b = ref Ꮡb.DerefOrNull();

    if (n == 0) {
        Ꮡb.Skip(canTCreateEmptyMapViaˢ);
    }
    checkAllocSize<K, E>(Ꮡb, n);
    var k = genValues<K>(0, n);
    var e = genValues<E>(0, n);
    b.ResetTimer();
    map<K, E> m = default!;
    for (nint i = 0; i < b.N; i++) {
        if (rem(i, n) == 0) {
            m = new map<K, E>(n);
        }
        m[k[rem(i, n)]] = e[rem(i, n)];
    }
}

public static void BenchmarkMapAssignFillHint(ж<testing.B> Ꮡb) {
    Ꮡb.Run(keyInt32ElemInt32ˢ, benchSizes(benchmarkMapAssignFillHint<int32, int32>));
    Ꮡb.Run(keyInt64ElemInt64ˢ, benchSizes(benchmarkMapAssignFillHint<int64, int64>));
    Ꮡb.Run(keyStringElemStringˢ, benchSizes(benchmarkMapAssignFillHint<@string, @string>));
    Ꮡb.Run(keySmallTypeElemInt32ˢ, benchSizes(benchmarkMapAssignFillHint<smallType, int32>));
    Ꮡb.Run(keyMediumTypeElemInt32ˢ, benchSizes(benchmarkMapAssignFillHint<mediumType, int32>));
    Ꮡb.Run(keyBigTypeElemInt32ˢ, benchSizes(benchmarkMapAssignFillHint<bigType, int32>));
    Ꮡb.Run(keyBigTypeElemBigTypeˢ, benchSizes(benchmarkMapAssignFillHint<bigType, bigType>));
    Ꮡb.Run(keyInt32ElemBigTypeˢ, benchSizes(benchmarkMapAssignFillHint<int32, bigType>));
    Ꮡb.Run(keyInt32ElemInt32ˢ2, benchSizes(benchmarkMapAssignFillHint<ж<int32>, int32>));
    Ꮡb.Run(keyInt32ElemInt32ˢ3, benchSizes(benchmarkMapAssignFillHint<int32, ж<int32>>));
}

// Fill a map of size n, reusing the same map. Time is per-key. The map is
// cleared every n assignments.
//
// TODO(prattmic): Results don't make much sense if b.N < n.
internal static void benchmarkMapAssignFillClear<K, E>(ж<testing.B> Ꮡb, nint n) {
    ref var b = ref Ꮡb.DerefOrNull();

    if (n == 0) {
        Ꮡb.Skip(canTCreateEmptyMapViaˢ);
    }
    checkAllocSize<K, E>(Ꮡb, n);
    var k = genValues<K>(0, n);
    var e = genValues<E>(0, n);
    var m = fillMap(k, e);
    b.ResetTimer();
    for (nint i = 0; i < b.N; i++) {
        if (rem(i, n) == 0) {
            builtin.clear(m);
        }
        m[k[rem(i, n)]] = e[rem(i, n)];
    }
}

public static void BenchmarkMapAssignFillClear(ж<testing.B> Ꮡb) {
    Ꮡb.Run(keyInt32ElemInt32ˢ, benchSizes(benchmarkMapAssignFillClear<int32, int32>));
    Ꮡb.Run(keyInt64ElemInt64ˢ, benchSizes(benchmarkMapAssignFillClear<int64, int64>));
    Ꮡb.Run(keyStringElemStringˢ, benchSizes(benchmarkMapAssignFillClear<@string, @string>));
    Ꮡb.Run(keySmallTypeElemInt32ˢ, benchSizes(benchmarkMapAssignFillClear<smallType, int32>));
    Ꮡb.Run(keyMediumTypeElemInt32ˢ, benchSizes(benchmarkMapAssignFillClear<mediumType, int32>));
    Ꮡb.Run(keyBigTypeElemInt32ˢ, benchSizes(benchmarkMapAssignFillClear<bigType, int32>));
    Ꮡb.Run(keyBigTypeElemBigTypeˢ, benchSizes(benchmarkMapAssignFillClear<bigType, bigType>));
    Ꮡb.Run(keyInt32ElemBigTypeˢ, benchSizes(benchmarkMapAssignFillClear<int32, bigType>));
    Ꮡb.Run(keyInt32ElemInt32ˢ2, benchSizes(benchmarkMapAssignFillClear<ж<int32>, int32>));
    Ꮡb.Run(keyInt32ElemInt32ˢ3, benchSizes(benchmarkMapAssignFillClear<int32, ж<int32>>));
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static readonly object canTModifyEmptyMapViaˢ = (@string)"can't modify empty map via assignment"u8;

// Modify values using +=.
internal static void benchmarkMapAssignAddition<K, E>(ж<testing.B> Ꮡb, nint n)
    where E : /* int32 | int64 | string */ IAdditionOperators<E, E, E>, IEqualityOperators<E, E, bool>, IComparisonOperators<E, E, bool>, new()
{
    ref var b = ref Ꮡb.DerefOrNull();

    if (n == 0) {
        Ꮡb.Skip(canTModifyEmptyMapViaˢ);
    }
    checkAllocSize<K, E>(Ꮡb, n);
    var k = genValues<K>(0, n);
    var e = genValues<E>(0, n);
    var m = fillMap(k, e);
    b.ResetTimer();
    for (nint i = 0; i < b.N; i++) {
        m[k[rem(i, n)]] += e[rem(i, n)];
    }
}

public static void BenchmarkMapAssignAddition(ж<testing.B> Ꮡb) {
    Ꮡb.Run(keyInt32ElemInt32ˢ, benchSizes(benchmarkMapAssignAddition<int32, int32>));
    Ꮡb.Run(keyInt64ElemInt64ˢ, benchSizes(benchmarkMapAssignAddition<int64, int64>));
    Ꮡb.Run(keyStringElemStringˢ, benchSizes(benchmarkMapAssignAddition<@string, @string>));
    Ꮡb.Run(keySmallTypeElemInt32ˢ, benchSizes(benchmarkMapAssignAddition<smallType, int32>));
    Ꮡb.Run(keyMediumTypeElemInt32ˢ, benchSizes(benchmarkMapAssignAddition<mediumType, int32>));
    Ꮡb.Run(keyBigTypeElemInt32ˢ, benchSizes(benchmarkMapAssignAddition<bigType, int32>));
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static readonly object canTModifyEmptyMapViaˢ2 = (@string)"can't modify empty map via append"u8;

// Modify values append.
internal static void benchmarkMapAssignAppend<K>(ж<testing.B> Ꮡb, nint n) {
    ref var b = ref Ꮡb.DerefOrNull();

    if (n == 0) {
        Ꮡb.Skip(canTModifyEmptyMapViaˢ2);
    }
    checkAllocSize<K, slice<int32>>(Ꮡb, n);
    var k = genValues<K>(0, n);
    var e = genValues<slice<int32>>(0, n);
    var m = fillMap(k, e);
    b.ResetTimer();
    for (nint i = 0; i < b.N; i++) {
        m[k[rem(i, n)]] = append(m[k[rem(i, n)]], e[rem(i, n)][0]);
    }
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static readonly @string keyInt32ElemInt32ˢ4 = "Key=int32/Elem=[]int32"u8;
internal static readonly @string keyInt64ElemInt32ˢ = "Key=int64/Elem=[]int32"u8;
internal static readonly @string keyStringElemInt32ˢ = "Key=string/Elem=[]int32"u8;

public static void BenchmarkMapAssignAppend(ж<testing.B> Ꮡb) {
    Ꮡb.Run(keyInt32ElemInt32ˢ4, benchSizes(benchmarkMapAssignAppend<int32>));
    Ꮡb.Run(keyInt64ElemInt32ˢ, benchSizes(benchmarkMapAssignAppend<int64>));
    Ꮡb.Run(keyStringElemInt32ˢ, benchSizes(benchmarkMapAssignAppend<@string>));
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static readonly object canTDeleteFromEmptyMapˢ = (@string)"can't delete from empty map"u8;

internal static void benchmarkMapDelete<K, E>(ж<testing.B> Ꮡb, nint n) {
    ref var b = ref Ꮡb.DerefOrNull();

    if (n == 0) {
        Ꮡb.Skip(canTDeleteFromEmptyMapˢ);
    }
    checkAllocSize<K, E>(Ꮡb, n);
    var k = genValues<K>(0, n);
    var e = genValues<E>(0, n);
    var m = fillMap(k, e);
    b.ResetTimer();
    for (nint i = 0; i < b.N; i++) {
        if (len(m) == 0) {
            // We'd like to StopTimer while refilling the map, but
            // it is way too expensive and thus makes the benchmark
            // take a long time. See https://go.dev/issue/20875.
            foreach (var (j, _) in k) {
                m[k[j]] = e[j];
            }
        }
        delete(m, k[rem(i, n)]);
    }
}

public static void BenchmarkMapDelete(ж<testing.B> Ꮡb) {
    Ꮡb.Run(keyInt32ElemInt32ˢ, benchSizes(benchmarkMapDelete<int32, int32>));
    Ꮡb.Run(keyInt64ElemInt64ˢ, benchSizes(benchmarkMapDelete<int64, int64>));
    Ꮡb.Run(keyStringElemStringˢ, benchSizes(benchmarkMapDelete<@string, @string>));
    Ꮡb.Run(keySmallTypeElemInt32ˢ, benchSizes(benchmarkMapDelete<smallType, int32>));
    Ꮡb.Run(keyMediumTypeElemInt32ˢ, benchSizes(benchmarkMapDelete<mediumType, int32>));
    Ꮡb.Run(keyBigTypeElemInt32ˢ, benchSizes(benchmarkMapDelete<bigType, int32>));
    Ꮡb.Run(keyBigTypeElemBigTypeˢ, benchSizes(benchmarkMapDelete<bigType, bigType>));
    Ꮡb.Run(keyInt32ElemBigTypeˢ, benchSizes(benchmarkMapDelete<int32, bigType>));
    Ꮡb.Run(keyInt32ElemInt32ˢ2, benchSizes(benchmarkMapDelete<ж<int32>, int32>));
    Ꮡb.Run(keyInt32ElemInt32ˢ3, benchSizes(benchmarkMapDelete<int32, ж<int32>>));
}

// Use iterator to pop an element. We want this to be fast, see
// https://go.dev/issue/8412.
internal static void benchmarkMapPop<K, E>(ж<testing.B> Ꮡb, nint n) {
    ref var b = ref Ꮡb.DerefOrNull();

    if (n == 0) {
        Ꮡb.Skip(canTDeleteFromEmptyMapˢ);
    }
    checkAllocSize<K, E>(Ꮡb, n);
    var k = genValues<K>(0, n);
    var e = genValues<E>(0, n);
    var m = fillMap(k, e);
    b.ResetTimer();
    for (nint i = 0; i < b.N; i++) {
        if (len(m) == 0) {
            // We'd like to StopTimer while refilling the map, but
            // it is way too expensive and thus makes the benchmark
            // take a long time. See https://go.dev/issue/20875.
            foreach (var (j, _) in k) {
                m[k[j]] = e[j];
            }
        }
        foreach (var (key, _) in m) {
            delete(m, key);
            break;
        }
    }
}

public static void BenchmarkMapPop(ж<testing.B> Ꮡb) {
    Ꮡb.Run(keyInt32ElemInt32ˢ, benchSizes(benchmarkMapPop<int32, int32>));
    Ꮡb.Run(keyInt64ElemInt64ˢ, benchSizes(benchmarkMapPop<int64, int64>));
    Ꮡb.Run(keyStringElemStringˢ, benchSizes(benchmarkMapPop<@string, @string>));
    Ꮡb.Run(keySmallTypeElemInt32ˢ, benchSizes(benchmarkMapPop<smallType, int32>));
    Ꮡb.Run(keyMediumTypeElemInt32ˢ, benchSizes(benchmarkMapPop<mediumType, int32>));
    Ꮡb.Run(keyBigTypeElemInt32ˢ, benchSizes(benchmarkMapPop<bigType, int32>));
    Ꮡb.Run(keyBigTypeElemBigTypeˢ, benchSizes(benchmarkMapPop<bigType, bigType>));
    Ꮡb.Run(keyInt32ElemBigTypeˢ, benchSizes(benchmarkMapPop<int32, bigType>));
    Ꮡb.Run(keyInt32ElemInt32ˢ2, benchSizes(benchmarkMapPop<ж<int32>, int32>));
    Ꮡb.Run(keyInt32ElemInt32ˢ3, benchSizes(benchmarkMapPop<int32, ж<int32>>));
}

public static void BenchmarkMapDeleteLargeKey(ж<testing.B> Ꮡb) {
    ref var b = ref Ꮡb.DerefOrNull();

    var m = new map<@string, nint>{};
    foreach (var i in range(9)) {
        m[fmt.Sprintf("%d"u8, i)] = i;
    }
    @string key = strings.Repeat("*"u8, 10000);
    foreach (var _ᴛ1 in range(b.N)) {
        delete(m, key);
    }
}

public static void BenchmarkMapSmallAccessHit(ж<testing.B> Ꮡb) {
    Ꮡb.Run(keyInt32ElemInt32ˢ, smallBenchSizes(benchmarkMapAccessHit<int32, int32>));
    Ꮡb.Run(keyInt64ElemInt64ˢ, smallBenchSizes(benchmarkMapAccessHit<int64, int64>));
    Ꮡb.Run(keyStringElemStringˢ, smallBenchSizes(benchmarkMapAccessHit<@string, @string>));
}

public static void BenchmarkMapSmallAccessMiss(ж<testing.B> Ꮡb) {
    Ꮡb.Run(keyInt32ElemInt32ˢ, smallBenchSizes(benchmarkMapAccessMiss<int32, int32>));
    Ꮡb.Run(keyInt64ElemInt64ˢ, smallBenchSizes(benchmarkMapAccessMiss<int64, int64>));
    Ꮡb.Run(keyStringElemStringˢ, smallBenchSizes(benchmarkMapAccessMiss<@string, @string>));
}

} // end runtime_test_package
