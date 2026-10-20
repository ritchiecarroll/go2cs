// Copyright 2021 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.
namespace go.@internal.coverage;

using binary = encoding.binary_package;
using io = io_package;
using testing = testing_package;
using encoding;
using static go.@internal.coverage.slicereader_package;

partial class slicereader_internal_test_package {

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static readonly @string foobarˢ = "foobar"u8;
internal static readonly @string bazbasherˢ = "bazbasher"u8;

public static void TestSliceReader(ж<testing.T> Ꮡt) {
    var b = new byte[]{}.slice();

    var bt = new slice<byte>(4);
    var e32 = (uint32)1030507;
    binary.LittleEndian.PutUint32(bt, e32);
    b = appendꓸꓸꓸ(b, bt);

    bt = new slice<byte>(8);
    var e64 = (uint64)907050301;
    binary.LittleEndian.PutUint64(bt, e64);
    b = appendꓸꓸꓸ(b, bt);

    b = appendUleb128(b, (nuint)e32);
    b = appendUleb128(b, (nuint)e64);
    b = appendUleb128(b, 6);
    @string s1 = foobarˢ;
    var s1b = slice<byte>(s1);
    b = appendꓸꓸꓸ(b, s1b);
    b = appendUleb128(b, 9);
    @string s2 = bazbasherˢ;
    var s2b = slice<byte>(s2);
    b = appendꓸꓸꓸ(b, s2b);

    @string readStr(ж<global::go.@internal.coverage.slicereader_package.Reader> slr) {
        var len = slr.ReadULEB128();
        return slr.ReadString((int64)len);
    }

    for (nint i = 0; i < 2; i++) {
        var slr = NewReader(b, i == 0);
        var g32 = slr.ReadUint32();
        if (g32 != e32) {
            Ꮡt.Fatalf("slr.ReadUint32() got %d want %d"u8, g32, e32);
        }
        var g64 = slr.ReadUint64();
        if (g64 != e64) {
            Ꮡt.Fatalf("slr.ReadUint64() got %d want %d"u8, g64, e64);
        }
        g32 = (uint32)slr.ReadULEB128();
        if (g32 != e32) {
            Ꮡt.Fatalf("slr.ReadULEB128() got %d want %d"u8, g32, e32);
        }
        g64 = slr.ReadULEB128();
        if (g64 != e64) {
            Ꮡt.Fatalf("slr.ReadULEB128() got %d want %d"u8, g64, e64);
        }
        @string gs1 = readStr(slr);
        if (gs1 != s1) {
            Ꮡt.Fatalf("readStr got %s want %s"u8, gs1, s1);
        }
        @string gs2 = readStr(slr);
        if (gs2 != s2) {
            Ꮡt.Fatalf("readStr got %s want %s"u8, gs2, s2);
        }
        {
            var (_, err) = slr.Seek(4, io.SeekStart); if (err != default!) {
                Ꮡt.Fatal(err);
            }
        }
        var off = slr.Offset();
        if (off != 4) {
            Ꮡt.Fatalf("Offset() returned %d wanted 4"u8, off);
        }
        g64 = slr.ReadUint64();
        if (g64 != e64) {
            Ꮡt.Fatalf("post-seek slr.ReadUint64() got %d want %d"u8, g64, e64);
        }
    }
}

internal static slice<byte> appendUleb128(slice<byte> b, nuint v) {
    while (ᐧ) {
        var c = (uint8)(v & 0x7f);
        v >>= (int)(7);
        if (v != 0) {
            c |= (uint8)(0x80);
        }
        b = append(b, c);
        if ((uint8)(c & 0x80) == 0) {
            break;
        }
    }
    return b;
}

} // end slicereader_internal_test_package
