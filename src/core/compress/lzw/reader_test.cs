// Copyright 2011 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.
namespace go.compress;

using bytes = bytes_package;
using fmt = fmt_package;
using io = io_package;
using math = math_package;
using os = os_package;
using runtime = runtime_package;
using strconv = strconv_package;
using strings = strings_package;
using testing = testing_package;
using static go.compress.lzw_package;

partial class lzw_internal_test_package {

internal partial struct lzwTest {
    internal @string desc;
    internal @string raw;
    internal @string compressed;
    internal error err;
}

// This example comes from https://en.wikipedia.org/wiki/Graphics_Interchange_Format.
// This example comes from http://compgroups.net/comp.lang.ruby/Decompressing-LZW-compression-from-PDF-file
internal static slice<lzwTest> lzwTests = new lzwTest[]{
    new(
        "empty;LSB;8"u8,
        ""u8,
        "\x01\x01"u8,
        default!
    ),
    new(
        "empty;MSB;8"u8,
        ""u8,
        ((@string)(new byte[]{0x80, 0x80})),
        default!
    ),
    new(
        "tobe;LSB;7"u8,
        "TOBEORNOTTOBEORTOBEORNOT"u8,
        ((@string)(new byte[]{0x54, 0x4f, 0x42, 0x45, 0x4f, 0x52, 0x4e, 0x4f, 0x54, 0x82, 0x84, 0x86, 0x8b, 0x85, 0x87, 0x89, 0x81})),
        default!
    ),
    new(
        "tobe;LSB;8"u8,
        "TOBEORNOTTOBEORTOBEORNOT"u8,
        ((@string)(new byte[]{0x54, 0x9e, 0x08, 0x29, 0xf2, 0x44, 0x8a, 0x93, 0x27, 0x54, 0x04, 0x12, 0x34, 0xb8, 0xb0, 0xe0, 0xc1, 0x84, 0x01, 0x01})),
        default!
    ),
    new(
        "tobe;MSB;7"u8,
        "TOBEORNOTTOBEORTOBEORNOT"u8,
        ((@string)(new byte[]{0x54, 0x4f, 0x42, 0x45, 0x4f, 0x52, 0x4e, 0x4f, 0x54, 0x82, 0x84, 0x86, 0x8b, 0x85, 0x87, 0x89, 0x81})),
        default!
    ),
    new(
        "tobe;MSB;8"u8,
        "TOBEORNOTTOBEORTOBEORNOT"u8,
        ((@string)(new byte[]{0x2a, 0x13, 0xc8, 0x44, 0x52, 0x79, 0x48, 0x9c, 0x4f, 0x2a, 0x40, 0xa0, 0x90, 0x68, 0x5c, 0x16, 0x0f, 0x09, 0x80, 0x80})),
        default!
    ),
    new(
        "tobe-truncated;LSB;8"u8,
        "TOBEORNOTTOBEORTOBEORNOT"u8,
        ((@string)(new byte[]{0x54, 0x9e, 0x08, 0x29, 0xf2, 0x44, 0x8a, 0x93, 0x27, 0x54, 0x04})),
        io.ErrUnexpectedEOF
    ),
    new(
        "gif;LSB;8"u8,
        ((@string)(new byte[]{0x28, 0xff, 0xff, 0xff, 0x28, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff})),
        ((@string)(new byte[]{0x00, 0x51, 0xfc, 0x1b, 0x28, 0x70, 0xa0, 0xc1, 0x83, 0x01, 0x01})),
        default!
    ),
    new(
        "pdf;MSB;8"u8,
        "-----A---B"u8,
        ((@string)(new byte[]{0x80, 0x0b, 0x60, 0x50, 0x22, 0x0c, 0x0c, 0x85, 0x01})),
        default!
    )
}.slice();

public static void TestReader(ж<testing.T> Ꮡt) {
    GoFrame ᒐ = default;
    try {
        ref var b = ref heap(new bytes.Buffer(), out var Ꮡb);
        foreach (var (_, tt) in lzwTests) {
            var d = strings.Split(tt.desc, ";"u8);
            global::go.compress.lzw_package.Order order = default!;
            var exprᴛ1 = d[1];
            if (exprᴛ1 == "LSB"u8) {
                order = LSB;
            }
            else if (exprᴛ1 == "MSB"u8) {
                order = MSB;
            }
            else { /* default: */
                Ꮡt.Errorf("%s: bad order %q"u8, tt.desc, d[1]);
            }

            var (litWidth, _) = strconv.Atoi(d[2]);
            var rc = NewReader(new lzw_internal_test_package.strings_ReaderжReader(strings.NewReader(tt.compressed)), order, litWidth);
            var rcʗ1 = rc;
            defer(() => rcʗ1.Close(), ref ᒐ);
            b.Reset();
            var (n, err) = io.Copy(new lzw_internal_test_package.bytes_BufferжWriter(Ꮡb), rc);
            @string s = Ꮡb.String();
            if (err != default!) {
                if (!AreEqual(err, tt.err)) {
                    Ꮡt.Errorf("%s: io.Copy: %v want %v"u8, tt.desc, err, tt.err);
                }
                if (AreEqual(err, io.ErrUnexpectedEOF)) {
                    // Even if the input is truncated, we should still return the
                    // partial decoded result.
                    if (n == 0 || !strings.HasPrefix(tt.raw, s)) {
                        Ꮡt.Errorf("got %d bytes (%q), want a non-empty prefix of %q"u8, n, s, tt.raw);
                    }
                }
                continue;
            }
            if (s != tt.raw) {
                Ꮡt.Errorf("%s: got %d-byte %q want %d-byte %q"u8, tt.desc, n, s, len(tt.raw), tt.raw);
            }
        }
    }
    catch (Exception ᒐex) when (GoFrame.IsPanic(ᒐex, out PanicException? ᒐp)) { GoFrame.Capture(ᒐp); }
    finally { ᒐ.Run(); }
}

public static void TestReaderReset(ж<testing.T> Ꮡt) {
    GoFrame ᒐ = default;
    try {
        ref var b = ref heap(new bytes.Buffer(), out var Ꮡb);
        foreach (var (_, tt) in lzwTests) {
            var d = strings.Split(tt.desc, ";"u8);
            global::go.compress.lzw_package.Order order = default!;
            var exprᴛ1 = d[1];
            if (exprᴛ1 == "LSB"u8) {
                order = LSB;
            }
            else if (exprᴛ1 == "MSB"u8) {
                order = MSB;
            }
            else { /* default: */
                Ꮡt.Errorf("%s: bad order %q"u8, tt.desc, d[1]);
            }

            var (litWidth, _) = strconv.Atoi(d[2]);
            var rc = NewReader(new lzw_internal_test_package.strings_ReaderжReader(strings.NewReader(tt.compressed)), order, litWidth);
            var rcʗ1 = rc;
            defer(() => rcʗ1.Close(), ref ᒐ);
            b.Reset();
            var (n, err) = io.Copy(new lzw_internal_test_package.bytes_BufferжWriter(Ꮡb), rc);
            var b1 = b.Bytes();
            if (err != default!) {
                if (!AreEqual(err, tt.err)) {
                    Ꮡt.Errorf("%s: io.Copy: %v want %v"u8, tt.desc, err, tt.err);
                }
                if (AreEqual(err, io.ErrUnexpectedEOF)) {
                    // Even if the input is truncated, we should still return the
                    // partial decoded result.
                    if (n == 0 || !strings.HasPrefix(tt.raw, Ꮡb.String())) {
                        Ꮡt.Errorf("got %d bytes (%q), want a non-empty prefix of %q"u8, n, Ꮡb.String(), tt.raw);
                    }
                }
                continue;
            }
            b.Reset();
            rc._<ж<global::go.compress.lzw_package.Reader>>().Reset(new lzw_internal_test_package.strings_ReaderжReader(strings.NewReader(tt.compressed)), order, litWidth);
            (n, err) = io.Copy(new lzw_internal_test_package.bytes_BufferжWriter(Ꮡb), rc);
            var b2 = b.Bytes();
            if (err != default!) {
                Ꮡt.Errorf("%s: io.Copy: %v want %v"u8, tt.desc, err, (any)(default!));
                continue;
            }
            if (!bytes.Equal(b1, b2)) {
                Ꮡt.Errorf("bytes read were not the same"u8);
            }
        }
    }
    catch (Exception ᒐex) when (GoFrame.IsPanic(ᒐex, out PanicException? ᒐp)) { GoFrame.Capture(ᒐp); }
    finally { ᒐ.Run(); }
}

internal partial struct devZero {
}

internal static (nint, error) Read(this devZero _, slice<byte> p) {
    clear(p);
    return (len(p), default!);
}

public static void TestHiCodeDoesNotOverflow(ж<testing.T> Ꮡt) {
    var r = NewReader(new devZero(nil), LSB, 8);
    var d = r._<ж<global::go.compress.lzw_package.Reader>>();
    var buf = new slice<byte>(1024);
    var oldHi = (uint16)0;
    for (nint i = 0; i < 100; i++) {
        {
            var (_, err) = io.ReadFull(r, buf); if (err != default!) {
                Ꮡt.Fatalf("i=%d: %v"u8, i, err);
            }
        }
        // The hi code should never decrease.
        if ((~d).hi < oldHi) {
            Ꮡt.Fatalf("i=%d: hi=%d decreased from previous value %d"u8, i, (~d).hi, oldHi);
        }
        oldHi = d.Value.hi;
    }
}

internal partial struct TestNoLongerSavingPriorExpansions_iterations /*dyn*/ {
    internal nint width, n;
}

// TestNoLongerSavingPriorExpansions tests the decoder state when codes other
// than clear codes continue to be seen after decoder.hi and decoder.width
// reach their maximum values (4095 and 12), i.e. after we no longer save prior
// expansions. In particular, it tests seeing the highest possible code, 4095.
public static void TestNoLongerSavingPriorExpansions(ж<testing.T> Ꮡt) {
    // Iterations is used to calculate how many input bits are needed to get
    // the decoder.hi and decoder.width values up to their maximum.
    var iterations = new TestNoLongerSavingPriorExpansions_iterations[]{ // The final term is 257, not 256, as NewReader initializes d.hi to
 // d.clear+1 and the clear code is 256.

        new(9, 512 - 257),
        new(10, 1024 - 512),
        new(11, 2048 - 1024),
        new(12, 4096 - 2048)
    }.slice();
    nint nCodes = 0;
    nint nBits = 0;
    foreach (var (_, e) in iterations) {
        nCodes += e.n;
        nBits += e.n * e.width;
    }
    if (nCodes != 3839) {
        Ꮡt.Fatalf("nCodes: got %v, want %v"u8, nCodes, (nint)(3839));
    }
    if (nBits != 43255) {
        Ꮡt.Fatalf("nBits: got %v, want %v"u8, nBits, (nint)(43255));
    }
    // Construct our input of 43255 zero bits (which gets d.hi and d.width up
    // to 4095 and 12), followed by 0xfff (4095) as 12 bits, followed by 0x101
    // (EOF) as 12 bits.
    //
    // 43255 = 5406*8 + 7, and codes are read in LSB order. The final bytes are
    // therefore:
    //
    // xwwwwwww xxxxxxxx yyyyyxxx zyyyyyyy
    // 10000000 11111111 00001111 00001000
    //
    // or split out:
    //
    // .0000000 ........ ........ ........   w = 0x000
    // 1....... 11111111 .....111 ........   x = 0xfff
    // ........ ........ 00001... .0001000   y = 0x101
    //
    // The 12 'w' bits (not all are shown) form the 3839'th code, with value
    // 0x000. Just after decoder.read returns that code, d.hi == 4095 and
    // d.last == 0.
    //
    // The 12 'x' bits form the 3840'th code, with value 0xfff or 4095. Just
    // after decoder.read returns that code, d.hi == 4095 and d.last ==
    // decoderInvalidCode.
    //
    // The 12 'y' bits form the 3841'st code, with value 0x101, the EOF code.
    //
    // The 'z' bit is unused.
    var @in = new slice<byte>(5406);
    @in = append(@in, (byte)(0x80), (byte)(0xff), (byte)(0x0f), (byte)(0x08));
    var r = NewReader(new lzw_internal_test_package.bytes_ReaderжReader(bytes.NewReader(@in)), LSB, 8);
    var (nDecoded, err) = io.Copy(io.Discard, r);
    if (err != default!) {
        Ꮡt.Fatalf("Copy: %v"u8, err);
    }
    // nDecoded should be 3841: 3839 literal codes and then 2 decoded bytes
    // from 1 non-literal code. The EOF code contributes 0 decoded bytes.
    if (nDecoded != (int64)(nCodes + 2)) {
        Ꮡt.Fatalf("nDecoded: got %v, want %v"u8, nDecoded, nCodes + 2);
    }
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static readonly @string testdataETxtˢ = "../testdata/e.txt"u8;
internal static readonly object reuseˢ = (@string)"1e-Reuse"u8;

public static void BenchmarkDecoder(ж<testing.B> Ꮡb) {
    var (buf, err) = os.ReadFile(testdataETxtˢ);
    if (err != default!) {
        Ꮡb.Fatal(err);
    }
    if (len(buf) == 0) {
        Ꮡb.Fatalf("test file has no data"u8);
    }
    slice<byte> getInputBuf(slice<byte> bufΔ1, nint n) {
        var compressed = @new<bytes.Buffer>();
        var w = NewWriter(new lzw_internal_test_package.bytes_BufferжWriter(compressed), LSB, 8);
        for (nint i = 0; i < n; i += len(bufΔ1)) {
            if (len(bufΔ1) > n - i) {
                bufΔ1 = bufΔ1.slice(0, n - i);
            }
            w.Write(bufΔ1);
        }
        w.Close();
        return compressed.Bytes();
    }
    for (nint e = 4; e <= 6; e++) {
        nint n = (nint)math.Pow10(e);
        var bufʗ1 = buf;
        var getInputBufʗ1 = getInputBuf;
        Ꮡb.Run(fmt.Sprint((@string)"1e"u8, e), (ж<testing.B> bΔ1) => {
            bΔ1.StopTimer();
            bΔ1.SetBytes((int64)n);
            var buf1 = getInputBufʗ1(bufʗ1, n);
            runtime.GC();
            bΔ1.StartTimer();
            for (nint i = 0; i < (~bΔ1).N; i++) {
                io.Copy(io.Discard, NewReader(new lzw_internal_test_package.bytes_ReaderжReader(bytes.NewReader(buf1)), LSB, 8));
            }
        });
        var bufʗ2 = buf;
        var getInputBufʗ2 = getInputBuf;
        Ꮡb.Run(fmt.Sprint(reuseˢ, e), (ж<testing.B> bΔ2) => {
            bΔ2.StopTimer();
            bΔ2.SetBytes((int64)n);
            var buf1 = getInputBufʗ2(bufʗ2, n);
            runtime.GC();
            bΔ2.StartTimer();
            var r = NewReader(new lzw_internal_test_package.bytes_ReaderжReader(bytes.NewReader(buf1)), LSB, 8);
            for (nint i = 0; i < (~bΔ2).N; i++) {
                io.Copy(io.Discard, r);
                r.Close();
                r._<ж<global::go.compress.lzw_package.Reader>>().Reset(new lzw_internal_test_package.bytes_ReaderжReader(bytes.NewReader(buf1)), LSB, 8);
            }
        });
    }
}

} // end lzw_internal_test_package
