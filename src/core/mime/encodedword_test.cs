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
internal static ref readonly @string c3A9ˢ => ref c3A9ˢᵇ;
private static readonly @string c3A9ˢᵇ = "=C3=A9"u8;
internal static ref readonly @string gICAˢ => ref gICAˢᵇ;
private static readonly @string gICAˢᵇ = "gICA"u8;

internal partial struct TestEncodeWord_tests /*dyn*/ {
    internal global::go.mime_package.WordEncoder enc;
    internal @string charset;
    internal @string src, exp;
}

public static void TestEncodeWord(ж<testing.T> t) {
    @string utf8 = utf8ˢ2;
    @string iso88591 = iso88591ˢ;
    var tests = slice<TestEncodeWord_tests>([
        new(QEncoding, utf8, "François-Jérôme", "=?utf-8?q?Fran=C3=A7ois-J=C3=A9r=C3=B4me?="),
        new(BEncoding, utf8, "Café", "=?utf-8?b?Q2Fmw6k=?="),
        new(QEncoding, iso88591, "La Seleção", "=?iso-8859-1?q?La_Sele=C3=A7=C3=A3o?="),
        new(QEncoding, utf8, "", ""),
        new(QEncoding, utf8, "A", "A"),
        new(QEncoding, iso88591, "a", "a"),
        new(QEncoding, utf8, "123 456", "123 456"),
        new(QEncoding, utf8, "\t !\"#$%&'()*+,-./ :;<>?@[\\]^_`{|}~", "\t !\"#$%&'()*+,-./ :;<>?@[\\]^_`{|}~"),
        new(QEncoding, utf8, strings.Repeat("é", 10), "=?utf-8?q?"u8 + strings.Repeat(c3A9ˢ, 10) + "?="u8),
        new(QEncoding, utf8, strings.Repeat("é", 11), "=?utf-8?q?"u8 + strings.Repeat(c3A9ˢ, 10) + "?= =?utf-8?q?=C3=A9?="u8),
        new(QEncoding, iso88591, strings.Repeat(((@string)(new byte[]{0xe9})), 22), "=?iso-8859-1?q?"u8 + strings.Repeat("=E9", 22) + "?="u8),
        new(QEncoding, utf8, strings.Repeat(((@string)(new byte[]{0x80})), 22), "=?utf-8?q?"u8 + strings.Repeat("=80", 21) + "?= =?utf-8?q?=80?="u8),
        new(BEncoding, iso88591, strings.Repeat(((@string)(new byte[]{0xe9})), 45), "=?iso-8859-1?b?"u8 + strings.Repeat("6enp", 15) + "?="u8),
        new(BEncoding, utf8, strings.Repeat(((@string)(new byte[]{0x80})), 48), "=?utf-8?b?"u8 + strings.Repeat(gICAˢ, 15) + "?= =?utf-8?b?gICA?="u8)
    ]);

    foreach (var (_, test) in range(tests)) {
        @string s = test.enc.Encode(test.charset, test.src); if (s != test.exp) {
            t.Errorf("Encode(%q) = %q, want %q", test.src, s, test.exp);
        }
    }
}

internal partial struct TestEncodedWordLength_tests /*dyn*/ {
    internal global::go.mime_package.WordEncoder enc;
    internal @string src;
}

public static void TestEncodedWordLength(ж<testing.T> t) {
    var tests = slice<TestEncodedWordLength_tests>([
        new(QEncoding, strings.Repeat("à", 30)),
        new(QEncoding, strings.Repeat("é", 60)),
        new(BEncoding, strings.Repeat("ï", 25)),
        new(BEncoding, strings.Repeat("ô", 37)),
        new(BEncoding, strings.Repeat(((@string)(new byte[]{0x80})), 50)),
        new(QEncoding, "{$firstname} Bienvendio a Apostolica, aquà inicia el camino de tu")
    ]);

    foreach (var (_, test) in range(tests)) {
        @string s = test.enc.Encode(utf8ˢ2, test.src);
        nint wordLen = 0;
        for (nint i = 0; i < len(s); i++) {
            if (s[i] == ' ') {
                wordLen = 0;
                continue;
            }

            wordLen++;
            if (wordLen > maxEncodedWordLen) {
                t.Errorf("Encode(%q) has more than %d characters: %q",
                    test.src, (nint)(maxEncodedWordLen), s);
            }
        }
    }
}

internal partial struct TestDecodeWord_tests /*dyn*/ {
    internal @string src, exp;
    internal bool hasErr;
}

public static void TestDecodeWord(ж<testing.T> t) {
    var tests = slice<TestDecodeWord_tests>([
        new("=?UTF-8?Q?=C2=A1Hola,_se=C3=B1or!?=", "¡Hola, señor!", false),
        new("=?UTF-8?Q?Fran=C3=A7ois-J=C3=A9r=C3=B4me?=", "François-Jérôme", false),
        new("=?UTF-8?q?ascii?=", "ascii", false),
        new("=?utf-8?B?QW5kcsOp?=", "André", false),
        new("=?ISO-8859-1?Q?Rapha=EBl_Dupont?=", "Raphaël Dupont", false),
        new("=?utf-8?b?IkFudG9uaW8gSm9zw6kiIDxqb3NlQGV4YW1wbGUub3JnPg==?=", @"""Antonio José"" <jose@example.org>", false),
        new("=?UTF-8?A?Test?=", "", true),
        new("=?UTF-8?Q?A=B?=", "", true),
        new("=?UTF-8?Q?=A?=", "", true),
        new("=?UTF-8?A?A?=", "", true),
        new("=????=", "", true),
        new("=?UTF-8???=", "", true),
        new("=?UTF-8?Q??=", "", false)
    ]);

    foreach (var (_, test) in range(tests)) {
        var dec = @new<global::go.mime_package.WordDecoder>();
        var (s, err) = dec.Decode(test.src);
        if (test.hasErr && err == nil) {
            t.Errorf("Decode(%q) should return an error", test.src);
            continue;
        }
        if (!test.hasErr && err != nil) {
            t.Errorf("Decode(%q): %v", test.src, err);
            continue;
        }
        if (s != test.exp) {
            t.Errorf("Decode(%q) = %q, want %q", test.src, s, test.exp);
        }
    }
}

internal partial struct TestDecodeHeader_tests /*dyn*/ {
    internal @string src, exp;
}

public static void TestDecodeHeader(ж<testing.T> t) {
    var tests = slice<TestDecodeHeader_tests>([
        new("=?UTF-8?Q?=C2=A1Hola,_se=C3=B1or!?=", "¡Hola, señor!"),
        new("=?UTF-8?Q?Fran=C3=A7ois-J=C3=A9r=C3=B4me?=", "François-Jérôme"),
        new("=?UTF-8?q?ascii?=", "ascii"),
        new("=?utf-8?B?QW5kcsOp?=", "André"),
        new("=?ISO-8859-1?Q?Rapha=EBl_Dupont?=", "Raphaël Dupont"),
        new("Jean", "Jean"),
        new("=?utf-8?b?IkFudG9uaW8gSm9zw6kiIDxqb3NlQGV4YW1wbGUub3JnPg==?=", @"""Antonio José"" <jose@example.org>"),
        new("=?UTF-8?A?Test?=", "=?UTF-8?A?Test?="),
        new("=?UTF-8?Q?A=B?=", "=?UTF-8?Q?A=B?="),
        new("=?UTF-8?Q?=A?=", "=?UTF-8?Q?=A?="),
        new("=?UTF-8?A?A?=", "=?UTF-8?A?A?="),
        // Incomplete words
        new("=?", "=?"),
        new("=?UTF-8?", "=?UTF-8?"),
        new("=?UTF-8?=", "=?UTF-8?="),
        new("=?UTF-8?Q", "=?UTF-8?Q"),
        new("=?UTF-8?Q?", "=?UTF-8?Q?"),
        new("=?UTF-8?Q?=", "=?UTF-8?Q?="),
        new("=?UTF-8?Q?A", "=?UTF-8?Q?A"),
        new("=?UTF-8?Q?A?", "=?UTF-8?Q?A?"),
        // Tests from RFC 2047
        new("=?ISO-8859-1?Q?a?=", "a"),
        new("=?ISO-8859-1?Q?a?= b", "a b"),
        new("=?ISO-8859-1?Q?a?= =?ISO-8859-1?Q?b?=", "ab"),
        new("=?ISO-8859-1?Q?a?=  =?ISO-8859-1?Q?b?=", "ab"),
        new("=?ISO-8859-1?Q?a?= \r\n\t =?ISO-8859-1?Q?b?=", "ab"),
        new("=?ISO-8859-1?Q?a_b?=", "a b"),
        // Undecodable words
        new("=?UTF-8?b?garbage?= =?UTF-8?b?QW5kcsOp?= =?UTF-8?b?garbage?=", "=?UTF-8?b?garbage?= André =?UTF-8?b?garbage?="),
        new("=?UTF-8?b?QW5kcsOp", "=?UTF-8?b?QW5kcsOp"),
        new("=?UTF-8?x?y?=?UTF-8?x?y=?", "=?UTF-8?x?y?=?UTF-8?x?y=?")
    ]);

    foreach (var (_, test) in range(tests)) {
        var dec = @new<global::go.mime_package.WordDecoder>();
        var (s, err) = dec.DecodeHeader(test.src);
        if (err != nil) {
            t.Errorf("DecodeHeader(%q): %v", test.src, err);
        }
        if (s != test.exp) {
            t.Errorf("DecodeHeader(%q) = %q, want %q", test.src, s, test.exp);
        }
    }
}

internal partial struct TestCharsetDecoder_tests /*dyn*/ {
    internal @string src;
    internal @string want;
    internal slice<@string> charsets;
    internal slice<@string> content;
}

public static void TestCharsetDecoder(ж<testing.T> t) {
    var tests = slice<TestCharsetDecoder_tests>([
        new("=?utf-8?b?Q2Fmw6k=?=", "Café", default, default),
        new("=?ISO-8859-1?Q?caf=E9?=", "café", default, default),
        new("=?US-ASCII?Q?foo_bar?=", "foo bar", default, default),
        new("=?utf-8?Q?=?=", "=?utf-8?Q?=?=", default, default),
        new("=?utf-8?Q?=A?=", "=?utf-8?Q?=A?=", default, default),
        new(
            "=?ISO-8859-15?Q?f=F5=F6?=  =?windows-1252?Q?b=E0r?=",
            ((@string)(new byte[]{0x66, 0xf5, 0xf6, 0x62, 0xe0, 0x72})),
            ["iso-8859-15", "windows-1252"],
            [((@string)(new byte[]{0x66, 0xf5, 0xf6})), ((@string)(new byte[]{0x62, 0xe0, 0x72}))]
        )
    ]);

    foreach (var (_, vᴛ1) in range(tests)) {
        ref var test = ref heap(new TestCharsetDecoder_tests(), out var Ꮡtest);
        test = vᴛ1;

        nint i = 0;
            var testʗ1 = test;
        var dec = Ꮡ(new WordDecoder(
            CharsetReader: (@string charset, Δio.Reader input) => {
                if (charset != testʗ1.charsets[i]) {
                    t.Errorf("DecodeHeader(%q), got charset %q, want %q", testʗ1.src, charset, testʗ1.charsets[i]);
                }
                var (content, errΔ1) = Δio.ReadAll(input);
                if (errΔ1 != nil) {
                    t.Errorf("DecodeHeader(%q), error in reader: %v", testʗ1.src, errΔ1);
                }
                @string gotΔ1 = (@string)content;
                if (gotΔ1 != testʗ1.content[i]) {
                    t.Errorf("DecodeHeader(%q), got content %q, want %q", testʗ1.src, gotΔ1, testʗ1.content[i]);
                }
                i++;

                return (new mime_test_package.strings_ReaderжReader(strings.NewReader(gotΔ1)), default);
            }
        ));
        var (got, err) = dec.DecodeHeader(test.src);
        if (err != nil) {
            t.Errorf("DecodeHeader(%q): %v", test.src, err);
        }
        if (got != test.want) {
            t.Errorf("DecodeHeader(%q) = %q, want %q", test.src, got, test.want);
        }
    }
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static ref readonly @string testErrorˢ => ref testErrorˢᵇ;
private static readonly @string testErrorˢᵇ = "Test error"u8;
internal static ref readonly @string charsetQFooˢ => ref charsetQFooˢᵇ;
private static readonly @string charsetQFooˢᵇ = "=?charset?Q?foo?="u8;
internal static readonly object decodeHeaderShouldReturnˢ = (@string)"DecodeHeader should return an error"u8;

public static void TestCharsetDecoderError(ж<testing.T> t) {
    var dec = Ꮡ(new WordDecoder(
        CharsetReader: (@string charset, Δio.Reader input) => (default, errors.New(testErrorˢ))
    ));

    var (_, err) = dec.DecodeHeader(charsetQFooˢ); if (err == nil) {
        t.Error(decodeHeaderShouldReturnˢ);
    }
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static ref readonly @string holaSeOrˢ => ref holaSeOrˢᵇ;
private static readonly @string holaSeOrˢᵇ = "¡Hola, señor!"u8;

public static void BenchmarkQEncodeWord(ж<testing.B> Ꮡb) {
    ref var b = ref Ꮡb.DerefOrNull();

    for (nint i = 0; i < b.N; i++) {
        QEncoding.Encode(utf8ˢ, holaSeOrˢ);
    }
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static ref readonly @string utf8QC2A1HolaSeC3B1orˢ => ref utf8QC2A1HolaSeC3B1orˢᵇ;
private static readonly @string utf8QC2A1HolaSeC3B1orˢᵇ = "=?utf-8?q?=C2=A1Hola,_se=C3=B1or!?="u8;

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
