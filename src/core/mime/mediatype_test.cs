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

public static void TestConsumeToken(ж<testing.T> Ꮡt) {
    var tests = new array<@string>[]{
        new @string[]{"foo bar"u8, "foo"u8, " bar"u8}.array(),
        new @string[]{"bar"u8, "bar"u8, ""u8}.array(),
        new @string[]{""u8, ""u8, ""u8}.array(),
        new @string[]{" foo"u8, ""u8, " foo"u8}.array()
    }.array();
    foreach (var (_, vᴛ1) in tests.ΔRangeSnapshot()) {
        var test = vᴛ1.Clone();

        var (token, rest) = consumeToken(test[0]);
        @string expectedToken = test[1];
        @string expectedRest = test[2];
        if (token != expectedToken){
            Ꮡt.Errorf("expected to consume token '%s', not '%s' from '%s'"u8,
                expectedToken, token, test[0]);
        } else 
        if (rest != expectedRest) {
            Ꮡt.Errorf("expected to have left '%s', not '%s' after reading token '%s' from '%s'"u8,
                expectedRest, rest, token, test[0]);
        }
    }
}

public static void TestConsumeValue(ж<testing.T> Ꮡt) {
    var tests = new array<@string>[]{
        new @string[]{"foo bar"u8, "foo"u8, " bar"u8}.array(),
        new @string[]{"bar"u8, "bar"u8, ""u8}.array(),
        new @string[]{" bar "u8, ""u8, " bar "u8}.array(),
        new @string[]{@"""My value""end"u8, "My value"u8, "end"u8}.array(),
        new @string[]{@"""My value"" end"u8, "My value"u8, " end"u8}.array(),
        new @string[]{@"""\\"" rest"u8, "\\"u8, " rest"u8}.array(),
        new @string[]{@"""My \"" value""end"u8, "My \" value"u8, "end"u8}.array(),
        new @string[]{@"""\"" rest"u8, ""u8, @"""\"" rest"u8}.array(),
        new @string[]{@"""C:\dev\go\robots.txt"""u8, @"C:\dev\go\robots.txt"u8, ""u8}.array(),
        new @string[]{@"""C:\新建文件夹\中文第二次测试.mp4"""u8, @"C:\新建文件夹\中文第二次测试.mp4"u8, ""u8}.array()
    }.array();
    foreach (var (_, vᴛ1) in tests.ΔRangeSnapshot()) {
        var test = vᴛ1.Clone();

        var (value, rest) = consumeValue(test[0]);
        @string expectedValue = test[1];
        @string expectedRest = test[2];
        if (value != expectedValue){
            Ꮡt.Errorf("expected to consume value [%s], not [%s] from [%s]"u8,
                expectedValue, value, test[0]);
        } else 
        if (rest != expectedRest) {
            Ꮡt.Errorf("expected to have left [%s], not [%s] after reading value [%s] from [%s]"u8,
                expectedRest, rest, value, test[0]);
        }
    }
}

public static void TestConsumeMediaParam(ж<testing.T> Ꮡt) {
    var tests = new array<@string>[]{
        new @string[]{" ; foo=bar"u8, "foo"u8, "bar"u8, ""u8}.array(),
        new @string[]{"; foo=bar"u8, "foo"u8, "bar"u8, ""u8}.array(),
        new @string[]{";foo=bar"u8, "foo"u8, "bar"u8, ""u8}.array(),
        new @string[]{";FOO=bar"u8, "foo"u8, "bar"u8, ""u8}.array(),
        new @string[]{@";foo=""bar"""u8, "foo"u8, "bar"u8, ""u8}.array(),
        new @string[]{@";foo=""bar""; "u8, "foo"u8, "bar"u8, "; "u8}.array(),
        new @string[]{@";foo=""bar""; foo=baz"u8, "foo"u8, "bar"u8, "; foo=baz"u8}.array(),
        new @string[]{@" ; boundary=----CUT;"u8, "boundary"u8, "----CUT"u8, ";"u8}.array(),
        new @string[]{@" ; key=value;  blah=""value"";name=""foo"" "u8, "key"u8, "value"u8, @";  blah=""value"";name=""foo"" "u8}.array(),
        new @string[]{@";  blah=""value"";name=""foo"" "u8, "blah"u8, "value"u8, @";name=""foo"" "u8}.array(),
        new @string[]{@";name=""foo"" "u8, "name"u8, "foo"u8, @" "u8}.array()
    }.array();
    foreach (var (_, vᴛ1) in tests.ΔRangeSnapshot()) {
        var test = vᴛ1.Clone();

        var (param, value, rest) = consumeMediaParam(test[0]);
        @string expectedParam = test[1];
        @string expectedValue = test[2];
        @string expectedRest = test[3];
        if (param != expectedParam){
            Ꮡt.Errorf("expected to consume param [%s], not [%s] from [%s]"u8,
                expectedParam, param, test[0]);
        } else 
        if (value != expectedValue){
            Ꮡt.Errorf("expected to consume value [%s], not [%s] from [%s]"u8,
                expectedValue, value, test[0]);
        } else 
        if (rest != expectedRest) {
            Ꮡt.Errorf("expected to have left [%s], not [%s] after reading [%s/%s] from [%s]"u8,
                expectedRest, rest, param, value, test[0]);
        }
    }
}

internal partial struct mediaTypeTest {
    internal @string @in;
    internal @string t;
    internal map<@string, @string> p;
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static readonly @string keyˢ = "key"u8;
internal static readonly @string valueˢ = "value"u8;
internal static readonly @string blahˢ = "blah"u8;
internal static readonly @string nameˢ = "name"u8;
internal static readonly @string fooˢ = "foo"u8;
internal static readonly @string titleˢ = "title"u8;
internal static readonly @string thisIsFunˢ = "This is ***fun***"u8;
internal static readonly @string accessTypeˢ = "access-type"u8;
internal static readonly @string urlˢ = "URL"u8;
internal static readonly @string urlˢ2 = "url"u8;
internal static readonly @string ftpCsUtkEduPubMooreBulkˢ = "ftp://cs.utk.edu/pub/moore/bulk-mailer/bulk-mailer.tar"u8;
internal static readonly @string thisIsEvenMoreFunIsnTItˢ = "This is even more ***fun*** isn't it!"u8;
internal static readonly @string filenameˢ = "filename"u8;
internal static readonly @string fooHtmlˢ = "foo.html"u8;
internal static readonly @string fOoHtmlˢ = "f\\oo.html"u8;
internal static readonly @string quotingTestedHtmlˢ = @"""quoting"" tested.html"u8;
internal static readonly @string hereSASemicolonHtmlˢ = "Here's a semicolon;.html"u8;
internal static readonly @string barˢ = "bar"u8;
internal static readonly @string fooHtmlˢ2 = "'foo.html'"u8;
internal static readonly @string fooHtmlˢ3 = "foo-ä.html"u8;
internal static readonly @string fooHtmlˢ4 = "foo-Ã¤.html"u8;
internal static readonly @string foo41Htmlˢ = "foo-%41.html"u8;
internal static readonly @string htmlˢ = "50%.html"u8;
internal static readonly @string foo41Htmlˢ2 = "foo-%\\41.html"u8;
internal static readonly @string htmlˢ2 = "ä-%41.html"u8;
internal static readonly @string fooC3A4E282AcHtmlˢ = "foo-%c3%a4-%e2%82%ac.html"u8;
internal static readonly @string xfilenameˢ = "xfilename"u8;
internal static readonly @string creationDateˢ = "creation-date"u8;
internal static readonly @string wed12Feb19971629510500ˢ = "Wed, 12 Feb 1997 16:29:51 -0500"u8;
internal static readonly @string modificationDateˢ = "modification-date"u8;
internal static readonly @string exampleˢ = "example"u8;
internal static readonly @string filenameExampleTxtˢ = "filename=example.txt"u8;
internal static readonly @string fooHtmlˢ5 = "foo-ä-€.html"u8;
internal static readonly @string fooAHtmlˢ = "foo-ä.html"u8;
internal static readonly @string a41Htmlˢ = "A-%41.html"u8;
internal static readonly @string foobarˢ = "foobar"u8;
internal static readonly @string firstnameˢ = "firstname"u8;
internal static readonly @string lastnameˢ = "lastname"u8;
internal static readonly @string fileˢ = "file"u8;
internal static readonly @string cDevGoRobotsTxtˢ = @"C:\dev\go\robots.txt"u8;
internal static readonly @string cMp4ˢ = @"C:\新建文件夹\中文第二次测试.mp4"u8;
internal static readonly @string formatˢ = "format"u8;
internal static readonly @string fixedˢ = "fixed"u8;
internal static readonly @string flowedˢ = "flowed"u8;

public static void TestParseMediaType(ж<testing.T> Ꮡt) {
    ref var t = ref Ꮡt.DerefOrNull();

    // Convenience map initializer
    map<@string, @string> m(params ꓸꓸꓸstring sʗp) {
        var s = sʗp.sslice();
        var sm = new map<@string, @string>();
        for (nint i = 0; i < len(s); i += 2) {
            sm[s[i]] = s[i + 1];
        }
        return sm;
    }
    var nameFoo = new map<@string, @string>{["name"u8] = "foo"u8};
    var tests = new mediaTypeTest[]{
        new(@"form-data; name=""foo"""u8, "form-data"u8, nameFoo),
        new(@" form-data ; name=foo"u8, "form-data"u8, nameFoo),
        new(@"FORM-DATA;name=""foo"""u8, "form-data"u8, nameFoo),
        new(@" FORM-DATA ; name=""foo"""u8, "form-data"u8, nameFoo),
        new(@" FORM-DATA ; name=""foo"""u8, "form-data"u8, nameFoo),
        new(@"form-data; key=value;  blah=""value"";name=""foo"" "u8,
            "form-data"u8,
            m(keyˢ, valueˢ, blahˢ, valueˢ, nameˢ, fooˢ)),
        new(@"foo; key=val1; key=the-key-appears-again-which-is-bogus"u8,
            ""u8, m()), // From RFC 2231:

        new(@"application/x-stuff; title*=us-ascii'en-us'This%20is%20%2A%2A%2Afun%2A%2A%2A"u8,
            "application/x-stuff"u8,
            m(titleˢ, thisIsFunˢ)),
        new(@"message/external-body; access-type=URL; "u8 + @"URL*0=""ftp://"";"u8 + @"URL*1=""cs.utk.edu/pub/moore/bulk-mailer/bulk-mailer.tar"""u8,
            "message/external-body"u8,
            m(accessTypeˢ, urlˢ,
                urlˢ2, ftpCsUtkEduPubMooreBulkˢ)),
        new(@"application/x-stuff; "u8 + @"title*0*=us-ascii'en'This%20is%20even%20more%20; "u8 + @"title*1*=%2A%2A%2Afun%2A%2A%2A%20; "u8 + @"title*2=""isn't it!"""u8,
            "application/x-stuff"u8,
            m(titleˢ, thisIsEvenMoreFunIsnTItˢ)), // Tests from http://greenbytes.de/tech/tc2231/
 // Note: Backslash escape handling is a bit loose, like MSIE.
 // #attonly

        new(@"attachment"u8,
            "attachment"u8,
            m()), // #attonlyucase

        new(@"ATTACHMENT"u8,
            "attachment"u8,
            m()), // #attwithasciifilename

        new(@"attachment; filename=""foo.html"""u8,
            "attachment"u8,
            m(filenameˢ, fooHtmlˢ)), // #attwithasciifilename25

        new(@"attachment; filename=""0000000000111111111122222"""u8,
            "attachment"u8,
            m(filenameˢ, "0000000000111111111122222")), // #attwithasciifilename35

        new(@"attachment; filename=""00000000001111111111222222222233333"""u8,
            "attachment"u8,
            m(filenameˢ, "00000000001111111111222222222233333")), // #attwithasciifnescapedchar

        new(@"attachment; filename=""f\oo.html"""u8,
            "attachment"u8,
            m(filenameˢ, fOoHtmlˢ)), // #attwithasciifnescapedquote

        new(@"attachment; filename=""\""quoting\"" tested.html"""u8,
            "attachment"u8,
            m(filenameˢ, quotingTestedHtmlˢ)), // #attwithquotedsemicolon

        new(@"attachment; filename=""Here's a semicolon;.html"""u8,
            "attachment"u8,
            m(filenameˢ, hereSASemicolonHtmlˢ)), // #attwithfilenameandextparam

        new(@"attachment; foo=""bar""; filename=""foo.html"""u8,
            "attachment"u8,
            m(fooˢ, barˢ, filenameˢ, fooHtmlˢ)), // #attwithfilenameandextparamescaped

        new(@"attachment; foo=""\""\\"";filename=""foo.html"""u8,
            "attachment"u8,
            m(fooˢ, "\"\\", filenameˢ, fooHtmlˢ)), // #attwithasciifilenameucase

        new(@"attachment; FILENAME=""foo.html"""u8,
            "attachment"u8,
            m(filenameˢ, fooHtmlˢ)), // #attwithasciifilenamenq

        new(@"attachment; filename=foo.html"u8,
            "attachment"u8,
            m(filenameˢ, fooHtmlˢ)), // #attwithasciifilenamenqs

        new(@"attachment; filename=foo.html ;"u8,
            "attachment"u8,
            m(filenameˢ, fooHtmlˢ)), // #attwithfntokensq

        new(@"attachment; filename='foo.html'"u8,
            "attachment"u8,
            m(filenameˢ, fooHtmlˢ2)), // #attwithisofnplain

        new(@"attachment; filename=""foo-ä.html"""u8,
            "attachment"u8,
            m(filenameˢ, fooHtmlˢ3)), // #attwithutf8fnplain

        new(@"attachment; filename=""foo-Ã¤.html"""u8,
            "attachment"u8,
            m(filenameˢ, fooHtmlˢ4)), // #attwithfnrawpctenca

        new(@"attachment; filename=""foo-%41.html"""u8,
            "attachment"u8,
            m(filenameˢ, foo41Htmlˢ)), // #attwithfnusingpct

        new(@"attachment; filename=""50%.html"""u8,
            "attachment"u8,
            m(filenameˢ, htmlˢ)), // #attwithfnrawpctencaq

        new(@"attachment; filename=""foo-%\41.html"""u8,
            "attachment"u8,
            m(filenameˢ, foo41Htmlˢ2)), // #attwithnamepct

        new(@"attachment; name=""foo-%41.html"""u8,
            "attachment"u8,
            m(nameˢ, foo41Htmlˢ)), // #attwithfilenamepctandiso

        new(@"attachment; name=""ä-%41.html"""u8,
            "attachment"u8,
            m(nameˢ, htmlˢ2)), // #attwithfnrawpctenclong

        new(@"attachment; filename=""foo-%c3%a4-%e2%82%ac.html"""u8,
            "attachment"u8,
            m(filenameˢ, fooC3A4E282AcHtmlˢ)), // #attwithasciifilenamews1

        new(@"attachment; filename =""foo.html"""u8,
            "attachment"u8,
            m(filenameˢ, fooHtmlˢ)), // #attmissingdisposition

        new(@"filename=foo.html"u8,
            ""u8, m()), // #attmissingdisposition2

        new(@"x=y; filename=foo.html"u8,
            ""u8, m()), // #attmissingdisposition3

        new(@"""foo; filename=bar;baz""; filename=qux"u8,
            ""u8, m()), // #attmissingdisposition4

        new(@"filename=foo.html, filename=bar.html"u8,
            ""u8, m()), // #emptydisposition

        new(@"; filename=foo.html"u8,
            ""u8, m()), // #doublecolon

        new(@": inline; attachment; filename=foo.html"u8,
            ""u8, m()), // #attandinline

        new(@"inline; attachment; filename=foo.html"u8,
            ""u8, m()), // #attandinline2

        new(@"attachment; inline; filename=foo.html"u8,
            ""u8, m()), // #attbrokenquotedfn

        new(@"attachment; filename=""foo.html"".txt"u8,
            ""u8, m()), // #attbrokenquotedfn2

        new(@"attachment; filename=""bar"u8,
            ""u8, m()), // #attbrokenquotedfn3

        new(@"attachment; filename=foo""bar;baz""qux"u8,
            ""u8, m()), // #attmultinstances

        new(@"attachment; filename=foo.html, attachment; filename=bar.html"u8,
            ""u8, m()), // #attmissingdelim

        new(@"attachment; foo=foo filename=bar"u8,
            ""u8, m()), // #attmissingdelim2

        new(@"attachment; filename=bar foo=foo"u8,
            ""u8, m()), // #attmissingdelim3

        new(@"attachment filename=bar"u8,
            ""u8, m()), // #attreversed

        new(@"filename=foo.html; attachment"u8,
            ""u8, m()), // #attconfusedparam

        new(@"attachment; xfilename=foo.html"u8,
            "attachment"u8,
            m(xfilenameˢ, fooHtmlˢ)), // #attcdate

        new(@"attachment; creation-date=""Wed, 12 Feb 1997 16:29:51 -0500"""u8,
            "attachment"u8,
            m(creationDateˢ, wed12Feb19971629510500ˢ)), // #attmdate

        new(@"attachment; modification-date=""Wed, 12 Feb 1997 16:29:51 -0500"""u8,
            "attachment"u8,
            m(modificationDateˢ, wed12Feb19971629510500ˢ)), // #dispext

        new(@"foobar"u8, "foobar"u8, m()), // #dispextbadfn

        new(@"attachment; example=""filename=example.txt"""u8,
            "attachment"u8,
            m(exampleˢ, filenameExampleTxtˢ)), // #attwithfn2231utf8

        new(@"attachment; filename*=UTF-8''foo-%c3%a4-%e2%82%ac.html"u8,
            "attachment"u8,
            m(filenameˢ, fooHtmlˢ5)), // #attwithfn2231noc

        new(@"attachment; filename*=''foo-%c3%a4-%e2%82%ac.html"u8,
            "attachment"u8,
            m()), // #attwithfn2231utf8comp

        new(@"attachment; filename*=UTF-8''foo-a%cc%88.html"u8,
            "attachment"u8,
            m(filenameˢ, fooAHtmlˢ)), // #attwithfn2231ws2

        new(@"attachment; filename*= UTF-8''foo-%c3%a4.html"u8,
            "attachment"u8,
            m(filenameˢ, fooHtmlˢ3)), // #attwithfn2231ws3

        new(@"attachment; filename* =UTF-8''foo-%c3%a4.html"u8,
            "attachment"u8,
            m(filenameˢ, fooHtmlˢ3)), // #attwithfn2231quot

        new(@"attachment; filename*=""UTF-8''foo-%c3%a4.html"""u8,
            "attachment"u8,
            m(filenameˢ, fooHtmlˢ3)), // #attwithfn2231quot2

        new(@"attachment; filename*=""foo%20bar.html"""u8,
            "attachment"u8,
            m()), // #attwithfn2231singleqmissing

        new(@"attachment; filename*=UTF-8'foo-%c3%a4.html"u8,
            "attachment"u8,
            m()), // #attwithfn2231nbadpct1

        new(@"attachment; filename*=UTF-8''foo%"u8,
            "attachment"u8,
            m()), // #attwithfn2231nbadpct2

        new(@"attachment; filename*=UTF-8''f%oo.html"u8,
            "attachment"u8,
            m()), // #attwithfn2231dpct

        new(@"attachment; filename*=UTF-8''A-%2541.html"u8,
            "attachment"u8,
            m(filenameˢ, a41Htmlˢ)), // #attfncont

        new(@"attachment; filename*0=""foo.""; filename*1=""html"""u8,
            "attachment"u8,
            m(filenameˢ, fooHtmlˢ)), // #attfncontenc

        new(@"attachment; filename*0*=UTF-8''foo-%c3%a4; filename*1="".html"""u8,
            "attachment"u8,
            m(filenameˢ, fooHtmlˢ3)), // #attfncontlz

        new(@"attachment; filename*0=""foo""; filename*01=""bar"""u8,
            "attachment"u8,
            m(filenameˢ, fooˢ)), // #attfncontnc

        new(@"attachment; filename*0=""foo""; filename*2=""bar"""u8,
            "attachment"u8,
            m(filenameˢ, fooˢ)), // #attfnconts1

        new(@"attachment; filename*1=""foo.""; filename*2=""html"""u8,
            "attachment"u8, m()), // #attfncontord

        new(@"attachment; filename*1=""bar""; filename*0=""foo"""u8,
            "attachment"u8,
            m(filenameˢ, foobarˢ)), // #attfnboth

        new(@"attachment; filename=""foo-ae.html""; filename*=UTF-8''foo-%c3%a4.html"u8,
            "attachment"u8,
            m(filenameˢ, fooHtmlˢ3)), // #attfnboth2

        new(@"attachment; filename*=UTF-8''foo-%c3%a4.html; filename=""foo-ae.html"""u8,
            "attachment"u8,
            m(filenameˢ, fooHtmlˢ3)), // #attfnboth3

        new(@"attachment; filename*0*=ISO-8859-15''euro-sign%3d%a4; filename*=ISO-8859-1''currency-sign%3d%a4"u8,
            "attachment"u8,
            m()), // #attnewandfn

        new(@"attachment; foobar=x; filename=""foo.html"""u8,
            "attachment"u8,
            m(foobarˢ, "x", filenameˢ, fooHtmlˢ)), // Browsers also just send UTF-8 directly without RFC 2231,
 // at least when the source page is served with UTF-8.

        new(@"form-data; firstname=""Брэд""; lastname=""Фицпатрик"""u8,
            "form-data"u8,
            m(firstnameˢ, "Брэд", lastnameˢ, "Фицпатрик")), // Empty string used to be mishandled.

        new(@"foo; bar="""""u8, "foo"u8, m(barˢ, "")), // Microsoft browsers in intranet mode do not think they need to escape \ in file name.

        new(@"form-data; name=""file""; filename=""C:\dev\go\robots.txt"""u8, "form-data"u8, m(nameˢ, fileˢ, filenameˢ, cDevGoRobotsTxtˢ)),
        new(@"form-data; name=""file""; filename=""C:\新建文件夹\中文第二次测试.mp4"""u8, "form-data"u8, m(nameˢ, fileˢ, filenameˢ, cMp4ˢ)), // issue #46323 (https://github.com/golang/go/issues/46323)

        new(
            """
message/external-body; access-type=URL;
		URL*0="ftp://";
		URL*1="cs.utk.edu/pub/moore/bulk-mailer/bulk-mailer.tar";
"""u8, // example from rfc2231-p.3 (https://datatracker.ietf.org/doc/html/rfc2231)
 // <-- trailing semicolon

            @"message/external-body"u8,
            m(accessTypeˢ, urlˢ, urlˢ2, ftpCsUtkEduPubMooreBulkˢ)
        ), // Issue #48866: duplicate parameters containing equal values should be allowed

        new(@"text; charset=utf-8; charset=utf-8; format=fixed"u8, "text"u8, m(charsetˢ, utf8ˢ2, formatˢ, fixedˢ)),
        new(@"text; charset=utf-8; format=flowed; charset=utf-8"u8, "text"u8, m(charsetˢ, utf8ˢ2, formatˢ, flowedˢ))
    }.slice();
    foreach (var (_, test) in tests) {
        var (mt, @params, err) = ParseMediaType(test.@in);
        if (err != default!) {
            if (test.t != ""u8) {
                Ꮡt.Errorf("for input %#q, unexpected error: %v"u8, test.@in, err);
                continue;
            }
            continue;
        }
        {
            @string g = mt;
            @string e = test.t; if (g != e) {
                Ꮡt.Errorf("for input %#q, expected type %q, got %q"u8,
                    test.@in, e, g);
                continue;
            }
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

internal partial struct badMediaTypeTest {
    internal @string @in;
    internal @string mt;
    internal @string err;
}

// The following example is from real email delivered by gmail (error: missing semicolon)
// and it is there to check behavior described in #19498
// Tests from http://greenbytes.de/tech/tc2231/
internal static slice<badMediaTypeTest> badMediaTypeTests = new badMediaTypeTest[]{
    new("bogus ;========="u8, "bogus"u8, "mime: invalid media parameter"u8),
    new("application/pdf; x-mac-type=\"3F3F3F3F\"; x-mac-creator=\"3F3F3F3F\" name=\"a.pdf\";"u8,
        "application/pdf"u8, "mime: invalid media parameter"u8),
    new("bogus/<script>alert</script>"u8, ""u8, "mime: expected token after slash"u8),
    new("bogus/bogus<script>alert</script>"u8, ""u8, "mime: unexpected content after media subtype"u8),
    new(@"""attachment"""u8, "attachment"u8, "mime: no media type"u8),
    new("attachment; filename=foo,bar.html"u8, "attachment"u8, "mime: invalid media parameter"u8),
    new("attachment; ;filename=foo"u8, "attachment"u8, "mime: invalid media parameter"u8),
    new("attachment; filename=foo bar.html"u8, "attachment"u8, "mime: invalid media parameter"u8),
    new(@"attachment; filename=""foo.html""; filename=""bar.html"""u8, "attachment"u8, "mime: duplicate parameter name"u8),
    new("attachment; filename=foo[1](2).html"u8, "attachment"u8, "mime: invalid media parameter"u8),
    new("attachment; filename=foo-ä.html"u8, "attachment"u8, "mime: invalid media parameter"u8),
    new("attachment; filename=foo-Ã¤.html"u8, "attachment"u8, "mime: invalid media parameter"u8),
    new(@"attachment; filename *=UTF-8''foo-%c3%a4.html"u8, "attachment"u8, "mime: invalid media parameter"u8)
}.slice();

public static void TestParseMediaTypeBogus(ж<testing.T> Ꮡt) {
    foreach (var (_, tt) in badMediaTypeTests) {
        var (mt, @params, err) = ParseMediaType(tt.@in);
        if (err == default!) {
            Ꮡt.Errorf("ParseMediaType(%q) = nil error; want parse error"u8, tt.@in);
            continue;
        }
        if (err.Error() != tt.err) {
            Ꮡt.Errorf("ParseMediaType(%q) = err %q; want %q"u8, tt.@in, err.Error(), tt.err);
        }
        if (@params != default!) {
            Ꮡt.Errorf("ParseMediaType(%q): got non-nil params on error"u8, tt.@in);
        }
        if (!AreEqual(err, ErrInvalidMediaParameter) && mt != ""u8) {
            Ꮡt.Errorf("ParseMediaType(%q): got unexpected non-empty media type string"u8, tt.@in);
        }
        if (AreEqual(err, ErrInvalidMediaParameter) && mt != tt.mt) {
            Ꮡt.Errorf("ParseMediaType(%q): in case of invalid parameters: expected type %q, got %q"u8, tt.@in, tt.mt, mt);
        }
    }
}

internal partial struct formatTest {
    internal @string typ;
    internal map<@string, @string> @params;
    internal @string want;
}

// e.g. Content-Disposition values (RFC 2183); issue 11289
internal static slice<formatTest> formatTests = new formatTest[]{
    new("noslash"u8, new map<@string, @string>{["X"u8] = "Y"u8}, "noslash; x=Y"u8),
    new("foo bar/baz"u8, default!, ""u8),
    new("foo/bar baz"u8, default!, ""u8),
    new("attachment"u8, new map<@string, @string>{["filename"u8] = "ĄĄŽŽČČŠŠ"u8}, "attachment; filename*=utf-8''%C4%84%C4%84%C5%BD%C5%BD%C4%8C%C4%8C%C5%A0%C5%A0"u8),
    new("attachment"u8, new map<@string, @string>{["filename"u8] = "ÁÁÊÊÇÇÎÎ"u8}, "attachment; filename*=utf-8''%C3%81%C3%81%C3%8A%C3%8A%C3%87%C3%87%C3%8E%C3%8E"u8),
    new("attachment"u8, new map<@string, @string>{["filename"u8] = "数据统计.png"u8}, "attachment; filename*=utf-8''%E6%95%B0%E6%8D%AE%E7%BB%9F%E8%AE%A1.png"u8),
    new("foo/BAR"u8, default!, "foo/bar"u8),
    new("foo/BAR"u8, new map<@string, @string>{["X"u8] = "Y"u8}, "foo/bar; x=Y"u8),
    new("foo/BAR"u8, new map<@string, @string>{["space"u8] = "With space"u8}, @"foo/bar; space=""With space"""u8),
    new("foo/BAR"u8, new map<@string, @string>{["quote"u8] = @"With ""quote"u8}, @"foo/bar; quote=""With \""quote"""u8),
    new("foo/BAR"u8, new map<@string, @string>{["bslash"u8] = @"With \backslash"u8}, @"foo/bar; bslash=""With \\backslash"""u8),
    new("foo/BAR"u8, new map<@string, @string>{["both"u8] = @"With \backslash and ""quote"u8}, @"foo/bar; both=""With \\backslash and \""quote"""u8),
    new("foo/BAR"u8, new map<@string, @string>{[""u8] = "empty attribute"u8}, ""u8),
    new("foo/BAR"u8, new map<@string, @string>{["bad attribute"u8] = "baz"u8}, ""u8),
    new("foo/BAR"u8, new map<@string, @string>{["nonascii"u8] = "not an ascii character: ä"u8}, "foo/bar; nonascii*=utf-8''not%20an%20ascii%20character%3A%20%C3%A4"u8),
    new("foo/BAR"u8, new map<@string, @string>{["ctl"u8] = "newline: \n nil: \u0000"u8}, "foo/bar; ctl*=utf-8''newline%3A%20%0A%20nil%3A%20%00"u8),
    new("foo/bar"u8, new map<@string, @string>{["a"u8] = "av"u8, ["b"u8] = "bv"u8, ["c"u8] = "cv"u8}, "foo/bar; a=av; b=bv; c=cv"u8),
    new("foo/bar"u8, new map<@string, @string>{["0"u8] = "'"u8, ["9"u8] = "'"u8}, "foo/bar; 0='; 9='"u8),
    new("foo"u8, new map<@string, @string>{["bar"u8] = ""u8}, @"foo; bar="""""u8)
}.slice();

public static void TestFormatMediaType(ж<testing.T> Ꮡt) {
    foreach (var (i, tt) in formatTests) {
        @string got = FormatMediaType(tt.typ, tt.@params);
        if (got != tt.want) {
            Ꮡt.Errorf("%d. FormatMediaType(%q, %v) = %q; want %q"u8, i, tt.typ, tt.@params, got, tt.want);
        }
        if (got == ""u8) {
            continue;
        }
        var (typ, @params, err) = ParseMediaType(got);
        if (err != default!) {
            Ꮡt.Errorf("%d. ParseMediaType(%q) err: %v"u8, i, got, err);
        }
        if (typ != strings.ToLower(tt.typ)) {
            Ꮡt.Errorf("%d. ParseMediaType(%q) typ = %q; want %q"u8, i, got, typ, tt.typ);
        }
        foreach (var (kᴛ1, v) in tt.@params) {
            var k = kᴛ1;

            k = strings.ToLower(k);
            if (@params[k] != v) {
                Ꮡt.Errorf("%d. ParseMediaType(%q) params[%s] = %q; want %q"u8, i, got, k, @params[k], v);
            }
        }
    }
}

} // end mime_internal_test_package
