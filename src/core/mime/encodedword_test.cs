// Copyright 2015 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.
namespace go;

using errors = errors_package;
using Δio = io_package;
using strings = strings_package;
using testing = testing_package;
using static go.mime_package;

partial class mime_internal_test_package {

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static readonly @string c3A9ˢ = "=C3=A9"u8;
internal static readonly @string gICAˢ = "gICA"u8;

internal partial struct TestEncodeWord_tests /*dyn*/ {
    internal global::go.mime_package.WordEncoder enc;
    internal @string charset;
    internal @string src, exp;
}

public static void TestEncodeWord(ж<testing.T> Ꮡt) {
    ref var t = ref Ꮡt.DerefOrNull();

    @string utf8 = utf8ˢ2;
    @string iso88591 = iso88591ˢ;
    var tests = new TestEncodeWord_tests[]{
        new(QEncoding, utf8, "François-Jérôme"u8, "=?utf-8?q?Fran=C3=A7ois-J=C3=A9r=C3=B4me?="u8),
        new(BEncoding, utf8, "Café"u8, "=?utf-8?b?Q2Fmw6k=?="u8),
        new(QEncoding, iso88591, "La Seleção"u8, "=?iso-8859-1?q?La_Sele=C3=A7=C3=A3o?="u8),
        new(QEncoding, utf8, ""u8, ""u8),
        new(QEncoding, utf8, "A"u8, "A"u8),
        new(QEncoding, iso88591, "a"u8, "a"u8),
        new(QEncoding, utf8, "123 456"u8, "123 456"u8),
        new(QEncoding, utf8, "\t !\"#$%&'()*+,-./ :;<>?@[\\]^_`{|}~"u8, "\t !\"#$%&'()*+,-./ :;<>?@[\\]^_`{|}~"u8),
        new(QEncoding, utf8, strings.Repeat("é"u8, 10), "=?utf-8?q?"u8 + strings.Repeat(c3A9ˢ, 10) + "?="u8),
        new(QEncoding, utf8, strings.Repeat("é"u8, 11), "=?utf-8?q?"u8 + strings.Repeat(c3A9ˢ, 10) + "?= =?utf-8?q?=C3=A9?="u8),
        new(QEncoding, iso88591, strings.Repeat(((@string)(new byte[]{0xe9})), 22), "=?iso-8859-1?q?"u8 + strings.Repeat("=E9"u8, 22) + "?="u8),
        new(QEncoding, utf8, strings.Repeat(((@string)(new byte[]{0x80})), 22), "=?utf-8?q?"u8 + strings.Repeat("=80"u8, 21) + "?= =?utf-8?q?=80?="u8),
        new(BEncoding, iso88591, strings.Repeat(((@string)(new byte[]{0xe9})), 45), "=?iso-8859-1?b?"u8 + strings.Repeat("6enp"u8, 15) + "?="u8),
        new(BEncoding, utf8, strings.Repeat(((@string)(new byte[]{0x80})), 48), "=?utf-8?b?"u8 + strings.Repeat(gICAˢ, 15) + "?= =?utf-8?b?gICA?="u8)
    }.slice();
    foreach (var (_, test) in tests) {
        {
            @string s = test.enc.Encode(test.charset, test.src); if (s != test.exp) {
                Ꮡt.Errorf("Encode(%q) = %q, want %q"u8, test.src, s, test.exp);
            }
        }
    }
}

internal partial struct TestEncodedWordLength_tests /*dyn*/ {
    internal global::go.mime_package.WordEncoder enc;
    internal @string src;
}

public static void TestEncodedWordLength(ж<testing.T> Ꮡt) {
    var tests = new TestEncodedWordLength_tests[]{
        new(QEncoding, strings.Repeat("à"u8, 30)),
        new(QEncoding, strings.Repeat("é"u8, 60)),
        new(BEncoding, strings.Repeat("ï"u8, 25)),
        new(BEncoding, strings.Repeat("ô"u8, 37)),
        new(BEncoding, strings.Repeat(((@string)(new byte[]{0x80})), 50)),
        new(QEncoding, "{$firstname} Bienvendio a Apostolica, aquà inicia el camino de tu"u8)
    }.slice();
    foreach (var (_, test) in tests) {
        @string s = test.enc.Encode(utf8ˢ2, test.src);
        nint wordLen = 0;
        for (nint i = 0; i < len(s); i++) {
            if (s[i] == (rune)' ') {
                wordLen = 0;
                continue;
            }
            wordLen++;
            if (wordLen > maxEncodedWordLen) {
                Ꮡt.Errorf("Encode(%q) has more than %d characters: %q"u8,
                    test.src, (nint)(maxEncodedWordLen), s);
            }
        }
    }
}

internal partial struct TestDecodeWord_tests /*dyn*/ {
    internal @string src, exp;
    internal bool hasErr;
}

public static void TestDecodeWord(ж<testing.T> Ꮡt) {
    var tests = new TestDecodeWord_tests[]{
        new("=?UTF-8?Q?=C2=A1Hola,_se=C3=B1or!?="u8, "¡Hola, señor!"u8, false),
        new("=?UTF-8?Q?Fran=C3=A7ois-J=C3=A9r=C3=B4me?="u8, "François-Jérôme"u8, false),
        new("=?UTF-8?q?ascii?="u8, "ascii"u8, false),
        new("=?utf-8?B?QW5kcsOp?="u8, "André"u8, false),
        new("=?ISO-8859-1?Q?Rapha=EBl_Dupont?="u8, "Raphaël Dupont"u8, false),
        new("=?utf-8?b?IkFudG9uaW8gSm9zw6kiIDxqb3NlQGV4YW1wbGUub3JnPg==?="u8, @"""Antonio José"" <jose@example.org>"u8, false),
        new("=?UTF-8?A?Test?="u8, ""u8, true),
        new("=?UTF-8?Q?A=B?="u8, ""u8, true),
        new("=?UTF-8?Q?=A?="u8, ""u8, true),
        new("=?UTF-8?A?A?="u8, ""u8, true),
        new("=????="u8, ""u8, true),
        new("=?UTF-8???="u8, ""u8, true),
        new("=?UTF-8?Q??="u8, ""u8, false)
    }.slice();
    foreach (var (_, test) in tests) {
        var dec = @new<global::go.mime_package.WordDecoder>();
        var (s, err) = dec.Decode(test.src);
        if (test.hasErr && err == default!) {
            Ꮡt.Errorf("Decode(%q) should return an error"u8, test.src);
            continue;
        }
        if (!test.hasErr && err != default!) {
            Ꮡt.Errorf("Decode(%q): %v"u8, test.src, err);
            continue;
        }
        if (s != test.exp) {
            Ꮡt.Errorf("Decode(%q) = %q, want %q"u8, test.src, s, test.exp);
        }
    }
}

internal partial struct TestDecodeHeader_tests /*dyn*/ {
    internal @string src, exp;
}

public static void TestDecodeHeader(ж<testing.T> Ꮡt) {
    ref var t = ref Ꮡt.DerefOrNull();

    var tests = new TestDecodeHeader_tests[]{
        new("=?UTF-8?Q?=C2=A1Hola,_se=C3=B1or!?="u8, "¡Hola, señor!"u8),
        new("=?UTF-8?Q?Fran=C3=A7ois-J=C3=A9r=C3=B4me?="u8, "François-Jérôme"u8),
        new("=?UTF-8?q?ascii?="u8, "ascii"u8),
        new("=?utf-8?B?QW5kcsOp?="u8, "André"u8),
        new("=?ISO-8859-1?Q?Rapha=EBl_Dupont?="u8, "Raphaël Dupont"u8),
        new("Jean"u8, "Jean"u8),
        new("=?utf-8?b?IkFudG9uaW8gSm9zw6kiIDxqb3NlQGV4YW1wbGUub3JnPg==?="u8, @"""Antonio José"" <jose@example.org>"u8),
        new("=?UTF-8?A?Test?="u8, "=?UTF-8?A?Test?="u8),
        new("=?UTF-8?Q?A=B?="u8, "=?UTF-8?Q?A=B?="u8),
        new("=?UTF-8?Q?=A?="u8, "=?UTF-8?Q?=A?="u8),
        new("=?UTF-8?A?A?="u8, "=?UTF-8?A?A?="u8), // Incomplete words

        new("=?"u8, "=?"u8),
        new("=?UTF-8?"u8, "=?UTF-8?"u8),
        new("=?UTF-8?="u8, "=?UTF-8?="u8),
        new("=?UTF-8?Q"u8, "=?UTF-8?Q"u8),
        new("=?UTF-8?Q?"u8, "=?UTF-8?Q?"u8),
        new("=?UTF-8?Q?="u8, "=?UTF-8?Q?="u8),
        new("=?UTF-8?Q?A"u8, "=?UTF-8?Q?A"u8),
        new("=?UTF-8?Q?A?"u8, "=?UTF-8?Q?A?"u8), // Tests from RFC 2047

        new("=?ISO-8859-1?Q?a?="u8, "a"u8),
        new("=?ISO-8859-1?Q?a?= b"u8, "a b"u8),
        new("=?ISO-8859-1?Q?a?= =?ISO-8859-1?Q?b?="u8, "ab"u8),
        new("=?ISO-8859-1?Q?a?=  =?ISO-8859-1?Q?b?="u8, "ab"u8),
        new("=?ISO-8859-1?Q?a?= \r\n\t =?ISO-8859-1?Q?b?="u8, "ab"u8),
        new("=?ISO-8859-1?Q?a_b?="u8, "a b"u8)
    }.slice();
    foreach (var (_, test) in tests) {
        var dec = @new<global::go.mime_package.WordDecoder>();
        var (s, err) = dec.DecodeHeader(test.src);
        if (err != default!) {
            Ꮡt.Errorf("DecodeHeader(%q): %v"u8, test.src, err);
        }
        if (s != test.exp) {
            Ꮡt.Errorf("DecodeHeader(%q) = %q, want %q"u8, test.src, s, test.exp);
        }
    }
}

internal partial struct TestCharsetDecoder_tests /*dyn*/ {
    internal @string src;
    internal @string want;
    internal slice<@string> charsets;
    internal slice<@string> content;
}

public static void TestCharsetDecoder(ж<testing.T> Ꮡt) {
    var tests = new TestCharsetDecoder_tests[]{
        new("=?utf-8?b?Q2Fmw6k=?="u8, "Café"u8, default!, default!),
        new("=?ISO-8859-1?Q?caf=E9?="u8, "café"u8, default!, default!),
        new("=?US-ASCII?Q?foo_bar?="u8, "foo bar"u8, default!, default!),
        new("=?utf-8?Q?=?="u8, "=?utf-8?Q?=?="u8, default!, default!),
        new("=?utf-8?Q?=A?="u8, "=?utf-8?Q?=A?="u8, default!, default!),
        new(
            "=?ISO-8859-15?Q?f=F5=F6?=  =?windows-1252?Q?b=E0r?="u8,
            ((@string)(new byte[]{0x66, 0xf5, 0xf6, 0x62, 0xe0, 0x72})),
            new @string[]{"iso-8859-15"u8, "windows-1252"u8}.slice(),
            new @string[]{((@string)(new byte[]{0x66, 0xf5, 0xf6})), ((@string)(new byte[]{0x62, 0xe0, 0x72}))}.slice()
        )
    }.slice();
    foreach (var (_, vᴛ1) in tests) {
        ref var test = ref heap(new TestCharsetDecoder_tests(), out var Ꮡtest);
        test = vᴛ1;

        nint i = 0;
            var testʗ1 = test;
        var dec = Ꮡ(new WordDecoder(
            CharsetReader: (@string charset, Δio.Reader input) => {
                if (charset != testʗ1.charsets[i]) {
                    Ꮡt.Errorf("DecodeHeader(%q), got charset %q, want %q"u8, testʗ1.src, charset, testʗ1.charsets[i]);
                }
                var (content, errΔ1) = Δio.ReadAll(input);
                if (errΔ1 != default!) {
                    Ꮡt.Errorf("DecodeHeader(%q), error in reader: %v"u8, testʗ1.src, errΔ1);
                }
                @string gotΔ1 = ((@string)content);
                if (gotΔ1 != testʗ1.content[i]) {
                    Ꮡt.Errorf("DecodeHeader(%q), got content %q, want %q"u8, testʗ1.src, gotΔ1, testʗ1.content[i]);
                }
                i++;
                return (new mime_test_package.strings_ReaderжReader(strings.NewReader(gotΔ1)), default!);
            }
        ));
        var (got, err) = dec.DecodeHeader(test.src);
        if (err != default!) {
            Ꮡt.Errorf("DecodeHeader(%q): %v"u8, test.src, err);
        }
        if (got != test.want) {
            Ꮡt.Errorf("DecodeHeader(%q) = %q, want %q"u8, test.src, got, test.want);
        }
    }
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static readonly @string testErrorˢ = "Test error"u8;
internal static readonly @string charsetQFooˢ = "=?charset?Q?foo?="u8;
internal static readonly object decodeHeaderShouldReturnˢ = (@string)"DecodeHeader should return an error"u8;

public static void TestCharsetDecoderError(ж<testing.T> Ꮡt) {
    var dec = Ꮡ(new WordDecoder(
        CharsetReader: (@string charset, Δio.Reader input) => (default!, errors.New(testErrorˢ))
    ));
    {
        var (_, err) = dec.DecodeHeader(charsetQFooˢ); if (err == default!) {
            Ꮡt.Error(decodeHeaderShouldReturnˢ);
        }
    }
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static readonly @string holaSeOrˢ = "¡Hola, señor!"u8;

public static void BenchmarkQEncodeWord(ж<testing.B> Ꮡb) {
    ref var b = ref Ꮡb.DerefOrNull();

    for (nint i = 0; i < b.N; i++) {
        QEncoding.Encode(utf8ˢ, holaSeOrˢ);
    }
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static readonly @string utf8QC2A1HolaSeC3B1orˢ = "=?utf-8?q?=C2=A1Hola,_se=C3=B1or!?="u8;

public static void BenchmarkQDecodeWord(ж<testing.B> Ꮡb) {
    ref var b = ref Ꮡb.DerefOrNull();

    var dec = @new<global::go.mime_package.WordDecoder>();
    for (nint i = 0; i < b.N; i++) {
        dec.Decode(utf8QC2A1HolaSeC3B1orˢ);
    }
}

public static void BenchmarkQDecodeHeader(ж<testing.B> Ꮡb) {
    ref var b = ref Ꮡb.DerefOrNull();

    var dec = @new<global::go.mime_package.WordDecoder>();
    for (nint i = 0; i < b.N; i++) {
        dec.DecodeHeader(utf8QC2A1HolaSeC3B1orˢ);
    }
}

} // end mime_internal_test_package
