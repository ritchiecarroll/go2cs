// Copyright 2019 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.
namespace go.hash;

using bytes = bytes_package;
using fmt = fmt_package;
using hash = hash_package;
using asan = @internal.asan_package;
using math = math_package;
using reflect = reflect_package;
using strings = strings_package;
using testing = testing_package;
using @unsafe = unsafe_package;
using @internal;
using MethodImplAttribute = global::System.Runtime.CompilerServices.MethodImplAttribute;
using MethodImplOptions = global::System.Runtime.CompilerServices.MethodImplOptions;
using static go.hash.maphash_package;

partial class maphash_internal_test_package {

public static void TestUnseededHash(ж<testing.T> Ꮡt) {
    var m = new map<uint64, EmptyStruct>{};
    for (nint i = 0; i < 1000; i++) {
        var h = @new<global::go.hash.maphash_package.Hash>();
        m[h.Sum64()] = new EmptyStruct();
    }
    if (len(m) < 900) {
        Ꮡt.Errorf("empty hash not sufficiently random: got %d, want 1000"u8, len(m));
    }
}

public static void TestSeededHash(ж<testing.T> Ꮡt) {
    var s = MakeSeed();
    var m = new map<uint64, EmptyStruct>{};
    for (nint i = 0; i < 1000; i++) {
        var h = @new<global::go.hash.maphash_package.Hash>();
        h.SetSeed(s);
        m[h.Sum64()] = new EmptyStruct();
    }
    if (len(m) != 1) {
        Ꮡt.Errorf("seeded hash is random: got %d, want 1"u8, len(m));
    }
}

public static void TestHashGrouping(ж<testing.T> Ꮡt) {
    var b = bytes.Repeat(slice<byte>("foo"u8), 100);
    var hh = new slice<ж<global::go.hash.maphash_package.Hash>>(7);
    foreach (var (i, _) in hh) {
        hh[i] = @new<global::go.hash.maphash_package.Hash>();
    }
    foreach (var (_, h) in hh[1..]) {
        h.SetSeed(hh[0].Seed());
    }
    hh[0].Write(b);
    hh[1].WriteString(((@string)b));
    void writeByte(ж<global::go.hash.maphash_package.Hash> h, byte bΔ1) {
        var err = h.WriteByte(bΔ1);
        if (err != default!) {
            Ꮡt.Fatalf("WriteByte: %v"u8, err);
        }
    }
    void writeSingleByte(ж<global::go.hash.maphash_package.Hash> h, byte bΔ2) {
        var (_, err) = h.Write(new byte[]{bΔ2}.slice());
        if (err != default!) {
            Ꮡt.Fatalf("Write single byte: %v"u8, err);
        }
    }
    void writeStringSingleByte(ж<global::go.hash.maphash_package.Hash> h, byte bΔ3) {
        var (_, err) = h.WriteString(((@string)new byte[]{bΔ3}.slice()));
        if (err != default!) {
            Ꮡt.Fatalf("WriteString single byte: %v"u8, err);
        }
    }
    foreach (var (i, x) in b) {
        writeByte(hh[2], x);
        writeSingleByte(hh[3], x);
        if (i == 0){
            writeByte(hh[4], x);
        } else {
            writeSingleByte(hh[4], x);
        }
        writeStringSingleByte(hh[5], x);
        if (i == 0){
            writeByte(hh[6], x);
        } else {
            writeStringSingleByte(hh[6], x);
        }
    }
    var sum = hh[0].Sum64();
    foreach (var (i, h) in hh) {
        if (sum != h.Sum64()) {
            Ꮡt.Errorf("hash %d not identical to a single Write"u8, i);
        }
    }
    {
        var sum1 = Bytes(hh[0].Seed(), b); if (sum1 != hh[0].Sum64()) {
            Ꮡt.Errorf("hash using Bytes not identical to a single Write"u8);
        }
    }
    {
        var sum1 = String(hh[0].Seed(), ((@string)b)); if (sum1 != hh[0].Sum64()) {
            Ꮡt.Errorf("hash using String not identical to a single Write"u8);
        }
    }
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static readonly @string fooˢ = "foo"u8;

public static void TestHashBytesVsString(ж<testing.T> Ꮡt) {
    @string s = fooˢ;
    var b = slice<byte>(s);
    var h1 = @new<global::go.hash.maphash_package.Hash>();
    var h2 = @new<global::go.hash.maphash_package.Hash>();
    h2.SetSeed(h1.Seed());
    var (n1, err1) = h1.WriteString(s);
    if (n1 != len(s) || err1 != default!) {
        Ꮡt.Fatalf("WriteString(s) = %d, %v, want %d, nil"u8, n1, err1, len(s));
    }
    var (n2, err2) = h2.Write(b);
    if (n2 != len(b) || err2 != default!) {
        Ꮡt.Fatalf("Write(b) = %d, %v, want %d, nil"u8, n2, err2, len(b));
    }
    if (h1.Sum64() != h2.Sum64()) {
        Ꮡt.Errorf("hash of string and bytes not identical"u8);
    }
}

public static void TestHashHighBytes(ж<testing.T> Ꮡt) {
    // See issue 34925.
    UntypedInt N = 10;
    var m = new map<uint64, EmptyStruct>{};
    for (nint i = 0; i < N; i++) {
        var h = @new<global::go.hash.maphash_package.Hash>();
        h.WriteString(fooˢ);
        m[(h.Sum64() >> (int)(32))] = new EmptyStruct();
    }
    if (len(m) < (nint)(N / 2)) {
        Ꮡt.Errorf("from %d seeds, wanted at least %d different hashes; got %d"u8, (nint)(N), (nint)(N / 2), len(m));
    }
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static readonly @string testingˢ = "testing"u8;

public static void TestRepeat(ж<testing.T> Ꮡt) {
    var h1 = @new<global::go.hash.maphash_package.Hash>();
    h1.WriteString(testingˢ);
    var sum1 = h1.Sum64();
    h1.Reset();
    h1.WriteString(testingˢ);
    var sum2 = h1.Sum64();
    if (sum1 != sum2) {
        Ꮡt.Errorf("different sum after resetting: %#x != %#x"u8, sum1, sum2);
    }
    var h2 = @new<global::go.hash.maphash_package.Hash>();
    h2.SetSeed(h1.Seed());
    h2.WriteString(testingˢ);
    var sum3 = h2.Sum64();
    if (sum1 != sum3) {
        Ꮡt.Errorf("different sum on the same seed: %#x != %#x"u8, sum1, sum3);
    }
}

public static void TestSeedFromSum64(ж<testing.T> Ꮡt) {
    ref var t = ref Ꮡt.DerefOrNull();

    var h1 = @new<global::go.hash.maphash_package.Hash>();
    h1.WriteString(fooˢ);
    var x = h1.Sum64(); // seed generated here
    var h2 = @new<global::go.hash.maphash_package.Hash>();
    h2.SetSeed(h1.Seed());
    h2.WriteString(fooˢ);
    var y = h2.Sum64();
    if (x != y) {
        Ꮡt.Errorf("hashes don't match: want %x, got %x"u8, x, y);
    }
}

public static void TestSeedFromSeed(ж<testing.T> Ꮡt) {
    ref var t = ref Ꮡt.DerefOrNull();

    var h1 = @new<global::go.hash.maphash_package.Hash>();
    h1.WriteString(fooˢ);
    _ = h1.Seed(); // seed generated here
    var x = h1.Sum64();
    var h2 = @new<global::go.hash.maphash_package.Hash>();
    h2.SetSeed(h1.Seed());
    h2.WriteString(fooˢ);
    var y = h2.Sum64();
    if (x != y) {
        Ꮡt.Errorf("hashes don't match: want %x, got %x"u8, x, y);
    }
}

public static void TestSeedFromFlush(ж<testing.T> Ꮡt) {
    ref var t = ref Ꮡt.DerefOrNull();

    var b = new slice<byte>(65);
    var h1 = @new<global::go.hash.maphash_package.Hash>();
    h1.Write(b); // seed generated here
    var x = h1.Sum64();
    var h2 = @new<global::go.hash.maphash_package.Hash>();
    h2.SetSeed(h1.Seed());
    h2.Write(b);
    var y = h2.Sum64();
    if (x != y) {
        Ꮡt.Errorf("hashes don't match: want %x, got %x"u8, x, y);
    }
}

public static void TestSeedFromReset(ж<testing.T> Ꮡt) {
    ref var t = ref Ꮡt.DerefOrNull();

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
        Ꮡt.Errorf("hashes don't match: want %x, got %x"u8, x, y);
    }
}

internal static T negativeZero<T>()
    where T : /* float32 | float64 */ IAdditionOperators<T, T, T>, ISubtractionOperators<T, T, T>, IMultiplyOperators<T, T, T>, IDivisionOperators<T, T, T>, IIncrementOperators<T>, IDecrementOperators<T>, IUnaryNegationOperators<T, T>, IEqualityOperators<T, T, bool>, IComparisonOperators<T, T, bool>, new()
{
    T f = GoZero<T>();
    f = -f;
    return f;
}

[GoType("dyn")] internal partial struct TestComparable_v {
    internal nint i;
    internal nuint u;
    internal bool b;
    internal float64 f;
    internal ж<nint> p;
    internal any a;
}

[GoType("dyn")] [GoLocalName("S")] internal partial struct TestComparable_S {
    internal @string s;
}

[GoType("dyn")] internal partial struct TestComparable_v1 {
    internal @string a, b;
}

[GoType("dyn")] internal partial struct TestComparable_v1ᴛ1 {
    internal any a, b;
}

public static void TestComparable(ж<testing.T> Ꮡt) {
    testComparable<int64, int64>(Ꮡt, (int64)2);
    testComparable<uint64, uint64>(Ꮡt, (uint64)8);
    testComparable<uintptr, uintptr>(Ꮡt, (uintptr)12);
    testComparable<any, any>(Ꮡt, ((any)(@string)("s"u8)));
    testComparable<@string, @string>(Ꮡt, (@string)"s");
    testComparable<bool, bool>(Ꮡt, true);
    testComparable<ж<float64>, ж<float64>>(Ꮡt, @new<float64>());
    testComparable<float64, float64>(Ꮡt, (float64)9D);
    testComparable<complex128, complex128>(Ꮡt, (complex128)(1D + 9D.i()));
    testComparable<EmptyStruct, EmptyStruct>(Ꮡt, new EmptyStruct());
    testComparable<TestComparable_v, TestComparable_v>(Ꮡt, new TestComparable_v(i: 9, u: 1, b: true, f: 9.9D, p: @new<nint>(), a: (nint)(1)));
    var s1 = new TestComparable_S(s: heapStr(Ꮡt));
    var s2 = new TestComparable_S(s: heapStr(Ꮡt));
    if (@unsafe.StringData(s1.s) == @unsafe.StringData(s2.s)) {
        Ꮡt.Fatalf("unexpected two heapStr ptr equal"u8);
    }
    if (s1.s != s2.s) {
        Ꮡt.Fatalf("unexpected two heapStr value not equal"u8);
    }
    testComparable<TestComparable_S, TestComparable_S>(Ꮡt, s1, s2);
    testComparable<@string, @string>(Ꮡt, s1.s, s2.s);
    var c1 = new channel<EmptyStruct>(0);
    var c2 = new channel<EmptyStruct>(0);
    testComparable<channel<EmptyStruct>, channel<EmptyStruct>>(Ꮡt, c1, c1);
    testComparable<channel<EmptyStruct>, channel<EmptyStruct>>(Ꮡt, ((channel<EmptyStruct>)default!));
    testComparable<float32, float32>(Ꮡt, (float32)0F, negativeZero<float32>());
    testComparable<float64, float64>(Ꮡt, (float64)0D, negativeZero<float64>());
    testComparableNoEqual(Ꮡt, math.NaN(), math.NaN());
    testComparableNoEqual(Ꮡt, new @string[]{"a"u8, ""u8}.array(), new @string[]{""u8, "a"u8}.array());
    testComparableNoEqual(Ꮡt, new TestComparable_v1("foo"u8, ""u8), new TestComparable_v1(""u8, "foo"u8));
    testComparableNoEqual(Ꮡt, new TestComparable_v1ᴛ1((nint)0, new EmptyStruct()), new TestComparable_v1ᴛ1(new EmptyStruct(), (nint)0));
    testComparableNoEqual(Ꮡt, c1, c2);
}

internal static void testComparableNoEqual<T>(ж<testing.T> Ꮡt, T v1, T v2) {
    var seed = MakeSeed();
    if (Comparable(seed, v1) == Comparable(seed, v2)) {
        Ꮡt.Fatalf("Comparable(seed, %v) == Comparable(seed, %v)"u8, v1, v2);
    }
}

internal static slice<byte> heapStrValue = slice<byte>("aTestString"u8);

internal static @string heapStr(ж<testing.T> Ꮡt) {
    return ((@string)heapStrValue);
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static readonly object comparableSeedPtrˢ = (@string)"Comparable(seed, ptr) != Comparable(seed, ptr)"u8;

internal static void testComparable<T, Tᴺ>(ж<testing.T> Ꮡt, T v, params Span<T> v2ʗp) {
    var v2 = v2ʗp.slice();

    var v2ʗ1 = v2;
    Ꮡt.Run(reflect.TypeFor<Tᴺ>().String(), (ж<testing.T> tΔ1) => {
        ref var a = ref heap<T>(out var Ꮡa);

        a = v;
        T b = v;
        if (len(v2ʗ1) != 0) {
            b = v2ʗ1[0];
        }
        ж<T> pa = Ꮡa;
        var seed = MakeSeed();
        if (Comparable(seed, a) != Comparable(seed, b)) {
            tΔ1.Fatalf("Comparable(seed, %v) != Comparable(seed, %v)"u8, a, b);
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
[MethodImpl(MethodImplOptions.NoInlining)] internal static void stackGrow(nint dep) {
    if (dep == 0) {
        return;
    }
    array<byte> local = new(1024);
    // make sure local is allocated on the stack.
    local[randUint64() % 1024] = (byte)randUint64();
    use = local[randUint64() % 1024];
    stackGrow(dep - 1);
}

[GoType("dyn")] [GoLocalName("S")] internal partial struct TestWriteComparable_S {
    internal @string s;
}

public static void TestWriteComparable(ж<testing.T> Ꮡt) {
    testWriteComparable<int64, int64>(Ꮡt, (int64)2);
    testWriteComparable<uint64, uint64>(Ꮡt, (uint64)8);
    testWriteComparable<uintptr, uintptr>(Ꮡt, (uintptr)12);
    testWriteComparable<any, any>(Ꮡt, ((any)(@string)("s"u8)));
    testWriteComparable<@string, @string>(Ꮡt, (@string)"s");
    testComparable<bool, bool>(Ꮡt, true);
    testWriteComparable<ж<float64>, ж<float64>>(Ꮡt, @new<float64>());
    testWriteComparable<float64, float64>(Ꮡt, (float64)9D);
    testWriteComparable<complex128, complex128>(Ꮡt, (complex128)(1D + 9D.i()));
    testWriteComparable<EmptyStruct, EmptyStruct>(Ꮡt, new EmptyStruct());
    testWriteComparable<TestComparable_v, TestComparable_v>(Ꮡt, new TestComparable_v(i: 9, u: 1, b: true, f: 9.9D, p: @new<nint>(), a: (nint)(1)));
    var s1 = new TestWriteComparable_S(s: heapStr(Ꮡt));
    var s2 = new TestWriteComparable_S(s: heapStr(Ꮡt));
    if (@unsafe.StringData(s1.s) == @unsafe.StringData(s2.s)) {
        Ꮡt.Fatalf("unexpected two heapStr ptr equal"u8);
    }
    if (s1.s != s2.s) {
        Ꮡt.Fatalf("unexpected two heapStr value not equal"u8);
    }
    testWriteComparable<TestWriteComparable_S, TestWriteComparable_S>(Ꮡt, s1, s2);
    testWriteComparable<@string, @string>(Ꮡt, s1.s, s2.s);
    testWriteComparable<float32, float32>(Ꮡt, (float32)0F, negativeZero<float32>());
    testWriteComparable<float64, float64>(Ꮡt, (float64)0D, negativeZero<float64>());
    testWriteComparableNoEqual(Ꮡt, math.NaN(), math.NaN());
    testWriteComparableNoEqual(Ꮡt, new @string[]{"a"u8, ""u8}.array(), new @string[]{""u8, "a"u8}.array());
    testWriteComparableNoEqual(Ꮡt, new TestComparable_v1("foo"u8, ""u8), new TestComparable_v1(""u8, "foo"u8));
    testWriteComparableNoEqual(Ꮡt, new TestComparable_v1ᴛ1((nint)0, new EmptyStruct()), new TestComparable_v1ᴛ1(new EmptyStruct(), (nint)0));
}

internal static void testWriteComparableNoEqual<T>(ж<testing.T> Ꮡt, T v1, T v2) {
    var seed = MakeSeed();
    ref var h1 = ref heap<global::go.hash.maphash_package.Hash>(out var Ꮡh1);
    h1 = new Hash(nil);
    ref var h2 = ref heap<global::go.hash.maphash_package.Hash>(out var Ꮡh2);
    h2 = new Hash(nil);
    (h1.seed, h2.seed) = (seed, seed);
    WriteComparable(Ꮡh1, v1);
    WriteComparable(Ꮡh2, v2);
    if (h1.Sum64() == h2.Sum64()) {
        Ꮡt.Fatalf("WriteComparable(seed, %v) == WriteComparable(seed, %v)"u8, v1, v2);
    }
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static readonly object writeComparableSeedPtrˢ = (@string)"WriteComparable(seed, ptr) != WriteComparable(seed, ptr)"u8;

internal static void testWriteComparable<T, Tᴺ>(ж<testing.T> Ꮡt, T v, params Span<T> v2ʗp) {
    var v2 = v2ʗp.slice();

    var v2ʗ1 = v2;
    Ꮡt.Run(reflect.TypeFor<Tᴺ>().String(), (ж<testing.T> tΔ1) => {
        ref var a = ref heap<T>(out var Ꮡa);

        a = v;
        T b = v;
        if (len(v2ʗ1) != 0) {
            b = v2ʗ1[0];
        }
        ж<T> pa = Ꮡa;
        ref var h1 = ref heap<global::go.hash.maphash_package.Hash>(out var Ꮡh1);
        h1 = new Hash(nil);
        ref var h2 = ref heap<global::go.hash.maphash_package.Hash>(out var Ꮡh2);
        h2 = new Hash(nil);
        h1.seed = MakeSeed();
        h2.seed = h1.seed;
        WriteComparable(Ꮡh1, a);
        WriteComparable(Ꮡh2, b);
        if (h1.Sum64() != h2.Sum64()) {
            tΔ1.Fatalf("WriteComparable(h, %v) != WriteComparable(h, %v)"u8, a, b);
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
internal static readonly @string hashOfUnhashableTypeˢ = "hash of unhashable type []uint8"u8;

public static void TestComparableShouldPanic(ж<testing.T> Ꮡt) {
    GoFrame ᒐ = default;
    try {
        var s = slice<byte>("s"u8);
        var a = ((any)s);
        defer(() => {
            var e = recover();
            var (err, ok) = e._<error>(ᐧ);
            if (!ok) {
                Ꮡt.Fatalf("Comaparable(any([]byte)) should panic"u8);
            }
            @string want = hashOfUnhashableTypeˢ;
            {
                @string sΔ1 = err.Error(); if (!strings.Contains(sΔ1, want)) {
                    Ꮡt.Fatalf("want %s, got %s"u8, want, sΔ1);
                }
            }
        }, ref ᒐ);
        Comparable(MakeSeed(), a);
    }
    catch (Exception ᒐex) when (GoFrame.IsPanic(ᒐex, out PanicException? ᒐp)) { GoFrame.Capture(ᒐp); }
    finally { ᒐ.Run(); }
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static readonly @string abcˢ = "abc"u8;

public static void TestWriteComparableNoncommute(ж<testing.T> Ꮡt) {
    var seed = MakeSeed();
    ref var h1 = ref heap(new global::go.hash.maphash_package.Hash(), out var Ꮡh1);
    ref var h2 = ref heap(new global::go.hash.maphash_package.Hash(), out var Ꮡh2);
    h1.SetSeed(seed);
    h2.SetSeed(seed);
    h1.WriteString(abcˢ);
    WriteComparable(Ꮡh1, 123);
    WriteComparable(Ꮡh2, 123);
    h2.WriteString(abcˢ);
    if (h1.Sum64() == h2.Sum64()) {
        Ꮡt.Errorf("WriteComparable and WriteString unexpectedly commute"u8);
    }
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static readonly object skipAllocationTestInˢ = (@string)"skip allocation test in purego mode - reflect-based implementation allocates more"u8;
internal static readonly object skipAllocationTestUnderˢ = (@string)"skip allocation test under -asan"u8;

[GoType("dyn")] [GoLocalName("S")] internal partial struct TestComparableAllocations_S {
    internal nint a;
    internal @string b;
}

public static void TestComparableAllocations(ж<testing.T> Ꮡt) {
    if (purego) {
        Ꮡt.Skip(skipAllocationTestInˢ);
    }
    if (asan.Enabled) {
        Ꮡt.Skip(skipAllocationTestUnderˢ);
    }
    ref var seed = ref heap<global::go.hash.maphash_package.ΔSeed>(out var Ꮡseed);
    seed = MakeSeed();
    @string x = heapStr(Ꮡt);
    var seedʗ1 = seed;
    var allocs = testing.AllocsPerRun(10, () => {
        @string s = "s"u8 + x;
        Comparable(seedʗ1, s);
    });
    if (allocs > 0D) {
        Ꮡt.Errorf("got %v allocs, want 0"u8, allocs);
    }
    var seedʗ2 = seed;
    allocs = testing.AllocsPerRun(10, () => {
        var s = new TestComparableAllocations_S(123, "s"u8 + x);
        Comparable(seedʗ2, s);
    });
    if (allocs > 0D) {
        Ꮡt.Errorf("got %v allocs, want 0"u8, allocs);
    }
}

// Make sure a Hash implements the hash.Hash and hash.Hash64 interfaces.
internal static hash.Hash _ᴛ1ʗ = new maphash_test_package.maphash_HashжHash(Ꮡ(new Hash(nil)));

internal static hash.Hash64 _ᴛ2ʗ = new maphash_test_package.maphash_HashжHash64(Ꮡ(new Hash(nil)));

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static readonly @string writeˢ = "Write"u8;
internal static readonly @string bytesˢ = "Bytes"u8;
internal static readonly @string stringˢ = "String"u8;

internal static void benchmarkSize(ж<testing.B> Ꮡb, nint size) {
    var h = Ꮡ(new Hash(nil));
    var buf = new slice<byte>(size);
    @string s = ((@string)buf);
    var bufʗ1 = buf;
    var hʗ1 = h;
    Ꮡb.Run(writeˢ, (ж<testing.B> bΔ1) => {
        bΔ1.SetBytes((int64)size);
        for (nint i = 0; i < (~bΔ1).N; i++) {
            hʗ1.Reset();
            hʗ1.Write(bufʗ1);
            hʗ1.Sum64();
        }
    });
    var bufʗ2 = buf;
    var hʗ2 = h;
    Ꮡb.Run(bytesˢ, (ж<testing.B> bΔ2) => {
        bΔ2.SetBytes((int64)size);
        var seed = hʗ2.Seed();
        for (nint i = 0; i < (~bΔ2).N; i++) {
            Bytes(seed, bufʗ2);
        }
    });
    var hʗ3 = h;
    Ꮡb.Run(stringˢ, (ж<testing.B> bΔ3) => {
        bΔ3.SetBytes((int64)size);
        var seed = hʗ3.Seed();
        for (nint i = 0; i < (~bΔ3).N; i++) {
            String(seed, s);
        }
    });
}

public static void BenchmarkHash(ж<testing.B> Ꮡb) {
    var sizes = new nint[]{4, 8, 16, 32, 64, 256, 320, 1024, 4096, 16384}.slice();
    foreach (var (_, size) in sizes) {
        Ꮡb.Run(fmt.Sprint((@string)"n="u8, size), (ж<testing.B> bΔ1) => {
            benchmarkSize(bΔ1, size);
        });
    }
}

internal static void benchmarkComparable<T, Tᴺ>(ж<testing.B> Ꮡb, T v) {
    Ꮡb.Run(reflect.TypeFor<Tᴺ>().String(), (ж<testing.B> bΔ1) => {
        var seed = MakeSeed();
        for (nint i = 0; i < (~bΔ1).N; i++) {
            Comparable(seed, v);
        }
    });
}

[GoType("dyn")] [GoLocalName("testStruct")] internal partial struct BenchmarkComparable_testStruct {
    internal nint i;
    internal nuint u;
    internal bool b;
    internal float64 f;
    internal ж<nint> p;
    internal any a;
}

public static void BenchmarkComparable(ж<testing.B> Ꮡb) {
    benchmarkComparable<int64, int64>(Ꮡb, (int64)2);
    benchmarkComparable<uint64, uint64>(Ꮡb, (uint64)8);
    benchmarkComparable<uintptr, uintptr>(Ꮡb, (uintptr)12);
    benchmarkComparable<any, any>(Ꮡb, ((any)(@string)("s"u8)));
    benchmarkComparable<@string, @string>(Ꮡb, (@string)"s");
    benchmarkComparable<bool, bool>(Ꮡb, true);
    benchmarkComparable<ж<float64>, ж<float64>>(Ꮡb, @new<float64>());
    benchmarkComparable<float64, float64>(Ꮡb, (float64)9D);
    benchmarkComparable<complex128, complex128>(Ꮡb, (complex128)(1D + 9D.i()));
    benchmarkComparable<EmptyStruct, EmptyStruct>(Ꮡb, new EmptyStruct());
    benchmarkComparable<BenchmarkComparable_testStruct, BenchmarkComparable_testStruct>(Ꮡb, new BenchmarkComparable_testStruct(i: 9, u: 1, b: true, f: 9.9D, p: @new<nint>(), a: (nint)(1)));
}

} // end maphash_internal_test_package
