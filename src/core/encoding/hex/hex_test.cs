// Copyright 2009 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.
namespace go.encoding;

using bytes = bytes_package;
using fmt = fmt_package;
using io = io_package;
using strings = strings_package;
using testing = testing_package;
using static go.encoding.hex_package;

partial class hex_internal_test_package {

[GoType] internal partial struct encDecTest {
    internal @string enc;
    internal slice<byte> dec;
}

internal static slice<encDecTest> encDecTests = new encDecTest[]{
    new(""u8, new byte[]{}.slice()),
    new("0001020304050607"u8, new byte[]{0, 1, 2, 3, 4, 5, 6, 7}.slice()),
    new("08090a0b0c0d0e0f"u8, new byte[]{8, 9, 10, 11, 12, 13, 14, 15}.slice()),
    new("f0f1f2f3f4f5f6f7"u8, new byte[]{0xf0, 0xf1, 0xf2, 0xf3, 0xf4, 0xf5, 0xf6, 0xf7}.slice()),
    new("f8f9fafbfcfdfeff"u8, new byte[]{0xf8, 0xf9, 0xfa, 0xfb, 0xfc, 0xfd, 0xfe, 0xff}.slice()),
    new("67"u8, new byte[]{(rune)'g'}.slice()),
    new("e3a1"u8, new byte[]{0xe3, 0xa1}.slice())
}.slice();

public static void TestEncode(ж<testing.T> Ꮡt) {
    foreach (var (i, test) in encDecTests) {
        var dst = new slice<byte>(EncodedLen(len(test.dec)));
        nint n = Encode(dst, test.dec);
        if (n != len(dst)) {
            Ꮡt.Errorf("#%d: bad return value: got: %d want: %d"u8, i, n, len(dst));
        }
        if (((sstring)dst) != test.enc) {
            Ꮡt.Errorf("#%d: got: %#v want: %#v"u8, i, dst, test.enc);
        }
        dst = slice<byte>("lead"u8);
        dst = AppendEncode(dst, test.dec);
        if (((@string)dst) != "lead"u8 + test.enc) {
            Ꮡt.Errorf("#%d: got: %#v want: %#v"u8, i, dst, "lead" + test.enc);
        }
    }
}

public static void TestDecode(ж<testing.T> Ꮡt) {
    // Case for decoding uppercase hex characters, since
    // Encode always uses lowercase.
    var decTests = append(encDecTests, new encDecTest("F8F9FAFBFCFDFEFF"u8, new byte[]{0xf8, 0xf9, 0xfa, 0xfb, 0xfc, 0xfd, 0xfe, 0xff}.slice()));
    foreach (var (i, test) in decTests) {
        var dst = new slice<byte>(DecodedLen(len(test.enc)));
        var (n, err) = Decode(dst, slice<byte>(test.enc));
        if (err != default!){
            Ꮡt.Errorf("#%d: bad return value: got:%d want:%d"u8, i, n, len(dst));
        } else 
        if (!bytes.Equal(dst, test.dec)) {
            Ꮡt.Errorf("#%d: got: %#v want: %#v"u8, i, dst, test.dec);
        }
        dst = slice<byte>("lead"u8);
        (dst, err) = AppendDecode(dst, slice<byte>(test.enc));
        if (err != default!){
            Ꮡt.Errorf("#%d: AppendDecode error: %v"u8, i, err);
        } else 
        if (((@string)dst) != "lead"u8 + ((sstring)test.dec)) {
            Ꮡt.Errorf("#%d: got: %#v want: %#v"u8, i, dst, "lead" + ((sstring)test.dec));
        }
    }
}

public static void TestEncodeToString(ж<testing.T> Ꮡt) {
    foreach (var (i, test) in encDecTests) {
        @string s = EncodeToString(test.dec);
        if (s != test.enc) {
            Ꮡt.Errorf("#%d got:%s want:%s"u8, i, s, test.enc);
        }
    }
}

public static void TestDecodeString(ж<testing.T> Ꮡt) {
    foreach (var (i, test) in encDecTests) {
        var (dst, err) = DecodeString(test.enc);
        if (err != default!) {
            Ꮡt.Errorf("#%d: unexpected err value: %s"u8, i, err);
            continue;
        }
        if (!bytes.Equal(dst, test.dec)) {
            Ꮡt.Errorf("#%d: got: %#v want: #%v"u8, i, dst, test.dec);
        }
    }
}


[GoType("dyn")] partial struct errTestsᴛ1 {
    internal @string @in;
    internal @string @out;
    internal error err;
}
internal static slice<errTestsᴛ1> errTests;
internal static void initᴛerrTests() { errTests = new errTestsᴛ1[]{
    new(""u8, ""u8, default!),
    new("0"u8, ""u8, ErrLength),
    new("zd4aa"u8, ""u8, ((global::go.encoding.hex_package.InvalidByteError)(rune)'z')),
    new("d4aaz"u8, ((@string)(new byte[]{0xd4, 0xaa})), ((global::go.encoding.hex_package.InvalidByteError)(rune)'z')),
    new("30313"u8, "01"u8, ErrLength),
    new("0g"u8, ""u8, ((global::go.encoding.hex_package.InvalidByteError)(rune)'g')),
    new("00gg"u8, "\x00"u8, ((global::go.encoding.hex_package.InvalidByteError)(rune)'g')),
    new("0\x01"u8, ""u8, ((global::go.encoding.hex_package.InvalidByteError)(rune)'\x01')),
    new("ffeed"u8, ((@string)(new byte[]{0xff, 0xee})), ErrLength)
}.slice(); }

public static void TestDecodeErr(ж<testing.T> Ꮡt) {
    foreach (var (_, tt) in errTests) {
        var @out = new slice<byte>(len(tt.@in) + 10);
        var (n, err) = Decode(@out, slice<byte>(tt.@in));
        if (((sstring)(@out.slice(0, n))) != tt.@out || !AreEqual(err, tt.err)) {
            Ꮡt.Errorf("Decode(%q) = %q, %v, want %q, %v"u8, tt.@in, ((@string)(@out.slice(0, n))), err, tt.@out, tt.err);
        }
    }
}

public static void TestDecodeStringErr(ж<testing.T> Ꮡt) {
    foreach (var (_, tt) in errTests) {
        var (@out, err) = DecodeString(tt.@in);
        if (((sstring)@out) != tt.@out || !AreEqual(err, tt.err)) {
            Ꮡt.Errorf("DecodeString(%q) = %q, %v, want %q, %v"u8, tt.@in, @out, err, tt.@out, tt.err);
        }
    }
}

[GoType("dyn")] internal partial struct TestEncoderDecoder_r {
    [GoEmbedded] public io_package.Reader Reader;
}

[GoType("dyn")] internal partial struct TestEncoderDecoder_w {
    [GoEmbedded] public io_package.Writer Writer;
}

public static void TestEncoderDecoder(ж<testing.T> Ꮡt) {
    foreach (var (_, multiplier) in new nint[]{1, 128, 192}.slice()) {
        foreach (var (_, test) in encDecTests) {
            var input = bytes.Repeat(test.dec, multiplier);
            @string output = strings.Repeat(test.enc, multiplier);
            ref var buf = ref heap(new bytes.Buffer(), out var Ꮡbuf);
            var enc = NewEncoder(new hex_test_package.bytes_BufferжWriter(Ꮡbuf));
            var r = new TestEncoderDecoder_r(new hex_test_package.bytes_ReaderжReader(bytes.NewReader(input))); // io.Reader only; not io.WriterTo
            {
                var (n, err) = io.CopyBuffer(enc, r, new slice<byte>(7)); if (n != (int64)len(input) || err != default!) {
                    Ꮡt.Errorf("encoder.Write(%q*%d) = (%d, %v), want (%d, nil)"u8, test.dec, multiplier, n, err, len(input));
                    continue;
                }
            }
            {
                @string encDst = Ꮡbuf.String(); if (encDst != output) {
                    Ꮡt.Errorf("buf(%q*%d) = %v, want %v"u8, test.dec, multiplier, encDst, output);
                    continue;
                }
            }
            var dec = NewDecoder(new hex_test_package.bytes_BufferжReader(Ꮡbuf));
            ref var decBuf = ref heap(new bytes.Buffer(), out var ᏑdecBuf);
            var w = new TestEncoderDecoder_w(new hex_test_package.bytes_BufferжWriter(ᏑdecBuf)); // io.Writer only; not io.ReaderFrom
            {
                var (_, err) = io.CopyBuffer(w, dec, new slice<byte>(7)); if (err != default! || decBuf.Len() != len(input)) {
                    Ꮡt.Errorf("decoder.Read(%q*%d) = (%d, %v), want (%d, nil)"u8, test.enc, multiplier, decBuf.Len(), err, len(input));
                }
            }
            if (!bytes.Equal(decBuf.Bytes(), input)) {
                Ꮡt.Errorf("decBuf(%q*%d) = %v, want %v"u8, test.dec, multiplier, decBuf.Bytes(), input);
                continue;
            }
        }
    }
}

public static void TestDecoderErr(ж<testing.T> Ꮡt) {
    foreach (var (_, tt) in errTests) {
        var dec = NewDecoder(new hex_test_package.strings_ReaderжReader(strings.NewReader(tt.@in)));
        var (@out, err) = io.ReadAll(dec);
        var wantErr = tt.err;
        // Decoder is reading from stream, so it reports io.ErrUnexpectedEOF instead of ErrLength.
        if (AreEqual(wantErr, ErrLength)) {
            wantErr = io.ErrUnexpectedEOF;
        }
        if (((sstring)@out) != tt.@out || !AreEqual(err, wantErr)) {
            Ꮡt.Errorf("NewDecoder(%q) = %q, %v, want %q, %v"u8, tt.@in, @out, err, tt.@out, wantErr);
        }
    }
}

public static void TestDumper(ж<testing.T> Ꮡt) {
    array<byte> @in = new(40);
    foreach (var (i, _) in @in) {
        @in[i] = (byte)(i + 30);
    }
    for (nint stride = 1; stride < len(@in); stride++) {
        ref var @out = ref heap(new bytes.Buffer(), out var Ꮡout);
        var dumper = Dumper(new hex_test_package.bytes_BufferжWriter(Ꮡout));
        nint done = 0;
        while (done < len(@in)) {
            nint todo = done + stride;
            if (todo > len(@in)) {
                todo = len(@in);
            }
            dumper.Write(@in.slice(done, todo));
            done = todo;
        }
        dumper.Close();
        if (!bytes.Equal(@out.Bytes(), expectedHexDump)) {
            Ꮡt.Errorf("stride: %d failed. got:\n%s\nwant:\n%s"u8, stride, @out.Bytes(), expectedHexDump);
        }
    }
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static readonly @string gopherˢ = "00000000  67 6f 70 68 65 72                                 |gopher|\n"u8;

public static void TestDumper_doubleclose(ж<testing.T> Ꮡt) {
    ref var @out = ref heap(new strings.Builder(), out var Ꮡout);
    var dumper = Dumper(new hex_test_package.strings_BuilderжWriter(Ꮡout));
    dumper.Write(slice<byte>(@"gopher"u8));
    dumper.Close();
    dumper.Close();
    dumper.Write(slice<byte>(@"gopher"u8));
    dumper.Close();
    @string expected = gopherˢ;
    if (@out.String() != expected) {
        Ꮡt.Fatalf("got:\n%#v\nwant:\n%#v"u8, @out.String(), expected);
    }
}

public static void TestDumper_earlyclose(ж<testing.T> Ꮡt) {
    ref var @out = ref heap(new strings.Builder(), out var Ꮡout);
    var dumper = Dumper(new hex_test_package.strings_BuilderжWriter(Ꮡout));
    dumper.Close();
    dumper.Write(slice<byte>(@"gopher"u8));
    @string expected = ""u8;
    if (@out.String() != expected) {
        Ꮡt.Fatalf("got:\n%#v\nwant:\n%#v"u8, @out.String(), expected);
    }
}

public static void TestDump(ж<testing.T> Ꮡt) {
    array<byte> @in = new(40);
    foreach (var (i, _) in @in) {
        @in[i] = (byte)(i + 30);
    }
    var @out = slice<byte>(Dump(@in[..]));
    if (!bytes.Equal(@out, expectedHexDump)) {
        Ꮡt.Errorf("got:\n%s\nwant:\n%s"u8, @out, expectedHexDump);
    }
}

internal static slice<byte> expectedHexDump = slice<byte>("""
00000000  1e 1f 20 21 22 23 24 25  26 27 28 29 2a 2b 2c 2d  |.. !"#$%&'()*+,-|
00000010  2e 2f 30 31 32 33 34 35  36 37 38 39 3a 3b 3c 3d  |./0123456789:;<=|
00000020  3e 3f 40 41 42 43 44 45                           |>?@ABCDE|

"""u8);

internal static slice<byte> sink;

public static void BenchmarkEncode(ж<testing.B> Ꮡb) {
    foreach (var (_, size) in new nint[]{256, 1024, 4096, 16384}.slice()) {
        var src = bytes.Repeat(new byte[]{2, 3, 5, 7, 9, 11, 13, 17}.slice(), size / 8);
        sink = new slice<byte>(2 * size);
        var srcʗ1 = src;
        Ꮡb.Run(fmt.Sprintf("%v"u8, size), (ж<testing.B> bΔ1) => {
            bΔ1.SetBytes((int64)size);
            for (nint i = 0; i < (~bΔ1).N; i++) {
                Encode(sink, srcʗ1);
            }
        });
    }
}

public static void BenchmarkDecode(ж<testing.B> Ꮡb) {
    ref var b = ref Ꮡb.DerefOrNull();

    foreach (var (_, size) in new nint[]{256, 1024, 4096, 16384}.slice()) {
        var src = bytes.Repeat(new byte[]{(rune)'2', (rune)'b', (rune)'7', (rune)'4', (rune)'4', (rune)'f', (rune)'a', (rune)'a'}.slice(), size / 8);
        sink = new slice<byte>(size / 2);
        var srcʗ1 = src;
        Ꮡb.Run(fmt.Sprintf("%v"u8, size), (ж<testing.B> bΔ1) => {
            bΔ1.SetBytes((int64)size);
            for (nint i = 0; i < (~bΔ1).N; i++) {
                Decode(sink, srcʗ1);
            }
        });
    }
}

public static void BenchmarkDecodeString(ж<testing.B> Ꮡb) {
    foreach (var (_, size) in new nint[]{256, 1024, 4096, 16384}.slice()) {
        @string src = strings.Repeat("2b744faa"u8, size / 8);
        Ꮡb.Run(fmt.Sprintf("%v"u8, size), (ж<testing.B> bΔ1) => {
            bΔ1.SetBytes((int64)size);
            for (nint i = 0; i < (~bΔ1).N; i++) {
                (sink, _) = DecodeString(src);
            }
        });
    }
}

public static void BenchmarkDump(ж<testing.B> Ꮡb) {
    foreach (var (_, size) in new nint[]{256, 1024, 4096, 16384}.slice()) {
        var src = bytes.Repeat(new byte[]{2, 3, 5, 7, 9, 11, 13, 17}.slice(), size / 8);
        var srcʗ1 = src;
        Ꮡb.Run(fmt.Sprintf("%v"u8, size), (ж<testing.B> bΔ1) => {
            bΔ1.SetBytes((int64)size);
            for (nint i = 0; i < (~bΔ1).N; i++) {
                Dump(srcʗ1);
            }
        });
    }
}

} // end hex_internal_test_package
