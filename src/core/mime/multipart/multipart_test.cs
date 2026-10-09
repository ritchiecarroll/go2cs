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
internal static readonly @string myBoundaryˢ = "myBoundary"u8;
internal static readonly object expectedˢ = (@string)"expected"u8;
internal static readonly object expectedFailˢ = (@string)"expected fail"u8;

public static void TestBoundaryLine(ж<testing.T> Ꮡt) {
    var mr = NewReader(new multipart_test_package.strings_ReaderжReader(strings.NewReader(""u8)), myBoundaryˢ);
    if (!mr.isBoundaryDelimiterLine(slice<byte>("--myBoundary\r\n"u8))) {
        Ꮡt.Error(expectedˢ);
    }
    if (!mr.isBoundaryDelimiterLine(slice<byte>("--myBoundary \r\n"u8))) {
        Ꮡt.Error(expectedˢ);
    }
    if (!mr.isBoundaryDelimiterLine(slice<byte>("--myBoundary \n"u8))) {
        Ꮡt.Error(expectedˢ);
    }
    if (mr.isBoundaryDelimiterLine(slice<byte>("--myBoundary bogus \n"u8))) {
        Ꮡt.Error(expectedFailˢ);
    }
    if (mr.isBoundaryDelimiterLine(slice<byte>("--myBoundary bogus--"u8))) {
        Ꮡt.Error(expectedFailˢ);
    }
}

internal static @string escapeString(@string v) {
    var (bytes, _) = json.Marshal(v);
    return ((@string)bytes);
}

internal static void expectEq(ж<testing.T> Ꮡt, @string expected, @string actual, @string what) {
    if (expected == actual) {
        return;
    }
    Ꮡt.Errorf("Unexpected value for %s; got %s (len %d) but expected: %s (len %d)"u8,
        what, escapeString(actual), len(actual), escapeString(expected), len(expected));
}

public static void TestNameAccessors(ж<testing.T> Ꮡt) {
    var tests = new array<@string>[]{
        new @string[]{@"form-data; name=""foo"""u8, "foo"u8, ""u8}.array(),
        new @string[]{@" form-data ; name=foo"u8, "foo"u8, ""u8}.array(),
        new @string[]{@"FORM-DATA;name=""foo"""u8, "foo"u8, ""u8}.array(),
        new @string[]{@" FORM-DATA ; name=""foo"""u8, "foo"u8, ""u8}.array(),
        new @string[]{@" FORM-DATA ; name=""foo"""u8, "foo"u8, ""u8}.array(),
        new @string[]{@" FORM-DATA ; name=foo"u8, "foo"u8, ""u8}.array(),
        new @string[]{@" FORM-DATA ; filename=""foo.txt""; name=foo; baz=quux"u8, "foo"u8, "foo.txt"u8}.array(),
        new @string[]{@" not-form-data ; filename=""bar.txt""; name=foo; baz=quux"u8, ""u8, "bar.txt"u8}.array()
    }.array();
    foreach (var (i, vᴛ1) in tests.ΔRangeSnapshot()) {
        var test = vᴛ1.Clone();

        var p = Ꮡ(new Part(Header: new map<@string, slice<@string>>()));
        (~p).Header.Set(contentDispositionˢ, test[0]);
        {
            @string g = p.FormName();
            @string e = test[1]; if (g != e) {
                Ꮡt.Errorf("test %d: FormName() = %q; want %q"u8, i, g, e);
            }
        }
        {
            @string g = p.FileName();
            @string e = test[2]; if (g != e) {
                Ꮡt.Errorf("test %d: FileName() = %q; want %q"u8, i, g, e);
            }
        }
    }
}

internal static @string longLine = strings.Repeat("\n\n\r\r\r\n\r\u0000"u8, ((1 << (int)(20))) / 8);

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static readonly @string thisIsAMultiPartMessageˢ = """

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

"""u8;
internal static readonly @string longlineˢ = "[longline]"u8;

internal static @string testMultipartBody(@string sep) {
    @string testBody = thisIsAMultiPartMessageˢ;
    testBody = strings.ReplaceAll(testBody, "\n"u8, sep);
    return strings.Replace(testBody, longlineˢ, longLine, 1);
}

public static void TestMultipart(ж<testing.T> Ꮡt) {
    var bodyReader = strings.NewReader(testMultipartBody("\r\n"u8));
    testMultipart(Ꮡt, new multipart_test_package.strings_ReaderжReader(bodyReader), false);
}

public static void TestMultipartOnlyNewlines(ж<testing.T> Ꮡt) {
    var bodyReader = strings.NewReader(testMultipartBody("\n"u8));
    testMultipart(Ꮡt, new multipart_test_package.strings_ReaderжReader(bodyReader), true);
}

public static void TestMultipartSlowInput(ж<testing.T> Ꮡt) {
    var bodyReader = strings.NewReader(testMultipartBody("\r\n"u8));
    testMultipart(Ꮡt, new multipart_internal_test_package.slowReaderжReader(Ꮡ(new slowReader(new multipart_test_package.strings_ReaderжReader(bodyReader)))), false);
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static readonly @string myBoundaryˢ2 = "MyBoundary"u8;
internal static readonly object expectedPart1ˢ = (@string)"Expected part1"u8;
internal static readonly @string header1ˢ = "Header1"u8;
internal static readonly object value1ˢ = (@string)"value1"u8;
internal static readonly @string fooBarˢ = "foo-bar"u8;
internal static readonly object bazˢ = (@string)"baz"u8;
internal static readonly @string fooBarˢ2 = "Foo-Bar"u8;
internal static readonly @string myValueTheEndˢ = "My value\r\nThe end."u8;
internal static readonly @string valueOfFirstPartˢ = "Value of first part"u8;
internal static readonly @string bigsectionˢ = "bigsection"u8;
internal static readonly object expectedPart3ˢ = (@string)"Expected part3"u8;
internal static readonly object expectedFooBarBazbˢ = (@string)"Expected foo-bar: bazb"u8;
internal static readonly @string line1Line2Line3EndsInAˢ = "Line 1\r\nLine 2\r\nLine 3 ends in a newline, but just one.\r\n"u8;
internal static readonly @string bodyOfPart3ˢ = "body of part 3"u8;
internal static readonly object expectedPart4Withoutˢ = (@string)"Expected part 4 without errors"u8;
internal static readonly object didnTExpectAFifthPartˢ = (@string)"Didn't expect a fifth part."u8;

internal static void testMultipart(ж<testing.T> Ꮡt, io.Reader r, bool onlyNewlines) {
    ref var t = ref Ꮡt.DerefOrNull();

    Ꮡt.Parallel();
    var reader = NewReader(r, myBoundaryˢ2);
    var buf = @new<strings.Builder>();
    // Part1
    var (part, err) = reader.NextPart();
    if (part == nil || err != default!) {
        Ꮡt.Error(expectedPart1ˢ);
        return;
    }
    {
        @string x = (~part).Header.Get(header1ˢ); if (x != "value1"u8) {
            Ꮡt.Errorf("part.Header.Get(%q) = %q, want %q"u8, header1ˢ, x, value1ˢ);
        }
    }
    {
        @string x = (~part).Header.Get(fooBarˢ); if (x != "baz"u8) {
            Ꮡt.Errorf("part.Header.Get(%q) = %q, want %q"u8, fooBarˢ, x, bazˢ);
        }
    }
    {
        @string x = (~part).Header.Get(fooBarˢ2); if (x != "baz"u8) {
            Ꮡt.Errorf("part.Header.Get(%q) = %q, want %q"u8, fooBarˢ2, x, bazˢ);
        }
    }
    buf.Reset();
    {
        var (_, errΔ1) = io.Copy(new multipart_test_package.strings_BuilderжWriter(buf), new global::go.mime.multipart_package.PartжReader(part)); if (errΔ1 != default!) {
            Ꮡt.Errorf("part 1 copy: %v"u8, errΔ1);
        }
    }
    @string adjustNewlines(@string sΔ1) {
        if (onlyNewlines) {
            return strings.ReplaceAll(sΔ1, "\r\n"u8, "\n"u8);
        }
        return sΔ1;
    }
    expectEq(Ꮡt, adjustNewlines(myValueTheEndˢ), buf.String(), valueOfFirstPartˢ);
    // Part2
    (part, err) = reader.NextPart();
    if (err != default!) {
        Ꮡt.Fatalf("Expected part2; got: %v"u8, err);
        return;
    }
    {
        @string e = bigsectionˢ;
        @string g = (~part).Header.Get(nameˢ); if (e != g) {
            Ꮡt.Errorf("part2's name header: expected %q, got %q"u8, e, g);
        }
    }
    buf.Reset();
    {
        var (_, errΔ2) = io.Copy(new multipart_test_package.strings_BuilderжWriter(buf), new global::go.mime.multipart_package.PartжReader(part)); if (errΔ2 != default!) {
            Ꮡt.Errorf("part 2 copy: %v"u8, errΔ2);
        }
    }
    @string s = buf.String();
    if (len(s) != len(longLine)) {
        Ꮡt.Errorf("part2 body expected long line of length %d; got length %d"u8,
            len(longLine), len(s));
    }
    if (s != longLine) {
        Ꮡt.Errorf("part2 long body didn't match"u8);
    }
    // Part3
    (part, err) = reader.NextPart();
    if (part == nil || err != default!) {
        Ꮡt.Error(expectedPart3ˢ);
        return;
    }
    if ((~part).Header.Get(fooBarˢ) != "bazb"u8) {
        Ꮡt.Error(expectedFooBarBazbˢ);
    }
    buf.Reset();
    {
        var (_, errΔ3) = io.Copy(new multipart_test_package.strings_BuilderжWriter(buf), new global::go.mime.multipart_package.PartжReader(part)); if (errΔ3 != default!) {
            Ꮡt.Errorf("part 3 copy: %v"u8, errΔ3);
        }
    }
    expectEq(Ꮡt, adjustNewlines(line1Line2Line3EndsInAˢ),
        buf.String(), bodyOfPart3ˢ);
    // Part4
    (part, err) = reader.NextPart();
    if (part == nil || err != default!) {
        Ꮡt.Error(expectedPart4Withoutˢ);
        return;
    }
    // Non-existent part5
    (part, err) = reader.NextPart();
    if (part != nil) {
        Ꮡt.Error(didnTExpectAFifthPartˢ);
    }
    if (!AreEqual(err, io.EOF)) {
        Ꮡt.Errorf("On fifth part expected io.EOF; got %v"u8, err);
    }
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static readonly @string boundaryˢ2 = "BOUNDARY"u8;

public static void TestVariousTextLineEndings(ж<testing.T> Ꮡt) {
    var tests = new @string[]{
        "Foo\nBar"u8,
        "Foo\nBar\n"u8,
        "Foo\r\nBar"u8,
        "Foo\r\nBar\r\n"u8,
        "Foo\rBar"u8,
        "Foo\rBar\r"u8,
        "\x00\x01\x02\x09\x0a\x0b\x0c\x0d\x0e\x0f\x10"u8
    }.array();
    foreach (var (testNum, expectedBody) in tests.ΔRangeSnapshot()) {
        @string body = "--BOUNDARY\r\n"u8 + "Content-Disposition: form-data; name=\"value\"\r\n"u8 + "\r\n"u8 + expectedBody + "\r\n--BOUNDARY--\r\n"u8;
        var bodyReader = strings.NewReader(body);
        var reader = NewReader(new multipart_test_package.strings_ReaderжReader(bodyReader), boundaryˢ2);
        var buf = @new<bytes.Buffer>();
        var (part, err) = reader.NextPart();
        if (part == nil) {
            Ꮡt.Errorf("Expected a body part on text %d"u8, testNum);
            continue;
        }
        if (err != default!) {
            Ꮡt.Errorf("Unexpected error on text %d: %v"u8, testNum, err);
            continue;
        }
        (var written, err) = io.Copy(new multipart_test_package.bytes_BufferжWriter(buf), new global::go.mime.multipart_package.PartжReader(part));
        expectEq(Ꮡt, expectedBody, buf.String(), fmt.Sprintf("test %d"u8, testNum));
        if (err != default!) {
            Ꮡt.Errorf("Error copying multipart; bytes=%v, error=%v"u8, written, err);
        }
        (part, err) = reader.NextPart();
        if (part != nil) {
            Ꮡt.Errorf("Unexpected part in test %d"u8, testNum);
        }
        if (!AreEqual(err, io.EOF)) {
            Ꮡt.Errorf("On test %d expected io.EOF; got %v"u8, testNum, err);
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
    return (len(b), default!);
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static readonly @string fooBoundaryˢ = "fooBoundary"u8;

public static void TestLineLimit(ж<testing.T> Ꮡt) {
    var mr = Ꮡ(new maliciousReader(t: Ꮡt));
    var r = NewReader(new multipart_internal_test_package.maliciousReaderжReader(mr), fooBoundaryˢ);
    var (part, err) = r.NextPart();
    if (part != nil) {
        Ꮡt.Errorf("unexpected part read"u8);
    }
    if (err == default!) {
        Ꮡt.Errorf("expected an error"u8);
    }
    if ((~mr).n >= maxReadThreshold) {
        Ꮡt.Errorf("expected to read < %d bytes; read %d"u8, (nint)(maxReadThreshold), (~mr).n);
    }
}

public static void TestMultipartTruncated(ж<testing.T> Ꮡt) {
    ref var t = ref Ꮡt.DerefOrNull();

    foreach (var (_, vᴛ1) in new @string[]{
        """

This is a multi-part message.  This line is ignored.
--MyBoundary
foo-bar: baz

Oh no, premature EOF!

"""u8,
        """

This is a multi-part message.  This line is ignored.
--MyBoundary
foo-bar: baz

Oh no, premature EOF!
--MyBoundary-
"""u8
    }.slice()) {
        var body = vᴛ1;

        body = strings.ReplaceAll(body, "\n"u8, "\r\n"u8);
        var bodyReader = strings.NewReader(body);
        var r = NewReader(new multipart_test_package.strings_ReaderжReader(bodyReader), myBoundaryˢ2);
        var (part, err) = r.NextPart();
        if (err != default!) {
            Ꮡt.Fatalf("didn't get a part"u8);
        }
        (_, err) = io.Copy(io.Discard, new global::go.mime.multipart_package.PartжReader(part));
        if (!AreEqual(err, io.ErrUnexpectedEOF)) {
            Ꮡt.Fatalf("expected error io.ErrUnexpectedEOF; got %v"u8, err);
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
    if (s.done != default!) {
        builtin.close(s.done);
        s.done = default!;
    }
    return (0, io.EOF);
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static readonly @string thisIsAMultiPartMessageˢ2 = """

This is a multi-part message.  This line is ignored.
--MyBoundary
foo-bar: baz

Body
--MyBoundary

"""u8;
internal static readonly @string fooBarBopBody2MyBoundaryˢ = """
foo-bar: bop

Body 2
--MyBoundary--

"""u8;
internal static readonly @string bodyˢ = "Body"u8;
internal static readonly @string body2ˢ = "Body 2"u8;

// TestMultipartStreamReadahead tests that PartReader does not block
// on reading past the end of a part, ensuring that it can be used on
// a stream like multipart/x-mixed-replace. See golang.org/issue/15431
public static void TestMultipartStreamReadahead(ж<testing.T> Ꮡt) {
    @string testBody1 = thisIsAMultiPartMessageˢ2;
    @string testBody2 = fooBarBopBody2MyBoundaryˢ;
    var done1 = new channel<EmptyStruct>(0);
    var reader = NewReader(
        io.MultiReader(
            new multipart_test_package.strings_ReaderжReader(strings.NewReader(testBody1)),
            new multipart_internal_test_package.sentinelReaderжReader(Ꮡ(new sentinelReader(done1))),
            new multipart_test_package.strings_ReaderжReader(strings.NewReader(testBody2))),
        myBoundaryˢ2);
    nint i = default!;
    var readerʗ1 = reader;
    void readPart(textproto.MIMEHeader hdr, @string body) {
        var (part, err) = readerʗ1.NextPart();
        if (part == nil || err != default!) {
            Ꮡt.Fatalf("Part %d: NextPart failed: %v"u8, i, err);
        }
        if (!reflect.DeepEqual((~part).Header, hdr)) {
            Ꮡt.Errorf("Part %d: part.Header = %v, want %v"u8, i, (~part).Header, hdr);
        }
        (var data, err) = io.ReadAll(new global::go.mime.multipart_package.PartжReader(part));
        expectEq(Ꮡt, body, ((@string)data), fmt.Sprintf("Part %d body"u8, i));
        if (err != default!) {
            Ꮡt.Fatalf("Part %d: ReadAll failed: %v"u8, i, err);
        }
        i++;
    }
    readPart(new textproto.MIMEHeader(new map<@string, slice<@string>>{["Foo-Bar"u8] = new @string[]{"baz"u8}.slice()}), bodyˢ);
    var selᴛ1 = done1;
    switch (trySelect(ᐸꟷ(selᴛ1, ꓸꓸꓸ))) {
    case 0 when selᴛ1.ꟷᐳ(out _): {
        Ꮡt.Errorf("Reader read past second boundary"u8);
        break;
    }
    default: {
        break;
    }}
    readPart(new textproto.MIMEHeader(new map<@string, slice<@string>>{["Foo-Bar"u8] = new @string[]{"bop"u8}.slice()}), body2ˢ);
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static readonly @string appleMail2292336769ˢ = "\n--Apple-Mail-2-292336769\nContent-Transfer-Encoding: 7bit\nContent-Type: text/plain;\n\tcharset=US-ASCII;\n\tdelsp=yes;\n\tformat=flowed\n\nI'm finding the same thing happening on my system (10.4.1).\n\n\n--Apple-Mail-2-292336769\nContent-Transfer-Encoding: quoted-printable\nContent-Type: text/html;\n\tcharset=ISO-8859-1\n\n<HTML><BODY>I'm finding the same thing =\nhappening on my system (10.4.1).=A0 But I built it with XCode =\n2.0.</BODY></=\nHTML>=\n\r\n--Apple-Mail-2-292336769--\n"u8;
internal static readonly @string appleMail2292336769ˢ2 = "Apple-Mail-2-292336769"u8;

public static void TestLineContinuation(ж<testing.T> Ꮡt) {
    ref var t = ref Ꮡt.DerefOrNull();

    // This body, extracted from an email, contains headers that span multiple
    // lines.
    // TODO: The original mail ended with a double-newline before the
    // final delimiter; this was manually edited to use a CRLF.
    @string testBody = appleMail2292336769ˢ;
    var r = NewReader(new multipart_test_package.strings_ReaderжReader(strings.NewReader(testBody)), appleMail2292336769ˢ2);
    for (nint i = 0; i < 2; i++) {
        var (part, err) = r.NextPart();
        if (err != default!) {
            Ꮡt.Fatalf("didn't get a part"u8);
        }
        ref var buf = ref heap(new strings.Builder(), out var Ꮡbuf);
        (var n, err) = io.Copy(new multipart_test_package.strings_BuilderжWriter(Ꮡbuf), new global::go.mime.multipart_package.PartжReader(part));
        if (err != default!) {
            Ꮡt.Errorf("error reading part: %v\nread so far: %q"u8, err, buf.String());
        }
        if (n <= 0) {
            Ꮡt.Errorf("read %d bytes; expected >0"u8, n);
        }
    }
}

public static void TestQuotedPrintableEncoding(ж<testing.T> Ꮡt) {
    foreach (var (_, cte) in new @string[]{"quoted-printable"u8, "Quoted-PRINTABLE"u8}.slice()) {
        Ꮡt.Run(cte, (ж<testing.T> tΔ1) => {
            testQuotedPrintableEncoding(tΔ1, cte);
        });
    }
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static readonly @string contentTransferEncodingˢ = "Content-Transfer-Encoding"u8;
internal static readonly @string wordsWordsWordsWordsˢ = "words words words words words words words words words words words words words words words words words words words words words words words words words words words words words words words words words words words words words words words words words words words words words words words words words words words words words words words words words words words"u8;

internal static void testQuotedPrintableEncoding(ж<testing.T> Ꮡt, @string cte) {
    // From https://golang.org/issue/4411
    @string body = "--0016e68ee29c5d515f04cedf6733\r\nContent-Type: text/plain; charset=ISO-8859-1\r\nContent-Disposition: form-data; name=text\r\nContent-Transfer-Encoding: "u8 + cte + "\r\n\r\nwords words words words words words words words words words words words wor=\r\nds words words words words words words words words words words words words =\r\nwords words words words words words words words words words words words wor=\r\nds words words words words words words words words words words words words =\r\nwords words words words words words words words words\r\n--0016e68ee29c5d515f04cedf6733\r\nContent-Type: text/plain; charset=ISO-8859-1\r\nContent-Disposition: form-data; name=submit\r\n\r\nSubmit\r\n--0016e68ee29c5d515f04cedf6733--"u8;
    var r = NewReader(new multipart_test_package.strings_ReaderжReader(strings.NewReader(body)), "0016e68ee29c5d515f04cedf6733"u8);
    var (part, err) = r.NextPart();
    if (err != default!) {
        Ꮡt.Fatal(err);
    }
    {
        var (te, ok) = (~part).Header[contentTransferEncodingˢ, ꟷ]; if (ok) {
            Ꮡt.Errorf("unexpected Content-Transfer-Encoding of %q"u8, te);
        }
    }
    ref var buf = ref heap(new strings.Builder(), out var Ꮡbuf);
    (_, err) = io.Copy(new multipart_test_package.strings_BuilderжWriter(Ꮡbuf), new global::go.mime.multipart_package.PartжReader(part));
    if (err != default!) {
        Ꮡt.Error(err);
    }
    @string got = buf.String();
    @string want = wordsWordsWordsWordsˢ;
    if (got != want) {
        Ꮡt.Errorf("wrong part value:\n got: %q\nwant: %q"u8, got, want);
    }
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static readonly @string contentTypeTextPlainˢ = """
--0016e68ee29c5d515f04cedf6733
Content-Type: text/plain; charset="utf-8"
Content-Transfer-Encoding: quoted-printable

<div dir=3D"ltr">Hello World.</div>
--0016e68ee29c5d515f04cedf6733
Content-Type: text/plain; charset="utf-8"
Content-Transfer-Encoding: quoted-printable

<div dir=3D"ltr">Hello World.</div>
--0016e68ee29c5d515f04cedf6733--
"""u8;
internal static readonly @string divDir3dLtrHelloWorldDivˢ = @"<div dir=3D""ltr"">Hello World.</div>"u8;
internal static readonly @string divDirLtrHelloWorldDivˢ = @"<div dir=""ltr"">Hello World.</div>"u8;

public static void TestRawPart(ж<testing.T> Ꮡt) {
    // https://github.com/golang/go/issues/29090
    @string body = strings.Replace(contentTypeTextPlainˢ, "\n"u8, "\r\n"u8, -1);
    var r = NewReader(new multipart_test_package.strings_ReaderжReader(strings.NewReader(body)), "0016e68ee29c5d515f04cedf6733"u8);
    // This part is expected to be raw, bypassing the automatic handling
    // of quoted-printable.
    var (part, err) = r.NextRawPart();
    if (err != default!) {
        Ꮡt.Fatal(err);
    }
    {
        var (_, ok) = (~part).Header[contentTransferEncodingˢ, ꟷ]; if (!ok) {
            Ꮡt.Errorf("missing Content-Transfer-Encoding"u8);
        }
    }
    ref var buf = ref heap(new strings.Builder(), out var Ꮡbuf);
    (_, err) = io.Copy(new multipart_test_package.strings_BuilderжWriter(Ꮡbuf), new global::go.mime.multipart_package.PartжReader(part));
    if (err != default!) {
        Ꮡt.Error(err);
    }
    @string got = buf.String();
    // Data is still quoted-printable.
    @string want = divDir3dLtrHelloWorldDivˢ;
    if (got != want) {
        Ꮡt.Errorf("wrong part value:\n got: %q\nwant: %q"u8, got, want);
    }
    // This part is expected to have automatic decoding of quoted-printable.
    (part, err) = r.NextPart();
    if (err != default!) {
        Ꮡt.Fatal(err);
    }
    {
        var (te, ok) = (~part).Header[contentTransferEncodingˢ, ꟷ]; if (ok) {
            Ꮡt.Errorf("unexpected Content-Transfer-Encoding of %q"u8, te);
        }
    }
    buf.Reset();
    (_, err) = io.Copy(new multipart_test_package.strings_BuilderжWriter(Ꮡbuf), new global::go.mime.multipart_package.PartжReader(part));
    if (err != default!) {
        Ꮡt.Error(err);
    }
    got = buf.String();
    // QP data has been decoded.
    want = divDirLtrHelloWorldDivˢ;
    if (got != want) {
        Ꮡt.Errorf("wrong part value:\n got: %q\nwant: %q"u8, got, want);
    }
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static readonly @string testdataNestedMimeˢ = "testdata/nested-mime"u8;

// Test parsing an image attachment from gmail, which previously failed.
public static void TestNested(ж<testing.T> Ꮡt) {
    GoFrame ᒐ = default;
    try {
        // nested-mime is the body part of a multipart/mixed email
        // with boundary e89a8ff1c1e83553e304be640612
        var (f, err) = os.Open(testdataNestedMimeˢ);
        if (err != default!) {
            Ꮡt.Fatal(err);
        }
        var fʗ1 = f;
        defer(() => fʗ1.Close(), ref ᒐ);
        var mr = NewReader(new multipart_test_package.os_FileжReader(f), "e89a8ff1c1e83553e304be640612"u8);
        (var p, err) = mr.NextPart();
        if (err != default!) {
            Ꮡt.Fatalf("error reading first section (alternative): %v"u8, err);
        }
        // Read the inner text/plain and text/html sections of the multipart/alternative.
        var mr2 = NewReader(new global::go.mime.multipart_package.PartжReader(p), "e89a8ff1c1e83553e004be640610"u8);
        (p, err) = mr2.NextPart();
        if (err != default!) {
            Ꮡt.Fatalf("reading text/plain part: %v"u8, err);
        }
        {
            var (b, errΔ1) = io.ReadAll(new global::go.mime.multipart_package.PartжReader(p)); if (((sstring)b) != "*body*\r\n"u8 || errΔ1 != default!) {
                Ꮡt.Fatalf("reading text/plain part: got %q, %v"u8, b, errΔ1);
            }
        }
        (p, err) = mr2.NextPart();
        if (err != default!) {
            Ꮡt.Fatalf("reading text/html part: %v"u8, err);
        }
        {
            var (b, errΔ2) = io.ReadAll(new global::go.mime.multipart_package.PartжReader(p)); if (((sstring)b) != "<b>body</b>\r\n"u8 || errΔ2 != default!) {
                Ꮡt.Fatalf("reading text/html part: got %q, %v"u8, b, errΔ2);
            }
        }
        (p, err) = mr2.NextPart();
        if (!AreEqual(err, io.EOF)) {
            Ꮡt.Fatalf("final inner NextPart = %v; want io.EOF"u8, err);
        }
        // Back to the outer multipart/mixed, reading the image attachment.
        (_, err) = mr.NextPart();
        if (err != default!) {
            Ꮡt.Fatalf("error reading the image attachment at the end: %v"u8, err);
        }
        (_, err) = mr.NextPart();
        if (!AreEqual(err, io.EOF)) {
            Ꮡt.Fatalf("final outer NextPart = %v; want io.EOF"u8, err);
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
            ["Content-Type"u8] = new @string[]{"text/plain; charset=ISO-8859-1"u8}.slice(),
            ["Content-Disposition"u8] = new @string[]{"form-data; name="u8 + key}.slice()
        }),
        value
    );
}

internal partial struct parseTest {
    internal @string name;
    internal @string @in, sep;
    internal slice<headerBody> want;
}

// Actual body from App Engine on a blob upload. The final part (the
// Content-Type: message/external-body) is what App Engine replaces
// the uploaded file with. The other form fields (prefixed with
// "other" in their form-data name) are unchanged. A bug was
// reported with blob uploads failing when the other fields were
// empty. This was the MIME POST body that previously failed.
// Single empty part, ended with --boundary immediately after headers.
// Single empty part, ended with \r\n--boundary immediately after headers.
// Final part empty.
// Final part empty with newlines after final separator.
// Final part empty with lwsp-chars after final separator.
// No parts (empty form as submitted by Chrome)
// Part containing data starting with the boundary, but with additional suffix.
// Part containing a boundary with whitespace following it.
// With ignored leading line.
// Issue 10616; minimal
// Issue 10616; full example from bug.
// Issue 12662: Check that we don't consume the leading \r if the peekBuffer
// ends in '\r\n--separator-'
// Issue 12662: Same test as above with \r\n at the end
// Issue 12662v2: We want to make sure that for short buffers that end with
// '\r\n--separator-' we always consume at least one (valid) symbol from the
// peekBuffer
// Context: https://github.com/camlistore/camlistore/issues/642
// If the file contents in the form happens to have a size such as:
// size = peekBufferSize - (len("\n--") + len(boundary) + len("\r") + 1), (modulo peekBufferSize)
// then peekBufferSeparatorIndex was wrongly returning (-1, false), which was leading to an nCopy
// cut such as:
// "somedata\r| |\n--Boundary\r" (instead of "somedata| |\r\n--Boundary\r"), which was making the
// subsequent Read miss the boundary.
// Issue 46042; a nested multipart uses the outer separator followed by
// a dash.
// A nested boundary cannot be the outer separator followed by double dash.
internal static slice<parseTest> parseTests = new parseTest[]{
    new(
        name: "App Engine post"u8,
        sep: "00151757727e9583fd04bfbca4c6"u8,
        @in: "--00151757727e9583fd04bfbca4c6\r\nContent-Type: text/plain; charset=ISO-8859-1\r\nContent-Disposition: form-data; name=otherEmpty1\r\n\r\n--00151757727e9583fd04bfbca4c6\r\nContent-Type: text/plain; charset=ISO-8859-1\r\nContent-Disposition: form-data; name=otherFoo1\r\n\r\nfoo\r\n--00151757727e9583fd04bfbca4c6\r\nContent-Type: text/plain; charset=ISO-8859-1\r\nContent-Disposition: form-data; name=otherFoo2\r\n\r\nfoo\r\n--00151757727e9583fd04bfbca4c6\r\nContent-Type: text/plain; charset=ISO-8859-1\r\nContent-Disposition: form-data; name=otherEmpty2\r\n\r\n--00151757727e9583fd04bfbca4c6\r\nContent-Type: text/plain; charset=ISO-8859-1\r\nContent-Disposition: form-data; name=otherRepeatFoo\r\n\r\nfoo\r\n--00151757727e9583fd04bfbca4c6\r\nContent-Type: text/plain; charset=ISO-8859-1\r\nContent-Disposition: form-data; name=otherRepeatFoo\r\n\r\nfoo\r\n--00151757727e9583fd04bfbca4c6\r\nContent-Type: text/plain; charset=ISO-8859-1\r\nContent-Disposition: form-data; name=otherRepeatEmpty\r\n\r\n--00151757727e9583fd04bfbca4c6\r\nContent-Type: text/plain; charset=ISO-8859-1\r\nContent-Disposition: form-data; name=otherRepeatEmpty\r\n\r\n--00151757727e9583fd04bfbca4c6\r\nContent-Type: text/plain; charset=ISO-8859-1\r\nContent-Disposition: form-data; name=submit\r\n\r\nSubmit\r\n--00151757727e9583fd04bfbca4c6\r\nContent-Type: message/external-body; charset=ISO-8859-1; blob-key=AHAZQqG84qllx7HUqO_oou5EvdYQNS3Mbbkb0RjjBoM_Kc1UqEN2ygDxWiyCPulIhpHRPx-VbpB6RX4MrsqhWAi_ZxJ48O9P2cTIACbvATHvg7IgbvZytyGMpL7xO1tlIvgwcM47JNfv_tGhy1XwyEUO8oldjPqg5Q\r\nContent-Disposition: form-data; name=file; filename=\"fall.png\"\r\n\r\nContent-Type: image/png\r\nContent-Length: 232303\r\nX-AppEngine-Upload-Creation: 2012-05-10 23:14:02.715173\r\nContent-MD5: MzRjODU1ZDZhZGU1NmRlOWEwZmMwMDdlODBmZTA0NzA=\r\nContent-Disposition: form-data; name=file; filename=\"fall.png\"\r\n\r\n\r\n--00151757727e9583fd04bfbca4c6--"u8,
        want: new headerBody[]{
            formData("otherEmpty1"u8, ""u8),
            formData("otherFoo1"u8, "foo"u8),
            formData("otherFoo2"u8, "foo"u8),
            formData("otherEmpty2"u8, ""u8),
            formData("otherRepeatFoo"u8, "foo"u8),
            formData("otherRepeatFoo"u8, "foo"u8),
            formData("otherRepeatEmpty"u8, ""u8),
            formData("otherRepeatEmpty"u8, ""u8),
            formData("submit"u8, "Submit"u8),
            new(new textproto.MIMEHeader(new map<@string, slice<@string>>{
                ["Content-Type"u8] = new @string[]{"message/external-body; charset=ISO-8859-1; blob-key=AHAZQqG84qllx7HUqO_oou5EvdYQNS3Mbbkb0RjjBoM_Kc1UqEN2ygDxWiyCPulIhpHRPx-VbpB6RX4MrsqhWAi_ZxJ48O9P2cTIACbvATHvg7IgbvZytyGMpL7xO1tlIvgwcM47JNfv_tGhy1XwyEUO8oldjPqg5Q"u8}.slice(),
                ["Content-Disposition"u8] = new @string[]{"form-data; name=file; filename=\"fall.png\""u8}.slice()
            }), "Content-Type: image/png\r\nContent-Length: 232303\r\nX-AppEngine-Upload-Creation: 2012-05-10 23:14:02.715173\r\nContent-MD5: MzRjODU1ZDZhZGU1NmRlOWEwZmMwMDdlODBmZTA0NzA=\r\nContent-Disposition: form-data; name=file; filename=\"fall.png\"\r\n\r\n"u8)
        }.slice()
    ),
    new(
        name: "single empty part, --boundary"u8,
        sep: "abc"u8,
        @in: "--abc\r\nFoo: bar\r\n\r\n--abc--"u8,
        want: new headerBody[]{
            new(new textproto.MIMEHeader(new map<@string, slice<@string>>{["Foo"u8] = new @string[]{"bar"u8}.slice()}), ""u8)
        }.slice()
    ),
    new(
        name: "single empty part, \r\n--boundary"u8,
        sep: "abc"u8,
        @in: "--abc\r\nFoo: bar\r\n\r\n\r\n--abc--"u8,
        want: new headerBody[]{
            new(new textproto.MIMEHeader(new map<@string, slice<@string>>{["Foo"u8] = new @string[]{"bar"u8}.slice()}), ""u8)
        }.slice()
    ),
    new(
        name: "final part empty"u8,
        sep: "abc"u8,
        @in: "--abc\r\nFoo: bar\r\n\r\n--abc\r\nFoo2: bar2\r\n\r\n--abc--"u8,
        want: new headerBody[]{
            new(new textproto.MIMEHeader(new map<@string, slice<@string>>{["Foo"u8] = new @string[]{"bar"u8}.slice()}), ""u8),
            new(new textproto.MIMEHeader(new map<@string, slice<@string>>{["Foo2"u8] = new @string[]{"bar2"u8}.slice()}), ""u8)
        }.slice()
    ),
    new(
        name: "final part empty then crlf"u8,
        sep: "abc"u8,
        @in: "--abc\r\nFoo: bar\r\n\r\n--abc--\r\n"u8,
        want: new headerBody[]{
            new(new textproto.MIMEHeader(new map<@string, slice<@string>>{["Foo"u8] = new @string[]{"bar"u8}.slice()}), ""u8)
        }.slice()
    ),
    new(
        name: "final part empty then lwsp"u8,
        sep: "abc"u8,
        @in: "--abc\r\nFoo: bar\r\n\r\n--abc-- \t"u8,
        want: new headerBody[]{
            new(new textproto.MIMEHeader(new map<@string, slice<@string>>{["Foo"u8] = new @string[]{"bar"u8}.slice()}), ""u8)
        }.slice()
    ),
    new(
        name: "no parts"u8,
        sep: "----WebKitFormBoundaryQfEAfzFOiSemeHfA"u8,
        @in: "------WebKitFormBoundaryQfEAfzFOiSemeHfA--\r\n"u8,
        want: new headerBody[]{}.slice()
    ),
    new(
        name: "fake separator as data"u8,
        sep: "sep"u8,
        @in: "--sep\r\nFoo: bar\r\n\r\n--sepFAKE\r\n--sep--"u8,
        want: new headerBody[]{
            new(new textproto.MIMEHeader(new map<@string, slice<@string>>{["Foo"u8] = new @string[]{"bar"u8}.slice()}), "--sepFAKE"u8)
        }.slice()
    ),
    new(
        name: "boundary with whitespace"u8,
        sep: "sep"u8,
        @in: "--sep \r\nFoo: bar\r\n\r\ntext\r\n--sep--"u8,
        want: new headerBody[]{
            new(new textproto.MIMEHeader(new map<@string, slice<@string>>{["Foo"u8] = new @string[]{"bar"u8}.slice()}), "text"u8)
        }.slice()
    ),
    new(
        name: "leading line"u8,
        sep: "MyBoundary"u8,
        @in: strings.Replace("""
This is a multi-part message.  This line is ignored.
--MyBoundary
foo: bar


--MyBoundary--
"""u8, "\n"u8, "\r\n"u8, -1),
        want: new headerBody[]{
            new(new textproto.MIMEHeader(new map<@string, slice<@string>>{["Foo"u8] = new @string[]{"bar"u8}.slice()}), ""u8)
        }.slice()
    ),
    new(
        name: "issue 10616 minimal"u8,
        sep: "sep"u8,
        @in: "--sep \r\nFoo: bar\r\n\r\n"u8 + "a\r\n"u8 + "--sep_alt\r\n"u8 + "b\r\n"u8 + "\r\n--sep--"u8,
        want: new headerBody[]{
            new(new textproto.MIMEHeader(new map<@string, slice<@string>>{["Foo"u8] = new @string[]{"bar"u8}.slice()}), "a\r\n--sep_alt\r\nb\r\n"u8)
        }.slice()
    ),
    new(
        name: "nested separator prefix is outer separator"u8,
        sep: "----=_NextPart_4c2fbafd7ec4c8bf08034fe724b608d9"u8,
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
"""u8, "\n"u8, "\r\n"u8, -1),
        want: new headerBody[]{
            new(new textproto.MIMEHeader(new map<@string, slice<@string>>{["Content-Type"u8] = new @string[]{@"multipart/alternative; boundary=""----=_NextPart_4c2fbafd7ec4c8bf08034fe724b608d9_alt"""u8}.slice()}),
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
"""u8, "\n"u8, "\r\n"u8, -1)
            )
        }.slice()
    ),
    new(
        name: "peek buffer boundary condition"u8,
        sep: "00ffded004d4dd0fdf945fbdef9d9050cfd6a13a821846299b27fc71b9db"u8,
        @in: strings.Replace("""
--00ffded004d4dd0fdf945fbdef9d9050cfd6a13a821846299b27fc71b9db
Content-Disposition: form-data; name="block"; filename="block"
Content-Type: application/octet-stream


"""u8 + strings.Repeat("A"u8, peekBufferSize - 65) + "\n--00ffded004d4dd0fdf945fbdef9d9050cfd6a13a821846299b27fc71b9db--"u8, "\n"u8, "\r\n"u8, -1),
        want: new headerBody[]{
            new(new textproto.MIMEHeader(new map<@string, slice<@string>>{["Content-Type"u8] = new @string[]{@"application/octet-stream"u8}.slice(), ["Content-Disposition"u8] = new @string[]{@"form-data; name=""block""; filename=""block"""u8}.slice()}),
                strings.Repeat("A"u8, peekBufferSize - 65)
            )
        }.slice()
    ),
    new(
        name: "peek buffer boundary condition"u8,
        sep: "00ffded004d4dd0fdf945fbdef9d9050cfd6a13a821846299b27fc71b9db"u8,
        @in: strings.Replace("""
--00ffded004d4dd0fdf945fbdef9d9050cfd6a13a821846299b27fc71b9db
Content-Disposition: form-data; name="block"; filename="block"
Content-Type: application/octet-stream


"""u8 + strings.Repeat("A"u8, peekBufferSize - 65) + "\n--00ffded004d4dd0fdf945fbdef9d9050cfd6a13a821846299b27fc71b9db--\n"u8, "\n"u8, "\r\n"u8, -1),
        want: new headerBody[]{
            new(new textproto.MIMEHeader(new map<@string, slice<@string>>{["Content-Type"u8] = new @string[]{@"application/octet-stream"u8}.slice(), ["Content-Disposition"u8] = new @string[]{@"form-data; name=""block""; filename=""block"""u8}.slice()}),
                strings.Repeat("A"u8, peekBufferSize - 65)
            )
        }.slice()
    ),
    new(
        name: "peek buffer boundary condition"u8,
        sep: "aaaaaaaaaa00ffded004d4dd0fdf945fbdef9d9050cfd6a13a821846299b27fc71b9db"u8,
        @in: strings.Replace("""
--aaaaaaaaaa00ffded004d4dd0fdf945fbdef9d9050cfd6a13a821846299b27fc71b9db
Content-Disposition: form-data; name="block"; filename="block"
Content-Type: application/octet-stream


"""u8 + strings.Repeat("A"u8, peekBufferSize) + "\n--aaaaaaaaaa00ffded004d4dd0fdf945fbdef9d9050cfd6a13a821846299b27fc71b9db--"u8, "\n"u8, "\r\n"u8, -1),
        want: new headerBody[]{
            new(new textproto.MIMEHeader(new map<@string, slice<@string>>{["Content-Type"u8] = new @string[]{@"application/octet-stream"u8}.slice(), ["Content-Disposition"u8] = new @string[]{@"form-data; name=""block""; filename=""block"""u8}.slice()}),
                strings.Repeat("A"u8, peekBufferSize)
            )
        }.slice()
    ),
    new(
        name: "safeCount off by one"u8,
        sep: "08b84578eabc563dcba967a945cdf0d9f613864a8f4a716f0e81caa71a74"u8,
        @in: strings.Replace("""
--08b84578eabc563dcba967a945cdf0d9f613864a8f4a716f0e81caa71a74
Content-Disposition: form-data; name="myfile"; filename="my-file.txt"
Content-Type: application/octet-stream


"""u8, "\n"u8, "\r\n"u8, -1) + strings.Repeat("A"u8, (nint)peekBufferSize - (len("\n--") + len("08b84578eabc563dcba967a945cdf0d9f613864a8f4a716f0e81caa71a74") + len("\r") + 1)) + strings.Replace("""

--08b84578eabc563dcba967a945cdf0d9f613864a8f4a716f0e81caa71a74
Content-Disposition: form-data; name="key"

val
--08b84578eabc563dcba967a945cdf0d9f613864a8f4a716f0e81caa71a74--

"""u8, "\n"u8, "\r\n"u8, -1),
        want: new headerBody[]{
            new(new textproto.MIMEHeader(new map<@string, slice<@string>>{["Content-Type"u8] = new @string[]{@"application/octet-stream"u8}.slice(), ["Content-Disposition"u8] = new @string[]{@"form-data; name=""myfile""; filename=""my-file.txt"""u8}.slice()}),
                strings.Repeat("A"u8, (nint)peekBufferSize - (len("\n--") + len("08b84578eabc563dcba967a945cdf0d9f613864a8f4a716f0e81caa71a74") + len("\r") + 1))
            ),
            new(new textproto.MIMEHeader(new map<@string, slice<@string>>{["Content-Disposition"u8] = new @string[]{@"form-data; name=""key"""u8}.slice()}),
                "val"u8
            )
        }.slice()
    ),
    new(
        name: "nested separator prefix is outer separator followed by a dash"u8,
        sep: "foo"u8,
        @in: strings.Replace("""
--foo
Content-Type: multipart/alternative; boundary="foo-bar"

--foo-bar

Body
--foo-bar

Body2
--foo-bar--
--foo--
"""u8, "\n"u8, "\r\n"u8, -1),
        want: new headerBody[]{
            new(new textproto.MIMEHeader(new map<@string, slice<@string>>{["Content-Type"u8] = new @string[]{@"multipart/alternative; boundary=""foo-bar"""u8}.slice()}),
                strings.Replace("""
--foo-bar

Body
--foo-bar

Body2
--foo-bar--
"""u8, "\n"u8, "\r\n"u8, -1)
            )
        }.slice()
    ),
    new(
        name: "nested separator prefix is outer separator followed by double dash"u8,
        sep: "foo"u8,
        @in: strings.Replace("""
--foo
Content-Type: multipart/alternative; boundary="foo--"

--foo--

Body

--foo--
"""u8, "\n"u8, "\r\n"u8, -1),
        want: new headerBody[]{
            new(new textproto.MIMEHeader(new map<@string, slice<@string>>{["Content-Type"u8] = new @string[]{@"multipart/alternative; boundary=""foo--"""u8}.slice()}), ""u8)
        }.slice()
    ),
    roundTripParseTest()
}.slice();

public static void TestParse(ж<testing.T> Ꮡt) {
Cases:
    foreach (var (_, tt) in parseTests) {
        var r = NewReader(new multipart_test_package.strings_ReaderжReader(strings.NewReader(tt.@in)), tt.sep);
        var got = new headerBody[]{}.slice();
        while (ᐧ) {
            var (p, err) = r.NextPart();
            if (AreEqual(err, io.EOF)) {
                break;
            }
            if (err != default!) {
                Ꮡt.Errorf("in test %q, NextPart: %v"u8, tt.name, err);
                goto continue_Cases;
            }
            (var pbody, err) = io.ReadAll(new global::go.mime.multipart_package.PartжReader(p));
            if (err != default!) {
                Ꮡt.Errorf("in test %q, error reading part: %v"u8, tt.name, err);
                goto continue_Cases;
            }
            got = append(got, new headerBody((~p).Header, ((@string)pbody)));
        }
        if (!reflect.DeepEqual(tt.want, got)) {
            Ꮡt.Errorf("test %q:\n got: %v\nwant: %v"u8, tt.name, got, tt.want);
            if (len(tt.want) != len(got)){
                Ꮡt.Errorf("test %q: got %d parts, want %d"u8, tt.name, len(got), len(tt.want));
            } else 
            if (len(got) > 1) {
                foreach (var (pi, wantPart) in tt.want) {
                    if (!reflect.DeepEqual(wantPart, got[pi])) {
                        Ꮡt.Errorf("test %q, part %d:\n got: %v\nwant: %v"u8, tt.name, pi, got[pi], wantPart);
                    }
                }
            }
        }
continue_Cases:;
    }
break_Cases:;
}

internal static (slice<headerBody>, error) partsFromReader(ж<global::go.mime.multipart_package.Reader> Ꮡr) {
    var got = new headerBody[]{}.slice();
    while (ᐧ) {
        var (p, err) = Ꮡr.NextPart();
        if (AreEqual(err, io.EOF)) {
            return (got, default!);
        }
        if (err != default!) {
            return (default!, fmt.Errorf("NextPart: %v"u8, err));
        }
        (var pbody, err) = io.ReadAll(new global::go.mime.multipart_package.PartжReader(p));
        if (err != default!) {
            return (default!, fmt.Errorf("error reading part: %v"u8, err));
        }
        got = append(got, new headerBody((~p).Header, ((@string)pbody)));
    }
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static readonly @string keyˢ = "key"u8;

public static void TestParseAllSizes(ж<testing.T> Ꮡt) {
    Ꮡt.Parallel();
    nint maxSize = (5 << (int)(10));
    if (testing.Short()) {
        maxSize = 512;
    }
    ref var buf = ref heap(new bytes.Buffer(), out var Ꮡbuf);
    @string body = strings.Repeat("a"u8, maxSize);
    var bodyb = slice<byte>(body);
    for (nint size = 0; size < maxSize; size++) {
        buf.Reset();
        var w = NewWriter(new multipart_test_package.bytes_BufferжWriter(Ꮡbuf));
        var (part, _) = w.CreateFormField("f"u8);
        part.Write(bodyb.slice(0, size));
        (part, _) = w.CreateFormField(keyˢ);
        part.Write(slice<byte>("val"u8));
        w.Close();
        var r = NewReader(new multipart_test_package.bytes_BufferжReader(Ꮡbuf), w.Boundary());
        var (got, err) = partsFromReader(r);
        if (err != default!) {
            Ꮡt.Errorf("For size %d: %v"u8, size, err);
            continue;
        }
        if (len(got) != 2) {
            Ꮡt.Errorf("For size %d, num parts = %d; want 2"u8, size, len(got));
            continue;
        }
        if (got[0].body != body.slice(0, size)) {
            Ꮡt.Errorf("For size %d, got unexpected len %d: %q"u8, size, len(got[0].body), got[0].body);
        }
    }
}

internal static parseTest roundTripParseTest() {
    var t = new parseTest(
        name: "round trip"u8,
        want: new headerBody[]{
            formData("empty"u8, ""u8),
            formData("lf"u8, "\n"u8),
            formData("cr"u8, "\r"u8),
            formData("crlf"u8, "\r\n"u8),
            formData("foo"u8, "bar"u8)
        }.slice()
    );
    ref var buf = ref heap(new strings.Builder(), out var Ꮡbuf);
    var w = NewWriter(new multipart_test_package.strings_BuilderжWriter(Ꮡbuf));
    foreach (var (_, p) in t.want) {
        var (pw, err) = w.CreatePart(p.header);
        if (err != default!) {
            throw panic(err);
        }
        (_, err) = pw.Write(slice<byte>(p.body));
        if (err != default!) {
            throw panic(err);
        }
    }
    w.Close();
    t.@in = buf.String();
    t.sep = w.Boundary();
    return t;
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static readonly @string multipartBoundaryIsEmptyˢ = "multipart: boundary is empty"u8;

public static void TestNoBoundary(ж<testing.T> Ꮡt) {
    var mr = NewReader(new multipart_test_package.strings_ReaderжReader(strings.NewReader(""u8)), ""u8);
    var (_, err) = mr.NextPart();
    {
        @string got = fmt.Sprint(err);
        @string want = multipartBoundaryIsEmptyˢ; if (got != want) {
            Ꮡt.Errorf("NextPart error = %v; want %v"u8, got, want);
        }
    }
}

} // end multipart_internal_test_package
