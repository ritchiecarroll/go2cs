// Copyright 2012 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.
namespace go.net.http;

using testing = testing_package;
using static go.net.http.cookiejar_package;

partial class cookiejar_internal_test_package {

// The test cases below come from RFC 3492 section 7.1 with Errata 3026.
// (A) Arabic (Egyptian).
// (B) Chinese (simplified).
// (C) Chinese (traditional).
// (D) Czech.
// (E) Hebrew.
// (F) Hindi (Devanagari).
// (G) Japanese (kanji and hiragana).
// (H) Korean (Hangul syllables).
// (I) Russian (Cyrillic).
// (J) Spanish.
// (K) Vietnamese.
// (L) 3<nen>B<gumi><kinpachi><sensei>.
// (M) <amuro><namie>-with-SUPER-MONKEYS.
// (N) Hello-Another-Way-<sorezore><no><basho>.
// (O) <hitotsu><yane><no><shita>2.
// (P) Maji<de>Koi<suru>5<byou><mae>
// (Q) <pafii>de<runba>
// (R) <sono><supiido><de>
// (S) -> $1.00 <-

partial struct punycodeTestCasesᴛ1 /*dyn*/ {
    internal @string s, encoded;
}
internal static array<punycodeTestCasesᴛ1> punycodeTestCases = new punycodeTestCasesᴛ1[]{
    new(""u8, ""u8),
    new("-"u8, "--"u8),
    new("-a"u8, "-a-"u8),
    new("-a-"u8, "-a--"u8),
    new("a"u8, "a-"u8),
    new("a-"u8, "a--"u8),
    new("a-b"u8, "a-b-"u8),
    new("books"u8, "books-"u8),
    new("bücher"u8, "bcher-kva"u8),
    new("Hello世界"u8, "Hello-ck1hg65u"u8),
    new("ü"u8, "tda"u8),
    new("üý"u8, "tdac"u8),
    new(
        "\u0644\u064A\u0647\u0645\u0627\u0628\u062A\u0643\u0644"u8 + "\u0645\u0648\u0634\u0639\u0631\u0628\u064A\u061F"u8,
        "egbpdaj6bu4bxfgehfvwxn"u8
    ),
    new(
        "\u4ED6\u4EEC\u4E3A\u4EC0\u4E48\u4E0D\u8BF4\u4E2D\u6587"u8,
        "ihqwcrb4cv8a8dqg056pqjye"u8
    ),
    new(
        "\u4ED6\u5011\u7232\u4EC0\u9EBD\u4E0D\u8AAA\u4E2D\u6587"u8,
        "ihqwctvzc91f659drss3x8bo0yb"u8
    ),
    new(
        "\u0050\u0072\u006F\u010D\u0070\u0072\u006F\u0073\u0074"u8 + "\u011B\u006E\u0065\u006D\u006C\u0075\u0076\u00ED\u010D"u8 + "\u0065\u0073\u006B\u0079"u8,
        "Proprostnemluvesky-uyb24dma41a"u8
    ),
    new(
        "\u05DC\u05DE\u05D4\u05D4\u05DD\u05E4\u05E9\u05D5\u05D8"u8 + "\u05DC\u05D0\u05DE\u05D3\u05D1\u05E8\u05D9\u05DD\u05E2"u8 + "\u05D1\u05E8\u05D9\u05EA"u8,
        "4dbcagdahymbxekheh6e0a7fei0b"u8
    ),
    new(
        "\u092F\u0939\u0932\u094B\u0917\u0939\u093F\u0928\u094D"u8 + "\u0926\u0940\u0915\u094D\u092F\u094B\u0902\u0928\u0939"u8 + "\u0940\u0902\u092C\u094B\u0932\u0938\u0915\u0924\u0947"u8 + "\u0939\u0948\u0902"u8,
        "i1baa7eci9glrd9b2ae1bj0hfcgg6iyaf8o0a1dig0cd"u8
    ),
    new(
        "\u306A\u305C\u307F\u3093\u306A\u65E5\u672C\u8A9E\u3092"u8 + "\u8A71\u3057\u3066\u304F\u308C\u306A\u3044\u306E\u304B"u8,
        "n8jok5ay5dzabd5bym9f0cm5685rrjetr6pdxa"u8
    ),
    new(
        "\uC138\uACC4\uC758\uBAA8\uB4E0\uC0AC\uB78C\uB4E4\uC774"u8 + "\uD55C\uAD6D\uC5B4\uB97C\uC774\uD574\uD55C\uB2E4\uBA74"u8 + "\uC5BC\uB9C8\uB098\uC88B\uC744\uAE4C"u8,
        "989aomsvi5e83db1d2a355cv1e0vak1dwrv93d5xbh15a0dt30a5j"u8 + "psd879ccm6fea98c"u8
    ),
    new(
        "\u043F\u043E\u0447\u0435\u043C\u0443\u0436\u0435\u043E"u8 + "\u043D\u0438\u043D\u0435\u0433\u043E\u0432\u043E\u0440"u8 + "\u044F\u0442\u043F\u043E\u0440\u0443\u0441\u0441\u043A"u8 + "\u0438"u8,
        "b1abfaaepdrnnbgefbadotcwatmq2g4l"u8
    ),
    new(
        "\u0050\u006F\u0072\u0071\u0075\u00E9\u006E\u006F\u0070"u8 + "\u0075\u0065\u0064\u0065\u006E\u0073\u0069\u006D\u0070"u8 + "\u006C\u0065\u006D\u0065\u006E\u0074\u0065\u0068\u0061"u8 + "\u0062\u006C\u0061\u0072\u0065\u006E\u0045\u0073\u0070"u8 + "\u0061\u00F1\u006F\u006C"u8,
        "PorqunopuedensimplementehablarenEspaol-fmd56a"u8
    ),
    new(
        "\u0054\u1EA1\u0069\u0073\u0061\u006F\u0068\u1ECD\u006B"u8 + "\u0068\u00F4\u006E\u0067\u0074\u0068\u1EC3\u0063\u0068"u8 + "\u1EC9\u006E\u00F3\u0069\u0074\u0069\u1EBF\u006E\u0067"u8 + "\u0056\u0069\u1EC7\u0074"u8,
        "TisaohkhngthchnitingVit-kjcr8268qyxafd2f1b9g"u8
    ),
    new(
        "\u0033\u5E74\u0042\u7D44\u91D1\u516B\u5148\u751F"u8,
        "3B-ww4c5e180e575a65lsy2b"u8
    ),
    new(
        "\u5B89\u5BA4\u5948\u7F8E\u6075\u002D\u0077\u0069\u0074"u8 + "\u0068\u002D\u0053\u0055\u0050\u0045\u0052\u002D\u004D"u8 + "\u004F\u004E\u004B\u0045\u0059\u0053"u8,
        "-with-SUPER-MONKEYS-pc58ag80a8qai00g7n9n"u8
    ),
    new(
        "\u0048\u0065\u006C\u006C\u006F\u002D\u0041\u006E\u006F"u8 + "\u0074\u0068\u0065\u0072\u002D\u0057\u0061\u0079\u002D"u8 + "\u305D\u308C\u305E\u308C\u306E\u5834\u6240"u8,
        "Hello-Another-Way--fc4qua05auwb3674vfr0b"u8
    ),
    new(
        "\u3072\u3068\u3064\u5C4B\u6839\u306E\u4E0B\u0032"u8,
        "2-u9tlzr9756bt3uc0v"u8
    ),
    new(
        "\u004D\u0061\u006A\u0069\u3067\u004B\u006F\u0069\u3059"u8 + "\u308B\u0035\u79D2\u524D"u8,
        "MajiKoi5-783gue6qz075azm5e"u8
    ),
    new(
        "\u30D1\u30D5\u30A3\u30FC\u0064\u0065\u30EB\u30F3\u30D0"u8,
        "de-jg4avhby1noc0d"u8
    ),
    new(
        "\u305D\u306E\u30B9\u30D4\u30FC\u30C9\u3067"u8,
        "d9juau41awczczp"u8
    ),
    new(
        "\u002D\u003E\u0020\u0024\u0031\u002E\u0030\u0030\u0020"u8 + "\u003C\u002D"u8,
        "-> $1.00 <--"u8
    )
}.array();

public static void TestPunycode(ж<testing.T> Ꮡt) {
    foreach (var (_, tc) in punycodeTestCases.ΔRangeSnapshot()) {
        {
            var (got, err) = encode(""u8, tc.s); if (err != default!){
                Ꮡt.Errorf(@"encode("""", %q): %v"u8, tc.s, err);
            } else 
            if (got != tc.encoded) {
                Ꮡt.Errorf(@"encode("""", %q): got %q, want %q"u8, tc.s, got, tc.encoded);
            }
        }
    }
}

} // end cookiejar_internal_test_package
