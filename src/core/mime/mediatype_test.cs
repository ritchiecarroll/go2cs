// Copyright 2010 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.
namespace go;

using maps = maps_package;
using strings = strings_package;
using testing = testing_package;
using static go.mime_package;
using ꓸꓸꓸstring = Span<@string>;

partial class mime_internal_test_package {

public static void TestConsumeToken(ж<testing.T> t) {
    var tests = new array<@string>[]{
        new @string[]{"foo bar", "foo", " bar"}.array(),
        new @string[]{"bar", "bar", ""}.array(),
        new @string[]{"", "", ""}.array(),
        new @string[]{" foo", "", " foo"}.array()
    }.array();
    foreach (var (_, vᴛ1) in range(tests.ΔRangeSnapshot())) {
        var test = vᴛ1.Clone();

        var (token, rest) = consumeToken(test[0]);
        @string expectedToken = test[1];
        @string expectedRest = test[2];
        if (token != expectedToken) {
            t.Errorf("expected to consume token '%s', not '%s' from '%s'",
                expectedToken, token, test[0]);
        } else if (rest != expectedRest) {
            t.Errorf("expected to have left '%s', not '%s' after reading token '%s' from '%s'",
                expectedRest, rest, token, test[0]);
        }
    }
}

public static void TestConsumeValue(ж<testing.T> t) {
    var tests = new array<@string>[]{
        new @string[]{"foo bar", "foo", " bar"}.array(),
        new @string[]{"bar", "bar", ""}.array(),
        new @string[]{" bar ", "", " bar "}.array(),
        new @string[]{@"""My value""end", "My value", "end"}.array(),
        new @string[]{@"""My value"" end", "My value", " end"}.array(),
        new @string[]{@"""\\"" rest", "\\", " rest"}.array(),
        new @string[]{@"""My \"" value""end", "My \" value", "end"}.array(),
        new @string[]{@"""\"" rest", "", @"""\"" rest"}.array(),
        new @string[]{@"""C:\dev\go\robots.txt""", @"C:\dev\go\robots.txt", ""}.array(),
        new @string[]{@"""C:\新建文件夹\中文第二次测试.mp4""", @"C:\新建文件夹\中文第二次测试.mp4", ""}.array()
    }.array();
    foreach (var (_, vᴛ1) in range(tests.ΔRangeSnapshot())) {
        var test = vᴛ1.Clone();

        var (value, rest) = consumeValue(test[0]);
        @string expectedValue = test[1];
        @string expectedRest = test[2];
        if (value != expectedValue) {
            t.Errorf("expected to consume value [%s], not [%s] from [%s]",
                expectedValue, value, test[0]);
        } else if (rest != expectedRest) {
            t.Errorf("expected to have left [%s], not [%s] after reading value [%s] from [%s]",
                expectedRest, rest, value, test[0]);
        }
    }
}

public static void TestConsumeMediaParam(ж<testing.T> t) {
    var tests = new array<@string>[]{
        new @string[]{" ; foo=bar", "foo", "bar", ""}.array(),
        new @string[]{"; foo=bar", "foo", "bar", ""}.array(),
        new @string[]{";foo=bar", "foo", "bar", ""}.array(),
        new @string[]{";FOO=bar", "foo", "bar", ""}.array(),
        new @string[]{@";foo=""bar""", "foo", "bar", ""}.array(),
        new @string[]{@";foo=""bar""; ", "foo", "bar", "; "}.array(),
        new @string[]{@";foo=""bar""; foo=baz", "foo", "bar", "; foo=baz"}.array(),
        new @string[]{@" ; boundary=----CUT;", "boundary", "----CUT", ";"}.array(),
        new @string[]{@" ; key=value;  blah=""value"";name=""foo"" ", "key", "value", @";  blah=""value"";name=""foo"" "}.array(),
        new @string[]{@";  blah=""value"";name=""foo"" ", "blah", "value", @";name=""foo"" "}.array(),
        new @string[]{@";name=""foo"" ", "name", "foo", @" "}.array()
    }.array();
    foreach (var (_, vᴛ1) in range(tests.ΔRangeSnapshot())) {
        var test = vᴛ1.Clone();

        var (param, value, rest) = consumeMediaParam(test[0]);
        @string expectedParam = test[1];
        @string expectedValue = test[2];
        @string expectedRest = test[3];
        if (param != expectedParam) {
            t.Errorf("expected to consume param [%s], not [%s] from [%s]",
                expectedParam, param, test[0]);
        } else if (value != expectedValue) {
            t.Errorf("expected to consume value [%s], not [%s] from [%s]",
                expectedValue, value, test[0]);
        } else if (rest != expectedRest) {
            t.Errorf("expected to have left [%s], not [%s] after reading [%s/%s] from [%s]",
                expectedRest, rest, param, value, test[0]);
        }
    }
}

internal partial struct mediaTypeTest {
    internal @string @in;
    internal @string t;
    internal map<@string, @string> p;
}

internal static slice<mediaTypeTest> parseMediaTypeTests;

[GoInit] internal static void init() {
    // Convenience map initializer
    map<@string, @string> m(params ꓸꓸꓸstring sʗp) {
        var s = sʗp.sslice();
        var sm = new map<@string, @string>();
        for (nint i = 0; i < len(s); i += 2) {
            sm[s[i]] = s[i + 1];
        }
        return sm;
    }

    var nameFoo = new map<@string, @string>{["name"] = "foo"};
    parseMediaTypeTests = slice<mediaTypeTest>([
        new(@"form-data; name=""foo""", "form-data", nameFoo),
        new(@" form-data ; name=foo", "form-data", nameFoo),
        new(@"FORM-DATA;name=""foo""", "form-data", nameFoo),
        new(@" FORM-DATA ; name=""foo""", "form-data", nameFoo),
        new(@" FORM-DATA ; name=""foo""", "form-data", nameFoo),

        new(@"form-data; key=value;  blah=""value"";name=""foo"" ",
            "form-data",
            m("key", "value", "blah", "value", "name", "foo")),

        new(@"foo; key=val1; key=the-key-appears-again-which-is-bogus",
            "", m()),

        // From RFC 2231:
        new(@"application/x-stuff; title*=us-ascii'en-us'This%20is%20%2A%2A%2Afun%2A%2A%2A",
            "application/x-stuff",
            m("title", "This is ***fun***")),

        new(@"message/external-body; access-type=URL; "u8 + @"URL*0=""ftp://"";"u8 + @"URL*1=""cs.utk.edu/pub/moore/bulk-mailer/bulk-mailer.tar"""u8,
            "message/external-body",
            m("access-type", "URL",
                "url", "ftp://cs.utk.edu/pub/moore/bulk-mailer/bulk-mailer.tar")),

        new(@"application/x-stuff; "u8 + @"title*0*=us-ascii'en'This%20is%20even%20more%20; "u8 + @"title*1*=%2A%2A%2Afun%2A%2A%2A%20; "u8 + @"title*2=""isn't it!"""u8,
            "application/x-stuff",
            m("title", "This is even more ***fun*** isn't it!")),

        // Tests from http://greenbytes.de/tech/tc2231/
        // Note: Backslash escape handling is a bit loose, like MSIE.

        // #attonly
        new(@"attachment",
            "attachment",
            m()),
        // #attonlyucase
        new(@"ATTACHMENT",
            "attachment",
            m()),
        // #attwithasciifilename
        new(@"attachment; filename=""foo.html""",
            "attachment",
            m("filename", "foo.html")),
        // #attwithasciifilename25
        new(@"attachment; filename=""0000000000111111111122222""",
            "attachment",
            m("filename", "0000000000111111111122222")),
        // #attwithasciifilename35
        new(@"attachment; filename=""00000000001111111111222222222233333""",
            "attachment",
            m("filename", "00000000001111111111222222222233333")),
        // #attwithasciifnescapedchar
        new(@"attachment; filename=""f\oo.html""",
            "attachment",
            m("filename", "f\\oo.html")),
        // #attwithasciifnescapedquote
        new(@"attachment; filename=""\""quoting\"" tested.html""",
            "attachment",
            m("filename", @"""quoting"" tested.html")),
        // #attwithquotedsemicolon
        new(@"attachment; filename=""Here's a semicolon;.html""",
            "attachment",
            m("filename", "Here's a semicolon;.html")),
        // #attwithfilenameandextparam
        new(@"attachment; foo=""bar""; filename=""foo.html""",
            "attachment",
            m("foo", "bar", "filename", "foo.html")),
        // #attwithfilenameandextparamescaped
        new(@"attachment; foo=""\""\\"";filename=""foo.html""",
            "attachment",
            m("foo", "\"\\", "filename", "foo.html")),
        // #attwithasciifilenameucase
        new(@"attachment; FILENAME=""foo.html""",
            "attachment",
            m("filename", "foo.html")),
        // #attwithasciifilenamenq
        new(@"attachment; filename=foo.html",
            "attachment",
            m("filename", "foo.html")),
        // #attwithasciifilenamenqs
        new(@"attachment; filename=foo.html ;",
            "attachment",
            m("filename", "foo.html")),
        // #attwithfntokensq
        new(@"attachment; filename='foo.html'",
            "attachment",
            m("filename", "'foo.html'")),
        // #attwithisofnplain
        new(@"attachment; filename=""foo-ä.html""",
            "attachment",
            m("filename", "foo-ä.html")),
        // #attwithutf8fnplain
        new(@"attachment; filename=""foo-Ã¤.html""",
            "attachment",
            m("filename", "foo-Ã¤.html")),
        // #attwithfnrawpctenca
        new(@"attachment; filename=""foo-%41.html""",
            "attachment",
            m("filename", "foo-%41.html")),
        // #attwithfnusingpct
        new(@"attachment; filename=""50%.html""",
            "attachment",
            m("filename", "50%.html")),
        // #attwithfnrawpctencaq
        new(@"attachment; filename=""foo-%\41.html""",
            "attachment",
            m("filename", "foo-%\\41.html")),
        // #attwithnamepct
        new(@"attachment; name=""foo-%41.html""",
            "attachment",
            m("name", "foo-%41.html")),
        // #attwithfilenamepctandiso
        new(@"attachment; name=""ä-%41.html""",
            "attachment",
            m("name", "ä-%41.html")),
        // #attwithfnrawpctenclong
        new(@"attachment; filename=""foo-%c3%a4-%e2%82%ac.html""",
            "attachment",
            m("filename", "foo-%c3%a4-%e2%82%ac.html")),
        // #attwithasciifilenamews1
        new(@"attachment; filename =""foo.html""",
            "attachment",
            m("filename", "foo.html")),
        // #attmissingdisposition
        new(@"filename=foo.html",
            "", m()),
        // #attmissingdisposition2
        new(@"x=y; filename=foo.html",
            "", m()),
        // #attmissingdisposition3
        new(@"""foo; filename=bar;baz""; filename=qux",
            "", m()),
        // #attmissingdisposition4
        new(@"filename=foo.html, filename=bar.html",
            "", m()),
        // #emptydisposition
        new(@"; filename=foo.html",
            "", m()),
        // #doublecolon
        new(@": inline; attachment; filename=foo.html",
            "", m()),
        // #attandinline
        new(@"inline; attachment; filename=foo.html",
            "", m()),
        // #attandinline2
        new(@"attachment; inline; filename=foo.html",
            "", m()),
        // #attbrokenquotedfn
        new(@"attachment; filename=""foo.html"".txt",
            "", m()),
        // #attbrokenquotedfn2
        new(@"attachment; filename=""bar",
            "", m()),
        // #attbrokenquotedfn3
        new(@"attachment; filename=foo""bar;baz""qux",
            "", m()),
        // #attmultinstances
        new(@"attachment; filename=foo.html, attachment; filename=bar.html",
            "", m()),
        // #attmissingdelim
        new(@"attachment; foo=foo filename=bar",
            "", m()),
        // #attmissingdelim2
        new(@"attachment; filename=bar foo=foo",
            "", m()),
        // #attmissingdelim3
        new(@"attachment filename=bar",
            "", m()),
        // #attreversed
        new(@"filename=foo.html; attachment",
            "", m()),
        // #attconfusedparam
        new(@"attachment; xfilename=foo.html",
            "attachment",
            m("xfilename", "foo.html")),
        // #attcdate
        new(@"attachment; creation-date=""Wed, 12 Feb 1997 16:29:51 -0500""",
            "attachment",
            m("creation-date", "Wed, 12 Feb 1997 16:29:51 -0500")),
        // #attmdate
        new(@"attachment; modification-date=""Wed, 12 Feb 1997 16:29:51 -0500""",
            "attachment",
            m("modification-date", "Wed, 12 Feb 1997 16:29:51 -0500")),
        // #dispext
        new(@"foobar", "foobar", m()),
        // #dispextbadfn
        new(@"attachment; example=""filename=example.txt""",
            "attachment",
            m("example", "filename=example.txt")),
        // #attwithfn2231utf8
        new(@"attachment; filename*=UTF-8''foo-%c3%a4-%e2%82%ac.html",
            "attachment",
            m("filename", "foo-ä-€.html")),
        // #attwithfn2231noc
        new(@"attachment; filename*=''foo-%c3%a4-%e2%82%ac.html",
            "attachment",
            m()),
        // #attwithfn2231utf8comp
        new(@"attachment; filename*=UTF-8''foo-a%cc%88.html",
            "attachment",
            m("filename", "foo-ä.html")),
        // #attwithfn2231ws2
        new(@"attachment; filename*= UTF-8''foo-%c3%a4.html",
            "attachment",
            m("filename", "foo-ä.html")),
        // #attwithfn2231ws3
        new(@"attachment; filename* =UTF-8''foo-%c3%a4.html",
            "attachment",
            m("filename", "foo-ä.html")),
        // #attwithfn2231quot
        new(@"attachment; filename*=""UTF-8''foo-%c3%a4.html""",
            "attachment",
            m("filename", "foo-ä.html")),
        // #attwithfn2231quot2
        new(@"attachment; filename*=""foo%20bar.html""",
            "attachment",
            m()),
        // #attwithfn2231singleqmissing
        new(@"attachment; filename*=UTF-8'foo-%c3%a4.html",
            "attachment",
            m()),
        // #attwithfn2231nbadpct1
        new(@"attachment; filename*=UTF-8''foo%",
            "attachment",
            m()),
        // #attwithfn2231nbadpct2
        new(@"attachment; filename*=UTF-8''f%oo.html",
            "attachment",
            m()),
        // #attwithfn2231dpct
        new(@"attachment; filename*=UTF-8''A-%2541.html",
            "attachment",
            m("filename", "A-%41.html")),
        // #attfncont
        new(@"attachment; filename*0=""foo.""; filename*1=""html""",
            "attachment",
            m("filename", "foo.html")),
        // #attfncontenc
        new(@"attachment; filename*0*=UTF-8''foo-%c3%a4; filename*1="".html""",
            "attachment",
            m("filename", "foo-ä.html")),
        // #attfncontlz
        new(@"attachment; filename*0=""foo""; filename*01=""bar""",
            "attachment",
            m("filename", "foo")),
        // #attfncontnc
        new(@"attachment; filename*0=""foo""; filename*2=""bar""",
            "attachment",
            m("filename", "foo")),
        // #attfnconts1
        new(@"attachment; filename*1=""foo.""; filename*2=""html""",
            "attachment", m()),
        // #attfncontord
        new(@"attachment; filename*1=""bar""; filename*0=""foo""",
            "attachment",
            m("filename", "foobar")),
        // #attfnboth
        new(@"attachment; filename=""foo-ae.html""; filename*=UTF-8''foo-%c3%a4.html",
            "attachment",
            m("filename", "foo-ä.html")),
        // #attfnboth2
        new(@"attachment; filename*=UTF-8''foo-%c3%a4.html; filename=""foo-ae.html""",
            "attachment",
            m("filename", "foo-ä.html")),
        // #attfnboth3
        new(@"attachment; filename*0*=ISO-8859-15''euro-sign%3d%a4; filename*=ISO-8859-1''currency-sign%3d%a4",
            "attachment",
            m()),
        // #attnewandfn
        new(@"attachment; foobar=x; filename=""foo.html""",
            "attachment",
            m("foobar", "x", "filename", "foo.html")),

        // Browsers also just send UTF-8 directly without RFC 2231,
        // at least when the source page is served with UTF-8.
        new(@"form-data; firstname=""Брэд""; lastname=""Фицпатрик""",
            "form-data",
            m("firstname", "Брэд", "lastname", "Фицпатрик")),

        // Empty string used to be mishandled.
        new(@"foo; bar=""""", "foo", m("bar", "")),

        // Microsoft browsers in intranet mode do not think they need to escape \ in file name.
        new(@"form-data; name=""file""; filename=""C:\dev\go\robots.txt""", "form-data", m("name", "file", "filename", @"C:\dev\go\robots.txt")),
        new(@"form-data; name=""file""; filename=""C:\新建文件夹\中文第二次测试.mp4""", "form-data", m("name", "file", "filename", @"C:\新建文件夹\中文第二次测试.mp4")),

        // issue #46323 (https://github.com/golang/go/issues/46323)
        new(
            // example from rfc2231-p.3 (https://datatracker.ietf.org/doc/html/rfc2231)
            """
message/external-body; access-type=URL;
		URL*0="ftp://";
		URL*1="cs.utk.edu/pub/moore/bulk-mailer/bulk-mailer.tar";
"""u8.Raw(), // <-- trailing semicolon
            @"message/external-body",
            m("access-type", "URL", "url", "ftp://cs.utk.edu/pub/moore/bulk-mailer/bulk-mailer.tar")
        ),

        // Issue #48866: duplicate parameters containing equal values should be allowed
        new(@"text; charset=utf-8; charset=utf-8; format=fixed", "text", m("charset", "utf-8", "format", "fixed")),
        new(@"text; charset=utf-8; format=flowed; charset=utf-8", "text", m("charset", "utf-8", "format", "flowed")),

        // Issue #76236: '{' and '}' are token chars.
        new("attachment; filename={file}.png", "attachment", m("filename", "{file}.png"))
    ]);
}

public static void TestParseMediaType(ж<testing.T> Ꮡt) {
    ref var t = ref Ꮡt.DerefOrNull();

    foreach (var (_, test) in range(parseMediaTypeTests)) {
        var (mt, @params, err) = ParseMediaType(test.@in);
        if (err != nil) {
            if (test.t != ""u8) {
                Ꮡt.Errorf("for input %#q, unexpected error: %v", test.@in, err);
                continue;
            }
            continue;
        }
        @string g = mt;
        @string e = test.t; if (g != e) {
            Ꮡt.Errorf("for input %#q, expected type %q, got %q",
                test.@in, e, g);
            continue;
        }
        if (len(@params) == 0 && len(test.p) == 0) {
            continue;
        }
        if (!maps.Equal<map<@string, @string>, map<@string, @string>, @string, @string>(@params, test.p)) {
            Ꮡt.Errorf("for input %#q, wrong params.\n"u8 + "expected: %#v\n"u8 + "     got: %#v"u8,
                test.@in, test.p, @params);
        }
    }
}

public static void BenchmarkParseMediaType(ж<testing.B> Ꮡb) {
    ref var b = ref Ꮡb.DerefOrNull();

    foreach (var _ᴛ1 in range(b.N)) {
        foreach (var (_, test) in range(parseMediaTypeTests)) {
            ParseMediaType(test.@in);
        }
    }
}

internal partial struct badMediaTypeTest {
    internal @string @in;
    internal @string mt;
    internal @string err;
}

internal static slice<badMediaTypeTest> badMediaTypeTests = [
    new("bogus ;=========", "bogus", "mime: invalid media parameter"),
    // The following example is from real email delivered by gmail (error: missing semicolon)
    // and it is there to check behavior described in #19498
    new("application/pdf; x-mac-type=\"3F3F3F3F\"; x-mac-creator=\"3F3F3F3F\" name=\"a.pdf\";",
        "application/pdf", "mime: invalid media parameter"),
    new("bogus/<script>alert</script>", "", "mime: expected token after slash"),
    new("bogus/bogus<script>alert</script>", "", "mime: unexpected content after media subtype"),
    // Tests from http://greenbytes.de/tech/tc2231/
    new(@"""attachment""", "attachment", "mime: no media type"),
    new("attachment; filename=foo,bar.html", "attachment", "mime: invalid media parameter"),
    new("attachment; ;filename=foo", "attachment", "mime: invalid media parameter"),
    new("attachment; filename=foo bar.html", "attachment", "mime: invalid media parameter"),
    new(@"attachment; filename=""foo.html""; filename=""bar.html""", "attachment", "mime: duplicate parameter name"),
    new("attachment; filename=foo[1](2).html", "attachment", "mime: invalid media parameter"),
    new("attachment; filename=foo-ä.html", "attachment", "mime: invalid media parameter"),
    new("attachment; filename=foo-Ã¤.html", "attachment", "mime: invalid media parameter"),
    new(@"attachment; filename *=UTF-8''foo-%c3%a4.html", "attachment", "mime: invalid media parameter")
];

public static void TestParseMediaTypeBogus(ж<testing.T> t) {
    foreach (var (_, tt) in range(badMediaTypeTests)) {
        var (mt, @params, err) = ParseMediaType(tt.@in);
        if (err == nil) {
            t.Errorf("ParseMediaType(%q) = nil error; want parse error", tt.@in);
            continue;
        }
        if (err.Error() != tt.err) {
            t.Errorf("ParseMediaType(%q) = err %q; want %q", tt.@in, err.Error(), tt.err);
        }
        if (@params != nil) {
            t.Errorf("ParseMediaType(%q): got non-nil params on error", tt.@in);
        }
        if (!AreEqual(err, ErrInvalidMediaParameter) && mt != ""u8) {
            t.Errorf("ParseMediaType(%q): got unexpected non-empty media type string", tt.@in);
        }
        if (AreEqual(err, ErrInvalidMediaParameter) && mt != tt.mt) {
            t.Errorf("ParseMediaType(%q): in case of invalid parameters: expected type %q, got %q", tt.@in, tt.mt, mt);
        }
    }
}

public static void BenchmarkParseMediaTypeBogus(ж<testing.B> Ꮡb) {
    ref var b = ref Ꮡb.DerefOrNull();

    foreach (var _ᴛ1 in range(b.N)) {
        foreach (var (_, test) in range(badMediaTypeTests)) {
            ParseMediaType(test.@in);
        }
    }
}

internal partial struct formatTest {
    internal @string typ;
    internal map<@string, @string> @params;
    internal @string want;
}

internal static slice<formatTest> formatTests = [
    new("noslash", new map<@string, @string>{["X"] = "Y"}, "noslash; x=Y"), // e.g. Content-Disposition values (RFC 2183); issue 11289
    new("foo bar/baz", default, ""),
    new("foo/bar baz", default, ""),
    new("attachment", new map<@string, @string>{["filename"] = "ĄĄŽŽČČŠŠ"}, "attachment; filename*=utf-8''%C4%84%C4%84%C5%BD%C5%BD%C4%8C%C4%8C%C5%A0%C5%A0"),
    new("attachment", new map<@string, @string>{["filename"] = "ÁÁÊÊÇÇÎÎ"}, "attachment; filename*=utf-8''%C3%81%C3%81%C3%8A%C3%8A%C3%87%C3%87%C3%8E%C3%8E"),
    new("attachment", new map<@string, @string>{["filename"] = "数据统计.png"}, "attachment; filename*=utf-8''%E6%95%B0%E6%8D%AE%E7%BB%9F%E8%AE%A1.png"),
    new("foo/BAR", default, "foo/bar"),
    new("foo/BAR", new map<@string, @string>{["X"] = "Y"}, "foo/bar; x=Y"),
    new("foo/BAR", new map<@string, @string>{["space"] = "With space"}, @"foo/bar; space=""With space"""),
    new("foo/BAR", new map<@string, @string>{["quote"] = @"With ""quote"}, @"foo/bar; quote=""With \""quote"""),
    new("foo/BAR", new map<@string, @string>{["bslash"] = @"With \backslash"}, @"foo/bar; bslash=""With \\backslash"""),
    new("foo/BAR", new map<@string, @string>{["both"] = @"With \backslash and ""quote"}, @"foo/bar; both=""With \\backslash and \""quote"""),
    new("foo/BAR", new map<@string, @string>{[""] = "empty attribute"}, ""),
    new("foo/BAR", new map<@string, @string>{["bad attribute"] = "baz"}, ""),
    new("foo/BAR", new map<@string, @string>{["nonascii"] = "not an ascii character: ä"}, "foo/bar; nonascii*=utf-8''not%20an%20ascii%20character%3A%20%C3%A4"),
    new("foo/BAR", new map<@string, @string>{["ctl"] = "newline: \n nil: \u0000"}, "foo/bar; ctl*=utf-8''newline%3A%20%0A%20nil%3A%20%00"),
    new("foo/bar", new map<@string, @string>{["a"] = "av", ["b"] = "bv", ["c"] = "cv"}, "foo/bar; a=av; b=bv; c=cv"),
    new("foo/bar", new map<@string, @string>{["0"] = "'", ["9"] = "'"}, "foo/bar; 0='; 9='"),
    new("foo", new map<@string, @string>{["bar"] = ""}, @"foo; bar=""""")
];

public static void TestFormatMediaType(ж<testing.T> t) {
    foreach (var (i, tt) in range(formatTests)) {
        @string got = FormatMediaType(tt.typ, tt.@params);
        if (got != tt.want) {
            t.Errorf("%d. FormatMediaType(%q, %v) = %q; want %q", i, tt.typ, tt.@params, got, tt.want);
        }
        if (got == ""u8) {
            continue;
        }
        var (typ, @params, err) = ParseMediaType(got);
        if (err != nil) {
            t.Errorf("%d. ParseMediaType(%q) err: %v", i, got, err);
        }
        if (typ != strings.ToLower(tt.typ)) {
            t.Errorf("%d. ParseMediaType(%q) typ = %q; want %q", i, got, typ, tt.typ);
        }
        foreach (var (kᴛ1, v) in tt.@params) {
            var k = kᴛ1;

            k = strings.ToLower(k);
            if (@params[k] != v) {
                t.Errorf("%d. ParseMediaType(%q) params[%s] = %q; want %q", i, got, k, @params[k], v);
            }
        }
    }
}

} // end mime_internal_test_package
