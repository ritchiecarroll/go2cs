// Copyright 2009 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.
namespace go.crypto;

using bytes = bytes_package;
using cipher = go.crypto.cipher_package;
using cryptotest = go.crypto.@internal.cryptotest_package;
using fmt = fmt_package;
using testing = testing_package;
using go.crypto;
using go.crypto.@internal;
using static go.crypto.rc4_package;

partial class rc4_internal_test_package {

[GoType] internal partial struct rc4Test {
    internal slice<byte> key, keystream;
}

// Test vectors from the original cypherpunk posting of ARC4:
//   https://groups.google.com/group/sci.crypt/msg/10a300c9d21afca0?pli=1
// Test vectors from the Wikipedia page: https://en.wikipedia.org/wiki/RC4
internal static slice<rc4Test> golden = new rc4Test[]{
    new(
        new byte[]{0x01, 0x23, 0x45, 0x67, 0x89, 0xab, 0xcd, 0xef}.slice(),
        new byte[]{0x74, 0x94, 0xc2, 0xe7, 0x10, 0x4b, 0x08, 0x79}.slice()
    ),
    new(
        new byte[]{0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00}.slice(),
        new byte[]{0xde, 0x18, 0x89, 0x41, 0xa3, 0x37, 0x5d, 0x3a}.slice()
    ),
    new(
        new byte[]{0xef, 0x01, 0x23, 0x45}.slice(),
        new byte[]{0xd6, 0xa1, 0x41, 0xa7, 0xec, 0x3c, 0x38, 0xdf, 0xbd, 0x61}.slice()
    ),
    new(
        new byte[]{0x4b, 0x65, 0x79}.slice(),
        new byte[]{0xeb, 0x9f, 0x77, 0x81, 0xb7, 0x34, 0xca, 0x72, 0xa7, 0x19}.slice()
    ),
    new(
        new byte[]{0x57, 0x69, 0x6b, 0x69}.slice(),
        new byte[]{0x60, 0x44, 0xdb, 0x6d, 0x41, 0xb7}.slice()
    ),
    new(
        new byte[]{0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00}.slice(),
        new byte[]{
            0xde, 0x18, 0x89, 0x41, 0xa3, 0x37, 0x5d, 0x3a,
            0x8a, 0x06, 0x1e, 0x67, 0x57, 0x6e, 0x92, 0x6d,
            0xc7, 0x1a, 0x7f, 0xa3, 0xf0, 0xcc, 0xeb, 0x97,
            0x45, 0x2b, 0x4d, 0x32, 0x27, 0x96, 0x5f, 0x9e,
            0xa8, 0xcc, 0x75, 0x07, 0x6d, 0x9f, 0xb9, 0xc5,
            0x41, 0x7a, 0xa5, 0xcb, 0x30, 0xfc, 0x22, 0x19,
            0x8b, 0x34, 0x98, 0x2d, 0xbb, 0x62, 0x9e, 0xc0,
            0x4b, 0x4f, 0x8b, 0x05, 0xa0, 0x71, 0x08, 0x50,
            0x92, 0xa0, 0xc3, 0x58, 0x4a, 0x48, 0xe4, 0xa3,
            0x0a, 0x39, 0x7b, 0x8a, 0xcd, 0x1d, 0x00, 0x9e,
            0xc8, 0x7d, 0x68, 0x11, 0xf2, 0x2c, 0xf4, 0x9c,
            0xa3, 0xe5, 0x93, 0x54, 0xb9, 0x45, 0x15, 0x35,
            0xa2, 0x18, 0x7a, 0x86, 0x42, 0x6c, 0xca, 0x7d,
            0x5e, 0x82, 0x3e, 0xba, 0x00, 0x44, 0x12, 0x67,
            0x12, 0x57, 0xb8, 0xd8, 0x60, 0xae, 0x4c, 0xbd,
            0x4c, 0x49, 0x06, 0xbb, 0xc5, 0x35, 0xef, 0xe1,
            0x58, 0x7f, 0x08, 0xdb, 0x33, 0x95, 0x5c, 0xdb,
            0xcb, 0xad, 0x9b, 0x10, 0xf5, 0x3f, 0xc4, 0xe5,
            0x2c, 0x59, 0x15, 0x65, 0x51, 0x84, 0x87, 0xfe,
            0x08, 0x4d, 0x0e, 0x3f, 0x03, 0xde, 0xbc, 0xc9,
            0xda, 0x1c, 0xe9, 0x0d, 0x08, 0x5c, 0x2d, 0x8a,
            0x19, 0xd8, 0x37, 0x30, 0x86, 0x16, 0x36, 0x92,
            0x14, 0x2b, 0xd8, 0xfc, 0x5d, 0x7a, 0x73, 0x49,
            0x6a, 0x8e, 0x59, 0xee, 0x7e, 0xcf, 0x6b, 0x94,
            0x06, 0x63, 0xf4, 0xa6, 0xbe, 0xe6, 0x5b, 0xd2,
            0xc8, 0x5c, 0x46, 0x98, 0x6c, 0x1b, 0xef, 0x34,
            0x90, 0xd3, 0x7b, 0x38, 0xda, 0x85, 0xd3, 0x2e,
            0x97, 0x39, 0xcb, 0x23, 0x4a, 0x2b, 0xe7, 0x40
        }.slice()
    )
}.slice();

internal static void testEncrypt(ж<testing.T> Ꮡt, @string desc, ж<global::go.crypto.rc4_package.Cipher> Ꮡc, slice<byte> src, slice<byte> expect) {
    ref var c = ref Ꮡc.DerefOrNull();

    var dst = new slice<byte>(len(src));
    c.XORKeyStream(dst, src);
    foreach (var (i, v) in dst) {
        if (v != expect[i]) {
            Ꮡt.Fatalf("%s: mismatch at byte %d:\nhave %x\nwant %x"u8, desc, i, dst, expect);
        }
    }
}

public static void TestGolden(ж<testing.T> Ꮡt) {
    foreach (var (gi, g) in golden) {
        var data = new slice<byte>(len(g.keystream));
        foreach (var (i, _) in data) {
            data[i] = (byte)i;
        }
        var expect = new slice<byte>(len(g.keystream));
        foreach (var (i, _) in expect) {
            expect[i] = (byte)((byte)i ^ g.keystream[i]);
        }
        for (nint size = 1; size <= len(g.keystream); size++) {
            var (c, err) = NewCipher(g.key);
            if (err != default!) {
                Ꮡt.Fatalf("#%d: NewCipher: %v"u8, gi, err);
            }
            nint off = 0;
            while (off < len(g.keystream)) {
                nint n = len(g.keystream) - off;
                if (n > size) {
                    n = size;
                }
                @string desc = fmt.Sprintf("#%d@[%d:%d]"u8, gi, off, off + n);
                testEncrypt(Ꮡt, desc, c, data.slice(off, off + n), expect.slice(off, off + n));
                off += n;
            }
        }
    }
}

public static void TestBlock(ж<testing.T> Ꮡt) {
    var (c1a, _) = NewCipher(golden[0].key);
    var (c1b, _) = NewCipher(golden[1].key);
    var data1 = new slice<byte>((1 << (int)(20)));
    foreach (var (i, _) in data1) {
        c1a.XORKeyStream(data1.slice(i, i + 1), data1.slice(i, i + 1));
        c1b.XORKeyStream(data1.slice(i, i + 1), data1.slice(i, i + 1));
    }
    var (c2a, _) = NewCipher(golden[0].key);
    var (c2b, _) = NewCipher(golden[1].key);
    var data2 = new slice<byte>((1 << (int)(20)));
    c2a.XORKeyStream(data2, data2);
    c2b.XORKeyStream(data2, data2);
    if (!bytes.Equal(data1, data2)) {
        Ꮡt.Fatalf("bad block"u8);
    }
}

public static void TestRC4Stream(ж<testing.T> Ꮡt) {
    cryptotest.TestStream(Ꮡt, () => {
        var (c, _) = NewCipher(golden[0].key);
        return new rc4_internal_test_package.rc4_CipherжStream(c);
    });
}

internal static void benchmark(ж<testing.B> Ꮡb, int64 size) {
    ref var b = ref Ꮡb.DerefOrNull();

    var buf = new slice<byte>((nint)(size));
    var (c, err) = NewCipher(golden[0].key);
    if (err != default!) {
        throw panic(err);
    }
    b.SetBytes(size);
    for (nint i = 0; i < b.N; i++) {
        c.XORKeyStream(buf, buf);
    }
}

public static void BenchmarkRC4_128(ж<testing.B> Ꮡb) {
    benchmark(Ꮡb, 128);
}

public static void BenchmarkRC4_1K(ж<testing.B> Ꮡb) {
    benchmark(Ꮡb, 1024);
}

public static void BenchmarkRC4_8K(ж<testing.B> Ꮡb) {
    benchmark(Ꮡb, 8096);
}

} // end rc4_internal_test_package
