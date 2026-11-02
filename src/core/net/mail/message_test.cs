// Copyright 2011 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.
namespace go.net;

using bytes = bytes_package;
using fmt = fmt_package;
using io = io_package;
using mime = mime_package;
using reflect = reflect_package;
using slices = slices_package;
using strings = strings_package;
using testing = testing_package;
using time = time_package;
using static go.net.mail_package;

partial class mail_internal_test_package {

partial struct parseTestsᴛ1 /*dyn*/ {
    internal @string @in;
    internal global::go.net.mail_package.Header header;
    internal @string body;
}
internal static slice<parseTestsᴛ1> parseTests = slice<parseTestsᴛ1>([
    new(
        // RFC 5322, Appendix A.1.1
        @in: """
From: John Doe <jdoe@machine.example>
To: Mary Smith <mary@example.net>
Subject: Saying Hello
Date: Fri, 21 Nov 1997 09:55:06 -0600
Message-ID: <1234@local.machine.example>

This is a message just to say hello.
So, "Hello".

"""u8.Raw(),
        header: new Header(new map<@string, slice<@string>>{
            ["From"] = slice<@string>(["John Doe <jdoe@machine.example>"]),
            ["To"] = slice<@string>(["Mary Smith <mary@example.net>"]),
            ["Subject"] = slice<@string>(["Saying Hello"]),
            ["Date"] = slice<@string>(["Fri, 21 Nov 1997 09:55:06 -0600"]),
            ["Message-Id"] = slice<@string>(["<1234@local.machine.example>"])
        }),
        body: "This is a message just to say hello.\nSo, \"Hello\".\n"
    ),
    new(
        // RFC 5965, Appendix B.1, a part of the multipart message (a header-only sub message)
        @in: """
Feedback-Type: abuse
User-Agent: SomeGenerator/1.0
Version: 1

"""u8.Raw(),
        header: new Header(new map<@string, slice<@string>>{
            ["Feedback-Type"] = slice<@string>(["abuse"]),
            ["User-Agent"] = slice<@string>(["SomeGenerator/1.0"]),
            ["Version"] = slice<@string>(["1"])
        }),
        body: ""
    ),
    new(
        // RFC 5322 permits any printable ASCII character,
        // except colon, in a header key. Issue #58862.
        @in: """
From: iant@golang.org
Custom/Header: v

Body

"""u8.Raw(),
        header: new Header(new map<@string, slice<@string>>{
            ["From"] = slice<@string>(["iant@golang.org"]),
            ["Custom/Header"] = slice<@string>(["v"])
        }),
        body: "Body\n"
    ),
    new(
        // RFC 4155 mbox format. We've historically permitted this,
        // so we continue to permit it. Issue #60332.
        @in: """
From iant@golang.org Mon Jun 19 00:00:00 2023
From: iant@golang.org

Hello, gophers!

"""u8.Raw(),
        header: new Header(new map<@string, slice<@string>>{
            ["From"] = slice<@string>(["iant@golang.org"]),
            ["From iant@golang.org Mon Jun 19 00"] = slice<@string>(["00:00 2023"])
        }),
        body: "Hello, gophers!\n"
    )
]);

public static void TestParsing(ж<testing.T> t) {
    foreach (var (i, test) in parseTests) {
        var (msg, err) = ReadMessage(new mail_test_package.bytes_BufferжReader(bytes.NewBuffer(slice<byte>(test.@in))));
        if (err != nil) {
            t.Errorf("test #%d: Failed parsing message: %v", i, err);
            continue;
        }
        if (!headerEq(msg.Header, test.header)) {
            t.Errorf("test #%d: Incorrectly parsed message header.\nGot:\n%+v\nWant:\n%+v",
                i, msg.Header, test.header);
        }
        (var body, err) = io.ReadAll(msg.Body);
        if (err != nil) {
            t.Errorf("test #%d: Failed reading body: %v", i, err);
            continue;
        }
        @string bodyStr = (@string)body;
        if (bodyStr != test.body) {
            t.Errorf("test #%d: Incorrectly parsed message body.\nGot:\n%+v\nWant:\n%+v",
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
internal static ref readonly @string gmtˢ => ref gmtˢᵇ;
private static readonly @string gmtˢᵇ = "GMT"u8;

internal partial struct TestDateParsing_tests /*dyn*/ {
    internal @string dateStr;
    internal time.Time exp;
}

public static void TestDateParsing(ж<testing.T> t) {
    var tests = slice<TestDateParsing_tests>([
        // RFC 5322, Appendix A.1.1
        new(
            "Fri, 21 Nov 1997 09:55:06 -0600",
            time.Date(1997, 11, 21, 9, 55, 6, 0, time.FixedZone(""u8, -6 * 60 * 60))
        ),
        // RFC 5322, Appendix A.6.2
        // Obsolete date.
        new(
            "21 Nov 97 09:55:06 GMT",
            time.Date(1997, 11, 21, 9, 55, 6, 0, time.FixedZone(gmtˢ, 0))
        ),
        // Commonly found format not specified by RFC 5322.
        new(
            "Fri, 21 Nov 1997 09:55:06 -0600 (MDT)",
            time.Date(1997, 11, 21, 9, 55, 6, 0, time.FixedZone(""u8, -6 * 60 * 60))
        ),
        new(
            "Thu, 20 Nov 1997 09:55:06 -0600 (MDT)",
            time.Date(1997, 11, 20, 9, 55, 6, 0, time.FixedZone(""u8, -6 * 60 * 60))
        ),
        new(
            "Thu, 20 Nov 1997 09:55:06 GMT (GMT)",
            time.Date(1997, 11, 20, 9, 55, 6, 0, time.ΔUTC)
        ),
        new(
            "Fri, 21 Nov 1997 09:55:06 +1300 (TOT)",
            time.Date(1997, 11, 21, 9, 55, 6, 0, time.FixedZone(""u8, +13 * 60 * 60))
        )
    ]);
    foreach (var (_, test) in tests) {
        var hdr = new Header(new map<@string, slice<@string>>{
            ["Date"] = slice<@string>([test.dateStr])
        });
        var (date, err) = hdr.Date();
        if (err != nil) {
            t.Errorf("Header(Date: %s).Date(): %v", test.dateStr, err);
        } else if (!date.Equal(test.exp)) {
            t.Errorf("Header(Date: %s).Date() = %+v, want %+v", test.dateStr, date, test.exp);
        }

        (date, err) = ParseDate(test.dateStr);
        if (err != nil) {
            t.Errorf("ParseDate(%s): %v", test.dateStr, err);
        } else if (!date.Equal(test.exp)) {
            t.Errorf("ParseDate(%s) = %+v, want %+v", test.dateStr, date, test.exp);
        }
    }
}

internal partial struct TestDateParsingCFWS_tests /*dyn*/ {
    internal @string dateStr;
    internal time.Time exp;
    internal bool valid;
}

public static void TestDateParsingCFWS(ж<testing.T> t) {
    var tests = slice<TestDateParsingCFWS_tests>([
        // FWS-only. No date.
        new(
            "   ",
            // nil is not allowed
            time.Date(1997, 11, 21, 9, 55, 6, 0, time.FixedZone(""u8, -6 * 60 * 60)),
            false
        ),
        // FWS is allowed before optional day of week.
        new(
            "   Fri, 21 Nov 1997 09:55:06 -0600",
            time.Date(1997, 11, 21, 9, 55, 6, 0, time.FixedZone(""u8, -6 * 60 * 60)),
            true
        ),
        new(
            "21 Nov 1997 09:55:06 -0600",
            time.Date(1997, 11, 21, 9, 55, 6, 0, time.FixedZone(""u8, -6 * 60 * 60)),
            true
        ),
        new(
            "Fri 21 Nov 1997 09:55:06 -0600",
            time.Date(1997, 11, 21, 9, 55, 6, 0, time.FixedZone(""u8, -6 * 60 * 60)),
            false // missing ,

        ),
        // FWS is allowed before day of month but HTAB fails.
        new(
            "Fri,        21 Nov 1997 09:55:06 -0600",
            time.Date(1997, 11, 21, 9, 55, 6, 0, time.FixedZone(""u8, -6 * 60 * 60)),
            true
        ),
        // FWS is allowed before and after year but HTAB fails.
        new(
            "Fri, 21 Nov       1997     09:55:06 -0600",
            time.Date(1997, 11, 21, 9, 55, 6, 0, time.FixedZone(""u8, -6 * 60 * 60)),
            true
        ),
        // FWS is allowed before zone but HTAB is not handled. Obsolete timezone is handled.
        new(
            "Fri, 21 Nov 1997 09:55:06           CST",
            new time.Time(),
            true
        ),
        // FWS is allowed after date and a CRLF is already replaced.
        new(
            "Fri, 21 Nov 1997 09:55:06           CST (no leading FWS and a trailing CRLF) \r\n",
            new time.Time(),
            true
        ),
        // CFWS is a reduced set of US-ASCII where space and accentuated are obsolete. No error.
        new(
            "Fri, 21    Nov 1997    09:55:06 -0600 (MDT and non-US-ASCII signs éèç )",
            time.Date(1997, 11, 21, 9, 55, 6, 0, time.FixedZone(""u8, -6 * 60 * 60)),
            true
        ),
        // CFWS is allowed after zone including a nested comment.
        // Trailing FWS is allowed.
        new(
            "Fri, 21 Nov 1997 09:55:06 -0600    \r\n (thisisa(valid)cfws)   \t ",
            time.Date(1997, 11, 21, 9, 55, 6, 0, time.FixedZone(""u8, -6 * 60 * 60)),
            true
        ),
        // CRLF is incomplete and misplaced.
        new(
            "Fri, 21 Nov 1997 \r 09:55:06 -0600    \r\n (thisisa(valid)cfws)   \t ",
            time.Date(1997, 11, 21, 9, 55, 6, 0, time.FixedZone(""u8, -6 * 60 * 60)),
            false
        ),
        // CRLF is complete but misplaced. No error is returned.
        new(
            "Fri, 21 Nov 199\r\n7  09:55:06 -0600    \r\n (thisisa(valid)cfws)   \t ",
            time.Date(1997, 11, 21, 9, 55, 6, 0, time.FixedZone(""u8, -6 * 60 * 60)),
            true // should be false in the strict interpretation of RFC 5322.

        ),
        // Invalid ASCII in date.
        new(
            "Fri, 21 Nov 1997 ù 09:55:06 -0600    \r\n (thisisa(valid)cfws)   \t ",
            time.Date(1997, 11, 21, 9, 55, 6, 0, time.FixedZone(""u8, -6 * 60 * 60)),
            false
        ),
        // CFWS chars () in date.
        new(
            "Fri, 21 Nov () 1997 09:55:06 -0600    \r\n (thisisa(valid)cfws)   \t ",
            time.Date(1997, 11, 21, 9, 55, 6, 0, time.FixedZone(""u8, -6 * 60 * 60)),
            false
        ),
        // Timezone is invalid but T is found in comment.
        new(
            "Fri, 21 Nov 1997 09:55:06 -060    \r\n (Thisisa(valid)cfws)   \t ",
            time.Date(1997, 11, 21, 9, 55, 6, 0, time.FixedZone(""u8, -6 * 60 * 60)),
            false
        ),
        // Date has no month.
        new(
            "Fri, 21  1997 09:55:06 -0600",
            time.Date(1997, 11, 21, 9, 55, 6, 0, time.FixedZone(""u8, -6 * 60 * 60)),
            false
        ),
        // Invalid month : OCT iso Oct
        new(
            "Fri, 21 OCT 1997 09:55:06 CST",
            new time.Time(),
            false
        ),
        // A too short time zone.
        new(
            "Fri, 21 Nov 1997 09:55:06 -060",
            time.Date(1997, 11, 21, 9, 55, 6, 0, time.FixedZone(""u8, -6 * 60 * 60)),
            false
        ),
        // A too short obsolete time zone.
        new(
            "Fri, 21  1997 09:55:06 GT",
            time.Date(1997, 11, 21, 9, 55, 6, 0, time.FixedZone(""u8, -6 * 60 * 60)),
            false
        ),
        // Ensure that the presence of "T" in the date
        // doesn't trip out ParseDate, as per issue 39260.
        new(
            "Tue, 26 May 2020 14:04:40 GMT",
            time.Date(2020, 5, 26, 14, 4, 40, 0, time.ΔUTC),
            true
        ),
        new(
            "Tue, 26 May 2020 14:04:40 UT",
            time.Date(2020, 5, 26, 14, 4, 40, 0, time.ΔUTC),
            true
        ),
        new(
            "Thu, 21 May 2020 14:04:40 UT",
            time.Date(2020, 5, 21, 14, 4, 40, 0, time.ΔUTC),
            true
        ),
        new(
            "Tue, 26 May 2020 14:04:40 XT",
            time.Date(2020, 5, 26, 14, 4, 40, 0, time.ΔUTC),
            false
        ),
        new(
            "Thu, 21 May 2020 14:04:40 XT",
            time.Date(2020, 5, 21, 14, 4, 40, 0, time.ΔUTC),
            false
        ),
        new(
            "Thu, 21 May 2020 14:04:40 UTC",
            time.Date(2020, 5, 21, 14, 4, 40, 0, time.ΔUTC),
            true
        ),
        new(
            "Fri, 21 Nov 1997 09:55:06 GMT (GMT)",
            time.Date(1997, 11, 21, 9, 55, 6, 0, time.ΔUTC),
            true
        )
    ]);
    foreach (var (_, test) in tests) {
        var hdr = new Header(new map<@string, slice<@string>>{
            ["Date"] = slice<@string>([test.dateStr])
        });
        var (date, err) = hdr.Date();
        if (err != nil && test.valid) {
            t.Errorf("Header(Date: %s).Date(): %v", test.dateStr, err);
        } else if (err == nil && test.exp.IsZero()) {
            // OK.  Used when exact result depends on the
            // system's local zoneinfo.
        } else if (err == nil && !date.Equal(test.exp) && test.valid) {
            t.Errorf("Header(Date: %s).Date() = %+v, want %+v", test.dateStr, date, test.exp);
        } else if (err == nil && !test.valid) { // an invalid expression was tested
            t.Errorf("Header(Date: %s).Date() did not return an error but %v", test.dateStr, date);
        }

        (date, err) = ParseDate(test.dateStr);
        if (err != nil && test.valid) {
            t.Errorf("ParseDate(%s): %v", test.dateStr, err);
        } else if (err == nil && test.exp.IsZero()) {
            // OK.  Used when exact result depends on the
            // system's local zoneinfo.
        } else if (err == nil && !test.valid) { // an invalid expression was tested
            t.Errorf("ParseDate(%s) did not return an error but %v", test.dateStr, date);
        } else if (err == nil && test.valid && !date.Equal(test.exp)) {
            t.Errorf("ParseDate(%s) = %+v, want %+v", test.dateStr, date, test.exp);
        }
    }
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static ref readonly @string customWordDecoderˢ => ref customWordDecoderˢᵇ;
private static readonly @string customWordDecoderˢᵇ = "CustomWordDecoder"u8;

internal partial struct TestAddressParsingError_mustErrTestCases /*dyn*/ {
    internal @string text;
    internal @string wantErrText;
}

public static void TestAddressParsingError(ж<testing.T> t) {
    ref var mustErrTestCases = ref heap<array<TestAddressParsingError_mustErrTestCases>>(new array<TestAddressParsingError_mustErrTestCases>(25){
        [0] = new("=?iso-8859-2?Q?Bogl=E1rka_Tak=E1cs?= <unknown@gmail.com>", "charset not supported"),
        [1] = new("a@gmail.com b@gmail.com", "expected single address"),
        [2] = new(((sstring)slice<byte>([0xed, 0xa0, 0x80])) + " <micro@example.net>"u8, "invalid utf-8 in address"),
        [3] = new("\""u8 + ((sstring)slice<byte>([0xed, 0xa0, 0x80])) + "\" <half-surrogate@example.com>"u8, "invalid utf-8 in quoted-string"),
        [4] = new("\"\\"u8 + ((sstring)slice<byte>([0x80])) + "\" <escaped-invalid-unicode@example.net>"u8, "invalid utf-8 in quoted-string"),
        [5] = new("\"\x00\" <null@example.net>", "bad character in quoted-string"),
        [6] = new("\"\\\x00\" <escaped-null@example.net>", "bad character in quoted-string"),
        [7] = new("John Doe", "no angle-addr"),
        [8] = new(@"<jdoe#machine.example>", "missing @ in addr-spec"),
        [9] = new(@"John <middle> Doe <jdoe@machine.example>", "missing @ in addr-spec"),
        [10] = new("cfws@example.com (", "misformatted parenthetical comment"),
        [11] = new("empty group: ;", "empty group"),
        [12] = new("root group: embed group: null@example.com;", "no angle-addr"),
        [13] = new("group not closed: null@example.com", "expected comma"),
        [14] = new("group: first@example.com, second@example.com;", "group with multiple addresses"),
        [15] = new("john.doe", "missing '@' or angle-addr"),
        [16] = new("john.doe@", "missing '@' or angle-addr"),
        [17] = new("John Doe@foo.bar", "no angle-addr"),
        [18] = new(" group: null@example.com; (asd", "misformatted parenthetical comment"),
        [19] = new(" group: ; (asd", "misformatted parenthetical comment"),
        [20] = new(@"(John) Doe <jdoe@machine.example>", "missing word in phrase:"),
        [21] = new("<jdoe@["u8 + ((sstring)slice<byte>([0xed, 0xa0, 0x80])) + "192.168.0.1]>"u8, "invalid utf-8 in domain-literal"),
        [22] = new("<jdoe@[[192.168.0.1]>", "bad character in domain-literal"),
        [23] = new("<jdoe@[192.168.0.1>", "unclosed domain-literal"),
        [24] = new("<jdoe@[256.0.0.1]>", "invalid IP address in domain-literal")
    }, out var ᏑmustErrTestCases);

    foreach (var (i, tc) in mustErrTestCases.ΔRangeSnapshot()) {
        var (_, err) = ParseAddress(tc.text);
        if (err == nil || !strings.Contains(err.Error(), tc.wantErrText)) {
            t.Errorf(@"mail.ParseAddress(%q) #%d want %q, got %v", tc.text, i, tc.wantErrText, err);
        }
    }

    var mustErrTestCasesʗ1 = mustErrTestCases;
    t.Run(customWordDecoderˢ, (ж<testing.T> tΔ1) => {
        var p = Ꮡ(new AddressParser(WordDecoder: Ꮡ(new mime.WordDecoder())));
        foreach (var (i, tc) in mustErrTestCasesʗ1.ΔRangeSnapshot()) {
            var (_, err) = p.Parse(tc.text);
            if (err == nil || !strings.Contains(err.Error(), tc.wantErrText)) {
                tΔ1.Errorf(@"p.Parse(%q) #%d want %q, got %v", tc.text, i, tc.wantErrText, err);
            }
        }
    });
}

internal partial struct TestAddressParsing_tests /*dyn*/ {
    internal @string addrsStr;
    internal slice<ж<global::go.net.mail_package.Address>> exp;
}

public static void TestAddressParsing(ж<testing.T> t) {
    var tests = slice<TestAddressParsing_tests>([
        // Bare address
        new(
            @"jdoe@machine.example",
            slice<ж<global::go.net.mail_package.Address>>([Ꮡ(new global::go.net.mail_package.Address(
                ΔAddress: "jdoe@machine.example"))
            ])
        ),
        // RFC 5322, Appendix A.1.1
        new(
            @"John Doe <jdoe@machine.example>",
            slice<ж<global::go.net.mail_package.Address>>([Ꮡ(new global::go.net.mail_package.Address(
                Name: "John Doe",
                ΔAddress: "jdoe@machine.example"))
            ])
        ),
        // RFC 5322, Appendix A.1.2
        new(
            @"""Joe Q. Public"" <john.q.public@example.com>",
            slice<ж<global::go.net.mail_package.Address>>([Ꮡ(new global::go.net.mail_package.Address(
                Name: "Joe Q. Public",
                ΔAddress: "john.q.public@example.com"))
            ])
        ),
        // Comment in display name
        new(
            @"John (middle) Doe <jdoe@machine.example>",
            slice<ж<global::go.net.mail_package.Address>>([Ꮡ(new global::go.net.mail_package.Address(
                Name: "John Doe",
                ΔAddress: "jdoe@machine.example"))
            ])
        ),
        // Display name is quoted string, so comment is not a comment
        new(
            @"""John (middle) Doe"" <jdoe@machine.example>",
            slice<ж<global::go.net.mail_package.Address>>([Ꮡ(new global::go.net.mail_package.Address(
                Name: "John (middle) Doe",
                ΔAddress: "jdoe@machine.example"))
            ])
        ),
        new(
            @"""John <middle> Doe"" <jdoe@machine.example>",
            slice<ж<global::go.net.mail_package.Address>>([Ꮡ(new global::go.net.mail_package.Address(
                Name: "John <middle> Doe",
                ΔAddress: "jdoe@machine.example"))
            ])
        ),
        new(
            @"Mary Smith <mary@x.test>, jdoe@example.org, Who? <one@y.test>",
            slice<ж<global::go.net.mail_package.Address>>([
                Ꮡ(new global::go.net.mail_package.Address(
                    Name: "Mary Smith",
                    ΔAddress: "mary@x.test")),
                Ꮡ(new global::go.net.mail_package.Address(
                    ΔAddress: "jdoe@example.org")),
                Ꮡ(new global::go.net.mail_package.Address(
                    Name: "Who?",
                    ΔAddress: "one@y.test"))
            ])
        ),
        new(
            @"<boss@nil.test>, ""Giant; \""Big\"" Box"" <sysservices@example.net>",
            slice<ж<global::go.net.mail_package.Address>>([
                Ꮡ(new global::go.net.mail_package.Address(
                    ΔAddress: "boss@nil.test")),
                Ꮡ(new global::go.net.mail_package.Address(
                    Name: @"Giant; ""Big"" Box",
                    ΔAddress: "sysservices@example.net"))
            ])
        ),
        // RFC 5322, Appendix A.6.1
        new(
            @"Joe Q. Public <john.q.public@example.com>",
            slice<ж<global::go.net.mail_package.Address>>([Ꮡ(new global::go.net.mail_package.Address(
                Name: "Joe Q. Public",
                ΔAddress: "john.q.public@example.com"))
            ])
        ),
        // RFC 5322, Appendix A.1.3
        new(
            @"group1: groupaddr1@example.com;",
            slice<ж<global::go.net.mail_package.Address>>([
                Ꮡ(new global::go.net.mail_package.Address(
                    Name: "",
                    ΔAddress: "groupaddr1@example.com"))
            ])
        ),
        new(
            @"empty group: ;",
            slice<ж<global::go.net.mail_package.Address>>(default)
        ),
        new(
            @"A Group:Ed Jones <c@a.test>,joe@where.test,John <jdoe@one.test>;",
            slice<ж<global::go.net.mail_package.Address>>([
                Ꮡ(new global::go.net.mail_package.Address(
                    Name: "Ed Jones",
                    ΔAddress: "c@a.test")),
                Ꮡ(new global::go.net.mail_package.Address(
                    Name: "",
                    ΔAddress: "joe@where.test")),
                Ꮡ(new global::go.net.mail_package.Address(
                    Name: "John",
                    ΔAddress: "jdoe@one.test"))
            ])
        ),
        // RFC5322 4.4 obs-addr-list
        new(
            @" , joe@where.test,,John <jdoe@one.test>,",
            slice<ж<global::go.net.mail_package.Address>>([
                Ꮡ(new global::go.net.mail_package.Address(
                    Name: "",
                    ΔAddress: "joe@where.test")),
                Ꮡ(new global::go.net.mail_package.Address(
                    Name: "John",
                    ΔAddress: "jdoe@one.test"))
            ])
        ),
        new(
            @" , joe@where.test,,John <jdoe@one.test>,,",
            slice<ж<global::go.net.mail_package.Address>>([
                Ꮡ(new global::go.net.mail_package.Address(
                    Name: "",
                    ΔAddress: "joe@where.test")),
                Ꮡ(new global::go.net.mail_package.Address(
                    Name: "John",
                    ΔAddress: "jdoe@one.test"))
            ])
        ),
        new(
            @"Group1: <addr1@example.com>;, Group 2: addr2@example.com;, John <addr3@example.com>",
            slice<ж<global::go.net.mail_package.Address>>([
                Ꮡ(new global::go.net.mail_package.Address(
                    Name: "",
                    ΔAddress: "addr1@example.com")),
                Ꮡ(new global::go.net.mail_package.Address(
                    Name: "",
                    ΔAddress: "addr2@example.com")),
                Ꮡ(new global::go.net.mail_package.Address(
                    Name: "John",
                    ΔAddress: "addr3@example.com"))
            ])
        ),
        // RFC 2047 "Q"-encoded ISO-8859-1 address.
        new(
            @"=?iso-8859-1?q?J=F6rg_Doe?= <joerg@example.com>",
            slice<ж<global::go.net.mail_package.Address>>([
                Ꮡ(new global::go.net.mail_package.Address(
                    Name: @"Jörg Doe",
                    ΔAddress: "joerg@example.com"))
            ])
        ),
        // RFC 2047 "Q"-encoded US-ASCII address. Dumb but legal.
        new(
            @"=?us-ascii?q?J=6Frg_Doe?= <joerg@example.com>",
            slice<ж<global::go.net.mail_package.Address>>([
                Ꮡ(new global::go.net.mail_package.Address(
                    Name: @"Jorg Doe",
                    ΔAddress: "joerg@example.com"))
            ])
        ),
        // RFC 2047 "Q"-encoded UTF-8 address.
        new(
            @"=?utf-8?q?J=C3=B6rg_Doe?= <joerg@example.com>",
            slice<ж<global::go.net.mail_package.Address>>([
                Ꮡ(new global::go.net.mail_package.Address(
                    Name: @"Jörg Doe",
                    ΔAddress: "joerg@example.com"))
            ])
        ),
        // RFC 2047 "Q"-encoded UTF-8 address with multiple encoded-words.
        new(
            @"=?utf-8?q?J=C3=B6rg?=  =?utf-8?q?Doe?= <joerg@example.com>",
            slice<ж<global::go.net.mail_package.Address>>([
                Ꮡ(new global::go.net.mail_package.Address(
                    Name: @"JörgDoe",
                    ΔAddress: "joerg@example.com"))
            ])
        ),
        // RFC 2047, Section 8.
        new(
            @"=?ISO-8859-1?Q?Andr=E9?= Pirard <PIRARD@vm1.ulg.ac.be>",
            slice<ж<global::go.net.mail_package.Address>>([
                Ꮡ(new global::go.net.mail_package.Address(
                    Name: @"André Pirard",
                    ΔAddress: "PIRARD@vm1.ulg.ac.be"))
            ])
        ),
        // Custom example of RFC 2047 "B"-encoded ISO-8859-1 address.
        new(
            @"=?ISO-8859-1?B?SvZyZw==?= <joerg@example.com>",
            slice<ж<global::go.net.mail_package.Address>>([
                Ꮡ(new global::go.net.mail_package.Address(
                    Name: @"Jörg",
                    ΔAddress: "joerg@example.com"))
            ])
        ),
        // Custom example of RFC 2047 "B"-encoded UTF-8 address.
        new(
            @"=?UTF-8?B?SsO2cmc=?= <joerg@example.com>",
            slice<ж<global::go.net.mail_package.Address>>([
                Ꮡ(new global::go.net.mail_package.Address(
                    Name: @"Jörg",
                    ΔAddress: "joerg@example.com"))
            ])
        ),
        // Custom example with "." in name. For issue 4938
        new(
            @"Asem H. <noreply@example.com>",
            slice<ж<global::go.net.mail_package.Address>>([
                Ꮡ(new global::go.net.mail_package.Address(
                    Name: @"Asem H.",
                    ΔAddress: "noreply@example.com"))
            ])
        ),
        // RFC 6532 3.2.3, qtext /= UTF8-non-ascii
        new(
            @"""Gø Pher"" <gopher@example.com>",
            slice<ж<global::go.net.mail_package.Address>>([
                Ꮡ(new global::go.net.mail_package.Address(
                    Name: @"Gø Pher",
                    ΔAddress: "gopher@example.com"))
            ])
        ),
        // RFC 6532 3.2, atext /= UTF8-non-ascii
        new(
            @"µ <micro@example.com>",
            slice<ж<global::go.net.mail_package.Address>>([
                Ꮡ(new global::go.net.mail_package.Address(
                    Name: @"µ",
                    ΔAddress: "micro@example.com"))
            ])
        ),
        // RFC 6532 3.2.2, local address parts allow UTF-8
        new(
            @"Micro <µ@example.com>",
            slice<ж<global::go.net.mail_package.Address>>([
                Ꮡ(new global::go.net.mail_package.Address(
                    Name: @"Micro",
                    ΔAddress: "µ@example.com"))
            ])
        ),
        // RFC 6532 3.2.4, domains parts allow UTF-8
        new(
            @"Micro <micro@µ.example.com>",
            slice<ж<global::go.net.mail_package.Address>>([
                Ꮡ(new global::go.net.mail_package.Address(
                    Name: @"Micro",
                    ΔAddress: "micro@µ.example.com"))
            ])
        ),
        // Issue 14866
        new(
            @""""" <emptystring@example.com>",
            slice<ж<global::go.net.mail_package.Address>>([
                Ꮡ(new global::go.net.mail_package.Address(
                    Name: "",
                    ΔAddress: "emptystring@example.com"))
            ])
        ),
        // CFWS
        new(
            @"<cfws@example.com> (CFWS (cfws))  (another comment)",
            slice<ж<global::go.net.mail_package.Address>>([
                Ꮡ(new global::go.net.mail_package.Address(
                    Name: "",
                    ΔAddress: "cfws@example.com"))
            ])
        ),
        new(
            @"<cfws@example.com> ()  (another comment), <cfws2@example.com> (another)",
            slice<ж<global::go.net.mail_package.Address>>([
                Ꮡ(new global::go.net.mail_package.Address(
                    Name: "",
                    ΔAddress: "cfws@example.com")),
                Ꮡ(new global::go.net.mail_package.Address(
                    Name: "",
                    ΔAddress: "cfws2@example.com"))
            ])
        ),
        // Comment as display name
        new(
            @"john@example.com (John Doe)",
            slice<ж<global::go.net.mail_package.Address>>([
                Ꮡ(new global::go.net.mail_package.Address(
                    Name: "John Doe",
                    ΔAddress: "john@example.com"))
            ])
        ),
        // Comment and display name
        new(
            @"John Doe <john@example.com> (Joey)",
            slice<ж<global::go.net.mail_package.Address>>([
                Ꮡ(new global::go.net.mail_package.Address(
                    Name: "John Doe",
                    ΔAddress: "john@example.com"))
            ])
        ),
        // Comment as display name, no space
        new(
            @"john@example.com(John Doe)",
            slice<ж<global::go.net.mail_package.Address>>([
                Ꮡ(new global::go.net.mail_package.Address(
                    Name: "John Doe",
                    ΔAddress: "john@example.com"))
            ])
        ),
        // Comment as display name, Q-encoded
        new(
            @"asjo@example.com (Adam =?utf-8?Q?Sj=C3=B8gren?=)",
            slice<ж<global::go.net.mail_package.Address>>([
                Ꮡ(new global::go.net.mail_package.Address(
                    Name: "Adam Sjøgren",
                    ΔAddress: "asjo@example.com"))
            ])
        ),
        // Comment as display name, Q-encoded and tab-separated
        new(
            @"asjo@example.com (Adam	=?utf-8?Q?Sj=C3=B8gren?=)",
            slice<ж<global::go.net.mail_package.Address>>([
                Ꮡ(new global::go.net.mail_package.Address(
                    Name: "Adam Sjøgren",
                    ΔAddress: "asjo@example.com"))
            ])
        ),
        // Nested comment as display name, Q-encoded
        new(
            @"asjo@example.com (Adam =?utf-8?Q?Sj=C3=B8gren?= (Debian))",
            slice<ж<global::go.net.mail_package.Address>>([
                Ꮡ(new global::go.net.mail_package.Address(
                    Name: "Adam Sjøgren (Debian)",
                    ΔAddress: "asjo@example.com"))
            ])
        ),
        // Comment in group display name
        new(
            @"group (comment:): a@example.com, b@example.com;",
            slice<ж<global::go.net.mail_package.Address>>([
                Ꮡ(new global::go.net.mail_package.Address(
                    ΔAddress: "a@example.com")),
                Ꮡ(new global::go.net.mail_package.Address(
                    ΔAddress: "b@example.com"))
            ])
        ),
        new(
            @"x(:""):""@a.example;(""@b.example;",
            slice<ж<global::go.net.mail_package.Address>>([
                Ꮡ(new global::go.net.mail_package.Address(
                    ΔAddress: @"@a.example;(@b.example"))
            ])
        ),
        // Domain-literal
        new(
            @"jdoe@[192.168.0.1]",
            slice<ж<global::go.net.mail_package.Address>>([Ꮡ(new global::go.net.mail_package.Address(
                ΔAddress: "jdoe@[192.168.0.1]"))
            ])
        ),
        new(
            @"John Doe <jdoe@[192.168.0.1]>",
            slice<ж<global::go.net.mail_package.Address>>([Ꮡ(new global::go.net.mail_package.Address(
                Name: "John Doe",
                ΔAddress: "jdoe@[192.168.0.1]"))
            ])
        )
    ]);
    foreach (var (_, test) in tests) {
        if (builtin.len(test.exp) == 1) {
            var (addr, errΔ1) = ParseAddress(test.addrsStr);
            if (errΔ1 != nil) {
                t.Errorf("Failed parsing (single) %q: %v", test.addrsStr, errΔ1);
                continue;
            }
            if (!reflect.DeepEqual(slice<ж<global::go.net.mail_package.Address>>([addr]), test.exp)) {
                t.Errorf("Parse (single) of %q: got %+v, want %+v", test.addrsStr, addr.OrTypedNil(), test.exp);
            }
        }

        var (addrs, err) = ParseAddressList(test.addrsStr);
        if (err != nil) {
            t.Errorf("Failed parsing (list) %q: %v", test.addrsStr, err);
            continue;
        }
        if (!reflect.DeepEqual(addrs, test.exp)) {
            t.Errorf("Parse (list) of %q: got %+v, want %+v", test.addrsStr, addrs, test.exp);
        }
    }
}

internal partial struct TestAddressParser_tests /*dyn*/ {
    internal @string addrsStr;
    internal slice<ж<global::go.net.mail_package.Address>> exp;
}

public static void TestAddressParser(ж<testing.T> t) {
    var tests = slice<TestAddressParser_tests>([
        // Bare address
        new(
            @"jdoe@machine.example",
            slice<ж<global::go.net.mail_package.Address>>([Ꮡ(new global::go.net.mail_package.Address(
                ΔAddress: "jdoe@machine.example"))
            ])
        ),
        // RFC 5322, Appendix A.1.1
        new(
            @"John Doe <jdoe@machine.example>",
            slice<ж<global::go.net.mail_package.Address>>([Ꮡ(new global::go.net.mail_package.Address(
                Name: "John Doe",
                ΔAddress: "jdoe@machine.example"))
            ])
        ),
        // RFC 5322, Appendix A.1.2
        new(
            @"""Joe Q. Public"" <john.q.public@example.com>",
            slice<ж<global::go.net.mail_package.Address>>([Ꮡ(new global::go.net.mail_package.Address(
                Name: "Joe Q. Public",
                ΔAddress: "john.q.public@example.com"))
            ])
        ),
        new(
            @"Mary Smith <mary@x.test>, jdoe@example.org, Who? <one@y.test>",
            slice<ж<global::go.net.mail_package.Address>>([
                Ꮡ(new global::go.net.mail_package.Address(
                    Name: "Mary Smith",
                    ΔAddress: "mary@x.test")),
                Ꮡ(new global::go.net.mail_package.Address(
                    ΔAddress: "jdoe@example.org")),
                Ꮡ(new global::go.net.mail_package.Address(
                    Name: "Who?",
                    ΔAddress: "one@y.test"))
            ])
        ),
        new(
            @"<boss@nil.test>, ""Giant; \""Big\"" Box"" <sysservices@example.net>",
            slice<ж<global::go.net.mail_package.Address>>([
                Ꮡ(new global::go.net.mail_package.Address(
                    ΔAddress: "boss@nil.test")),
                Ꮡ(new global::go.net.mail_package.Address(
                    Name: @"Giant; ""Big"" Box",
                    ΔAddress: "sysservices@example.net"))
            ])
        ),
        // RFC 2047 "Q"-encoded ISO-8859-1 address.
        new(
            @"=?iso-8859-1?q?J=F6rg_Doe?= <joerg@example.com>",
            slice<ж<global::go.net.mail_package.Address>>([
                Ꮡ(new global::go.net.mail_package.Address(
                    Name: @"Jörg Doe",
                    ΔAddress: "joerg@example.com"))
            ])
        ),
        // RFC 2047 "Q"-encoded US-ASCII address. Dumb but legal.
        new(
            @"=?us-ascii?q?J=6Frg_Doe?= <joerg@example.com>",
            slice<ж<global::go.net.mail_package.Address>>([
                Ꮡ(new global::go.net.mail_package.Address(
                    Name: @"Jorg Doe",
                    ΔAddress: "joerg@example.com"))
            ])
        ),
        // RFC 2047 "Q"-encoded ISO-8859-15 address.
        new(
            @"=?ISO-8859-15?Q?J=F6rg_Doe?= <joerg@example.com>",
            slice<ж<global::go.net.mail_package.Address>>([
                Ꮡ(new global::go.net.mail_package.Address(
                    Name: @"Jörg Doe",
                    ΔAddress: "joerg@example.com"))
            ])
        ),
        // RFC 2047 "B"-encoded windows-1252 address.
        new(
            @"=?windows-1252?q?Andr=E9?= Pirard <PIRARD@vm1.ulg.ac.be>",
            slice<ж<global::go.net.mail_package.Address>>([
                Ꮡ(new global::go.net.mail_package.Address(
                    Name: @"André Pirard",
                    ΔAddress: "PIRARD@vm1.ulg.ac.be"))
            ])
        ),
        // Custom example of RFC 2047 "B"-encoded ISO-8859-15 address.
        new(
            @"=?ISO-8859-15?B?SvZyZw==?= <joerg@example.com>",
            slice<ж<global::go.net.mail_package.Address>>([
                Ꮡ(new global::go.net.mail_package.Address(
                    Name: @"Jörg",
                    ΔAddress: "joerg@example.com"))
            ])
        ),
        // Custom example of RFC 2047 "B"-encoded UTF-8 address.
        new(
            @"=?UTF-8?B?SsO2cmc=?= <joerg@example.com>",
            slice<ж<global::go.net.mail_package.Address>>([
                Ꮡ(new global::go.net.mail_package.Address(
                    Name: @"Jörg",
                    ΔAddress: "joerg@example.com"))
            ])
        ),
        // Custom example with "." in name. For issue 4938
        new(
            @"Asem H. <noreply@example.com>",
            slice<ж<global::go.net.mail_package.Address>>([
                Ꮡ(new global::go.net.mail_package.Address(
                    Name: @"Asem H.",
                    ΔAddress: "noreply@example.com"))
            ])
        ),
        // Domain-literal
        new(
            @"jdoe@[192.168.0.1]",
            slice<ж<global::go.net.mail_package.Address>>([Ꮡ(new global::go.net.mail_package.Address(
                ΔAddress: "jdoe@[192.168.0.1]"))
            ])
        ),
        new(
            @"John Doe <jdoe@[192.168.0.1]>",
            slice<ж<global::go.net.mail_package.Address>>([Ꮡ(new global::go.net.mail_package.Address(
                Name: "John Doe",
                ΔAddress: "jdoe@[192.168.0.1]"))
            ])
        )
    ]);

    var ap = new AddressParser(WordDecoder: Ꮡ(new mime.WordDecoder(
        CharsetReader: (@string charset, io.Reader input) => {
            var (@in, err) = io.ReadAll(input);
            if (err != nil) {
                return (default, err);
            }

            var exprᴛ1 = charset;
            switch (ᐧ) {
            case {} when exprᴛ1 == "iso-8859-15"u8: {
                @in = bytes.ReplaceAll(@in, slice<byte>(((@string)(new byte[]{0xf6}))), slice<byte>("ö"u8));
                break;
            }
            case {} when exprᴛ1 == "windows-1252"u8: {
                @in = bytes.ReplaceAll(@in, slice<byte>(((@string)(new byte[]{0xe9}))), slice<byte>("é"u8));
                break;
            }}

            return (new mail_test_package.bytes_ReaderжReader(bytes.NewReader(@in)), default);
        }
    ))
    );

    foreach (var (_, test) in tests) {
        if (builtin.len(test.exp) == 1) {
            var (addr, errΔ1) = ap.Parse(test.addrsStr);
            if (errΔ1 != nil) {
                t.Errorf("Failed parsing (single) %q: %v", test.addrsStr, errΔ1);
                continue;
            }
            if (!reflect.DeepEqual(slice<ж<global::go.net.mail_package.Address>>([addr]), test.exp)) {
                t.Errorf("Parse (single) of %q: got %+v, want %+v", test.addrsStr, addr.OrTypedNil(), test.exp);
            }
        }

        var (addrs, err) = ap.ParseList(test.addrsStr);
        if (err != nil) {
            t.Errorf("Failed parsing (list) %q: %v", test.addrsStr, err);
            continue;
        }
        if (!reflect.DeepEqual(addrs, test.exp)) {
            t.Errorf("Parse (list) of %q: got %+v, want %+v", test.addrsStr, addrs, test.exp);
        }
    }
}

internal partial struct TestAddressString_tests /*dyn*/ {
    internal ж<global::go.net.mail_package.Address> addr;
    internal @string exp;
}

public static void TestAddressString(ж<testing.T> t) {
    var tests = slice<TestAddressString_tests>([
        new(
            Ꮡ(new Address(ΔAddress: "bob@example.com")),
            "<bob@example.com>"
        ),
        new( // quoted local parts: RFC 5322, 3.4.1. and 3.2.4.
            Ꮡ(new Address(ΔAddress: @"my@idiot@address@example.com")),
            @"<""my@idiot@address""@example.com>"
        ),
        new( // quoted local parts
            Ꮡ(new Address(ΔAddress: @" @example.com")),
            @"<"" ""@example.com>"
        ),
        new(
            Ꮡ(new Address(Name: "Bob", ΔAddress: "bob@example.com")),
            @"""Bob"" <bob@example.com>"
        ),
        new(
            // note the ö (o with an umlaut)
            Ꮡ(new Address(Name: "Böb", ΔAddress: "bob@example.com")),
            @"=?utf-8?q?B=C3=B6b?= <bob@example.com>"
        ),
        new(
            Ꮡ(new Address(Name: "Bob Jane", ΔAddress: "bob@example.com")),
            @"""Bob Jane"" <bob@example.com>"
        ),
        new(
            Ꮡ(new Address(Name: "Böb Jacöb", ΔAddress: "bob@example.com")),
            @"=?utf-8?q?B=C3=B6b_Jac=C3=B6b?= <bob@example.com>"
        ),
        new( // https://golang.org/issue/12098
            Ꮡ(new Address(Name: "Rob", ΔAddress: "")),
            @"""Rob"" <@>"
        ),
        new( // https://golang.org/issue/12098
            Ꮡ(new Address(Name: "Rob", ΔAddress: "@")),
            @"""Rob"" <@>"
        ),
        new(
            Ꮡ(new Address(Name: "Böb, Jacöb", ΔAddress: "bob@example.com")),
            @"=?utf-8?b?QsO2YiwgSmFjw7Zi?= <bob@example.com>"
        ),
        new(
            Ꮡ(new Address(Name: "=??Q?x?=", ΔAddress: "hello@world.com")),
            @"""=??Q?x?="" <hello@world.com>"
        ),
        new(
            Ꮡ(new Address(Name: "=?hello", ΔAddress: "hello@world.com")),
            @"""=?hello"" <hello@world.com>"
        ),
        new(
            Ꮡ(new Address(Name: "world?=", ΔAddress: "hello@world.com")),
            @"""world?="" <hello@world.com>"
        ),
        new(
            // should q-encode even for invalid utf-8.
            Ꮡ(new Address(Name: ((@string)slice<byte>([0xed, 0xa0, 0x80])), ΔAddress: "invalid-utf8@example.net")),
            "=?utf-8?q?=ED=A0=80?= <invalid-utf8@example.net>"
        ),
        // Domain-literal
        new(
            Ꮡ(new Address(ΔAddress: "bob@[192.168.0.1]")),
            "<bob@[192.168.0.1]>"
        ),
        new(
            Ꮡ(new Address(Name: "Bob", ΔAddress: "bob@[192.168.0.1]")),
            @"""Bob"" <bob@[192.168.0.1]>"
        )
    ]);
    foreach (var (_, test) in tests) {
        @string s = test.addr.String();
        if (s != test.exp) {
            t.Errorf("Address%+v.String() = %v, want %v", test.addr.Value, s, test.exp);
            continue;
        }

        // Check round-trip.
        if (test.addr.ΔAddress != ""u8 && test.addr.ΔAddress != "@"u8) {
            var (a, err) = ParseAddress(test.exp);
            if (err != nil) {
                t.Errorf("ParseAddress(%#q): %v", test.exp, err);
                continue;
            }
            if (a.Name != test.addr.Name || a.ΔAddress != test.addr.ΔAddress) {
                t.Errorf("ParseAddress(%#q) = %#v, want %#v", test.exp, a.OrTypedNil(), test.addr.OrTypedNil());
            }
        }
    }
}

// Check if all valid addresses can be parsed, formatted and parsed again
public static void TestAddressParsingAndFormatting(ж<testing.T> t) {
    // Should pass
    var tests = slice<@string>([
        @"<Bob@example.com>",
        @"<bob.bob@example.com>",
        @"<"".bob""@example.com>",
        @"<"" ""@example.com>",
        @"<some.mail-with-dash@example.com>",
        @"<""dot.and space""@example.com>",
        @"<""very.unusual.@.unusual.com""@example.com>",
        @"<admin@mailserver1>",
        @"<postmaster@localhost>",
        "<#!$%&'*+-/=?^_`{}|~@example.org>",
        @"<""very.(),:;<>[]\"".VERY.\""very@\\ \""very\"".unusual""@strange.example.com>", // escaped quotes
        @"<""()<>[]:,;@\\\""!#$%&'*+-/=?^_{}| ~.a""@example.org>",                         // escaped backslashes
        @"<""Abc\\@def""@example.com>",
        @"<""Joe\\Blow""@example.com>",
        @"<test1/test2=test3@example.com>",
        @"<def!xyz%abc@example.com>",
        @"<_somename@example.com>",
        @"<joe@uk>",
        @"<~@example.com>",
        @"<""...""@test.com>",
        @"<""john..doe""@example.com>",
        @"<""john.doe.""@example.com>",
        @"<"".john.doe""@example.com>",
        @"<"".""@example.com>",
        @"<""..""@example.com>",
        @"<""0:""@0>",
        @"<Bob@[192.168.0.1]>"
    ]);

    foreach (var (_, test) in tests) {
        var (addr, err) = ParseAddress(test);
        if (err != nil) {
            t.Errorf("Couldn't parse address %s: %s", test, err.Error());
            continue;
        }
        @string str = addr.String();
        (addr, err) = ParseAddress(str);
        if (err != nil) {
            t.Errorf("ParseAddr(%q) error: %v", test, err);
            continue;
        }

        if (addr.String() != test) {
            t.Errorf("String() round-trip = %q; want %q", addr.OrTypedNil(), test);
            continue;
        }
    }

    // Should fail
    var badTests = slice<@string>([
        @"<Abc.example.com>",
        @"<A@b@c@example.com>",
        @"<a""b(c)d,e:f;g<h>i[j\k]l@example.com>",
        @"<just""not""right@example.com>",
        @"<this is""not\allowed@example.com>",
        @"<this\ still\""not\\allowed@example.com>",
        @"<john..doe@example.com>",
        @"<john.doe@example..com>",
        @"<john.doe@example..com>",
        @"<john.doe.@example.com>",
        @"<john.doe.@.example.com>",
        @"<.john.doe@example.com>",
        @"<@example.com>",
        @"<.@example.com>",
        @"<test@.>",
        @"< @example.com>",
        @"<""""test""""blah""""@example.com>",
        @"<""""@0>"
    ]);

    foreach (var (_, test) in badTests) {
        var (_, err) = ParseAddress(test);
        if (err == nil) {
            t.Errorf("Should have failed to parse address: %s", test);
            continue;
        }
    }
}

public static void TestAddressFormattingAndParsing(ж<testing.T> t) {
    var tests = slice<ж<global::go.net.mail_package.Address>>([
        Ꮡ(new global::go.net.mail_package.Address(Name: "@lïce", ΔAddress: "alice@example.com")),
        Ꮡ(new global::go.net.mail_package.Address(Name: "Böb O'Connor", ΔAddress: "bob@example.com")),
        Ꮡ(new global::go.net.mail_package.Address(Name: "???", ΔAddress: "bob@example.com")),
        Ꮡ(new global::go.net.mail_package.Address(Name: "Böb ???", ΔAddress: "bob@example.com")),
        Ꮡ(new global::go.net.mail_package.Address(Name: "Böb (Jacöb)", ΔAddress: "bob@example.com")),
        Ꮡ(new global::go.net.mail_package.Address(Name: "à#$%&'(),.:;<>@[]^`{|}~'", ΔAddress: "bob@example.com")),
        // https://golang.org/issue/11292
        Ꮡ(new global::go.net.mail_package.Address(Name: "\"\\\x1f,\"", ΔAddress: "0@0")),
        // https://golang.org/issue/12782
        Ꮡ(new global::go.net.mail_package.Address(Name: "naé, mée", ΔAddress: "test.mail@gmail.com"))
    ]);

    foreach (var (i, test) in tests) {
        var (parsed, err) = ParseAddress(test.String());
        if (err != nil) {
            t.Errorf("test #%d: ParseAddr(%q) error: %v", i, test.String(), err);
            continue;
        }
        if (parsed.Name != test.Name) {
            t.Errorf("test #%d: Parsed name = %q; want %q", i, parsed.Name, test.Name);
        }
        if (parsed.ΔAddress != test.ΔAddress) {
            t.Errorf("test #%d: Parsed address = %q; want %q", i, parsed.ΔAddress, test.ΔAddress);
        }
    }
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static ref readonly @string aBCDˢ => ref aBCDˢᵇ;
private static readonly @string aBCDˢᵇ = "a@b c@d"u8;

public static void TestEmptyAddress(ж<testing.T> t) {
    var (parsed, err) = ParseAddress(""u8);
    if (parsed != nil || err == nil) {
        t.Errorf(@"ParseAddress("""") = %v, %v, want nil, error", parsed.OrTypedNil(), err);
    }
    (var list, err) = ParseAddressList(""u8);
    if (builtin.len(list) > 0 || err == nil) {
        t.Errorf(@"ParseAddressList("""") = %v, %v, want nil, error", list, err);
    }
    (list, err) = ParseAddressList(",");
    if (builtin.len(list) > 0 || err == nil) {
        t.Errorf(@"ParseAddressList("""") = %v, %v, want nil, error", list, err);
    }
    (list, err) = ParseAddressList(aBCDˢ);
    if (builtin.len(list) > 0 || err == nil) {
        t.Errorf(@"ParseAddressList("""") = %v, %v, want nil, error", list, err);
    }
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static ref readonly @string utf8QHelloˢ => ref utf8QHelloˢᵇ;
private static readonly @string utf8QHelloˢᵇ = "=?utf-8?q?hello?= "u8;

public static void BenchmarkConsumePhrase(ж<testing.B> b) {
    foreach (var (_, n) in slice<nint>([10, 100, 1000, 10000])) {
        b.Run(fmt.Sprintf("words-%d"u8, n), (ж<testing.B> bΔ1) => {
            ref var input = ref heap<@string>(strings.Repeat(utf8QHelloˢ, n) + "<user@example.com>"u8, out var Ꮡinput);
            while (bΔ1.Loop()) {
                (Ꮡ(new addrParser(s: input))).consumePhrase();
            }
        });
    }
}

public static void BenchmarkConsumeComment(ж<testing.B> b) {
    foreach (var (_, n) in slice<nint>([10, 100, 1000, 10000])) {
        b.Run(fmt.Sprintf("depth-%d"u8, n), (ж<testing.B> bΔ1) => {
            // Build a deeply nested comment: (((...a...)))
            @string open = strings.Repeat("(", n);
            @string close = strings.Repeat(")", n);
            // consumeComment expects the leading '(' already consumed,
            // so we start with one fewer opening paren and the parser
            // will handle nesting from there.
            @string input = open.slice(0, n - 1) + "a" + close;
            while (bΔ1.Loop()) {
                var p = new addrParser(s: input);
                p.consumeComment();
            }
        });
    }
}

} // end mail_internal_test_package
