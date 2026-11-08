// Copyright 2009 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.
namespace go;

using bytes = bytes_package;
using fmt = fmt_package;
using static strings_package;
using testing = testing_package;
using static go.strings_internal_test_package;
using strings = strings_package;
using Δio = io_package;

partial class strings_test_package {

internal static ж<strings.Replacer> htmlEscaper = NewReplacer(
    "&", "&amp;",
    "<", "&lt;",
    ">", "&gt;",
    @"""", "&quot;",
    "'", "&apos;");

internal static ж<strings.Replacer> htmlUnescaper = NewReplacer(
    "&amp;", "&",
    "&lt;", "<",
    "&gt;", ">",
    "&quot;", @"""",
    "&apos;", "'");

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static ref readonly @string ampˢ => ref ampˢᵇ;
private static readonly @string ampˢᵇ = "&amp;"u8;
internal static ref readonly @string quotˢ => ref quotˢᵇ;
private static readonly @string quotˢᵇ = "&quot;"u8;
internal static ref readonly @string aposˢ => ref aposˢᵇ;
private static readonly @string aposˢᵇ = "&apos;"u8;

// The http package's old HTML escaping function.
internal static @string oldHTMLEscape(@string s) {
    s = Replace(s, "&", ampˢ, -1);
    s = Replace(s, "<", "&lt;", -1);
    s = Replace(s, ">", "&gt;", -1);
    s = Replace(s, @"""", quotˢ, -1);
    s = Replace(s, "'", aposˢ, -1);
    return s;
}

internal static ж<strings.Replacer> capitalLetters = NewReplacer("a", "A", "b", "B");

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static ref readonly @string xxxˢ => ref xxxˢᵇ;
private static readonly @string xxxˢᵇ = "xxx"u8;
internal static ref readonly @string aaaˢ2 => ref aaaˢ2ᵇ;
private static readonly @string aaaˢ2ᵇ = "3[aaa]"u8;
internal static ref readonly @string longerstˢ => ref longerstˢᵇ;
private static readonly @string longerstˢᵇ = "longerst"u8;
internal static ref readonly @string mostLongˢ => ref mostLongˢᵇ;
private static readonly @string mostLongˢᵇ = "most long"u8;
internal static ref readonly @string longerˢ => ref longerˢᵇ;
private static readonly @string longerˢᵇ = "longer"u8;
internal static ref readonly @string mediumˢ => ref mediumˢᵇ;
private static readonly @string mediumˢᵇ = "medium"u8;
internal static ref readonly @string longˢ => ref longˢᵇ;
private static readonly @string longˢᵇ = "long"u8;
internal static ref readonly @string shortˢ => ref shortˢᵇ;
private static readonly @string shortˢᵇ = "short"u8;
internal static ref readonly @string rosesˢ => ref rosesˢᵇ;
private static readonly @string rosesˢᵇ = "roses"u8;
internal static ref readonly @string redˢ => ref redˢᵇ;
private static readonly @string redˢᵇ = "red"u8;
internal static ref readonly @string violetsˢ => ref violetsˢᵇ;
private static readonly @string violetsˢᵇ = "violets"u8;
internal static ref readonly @string blueˢ => ref blueˢᵇ;
private static readonly @string blueˢᵇ = "blue"u8;
internal static ref readonly @string sugarˢ => ref sugarˢᵇ;
private static readonly @string sugarˢᵇ = "sugar"u8;
internal static ref readonly @string sweetˢ => ref sweetˢᵇ;
private static readonly @string sweetˢᵇ = "sweet"u8;
internal static ref readonly @string abracadabraˢ => ref abracadabraˢᵇ;
private static readonly @string abracadabraˢᵇ = "abracadabra"u8;
internal static ref readonly @string poofˢ => ref poofˢᵇ;
private static readonly @string poofˢᵇ = "poof"u8;
internal static ref readonly @string abracadabrakazamˢ => ref abracadabrakazamˢᵇ;
private static readonly @string abracadabrakazamˢᵇ = "abracadabrakazam"u8;
internal static ref readonly @string splatˢ => ref splatˢᵇ;
private static readonly @string splatˢᵇ = "splat"u8;
internal static ref readonly @string abrahamˢ => ref abrahamˢᵇ;
private static readonly @string abrahamˢᵇ = "abraham"u8;
internal static ref readonly @string lincolnˢ => ref lincolnˢᵇ;
private static readonly @string lincolnˢᵇ = "lincoln"u8;
internal static ref readonly @string abrasionˢ => ref abrasionˢᵇ;
private static readonly @string abrasionˢᵇ = "abrasion"u8;
internal static ref readonly @string scrapeˢ => ref scrapeˢᵇ;
private static readonly @string scrapeˢᵇ = "scrape"u8;
internal static ref readonly @string isaacˢ => ref isaacˢᵇ;
private static readonly @string isaacˢᵇ = "isaac"u8;
internal static ref readonly @string foo1ˢ => ref foo1ˢᵇ;
private static readonly @string foo1ˢᵇ = "foo1"u8;
internal static ref readonly @string foo2ˢ => ref foo2ˢᵇ;
private static readonly @string foo2ˢᵇ = "foo2"u8;
internal static ref readonly @string foo3ˢ => ref foo3ˢᵇ;
private static readonly @string foo3ˢᵇ = "foo3"u8;
internal static ref readonly @string foo31ˢ => ref foo31ˢᵇ;
private static readonly @string foo31ˢᵇ = "foo31"u8;
internal static ref readonly @string foo32ˢ => ref foo32ˢᵇ;
private static readonly @string foo32ˢᵇ = "foo32"u8;
internal static ref readonly @string foo11ˢ => ref foo11ˢᵇ;
private static readonly @string foo11ˢᵇ = "foo11"u8;
internal static ref readonly @string foo12ˢ => ref foo12ˢᵇ;
private static readonly @string foo12ˢᵇ = "foo12"u8;
internal static ref readonly @string allˢ => ref allˢᵇ;
private static readonly @string allˢᵇ = "[all]"u8;
internal static ref readonly @string foobarˢ => ref foobarˢᵇ;
private static readonly @string foobarˢᵇ = "foobar"u8;
internal static ref readonly @string foobazˢ => ref foobazˢᵇ;
private static readonly @string foobazˢᵇ = "foobaz"u8;
internal static ref readonly @string matchˢ => ref matchˢᵇ;
private static readonly @string matchˢᵇ = "[match]"u8;
internal static ref readonly @string helloˢ3 => ref helloˢ3ᵇ;
private static readonly @string helloˢ3ᵇ = "Hello"u8;

internal partial struct TestReplacer_testCase /*dyn*/ {
    internal ж<strings.Replacer> r;
    internal @string @in, @out;
}

// TestReplacer tests the replacer implementations.
public static void TestReplacer(ж<testing.T> t) {
    var sᴷ = new stackAppend<@string>();

    slice<TestReplacer_testCase> testCases = default;

    // str converts 0xff to "\xff". This isn't just string(b) since that converts to UTF-8.
    @string str(byte b) => (@string)slice<byte>([b]);
    slice<@string> s = default;

    // inc maps "\x00"->"\x01", ..., "a"->"b", "b"->"c", ..., "\xff"->"\x00".
    s = default;
    for (nint i = 0; i < 256; i++) {
        s = append(sᴷ, s, str((byte)i), str((byte)(i + 1)));
    }
    var inc = NewReplacer(s.ꓸꓸꓸ);

    // Test cases with 1-byte old strings, 1-byte new strings.
    testCases = append(testCases,
        new TestReplacer_testCase(capitalLetters, "brad", "BrAd"),
        new TestReplacer_testCase(capitalLetters, Repeat("a", (32 << 10) + 123), Repeat("A", (32 << 10) + 123)),
        new TestReplacer_testCase(capitalLetters, "", ""),

        new TestReplacer_testCase(inc, "brad", "csbe"),
        new TestReplacer_testCase(inc, ((@string)(new byte[]{0x00, 0xff})), "\x01\x00"),
        new TestReplacer_testCase(inc, "", ""),

        new TestReplacer_testCase(NewReplacer("a", "1", "a", "2"), "brad", "br1d"));

    // repeat maps "a"->"a", "b"->"bb", "c"->"ccc", ...
    s = default;
    for (nint i = 0; i < 256; i++) {
        nint n = i + 1 - 'a';
        if (n < 1) {
            n = 1;
        }
        s = append(s, str((byte)i), Repeat(str((byte)i), n));
    }
    var repeat = NewReplacer(s.ꓸꓸꓸ);

    // Test cases with 1-byte old strings, variable length new strings.
    testCases = append(testCases,
        new TestReplacer_testCase(htmlEscaper, "No changes", "No changes"),
        new TestReplacer_testCase(htmlEscaper, "I <3 escaping & stuff", "I &lt;3 escaping &amp; stuff"),
        new TestReplacer_testCase(htmlEscaper, "&&&", "&amp;&amp;&amp;"),
        new TestReplacer_testCase(htmlEscaper, "", ""),

        new TestReplacer_testCase(repeat, "brad", "bbrrrrrrrrrrrrrrrrrradddd"),
        new TestReplacer_testCase(repeat, "abba", "abbbba"),
        new TestReplacer_testCase(repeat, "", ""),

        new TestReplacer_testCase(NewReplacer("a", "11", "a", "22"), "brad", "br11d"));

    // The remaining test cases have variable length old strings.

    testCases = append(testCases,
        new TestReplacer_testCase(htmlUnescaper, "&amp;amp;", "&amp;"),
        new TestReplacer_testCase(htmlUnescaper, "&lt;b&gt;HTML&apos;s neat&lt;/b&gt;", "<b>HTML's neat</b>"),
        new TestReplacer_testCase(htmlUnescaper, "", ""),

        new TestReplacer_testCase(NewReplacer("a", "1", "a", "2", xxxˢ, xxxˢ), "brad", "br1d"),

        new TestReplacer_testCase(NewReplacer("a", "1", "aa", "2", aaaˢ, "3"), "aaaa", "1111"),

        new TestReplacer_testCase(NewReplacer(aaaˢ, "3", "aa", "2", "a", "1"), "aaaa", "31"));

    // gen1 has multiple old strings of variable length. There is no
    // overall non-empty common prefix, but some pairwise common prefixes.
    var gen1 = NewReplacer(
        aaaˢ, aaaˢ2,
        "aa", "2[aa]",
        "a", "1[a]",
        "i", "i",
        longerstˢ, mostLongˢ,
        longerˢ, mediumˢ,
        longˢ, shortˢ,
        "xx", "xx",
        "x", "X",
        "X", "Y",
        "Y", "Z");
    testCases = append(testCases,
        new TestReplacer_testCase(gen1, "fooaaabar", "foo3[aaa]b1[a]r"),
        new TestReplacer_testCase(gen1, "long, longerst, longer", "short, most long, medium"),
        new TestReplacer_testCase(gen1, "xxxxx", "xxxxX"),
        new TestReplacer_testCase(gen1, "XiX", "YiY"),
        new TestReplacer_testCase(gen1, "", ""));

    // gen2 has multiple old strings with no pairwise common prefix.
    var gen2 = NewReplacer(
        rosesˢ, redˢ,
        violetsˢ, blueˢ,
        sugarˢ, sweetˢ);
    testCases = append(testCases,
        new TestReplacer_testCase(gen2, "roses are red, violets are blue...", "red are red, blue are blue..."),
        new TestReplacer_testCase(gen2, "", ""));

    // gen3 has multiple old strings with an overall common prefix.
    var gen3 = NewReplacer(
        abracadabraˢ, poofˢ,
        abracadabrakazamˢ, splatˢ,
        abrahamˢ, lincolnˢ,
        abrasionˢ, scrapeˢ,
        abrahamˢ, isaacˢ);
    testCases = append(testCases,
        new TestReplacer_testCase(gen3, "abracadabrakazam abraham", "poofkazam lincoln"),
        new TestReplacer_testCase(gen3, "abrasion abracad", "scrape abracad"),
        new TestReplacer_testCase(gen3, "abba abram abrasive", "abba abram abrasive"),
        new TestReplacer_testCase(gen3, "", ""));

    // foo{1,2,3,4} have multiple old strings with an overall common prefix
    // and 1- or 2- byte extensions from the common prefix.
    var foo1 = NewReplacer(
        foo1ˢ, "A",
        foo2ˢ, "B",
        foo3ˢ, "C");
    var foo2 = NewReplacer(
        foo1ˢ, "A",
        foo2ˢ, "B",
        foo31ˢ, "C",
        foo32ˢ, "D");
    var foo3 = NewReplacer(
        foo11ˢ, "A",
        foo12ˢ, "B",
        foo31ˢ, "C",
        foo32ˢ, "D");
    var foo4 = NewReplacer(
        foo12ˢ, "B",
        foo32ˢ, "D");
    testCases = append(testCases,
        new TestReplacer_testCase(foo1, "fofoofoo12foo32oo", "fofooA2C2oo"),
        new TestReplacer_testCase(foo1, "", ""),

        new TestReplacer_testCase(foo2, "fofoofoo12foo32oo", "fofooA2Doo"),
        new TestReplacer_testCase(foo2, "", ""),

        new TestReplacer_testCase(foo3, "fofoofoo12foo32oo", "fofooBDoo"),
        new TestReplacer_testCase(foo3, "", ""),

        new TestReplacer_testCase(foo4, "fofoofoo12foo32oo", "fofooBDoo"),
        new TestReplacer_testCase(foo4, "", ""));

    // genAll maps "\x00\x01\x02...\xfe\xff" to "[all]", amongst other things.
    var allBytes = new slice<byte>(256);
    foreach (var i in range(len(allBytes))) {
        allBytes[i] = (byte)i;
    }
    @string allString = (@string)allBytes;
    var genAll = NewReplacer(
        allString, allˢ,
        ((@string)(new byte[]{0xff})), "[ff]",
        "\x00", "[00]");
    testCases = append(testCases,
        new TestReplacer_testCase(genAll, allString, "[all]"),
        new TestReplacer_testCase(genAll, ((@string)(new byte[]{0x61, 0xff})) + allString + "\x00"u8, "a[ff][all][00]"),
        new TestReplacer_testCase(genAll, "", ""));

    // Test cases with empty old strings.

    var blankToX1 = NewReplacer(""u8, "X");
    var blankToX2 = NewReplacer(""u8, "X", "", "");
    var blankHighPriority = NewReplacer(""u8, "X", "o", "O");
    var blankLowPriority = NewReplacer("o", "O", "", "X");
    var blankNoOp1 = NewReplacer(""u8, "");
    var blankNoOp2 = NewReplacer(""u8, "", "", "A");
    var blankFoo = NewReplacer(""u8, "X", foobarˢ, "R", foobazˢ, "Z");
    testCases = append(testCases,
        new TestReplacer_testCase(blankToX1, "foo", "XfXoXoX"),
        new TestReplacer_testCase(blankToX1, "", "X"),

        new TestReplacer_testCase(blankToX2, "foo", "XfXoXoX"),
        new TestReplacer_testCase(blankToX2, "", "X"),

        new TestReplacer_testCase(blankHighPriority, "oo", "XOXOX"),
        new TestReplacer_testCase(blankHighPriority, "ii", "XiXiX"),
        new TestReplacer_testCase(blankHighPriority, "oiio", "XOXiXiXOX"),
        new TestReplacer_testCase(blankHighPriority, "iooi", "XiXOXOXiX"),
        new TestReplacer_testCase(blankHighPriority, "", "X"),

        new TestReplacer_testCase(blankLowPriority, "oo", "OOX"),
        new TestReplacer_testCase(blankLowPriority, "ii", "XiXiX"),
        new TestReplacer_testCase(blankLowPriority, "oiio", "OXiXiOX"),
        new TestReplacer_testCase(blankLowPriority, "iooi", "XiOOXiX"),
        new TestReplacer_testCase(blankLowPriority, "", "X"),

        new TestReplacer_testCase(blankNoOp1, "foo", "foo"),
        new TestReplacer_testCase(blankNoOp1, "", ""),

        new TestReplacer_testCase(blankNoOp2, "foo", "foo"),
        new TestReplacer_testCase(blankNoOp2, "", ""),

        new TestReplacer_testCase(blankFoo, "foobarfoobaz", "XRXZX"),
        new TestReplacer_testCase(blankFoo, "foobar-foobaz", "XRX-XZX"),
        new TestReplacer_testCase(blankFoo, "", "X"));

    // single string replacer

    var abcMatcher = NewReplacer(abcˢ, matchˢ);

    testCases = append(testCases,
        new TestReplacer_testCase(abcMatcher, "", ""),
        new TestReplacer_testCase(abcMatcher, "ab", "ab"),
        new TestReplacer_testCase(abcMatcher, "abc", "[match]"),
        new TestReplacer_testCase(abcMatcher, "abcd", "[match]d"),
        new TestReplacer_testCase(abcMatcher, "cabcabcdabca", "c[match][match]d[match]a"));

    // Issue 6659 cases (more single string replacer)

    var noHello = NewReplacer(helloˢ3, "");
    testCases = append(testCases,
        new TestReplacer_testCase(noHello, "Hello", ""),
        new TestReplacer_testCase(noHello, "Hellox", "x"),
        new TestReplacer_testCase(noHello, "xHello", "x"),
        new TestReplacer_testCase(noHello, "xHellox", "xx"));

    // No-arg test cases.

    var nop = NewReplacer();
    testCases = append(testCases,
        new TestReplacer_testCase(nop, "abc", "abc"),
        new TestReplacer_testCase(nop, "", ""));

    // Run the test cases.

    foreach (var (i, tc) in range(testCases)) {
        @string sΔ1 = tc.r.Replace(tc.@in); if (sΔ1 != tc.@out) {
            t.Errorf("%d. Replace(%q) = %q, want %q", i, tc.@in, sΔ1, tc.@out);
        }
        ref var buf = ref heap(new bytes.Buffer(), out var Ꮡbuf);
        var (n, err) = tc.r.WriteString(new strings_test_package.bytes_BufferжWriter(Ꮡbuf), tc.@in);
        if (err != nil) {
            t.Errorf("%d. WriteString: %v", i, err);
            continue;
        }
        @string got = Ꮡbuf.String();
        if (got != tc.@out) {
            t.Errorf("%d. WriteString(%q) wrote %q, want %q", i, tc.@in, got, tc.@out);
            continue;
        }
        if (n != len(tc.@out)) {
            t.Errorf("%d. WriteString(%q) wrote correct string but reported %d bytes; want %d (%q)",
                i, tc.@in, n, len(tc.@out), tc.@out);
        }
    }
}

partial struct algorithmTestCasesᴛ1 /*dyn*/ {
    internal ж<strings.Replacer> r;
    internal @string want;
}
internal static slice<algorithmTestCasesᴛ1> algorithmTestCases = slice<algorithmTestCasesᴛ1>([
    new(capitalLetters, "*strings.byteReplacer"),
    new(htmlEscaper, "*strings.byteStringReplacer"),
    new(NewReplacer("12", "123"), "*strings.singleStringReplacer"),
    new(NewReplacer("1", "12"), "*strings.byteStringReplacer"),
    new(NewReplacer("", "X"), "*strings.genericReplacer"),
    new(NewReplacer("a", "1", "b", "12", "cde", "123"), "*strings.genericReplacer")
]);

// TestPickAlgorithm tests that NewReplacer picks the correct algorithm.
public static void TestPickAlgorithm(ж<testing.T> t) {
    foreach (var (i, tc) in range(algorithmTestCases)) {
        @string got = fmt.Sprintf("%T"u8, tc.r.ΔReplacer());
        if (got != tc.want) {
            t.Errorf("%d. algorithm = %s, want %s", i, got, tc.want);
        }
    }
}

partial struct errWriter {
}

internal static (nint n, error err) Write(this errWriter _, slice<byte> p) {
    return (0, fmt.Errorf("unwritable"u8));
}

// TestWriteStringError tests that WriteString returns an error
// received from the underlying io.Writer.
public static void TestWriteStringError(ж<testing.T> t) {
    foreach (var (i, tc) in range(algorithmTestCases)) {
        var (n, err) = tc.r.WriteString(new errWriter(), abcˢ);
        if (n != 0 || err == nil || err.Error() != "unwritable"u8) {
            t.Errorf("%d. WriteStringError = %d, %v, want 0, unwritable", i, n, err);
        }
    }
}

internal partial struct TestGenericTrieBuilding_testCases /*dyn*/ {
    internal @string @in, @out;
}

// TestGenericTrieBuilding verifies the structure of the generated trie. There
// is one node per line, and the key ending with the current line is in the
// trie if it ends with a "+".
public static void TestGenericTrieBuilding(ж<testing.T> t) {
    var wantbufᴷ = new stackAppend<byte>();

    var testCases = slice<TestGenericTrieBuilding_testCases>([
        new("abc;abdef;abdefgh;xx;xy;z", """
-
			a-
			.b-
			..c+
			..d-
			...ef+
			.....gh+
			x-
			.x+
			.y+
			z+
			
"""u8.Raw()),
        new("abracadabra;abracadabrakazam;abraham;abrasion", """
-
			a-
			.bra-
			....c-
			.....adabra+
			...........kazam+
			....h-
			.....am+
			....s-
			.....ion+
			
"""u8.Raw()),
        new("aaa;aa;a;i;longerst;longer;long;xx;x;X;Y", """
-
			X+
			Y+
			a+
			.a+
			..a+
			i+
			l-
			.ong+
			....er+
			......st+
			x+
			.x+
			
"""u8.Raw()),
        new("foo;;foo;foo1", """
+
			f-
			.oo+
			...1+
			
"""u8.Raw())
    ]);

    foreach (var (_, tc) in range(testCases)) {
        var keys = Split(tc.@in, ";");
        var args = new slice<@string>(len(keys) * 2);
        foreach (var (i, key) in range(keys)) {
            args[i * 2] = key;
        }

        @string got = NewReplacer(args.ꓸꓸꓸ).PrintTrie();
        // Remove tabs from tc.out
        var wantbuf = new slice<byte>(0, len(tc.@out));
        for (nint i = 0; i < len(tc.@out); i++) {
            if (tc.@out[i] != '\t') {
                wantbuf = append(wantbufᴷ, wantbuf, tc.@out[i]);
            }
        }
        @string want = (@string)wantbuf;

        if (got != want) {
            t.Errorf("PrintTrie(%q)\ngot\n%swant\n%s", tc.@in, got, want);
        }
    }
}

public static void BenchmarkGenericNoMatch(ж<testing.B> Ꮡb) {
    ref var b = ref Ꮡb.DerefOrNull();

    @string str = Repeat("A", 100) + Repeat("B", 100);
    var generic = NewReplacer("a", "A", "b", "B", "12", "123"); // varying lengths forces generic
    for (nint i = 0; i < b.N; i++) {
        generic.Replace(str);
    }
}

public static void BenchmarkGenericMatch1(ж<testing.B> Ꮡb) {
    ref var b = ref Ꮡb.DerefOrNull();

    @string str = Repeat("a", 100) + Repeat("b", 100);
    var generic = NewReplacer("a", "A", "b", "B", "12", "123");
    for (nint i = 0; i < b.N; i++) {
        generic.Replace(str);
    }
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static ref readonly @string itAposSLtBGtHtmlLtBGtˢ => ref itAposSLtBGtHtmlLtBGtˢᵇ;
private static readonly @string itAposSLtBGtHtmlLtBGtˢᵇ = "It&apos;s &lt;b&gt;HTML&lt;/b&gt;!"u8;

public static void BenchmarkGenericMatch2(ж<testing.B> Ꮡb) {
    ref var b = ref Ꮡb.DerefOrNull();

    @string str = Repeat(itAposSLtBGtHtmlLtBGtˢ, 100);
    for (nint i = 0; i < b.N; i++) {
        htmlUnescaper.Replace(str);
    }
}

internal static void benchmarkSingleString(ж<testing.B> Ꮡb, @string pattern, @string text) {
    ref var b = ref Ꮡb.DerefOrNull();

    var r = NewReplacer(pattern, matchˢ);
    b.SetBytes((int64)len(text));
    b.ResetTimer();
    for (nint i = 0; i < b.N; i++) {
        r.Replace(text);
    }
}

public static void BenchmarkSingleMaxSkipping(ж<testing.B> b) {
    benchmarkSingleString(b, Repeat("b", 25), Repeat("a", 10000));
}

public static void BenchmarkSingleLongSuffixFail(ж<testing.B> b) {
    benchmarkSingleString(b, "b"u8 + Repeat("a", 500), Repeat("a", 1002));
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static ref readonly @string abcdefˢ => ref abcdefˢᵇ;
private static readonly @string abcdefˢᵇ = "abcdef"u8;
internal static ref readonly @string abcdefghijklmnoˢ => ref abcdefghijklmnoˢᵇ;
private static readonly @string abcdefghijklmnoˢᵇ = "abcdefghijklmno"u8;

public static void BenchmarkSingleMatch(ж<testing.B> b) {
    benchmarkSingleString(b, abcdefˢ, Repeat(abcdefghijklmnoˢ, 1000));
}

public static void BenchmarkByteByteNoMatch(ж<testing.B> Ꮡb) {
    ref var b = ref Ꮡb.DerefOrNull();

    @string str = Repeat("A", 100) + Repeat("B", 100);
    for (nint i = 0; i < b.N; i++) {
        capitalLetters.Replace(str);
    }
}

public static void BenchmarkByteByteMatch(ж<testing.B> Ꮡb) {
    ref var b = ref Ꮡb.DerefOrNull();

    @string str = Repeat("a", 100) + Repeat("b", 100);
    for (nint i = 0; i < b.N; i++) {
        capitalLetters.Replace(str);
    }
}

public static void BenchmarkByteStringMatch(ж<testing.B> Ꮡb) {
    ref var b = ref Ꮡb.DerefOrNull();

    @string str = "<"u8 + Repeat("a", 99) + Repeat("b", 99) + ">"u8;
    for (nint i = 0; i < b.N; i++) {
        htmlEscaper.Replace(str);
    }
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static ref readonly @string i3ToEscapeHtmlOtherTextˢ => ref i3ToEscapeHtmlOtherTextˢᵇ;
private static readonly @string i3ToEscapeHtmlOtherTextˢᵇ = "I <3 to escape HTML & other text too."u8;

public static void BenchmarkHTMLEscapeNew(ж<testing.B> Ꮡb) {
    ref var b = ref Ꮡb.DerefOrNull();

    @string str = i3ToEscapeHtmlOtherTextˢ;
    for (nint i = 0; i < b.N; i++) {
        htmlEscaper.Replace(str);
    }
}

public static void BenchmarkHTMLEscapeOld(ж<testing.B> Ꮡb) {
    ref var b = ref Ꮡb.DerefOrNull();

    @string str = i3ToEscapeHtmlOtherTextˢ;
    for (nint i = 0; i < b.N; i++) {
        oldHTMLEscape(str);
    }
}

public static void BenchmarkByteStringReplacerWriteString(ж<testing.B> Ꮡb) {
    ref var b = ref Ꮡb.DerefOrNull();

    @string str = Repeat(i3ToEscapeHtmlOtherTextˢ, 100);
    var buf = @new<bytes.Buffer>();
    for (nint i = 0; i < b.N; i++) {
        htmlEscaper.WriteString(new strings_test_package.bytes_BufferжWriter(buf), str);
        buf.Reset();
    }
}

public static void BenchmarkByteReplacerWriteString(ж<testing.B> Ꮡb) {
    ref var b = ref Ꮡb.DerefOrNull();

    @string str = Repeat("abcdefghijklmnopqrstuvwxyz", 100);
    var buf = @new<bytes.Buffer>();
    for (nint i = 0; i < b.N; i++) {
        capitalLetters.WriteString(new strings_test_package.bytes_BufferжWriter(buf), str);
        buf.Reset();
    }
}

// BenchmarkByteByteReplaces compares byteByteImpl against multiple Replaces.
public static void BenchmarkByteByteReplaces(ж<testing.B> Ꮡb) {
    ref var b = ref Ꮡb.DerefOrNull();

    @string str = Repeat("a", 100) + Repeat("b", 100);
    for (nint i = 0; i < b.N; i++) {
        Replace(Replace(str, "a", "A", -1), "b", "B", -1);
    }
}

// BenchmarkByteByteMap compares byteByteImpl against Map.
public static void BenchmarkByteByteMap(ж<testing.B> Ꮡb) {
    ref var b = ref Ꮡb.DerefOrNull();

    @string str = Repeat("a", 100) + Repeat("b", 100);
    var fn = (rune r) => {
        switch (r) {
        case 'a': {
            return (rune)'A';
        }
        case 'b': {
            return (rune)'B';
        }}
        return r;
    };
    for (nint i = 0; i < b.N; i++) {
        Map(fn, str);
    }
}

partial struct mapdataᴛ1 /*dyn*/ {
    internal @string name, data;
}
internal static slice<mapdataᴛ1> mapdata = [
    new("ASCII", "a b c d e f g h i j k l m n o p q r s t u v w x y z"),
    new("Greek", "α β γ δ ε ζ η θ ι κ λ μ ν ξ ο π ρ ς σ τ υ φ χ ψ ω")
];

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static ref readonly @string identityˢ => ref identityˢᵇ;
private static readonly @string identityˢᵇ = "identity"u8;
internal static ref readonly @string changeˢ => ref changeˢᵇ;
private static readonly @string changeˢᵇ = "change"u8;

public static void BenchmarkMap(ж<testing.B> b) {
    var mapidentity = (rune r) => r;

    var mapidentityʗ1 = mapidentity;
    b.Run(identityˢ, (ж<testing.B> bΔ1) => {
        foreach (var (_, vᴛ1) in range(mapdata)) {
            ref var md = ref heap(new mapdataᴛ1(), out var Ꮡmd);
            md = vᴛ1;

            var mapidentityʗ2 = mapidentityʗ1;
            var mdʗ1 = md;
            bΔ1.Run(md.name, (ж<testing.B> bΔ2) => {
                for (nint i = 0; i < (~bΔ2).N; i++) {
                    Map(mapidentityʗ2, mdʗ1.data);
                }
            });
        }
    });

    var mapchange = (rune r) => {
        if ('a' <= r && r <= 'z') {
            return r + 'A' - 'a';
        }
        if ('α' <= r && r <= 'ω') {
            return r + 'Α' - 'α';
        }
        return r;
    };

    var mapchangeʗ1 = mapchange;
    b.Run(changeˢ, (ж<testing.B> bΔ3) => {
        foreach (var (_, vᴛ2) in range(mapdata)) {
            ref var md = ref heap(new mapdataᴛ1(), out var Ꮡmd);
            md = vᴛ2;

            var mapchangeʗ2 = mapchangeʗ1;
            var mdʗ2 = md;
            bΔ3.Run(md.name, (ж<testing.B> bΔ4) => {
                for (nint i = 0; i < (~bΔ4).N; i++) {
                    Map(mapchangeʗ2, mdʗ2.data);
                }
            });
        }
    });
}

} // end strings_test_package
