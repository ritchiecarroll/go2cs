// Copyright 2013 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.
namespace go;

using binary = encoding.binary_package;
using fmt = fmt_package;
using race = @internal.race_package;
using testenv = @internal.testenv_package;
using Δmath = math_package;
using rand = global::go.math.rand_package;
using Δos = os_package;
using static runtime_package;
using slices = slices_package;
using strings = strings_package;
using testing = testing_package;
using @unsafe = unsafe_package;
using @internal;
using MethodImplAttribute = global::System.Runtime.CompilerServices.MethodImplAttribute;
using MethodImplOptions = global::System.Runtime.CompilerServices.MethodImplOptions;
using encoding;
using exec = global::go.os.exec_package;
using global::go.math;
using global::go.os;
using static global::go.runtime_internal_test_package;

partial class runtime_test_package {

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static readonly object skippingSinceAesHashˢ = (@string)"skipping since AES hash implementation is used"u8;

public static void TestMemHash32Equality(ж<testing.T> Ꮡt) {
    if (runtime_internal_test_package.UseAeshash.Value) {
        Ꮡt.Skip(skippingSinceAesHashˢ);
    }
    ref var b = ref heap(new array<byte>(4), out var Ꮡb);
    var r = rand.New(rand.NewSource(1234));
    var seed = (uintptr)r.Uint64();
    for (nint i = 0; i < 100; i++) {
        randBytes(r, b[..]);
        var got = runtime_internal_test_package.MemHash32(@unsafe.Pointer.FromPinnedBox(Ꮡb), seed);
        var want = runtime_internal_test_package.MemHash(@unsafe.Pointer.FromPinnedBox(Ꮡb), seed, 4);
        if (got != want) {
            Ꮡt.Errorf("MemHash32(%x, %v) = %v; want %v"u8, b, seed, got, want);
        }
    }
}

public static void TestMemHash64Equality(ж<testing.T> Ꮡt) {
    if (runtime_internal_test_package.UseAeshash.Value) {
        Ꮡt.Skip(skippingSinceAesHashˢ);
    }
    ref var b = ref heap(new array<byte>(8), out var Ꮡb);
    var r = rand.New(rand.NewSource(1234));
    var seed = (uintptr)r.Uint64();
    for (nint i = 0; i < 100; i++) {
        randBytes(r, b[..]);
        var got = runtime_internal_test_package.MemHash64(@unsafe.Pointer.FromPinnedBox(Ꮡb), seed);
        var want = runtime_internal_test_package.MemHash(@unsafe.Pointer.FromPinnedBox(Ꮡb), seed, 8);
        if (got != want) {
            Ꮡt.Errorf("MemHash64(%x, %v) = %v; want %v"u8, b, seed, got, want);
        }
    }
}

// Smhasher is a torture test for hash functions.
// https://code.google.com/p/smhasher/
// This code is a port of some of the Smhasher tests to Go.
//
// The current AES hash function passes Smhasher. Our fallback
// hash functions don't, so we only enable the difficult tests when
// we know the AES implementation is available.

// Sanity checks.
// hash should not depend on values outside key.
// hash should not depend on alignment.
public static void TestSmhasherSanity(ж<testing.T> Ꮡt) {
    var r = rand.New(rand.NewSource(1234));
    const nint REP = 10;
    UntypedInt KEYMAX = 128;
    UntypedInt PAD = 16;
    UntypedInt OFFMAX = 16;
    for (nint k = 0; k < REP; k++) {
        for (nint n = 0; n < KEYMAX; n++) {
            for (nint i = 0; i < OFFMAX; i++) {
                array<byte> b = new(176); /* KEYMAX + OFFMAX + 2 * PAD */
                array<byte> c = new(176); /* KEYMAX + OFFMAX + 2 * PAD */
                randBytes(r, b[..]);
                randBytes(r, c[..]);
                copy(c.slice((nint)PAD + i, (nint)PAD + i + n), b.slice(PAD, (nint)PAD + n));
                if (runtime_internal_test_package.BytesHash(b.slice(PAD, (nint)PAD + n), 0) != runtime_internal_test_package.BytesHash(c.slice((nint)PAD + i, (nint)PAD + i + n), 0)) {
                    Ꮡt.Errorf("hash depends on bytes outside key"u8);
                }
            }
        }
    }
}

partial struct HashSet {
    internal slice<uintptr> list; // list of hashes added
}

internal static partial ж<HashSet> newHashSet() {
    return Ꮡ(new HashSet(list: new slice<uintptr>(0, 1024)));
}

internal static void add(this ref HashSet s, uintptr h) {
    s.list = append(s.list, h);
}

internal static void addS(this ref HashSet s, @string x) {
    s.add(runtime_internal_test_package.StringHash(x, 0));
}

internal static void addB(this ref HashSet s, slice<byte> x) {
    s.add(runtime_internal_test_package.BytesHash(x, 0));
}

internal static void addS_seed(this ref HashSet s, @string x, uintptr seed) {
    s.add(runtime_internal_test_package.StringHash(x, seed));
}

internal static void check(this ref HashSet s, ж<testing.T> Ꮡt) {
    var list = s.list;
    slices.Sort<slice<uintptr>, uintptr>(list);
    nint collisions = 0;
    for (nint i = 1; i < len(list); i++) {
        if (list[i] == list[i - 1]) {
            collisions++;
        }
    }
    nint n = len(list);
    const float64 SLOP = 50.0;
    var pairs = (int64)n * (int64)(n - 1) / 2;
    var expected = (float64)pairs / Δmath.Pow(2.0D, (float64)hashSize);
    var stddev = Δmath.Sqrt(expected);
    if ((float64)collisions > expected + SLOP * (3D * stddev + 1D)) {
        Ꮡt.Errorf("unexpected number of collisions: got=%d mean=%f stddev=%f threshold=%f"u8, collisions, expected, stddev, expected + SLOP * (3D * stddev + 1D));
    }
    // Reset for reuse
    s.list = s.list[..0];
}

// a string plus adding zeros must make distinct hashes
public static void TestSmhasherAppendedZeros(ж<testing.T> Ꮡt) {
    @string s = "hello"u8 + strings.Repeat("\x00"u8, 256);
    var h = newHashSet();
    for (nint i = 0; i <= len(s); i++) {
        h.addS(s.slice(0, i));
    }
    h.check(Ꮡt);
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static readonly object tooLongForRaceModeˢ = (@string)"Too long for race mode"u8;

// All 0-3 byte strings have distinct hashes.
public static void TestSmhasherSmallKeys(ж<testing.T> Ꮡt) {
    if (race.Enabled) {
        Ꮡt.Skip(tooLongForRaceModeˢ);
    }
    testenv.ParallelOn64Bit(Ꮡt);
    var h = newHashSet();
    array<byte> b = new(3);
    for (nint i = 0; i < 256; i++) {
        b[0] = (byte)i;
        h.addB(b[..1]);
        for (nint j = 0; j < 256; j++) {
            b[1] = (byte)j;
            h.addB(b[..2]);
            if (!testing.Short()) {
                for (nint k = 0; k < 256; k++) {
                    b[2] = (byte)k;
                    h.addB(b[..3]);
                }
            }
        }
    }
    h.check(Ꮡt);
}

// Different length strings of all zeros have distinct hashes.
public static void TestSmhasherZeros(ж<testing.T> Ꮡt) {
    Ꮡt.Parallel();
    nint N = 256 * 1024;
    if (testing.Short()) {
        N = 1024;
    }
    var h = newHashSet();
    var b = new slice<byte>(N);
    for (nint i = 0; i <= N; i++) {
        h.addB(b.slice(0, i));
    }
    h.check(Ꮡt);
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static readonly object tooSlowOnWasmˢ = (@string)"Too slow on wasm"u8;

// Strings with up to two nonzero bytes all have distinct hashes.
public static void TestSmhasherTwoNonzero(ж<testing.T> Ꮡt) {
    if (GOARCH == "wasm"u8) {
        Ꮡt.Skip(tooSlowOnWasmˢ);
    }
    if (testing.Short()) {
        Ꮡt.Skip(skippingInShortModeˢ);
    }
    if (race.Enabled) {
        Ꮡt.Skip(tooLongForRaceModeˢ);
    }
    testenv.ParallelOn64Bit(Ꮡt);
    var h = newHashSet();
    for (nint n = 2; n <= 16; n++) {
        twoNonZero(h, n);
    }
    h.check(Ꮡt);
}

internal static void twoNonZero(ж<HashSet> Ꮡh, nint n) {
    ref var h = ref Ꮡh.DerefOrNull();

    var b = new slice<byte>(n);
    // all zero
    h.addB(b);
    // one non-zero byte
    for (nint i = 0; i < n; i++) {
        for (nint x = 1; x < 256; x++) {
            b[i] = (byte)x;
            h.addB(b);
            b[i] = 0;
        }
    }
    // two non-zero bytes
    for (nint i = 0; i < n; i++) {
        for (nint x = 1; x < 256; x++) {
            b[i] = (byte)x;
            for (nint j = i + 1; j < n; j++) {
                for (nint y = 1; y < 256; y++) {
                    b[j] = (byte)y;
                    h.addB(b);
                    b[j] = 0;
                }
            }
            b[i] = 0;
        }
    }
}

// Test strings with repeats, like "abcdabcdabcdabcd..."
public static void TestSmhasherCyclic(ж<testing.T> Ꮡt) {
    if (testing.Short()) {
        Ꮡt.Skip(skippingInShortModeˢ);
    }
    if (race.Enabled) {
        Ꮡt.Skip(tooLongForRaceModeˢ);
    }
    Ꮡt.Parallel();
    var r = rand.New(rand.NewSource(1234));
    const nint REPEAT = 8;
    const nint N = 1000000;
    var h = newHashSet();
    for (nint n = 4; n <= 12; n++) {
        var b = new slice<byte>(REPEAT * n);
        for (nint i = 0; i < N; i++) {
            b[0] = (byte)(i * 79 % 97);
            b[1] = (byte)(i * 43 % 137);
            b[2] = (byte)(i * 151 % 197);
            b[3] = (byte)(i * 199 % 251);
            randBytes(r, b.slice(4, n));
            for (nint j = n; j < n * REPEAT; j++) {
                b[j] = b[j - n];
            }
            h.addB(b);
        }
        h.check(Ꮡt);
    }
}

// Test strings with only a few bits set
public static void TestSmhasherSparse(ж<testing.T> Ꮡt) {
    if (GOARCH == "wasm"u8) {
        Ꮡt.Skip(tooSlowOnWasmˢ);
    }
    if (testing.Short()) {
        Ꮡt.Skip(skippingInShortModeˢ);
    }
    Ꮡt.Parallel();
    var h = newHashSet();
    sparse(Ꮡt, h, 32, 6);
    sparse(Ꮡt, h, 40, 6);
    sparse(Ꮡt, h, 48, 5);
    sparse(Ꮡt, h, 56, 5);
    sparse(Ꮡt, h, 64, 5);
    sparse(Ꮡt, h, 96, 4);
    sparse(Ꮡt, h, 256, 3);
    sparse(Ꮡt, h, 2048, 2);
}

internal static void sparse(ж<testing.T> Ꮡt, ж<HashSet> Ꮡh, nint n, nint k) {
    ref var h = ref Ꮡh.DerefOrNull();

    var b = new slice<byte>(n / 8);
    setbits(Ꮡh, b, 0, k);
    h.check(Ꮡt);
}

// set up to k bits at index i and greater
internal static void setbits(ж<HashSet> Ꮡh, slice<byte> b, nint i, nint k) {
    ref var h = ref Ꮡh.DerefOrNull();

    h.addB(b);
    if (k == 0) {
        return;
    }
    for (nint j = i; j < len(b) * 8; j++) {
        b[j / 8] |= (byte)((byte)(((byte)1).Lsh((nuint)((nint)(j & 7)))));
        setbits(Ꮡh, b, j + 1, k - 1);
        b[j / 8] &= (byte)((byte)(((byte)(~(((byte)1).Lsh((nuint)((nint)(j & 7))))))));
    }
}

// Test all possible combinations of n blocks from the set s.
// "permutation" is a bad name here, but it is what Smhasher uses.
public static void TestSmhasherPermutation(ж<testing.T> Ꮡt) {
    if (GOARCH == "wasm"u8) {
        Ꮡt.Skip(tooSlowOnWasmˢ);
    }
    if (testing.Short()) {
        Ꮡt.Skip(skippingInShortModeˢ);
    }
    if (race.Enabled) {
        Ꮡt.Skip(tooLongForRaceModeˢ);
    }
    testenv.ParallelOn64Bit(Ꮡt);
    var h = newHashSet();
    permutation(Ꮡt, h, new uint32[]{0, 1, 2, 3, 4, 5, 6, 7}.slice(), 8);
    permutation(Ꮡt, h, new uint32[]{0, ((uint32)1 << (int)(29)), ((uint32)2 << (int)(29)), ((uint32)3 << (int)(29)), ((uint32)4 << (int)(29)), ((uint32)5 << (int)(29)), ((uint32)6 << (int)(29)), ((uint32)7 << (int)(29))}.slice(), 8);
    permutation(Ꮡt, h, new uint32[]{0, 1}.slice(), 20);
    permutation(Ꮡt, h, new uint32[]{0, ((uint32)1 << (int)(31))}.slice(), 20);
    permutation(Ꮡt, h, new uint32[]{0, 1, 2, 3, 4, 5, 6, 7, ((uint32)1 << (int)(29)), ((uint32)2 << (int)(29)), ((uint32)3 << (int)(29)), ((uint32)4 << (int)(29)), ((uint32)5 << (int)(29)), ((uint32)6 << (int)(29)), ((uint32)7 << (int)(29))}.slice(), 6);
}

internal static void permutation(ж<testing.T> Ꮡt, ж<HashSet> Ꮡh, slice<uint32> s, nint n) {
    ref var h = ref Ꮡh.DerefOrNull();

    var b = new slice<byte>(n * 4);
    genPerm(Ꮡh, b, s, 0);
    h.check(Ꮡt);
}

internal static void genPerm(ж<HashSet> Ꮡh, slice<byte> b, slice<uint32> s, nint n) {
    ref var h = ref Ꮡh.DerefOrNull();

    h.addB(b.slice(0, n));
    if (n == len(b)) {
        return;
    }
    foreach (var (_, v) in s) {
        b[n] = (byte)v;
        b[n + 1] = (byte)((v >> (int)(8)));
        b[n + 2] = (byte)((v >> (int)(16)));
        b[n + 3] = (byte)((v >> (int)(24)));
        genPerm(Ꮡh, b, s, n + 4);
    }
}

partial interface Key {
    void clear();          // set bits all to 0
    void random(ж<rand.Rand> r); // set key to something random
    nint bits();          // how many bits key has
    void flipBit(nint i);  // flip bit i of the key
    uintptr hash();       // hash the key
    @string name();       // for error reporting
}

partial struct BytesKey {
    internal slice<byte> b;
}

internal static void clear(this ref BytesKey k) {
    builtin.clear(k.b);
}

internal static void random(this ref BytesKey k, ж<rand.Rand> Ꮡr) {
    randBytes(Ꮡr, k.b);
}

internal static nint bits(this ref BytesKey k) {
    return len(k.b) * 8;
}

internal static void flipBit(this ref BytesKey k, nint i) {
    k.b[(i >> (int)(3))] ^= (byte)((byte)(((byte)1).Lsh((nuint)((nint)(i & 7)))));
}

internal static partial uintptr hash(this ref BytesKey k) {
    return runtime_internal_test_package.BytesHash(k.b, 0);
}

internal static @string name(this ref BytesKey k) {
    return fmt.Sprintf("bytes%d"u8, len(k.b));
}

partial struct Int32Key {
    internal uint32 i;
}

internal static void clear(this ref Int32Key k) {
    k.i = 0;
}

internal static void random(this ref Int32Key k, ж<rand.Rand> Ꮡr) {
    ref var r = ref Ꮡr.DerefOrNull();

    k.i = r.Uint32();
}

internal static nint bits(this ref Int32Key k) {
    return 32;
}

internal static void flipBit(this ref Int32Key k, nint i) {
    k.i ^= (uint32)(((uint32)1).Lsh((nuint)i));
}

internal static partial uintptr hash(this ref Int32Key k) {
    return runtime_internal_test_package.Int32Hash(k.i, 0);
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static readonly @string int32ˢ = "int32"u8;

internal static @string name(this ref Int32Key k) {
    return int32ˢ;
}

partial struct Int64Key {
    internal uint64 i;
}

internal static void clear(this ref Int64Key k) {
    k.i = 0;
}

internal static void random(this ref Int64Key k, ж<rand.Rand> Ꮡr) {
    ref var r = ref Ꮡr.DerefOrNull();

    k.i = (uint64)r.Uint32() + ((uint64)r.Uint32() << (int)(32));
}

internal static nint bits(this ref Int64Key k) {
    return 64;
}

internal static void flipBit(this ref Int64Key k, nint i) {
    k.i ^= (uint64)(((uint64)1).Lsh((nuint)i));
}

internal static partial uintptr hash(this ref Int64Key k) {
    return runtime_internal_test_package.Int64Hash(k.i, 0);
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static readonly @string int64ˢ = "int64"u8;

internal static @string name(this ref Int64Key k) {
    return int64ˢ;
}

partial struct EfaceKey {
    internal any i;
}

internal static void clear(this ref EfaceKey k) {
    k.i = default!;
}

internal static void random(this ref EfaceKey k, ж<rand.Rand> Ꮡr) {
    ref var r = ref Ꮡr.DerefOrNull();

    k.i = (uint64)r.Int63();
}

internal static nint bits(this ref EfaceKey k) {
    // use 64 bits. This tests inlined interfaces
    // on 64-bit targets and indirect interfaces on
    // 32-bit targets.
    return 64;
}

internal static void flipBit(this ref EfaceKey k, nint i) {
    k.i = (uint64)(k.i._<uint64>() ^ ((uint64)1).Lsh((nuint)i));
}

internal static partial uintptr hash(this ref EfaceKey k) {
    return runtime_internal_test_package.EfaceHash(k.i, 0);
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static readonly @string efaceˢ = "Eface"u8;

internal static @string name(this ref EfaceKey k) {
    return efaceˢ;
}


partial struct IfaceKey {
    internal ifaceHash_i i;
}

partial struct fInter /*num:uint64*/;

internal static void F(this fInter x) {
}

internal static void clear(this ref IfaceKey k) {
    k.i = default!;
}

internal static void random(this ref IfaceKey k, ж<rand.Rand> Ꮡr) {
    ref var r = ref Ꮡr.DerefOrNull();

    k.i = ((fInter)(uint64)r.Int63());
}

internal static nint bits(this ref IfaceKey k) {
    // use 64 bits. This tests inlined interfaces
    // on 64-bit targets and indirect interfaces on
    // 32-bit targets.
    return 64;
}

internal static void flipBit(this ref IfaceKey k, nint i) {
    k.i = (fInter)(k.i._<fInter>() ^ (((fInter)1) << (int)((nuint)i)));
}

internal static partial uintptr hash(this ref IfaceKey k) {
    return runtime_internal_test_package.IfaceHash(k.i, 0);
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static readonly @string ifaceˢ = "Iface"u8;

internal static @string name(this ref IfaceKey k) {
    return ifaceˢ;
}

// Flipping a single bit of a key should flip each output bit with 50% probability.
public static void TestSmhasherAvalanche(ж<testing.T> Ꮡt) {
    if (GOARCH == "wasm"u8) {
        Ꮡt.Skip(tooSlowOnWasmˢ);
    }
    if (testing.Short()) {
        Ꮡt.Skip(skippingInShortModeˢ);
    }
    if (race.Enabled) {
        Ꮡt.Skip(tooLongForRaceModeˢ);
    }
    Ꮡt.Parallel();
    avalancheTest1(Ꮡt, new runtime_test_package.BytesKeyжKey(Ꮡ(new BytesKey(new slice<byte>(2)))));
    avalancheTest1(Ꮡt, new runtime_test_package.BytesKeyжKey(Ꮡ(new BytesKey(new slice<byte>(4)))));
    avalancheTest1(Ꮡt, new runtime_test_package.BytesKeyжKey(Ꮡ(new BytesKey(new slice<byte>(8)))));
    avalancheTest1(Ꮡt, new runtime_test_package.BytesKeyжKey(Ꮡ(new BytesKey(new slice<byte>(16)))));
    avalancheTest1(Ꮡt, new runtime_test_package.BytesKeyжKey(Ꮡ(new BytesKey(new slice<byte>(32)))));
    avalancheTest1(Ꮡt, new runtime_test_package.BytesKeyжKey(Ꮡ(new BytesKey(new slice<byte>(200)))));
    avalancheTest1(Ꮡt, new runtime_test_package.Int32KeyжKey(Ꮡ(new Int32Key(nil))));
    avalancheTest1(Ꮡt, new runtime_test_package.Int64KeyжKey(Ꮡ(new Int64Key(nil))));
    avalancheTest1(Ꮡt, new runtime_test_package.EfaceKeyжKey(Ꮡ(new EfaceKey(nil))));
    avalancheTest1(Ꮡt, new runtime_test_package.IfaceKeyжKey(Ꮡ(new IfaceKey(nil))));
}

internal static void avalancheTest1(ж<testing.T> Ꮡt, Key k) {
    ref var t = ref Ꮡt.DerefOrNull();

    UntypedInt REP = 100000;
    var r = rand.New(rand.NewSource(1234));
    nint n = k.bits();
    // grid[i][j] is a count of whether flipping
    // input bit i affects output bit j.
    var grid = GoReflect.WithElemDims(new slice<array<nint>>(n, () => new(64)), 64);
    for (nint z = 0; z < REP; z++) {
        // pick a random key, hash it
        k.random(r);
        var h = k.hash();
        // flip each bit, hash & compare the results
        for (nint i = 0; i < n; i++) {
            k.flipBit(i);
            var d = (uintptr)(h ^ k.hash());
            k.flipBit(i);
            // record the effects of that bit flip
            var g = Ꮡ(grid, i);
            for (nint j = 0; j < hashSize; j++) {
                g.Value[j] += (nint)((uintptr)(d & 1));
                d >>= (int)(1);
            }
        }
    }
    // Each entry in the grid should be about REP/2.
    // More precisely, we did N = k.bits() * hashSize experiments where
    // each is the sum of REP coin flips. We want to find bounds on the
    // sum of coin flips such that a truly random experiment would have
    // all sums inside those bounds with 99% probability.
    nint N = n * hashSize;
    float64 c = default!;
    // find c such that Prob(mean-c*stddev < x < mean+c*stddev)^N > .9999
    for (c = 0.0D; Δmath.Pow(Δmath.Erf(c / Δmath.Sqrt(2D)), (float64)N) < .9999D; c += .1D) {
    }
    c *= 11.0D; // allowed slack: 40% to 60% - we don't need to be perfectly random
    var mean = /* .5 * REP */ 50000D;
    var stddev = .5D * Δmath.Sqrt(REP);
    nint low = (nint)(mean - c * stddev);
    nint high = (nint)(mean + c * stddev);
    for (nint i = 0; i < n; i++) {
        for (nint j = 0; j < hashSize; j++) {
            nint x = grid[i][j];
            if (x < low || x > high) {
                Ꮡt.Errorf("bad bias for %s bit %d -> bit %d: %d/%d\n"u8, k.name(), i, j, x, (nint)(REP));
            }
        }
    }
}

// All bit rotations of a set of distinct keys
public static void TestSmhasherWindowed(ж<testing.T> Ꮡt) {
    if (race.Enabled) {
        Ꮡt.Skip(tooLongForRaceModeˢ);
    }
    Ꮡt.Parallel();
    var h = newHashSet();
    Ꮡt.Logf("32 bit keys"u8);
    windowed(Ꮡt, h, new runtime_test_package.Int32KeyжKey(Ꮡ(new Int32Key(nil))));
    Ꮡt.Logf("64 bit keys"u8);
    windowed(Ꮡt, h, new runtime_test_package.Int64KeyжKey(Ꮡ(new Int64Key(nil))));
    Ꮡt.Logf("string keys"u8);
    windowed(Ꮡt, h, new runtime_test_package.BytesKeyжKey(Ꮡ(new BytesKey(new slice<byte>(128)))));
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static readonly object flakyOn32BitSystemsˢ = (@string)"Flaky on 32-bit systems"u8;

internal static void windowed(ж<testing.T> Ꮡt, ж<HashSet> Ꮡh, Key k) {
    ref var h = ref Ꮡh.DerefOrNull();

    if (GOARCH == "wasm"u8) {
        Ꮡt.Skip(tooSlowOnWasmˢ);
    }
    if (runtime_internal_test_package.PtrSize == 4) {
        // This test tends to be flaky on 32-bit systems.
        // There's not enough bits in the hash output, so we
        // expect a nontrivial number of collisions, and it is
        // often quite a bit higher than expected. See issue 43130.
        Ꮡt.Skip(flakyOn32BitSystemsˢ);
    }
    if (testing.Short()) {
        Ꮡt.Skip(skippingInShortModeˢ);
    }
    UntypedInt BITS = 16;
    for (nint r = 0; r < k.bits(); r++) {
        for (nint i = 0; i < (nint)((1 << (int)(BITS))); i++) {
            k.clear();
            for (nint j = 0; j < BITS; j++) {
                if ((nint)(i.Rsh((nuint)j) & 1) != 0) {
                    k.flipBit(rem((j + r), k.bits()));
                }
            }
            h.add(k.hash());
        }
        h.check(Ꮡt);
    }
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static readonly @string fooˢ = "Foo"u8;
internal static readonly @string barˢ = "Bar"u8;
internal static readonly @string fooBarˢ = "FooBar"u8;

// All keys of the form prefix + [A-Za-z0-9]*N + suffix.
public static void TestSmhasherText(ж<testing.T> Ꮡt) {
    if (testing.Short()) {
        Ꮡt.Skip(skippingInShortModeˢ);
    }
    Ꮡt.Parallel();
    var h = newHashSet();
    text(Ꮡt, h, fooˢ, barˢ);
    text(Ꮡt, h, fooBarˢ, ""u8);
    text(Ꮡt, h, ""u8, fooBarˢ);
}

// Hoisted Go string constant (single allocation; Go keeps it in RODATA)
internal static readonly @string Sᶜ = "ABCDEFGHIJKLMNOPQRSTabcdefghijklmnopqrst0123456789"u8;

internal static void text(ж<testing.T> Ꮡt, ж<HashSet> Ꮡh, @string prefix, @string suffix) {
    ref var h = ref Ꮡh.DerefOrNull();

    const nint N = 4;
    @string S = Sᶜ;
    const nint L = /* len(S) */ 50;
    var b = new slice<byte>(len(prefix) + N + len(suffix));
    copy(b, prefix);
    copy(b.slice(len(prefix) + N), suffix);
    var c = b.slice(len(prefix));
    for (nint i = 0; i < L; i++) {
        c[0] = S[i];
        for (nint j = 0; j < L; j++) {
            c[1] = S[j];
            for (nint k = 0; k < L; k++) {
                c[2] = S[k];
                for (nint x = 0; x < L; x++) {
                    c[3] = S[x];
                    h.addB(b);
                }
            }
        }
    }
    h.check(Ꮡt);
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static readonly @string helloˢ = "hello"u8;

// Make sure different seed values generate different hashes.
public static void TestSmhasherSeed(ж<testing.T> Ꮡt) {
    var h = newHashSet();
    const nint N = 100000;
    @string s = helloˢ;
    for (nint i = 0; i < N; i++) {
        h.addS_seed(s, (uintptr)i);
    }
    h.check(Ꮡt);
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static readonly @string testIssue66841ˢ = "TEST_ISSUE_66841"u8;
internal static readonly @string testRunTestIssue66841ˢ = "-test.run=^TestIssue66841$"u8;

public static void TestIssue66841(ж<testing.T> Ꮡt) {
    testenv.MustHaveExec(new runtime_test_package.testing_TжTB(Ꮡt));
    if (runtime_internal_test_package.UseAeshash.Value && Δos.Getenv(testIssue66841ˢ) == ""u8) {
        // We want to test the backup hash, so if we're running on a machine
        // that uses aeshash, exec ourselves while turning aes off.
        var cmd = testenv.CleanCmdEnv(testenv.Command(new runtime_test_package.testing_TжTB(Ꮡt), Δos.Args[0], testRunTestIssue66841ˢ));
        cmd.Value.Env = append((~cmd).Env, "GODEBUG=cpu.aes=off"u8, "TEST_ISSUE_66841=1");
        var (@out, err) = cmd.CombinedOutput();
        if (err != default!) {
            Ꮡt.Errorf("%s"u8, ((@string)@out));
        }
    }
    // Fall through. Might as well run this test when aeshash is on also.
    var h = newHashSet();
    array<byte> b = new(16);
    binary.LittleEndian.PutUint64(b[..8], 0xe7037ed1a0b428dbUL); // runtime.m2
    for (nint i = 0; i < 1000; i++) {
        binary.LittleEndian.PutUint64(b[8..], (uint64)i);
        h.addB(b[..]);
    }
    h.check(Ꮡt);
}

// size of the hash output (32 or 64 bits)
internal const nint hashSize = /* 32 + int(^uintptr(0)>>63<<5) */ 64;

internal static void randBytes(ж<rand.Rand> Ꮡr, slice<byte> b) {
    ref var r = ref Ꮡr.DerefOrNull();

    foreach (var (i, _) in b) {
        b[i] = (byte)r.Uint32();
    }
}

internal static void benchmarkHash(ж<testing.B> Ꮡb, nint n) {
    ref var b = ref Ꮡb.DerefOrNull();

    @string s = strings.Repeat("A"u8, n);
    for (nint i = 0; i < b.N; i++) {
        runtime_internal_test_package.StringHash(s, 0);
    }
    b.SetBytes((int64)n);
}

public static void BenchmarkHash5(ж<testing.B> Ꮡb) {
    benchmarkHash(Ꮡb, 5);
}

public static void BenchmarkHash16(ж<testing.B> Ꮡb) {
    benchmarkHash(Ꮡb, 16);
}

public static void BenchmarkHash64(ж<testing.B> Ꮡb) {
    benchmarkHash(Ꮡb, 64);
}

public static void BenchmarkHash1024(ж<testing.B> Ꮡb) {
    benchmarkHash(Ꮡb, 1024);
}

public static void BenchmarkHash65536(ж<testing.B> Ꮡb) {
    benchmarkHash(Ꮡb, 65536);
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static readonly @string fooˢ2 = "foo"u8;

internal partial struct TestArrayHash_key /*[8]@string*/;

public static void TestArrayHash(ж<testing.T> Ꮡt) {
    ref var t = ref Ꮡt.DerefOrNull();

    // Make sure that "" in arrays hash correctly. The hash
    // should at least scramble the input seed so that, e.g.,
    // {"","foo"} and {"foo",""} have different hashes.
    // If the hash is bad, then all (8 choose 4) = 70 keys
    // have the same hash. If so, we allocate 70/8 = 8
    // overflow buckets. If the hash is good we don't
    // normally allocate any overflow buckets, and the
    // probability of even one or two overflows goes down rapidly.
    // (There is always 1 allocation of the bucket array. The map
    // header is allocated on the stack.)
    var f = () => {
        var m = new map<TestArrayHash_key, bool>(70);
        // fill m with keys that have 4 "foo"s and 4 ""s.
        for (nint i = 0; i < 256; i++) {
            TestArrayHash_key k = default!;
            nint cnt = 0;
            for (nuint j = (nuint)0; j < 8; j++) {
                if ((nint)(i.Rsh(j) & 1) != 0) {
                    k[j] = fooˢ2;
                    cnt++;
                }
            }
            if (cnt == 4) {
                m[k] = true;
            }
        }
        if (len(m) != 70) {
            Ꮡt.Errorf("bad test: (8 choose 4) should be 70, not %d"u8, len(m));
        }
    };
    {
        var n = testing.AllocsPerRun(10, f); if (n > 6D) {
            Ꮡt.Errorf("too many allocs %f - hash not balanced"u8, n);
        }
    }
}

internal partial struct TestStructHash_key /*dyn*/ {
    internal @string a, b, c, d, e, f, g, h;
}

public static void TestStructHash(ж<testing.T> Ꮡt) {
    // See the comment in TestArrayHash.
    var f = () => {
        var m = new map<TestStructHash_key, bool>(70);
        // fill m with keys that have 4 "foo"s and 4 ""s.
        for (nint i = 0; i < 256; i++) {
            TestStructHash_key k = default!;
            nint cnt = 0;
            if ((nint)(i & 1) != 0) {
                k.a = fooˢ2;
                cnt++;
            }
            if ((nint)(i & 2) != 0) {
                k.b = fooˢ2;
                cnt++;
            }
            if ((nint)(i & 4) != 0) {
                k.c = fooˢ2;
                cnt++;
            }
            if ((nint)(i & 8) != 0) {
                k.d = fooˢ2;
                cnt++;
            }
            if ((nint)(i & 16) != 0) {
                k.e = fooˢ2;
                cnt++;
            }
            if ((nint)(i & 32) != 0) {
                k.f = fooˢ2;
                cnt++;
            }
            if ((nint)(i & 64) != 0) {
                k.g = fooˢ2;
                cnt++;
            }
            if ((nint)(i & 128) != 0) {
                k.h = fooˢ2;
                cnt++;
            }
            if (cnt == 4) {
                m[k] = true;
            }
        }
        if (len(m) != 70) {
            Ꮡt.Errorf("bad test: (8 choose 4) should be 70, not %d"u8, len(m));
        }
    };
    {
        var n = testing.AllocsPerRun(10, f); if (n > 6D) {
            Ꮡt.Errorf("too many allocs %f - hash not balanced"u8, n);
        }
    }
}

internal static uint64 sink;

public static void BenchmarkAlignedLoad(ж<testing.B> Ꮡb) {
    ref var b = ref Ꮡb.DerefOrNull();

    ref var buf = ref heap(new array<byte>(16), out var Ꮡbuf);
    @unsafe.Pointer p = @unsafe.Pointer.FromPinnedBox(Ꮡbuf.at<byte>(0));
    uint64 s = default!;
    for (nint i = 0; i < b.N; i++) {
        s += runtime_internal_test_package.ReadUnaligned64(p);
    }
    sink = s;
}

public static void BenchmarkUnalignedLoad(ж<testing.B> Ꮡb) {
    ref var b = ref Ꮡb.DerefOrNull();

    ref var buf = ref heap(new array<byte>(16), out var Ꮡbuf);
    @unsafe.Pointer p = @unsafe.Pointer.FromPinnedBox(Ꮡbuf.at<byte>(1));
    uint64 s = default!;
    for (nint i = 0; i < b.N; i++) {
        s += runtime_internal_test_package.ReadUnaligned64(p);
    }
    sink = s;
}

public static void TestCollisions(ж<testing.T> Ꮡt) {
    if (testing.Short()) {
        Ꮡt.Skip(skippingInShortModeˢ);
    }
    Ꮡt.Parallel();
    for (nint i = 0; i < 16; i++) {
        for (nint j = 0; j < 16; j++) {
            if (j == i) {
                continue;
            }
            array<byte> a = new(16);
            var m = new map<uint16, EmptyStruct>((1 << (int)(16)));
            for (nint n = 0; n < (1 << (int)(16)); n++) {
                a[i] = (byte)n;
                a[j] = (byte)((n >> (int)(8)));
                m[(uint16)runtime_internal_test_package.BytesHash(a[..], 0)] = new EmptyStruct();
            }
            // N balls in N bins, for N=65536
            nint avg = 41427;
            nint stdDev = 123;
            if (len(m) < avg - 40 * stdDev || len(m) > avg + 40 * stdDev) {
                Ꮡt.Errorf("bad number of collisions i=%d j=%d outputs=%d out of 65536\n"u8, i, j, len(m));
            }
        }
    }
}

} // end runtime_test_package
