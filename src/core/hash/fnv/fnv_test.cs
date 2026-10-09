// Copyright 2011 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.
namespace go.hash;

using bytes = bytes_package;
using encoding = encoding_package;
using binary = go.encoding.binary_package;
using hash = hash_package;
using io = io_package;
using testing = testing_package;
using go.encoding;
using static go.hash.fnv_package;

partial class fnv_internal_test_package {

internal partial struct golden {
    internal slice<byte> @out;
    internal @string @in;
    internal @string halfState; // marshaled hash state after first half of in written, used by TestGoldenMarshal
}

internal static slice<golden> golden32 = new golden[]{
    new(new byte[]{0x81, 0x1c, 0x9d, 0xc5}.slice(), ""u8, ((@string)(new byte[]{0x66, 0x6e, 0x76, 0x01, 0x81, 0x1c, 0x9d, 0xc5}))),
    new(new byte[]{0x05, 0x0c, 0x5d, 0x7e}.slice(), "a"u8, ((@string)(new byte[]{0x66, 0x6e, 0x76, 0x01, 0x81, 0x1c, 0x9d, 0xc5}))),
    new(new byte[]{0x70, 0x77, 0x2d, 0x38}.slice(), "ab"u8, "fnv\x01\x05\f]~"u8),
    new(new byte[]{0x43, 0x9c, 0x2f, 0x4b}.slice(), "abc"u8, "fnv\x01\x05\f]~"u8)
}.slice();

internal static slice<golden> golden32a = new golden[]{
    new(new byte[]{0x81, 0x1c, 0x9d, 0xc5}.slice(), ""u8, ((@string)(new byte[]{0x66, 0x6e, 0x76, 0x02, 0x81, 0x1c, 0x9d, 0xc5}))),
    new(new byte[]{0xe4, 0x0c, 0x29, 0x2c}.slice(), "a"u8, ((@string)(new byte[]{0x66, 0x6e, 0x76, 0x02, 0x81, 0x1c, 0x9d, 0xc5}))),
    new(new byte[]{0x4d, 0x25, 0x05, 0xca}.slice(), "ab"u8, ((@string)(new byte[]{0x66, 0x6e, 0x76, 0x02, 0xe4, 0x0c, 0x29, 0x2c}))),
    new(new byte[]{0x1a, 0x47, 0xe9, 0x0b}.slice(), "abc"u8, ((@string)(new byte[]{0x66, 0x6e, 0x76, 0x02, 0xe4, 0x0c, 0x29, 0x2c})))
}.slice();

internal static slice<golden> golden64 = new golden[]{
    new(new byte[]{0xcb, 0xf2, 0x9c, 0xe4, 0x84, 0x22, 0x23, 0x25}.slice(), ""u8, ((@string)(new byte[]{0x66, 0x6e, 0x76, 0x03, 0xcb, 0xf2, 0x9c, 0xe4, 0x84, 0x22, 0x23, 0x25}))),
    new(new byte[]{0xaf, 0x63, 0xbd, 0x4c, 0x86, 0x01, 0xb7, 0xbe}.slice(), "a"u8, ((@string)(new byte[]{0x66, 0x6e, 0x76, 0x03, 0xcb, 0xf2, 0x9c, 0xe4, 0x84, 0x22, 0x23, 0x25}))),
    new(new byte[]{0x08, 0x32, 0x67, 0x07, 0xb4, 0xeb, 0x37, 0xb8}.slice(), "ab"u8, ((@string)(new byte[]{0x66, 0x6e, 0x76, 0x03, 0xaf, 0x63, 0xbd, 0x4c, 0x86, 0x01, 0xb7, 0xbe}))),
    new(new byte[]{0xd8, 0xdc, 0xca, 0x18, 0x6b, 0xaf, 0xad, 0xcb}.slice(), "abc"u8, ((@string)(new byte[]{0x66, 0x6e, 0x76, 0x03, 0xaf, 0x63, 0xbd, 0x4c, 0x86, 0x01, 0xb7, 0xbe})))
}.slice();

internal static slice<golden> golden64a = new golden[]{
    new(new byte[]{0xcb, 0xf2, 0x9c, 0xe4, 0x84, 0x22, 0x23, 0x25}.slice(), ""u8, ((@string)(new byte[]{0x66, 0x6e, 0x76, 0x04, 0xcb, 0xf2, 0x9c, 0xe4, 0x84, 0x22, 0x23, 0x25}))),
    new(new byte[]{0xaf, 0x63, 0xdc, 0x4c, 0x86, 0x01, 0xec, 0x8c}.slice(), "a"u8, ((@string)(new byte[]{0x66, 0x6e, 0x76, 0x04, 0xcb, 0xf2, 0x9c, 0xe4, 0x84, 0x22, 0x23, 0x25}))),
    new(new byte[]{0x08, 0x9c, 0x44, 0x07, 0xb5, 0x45, 0x98, 0x6a}.slice(), "ab"u8, ((@string)(new byte[]{0x66, 0x6e, 0x76, 0x04, 0xaf, 0x63, 0xdc, 0x4c, 0x86, 0x01, 0xec, 0x8c}))),
    new(new byte[]{0xe7, 0x1f, 0xa2, 0x19, 0x05, 0x41, 0x57, 0x4b}.slice(), "abc"u8, ((@string)(new byte[]{0x66, 0x6e, 0x76, 0x04, 0xaf, 0x63, 0xdc, 0x4c, 0x86, 0x01, 0xec, 0x8c})))
}.slice();

internal static slice<golden> golden128 = new golden[]{
    new(new byte[]{0x6c, 0x62, 0x27, 0x2e, 0x07, 0xbb, 0x01, 0x42, 0x62, 0xb8, 0x21, 0x75, 0x62, 0x95, 0xc5, 0x8d}.slice(), ""u8, ((@string)(new byte[]{0x66, 0x6e, 0x76, 0x05, 0x6c, 0x62, 0x27, 0x2e, 0x07, 0xbb, 0x01, 0x42, 0x62, 0xb8, 0x21, 0x75, 0x62, 0x95, 0xc5, 0x8d}))),
    new(new byte[]{0xd2, 0x28, 0xcb, 0x69, 0x10, 0x1a, 0x8c, 0xaf, 0x78, 0x91, 0x2b, 0x70, 0x4e, 0x4a, 0x14, 0x1e}.slice(), "a"u8, ((@string)(new byte[]{0x66, 0x6e, 0x76, 0x05, 0x6c, 0x62, 0x27, 0x2e, 0x07, 0xbb, 0x01, 0x42, 0x62, 0xb8, 0x21, 0x75, 0x62, 0x95, 0xc5, 0x8d}))),
    new(new byte[]{0x8, 0x80, 0x94, 0x5a, 0xee, 0xab, 0x1b, 0xe9, 0x5a, 0xa0, 0x73, 0x30, 0x55, 0x26, 0xc0, 0x88}.slice(), "ab"u8, ((@string)(new byte[]{0x66, 0x6e, 0x76, 0x05, 0xd2, 0x28, 0xcb, 0x69, 0x10, 0x1a, 0x8c, 0xaf, 0x78, 0x91, 0x2b, 0x70, 0x4e, 0x4a, 0x14, 0x1e}))),
    new(new byte[]{0xa6, 0x8b, 0xb2, 0xa4, 0x34, 0x8b, 0x58, 0x22, 0x83, 0x6d, 0xbc, 0x78, 0xc6, 0xae, 0xe7, 0x3b}.slice(), "abc"u8, ((@string)(new byte[]{0x66, 0x6e, 0x76, 0x05, 0xd2, 0x28, 0xcb, 0x69, 0x10, 0x1a, 0x8c, 0xaf, 0x78, 0x91, 0x2b, 0x70, 0x4e, 0x4a, 0x14, 0x1e})))
}.slice();

internal static slice<golden> golden128a = new golden[]{
    new(new byte[]{0x6c, 0x62, 0x27, 0x2e, 0x07, 0xbb, 0x01, 0x42, 0x62, 0xb8, 0x21, 0x75, 0x62, 0x95, 0xc5, 0x8d}.slice(), ""u8, ((@string)(new byte[]{0x66, 0x6e, 0x76, 0x06, 0x6c, 0x62, 0x27, 0x2e, 0x07, 0xbb, 0x01, 0x42, 0x62, 0xb8, 0x21, 0x75, 0x62, 0x95, 0xc5, 0x8d}))),
    new(new byte[]{0xd2, 0x28, 0xcb, 0x69, 0x6f, 0x1a, 0x8c, 0xaf, 0x78, 0x91, 0x2b, 0x70, 0x4e, 0x4a, 0x89, 0x64}.slice(), "a"u8, ((@string)(new byte[]{0x66, 0x6e, 0x76, 0x06, 0x6c, 0x62, 0x27, 0x2e, 0x07, 0xbb, 0x01, 0x42, 0x62, 0xb8, 0x21, 0x75, 0x62, 0x95, 0xc5, 0x8d}))),
    new(new byte[]{0x08, 0x80, 0x95, 0x44, 0xbb, 0xab, 0x1b, 0xe9, 0x5a, 0xa0, 0x73, 0x30, 0x55, 0xb6, 0x9a, 0x62}.slice(), "ab"u8, ((@string)(new byte[]{0x66, 0x6e, 0x76, 0x06, 0xd2, 0x28, 0xcb, 0x69, 0x6f, 0x1a, 0x8c, 0xaf, 0x78, 0x91, 0x2b, 0x70, 0x4e, 0x4a, 0x89, 0x64}))),
    new(new byte[]{0xa6, 0x8d, 0x62, 0x2c, 0xec, 0x8b, 0x58, 0x22, 0x83, 0x6d, 0xbc, 0x79, 0x77, 0xaf, 0x7f, 0x3b}.slice(), "abc"u8, ((@string)(new byte[]{0x66, 0x6e, 0x76, 0x06, 0xd2, 0x28, 0xcb, 0x69, 0x6f, 0x1a, 0x8c, 0xaf, 0x78, 0x91, 0x2b, 0x70, 0x4e, 0x4a, 0x89, 0x64})))
}.slice();

public static void TestGolden32(ж<testing.T> Ꮡt) {
    testGolden(Ꮡt, new fnv_internal_test_package.hash_Hash32ᴠHash(New32()), golden32);
}

public static void TestGolden32a(ж<testing.T> Ꮡt) {
    testGolden(Ꮡt, new fnv_internal_test_package.hash_Hash32ᴠHash(New32a()), golden32a);
}

public static void TestGolden64(ж<testing.T> Ꮡt) {
    testGolden(Ꮡt, new fnv_internal_test_package.hash_Hash64ᴠHash(New64()), golden64);
}

public static void TestGolden64a(ж<testing.T> Ꮡt) {
    testGolden(Ꮡt, new fnv_internal_test_package.hash_Hash64ᴠHash(New64a()), golden64a);
}

public static void TestGolden128(ж<testing.T> Ꮡt) {
    testGolden(Ꮡt, New128(), golden128);
}

public static void TestGolden128a(ж<testing.T> Ꮡt) {
    testGolden(Ꮡt, New128a(), golden128a);
}

internal static void testGolden(ж<testing.T> Ꮡt, hash.Hash hashΔ1, slice<golden> gold) {
    foreach (var (_, g) in gold) {
        hashΔ1.Reset();
        var (done, error) = hashΔ1.Write(slice<byte>(g.@in));
        if (error != default!) {
            Ꮡt.Fatalf("write error: %s"u8, error);
        }
        if (done != len(g.@in)) {
            Ꮡt.Fatalf("wrote only %d out of %d bytes"u8, done, len(g.@in));
        }
        {
            var actual = hashΔ1.Sum(default!); if (!bytes.Equal(g.@out, actual)) {
                Ꮡt.Errorf("hash(%q) = 0x%x want 0x%x"u8, g.@in, actual, g.@out);
            }
        }
    }
}

internal partial struct TestGoldenMarshal_tests /*dyn*/ {
    internal @string name;
    internal Func<hash.Hash> newHash;
    internal slice<golden> gold;
}

public static void TestGoldenMarshal(ж<testing.T> Ꮡt) {
    var tests = new TestGoldenMarshal_tests[]{
        new("32"u8, () => new fnv_internal_test_package.hash_Hash32ᴠHash(New32()), golden32),
        new("32a"u8, () => new fnv_internal_test_package.hash_Hash32ᴠHash(New32a()), golden32a),
        new("64"u8, () => new fnv_internal_test_package.hash_Hash64ᴠHash(New64()), golden64),
        new("64a"u8, () => new fnv_internal_test_package.hash_Hash64ᴠHash(New64a()), golden64a),
        new("128"u8, () => New128(), golden128),
        new("128a"u8, () => New128a(), golden128a)
    }.slice();
    foreach (var (_, vᴛ1) in tests) {
        ref var tt = ref heap(new TestGoldenMarshal_tests(), out var Ꮡtt);
        tt = vᴛ1;

        var ttʗ1 = tt;
        Ꮡt.Run(tt.name, (ж<testing.T> tΔ1) => {
            foreach (var (_, g) in ttʗ1.gold) {
                var h = ttʗ1.newHash();
                var h2 = ttʗ1.newHash();
                io.WriteString(h, g.@in.slice(0, len(g.@in) / 2));
                var (state, err) = h._<encoding.BinaryMarshaler>().MarshalBinary();
                if (err != default!) {
                    tΔ1.Errorf("could not marshal: %v"u8, err);
                    continue;
                }
                (var stateAppend, err) = h._<encoding.BinaryAppender>().AppendBinary(new slice<byte>(4, 32));
                if (err != default!) {
                    tΔ1.Errorf("could not marshal: %v"u8, err);
                    continue;
                }
                stateAppend = stateAppend[4..];
                if (((sstring)state) != g.halfState) {
                    tΔ1.Errorf("checksum(%q) state = %q, want %q"u8, g.@in, state, g.halfState);
                    continue;
                }
                if (((sstring)stateAppend) != g.halfState) {
                    tΔ1.Errorf("checksum(%q) state = %q, want %q"u8, g.@in, stateAppend, g.halfState);
                    continue;
                }
                {
                    var errΔ1 = h2._<encoding.BinaryUnmarshaler>().UnmarshalBinary(state); if (errΔ1 != default!) {
                        tΔ1.Errorf("could not unmarshal: %v"u8, errΔ1);
                        continue;
                    }
                }
                io.WriteString(h, g.@in.slice(len(g.@in) / 2));
                io.WriteString(h2, g.@in.slice(len(g.@in) / 2));
                {
                    var (actual, actual2) = (h.Sum(default!), h2.Sum(default!)); if (!bytes.Equal(actual, actual2)) {
                        tΔ1.Errorf("hash(%q) = 0x%x != marshaled 0x%x"u8, g.@in, actual, actual2);
                    }
                }
            }
        });
    }
}

public static void TestIntegrity32(ж<testing.T> Ꮡt) {
    testIntegrity(Ꮡt, new fnv_internal_test_package.hash_Hash32ᴠHash(New32()));
}

public static void TestIntegrity32a(ж<testing.T> Ꮡt) {
    testIntegrity(Ꮡt, new fnv_internal_test_package.hash_Hash32ᴠHash(New32a()));
}

public static void TestIntegrity64(ж<testing.T> Ꮡt) {
    testIntegrity(Ꮡt, new fnv_internal_test_package.hash_Hash64ᴠHash(New64()));
}

public static void TestIntegrity64a(ж<testing.T> Ꮡt) {
    testIntegrity(Ꮡt, new fnv_internal_test_package.hash_Hash64ᴠHash(New64a()));
}

public static void TestIntegrity128(ж<testing.T> Ꮡt) {
    testIntegrity(Ꮡt, New128());
}

public static void TestIntegrity128a(ж<testing.T> Ꮡt) {
    testIntegrity(Ꮡt, New128a());
}

internal static void testIntegrity(ж<testing.T> Ꮡt, hash.Hash h) {
    var data = new byte[]{(rune)'1', (rune)'2', 3, 4, 5}.slice();
    h.Write(data);
    var sum = h.Sum(default!);
    {
        nint size = h.Size(); if (size != len(sum)) {
            Ꮡt.Fatalf("Size()=%d but len(Sum())=%d"u8, size, len(sum));
        }
    }
    {
        var a = h.Sum(default!); if (!bytes.Equal(sum, a)) {
            Ꮡt.Fatalf("first Sum()=0x%x, second Sum()=0x%x"u8, sum, a);
        }
    }
    h.Reset();
    h.Write(data);
    {
        var a = h.Sum(default!); if (!bytes.Equal(sum, a)) {
            Ꮡt.Fatalf("Sum()=0x%x, but after Reset() Sum()=0x%x"u8, sum, a);
        }
    }
    h.Reset();
    h.Write(data[..2]);
    h.Write(data[2..]);
    {
        var a = h.Sum(default!); if (!bytes.Equal(sum, a)) {
            Ꮡt.Fatalf("Sum()=0x%x, but with partial writes, Sum()=0x%x"u8, sum, a);
        }
    }
    switch (h.Size()) {
    case 4: {
        var sum32 = h._<hash.Hash32>().Sum32();
        if (sum32 != binary.BigEndian.Uint32(sum)) {
            Ꮡt.Fatalf("Sum()=0x%x, but Sum32()=0x%x"u8, sum, sum32);
        }
        break;
    }
    case 8: {
        var sum64 = h._<hash.Hash64>().Sum64();
        if (sum64 != binary.BigEndian.Uint64(sum)) {
            Ꮡt.Fatalf("Sum()=0x%x, but Sum64()=0x%x"u8, sum, sum64);
        }
        break;
    }
    case 16: {
        break;
    }}

}

// There's no Sum128 function, so we don't need to test anything here.
public static void BenchmarkFnv32KB(ж<testing.B> Ꮡb) {
    benchmarkKB(Ꮡb, new fnv_internal_test_package.hash_Hash32ᴠHash(New32()));
}

public static void BenchmarkFnv32aKB(ж<testing.B> Ꮡb) {
    benchmarkKB(Ꮡb, new fnv_internal_test_package.hash_Hash32ᴠHash(New32a()));
}

public static void BenchmarkFnv64KB(ж<testing.B> Ꮡb) {
    benchmarkKB(Ꮡb, new fnv_internal_test_package.hash_Hash64ᴠHash(New64()));
}

public static void BenchmarkFnv64aKB(ж<testing.B> Ꮡb) {
    benchmarkKB(Ꮡb, new fnv_internal_test_package.hash_Hash64ᴠHash(New64a()));
}

public static void BenchmarkFnv128KB(ж<testing.B> Ꮡb) {
    benchmarkKB(Ꮡb, New128());
}

public static void BenchmarkFnv128aKB(ж<testing.B> Ꮡb) {
    benchmarkKB(Ꮡb, New128a());
}

internal static void benchmarkKB(ж<testing.B> Ꮡb, hash.Hash h) {
    ref var b = ref Ꮡb.DerefOrNull();

    b.SetBytes(1024);
    var data = new slice<byte>(1024);
    foreach (var (i, _) in data) {
        data[i] = (byte)i;
    }
    var @in = new slice<byte>(0, h.Size());
    b.ResetTimer();
    for (nint i = 0; i < b.N; i++) {
        h.Reset();
        h.Write(data);
        h.Sum(@in);
    }
}

} // end fnv_internal_test_package
