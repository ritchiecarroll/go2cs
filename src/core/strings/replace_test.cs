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
    "&"u8, "&amp;",
    "<", "&lt;",
    ">", "&gt;",
    @"""", "&quot;",
    "'", "&apos;");

internal static ж<strings.Replacer> htmlUnescaper = NewReplacer(
    "&amp;"u8, "&",
    "&lt;", "<",
    "&gt;", ">",
    "&quot;", @"""",
    "&apos;", "'");

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static readonly @string ampˢ = "&amp;"u8;
internal static readonly @string quotˢ = "&quot;"u8;
internal static readonly @string aposˢ = "&apos;"u8;

// The http package's old HTML escaping function.
internal static @string oldHTMLEscape(@string s) {
    s = Replace(s, "&"u8, ampˢ, -1);
    s = Replace(s, "<"u8, "&lt;"u8, -1);
    s = Replace(s, ">"u8, "&gt;"u8, -1);
    s = Replace(s, @""""u8, quotˢ, -1);
    s = Replace(s, "'"u8, aposˢ, -1);
    return s;
}

internal static ж<strings.Replacer> capitalLetters = NewReplacer("a"u8, "A", "b", "B");

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static readonly @string xxxˢ = "xxx"u8;
internal static readonly @string aaaˢ2 = "3[aaa]"u8;
internal static readonly @string longerstˢ = "longerst"u8;
internal static readonly @string mostLongˢ = "most long"u8;
internal static readonly @string longerˢ = "longer"u8;
internal static readonly @string mediumˢ = "medium"u8;
internal static readonly @string longˢ = "long"u8;
internal static readonly @string shortˢ = "short"u8;
internal static readonly @string rosesˢ = "roses"u8;
internal static readonly @string redˢ = "red"u8;
internal static readonly @string violetsˢ = "violets"u8;
internal static readonly @string blueˢ = "blue"u8;
internal static readonly @string sugarˢ = "sugar"u8;
internal static readonly @string sweetˢ = "sweet"u8;
internal static readonly @string abracadabraˢ = "abracadabra"u8;
internal static readonly @string poofˢ = "poof"u8;
internal static readonly @string abracadabrakazamˢ = "abracadabrakazam"u8;
internal static readonly @string splatˢ = "splat"u8;
internal static readonly @string abrahamˢ = "abraham"u8;
internal static readonly @string lincolnˢ = "lincoln"u8;
internal static readonly @string abrasionˢ = "abrasion"u8;
internal static readonly @string scrapeˢ = "scrape"u8;
internal static readonly @string isaacˢ = "isaac"u8;
internal static readonly @string foo1ˢ = "foo1"u8;
internal static readonly @string foo2ˢ = "foo2"u8;
internal static readonly @string foo3ˢ = "foo3"u8;
internal static readonly @string foo31ˢ = "foo31"u8;
internal static readonly @string foo32ˢ = "foo32"u8;
internal static readonly @string foo11ˢ = "foo11"u8;
internal static readonly @string foo12ˢ = "foo12"u8;
internal static readonly @string allˢ = "[all]"u8;
internal static readonly @string foobarˢ = "foobar"u8;
internal static readonly @string foobazˢ = "foobaz"u8;
internal static readonly @string matchˢ = "[match]"u8;
internal static readonly @string helloˢ3 = "Hello"u8;

internal partial struct TestReplacer_testCase /*dyn*/ {
    internal ж<strings.Replacer> r;
    internal @string @in, @out;
}

// TestReplacer tests the replacer implementations.
public static void TestReplacer(ж<testing.T> Ꮡt) {
    ref var t = ref Ꮡt.DerefOrNull();

    slice<TestReplacer_testCase> testCases = default!;
    // str converts 0xff to "\xff". This isn't just string(b) since that converts to UTF-8.
    @string str(byte b) => ((@string)new byte[]{b}.slice());
    slice<@string> s = default!;
    // inc maps "\x00"->"\x01", ..., "a"->"b", "b"->"c", ..., "\xff"->"\x00".
    s = default!;
    for (nint i = 0; i < 256; i++) {
        s = append(s, str((byte)i), str((byte)(i + 1)));
    }
    var inc = NewReplacer(s.ꓸꓸꓸ);
    // Test cases with 1-byte old strings, 1-byte new strings.
    testCases = append(testCases,
        new TestReplacer_testCase(capitalLetters, "brad"u8, "BrAd"u8),
        new TestReplacer_testCase(capitalLetters, Repeat("a"u8, ((32 << (int)(10))) + 123), Repeat("A"u8, ((32 << (int)(10))) + 123)),
        new TestReplacer_testCase(capitalLetters, ""u8, ""u8),
        new TestReplacer_testCase(inc, "brad"u8, "csbe"u8),
        new TestReplacer_testCase(inc, ((@string)(new byte[]{0x00, 0xff})), "\x01\x00"u8),
        new TestReplacer_testCase(inc, ""u8, ""u8),
        new TestReplacer_testCase(NewReplacer("a"u8, "1", "a", "2"), "brad"u8, "br1d"u8));
    // repeat maps "a"->"a", "b"->"bb", "c"->"ccc", ...
    s = default!;
    for (nint i = 0; i < 256; i++) {
        nint n = i + 1 - (rune)'a';
        if (n < 1) {
            n = 1;
        }
        s = append(s, str((byte)i), Repeat(str((byte)i), n));
    }
    var repeat = NewReplacer(s.ꓸꓸꓸ);
    // Test cases with 1-byte old strings, variable length new strings.
    testCases = append(testCases,
        new TestReplacer_testCase(htmlEscaper, "No changes"u8, "No changes"u8),
        new TestReplacer_testCase(htmlEscaper, "I <3 escaping & stuff"u8, "I &lt;3 escaping &amp; stuff"u8),
        new TestReplacer_testCase(htmlEscaper, "&&&"u8, "&amp;&amp;&amp;"u8),
        new TestReplacer_testCase(htmlEscaper, ""u8, ""u8),
        new TestReplacer_testCase(repeat, "brad"u8, "bbrrrrrrrrrrrrrrrrrradddd"u8),
        new TestReplacer_testCase(repeat, "abba"u8, "abbbba"u8),
        new TestReplacer_testCase(repeat, ""u8, ""u8),
        new TestReplacer_testCase(NewReplacer("a"u8, "11", "a", "22"), "brad"u8, "br11d"u8));
    // The remaining test cases have variable length old strings.
    testCases = append(testCases,
        new TestReplacer_testCase(htmlUnescaper, "&amp;amp;"u8, "&amp;"u8),
        new TestReplacer_testCase(htmlUnescaper, "&lt;b&gt;HTML&apos;s neat&lt;/b&gt;"u8, "<b>HTML's neat</b>"u8),
        new TestReplacer_testCase(htmlUnescaper, ""u8, ""u8),
        new TestReplacer_testCase(NewReplacer("a"u8, "1", "a", "2", xxxˢ, xxxˢ), "brad"u8, "br1d"u8),
        new TestReplacer_testCase(NewReplacer("a"u8, "1", "aa", "2", aaaˢ, "3"), "aaaa"u8, "1111"u8),
        new TestReplacer_testCase(NewReplacer(aaaˢ, "3", "aa", "2", "a", "1"), "aaaa"u8, "31"u8));
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
        new TestReplacer_testCase(gen1, "fooaaabar"u8, "foo3[aaa]b1[a]r"u8),
        new TestReplacer_testCase(gen1, "long, longerst, longer"u8, "short, most long, medium"u8),
        new TestReplacer_testCase(gen1, "xxxxx"u8, "xxxxX"u8),
        new TestReplacer_testCase(gen1, "XiX"u8, "YiY"u8),
        new TestReplacer_testCase(gen1, ""u8, ""u8));
    // gen2 has multiple old strings with no pairwise common prefix.
    var gen2 = NewReplacer(
        rosesˢ, redˢ,
        violetsˢ, blueˢ,
        sugarˢ, sweetˢ);
    testCases = append(testCases,
        new TestReplacer_testCase(gen2, "roses are red, violets are blue..."u8, "red are red, blue are blue..."u8),
        new TestReplacer_testCase(gen2, ""u8, ""u8));
    // gen3 has multiple old strings with an overall common prefix.
    var gen3 = NewReplacer(
        abracadabraˢ, poofˢ,
        abracadabrakazamˢ, splatˢ,
        abrahamˢ, lincolnˢ,
        abrasionˢ, scrapeˢ,
        abrahamˢ, isaacˢ);
    testCases = append(testCases,
        new TestReplacer_testCase(gen3, "abracadabrakazam abraham"u8, "poofkazam lincoln"u8),
        new TestReplacer_testCase(gen3, "abrasion abracad"u8, "scrape abracad"u8),
        new TestReplacer_testCase(gen3, "abba abram abrasive"u8, "abba abram abrasive"u8),
        new TestReplacer_testCase(gen3, ""u8, ""u8));
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
        new TestReplacer_testCase(foo1, "fofoofoo12foo32oo"u8, "fofooA2C2oo"u8),
        new TestReplacer_testCase(foo1, ""u8, ""u8),
        new TestReplacer_testCase(foo2, "fofoofoo12foo32oo"u8, "fofooA2Doo"u8),
        new TestReplacer_testCase(foo2, ""u8, ""u8),
        new TestReplacer_testCase(foo3, "fofoofoo12foo32oo"u8, "fofooBDoo"u8),
        new TestReplacer_testCase(foo3, ""u8, ""u8),
        new TestReplacer_testCase(foo4, "fofoofoo12foo32oo"u8, "fofooBDoo"u8),
        new TestReplacer_testCase(foo4, ""u8, ""u8));
    // genAll maps "\x00\x01\x02...\xfe\xff" to "[all]", amongst other things.
    var allBytes = new slice<byte>(256);
    foreach (var (i, _) in allBytes) {
        allBytes[i] = (byte)i;
    }
    @string allString = ((@string)allBytes);
    var genAll = NewReplacer(
        allString, allˢ,
        ((@string)(new byte[]{0xff})), "[ff]",
        "\x00", "[00]");
    testCases = append(testCases,
        new TestReplacer_testCase(genAll, allString, "[all]"u8),
        new TestReplacer_testCase(genAll, ((@string)(new byte[]{0x61, 0xff})) + allString + "\x00"u8, "a[ff][all][00]"u8),
        new TestReplacer_testCase(genAll, ""u8, ""u8));
    // Test cases with empty old strings.
    var blankToX1 = NewReplacer(""u8, "X");
    var blankToX2 = NewReplacer(""u8, "X", "", "");
    var blankHighPriority = NewReplacer(""u8, "X", "o", "O");
    var blankLowPriority = NewReplacer("o"u8, "O", "", "X");
    var blankNoOp1 = NewReplacer(""u8, "");
    var blankNoOp2 = NewReplacer(""u8, "", "", "A");
    var blankFoo = NewReplacer(""u8, "X", foobarˢ, "R", foobazˢ, "Z");
    testCases = append(testCases,
        new TestReplacer_testCase(blankToX1, "foo"u8, "XfXoXoX"u8),
        new TestReplacer_testCase(blankToX1, ""u8, "X"u8),
        new TestReplacer_testCase(blankToX2, "foo"u8, "XfXoXoX"u8),
        new TestReplacer_testCase(blankToX2, ""u8, "X"u8),
        new TestReplacer_testCase(blankHighPriority, "oo"u8, "XOXOX"u8),
        new TestReplacer_testCase(blankHighPriority, "ii"u8, "XiXiX"u8),
        new TestReplacer_testCase(blankHighPriority, "oiio"u8, "XOXiXiXOX"u8),
        new TestReplacer_testCase(blankHighPriority, "iooi"u8, "XiXOXOXiX"u8),
        new TestReplacer_testCase(blankHighPriority, ""u8, "X"u8),
        new TestReplacer_testCase(blankLowPriority, "oo"u8, "OOX"u8),
        new TestReplacer_testCase(blankLowPriority, "ii"u8, "XiXiX"u8),
        new TestReplacer_testCase(blankLowPriority, "oiio"u8, "OXiXiOX"u8),
        new TestReplacer_testCase(blankLowPriority, "iooi"u8, "XiOOXiX"u8),
        new TestReplacer_testCase(blankLowPriority, ""u8, "X"u8),
        new TestReplacer_testCase(blankNoOp1, "foo"u8, "foo"u8),
        new TestReplacer_testCase(blankNoOp1, ""u8, ""u8),
        new TestReplacer_testCase(blankNoOp2, "foo"u8, "foo"u8),
        new TestReplacer_testCase(blankNoOp2, ""u8, ""u8),
        new TestReplacer_testCase(blankFoo, "foobarfoobaz"u8, "XRXZX"u8),
        new TestReplacer_testCase(blankFoo, "foobar-foobaz"u8, "XRX-XZX"u8),
        new TestReplacer_testCase(blankFoo, ""u8, "X"u8));
    // single string replacer
    var abcMatcher = NewReplacer(abcˢ, matchˢ);
    testCases = append(testCases,
        new TestReplacer_testCase(abcMatcher, ""u8, ""u8),
        new TestReplacer_testCase(abcMatcher, "ab"u8, "ab"u8),
        new TestReplacer_testCase(abcMatcher, "abc"u8, "[match]"u8),
        new TestReplacer_testCase(abcMatcher, "abcd"u8, "[match]d"u8),
        new TestReplacer_testCase(abcMatcher, "cabcabcdabca"u8, "c[match][match]d[match]a"u8));
    // Issue 6659 cases (more single string replacer)
    var noHello = NewReplacer(helloˢ3, "");
    testCases = append(testCases,
        new TestReplacer_testCase(noHello, "Hello"u8, ""u8),
        new TestReplacer_testCase(noHello, "Hellox"u8, "x"u8),
        new TestReplacer_testCase(noHello, "xHello"u8, "x"u8),
        new TestReplacer_testCase(noHello, "xHellox"u8, "xx"u8));
    // No-arg test cases.
    var nop = NewReplacer();
    testCases = append(testCases,
        new TestReplacer_testCase(nop, "abc"u8, "abc"u8),
        new TestReplacer_testCase(nop, ""u8, ""u8));
    // Run the test cases.
    foreach (var (i, tc) in testCases) {
        {
            @string sΔ1 = tc.r.Replace(tc.@in); if (sΔ1 != tc.@out) {
                Ꮡt.Errorf("%d. Replace(%q) = %q, want %q"u8, i, tc.@in, sΔ1, tc.@out);
            }
        }
        ref var buf = ref heap(new bytes.Buffer(), out var Ꮡbuf);
        var (n, err) = tc.r.WriteString(new strings_test_package.bytes_BufferжWriter(Ꮡbuf), tc.@in);
        if (err != default!) {
            Ꮡt.Errorf("%d. WriteString: %v"u8, i, err);
            continue;
        }
        @string got = Ꮡbuf.String();
        if (got != tc.@out) {
            Ꮡt.Errorf("%d. WriteString(%q) wrote %q, want %q"u8, i, tc.@in, got, tc.@out);
            continue;
        }
        if (n != len(tc.@out)) {
            Ꮡt.Errorf("%d. WriteString(%q) wrote correct string but reported %d bytes; want %d (%q)"u8,
                i, tc.@in, n, len(tc.@out), tc.@out);
        }
    }
}


partial struct algorithmTestCasesᴛ1 /*dyn*/ {
    internal ж<strings.Replacer> r;
    internal @string want;
}
internal static slice<algorithmTestCasesᴛ1> algorithmTestCases = new algorithmTestCasesᴛ1[]{
    new(capitalLetters, "*strings.byteReplacer"u8),
    new(htmlEscaper, "*strings.byteStringReplacer"u8),
    new(NewReplacer("12"u8, "123"), "*strings.singleStringReplacer"u8),
    new(NewReplacer("1"u8, "12"), "*strings.byteStringReplacer"u8),
    new(NewReplacer(""u8, "X"), "*strings.genericReplacer"u8),
    new(NewReplacer("a"u8, "1", "b", "12", "cde", "123"), "*strings.genericReplacer"u8)
}.slice();

// TestPickAlgorithm tests that NewReplacer picks the correct algorithm.
public static void TestPickAlgorithm(ж<testing.T> Ꮡt) {
    foreach (var (i, tc) in algorithmTestCases) {
        @string got = fmt.Sprintf("%T"u8, tc.r.ΔReplacer());
        if (got != tc.want) {
            Ꮡt.Errorf("%d. algorithm = %s, want %s"u8, i, got, tc.want);
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
public static void TestWriteStringError(ж<testing.T> Ꮡt) {
    foreach (var (i, tc) in algorithmTestCases) {
        var (n, err) = tc.r.WriteString(new errWriter(nil), abcˢ);
        if (n != 0 || err == default! || err.Error() != "unwritable"u8) {
            Ꮡt.Errorf("%d. WriteStringError = %d, %v, want 0, unwritable"u8, i, n, err);
        }
    }
}

internal partial struct TestGenericTrieBuilding_testCases /*dyn*/ {
    internal @string @in, @out;
}

// TestGenericTrieBuilding verifies the structure of the generated trie. There
// is one node per line, and the key ending with the current line is in the
// trie if it ends with a "+".
public static void TestGenericTrieBuilding(ж<testing.T> Ꮡt) {
    ref var t = ref Ꮡt.DerefOrNull();

    var testCases = new TestGenericTrieBuilding_testCases[]{
        new("abc;abdef;abdefgh;xx;xy;z"u8, """
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
			
"""u8),
        new("abracadabra;abracadabrakazam;abraham;abrasion"u8, """
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
			
"""u8),
        new("aaa;aa;a;i;longerst;longer;long;xx;x;X;Y"u8, """
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
			
"""u8),
        new("foo;;foo;foo1"u8, """
+
			f-
			.oo+
			...1+
			
"""u8)
    }.slice();
    foreach (var (_, tc) in testCases) {
        var keys = Split(tc.@in, ";"u8);
        var args = new slice<@string>(len(keys) * 2);
        foreach (var (i, key) in keys) {
            args[i * 2] = key;
        }
        @string got = NewReplacer(args.ꓸꓸꓸ).PrintTrie();
        // Remove tabs from tc.out
        var wantbuf = new slice<byte>(0, len(tc.@out));
        for (nint i = 0; i < len(tc.@out); i++) {
            if (tc.@out[i] != (rune)'\t') {
                wantbuf = append(wantbuf, tc.@out[i]);
            }
        }
        @string want = ((@string)wantbuf);
        if (got != want) {
            Ꮡt.Errorf("PrintTrie(%q)\ngot\n%swant\n%s"u8, tc.@in, got, want);
        }
    }
}

public static void BenchmarkGenericNoMatch(ж<testing.B> Ꮡb) {
    ref var b = ref Ꮡb.DerefOrNull();

    @string str = Repeat("A"u8, 100) + Repeat("B"u8, 100);
    var generic = NewReplacer("a"u8, "A", "b", "B", "12", "123"); // varying lengths forces generic
    for (nint i = 0; i < b.N; i++) {
        generic.Replace(str);
    }
}

public static void BenchmarkGenericMatch1(ж<testing.B> Ꮡb) {
    ref var b = ref Ꮡb.DerefOrNull();

    @string str = Repeat("a"u8, 100) + Repeat("b"u8, 100);
    var generic = NewReplacer("a"u8, "A", "b", "B", "12", "123");
    for (nint i = 0; i < b.N; i++) {
        generic.Replace(str);
    }
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static readonly @string itAposSLtBGtHtmlLtBGtˢ = "It&apos;s &lt;b&gt;HTML&lt;/b&gt;!"u8;

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

public static void BenchmarkSingleMaxSkipping(ж<testing.B> Ꮡb) {
    ref var b = ref Ꮡb.DerefOrNull();

    benchmarkSingleString(Ꮡb, Repeat("b"u8, 25), Repeat("a"u8, 10000));
}

public static void BenchmarkSingleLongSuffixFail(ж<testing.B> Ꮡb) {
    ref var b = ref Ꮡb.DerefOrNull();

    benchmarkSingleString(Ꮡb, "b"u8 + Repeat("a"u8, 500), Repeat("a"u8, 1002));
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static readonly @string abcdefˢ = "abcdef"u8;
internal static readonly @string abcdefghijklmnoˢ = "abcdefghijklmno"u8;

public static void BenchmarkSingleMatch(ж<testing.B> Ꮡb) {
    benchmarkSingleString(Ꮡb, abcdefˢ, Repeat(abcdefghijklmnoˢ, 1000));
}

public static void BenchmarkByteByteNoMatch(ж<testing.B> Ꮡb) {
    ref var b = ref Ꮡb.DerefOrNull();

    @string str = Repeat("A"u8, 100) + Repeat("B"u8, 100);
    for (nint i = 0; i < b.N; i++) {
        capitalLetters.Replace(str);
    }
}

public static void BenchmarkByteByteMatch(ж<testing.B> Ꮡb) {
    ref var b = ref Ꮡb.DerefOrNull();

    @string str = Repeat("a"u8, 100) + Repeat("b"u8, 100);
    for (nint i = 0; i < b.N; i++) {
        capitalLetters.Replace(str);
    }
}

public static void BenchmarkByteStringMatch(ж<testing.B> Ꮡb) {
    ref var b = ref Ꮡb.DerefOrNull();

    @string str = "<"u8 + Repeat("a"u8, 99) + Repeat("b"u8, 99) + ">"u8;
    for (nint i = 0; i < b.N; i++) {
        htmlEscaper.Replace(str);
    }
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static readonly @string i3ToEscapeHtmlOtherTextˢ = "I <3 to escape HTML & other text too."u8;

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

    @string str = Repeat("abcdefghijklmnopqrstuvwxyz"u8, 100);
    var buf = @new<bytes.Buffer>();
    for (nint i = 0; i < b.N; i++) {
        capitalLetters.WriteString(new strings_test_package.bytes_BufferжWriter(buf), str);
        buf.Reset();
    }
}

// BenchmarkByteByteReplaces compares byteByteImpl against multiple Replaces.
public static void BenchmarkByteByteReplaces(ж<testing.B> Ꮡb) {
    ref var b = ref Ꮡb.DerefOrNull();

    @string str = Repeat("a"u8, 100) + Repeat("b"u8, 100);
    for (nint i = 0; i < b.N; i++) {
        Replace(Replace(str, "a"u8, "A"u8, -1), "b"u8, "B"u8, -1);
    }
}

// BenchmarkByteByteMap compares byteByteImpl against Map.
public static void BenchmarkByteByteMap(ж<testing.B> Ꮡb) {
    ref var b = ref Ꮡb.DerefOrNull();

    @string str = Repeat("a"u8, 100) + Repeat("b"u8, 100);
    var fn = (rune r) => {
        switch (r) {
        case (rune)'a': {
            return (rune)'A';
        }
        case (rune)'b': {
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
internal static slice<mapdataᴛ1> mapdata = new mapdataᴛ1[]{
    new("ASCII"u8, "a b c d e f g h i j k l m n o p q r s t u v w x y z"u8),
    new("Greek"u8, "α β γ δ ε ζ η θ ι κ λ μ ν ξ ο π ρ ς σ τ υ φ χ ψ ω"u8)
}.slice();

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static readonly @string identityˢ = "identity"u8;
internal static readonly @string changeˢ = "change"u8;

public static void BenchmarkMap(ж<testing.B> Ꮡb) {
    var mapidentity = (rune r) => r;
    var mapidentityʗ1 = mapidentity;
    Ꮡb.Run(identityˢ, (ж<testing.B> bΔ1) => {
        foreach (var (_, vᴛ1) in mapdata) {
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
        if ((rune)'a' <= r && r <= (rune)'z') {
            return r + (rune)'A' - (rune)'a';
        }
        if ((rune)'α' <= r && r <= (rune)'ω') {
            return r + (rune)'Α' - (rune)'α';
        }
        return r;
    };
    var mapchangeʗ1 = mapchange;
    Ꮡb.Run(changeˢ, (ж<testing.B> bΔ3) => {
        foreach (var (_, vᴛ2) in mapdata) {
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
