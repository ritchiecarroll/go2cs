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
        return (default!, io.EOF);
    }
    ΔToken tok = default!;
    (tok, t.t) = (t.t[0], t.t[1..]);
    if (t.earlyEOF && len(t.t) == 0) {
        return (tok, io.EOF);
    }
    return (tok, default!);
}

internal partial struct TestDecodeEOF_tests /*dyn*/ {
    internal @string name;
    internal slice<ΔToken> tokens;
    internal bool ok;
}

internal partial struct TestDecodeEOF_type /*dyn*/ {
    public global::go.encoding.xml_package.Name XMLName; /*`xml:"test"`*/
}

public static void TestDecodeEOF(ж<testing.T> Ꮡt) {
    var start = new StartElement(Name: new Name(Local: "test"u8));
    var tests = new TestDecodeEOF_tests[]{
        new(
            name: "OK"u8,
            tokens: new ΔToken[]{
                start,
                start.End()
            }.slice(),
            ok: true
        ),
        new(
            name: "Malformed"u8,
            tokens: new ΔToken[]{
                start,
                new StartElement(Name: new Name(Local: "bad"u8)),
                start.End()
            }.slice(),
            ok: false
        )
    }.slice();
    foreach (var (_, vᴛ1) in tests) {
        ref var tc = ref heap(new TestDecodeEOF_tests(), out var Ꮡtc);
        tc = vᴛ1;

        foreach (var (_, vᴛ2) in new bool[]{true, false}.slice()) {
            ref var eof = ref heap(new bool(), out var Ꮡeof);
            eof = vᴛ2;

            @string name = fmt.Sprintf("%s/earlyEOF=%v"u8, tc.name, eof);
            var eofʗ1 = eof;
            var tcʗ1 = tc;
            Ꮡt.Run(name, (ж<testing.T> tΔ1) => {
                var d = NewTokenDecoder(new xml_internal_test_package.toksжTokenReader(Ꮡ(new toks(
                    earlyEOF: eofʗ1,
                    t: tcʗ1.tokens
                ))));
                var err = d.Decode(Ꮡ(new TestDecodeEOF_type()));
                if (tcʗ1.ok && err != default!) {
                    tΔ1.Fatalf("d.Decode: expected nil error, got %v"u8, err);
                }
                {
                    var (_, ok) = err._<ж<global::go.encoding.xml_package.SyntaxError>>(ᐧ); if (!tcʗ1.ok && !ok) {
                        tΔ1.Errorf("d.Decode: expected syntax error, got %v"u8, err);
                    }
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
            return (default!, default!);
        }
        return (default!, io.EOF);
    }
    ΔToken tok = default!;
    (tok, t.t) = (t.t[0], t.t[1..]);
    return (tok, default!);
}

internal partial struct TestDecodeNilToken_type /*dyn*/ {
    public global::go.encoding.xml_package.Name XMLName; /*`xml:"test"`*/
}

public static void TestDecodeNilToken(ж<testing.T> Ꮡt) {
    ref var t = ref Ꮡt.DerefOrNull();

    foreach (var (_, strict) in new bool[]{true, false}.slice()) {
        @string name = fmt.Sprintf("Strict=%v"u8, strict);
        Ꮡt.Run(name, (ж<testing.T> tΔ1) => {
            var start = new StartElement(Name: new Name(Local: "test"u8));
            var bad = new StartElement(Name: new Name(Local: "bad"u8));
            var d = NewTokenDecoder(new xml_internal_test_package.toksNilжTokenReader(Ꮡ(new toksNil( // Malformed

                t: new ΔToken[]{start, bad, start.End()}.slice()
            ))));
            d.Value.Strict = strict;
            var err = d.Decode(Ꮡ(new TestDecodeNilToken_type()));
            {
                var (_, ok) = err._<ж<global::go.encoding.xml_package.SyntaxError>>(ᐧ); if (!ok) {
                    tΔ1.Errorf("d.Decode: expected syntax error, got %v"u8, err);
                }
            }
        });
    }
}

internal static readonly @string testInput = "\n<?xml version=\"1.0\" encoding=\"UTF-8\"?>\n<!DOCTYPE html PUBLIC \"-//W3C//DTD XHTML 1.0 Transitional//EN\"\n  \"http://www.w3.org/TR/xhtml1/DTD/xhtml1-transitional.dtd\">\n<body xmlns:foo=\"ns1\" xmlns=\"ns2\" xmlns:tag=\"ns3\" \r\n\t  >\n  <hello lang=\"en\">World &lt;&gt;&apos;&quot; &#x767d;&#40300;翔</hello>\n  <query>&何; &is-it;</query>\n  <goodbye />\n  <outer foo:attr=\"value\" xmlns:tag=\"ns4\">\n    <inner/>\n  </outer>\n  <tag:name>\n    <![CDATA[Some text here.]]>\n  </tag:name>\n</body><!-- missing final newline -->";

internal static map<@string, @string> testEntity = new map<@string, @string>{["何"u8] = "What"u8, ["is-it"u8] = "is it?"u8};

internal static slice<ΔToken> rawTokens = new ΔToken[]{
    ((global::go.encoding.xml_package.CharData)slice<byte>((@string)"\n"u8)),
    new ProcInst("xml"u8, slice<byte>(@"version=""1.0"" encoding=""UTF-8"""u8)),
    ((global::go.encoding.xml_package.CharData)slice<byte>((@string)"\n"u8)),
    ((global::go.encoding.xml_package.Directive)slice<byte>((@string)"""
DOCTYPE html PUBLIC "-//W3C//DTD XHTML 1.0 Transitional//EN"
  "http://www.w3.org/TR/xhtml1/DTD/xhtml1-transitional.dtd"
"""u8)),
    ((global::go.encoding.xml_package.CharData)slice<byte>((@string)"\n"u8)),
    new StartElement(new Name(""u8, "body"u8), new global::go.encoding.xml_package.Attr[]{new(new Name("xmlns"u8, "foo"u8), "ns1"u8), new(new Name(""u8, "xmlns"u8), "ns2"u8), new(new Name("xmlns"u8, "tag"u8), "ns3"u8)}.slice()),
    ((global::go.encoding.xml_package.CharData)slice<byte>((@string)"\n  "u8)),
    new StartElement(new Name(""u8, "hello"u8), new global::go.encoding.xml_package.Attr[]{new(new Name(""u8, "lang"u8), "en"u8)}.slice()),
    ((global::go.encoding.xml_package.CharData)slice<byte>((@string)"World <>'\" 白鵬翔"u8)),
    new EndElement(new Name(""u8, "hello"u8)),
    ((global::go.encoding.xml_package.CharData)slice<byte>((@string)"\n  "u8)),
    new StartElement(new Name(""u8, "query"u8), new global::go.encoding.xml_package.Attr[]{}.slice()),
    ((global::go.encoding.xml_package.CharData)slice<byte>((@string)"What is it?"u8)),
    new EndElement(new Name(""u8, "query"u8)),
    ((global::go.encoding.xml_package.CharData)slice<byte>((@string)"\n  "u8)),
    new StartElement(new Name(""u8, "goodbye"u8), new global::go.encoding.xml_package.Attr[]{}.slice()),
    new EndElement(new Name(""u8, "goodbye"u8)),
    ((global::go.encoding.xml_package.CharData)slice<byte>((@string)"\n  "u8)),
    new StartElement(new Name(""u8, "outer"u8), new global::go.encoding.xml_package.Attr[]{new(new Name("foo"u8, "attr"u8), "value"u8), new(new Name("xmlns"u8, "tag"u8), "ns4"u8)}.slice()),
    ((global::go.encoding.xml_package.CharData)slice<byte>((@string)"\n    "u8)),
    new StartElement(new Name(""u8, "inner"u8), new global::go.encoding.xml_package.Attr[]{}.slice()),
    new EndElement(new Name(""u8, "inner"u8)),
    ((global::go.encoding.xml_package.CharData)slice<byte>((@string)"\n  "u8)),
    new EndElement(new Name(""u8, "outer"u8)),
    ((global::go.encoding.xml_package.CharData)slice<byte>((@string)"\n  "u8)),
    new StartElement(new Name("tag"u8, "name"u8), new global::go.encoding.xml_package.Attr[]{}.slice()),
    ((global::go.encoding.xml_package.CharData)slice<byte>((@string)"\n    "u8)),
    ((global::go.encoding.xml_package.CharData)slice<byte>((@string)"Some text here."u8)),
    ((global::go.encoding.xml_package.CharData)slice<byte>((@string)"\n  "u8)),
    new EndElement(new Name("tag"u8, "name"u8)),
    ((global::go.encoding.xml_package.CharData)slice<byte>((@string)"\n"u8)),
    new EndElement(new Name(""u8, "body"u8)),
    ((global::go.encoding.xml_package.Comment)slice<byte>((@string)" missing final newline "u8))
}.slice();

internal static slice<ΔToken> cookedTokens = new ΔToken[]{
    ((global::go.encoding.xml_package.CharData)slice<byte>((@string)"\n"u8)),
    new ProcInst("xml"u8, slice<byte>(@"version=""1.0"" encoding=""UTF-8"""u8)),
    ((global::go.encoding.xml_package.CharData)slice<byte>((@string)"\n"u8)),
    ((global::go.encoding.xml_package.Directive)slice<byte>((@string)"""
DOCTYPE html PUBLIC "-//W3C//DTD XHTML 1.0 Transitional//EN"
  "http://www.w3.org/TR/xhtml1/DTD/xhtml1-transitional.dtd"
"""u8)),
    ((global::go.encoding.xml_package.CharData)slice<byte>((@string)"\n"u8)),
    new StartElement(new Name("ns2"u8, "body"u8), new global::go.encoding.xml_package.Attr[]{new(new Name("xmlns"u8, "foo"u8), "ns1"u8), new(new Name(""u8, "xmlns"u8), "ns2"u8), new(new Name("xmlns"u8, "tag"u8), "ns3"u8)}.slice()),
    ((global::go.encoding.xml_package.CharData)slice<byte>((@string)"\n  "u8)),
    new StartElement(new Name("ns2"u8, "hello"u8), new global::go.encoding.xml_package.Attr[]{new(new Name(""u8, "lang"u8), "en"u8)}.slice()),
    ((global::go.encoding.xml_package.CharData)slice<byte>((@string)"World <>'\" 白鵬翔"u8)),
    new EndElement(new Name("ns2"u8, "hello"u8)),
    ((global::go.encoding.xml_package.CharData)slice<byte>((@string)"\n  "u8)),
    new StartElement(new Name("ns2"u8, "query"u8), new global::go.encoding.xml_package.Attr[]{}.slice()),
    ((global::go.encoding.xml_package.CharData)slice<byte>((@string)"What is it?"u8)),
    new EndElement(new Name("ns2"u8, "query"u8)),
    ((global::go.encoding.xml_package.CharData)slice<byte>((@string)"\n  "u8)),
    new StartElement(new Name("ns2"u8, "goodbye"u8), new global::go.encoding.xml_package.Attr[]{}.slice()),
    new EndElement(new Name("ns2"u8, "goodbye"u8)),
    ((global::go.encoding.xml_package.CharData)slice<byte>((@string)"\n  "u8)),
    new StartElement(new Name("ns2"u8, "outer"u8), new global::go.encoding.xml_package.Attr[]{new(new Name("ns1"u8, "attr"u8), "value"u8), new(new Name("xmlns"u8, "tag"u8), "ns4"u8)}.slice()),
    ((global::go.encoding.xml_package.CharData)slice<byte>((@string)"\n    "u8)),
    new StartElement(new Name("ns2"u8, "inner"u8), new global::go.encoding.xml_package.Attr[]{}.slice()),
    new EndElement(new Name("ns2"u8, "inner"u8)),
    ((global::go.encoding.xml_package.CharData)slice<byte>((@string)"\n  "u8)),
    new EndElement(new Name("ns2"u8, "outer"u8)),
    ((global::go.encoding.xml_package.CharData)slice<byte>((@string)"\n  "u8)),
    new StartElement(new Name("ns3"u8, "name"u8), new global::go.encoding.xml_package.Attr[]{}.slice()),
    ((global::go.encoding.xml_package.CharData)slice<byte>((@string)"\n    "u8)),
    ((global::go.encoding.xml_package.CharData)slice<byte>((@string)"Some text here."u8)),
    ((global::go.encoding.xml_package.CharData)slice<byte>((@string)"\n  "u8)),
    new EndElement(new Name("ns3"u8, "name"u8)),
    ((global::go.encoding.xml_package.CharData)slice<byte>((@string)"\n"u8)),
    new EndElement(new Name("ns2"u8, "body"u8)),
    ((global::go.encoding.xml_package.Comment)slice<byte>((@string)" missing final newline "u8))
}.slice();

internal static readonly @string testInputAltEncoding = """

<?xml version="1.0" encoding="x-testing-uppercase"?>
<TAG>VALUE</TAG>
"""u8;

internal static slice<ΔToken> rawTokensAltEncoding = new ΔToken[]{
    ((global::go.encoding.xml_package.CharData)slice<byte>((@string)"\n"u8)),
    new ProcInst("xml"u8, slice<byte>(@"version=""1.0"" encoding=""x-testing-uppercase"""u8)),
    ((global::go.encoding.xml_package.CharData)slice<byte>((@string)"\n"u8)),
    new StartElement(new Name(""u8, "tag"u8), new global::go.encoding.xml_package.Attr[]{}.slice()),
    ((global::go.encoding.xml_package.CharData)slice<byte>((@string)"value"u8)),
    new EndElement(new Name(""u8, "tag"u8))
}.slice();

// unexpected EOF cases
// other Syntax errors
//	"<!0 >",	// let the Token() caller handle
//	"<![CDATA[d]]>",	// let the Token() caller handle
internal static slice<@string> xmlInput = new @string[]{
    "<"u8,
    "<t"u8,
    "<t "u8,
    "<t/"u8,
    "<!"u8,
    "<!-"u8,
    "<!--"u8,
    "<!--c-"u8,
    "<!--c--"u8,
    "<!d"u8,
    "<t></"u8,
    "<t></t"u8,
    "<?"u8,
    "<?p"u8,
    "<t a"u8,
    "<t a="u8,
    "<t a='"u8,
    "<t a=''"u8,
    "<t/><!["u8,
    "<t/><![C"u8,
    "<t/><![CDATA[d"u8,
    "<t/><![CDATA[d]"u8,
    "<t/><![CDATA[d]]"u8,
    "<>"u8,
    "<t/a"u8,
    "<0 />"u8,
    "<?0 >"u8,
    "</0>"u8,
    "<t 0=''>"u8,
    "<t a='&'>"u8,
    "<t a='<'>"u8,
    "<t>&nbspc;</t>"u8,
    "<t a>"u8,
    "<t a=>"u8,
    "<t a=v>"u8,
    "<t></e>"u8,
    "<t></>"u8,
    "<t></t!"u8,
    "<t>cdata]]></t>"u8
}.slice();

public static void TestRawToken(ж<testing.T> Ꮡt) {
    var d = NewDecoder(new xml_test_package.strings_ReaderжReader(strings.NewReader(testInput)));
    d.Value.Entity = testEntity;
    testRawToken(Ꮡt, d, testInput, rawTokens);
}

internal static readonly @string nonStrictInput = """

<tag>non&entity</tag>
<tag>&unknown;entity</tag>
<tag>&#123</tag>
<tag>&#zzz;</tag>
<tag>&なまえ3;</tag>
<tag>&lt-gt;</tag>
<tag>&;</tag>
<tag>&0a;</tag>

"""u8;

internal static slice<ΔToken> nonStrictTokens = new ΔToken[]{
    ((global::go.encoding.xml_package.CharData)slice<byte>((@string)"\n"u8)),
    new StartElement(new Name(""u8, "tag"u8), new global::go.encoding.xml_package.Attr[]{}.slice()),
    ((global::go.encoding.xml_package.CharData)slice<byte>((@string)"non&entity"u8)),
    new EndElement(new Name(""u8, "tag"u8)),
    ((global::go.encoding.xml_package.CharData)slice<byte>((@string)"\n"u8)),
    new StartElement(new Name(""u8, "tag"u8), new global::go.encoding.xml_package.Attr[]{}.slice()),
    ((global::go.encoding.xml_package.CharData)slice<byte>((@string)"&unknown;entity"u8)),
    new EndElement(new Name(""u8, "tag"u8)),
    ((global::go.encoding.xml_package.CharData)slice<byte>((@string)"\n"u8)),
    new StartElement(new Name(""u8, "tag"u8), new global::go.encoding.xml_package.Attr[]{}.slice()),
    ((global::go.encoding.xml_package.CharData)slice<byte>((@string)"&#123"u8)),
    new EndElement(new Name(""u8, "tag"u8)),
    ((global::go.encoding.xml_package.CharData)slice<byte>((@string)"\n"u8)),
    new StartElement(new Name(""u8, "tag"u8), new global::go.encoding.xml_package.Attr[]{}.slice()),
    ((global::go.encoding.xml_package.CharData)slice<byte>((@string)"&#zzz;"u8)),
    new EndElement(new Name(""u8, "tag"u8)),
    ((global::go.encoding.xml_package.CharData)slice<byte>((@string)"\n"u8)),
    new StartElement(new Name(""u8, "tag"u8), new global::go.encoding.xml_package.Attr[]{}.slice()),
    ((global::go.encoding.xml_package.CharData)slice<byte>((@string)"&なまえ3;"u8)),
    new EndElement(new Name(""u8, "tag"u8)),
    ((global::go.encoding.xml_package.CharData)slice<byte>((@string)"\n"u8)),
    new StartElement(new Name(""u8, "tag"u8), new global::go.encoding.xml_package.Attr[]{}.slice()),
    ((global::go.encoding.xml_package.CharData)slice<byte>((@string)"&lt-gt;"u8)),
    new EndElement(new Name(""u8, "tag"u8)),
    ((global::go.encoding.xml_package.CharData)slice<byte>((@string)"\n"u8)),
    new StartElement(new Name(""u8, "tag"u8), new global::go.encoding.xml_package.Attr[]{}.slice()),
    ((global::go.encoding.xml_package.CharData)slice<byte>((@string)"&;"u8)),
    new EndElement(new Name(""u8, "tag"u8)),
    ((global::go.encoding.xml_package.CharData)slice<byte>((@string)"\n"u8)),
    new StartElement(new Name(""u8, "tag"u8), new global::go.encoding.xml_package.Attr[]{}.slice()),
    ((global::go.encoding.xml_package.CharData)slice<byte>((@string)"&0a;"u8)),
    new EndElement(new Name(""u8, "tag"u8)),
    ((global::go.encoding.xml_package.CharData)slice<byte>((@string)"\n"u8))
}.slice();

public static void TestNonStrictRawToken(ж<testing.T> Ꮡt) {
    var d = NewDecoder(new xml_test_package.strings_ReaderжReader(strings.NewReader(nonStrictInput)));
    d.Value.Strict = false;
    testRawToken(Ꮡt, d, nonStrictInput, nonStrictTokens);
}

internal partial struct downCaser {
    internal ж<testing.T> t;
    internal io.ByteReader r;
}

internal static (byte c, error err) ReadByte(this ref downCaser d) {
    byte c = default!;
    error err = default!;

    (c, err) = d.r.ReadByte();
    if (c >= (rune)'A' && c <= (rune)'Z') {
        c += (byte)((rune)'a' - (rune)'A');
    }
    return (c, err);
}

internal static (nint, error) Read(this ref downCaser d, slice<byte> p) {
    d.t.Fatalf("unexpected Read call on downCaser reader"u8);
    throw panic("unreachable");
}

public static void TestRawTokenAltEncoding(ж<testing.T> Ꮡt) {
    var d = NewDecoder(new xml_test_package.strings_ReaderжReader(strings.NewReader(testInputAltEncoding)));
    d.Value.CharsetReader = (io.Reader, error) (@string charset, io.Reader input) => {
        if (charset != "x-testing-uppercase"u8) {
            Ꮡt.Fatalf("unexpected charset %q"u8, charset);
        }
        return (new xml_internal_test_package.downCaserжReader(Ꮡ(new downCaser(Ꮡt, input._<io.ByteReader>()))), default!);
    };
    testRawToken(Ꮡt, d, testInputAltEncoding, rawTokensAltEncoding);
}

// Hoisted Go string constant (single allocation; Go keeps it in RODATA)
internal static readonly @string encodingᶜ = "x-testing-uppercase"u8;

public static void TestRawTokenAltEncodingNoConverter(ж<testing.T> Ꮡt) {
    var d = NewDecoder(new xml_test_package.strings_ReaderжReader(strings.NewReader(testInputAltEncoding)));
    var (token, err) = d.RawToken();
    if (token == default!) {
        Ꮡt.Fatalf("expected a token on first RawToken call"u8);
    }
    if (err != default!) {
        Ꮡt.Fatal(err);
    }
    (token, err) = d.RawToken();
    if (token != default!) {
        Ꮡt.Errorf("expected a nil token; got %#v"u8, token);
    }
    if (err == default!) {
        Ꮡt.Fatalf("expected an error on second RawToken call"u8);
    }
    @string encoding = encodingᶜ;
    if (!strings.Contains(err.Error(), encoding)) {
        Ꮡt.Errorf("expected error to contain %q; got error: %v"u8,
            encoding, err);
    }
}

internal static void testRawToken(ж<testing.T> Ꮡt, ж<global::go.encoding.xml_package.Decoder> Ꮡd, @string raw, slice<ΔToken> rawTokens) {
    ref var d = ref Ꮡd.DerefOrNull();

    var lastEnd = (int64)0;
    foreach (var (i, want) in rawTokens) {
        var start = d.InputOffset();
        var (have, err) = d.RawToken();
        var end = d.InputOffset();
        if (err != default!) {
            Ꮡt.Fatalf("token %d: unexpected error: %s"u8, i, err);
        }
        if (!reflect.DeepEqual(have, want)) {
            @string shave = default!;
            @string swant = default!;
            {
                var (_, ok) = have._<CharData>(ᐧ); if (ok){
                    shave = fmt.Sprintf("CharData(%q)"u8, have);
                } else {
                    shave = fmt.Sprintf("%#v"u8, have);
                }
            }
            {
                var (_, ok) = want._<CharData>(ᐧ); if (ok){
                    swant = fmt.Sprintf("CharData(%q)"u8, want);
                } else {
                    swant = fmt.Sprintf("%#v"u8, want);
                }
            }
            Ꮡt.Errorf("token %d = %s, want %s"u8, i, shave, swant);
        }
        // Check that InputOffset returned actual token.
        switch (ᐧ) {
        case {} when start < lastEnd: {
            Ꮡt.Errorf("token %d: position [%d,%d) for %T is before previous token"u8, i, start, end, have);
            break;
        }
        case {} when start >= end: {
            if (start == end && end == lastEnd) {
                // Special case: EndElement can be synthesized.
                break;
            }
            Ꮡt.Errorf("token %d: position [%d,%d) for %T is empty"u8, i, start, end, have);
            break;
        }
        case {} when end > (int64)len(raw): {
            Ꮡt.Errorf("token %d: position [%d,%d) for %T extends beyond input"u8, i, start, end, have);
            break;
        }
        default: {
            @string text = raw.slice((nint)(start), (nint)(end));
            if (strings.ContainsAny(text, "<>"u8) && (!strings.HasPrefix(text, "<"u8) || !strings.HasSuffix(text, ">"u8))) {
                Ꮡt.Errorf("token %d: misaligned raw token %#q for %T"u8, i, text, have);
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

"""u8;

internal static slice<ΔToken> nestedDirectivesTokens = new ΔToken[]{
    ((global::go.encoding.xml_package.CharData)slice<byte>((@string)"\n"u8)),
    ((global::go.encoding.xml_package.Directive)slice<byte>((@string)@"DOCTYPE [<!ENTITY rdf ""http://www.w3.org/1999/02/22-rdf-syntax-ns#"">]"u8)),
    ((global::go.encoding.xml_package.CharData)slice<byte>((@string)"\n"u8)),
    ((global::go.encoding.xml_package.Directive)slice<byte>((@string)@"DOCTYPE [<!ENTITY xlt "">"">]"u8)),
    ((global::go.encoding.xml_package.CharData)slice<byte>((@string)"\n"u8)),
    ((global::go.encoding.xml_package.Directive)slice<byte>((@string)@"DOCTYPE [<!ENTITY xlt ""<"">]"u8)),
    ((global::go.encoding.xml_package.CharData)slice<byte>((@string)"\n"u8)),
    ((global::go.encoding.xml_package.Directive)slice<byte>((@string)@"DOCTYPE [<!ENTITY xlt '>'>]"u8)),
    ((global::go.encoding.xml_package.CharData)slice<byte>((@string)"\n"u8)),
    ((global::go.encoding.xml_package.Directive)slice<byte>((@string)@"DOCTYPE [<!ENTITY xlt '<'>]"u8)),
    ((global::go.encoding.xml_package.CharData)slice<byte>((@string)"\n"u8)),
    ((global::go.encoding.xml_package.Directive)slice<byte>((@string)@"DOCTYPE [<!ENTITY xlt '"">'>]"u8)),
    ((global::go.encoding.xml_package.CharData)slice<byte>((@string)"\n"u8)),
    ((global::go.encoding.xml_package.Directive)slice<byte>((@string)@"DOCTYPE [<!ENTITY xlt ""'<"">]"u8)),
    ((global::go.encoding.xml_package.CharData)slice<byte>((@string)"\n"u8))
}.slice();

public static void TestNestedDirectives(ж<testing.T> Ꮡt) {
    var d = NewDecoder(new xml_test_package.strings_ReaderжReader(strings.NewReader(nestedDirectivesInput)));
    foreach (var (i, want) in nestedDirectivesTokens) {
        var (have, err) = d.Token();
        if (err != default!) {
            Ꮡt.Fatalf("token %d: unexpected error: %s"u8, i, err);
        }
        if (!reflect.DeepEqual(have, want)) {
            Ꮡt.Errorf("token %d = %#v want %#v"u8, i, have, want);
        }
    }
}

public static void TestToken(ж<testing.T> Ꮡt) {
    var d = NewDecoder(new xml_test_package.strings_ReaderжReader(strings.NewReader(testInput)));
    d.Value.Entity = testEntity;
    foreach (var (i, want) in cookedTokens) {
        var (have, err) = d.Token();
        if (err != default!) {
            Ꮡt.Fatalf("token %d: unexpected error: %s"u8, i, err);
        }
        if (!reflect.DeepEqual(have, want)) {
            Ꮡt.Errorf("token %d = %#v want %#v"u8, i, have, want);
        }
    }
}

public static void TestSyntax(ж<testing.T> Ꮡt) {
    foreach (var (i, _) in xmlInput) {
        var d = NewDecoder(new xml_test_package.strings_ReaderжReader(strings.NewReader(xmlInput[i])));
        error err = default!;
        for ((_, err) = d.Token(); err == default!; (_, err) = d.Token()) {
        }
        {
            var (_, ok) = err._<ж<global::go.encoding.xml_package.SyntaxError>>(ᐧ); if (!ok) {
                Ꮡt.Fatalf(@"xmlInput ""%s"": expected SyntaxError not received"u8, xmlInput[i]);
            }
        }
    }
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static readonly @string rootPiEltAttValCdataEltˢ = """
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
"""u8;

public static void TestInputLinePos(ж<testing.T> Ꮡt) {
    @string testInput = rootPiEltAttValCdataEltˢ;
    var linePos = new slice<nint>[]{
        new nint[]{1, 7}.slice(),
        new nint[]{2, 1}.slice(),
        new nint[]{3, 4}.slice(),
        new nint[]{3, 6}.slice(),
        new nint[]{6, 7}.slice(),
        new nint[]{7, 1}.slice(),
        new nint[]{8, 4}.slice(),
        new nint[]{10, 4}.slice(),
        new nint[]{10, 10}.slice(),
        new nint[]{11, 1}.slice(),
        new nint[]{11, 8}.slice()
    }.slice();
    var dec = NewDecoder(new xml_test_package.strings_ReaderжReader(strings.NewReader(testInput)));
    foreach (var (_, want) in linePos) {
        {
            var (_, err) = dec.Token(); if (err != default!) {
                Ꮡt.Errorf("Unexpected error: %v"u8, err);
                continue;
            }
        }
        var (gotLine, gotCol) = dec.InputPos();
        if (gotLine != want[0] || gotCol != want[1]) {
            Ꮡt.Errorf("dec.InputPos() = %d,%d, want %d,%d"u8, gotLine, gotCol, want[0], want[1]);
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
    String: "15"u8,
    PtrString: Ꮡsixteen
); }

internal static ж<@string> Ꮡsixteen = new StandardBox<@string>("16"u8);
internal static ref @string sixteen => ref Ꮡsixteen.Value;

internal static readonly @string testScalarsInput = """
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
"""u8;

public static void TestAllScalars(ж<testing.T> Ꮡt) {
    ref var a = ref heap(new allScalars(), out var Ꮡa);
    var err = Unmarshal(slice<byte>(testScalarsInput), Ꮡa);
    if (err != default!) {
        Ꮡt.Fatal(err);
    }
    if (!reflect.DeepEqual(a, all)) {
        Ꮡt.Errorf("have %+v want %+v"u8, a, all);
    }
}

internal partial struct item {
    public @string FieldA;
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static readonly @string itemBˢ = @"<item b=']]>'/>"u8;
internal static readonly object wrongStartElementˢ = (@string)"Wrong start element"u8;
internal static readonly object wrongEndElementˢ = (@string)"Wrong end element"u8;

public static void TestIssue68387(ж<testing.T> Ꮡt) {
    @string data = itemBˢ;
    var dec = NewDecoder(new xml_test_package.strings_ReaderжReader(strings.NewReader(data)));
    ΔToken tok1 = default!;
    ΔToken tok2 = default!;
    ΔToken tok3 = default!;
    error err = default!;
    {
        (tok1, err) = dec.RawToken(); if (err != default!) {
            Ꮡt.Fatalf("RawToken() failed: %v"u8, err);
        }
    }
    {
        (tok2, err) = dec.RawToken(); if (err != default!) {
            Ꮡt.Fatalf("RawToken() failed: %v"u8, err);
        }
    }
    {
        (tok3, err) = dec.RawToken(); if (!AreEqual(err, io.EOF) || tok3 != default!) {
            Ꮡt.Fatalf("Missed EOF"u8);
        }
    }
    var s = new StartElement(new Name(""u8, "item"u8), new global::go.encoding.xml_package.Attr[]{new Attr(new Name(""u8, "b"u8), "]]>"u8)}.slice());
    if (!reflect.DeepEqual(tok1._<StartElement>(), s)) {
        Ꮡt.Error(wrongStartElementˢ);
    }
    var e = new EndElement(new Name(""u8, "item"u8));
    if (tok2._<EndElement>() != e) {
        Ꮡt.Error(wrongEndElementˢ);
    }
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static readonly @string itemFieldAAbcdFieldAItemˢ = @"<item><FieldA>abcd</FieldA></item>"u8;
internal static readonly object expectingAbcdˢ = (@string)"Expecting abcd"u8;

public static void TestIssue569(ж<testing.T> Ꮡt) {
    @string data = itemFieldAAbcdFieldAItemˢ;
    ref var i = ref heap(new item(), out var Ꮡi);
    var err = Unmarshal(slice<byte>(data), Ꮡi);
    if (err != default! || i.FieldA != "abcd"u8) {
        Ꮡt.Fatal(expectingAbcdˢ);
    }
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static readonly @string tagAttrAzAZ09ˢ = "<tag attr=azAZ09:-_\t>"u8;

public static void TestUnquotedAttrs(ж<testing.T> Ꮡt) {
    @string data = tagAttrAzAZ09ˢ;
    var d = NewDecoder(new xml_test_package.strings_ReaderжReader(strings.NewReader(data)));
    d.Value.Strict = false;
    var (token, err) = d.Token();
    {
        var (_, ok) = err._<ж<global::go.encoding.xml_package.SyntaxError>>(ᐧ); if (ok) {
            Ꮡt.Errorf("Unexpected error: %v"u8, err);
        }
    }
    if (token._<StartElement>().Name.Local != "tag"u8) {
        Ꮡt.Errorf("Unexpected tag name: %v"u8, token._<StartElement>().Name.Local);
    }
    var attr = token._<StartElement>().Attr[0];
    if (attr.Value != "azAZ09:-_"u8) {
        Ꮡt.Errorf("Unexpected attribute value: %v"u8, attr.Value);
    }
    if (attr.Name.Local != "attr"u8) {
        Ꮡt.Errorf("Unexpected attribute name: %v"u8, attr.Name.Local);
    }
}

public static void TestValuelessAttrs(ж<testing.T> Ꮡt) {
    var tests = GoReflect.WithElemDims(new array<@string>[]{
        new @string[]{"<p nowrap>"u8, "p"u8, "nowrap"u8}.array(),
        new @string[]{"<p nowrap >"u8, "p"u8, "nowrap"u8}.array(),
        new @string[]{"<input checked/>"u8, "input"u8, "checked"u8}.array(),
        new @string[]{"<input checked />"u8, "input"u8, "checked"u8}.array()
    }.slice(), 3);
    foreach (var (_, vᴛ1) in tests) {
        var test = vᴛ1.Clone();

        var d = NewDecoder(new xml_test_package.strings_ReaderжReader(strings.NewReader(test[0])));
        d.Value.Strict = false;
        var (token, err) = d.Token();
        {
            var (_, ok) = err._<ж<global::go.encoding.xml_package.SyntaxError>>(ᐧ); if (ok) {
                Ꮡt.Errorf("Unexpected error: %v"u8, err);
            }
        }
        if (token._<StartElement>().Name.Local != test[1]) {
            Ꮡt.Errorf("Unexpected tag name: %v"u8, token._<StartElement>().Name.Local);
        }
        var attr = token._<StartElement>().Attr[0];
        if (attr.Value != test[2]) {
            Ꮡt.Errorf("Unexpected attribute value: %v"u8, attr.Value);
        }
        if (attr.Name.Local != test[2]) {
            Ꮡt.Errorf("Unexpected attribute name: %v"u8, attr.Name.Local);
        }
    }
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static readonly object copyTokenCharDataˢ = (@string)"CopyToken(CharData) != CharData"u8;
internal static readonly object copyTokenCharDataUsesˢ = (@string)"CopyToken(CharData) uses same buffer."u8;

public static void TestCopyTokenCharData(ж<testing.T> Ꮡt) {
    var data = slice<byte>("same data"u8);
    ΔToken tok1 = ((global::go.encoding.xml_package.CharData)data);
    var tok2 = CopyToken(tok1);
    if (!reflect.DeepEqual(tok1, tok2)) {
        Ꮡt.Error(copyTokenCharDataˢ);
    }
    data[1] = (rune)'o';
    if (reflect.DeepEqual(tok1, tok2)) {
        Ꮡt.Error(copyTokenCharDataUsesˢ);
    }
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static readonly object copyTokenOverwroteAttr0ˢ = (@string)"CopyToken overwrote Attr[0]"u8;
internal static readonly object copyTokenStartElementˢ = (@string)"CopyToken(StartElement) != StartElement"u8;

public static void TestCopyTokenStartElement(ж<testing.T> Ꮡt) {
    var elt = new StartElement(new Name(""u8, "hello"u8), new global::go.encoding.xml_package.Attr[]{new(new Name(""u8, "lang"u8), "en"u8)}.slice());
    ΔToken tok1 = elt;
    var tok2 = CopyToken(tok1);
    if (tok1._<StartElement>().Attr[0].Value != "en"u8) {
        Ꮡt.Error(copyTokenOverwroteAttr0ˢ);
    }
    if (!reflect.DeepEqual(tok1, tok2)) {
        Ꮡt.Error(copyTokenStartElementˢ);
    }
    tok1._<StartElement>().Attr[0] = new Attr(new Name(""u8, "lang"u8), "de"u8);
    if (reflect.DeepEqual(tok1, tok2)) {
        Ꮡt.Error(copyTokenCharDataUsesˢ);
    }
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static readonly object copyTokenCommentCommentˢ = (@string)"CopyToken(Comment) != Comment"u8;
internal static readonly object copyTokenCommentUsesSameˢ = (@string)"CopyToken(Comment) uses same buffer."u8;

public static void TestCopyTokenComment(ж<testing.T> Ꮡt) {
    var data = slice<byte>("<!-- some comment -->"u8);
    ΔToken tok1 = ((global::go.encoding.xml_package.Comment)data);
    var tok2 = CopyToken(tok1);
    if (!reflect.DeepEqual(tok1, tok2)) {
        Ꮡt.Error(copyTokenCommentCommentˢ);
    }
    data[1] = (rune)'o';
    if (reflect.DeepEqual(tok1, tok2)) {
        Ꮡt.Error(copyTokenCommentUsesSameˢ);
    }
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static readonly @string pFooPPBarˢ = "<P>Foo<P>\n\n<P>Bar</>\n"u8;
internal static readonly object expectedSyntaxErrorˢ = (@string)"Expected SyntaxError."u8;
internal static readonly object syntaxErrorDidnTHaveˢ = (@string)"SyntaxError didn't have correct line number."u8;

public static void TestSyntaxErrorLineNum(ж<testing.T> Ꮡt) {
    @string testInput = pFooPPBarˢ;
    var d = NewDecoder(new xml_test_package.strings_ReaderжReader(strings.NewReader(testInput)));
    error err = default!;
    for ((_, err) = d.Token(); err == default!; (_, err) = d.Token()) {
    }
    var (synerr, ok) = err._<ж<global::go.encoding.xml_package.SyntaxError>>(ᐧ);
    if (!ok) {
        Ꮡt.Error(expectedSyntaxErrorˢ);
    }
    if ((~synerr).Line != 3) {
        Ꮡt.Error(syntaxErrorDidnTHaveˢ);
    }
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static readonly @string fooFooˢ = @"<FOO></FOO>  "u8;

public static void TestTrailingRawToken(ж<testing.T> Ꮡt) {
    @string input = fooFooˢ;
    var d = NewDecoder(new xml_test_package.strings_ReaderжReader(strings.NewReader(input)));
    error err = default!;
    for ((_, err) = d.RawToken(); err == default!; (_, err) = d.RawToken()) {
    }
    if (!AreEqual(err, io.EOF)) {
        Ꮡt.Fatalf("d.RawToken() = _, %v, want _, io.EOF"u8, err);
    }
}

public static void TestTrailingToken(ж<testing.T> Ꮡt) {
    @string input = fooFooˢ;
    var d = NewDecoder(new xml_test_package.strings_ReaderжReader(strings.NewReader(input)));
    error err = default!;
    for ((_, err) = d.Token(); err == default!; (_, err) = d.Token()) {
    }
    if (!AreEqual(err, io.EOF)) {
        Ꮡt.Fatalf("d.Token() = _, %v, want _, io.EOF"u8, err);
    }
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static readonly @string testCdataValFooTestˢ = @"<test><![CDATA[ &val=foo ]]></test>"u8;

public static void TestEntityInsideCDATA(ж<testing.T> Ꮡt) {
    @string input = testCdataValFooTestˢ;
    var d = NewDecoder(new xml_test_package.strings_ReaderжReader(strings.NewReader(input)));
    error err = default!;
    for ((_, err) = d.Token(); err == default!; (_, err) = d.Token()) {
    }
    if (!AreEqual(err, io.EOF)) {
        Ꮡt.Fatalf("d.Token() = _, %v, want _, io.EOF"u8, err);
    }
}


partial struct characterTestsᴛ1 /*dyn*/ {
    internal @string @in;
    internal @string err;
}
internal static slice<characterTestsᴛ1> characterTests = new characterTestsᴛ1[]{
    new("\x12<doc/>"u8, "illegal character code U+0012"u8),
    new("<?xml version=\"1.0\"?>\x0b<doc/>"u8, "illegal character code U+000B"u8),
    new(((@string)(new byte[]{0xef, 0xbf, 0xbe, 0x3c, 0x64, 0x6f, 0x63, 0x2f, 0x3e})), "illegal character code U+FFFE"u8),
    new("<?xml version=\"1.0\"?><doc>\r\n<hiya/>\x07<toots/></doc>"u8, "illegal character code U+0007"u8),
    new("<?xml version=\"1.0\"?><doc \x12='value'>what's up</doc>"u8, "expected attribute name in element"u8),
    new("<doc>&abc\x01;</doc>"u8, "invalid character entity &abc (no semicolon)"u8),
    new("<doc>&\x01;</doc>"u8, "invalid character entity & (no semicolon)"u8),
    new(((@string)(new byte[]{0x3c, 0x64, 0x6f, 0x63, 0x3e, 0x26, 0xef, 0xbf, 0xbe, 0x3b, 0x3c, 0x2f, 0x64, 0x6f, 0x63, 0x3e})), "invalid character entity &\uFFFE;"u8),
    new("<doc>&hello;</doc>"u8, "invalid character entity &hello;"u8)
}.slice();

public static void TestDisallowedCharacters(ж<testing.T> Ꮡt) {
    foreach (var (i, tt) in characterTests) {
        var d = NewDecoder(new xml_test_package.strings_ReaderжReader(strings.NewReader(tt.@in)));
        error err = default!;
        while (err == default!) {
            (_, err) = d.Token();
        }
        var (synerr, ok) = err._<ж<global::go.encoding.xml_package.SyntaxError>>(ᐧ);
        if (!ok) {
            Ꮡt.Fatalf("input %d d.Token() = _, %v, want _, *SyntaxError"u8, i, err);
        }
        if ((~synerr).Msg != tt.err) {
            Ꮡt.Fatalf("input %d synerr.Msg wrong: want %q, got %q"u8, i, tt.err, (~synerr).Msg);
        }
    }
}

public static void TestIsInCharacterRange(ж<testing.T> Ꮡt) {
    var invalid = new rune[]{
        utf8.MaxRune + 1,
        0xD800, // surrogate min

        0xDFFF, // surrogate max

        -1
    }.slice();
    foreach (var (_, r) in invalid) {
        if (isInCharacterRange(r)) {
            Ꮡt.Errorf("rune %U considered valid"u8, r);
        }
    }
}

// TODO: what's the right approach to handle these nested cases?

partial struct procInstTestsᴛ1 /*dyn*/ {
    internal @string input;
    internal array<@string> expect = new(2);
}
internal static slice<procInstTestsᴛ1> procInstTests = new procInstTestsᴛ1[]{
    new(@"version=""1.0"" encoding=""utf-8"""u8, new @string[]{"1.0"u8, "utf-8"u8}.array()),
    new(@"version=""1.0"" encoding='utf-8'"u8, new @string[]{"1.0"u8, "utf-8"u8}.array()),
    new(@"version=""1.0"" encoding='utf-8' "u8, new @string[]{"1.0"u8, "utf-8"u8}.array()),
    new(@"version=""1.0"" encoding=utf-8"u8, new @string[]{"1.0"u8, ""u8}.array()),
    new(@"encoding=""FOO"" "u8, new @string[]{""u8, "FOO"u8}.array()),
    new(@"version=2.0 version=""1.0"" encoding=utf-7 encoding='utf-8'"u8, new @string[]{"1.0"u8, "utf-8"u8}.array()),
    new(@"version= encoding="u8, new @string[]{""u8, ""u8}.array()),
    new(@"encoding=""version=1.0"""u8, new @string[]{""u8, "version=1.0"u8}.array()),
    new(@""u8, new @string[]{""u8, ""u8}.array()),
    new(@"encoding=""version='1.0'"""u8, new @string[]{"1.0"u8, "version='1.0'"u8}.array()),
    new(@"version=""encoding='utf-8'"""u8, new @string[]{"encoding='utf-8'"u8, "utf-8"u8}.array())
}.slice();

public static void TestProcInstEncoding(ж<testing.T> Ꮡt) {
    foreach (var (_, vᴛ1) in procInstTests) {
        var test = vᴛ1.ΔClone();

        {
            @string got = procInst(versionˢ, test.input); if (got != test.expect[0]) {
                Ꮡt.Errorf("procInst(version, %q) = %q; want %q"u8, test.input, got, test.expect[0]);
            }
        }
        {
            @string got = procInst(encodingˢ, test.input); if (got != test.expect[1]) {
                Ꮡt.Errorf("procInst(encoding, %q) = %q; want %q"u8, test.input, got, test.expect[1]);
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

"""u8;

internal static slice<ΔToken> directivesWithCommentsTokens = new ΔToken[]{
    ((global::go.encoding.xml_package.CharData)slice<byte>((@string)"\n"u8)),
    ((global::go.encoding.xml_package.Directive)slice<byte>((@string)@"DOCTYPE [ <!ENTITY rdf ""http://www.w3.org/1999/02/22-rdf-syntax-ns#"">]"u8)),
    ((global::go.encoding.xml_package.CharData)slice<byte>((@string)"\n"u8)),
    ((global::go.encoding.xml_package.Directive)slice<byte>((@string)@"DOCTYPE [<!ENTITY go ""Golang""> ]"u8)),
    ((global::go.encoding.xml_package.CharData)slice<byte>((@string)"\n"u8)),
    ((global::go.encoding.xml_package.Directive)slice<byte>((@string)@"DOCTYPE <!-> <!>       [<!ENTITY go ""Golang""> ]"u8)),
    ((global::go.encoding.xml_package.CharData)slice<byte>((@string)"\n"u8))
}.slice();

public static void TestDirectivesWithComments(ж<testing.T> Ꮡt) {
    var d = NewDecoder(new xml_test_package.strings_ReaderжReader(strings.NewReader(directivesWithCommentsInput)));
    foreach (var (i, want) in directivesWithCommentsTokens) {
        var (have, err) = d.Token();
        if (err != default!) {
            Ꮡt.Fatalf("token %d: unexpected error: %s"u8, i, err);
        }
        if (!reflect.DeepEqual(have, want)) {
            Ꮡt.Errorf("token %d = %#v want %#v"u8, i, have, want);
        }
    }
}

// Writer whose Write method always returns an error.
internal partial struct errWriter {
}

internal static (nint n, error err) Write(this errWriter _, slice<byte> p) {
    return (0, fmt.Errorf("unwritable"u8));
}

public static void TestEscapeTextIOErrors(ж<testing.T> Ꮡt) {
    @string expectErr = unwritableˢ;
    var err = EscapeText(new errWriter(nil), new byte[]{(rune)'A'}.slice());
    if (err == default! || err.Error() != expectErr) {
        Ꮡt.Errorf("have %v, want %v"u8, err, expectErr);
    }
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static readonly @string aTerminatedStringˢ = "A \uFFFD terminated string."u8;

public static void TestEscapeTextInvalidChar(ж<testing.T> Ꮡt) {
    var input = slice<byte>("A \x00 terminated string."u8);
    @string expected = aTerminatedStringˢ;
    var buff = @new<strings.Builder>();
    {
        var err = EscapeText(new xml_test_package.strings_BuilderжWriter(buff), input); if (err != default!) {
            Ꮡt.Fatalf("have %v, want nil"u8, err);
        }
    }
    @string text = buff.String();
    if (text != expected) {
        Ꮡt.Errorf("have %v, want %v"u8, text, expected);
    }
}

internal partial struct TestIssue5880_T /*[]byte*/;

public static void TestIssue5880(ж<testing.T> Ꮡt) {
    var (data, err) = Marshal(new TestIssue5880_T(new byte[]{192, 168, 0, 1}.slice()));
    if (err != default!) {
        Ꮡt.Errorf("Marshal error: %v"u8, err);
    }
    if (!utf8.Valid(data)) {
        Ꮡt.Errorf("Marshal generated invalid UTF-8: %x"u8, data);
    }
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static readonly @string exampleTitleExampleTitleˢ = """
<example>
			<title>Example</title>
			<link>http://example.com/default</link> <!-- not assigned -->
			<link>http://example.com/home</link> <!-- not assigned -->
			<ns:link xmlns:ns="http://www.w3.org/2005/Atom">http://example.com/ns</ns:link>
		</example>
"""u8;

internal partial struct TestIssue8535_ExampleConflict /*dyn*/ {
    public global::go.encoding.xml_package.Name XMLName; /*`xml:"example"`*/
    public @string Link; /*`xml:"link"`*/
    public @string AtomLink; /*`xml:"http://www.w3.org/2005/Atom link"`*/ // Same name in a different name space
}

public static void TestIssue8535(ж<testing.T> Ꮡt) {
    @string testCase = exampleTitleExampleTitleˢ;
    ref var dest = ref heap(new TestIssue8535_ExampleConflict(), out var Ꮡdest);
    var d = NewDecoder(new xml_test_package.strings_ReaderжReader(strings.NewReader(testCase)));
    {
        var err = d.Decode(Ꮡdest); if (err != default!) {
            Ꮡt.Fatal(err);
        }
    }
}

internal partial struct TestEncodeXMLNS_testCases /*dyn*/ {
    internal Func<(slice<byte>, error)> f;
    internal @string want;
    internal bool ok;
}

public static void TestEncodeXMLNS(ж<testing.T> Ꮡt) {
    var testCases = new TestEncodeXMLNS_testCases[]{
        new(encodeXMLNS1, @"<Test xmlns=""http://example.com/ns""><Body>hello world</Body></Test>"u8, true),
        new(encodeXMLNS2, @"<Test><body xmlns=""http://example.com/ns"">hello world</body></Test>"u8, true),
        new(encodeXMLNS3, @"<Test xmlns=""http://example.com/ns""><Body>hello world</Body></Test>"u8, true),
        new(encodeXMLNS4, @"<Test xmlns=""http://example.com/ns""><Body>hello world</Body></Test>"u8, false)
    }.slice();
    foreach (var (i, tc) in testCases) {
        {
            var (b, err) = tc.f(); if (err == default!){
                {
                    @string got = ((@string)b);
                    @string want = tc.want; if (got != want) {
                        Ꮡt.Errorf("%d: got %s, want %s \n"u8, i, got, want);
                    }
                }
            } else {
                Ꮡt.Errorf("%d: marshal failed with %s"u8, i, err);
            }
        }
    }
}

internal partial struct encodeXMLNS1_T /*dyn*/ {
    public global::go.encoding.xml_package.Name XMLName; /*`xml:"Test"`*/
    public @string Ns; /*`xml:"xmlns,attr"`*/
    public @string Body;
}

internal static (slice<byte>, error) encodeXMLNS1() {
    var s = Ꮡ(new encodeXMLNS1_T(Ns: "http://example.com/ns"u8, Body: "hello world"u8));
    return Marshal(s.OrTypedNil());
}

internal partial struct encodeXMLNS2_Test /*dyn*/ {
    public @string Body; /*`xml:"http://example.com/ns body"`*/
}

internal static (slice<byte>, error) encodeXMLNS2() {
    var s = Ꮡ(new encodeXMLNS2_Test(Body: "hello world"u8));
    return Marshal(s.OrTypedNil());
}

internal partial struct encodeXMLNS3_Test /*dyn*/ {
    public global::go.encoding.xml_package.Name XMLName; /*`xml:"http://example.com/ns Test"`*/
    public @string Body;
}

internal static (slice<byte>, error) encodeXMLNS3() {
    //s := &Test{XMLName: Name{"http://example.com/ns",""}, Body: "hello world"} is unusable as the "-" is missing
    // as documentation states
    var s = Ꮡ(new encodeXMLNS3_Test(Body: "hello world"u8));
    return Marshal(s.OrTypedNil());
}

internal partial struct encodeXMLNS4_Test /*dyn*/ {
    public @string Ns; /*`xml:"xmlns,attr"`*/
    public @string Body;
}

internal static (slice<byte>, error) encodeXMLNS4() {
    var s = Ꮡ(new encodeXMLNS4_Test(Ns: "http://example.com/ns"u8, Body: "hello world"u8));
    return Marshal(s.OrTypedNil());
}

public static void TestIssue11405(ж<testing.T> Ꮡt) {
    var testCases = new @string[]{
        "<root>"u8,
        "<root><foo>"u8,
        "<root><foo></foo>"u8
    }.slice();
    foreach (var (_, tc) in testCases) {
        var d = NewDecoder(new xml_test_package.strings_ReaderжReader(strings.NewReader(tc)));
        error err = default!;
        while (ᐧ) {
            (_, err) = d.Token();
            if (err != default!) {
                break;
            }
        }
        {
            var (_, ok) = err._<ж<global::go.encoding.xml_package.SyntaxError>>(ᐧ); if (!ok) {
                Ꮡt.Errorf("%s: Token: Got error %v, want SyntaxError"u8, tc, err);
            }
        }
    }
}

internal partial struct TestIssue12417_testCases /*dyn*/ {
    internal @string s;
    internal bool ok;
}

public static void TestIssue12417(ж<testing.T> Ꮡt) {
    var testCases = new TestIssue12417_testCases[]{
        new(@"<?xml encoding=""UtF-8"" version=""1.0""?><root/>"u8, true),
        new(@"<?xml encoding=""UTF-8"" version=""1.0""?><root/>"u8, true),
        new(@"<?xml encoding=""utf-8"" version=""1.0""?><root/>"u8, true),
        new(@"<?xml encoding=""uuu-9"" version=""1.0""?><root/>"u8, false)
    }.slice();
    foreach (var (_, tc) in testCases) {
        var d = NewDecoder(new xml_test_package.strings_ReaderжReader(strings.NewReader(tc.s)));
        error err = default!;
        while (ᐧ) {
            (_, err) = d.Token();
            if (err != default!) {
                if (AreEqual(err, io.EOF)) {
                    err = default!;
                }
                break;
            }
        }
        if (err != default! && tc.ok) {
            Ꮡt.Errorf("%q: Encoding charset: expected no error, got %s"u8, tc.s, err);
            continue;
        }
        if (err == default! && !tc.ok) {
            Ꮡt.Errorf("%q: Encoding charset: expected error, got nil"u8, tc.s);
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

public static void TestIssue7113(ж<testing.T> Ꮡt) {
    ref var a = ref heap(new TestIssue7113_A(), out var Ꮡa);
    @string structSpace = "b"u8;
    @string xmlTest = @"<A xmlns="""u8 + structSpace + @"""><C xmlns=""""></C><d></d></A>"u8;
    Ꮡt.Log(xmlTest);
    var err = Unmarshal(slice<byte>(xmlTest), Ꮡa);
    if (err != default!) {
        Ꮡt.Fatal(err);
    }
    if (a.XMLName.Space != structSpace) {
        Ꮡt.Errorf("overidding with empty namespace: unmarshaling, got %s, want %s\n"u8, a.XMLName.Space, structSpace);
    }
    if (len(a.C.XMLName.Space) != 0) {
        Ꮡt.Fatalf("overidding with empty namespace: unmarshaling, got %s, want empty\n"u8, a.C.XMLName.Space);
    }
    slice<byte> b = default!;
    (b, err) = Marshal(Ꮡa);
    if (err != default!) {
        Ꮡt.Fatal(err);
    }
    if (len(a.C.XMLName.Space) != 0) {
        Ꮡt.Errorf("overidding with empty namespace: marshaling, got %s in C tag which should be empty\n"u8, a.C.XMLName.Space);
    }
    if (((sstring)b) != xmlTest) {
        Ꮡt.Fatalf("overidding with empty namespace: marshaling, got %s, want %s\n"u8, b, xmlTest);
    }
    ref var c = ref heap(new TestIssue7113_A(), out var Ꮡc);
    err = Unmarshal(b, Ꮡc);
    if (err != default!) {
        Ꮡt.Fatalf("second Unmarshal failed: %s"u8, err);
    }
    if (c.XMLName.Space != "b"u8) {
        Ꮡt.Errorf("overidding with empty namespace: after marshaling & unmarshaling, XML name space: got %s, want %s\n"u8, a.XMLName.Space, structSpace);
    }
    if (len(c.C.XMLName.Space) != 0) {
        Ꮡt.Errorf("overidding with empty namespace: after marshaling & unmarshaling, got %s, want empty\n"u8, a.C.XMLName.Space);
    }
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static readonly @string xmlSyntaxErrorOnLine1ˢ = "XML syntax error on line 1: expected attribute name in element"u8;
internal static readonly @string xmlSyntaxErrorOnLine1ˢ2 = "XML syntax error on line 1: expected element name after <"u8;

internal partial struct TestIssue20396_testCases /*dyn*/ {
    internal @string s;
    internal error wantErr;
}

public static void TestIssue20396(ж<testing.T> Ꮡt) {
    global::go.encoding.xml_package.UnmarshalError attrError = ((global::go.encoding.xml_package.UnmarshalError)(@string)xmlSyntaxErrorOnLine1ˢ);
    var testCases = new TestIssue20396_testCases[]{
        new(@"<a:te:st xmlns:a=""abcd""/>"u8, // Issue 20396

            ((global::go.encoding.xml_package.UnmarshalError)(@string)xmlSyntaxErrorOnLine1ˢ2)),
        new(@"<a:te=st xmlns:a=""abcd""/>"u8, attrError),
        new(@"<a:te&st xmlns:a=""abcd""/>"u8, attrError),
        new(@"<a:test xmlns:a=""abcd""/>"u8, default!),
        new(@"<a:te:st xmlns:a=""abcd"">1</a:te:st>"u8,
            ((global::go.encoding.xml_package.UnmarshalError)(@string)xmlSyntaxErrorOnLine1ˢ2)),
        new(@"<a:te=st xmlns:a=""abcd"">1</a:te=st>"u8, attrError),
        new(@"<a:te&st xmlns:a=""abcd"">1</a:te&st>"u8, attrError),
        new(@"<a:test xmlns:a=""abcd"">1</a:test>"u8, default!)
    }.slice();
    ref var dest = ref heap(new @string(), out var Ꮡdest);
    foreach (var (_, tc) in testCases) {
        {
            var (got, want) = (Unmarshal(slice<byte>(tc.s), Ꮡdest), tc.wantErr); if (!AreEqual(got, want)) {
                if (got == default!){
                    Ꮡt.Errorf("%s: Unexpected success, want %v"u8, tc.s, want);
                } else 
                if (want == default!){
                    Ꮡt.Errorf("%s: Unexpected error, got %v"u8, tc.s, got);
                } else 
                if (got.Error() != want.Error()) {
                    Ꮡt.Errorf("%s: got %v, want %v"u8, tc.s, got, want);
                }
            }
        }
    }
}

internal partial struct TestIssue20685_testCases /*dyn*/ {
    internal @string s;
    internal bool ok;
}

public static void TestIssue20685(ж<testing.T> Ꮡt) {
    var testCases = new TestIssue20685_testCases[]{
        new(@"<x:book xmlns:x=""abcd"" xmlns:y=""abcd""><unclosetag>one</x:book>"u8, false),
        new(@"<x:book xmlns:x=""abcd"" xmlns:y=""abcd"">one</x:book>"u8, true),
        new(@"<x:book xmlns:x=""abcd"" xmlns:y=""abcd"">one</y:book>"u8, false),
        new(@"<x:book xmlns:y=""abcd"" xmlns:x=""abcd"">one</y:book>"u8, false),
        new(@"<x:book xmlns:x=""abcd"">one</y:book>"u8, false),
        new(@"<x:book>one</y:book>"u8, false),
        new(@"<xbook>one</ybook>"u8, false)
    }.slice();
    foreach (var (_, tc) in testCases) {
        var d = NewDecoder(new xml_test_package.strings_ReaderжReader(strings.NewReader(tc.s)));
        error err = default!;
        while (ᐧ) {
            (_, err) = d.Token();
            if (err != default!) {
                if (AreEqual(err, io.EOF)) {
                    err = default!;
                }
                break;
            }
        }
        if (err != default! && tc.ok) {
            Ꮡt.Errorf("%q: Closing tag with namespace : expected no error, got %s"u8, tc.s, err);
            continue;
        }
        if (err == default! && !tc.ok) {
            Ꮡt.Errorf("%q: Closing tag with namespace : expected error, got nil"u8, tc.s);
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
    if (err != default!) {
        return (default!, err);
    }
    return (m.f(tok), default!);
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static readonly object newTokenDecoderDidNotˢ = (@string)"NewTokenDecoder did not detect underlying Decoder"u8;

public static void TestNewTokenDecoderIdempotent(ж<testing.T> Ꮡt) {
    var d = NewDecoder(new xml_test_package.strings_ReaderжReader(strings.NewReader(@"<br>"u8)));
    var d2 = NewTokenDecoder(new global::go.encoding.xml_package.DecoderжTokenReader(d));
    if (d != d2) {
        Ꮡt.Error(newTokenDecoderDidNotˢ);
    }
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static readonly @string quoteReEnterClownWithAˢ = @"<quote>[Re-enter Clown with a letter, and FABIAN]</quote>"u8;
internal static readonly @string blockingˢ = "blocking"u8;
internal static readonly object gotUnexpectedErrorWhileˢ = (@string)"Got unexpected error while decoding:"u8;

internal partial struct TestWrapDecoder_o /*dyn*/ {
    public global::go.encoding.xml_package.Name XMLName; /*`xml:"blocking"`*/
    public @string Chardata; /*`xml:",chardata"`*/
}

public static void TestWrapDecoder(ж<testing.T> Ꮡt) {
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
    ref var o = ref heap<TestWrapDecoder_o>(out var Ꮡo);
    o = new TestWrapDecoder_o();
    {
        var err = d.Decode(Ꮡo); if (err != default!) {
            Ꮡt.Fatal(gotUnexpectedErrorWhileˢ, err);
        }
    }
    if (o.Chardata != "[Re-enter Clown with a letter, and FABIAN]"u8) {
        Ꮡt.Fatalf("Got unexpected chardata: `%s`\n"u8, o.Chardata);
    }
}

internal partial struct tokReader {
}

internal static (ΔToken, error) Token(this tokReader _) {
    return (new StartElement(nil), default!);
}

public partial struct Failure {
}

public static error UnmarshalXML(this Failure _Δp0, ж<global::go.encoding.xml_package.Decoder> _Δp1, global::go.encoding.xml_package.StartElement _Δp2) {
    return default!;
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static readonly object unexpectedPanicUsingˢ = (@string)"Unexpected panic using custom token unmarshaler"u8;

public static void TestTokenUnmarshaler(ж<testing.T> Ꮡt) {
    GoFrame ᒐ = default;
    try {
        defer(() => {
            {
                var r = recover(); if (r != default!) {
                    Ꮡt.Error(unexpectedPanicUsingˢ);
                }
            }
        }, ref ᒐ);
        var d = NewTokenDecoder(new tokReader(nil));
        d.Decode(Ꮡ(new Failure(nil)));
    }
    catch (Exception ᒐex) when (GoFrame.IsPanic(ᒐex, out PanicException? ᒐp)) { GoFrame.Capture(ᒐp); }
    finally { ᒐ.Run(); }
}

internal static void testRoundTrip(ж<testing.T> Ꮡt, @string input) {
    var d = NewDecoder(new xml_test_package.strings_ReaderжReader(strings.NewReader(input)));
    slice<ΔToken> tokens = default!;
    ref var buf = ref heap(new bytes.Buffer(), out var Ꮡbuf);
    var e = NewEncoder(new xml_test_package.bytes_BufferжWriter(Ꮡbuf));
    while (ᐧ) {
        var (tok, err) = d.Token();
        if (AreEqual(err, io.EOF)) {
            break;
        }
        if (err != default!) {
            Ꮡt.Fatalf("invalid input: %v"u8, err);
        }
        {
            var errΔ1 = e.EncodeToken(tok); if (errΔ1 != default!) {
                Ꮡt.Fatalf("failed to re-encode input: %v"u8, errΔ1);
            }
        }
        tokens = append(tokens, CopyToken(tok));
    }
    {
        var err = e.Flush(); if (err != default!) {
            Ꮡt.Fatal(err);
        }
    }
    d = NewDecoder(new xml_test_package.bytes_BufferжReader(Ꮡbuf));
    while (ᐧ) {
        var (tok, err) = d.Token();
        if (AreEqual(err, io.EOF)) {
            break;
        }
        if (err != default!) {
            Ꮡt.Fatalf("failed to decode output: %v"u8, err);
        }
        if (len(tokens) == 0) {
            Ꮡt.Fatalf("unexpected token: %#v"u8, tok);
        }
        var (a, b) = (tokens[0], tok);
        if (!reflect.DeepEqual(a, b)) {
            Ꮡt.Fatalf("token mismatch: %#v vs %#v"u8, a, b);
        }
        tokens = tokens[1..];
    }
    if (len(tokens) > 0) {
        Ꮡt.Fatalf("lost tokens: %#v"u8, tokens);
    }
}

public static void TestRoundTrip(ж<testing.T> Ꮡt) {
    var tests = new map<@string, @string>{
        ["trailing colon"u8] = @"<foo abc:=""x""></foo>"u8,
        ["comments in directives"u8] = @"<!ENTITY x<!<!-- c1 [ "" -->--x --> > <e></e> <!DOCTYPE xxx [ x<!-- c2 "" -->--x ]>"u8
    };
    foreach (var (name, input) in tests) {
        Ꮡt.Run(name, (ж<testing.T> tΔ1) => {
            testRoundTrip(tΔ1, input);
        });
    }
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static readonly @string fooˢ = @"</foo>"u8;
internal static readonly @string xFooYFooˢ = @"<x:foo></y:foo>"u8;
internal static readonly @string notOkˢ = @"<? not ok ?>"u8;
internal static readonly @string notOkˢ2 = @"<!- not ok -->"u8;
internal static readonly @string notOkˢ3 = @"<!-? not ok -->"u8;
internal static readonly @string notOkˢ4 = @"<![not ok]>"u8;
internal static readonly @string zzzFooXmlnsZzzHttpˢ = @"<zzz:foo xmlns:zzz=""http://example.com""><bar>baz</bar></foo>"u8;
internal static readonly @string okVersionOkˢ = @"<?ok version=""ok""?>"u8;

internal partial struct TestParseErrors_tests /*dyn*/ {
    internal @string src;
    internal @string err;
}

public static void TestParseErrors(ж<testing.T> Ꮡt) {
    ref var t = ref Ꮡt.DerefOrNull();

    @string withDefaultHeader(@string s) => @"<?xml version=""1.0"" encoding=""UTF-8""?>"u8 + s;
    var tests = new TestParseErrors_tests[]{
        new(withDefaultHeader(fooˢ), @"unexpected end element </foo>"u8),
        new(withDefaultHeader(xFooYFooˢ), @"element <foo> in space x closed by </foo> in space y"u8),
        new(withDefaultHeader(notOkˢ), @"expected target name after <?"u8),
        new(withDefaultHeader(notOkˢ2), @"invalid sequence <!- not part of <!--"u8),
        new(withDefaultHeader(notOkˢ3), @"invalid sequence <!- not part of <!--"u8),
        new(withDefaultHeader(notOkˢ4), @"invalid <![ sequence"u8),
        new(withDefaultHeader(zzzFooXmlnsZzzHttpˢ),
            @"element <foo> in space zzz closed by </foo> in space """""u8),
        new(withDefaultHeader(((@string)(new byte[]{0xf1}))), @"invalid UTF-8"u8), // Header-related errors.

        new(@"<?xml version=""1.1"" encoding=""UTF-8""?>"u8, @"unsupported version ""1.1""; only version 1.0 is supported"u8), // Cases below are for "no errors".

        new(withDefaultHeader(@"<?ok?>"u8), @""u8),
        new(withDefaultHeader(okVersionOkˢ), @""u8)
    }.slice();
    foreach (var (_, test) in tests) {
        var d = NewDecoder(new xml_test_package.strings_ReaderжReader(strings.NewReader(test.src)));
        error err = default!;
        while (ᐧ) {
            (_, err) = d.Token();
            if (err != default!) {
                break;
            }
        }
        if (test.err == ""u8) {
            if (!AreEqual(err, io.EOF)) {
                Ꮡt.Errorf("parse %s: have %q error, expected none"u8, test.src, err);
            }
            continue;
        }
        // Inv: err != nil
        if (AreEqual(err, io.EOF)) {
            Ꮡt.Errorf("parse %s: unexpected EOF"u8, test.src);
            continue;
        }
        if (!strings.Contains(err.Error(), test.err)) {
            Ꮡt.Errorf("parse %s: can't find %q error substring\nerror: %q"u8, test.src, test.err, err);
            continue;
        }
    }
}

internal static readonly @string testInputHTMLAutoClose = """
<?xml version="1.0" encoding="UTF-8"?>
<br>
<br/><br/>
<br><br>
<br></br>
<BR>
<BR/><BR/>
<Br></Br>
<BR><span id="test">abc</span><br/><br/>
"""u8;

public static void BenchmarkHTMLAutoClose(ж<testing.B> Ꮡb) {
    Ꮡb.RunParallel((ж<testing.PB> p) => {
        while (p.Next()) {
            var d = NewDecoder(new xml_test_package.strings_ReaderжReader(strings.NewReader(testInputHTMLAutoClose)));
            d.Value.Strict = false;
            d.Value.AutoClose = HTMLAutoClose;
            d.Value.Entity = HTMLEntity;
            while (ᐧ) {
                var (_, err) = d.Token();
                if (err != default!) {
                    if (AreEqual(err, io.EOF)) {
                        break;
                    }
                    Ꮡb.Fatalf("unexpected error: %v"u8, err);
                }
            }
        }
    });
}

public static void TestHTMLAutoClose(ж<testing.T> Ꮡt) {
    var wantTokens = new ΔToken[]{
        new ProcInst("xml"u8, slice<byte>(@"version=""1.0"" encoding=""UTF-8"""u8)),
        ((global::go.encoding.xml_package.CharData)slice<byte>((@string)"\n"u8)),
        new StartElement(new Name(""u8, "br"u8), new global::go.encoding.xml_package.Attr[]{}.slice()),
        new EndElement(new Name(""u8, "br"u8)),
        ((global::go.encoding.xml_package.CharData)slice<byte>((@string)"\n"u8)),
        new StartElement(new Name(""u8, "br"u8), new global::go.encoding.xml_package.Attr[]{}.slice()),
        new EndElement(new Name(""u8, "br"u8)),
        new StartElement(new Name(""u8, "br"u8), new global::go.encoding.xml_package.Attr[]{}.slice()),
        new EndElement(new Name(""u8, "br"u8)),
        ((global::go.encoding.xml_package.CharData)slice<byte>((@string)"\n"u8)),
        new StartElement(new Name(""u8, "br"u8), new global::go.encoding.xml_package.Attr[]{}.slice()),
        new EndElement(new Name(""u8, "br"u8)),
        new StartElement(new Name(""u8, "br"u8), new global::go.encoding.xml_package.Attr[]{}.slice()),
        new EndElement(new Name(""u8, "br"u8)),
        ((global::go.encoding.xml_package.CharData)slice<byte>((@string)"\n"u8)),
        new StartElement(new Name(""u8, "br"u8), new global::go.encoding.xml_package.Attr[]{}.slice()),
        new EndElement(new Name(""u8, "br"u8)),
        ((global::go.encoding.xml_package.CharData)slice<byte>((@string)"\n"u8)),
        new StartElement(new Name(""u8, "BR"u8), new global::go.encoding.xml_package.Attr[]{}.slice()),
        new EndElement(new Name(""u8, "BR"u8)),
        ((global::go.encoding.xml_package.CharData)slice<byte>((@string)"\n"u8)),
        new StartElement(new Name(""u8, "BR"u8), new global::go.encoding.xml_package.Attr[]{}.slice()),
        new EndElement(new Name(""u8, "BR"u8)),
        new StartElement(new Name(""u8, "BR"u8), new global::go.encoding.xml_package.Attr[]{}.slice()),
        new EndElement(new Name(""u8, "BR"u8)),
        ((global::go.encoding.xml_package.CharData)slice<byte>((@string)"\n"u8)),
        new StartElement(new Name(""u8, "Br"u8), new global::go.encoding.xml_package.Attr[]{}.slice()),
        new EndElement(new Name(""u8, "Br"u8)),
        ((global::go.encoding.xml_package.CharData)slice<byte>((@string)"\n"u8)),
        new StartElement(new Name(""u8, "BR"u8), new global::go.encoding.xml_package.Attr[]{}.slice()),
        new EndElement(new Name(""u8, "BR"u8)),
        new StartElement(new Name(""u8, "span"u8), new global::go.encoding.xml_package.Attr[]{new(Name: new Name(""u8, "id"u8), Value: "test"u8)}.slice()),
        ((global::go.encoding.xml_package.CharData)slice<byte>((@string)"abc"u8)),
        new EndElement(new Name(""u8, "span"u8)),
        new StartElement(new Name(""u8, "br"u8), new global::go.encoding.xml_package.Attr[]{}.slice()),
        new EndElement(new Name(""u8, "br"u8)),
        new StartElement(new Name(""u8, "br"u8), new global::go.encoding.xml_package.Attr[]{}.slice()),
        new EndElement(new Name(""u8, "br"u8))
    }.slice();
    var d = NewDecoder(new xml_test_package.strings_ReaderжReader(strings.NewReader(testInputHTMLAutoClose)));
    d.Value.Strict = false;
    d.Value.AutoClose = HTMLAutoClose;
    d.Value.Entity = HTMLEntity;
    slice<ΔToken> haveTokens = default!;
    while (ᐧ) {
        var (tok, err) = d.Token();
        if (err != default!) {
            if (AreEqual(err, io.EOF)) {
                break;
            }
            Ꮡt.Fatalf("unexpected error: %v"u8, err);
        }
        haveTokens = append(haveTokens, CopyToken(tok));
    }
    if (len(haveTokens) != len(wantTokens)) {
        Ꮡt.Errorf("tokens count mismatch: have %d, want %d"u8, len(haveTokens), len(wantTokens));
    }
    foreach (var (i, want) in wantTokens) {
        if (i >= len(haveTokens)){
            Ꮡt.Errorf("token[%d] expected %#v, have no token"u8, i, want);
        } else {
            var have = haveTokens[i];
            if (!reflect.DeepEqual(have, want)) {
                Ꮡt.Errorf("token[%d] mismatch:\nhave: %#v\nwant: %#v"u8, i, have, want);
            }
        }
    }
}

} // end xml_internal_test_package
