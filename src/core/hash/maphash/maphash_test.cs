// Copyright 2019 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.
namespace go.hash;

using bytes = bytes_package;
using fmt = fmt_package;
using hash = hash_package;
using asan = @internal.asan_package;
using testhash = @internal.testhash_package;
using math = math_package;
using reflect = reflect_package;
using strings = strings_package;
using testing = testing_package;
using @unsafe = unsafe_package;
using @internal;
using static go.hash.maphash_package;

partial class maphash_internal_test_package {

public static void TestUnseededHash(ж<testing.T> t) {
    var m = new map<uint64, EmptyStruct>{};
    for (nint i = 0; i < 1000; i++) {
        var h = @new<global::go.hash.maphash_package.Hash>();
        m[h.Sum64()] = new EmptyStruct();
    }
    if (len(m) < 900) {
        t.Errorf("empty hash not sufficiently random: got %d, want 1000", len(m));
    }
}

public static void TestSeededHash(ж<testing.T> t) {
    var s = MakeSeed();
    var m = new map<uint64, EmptyStruct>{};
    for (nint i = 0; i < 1000; i++) {
        var h = @new<global::go.hash.maphash_package.Hash>();
        h.SetSeed(s);
        m[h.Sum64()] = new EmptyStruct();
    }
    if (len(m) != 1) {
        t.Errorf("seeded hash is random: got %d, want 1", len(m));
    }
}

public static void TestHashGrouping(ж<testing.T> t) {
    var b = bytes.Repeat(slice<byte>("foo"u8), 100);
    var hh = new slice<ж<global::go.hash.maphash_package.Hash>>(7);
    foreach (var i in range(len(hh))) {
        hh[i] = @new<global::go.hash.maphash_package.Hash>();
    }
    foreach (var (_, h) in range(hh[1..])) {
        h.SetSeed(hh[0].Seed());
    }
    hh[0].Write(b);
    hh[1].WriteString((@string)b);

    void writeByte(ж<global::go.hash.maphash_package.Hash> h, byte bΔ1) {
        var err = h.WriteByte(bΔ1);
        if (err != nil) {
            t.Fatalf("WriteByte: %v", err);
        }
    }
    void writeSingleByte(ж<global::go.hash.maphash_package.Hash> h, byte bΔ2) {
        var (_, err) = h.Write([bΔ2]);
        if (err != nil) {
            t.Fatalf("Write single byte: %v", err);
        }
    }
    void writeStringSingleByte(ж<global::go.hash.maphash_package.Hash> h, byte bΔ3) {
        var (_, err) = h.WriteString((@string)slice<byte>([bΔ3]));
        if (err != nil) {
            t.Fatalf("WriteString single byte: %v", err);
        }
    }

    foreach (var (i, x) in range(b)) {
        writeByte(hh[2], x);
        writeSingleByte(hh[3], x);
        if (i == 0) {
            writeByte(hh[4], x);
        } else {
            writeSingleByte(hh[4], x);
        }
        writeStringSingleByte(hh[5], x);
        if (i == 0) {
            writeByte(hh[6], x);
        } else {
            writeStringSingleByte(hh[6], x);
        }
    }

    var sum = hh[0].Sum64();
    foreach (var (i, h) in range(hh)) {
        if (sum != h.Sum64()) {
            t.Errorf("hash %d not identical to a single Write", i);
        }
    }

    {
        var sum1 = Bytes(hh[0].Seed(), b); if (sum1 != hh[0].Sum64()) {
            t.Errorf("hash using Bytes not identical to a single Write");
        }
    }

    {
        var sum1 = String(hh[0].Seed(), (@string)b); if (sum1 != hh[0].Sum64()) {
            t.Errorf("hash using String not identical to a single Write");
        }
    }
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static ref readonly @string fooˢ => ref fooˢᵇ;
private static readonly @string fooˢᵇ = "foo"u8;

public static void TestHashBytesVsString(ж<testing.T> t) {
    @string s = fooˢ;
    var b = slice<byte>(s);
    var h1 = @new<global::go.hash.maphash_package.Hash>();
    var h2 = @new<global::go.hash.maphash_package.Hash>();
    h2.SetSeed(h1.Seed());
    var (n1, err1) = h1.WriteString(s);
    if (n1 != len(s) || err1 != nil) {
        t.Fatalf("WriteString(s) = %d, %v, want %d, nil", n1, err1, len(s));
    }
    var (n2, err2) = h2.Write(b);
    if (n2 != len(b) || err2 != nil) {
        t.Fatalf("Write(b) = %d, %v, want %d, nil", n2, err2, len(b));
    }
    if (h1.Sum64() != h2.Sum64()) {
        t.Errorf("hash of string and bytes not identical");
    }
}

public static void TestHashHighBytes(ж<testing.T> t) {
    // See issue 34925.
    UntypedInt N = 10;
    var m = new map<uint64, EmptyStruct>{};
    for (nint i = 0; i < N; i++) {
        var h = @new<global::go.hash.maphash_package.Hash>();
        h.WriteString(fooˢ);
        m[(h.Sum64() >> 32)] = new EmptyStruct();
    }
    if (len(m) < (nint)(N / 2)) {
        t.Errorf("from %d seeds, wanted at least %d different hashes; got %d", (nint)(N), (nint)(N / 2), len(m));
    }
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static ref readonly @string testingˢ => ref testingˢᵇ;
private static readonly @string testingˢᵇ = "testing"u8;

public static void TestRepeat(ж<testing.T> t) {
    var h1 = @new<global::go.hash.maphash_package.Hash>();
    h1.WriteString(testingˢ);
    var sum1 = h1.Sum64();

    h1.Reset();
    h1.WriteString(testingˢ);
    var sum2 = h1.Sum64();

    if (sum1 != sum2) {
        t.Errorf("different sum after resetting: %#x != %#x", sum1, sum2);
    }

    var h2 = @new<global::go.hash.maphash_package.Hash>();
    h2.SetSeed(h1.Seed());
    h2.WriteString(testingˢ);
    var sum3 = h2.Sum64();

    if (sum1 != sum3) {
        t.Errorf("different sum on the same seed: %#x != %#x", sum1, sum3);
    }
}

public static void TestSeedFromSum64(ж<testing.T> t) {
    var h1 = @new<global::go.hash.maphash_package.Hash>();
    h1.WriteString(fooˢ);
    var x = h1.Sum64(); // seed generated here
    var h2 = @new<global::go.hash.maphash_package.Hash>();
    h2.SetSeed(h1.Seed());
    h2.WriteString(fooˢ);
    var y = h2.Sum64();
    if (x != y) {
        t.Errorf("hashes don't match: want %x, got %x", x, y);
    }
}

public static void TestSeedFromSeed(ж<testing.T> t) {
    var h1 = @new<global::go.hash.maphash_package.Hash>();
    h1.WriteString(fooˢ);
    _ = h1.Seed(); // seed generated here
    var x = h1.Sum64();
    var h2 = @new<global::go.hash.maphash_package.Hash>();
    h2.SetSeed(h1.Seed());
    h2.WriteString(fooˢ);
    var y = h2.Sum64();
    if (x != y) {
        t.Errorf("hashes don't match: want %x, got %x", x, y);
    }
}

public static void TestSeedFromFlush(ж<testing.T> t) {
    var b = new slice<byte>(65);
    var h1 = @new<global::go.hash.maphash_package.Hash>();
    h1.Write(b); // seed generated here
    var x = h1.Sum64();
    var h2 = @new<global::go.hash.maphash_package.Hash>();
    h2.SetSeed(h1.Seed());
    h2.Write(b);
    var y = h2.Sum64();
    if (x != y) {
        t.Errorf("hashes don't match: want %x, got %x", x, y);
    }
}

public static void TestSeedFromReset(ж<testing.T> t) {
    var h1 = @new<global::go.hash.maphash_package.Hash>();
    h1.WriteString(fooˢ);
    h1.Reset(); // seed generated here
    h1.WriteString(fooˢ);
    var x = h1.Sum64();
    var h2 = @new<global::go.hash.maphash_package.Hash>();
    h2.SetSeed(h1.Seed());
    h2.WriteString(fooˢ);
    var y = h2.Sum64();
    if (x != y) {
        t.Errorf("hashes don't match: want %x, got %x", x, y);
    }
}

internal static T negativeZero<T>()
    where T : /* float32 | float64 */ IAdditionOperators<T, T, T>, ISubtractionOperators<T, T, T>, IMultiplyOperators<T, T, T>, IDivisionOperators<T, T, T>, IIncrementOperators<T>, IDecrementOperators<T>, IUnaryNegationOperators<T, T>, IEqualityOperators<T, T, bool>, IComparisonOperators<T, T, bool>, new()
{
    T f = GoZero<T>();
    f = -f;
    return f;
}

internal partial struct TestComparable_v /*dyn*/ {
    internal nint i;
    internal nuint u;
    internal bool b;
    internal float64 f;
    internal ж<nint> p;
    internal any a;
}

internal partial struct TestComparable_S /*dyn*/ {
    internal @string s;
}

internal partial struct TestComparable_v1 /*dyn*/ {
    internal @string a, b;
}

internal partial struct TestComparable_v1ᴛ1 /*dyn*/ {
    internal any a, b;
}

public static void TestComparable(ж<testing.T> t) {
    testComparable<int64, int64>(t, (int64)2);
    testComparable<uint64, uint64>(t, (uint64)8);
    testComparable<uintptr, uintptr>(t, (uintptr)12);
    testComparable<any, any>(t, (any)(@string)("s"u8));
    testComparable<@string, @string>(t, (@string)"s");
    testComparable<bool, bool>(t, true);
    testComparable<ж<float64>, ж<float64>>(t, @new<float64>());
    testComparable<float64, float64>(t, (float64)9D);
    testComparable<complex128, complex128>(t, (complex128)(1D + 9D.i()));
    testComparable<EmptyStruct, EmptyStruct>(t, new EmptyStruct());
    testComparable<TestComparable_v, TestComparable_v>(t, new TestComparable_v(i: 9, u: 1, b: true, f: 9.9D, p: @new<nint>(), a: (nint)1));
    var s1 = new TestComparable_S(s: heapStr(t));
    var s2 = new TestComparable_S(s: heapStr(t));
    if (@unsafe.StringData(s1.s) == @unsafe.StringData(s2.s)) {
        t.Fatalf("unexpected two heapStr ptr equal");
    }
    if (s1.s != s2.s) {
        t.Fatalf("unexpected two heapStr value not equal");
    }
    testComparable<TestComparable_S, TestComparable_S>(t, s1, s2);
    testComparable<@string, @string>(t, s1.s, s2.s);
    var c1 = new channel<EmptyStruct>(0);
    var c2 = new channel<EmptyStruct>(0);
    testComparable<channel<EmptyStruct>, channel<EmptyStruct>>(t, c1, c1);
    testComparable<channel<EmptyStruct>, channel<EmptyStruct>>(t, (channel<EmptyStruct>)default);
    testComparable<float32, float32>(t, (float32)0F, negativeZero<float32>());
    testComparable<float64, float64>(t, (float64)0D, negativeZero<float64>());
    testComparableNoEqual(t, math.NaN(), math.NaN());
    testComparableNoEqual(t, new @string[]{"a", ""}.array(), new @string[]{"", "a"}.array());
    testComparableNoEqual(t, new TestComparable_v1("foo", ""), new TestComparable_v1("", "foo"));
    testComparableNoEqual(t, new TestComparable_v1ᴛ1((nint)0, new EmptyStruct()), new TestComparable_v1ᴛ1(new EmptyStruct(), (nint)0));
    testComparableNoEqual(t, c1, c2);
}

internal static partial void testComparableNoEqual<T>(ж<testing.T> t, T v1, T v2) {
    var seed = MakeSeed();
    if (Comparable(seed, v1) == Comparable(seed, v2)) {
        t.Fatalf("Comparable(seed, %v) == Comparable(seed, %v)", (v1).OrTypedNilOf(), (v2).OrTypedNilOf());
    }
}

internal static slice<byte> heapStrValue = slice<byte>("aTestString"u8);

internal static @string heapStr(ж<testing.T> t) {
    return (@string)heapStrValue;
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static readonly object comparableSeedPtrˢ = (@string)"Comparable(seed, ptr) != Comparable(seed, ptr)"u8;

internal static void testComparable<T, Tᴺ>(ж<testing.T> t, T v, params Span<T> v2ʗp) {
    var v2 = v2ʗp.slice();

    var v2ʗ1 = v2;
    t.Run(reflect.TypeFor<Tᴺ>().String(), (ж<testing.T> tΔ1) => {
        ref var a = ref heap<T>(out var Ꮡa);

        a = v;
        T b = v;
        if (len(v2ʗ1) != 0) {
            b = v2ʗ1[0];
        }
        ж<T> pa = Ꮡa;
        var seed = MakeSeed();
        if (Comparable(seed, a) != Comparable(seed, b)) {
            tΔ1.Fatalf("Comparable(seed, %v) != Comparable(seed, %v)", (a).OrTypedNilOf(), (b).OrTypedNilOf());
        }
        var old = Comparable(seed, pa);
        stackGrow(8192);
        var @new = Comparable(seed, pa);
        if (old != @new) {
            tΔ1.Fatal(comparableSeedPtrˢ);
        }
    });
}

internal static byte use;

//go:noinline
internal static partial void stackGrow(nint dep) {
    if (dep == 0) {
        return;
    }
    array<byte> local = new(1024);
    // make sure local is allocated on the stack.
    local[randUint64() % 1024] = (byte)randUint64();
    use = local[randUint64() % 1024];
    stackGrow(dep - 1);
}

internal partial struct TestWriteComparable_S /*dyn*/ {
    internal @string s;
}

public static void TestWriteComparable(ж<testing.T> t) {
    testWriteComparable<int64, int64>(t, (int64)2);
    testWriteComparable<uint64, uint64>(t, (uint64)8);
    testWriteComparable<uintptr, uintptr>(t, (uintptr)12);
    testWriteComparable<any, any>(t, (any)(@string)("s"u8));
    testWriteComparable<@string, @string>(t, (@string)"s");
    testComparable<bool, bool>(t, true);
    testWriteComparable<ж<float64>, ж<float64>>(t, @new<float64>());
    testWriteComparable<float64, float64>(t, (float64)9D);
    testWriteComparable<complex128, complex128>(t, (complex128)(1D + 9D.i()));
    testWriteComparable<EmptyStruct, EmptyStruct>(t, new EmptyStruct());
    testWriteComparable<TestComparable_v, TestComparable_v>(t, new TestComparable_v(i: 9, u: 1, b: true, f: 9.9D, p: @new<nint>(), a: (nint)1));
    var s1 = new TestWriteComparable_S(s: heapStr(t));
    var s2 = new TestWriteComparable_S(s: heapStr(t));
    if (@unsafe.StringData(s1.s) == @unsafe.StringData(s2.s)) {
        t.Fatalf("unexpected two heapStr ptr equal");
    }
    if (s1.s != s2.s) {
        t.Fatalf("unexpected two heapStr value not equal");
    }
    testWriteComparable<TestWriteComparable_S, TestWriteComparable_S>(t, s1, s2);
    testWriteComparable<@string, @string>(t, s1.s, s2.s);
    testWriteComparable<float32, float32>(t, (float32)0F, negativeZero<float32>());
    testWriteComparable<float64, float64>(t, (float64)0D, negativeZero<float64>());
    testWriteComparableNoEqual(t, math.NaN(), math.NaN());
    testWriteComparableNoEqual(t, new @string[]{"a", ""}.array(), new @string[]{"", "a"}.array());
    testWriteComparableNoEqual(t, new TestComparable_v1("foo", ""), new TestComparable_v1("", "foo"));
    testWriteComparableNoEqual(t, new TestComparable_v1ᴛ1((nint)0, new EmptyStruct()), new TestComparable_v1ᴛ1(new EmptyStruct(), (nint)0));
}

internal static void testWriteComparableNoEqual<T>(ж<testing.T> t, T v1, T v2) {
    var seed = MakeSeed();
    ref var h1 = ref heap<global::go.hash.maphash_package.Hash>(new Hash(), out var Ꮡh1);
    ref var h2 = ref heap<global::go.hash.maphash_package.Hash>(new Hash(), out var Ꮡh2);
    (h1.seed, h2.seed) = (seed, seed);
    WriteComparable(Ꮡh1, v1);
    WriteComparable(Ꮡh2, v2);
    if (h1.Sum64() == h2.Sum64()) {
        t.Fatalf("WriteComparable(seed, %v) == WriteComparable(seed, %v)", (v1).OrTypedNilOf(), (v2).OrTypedNilOf());
    }
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static readonly object writeComparableSeedPtrˢ = (@string)"WriteComparable(seed, ptr) != WriteComparable(seed, ptr)"u8;

internal static void testWriteComparable<T, Tᴺ>(ж<testing.T> t, T v, params Span<T> v2ʗp) {
    var v2 = v2ʗp.slice();

    var v2ʗ1 = v2;
    t.Run(reflect.TypeFor<Tᴺ>().String(), (ж<testing.T> tΔ1) => {
        ref var a = ref heap<T>(out var Ꮡa);

        a = v;
        T b = v;
        if (len(v2ʗ1) != 0) {
            b = v2ʗ1[0];
        }
        ж<T> pa = Ꮡa;
        ref var h1 = ref heap<global::go.hash.maphash_package.Hash>(new Hash(), out var Ꮡh1);
        ref var h2 = ref heap<global::go.hash.maphash_package.Hash>(new Hash(), out var Ꮡh2);
        h1.seed = MakeSeed();
        h2.seed = h1.seed;
        WriteComparable(Ꮡh1, a);
        WriteComparable(Ꮡh2, b);
        if (h1.Sum64() != h2.Sum64()) {
            tΔ1.Fatalf("WriteComparable(h, %v) != WriteComparable(h, %v)", (a).OrTypedNilOf(), (b).OrTypedNilOf());
        }
        WriteComparable(Ꮡh1, pa);
        var old = h1.Sum64();
        stackGrow(8192);
        WriteComparable(Ꮡh2, pa);
        var @new = h2.Sum64();
        if (old != @new) {
            tΔ1.Fatal(writeComparableSeedPtrˢ);
        }
    });
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static ref readonly @string hashOfUnhashableTypeˢ => ref hashOfUnhashableTypeˢᵇ;
private static readonly @string hashOfUnhashableTypeˢᵇ = "hash of unhashable type []uint8"u8;

public static void TestComparableShouldPanic(ж<testing.T> t) {
    GoFrame ᒐ = default;
    try {
        var s = slice<byte>("s"u8);
        var a = (any)s;
        defer(() => {
            var e = recover();
            var (err, ok) = e._<error>(ᐧ);
            if (!ok) {
                t.Fatalf("Comaparable(any([]byte)) should panic");
            }
            @string want = hashOfUnhashableTypeˢ;
            @string sΔ1 = err.Error(); if (!strings.Contains(sΔ1, want)) {
                t.Fatalf("want %s, got %s", want, sΔ1);
            }
        }, ref ᒐ);
        Comparable(MakeSeed(), a);
    }
    catch (Exception ᒐex) when (GoFrame.IsPanic(ᒐex, out PanicException? ᒐp)) { GoFrame.Capture(ᒐp); }
    finally { ᒐ.Run(); }
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static ref readonly @string abcˢ => ref abcˢᵇ;
private static readonly @string abcˢᵇ = "abc"u8;

public static void TestWriteComparableNoncommute(ж<testing.T> t) {
    var seed = MakeSeed();
    ref var h1 = ref heap(new global::go.hash.maphash_package.Hash(), out var Ꮡh1);
    ref var h2 = ref heap(new global::go.hash.maphash_package.Hash(), out var Ꮡh2);
    h1.SetSeed(seed);
    h2.SetSeed(seed);

    h1.WriteString(abcˢ);
    WriteComparable<nint>(Ꮡh1, 123);
    WriteComparable<nint>(Ꮡh2, 123);
    h2.WriteString(abcˢ);

    if (h1.Sum64() == h2.Sum64()) {
        t.Errorf("WriteComparable and WriteString unexpectedly commute");
    }
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static readonly object skipAllocationTestInˢ = (@string)"skip allocation test in purego mode - reflect-based implementation allocates more"u8;
internal static readonly object skipAllocationTestUnderˢ = (@string)"skip allocation test under -asan"u8;

internal partial struct TestComparableAllocations_S /*dyn*/ {
    internal nint a;
    internal @string b;
}

public static void TestComparableAllocations(ж<testing.T> t) {
    if (purego) {
        t.Skip(skipAllocationTestInˢ);
    }
    if (asan.Enabled) {
        t.Skip(skipAllocationTestUnderˢ);
    }
    ref var seed = ref heap<global::go.hash.maphash_package.ΔSeed>(MakeSeed(), out var Ꮡseed);
    @string x = heapStr(t);
    var seedʗ1 = seed;
    var allocs = testing.AllocsPerRun(10, () => {
        @string s = "s"u8 + x;
        Comparable(seedʗ1, s);
    });
    if (allocs > 0D) {
        t.Errorf("got %v allocs, want 0", allocs);
    }
    var seedʗ2 = seed;
    allocs = testing.AllocsPerRun(10, () => {
        var s = new TestComparableAllocations_S(123, "s"u8 + x);
        Comparable(seedʗ2, s);
    });
    if (allocs > 0D) {
        t.Errorf("got %v allocs, want 0", allocs);
    }
}

// Make sure a Hash implements the hash.Hash and hash.Hash64 interfaces.
internal static hash.Hash _ᴛ1ʗ = new maphash_test_package.maphash_HashжHash(Ꮡ(new Hash()));
internal static hash.Hash64 _ᴛ2ʗ = new maphash_test_package.maphash_HashжHash64(Ꮡ(new Hash()));

public static void TestHashInterface(ж<testing.T> t) {
    testhash.TestHash(t, () => new maphash_test_package.maphash_HashжHash(@new<global::go.hash.maphash_package.Hash>()));
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static ref readonly @string writeˢ => ref writeˢᵇ;
private static readonly @string writeˢᵇ = "Write"u8;
internal static ref readonly @string bytesˢ => ref bytesˢᵇ;
private static readonly @string bytesˢᵇ = "Bytes"u8;
internal static ref readonly @string stringˢ => ref stringˢᵇ;
private static readonly @string stringˢᵇ = "String"u8;

internal static void benchmarkSize(ж<testing.B> b, nint size) {
    var h = Ꮡ(new Hash());
    var buf = new slice<byte>(size);
    @string s = (@string)buf;

    var bufʗ1 = buf;
    var hʗ1 = h;
    b.Run(writeˢ, (ж<testing.B> bΔ1) => {
        bΔ1.SetBytes((int64)size);
        for (nint i = 0; i < (~bΔ1).N; i++) {
            hʗ1.Reset();
            hʗ1.Write(bufʗ1);
            hʗ1.Sum64();
        }
    });

    var bufʗ2 = buf;
    var hʗ2 = h;
    b.Run(bytesˢ, (ж<testing.B> bΔ2) => {
        bΔ2.SetBytes((int64)size);
        var seed = hʗ2.Seed();
        for (nint i = 0; i < (~bΔ2).N; i++) {
            Bytes(seed, bufʗ2);
        }
    });

    var hʗ3 = h;
    b.Run(stringˢ, (ж<testing.B> bΔ3) => {
        bΔ3.SetBytes((int64)size);
        var seed = hʗ3.Seed();
        for (nint i = 0; i < (~bΔ3).N; i++) {
            String(seed, s);
        }
    });
}

public static void BenchmarkHash(ж<testing.B> b) {
    var sizes = slice<nint>([4, 8, 16, 32, 64, 256, 320, 1024, 4096, 16384]);
    foreach (var (_, size) in range(sizes)) {
        b.Run(fmt.Sprint("n=", size), (ж<testing.B> bΔ1) => {
            benchmarkSize(bΔ1, size);
        });
    }
}

internal static void benchmarkComparable<T, Tᴺ>(ж<testing.B> b, T v) {
    b.Run(reflect.TypeFor<Tᴺ>().String(), (ж<testing.B> bΔ1) => {
        var seed = MakeSeed();
        for (nint i = 0; i < (~bΔ1).N; i++) {
            Comparable(seed, v);
        }
    });
}

internal partial struct BenchmarkComparable_testStruct /*dyn*/ {
    internal nint i;
    internal nuint u;
    internal bool b;
    internal float64 f;
    internal ж<nint> p;
    internal any a;
}

public static void BenchmarkComparable(ж<testing.B> b) {
    benchmarkComparable<int64, int64>(b, (int64)2);
    benchmarkComparable<uint64, uint64>(b, (uint64)8);
    benchmarkComparable<uintptr, uintptr>(b, (uintptr)12);
    benchmarkComparable<any, any>(b, (any)(@string)("s"u8));
    benchmarkComparable<@string, @string>(b, (@string)"s");
    benchmarkComparable<bool, bool>(b, true);
    benchmarkComparable<ж<float64>, ж<float64>>(b, @new<float64>());
    benchmarkComparable<float64, float64>(b, (float64)9D);
    benchmarkComparable<complex128, complex128>(b, (complex128)(1D + 9D.i()));
    benchmarkComparable<EmptyStruct, EmptyStruct>(b, new EmptyStruct());
    benchmarkComparable<BenchmarkComparable_testStruct, BenchmarkComparable_testStruct>(b, new BenchmarkComparable_testStruct(i: 9, u: 1, b: true, f: 9.9D, p: @new<nint>(), a: (nint)1));
}

} // end maphash_internal_test_package
