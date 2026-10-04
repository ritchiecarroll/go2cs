// Copyright 2019 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.
//go:build !race
namespace go.hash;

using fmt = fmt_package;
using testenv = @internal.testenv_package;
using math = math_package;
using rand = go.math.rand_package;
using runtime = runtime_package;
using slices = slices_package;
using strings = strings_package;
using testing = testing_package;
using @unsafe = unsafe_package;
using @internal;
using go.math;
using static go.hash.maphash_package;

partial class maphash_internal_test_package {

// Smhasher is a torture test for hash functions.
// https://code.google.com/p/smhasher/
// This code is a port of some of the Smhasher tests to Go.
// Note: due to the long running time of these tests, they are
// currently disabled in -race mode.
internal static global::go.hash.maphash_package.ΔSeed fixedSeed = MakeSeed();

// Sanity checks.
// hash should not depend on values outside key.
// hash should not depend on alignment.
public static void TestSmhasherSanity(ж<testing.T> Ꮡt) {
    Ꮡt.Parallel();
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
                if (bytesHash(b.slice(PAD, (nint)PAD + n)) != bytesHash(c.slice((nint)PAD + i, (nint)PAD + i + n))) {
                    Ꮡt.Errorf("hash depends on bytes outside key"u8);
                }
            }
        }
    }
}

internal static uint64 bytesHash(slice<byte> b) {
    global::go.hash.maphash_package.Hash h = new();
    h.SetSeed(fixedSeed);
    h.Write(b);
    return h.Sum64();
}

internal static uint64 stringHash(@string s) {
    global::go.hash.maphash_package.Hash h = new();
    h.SetSeed(fixedSeed);
    h.WriteString(s);
    return h.Sum64();
}

internal static UntypedInt hashSize => 64;

internal static void randBytes(ж<rand.Rand> Ꮡr, slice<byte> b) {
    Ꮡr.Read(b); // can't fail
}

// A hashSet measures the frequency of hash collisions.
[GoType] internal partial struct hashSet {
    internal slice<uint64> list; // list of hashes added
}

internal static ж<hashSet> newHashSet() {
    return Ꮡ(new hashSet(list: new slice<uint64>(0, 1024)));
}

[GoRecv] internal static void add(this ref hashSet s, uint64 h) {
    s.list = append(s.list, h);
}

[GoRecv] internal static void addS(this ref hashSet s, @string x) {
    s.add(stringHash(x));
}

[GoRecv] internal static void addB(this ref hashSet s, slice<byte> x) {
    s.add(bytesHash(x));
}

[GoRecv] internal static void addS_seed(this ref hashSet s, @string x, global::go.hash.maphash_package.ΔSeed seed) {
    global::go.hash.maphash_package.Hash h = new();
    h.SetSeed(seed);
    h.WriteString(x);
    s.add(h.Sum64());
}

[GoRecv] internal static void check(this ref hashSet s, ж<testing.T> Ꮡt) {
    Ꮡt.Helper();
    var list = s.list;
    slices.Sort<slice<uint64>, uint64>(list);
    nint collisions = 0;
    for (nint i = 1; i < len(list); i++) {
        if (list[i] == list[i - 1]) {
            collisions++;
        }
    }
    nint n = len(list);
    const float64 SLOP = 10.0;
    var pairs = (int64)n * (int64)(n - 1) / 2;
    var expected = (float64)pairs / math.Pow(2.0D, (float64)hashSize);
    var stddev = math.Sqrt(expected);
    if ((float64)collisions > expected + SLOP * (3D * stddev + 1D)) {
        Ꮡt.Errorf("unexpected number of collisions: got=%d mean=%f stddev=%f"u8, collisions, expected, stddev);
    }
    // Reset for reuse
    s.list = s.list[..0];
}

// a string plus adding zeros must make distinct hashes
public static void TestSmhasherAppendedZeros(ж<testing.T> Ꮡt) {
    Ꮡt.Parallel();
    @string s = "hello"u8 + strings.Repeat("\x00"u8, 256);
    var h = newHashSet();
    for (nint i = 0; i <= len(s); i++) {
        h.addS(s.slice(0, i));
    }
    h.check(Ꮡt);
}

// All 0-3 byte strings have distinct hashes.
public static void TestSmhasherSmallKeys(ж<testing.T> Ꮡt) {
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
internal static readonly object skippingInShortModeˢ = (@string)"Skipping in short mode"u8;

// Strings with up to two nonzero bytes all have distinct hashes.
public static void TestSmhasherTwoNonzero(ж<testing.T> Ꮡt) {
    if (runtime.GOARCH == "wasm"u8) {
        Ꮡt.Skip(tooSlowOnWasmˢ);
    }
    if (testing.Short()) {
        Ꮡt.Skip(skippingInShortModeˢ);
    }
    testenv.ParallelOn64Bit(Ꮡt);
    var h = newHashSet();
    for (nint n = 2; n <= 16; n++) {
        twoNonZero(h, n);
    }
    h.check(Ꮡt);
}

internal static void twoNonZero(ж<hashSet> Ꮡh, nint n) {
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
    if (runtime.GOARCH == "wasm"u8) {
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

internal static void sparse(ж<testing.T> Ꮡt, ж<hashSet> Ꮡh, nint n, nint k) {
    ref var h = ref Ꮡh.DerefOrNull();

    Ꮡt.Helper();
    var b = new slice<byte>(n / 8);
    setbits(Ꮡh, b, 0, k);
    h.check(Ꮡt);
}

// set up to k bits at index i and greater
internal static void setbits(ж<hashSet> Ꮡh, slice<byte> b, nint i, nint k) {
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
    if (runtime.GOARCH == "wasm"u8) {
        Ꮡt.Skip(tooSlowOnWasmˢ);
    }
    if (testing.Short()) {
        Ꮡt.Skip(skippingInShortModeˢ);
    }
    testenv.ParallelOn64Bit(Ꮡt);
    var h = newHashSet();
    permutation(Ꮡt, h, new uint32[]{0, 1, 2, 3, 4, 5, 6, 7}.slice(), 8);
    permutation(Ꮡt, h, new uint32[]{0, ((uint32)1 << (int)(29)), ((uint32)2 << (int)(29)), ((uint32)3 << (int)(29)), ((uint32)4 << (int)(29)), ((uint32)5 << (int)(29)), ((uint32)6 << (int)(29)), ((uint32)7 << (int)(29))}.slice(), 8);
    permutation(Ꮡt, h, new uint32[]{0, 1}.slice(), 20);
    permutation(Ꮡt, h, new uint32[]{0, ((uint32)1 << (int)(31))}.slice(), 20);
    permutation(Ꮡt, h, new uint32[]{0, 1, 2, 3, 4, 5, 6, 7, ((uint32)1 << (int)(29)), ((uint32)2 << (int)(29)), ((uint32)3 << (int)(29)), ((uint32)4 << (int)(29)), ((uint32)5 << (int)(29)), ((uint32)6 << (int)(29)), ((uint32)7 << (int)(29))}.slice(), 6);
}

internal static void permutation(ж<testing.T> Ꮡt, ж<hashSet> Ꮡh, slice<uint32> s, nint n) {
    ref var h = ref Ꮡh.DerefOrNull();

    Ꮡt.Helper();
    var b = new slice<byte>(n * 4);
    genPerm(Ꮡh, b, s, 0);
    h.check(Ꮡt);
}

internal static void genPerm(ж<hashSet> Ꮡh, slice<byte> b, slice<uint32> s, nint n) {
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

[GoType] internal partial interface key {
    void clear();          // set bits all to 0
    void random(ж<rand.Rand> r); // set key to something random
    nint bits();          // how many bits key has
    void flipBit(nint i);  // flip bit i of the key
    uint64 hash();        // hash the key
    @string name();       // for error reporting
}

[GoType] internal partial struct bytesKey {
    internal slice<byte> b;
}

[GoRecv] internal static void clear(this ref bytesKey k) {
    builtin.clear(k.b);
}

[GoRecv] internal static void random(this ref bytesKey k, ж<rand.Rand> Ꮡr) {
    randBytes(Ꮡr, k.b);
}

[GoRecv] internal static nint bits(this ref bytesKey k) {
    return len(k.b) * 8;
}

[GoRecv] internal static void flipBit(this ref bytesKey k, nint i) {
    k.b[(i >> (int)(3))] ^= (byte)((byte)(((byte)1).Lsh((nuint)((nint)(i & 7)))));
}

[GoRecv] internal static uint64 hash(this ref bytesKey k) {
    return bytesHash(k.b);
}

[GoRecv] internal static @string name(this ref bytesKey k) {
    return fmt.Sprintf("bytes%d"u8, len(k.b));
}

// Flipping a single bit of a key should flip each output bit with 50% probability.
public static void TestSmhasherAvalanche(ж<testing.T> Ꮡt) {
    if (runtime.GOARCH == "wasm"u8) {
        Ꮡt.Skip(tooSlowOnWasmˢ);
    }
    if (testing.Short()) {
        Ꮡt.Skip(skippingInShortModeˢ);
    }
    Ꮡt.Parallel();
    avalancheTest1(Ꮡt, new maphash_internal_test_package.bytesKeyжkey(Ꮡ(new bytesKey(new slice<byte>(2)))));
    avalancheTest1(Ꮡt, new maphash_internal_test_package.bytesKeyжkey(Ꮡ(new bytesKey(new slice<byte>(4)))));
    avalancheTest1(Ꮡt, new maphash_internal_test_package.bytesKeyжkey(Ꮡ(new bytesKey(new slice<byte>(8)))));
    avalancheTest1(Ꮡt, new maphash_internal_test_package.bytesKeyжkey(Ꮡ(new bytesKey(new slice<byte>(16)))));
    avalancheTest1(Ꮡt, new maphash_internal_test_package.bytesKeyжkey(Ꮡ(new bytesKey(new slice<byte>(32)))));
    avalancheTest1(Ꮡt, new maphash_internal_test_package.bytesKeyжkey(Ꮡ(new bytesKey(new slice<byte>(200)))));
}

internal static void avalancheTest1(ж<testing.T> Ꮡt, key k) {
    ref var t = ref Ꮡt.DerefOrNull();

    Ꮡt.Helper();
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
            var d = (uint64)(h ^ k.hash());
            k.flipBit(i);
            // record the effects of that bit flip
            var g = Ꮡ(grid, i);
            for (nint j = 0; j < hashSize; j++) {
                g.Value[j] += (nint)((uint64)(d & 1));
                d >>= (int)(1);
            }
        }
    }
    // Each entry in the grid should be about REP/2.
    // More precisely, we did N = k.bits() * hashSize experiments where
    // each is the sum of REP coin flips. We want to find bounds on the
    // sum of coin flips such that a truly random experiment would have
    // all sums inside those bounds with 99% probability.
    nint N = n * (nint)hashSize;
    float64 c = default!;
    // find c such that Prob(mean-c*stddev < x < mean+c*stddev)^N > .9999
    for (c = 0.0D; math.Pow(math.Erf(c / math.Sqrt(2D)), (float64)N) < .9999D; c += .1D) {
    }
    c *= 11.0D; // allowed slack: 40% to 60% - we don't need to be perfectly random
    var mean = /* .5 * REP */ 50000D;
    var stddev = .5D * math.Sqrt(REP);
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
    Ꮡt.Parallel();
    windowed(Ꮡt, new maphash_internal_test_package.bytesKeyжkey(Ꮡ(new bytesKey(new slice<byte>(128)))));
}

internal static void windowed(ж<testing.T> Ꮡt, key k) {
    if (runtime.GOARCH == "wasm"u8) {
        Ꮡt.Skip(tooSlowOnWasmˢ);
    }
    if (testing.Short()) {
        Ꮡt.Skip(skippingInShortModeˢ);
    }
    UntypedInt BITS = 16;
    var h = newHashSet();
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
internal static readonly @string fooˢ2 = "Foo"u8;
internal static readonly @string barˢ = "Bar"u8;
internal static readonly @string fooBarˢ = "FooBar"u8;

// All keys of the form prefix + [A-Za-z0-9]*N + suffix.
public static void TestSmhasherText(ж<testing.T> Ꮡt) {
    if (testing.Short()) {
        Ꮡt.Skip(skippingInShortModeˢ);
    }
    Ꮡt.Parallel();
    var h = newHashSet();
    text(Ꮡt, h, fooˢ2, barˢ);
    text(Ꮡt, h, fooBarˢ, ""u8);
    text(Ꮡt, h, ""u8, fooBarˢ);
}

// Hoisted Go string constant (single allocation; Go keeps it in RODATA)
internal static readonly @string Sᶜ = "ABCDEFGHIJKLMNOPQRSTabcdefghijklmnopqrst0123456789"u8;

internal static void text(ж<testing.T> Ꮡt, ж<hashSet> Ꮡh, @string prefix, @string suffix) {
    ref var h = ref Ꮡh.DerefOrNull();

    Ꮡt.Helper();
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
internal static readonly object bitPlatformsDonTHaveˢ = (@string)"32-bit platforms don't have ideal seed-input distributions (see issue 33988)"u8;
internal static readonly @string helloˢ = "hello"u8;

// Make sure different seed values generate different hashes.
public static void TestSmhasherSeed(ж<testing.T> Ꮡt) {
    if (/* unsafe.Sizeof(uintptr(0)) */ (uintptr)8 == 4) {
        Ꮡt.Skip(bitPlatformsDonTHaveˢ);
    }
    Ꮡt.Parallel();
    var h = newHashSet();
    const nint N = 100000;
    @string s = helloˢ;
    for (nint i = 0; i < N; i++) {
        h.addS_seed(s, new ΔSeed(s: (uint64)(i + 1)));
        h.addS_seed(s, new ΔSeed(s: ((uint64)(i + 1) << (int)(32)))); // make sure high bits are used
    }
    h.check(Ꮡt);
}

} // end maphash_internal_test_package
