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
using MethodImplAttribute = global::System.Runtime.CompilerServices.MethodImplAttribute;
using MethodImplOptions = global::System.Runtime.CompilerServices.MethodImplOptions;
using runtime;
using static go.encoding.base64_package;
using ꓸꓸꓸany = Span<any>;

partial class base64_internal_test_package {

internal partial struct testpair {
    internal @string decoded, encoded;
}

internal static slice<testpair> pairs = [
    // RFC 3548 examples
    new(((@string)(new byte[]{0x14, 0xfb, 0x9c, 0x03, 0xd9, 0x7e})), "FPucA9l+"),
    new(((@string)(new byte[]{0x14, 0xfb, 0x9c, 0x03, 0xd9})), "FPucA9k="),
    new(((@string)(new byte[]{0x14, 0xfb, 0x9c, 0x03})), "FPucAw=="),

    // RFC 4648 examples
    new("", ""),
    new("f", "Zg=="),
    new("fo", "Zm8="),
    new("foo", "Zm9v"),
    new("foob", "Zm9vYg=="),
    new("fooba", "Zm9vYmE="),
    new("foobar", "Zm9vYmFy"),

    // Wikipedia examples
    new("sure.", "c3VyZS4="),
    new("sure", "c3VyZQ=="),
    new("sur", "c3Vy"),
    new("su", "c3U="),
    new("leasure.", "bGVhc3VyZS4="),
    new("easure.", "ZWFzdXJlLg=="),
    new("asure.", "YXN1cmUu"),
    new("sure.", "c3VyZS4=")
];

// Do nothing to a reference base64 string (leave in standard format)
internal static @string stdRef(@string @ref) {
    return @ref;
}

// Convert a reference string to URL-encoding
internal static @string urlRef(@string @ref) {
    @ref = strings.ReplaceAll(@ref, "+", "-");
    @ref = strings.ReplaceAll(@ref, "/", "_");
    return @ref;
}

// Convert a reference string to raw, unpadded format
internal static @string rawRef(@string @ref) {
    return strings.TrimRight(@ref, "=");
}

// Both URL and unpadding conversions
internal static @string rawURLRef(@string @ref) {
    return rawRef(urlRef(@ref));
}

internal static ref readonly @string encodeStd => ref encodeStdᵇ;
private static readonly @string encodeStdᵇ = "ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz0123456789+/"u8;

// A nonstandard encoding with a funny padding character, for testing
internal static ж<global::go.encoding.base64_package.Encoding> funnyEncoding;
internal static void initᴛfunnyEncoding() { funnyEncoding = NewEncoding(encodeStd).WithPadding((rune)'@'); }

internal static @string funnyRef(@string @ref) {
    return strings.ReplaceAll(@ref, "=", "@");
}

internal partial struct encodingTest {
    internal ж<global::go.encoding.base64_package.Encoding> enc; // Encoding to test
    internal Func<@string, @string> conv;                        // Reference string converter
}

internal static slice<encodingTest> encodingTests;
internal static void initᴛencodingTests() { encodingTests = slice<encodingTest>([
    new(StdEncoding, stdRef),
    new(URLEncoding, urlRef),
    new(RawStdEncoding, rawRef),
    new(RawURLEncoding, rawURLRef),
    new(funnyEncoding, funnyRef),
    new(StdEncoding.Strict(), stdRef),
    new(URLEncoding.Strict(), urlRef),
    new(RawStdEncoding.Strict(), rawRef),
    new(RawURLEncoding.Strict(), rawURLRef),
    new(funnyEncoding.Strict(), funnyRef)
]); }

internal static testpair bigtest = new testpair(
    "Twas brillig, and the slithy toves",
    "VHdhcyBicmlsbGlnLCBhbmQgdGhlIHNsaXRoeSB0b3Zlcw=="
);

internal static bool testEqual(ж<testing.T> t, @string msg, params ꓸꓸꓸany argsʗp) {
    var args = argsʗp.sslice();

    t.Helper();
    if (!AreEqual(args[len(args) - 2], args[len(args) - 1])) {
        t.Errorf(msg, args.ꓸꓸꓸ);
        return false;
    }
    return true;
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static ref readonly @string encodeQQWantQˢ => ref encodeQQWantQˢᵇ;
private static readonly @string encodeQQWantQˢᵇ = "Encode(%q) = %q, want %q"u8;
internal static ref readonly @string appendEncodeLeadQQWantQˢ => ref appendEncodeLeadQQWantQˢᵇ;
private static readonly @string appendEncodeLeadQQWantQˢᵇ = @"AppendEncode(""lead"", %q) = %q, want %q"u8;

public static void TestEncode(ж<testing.T> t) {
    foreach (var (_, p) in range(pairs)) {
        foreach (var (_, tt) in range(encodingTests)) {
            @string got = tt.enc.EncodeToString(slice<byte>(p.decoded));
            testEqual(t, encodeQQWantQˢ, p.decoded, got, tt.conv(p.encoded));
            var dst = tt.enc.AppendEncode(slice<byte>("lead"u8), slice<byte>(p.decoded));
            testEqual(t, appendEncodeLeadQQWantQˢ, p.decoded, (@string)dst, "lead" + tt.conv(p.encoded));
        }
    }
}

public static void TestEncoder(ж<testing.T> t) {
    foreach (var (_, p) in range(pairs)) {
        var bb = Ꮡ(new strings.Builder());
        var encoder = NewEncoder(StdEncoding, new base64_test_package.strings_BuilderжWriter(bb));
        encoder.Write(slice<byte>(p.decoded));
        encoder.Close();
        testEqual(t, encodeQQWantQˢ, p.decoded, bb.String(), p.encoded);
    }
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static ref readonly @string writeQGaveErrorVWantVˢ => ref writeQGaveErrorVWantVˢᵇ;
private static readonly @string writeQGaveErrorVWantVˢᵇ = "Write(%q) gave error %v, want %v"u8;
internal static ref readonly @string writeQGaveLengthVWantVˢ => ref writeQGaveLengthVWantVˢᵇ;
private static readonly @string writeQGaveLengthVWantVˢᵇ = "Write(%q) gave length %v, want %v"u8;
internal static ref readonly @string closeGaveErrorVWantVˢ => ref closeGaveErrorVWantVˢᵇ;
private static readonly @string closeGaveErrorVWantVˢᵇ = "Close gave error %v, want %v"u8;
internal static ref readonly @string encodingDOfQQWantQˢ => ref encodingDOfQQWantQˢᵇ;
private static readonly @string encodingDOfQQWantQˢᵇ = "Encoding/%d of %q = %q, want %q"u8;

public static void TestEncoderBuffering(ж<testing.T> t) {
    var input = slice<byte>(bigtest.decoded);
    for (nint bs = 1; bs <= 12; bs++) {
        var bb = Ꮡ(new strings.Builder());
        var encoder = NewEncoder(StdEncoding, new base64_test_package.strings_BuilderжWriter(bb));
        for (nint pos = 0; pos < len(input); pos += bs) {
            nint end = pos + bs;
            if (end > len(input)) {
                end = len(input);
            }
            var (n, errΔ1) = encoder.Write(input.slice(pos, end));
            testEqual(t, writeQGaveErrorVWantVˢ, input.slice(pos, end), errΔ1, (error)default);
            testEqual(t, writeQGaveLengthVWantVˢ, input.slice(pos, end), n, end - pos);
        }
        var err = encoder.Close();
        testEqual(t, closeGaveErrorVWantVˢ, err, (error)default);
        testEqual(t, encodingDOfQQWantQˢ, bs, bigtest.decoded, bb.String(), bigtest.encoded);
    }
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static ref readonly @string decodeQErrorVWantVˢ => ref decodeQErrorVWantVˢᵇ;
private static readonly @string decodeQErrorVWantVˢᵇ = "Decode(%q) = error %v, want %v"u8;
internal static ref readonly @string decodeQLengthVWantVˢ => ref decodeQLengthVWantVˢᵇ;
private static readonly @string decodeQLengthVWantVˢᵇ = "Decode(%q) = length %v, want %v"u8;
internal static ref readonly @string decodeQQWantQˢ => ref decodeQQWantQˢᵇ;
private static readonly @string decodeQQWantQˢᵇ = "Decode(%q) = %q, want %q"u8;
internal static ref readonly @string decodeStringQErrorVWantVˢ => ref decodeStringQErrorVWantVˢᵇ;
private static readonly @string decodeStringQErrorVWantVˢᵇ = "DecodeString(%q) = error %v, want %v"u8;
internal static ref readonly @string decodeStringQQWantQˢ => ref decodeStringQQWantQˢᵇ;
private static readonly @string decodeStringQQWantQˢᵇ = "DecodeString(%q) = %q, want %q"u8;
internal static ref readonly @string appendDecodeQErrorVWantVˢ => ref appendDecodeQErrorVWantVˢᵇ;
private static readonly @string appendDecodeQErrorVWantVˢᵇ = "AppendDecode(%q) = error %v, want %v"u8;
internal static ref readonly @string appendDecodeLeadQQWantQˢ => ref appendDecodeLeadQQWantQˢᵇ;
private static readonly @string appendDecodeLeadQQWantQˢᵇ = @"AppendDecode(""lead"", %q) = %q, want %q"u8;
internal static ref readonly @string appendDecodeQQWantQˢ => ref appendDecodeQQWantQˢᵇ;
private static readonly @string appendDecodeQQWantQˢᵇ = @"AppendDecode("""", %q) = %q, want %q"u8;

public static void TestDecode(ж<testing.T> t) {
    foreach (var (_, p) in range(pairs)) {
        foreach (var (_, tt) in range(encodingTests)) {
            @string encoded = tt.conv(p.encoded);
            var dbuf = new slice<byte>(tt.enc.DecodedLen(len(encoded)));
            var (count, err) = tt.enc.Decode(dbuf, slice<byte>(encoded));
            testEqual(t, decodeQErrorVWantVˢ, encoded, err, (error)default);
            testEqual(t, decodeQLengthVWantVˢ, encoded, count, len(p.decoded));
            testEqual(t, decodeQQWantQˢ, encoded, (@string)(dbuf.slice(0, count)), p.decoded);

            (dbuf, err) = tt.enc.DecodeString(encoded);
            testEqual(t, decodeStringQErrorVWantVˢ, encoded, err, (error)default);
            testEqual(t, decodeStringQQWantQˢ, encoded, (@string)dbuf, p.decoded);

            (var dst, err) = tt.enc.AppendDecode(slice<byte>("lead"u8), slice<byte>(encoded));
            testEqual(t, appendDecodeQErrorVWantVˢ, p.encoded, err, (error)default);
            testEqual(t, appendDecodeLeadQQWantQˢ, p.encoded, (@string)dst, "lead" + p.decoded);

            (var dst2, err) = tt.enc.AppendDecode(dst.slice(0, 0, len(p.decoded)), slice<byte>(encoded));
            testEqual(t, appendDecodeQErrorVWantVˢ, p.encoded, err, (error)default);
            testEqual(t, appendDecodeQQWantQˢ, p.encoded, (@string)dst2, p.decoded);
            if (len(dst) > 0 && len(dst2) > 0 && Ꮡelem(dst, 0) != Ꮡelem(dst2, 0)) {
                t.Errorf("unexpected capacity growth: got %d, want %d", cap(dst2), cap(dst));
            }
        }
    }
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static readonly object readFailedˢ = (@string)"Read failed"u8;
internal static ref readonly @string readFromQLengthVWantVˢ => ref readFromQLengthVWantVˢᵇ;
private static readonly @string readFromQLengthVWantVˢᵇ = "Read from %q = length %v, want %v"u8;
internal static ref readonly @string decodingOfQQWantQˢ => ref decodingOfQQWantQˢᵇ;
private static readonly @string decodingOfQQWantQˢᵇ = "Decoding of %q = %q, want %q"u8;
internal static ref readonly @string readFromQVWantVˢ => ref readFromQVWantVˢᵇ;
private static readonly @string readFromQVWantVˢᵇ = "Read from %q = %v, want %v"u8;

public static void TestDecoder(ж<testing.T> t) {
    foreach (var (_, p) in range(pairs)) {
        var decoder = NewDecoder(StdEncoding, new base64_test_package.strings_ReaderжReader(strings.NewReader(p.encoded)));
        var dbuf = new slice<byte>(StdEncoding.DecodedLen(len(p.encoded)));
        var (count, err) = decoder.Read(dbuf);
        if (err != nil && !AreEqual(err, io.EOF)) {
            t.Fatal(readFailedˢ, err);
        }
        testEqual(t, readFromQLengthVWantVˢ, p.encoded, count, len(p.decoded));
        testEqual(t, decodingOfQQWantQˢ, p.encoded, (@string)(dbuf.slice(0, count)), p.decoded);
        if (!AreEqual(err, io.EOF)) {
            (_, err) = decoder.Read(dbuf);
        }
        testEqual(t, readFromQVWantVˢ, p.encoded, err, io.EOF);
    }
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static ref readonly @string decodingDOfQQWantQˢ => ref decodingDOfQQWantQˢᵇ;
private static readonly @string decodingDOfQQWantQˢᵇ = "Decoding/%d of %q = %q, want %q"u8;

public static void TestDecoderBuffering(ж<testing.T> t) {
    for (nint bs = 1; bs <= 12; bs++) {
        var decoder = NewDecoder(StdEncoding, new base64_test_package.strings_ReaderжReader(strings.NewReader(bigtest.encoded)));
        var buf = new slice<byte>(len(bigtest.decoded) + 12);
        nint total = default;
        nint n = default;
        error err = default;
        for (total = 0; total < len(bigtest.decoded) && err == nil; ) {
            (n, err) = decoder.Read(buf.slice(total, total + bs));
            total += n;
        }
        if (err != nil && !AreEqual(err, io.EOF)) {
            t.Errorf("Read from %q at pos %d = %d, unexpected error %v", bigtest.encoded, total, n, err);
        }
        testEqual(t, decodingDOfQQWantQˢ, bs, bigtest.encoded, (@string)(buf.slice(0, total)), bigtest.decoded);
    }
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static readonly object decoderWronglyDetectedˢ = (@string)"Decoder wrongly detected corruption in"u8;
internal static ref readonly @string corruptionInQAtOffsetVˢ => ref corruptionInQAtOffsetVˢᵇ;
private static readonly @string corruptionInQAtOffsetVˢᵇ = "Corruption in %q at offset %v, want %v"u8;
internal static readonly object decoderFailedToDetectˢ = (@string)"Decoder failed to detect corruption in"u8;

internal partial struct TestDecodeCorrupt_testCases /*dyn*/ {
    internal @string input;
    internal nint offset; // -1 means no corruption.
}

public static void TestDecodeCorrupt(ж<testing.T> t) {
    var testCases = slice<TestDecodeCorrupt_testCases>([
        new("", -1),
        new("\n", -1),
        new("AAA=\n", -1),
        new("AAAA\n", -1),
        new("!!!!", 0),
        new("====", 0),
        new("x===", 1),
        new("=AAA", 0),
        new("A=AA", 1),
        new("AA=A", 2),
        new("AA==A", 4),
        new("AAA=AAAA", 4),
        new("AAAAA", 4),
        new("AAAAAA", 4),
        new("A=", 1),
        new("A==", 1),
        new("AA=", 3),
        new("AA==", -1),
        new("AAA=", -1),
        new("AAAA", -1),
        new("AAAAAA=", 7),
        new("YWJjZA=====", 8),
        new("A!\n", 1),
        new("A=\n", 1)
    ]);
    foreach (var (_, tc) in range(testCases)) {
        var dbuf = new slice<byte>(StdEncoding.DecodedLen(len(tc.input)));
        var (_, err) = StdEncoding.Decode(dbuf, slice<byte>(tc.input));
        if (tc.offset == -1) {
            if (err != nil) {
                t.Error(decoderWronglyDetectedˢ, tc.input);
            }
            continue;
        }
        switch (err.type()) {
        case CorruptInputError errΔ1: {
            testEqual(t, corruptionInQAtOffsetVˢ, tc.input, (nint)errΔ1, tc.offset);
            break;
        }
        default: {
            var errΔ1 = err;
            t.Error(decoderFailedToDetectˢ, tc);
            break;
        }}
    }
}

public static void TestDecodeBounds(ж<testing.T> t) {
    GoFrame ᒐ = default;
    try {
        array<byte> buf = new(32);
        @string s = StdEncoding.EncodeToString(buf[..]);
        defer([MethodImpl(MethodImplOptions.NoInlining)] () => {
            var errΔ1 = recover(); if (errΔ1 != nil) {
                t.Fatalf("Decode panicked unexpectedly: %v\n%s", errΔ1, debug.Stack());
            }
        }, ref ᒐ);
        var (n, err) = StdEncoding.Decode(buf[..], slice<byte>(s));
        if (n != len(buf) || err != nil) {
            t.Fatalf("StdEncoding.Decode = %d, %v, want %d, nil", n, err, len(buf));
        }
    }
    catch (Exception ᒐex) when (GoFrame.IsPanic(ᒐex, out PanicException? ᒐp)) { GoFrame.Capture(ᒐp); }
    finally { ᒐ.Run(); }
}

internal partial struct TestEncodedLen_test /*dyn*/ {
    internal ж<global::go.encoding.base64_package.Encoding> enc;
    internal nint n;
    internal int64 want;
}

public static void TestEncodedLen(ж<testing.T> t) {
    var testsᴷ = new stackAppend<TestEncodedLen_test>();

    var tests = slice<TestEncodedLen_test>([
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
    ]);
    // check overflow
    switch (strconv.IntSize) {
    case var ᴛ when ᴛ == 32: {
        tests = append(tests, new TestEncodedLen_test(RawStdEncoding, unchecked((nint)(1152921504606846976L)), 357913942));
        tests = append(tests, new TestEncodedLen_test(RawStdEncoding, unchecked((nint)(6917529027641081855L)), math.MaxInt));
        break;
    }
    case var ᴛ when ᴛ == 64: {
        tests = append(testsᴷ, tests, new TestEncodedLen_test(RawStdEncoding, unchecked((nint)(1152921504606846976L)), 1537228672809129302L));
        tests = append(tests, new TestEncodedLen_test(RawStdEncoding, unchecked((nint)(6917529027641081855L)), math.MaxInt));
        break;
    }}
    foreach (var (_, tt) in range(tests)) {
        nint got = tt.enc.EncodedLen(tt.n); if ((int64)got != tt.want) {
            t.Errorf("EncodedLen(%d): got %d, want %d", tt.n, got, tt.want);
        }
    }
}

internal partial struct TestDecodedLen_test /*dyn*/ {
    internal ж<global::go.encoding.base64_package.Encoding> enc;
    internal nint n;
    internal int64 want;
}

public static void TestDecodedLen(ж<testing.T> t) {
    var testsᴷ = new stackAppend<TestDecodedLen_test>();

    var tests = slice<TestDecodedLen_test>([
        new(RawStdEncoding, 0, 0),
        new(RawStdEncoding, 2, 1),
        new(RawStdEncoding, 3, 2),
        new(RawStdEncoding, 4, 3),
        new(RawStdEncoding, 10, 7),
        new(StdEncoding, 0, 0),
        new(StdEncoding, 4, 3),
        new(StdEncoding, 8, 6)
    ]);
    // check overflow
    switch (strconv.IntSize) {
    case var ᴛ when ᴛ == 32: {
        tests = append(tests, new TestDecodedLen_test(RawStdEncoding, unchecked((nint)(1537228672809129302L)), 268435456));
        tests = append(tests, new TestDecodedLen_test(RawStdEncoding, math.MaxInt, 1610612735));
        break;
    }
    case var ᴛ when ᴛ == 64: {
        tests = append(testsᴷ, tests, new TestDecodedLen_test(RawStdEncoding, unchecked((nint)(1537228672809129302L)), 1152921504606846976L));
        tests = append(tests, new TestDecodedLen_test(RawStdEncoding, math.MaxInt, 6917529027641081855L));
        break;
    }}
    foreach (var (_, tt) in range(tests)) {
        nint got = tt.enc.DecodedLen(tt.n); if ((int64)got != tt.want) {
            t.Errorf("DecodedLen(%d): got %d, want %d", tt.n, got, tt.want);
        }
    }
}

// Hoisted Go string constant (single allocation; Go keeps it in RODATA)
internal static ref readonly @string alphaᶜ => ref alphaᶜᵇ;
private static readonly @string alphaᶜᵇ = "0123456789abcdefghijklmnopqrstuvwxyzABCDEFGHIJKLMNOPQRSTUVWXYZ"u8;

public static void TestBig(ж<testing.T> t) {
    nint n = 3 * 1000 + 1;
    var raw = new slice<byte>(n);
    @string alpha = alphaᶜ;
    for (nint i = 0; i < n; i++) {
        raw[i] = alpha[i % len(alpha)];
    }
    var encoded = @new<bytes.Buffer>();
    var w = NewEncoder(StdEncoding, new base64_test_package.bytes_BufferжWriter(encoded));
    var (nn, err) = w.Write(raw);
    if (nn != n || err != nil) {
        t.Fatalf("Encoder.Write(raw) = %d, %v want %d, nil", nn, err, n);
    }
    err = w.Close();
    if (err != nil) {
        t.Fatalf("Encoder.Close() = %v want nil", err);
    }
    (var decoded, err) = io.ReadAll(NewDecoder(StdEncoding, new base64_test_package.bytes_BufferжReader(encoded)));
    if (err != nil) {
        t.Fatalf("io.ReadAll(NewDecoder(...)): %v", err);
    }

    if (!bytes.Equal(raw, decoded)) {
        nint i = default;
        for (i = 0; i < len(decoded) && i < len(raw); i++) {
            if (decoded[i] != raw[i]) {
                break;
            }
        }
        t.Errorf("Decode(Encode(%d-byte string)) failed at offset %d", n, i);
    }
}

// Hoisted Go string constant (single allocation; Go keeps it in RODATA)
internal static ref readonly @string expectedᶜ => ref expectedᶜᵇ;
private static readonly @string expectedᶜᵇ = "sure"u8;

public static void TestNewLineCharacters(ж<testing.T> t) {
    // Each of these should decode to the string "sure", without errors.
    @string expected = expectedᶜ;
    var examples = slice<@string>([
        "c3VyZQ==",
        "c3VyZQ==\r",
        "c3VyZQ==\n",
        "c3VyZQ==\r\n",
        "c3VyZ\r\nQ==",
        "c3V\ryZ\nQ==",
        "c3V\nyZ\rQ==",
        "c3VyZ\nQ==",
        "c3VyZQ\n==",
        "c3VyZQ=\n=",
        "c3VyZQ=\r\n\r\n="
    ]);
    foreach (var (_, e) in range(examples)) {
        var (buf, err) = StdEncoding.DecodeString(e);
        if (err != nil) {
            t.Errorf("Decode(%q) failed: %v", e, err);
            continue;
        }
        @string s = (@string)buf; if (s != expected) {
            t.Errorf("Decode(%q) = %q, want %q", e, s, expected);
        }
    }
}

internal partial struct nextRead {
    internal nint n;    // bytes to return
    internal error err; // error to return
}

// faultInjectReader returns data from source, rate-limited
// and with the errors as written to nextc.
internal partial struct faultInjectReader {
    internal @string source;
    internal /*<-*/channel<nextRead> nextc = /*<-*/channel<nextRead>.RecvOnly;
}

internal static (nint, error) Read(this ref faultInjectReader r, slice<byte> p) {
    var nr = ᐸꟷ(r.nextc);
    if (len(p) > nr.n) {
        p = p.slice(0, nr.n);
    }
    nint n = copy(p, r.source);
    r.source = r.source.slice(n);
    return (n, nr.err);
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static ref readonly @string myErrorˢ => ref myErrorˢᵇ;
private static readonly @string myErrorˢᵇ = "my error"u8;

// tests that we don't ignore errors from our underlying reader
public static partial void TestDecoderIssue3577(ж<testing.T> t) {
    var next = new channel<nextRead>(10);
    var wantErr = errors.New(myErrorˢ);
    next.ᐸꟷ(new nextRead(5, default));
    next.ᐸꟷ(new nextRead(10, wantErr));
    next.ᐸꟷ(new nextRead(0, wantErr));
    var d = NewDecoder(StdEncoding, new base64_internal_test_package.faultInjectReaderжReader(Ꮡ(new faultInjectReader(
        source: "VHdhcyBicmlsbGlnLCBhbmQgdGhlIHNsaXRoeSB0b3Zlcw==", // twas brillig...
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
            t.Errorf("got error %v; want %v", err, wantErr);
        }
        break;
    }
    case 1 when selᴛ2.ꟷᐳ(out _): {
        t.Errorf("timeout; Decoder blocked without returning an error");
        break;
    }}
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static ref readonly @string cpEat8aaaefˢ => ref cpEat8aaaefˢᵇ;
private static readonly @string cpEat8aaaefˢᵇ = """
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

"""u8.Raw();
internal static readonly object decodedResultsNotEqualˢ = (@string)"Decoded results not equal"u8;

public static void TestDecoderIssue4779(ж<testing.T> t) {
    @string encoded = cpEat8aaaefˢ;
    @string encodedShort = strings.ReplaceAll(encoded, "\n", ""u8);

    var dec = NewDecoder(StdEncoding, new base64_test_package.strings_ReaderжReader(strings.NewReader(encoded)));
    var (res1, err) = io.ReadAll(dec);
    if (err != nil) {
        t.Errorf("ReadAll failed: %v", err);
    }

    dec = NewDecoder(StdEncoding, new base64_test_package.strings_ReaderжReader(strings.NewReader(encodedShort)));
    slice<byte> res2 = default;
    (res2, err) = io.ReadAll(dec);
    if (err != nil) {
        t.Errorf("ReadAll failed: %v", err);
    }

    if (!bytes.Equal(res1, res2)) {
        t.Error(decodedResultsNotEqualˢ);
    }
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static ref readonly @string ywJjZAˢ => ref ywJjZAˢᵇ;
private static readonly @string ywJjZAˢᵇ = "YWJjZA====="u8;

public static void TestDecoderIssue7733(ж<testing.T> t) {
    var (s, err) = StdEncoding.DecodeString(ywJjZAˢ);
    var want = (global::go.encoding.base64_package.CorruptInputError)8;
    if (!reflect.DeepEqual(want, err)) {
        t.Errorf("Error = %v; want CorruptInputError(8)", err);
    }
    if (((sstring)s) != "abcd"u8) {
        t.Errorf("DecodeString = %q; want abcd", s);
    }
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static ref readonly @string wvLTlMrX9NpYDQlEIFlnDBˢ => ref wvLTlMrX9NpYDQlEIFlnDBˢᵇ;
private static readonly @string wvLTlMrX9NpYDQlEIFlnDBˢᵇ = "WvLTlMrX9NpYDQlEIFlnDB=="u8;
internal static ref readonly @string wvLTlMrX9NpYDQlEIFlnDAˢ => ref wvLTlMrX9NpYDQlEIFlnDAˢᵇ;
private static readonly @string wvLTlMrX9NpYDQlEIFlnDAˢᵇ = "WvLTlMrX9NpYDQlEIFlnDA=="u8;

public static void TestDecoderIssue15656(ж<testing.T> t) {
    var (_, err) = StdEncoding.Strict().DecodeString(wvLTlMrX9NpYDQlEIFlnDBˢ);
    var want = (global::go.encoding.base64_package.CorruptInputError)22;
    if (!reflect.DeepEqual(want, err)) {
        t.Errorf("Error = %v; want CorruptInputError(22)", err);
    }
    (_, err) = StdEncoding.Strict().DecodeString(wvLTlMrX9NpYDQlEIFlnDAˢ);
    if (err != nil) {
        t.Errorf("Error = %v; want nil", err);
    }
    (_, err) = StdEncoding.DecodeString(wvLTlMrX9NpYDQlEIFlnDBˢ);
    if (err != nil) {
        t.Errorf("Error = %v; want nil", err);
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

public static void BenchmarkDecodeString(ж<testing.B> b) {
    var sizes = slice<nint>([2, 4, 8, 64, 8192]);
    void benchFunc(ж<testing.B> bΔ1, nint benchSize) {
        @string data = StdEncoding.EncodeToString(new slice<byte>(benchSize));
        bΔ1.SetBytes((int64)len(data));
        bΔ1.ResetTimer();
        for (nint i = 0; i < (~bΔ1).N; i++) {
            StdEncoding.DecodeString(data);
        }
    }
    foreach (var (_, size) in range(sizes)) {
        var benchFuncʗ1 = benchFunc;
        b.Run(fmt.Sprintf("%d"u8, size), (ж<testing.B> bΔ2) => {
            benchFuncʗ1(bΔ2, size);
        });
    }
}

public static void BenchmarkNewEncoding(ж<testing.B> Ꮡb) {
    ref var b = ref Ꮡb.DerefOrNull();

    b.SetBytes((int64)len(new Encoding().decodeMap));
    for (nint i = 0; i < b.N; i++) {
        var e = NewEncoding(encodeStd);
        foreach (var (_, v) in range(e.decodeMap.ΔRangeSnapshot())) {
            _ = v;
        }
    }
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static ref readonly @string aaaaaaˢ => ref aaaaaaˢᵇ;
private static readonly @string aaaaaaˢᵇ = "AAAAAA"u8;

public static void TestDecoderRaw(ж<testing.T> t) {
    @string source = aaaaaaˢ;
    var want = slice<byte>([0, 0, 0, 0]);

    // Direct.
    var (dec1, err) = RawURLEncoding.DecodeString(source);
    if (err != nil || !bytes.Equal(dec1, want)) {
        t.Errorf("RawURLEncoding.DecodeString(%q) = %x, %v, want %x, nil", source, dec1, err, want);
    }

    // Through reader. Used to fail.
    var r = NewDecoder(RawURLEncoding, new base64_test_package.bytes_ReaderжReader(bytes.NewReader(slice<byte>(source))));
    (var dec2, err) = io.ReadAll(io.LimitReader(r, 100));
    if (err != nil || !bytes.Equal(dec2, want)) {
        t.Errorf("reading NewDecoder(RawURLEncoding, %q) = %x, %v, want %x, nil", source, dec2, err, want);
    }

    // Should work with padding.
    r = NewDecoder(URLEncoding, new base64_test_package.bytes_ReaderжReader(bytes.NewReader(slice<byte>(source + "=="))));
    (var dec3, err) = io.ReadAll(r);
    if (err != nil || !bytes.Equal(dec3, want)) {
        t.Errorf("reading NewDecoder(URLEncoding, %q) = %x, %v, want %x, nil", source + "==", dec3, err, want);
    }
}

} // end base64_internal_test_package
