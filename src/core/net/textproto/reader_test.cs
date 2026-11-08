// Copyright 2010 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.
namespace go.net;

using bufio = bufio_package;
using bytes = bytes_package;
using io = io_package;
using net = net_package;
using reflect = reflect_package;
using runtime = runtime_package;
using slices = slices_package;
using strings = strings_package;
using sync = sync_package;
using testing = testing_package;
using static go.net.textproto_package;

partial class textproto_internal_test_package {

internal static ж<global::go.net.textproto_package.Reader> reader(@string s) {
    return NewReader(bufio.NewReader(new textproto_internal_test_package.strings_ReaderжReader(strings.NewReader(s))));
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static ref readonly @string line1Line2ˢ => ref line1Line2ˢᵇ;
private static readonly @string line1Line2ˢᵇ = "line1\nline2\n"u8;

public static void TestReadLine(ж<testing.T> t) {
    var r = reader(line1Line2ˢ);
    var (s, err) = r.ReadLine();
    if (s != "line1"u8 || err != nil) {
        t.Fatalf("Line 1: %s, %v", s, err);
    }
    (s, err) = r.ReadLine();
    if (s != "line2"u8 || err != nil) {
        t.Fatalf("Line 2: %s, %v", s, err);
    }
    (s, err) = r.ReadLine();
    if (s != ""u8 || !AreEqual(err, io.EOF)) {
        t.Fatalf("EOF: %s, %v", s, err);
    }
}

public static void TestReadLineLongLine(ж<testing.T> t) {
    @string line = strings.Repeat("12345", 10000);
    var r = reader(line + "\r\n"u8);
    var (s, err) = r.ReadLine();
    if (err != nil) {
        t.Fatalf("Line 1: %v", err);
    }
    if (s != line) {
        t.Fatalf("%v-byte line does not match expected %v-byte line", len(s), len(line));
    }
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static ref readonly @string line1Line2Line3ˢ => ref line1Line2Line3ˢᵇ;
private static readonly @string line1Line2Line3ˢᵇ = "line1\nline\n 2\nline3\n"u8;

public static void TestReadContinuedLine(ж<testing.T> t) {
    var r = reader(line1Line2Line3ˢ);
    var (s, err) = r.ReadContinuedLine();
    if (s != "line1"u8 || err != nil) {
        t.Fatalf("Line 1: %s, %v", s, err);
    }
    (s, err) = r.ReadContinuedLine();
    if (s != "line 2"u8 || err != nil) {
        t.Fatalf("Line 2: %s, %v", s, err);
    }
    (s, err) = r.ReadContinuedLine();
    if (s != "line3"u8 || err != nil) {
        t.Fatalf("Line 3: %s, %v", s, err);
    }
    (s, err) = r.ReadContinuedLine();
    if (s != ""u8 || !AreEqual(err, io.EOF)) {
        t.Fatalf("EOF: %s, %v", s, err);
    }
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static ref readonly @string hi234Bye345NoWayˢ => ref hi234Bye345NoWayˢᵇ;
private static readonly @string hi234Bye345NoWayˢᵇ = "123 hi\n234 bye\n345 no way\n"u8;

public static void TestReadCodeLine(ж<testing.T> t) {
    var r = reader(hi234Bye345NoWayˢ);
    var (code, msg, err) = r.ReadCodeLine(0);
    if (code != 123 || msg != "hi"u8 || err != nil) {
        t.Fatalf("Line 1: %d, %s, %v", code, msg, err);
    }
    (code, msg, err) = r.ReadCodeLine(23);
    if (code != 234 || msg != "bye"u8 || err != nil) {
        t.Fatalf("Line 2: %d, %s, %v", code, msg, err);
    }
    (code, msg, err) = r.ReadCodeLine(346);
    if (code != 345 || msg != "no way"u8 || err == nil) {
        t.Fatalf("Line 3: %d, %s, %v", code, msg, err);
    }
    var (e, ok) = err._<ж<global::go.net.textproto_package.ΔError>>(ᐧ); if (!ok || e.Code != code || e.Msg != msg) {
        t.Fatalf("Line 3: wrong error %v\n", err);
    }
    (code, msg, err) = r.ReadCodeLine(1);
    if (code != 0 || msg != ""u8 || !AreEqual(err, io.EOF)) {
        t.Fatalf("EOF: %d, %s, %v", code, msg, err);
    }
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static ref readonly @string dotlinesFooBarBazQuuxˢ => ref dotlinesFooBarBazQuuxˢᵇ;
private static readonly @string dotlinesFooBarBazQuuxˢᵇ = "dotlines\r\n.foo\r\n..bar\n...baz\nquux\r\n\r\n.\r\nanother\n"u8;

public static void TestReadDotLines(ж<testing.T> t) {
    var r = reader(dotlinesFooBarBazQuuxˢ);
    var (s, err) = r.ReadDotLines();
    var want = slice<@string>(["dotlines", "foo", ".bar", "..baz", "quux", ""]);
    if (!slices.Equal<slice<@string>, @string>(s, want) || err != nil) {
        t.Fatalf("ReadDotLines: %v, %v", s, err);
    }

    (s, err) = r.ReadDotLines();
    want = ["another"];
    if (!slices.Equal<slice<@string>, @string>(s, want) || !AreEqual(err, io.ErrUnexpectedEOF)) {
        t.Fatalf("ReadDotLines2: %v, %v", s, err);
    }
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static ref readonly @string dotlinesFooBarBazQuuxˢ2 => ref dotlinesFooBarBazQuuxˢ2ᵇ;
private static readonly @string dotlinesFooBarBazQuuxˢ2ᵇ = "dotlines\r\n.foo\r\n..bar\n...baz\nquux\r\n\r\n.\r\nanot.her\r\n"u8;

public static void TestReadDotBytes(ж<testing.T> t) {
    var r = reader(dotlinesFooBarBazQuuxˢ2);
    var (b, err) = r.ReadDotBytes();
    var want = slice<byte>("dotlines\nfoo\n.bar\n..baz\nquux\n\n"u8);
    if (!slices.Equal<slice<byte>, byte>(b, want) || err != nil) {
        t.Fatalf("ReadDotBytes: %q, %v", b, err);
    }

    (b, err) = r.ReadDotBytes();
    want = slice<byte>("anot.her\n"u8);
    if (!slices.Equal<slice<byte>, byte>(b, want) || !AreEqual(err, io.ErrUnexpectedEOF)) {
        t.Fatalf("ReadDotBytes2: %q, %v", b, err);
    }
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static ref readonly @string myKeyValue1LongKeyEvenˢ => ref myKeyValue1LongKeyEvenˢᵇ;
private static readonly @string myKeyValue1LongKeyEvenˢᵇ = "my-key: Value 1  \r\nLong-key: Even \n Longer Value\r\nmy-Key: Value 2\r\n\n"u8;

public static void TestReadMIMEHeader(ж<testing.T> t) {
    var r = reader(myKeyValue1LongKeyEvenˢ);
    var (m, err) = r.ReadMIMEHeader();
    var want = new MIMEHeader(new map<@string, slice<@string>>{
        ["My-Key"] = ["Value 1", "Value 2"],
        ["Long-Key"] = ["Even Longer Value"]
    });
    if (!reflect.DeepEqual(m, want) || err != nil) {
        t.Fatalf("ReadMIMEHeader: %v, %v; want %v", m, err, want);
    }
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static ref readonly @string fooBarˢ => ref fooBarˢᵇ;
private static readonly @string fooBarˢᵇ = "Foo: bar\n\n"u8;

public static void TestReadMIMEHeaderSingle(ж<testing.T> t) {
    var r = reader(fooBarˢ);
    var (m, err) = r.ReadMIMEHeader();
    var want = new MIMEHeader(new map<@string, slice<@string>>{["Foo"] = ["bar"]});
    if (!reflect.DeepEqual(m, want) || err != nil) {
        t.Fatalf("ReadMIMEHeader: %v, %v; want %v", m, err, want);
    }
}

internal partial struct TestReaderUpcomingHeaderKeys_type /*dyn*/ {
    internal @string input;
    internal nint want;
}

// TestReaderUpcomingHeaderKeys is testing an internal function, but it's very
// difficult to test well via the external API.
public static void TestReaderUpcomingHeaderKeys(ж<testing.T> t) {
    foreach (var (_, test) in range(slice<TestReaderUpcomingHeaderKeys_type>([new(
        input: "",
        want: 0
    ), new(
        input: "A: v",
        want: 1
    ), new(
        input: "A: v\r\nB: v\r\n",
        want: 2
    ), new(
        input: "A: v\nB: v\n",
        want: 2
    ), new(
        input: "A: v\r\n  continued\r\n  still continued\r\nB: v\r\n\r\n",
        want: 2
    ), new(
        input: "A: v\r\n\r\nB: v\r\nC: v\r\n",
        want: 1
    ), new(
        input: "A: v"u8 + strings.Repeat("\n", 1000),
        want: 1
    )
    ]))) {
        var r = reader(test.input);
        nint got = r.upcomingHeaderKeys();
        if (test.want != got) {
            t.Fatalf("upcomingHeaderKeys(%q): %v; want %v", test.input, got, test.want);
        }
    }
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static ref readonly @string barTest11ˢ => ref barTest11ˢᵇ;
private static readonly @string barTest11ˢᵇ = ": bar\ntest-1: 1\n\n"u8;

public static void TestReadMIMEHeaderNoKey(ж<testing.T> t) {
    var r = reader(barTest11ˢ);
    var (m, err) = r.ReadMIMEHeader();
    var want = new MIMEHeader(new map<@string, slice<@string>>{});
    if (!reflect.DeepEqual(m, want) || err == nil) {
        t.Fatalf("ReadMIMEHeader: %v, %v; want %v", m, err, want);
    }
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static ref readonly @string cookieˢ => ref cookieˢᵇ;
private static readonly @string cookieˢᵇ = "Cookie"u8;

public static void TestLargeReadMIMEHeader(ж<testing.T> t) {
    var data = new slice<byte>(16 * 1024);
    for (nint i = 0; i < len(data); i++) {
        data[i] = (byte)'x';
    }
    @string sdata = (@string)data;
    var r = reader("Cookie: "u8 + sdata + "\r\n\n"u8);
    var (m, err) = r.ReadMIMEHeader();
    if (err != nil) {
        t.Fatalf("ReadMIMEHeader: %v", err);
    }
    @string cookie = m.Get(cookieˢ);
    if (cookie != sdata) {
        t.Fatalf("ReadMIMEHeader: %v bytes, want %v bytes", len(cookie), len(sdata));
    }
}

// TestReadMIMEHeaderNonCompliant checks that we don't normalize headers
// with spaces before colons, and accept spaces in keys.
public static void TestReadMIMEHeaderNonCompliant(ж<testing.T> t) {
    // These invalid headers will be rejected by net/http according to RFC 7230.
    var r = reader("Foo: bar\r\n"u8 + "Content-Language: en\r\n"u8 + "SID : 0\r\n"u8 + "Audio Mode : None\r\n"u8 + "Privilege : 127\r\n\r\n"u8);
    var (m, err) = r.ReadMIMEHeader();
    var want = new MIMEHeader(new map<@string, slice<@string>>{
        ["Foo"] = ["bar"],
        ["Content-Language"] = ["en"],
        ["SID "] = ["0"],
        ["Audio Mode "] = ["None"],
        ["Privilege "] = ["127"]
    });
    if (!reflect.DeepEqual(m, want) || err != nil) {
        t.Fatalf("ReadMIMEHeader =\n%v, %v; want:\n%v", m, err, want);
    }
}

public static void TestReadMIMEHeaderMalformed(ж<testing.T> t) {
    var inputs = slice<@string>([
        "No colon first line\r\nFoo: foo\r\n\r\n",
        " No colon first line with leading space\r\nFoo: foo\r\n\r\n",
        "\tNo colon first line with leading tab\r\nFoo: foo\r\n\r\n",
        " First: line with leading space\r\nFoo: foo\r\n\r\n",
        "\tFirst: line with leading tab\r\nFoo: foo\r\n\r\n",
        "Foo: foo\r\nNo colon second line\r\n\r\n",
        "Foo-\n\tBar: foo\r\n\r\n",
        "Foo-\r\n\tBar: foo\r\n\r\n",
        "Foo\r\n\t: foo\r\n\r\n",
        "Foo-\n\tBar",
        "Foo \tBar: foo\r\n\r\n",
        ": empty key\r\n\r\n"
    ]);
    foreach (var (_, input) in range(inputs)) {
        var r = reader(input);
        var (m, err) = r.ReadMIMEHeader(); if (err == nil || AreEqual(err, io.EOF)) {
            t.Errorf("ReadMIMEHeader(%q) = %v, %v; want nil, err", input, m, err);
        }
    }
}

public static void TestReadMIMEHeaderBytes(ж<testing.T> t) {
    for (nint i = 0; i <= 0xff; i++) {
        @string s = "Foo"u8 + ((@string)(rune)i) + "Bar: foo\r\n\r\n"u8;
        var r = reader(s);
        var wantErr = true;
        switch (ᐧ) {
        case {} when i >= '0' && i <= '9': {
            wantErr = false;
            break;
        }
        case {} when i >= 'a' && i <= 'z': {
            wantErr = false;
            break;
        }
        case {} when i >= 'A' && i <= 'Z': {
            wantErr = false;
            break;
        }
        case {} when i == '!' || i == '#' || i == '$' || i == '%' || i == '&' || i == '\'' || i == '*' || i == '+' || i == '-' || i == '.' || i == '^' || i == '_' || i == '`' || i == '|' || i == '~': {
            wantErr = false;
            break;
        }
        case {} when i is ':': {
            // Special case: "Foo:Bar: foo" is the header "Foo".
            wantErr = false;
            break;
        }
        case {} when i is ' ': {
            wantErr = false;
            break;
        }}
        var (m, err) = r.ReadMIMEHeader();
        if (err != nil != wantErr) {
            t.Errorf("ReadMIMEHeader(%q) = %v, %v; want error=%v", s, m, err, wantErr);
        }
    }
    for (nint i = 0; i <= 0xff; i++) {
        @string s = "Foo: foo"u8 + ((@string)(rune)i) + "bar\r\n\r\n"u8;
        var r = reader(s);
        var wantErr = true;
        switch (ᐧ) {
        case {} when i >= 0x21 && i <= 0x7e: {
            wantErr = false;
            break;
        }
        case {} when i is ' ': {
            wantErr = false;
            break;
        }
        case {} when i is '\t': {
            wantErr = false;
            break;
        }
        case {} when i >= 0x80 && i <= 0xff: {
            wantErr = false;
            break;
        }}
        var (m, err) = r.ReadMIMEHeader();
        if ((err != nil) != wantErr) {
            t.Errorf("ReadMIMEHeader(%q) = %v, %v; want error=%v", s, m, err, wantErr);
        }
    }
}

// Test that continued lines are properly trimmed. Issue 11204.
public static void TestReadMIMEHeaderTrimContinued(ж<testing.T> t) {
    // In this header, \n and \r\n terminated lines are mixed on purpose.
    // We expect each line to be trimmed (prefix and suffix) before being concatenated.
    // Keep the spaces as they are.
    var r = reader(""u8 + // for code formatting purpose.
        "a:\n"u8 + " 0 \r\n"u8 + "b:1 \t\r\n"u8 + "c: 2\r\n"u8 + " 3\t\n"u8 + "  \t 4  \r\n\n"u8);
    var (m, err) = r.ReadMIMEHeader();
    if (err != nil) {
        t.Fatal(err);
    }
    var want = new MIMEHeader(new map<@string, slice<@string>>{
        ["A"] = ["0"],
        ["B"] = ["1"],
        ["C"] = ["2 3 4"]
    });
    if (!reflect.DeepEqual(m, want)) {
        t.Fatalf("ReadMIMEHeader mismatch.\n got: %q\nwant: %q", m, want);
    }
}

// Test that reading a header doesn't overallocate. Issue 58975.
public static void TestReadMIMEHeaderAllocations(ж<testing.T> t) {
    uint64 totalAlloc = default;
    UntypedInt count = 200;
    for (nint i = 0; i < count; i++) {
        var r = reader("A: b\r\n\r\n"u8 + strings.Repeat("\n", 4096));
        ref var m1 = ref heap(new runtime.MemStats(), out var Ꮡm1);
        ref var m2 = ref heap(new runtime.MemStats(), out var Ꮡm2);
        runtime.ReadMemStats(Ꮡm1);
        var (_, err) = r.ReadMIMEHeader();
        if (err != nil) {
            t.Fatalf("ReadMIMEHeader: %v", err);
        }
        runtime.ReadMemStats(Ꮡm2);
        totalAlloc += m2.TotalAlloc - m1.TotalAlloc;
    }
    // 32k is large and we actually allocate substantially less,
    // but prior to the fix for #58975 we allocated ~400k in this case.
    var (got, want) = (totalAlloc / (uint64)count, (uint64)32768); if (got > want) {
        t.Fatalf("ReadMIMEHeader allocated %v bytes, want < %v", got, want);
    }
}

internal partial struct testReader {
    internal Func<slice<byte>, (nint, error)> read;
}

internal static (nint n, error err) Read(this testReader r, slice<byte> p) { return r.read(p); }

public static void TestReadMIMEHeaderShortLimitLongLine(ж<testing.T> t) {
    // Small limit, long first line.
    var r = NewReader(bufio.NewReader(
        io.MultiReader(
            new textproto_internal_test_package.strings_ReaderжReader(strings.NewReader("K:")),
            new testReader(
                read: (slice<byte> p) => {
                    foreach (var i in range(len(p))) {
                        p[i] = (byte)' ';
                    }
                    return (len(p), default);
                }
            ))));
    var (_, err) = readMIMEHeader(r, 1, 1);
    if (!AreEqual(err, errMessageTooLarge)) {
        t.Fatalf("readMIMEHeader = %v, want errMessageTooLarge", err);
    }
}

internal partial struct readResponseTest {
    internal @string @in;
    internal nint inCode;
    internal nint wantCode;
    internal @string wantMsg;
}

internal static slice<readResponseTest> readResponseTests = [
    new("230-Anonymous access granted, restrictions apply\n"u8 + "Read the file README.txt,\n"u8 + "230  please"u8,
        23,
        230,
        "Anonymous access granted, restrictions apply\nRead the file README.txt,\n please"
    ),

    new("230 Anonymous access granted, restrictions apply\n",
        23,
        230,
        "Anonymous access granted, restrictions apply"
    ),

    new("400-A\n400-B\n400 C",
        4,
        400,
        "A\nB\nC"
    ),

    new("400-A\r\n400-B\r\n400 C\r\n",
        4,
        400,
        "A\nB\nC"
    )
];

// See https://www.ietf.org/rfc/rfc959.txt page 36.
public static void TestRFC959Lines(ж<testing.T> t) {
    foreach (var (i, tt) in range(readResponseTests)) {
        var r = reader(tt.@in + "\nFOLLOWING DATA"u8);
        var (code, msg, err) = r.ReadResponse(tt.inCode);
        if (err != nil) {
            t.Errorf("#%d: ReadResponse: %v", i, err);
            continue;
        }
        if (code != tt.wantCode) {
            t.Errorf("#%d: code=%d, want %d", i, code, tt.wantCode);
        }
        if (msg != tt.wantMsg) {
            t.Errorf("#%d: msg=%q, want %q", i, msg, tt.wantMsg);
        }
    }
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static ref readonly @string theEmailAccountThatYouˢ => ref theEmailAccountThatYouˢᵇ;
private static readonly @string theEmailAccountThatYouˢᵇ = @"550 ""5.1.1 The email account that you tried to reach does not exist. Please try\n5.1.1 double-checking the recipient's email address for typos or\n5.1.1 unnecessary spaces. Learn more at\nUnexpected but legal text!\n5.1.1 https://support.google.com/mail/answer/6596 h20si25154304pfd.166 - gsmtp"""u8;

// Test that multi-line errors are appropriately and fully read. Issue 10230.
public static void TestReadMultiLineError(ж<testing.T> t) {
    var r = reader("550-5.1.1 The email account that you tried to reach does not exist. Please try\n"u8 + "550-5.1.1 double-checking the recipient's email address for typos or\n"u8 + "550-5.1.1 unnecessary spaces. Learn more at\n"u8 + "Unexpected but legal text!\n"u8 + "550 5.1.1 https://support.google.com/mail/answer/6596 h20si25154304pfd.166 - gsmtp\n"u8);

    @string wantMsg = "5.1.1 The email account that you tried to reach does not exist. Please try\n"u8 + "5.1.1 double-checking the recipient's email address for typos or\n"u8 + "5.1.1 unnecessary spaces. Learn more at\n"u8 + "Unexpected but legal text!\n"u8 + "5.1.1 https://support.google.com/mail/answer/6596 h20si25154304pfd.166 - gsmtp"u8;

    @string wantError = theEmailAccountThatYouˢ;

    var (code, msg, err) = r.ReadResponse(250);
    if (err == nil) {
        t.Errorf("ReadResponse: no error, want error");
    }
    if (code != 550) {
        t.Errorf("ReadResponse: code=%d, want %d", code, (nint)550);
    }
    if (msg != wantMsg) {
        t.Errorf("ReadResponse: msg=%q, want %q", msg, wantMsg);
    }
    if (err != nil && err.Error() != wantError) {
        t.Errorf("ReadResponse: error=%q, want %q", err.Error(), wantError);
    }
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static ref readonly @string contentLengthˢ => ref contentLengthˢᵇ;
private static readonly @string contentLengthˢᵇ = "Content-Length"u8;

public static void TestCommonHeaders(ж<testing.T> t) {
    ᏑcommonHeaderOnce.Do(initCommonHeader);
    foreach (var (h, _) in commonHeader) {
        if (h != CanonicalMIMEHeaderKey(h)) {
            t.Errorf("Non-canonical header %q in commonHeader", h);
        }
    }
    var b = slice<byte>("content-Length"u8);
    @string want = contentLengthˢ;
    var bʗ1 = b;
    var n = testing.AllocsPerRun(200, () => {
        var (x, _) = canonicalMIMEHeaderKey(bʗ1); if (x != want) {
            t.Fatalf("canonicalMIMEHeaderKey(%q) = %q; want %q", bʗ1, x, want);
        }
    });
    if (n > 0D) {
        t.Errorf("canonicalMIMEHeaderKey allocs = %v; want 0", n);
    }
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static readonly object canonicalMIMEHeaderKeyˢ = (@string)"CanonicalMIMEHeaderKey should initialize commonHeader"u8;

public static partial void TestIssue46363(ж<testing.T> t) {
    // Regression test for data race reported in issue 46363:
    // ReadMIMEHeader reads commonHeader before commonHeader has been initialized.
    // Run this test with the race detector enabled to catch the reported data race.

    // Reset commonHeaderOnce, so that commonHeader will have to be initialized
    commonHeaderOnce = new sync.Once();
    commonHeader = default;

    // Test for data race by calling ReadMIMEHeader and CanonicalMIMEHeaderKey concurrently

    // Send MIME header over net.Conn
    var (r, w) = net.Pipe();
    var rʗ1 = r;
    goǃ(() => {
        // ReadMIMEHeader calls canonicalMIMEHeaderKey, which reads from commonHeader
        NewConn(new textproto_internal_test_package.net_ConnᴠReadWriteCloser(rʗ1)).of(global::go.net.textproto_package.Conn.ᏑReader).ReadMIMEHeader();
    });
    w.Write(slice<byte>("A: 1\r\nB: 2\r\nC: 3\r\n\r\n"u8));

    // CanonicalMIMEHeaderKey calls commonHeaderOnce.Do(initCommonHeader) which initializes commonHeader
    CanonicalMIMEHeaderKey("a");

    if (commonHeader == nil) {
        t.Fatal(canonicalMIMEHeaderKeyˢ);
    }
}

internal static @string clientHeaders = strings.Replace("""
Host: golang.org
Connection: keep-alive
Cache-Control: max-age=0
Accept: application/xml,application/xhtml+xml,text/html;q=0.9,text/plain;q=0.8,image/png,*/*;q=0.5
User-Agent: Mozilla/5.0 (X11; U; Linux x86_64; en-US) AppleWebKit/534.3 (KHTML, like Gecko) Chrome/6.0.472.63 Safari/534.3
Accept-Encoding: gzip,deflate,sdch
Accept-Language: en-US,en;q=0.8,fr-CH;q=0.6
Accept-Charset: ISO-8859-1,utf-8;q=0.7,*;q=0.3
COOKIE: __utma=000000000.0000000000.0000000000.0000000000.0000000000.00; __utmb=000000000.0.00.0000000000; __utmc=000000000; __utmz=000000000.0000000000.00.0.utmcsr=code.google.com|utmccn=(referral)|utmcmd=referral|utmcct=/p/go/issues/detail
Non-Interned: test


"""u8.Raw(), "\n", "\r\n", -1);

internal static @string serverHeaders = strings.Replace("""
Content-Type: text/html; charset=utf-8
Content-Encoding: gzip
Date: Thu, 27 Sep 2012 09:03:33 GMT
Server: Google Frontend
Cache-Control: private
Content-Length: 2298
VIA: 1.1 proxy.example.com:80 (XXX/n.n.n-nnn)
Connection: Close
Non-Interned: test


"""u8.Raw(), "\n", "\r\n", -1);

internal partial struct BenchmarkReadMIMEHeader_type /*dyn*/ {
    internal @string name;
    internal @string headers;
}

public static void BenchmarkReadMIMEHeader(ж<testing.B> Ꮡb) {
    ref var b = ref Ꮡb.DerefOrNull();

    b.ReportAllocs();
    foreach (var (_, vᴛ1) in range(slice<BenchmarkReadMIMEHeader_type>([
        new("client_headers", clientHeaders),
        new("server_headers", serverHeaders)
    ]))) {
        ref var set = ref heap(new BenchmarkReadMIMEHeader_type(), out var Ꮡset);
        set = vᴛ1;

        var setʗ1 = set;
        Ꮡb.Run(set.name, (ж<testing.B> bΔ1) => {
            ref var buf = ref heap(new bytes.Buffer(), out var Ꮡbuf);
            var br = bufio.NewReader(new textproto_internal_test_package.bytes_BufferжReader(Ꮡbuf));
            var r = NewReader(br);

            for (nint i = 0; i < (~bΔ1).N; i++) {
                buf.WriteString(setʗ1.headers);
                var (_, err) = r.ReadMIMEHeader(); if (err != nil) {
                    bΔ1.Fatal(err);
                }
            }
        });
    }
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static ref readonly @string uncommonHeaderForˢ => ref uncommonHeaderForˢᵇ;
private static readonly @string uncommonHeaderForˢᵇ = "uncommon-header-for-benchmark: foo\r\n\r\n"u8;
internal static ref readonly @string uncommonHeaderForˢ2 => ref uncommonHeaderForˢ2ᵇ;
private static readonly @string uncommonHeaderForˢ2ᵇ = "Uncommon-Header-For-Benchmark"u8;
internal static readonly object missingResultHeaderˢ = (@string)"Missing result header."u8;

public static void BenchmarkUncommon(ж<testing.B> Ꮡb) {
    ref var b = ref Ꮡb.DerefOrNull();

    b.ReportAllocs();
    ref var buf = ref heap(new bytes.Buffer(), out var Ꮡbuf);
    var br = bufio.NewReader(new textproto_internal_test_package.bytes_BufferжReader(Ꮡbuf));
    var r = NewReader(br);
    for (nint i = 0; i < b.N; i++) {
        buf.WriteString(uncommonHeaderForˢ);
        var (h, err) = r.ReadMIMEHeader();
        if (err != nil) {
            Ꮡb.Fatal(err);
        }
        var (_, ok) = h[uncommonHeaderForˢ2, ꟷ]; if (!ok) {
            Ꮡb.Fatal(missingResultHeaderˢ);
        }
    }
}

} // end textproto_internal_test_package
