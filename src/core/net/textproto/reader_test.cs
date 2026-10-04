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
using MethodImplAttribute = global::System.Runtime.CompilerServices.MethodImplAttribute;
using MethodImplOptions = global::System.Runtime.CompilerServices.MethodImplOptions;
using static go.net.textproto_package;

partial class textproto_internal_test_package {

internal static ж<global::go.net.textproto_package.Reader> reader(@string s) {
    return NewReader(bufio.NewReader(new textproto_internal_test_package.strings_ReaderжReader(strings.NewReader(s))));
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static readonly @string line1Line2ˢ = "line1\nline2\n"u8;

public static void TestReadLine(ж<testing.T> Ꮡt) {
    var r = reader(line1Line2ˢ);
    var (s, err) = r.ReadLine();
    if (s != "line1"u8 || err != default!) {
        Ꮡt.Fatalf("Line 1: %s, %v"u8, s, err);
    }
    (s, err) = r.ReadLine();
    if (s != "line2"u8 || err != default!) {
        Ꮡt.Fatalf("Line 2: %s, %v"u8, s, err);
    }
    (s, err) = r.ReadLine();
    if (s != ""u8 || !AreEqual(err, io.EOF)) {
        Ꮡt.Fatalf("EOF: %s, %v"u8, s, err);
    }
}

public static void TestReadLineLongLine(ж<testing.T> Ꮡt) {
    @string line = strings.Repeat("12345"u8, 10000);
    var r = reader(line + "\r\n"u8);
    var (s, err) = r.ReadLine();
    if (err != default!) {
        Ꮡt.Fatalf("Line 1: %v"u8, err);
    }
    if (s != line) {
        Ꮡt.Fatalf("%v-byte line does not match expected %v-byte line"u8, len(s), len(line));
    }
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static readonly @string line1Line2Line3ˢ = "line1\nline\n 2\nline3\n"u8;

public static void TestReadContinuedLine(ж<testing.T> Ꮡt) {
    var r = reader(line1Line2Line3ˢ);
    var (s, err) = r.ReadContinuedLine();
    if (s != "line1"u8 || err != default!) {
        Ꮡt.Fatalf("Line 1: %s, %v"u8, s, err);
    }
    (s, err) = r.ReadContinuedLine();
    if (s != "line 2"u8 || err != default!) {
        Ꮡt.Fatalf("Line 2: %s, %v"u8, s, err);
    }
    (s, err) = r.ReadContinuedLine();
    if (s != "line3"u8 || err != default!) {
        Ꮡt.Fatalf("Line 3: %s, %v"u8, s, err);
    }
    (s, err) = r.ReadContinuedLine();
    if (s != ""u8 || !AreEqual(err, io.EOF)) {
        Ꮡt.Fatalf("EOF: %s, %v"u8, s, err);
    }
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static readonly @string hi234Bye345NoWayˢ = "123 hi\n234 bye\n345 no way\n"u8;

public static void TestReadCodeLine(ж<testing.T> Ꮡt) {
    var r = reader(hi234Bye345NoWayˢ);
    var (code, msg, err) = r.ReadCodeLine(0);
    if (code != 123 || msg != "hi"u8 || err != default!) {
        Ꮡt.Fatalf("Line 1: %d, %s, %v"u8, code, msg, err);
    }
    (code, msg, err) = r.ReadCodeLine(23);
    if (code != 234 || msg != "bye"u8 || err != default!) {
        Ꮡt.Fatalf("Line 2: %d, %s, %v"u8, code, msg, err);
    }
    (code, msg, err) = r.ReadCodeLine(346);
    if (code != 345 || msg != "no way"u8 || err == default!) {
        Ꮡt.Fatalf("Line 3: %d, %s, %v"u8, code, msg, err);
    }
    {
        var (e, ok) = err._<ж<global::go.net.textproto_package.ΔError>>(ᐧ); if (!ok || (~e).Code != code || (~e).Msg != msg) {
            Ꮡt.Fatalf("Line 3: wrong error %v\n"u8, err);
        }
    }
    (code, msg, err) = r.ReadCodeLine(1);
    if (code != 0 || msg != ""u8 || !AreEqual(err, io.EOF)) {
        Ꮡt.Fatalf("EOF: %d, %s, %v"u8, code, msg, err);
    }
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static readonly @string dotlinesFooBarBazQuuxˢ = "dotlines\r\n.foo\r\n..bar\n...baz\nquux\r\n\r\n.\r\nanother\n"u8;

public static void TestReadDotLines(ж<testing.T> Ꮡt) {
    var r = reader(dotlinesFooBarBazQuuxˢ);
    var (s, err) = r.ReadDotLines();
    var want = new @string[]{"dotlines"u8, "foo"u8, ".bar"u8, "..baz"u8, "quux"u8, ""u8}.slice();
    if (!slices.Equal<slice<@string>, @string>(s, want) || err != default!) {
        Ꮡt.Fatalf("ReadDotLines: %v, %v"u8, s, err);
    }
    (s, err) = r.ReadDotLines();
    want = new @string[]{"another"u8}.slice();
    if (!slices.Equal<slice<@string>, @string>(s, want) || !AreEqual(err, io.ErrUnexpectedEOF)) {
        Ꮡt.Fatalf("ReadDotLines2: %v, %v"u8, s, err);
    }
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static readonly @string dotlinesFooBarBazQuuxˢ2 = "dotlines\r\n.foo\r\n..bar\n...baz\nquux\r\n\r\n.\r\nanot.her\r\n"u8;

public static void TestReadDotBytes(ж<testing.T> Ꮡt) {
    var r = reader(dotlinesFooBarBazQuuxˢ2);
    var (b, err) = r.ReadDotBytes();
    var want = slice<byte>("dotlines\nfoo\n.bar\n..baz\nquux\n\n"u8);
    if (!slices.Equal<slice<byte>, byte>(b, want) || err != default!) {
        Ꮡt.Fatalf("ReadDotBytes: %q, %v"u8, b, err);
    }
    (b, err) = r.ReadDotBytes();
    want = slice<byte>("anot.her\n"u8);
    if (!slices.Equal<slice<byte>, byte>(b, want) || !AreEqual(err, io.ErrUnexpectedEOF)) {
        Ꮡt.Fatalf("ReadDotBytes2: %q, %v"u8, b, err);
    }
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static readonly @string myKeyValue1LongKeyEvenˢ = "my-key: Value 1  \r\nLong-key: Even \n Longer Value\r\nmy-Key: Value 2\r\n\n"u8;

public static void TestReadMIMEHeader(ж<testing.T> Ꮡt) {
    var r = reader(myKeyValue1LongKeyEvenˢ);
    var (m, err) = r.ReadMIMEHeader();
    var want = new MIMEHeader(new map<@string, slice<@string>>{
        ["My-Key"u8] = new @string[]{"Value 1"u8, "Value 2"u8}.slice(),
        ["Long-Key"u8] = new @string[]{"Even Longer Value"u8}.slice()
    });
    if (!reflect.DeepEqual(m, want) || err != default!) {
        Ꮡt.Fatalf("ReadMIMEHeader: %v, %v; want %v"u8, m, err, want);
    }
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static readonly @string fooBarˢ = "Foo: bar\n\n"u8;

public static void TestReadMIMEHeaderSingle(ж<testing.T> Ꮡt) {
    var r = reader(fooBarˢ);
    var (m, err) = r.ReadMIMEHeader();
    var want = new MIMEHeader(new map<@string, slice<@string>>{["Foo"u8] = new @string[]{"bar"u8}.slice()});
    if (!reflect.DeepEqual(m, want) || err != default!) {
        Ꮡt.Fatalf("ReadMIMEHeader: %v, %v; want %v"u8, m, err, want);
    }
}

[GoType("dyn")] internal partial struct TestReaderUpcomingHeaderKeys_type {
    internal @string input;
    internal nint want;
}

// TestReaderUpcomingHeaderKeys is testing an internal function, but it's very
// difficult to test well via the external API.
public static void TestReaderUpcomingHeaderKeys(ж<testing.T> Ꮡt) {
    foreach (var (_, test) in new TestReaderUpcomingHeaderKeys_type[]{new(
        input: ""u8,
        want: 0
    ), new(
        input: "A: v"u8,
        want: 1
    ), new(
        input: "A: v\r\nB: v\r\n"u8,
        want: 2
    ), new(
        input: "A: v\nB: v\n"u8,
        want: 2
    ), new(
        input: "A: v\r\n  continued\r\n  still continued\r\nB: v\r\n\r\n"u8,
        want: 2
    ), new(
        input: "A: v\r\n\r\nB: v\r\nC: v\r\n"u8,
        want: 1
    ), new(
        input: "A: v"u8 + strings.Repeat("\n"u8, 1000),
        want: 1
    )
    }.slice()) {
        var r = reader(test.input);
        nint got = r.upcomingHeaderKeys();
        if (test.want != got) {
            Ꮡt.Fatalf("upcomingHeaderKeys(%q): %v; want %v"u8, test.input, got, test.want);
        }
    }
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static readonly @string barTest11ˢ = ": bar\ntest-1: 1\n\n"u8;

public static void TestReadMIMEHeaderNoKey(ж<testing.T> Ꮡt) {
    var r = reader(barTest11ˢ);
    var (m, err) = r.ReadMIMEHeader();
    var want = new MIMEHeader(new map<@string, slice<@string>>{});
    if (!reflect.DeepEqual(m, want) || err == default!) {
        Ꮡt.Fatalf("ReadMIMEHeader: %v, %v; want %v"u8, m, err, want);
    }
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static readonly @string cookieˢ = "Cookie"u8;

public static void TestLargeReadMIMEHeader(ж<testing.T> Ꮡt) {
    var data = new slice<byte>(16 * 1024);
    for (nint i = 0; i < len(data); i++) {
        data[i] = (rune)'x';
    }
    @string sdata = ((@string)data);
    var r = reader("Cookie: "u8 + sdata + "\r\n\n"u8);
    var (m, err) = r.ReadMIMEHeader();
    if (err != default!) {
        Ꮡt.Fatalf("ReadMIMEHeader: %v"u8, err);
    }
    @string cookie = m.Get(cookieˢ);
    if (cookie != sdata) {
        Ꮡt.Fatalf("ReadMIMEHeader: %v bytes, want %v bytes"u8, len(cookie), len(sdata));
    }
}

// TestReadMIMEHeaderNonCompliant checks that we don't normalize headers
// with spaces before colons, and accept spaces in keys.
public static void TestReadMIMEHeaderNonCompliant(ж<testing.T> Ꮡt) {
    // These invalid headers will be rejected by net/http according to RFC 7230.
    var r = reader("Foo: bar\r\n"u8 + "Content-Language: en\r\n"u8 + "SID : 0\r\n"u8 + "Audio Mode : None\r\n"u8 + "Privilege : 127\r\n\r\n"u8);
    var (m, err) = r.ReadMIMEHeader();
    var want = new MIMEHeader(new map<@string, slice<@string>>{
        ["Foo"u8] = new @string[]{"bar"u8}.slice(),
        ["Content-Language"u8] = new @string[]{"en"u8}.slice(),
        ["SID "u8] = new @string[]{"0"u8}.slice(),
        ["Audio Mode "u8] = new @string[]{"None"u8}.slice(),
        ["Privilege "u8] = new @string[]{"127"u8}.slice()
    });
    if (!reflect.DeepEqual(m, want) || err != default!) {
        Ꮡt.Fatalf("ReadMIMEHeader =\n%v, %v; want:\n%v"u8, m, err, want);
    }
}

public static void TestReadMIMEHeaderMalformed(ж<testing.T> Ꮡt) {
    ref var t = ref Ꮡt.DerefOrNull();

    var inputs = new @string[]{
        "No colon first line\r\nFoo: foo\r\n\r\n"u8,
        " No colon first line with leading space\r\nFoo: foo\r\n\r\n"u8,
        "\tNo colon first line with leading tab\r\nFoo: foo\r\n\r\n"u8,
        " First: line with leading space\r\nFoo: foo\r\n\r\n"u8,
        "\tFirst: line with leading tab\r\nFoo: foo\r\n\r\n"u8,
        "Foo: foo\r\nNo colon second line\r\n\r\n"u8,
        "Foo-\n\tBar: foo\r\n\r\n"u8,
        "Foo-\r\n\tBar: foo\r\n\r\n"u8,
        "Foo\r\n\t: foo\r\n\r\n"u8,
        "Foo-\n\tBar"u8,
        "Foo \tBar: foo\r\n\r\n"u8,
        ": empty key\r\n\r\n"u8
    }.slice();
    foreach (var (_, input) in inputs) {
        var r = reader(input);
        {
            var (m, err) = r.ReadMIMEHeader(); if (err == default! || AreEqual(err, io.EOF)) {
                Ꮡt.Errorf("ReadMIMEHeader(%q) = %v, %v; want nil, err"u8, input, m, err);
            }
        }
    }
}

public static void TestReadMIMEHeaderBytes(ж<testing.T> Ꮡt) {
    ref var t = ref Ꮡt.DerefOrNull();

    for (nint i = 0; i <= 0xff; i++) {
        @string s = "Foo"u8 + ((@string)(rune)i) + "Bar: foo\r\n\r\n"u8;
        var r = reader(s);
        var wantErr = true;
        switch (ᐧ) {
        case {} when i >= (rune)'0' && i <= (rune)'9': {
            wantErr = false;
            break;
        }
        case {} when i >= (rune)'a' && i <= (rune)'z': {
            wantErr = false;
            break;
        }
        case {} when i >= (rune)'A' && i <= (rune)'Z': {
            wantErr = false;
            break;
        }
        case {} when i == (rune)'!' || i == (rune)'#' || i == (rune)'$' || i == (rune)'%' || i == (rune)'&' || i == (rune)'\'' || i == (rune)'*' || i == (rune)'+' || i == (rune)'-' || i == (rune)'.' || i == (rune)'^' || i == (rune)'_' || i == (rune)'`' || i == (rune)'|' || i == (rune)'~': {
            wantErr = false;
            break;
        }
        case {} when i is (rune)':': {
            wantErr = false;
            break;
        }
        case {} when i is (rune)' ': {
            wantErr = false;
            break;
        }}

        // Special case: "Foo:Bar: foo" is the header "Foo".
        var (m, err) = r.ReadMIMEHeader();
        if (err != default! != wantErr) {
            Ꮡt.Errorf("ReadMIMEHeader(%q) = %v, %v; want error=%v"u8, s, m, err, wantErr);
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
        case {} when i is (rune)' ': {
            wantErr = false;
            break;
        }
        case {} when i is (rune)'\t': {
            wantErr = false;
            break;
        }
        case {} when i >= 0x80 && i <= 0xff: {
            wantErr = false;
            break;
        }}

        var (m, err) = r.ReadMIMEHeader();
        if ((err != default!) != wantErr) {
            Ꮡt.Errorf("ReadMIMEHeader(%q) = %v, %v; want error=%v"u8, s, m, err, wantErr);
        }
    }
}

// Test that continued lines are properly trimmed. Issue 11204.
public static void TestReadMIMEHeaderTrimContinued(ж<testing.T> Ꮡt) {
    ref var t = ref Ꮡt.DerefOrNull();

    // In this header, \n and \r\n terminated lines are mixed on purpose.
    // We expect each line to be trimmed (prefix and suffix) before being concatenated.
    // Keep the spaces as they are.
    var r = reader(""u8 + "a:\n"u8 + " 0 \r\n"u8 + "b:1 \t\r\n"u8 + "c: 2\r\n"u8 + " 3\t\n"u8 + "  \t 4  \r\n\n"u8);
    // for code formatting purpose.
    var (m, err) = r.ReadMIMEHeader();
    if (err != default!) {
        Ꮡt.Fatal(err);
    }
    var want = new MIMEHeader(new map<@string, slice<@string>>{
        ["A"u8] = new @string[]{"0"u8}.slice(),
        ["B"u8] = new @string[]{"1"u8}.slice(),
        ["C"u8] = new @string[]{"2 3 4"u8}.slice()
    });
    if (!reflect.DeepEqual(m, want)) {
        Ꮡt.Fatalf("ReadMIMEHeader mismatch.\n got: %q\nwant: %q"u8, m, want);
    }
}

// Test that reading a header doesn't overallocate. Issue 58975.
public static void TestReadMIMEHeaderAllocations(ж<testing.T> Ꮡt) {
    uint64 totalAlloc = default!;
    UntypedInt count = 200;
    for (nint i = 0; i < count; i++) {
        var r = reader("A: b\r\n\r\n"u8 + strings.Repeat("\n"u8, 4096));
        ref var m1 = ref heap(new runtime.MemStats(), out var Ꮡm1);
        ref var m2 = ref heap(new runtime.MemStats(), out var Ꮡm2);
        runtime.ReadMemStats(Ꮡm1);
        var (_, err) = r.ReadMIMEHeader();
        if (err != default!) {
            Ꮡt.Fatalf("ReadMIMEHeader: %v"u8, err);
        }
        runtime.ReadMemStats(Ꮡm2);
        totalAlloc += m2.TotalAlloc - m1.TotalAlloc;
    }
    // 32k is large and we actually allocate substantially less,
    // but prior to the fix for #58975 we allocated ~400k in this case.
    {
        var (got, want) = (totalAlloc / (uint64)count, (uint64)32768); if (got > want) {
            Ꮡt.Fatalf("ReadMIMEHeader allocated %v bytes, want < %v"u8, got, want);
        }
    }
}

[GoType] internal partial struct readResponseTest {
    internal @string @in;
    internal nint inCode;
    internal nint wantCode;
    internal @string wantMsg;
}

internal static slice<readResponseTest> readResponseTests = new readResponseTest[]{
    new("230-Anonymous access granted, restrictions apply\n"u8 + "Read the file README.txt,\n"u8 + "230  please"u8,
        23,
        230,
        "Anonymous access granted, restrictions apply\nRead the file README.txt,\n please"u8
    ),
    new("230 Anonymous access granted, restrictions apply\n"u8,
        23,
        230,
        "Anonymous access granted, restrictions apply"u8
    ),
    new("400-A\n400-B\n400 C"u8,
        4,
        400,
        "A\nB\nC"u8
    ),
    new("400-A\r\n400-B\r\n400 C\r\n"u8,
        4,
        400,
        "A\nB\nC"u8
    )
}.slice();

// See https://www.ietf.org/rfc/rfc959.txt page 36.
public static void TestRFC959Lines(ж<testing.T> Ꮡt) {
    foreach (var (i, tt) in readResponseTests) {
        var r = reader(tt.@in + "\nFOLLOWING DATA"u8);
        var (code, msg, err) = r.ReadResponse(tt.inCode);
        if (err != default!) {
            Ꮡt.Errorf("#%d: ReadResponse: %v"u8, i, err);
            continue;
        }
        if (code != tt.wantCode) {
            Ꮡt.Errorf("#%d: code=%d, want %d"u8, i, code, tt.wantCode);
        }
        if (msg != tt.wantMsg) {
            Ꮡt.Errorf("#%d: msg=%q, want %q"u8, i, msg, tt.wantMsg);
        }
    }
}

// Test that multi-line errors are appropriately and fully read. Issue 10230.
public static void TestReadMultiLineError(ж<testing.T> Ꮡt) {
    var r = reader("550-5.1.1 The email account that you tried to reach does not exist. Please try\n"u8 + "550-5.1.1 double-checking the recipient's email address for typos or\n"u8 + "550-5.1.1 unnecessary spaces. Learn more at\n"u8 + "Unexpected but legal text!\n"u8 + "550 5.1.1 https://support.google.com/mail/answer/6596 h20si25154304pfd.166 - gsmtp\n"u8);
    @string wantMsg = "5.1.1 The email account that you tried to reach does not exist. Please try\n"u8 + "5.1.1 double-checking the recipient's email address for typos or\n"u8 + "5.1.1 unnecessary spaces. Learn more at\n"u8 + "Unexpected but legal text!\n"u8 + "5.1.1 https://support.google.com/mail/answer/6596 h20si25154304pfd.166 - gsmtp"u8;
    var (code, msg, err) = r.ReadResponse(250);
    if (err == default!) {
        Ꮡt.Errorf("ReadResponse: no error, want error"u8);
    }
    if (code != 550) {
        Ꮡt.Errorf("ReadResponse: code=%d, want %d"u8, code, (nint)(550));
    }
    if (msg != wantMsg) {
        Ꮡt.Errorf("ReadResponse: msg=%q, want %q"u8, msg, wantMsg);
    }
    if (err != default! && err.Error() != "550 "u8 + wantMsg) {
        Ꮡt.Errorf("ReadResponse: error=%q, want %q"u8, err.Error(), "550 " + wantMsg);
    }
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static readonly @string contentLengthˢ = "Content-Length"u8;

public static void TestCommonHeaders(ж<testing.T> Ꮡt) {
    ᏑcommonHeaderOnce.Do(initCommonHeader);
    foreach (var (h, _) in commonHeader) {
        if (h != CanonicalMIMEHeaderKey(h)) {
            Ꮡt.Errorf("Non-canonical header %q in commonHeader"u8, h);
        }
    }
    var b = slice<byte>("content-Length"u8);
    @string want = contentLengthˢ;
    var bʗ1 = b;
    var n = testing.AllocsPerRun(200, () => {
        {
            var (x, _) = canonicalMIMEHeaderKey(bʗ1); if (x != want) {
                Ꮡt.Fatalf("canonicalMIMEHeaderKey(%q) = %q; want %q"u8, bʗ1, x, want);
            }
        }
    });
    if (n > 0D) {
        Ꮡt.Errorf("canonicalMIMEHeaderKey allocs = %v; want 0"u8, n);
    }
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static readonly object canonicalMIMEHeaderKeyˢ = (@string)"CanonicalMIMEHeaderKey should initialize commonHeader"u8;

[MethodImpl(MethodImplOptions.NoInlining)] public static void TestIssue46363(ж<testing.T> Ꮡt) {
    // Regression test for data race reported in issue 46363:
    // ReadMIMEHeader reads commonHeader before commonHeader has been initialized.
    // Run this test with the race detector enabled to catch the reported data race.
    // Reset commonHeaderOnce, so that commonHeader will have to be initialized
    commonHeaderOnce = new sync.Once(nil);
    commonHeader = default!;
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
    CanonicalMIMEHeaderKey("a"u8);
    if (commonHeader == default!) {
        Ꮡt.Fatal(canonicalMIMEHeaderKeyˢ);
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


"""u8, "\n"u8, "\r\n"u8, -1);

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


"""u8, "\n"u8, "\r\n"u8, -1);

[GoType("dyn")] internal partial struct BenchmarkReadMIMEHeader_type {
    internal @string name;
    internal @string headers;
}

public static void BenchmarkReadMIMEHeader(ж<testing.B> Ꮡb) {
    ref var b = ref Ꮡb.DerefOrNull();

    b.ReportAllocs();
    foreach (var (_, vᴛ1) in new BenchmarkReadMIMEHeader_type[]{
        new("client_headers"u8, clientHeaders),
        new("server_headers"u8, serverHeaders)
    }.slice()) {
        ref var set = ref heap(new BenchmarkReadMIMEHeader_type(), out var Ꮡset);
        set = vᴛ1;

        var setʗ1 = set;
        Ꮡb.Run(set.name, (ж<testing.B> bΔ1) => {
            ref var buf = ref heap(new bytes.Buffer(), out var Ꮡbuf);
            var br = bufio.NewReader(new textproto_internal_test_package.bytes_BufferжReader(Ꮡbuf));
            var r = NewReader(br);
            for (nint i = 0; i < (~bΔ1).N; i++) {
                buf.WriteString(setʗ1.headers);
                {
                    var (_, err) = r.ReadMIMEHeader(); if (err != default!) {
                        bΔ1.Fatal(err);
                    }
                }
            }
        });
    }
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static readonly @string uncommonHeaderForˢ = "uncommon-header-for-benchmark: foo\r\n\r\n"u8;
internal static readonly @string uncommonHeaderForˢ2 = "Uncommon-Header-For-Benchmark"u8;
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
        if (err != default!) {
            Ꮡb.Fatal(err);
        }
        {
            var (_, ok) = h[uncommonHeaderForˢ2, ꟷ]; if (!ok) {
                Ꮡb.Fatal(missingResultHeaderˢ);
            }
        }
    }
}

} // end textproto_internal_test_package
