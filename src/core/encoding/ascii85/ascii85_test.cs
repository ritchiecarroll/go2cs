// Copyright 2009 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.
namespace go.encoding;

using bytes = bytes_package;
using io = io_package;
using strings = strings_package;
using testing = testing_package;
using static go.encoding.ascii85_package;
using ꓸꓸꓸany = Span<any>;

partial class ascii85_internal_test_package {

internal partial struct testpair {
    internal @string decoded, encoded;
}

internal static testpair bigtest = new testpair(
    "Man is distinguished, not only by his reason, but by this singular passion from "u8 + "other animals, which is a lust of the mind, that by a perseverance of delight in "u8 + "the continued and indefatigable generation of knowledge, exceeds the short "u8 + "vehemence of any carnal pleasure."u8,
    "9jqo^BlbD-BleB1DJ+*+F(f,q/0JhKF<GL>Cj@.4Gp$d7F!,L7@<6@)/0JDEF<G%<+EV:2F!,\n"u8 + "O<DJ+*.@<*K0@<6L(Df-\\0Ec5e;DffZ(EZee.Bl.9pF\"AGXBPCsi+DGm>@3BB/F*&OCAfu2/AKY\n"u8 + "i(DIb:@FD,*)+C]U=@3BN#EcYf8ATD3s@q?d$AftVqCh[NqF<G:8+EV:.+Cf>-FD5W8ARlolDIa\n"u8 + "l(DId<j@<?3r@:F%a+D58'ATD4$Bl@l3De:,-DJs`8ARoFb/0JMK@qB4^F!,R<AKZ&-DfTqBG%G\n"u8 + ">uD.RTpAKYo'+CT/5+Cei#DII?(E,9)oF*2M7/c\n"u8
);

internal static slice<testpair> pairs = [
    // Encode returns 0 when len(src) is 0
    new(
        "",
        ""
    ),
    // Wikipedia example
    bigtest,
    // Special case when shortening !!!!! to z.
    new(
        "\u0000\u0000\u0000\u0000",
        "z"
    )
];

internal static bool testEqual(ж<testing.T> t, @string msg, params ꓸꓸꓸany argsʗp) {
    var args = argsʗp.sslice();

    t.Helper();
    if (!AreEqual(args[len(args) - 2], args[len(args) - 1])) {
        t.Errorf(msg, args.ꓸꓸꓸ);
        return false;
    }
    return true;
}

internal static @string strip85(@string s) {
    var t = new slice<byte>(len(s));
    nint w = 0;
    for (nint r = 0; r < len(s); r++) {
        var c = s[r];
        if (c > ' ') {
            t[w] = c;
            w++;
        }
    }
    return (@string)(t.slice(0, w));
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static ref readonly @string encodeQQWantQˢ => ref encodeQQWantQˢᵇ;
private static readonly @string encodeQQWantQˢᵇ = "Encode(%q) = %q, want %q"u8;

public static void TestEncode(ж<testing.T> t) {
    foreach (var (_, p) in range(pairs)) {
        var buf = new slice<byte>(MaxEncodedLen(len(p.decoded)));
        nint n = Encode(buf, slice<byte>(p.decoded));
        buf = buf.slice(0, n);
        testEqual(t, encodeQQWantQˢ, p.decoded, strip85((@string)buf), strip85(p.encoded));
    }
}

public static void TestEncoder(ж<testing.T> t) {
    foreach (var (_, p) in range(pairs)) {
        var bb = Ꮡ(new strings.Builder());
        var encoder = NewEncoder(new ascii85_internal_test_package.strings_BuilderжWriter(bb));
        encoder.Write(slice<byte>(p.decoded));
        encoder.Close();
        testEqual(t, encodeQQWantQˢ, p.decoded, strip85(bb.String()), strip85(p.encoded));
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
        var encoder = NewEncoder(new ascii85_internal_test_package.strings_BuilderжWriter(bb));
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
        testEqual(t, encodingDOfQQWantQˢ, bs, bigtest.decoded, strip85(bb.String()), strip85(bigtest.encoded));
    }
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static ref readonly @string decodeQErrorVWantVˢ => ref decodeQErrorVWantVˢᵇ;
private static readonly @string decodeQErrorVWantVˢᵇ = "Decode(%q) = error %v, want %v"u8;
internal static ref readonly @string decodeQNsrcVWantVˢ => ref decodeQNsrcVWantVˢᵇ;
private static readonly @string decodeQNsrcVWantVˢᵇ = "Decode(%q) = nsrc %v, want %v"u8;
internal static ref readonly @string decodeQNdstVWantVˢ => ref decodeQNdstVWantVˢᵇ;
private static readonly @string decodeQNdstVWantVˢᵇ = "Decode(%q) = ndst %v, want %v"u8;
internal static ref readonly @string decodeQQWantQˢ => ref decodeQQWantQˢᵇ;
private static readonly @string decodeQQWantQˢᵇ = "Decode(%q) = %q, want %q"u8;

public static void TestDecode(ж<testing.T> t) {
    foreach (var (_, p) in range(pairs)) {
        var dbuf = new slice<byte>(4 * len(p.encoded));
        var (ndst, nsrc, err) = Decode(dbuf, slice<byte>(p.encoded), true);
        testEqual(t, decodeQErrorVWantVˢ, p.encoded, err, (error)default);
        testEqual(t, decodeQNsrcVWantVˢ, p.encoded, nsrc, len(p.encoded));
        testEqual(t, decodeQNdstVWantVˢ, p.encoded, ndst, len(p.decoded));
        testEqual(t, decodeQQWantQˢ, p.encoded, (@string)(dbuf.slice(0, ndst)), p.decoded);
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
        var decoder = NewDecoder(new ascii85_internal_test_package.strings_ReaderжReader(strings.NewReader(p.encoded)));
        var (dbuf, err) = io.ReadAll(decoder);
        if (err != nil) {
            t.Fatal(readFailedˢ, err);
        }
        testEqual(t, readFromQLengthVWantVˢ, p.encoded, len(dbuf), len(p.decoded));
        testEqual(t, decodingOfQQWantQˢ, p.encoded, (@string)dbuf, p.decoded);
        if (err != nil) {
            testEqual(t, readFromQVWantVˢ, p.encoded, err, io.EOF);
        }
    }
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static ref readonly @string decodingDOfQQWantQˢ => ref decodingDOfQQWantQˢᵇ;
private static readonly @string decodingDOfQQWantQˢᵇ = "Decoding/%d of %q = %q, want %q"u8;

public static void TestDecoderBuffering(ж<testing.T> t) {
    for (nint bs = 1; bs <= 12; bs++) {
        var decoder = NewDecoder(new ascii85_internal_test_package.strings_ReaderжReader(strings.NewReader(bigtest.encoded)));
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
internal static ref readonly @string corruptionInQAtOffsetVˢ => ref corruptionInQAtOffsetVˢᵇ;
private static readonly @string corruptionInQAtOffsetVˢᵇ = "Corruption in %q at offset %v, want %v"u8;
internal static readonly object decoderFailedToDetectˢ = (@string)"Decoder failed to detect corruption in"u8;

internal partial struct TestDecodeCorrupt_corrupt /*dyn*/ {
    internal @string e;
    internal nint p;
}

public static void TestDecodeCorrupt(ж<testing.T> t) {
    var examples = slice<TestDecodeCorrupt_corrupt>([
        new("v", 0),
        new("!z!!!!!!!!!", 1)
    ]);

    foreach (var (_, e) in range(examples)) {
        var dbuf = new slice<byte>(4 * len(e.e));
        var (_, _, err) = Decode(dbuf, slice<byte>(e.e), true);
        switch (err.type()) {
        case CorruptInputError errΔ1: {
            testEqual(t, corruptionInQAtOffsetVˢ, e.e, (nint)errΔ1, e.p);
            break;
        }
        default: {
            var errΔ1 = err;
            t.Error(decoderFailedToDetectˢ, e);
            break;
        }}
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
    var w = NewEncoder(new ascii85_internal_test_package.bytes_BufferжWriter(encoded));
    var (nn, err) = w.Write(raw);
    if (nn != n || err != nil) {
        t.Fatalf("Encoder.Write(raw) = %d, %v want %d, nil", nn, err, n);
    }
    err = w.Close();
    if (err != nil) {
        t.Fatalf("Encoder.Close() = %v want nil", err);
    }
    (var decoded, err) = io.ReadAll(NewDecoder(new ascii85_internal_test_package.bytes_BufferжReader(encoded)));
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

public static void TestDecoderInternalWhitespace(ж<testing.T> t) {
    @string s = strings.Repeat(" ", 2048) + "z"u8;
    var (decoded, err) = io.ReadAll(NewDecoder(new ascii85_internal_test_package.strings_ReaderжReader(strings.NewReader(s))));
    if (err != nil) {
        t.Errorf("Decode gave error %v", err);
    }
    var want = slice<byte>("\u0000\u0000\u0000\u0000"u8); if (!bytes.Equal(want, decoded)) {
        t.Errorf("Decode failed: got %v, want %v", decoded, want);
    }
}

} // end ascii85_internal_test_package
