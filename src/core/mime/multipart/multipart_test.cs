// Copyright 2010 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.
namespace go.mime;

using bytes = bytes_package;
using json = encoding.json_package;
using fmt = fmt_package;
using io = io_package;
using textproto = net.textproto_package;
using os = os_package;
using reflect = reflect_package;
using strings = strings_package;
using testing = testing_package;
using encoding;
using net;
using static go.mime.multipart_package;

partial class multipart_internal_test_package {

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static ref readonly @string myBoundaryˢ => ref myBoundaryˢᵇ;
private static readonly @string myBoundaryˢᵇ = "myBoundary"u8;
internal static readonly object expectedˢ = (@string)"expected"u8;
internal static readonly object expectedFailˢ = (@string)"expected fail"u8;

public static void TestBoundaryLine(ж<testing.T> t) {
    var mr = NewReader(new multipart_test_package.strings_ReaderжReader(strings.NewReader(""u8)), myBoundaryˢ);
    if (!mr.isBoundaryDelimiterLine(slice<byte>("--myBoundary\r\n"u8))) {
        t.Error(expectedˢ);
    }
    if (!mr.isBoundaryDelimiterLine(slice<byte>("--myBoundary \r\n"u8))) {
        t.Error(expectedˢ);
    }
    if (!mr.isBoundaryDelimiterLine(slice<byte>("--myBoundary \n"u8))) {
        t.Error(expectedˢ);
    }
    if (mr.isBoundaryDelimiterLine(slice<byte>("--myBoundary bogus \n"u8))) {
        t.Error(expectedFailˢ);
    }
    if (mr.isBoundaryDelimiterLine(slice<byte>("--myBoundary bogus--"u8))) {
        t.Error(expectedFailˢ);
    }
}

internal static @string escapeString(@string v) {
    var (bytes, _) = json.Marshal(v);
    return (@string)bytes;
}

internal static partial void expectEq(ж<testing.T> t, @string expected, @string actual, @string what) {
    if (expected == actual) {
        return;
    }
    t.Errorf("Unexpected value for %s; got %s (len %d) but expected: %s (len %d)",
        what, escapeString(actual), len(actual), escapeString(expected), len(expected));
}

public static void TestNameAccessors(ж<testing.T> t) {
    var tests = new array<@string>[]{
        new @string[]{@"form-data; name=""foo""", "foo", ""}.array(),
        new @string[]{@" form-data ; name=foo", "foo", ""}.array(),
        new @string[]{@"FORM-DATA;name=""foo""", "foo", ""}.array(),
        new @string[]{@" FORM-DATA ; name=""foo""", "foo", ""}.array(),
        new @string[]{@" FORM-DATA ; name=""foo""", "foo", ""}.array(),
        new @string[]{@" FORM-DATA ; name=foo", "foo", ""}.array(),
        new @string[]{@" FORM-DATA ; filename=""foo.txt""; name=foo; baz=quux", "foo", "foo.txt"}.array(),
        new @string[]{@" not-form-data ; filename=""bar.txt""; name=foo; baz=quux", "", "bar.txt"}.array()
    }.array();
    foreach (var (i, vᴛ1) in range(tests.ΔRangeSnapshot())) {
        var test = vᴛ1.Clone();

        var p = Ꮡ(new Part(Header: new map<@string, slice<@string>>()));
        p.Header.Set(contentDispositionˢ, test[0]);
        {
            @string g = p.FormName();
            @string e = test[1]; if (g != e) {
                t.Errorf("test %d: FormName() = %q; want %q", i, g, e);
            }
        }
        {
            @string g = p.FileName();
            @string e = test[2]; if (g != e) {
                t.Errorf("test %d: FileName() = %q; want %q", i, g, e);
            }
        }
    }
}

internal static @string longLine = strings.Repeat("\n\n\r\r\r\n\r\u0000", (1 << 20) / 8);

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static ref readonly @string thisIsAMultiPartMessageˢ => ref thisIsAMultiPartMessageˢᵇ;
private static readonly @string thisIsAMultiPartMessageˢᵇ = """

This is a multi-part message.  This line is ignored.
--MyBoundary
Header1: value1
HEADER2: value2
foo-bar: baz

My value
The end.
--MyBoundary
name: bigsection

[longline]
--MyBoundary
Header1: value1b
HEADER2: value2b
foo-bar: bazb

Line 1
Line 2
Line 3 ends in a newline, but just one.

--MyBoundary

never read data
--MyBoundary--


useless trailer

"""u8.Raw();
internal static ref readonly @string longlineˢ => ref longlineˢᵇ;
private static readonly @string longlineˢᵇ = "[longline]"u8;

internal static @string testMultipartBody(@string sep) {
    @string testBody = thisIsAMultiPartMessageˢ;
    testBody = strings.ReplaceAll(testBody, "\n", sep);
    return strings.Replace(testBody, longlineˢ, longLine, 1);
}

public static void TestMultipart(ж<testing.T> t) {
    var bodyReader = strings.NewReader(testMultipartBody("\r\n"));
    testMultipart(t, new multipart_test_package.strings_ReaderжReader(bodyReader), false);
}

public static void TestMultipartOnlyNewlines(ж<testing.T> t) {
    var bodyReader = strings.NewReader(testMultipartBody("\n"));
    testMultipart(t, new multipart_test_package.strings_ReaderжReader(bodyReader), true);
}

public static void TestMultipartSlowInput(ж<testing.T> t) {
    var bodyReader = strings.NewReader(testMultipartBody("\r\n"));
    testMultipart(t, new multipart_internal_test_package.slowReaderжReader(Ꮡ(new slowReader(new multipart_test_package.strings_ReaderжReader(bodyReader)))), false);
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static ref readonly @string myBoundaryˢ2 => ref myBoundaryˢ2ᵇ;
private static readonly @string myBoundaryˢ2ᵇ = "MyBoundary"u8;
internal static readonly object expectedPart1ˢ = (@string)"Expected part1"u8;
internal static ref readonly @string header1ˢ => ref header1ˢᵇ;
private static readonly @string header1ˢᵇ = "Header1"u8;
internal static readonly object value1ˢ = (@string)"value1"u8;
internal static ref readonly @string fooBarˢ => ref fooBarˢᵇ;
private static readonly @string fooBarˢᵇ = "foo-bar"u8;
internal static readonly object bazˢ = (@string)"baz"u8;
internal static ref readonly @string fooBarˢ2 => ref fooBarˢ2ᵇ;
private static readonly @string fooBarˢ2ᵇ = "Foo-Bar"u8;
internal static ref readonly @string myValueTheEndˢ => ref myValueTheEndˢᵇ;
private static readonly @string myValueTheEndˢᵇ = "My value\r\nThe end."u8;
internal static ref readonly @string valueOfFirstPartˢ => ref valueOfFirstPartˢᵇ;
private static readonly @string valueOfFirstPartˢᵇ = "Value of first part"u8;
internal static ref readonly @string bigsectionˢ => ref bigsectionˢᵇ;
private static readonly @string bigsectionˢᵇ = "bigsection"u8;
internal static readonly object expectedPart3ˢ = (@string)"Expected part3"u8;
internal static readonly object expectedFooBarBazbˢ = (@string)"Expected foo-bar: bazb"u8;
internal static ref readonly @string line1Line2Line3EndsInAˢ => ref line1Line2Line3EndsInAˢᵇ;
private static readonly @string line1Line2Line3EndsInAˢᵇ = "Line 1\r\nLine 2\r\nLine 3 ends in a newline, but just one.\r\n"u8;
internal static ref readonly @string bodyOfPart3ˢ => ref bodyOfPart3ˢᵇ;
private static readonly @string bodyOfPart3ˢᵇ = "body of part 3"u8;
internal static readonly object expectedPart4Withoutˢ = (@string)"Expected part 4 without errors"u8;
internal static readonly object didnTExpectAFifthPartˢ = (@string)"Didn't expect a fifth part."u8;

internal static void testMultipart(ж<testing.T> t, io.Reader r, bool onlyNewlines) {
    t.Parallel();
    var reader = NewReader(r, myBoundaryˢ2);
    var buf = @new<strings.Builder>();

    // Part1
    var (part, err) = reader.NextPart();
    if (part == nil || err != nil) {
        t.Error(expectedPart1ˢ);
        return;
    }
    {
        @string x = part.Header.Get(header1ˢ); if (x != "value1"u8) {
            t.Errorf("part.Header.Get(%q) = %q, want %q", header1ˢ, x, value1ˢ);
        }
    }
    {
        @string x = part.Header.Get(fooBarˢ); if (x != "baz"u8) {
            t.Errorf("part.Header.Get(%q) = %q, want %q", fooBarˢ, x, bazˢ);
        }
    }
    {
        @string x = part.Header.Get(fooBarˢ2); if (x != "baz"u8) {
            t.Errorf("part.Header.Get(%q) = %q, want %q", fooBarˢ2, x, bazˢ);
        }
    }
    buf.Reset();
    var (_, errΔ1) = io.Copy(new multipart_test_package.strings_BuilderжWriter(buf), new global::go.mime.multipart_package.PartжReader(part)); if (errΔ1 != nil) {
        t.Errorf("part 1 copy: %v", errΔ1);
    }

    @string adjustNewlines(@string sΔ1) {
        if (onlyNewlines) {
            return strings.ReplaceAll(sΔ1, "\r\n", "\n");
        }
        return sΔ1;
    }

    expectEq(t, adjustNewlines(myValueTheEndˢ), buf.String(), valueOfFirstPartˢ);

    // Part2
    (part, err) = reader.NextPart();
    if (err != nil) {
        t.Fatalf("Expected part2; got: %v", err);
        return;
    }
    @string e = bigsectionˢ;
    @string g = part.Header.Get(nameˢ); if (e != g) {
        t.Errorf("part2's name header: expected %q, got %q", e, g);
    }
    buf.Reset();
    var (_, errΔ2) = io.Copy(new multipart_test_package.strings_BuilderжWriter(buf), new global::go.mime.multipart_package.PartжReader(part)); if (errΔ2 != nil) {
        t.Errorf("part 2 copy: %v", errΔ2);
    }
    @string s = buf.String();
    if (len(s) != len(longLine)) {
        t.Errorf("part2 body expected long line of length %d; got length %d",
            len(longLine), len(s));
    }
    if (s != longLine) {
        t.Errorf("part2 long body didn't match");
    }

    // Part3
    (part, err) = reader.NextPart();
    if (part == nil || err != nil) {
        t.Error(expectedPart3ˢ);
        return;
    }
    if (part.Header.Get(fooBarˢ) != "bazb"u8) {
        t.Error(expectedFooBarBazbˢ);
    }
    buf.Reset();
    var (_, errΔ3) = io.Copy(new multipart_test_package.strings_BuilderжWriter(buf), new global::go.mime.multipart_package.PartжReader(part)); if (errΔ3 != nil) {
        t.Errorf("part 3 copy: %v", errΔ3);
    }
    expectEq(t, adjustNewlines(line1Line2Line3EndsInAˢ),
        buf.String(), bodyOfPart3ˢ);

    // Part4
    (part, err) = reader.NextPart();
    if (part == nil || err != nil) {
        t.Error(expectedPart4Withoutˢ);
        return;
    }

    // Non-existent part5
    (part, err) = reader.NextPart();
    if (part != nil) {
        t.Error(didnTExpectAFifthPartˢ);
    }
    if (!AreEqual(err, io.EOF)) {
        t.Errorf("On fifth part expected io.EOF; got %v", err);
    }
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static ref readonly @string boundaryˢ2 => ref boundaryˢ2ᵇ;
private static readonly @string boundaryˢ2ᵇ = "BOUNDARY"u8;

public static void TestVariousTextLineEndings(ж<testing.T> t) {
    var tests = new @string[]{
        "Foo\nBar",
        "Foo\nBar\n",
        "Foo\r\nBar",
        "Foo\r\nBar\r\n",
        "Foo\rBar",
        "Foo\rBar\r",
        "\x00\x01\x02\x09\x0a\x0b\x0c\x0d\x0e\x0f\x10"
    }.array();

    foreach (var (testNum, expectedBody) in range(tests.ΔRangeSnapshot())) {
        @string body = "--BOUNDARY\r\n"u8 + "Content-Disposition: form-data; name=\"value\"\r\n"u8 + "\r\n"u8 + expectedBody + "\r\n--BOUNDARY--\r\n"u8;
        var bodyReader = strings.NewReader(body);

        var reader = NewReader(new multipart_test_package.strings_ReaderжReader(bodyReader), boundaryˢ2);
        var buf = @new<bytes.Buffer>();
        var (part, err) = reader.NextPart();
        if (part == nil) {
            t.Errorf("Expected a body part on text %d", testNum);
            continue;
        }
        if (err != nil) {
            t.Errorf("Unexpected error on text %d: %v", testNum, err);
            continue;
        }
        (var written, err) = io.Copy(new multipart_test_package.bytes_BufferжWriter(buf), new global::go.mime.multipart_package.PartжReader(part));
        expectEq(t, expectedBody, buf.String(), fmt.Sprintf("test %d"u8, testNum));
        if (err != nil) {
            t.Errorf("Error copying multipart; bytes=%v, error=%v", written, err);
        }

        (part, err) = reader.NextPart();
        if (part != nil) {
            t.Errorf("Unexpected part in test %d", testNum);
        }
        if (!AreEqual(err, io.EOF)) {
            t.Errorf("On test %d expected io.EOF; got %v", testNum, err);
        }
    }
}

internal partial struct maliciousReader {
    internal ж<testing.T> t;
    internal nint n;
}

internal static UntypedInt maxReadThreshold => /* 1 << 20 */ 1048576;

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static readonly object tooMuchWasReadˢ = (@string)"too much was read"u8;

internal static (nint n, error err) Read(this ref maliciousReader mr, slice<byte> b) {
    mr.n += len(b);
    if (mr.n >= maxReadThreshold) {
        mr.t.Fatal(tooMuchWasReadˢ);
        return (0, io.EOF);
    }
    return (len(b), default);
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static ref readonly @string fooBoundaryˢ => ref fooBoundaryˢᵇ;
private static readonly @string fooBoundaryˢᵇ = "fooBoundary"u8;

public static void TestLineLimit(ж<testing.T> t) {
    var mr = Ꮡ(new maliciousReader(t: t));
    var r = NewReader(new multipart_internal_test_package.maliciousReaderжReader(mr), fooBoundaryˢ);
    var (part, err) = r.NextPart();
    if (part != nil) {
        t.Errorf("unexpected part read");
    }
    if (err == nil) {
        t.Errorf("expected an error");
    }
    if (mr.n >= maxReadThreshold) {
        t.Errorf("expected to read < %d bytes; read %d", (nint)(maxReadThreshold), mr.n);
    }
}

public static void TestMultipartTruncated(ж<testing.T> t) {
    foreach (var (_, vᴛ1) in range(slice<@string>([
        """

This is a multi-part message.  This line is ignored.
--MyBoundary
foo-bar: baz

Oh no, premature EOF!

"""u8.Raw(),
        """

This is a multi-part message.  This line is ignored.
--MyBoundary
foo-bar: baz

Oh no, premature EOF!
--MyBoundary-
"""u8.Raw()
    ]))) {
        var body = vᴛ1;

        body = strings.ReplaceAll(body, "\n", "\r\n");
        var bodyReader = strings.NewReader(body);
        var r = NewReader(new multipart_test_package.strings_ReaderжReader(bodyReader), myBoundaryˢ2);

        var (part, err) = r.NextPart();
        if (err != nil) {
            t.Fatalf("didn't get a part");
        }
        (_, err) = io.Copy(io.Discard, new global::go.mime.multipart_package.PartжReader(part));
        if (!AreEqual(err, io.ErrUnexpectedEOF)) {
            t.Fatalf("expected error io.ErrUnexpectedEOF; got %v", err);
        }
    }
}

internal partial struct slowReader {
    internal io.Reader r;
}

internal static (nint, error) Read(this ref slowReader s, slice<byte> p) {
    if (len(p) == 0) {
        return s.r.Read(p);
    }
    return s.r.Read(p[..1]);
}

internal partial struct sentinelReader {
    // done is closed when this reader is read from.
    internal channel<EmptyStruct> done;
}

internal static (nint, error) Read(this ref sentinelReader s, slice<byte> _) {
    if (s.done != nil) {
        builtin.close(s.done);
        s.done = default;
    }
    return (0, io.EOF);
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static ref readonly @string thisIsAMultiPartMessageˢ2 => ref thisIsAMultiPartMessageˢ2ᵇ;
private static readonly @string thisIsAMultiPartMessageˢ2ᵇ = """

This is a multi-part message.  This line is ignored.
--MyBoundary
foo-bar: baz

Body
--MyBoundary

"""u8.Raw();
internal static ref readonly @string fooBarBopBody2MyBoundaryˢ => ref fooBarBopBody2MyBoundaryˢᵇ;
private static readonly @string fooBarBopBody2MyBoundaryˢᵇ = """
foo-bar: bop

Body 2
--MyBoundary--

"""u8.Raw();
internal static ref readonly @string bodyˢ => ref bodyˢᵇ;
private static readonly @string bodyˢᵇ = "Body"u8;
internal static ref readonly @string body2ˢ => ref body2ˢᵇ;
private static readonly @string body2ˢᵇ = "Body 2"u8;

// TestMultipartStreamReadahead tests that PartReader does not block
// on reading past the end of a part, ensuring that it can be used on
// a stream like multipart/x-mixed-replace. See golang.org/issue/15431
public static void TestMultipartStreamReadahead(ж<testing.T> t) {
    @string testBody1 = thisIsAMultiPartMessageˢ2;
    @string testBody2 = fooBarBopBody2MyBoundaryˢ;
    var done1 = new channel<EmptyStruct>(0);
    var reader = NewReader(
        io.MultiReader(
            new multipart_test_package.strings_ReaderжReader(strings.NewReader(testBody1)),
            new multipart_internal_test_package.sentinelReaderжReader(Ꮡ(new sentinelReader(done1))),
            new multipart_test_package.strings_ReaderжReader(strings.NewReader(testBody2))),
        myBoundaryˢ2);

    nint i = default;
    var readerʗ1 = reader;
    void readPart(textproto.MIMEHeader hdr, @string body) {
        var (part, err) = readerʗ1.NextPart();
        if (part == nil || err != nil) {
            t.Fatalf("Part %d: NextPart failed: %v", i, err);
        }

        if (!reflect.DeepEqual(part.Header, hdr)) {
            t.Errorf("Part %d: part.Header = %v, want %v", i, part.Header, hdr);
        }
        (var data, err) = io.ReadAll(new global::go.mime.multipart_package.PartжReader(part));
        expectEq(t, body, (@string)data, fmt.Sprintf("Part %d body"u8, i));
        if (err != nil) {
            t.Fatalf("Part %d: ReadAll failed: %v", i, err);
        }
        i++;
    }

    readPart(new textproto.MIMEHeader(new map<@string, slice<@string>>{["Foo-Bar"] = ["baz"]}), bodyˢ);

    var selᴛ1 = done1;
    switch (trySelect(ᐸꟷ(selᴛ1, ꓸꓸꓸ))) {
    case 0 when selᴛ1.ꟷᐳ(out _): {
        t.Errorf("Reader read past second boundary");
        break;
    }
    default: {
        break;
    }}

    readPart(new textproto.MIMEHeader(new map<@string, slice<@string>>{["Foo-Bar"] = ["bop"]}), body2ˢ);
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static ref readonly @string appleMail2292336769ˢ => ref appleMail2292336769ˢᵇ;
private static readonly @string appleMail2292336769ˢᵇ = "\n--Apple-Mail-2-292336769\nContent-Transfer-Encoding: 7bit\nContent-Type: text/plain;\n\tcharset=US-ASCII;\n\tdelsp=yes;\n\tformat=flowed\n\nI'm finding the same thing happening on my system (10.4.1).\n\n\n--Apple-Mail-2-292336769\nContent-Transfer-Encoding: quoted-printable\nContent-Type: text/html;\n\tcharset=ISO-8859-1\n\n<HTML><BODY>I'm finding the same thing =\nhappening on my system (10.4.1).=A0 But I built it with XCode =\n2.0.</BODY></=\nHTML>=\n\r\n--Apple-Mail-2-292336769--\n"u8;
internal static ref readonly @string appleMail2292336769ˢ2 => ref appleMail2292336769ˢ2ᵇ;
private static readonly @string appleMail2292336769ˢ2ᵇ = "Apple-Mail-2-292336769"u8;

public static void TestLineContinuation(ж<testing.T> t) {
    // This body, extracted from an email, contains headers that span multiple
    // lines.

    // TODO: The original mail ended with a double-newline before the
    // final delimiter; this was manually edited to use a CRLF.
    @string testBody = appleMail2292336769ˢ;

    var r = NewReader(new multipart_test_package.strings_ReaderжReader(strings.NewReader(testBody)), appleMail2292336769ˢ2);

    for (nint i = 0; i < 2; i++) {
        var (part, err) = r.NextPart();
        if (err != nil) {
            t.Fatalf("didn't get a part");
        }
        ref var buf = ref heap(new strings.Builder(), out var Ꮡbuf);
        (var n, err) = io.Copy(new multipart_test_package.strings_BuilderжWriter(Ꮡbuf), new global::go.mime.multipart_package.PartжReader(part));
        if (err != nil) {
            t.Errorf("error reading part: %v\nread so far: %q", err, buf.String());
        }
        if (n <= 0) {
            t.Errorf("read %d bytes; expected >0", n);
        }
    }
}

public static void TestQuotedPrintableEncoding(ж<testing.T> t) {
    foreach (var (_, cte) in range(slice<@string>(["quoted-printable", "Quoted-PRINTABLE"]))) {
        t.Run(cte, (ж<testing.T> tΔ1) => {
            testQuotedPrintableEncoding(tΔ1, cte);
        });
    }
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static ref readonly @string contentTransferEncodingˢ => ref contentTransferEncodingˢᵇ;
private static readonly @string contentTransferEncodingˢᵇ = "Content-Transfer-Encoding"u8;
internal static ref readonly @string wordsWordsWordsWordsˢ => ref wordsWordsWordsWordsˢᵇ;
private static readonly @string wordsWordsWordsWordsˢᵇ = "words words words words words words words words words words words words words words words words words words words words words words words words words words words words words words words words words words words words words words words words words words words words words words words words words words words words words words words words words words words"u8;

internal static void testQuotedPrintableEncoding(ж<testing.T> t, @string cte) {
    // From https://golang.org/issue/4411
    @string body = "--0016e68ee29c5d515f04cedf6733\r\nContent-Type: text/plain; charset=ISO-8859-1\r\nContent-Disposition: form-data; name=text\r\nContent-Transfer-Encoding: "u8 + cte + "\r\n\r\nwords words words words words words words words words words words words wor=\r\nds words words words words words words words words words words words words =\r\nwords words words words words words words words words words words words wor=\r\nds words words words words words words words words words words words words =\r\nwords words words words words words words words words\r\n--0016e68ee29c5d515f04cedf6733\r\nContent-Type: text/plain; charset=ISO-8859-1\r\nContent-Disposition: form-data; name=submit\r\n\r\nSubmit\r\n--0016e68ee29c5d515f04cedf6733--"u8;
    var r = NewReader(new multipart_test_package.strings_ReaderжReader(strings.NewReader(body)), "0016e68ee29c5d515f04cedf6733");
    var (part, err) = r.NextPart();
    if (err != nil) {
        t.Fatal(err);
    }
    var (te, ok) = part.Header[contentTransferEncodingˢ, ꟷ]; if (ok) {
        t.Errorf("unexpected Content-Transfer-Encoding of %q", te);
    }
    ref var buf = ref heap(new strings.Builder(), out var Ꮡbuf);
    (_, err) = io.Copy(new multipart_test_package.strings_BuilderжWriter(Ꮡbuf), new global::go.mime.multipart_package.PartжReader(part));
    if (err != nil) {
        t.Error(err);
    }
    @string got = buf.String();
    @string want = wordsWordsWordsWordsˢ;
    if (got != want) {
        t.Errorf("wrong part value:\n got: %q\nwant: %q", got, want);
    }
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static ref readonly @string contentTypeTextPlainˢ => ref contentTypeTextPlainˢᵇ;
private static readonly @string contentTypeTextPlainˢᵇ = """
--0016e68ee29c5d515f04cedf6733
Content-Type: text/plain; charset="utf-8"
Content-Transfer-Encoding: quoted-printable

<div dir=3D"ltr">Hello World.</div>
--0016e68ee29c5d515f04cedf6733
Content-Type: text/plain; charset="utf-8"
Content-Transfer-Encoding: quoted-printable

<div dir=3D"ltr">Hello World.</div>
--0016e68ee29c5d515f04cedf6733--
"""u8.Raw();
internal static ref readonly @string divDir3dLtrHelloWorldDivˢ => ref divDir3dLtrHelloWorldDivˢᵇ;
private static readonly @string divDir3dLtrHelloWorldDivˢᵇ = @"<div dir=3D""ltr"">Hello World.</div>"u8;
internal static ref readonly @string divDirLtrHelloWorldDivˢ => ref divDirLtrHelloWorldDivˢᵇ;
private static readonly @string divDirLtrHelloWorldDivˢᵇ = @"<div dir=""ltr"">Hello World.</div>"u8;

public static void TestRawPart(ж<testing.T> t) {
    // https://github.com/golang/go/issues/29090

    @string body = strings.Replace(contentTypeTextPlainˢ, "\n", "\r\n", -1);

    var r = NewReader(new multipart_test_package.strings_ReaderжReader(strings.NewReader(body)), "0016e68ee29c5d515f04cedf6733");

    // This part is expected to be raw, bypassing the automatic handling
    // of quoted-printable.
    var (part, err) = r.NextRawPart();
    if (err != nil) {
        t.Fatal(err);
    }
    if (part.Header[contentTransferEncodingˢ, ꟷ] is not (_, true)) {
        t.Errorf("missing Content-Transfer-Encoding");
    }
    ref var buf = ref heap(new strings.Builder(), out var Ꮡbuf);
    (_, err) = io.Copy(new multipart_test_package.strings_BuilderжWriter(Ꮡbuf), new global::go.mime.multipart_package.PartжReader(part));
    if (err != nil) {
        t.Error(err);
    }
    @string got = buf.String();
    // Data is still quoted-printable.
    @string want = divDir3dLtrHelloWorldDivˢ;
    if (got != want) {
        t.Errorf("wrong part value:\n got: %q\nwant: %q", got, want);
    }

    // This part is expected to have automatic decoding of quoted-printable.
    (part, err) = r.NextPart();
    if (err != nil) {
        t.Fatal(err);
    }
    if (part.Header[contentTransferEncodingˢ, ꟷ] is (var te, true)) {
        t.Errorf("unexpected Content-Transfer-Encoding of %q", te);
    }

    buf.Reset();
    (_, err) = io.Copy(new multipart_test_package.strings_BuilderжWriter(Ꮡbuf), new global::go.mime.multipart_package.PartжReader(part));
    if (err != nil) {
        t.Error(err);
    }
    got = buf.String();
    // QP data has been decoded.
    want = divDirLtrHelloWorldDivˢ;
    if (got != want) {
        t.Errorf("wrong part value:\n got: %q\nwant: %q", got, want);
    }
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static ref readonly @string testdataNestedMimeˢ => ref testdataNestedMimeˢᵇ;
private static readonly @string testdataNestedMimeˢᵇ = "testdata/nested-mime"u8;

// Test parsing an image attachment from gmail, which previously failed.
public static void TestNested(ж<testing.T> t) {
    GoFrame ᒐ = default;
    try {
        // nested-mime is the body part of a multipart/mixed email
        // with boundary e89a8ff1c1e83553e304be640612
        var (f, err) = os.Open(testdataNestedMimeˢ);
        if (err != nil) {
            t.Fatal(err);
        }
        var fʗ1 = f;
        defer(() => fʗ1.Close(), ref ᒐ);
        var mr = NewReader(new multipart_test_package.os_FileжReader(f), "e89a8ff1c1e83553e304be640612");
        (var p, err) = mr.NextPart();
        if (err != nil) {
            t.Fatalf("error reading first section (alternative): %v", err);
        }

        // Read the inner text/plain and text/html sections of the multipart/alternative.
        var mr2 = NewReader(new global::go.mime.multipart_package.PartжReader(p), "e89a8ff1c1e83553e004be640610");
        (p, err) = mr2.NextPart();
        if (err != nil) {
            t.Fatalf("reading text/plain part: %v", err);
        }
        {
            var (b, errΔ1) = io.ReadAll(new global::go.mime.multipart_package.PartжReader(p)); if (((sstring)b) != "*body*\r\n"u8 || errΔ1 != nil) {
                t.Fatalf("reading text/plain part: got %q, %v", b, errΔ1);
            }
        }
        (p, err) = mr2.NextPart();
        if (err != nil) {
            t.Fatalf("reading text/html part: %v", err);
        }
        {
            var (b, errΔ2) = io.ReadAll(new global::go.mime.multipart_package.PartжReader(p)); if (((sstring)b) != "<b>body</b>\r\n"u8 || errΔ2 != nil) {
                t.Fatalf("reading text/html part: got %q, %v", b, errΔ2);
            }
        }

        (p, err) = mr2.NextPart();
        if (!AreEqual(err, io.EOF)) {
            t.Fatalf("final inner NextPart = %v; want io.EOF", err);
        }

        // Back to the outer multipart/mixed, reading the image attachment.
        (_, err) = mr.NextPart();
        if (err != nil) {
            t.Fatalf("error reading the image attachment at the end: %v", err);
        }

        (_, err) = mr.NextPart();
        if (!AreEqual(err, io.EOF)) {
            t.Fatalf("final outer NextPart = %v; want io.EOF", err);
        }
    }
    catch (Exception ᒐex) when (GoFrame.IsPanic(ᒐex, out PanicException? ᒐp)) { GoFrame.Capture(ᒐp); }
    finally { ᒐ.Run(); }
}

internal partial struct headerBody {
    internal textproto.MIMEHeader header;
    internal @string body;
}

internal static headerBody formData(@string key, @string value) {
    return new headerBody(
        new textproto.MIMEHeader(new map<@string, slice<@string>>{
            ["Content-Type"] = ["text/plain; charset=ISO-8859-1"],
            ["Content-Disposition"] = ["form-data; name="u8 + key]
        }),
        value
    );
}

internal partial struct parseTest {
    internal @string name;
    internal @string @in, sep;
    internal slice<headerBody> want;
}

internal static slice<parseTest> parseTests = slice<parseTest>([
    // Actual body from App Engine on a blob upload. The final part (the
    // Content-Type: message/external-body) is what App Engine replaces
    // the uploaded file with. The other form fields (prefixed with
    // "other" in their form-data name) are unchanged. A bug was
    // reported with blob uploads failing when the other fields were
    // empty. This was the MIME POST body that previously failed.
    new(
        name: "App Engine post",
        sep: "00151757727e9583fd04bfbca4c6",
        @in: "--00151757727e9583fd04bfbca4c6\r\nContent-Type: text/plain; charset=ISO-8859-1\r\nContent-Disposition: form-data; name=otherEmpty1\r\n\r\n--00151757727e9583fd04bfbca4c6\r\nContent-Type: text/plain; charset=ISO-8859-1\r\nContent-Disposition: form-data; name=otherFoo1\r\n\r\nfoo\r\n--00151757727e9583fd04bfbca4c6\r\nContent-Type: text/plain; charset=ISO-8859-1\r\nContent-Disposition: form-data; name=otherFoo2\r\n\r\nfoo\r\n--00151757727e9583fd04bfbca4c6\r\nContent-Type: text/plain; charset=ISO-8859-1\r\nContent-Disposition: form-data; name=otherEmpty2\r\n\r\n--00151757727e9583fd04bfbca4c6\r\nContent-Type: text/plain; charset=ISO-8859-1\r\nContent-Disposition: form-data; name=otherRepeatFoo\r\n\r\nfoo\r\n--00151757727e9583fd04bfbca4c6\r\nContent-Type: text/plain; charset=ISO-8859-1\r\nContent-Disposition: form-data; name=otherRepeatFoo\r\n\r\nfoo\r\n--00151757727e9583fd04bfbca4c6\r\nContent-Type: text/plain; charset=ISO-8859-1\r\nContent-Disposition: form-data; name=otherRepeatEmpty\r\n\r\n--00151757727e9583fd04bfbca4c6\r\nContent-Type: text/plain; charset=ISO-8859-1\r\nContent-Disposition: form-data; name=otherRepeatEmpty\r\n\r\n--00151757727e9583fd04bfbca4c6\r\nContent-Type: text/plain; charset=ISO-8859-1\r\nContent-Disposition: form-data; name=submit\r\n\r\nSubmit\r\n--00151757727e9583fd04bfbca4c6\r\nContent-Type: message/external-body; charset=ISO-8859-1; blob-key=AHAZQqG84qllx7HUqO_oou5EvdYQNS3Mbbkb0RjjBoM_Kc1UqEN2ygDxWiyCPulIhpHRPx-VbpB6RX4MrsqhWAi_ZxJ48O9P2cTIACbvATHvg7IgbvZytyGMpL7xO1tlIvgwcM47JNfv_tGhy1XwyEUO8oldjPqg5Q\r\nContent-Disposition: form-data; name=file; filename=\"fall.png\"\r\n\r\nContent-Type: image/png\r\nContent-Length: 232303\r\nX-AppEngine-Upload-Creation: 2012-05-10 23:14:02.715173\r\nContent-MD5: MzRjODU1ZDZhZGU1NmRlOWEwZmMwMDdlODBmZTA0NzA=\r\nContent-Disposition: form-data; name=file; filename=\"fall.png\"\r\n\r\n\r\n--00151757727e9583fd04bfbca4c6--",
        want: slice<headerBody>([
            formData("otherEmpty1", ""),
            formData("otherFoo1", "foo"),
            formData("otherFoo2", "foo"),
            formData("otherEmpty2", ""),
            formData("otherRepeatFoo", "foo"),
            formData("otherRepeatFoo", "foo"),
            formData("otherRepeatEmpty", ""),
            formData("otherRepeatEmpty", ""),
            formData("submit", "Submit"),
            new(new textproto.MIMEHeader(new map<@string, slice<@string>>{
                ["Content-Type"] = ["message/external-body; charset=ISO-8859-1; blob-key=AHAZQqG84qllx7HUqO_oou5EvdYQNS3Mbbkb0RjjBoM_Kc1UqEN2ygDxWiyCPulIhpHRPx-VbpB6RX4MrsqhWAi_ZxJ48O9P2cTIACbvATHvg7IgbvZytyGMpL7xO1tlIvgwcM47JNfv_tGhy1XwyEUO8oldjPqg5Q"],
                ["Content-Disposition"] = ["form-data; name=file; filename=\"fall.png\""]
            }), "Content-Type: image/png\r\nContent-Length: 232303\r\nX-AppEngine-Upload-Creation: 2012-05-10 23:14:02.715173\r\nContent-MD5: MzRjODU1ZDZhZGU1NmRlOWEwZmMwMDdlODBmZTA0NzA=\r\nContent-Disposition: form-data; name=file; filename=\"fall.png\"\r\n\r\n")
        ])
    ),

    // Single empty part, ended with --boundary immediately after headers.
    new(
        name: "single empty part, --boundary",
        sep: "abc",
        @in: "--abc\r\nFoo: bar\r\n\r\n--abc--",
        want: [
            new(new textproto.MIMEHeader(new map<@string, slice<@string>>{["Foo"] = ["bar"]}), "")
        ]
    ),

    // Single empty part, ended with \r\n--boundary immediately after headers.
    new(
        name: "single empty part, \r\n--boundary",
        sep: "abc",
        @in: "--abc\r\nFoo: bar\r\n\r\n\r\n--abc--",
        want: [
            new(new textproto.MIMEHeader(new map<@string, slice<@string>>{["Foo"] = ["bar"]}), "")
        ]
    ),

    // Final part empty.
    new(
        name: "final part empty",
        sep: "abc",
        @in: "--abc\r\nFoo: bar\r\n\r\n--abc\r\nFoo2: bar2\r\n\r\n--abc--",
        want: [
            new(new textproto.MIMEHeader(new map<@string, slice<@string>>{["Foo"] = ["bar"]}), ""),
            new(new textproto.MIMEHeader(new map<@string, slice<@string>>{["Foo2"] = ["bar2"]}), "")
        ]
    ),

    // Final part empty with newlines after final separator.
    new(
        name: "final part empty then crlf",
        sep: "abc",
        @in: "--abc\r\nFoo: bar\r\n\r\n--abc--\r\n",
        want: [
            new(new textproto.MIMEHeader(new map<@string, slice<@string>>{["Foo"] = ["bar"]}), "")
        ]
    ),

    // Final part empty with lwsp-chars after final separator.
    new(
        name: "final part empty then lwsp",
        sep: "abc",
        @in: "--abc\r\nFoo: bar\r\n\r\n--abc-- \t",
        want: [
            new(new textproto.MIMEHeader(new map<@string, slice<@string>>{["Foo"] = ["bar"]}), "")
        ]
    ),

    // No parts (empty form as submitted by Chrome)
    new(
        name: "no parts",
        sep: "----WebKitFormBoundaryQfEAfzFOiSemeHfA",
        @in: "------WebKitFormBoundaryQfEAfzFOiSemeHfA--\r\n",
        want: []
    ),

    // Part containing data starting with the boundary, but with additional suffix.
    new(
        name: "fake separator as data",
        sep: "sep",
        @in: "--sep\r\nFoo: bar\r\n\r\n--sepFAKE\r\n--sep--",
        want: [
            new(new textproto.MIMEHeader(new map<@string, slice<@string>>{["Foo"] = ["bar"]}), "--sepFAKE")
        ]
    ),

    // Part containing a boundary with whitespace following it.
    new(
        name: "boundary with whitespace",
        sep: "sep",
        @in: "--sep \r\nFoo: bar\r\n\r\ntext\r\n--sep--",
        want: [
            new(new textproto.MIMEHeader(new map<@string, slice<@string>>{["Foo"] = ["bar"]}), "text")
        ]
    ),

    // With ignored leading line.
    new(
        name: "leading line",
        sep: "MyBoundary",
        @in: strings.Replace("""
This is a multi-part message.  This line is ignored.
--MyBoundary
foo: bar


--MyBoundary--
"""u8.Raw(), "\n", "\r\n", -1),
        want: [
            new(new textproto.MIMEHeader(new map<@string, slice<@string>>{["Foo"] = ["bar"]}), "")
        ]
    ),

    // Issue 10616; minimal
    new(
        name: "issue 10616 minimal",
        sep: "sep",
        @in: "--sep \r\nFoo: bar\r\n\r\n"u8 + "a\r\n"u8 + "--sep_alt\r\n"u8 + "b\r\n"u8 + "\r\n--sep--"u8,
        want: [
            new(new textproto.MIMEHeader(new map<@string, slice<@string>>{["Foo"] = ["bar"]}), "a\r\n--sep_alt\r\nb\r\n")
        ]
    ),

    // Issue 10616; full example from bug.
    new(
        name: "nested separator prefix is outer separator",
        sep: "----=_NextPart_4c2fbafd7ec4c8bf08034fe724b608d9",
        @in: strings.Replace("""
------=_NextPart_4c2fbafd7ec4c8bf08034fe724b608d9
Content-Type: multipart/alternative; boundary="----=_NextPart_4c2fbafd7ec4c8bf08034fe724b608d9_alt"

------=_NextPart_4c2fbafd7ec4c8bf08034fe724b608d9_alt
Content-Type: text/plain; charset="utf-8"
Content-Transfer-Encoding: 8bit

This is a multi-part message in MIME format.

------=_NextPart_4c2fbafd7ec4c8bf08034fe724b608d9_alt
Content-Type: text/html; charset="utf-8"
Content-Transfer-Encoding: 8bit

html things
------=_NextPart_4c2fbafd7ec4c8bf08034fe724b608d9_alt--
------=_NextPart_4c2fbafd7ec4c8bf08034fe724b608d9--
"""u8.Raw(), "\n", "\r\n", -1),
        want: slice<headerBody>([
            new(new textproto.MIMEHeader(new map<@string, slice<@string>>{["Content-Type"] = [@"multipart/alternative; boundary=""----=_NextPart_4c2fbafd7ec4c8bf08034fe724b608d9_alt"""]}),
                strings.Replace("""
------=_NextPart_4c2fbafd7ec4c8bf08034fe724b608d9_alt
Content-Type: text/plain; charset="utf-8"
Content-Transfer-Encoding: 8bit

This is a multi-part message in MIME format.

------=_NextPart_4c2fbafd7ec4c8bf08034fe724b608d9_alt
Content-Type: text/html; charset="utf-8"
Content-Transfer-Encoding: 8bit

html things
------=_NextPart_4c2fbafd7ec4c8bf08034fe724b608d9_alt--
"""u8.Raw(), "\n", "\r\n", -1)
            )
        ])
    ),

    // Issue 12662: Check that we don't consume the leading \r if the peekBuffer
    // ends in '\r\n--separator-'
    new(
        name: "peek buffer boundary condition",
        sep: "00ffded004d4dd0fdf945fbdef9d9050cfd6a13a821846299b27fc71b9db",
        @in: strings.Replace("""
--00ffded004d4dd0fdf945fbdef9d9050cfd6a13a821846299b27fc71b9db
Content-Disposition: form-data; name="block"; filename="block"
Content-Type: application/octet-stream


"""u8.Raw() + strings.Repeat("A", peekBufferSize - 65) + "\n--00ffded004d4dd0fdf945fbdef9d9050cfd6a13a821846299b27fc71b9db--"u8, "\n", "\r\n", -1),
        want: slice<headerBody>([
            new(new textproto.MIMEHeader(new map<@string, slice<@string>>{["Content-Type"] = [@"application/octet-stream"], ["Content-Disposition"] = [@"form-data; name=""block""; filename=""block"""]}),
                strings.Repeat("A", peekBufferSize - 65)
            )
        ])
    ),

    // Issue 12662: Same test as above with \r\n at the end
    new(
        name: "peek buffer boundary condition",
        sep: "00ffded004d4dd0fdf945fbdef9d9050cfd6a13a821846299b27fc71b9db",
        @in: strings.Replace("""
--00ffded004d4dd0fdf945fbdef9d9050cfd6a13a821846299b27fc71b9db
Content-Disposition: form-data; name="block"; filename="block"
Content-Type: application/octet-stream


"""u8.Raw() + strings.Repeat("A", peekBufferSize - 65) + "\n--00ffded004d4dd0fdf945fbdef9d9050cfd6a13a821846299b27fc71b9db--\n"u8, "\n", "\r\n", -1),
        want: slice<headerBody>([
            new(new textproto.MIMEHeader(new map<@string, slice<@string>>{["Content-Type"] = [@"application/octet-stream"], ["Content-Disposition"] = [@"form-data; name=""block""; filename=""block"""]}),
                strings.Repeat("A", peekBufferSize - 65)
            )
        ])
    ),

    // Issue 12662v2: We want to make sure that for short buffers that end with
    // '\r\n--separator-' we always consume at least one (valid) symbol from the
    // peekBuffer
    new(
        name: "peek buffer boundary condition",
        sep: "aaaaaaaaaa00ffded004d4dd0fdf945fbdef9d9050cfd6a13a821846299b27fc71b9db",
        @in: strings.Replace("""
--aaaaaaaaaa00ffded004d4dd0fdf945fbdef9d9050cfd6a13a821846299b27fc71b9db
Content-Disposition: form-data; name="block"; filename="block"
Content-Type: application/octet-stream


"""u8.Raw() + strings.Repeat("A", peekBufferSize) + "\n--aaaaaaaaaa00ffded004d4dd0fdf945fbdef9d9050cfd6a13a821846299b27fc71b9db--"u8, "\n", "\r\n", -1),
        want: slice<headerBody>([
            new(new textproto.MIMEHeader(new map<@string, slice<@string>>{["Content-Type"] = [@"application/octet-stream"], ["Content-Disposition"] = [@"form-data; name=""block""; filename=""block"""]}),
                strings.Repeat("A", peekBufferSize)
            )
        ])
    ),

    // Context: https://github.com/camlistore/camlistore/issues/642
    // If the file contents in the form happens to have a size such as:
    // size = peekBufferSize - (len("\n--") + len(boundary) + len("\r") + 1), (modulo peekBufferSize)
    // then peekBufferSeparatorIndex was wrongly returning (-1, false), which was leading to an nCopy
    // cut such as:
    // "somedata\r| |\n--Boundary\r" (instead of "somedata| |\r\n--Boundary\r"), which was making the
    // subsequent Read miss the boundary.
    new(
        name: "safeCount off by one",
        sep: "08b84578eabc563dcba967a945cdf0d9f613864a8f4a716f0e81caa71a74",
        @in: strings.Replace("""
--08b84578eabc563dcba967a945cdf0d9f613864a8f4a716f0e81caa71a74
Content-Disposition: form-data; name="myfile"; filename="my-file.txt"
Content-Type: application/octet-stream


"""u8.Raw(), "\n", "\r\n", -1) + strings.Repeat("A", (nint)peekBufferSize - (len("\n--") + len("08b84578eabc563dcba967a945cdf0d9f613864a8f4a716f0e81caa71a74") + len("\r") + 1)) + strings.Replace("""

--08b84578eabc563dcba967a945cdf0d9f613864a8f4a716f0e81caa71a74
Content-Disposition: form-data; name="key"

val
--08b84578eabc563dcba967a945cdf0d9f613864a8f4a716f0e81caa71a74--

"""u8.Raw(), "\n", "\r\n", -1),
        want: slice<headerBody>([
            new(new textproto.MIMEHeader(new map<@string, slice<@string>>{["Content-Type"] = [@"application/octet-stream"], ["Content-Disposition"] = [@"form-data; name=""myfile""; filename=""my-file.txt"""]}),
                strings.Repeat("A", (nint)peekBufferSize - (len("\n--") + len("08b84578eabc563dcba967a945cdf0d9f613864a8f4a716f0e81caa71a74") + len("\r") + 1))
            ),
            new(new textproto.MIMEHeader(new map<@string, slice<@string>>{["Content-Disposition"] = [@"form-data; name=""key"""]}),
                "val"
            )
        ])
    ),

    // Issue 46042; a nested multipart uses the outer separator followed by
    // a dash.
    new(
        name: "nested separator prefix is outer separator followed by a dash",
        sep: "foo",
        @in: strings.Replace("""
--foo
Content-Type: multipart/alternative; boundary="foo-bar"

--foo-bar

Body
--foo-bar

Body2
--foo-bar--
--foo--
"""u8.Raw(), "\n", "\r\n", -1),
        want: slice<headerBody>([
            new(new textproto.MIMEHeader(new map<@string, slice<@string>>{["Content-Type"] = [@"multipart/alternative; boundary=""foo-bar"""]}),
                strings.Replace("""
--foo-bar

Body
--foo-bar

Body2
--foo-bar--
"""u8.Raw(), "\n", "\r\n", -1)
            )
        ])
    ),

    // A nested boundary cannot be the outer separator followed by double dash.
    new(
        name: "nested separator prefix is outer separator followed by double dash",
        sep: "foo",
        @in: strings.Replace("""
--foo
Content-Type: multipart/alternative; boundary="foo--"

--foo--

Body

--foo--
"""u8.Raw(), "\n", "\r\n", -1),
        want: [
            new(new textproto.MIMEHeader(new map<@string, slice<@string>>{["Content-Type"] = [@"multipart/alternative; boundary=""foo--"""]}), "")
        ]
    ),

    roundTripParseTest()
]);

public static void TestParse(ж<testing.T> t) {
Cases:
    foreach (var (_, tt) in range(parseTests)) {
        var r = NewReader(new multipart_test_package.strings_ReaderжReader(strings.NewReader(tt.@in)), tt.sep);
        var got = slice<headerBody>([]);
        for (;;) {
            var (p, err) = r.NextPart();
            if (AreEqual(err, io.EOF)) {
                break;
            }
            if (err != nil) {
                t.Errorf("in test %q, NextPart: %v", tt.name, err);
                goto continue_Cases;
            }
            (var pbody, err) = io.ReadAll(new global::go.mime.multipart_package.PartжReader(p));
            if (err != nil) {
                t.Errorf("in test %q, error reading part: %v", tt.name, err);
                goto continue_Cases;
            }
            got = append(got, new headerBody(p.Header, (@string)pbody));
        }
        if (!reflect.DeepEqual(tt.want, got)) {
            t.Errorf("test %q:\n got: %v\nwant: %v", tt.name, got, tt.want);
            if (len(tt.want) != len(got)) {
                t.Errorf("test %q: got %d parts, want %d", tt.name, len(got), len(tt.want));
            } else if (len(got) > 1) {
                foreach (var (pi, wantPart) in range(tt.want)) {
                    if (!reflect.DeepEqual(wantPart, got[pi])) {
                        t.Errorf("test %q, part %d:\n got: %v\nwant: %v", tt.name, pi, got[pi], wantPart);
                    }
                }
            }
        }
continue_Cases:;
    }
break_Cases:;
}

internal static (slice<headerBody>, error) partsFromReader(ж<global::go.mime.multipart_package.Reader> r) {
    var got = slice<headerBody>([]);
    for (;;) {
        var (p, err) = r.NextPart();
        if (AreEqual(err, io.EOF)) {
            return (got, default);
        }
        if (err != nil) {
            return (default, fmt.Errorf("NextPart: %v"u8, err));
        }
        (var pbody, err) = io.ReadAll(new global::go.mime.multipart_package.PartжReader(p));
        if (err != nil) {
            return (default, fmt.Errorf("error reading part: %v"u8, err));
        }
        got = append(got, new headerBody(p.Header, (@string)pbody));
    }
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static ref readonly @string keyˢ => ref keyˢᵇ;
private static readonly @string keyˢᵇ = "key"u8;

public static void TestParseAllSizes(ж<testing.T> t) {
    t.Parallel();
    nint maxSize = 5 << 10;
    if (testing.Short()) {
        maxSize = 512;
    }
    ref var buf = ref heap(new bytes.Buffer(), out var Ꮡbuf);
    @string body = strings.Repeat("a", maxSize);
    var bodyb = slice<byte>(body);
    for (nint size = 0; size < maxSize; size++) {
        buf.Reset();
        var w = NewWriter(new multipart_test_package.bytes_BufferжWriter(Ꮡbuf));
        var (part, _) = w.CreateFormField("f");
        part.Write(bodyb.slice(0, size));
        (part, _) = w.CreateFormField(keyˢ);
        part.Write(slice<byte>("val"u8));
        w.Close();
        var r = NewReader(new multipart_test_package.bytes_BufferжReader(Ꮡbuf), w.Boundary());
        var (got, err) = partsFromReader(r);
        if (err != nil) {
            t.Errorf("For size %d: %v", size, err);
            continue;
        }
        if (len(got) != 2) {
            t.Errorf("For size %d, num parts = %d; want 2", size, len(got));
            continue;
        }
        if (got[0].body != body.slice(0, size)) {
            t.Errorf("For size %d, got unexpected len %d: %q", size, len(got[0].body), got[0].body);
        }
    }
}

internal static parseTest roundTripParseTest() {
    var t = new parseTest(
        name: "round trip",
        want: slice<headerBody>([
            formData("empty", ""),
            formData("lf", "\n"),
            formData("cr", "\r"),
            formData("crlf", "\r\n"),
            formData("foo", "bar")
        ])
    );
    ref var buf = ref heap(new strings.Builder(), out var Ꮡbuf);
    var w = NewWriter(new multipart_test_package.strings_BuilderжWriter(Ꮡbuf));
    foreach (var (_, p) in range(t.want)) {
        var (pw, err) = w.CreatePart(p.header);
        if (err != nil) {
            throw panic(err);
        }
        (_, err) = pw.Write(slice<byte>(p.body));
        if (err != nil) {
            throw panic(err);
        }
    }
    w.Close();
    t.@in = buf.String();
    t.sep = w.Boundary();
    return t;
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static ref readonly @string multipartBoundaryIsEmptyˢ => ref multipartBoundaryIsEmptyˢᵇ;
private static readonly @string multipartBoundaryIsEmptyˢᵇ = "multipart: boundary is empty"u8;

public static void TestNoBoundary(ж<testing.T> t) {
    var mr = NewReader(new multipart_test_package.strings_ReaderжReader(strings.NewReader(""u8)), ""u8);
    var (_, err) = mr.NextPart();
    @string got = fmt.Sprint(err);
    @string want = multipartBoundaryIsEmptyˢ; if (got != want) {
        t.Errorf("NextPart error = %v; want %v", got, want);
    }
}

} // end multipart_internal_test_package
