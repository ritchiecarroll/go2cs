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

[GoType] internal partial struct testpair {
    internal @string decoded, encoded;
}

internal static testpair bigtest = new testpair(
    "Man is distinguished, not only by his reason, but by this singular passion from "u8 + "other animals, which is a lust of the mind, that by a perseverance of delight in "u8 + "the continued and indefatigable generation of knowledge, exceeds the short "u8 + "vehemence of any carnal pleasure."u8,
    "9jqo^BlbD-BleB1DJ+*+F(f,q/0JhKF<GL>Cj@.4Gp$d7F!,L7@<6@)/0JDEF<G%<+EV:2F!,\n"u8 + "O<DJ+*.@<*K0@<6L(Df-\\0Ec5e;DffZ(EZee.Bl.9pF\"AGXBPCsi+DGm>@3BB/F*&OCAfu2/AKY\n"u8 + "i(DIb:@FD,*)+C]U=@3BN#EcYf8ATD3s@q?d$AftVqCh[NqF<G:8+EV:.+Cf>-FD5W8ARlolDIa\n"u8 + "l(DId<j@<?3r@:F%a+D58'ATD4$Bl@l3De:,-DJs`8ARoFb/0JMK@qB4^F!,R<AKZ&-DfTqBG%G\n"u8 + ">uD.RTpAKYo'+CT/5+Cei#DII?(E,9)oF*2M7/c\n"u8
);

// Encode returns 0 when len(src) is 0
// Wikipedia example
// Special case when shortening !!!!! to z.
internal static slice<testpair> pairs = new testpair[]{
    new(
        ""u8,
        ""u8
    ),
    bigtest,
    new(
        "\u0000\u0000\u0000\u0000"u8,
        "z"u8
    )
}.slice();

internal static bool testEqual(ж<testing.T> Ꮡt, @string msg, params ꓸꓸꓸany argsʗp) {
    var args = argsʗp.sslice();

    Ꮡt.Helper();
    if (!AreEqual(args[len(args) - 2], args[len(args) - 1])) {
        Ꮡt.Errorf(msg, args.ꓸꓸꓸ);
        return false;
    }
    return true;
}

internal static @string strip85(@string s) {
    var t = new slice<byte>(len(s));
    nint w = 0;
    for (nint r = 0; r < len(s); r++) {
        var c = s[r];
        if (c > (rune)' ') {
            t[w] = c;
            w++;
        }
    }
    return ((@string)(t[0..(int)(w)]));
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static readonly @string encodeQQWantQˢ = "Encode(%q) = %q, want %q"u8;

public static void TestEncode(ж<testing.T> Ꮡt) {
    foreach (var (_, p) in pairs) {
        var buf = new slice<byte>(MaxEncodedLen(len(p.decoded)));
        nint n = Encode(buf, slice<byte>(p.decoded));
        buf = buf[0..(int)(n)];
        testEqual(Ꮡt, encodeQQWantQˢ, p.decoded, strip85(((@string)buf)), strip85(p.encoded));
    }
}

public static void TestEncoder(ж<testing.T> Ꮡt) {
    foreach (var (_, p) in pairs) {
        var bb = Ꮡ(new strings.Builder(nil));
        var encoder = NewEncoder(new ascii85_internal_test_package.strings_BuilderжWriter(bb));
        encoder.Write(slice<byte>(p.decoded));
        encoder.Close();
        testEqual(Ꮡt, encodeQQWantQˢ, p.decoded, strip85(bb.String()), strip85(p.encoded));
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
        var encoder = NewEncoder(new ascii85_internal_test_package.strings_BuilderжWriter(bb));
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
        testEqual(Ꮡt, encodingDOfQQWantQˢ, bs, bigtest.decoded, strip85(bb.String()), strip85(bigtest.encoded));
    }
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static readonly @string decodeQErrorVWantVˢ = "Decode(%q) = error %v, want %v"u8;
internal static readonly @string decodeQNsrcVWantVˢ = "Decode(%q) = nsrc %v, want %v"u8;
internal static readonly @string decodeQNdstVWantVˢ = "Decode(%q) = ndst %v, want %v"u8;
internal static readonly @string decodeQQWantQˢ = "Decode(%q) = %q, want %q"u8;

public static void TestDecode(ж<testing.T> Ꮡt) {
    foreach (var (_, p) in pairs) {
        var dbuf = new slice<byte>(4 * len(p.encoded));
        var (ndst, nsrc, err) = Decode(dbuf, slice<byte>(p.encoded), true);
        testEqual(Ꮡt, decodeQErrorVWantVˢ, p.encoded, err, ((error)default!));
        testEqual(Ꮡt, decodeQNsrcVWantVˢ, p.encoded, nsrc, len(p.encoded));
        testEqual(Ꮡt, decodeQNdstVWantVˢ, p.encoded, ndst, len(p.decoded));
        testEqual(Ꮡt, decodeQQWantQˢ, p.encoded, ((@string)(dbuf[0..(int)(ndst)])), p.decoded);
    }
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static readonly object readFailedˢ = (@string)"Read failed"u8;
internal static readonly @string readFromQLengthVWantVˢ = "Read from %q = length %v, want %v"u8;
internal static readonly @string decodingOfQQWantQˢ = "Decoding of %q = %q, want %q"u8;
internal static readonly @string readFromQVWantVˢ = "Read from %q = %v, want %v"u8;

public static void TestDecoder(ж<testing.T> Ꮡt) {
    foreach (var (_, p) in pairs) {
        var decoder = NewDecoder(new ascii85_internal_test_package.strings_ReaderжReader(strings.NewReader(p.encoded)));
        var (dbuf, err) = io.ReadAll(decoder);
        if (err != default!) {
            Ꮡt.Fatal(readFailedˢ, err);
        }
        testEqual(Ꮡt, readFromQLengthVWantVˢ, p.encoded, len(dbuf), len(p.decoded));
        testEqual(Ꮡt, decodingOfQQWantQˢ, p.encoded, ((@string)dbuf), p.decoded);
        if (err != default!) {
            testEqual(Ꮡt, readFromQVWantVˢ, p.encoded, err, io.EOF);
        }
    }
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static readonly @string decodingDOfQQWantQˢ = "Decoding/%d of %q = %q, want %q"u8;

public static void TestDecoderBuffering(ж<testing.T> Ꮡt) {
    for (nint bs = 1; bs <= 12; bs++) {
        var decoder = NewDecoder(new ascii85_internal_test_package.strings_ReaderжReader(strings.NewReader(bigtest.encoded)));
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
internal static readonly @string corruptionInQAtOffsetVˢ = "Corruption in %q at offset %v, want %v"u8;
internal static readonly object decoderFailedToDetectˢ = (@string)"Decoder failed to detect corruption in"u8;

[GoType("dyn")] [GoLocalName("corrupt")] internal partial struct TestDecodeCorrupt_corrupt {
    internal @string e;
    internal nint p;
}

public static void TestDecodeCorrupt(ж<testing.T> Ꮡt) {
    var examples = new TestDecodeCorrupt_corrupt[]{
        new("v"u8, 0),
        new("!z!!!!!!!!!"u8, 1)
    }.slice();
    foreach (var (_, e) in examples) {
        var dbuf = new slice<byte>(4 * len(e.e));
        var (_, _, err) = Decode(dbuf, slice<byte>(e.e), true);
        switch (err.type()) {
        case CorruptInputError errΔ1: {
            testEqual(Ꮡt, corruptionInQAtOffsetVˢ, e.e, (nint)(int64)errΔ1, e.p);
            break;
        }
        default: {
            var errΔ1 = err;
            Ꮡt.Error(decoderFailedToDetectˢ, e);
            break;
        }}
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
    var w = NewEncoder(new ascii85_internal_test_package.bytes_BufferжWriter(encoded));
    var (nn, err) = w.Write(raw);
    if (nn != n || err != default!) {
        Ꮡt.Fatalf("Encoder.Write(raw) = %d, %v want %d, nil"u8, nn, err, n);
    }
    err = w.Close();
    if (err != default!) {
        Ꮡt.Fatalf("Encoder.Close() = %v want nil"u8, err);
    }
    (var decoded, err) = io.ReadAll(NewDecoder(new ascii85_internal_test_package.bytes_BufferжReader(encoded)));
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

public static void TestDecoderInternalWhitespace(ж<testing.T> Ꮡt) {
    @string s = strings.Repeat(" "u8, 2048) + "z"u8;
    var (decoded, err) = io.ReadAll(NewDecoder(new ascii85_internal_test_package.strings_ReaderжReader(strings.NewReader(s))));
    if (err != default!) {
        Ꮡt.Errorf("Decode gave error %v"u8, err);
    }
    {
        var want = slice<byte>("\u0000\u0000\u0000\u0000"u8); if (!bytes.Equal(want, decoded)) {
            Ꮡt.Errorf("Decode failed: got %v, want %v"u8, decoded, want);
        }
    }
}

} // end ascii85_internal_test_package
