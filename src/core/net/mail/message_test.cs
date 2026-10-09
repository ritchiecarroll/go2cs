// Copyright 2011 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.
namespace go.net;

using bytes = bytes_package;
using io = io_package;
using mime = mime_package;
using reflect = reflect_package;
using slices = slices_package;
using strings = strings_package;
using testing = testing_package;
using time = time_package;
using static go.net.mail_package;

partial class mail_internal_test_package {

// RFC 5322, Appendix A.1.1
// RFC 5965, Appendix B.1, a part of the multipart message (a header-only sub message)
// RFC 5322 permits any printable ASCII character,
// except colon, in a header key. Issue #58862.
// RFC 4155 mbox format. We've historically permitted this,
// so we continue to permit it. Issue #60332.

partial struct parseTestsᴛ1 /*dyn*/ {
    internal @string @in;
    internal global::go.net.mail_package.Header header;
    internal @string body;
}
internal static slice<parseTestsᴛ1> parseTests = new parseTestsᴛ1[]{
    new(
        @in: """
From: John Doe <jdoe@machine.example>
To: Mary Smith <mary@example.net>
Subject: Saying Hello
Date: Fri, 21 Nov 1997 09:55:06 -0600
Message-ID: <1234@local.machine.example>

This is a message just to say hello.
So, "Hello".

"""u8,
        header: new Header(new map<@string, slice<@string>>{
            ["From"u8] = new @string[]{"John Doe <jdoe@machine.example>"u8}.slice(),
            ["To"u8] = new @string[]{"Mary Smith <mary@example.net>"u8}.slice(),
            ["Subject"u8] = new @string[]{"Saying Hello"u8}.slice(),
            ["Date"u8] = new @string[]{"Fri, 21 Nov 1997 09:55:06 -0600"u8}.slice(),
            ["Message-Id"u8] = new @string[]{"<1234@local.machine.example>"u8}.slice()
        }),
        body: "This is a message just to say hello.\nSo, \"Hello\".\n"u8
    ),
    new(
        @in: """
Feedback-Type: abuse
User-Agent: SomeGenerator/1.0
Version: 1

"""u8,
        header: new Header(new map<@string, slice<@string>>{
            ["Feedback-Type"u8] = new @string[]{"abuse"u8}.slice(),
            ["User-Agent"u8] = new @string[]{"SomeGenerator/1.0"u8}.slice(),
            ["Version"u8] = new @string[]{"1"u8}.slice()
        }),
        body: ""u8
    ),
    new(
        @in: """
From: iant@golang.org
Custom/Header: v

Body

"""u8,
        header: new Header(new map<@string, slice<@string>>{
            ["From"u8] = new @string[]{"iant@golang.org"u8}.slice(),
            ["Custom/Header"u8] = new @string[]{"v"u8}.slice()
        }),
        body: "Body\n"u8
    ),
    new(
        @in: """
From iant@golang.org Mon Jun 19 00:00:00 2023
From: iant@golang.org

Hello, gophers!

"""u8,
        header: new Header(new map<@string, slice<@string>>{
            ["From"u8] = new @string[]{"iant@golang.org"u8}.slice(),
            ["From iant@golang.org Mon Jun 19 00"u8] = new @string[]{"00:00 2023"u8}.slice()
        }),
        body: "Hello, gophers!\n"u8
    )
}.slice();

public static void TestParsing(ж<testing.T> Ꮡt) {
    foreach (var (i, test) in parseTests) {
        var (msg, err) = ReadMessage(new mail_test_package.bytes_BufferжReader(bytes.NewBuffer(slice<byte>(test.@in))));
        if (err != default!) {
            Ꮡt.Errorf("test #%d: Failed parsing message: %v"u8, i, err);
            continue;
        }
        if (!headerEq((~msg).Header, test.header)) {
            Ꮡt.Errorf("test #%d: Incorrectly parsed message header.\nGot:\n%+v\nWant:\n%+v"u8,
                i, (~msg).Header, test.header);
        }
        (var body, err) = io.ReadAll((~msg).Body);
        if (err != default!) {
            Ꮡt.Errorf("test #%d: Failed reading body: %v"u8, i, err);
            continue;
        }
        @string bodyStr = ((@string)body);
        if (bodyStr != test.body) {
            Ꮡt.Errorf("test #%d: Incorrectly parsed message body.\nGot:\n%+v\nWant:\n%+v"u8,
                i, bodyStr, test.body);
        }
    }
}

internal static bool headerEq(global::go.net.mail_package.Header a, global::go.net.mail_package.Header b) {
    if (builtin.len(a) != builtin.len(b)) {
        return false;
    }
    foreach (var (k, @as) in a) {
        var (bs, ok) = b[k, ꟷ];
        if (!ok) {
            return false;
        }
        if (!slices.Equal<slice<@string>, @string>(@as, bs)) {
            return false;
        }
    }
    return true;
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static readonly @string gmtˢ = "GMT"u8;

internal partial struct TestDateParsing_tests /*dyn*/ {
    internal @string dateStr;
    internal time.Time exp;
}

public static void TestDateParsing(ж<testing.T> Ꮡt) {
    var tests = new TestDateParsing_tests[]{ // RFC 5322, Appendix A.1.1

        new(
            "Fri, 21 Nov 1997 09:55:06 -0600"u8,
            time.Date(1997, 11, 21, 9, 55, 6, 0, time.FixedZone(""u8, -6 * 60 * 60))
        ), // RFC 5322, Appendix A.6.2
 // Obsolete date.

        new(
            "21 Nov 97 09:55:06 GMT"u8,
            time.Date(1997, 11, 21, 9, 55, 6, 0, time.FixedZone(gmtˢ, 0))
        ), // Commonly found format not specified by RFC 5322.

        new(
            "Fri, 21 Nov 1997 09:55:06 -0600 (MDT)"u8,
            time.Date(1997, 11, 21, 9, 55, 6, 0, time.FixedZone(""u8, -6 * 60 * 60))
        ),
        new(
            "Thu, 20 Nov 1997 09:55:06 -0600 (MDT)"u8,
            time.Date(1997, 11, 20, 9, 55, 6, 0, time.FixedZone(""u8, -6 * 60 * 60))
        ),
        new(
            "Thu, 20 Nov 1997 09:55:06 GMT (GMT)"u8,
            time.Date(1997, 11, 20, 9, 55, 6, 0, time.ΔUTC)
        ),
        new(
            "Fri, 21 Nov 1997 09:55:06 +1300 (TOT)"u8,
            time.Date(1997, 11, 21, 9, 55, 6, 0, time.FixedZone(""u8, +13 * 60 * 60))
        )
    }.slice();
    foreach (var (_, test) in tests) {
        var hdr = new Header(new map<@string, slice<@string>>{
            ["Date"u8] = new @string[]{test.dateStr}.slice()
        });
        var (date, err) = hdr.Date();
        if (err != default!){
            Ꮡt.Errorf("Header(Date: %s).Date(): %v"u8, test.dateStr, err);
        } else 
        if (!date.Equal(test.exp)) {
            Ꮡt.Errorf("Header(Date: %s).Date() = %+v, want %+v"u8, test.dateStr, date, test.exp);
        }
        (date, err) = ParseDate(test.dateStr);
        if (err != default!){
            Ꮡt.Errorf("ParseDate(%s): %v"u8, test.dateStr, err);
        } else 
        if (!date.Equal(test.exp)) {
            Ꮡt.Errorf("ParseDate(%s) = %+v, want %+v"u8, test.dateStr, date, test.exp);
        }
    }
}

internal partial struct TestDateParsingCFWS_tests /*dyn*/ {
    internal @string dateStr;
    internal time.Time exp;
    internal bool valid;
}

public static void TestDateParsingCFWS(ж<testing.T> Ꮡt) {
    ref var t = ref Ꮡt.DerefOrNull();

    var tests = new TestDateParsingCFWS_tests[]{ // FWS-only. No date.

        new(
            "   "u8, // nil is not allowed

            time.Date(1997, 11, 21, 9, 55, 6, 0, time.FixedZone(""u8, -6 * 60 * 60)),
            false
        ), // FWS is allowed before optional day of week.

        new(
            "   Fri, 21 Nov 1997 09:55:06 -0600"u8,
            time.Date(1997, 11, 21, 9, 55, 6, 0, time.FixedZone(""u8, -6 * 60 * 60)),
            true
        ),
        new(
            "21 Nov 1997 09:55:06 -0600"u8,
            time.Date(1997, 11, 21, 9, 55, 6, 0, time.FixedZone(""u8, -6 * 60 * 60)),
            true
        ),
        new(
            "Fri 21 Nov 1997 09:55:06 -0600"u8,
            time.Date(1997, 11, 21, 9, 55, 6, 0, time.FixedZone(""u8, -6 * 60 * 60)),
            false // missing ,

        ), // FWS is allowed before day of month but HTAB fails.

        new(
            "Fri,        21 Nov 1997 09:55:06 -0600"u8,
            time.Date(1997, 11, 21, 9, 55, 6, 0, time.FixedZone(""u8, -6 * 60 * 60)),
            true
        ), // FWS is allowed before and after year but HTAB fails.

        new(
            "Fri, 21 Nov       1997     09:55:06 -0600"u8,
            time.Date(1997, 11, 21, 9, 55, 6, 0, time.FixedZone(""u8, -6 * 60 * 60)),
            true
        ), // FWS is allowed before zone but HTAB is not handled. Obsolete timezone is handled.

        new(
            "Fri, 21 Nov 1997 09:55:06           CST"u8,
            new time.Time(nil),
            true
        ), // FWS is allowed after date and a CRLF is already replaced.

        new(
            "Fri, 21 Nov 1997 09:55:06           CST (no leading FWS and a trailing CRLF) \r\n"u8,
            new time.Time(nil),
            true
        ), // CFWS is a reduced set of US-ASCII where space and accentuated are obsolete. No error.

        new(
            "Fri, 21    Nov 1997    09:55:06 -0600 (MDT and non-US-ASCII signs éèç )"u8,
            time.Date(1997, 11, 21, 9, 55, 6, 0, time.FixedZone(""u8, -6 * 60 * 60)),
            true
        ), // CFWS is allowed after zone including a nested comment.
 // Trailing FWS is allowed.

        new(
            "Fri, 21 Nov 1997 09:55:06 -0600    \r\n (thisisa(valid)cfws)   \t "u8,
            time.Date(1997, 11, 21, 9, 55, 6, 0, time.FixedZone(""u8, -6 * 60 * 60)),
            true
        ), // CRLF is incomplete and misplaced.

        new(
            "Fri, 21 Nov 1997 \r 09:55:06 -0600    \r\n (thisisa(valid)cfws)   \t "u8,
            time.Date(1997, 11, 21, 9, 55, 6, 0, time.FixedZone(""u8, -6 * 60 * 60)),
            false
        ), // CRLF is complete but misplaced. No error is returned.

        new(
            "Fri, 21 Nov 199\r\n7  09:55:06 -0600    \r\n (thisisa(valid)cfws)   \t "u8,
            time.Date(1997, 11, 21, 9, 55, 6, 0, time.FixedZone(""u8, -6 * 60 * 60)),
            true // should be false in the strict interpretation of RFC 5322.

        ), // Invalid ASCII in date.

        new(
            "Fri, 21 Nov 1997 ù 09:55:06 -0600    \r\n (thisisa(valid)cfws)   \t "u8,
            time.Date(1997, 11, 21, 9, 55, 6, 0, time.FixedZone(""u8, -6 * 60 * 60)),
            false
        ), // CFWS chars () in date.

        new(
            "Fri, 21 Nov () 1997 09:55:06 -0600    \r\n (thisisa(valid)cfws)   \t "u8,
            time.Date(1997, 11, 21, 9, 55, 6, 0, time.FixedZone(""u8, -6 * 60 * 60)),
            false
        ), // Timezone is invalid but T is found in comment.

        new(
            "Fri, 21 Nov 1997 09:55:06 -060    \r\n (Thisisa(valid)cfws)   \t "u8,
            time.Date(1997, 11, 21, 9, 55, 6, 0, time.FixedZone(""u8, -6 * 60 * 60)),
            false
        ), // Date has no month.

        new(
            "Fri, 21  1997 09:55:06 -0600"u8,
            time.Date(1997, 11, 21, 9, 55, 6, 0, time.FixedZone(""u8, -6 * 60 * 60)),
            false
        ), // Invalid month : OCT iso Oct

        new(
            "Fri, 21 OCT 1997 09:55:06 CST"u8,
            new time.Time(nil),
            false
        ), // A too short time zone.

        new(
            "Fri, 21 Nov 1997 09:55:06 -060"u8,
            time.Date(1997, 11, 21, 9, 55, 6, 0, time.FixedZone(""u8, -6 * 60 * 60)),
            false
        ), // A too short obsolete time zone.

        new(
            "Fri, 21  1997 09:55:06 GT"u8,
            time.Date(1997, 11, 21, 9, 55, 6, 0, time.FixedZone(""u8, -6 * 60 * 60)),
            false
        ), // Ensure that the presence of "T" in the date
 // doesn't trip out ParseDate, as per issue 39260.

        new(
            "Tue, 26 May 2020 14:04:40 GMT"u8,
            time.Date(2020, 5, 26, 14, 4, 40, 0, time.ΔUTC),
            true
        ),
        new(
            "Tue, 26 May 2020 14:04:40 UT"u8,
            time.Date(2020, 5, 26, 14, 4, 40, 0, time.ΔUTC),
            true
        ),
        new(
            "Thu, 21 May 2020 14:04:40 UT"u8,
            time.Date(2020, 5, 21, 14, 4, 40, 0, time.ΔUTC),
            true
        ),
        new(
            "Tue, 26 May 2020 14:04:40 XT"u8,
            time.Date(2020, 5, 26, 14, 4, 40, 0, time.ΔUTC),
            false
        ),
        new(
            "Thu, 21 May 2020 14:04:40 XT"u8,
            time.Date(2020, 5, 21, 14, 4, 40, 0, time.ΔUTC),
            false
        ),
        new(
            "Thu, 21 May 2020 14:04:40 UTC"u8,
            time.Date(2020, 5, 21, 14, 4, 40, 0, time.ΔUTC),
            true
        ),
        new(
            "Fri, 21 Nov 1997 09:55:06 GMT (GMT)"u8,
            time.Date(1997, 11, 21, 9, 55, 6, 0, time.ΔUTC),
            true
        )
    }.slice();
    foreach (var (_, test) in tests) {
        var hdr = new Header(new map<@string, slice<@string>>{
            ["Date"u8] = new @string[]{test.dateStr}.slice()
        });
        var (date, err) = hdr.Date();
        if (err != default! && test.valid){
            Ꮡt.Errorf("Header(Date: %s).Date(): %v"u8, test.dateStr, err);
        } else 
        if (err == default! && test.exp.IsZero()){
        } else 
        if (err == default! && !date.Equal(test.exp) && test.valid){
            // OK.  Used when exact result depends on the
            // system's local zoneinfo.
            Ꮡt.Errorf("Header(Date: %s).Date() = %+v, want %+v"u8, test.dateStr, date, test.exp);
        } else 
        if (err == default! && !test.valid) {
            // an invalid expression was tested
            Ꮡt.Errorf("Header(Date: %s).Date() did not return an error but %v"u8, test.dateStr, date);
        }
        (date, err) = ParseDate(test.dateStr);
        if (err != default! && test.valid){
            Ꮡt.Errorf("ParseDate(%s): %v"u8, test.dateStr, err);
        } else 
        if (err == default! && test.exp.IsZero()){
        } else 
        if (err == default! && !test.valid){
            // OK.  Used when exact result depends on the
            // system's local zoneinfo.
            // an invalid expression was tested
            Ꮡt.Errorf("ParseDate(%s) did not return an error but %v"u8, test.dateStr, date);
        } else 
        if (err == default! && test.valid && !date.Equal(test.exp)) {
            Ꮡt.Errorf("ParseDate(%s) = %+v, want %+v"u8, test.dateStr, date, test.exp);
        }
    }
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static readonly @string customWordDecoderˢ = "CustomWordDecoder"u8;

internal partial struct TestAddressParsingError_mustErrTestCases /*dyn*/ {
    internal @string text;
    internal @string wantErrText;
}

public static void TestAddressParsingError(ж<testing.T> Ꮡt) {
    ref var mustErrTestCases = ref heap<array<TestAddressParsingError_mustErrTestCases>>(out var ᏑmustErrTestCases);
    mustErrTestCases = new array<TestAddressParsingError_mustErrTestCases>(25){
        [0] = new("=?iso-8859-2?Q?Bogl=E1rka_Tak=E1cs?= <unknown@gmail.com>"u8, "charset not supported"u8),
        [1] = new("a@gmail.com b@gmail.com"u8, "expected single address"u8),
        [2] = new(((sstring)new byte[]{0xed, 0xa0, 0x80}.slice()) + " <micro@example.net>"u8, "invalid utf-8 in address"u8),
        [3] = new("\""u8 + ((sstring)new byte[]{0xed, 0xa0, 0x80}.slice()) + "\" <half-surrogate@example.com>"u8, "invalid utf-8 in quoted-string"u8),
        [4] = new("\"\\"u8 + ((sstring)new byte[]{0x80}.slice()) + "\" <escaped-invalid-unicode@example.net>"u8, "invalid utf-8 in quoted-string"u8),
        [5] = new("\"\x00\" <null@example.net>"u8, "bad character in quoted-string"u8),
        [6] = new("\"\\\x00\" <escaped-null@example.net>"u8, "bad character in quoted-string"u8),
        [7] = new("John Doe"u8, "no angle-addr"u8),
        [8] = new(@"<jdoe#machine.example>"u8, "missing @ in addr-spec"u8),
        [9] = new(@"John <middle> Doe <jdoe@machine.example>"u8, "missing @ in addr-spec"u8),
        [10] = new("cfws@example.com ("u8, "misformatted parenthetical comment"u8),
        [11] = new("empty group: ;"u8, "empty group"u8),
        [12] = new("root group: embed group: null@example.com;"u8, "no angle-addr"u8),
        [13] = new("group not closed: null@example.com"u8, "expected comma"u8),
        [14] = new("group: first@example.com, second@example.com;"u8, "group with multiple addresses"u8),
        [15] = new("john.doe"u8, "missing '@' or angle-addr"u8),
        [16] = new("john.doe@"u8, "missing '@' or angle-addr"u8),
        [17] = new("John Doe@foo.bar"u8, "no angle-addr"u8),
        [18] = new(" group: null@example.com; (asd"u8, "misformatted parenthetical comment"u8),
        [19] = new(" group: ; (asd"u8, "misformatted parenthetical comment"u8),
        [20] = new(@"(John) Doe <jdoe@machine.example>"u8, "missing word in phrase:"u8),
        [21] = new("<jdoe@["u8 + ((sstring)new byte[]{0xed, 0xa0, 0x80}.slice()) + "192.168.0.1]>"u8, "invalid utf-8 in domain-literal"u8),
        [22] = new("<jdoe@[[192.168.0.1]>"u8, "bad character in domain-literal"u8),
        [23] = new("<jdoe@[192.168.0.1>"u8, "unclosed domain-literal"u8),
        [24] = new("<jdoe@[256.0.0.1]>"u8, "invalid IP address in domain-literal"u8)
    };
    foreach (var (i, tc) in mustErrTestCases.ΔRangeSnapshot()) {
        var (_, err) = ParseAddress(tc.text);
        if (err == default! || !strings.Contains(err.Error(), tc.wantErrText)) {
            Ꮡt.Errorf(@"mail.ParseAddress(%q) #%d want %q, got %v"u8, tc.text, i, tc.wantErrText, err);
        }
    }
    var mustErrTestCasesʗ1 = mustErrTestCases;
    Ꮡt.Run(customWordDecoderˢ, (ж<testing.T> tΔ1) => {
        var p = Ꮡ(new AddressParser(WordDecoder: Ꮡ(new mime.WordDecoder(nil))));
        foreach (var (i, tc) in mustErrTestCasesʗ1.ΔRangeSnapshot()) {
            var (_, err) = p.Parse(tc.text);
            if (err == default! || !strings.Contains(err.Error(), tc.wantErrText)) {
                tΔ1.Errorf(@"p.Parse(%q) #%d want %q, got %v"u8, tc.text, i, tc.wantErrText, err);
            }
        }
    });
}

internal partial struct TestAddressParsing_tests /*dyn*/ {
    internal @string addrsStr;
    internal slice<ж<global::go.net.mail_package.Address>> exp;
}

public static void TestAddressParsing(ж<testing.T> Ꮡt) {
    var tests = new TestAddressParsing_tests[]{ // Bare address

        new(
            @"jdoe@machine.example"u8,
            new ж<global::go.net.mail_package.Address>[]{Ꮡ(new global::go.net.mail_package.Address(
                ΔAddress: "jdoe@machine.example"u8))
            }.slice()
        ), // RFC 5322, Appendix A.1.1

        new(
            @"John Doe <jdoe@machine.example>"u8,
            new ж<global::go.net.mail_package.Address>[]{Ꮡ(new global::go.net.mail_package.Address(
                Name: "John Doe"u8,
                ΔAddress: "jdoe@machine.example"u8))
            }.slice()
        ), // RFC 5322, Appendix A.1.2

        new(
            @"""Joe Q. Public"" <john.q.public@example.com>"u8,
            new ж<global::go.net.mail_package.Address>[]{Ꮡ(new global::go.net.mail_package.Address(
                Name: "Joe Q. Public"u8,
                ΔAddress: "john.q.public@example.com"u8))
            }.slice()
        ), // Comment in display name

        new(
            @"John (middle) Doe <jdoe@machine.example>"u8,
            new ж<global::go.net.mail_package.Address>[]{Ꮡ(new global::go.net.mail_package.Address(
                Name: "John Doe"u8,
                ΔAddress: "jdoe@machine.example"u8))
            }.slice()
        ), // Display name is quoted string, so comment is not a comment

        new(
            @"""John (middle) Doe"" <jdoe@machine.example>"u8,
            new ж<global::go.net.mail_package.Address>[]{Ꮡ(new global::go.net.mail_package.Address(
                Name: "John (middle) Doe"u8,
                ΔAddress: "jdoe@machine.example"u8))
            }.slice()
        ),
        new(
            @"""John <middle> Doe"" <jdoe@machine.example>"u8,
            new ж<global::go.net.mail_package.Address>[]{Ꮡ(new global::go.net.mail_package.Address(
                Name: "John <middle> Doe"u8,
                ΔAddress: "jdoe@machine.example"u8))
            }.slice()
        ),
        new(
            @"Mary Smith <mary@x.test>, jdoe@example.org, Who? <one@y.test>"u8,
            new ж<global::go.net.mail_package.Address>[]{
                Ꮡ(new global::go.net.mail_package.Address(
                    Name: "Mary Smith"u8,
                    ΔAddress: "mary@x.test"u8)),
                Ꮡ(new global::go.net.mail_package.Address(
                    ΔAddress: "jdoe@example.org"u8)),
                Ꮡ(new global::go.net.mail_package.Address(
                    Name: "Who?"u8,
                    ΔAddress: "one@y.test"u8))
            }.slice()
        ),
        new(
            @"<boss@nil.test>, ""Giant; \""Big\"" Box"" <sysservices@example.net>"u8,
            new ж<global::go.net.mail_package.Address>[]{
                Ꮡ(new global::go.net.mail_package.Address(
                    ΔAddress: "boss@nil.test"u8)),
                Ꮡ(new global::go.net.mail_package.Address(
                    Name: @"Giant; ""Big"" Box"u8,
                    ΔAddress: "sysservices@example.net"u8))
            }.slice()
        ), // RFC 5322, Appendix A.6.1

        new(
            @"Joe Q. Public <john.q.public@example.com>"u8,
            new ж<global::go.net.mail_package.Address>[]{Ꮡ(new global::go.net.mail_package.Address(
                Name: "Joe Q. Public"u8,
                ΔAddress: "john.q.public@example.com"u8))
            }.slice()
        ), // RFC 5322, Appendix A.1.3

        new(
            @"group1: groupaddr1@example.com;"u8,
            new ж<global::go.net.mail_package.Address>[]{
                Ꮡ(new global::go.net.mail_package.Address(
                    Name: ""u8,
                    ΔAddress: "groupaddr1@example.com"u8))
            }.slice()
        ),
        new(
            @"empty group: ;"u8,
            slice<ж<global::go.net.mail_package.Address>>(default!)
        ),
        new(
            @"A Group:Ed Jones <c@a.test>,joe@where.test,John <jdoe@one.test>;"u8,
            new ж<global::go.net.mail_package.Address>[]{
                Ꮡ(new global::go.net.mail_package.Address(
                    Name: "Ed Jones"u8,
                    ΔAddress: "c@a.test"u8)),
                Ꮡ(new global::go.net.mail_package.Address(
                    Name: ""u8,
                    ΔAddress: "joe@where.test"u8)),
                Ꮡ(new global::go.net.mail_package.Address(
                    Name: "John"u8,
                    ΔAddress: "jdoe@one.test"u8))
            }.slice()
        ), // RFC5322 4.4 obs-addr-list

        new(
            @" , joe@where.test,,John <jdoe@one.test>,"u8,
            new ж<global::go.net.mail_package.Address>[]{
                Ꮡ(new global::go.net.mail_package.Address(
                    Name: ""u8,
                    ΔAddress: "joe@where.test"u8)),
                Ꮡ(new global::go.net.mail_package.Address(
                    Name: "John"u8,
                    ΔAddress: "jdoe@one.test"u8))
            }.slice()
        ),
        new(
            @" , joe@where.test,,John <jdoe@one.test>,,"u8,
            new ж<global::go.net.mail_package.Address>[]{
                Ꮡ(new global::go.net.mail_package.Address(
                    Name: ""u8,
                    ΔAddress: "joe@where.test"u8)),
                Ꮡ(new global::go.net.mail_package.Address(
                    Name: "John"u8,
                    ΔAddress: "jdoe@one.test"u8))
            }.slice()
        ),
        new(
            @"Group1: <addr1@example.com>;, Group 2: addr2@example.com;, John <addr3@example.com>"u8,
            new ж<global::go.net.mail_package.Address>[]{
                Ꮡ(new global::go.net.mail_package.Address(
                    Name: ""u8,
                    ΔAddress: "addr1@example.com"u8)),
                Ꮡ(new global::go.net.mail_package.Address(
                    Name: ""u8,
                    ΔAddress: "addr2@example.com"u8)),
                Ꮡ(new global::go.net.mail_package.Address(
                    Name: "John"u8,
                    ΔAddress: "addr3@example.com"u8))
            }.slice()
        ), // RFC 2047 "Q"-encoded ISO-8859-1 address.

        new(
            @"=?iso-8859-1?q?J=F6rg_Doe?= <joerg@example.com>"u8,
            new ж<global::go.net.mail_package.Address>[]{
                Ꮡ(new global::go.net.mail_package.Address(
                    Name: @"Jörg Doe"u8,
                    ΔAddress: "joerg@example.com"u8))
            }.slice()
        ), // RFC 2047 "Q"-encoded US-ASCII address. Dumb but legal.

        new(
            @"=?us-ascii?q?J=6Frg_Doe?= <joerg@example.com>"u8,
            new ж<global::go.net.mail_package.Address>[]{
                Ꮡ(new global::go.net.mail_package.Address(
                    Name: @"Jorg Doe"u8,
                    ΔAddress: "joerg@example.com"u8))
            }.slice()
        ), // RFC 2047 "Q"-encoded UTF-8 address.

        new(
            @"=?utf-8?q?J=C3=B6rg_Doe?= <joerg@example.com>"u8,
            new ж<global::go.net.mail_package.Address>[]{
                Ꮡ(new global::go.net.mail_package.Address(
                    Name: @"Jörg Doe"u8,
                    ΔAddress: "joerg@example.com"u8))
            }.slice()
        ), // RFC 2047 "Q"-encoded UTF-8 address with multiple encoded-words.

        new(
            @"=?utf-8?q?J=C3=B6rg?=  =?utf-8?q?Doe?= <joerg@example.com>"u8,
            new ж<global::go.net.mail_package.Address>[]{
                Ꮡ(new global::go.net.mail_package.Address(
                    Name: @"JörgDoe"u8,
                    ΔAddress: "joerg@example.com"u8))
            }.slice()
        ), // RFC 2047, Section 8.

        new(
            @"=?ISO-8859-1?Q?Andr=E9?= Pirard <PIRARD@vm1.ulg.ac.be>"u8,
            new ж<global::go.net.mail_package.Address>[]{
                Ꮡ(new global::go.net.mail_package.Address(
                    Name: @"André Pirard"u8,
                    ΔAddress: "PIRARD@vm1.ulg.ac.be"u8))
            }.slice()
        ), // Custom example of RFC 2047 "B"-encoded ISO-8859-1 address.

        new(
            @"=?ISO-8859-1?B?SvZyZw==?= <joerg@example.com>"u8,
            new ж<global::go.net.mail_package.Address>[]{
                Ꮡ(new global::go.net.mail_package.Address(
                    Name: @"Jörg"u8,
                    ΔAddress: "joerg@example.com"u8))
            }.slice()
        ), // Custom example of RFC 2047 "B"-encoded UTF-8 address.

        new(
            @"=?UTF-8?B?SsO2cmc=?= <joerg@example.com>"u8,
            new ж<global::go.net.mail_package.Address>[]{
                Ꮡ(new global::go.net.mail_package.Address(
                    Name: @"Jörg"u8,
                    ΔAddress: "joerg@example.com"u8))
            }.slice()
        ), // Custom example with "." in name. For issue 4938

        new(
            @"Asem H. <noreply@example.com>"u8,
            new ж<global::go.net.mail_package.Address>[]{
                Ꮡ(new global::go.net.mail_package.Address(
                    Name: @"Asem H."u8,
                    ΔAddress: "noreply@example.com"u8))
            }.slice()
        ), // RFC 6532 3.2.3, qtext /= UTF8-non-ascii

        new(
            @"""Gø Pher"" <gopher@example.com>"u8,
            new ж<global::go.net.mail_package.Address>[]{
                Ꮡ(new global::go.net.mail_package.Address(
                    Name: @"Gø Pher"u8,
                    ΔAddress: "gopher@example.com"u8))
            }.slice()
        ), // RFC 6532 3.2, atext /= UTF8-non-ascii

        new(
            @"µ <micro@example.com>"u8,
            new ж<global::go.net.mail_package.Address>[]{
                Ꮡ(new global::go.net.mail_package.Address(
                    Name: @"µ"u8,
                    ΔAddress: "micro@example.com"u8))
            }.slice()
        ), // RFC 6532 3.2.2, local address parts allow UTF-8

        new(
            @"Micro <µ@example.com>"u8,
            new ж<global::go.net.mail_package.Address>[]{
                Ꮡ(new global::go.net.mail_package.Address(
                    Name: @"Micro"u8,
                    ΔAddress: "µ@example.com"u8))
            }.slice()
        ), // RFC 6532 3.2.4, domains parts allow UTF-8

        new(
            @"Micro <micro@µ.example.com>"u8,
            new ж<global::go.net.mail_package.Address>[]{
                Ꮡ(new global::go.net.mail_package.Address(
                    Name: @"Micro"u8,
                    ΔAddress: "micro@µ.example.com"u8))
            }.slice()
        ), // Issue 14866

        new(
            @""""" <emptystring@example.com>"u8,
            new ж<global::go.net.mail_package.Address>[]{
                Ꮡ(new global::go.net.mail_package.Address(
                    Name: ""u8,
                    ΔAddress: "emptystring@example.com"u8))
            }.slice()
        ), // CFWS

        new(
            @"<cfws@example.com> (CFWS (cfws))  (another comment)"u8,
            new ж<global::go.net.mail_package.Address>[]{
                Ꮡ(new global::go.net.mail_package.Address(
                    Name: ""u8,
                    ΔAddress: "cfws@example.com"u8))
            }.slice()
        ),
        new(
            @"<cfws@example.com> ()  (another comment), <cfws2@example.com> (another)"u8,
            new ж<global::go.net.mail_package.Address>[]{
                Ꮡ(new global::go.net.mail_package.Address(
                    Name: ""u8,
                    ΔAddress: "cfws@example.com"u8)),
                Ꮡ(new global::go.net.mail_package.Address(
                    Name: ""u8,
                    ΔAddress: "cfws2@example.com"u8))
            }.slice()
        ), // Comment as display name

        new(
            @"john@example.com (John Doe)"u8,
            new ж<global::go.net.mail_package.Address>[]{
                Ꮡ(new global::go.net.mail_package.Address(
                    Name: "John Doe"u8,
                    ΔAddress: "john@example.com"u8))
            }.slice()
        ), // Comment and display name

        new(
            @"John Doe <john@example.com> (Joey)"u8,
            new ж<global::go.net.mail_package.Address>[]{
                Ꮡ(new global::go.net.mail_package.Address(
                    Name: "John Doe"u8,
                    ΔAddress: "john@example.com"u8))
            }.slice()
        ), // Comment as display name, no space

        new(
            @"john@example.com(John Doe)"u8,
            new ж<global::go.net.mail_package.Address>[]{
                Ꮡ(new global::go.net.mail_package.Address(
                    Name: "John Doe"u8,
                    ΔAddress: "john@example.com"u8))
            }.slice()
        ), // Comment as display name, Q-encoded

        new(
            @"asjo@example.com (Adam =?utf-8?Q?Sj=C3=B8gren?=)"u8,
            new ж<global::go.net.mail_package.Address>[]{
                Ꮡ(new global::go.net.mail_package.Address(
                    Name: "Adam Sjøgren"u8,
                    ΔAddress: "asjo@example.com"u8))
            }.slice()
        ), // Comment as display name, Q-encoded and tab-separated

        new(
            @"asjo@example.com (Adam	=?utf-8?Q?Sj=C3=B8gren?=)"u8,
            new ж<global::go.net.mail_package.Address>[]{
                Ꮡ(new global::go.net.mail_package.Address(
                    Name: "Adam Sjøgren"u8,
                    ΔAddress: "asjo@example.com"u8))
            }.slice()
        ), // Nested comment as display name, Q-encoded

        new(
            @"asjo@example.com (Adam =?utf-8?Q?Sj=C3=B8gren?= (Debian))"u8,
            new ж<global::go.net.mail_package.Address>[]{
                Ꮡ(new global::go.net.mail_package.Address(
                    Name: "Adam Sjøgren (Debian)"u8,
                    ΔAddress: "asjo@example.com"u8))
            }.slice()
        ), // Comment in group display name

        new(
            @"group (comment:): a@example.com, b@example.com;"u8,
            new ж<global::go.net.mail_package.Address>[]{
                Ꮡ(new global::go.net.mail_package.Address(
                    ΔAddress: "a@example.com"u8)),
                Ꮡ(new global::go.net.mail_package.Address(
                    ΔAddress: "b@example.com"u8))
            }.slice()
        ),
        new(
            @"x(:""):""@a.example;(""@b.example;"u8,
            new ж<global::go.net.mail_package.Address>[]{
                Ꮡ(new global::go.net.mail_package.Address(
                    ΔAddress: @"@a.example;(@b.example"u8))
            }.slice()
        ), // Domain-literal

        new(
            @"jdoe@[192.168.0.1]"u8,
            new ж<global::go.net.mail_package.Address>[]{Ꮡ(new global::go.net.mail_package.Address(
                ΔAddress: "jdoe@[192.168.0.1]"u8))
            }.slice()
        ),
        new(
            @"John Doe <jdoe@[192.168.0.1]>"u8,
            new ж<global::go.net.mail_package.Address>[]{Ꮡ(new global::go.net.mail_package.Address(
                Name: "John Doe"u8,
                ΔAddress: "jdoe@[192.168.0.1]"u8))
            }.slice()
        )
    }.slice();
    foreach (var (_, test) in tests) {
        if (builtin.len(test.exp) == 1) {
            var (addr, errΔ1) = ParseAddress(test.addrsStr);
            if (errΔ1 != default!) {
                Ꮡt.Errorf("Failed parsing (single) %q: %v"u8, test.addrsStr, errΔ1);
                continue;
            }
            if (!reflect.DeepEqual(new ж<global::go.net.mail_package.Address>[]{addr}.slice(), test.exp)) {
                Ꮡt.Errorf("Parse (single) of %q: got %+v, want %+v"u8, test.addrsStr, addr.OrTypedNil(), test.exp);
            }
        }
        var (addrs, err) = ParseAddressList(test.addrsStr);
        if (err != default!) {
            Ꮡt.Errorf("Failed parsing (list) %q: %v"u8, test.addrsStr, err);
            continue;
        }
        if (!reflect.DeepEqual(addrs, test.exp)) {
            Ꮡt.Errorf("Parse (list) of %q: got %+v, want %+v"u8, test.addrsStr, addrs, test.exp);
        }
    }
}

internal partial struct TestAddressParser_tests /*dyn*/ {
    internal @string addrsStr;
    internal slice<ж<global::go.net.mail_package.Address>> exp;
}

public static void TestAddressParser(ж<testing.T> Ꮡt) {
    var tests = new TestAddressParser_tests[]{ // Bare address

        new(
            @"jdoe@machine.example"u8,
            new ж<global::go.net.mail_package.Address>[]{Ꮡ(new global::go.net.mail_package.Address(
                ΔAddress: "jdoe@machine.example"u8))
            }.slice()
        ), // RFC 5322, Appendix A.1.1

        new(
            @"John Doe <jdoe@machine.example>"u8,
            new ж<global::go.net.mail_package.Address>[]{Ꮡ(new global::go.net.mail_package.Address(
                Name: "John Doe"u8,
                ΔAddress: "jdoe@machine.example"u8))
            }.slice()
        ), // RFC 5322, Appendix A.1.2

        new(
            @"""Joe Q. Public"" <john.q.public@example.com>"u8,
            new ж<global::go.net.mail_package.Address>[]{Ꮡ(new global::go.net.mail_package.Address(
                Name: "Joe Q. Public"u8,
                ΔAddress: "john.q.public@example.com"u8))
            }.slice()
        ),
        new(
            @"Mary Smith <mary@x.test>, jdoe@example.org, Who? <one@y.test>"u8,
            new ж<global::go.net.mail_package.Address>[]{
                Ꮡ(new global::go.net.mail_package.Address(
                    Name: "Mary Smith"u8,
                    ΔAddress: "mary@x.test"u8)),
                Ꮡ(new global::go.net.mail_package.Address(
                    ΔAddress: "jdoe@example.org"u8)),
                Ꮡ(new global::go.net.mail_package.Address(
                    Name: "Who?"u8,
                    ΔAddress: "one@y.test"u8))
            }.slice()
        ),
        new(
            @"<boss@nil.test>, ""Giant; \""Big\"" Box"" <sysservices@example.net>"u8,
            new ж<global::go.net.mail_package.Address>[]{
                Ꮡ(new global::go.net.mail_package.Address(
                    ΔAddress: "boss@nil.test"u8)),
                Ꮡ(new global::go.net.mail_package.Address(
                    Name: @"Giant; ""Big"" Box"u8,
                    ΔAddress: "sysservices@example.net"u8))
            }.slice()
        ), // RFC 2047 "Q"-encoded ISO-8859-1 address.

        new(
            @"=?iso-8859-1?q?J=F6rg_Doe?= <joerg@example.com>"u8,
            new ж<global::go.net.mail_package.Address>[]{
                Ꮡ(new global::go.net.mail_package.Address(
                    Name: @"Jörg Doe"u8,
                    ΔAddress: "joerg@example.com"u8))
            }.slice()
        ), // RFC 2047 "Q"-encoded US-ASCII address. Dumb but legal.

        new(
            @"=?us-ascii?q?J=6Frg_Doe?= <joerg@example.com>"u8,
            new ж<global::go.net.mail_package.Address>[]{
                Ꮡ(new global::go.net.mail_package.Address(
                    Name: @"Jorg Doe"u8,
                    ΔAddress: "joerg@example.com"u8))
            }.slice()
        ), // RFC 2047 "Q"-encoded ISO-8859-15 address.

        new(
            @"=?ISO-8859-15?Q?J=F6rg_Doe?= <joerg@example.com>"u8,
            new ж<global::go.net.mail_package.Address>[]{
                Ꮡ(new global::go.net.mail_package.Address(
                    Name: @"Jörg Doe"u8,
                    ΔAddress: "joerg@example.com"u8))
            }.slice()
        ), // RFC 2047 "B"-encoded windows-1252 address.

        new(
            @"=?windows-1252?q?Andr=E9?= Pirard <PIRARD@vm1.ulg.ac.be>"u8,
            new ж<global::go.net.mail_package.Address>[]{
                Ꮡ(new global::go.net.mail_package.Address(
                    Name: @"André Pirard"u8,
                    ΔAddress: "PIRARD@vm1.ulg.ac.be"u8))
            }.slice()
        ), // Custom example of RFC 2047 "B"-encoded ISO-8859-15 address.

        new(
            @"=?ISO-8859-15?B?SvZyZw==?= <joerg@example.com>"u8,
            new ж<global::go.net.mail_package.Address>[]{
                Ꮡ(new global::go.net.mail_package.Address(
                    Name: @"Jörg"u8,
                    ΔAddress: "joerg@example.com"u8))
            }.slice()
        ), // Custom example of RFC 2047 "B"-encoded UTF-8 address.

        new(
            @"=?UTF-8?B?SsO2cmc=?= <joerg@example.com>"u8,
            new ж<global::go.net.mail_package.Address>[]{
                Ꮡ(new global::go.net.mail_package.Address(
                    Name: @"Jörg"u8,
                    ΔAddress: "joerg@example.com"u8))
            }.slice()
        ), // Custom example with "." in name. For issue 4938

        new(
            @"Asem H. <noreply@example.com>"u8,
            new ж<global::go.net.mail_package.Address>[]{
                Ꮡ(new global::go.net.mail_package.Address(
                    Name: @"Asem H."u8,
                    ΔAddress: "noreply@example.com"u8))
            }.slice()
        ), // Domain-literal

        new(
            @"jdoe@[192.168.0.1]"u8,
            new ж<global::go.net.mail_package.Address>[]{Ꮡ(new global::go.net.mail_package.Address(
                ΔAddress: "jdoe@[192.168.0.1]"u8))
            }.slice()
        ),
        new(
            @"John Doe <jdoe@[192.168.0.1]>"u8,
            new ж<global::go.net.mail_package.Address>[]{Ꮡ(new global::go.net.mail_package.Address(
                Name: "John Doe"u8,
                ΔAddress: "jdoe@[192.168.0.1]"u8))
            }.slice()
        )
    }.slice();
    var ap = new AddressParser(WordDecoder: Ꮡ(new mime.WordDecoder(
        CharsetReader: (@string charset, io.Reader input) => {
            var (@in, err) = io.ReadAll(input);
            if (err != default!) {
                return (default!, err);
            }
            var exprᴛ1 = charset;
            if (exprᴛ1 == "iso-8859-15"u8) {
                @in = bytes.ReplaceAll(@in, slice<byte>(((@string)(new byte[]{0xf6}))), slice<byte>("ö"u8));
            }
            else if (exprᴛ1 == "windows-1252"u8) {
                @in = bytes.ReplaceAll(@in, slice<byte>(((@string)(new byte[]{0xe9}))), slice<byte>("é"u8));
            }

            return (new mail_test_package.bytes_ReaderжReader(bytes.NewReader(@in)), default!);
        }
    ))
    );
    foreach (var (_, test) in tests) {
        if (builtin.len(test.exp) == 1) {
            var (addr, errΔ1) = ap.Parse(test.addrsStr);
            if (errΔ1 != default!) {
                Ꮡt.Errorf("Failed parsing (single) %q: %v"u8, test.addrsStr, errΔ1);
                continue;
            }
            if (!reflect.DeepEqual(new ж<global::go.net.mail_package.Address>[]{addr}.slice(), test.exp)) {
                Ꮡt.Errorf("Parse (single) of %q: got %+v, want %+v"u8, test.addrsStr, addr.OrTypedNil(), test.exp);
            }
        }
        var (addrs, err) = ap.ParseList(test.addrsStr);
        if (err != default!) {
            Ꮡt.Errorf("Failed parsing (list) %q: %v"u8, test.addrsStr, err);
            continue;
        }
        if (!reflect.DeepEqual(addrs, test.exp)) {
            Ꮡt.Errorf("Parse (list) of %q: got %+v, want %+v"u8, test.addrsStr, addrs, test.exp);
        }
    }
}

internal partial struct TestAddressString_tests /*dyn*/ {
    internal ж<global::go.net.mail_package.Address> addr;
    internal @string exp;
}

public static void TestAddressString(ж<testing.T> Ꮡt) {
    var tests = new TestAddressString_tests[]{
        new(
            Ꮡ(new Address(ΔAddress: "bob@example.com"u8)),
            "<bob@example.com>"u8
        ),
        new(
            Ꮡ(new Address( // quoted local parts: RFC 5322, 3.4.1. and 3.2.4.
ΔAddress: @"my@idiot@address@example.com"u8)),
            @"<""my@idiot@address""@example.com>"u8
        ),
        new(
            Ꮡ(new Address( // quoted local parts
ΔAddress: @" @example.com"u8)),
            @"<"" ""@example.com>"u8
        ),
        new(
            Ꮡ(new Address(Name: "Bob"u8, ΔAddress: "bob@example.com"u8)),
            @"""Bob"" <bob@example.com>"u8
        ),
        new(
            Ꮡ(new Address( // note the ö (o with an umlaut)
Name: "Böb"u8, ΔAddress: "bob@example.com"u8)),
            @"=?utf-8?q?B=C3=B6b?= <bob@example.com>"u8
        ),
        new(
            Ꮡ(new Address(Name: "Bob Jane"u8, ΔAddress: "bob@example.com"u8)),
            @"""Bob Jane"" <bob@example.com>"u8
        ),
        new(
            Ꮡ(new Address(Name: "Böb Jacöb"u8, ΔAddress: "bob@example.com"u8)),
            @"=?utf-8?q?B=C3=B6b_Jac=C3=B6b?= <bob@example.com>"u8
        ),
        new(
            Ꮡ(new Address( // https://golang.org/issue/12098
Name: "Rob"u8, ΔAddress: ""u8)),
            @"""Rob"" <@>"u8
        ),
        new(
            Ꮡ(new Address( // https://golang.org/issue/12098
Name: "Rob"u8, ΔAddress: "@"u8)),
            @"""Rob"" <@>"u8
        ),
        new(
            Ꮡ(new Address(Name: "Böb, Jacöb"u8, ΔAddress: "bob@example.com"u8)),
            @"=?utf-8?b?QsO2YiwgSmFjw7Zi?= <bob@example.com>"u8
        ),
        new(
            Ꮡ(new Address(Name: "=??Q?x?="u8, ΔAddress: "hello@world.com"u8)),
            @"""=??Q?x?="" <hello@world.com>"u8
        ),
        new(
            Ꮡ(new Address(Name: "=?hello"u8, ΔAddress: "hello@world.com"u8)),
            @"""=?hello"" <hello@world.com>"u8
        ),
        new(
            Ꮡ(new Address(Name: "world?="u8, ΔAddress: "hello@world.com"u8)),
            @"""world?="" <hello@world.com>"u8
        ),
        new(
            Ꮡ(new Address( // should q-encode even for invalid utf-8.
Name: ((@string)new byte[]{0xed, 0xa0, 0x80}.slice()), ΔAddress: "invalid-utf8@example.net"u8)),
            "=?utf-8?q?=ED=A0=80?= <invalid-utf8@example.net>"u8
        ), // Domain-literal

        new(
            Ꮡ(new Address(ΔAddress: "bob@[192.168.0.1]"u8)),
            "<bob@[192.168.0.1]>"u8
        ),
        new(
            Ꮡ(new Address(Name: "Bob"u8, ΔAddress: "bob@[192.168.0.1]"u8)),
            @"""Bob"" <bob@[192.168.0.1]>"u8
        )
    }.slice();
    foreach (var (_, test) in tests) {
        @string s = test.addr.String();
        if (s != test.exp) {
            Ꮡt.Errorf("Address%+v.String() = %v, want %v"u8, test.addr.Value, s, test.exp);
            continue;
        }
        // Check round-trip.
        if ((~test.addr).ΔAddress != ""u8 && (~test.addr).ΔAddress != "@"u8) {
            var (a, err) = ParseAddress(test.exp);
            if (err != default!) {
                Ꮡt.Errorf("ParseAddress(%#q): %v"u8, test.exp, err);
                continue;
            }
            if ((~a).Name != (~test.addr).Name || (~a).ΔAddress != (~test.addr).ΔAddress) {
                Ꮡt.Errorf("ParseAddress(%#q) = %#v, want %#v"u8, test.exp, a.OrTypedNil(), test.addr.OrTypedNil());
            }
        }
    }
}

// Check if all valid addresses can be parsed, formatted and parsed again
public static void TestAddressParsingAndFormatting(ж<testing.T> Ꮡt) {
    ref var t = ref Ꮡt.DerefOrNull();

    // Should pass
    var tests = new @string[]{
        @"<Bob@example.com>"u8,
        @"<bob.bob@example.com>"u8,
        @"<"".bob""@example.com>"u8,
        @"<"" ""@example.com>"u8,
        @"<some.mail-with-dash@example.com>"u8,
        @"<""dot.and space""@example.com>"u8,
        @"<""very.unusual.@.unusual.com""@example.com>"u8,
        @"<admin@mailserver1>"u8,
        @"<postmaster@localhost>"u8,
        "<#!$%&'*+-/=?^_`{}|~@example.org>"u8,
        @"<""very.(),:;<>[]\"".VERY.\""very@\\ \""very\"".unusual""@strange.example.com>"u8, // escaped quotes

        @"<""()<>[]:,;@\\\""!#$%&'*+-/=?^_{}| ~.a""@example.org>"u8, // escaped backslashes

        @"<""Abc\\@def""@example.com>"u8,
        @"<""Joe\\Blow""@example.com>"u8,
        @"<test1/test2=test3@example.com>"u8,
        @"<def!xyz%abc@example.com>"u8,
        @"<_somename@example.com>"u8,
        @"<joe@uk>"u8,
        @"<~@example.com>"u8,
        @"<""...""@test.com>"u8,
        @"<""john..doe""@example.com>"u8,
        @"<""john.doe.""@example.com>"u8,
        @"<"".john.doe""@example.com>"u8,
        @"<"".""@example.com>"u8,
        @"<""..""@example.com>"u8,
        @"<""0:""@0>"u8,
        @"<Bob@[192.168.0.1]>"u8
    }.slice();
    foreach (var (_, test) in tests) {
        var (addr, err) = ParseAddress(test);
        if (err != default!) {
            Ꮡt.Errorf("Couldn't parse address %s: %s"u8, test, err.Error());
            continue;
        }
        @string str = addr.String();
        (addr, err) = ParseAddress(str);
        if (err != default!) {
            Ꮡt.Errorf("ParseAddr(%q) error: %v"u8, test, err);
            continue;
        }
        if (addr.String() != test) {
            Ꮡt.Errorf("String() round-trip = %q; want %q"u8, addr.OrTypedNil(), test);
            continue;
        }
    }
    // Should fail
    var badTests = new @string[]{
        @"<Abc.example.com>"u8,
        @"<A@b@c@example.com>"u8,
        @"<a""b(c)d,e:f;g<h>i[j\k]l@example.com>"u8,
        @"<just""not""right@example.com>"u8,
        @"<this is""not\allowed@example.com>"u8,
        @"<this\ still\""not\\allowed@example.com>"u8,
        @"<john..doe@example.com>"u8,
        @"<john.doe@example..com>"u8,
        @"<john.doe@example..com>"u8,
        @"<john.doe.@example.com>"u8,
        @"<john.doe.@.example.com>"u8,
        @"<.john.doe@example.com>"u8,
        @"<@example.com>"u8,
        @"<.@example.com>"u8,
        @"<test@.>"u8,
        @"< @example.com>"u8,
        @"<""""test""""blah""""@example.com>"u8,
        @"<""""@0>"u8
    }.slice();
    foreach (var (_, test) in badTests) {
        var (_, err) = ParseAddress(test);
        if (err == default!) {
            Ꮡt.Errorf("Should have failed to parse address: %s"u8, test);
            continue;
        }
    }
}

public static void TestAddressFormattingAndParsing(ж<testing.T> Ꮡt) {
    var tests = new ж<global::go.net.mail_package.Address>[]{
        Ꮡ(new global::go.net.mail_package.Address(Name: "@lïce"u8, ΔAddress: "alice@example.com"u8)),
        Ꮡ(new global::go.net.mail_package.Address(Name: "Böb O'Connor"u8, ΔAddress: "bob@example.com"u8)),
        Ꮡ(new global::go.net.mail_package.Address(Name: "???"u8, ΔAddress: "bob@example.com"u8)),
        Ꮡ(new global::go.net.mail_package.Address(Name: "Böb ???"u8, ΔAddress: "bob@example.com"u8)),
        Ꮡ(new global::go.net.mail_package.Address(Name: "Böb (Jacöb)"u8, ΔAddress: "bob@example.com"u8)),
        Ꮡ(new global::go.net.mail_package.Address(Name: "à#$%&'(),.:;<>@[]^`{|}~'"u8, ΔAddress: "bob@example.com"u8)), // https://golang.org/issue/11292

        Ꮡ(new global::go.net.mail_package.Address(Name: "\"\\\x1f,\""u8, ΔAddress: "0@0"u8)), // https://golang.org/issue/12782

        Ꮡ(new global::go.net.mail_package.Address(Name: "naé, mée"u8, ΔAddress: "test.mail@gmail.com"u8))
    }.slice();
    foreach (var (i, test) in tests) {
        var (parsed, err) = ParseAddress(test.String());
        if (err != default!) {
            Ꮡt.Errorf("test #%d: ParseAddr(%q) error: %v"u8, i, test.String(), err);
            continue;
        }
        if ((~parsed).Name != (~test).Name) {
            Ꮡt.Errorf("test #%d: Parsed name = %q; want %q"u8, i, (~parsed).Name, (~test).Name);
        }
        if ((~parsed).ΔAddress != (~test).ΔAddress) {
            Ꮡt.Errorf("test #%d: Parsed address = %q; want %q"u8, i, (~parsed).ΔAddress, (~test).ΔAddress);
        }
    }
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static readonly @string aBCDˢ = "a@b c@d"u8;

public static void TestEmptyAddress(ж<testing.T> Ꮡt) {
    var (parsed, err) = ParseAddress(""u8);
    if (parsed != nil || err == default!) {
        Ꮡt.Errorf(@"ParseAddress("""") = %v, %v, want nil, error"u8, parsed.OrTypedNil(), err);
    }
    (var list, err) = ParseAddressList(""u8);
    if (builtin.len(list) > 0 || err == default!) {
        Ꮡt.Errorf(@"ParseAddressList("""") = %v, %v, want nil, error"u8, list, err);
    }
    (list, err) = ParseAddressList(","u8);
    if (builtin.len(list) > 0 || err == default!) {
        Ꮡt.Errorf(@"ParseAddressList("""") = %v, %v, want nil, error"u8, list, err);
    }
    (list, err) = ParseAddressList(aBCDˢ);
    if (builtin.len(list) > 0 || err == default!) {
        Ꮡt.Errorf(@"ParseAddressList("""") = %v, %v, want nil, error"u8, list, err);
    }
}

} // end mail_internal_test_package
