// Copyright 2009 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.
namespace go.encoding;

using bytes = bytes_package;
using fmt = fmt_package;
using io = io_package;
using reflect = reflect_package;
using strings = strings_package;
using testing = testing_package;
using utf8 = go.unicode.utf8_package;
using go.unicode;
using static go.encoding.xml_package;

partial class xml_internal_test_package {

internal partial struct toks {
    internal bool earlyEOF;
    internal slice<ΔToken> t;
}

internal static (ΔToken, error) Token(this ref toks t) {
    if (len(t.t) == 0) {
        return (default, io.EOF);
    }
    ΔToken tok = default;
    (tok, t.t) = (t.t[0], t.t[1..]);
    if (t.earlyEOF && len(t.t) == 0) {
        return (tok, io.EOF);
    }
    return (tok, default);
}

internal partial struct TestDecodeEOF_tests /*dyn*/ {
    internal @string name;
    internal slice<ΔToken> tokens;
    internal bool ok;
}

internal partial struct TestDecodeEOF_type /*dyn*/ {
    public global::go.encoding.xml_package.Name XMLName; /*`xml:"test"`*/
}

public static void TestDecodeEOF(ж<testing.T> t) {
    var start = new StartElement(Name: new Name(Local: "test"));
    var tests = slice<TestDecodeEOF_tests>([
        new(
            name: "OK",
            tokens: slice<ΔToken>([
                start,
                start.End()
            ]),
            ok: true
        ),
        new(
            name: "Malformed",
            tokens: slice<ΔToken>([
                start,
                new StartElement(Name: new Name(Local: "bad")),
                start.End()
            ]),
            ok: false
        )
    ]);
    foreach (var (_, vᴛ1) in range(tests)) {
        ref var tc = ref heap(new TestDecodeEOF_tests(), out var Ꮡtc);
        tc = vᴛ1;

        foreach (var (_, vᴛ2) in range(slice<bool>([true, false]))) {
            ref var eof = ref heap(new bool(), out var Ꮡeof);
            eof = vᴛ2;

            @string name = fmt.Sprintf("%s/earlyEOF=%v"u8, tc.name, eof);
            var eofʗ1 = eof;
            var tcʗ1 = tc;
            t.Run(name, (ж<testing.T> tΔ1) => {
                var d = NewTokenDecoder(new xml_internal_test_package.toksжTokenReader(Ꮡ(new toks(
                    earlyEOF: eofʗ1,
                    t: tcʗ1.tokens
                ))));
                var err = d.Decode(Ꮡ(new TestDecodeEOF_type()));
                if (tcʗ1.ok && err != nil) {
                    tΔ1.Fatalf("d.Decode: expected nil error, got %v", err);
                }
                var (_, ok) = err._<ж<global::go.encoding.xml_package.SyntaxError>>(ᐧ); if (!tcʗ1.ok && !ok) {
                    tΔ1.Errorf("d.Decode: expected syntax error, got %v", err);
                }
            });
        }
    }
}

internal partial struct toksNil {
    internal bool returnEOF;
    internal slice<ΔToken> t;
}

internal static (ΔToken, error) Token(this ref toksNil t) {
    if (len(t.t) == 0) {
        if (!t.returnEOF) {
            // Return nil, nil before returning an EOF. It's legal, but
            // discouraged.
            t.returnEOF = true;
            return (default, default);
        }
        return (default, io.EOF);
    }
    ΔToken tok = default;
    (tok, t.t) = (t.t[0], t.t[1..]);
    return (tok, default);
}

internal partial struct TestDecodeNilToken_type /*dyn*/ {
    public global::go.encoding.xml_package.Name XMLName; /*`xml:"test"`*/
}

public static void TestDecodeNilToken(ж<testing.T> t) {
    foreach (var (_, strict) in range(slice<bool>([true, false]))) {
        @string name = fmt.Sprintf("Strict=%v"u8, strict);
        t.Run(name, (ж<testing.T> tΔ1) => {
            var start = new StartElement(Name: new Name(Local: "test"));
            var bad = new StartElement(Name: new Name(Local: "bad"));
            var d = NewTokenDecoder(new xml_internal_test_package.toksNilжTokenReader(Ꮡ(new toksNil(
                // Malformed
                t: slice<ΔToken>([start, bad, start.End()])
            ))));
            d.Strict = strict;
            var err = d.Decode(Ꮡ(new TestDecodeNilToken_type()));
            var (_, ok) = err._<ж<global::go.encoding.xml_package.SyntaxError>>(ᐧ); if (!ok) {
                tΔ1.Errorf("d.Decode: expected syntax error, got %v", err);
            }
        });
    }
}

internal static ref readonly @string testInput => ref testInputᵇ;
private static readonly @string testInputᵇ = "\n<?xml version=\"1.0\" encoding=\"UTF-8\"?>\n<!DOCTYPE html PUBLIC \"-//W3C//DTD XHTML 1.0 Transitional//EN\"\n  \"http://www.w3.org/TR/xhtml1/DTD/xhtml1-transitional.dtd\">\n<body xmlns:foo=\"ns1\" xmlns=\"ns2\" xmlns:tag=\"ns3\" \r\n\t  >\n  <hello lang=\"en\">World &lt;&gt;&apos;&quot; &#x767d;&#40300;翔</hello>\n  <query>&何; &is-it;</query>\n  <goodbye />\n  <outer foo:attr=\"value\" xmlns:tag=\"ns4\">\n    <inner/>\n  </outer>\n  <tag:name>\n    <![CDATA[Some text here.]]>\n  </tag:name>\n</body><!-- missing final newline -->";

internal static map<@string, @string> testEntity = new map<@string, @string>{["何"] = "What", ["is-it"] = "is it?"};

internal static slice<ΔToken> rawTokens = [
    (global::go.encoding.xml_package.CharData)slice<byte>((@string)"\n"u8),
    new ProcInst("xml", slice<byte>(@"version=""1.0"" encoding=""UTF-8"""u8)),
    (global::go.encoding.xml_package.CharData)slice<byte>((@string)"\n"u8),
    (global::go.encoding.xml_package.Directive)slice<byte>((@string)"""
DOCTYPE html PUBLIC "-//W3C//DTD XHTML 1.0 Transitional//EN"
  "http://www.w3.org/TR/xhtml1/DTD/xhtml1-transitional.dtd"
"""u8.Raw()),
    (global::go.encoding.xml_package.CharData)slice<byte>((@string)"\n"u8),
    new StartElement(new Name("", "body"), [new(new Name("xmlns", "foo"), "ns1"), new(new Name("", "xmlns"), "ns2"), new(new Name("xmlns", "tag"), "ns3")]),
    (global::go.encoding.xml_package.CharData)slice<byte>((@string)"\n  "u8),
    new StartElement(new Name("", "hello"), [new(new Name("", "lang"), "en")]),
    (global::go.encoding.xml_package.CharData)slice<byte>((@string)"World <>'\" 白鵬翔"u8),
    new EndElement(new Name("", "hello")),
    (global::go.encoding.xml_package.CharData)slice<byte>((@string)"\n  "u8),
    new StartElement(new Name("", "query"), []),
    (global::go.encoding.xml_package.CharData)slice<byte>((@string)"What is it?"u8),
    new EndElement(new Name("", "query")),
    (global::go.encoding.xml_package.CharData)slice<byte>((@string)"\n  "u8),
    new StartElement(new Name("", "goodbye"), []),
    new EndElement(new Name("", "goodbye")),
    (global::go.encoding.xml_package.CharData)slice<byte>((@string)"\n  "u8),
    new StartElement(new Name("", "outer"), [new(new Name("foo", "attr"), "value"), new(new Name("xmlns", "tag"), "ns4")]),
    (global::go.encoding.xml_package.CharData)slice<byte>((@string)"\n    "u8),
    new StartElement(new Name("", "inner"), []),
    new EndElement(new Name("", "inner")),
    (global::go.encoding.xml_package.CharData)slice<byte>((@string)"\n  "u8),
    new EndElement(new Name("", "outer")),
    (global::go.encoding.xml_package.CharData)slice<byte>((@string)"\n  "u8),
    new StartElement(new Name("tag", "name"), []),
    (global::go.encoding.xml_package.CharData)slice<byte>((@string)"\n    "u8),
    (global::go.encoding.xml_package.CharData)slice<byte>((@string)"Some text here."u8),
    (global::go.encoding.xml_package.CharData)slice<byte>((@string)"\n  "u8),
    new EndElement(new Name("tag", "name")),
    (global::go.encoding.xml_package.CharData)slice<byte>((@string)"\n"u8),
    new EndElement(new Name("", "body")),
    (global::go.encoding.xml_package.Comment)slice<byte>((@string)" missing final newline "u8)
];

internal static slice<ΔToken> cookedTokens = [
    (global::go.encoding.xml_package.CharData)slice<byte>((@string)"\n"u8),
    new ProcInst("xml", slice<byte>(@"version=""1.0"" encoding=""UTF-8"""u8)),
    (global::go.encoding.xml_package.CharData)slice<byte>((@string)"\n"u8),
    (global::go.encoding.xml_package.Directive)slice<byte>((@string)"""
DOCTYPE html PUBLIC "-//W3C//DTD XHTML 1.0 Transitional//EN"
  "http://www.w3.org/TR/xhtml1/DTD/xhtml1-transitional.dtd"
"""u8.Raw()),
    (global::go.encoding.xml_package.CharData)slice<byte>((@string)"\n"u8),
    new StartElement(new Name("ns2", "body"), [new(new Name("xmlns", "foo"), "ns1"), new(new Name("", "xmlns"), "ns2"), new(new Name("xmlns", "tag"), "ns3")]),
    (global::go.encoding.xml_package.CharData)slice<byte>((@string)"\n  "u8),
    new StartElement(new Name("ns2", "hello"), [new(new Name("", "lang"), "en")]),
    (global::go.encoding.xml_package.CharData)slice<byte>((@string)"World <>'\" 白鵬翔"u8),
    new EndElement(new Name("ns2", "hello")),
    (global::go.encoding.xml_package.CharData)slice<byte>((@string)"\n  "u8),
    new StartElement(new Name("ns2", "query"), []),
    (global::go.encoding.xml_package.CharData)slice<byte>((@string)"What is it?"u8),
    new EndElement(new Name("ns2", "query")),
    (global::go.encoding.xml_package.CharData)slice<byte>((@string)"\n  "u8),
    new StartElement(new Name("ns2", "goodbye"), []),
    new EndElement(new Name("ns2", "goodbye")),
    (global::go.encoding.xml_package.CharData)slice<byte>((@string)"\n  "u8),
    new StartElement(new Name("ns2", "outer"), [new(new Name("ns1", "attr"), "value"), new(new Name("xmlns", "tag"), "ns4")]),
    (global::go.encoding.xml_package.CharData)slice<byte>((@string)"\n    "u8),
    new StartElement(new Name("ns2", "inner"), []),
    new EndElement(new Name("ns2", "inner")),
    (global::go.encoding.xml_package.CharData)slice<byte>((@string)"\n  "u8),
    new EndElement(new Name("ns2", "outer")),
    (global::go.encoding.xml_package.CharData)slice<byte>((@string)"\n  "u8),
    new StartElement(new Name("ns3", "name"), []),
    (global::go.encoding.xml_package.CharData)slice<byte>((@string)"\n    "u8),
    (global::go.encoding.xml_package.CharData)slice<byte>((@string)"Some text here."u8),
    (global::go.encoding.xml_package.CharData)slice<byte>((@string)"\n  "u8),
    new EndElement(new Name("ns3", "name")),
    (global::go.encoding.xml_package.CharData)slice<byte>((@string)"\n"u8),
    new EndElement(new Name("ns2", "body")),
    (global::go.encoding.xml_package.Comment)slice<byte>((@string)" missing final newline "u8)
];

internal static ref readonly @string testInputAltEncoding => ref testInputAltEncodingᵇ;
private static readonly @string testInputAltEncodingᵇ = """

<?xml version="1.0" encoding="x-testing-uppercase"?>
<TAG>VALUE</TAG>
"""u8.Raw();

internal static slice<ΔToken> rawTokensAltEncoding = [
    (global::go.encoding.xml_package.CharData)slice<byte>((@string)"\n"u8),
    new ProcInst("xml", slice<byte>(@"version=""1.0"" encoding=""x-testing-uppercase"""u8)),
    (global::go.encoding.xml_package.CharData)slice<byte>((@string)"\n"u8),
    new StartElement(new Name("", "tag"), []),
    (global::go.encoding.xml_package.CharData)slice<byte>((@string)"value"u8),
    new EndElement(new Name("", "tag"))
];

internal static slice<@string> xmlInput = [
    // unexpected EOF cases
    "<",
    "<t",
    "<t ",
    "<t/",
    "<!",
    "<!-",
    "<!--",
    "<!--c-",
    "<!--c--",
    "<!d",
    "<t></",
    "<t></t",
    "<?",
    "<?p",
    "<t a",
    "<t a=",
    "<t a='",
    "<t a=''",
    "<t/><![",
    "<t/><![C",
    "<t/><![CDATA[d",
    "<t/><![CDATA[d]",
    "<t/><![CDATA[d]]",

    // other Syntax errors
    "<>",
    "<t/a",
    "<0 />",
    "<?0 >",
    //	"<!0 >",	// let the Token() caller handle
    "</0>",
    "<t 0=''>",
    "<t a='&'>",
    "<t a='<'>",
    "<t>&nbspc;</t>",
    "<t a>",
    "<t a=>",
    "<t a=v>",
    //	"<![CDATA[d]]>",	// let the Token() caller handle
    "<t></e>",
    "<t></>",
    "<t></t!",
    "<t>cdata]]></t>"
];

public static void TestRawToken(ж<testing.T> t) {
    var d = NewDecoder(new xml_test_package.strings_ReaderжReader(strings.NewReader(testInput)));
    d.Entity = testEntity;
    testRawToken(t, d, testInput, rawTokens);
}

internal static ref readonly @string nonStrictInput => ref nonStrictInputᵇ;
private static readonly @string nonStrictInputᵇ = """

<tag>non&entity</tag>
<tag>&unknown;entity</tag>
<tag>&#123</tag>
<tag>&#zzz;</tag>
<tag>&なまえ3;</tag>
<tag>&lt-gt;</tag>
<tag>&;</tag>
<tag>&0a;</tag>

"""u8.Raw();

internal static slice<ΔToken> nonStrictTokens = [
    (global::go.encoding.xml_package.CharData)slice<byte>((@string)"\n"u8),
    new StartElement(new Name("", "tag"), []),
    (global::go.encoding.xml_package.CharData)slice<byte>((@string)"non&entity"u8),
    new EndElement(new Name("", "tag")),
    (global::go.encoding.xml_package.CharData)slice<byte>((@string)"\n"u8),
    new StartElement(new Name("", "tag"), []),
    (global::go.encoding.xml_package.CharData)slice<byte>((@string)"&unknown;entity"u8),
    new EndElement(new Name("", "tag")),
    (global::go.encoding.xml_package.CharData)slice<byte>((@string)"\n"u8),
    new StartElement(new Name("", "tag"), []),
    (global::go.encoding.xml_package.CharData)slice<byte>((@string)"&#123"u8),
    new EndElement(new Name("", "tag")),
    (global::go.encoding.xml_package.CharData)slice<byte>((@string)"\n"u8),
    new StartElement(new Name("", "tag"), []),
    (global::go.encoding.xml_package.CharData)slice<byte>((@string)"&#zzz;"u8),
    new EndElement(new Name("", "tag")),
    (global::go.encoding.xml_package.CharData)slice<byte>((@string)"\n"u8),
    new StartElement(new Name("", "tag"), []),
    (global::go.encoding.xml_package.CharData)slice<byte>((@string)"&なまえ3;"u8),
    new EndElement(new Name("", "tag")),
    (global::go.encoding.xml_package.CharData)slice<byte>((@string)"\n"u8),
    new StartElement(new Name("", "tag"), []),
    (global::go.encoding.xml_package.CharData)slice<byte>((@string)"&lt-gt;"u8),
    new EndElement(new Name("", "tag")),
    (global::go.encoding.xml_package.CharData)slice<byte>((@string)"\n"u8),
    new StartElement(new Name("", "tag"), []),
    (global::go.encoding.xml_package.CharData)slice<byte>((@string)"&;"u8),
    new EndElement(new Name("", "tag")),
    (global::go.encoding.xml_package.CharData)slice<byte>((@string)"\n"u8),
    new StartElement(new Name("", "tag"), []),
    (global::go.encoding.xml_package.CharData)slice<byte>((@string)"&0a;"u8),
    new EndElement(new Name("", "tag")),
    (global::go.encoding.xml_package.CharData)slice<byte>((@string)"\n"u8)
];

public static void TestNonStrictRawToken(ж<testing.T> t) {
    var d = NewDecoder(new xml_test_package.strings_ReaderжReader(strings.NewReader(nonStrictInput)));
    d.Strict = false;
    testRawToken(t, d, nonStrictInput, nonStrictTokens);
}

internal partial struct downCaser {
    internal ж<testing.T> t;
    internal io.ByteReader r;
}

internal static (byte c, error err) ReadByte(this ref downCaser d) {
    byte c = default;
    error err = default;

    (c, err) = d.r.ReadByte();
    if (c >= 'A' && c <= 'Z') {
        c += (byte)((rune)'a' - (rune)'A');
    }
    return (c, err);
}

internal static partial (nint, error) Read(this ref downCaser d, slice<byte> p) {
    d.t.Fatalf("unexpected Read call on downCaser reader");
    throw panic("unreachable");
}

public static void TestRawTokenAltEncoding(ж<testing.T> t) {
    var d = NewDecoder(new xml_test_package.strings_ReaderжReader(strings.NewReader(testInputAltEncoding)));
    d.CharsetReader = (io.Reader, error) (@string charset, io.Reader input) => {
        if (charset != "x-testing-uppercase"u8) {
            t.Fatalf("unexpected charset %q", charset);
        }
        return (new xml_internal_test_package.downCaserжReader(Ꮡ(new downCaser(t, input._<io.ByteReader>()))), default);
    };
    testRawToken(t, d, testInputAltEncoding, rawTokensAltEncoding);
}

// Hoisted Go string constant (single allocation; Go keeps it in RODATA)
internal static ref readonly @string encodingᶜ => ref encodingᶜᵇ;
private static readonly @string encodingᶜᵇ = "x-testing-uppercase"u8;

public static void TestRawTokenAltEncodingNoConverter(ж<testing.T> t) {
    var d = NewDecoder(new xml_test_package.strings_ReaderжReader(strings.NewReader(testInputAltEncoding)));
    var (token, err) = d.RawToken();
    if (token == nil) {
        t.Fatalf("expected a token on first RawToken call");
    }
    if (err != nil) {
        t.Fatal(err);
    }
    (token, err) = d.RawToken();
    if (token != nil) {
        t.Errorf("expected a nil token; got %#v", token);
    }
    if (err == nil) {
        t.Fatalf("expected an error on second RawToken call");
    }
    @string encoding = encodingᶜ;
    if (!strings.Contains(err.Error(), encoding)) {
        t.Errorf("expected error to contain %q; got error: %v",
            encoding, err);
    }
}

internal static void testRawToken(ж<testing.T> t, ж<global::go.encoding.xml_package.Decoder> Ꮡd, @string raw, slice<ΔToken> rawTokens) {
    ref var d = ref Ꮡd.DerefOrNull();

    var lastEnd = (int64)0;
    foreach (var (i, want) in range(rawTokens)) {
        var start = d.InputOffset();
        var (have, err) = d.RawToken();
        var end = d.InputOffset();
        if (err != nil) {
            t.Fatalf("token %d: unexpected error: %s", i, err);
        }
        if (!reflect.DeepEqual(have, want)) {
            @string shave = default;
            @string swant = default;
            if (have._<CharData>(out _)) {
                shave = fmt.Sprintf("CharData(%q)"u8, have);
            } else {
                shave = fmt.Sprintf("%#v"u8, have);
            }
            if (want._<CharData>(out _)) {
                swant = fmt.Sprintf("CharData(%q)"u8, want);
            } else {
                swant = fmt.Sprintf("%#v"u8, want);
            }
            t.Errorf("token %d = %s, want %s", i, shave, swant);
        }

        // Check that InputOffset returned actual token.
        switch (ᐧ) {
        case {} when start < lastEnd: {
            t.Errorf("token %d: position [%d,%d) for %T is before previous token", i, start, end, have);
            break;
        }
        case {} when start >= end: {
            // Special case: EndElement can be synthesized.
            if (start == end && end == lastEnd) {
                break;
            }
            t.Errorf("token %d: position [%d,%d) for %T is empty", i, start, end, have);
            break;
        }
        case {} when end > (int64)len(raw): {
            t.Errorf("token %d: position [%d,%d) for %T extends beyond input", i, start, end, have);
            break;
        }
        default: {
            @string text = raw.slice((nint)(start), (nint)(end));
            if (strings.ContainsAny(text, "<>") && (!strings.HasPrefix(text, "<") || !strings.HasSuffix(text, ">"))) {
                t.Errorf("token %d: misaligned raw token %#q for %T", i, text, have);
            }
            break;
        }}
        lastEnd = end;
    }
}

// Ensure that directives (specifically !DOCTYPE) include the complete
// text of any nested directives, noting that < and > do not change
// nesting depth if they are in single or double quotes.
internal static @string nestedDirectivesInput = """

<!DOCTYPE [<!ENTITY rdf "http://www.w3.org/1999/02/22-rdf-syntax-ns#">]>
<!DOCTYPE [<!ENTITY xlt ">">]>
<!DOCTYPE [<!ENTITY xlt "<">]>
<!DOCTYPE [<!ENTITY xlt '>'>]>
<!DOCTYPE [<!ENTITY xlt '<'>]>
<!DOCTYPE [<!ENTITY xlt '">'>]>
<!DOCTYPE [<!ENTITY xlt "'<">]>

"""u8.Raw();

internal static slice<ΔToken> nestedDirectivesTokens = [
    (global::go.encoding.xml_package.CharData)slice<byte>((@string)"\n"u8),
    (global::go.encoding.xml_package.Directive)slice<byte>((@string)@"DOCTYPE [<!ENTITY rdf ""http://www.w3.org/1999/02/22-rdf-syntax-ns#"">]"u8),
    (global::go.encoding.xml_package.CharData)slice<byte>((@string)"\n"u8),
    (global::go.encoding.xml_package.Directive)slice<byte>((@string)@"DOCTYPE [<!ENTITY xlt "">"">]"u8),
    (global::go.encoding.xml_package.CharData)slice<byte>((@string)"\n"u8),
    (global::go.encoding.xml_package.Directive)slice<byte>((@string)@"DOCTYPE [<!ENTITY xlt ""<"">]"u8),
    (global::go.encoding.xml_package.CharData)slice<byte>((@string)"\n"u8),
    (global::go.encoding.xml_package.Directive)slice<byte>((@string)@"DOCTYPE [<!ENTITY xlt '>'>]"u8),
    (global::go.encoding.xml_package.CharData)slice<byte>((@string)"\n"u8),
    (global::go.encoding.xml_package.Directive)slice<byte>((@string)@"DOCTYPE [<!ENTITY xlt '<'>]"u8),
    (global::go.encoding.xml_package.CharData)slice<byte>((@string)"\n"u8),
    (global::go.encoding.xml_package.Directive)slice<byte>((@string)@"DOCTYPE [<!ENTITY xlt '"">'>]"u8),
    (global::go.encoding.xml_package.CharData)slice<byte>((@string)"\n"u8),
    (global::go.encoding.xml_package.Directive)slice<byte>((@string)@"DOCTYPE [<!ENTITY xlt ""'<"">]"u8),
    (global::go.encoding.xml_package.CharData)slice<byte>((@string)"\n"u8)
];

public static void TestNestedDirectives(ж<testing.T> t) {
    var d = NewDecoder(new xml_test_package.strings_ReaderжReader(strings.NewReader(nestedDirectivesInput)));

    foreach (var (i, want) in range(nestedDirectivesTokens)) {
        var (have, err) = d.Token();
        if (err != nil) {
            t.Fatalf("token %d: unexpected error: %s", i, err);
        }
        if (!reflect.DeepEqual(have, want)) {
            t.Errorf("token %d = %#v want %#v", i, have, want);
        }
    }
}

public static void TestToken(ж<testing.T> t) {
    var d = NewDecoder(new xml_test_package.strings_ReaderжReader(strings.NewReader(testInput)));
    d.Entity = testEntity;

    foreach (var (i, want) in range(cookedTokens)) {
        var (have, err) = d.Token();
        if (err != nil) {
            t.Fatalf("token %d: unexpected error: %s", i, err);
        }
        if (!reflect.DeepEqual(have, want)) {
            t.Errorf("token %d = %#v want %#v", i, have, want);
        }
    }
}

public static void TestSyntax(ж<testing.T> t) {
    foreach (var i in range(len(xmlInput))) {
        var d = NewDecoder(new xml_test_package.strings_ReaderжReader(strings.NewReader(xmlInput[i])));
        error err = default;
        for ((_, err) = d.Token(); err == nil; (_, err) = d.Token()) {
        }
        var (_, ok) = err._<ж<global::go.encoding.xml_package.SyntaxError>>(ᐧ); if (!ok) {
            t.Fatalf(@"xmlInput ""%s"": expected SyntaxError not received", xmlInput[i]);
        }
    }
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static ref readonly @string rootPiEltAttValCdataEltˢ => ref rootPiEltAttValCdataEltˢᵇ;
private static readonly @string rootPiEltAttValCdataEltˢᵇ = """
<root>
<?pi
 ?>  <elt
att
=
"val">
<![CDATA[
]]><!--

--></elt>
</root>
"""u8.Raw();

public static void TestInputLinePos(ж<testing.T> t) {
    @string testInput = rootPiEltAttValCdataEltˢ;
    var linePos = slice<slice<nint>>([
        [1, 7],
        [2, 1],
        [3, 4],
        [3, 6],
        [6, 7],
        [7, 1],
        [8, 4],
        [10, 4],
        [10, 10],
        [11, 1],
        [11, 8]
    ]);
    var dec = NewDecoder(new xml_test_package.strings_ReaderжReader(strings.NewReader(testInput)));
    foreach (var (_, want) in range(linePos)) {
        var (_, err) = dec.Token(); if (err != nil) {
            t.Errorf("Unexpected error: %v", err);
            continue;
        }

        var (gotLine, gotCol) = dec.InputPos();
        if (gotLine != want[0] || gotCol != want[1]) {
            t.Errorf("dec.InputPos() = %d,%d, want %d,%d", gotLine, gotCol, want[0], want[1]);
        }
    }
}

internal partial struct allScalars {
    public bool True1;
    public bool True2;
    public bool False1;
    public bool False2;
    public nint Int;
    public int8 Int8;
    public int16 Int16;
    public int32 Int32;
    public int64 Int64;
    public nint Uint;
    public uint8 Uint8;
    public uint16 Uint16;
    public uint32 Uint32;
    public uint64 Uint64;
    public uintptr Uintptr;
    public float32 Float32;
    public float64 Float64;
    public @string String;
    public ж<@string> PtrString;
}

internal static allScalars all;
internal static void initᴛall() { all = new allScalars(
    True1: true,
    True2: true,
    False1: false,
    False2: false,
    Int: 1,
    Int8: (int8)(-2),
    Int16: 3,
    Int32: -4,
    Int64: 5,
    Uint: 6,
    Uint8: 7,
    Uint16: 8,
    Uint32: 9,
    Uint64: 10,
    Uintptr: 11,
    Float32: 13.0F,
    Float64: 14.0D,
    String: "15",
    PtrString: Ꮡsixteen
); }

internal static readonly ж<@string> Ꮡsixteen = new StaticBox<@string>("16");
internal static ref @string sixteen => ref ((StaticBox<@string>)Ꮡsixteen).ValueSlot;

internal static ref readonly @string testScalarsInput => ref testScalarsInputᵇ;
private static readonly @string testScalarsInputᵇ = """
<allscalars>
	<True1>true</True1>
	<True2>1</True2>
	<False1>false</False1>
	<False2>0</False2>
	<Int>1</Int>
	<Int8>-2</Int8>
	<Int16>3</Int16>
	<Int32>-4</Int32>
	<Int64>5</Int64>
	<Uint>6</Uint>
	<Uint8>7</Uint8>
	<Uint16>8</Uint16>
	<Uint32>9</Uint32>
	<Uint64>10</Uint64>
	<Uintptr>11</Uintptr>
	<Float>12.0</Float>
	<Float32>13.0</Float32>
	<Float64>14.0</Float64>
	<String>15</String>
	<PtrString>16</PtrString>
</allscalars>
"""u8.Raw();

public static void TestAllScalars(ж<testing.T> t) {
    ref var a = ref heap(new allScalars(), out var Ꮡa);
    var err = Unmarshal(slice<byte>(testScalarsInput), Ꮡa);

    if (err != nil) {
        t.Fatal(err);
    }
    if (!reflect.DeepEqual(a, all)) {
        t.Errorf("have %+v want %+v", a, all);
    }
}

internal partial struct item {
    public @string FieldA;
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static ref readonly @string itemBˢ => ref itemBˢᵇ;
private static readonly @string itemBˢᵇ = @"<item b=']]>'/>"u8;
internal static readonly object wrongStartElementˢ = (@string)"Wrong start element"u8;
internal static readonly object wrongEndElementˢ = (@string)"Wrong end element"u8;

public static void TestIssue68387(ж<testing.T> t) {
    @string data = itemBˢ;
    var dec = NewDecoder(new xml_test_package.strings_ReaderжReader(strings.NewReader(data)));
    ΔToken tok1 = default;
    ΔToken tok2 = default;
    ΔToken tok3 = default;
    error err = default;
    (tok1, err) = dec.RawToken(); if (err != nil) {
        t.Fatalf("RawToken() failed: %v", err);
    }
    (tok2, err) = dec.RawToken(); if (err != nil) {
        t.Fatalf("RawToken() failed: %v", err);
    }
    (tok3, err) = dec.RawToken(); if (!AreEqual(err, io.EOF) || tok3 != nil) {
        t.Fatalf("Missed EOF");
    }
    var s = new StartElement(new Name("", "item"), [new Attr(new Name("", "b"), "]]>")]);
    if (!reflect.DeepEqual(tok1._<StartElement>(), s)) {
        t.Error(wrongStartElementˢ);
    }
    var e = new EndElement(new Name("", "item"));
    if (tok2._<EndElement>() != e) {
        t.Error(wrongEndElementˢ);
    }
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static ref readonly @string itemFieldAAbcdFieldAItemˢ => ref itemFieldAAbcdFieldAItemˢᵇ;
private static readonly @string itemFieldAAbcdFieldAItemˢᵇ = @"<item><FieldA>abcd</FieldA></item>"u8;
internal static readonly object expectingAbcdˢ = (@string)"Expecting abcd"u8;

public static void TestIssue569(ж<testing.T> t) {
    @string data = itemFieldAAbcdFieldAItemˢ;
    ref var i = ref heap(new item(), out var Ꮡi);
    var err = Unmarshal(slice<byte>(data), Ꮡi);

    if (err != nil || i.FieldA != "abcd"u8) {
        t.Fatal(expectingAbcdˢ);
    }
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static ref readonly @string tagAttrAzAZ09ˢ => ref tagAttrAzAZ09ˢᵇ;
private static readonly @string tagAttrAzAZ09ˢᵇ = "<tag attr=azAZ09:-_\t>"u8;

public static void TestUnquotedAttrs(ж<testing.T> t) {
    @string data = tagAttrAzAZ09ˢ;
    var d = NewDecoder(new xml_test_package.strings_ReaderжReader(strings.NewReader(data)));
    d.Strict = false;
    var (token, err) = d.Token();
    var (_, ok) = err._<ж<global::go.encoding.xml_package.SyntaxError>>(ᐧ); if (ok) {
        t.Errorf("Unexpected error: %v", err);
    }
    if (token._<StartElement>().Name.Local != "tag"u8) {
        t.Errorf("Unexpected tag name: %v", token._<StartElement>().Name.Local);
    }
    var attr = token._<StartElement>().Attr[0];
    if (attr.Value != "azAZ09:-_"u8) {
        t.Errorf("Unexpected attribute value: %v", attr.Value);
    }
    if (attr.Name.Local != "attr"u8) {
        t.Errorf("Unexpected attribute name: %v", attr.Name.Local);
    }
}

public static void TestValuelessAttrs(ж<testing.T> t) {
    var tests = GoReflect.WithElemDims(new array<@string>[]{
        new @string[]{"<p nowrap>", "p", "nowrap"}.array(),
        new @string[]{"<p nowrap >", "p", "nowrap"}.array(),
        new @string[]{"<input checked/>", "input", "checked"}.array(),
        new @string[]{"<input checked />", "input", "checked"}.array()
    }.slice(), 3);
    foreach (var (_, vᴛ1) in range(tests)) {
        var test = vᴛ1.Clone();

        var d = NewDecoder(new xml_test_package.strings_ReaderжReader(strings.NewReader(test[0])));
        d.Strict = false;
        var (token, err) = d.Token();
        var (_, ok) = err._<ж<global::go.encoding.xml_package.SyntaxError>>(ᐧ); if (ok) {
            t.Errorf("Unexpected error: %v", err);
        }
        if (token._<StartElement>().Name.Local != test[1]) {
            t.Errorf("Unexpected tag name: %v", token._<StartElement>().Name.Local);
        }
        var attr = token._<StartElement>().Attr[0];
        if (attr.Value != test[2]) {
            t.Errorf("Unexpected attribute value: %v", attr.Value);
        }
        if (attr.Name.Local != test[2]) {
            t.Errorf("Unexpected attribute name: %v", attr.Name.Local);
        }
    }
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static readonly object copyTokenCharDataˢ = (@string)"CopyToken(CharData) != CharData"u8;
internal static readonly object copyTokenCharDataUsesˢ = (@string)"CopyToken(CharData) uses same buffer."u8;

public static void TestCopyTokenCharData(ж<testing.T> t) {
    var data = slice<byte>("same data"u8);
    ΔToken tok1 = ((global::go.encoding.xml_package.CharData)data);
    var tok2 = CopyToken(tok1);
    if (!reflect.DeepEqual(tok1, tok2)) {
        t.Error(copyTokenCharDataˢ);
    }
    data[1] = (byte)'o';
    if (reflect.DeepEqual(tok1, tok2)) {
        t.Error(copyTokenCharDataUsesˢ);
    }
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static readonly object copyTokenOverwroteAttr0ˢ = (@string)"CopyToken overwrote Attr[0]"u8;
internal static readonly object copyTokenStartElementˢ = (@string)"CopyToken(StartElement) != StartElement"u8;

public static void TestCopyTokenStartElement(ж<testing.T> t) {
    var elt = new StartElement(new Name("", "hello"), [new(new Name("", "lang"), "en")]);
    ΔToken tok1 = elt;
    var tok2 = CopyToken(tok1);
    if (tok1._<StartElement>().Attr[0].Value != "en"u8) {
        t.Error(copyTokenOverwroteAttr0ˢ);
    }
    if (!reflect.DeepEqual(tok1, tok2)) {
        t.Error(copyTokenStartElementˢ);
    }
    tok1._<StartElement>().Attr[0] = new Attr(new Name("", "lang"), "de");
    if (reflect.DeepEqual(tok1, tok2)) {
        t.Error(copyTokenCharDataUsesˢ);
    }
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static readonly object copyTokenCommentCommentˢ = (@string)"CopyToken(Comment) != Comment"u8;
internal static readonly object copyTokenCommentUsesSameˢ = (@string)"CopyToken(Comment) uses same buffer."u8;

public static void TestCopyTokenComment(ж<testing.T> t) {
    var data = slice<byte>("<!-- some comment -->"u8);
    ΔToken tok1 = ((global::go.encoding.xml_package.Comment)data);
    var tok2 = CopyToken(tok1);
    if (!reflect.DeepEqual(tok1, tok2)) {
        t.Error(copyTokenCommentCommentˢ);
    }
    data[1] = (byte)'o';
    if (reflect.DeepEqual(tok1, tok2)) {
        t.Error(copyTokenCommentUsesSameˢ);
    }
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static ref readonly @string pFooPPBarˢ => ref pFooPPBarˢᵇ;
private static readonly @string pFooPPBarˢᵇ = "<P>Foo<P>\n\n<P>Bar</>\n"u8;
internal static readonly object expectedSyntaxErrorˢ = (@string)"Expected SyntaxError."u8;
internal static readonly object syntaxErrorDidnTHaveˢ = (@string)"SyntaxError didn't have correct line number."u8;

public static void TestSyntaxErrorLineNum(ж<testing.T> t) {
    @string testInput = pFooPPBarˢ;
    var d = NewDecoder(new xml_test_package.strings_ReaderжReader(strings.NewReader(testInput)));
    error err = default;
    for ((_, err) = d.Token(); err == nil; (_, err) = d.Token()) {
    }
    var (synerr, ok) = err._<ж<global::go.encoding.xml_package.SyntaxError>>(ᐧ);
    if (!ok) {
        t.Error(expectedSyntaxErrorˢ);
    }
    if (synerr.Line != 3) {
        t.Error(syntaxErrorDidnTHaveˢ);
    }
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static ref readonly @string fooFooˢ => ref fooFooˢᵇ;
private static readonly @string fooFooˢᵇ = @"<FOO></FOO>  "u8;

public static void TestTrailingRawToken(ж<testing.T> t) {
    @string input = fooFooˢ;
    var d = NewDecoder(new xml_test_package.strings_ReaderжReader(strings.NewReader(input)));
    error err = default;
    for ((_, err) = d.RawToken(); err == nil; (_, err) = d.RawToken()) {
    }
    if (!AreEqual(err, io.EOF)) {
        t.Fatalf("d.RawToken() = _, %v, want _, io.EOF", err);
    }
}

public static void TestTrailingToken(ж<testing.T> t) {
    @string input = fooFooˢ;
    var d = NewDecoder(new xml_test_package.strings_ReaderжReader(strings.NewReader(input)));
    error err = default;
    for ((_, err) = d.Token(); err == nil; (_, err) = d.Token()) {
    }
    if (!AreEqual(err, io.EOF)) {
        t.Fatalf("d.Token() = _, %v, want _, io.EOF", err);
    }
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static ref readonly @string testCdataValFooTestˢ => ref testCdataValFooTestˢᵇ;
private static readonly @string testCdataValFooTestˢᵇ = @"<test><![CDATA[ &val=foo ]]></test>"u8;

public static void TestEntityInsideCDATA(ж<testing.T> t) {
    @string input = testCdataValFooTestˢ;
    var d = NewDecoder(new xml_test_package.strings_ReaderжReader(strings.NewReader(input)));
    error err = default;
    for ((_, err) = d.Token(); err == nil; (_, err) = d.Token()) {
    }
    if (!AreEqual(err, io.EOF)) {
        t.Fatalf("d.Token() = _, %v, want _, io.EOF", err);
    }
}

partial struct characterTestsᴛ1 /*dyn*/ {
    internal @string @in;
    internal @string err;
}
internal static slice<characterTestsᴛ1> characterTests = [
    new("\x12<doc/>", "illegal character code U+0012"),
    new("<?xml version=\"1.0\"?>\x0b<doc/>", "illegal character code U+000B"),
    new(((@string)(new byte[]{0xef, 0xbf, 0xbe, 0x3c, 0x64, 0x6f, 0x63, 0x2f, 0x3e})), "illegal character code U+FFFE"),
    new("<?xml version=\"1.0\"?><doc>\r\n<hiya/>\x07<toots/></doc>", "illegal character code U+0007"),
    new("<?xml version=\"1.0\"?><doc \x12='value'>what's up</doc>", "expected attribute name in element"),
    new("<doc>&abc\x01;</doc>", "invalid character entity &abc (no semicolon)"),
    new("<doc>&\x01;</doc>", "invalid character entity & (no semicolon)"),
    new(((@string)(new byte[]{0x3c, 0x64, 0x6f, 0x63, 0x3e, 0x26, 0xef, 0xbf, 0xbe, 0x3b, 0x3c, 0x2f, 0x64, 0x6f, 0x63, 0x3e})), "invalid character entity &\uFFFE;"),
    new("<doc>&hello;</doc>", "invalid character entity &hello;")
];

public static void TestDisallowedCharacters(ж<testing.T> t) {
    foreach (var (i, tt) in range(characterTests)) {
        var d = NewDecoder(new xml_test_package.strings_ReaderжReader(strings.NewReader(tt.@in)));
        error err = default;

        while (err == nil) {
            (_, err) = d.Token();
        }
        var (synerr, ok) = err._<ж<global::go.encoding.xml_package.SyntaxError>>(ᐧ);
        if (!ok) {
            t.Fatalf("input %d d.Token() = _, %v, want _, *SyntaxError", i, err);
        }
        if (synerr.Msg != tt.err) {
            t.Fatalf("input %d synerr.Msg wrong: want %q, got %q", i, tt.err, synerr.Msg);
        }
    }
}

public static void TestIsInCharacterRange(ж<testing.T> t) {
    var invalid = slice<rune>([
        utf8.MaxRune + 1,
        0xD800, // surrogate min
        0xDFFF, // surrogate max
        -1
    ]);
    foreach (var (_, r) in range(invalid)) {
        if (isInCharacterRange(r)) {
            t.Errorf("rune %U considered valid", r);
        }
    }
}

partial struct procInstTestsᴛ1 /*dyn*/ {
    internal @string input;
    internal array<@string> expect = new(2);
}
internal static slice<procInstTestsᴛ1> procInstTests = [
    new(@"version=""1.0"" encoding=""utf-8""", new @string[]{"1.0", "utf-8"}.array()),
    new(@"version=""1.0"" encoding='utf-8'", new @string[]{"1.0", "utf-8"}.array()),
    new(@"version=""1.0"" encoding='utf-8' ", new @string[]{"1.0", "utf-8"}.array()),
    new(@"version=""1.0"" encoding=utf-8", new @string[]{"1.0", ""}.array()),
    new(@"encoding=""FOO"" ", new @string[]{"", "FOO"}.array()),
    new(@"version=2.0 version=""1.0"" encoding=utf-7 encoding='utf-8'", new @string[]{"1.0", "utf-8"}.array()),
    new(@"version= encoding=", new @string[]{"", ""}.array()),
    new(@"encoding=""version=1.0""", new @string[]{"", "version=1.0"}.array()),
    new(@"", new @string[]{"", ""}.array()),
    // TODO: what's the right approach to handle these nested cases?
    new(@"encoding=""version='1.0'""", new @string[]{"1.0", "version='1.0'"}.array()),
    new(@"version=""encoding='utf-8'""", new @string[]{"encoding='utf-8'", "utf-8"}.array())
];

public static void TestProcInstEncoding(ж<testing.T> t) {
    foreach (var (_, vᴛ1) in range(procInstTests)) {
        var test = vᴛ1.ΔClone();

        {
            @string got = procInst(versionˢ, test.input); if (got != test.expect[0]) {
                t.Errorf("procInst(version, %q) = %q; want %q", test.input, got, test.expect[0]);
            }
        }
        {
            @string got = procInst(encodingˢ, test.input); if (got != test.expect[1]) {
                t.Errorf("procInst(encoding, %q) = %q; want %q", test.input, got, test.expect[1]);
            }
        }
    }
}

// Ensure that directives with comments include the complete
// text of any nested directives.
internal static @string directivesWithCommentsInput = """

<!DOCTYPE [<!-- a comment --><!ENTITY rdf "http://www.w3.org/1999/02/22-rdf-syntax-ns#">]>
<!DOCTYPE [<!ENTITY go "Golang"><!-- a comment-->]>
<!DOCTYPE <!-> <!> <!----> <!-->--> <!--->--> [<!ENTITY go "Golang"><!-- a comment-->]>

"""u8.Raw();

internal static slice<ΔToken> directivesWithCommentsTokens = [
    (global::go.encoding.xml_package.CharData)slice<byte>((@string)"\n"u8),
    (global::go.encoding.xml_package.Directive)slice<byte>((@string)@"DOCTYPE [ <!ENTITY rdf ""http://www.w3.org/1999/02/22-rdf-syntax-ns#"">]"u8),
    (global::go.encoding.xml_package.CharData)slice<byte>((@string)"\n"u8),
    (global::go.encoding.xml_package.Directive)slice<byte>((@string)@"DOCTYPE [<!ENTITY go ""Golang""> ]"u8),
    (global::go.encoding.xml_package.CharData)slice<byte>((@string)"\n"u8),
    (global::go.encoding.xml_package.Directive)slice<byte>((@string)@"DOCTYPE <!-> <!>       [<!ENTITY go ""Golang""> ]"u8),
    (global::go.encoding.xml_package.CharData)slice<byte>((@string)"\n"u8)
];

public static void TestDirectivesWithComments(ж<testing.T> t) {
    var d = NewDecoder(new xml_test_package.strings_ReaderжReader(strings.NewReader(directivesWithCommentsInput)));

    foreach (var (i, want) in range(directivesWithCommentsTokens)) {
        var (have, err) = d.Token();
        if (err != nil) {
            t.Fatalf("token %d: unexpected error: %s", i, err);
        }
        if (!reflect.DeepEqual(have, want)) {
            t.Errorf("token %d = %#v want %#v", i, have, want);
        }
    }
}

// Writer whose Write method always returns an error.
internal partial struct errWriter {
}

internal static (nint n, error err) Write(this errWriter _, slice<byte> p) { return (0, fmt.Errorf("unwritable"u8)); }

public static void TestEscapeTextIOErrors(ж<testing.T> t) {
    @string expectErr = unwritableˢ;
    var err = EscapeText(new errWriter(), [(byte)'A']);

    if (err == nil || err.Error() != expectErr) {
        t.Errorf("have %v, want %v", err, expectErr);
    }
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static ref readonly @string aTerminatedStringˢ => ref aTerminatedStringˢᵇ;
private static readonly @string aTerminatedStringˢᵇ = "A \uFFFD terminated string."u8;

public static void TestEscapeTextInvalidChar(ж<testing.T> t) {
    var input = slice<byte>("A \x00 terminated string."u8);
    @string expected = aTerminatedStringˢ;

    var buff = @new<strings.Builder>();
    var err = EscapeText(new xml_test_package.strings_BuilderжWriter(buff), input); if (err != nil) {
        t.Fatalf("have %v, want nil", err);
    }
    @string text = buff.String();

    if (text != expected) {
        t.Errorf("have %v, want %v", text, expected);
    }
}

internal partial struct TestIssue5880_T /*[]byte*/;

public static void TestIssue5880(ж<testing.T> t) {
    var (data, err) = Marshal(new TestIssue5880_T(new byte[]{192, 168, 0, 1}.slice()));
    if (err != nil) {
        t.Errorf("Marshal error: %v", err);
    }
    if (!utf8.Valid(data)) {
        t.Errorf("Marshal generated invalid UTF-8: %x", data);
    }
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static ref readonly @string exampleTitleExampleTitleˢ => ref exampleTitleExampleTitleˢᵇ;
private static readonly @string exampleTitleExampleTitleˢᵇ = """
<example>
			<title>Example</title>
			<link>http://example.com/default</link> <!-- not assigned -->
			<link>http://example.com/home</link> <!-- not assigned -->
			<ns:link xmlns:ns="http://www.w3.org/2005/Atom">http://example.com/ns</ns:link>
		</example>
"""u8.Raw();

internal partial struct TestIssue8535_ExampleConflict /*dyn*/ {
    public global::go.encoding.xml_package.Name XMLName; /*`xml:"example"`*/
    public @string Link; /*`xml:"link"`*/
    public @string AtomLink; /*`xml:"http://www.w3.org/2005/Atom link"`*/ // Same name in a different name space
}

public static void TestIssue8535(ж<testing.T> t) {
    @string testCase = exampleTitleExampleTitleˢ;

    ref var dest = ref heap(new TestIssue8535_ExampleConflict(), out var Ꮡdest);
    var d = NewDecoder(new xml_test_package.strings_ReaderжReader(strings.NewReader(testCase)));
    var err = d.Decode(Ꮡdest); if (err != nil) {
        t.Fatal(err);
    }
}

internal partial struct TestEncodeXMLNS_testCases /*dyn*/ {
    internal Func<(slice<byte>, error)> f;
    internal @string want;
    internal bool ok;
}

public static void TestEncodeXMLNS(ж<testing.T> t) {
    var testCases = slice<TestEncodeXMLNS_testCases>([
        new(encodeXMLNS1, @"<Test xmlns=""http://example.com/ns""><Body>hello world</Body></Test>", true),
        new(encodeXMLNS2, @"<Test><body xmlns=""http://example.com/ns"">hello world</body></Test>", true),
        new(encodeXMLNS3, @"<Test xmlns=""http://example.com/ns""><Body>hello world</Body></Test>", true),
        new(encodeXMLNS4, @"<Test xmlns=""http://example.com/ns""><Body>hello world</Body></Test>", false)
    ]);

    foreach (var (i, tc) in range(testCases)) {
        var (b, err) = tc.f(); if (err == nil) {
            @string got = (@string)b;
            @string want = tc.want; if (got != want) {
                t.Errorf("%d: got %s, want %s \n", i, got, want);
            }
        } else {
            t.Errorf("%d: marshal failed with %s", i, err);
        }
    }
}

internal partial struct encodeXMLNS1_T /*dyn*/ {
    public global::go.encoding.xml_package.Name XMLName; /*`xml:"Test"`*/
    public @string Ns; /*`xml:"xmlns,attr"`*/
    public @string Body;
}

internal static (slice<byte>, error) encodeXMLNS1() {
    var s = Ꮡ(new encodeXMLNS1_T(Ns: "http://example.com/ns", Body: "hello world"));
    return Marshal(s.OrTypedNil());
}

internal partial struct encodeXMLNS2_Test /*dyn*/ {
    public @string Body; /*`xml:"http://example.com/ns body"`*/
}

internal static (slice<byte>, error) encodeXMLNS2() {
    var s = Ꮡ(new encodeXMLNS2_Test(Body: "hello world"));
    return Marshal(s.OrTypedNil());
}

internal partial struct encodeXMLNS3_Test /*dyn*/ {
    public global::go.encoding.xml_package.Name XMLName; /*`xml:"http://example.com/ns Test"`*/
    public @string Body;
}

internal static (slice<byte>, error) encodeXMLNS3() {
    //s := &Test{XMLName: Name{"http://example.com/ns",""}, Body: "hello world"} is unusable as the "-" is missing
    // as documentation states
    var s = Ꮡ(new encodeXMLNS3_Test(Body: "hello world"));
    return Marshal(s.OrTypedNil());
}

internal partial struct encodeXMLNS4_Test /*dyn*/ {
    public @string Ns; /*`xml:"xmlns,attr"`*/
    public @string Body;
}

internal static (slice<byte>, error) encodeXMLNS4() {
    var s = Ꮡ(new encodeXMLNS4_Test(Ns: "http://example.com/ns", Body: "hello world"));
    return Marshal(s.OrTypedNil());
}

public static void TestIssue11405(ж<testing.T> t) {
    var testCases = slice<@string>([
        "<root>",
        "<root><foo>",
        "<root><foo></foo>"
    ]);
    foreach (var (_, tc) in range(testCases)) {
        var d = NewDecoder(new xml_test_package.strings_ReaderжReader(strings.NewReader(tc)));
        error err = default;
        for (;;) {
            (_, err) = d.Token();
            if (err != nil) {
                break;
            }
        }
        var (_, ok) = err._<ж<global::go.encoding.xml_package.SyntaxError>>(ᐧ); if (!ok) {
            t.Errorf("%s: Token: Got error %v, want SyntaxError", tc, err);
        }
    }
}

internal partial struct TestIssue12417_testCases /*dyn*/ {
    internal @string s;
    internal bool ok;
}

public static void TestIssue12417(ж<testing.T> t) {
    var testCases = slice<TestIssue12417_testCases>([
        new(@"<?xml encoding=""UtF-8"" version=""1.0""?><root/>", true),
        new(@"<?xml encoding=""UTF-8"" version=""1.0""?><root/>", true),
        new(@"<?xml encoding=""utf-8"" version=""1.0""?><root/>", true),
        new(@"<?xml encoding=""uuu-9"" version=""1.0""?><root/>", false)
    ]);
    foreach (var (_, tc) in range(testCases)) {
        var d = NewDecoder(new xml_test_package.strings_ReaderжReader(strings.NewReader(tc.s)));
        error err = default;
        for (;;) {
            (_, err) = d.Token();
            if (err != nil) {
                if (AreEqual(err, io.EOF)) {
                    err = default;
                }
                break;
            }
        }
        if (err != nil && tc.ok) {
            t.Errorf("%q: Encoding charset: expected no error, got %s", tc.s, err);
            continue;
        }
        if (err == nil && !tc.ok) {
            t.Errorf("%q: Encoding charset: expected error, got nil", tc.s);
        }
    }
}

internal partial struct TestIssue7113_C /*dyn*/ {
    public global::go.encoding.xml_package.Name XMLName; /*`xml:""`*/ // Sets empty namespace
}

internal partial struct TestIssue7113_D /*dyn*/ {
    public global::go.encoding.xml_package.Name XMLName; /*`xml:"d"`*/
}

internal partial struct TestIssue7113_A /*dyn*/ {
    public global::go.encoding.xml_package.Name XMLName; /*`xml:""`*/
    public TestIssue7113_C C; /*`xml:""`*/
    public TestIssue7113_D D;
}

public static void TestIssue7113(ж<testing.T> t) {
    ref var a = ref heap(new TestIssue7113_A(), out var Ꮡa);
    @string structSpace = "b";
    @string xmlTest = @"<A xmlns="""u8 + structSpace + @"""><C xmlns=""""></C><d></d></A>"u8;
    t.Log(xmlTest);
    var err = Unmarshal(slice<byte>(xmlTest), Ꮡa);
    if (err != nil) {
        t.Fatal(err);
    }

    if (a.XMLName.Space != structSpace) {
        t.Errorf("overidding with empty namespace: unmarshaling, got %s, want %s\n", a.XMLName.Space, structSpace);
    }
    if (len(a.C.XMLName.Space) != 0) {
        t.Fatalf("overidding with empty namespace: unmarshaling, got %s, want empty\n", a.C.XMLName.Space);
    }

    slice<byte> b = default;
    (b, err) = Marshal(Ꮡa);
    if (err != nil) {
        t.Fatal(err);
    }
    if (len(a.C.XMLName.Space) != 0) {
        t.Errorf("overidding with empty namespace: marshaling, got %s in C tag which should be empty\n", a.C.XMLName.Space);
    }
    if (((sstring)b) != xmlTest) {
        t.Fatalf("overidding with empty namespace: marshaling, got %s, want %s\n", b, xmlTest);
    }
    ref var c = ref heap(new TestIssue7113_A(), out var Ꮡc);
    err = Unmarshal(b, Ꮡc);
    if (err != nil) {
        t.Fatalf("second Unmarshal failed: %s", err);
    }
    if (c.XMLName.Space != "b"u8) {
        t.Errorf("overidding with empty namespace: after marshaling & unmarshaling, XML name space: got %s, want %s\n", a.XMLName.Space, structSpace);
    }
    if (len(c.C.XMLName.Space) != 0) {
        t.Errorf("overidding with empty namespace: after marshaling & unmarshaling, got %s, want empty\n", a.C.XMLName.Space);
    }
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static ref readonly @string xmlSyntaxErrorOnLine1ˢ => ref xmlSyntaxErrorOnLine1ˢᵇ;
private static readonly @string xmlSyntaxErrorOnLine1ˢᵇ = "XML syntax error on line 1: expected attribute name in element"u8;
internal static ref readonly @string xmlSyntaxErrorOnLine1ˢ2 => ref xmlSyntaxErrorOnLine1ˢ2ᵇ;
private static readonly @string xmlSyntaxErrorOnLine1ˢ2ᵇ = "XML syntax error on line 1: expected element name after <"u8;

internal partial struct TestIssue20396_testCases /*dyn*/ {
    internal @string s;
    internal error wantErr;
}

public static void TestIssue20396(ж<testing.T> t) {
    global::go.encoding.xml_package.UnmarshalError attrError = ((global::go.encoding.xml_package.UnmarshalError)(@string)xmlSyntaxErrorOnLine1ˢ);

    var testCases = slice<TestIssue20396_testCases>([
        new(@"<a:te:st xmlns:a=""abcd""/>", // Issue 20396
            (global::go.encoding.xml_package.UnmarshalError)(@string)xmlSyntaxErrorOnLine1ˢ2),
        new(@"<a:te=st xmlns:a=""abcd""/>", attrError),
        new(@"<a:te&st xmlns:a=""abcd""/>", attrError),
        new(@"<a:test xmlns:a=""abcd""/>", default),
        new(@"<a:te:st xmlns:a=""abcd"">1</a:te:st>",
            (global::go.encoding.xml_package.UnmarshalError)(@string)xmlSyntaxErrorOnLine1ˢ2),
        new(@"<a:te=st xmlns:a=""abcd"">1</a:te=st>", attrError),
        new(@"<a:te&st xmlns:a=""abcd"">1</a:te&st>", attrError),
        new(@"<a:test xmlns:a=""abcd"">1</a:test>", default)
    ]);

    ref var dest = ref heap(new @string(), out var Ꮡdest);
    foreach (var (_, tc) in range(testCases)) {
        var (got, want) = (Unmarshal(slice<byte>(tc.s), Ꮡdest), tc.wantErr); if (!AreEqual(got, want)) {
            if (got == nil) {
                t.Errorf("%s: Unexpected success, want %v", tc.s, want);
            } else if (want == nil) {
                t.Errorf("%s: Unexpected error, got %v", tc.s, got);
            } else if (got.Error() != want.Error()) {
                t.Errorf("%s: got %v, want %v", tc.s, got, want);
            }
        }
    }
}

internal partial struct TestIssue20685_testCases /*dyn*/ {
    internal @string s;
    internal bool ok;
}

public static void TestIssue20685(ж<testing.T> t) {
    var testCases = slice<TestIssue20685_testCases>([
        new(@"<x:book xmlns:x=""abcd"" xmlns:y=""abcd""><unclosetag>one</x:book>", false),
        new(@"<x:book xmlns:x=""abcd"" xmlns:y=""abcd"">one</x:book>", true),
        new(@"<x:book xmlns:x=""abcd"" xmlns:y=""abcd"">one</y:book>", false),
        new(@"<x:book xmlns:y=""abcd"" xmlns:x=""abcd"">one</y:book>", false),
        new(@"<x:book xmlns:x=""abcd"">one</y:book>", false),
        new(@"<x:book>one</y:book>", false),
        new(@"<xbook>one</ybook>", false)
    ]);
    foreach (var (_, tc) in range(testCases)) {
        var d = NewDecoder(new xml_test_package.strings_ReaderжReader(strings.NewReader(tc.s)));
        error err = default;
        for (;;) {
            (_, err) = d.Token();
            if (err != nil) {
                if (AreEqual(err, io.EOF)) {
                    err = default;
                }
                break;
            }
        }
        if (err != nil && tc.ok) {
            t.Errorf("%q: Closing tag with namespace : expected no error, got %s", tc.s, err);
            continue;
        }
        if (err == nil && !tc.ok) {
            t.Errorf("%q: Closing tag with namespace : expected error, got nil", tc.s);
        }
    }
}

internal static Func<global::go.encoding.xml_package.TokenReader, global::go.encoding.xml_package.TokenReader> tokenMap(Func<ΔToken, ΔToken> mapping) {
    return (global::go.encoding.xml_package.TokenReader src) => new mapper(
            t: src,
            f: mapping
        );
}

internal partial struct mapper {
    internal global::go.encoding.xml_package.TokenReader t;
    internal Func<ΔToken, ΔToken> f;
}

internal static (ΔToken, error) Token(this mapper m) {
    var (tok, err) = m.t.Token();
    if (err != nil) {
        return (default, err);
    }
    return (m.f(tok), default);
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static readonly object newTokenDecoderDidNotˢ = (@string)"NewTokenDecoder did not detect underlying Decoder"u8;

public static void TestNewTokenDecoderIdempotent(ж<testing.T> t) {
    var d = NewDecoder(new xml_test_package.strings_ReaderжReader(strings.NewReader(@"<br>")));
    var d2 = NewTokenDecoder(new global::go.encoding.xml_package.DecoderжTokenReader(d));
    if (d != d2) {
        t.Error(newTokenDecoderDidNotˢ);
    }
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static ref readonly @string quoteReEnterClownWithAˢ => ref quoteReEnterClownWithAˢᵇ;
private static readonly @string quoteReEnterClownWithAˢᵇ = @"<quote>[Re-enter Clown with a letter, and FABIAN]</quote>"u8;
internal static ref readonly @string blockingˢ => ref blockingˢᵇ;
private static readonly @string blockingˢᵇ = "blocking"u8;
internal static readonly object gotUnexpectedErrorWhileˢ = (@string)"Got unexpected error while decoding:"u8;

internal partial struct TestWrapDecoder_o /*dyn*/ {
    public global::go.encoding.xml_package.Name XMLName; /*`xml:"blocking"`*/
    public @string Chardata; /*`xml:",chardata"`*/
}

public static void TestWrapDecoder(ж<testing.T> t) {
    var d = NewDecoder(new xml_test_package.strings_ReaderжReader(strings.NewReader(quoteReEnterClownWithAˢ)));
    var m = tokenMap((ΔToken tΔ1) => {
        switch (tΔ1.type()) {
        case StartElement tok: {
            if (tok.Name.Local == "quote"u8) {
                tok.Name.Local = blockingˢ;
                return tok;
            }
            break;
        }
        case EndElement tok: {
            if (tok.Name.Local == "quote"u8) {
                tok.Name.Local = blockingˢ;
                return tok;
            }
            break;
        }}
        return tΔ1;
    });

    d = NewTokenDecoder(m(new global::go.encoding.xml_package.DecoderжTokenReader(d)));

    ref var o = ref heap<TestWrapDecoder_o>(new TestWrapDecoder_o(), out var Ꮡo);

    var err = d.Decode(Ꮡo); if (err != nil) {
        t.Fatal(gotUnexpectedErrorWhileˢ, err);
    }

    if (o.Chardata != "[Re-enter Clown with a letter, and FABIAN]"u8) {
        t.Fatalf("Got unexpected chardata: `%s`\n", o.Chardata);
    }
}

internal partial struct tokReader {
}

internal static (ΔToken, error) Token(this tokReader _) {
    return (new StartElement(), default);
}

public partial struct Failure {
}

public static error UnmarshalXML(this Failure _Δp0, ж<global::go.encoding.xml_package.Decoder> _Δp1, global::go.encoding.xml_package.StartElement _Δp2) {
    return default;
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static readonly object unexpectedPanicUsingˢ = (@string)"Unexpected panic using custom token unmarshaler"u8;

public static void TestTokenUnmarshaler(ж<testing.T> t) {
    GoFrame ᒐ = default;
    try {
        defer(() => {
            var r = recover(); if (r != nil) {
                t.Error(unexpectedPanicUsingˢ);
            }
        }, ref ᒐ);

        var d = NewTokenDecoder(new tokReader());
        d.Decode(Ꮡ(new Failure()));
    }
    catch (Exception ᒐex) when (GoFrame.IsPanic(ᒐex, out PanicException? ᒐp)) { GoFrame.Capture(ᒐp); }
    finally { ᒐ.Run(); }
}

internal static void testRoundTrip(ж<testing.T> t, @string input) {
    var d = NewDecoder(new xml_test_package.strings_ReaderжReader(strings.NewReader(input)));
    slice<ΔToken> tokens = default;
    ref var buf = ref heap(new bytes.Buffer(), out var Ꮡbuf);
    var e = NewEncoder(new xml_test_package.bytes_BufferжWriter(Ꮡbuf));
    for (;;) {
        var (tok, err) = d.Token();
        if (AreEqual(err, io.EOF)) {
            break;
        }
        if (err != nil) {
            t.Fatalf("invalid input: %v", err);
        }
        var errΔ1 = e.EncodeToken(tok); if (errΔ1 != nil) {
            t.Fatalf("failed to re-encode input: %v", errΔ1);
        }
        tokens = append(tokens, CopyToken(tok));
    }
    {
        var err = e.Flush(); if (err != nil) {
            t.Fatal(err);
        }
    }

    d = NewDecoder(new xml_test_package.bytes_BufferжReader(Ꮡbuf));
    for (;;) {
        var (tok, err) = d.Token();
        if (AreEqual(err, io.EOF)) {
            break;
        }
        if (err != nil) {
            t.Fatalf("failed to decode output: %v", err);
        }
        if (len(tokens) == 0) {
            t.Fatalf("unexpected token: %#v", tok);
        }
        var (a, b) = (tokens[0], tok);
        if (!reflect.DeepEqual(a, b)) {
            t.Fatalf("token mismatch: %#v vs %#v", a, b);
        }
        tokens = tokens[1..];
    }
    if (len(tokens) > 0) {
        t.Fatalf("lost tokens: %#v", tokens);
    }
}

public static void TestRoundTrip(ж<testing.T> t) {
    var tests = new map<@string, @string>{
        ["trailing colon"] = @"<foo abc:=""x""></foo>",
        ["comments in directives"] = @"<!ENTITY x<!<!-- c1 [ "" -->--x --> > <e></e> <!DOCTYPE xxx [ x<!-- c2 "" -->--x ]>"
    };
    foreach (var (name, input) in tests) {
        t.Run(name, (ж<testing.T> tΔ1) => {
            testRoundTrip(tΔ1, input);
        });
    }
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static ref readonly @string fooˢ => ref fooˢᵇ;
private static readonly @string fooˢᵇ = @"</foo>"u8;
internal static ref readonly @string xFooYFooˢ => ref xFooYFooˢᵇ;
private static readonly @string xFooYFooˢᵇ = @"<x:foo></y:foo>"u8;
internal static ref readonly @string notOkˢ => ref notOkˢᵇ;
private static readonly @string notOkˢᵇ = @"<? not ok ?>"u8;
internal static ref readonly @string notOkˢ2 => ref notOkˢ2ᵇ;
private static readonly @string notOkˢ2ᵇ = @"<!- not ok -->"u8;
internal static ref readonly @string notOkˢ3 => ref notOkˢ3ᵇ;
private static readonly @string notOkˢ3ᵇ = @"<!-? not ok -->"u8;
internal static ref readonly @string notOkˢ4 => ref notOkˢ4ᵇ;
private static readonly @string notOkˢ4ᵇ = @"<![not ok]>"u8;
internal static ref readonly @string zzzFooXmlnsZzzHttpˢ => ref zzzFooXmlnsZzzHttpˢᵇ;
private static readonly @string zzzFooXmlnsZzzHttpˢᵇ = @"<zzz:foo xmlns:zzz=""http://example.com""><bar>baz</bar></foo>"u8;
internal static ref readonly @string okVersionOkˢ => ref okVersionOkˢᵇ;
private static readonly @string okVersionOkˢᵇ = @"<?ok version=""ok""?>"u8;

internal partial struct TestParseErrors_tests /*dyn*/ {
    internal @string src;
    internal @string err;
}

public static void TestParseErrors(ж<testing.T> t) {
    @string withDefaultHeader(@string s) => @"<?xml version=""1.0"" encoding=""UTF-8""?>"u8 + s;
    var tests = slice<TestParseErrors_tests>([
        new(withDefaultHeader(fooˢ), @"unexpected end element </foo>"),
        new(withDefaultHeader(xFooYFooˢ), @"element <foo> in space x closed by </foo> in space y"),
        new(withDefaultHeader(notOkˢ), @"expected target name after <?"),
        new(withDefaultHeader(notOkˢ2), @"invalid sequence <!- not part of <!--"),
        new(withDefaultHeader(notOkˢ3), @"invalid sequence <!- not part of <!--"),
        new(withDefaultHeader(notOkˢ4), @"invalid <![ sequence"),
        new(withDefaultHeader(zzzFooXmlnsZzzHttpˢ),
            @"element <foo> in space zzz closed by </foo> in space """""),
        new(withDefaultHeader(((@string)(new byte[]{0xf1}))), @"invalid UTF-8"),

        // Header-related errors.
        new(@"<?xml version=""1.1"" encoding=""UTF-8""?>", @"unsupported version ""1.1""; only version 1.0 is supported"),

        // Cases below are for "no errors".
        new(withDefaultHeader(@"<?ok?>"), @""),
        new(withDefaultHeader(okVersionOkˢ), @"")
    ]);

    foreach (var (_, test) in range(tests)) {
        var d = NewDecoder(new xml_test_package.strings_ReaderжReader(strings.NewReader(test.src)));
        error err = default;
        for (;;) {
            (_, err) = d.Token();
            if (err != nil) {
                break;
            }
        }
        if (test.err == ""u8) {
            if (!AreEqual(err, io.EOF)) {
                t.Errorf("parse %s: have %q error, expected none", test.src, err);
            }
            continue;
        }
        // Inv: err != nil
        if (AreEqual(err, io.EOF)) {
            t.Errorf("parse %s: unexpected EOF", test.src);
            continue;
        }
        if (!strings.Contains(err.Error(), test.err)) {
            t.Errorf("parse %s: can't find %q error substring\nerror: %q", test.src, test.err, err);
            continue;
        }
    }
}

internal static ref readonly @string testInputHTMLAutoClose => ref testInputHTMLAutoCloseᵇ;
private static readonly @string testInputHTMLAutoCloseᵇ = """
<?xml version="1.0" encoding="UTF-8"?>
<br>
<br/><br/>
<br><br>
<br></br>
<BR>
<BR/><BR/>
<Br></Br>
<BR><span id="test">abc</span><br/><br/>
"""u8.Raw();

public static void BenchmarkHTMLAutoClose(ж<testing.B> b) {
    b.RunParallel((ж<testing.PB> p) => {
        while (p.Next()) {
            var d = NewDecoder(new xml_test_package.strings_ReaderжReader(strings.NewReader(testInputHTMLAutoClose)));
            d.Strict = false;
            d.AutoClose = HTMLAutoClose;
            d.Entity = HTMLEntity;
            for (;;) {
                var (_, err) = d.Token();
                if (err != nil) {
                    if (AreEqual(err, io.EOF)) {
                        break;
                    }
                    b.Fatalf("unexpected error: %v", err);
                }
            }
        }
    });
}

public static void TestHTMLAutoClose(ж<testing.T> t) {
    var haveTokensᴷ = new stackAppend<ΔToken>();

    var wantTokens = slice<ΔToken>([
        new ProcInst("xml", slice<byte>(@"version=""1.0"" encoding=""UTF-8"""u8)),
        (global::go.encoding.xml_package.CharData)slice<byte>((@string)"\n"u8),
        new StartElement(new Name("", "br"), []),
        new EndElement(new Name("", "br")),
        (global::go.encoding.xml_package.CharData)slice<byte>((@string)"\n"u8),
        new StartElement(new Name("", "br"), []),
        new EndElement(new Name("", "br")),
        new StartElement(new Name("", "br"), []),
        new EndElement(new Name("", "br")),
        (global::go.encoding.xml_package.CharData)slice<byte>((@string)"\n"u8),
        new StartElement(new Name("", "br"), []),
        new EndElement(new Name("", "br")),
        new StartElement(new Name("", "br"), []),
        new EndElement(new Name("", "br")),
        (global::go.encoding.xml_package.CharData)slice<byte>((@string)"\n"u8),
        new StartElement(new Name("", "br"), []),
        new EndElement(new Name("", "br")),
        (global::go.encoding.xml_package.CharData)slice<byte>((@string)"\n"u8),
        new StartElement(new Name("", "BR"), []),
        new EndElement(new Name("", "BR")),
        (global::go.encoding.xml_package.CharData)slice<byte>((@string)"\n"u8),
        new StartElement(new Name("", "BR"), []),
        new EndElement(new Name("", "BR")),
        new StartElement(new Name("", "BR"), []),
        new EndElement(new Name("", "BR")),
        (global::go.encoding.xml_package.CharData)slice<byte>((@string)"\n"u8),
        new StartElement(new Name("", "Br"), []),
        new EndElement(new Name("", "Br")),
        (global::go.encoding.xml_package.CharData)slice<byte>((@string)"\n"u8),
        new StartElement(new Name("", "BR"), []),
        new EndElement(new Name("", "BR")),
        new StartElement(new Name("", "span"), [new(Name: new Name("", "id"), Value: "test")]),
        (global::go.encoding.xml_package.CharData)slice<byte>((@string)"abc"u8),
        new EndElement(new Name("", "span")),
        new StartElement(new Name("", "br"), []),
        new EndElement(new Name("", "br")),
        new StartElement(new Name("", "br"), []),
        new EndElement(new Name("", "br"))
    ]);

    var d = NewDecoder(new xml_test_package.strings_ReaderжReader(strings.NewReader(testInputHTMLAutoClose)));
    d.Strict = false;
    d.AutoClose = HTMLAutoClose;
    d.Entity = HTMLEntity;
    slice<ΔToken> haveTokens = default;
    for (;;) {
        var (tok, err) = d.Token();
        if (err != nil) {
            if (AreEqual(err, io.EOF)) {
                break;
            }
            t.Fatalf("unexpected error: %v", err);
        }
        haveTokens = append(haveTokensᴷ, haveTokens, CopyToken(tok));
    }
    if (len(haveTokens) != len(wantTokens)) {
        t.Errorf("tokens count mismatch: have %d, want %d", len(haveTokens), len(wantTokens));
    }
    foreach (var (i, want) in range(wantTokens)) {
        if (i >= len(haveTokens)) {
            t.Errorf("token[%d] expected %#v, have no token", i, want);
        } else {
            var have = haveTokens[i];
            if (!reflect.DeepEqual(have, want)) {
                t.Errorf("token[%d] mismatch:\nhave: %#v\nwant: %#v", i, have, want);
            }
        }
    }
}

} // end xml_internal_test_package
