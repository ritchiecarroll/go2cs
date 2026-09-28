// Copyright 2009 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.
namespace go.encoding;

using bytes = bytes_package;
using errors = errors_package;
using fmt = fmt_package;
using io = io_package;
using math = math_package;
using reflect = reflect_package;
using debug = runtime.debug_package;
using strconv = strconv_package;
using strings = strings_package;
using testing = testing_package;
using time = time_package;
using runtime;
using static go.encoding.base64_package;
using ꓸꓸꓸany = Span<any>;

partial class base64_internal_test_package {

[GoType] internal partial struct testpair {
    internal @string decoded, encoded;
}

// RFC 3548 examples
// RFC 4648 examples
// Wikipedia examples
internal static slice<testpair> pairs = new testpair[]{
    new(((@string)(new byte[]{0x14, 0xfb, 0x9c, 0x03, 0xd9, 0x7e})), "FPucA9l+"u8),
    new(((@string)(new byte[]{0x14, 0xfb, 0x9c, 0x03, 0xd9})), "FPucA9k="u8),
    new(((@string)(new byte[]{0x14, 0xfb, 0x9c, 0x03})), "FPucAw=="u8),
    new(""u8, ""u8),
    new("f"u8, "Zg=="u8),
    new("fo"u8, "Zm8="u8),
    new("foo"u8, "Zm9v"u8),
    new("foob"u8, "Zm9vYg=="u8),
    new("fooba"u8, "Zm9vYmE="u8),
    new("foobar"u8, "Zm9vYmFy"u8),
    new("sure."u8, "c3VyZS4="u8),
    new("sure"u8, "c3VyZQ=="u8),
    new("sur"u8, "c3Vy"u8),
    new("su"u8, "c3U="u8),
    new("leasure."u8, "bGVhc3VyZS4="u8),
    new("easure."u8, "ZWFzdXJlLg=="u8),
    new("asure."u8, "YXN1cmUu"u8),
    new("sure."u8, "c3VyZS4="u8)
}.slice();

// Do nothing to a reference base64 string (leave in standard format)
internal static @string stdRef(@string @ref) {
    return @ref;
}

// Convert a reference string to URL-encoding
internal static @string urlRef(@string @ref) {
    @ref = strings.ReplaceAll(@ref, "+"u8, "-"u8);
    @ref = strings.ReplaceAll(@ref, "/"u8, "_"u8);
    return @ref;
}

// Convert a reference string to raw, unpadded format
internal static @string rawRef(@string @ref) {
    return strings.TrimRight(@ref, "="u8);
}

// Both URL and unpadding conversions
internal static @string rawURLRef(@string @ref) {
    return rawRef(urlRef(@ref));
}

internal static readonly @string encodeStd = "ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz0123456789+/"u8;

// A nonstandard encoding with a funny padding character, for testing
internal static ж<global::go.encoding.base64_package.Encoding> funnyEncoding;
internal static void initᴛfunnyEncoding() { funnyEncoding = (~NewEncoding(encodeStd)).WithPadding((rune)(rune)'@'); }

internal static @string funnyRef(@string @ref) {
    return strings.ReplaceAll(@ref, "="u8, "@"u8);
}

[GoType] internal partial struct encodingTest {
    internal ж<global::go.encoding.base64_package.Encoding> enc;        // Encoding to test
    internal Func<@string, @string> conv; // Reference string converter
}

internal static slice<encodingTest> encodingTests;
internal static void initᴛencodingTests() { encodingTests = new encodingTest[]{
    new(StdEncoding, stdRef),
    new(URLEncoding, urlRef),
    new(RawStdEncoding, rawRef),
    new(RawURLEncoding, rawURLRef),
    new(funnyEncoding, funnyRef),
    new((~StdEncoding).Strict(), stdRef),
    new((~URLEncoding).Strict(), urlRef),
    new((~RawStdEncoding).Strict(), rawRef),
    new((~RawURLEncoding).Strict(), rawURLRef),
    new((~funnyEncoding).Strict(), funnyRef)
}.slice(); }

internal static testpair bigtest = new testpair(
    "Twas brillig, and the slithy toves"u8,
    "VHdhcyBicmlsbGlnLCBhbmQgdGhlIHNsaXRoeSB0b3Zlcw=="u8
);

internal static bool testEqual(ж<testing.T> Ꮡt, @string msg, params ꓸꓸꓸany argsʗp) {
    var args = argsʗp.sslice();

    Ꮡt.Helper();
    if (!AreEqual(args[len(args) - 2], args[len(args) - 1])) {
        Ꮡt.Errorf(msg, args.ꓸꓸꓸ);
        return false;
    }
    return true;
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static readonly @string encodeQQWantQˢ = "Encode(%q) = %q, want %q"u8;
internal static readonly @string appendEncodeLeadQQWantQˢ = @"AppendEncode(""lead"", %q) = %q, want %q"u8;

public static void TestEncode(ж<testing.T> Ꮡt) {
    foreach (var (_, p) in pairs) {
        foreach (var (_, tt) in encodingTests) {
            @string got = tt.enc.EncodeToString(slice<byte>(p.decoded));
            testEqual(Ꮡt, encodeQQWantQˢ, p.decoded, got, tt.conv(p.encoded));
            var dst = tt.enc.AppendEncode(slice<byte>("lead"u8), slice<byte>(p.decoded));
            testEqual(Ꮡt, appendEncodeLeadQQWantQˢ, p.decoded, ((@string)dst), "lead" + tt.conv(p.encoded));
        }
    }
}

public static void TestEncoder(ж<testing.T> Ꮡt) {
    foreach (var (_, p) in pairs) {
        var bb = Ꮡ(new strings.Builder(nil));
        var encoder = NewEncoder(StdEncoding, new base64_test_package.strings_BuilderжWriter(bb));
        encoder.Write(slice<byte>(p.decoded));
        encoder.Close();
        testEqual(Ꮡt, encodeQQWantQˢ, p.decoded, bb.String(), p.encoded);
    }
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static readonly @string writeQGaveErrorVWantVˢ = "Write(%q) gave error %v, want %v"u8;
internal static readonly @string writeQGaveLengthVWantVˢ = "Write(%q) gave length %v, want %v"u8;
internal static readonly @string closeGaveErrorVWantVˢ = "Close gave error %v, want %v"u8;
internal static readonly @string encodingDOfQQWantQˢ = "Encoding/%d of %q = %q, want %q"u8;

public static void TestEncoderBuffering(ж<testing.T> Ꮡt) {
    var input = slice<byte>(bigtest.decoded);
    for (nint bs = 1; bs <= 12; bs++) {
        var bb = Ꮡ(new strings.Builder(nil));
        var encoder = NewEncoder(StdEncoding, new base64_test_package.strings_BuilderжWriter(bb));
        for (nint pos = 0; pos < len(input); pos += bs) {
            nint end = pos + bs;
            if (end > len(input)) {
                end = len(input);
            }
            var (n, errΔ1) = encoder.Write(input[(int)(pos)..(int)(end)]);
            testEqual(Ꮡt, writeQGaveErrorVWantVˢ, input[(int)(pos)..(int)(end)], errΔ1, ((error)default!));
            testEqual(Ꮡt, writeQGaveLengthVWantVˢ, input[(int)(pos)..(int)(end)], n, end - pos);
        }
        var err = encoder.Close();
        testEqual(Ꮡt, closeGaveErrorVWantVˢ, err, ((error)default!));
        testEqual(Ꮡt, encodingDOfQQWantQˢ, bs, bigtest.decoded, bb.String(), bigtest.encoded);
    }
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static readonly @string decodeQErrorVWantVˢ = "Decode(%q) = error %v, want %v"u8;
internal static readonly @string decodeQLengthVWantVˢ = "Decode(%q) = length %v, want %v"u8;
internal static readonly @string decodeQQWantQˢ = "Decode(%q) = %q, want %q"u8;
internal static readonly @string decodeStringQErrorVWantVˢ = "DecodeString(%q) = error %v, want %v"u8;
internal static readonly @string decodeStringQQWantQˢ = "DecodeString(%q) = %q, want %q"u8;
internal static readonly @string appendDecodeQErrorVWantVˢ = "AppendDecode(%q) = error %v, want %v"u8;
internal static readonly @string appendDecodeLeadQQWantQˢ = @"AppendDecode(""lead"", %q) = %q, want %q"u8;
internal static readonly @string appendDecodeQQWantQˢ = @"AppendDecode("""", %q) = %q, want %q"u8;

public static void TestDecode(ж<testing.T> Ꮡt) {
    foreach (var (_, p) in pairs) {
        foreach (var (_, tt) in encodingTests) {
            @string encoded = tt.conv(p.encoded);
            var dbuf = new slice<byte>(tt.enc.DecodedLen(len(encoded)));
            var (count, err) = tt.enc.Decode(dbuf, slice<byte>(encoded));
            testEqual(Ꮡt, decodeQErrorVWantVˢ, encoded, err, ((error)default!));
            testEqual(Ꮡt, decodeQLengthVWantVˢ, encoded, count, len(p.decoded));
            testEqual(Ꮡt, decodeQQWantQˢ, encoded, ((@string)(dbuf[0..(int)(count)])), p.decoded);
            (dbuf, err) = tt.enc.DecodeString(encoded);
            testEqual(Ꮡt, decodeStringQErrorVWantVˢ, encoded, err, ((error)default!));
            testEqual(Ꮡt, decodeStringQQWantQˢ, encoded, ((@string)dbuf), p.decoded);
            (var dst, err) = tt.enc.AppendDecode(slice<byte>("lead"u8), slice<byte>(encoded));
            testEqual(Ꮡt, appendDecodeQErrorVWantVˢ, p.encoded, err, ((error)default!));
            testEqual(Ꮡt, appendDecodeLeadQQWantQˢ, p.encoded, ((@string)dst), "lead" + p.decoded);
            (var dst2, err) = tt.enc.AppendDecode(dst.slice(-1, 0, len(p.decoded)), slice<byte>(encoded));
            testEqual(Ꮡt, appendDecodeQErrorVWantVˢ, p.encoded, err, ((error)default!));
            testEqual(Ꮡt, appendDecodeQQWantQˢ, p.encoded, ((@string)dst2), p.decoded);
            if (len(dst) > 0 && len(dst2) > 0 && Ꮡ(dst, 0) != Ꮡ(dst2, 0)) {
                Ꮡt.Errorf("unexpected capacity growth: got %d, want %d"u8, cap(dst2), cap(dst));
            }
        }
    }
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static readonly object readFailedˢ = (@string)"Read failed"u8;
internal static readonly @string readFromQLengthVWantVˢ = "Read from %q = length %v, want %v"u8;
internal static readonly @string decodingOfQQWantQˢ = "Decoding of %q = %q, want %q"u8;
internal static readonly @string readFromQVWantVˢ = "Read from %q = %v, want %v"u8;

public static void TestDecoder(ж<testing.T> Ꮡt) {
    foreach (var (_, p) in pairs) {
        var decoder = NewDecoder(StdEncoding, new base64_test_package.strings_ReaderжReader(strings.NewReader(p.encoded)));
        var dbuf = new slice<byte>(StdEncoding.DecodedLen(len(p.encoded)));
        var (count, err) = decoder.Read(dbuf);
        if (err != default! && !AreEqual(err, io.EOF)) {
            Ꮡt.Fatal(readFailedˢ, err);
        }
        testEqual(Ꮡt, readFromQLengthVWantVˢ, p.encoded, count, len(p.decoded));
        testEqual(Ꮡt, decodingOfQQWantQˢ, p.encoded, ((@string)(dbuf[0..(int)(count)])), p.decoded);
        if (!AreEqual(err, io.EOF)) {
            (_, err) = decoder.Read(dbuf);
        }
        testEqual(Ꮡt, readFromQVWantVˢ, p.encoded, err, io.EOF);
    }
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static readonly @string decodingDOfQQWantQˢ = "Decoding/%d of %q = %q, want %q"u8;

public static void TestDecoderBuffering(ж<testing.T> Ꮡt) {
    for (nint bs = 1; bs <= 12; bs++) {
        var decoder = NewDecoder(StdEncoding, new base64_test_package.strings_ReaderжReader(strings.NewReader(bigtest.encoded)));
        var buf = new slice<byte>(len(bigtest.decoded) + 12);
        nint total = default!;
        nint n = default!;
        error err = default!;
        for (total = 0; total < len(bigtest.decoded) && err == default!; ) {
            (n, err) = decoder.Read(buf[(int)(total)..(int)(total + bs)]);
            total += n;
        }
        if (err != default! && !AreEqual(err, io.EOF)) {
            Ꮡt.Errorf("Read from %q at pos %d = %d, unexpected error %v"u8, bigtest.encoded, total, n, err);
        }
        testEqual(Ꮡt, decodingDOfQQWantQˢ, bs, bigtest.encoded, ((@string)(buf[0..(int)(total)])), bigtest.decoded);
    }
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static readonly object decoderWronglyDetectedˢ = (@string)"Decoder wrongly detected corruption in"u8;
internal static readonly @string corruptionInQAtOffsetVˢ = "Corruption in %q at offset %v, want %v"u8;
internal static readonly object decoderFailedToDetectˢ = (@string)"Decoder failed to detect corruption in"u8;

[GoType("dyn")] internal partial struct TestDecodeCorrupt_testCases {
    internal @string input;
    internal nint offset; // -1 means no corruption.
}

public static void TestDecodeCorrupt(ж<testing.T> Ꮡt) {
    var testCases = new TestDecodeCorrupt_testCases[]{
        new(""u8, -1),
        new("\n"u8, -1),
        new("AAA=\n"u8, -1),
        new("AAAA\n"u8, -1),
        new("!!!!"u8, 0),
        new("===="u8, 0),
        new("x==="u8, 1),
        new("=AAA"u8, 0),
        new("A=AA"u8, 1),
        new("AA=A"u8, 2),
        new("AA==A"u8, 4),
        new("AAA=AAAA"u8, 4),
        new("AAAAA"u8, 4),
        new("AAAAAA"u8, 4),
        new("A="u8, 1),
        new("A=="u8, 1),
        new("AA="u8, 3),
        new("AA=="u8, -1),
        new("AAA="u8, -1),
        new("AAAA"u8, -1),
        new("AAAAAA="u8, 7),
        new("YWJjZA====="u8, 8),
        new("A!\n"u8, 1),
        new("A=\n"u8, 1)
    }.slice();
    foreach (var (_, tc) in testCases) {
        var dbuf = new slice<byte>(StdEncoding.DecodedLen(len(tc.input)));
        var (_, err) = StdEncoding.Decode(dbuf, slice<byte>(tc.input));
        if (tc.offset == -1) {
            if (err != default!) {
                Ꮡt.Error(decoderWronglyDetectedˢ, tc.input);
            }
            continue;
        }
        switch (err.type()) {
        case CorruptInputError errΔ1: {
            testEqual(Ꮡt, corruptionInQAtOffsetVˢ, tc.input, (nint)(int64)errΔ1, tc.offset);
            break;
        }
        default: {
            var errΔ1 = err;
            Ꮡt.Error(decoderFailedToDetectˢ, tc);
            break;
        }}
    }
}

public static void TestDecodeBounds(ж<testing.T> Ꮡt) {
    GoFrame ᒐ = default;
    try {
        array<byte> buf = new(32);
        @string s = StdEncoding.EncodeToString(buf[..]);
        defer(() => {
            {
                var errΔ1 = recover(); if (errΔ1 != default!) {
                    Ꮡt.Fatalf("Decode panicked unexpectedly: %v\n%s"u8, errΔ1, debug.Stack());
                }
            }
        }, ref ᒐ);
        var (n, err) = StdEncoding.Decode(buf[..], slice<byte>(s));
        if (n != len(buf) || err != default!) {
            Ꮡt.Fatalf("StdEncoding.Decode = %d, %v, want %d, nil"u8, n, err, len(buf));
        }
    }
    catch (Exception ᒐex) when (GoFrame.IsPanic(ᒐex, out PanicException? ᒐp)) { GoFrame.Capture(ᒐp); }
    finally { ᒐ.Run(); }
}

[GoType("dyn")] [GoLocalName("test")] internal partial struct TestEncodedLen_test {
    internal ж<global::go.encoding.base64_package.Encoding> enc;
    internal nint n;
    internal int64 want;
}

public static void TestEncodedLen(ж<testing.T> Ꮡt) {
    var tests = new TestEncodedLen_test[]{
        new(RawStdEncoding, 0, 0),
        new(RawStdEncoding, 1, 2),
        new(RawStdEncoding, 2, 3),
        new(RawStdEncoding, 3, 4),
        new(RawStdEncoding, 7, 10),
        new(StdEncoding, 0, 0),
        new(StdEncoding, 1, 4),
        new(StdEncoding, 2, 4),
        new(StdEncoding, 3, 4),
        new(StdEncoding, 4, 8),
        new(StdEncoding, 7, 12)
    }.slice();
    // check overflow
    var exprᴛ1 = strconv.IntSize;
    if (exprᴛ1 == 32) {
        tests = append(tests, new TestEncodedLen_test(RawStdEncoding, unchecked((nint)(1152921504606846976L)), 357913942));
        tests = append(tests, new TestEncodedLen_test(RawStdEncoding, unchecked((nint)(6917529027641081855L)), math.MaxInt));
    }
    else if (exprᴛ1 == 64) {
        tests = append(tests, new TestEncodedLen_test(RawStdEncoding, unchecked((nint)(1152921504606846976L)), 1537228672809129302L));
        tests = append(tests, new TestEncodedLen_test(RawStdEncoding, unchecked((nint)(6917529027641081855L)), math.MaxInt));
    }

    foreach (var (_, tt) in tests) {
        {
            nint got = tt.enc.EncodedLen(tt.n); if ((int64)got != tt.want) {
                Ꮡt.Errorf("EncodedLen(%d): got %d, want %d"u8, tt.n, got, tt.want);
            }
        }
    }
}

[GoType("dyn")] [GoLocalName("test")] internal partial struct TestDecodedLen_test {
    internal ж<global::go.encoding.base64_package.Encoding> enc;
    internal nint n;
    internal int64 want;
}

public static void TestDecodedLen(ж<testing.T> Ꮡt) {
    var tests = new TestDecodedLen_test[]{
        new(RawStdEncoding, 0, 0),
        new(RawStdEncoding, 2, 1),
        new(RawStdEncoding, 3, 2),
        new(RawStdEncoding, 4, 3),
        new(RawStdEncoding, 10, 7),
        new(StdEncoding, 0, 0),
        new(StdEncoding, 4, 3),
        new(StdEncoding, 8, 6)
    }.slice();
    // check overflow
    var exprᴛ1 = strconv.IntSize;
    if (exprᴛ1 == 32) {
        tests = append(tests, new TestDecodedLen_test(RawStdEncoding, unchecked((nint)(1537228672809129302L)), 268435456));
        tests = append(tests, new TestDecodedLen_test(RawStdEncoding, math.MaxInt, 1610612735));
    }
    else if (exprᴛ1 == 64) {
        tests = append(tests, new TestDecodedLen_test(RawStdEncoding, unchecked((nint)(1537228672809129302L)), 1152921504606846976L));
        tests = append(tests, new TestDecodedLen_test(RawStdEncoding, math.MaxInt, 6917529027641081855L));
    }

    foreach (var (_, tt) in tests) {
        {
            nint got = tt.enc.DecodedLen(tt.n); if ((int64)got != tt.want) {
                Ꮡt.Errorf("DecodedLen(%d): got %d, want %d"u8, tt.n, got, tt.want);
            }
        }
    }
}

// Hoisted Go string constant (single allocation; Go keeps it in RODATA)
internal static readonly @string alphaᶜ = "0123456789abcdefghijklmnopqrstuvwxyzABCDEFGHIJKLMNOPQRSTUVWXYZ"u8;

public static void TestBig(ж<testing.T> Ꮡt) {
    nint n = 3 * 1000 + 1;
    var raw = new slice<byte>(n);
    @string alpha = alphaᶜ;
    for (nint i = 0; i < n; i++) {
        raw[i] = alpha[i % len(alpha)];
    }
    var encoded = @new<bytes.Buffer>();
    var w = NewEncoder(StdEncoding, new base64_test_package.bytes_BufferжWriter(encoded));
    var (nn, err) = w.Write(raw);
    if (nn != n || err != default!) {
        Ꮡt.Fatalf("Encoder.Write(raw) = %d, %v want %d, nil"u8, nn, err, n);
    }
    err = w.Close();
    if (err != default!) {
        Ꮡt.Fatalf("Encoder.Close() = %v want nil"u8, err);
    }
    (var decoded, err) = io.ReadAll(NewDecoder(StdEncoding, new base64_test_package.bytes_BufferжReader(encoded)));
    if (err != default!) {
        Ꮡt.Fatalf("io.ReadAll(NewDecoder(...)): %v"u8, err);
    }
    if (!bytes.Equal(raw, decoded)) {
        nint i = default!;
        for (i = 0; i < len(decoded) && i < len(raw); i++) {
            if (decoded[i] != raw[i]) {
                break;
            }
        }
        Ꮡt.Errorf("Decode(Encode(%d-byte string)) failed at offset %d"u8, n, i);
    }
}

// Hoisted Go string constant (single allocation; Go keeps it in RODATA)
internal static readonly @string expectedᶜ = "sure"u8;

public static void TestNewLineCharacters(ж<testing.T> Ꮡt) {
    // Each of these should decode to the string "sure", without errors.
    @string expected = expectedᶜ;
    var examples = new @string[]{
        "c3VyZQ=="u8,
        "c3VyZQ==\r"u8,
        "c3VyZQ==\n"u8,
        "c3VyZQ==\r\n"u8,
        "c3VyZ\r\nQ=="u8,
        "c3V\ryZ\nQ=="u8,
        "c3V\nyZ\rQ=="u8,
        "c3VyZ\nQ=="u8,
        "c3VyZQ\n=="u8,
        "c3VyZQ=\n="u8,
        "c3VyZQ=\r\n\r\n="u8
    }.slice();
    foreach (var (_, e) in examples) {
        var (buf, err) = StdEncoding.DecodeString(e);
        if (err != default!) {
            Ꮡt.Errorf("Decode(%q) failed: %v"u8, e, err);
            continue;
        }
        {
            @string s = ((@string)buf); if (s != expected) {
                Ꮡt.Errorf("Decode(%q) = %q, want %q"u8, e, s, expected);
            }
        }
    }
}

[GoType] internal partial struct nextRead {
    internal nint n;  // bytes to return
    internal error err; // error to return
}

// faultInjectReader returns data from source, rate-limited
// and with the errors as written to nextc.
[GoType] internal partial struct faultInjectReader {
    internal @string source;
    internal /*<-*/channel<nextRead> nextc = /*<-*/channel<nextRead>.RecvOnly;
}

[GoRecv] internal static (nint, error) Read(this ref faultInjectReader r, slice<byte> p) {
    var nr = ᐸꟷ(r.nextc);
    if (len(p) > nr.n) {
        p = p[..(int)(nr.n)];
    }
    nint n = copy(p, r.source);
    r.source = r.source[(int)(n)..];
    return (n, nr.err);
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static readonly @string myErrorˢ = "my error"u8;

// tests that we don't ignore errors from our underlying reader
public static void TestDecoderIssue3577(ж<testing.T> Ꮡt) {
    var next = new channel<nextRead>(10);
    var wantErr = errors.New(myErrorˢ);
    next.ᐸꟷ(new nextRead(5, default!));
    next.ᐸꟷ(new nextRead(10, wantErr));
    next.ᐸꟷ(new nextRead(0, wantErr));
    var d = NewDecoder(StdEncoding, new base64_internal_test_package.faultInjectReaderжReader(Ꮡ(new faultInjectReader(
        source: "VHdhcyBicmlsbGlnLCBhbmQgdGhlIHNsaXRoeSB0b3Zlcw=="u8, // twas brillig...

        nextc: next
    ))));
    var errc = new channel<error>(1);
    var dʗ1 = d;
    var errcʗ1 = errc;
    goǃ(() => {
        var (_, err) = io.ReadAll(dʗ1);
        errcʗ1.ᐸꟷ(err);
    });
    var selᴛ1 = errc;
    var selᴛ2 = time.After((time.Duration)(5000000000L));
    switch (select(ᐸꟷ(selᴛ1, ꓸꓸꓸ), ᐸꟷ(selᴛ2, ꓸꓸꓸ))) {
    case 0 when selᴛ1.ꟷᐳ(out var err): {
        if (!AreEqual(err, wantErr)) {
            Ꮡt.Errorf("got error %v; want %v"u8, err, wantErr);
        }
        break;
    }
    case 1 when selᴛ2.ꟷᐳ(out _): {
        Ꮡt.Errorf("timeout; Decoder blocked without returning an error"u8);
        break;
    }}
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static readonly @string cpEat8aaaefˢ = """
CP/EAT8AAAEF
AQEBAQEBAAAAAAAAAAMAAQIEBQYHCAkKCwEAAQUBAQEBAQEAAAAAAAAAAQACAwQFBgcICQoLEAAB
BAEDAgQCBQcGCAUDDDMBAAIRAwQhEjEFQVFhEyJxgTIGFJGhsUIjJBVSwWIzNHKC0UMHJZJT8OHx
Y3M1FqKygyZEk1RkRcKjdDYX0lXiZfKzhMPTdePzRieUpIW0lcTU5PSltcXV5fVWZnaGlqa2xtbm
9jdHV2d3h5ent8fX5/cRAAICAQIEBAMEBQYHBwYFNQEAAhEDITESBEFRYXEiEwUygZEUobFCI8FS
0fAzJGLhcoKSQ1MVY3M08SUGFqKygwcmNcLSRJNUoxdkRVU2dGXi8rOEw9N14/NGlKSFtJXE1OT0
pbXF1eX1VmZ2hpamtsbW5vYnN0dXZ3eHl6e3x//aAAwDAQACEQMRAD8A9VSSSSUpJJJJSkkkJ+Tj
1kiy1jCJJDnAcCTykpKkuQ6p/jN6FgmxlNduXawwAzaGH+V6jn/R/wCt71zdn+N/qL3kVYFNYB4N
ji6PDVjWpKp9TSXnvTf8bFNjg3qOEa2n6VlLpj/rT/pf567DpX1i6L1hs9Py67X8mqdtg/rUWbbf
+gkp0kkkklKSSSSUpJJJJT//0PVUkkklKVLq3WMDpGI7KzrNjADtYNXvI/Mqr/Pd/q9W3vaxjnvM
NaCXE9gNSvGPrf8AWS3qmba5jjsJhoB0DAf0NDf6sevf+/lf8Hj0JJATfWT6/dV6oXU1uOLQeKKn
EQP+Hubtfe/+R7Mf/g7f5xcocp++Z11JMCJPgFBxOg7/AOuqDx8I/ikpkXkmSdU8mJIJA/O8EMAy
j+mSARB/17pKVXYWHXjsj7yIex0PadzXMO1zT5KHoNA3HT8ietoGhgjsfA+CSnvvqh/jJtqsrwOv
2b6NGNzXfTYexzJ+nU7/ALkf4P8Awv6P9KvTQQ4AgyDqCF85Pho3CTB7eHwXoH+LT65uZbX9X+o2
bqbPb06551Y4

"""u8;
internal static readonly object decodedResultsNotEqualˢ = (@string)"Decoded results not equal"u8;

public static void TestDecoderIssue4779(ж<testing.T> Ꮡt) {
    @string encoded = cpEat8aaaefˢ;
    @string encodedShort = strings.ReplaceAll(encoded, "\n"u8, ""u8);
    var dec = NewDecoder(StdEncoding, new base64_test_package.strings_ReaderжReader(strings.NewReader(encoded)));
    var (res1, err) = io.ReadAll(dec);
    if (err != default!) {
        Ꮡt.Errorf("ReadAll failed: %v"u8, err);
    }
    dec = NewDecoder(StdEncoding, new base64_test_package.strings_ReaderжReader(strings.NewReader(encodedShort)));
    slice<byte> res2 = default!;
    (res2, err) = io.ReadAll(dec);
    if (err != default!) {
        Ꮡt.Errorf("ReadAll failed: %v"u8, err);
    }
    if (!bytes.Equal(res1, res2)) {
        Ꮡt.Error(decodedResultsNotEqualˢ);
    }
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static readonly @string ywJjZAˢ = "YWJjZA====="u8;

public static void TestDecoderIssue7733(ж<testing.T> Ꮡt) {
    var (s, err) = StdEncoding.DecodeString(ywJjZAˢ);
    var want = ((global::go.encoding.base64_package.CorruptInputError)8);
    if (!reflect.DeepEqual(want, err)) {
        Ꮡt.Errorf("Error = %v; want CorruptInputError(8)"u8, err);
    }
    if (((sstring)s) != "abcd"u8) {
        Ꮡt.Errorf("DecodeString = %q; want abcd"u8, s);
    }
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static readonly @string wvLTlMrX9NpYDQlEIFlnDBˢ = "WvLTlMrX9NpYDQlEIFlnDB=="u8;
internal static readonly @string wvLTlMrX9NpYDQlEIFlnDAˢ = "WvLTlMrX9NpYDQlEIFlnDA=="u8;

public static void TestDecoderIssue15656(ж<testing.T> Ꮡt) {
    var (_, err) = (~StdEncoding).Strict().DecodeString(wvLTlMrX9NpYDQlEIFlnDBˢ);
    var want = ((global::go.encoding.base64_package.CorruptInputError)22);
    if (!reflect.DeepEqual(want, err)) {
        Ꮡt.Errorf("Error = %v; want CorruptInputError(22)"u8, err);
    }
    (_, err) = (~StdEncoding).Strict().DecodeString(wvLTlMrX9NpYDQlEIFlnDAˢ);
    if (err != default!) {
        Ꮡt.Errorf("Error = %v; want nil"u8, err);
    }
    (_, err) = StdEncoding.DecodeString(wvLTlMrX9NpYDQlEIFlnDBˢ);
    if (err != default!) {
        Ꮡt.Errorf("Error = %v; want nil"u8, err);
    }
}

public static void BenchmarkEncodeToString(ж<testing.B> Ꮡb) {
    ref var b = ref Ꮡb.DerefOrNull();

    var data = new slice<byte>(8192);
    b.SetBytes((int64)len(data));
    for (nint i = 0; i < b.N; i++) {
        StdEncoding.EncodeToString(data);
    }
}

public static void BenchmarkDecodeString(ж<testing.B> Ꮡb) {
    var sizes = new nint[]{2, 4, 8, 64, 8192}.slice();
    void benchFunc(ж<testing.B> bΔ1, nint benchSize) {
        @string data = StdEncoding.EncodeToString(new slice<byte>(benchSize));
        bΔ1.SetBytes((int64)len(data));
        bΔ1.ResetTimer();
        for (nint i = 0; i < (~bΔ1).N; i++) {
            StdEncoding.DecodeString(data);
        }
    }
    foreach (var (_, size) in sizes) {
        var benchFuncʗ1 = benchFunc;
        Ꮡb.Run(fmt.Sprintf("%d"u8, size), (ж<testing.B> bΔ2) => {
            benchFuncʗ1(bΔ2, size);
        });
    }
}

public static void BenchmarkNewEncoding(ж<testing.B> Ꮡb) {
    ref var b = ref Ꮡb.DerefOrNull();

    b.SetBytes((int64)len(new Encoding(nil).decodeMap));
    for (nint i = 0; i < b.N; i++) {
        var e = NewEncoding(encodeStd);
        foreach (var (_, v) in (~e).decodeMap.ΔRangeSnapshot()) {
            _ = v;
        }
    }
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static readonly @string aaaaaaˢ = "AAAAAA"u8;

public static void TestDecoderRaw(ж<testing.T> Ꮡt) {
    @string source = aaaaaaˢ;
    var want = new byte[]{0, 0, 0, 0}.slice();
    // Direct.
    var (dec1, err) = RawURLEncoding.DecodeString(source);
    if (err != default! || !bytes.Equal(dec1, want)) {
        Ꮡt.Errorf("RawURLEncoding.DecodeString(%q) = %x, %v, want %x, nil"u8, source, dec1, err, want);
    }
    // Through reader. Used to fail.
    var r = NewDecoder(RawURLEncoding, new base64_test_package.bytes_ReaderжReader(bytes.NewReader(slice<byte>(source))));
    (var dec2, err) = io.ReadAll(io.LimitReader(r, 100));
    if (err != default! || !bytes.Equal(dec2, want)) {
        Ꮡt.Errorf("reading NewDecoder(RawURLEncoding, %q) = %x, %v, want %x, nil"u8, source, dec2, err, want);
    }
    // Should work with padding.
    r = NewDecoder(URLEncoding, new base64_test_package.bytes_ReaderжReader(bytes.NewReader(slice<byte>(source + "=="))));
    (var dec3, err) = io.ReadAll(r);
    if (err != default! || !bytes.Equal(dec3, want)) {
        Ꮡt.Errorf("reading NewDecoder(URLEncoding, %q) = %x, %v, want %x, nil"u8, source + "==", dec3, err, want);
    }
}

} // end base64_internal_test_package
