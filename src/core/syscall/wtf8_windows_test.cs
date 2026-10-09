// Copyright 2023 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.
namespace go;

using fmt = fmt_package;
using slices = slices_package;
using syscall = syscall_package;
using testing = testing_package;
using utf16 = unicode.utf16_package;
using utf8 = unicode.utf8_package;
using @unsafe = unsafe_package;
using static go.syscall_internal_test_package;
using unicode;

partial class syscall_test_package {

// 2-byte
// 3-byte
// unmatched surrogate halves
// high surrogates: 0xD800 to 0xDBFF
// "High surrogate followed by another high surrogate"
// "High surrogate followed by a symbol that is not a surrogate"
// "Unmatched high surrogate, followed by a surrogate pair, followed by an unmatched high surrogate"
// low surrogates: 0xDC00 to 0xDFFF
// "Low surrogate followed by another low surrogate"
// "Low surrogate followed by a symbol that is not a surrogate"
// "Unmatched low surrogate, followed by a surrogate pair, followed by an unmatched low surrogate"
// 4-byte

partial struct wtf8testsᴛ1 /*dyn*/ {
    internal @string str;
    internal slice<uint16> wstr;
}
internal static slice<wtf8testsᴛ1> wtf8tests = new wtf8testsᴛ1[]{
    new(
        str: "\x00"u8,
        wstr: new uint16[]{0x00}.slice()
    ),
    new(
        str: "\x5C"u8,
        wstr: new uint16[]{0x5C}.slice()
    ),
    new(
        str: "\x7F"u8,
        wstr: new uint16[]{0x7F}.slice()
    ),
    new(
        str: ((@string)(new byte[]{0xc2, 0x80})),
        wstr: new uint16[]{0x80}.slice()
    ),
    new(
        str: ((@string)(new byte[]{0xd7, 0x8a})),
        wstr: new uint16[]{0x05CA}.slice()
    ),
    new(
        str: ((@string)(new byte[]{0xdf, 0xbf})),
        wstr: new uint16[]{0x07FF}.slice()
    ),
    new(
        str: ((@string)(new byte[]{0xe0, 0xa0, 0x80})),
        wstr: new uint16[]{0x0800}.slice()
    ),
    new(
        str: ((@string)(new byte[]{0xe2, 0xb0, 0xbc})),
        wstr: new uint16[]{0x2C3C}.slice()
    ),
    new(
        str: ((@string)(new byte[]{0xef, 0xbf, 0xbf})),
        wstr: new uint16[]{0xFFFF}.slice()
    ),
    new(
        str: ((@string)(new byte[]{0xed, 0xa0, 0x80})),
        wstr: new uint16[]{0xD800}.slice()
    ),
    new(
        str: ((@string)(new byte[]{0xed, 0xa0, 0x80, 0xed, 0xa0, 0x80})),
        wstr: new uint16[]{0xD800, 0xD800}.slice()
    ),
    new(
        str: ((@string)new byte[]{0xED, 0xA0, 0x80, 0xA}.slice()),
        wstr: new uint16[]{0xD800, 0xA}.slice()
    ),
    new(
        str: ((@string)new byte[]{0xED, 0xA0, 0x80, 0xF0, 0x9D, 0x8C, 0x86, 0xED, 0xA0, 0x80}.slice()),
        wstr: new uint16[]{0xD800, 0xD834, 0xDF06, 0xD800}.slice()
    ),
    new(
        str: ((@string)(new byte[]{0xed, 0xa6, 0xaf})),
        wstr: new uint16[]{0xD9AF}.slice()
    ),
    new(
        str: ((@string)(new byte[]{0xed, 0xaf, 0xbf})),
        wstr: new uint16[]{0xDBFF}.slice()
    ),
    new(
        str: ((@string)(new byte[]{0xed, 0xb0, 0x80})),
        wstr: new uint16[]{0xDC00}.slice()
    ),
    new(
        str: ((@string)(new byte[]{0xed, 0xb0, 0x80, 0xed, 0xb0, 0x80})),
        wstr: new uint16[]{0xDC00, 0xDC00}.slice()
    ),
    new(
        str: ((@string)new byte[]{0xED, 0xB0, 0x80, 0xA}.slice()),
        wstr: new uint16[]{0xDC00, 0xA}.slice()
    ),
    new(
        str: ((@string)new byte[]{0xED, 0xB0, 0x80, 0xF0, 0x9D, 0x8C, 0x86, 0xED, 0xB0, 0x80}.slice()),
        wstr: new uint16[]{0xDC00, 0xD834, 0xDF06, 0xDC00}.slice()
    ),
    new(
        str: ((@string)(new byte[]{0xed, 0xbb, 0xae})),
        wstr: new uint16[]{0xDEEE}.slice()
    ),
    new(
        str: ((@string)(new byte[]{0xed, 0xbf, 0xbf})),
        wstr: new uint16[]{0xDFFF}.slice()
    ),
    new(
        str: ((@string)(new byte[]{0xf0, 0x90, 0x80, 0x80})),
        wstr: new uint16[]{0xD800, 0xDC00}.slice()
    ),
    new(
        str: ((@string)(new byte[]{0xf0, 0x9d, 0x8c, 0x86})),
        wstr: new uint16[]{0xD834, 0xDF06}.slice()
    ),
    new(
        str: ((@string)(new byte[]{0xf4, 0x8f, 0xbf, 0xbf})),
        wstr: new uint16[]{0xDBFF, 0xDFFF}.slice()
    )
}.slice();

public static void TestWTF16Rountrip(ж<testing.T> Ꮡt) {
    foreach (var (_, vᴛ1) in wtf8tests) {
        ref var tt = ref heap(new wtf8testsᴛ1(), out var Ꮡtt);
        tt = vᴛ1;

        var ttʗ1 = tt;
        Ꮡt.Run(fmt.Sprintf("%X"u8, tt.str), (ж<testing.T> tΔ1) => {
            var got = syscall_internal_test_package.EncodeWTF16(ttʗ1.str, default!);
            @string got2 = ((@string)syscall_internal_test_package.DecodeWTF16(got, default!));
            if (got2 != ttʗ1.str) {
                tΔ1.Errorf("got:\n%s\nwant:\n%s"u8, got2, ttʗ1.str);
            }
        });
    }
}

public static void TestWTF16Golden(ж<testing.T> Ꮡt) {
    foreach (var (_, vᴛ1) in wtf8tests) {
        ref var tt = ref heap(new wtf8testsᴛ1(), out var Ꮡtt);
        tt = vᴛ1;

        var ttʗ1 = tt;
        Ꮡt.Run(fmt.Sprintf("%X"u8, tt.str), (ж<testing.T> tΔ1) => {
            var got = syscall_internal_test_package.EncodeWTF16(ttʗ1.str, default!);
            if (!slices.Equal<slice<uint16>, uint16>(got, ttʗ1.wstr)) {
                tΔ1.Errorf("got:\n%v\nwant:\n%v"u8, got, ttʗ1.wstr);
            }
        });
    }
}

public static void FuzzEncodeWTF16(ж<testing.F> Ꮡf) {
    ref var f = ref Ꮡf.DerefOrNull();

    foreach (var (_, tt) in wtf8tests) {
        f.Add(tt.str);
    }
    Ꮡf.Fuzz((ж<testing.T> t, @string b) => {
        // test that there are no panics
        var got = syscall_internal_test_package.EncodeWTF16(b, default!);
        syscall_internal_test_package.DecodeWTF16(got, default!);
        if (utf8.ValidString(b)) {
            // if the input is a valid UTF-8 string, then
            // test that syscall.EncodeWTF16 behaves as
            // utf16.Encode
            var want = utf16.Encode(slice<rune>(b));
            if (!slices.Equal<slice<uint16>, uint16>(got, want)) {
                t.Errorf("got:\n%v\nwant:\n%v"u8, got, want);
            }
        }
    });
}

public static void FuzzDecodeWTF16(ж<testing.F> Ꮡf) {
    ref var f = ref Ꮡf.DerefOrNull();

    foreach (var (_, tt) in wtf8tests) {
        var b = @unsafe.Slice(@unsafe.SliceData(tt.wstr).Reinterpret<uint16, uint8>(), len(tt.wstr) * 2);
        f.Add(b);
    }
    Ꮡf.Fuzz((ж<testing.T> t, slice<byte> b) => {
        var u16 = @unsafe.Slice(@unsafe.SliceData(b).Reinterpret<byte, uint16>(), len(b) / 2);
        var got = syscall_internal_test_package.DecodeWTF16(u16, default!);
        if (utf8.Valid(got)) {
            // if the input is a valid UTF-8 string, then
            // test that syscall.DecodeWTF16 behaves as
            // utf16.Decode
            var want = utf16.Decode(u16);
            if (((@string)got) != ((@string)want)) {
                t.Errorf("got:\n%s\nwant:\n%s"u8, ((@string)got), ((@string)want));
            }
        }
        // WTF-8 should always roundtrip
        var got2 = syscall_internal_test_package.EncodeWTF16(((@string)got), default!);
        if (!slices.Equal<slice<uint16>, uint16>(got2, u16)) {
            t.Errorf("got:\n%v\nwant:\n%v"u8, got2, u16);
        }
    });
}

} // end syscall_test_package
