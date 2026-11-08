// Copyright 2011 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.
namespace go.html;

using bytes = bytes_package;
using fmt = fmt_package;
using strings = strings_package;
using testing = testing_package;
using io = io_package;
using static go.html.template_package;

partial class template_internal_test_package {

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static readonly object bFooOReillyBarˢ = (@string)@"<b> ""foo%"" O'Reilly &bar;"u8;
internal static ref readonly @string aHrefExampleComFooˢ => ref aHrefExampleComFooˢᵇ;
private static readonly @string aHrefExampleComFooˢᵇ = @"a[href =~ ""//example.com""]#foo"u8;
internal static ref readonly @string helloBWorldBAmpTcˢ => ref helloBWorldBAmpTcˢᵇ;
private static readonly @string helloBWorldBAmpTcˢᵇ = @"Hello, <b>World</b> &amp;tc!"u8;
internal static ref readonly @string dirLtrˢ => ref dirLtrˢᵇ;
private static readonly @string dirLtrˢᵇ = @" dir=""ltr"""u8;
internal static ref readonly @string cAlertHelloWorldˢ => ref cAlertHelloWorldˢᵇ;
private static readonly @string cAlertHelloWorldˢᵇ = @"c && alert(""Hello, World!"");"u8;
internal static ref readonly @string helloWorldOReillyU0021ˢ => ref helloWorldOReillyU0021ˢᵇ;
private static readonly @string helloWorldOReillyU0021ˢᵇ = @"Hello, World & O'Reilly\u0021"u8;
internal static ref readonly @string greetingH69Addresseeˢ => ref greetingH69Addresseeˢᵇ;
private static readonly @string greetingH69Addresseeˢᵇ = @"greeting=H%69,&addressee=(World)"u8;
internal static ref readonly @string greetingH69Addresseeˢ2 => ref greetingH69Addresseeˢ2ᵇ;
private static readonly @string greetingH69Addresseeˢ2ᵇ = @"greeting=H%69,&addressee=(World) 2x, https://golang.org/favicon.ico 500.5w"u8;
internal static ref readonly @string fooˢ => ref fooˢᵇ;
private static readonly @string fooˢᵇ = @",foo/,"u8;

internal partial struct TestTypedContent_tests /*dyn*/ {
    // A template containing a single {{.}}.
    internal @string input;
    internal slice<@string> want;
}

public static void TestTypedContent(ж<testing.T> t) {
    var data = slice<any>([
        bFooOReillyBarˢ,
        (global::go.html.template_package.CSS)(@string)aHrefExampleComFooˢ,
        (global::go.html.template_package.HTML)(@string)helloBWorldBAmpTcˢ,
        (global::go.html.template_package.HTMLAttr)(@string)dirLtrˢ,
        (global::go.html.template_package.JS)(@string)cAlertHelloWorldˢ,
        (global::go.html.template_package.JSStr)(@string)helloWorldOReillyU0021ˢ,
        (global::go.html.template_package.URL)(@string)greetingH69Addresseeˢ,
        (global::go.html.template_package.Srcset)(@string)greetingH69Addresseeˢ2,
        (global::go.html.template_package.URL)(@string)fooˢ
    ]);

    // For each content sensitive escaper, see how it does on
    // each of the typed strings above.
    var tests = slice<TestTypedContent_tests>([
        new(
            @"<style>{{.}} { color: blue }</style>",
            [
                @"ZgotmplZ",
                // Allowed but not escaped.
                @"a[href =~ ""//example.com""]#foo",
                @"ZgotmplZ",
                @"ZgotmplZ",
                @"ZgotmplZ",
                @"ZgotmplZ",
                @"ZgotmplZ",
                @"ZgotmplZ",
                @"ZgotmplZ"
            ]
        ),
        new(
            @"<div style=""{{.}}"">",
            [
                @"ZgotmplZ",
                // Allowed and HTML escaped.
                @"a[href =~ &#34;//example.com&#34;]#foo",
                @"ZgotmplZ",
                @"ZgotmplZ",
                @"ZgotmplZ",
                @"ZgotmplZ",
                @"ZgotmplZ",
                @"ZgotmplZ",
                @"ZgotmplZ"
            ]
        ),
        new(
            @"{{.}}",
            [
                @"&lt;b&gt; &#34;foo%&#34; O&#39;Reilly &amp;bar;",
                @"a[href =~ &#34;//example.com&#34;]#foo",
                // Not escaped.
                @"Hello, <b>World</b> &amp;tc!",
                @" dir=&#34;ltr&#34;",
                @"c &amp;&amp; alert(&#34;Hello, World!&#34;);",
                @"Hello, World &amp; O&#39;Reilly\u0021",
                @"greeting=H%69,&amp;addressee=(World)",
                @"greeting=H%69,&amp;addressee=(World) 2x, https://golang.org/favicon.ico 500.5w",
                @",foo/,"
            ]
        ),
        new(
            @"<a{{.}}>",
            [
                @"ZgotmplZ",
                @"ZgotmplZ",
                @"ZgotmplZ",
                // Allowed and HTML escaped.
                @" dir=""ltr""",
                @"ZgotmplZ",
                @"ZgotmplZ",
                @"ZgotmplZ",
                @"ZgotmplZ",
                @"ZgotmplZ"
            ]
        ),
        new(
            @"<a title={{.}}>",
            [
                @"&lt;b&gt;&#32;&#34;foo%&#34;&#32;O&#39;Reilly&#32;&amp;bar;",
                @"a[href&#32;&#61;~&#32;&#34;//example.com&#34;]#foo",
                // Tags stripped, spaces escaped, entity not re-escaped.
                @"Hello,&#32;World&#32;&amp;tc!",
                @"&#32;dir&#61;&#34;ltr&#34;",
                @"c&#32;&amp;&amp;&#32;alert(&#34;Hello,&#32;World!&#34;);",
                @"Hello,&#32;World&#32;&amp;&#32;O&#39;Reilly\u0021",
                @"greeting&#61;H%69,&amp;addressee&#61;(World)",
                @"greeting&#61;H%69,&amp;addressee&#61;(World)&#32;2x,&#32;https://golang.org/favicon.ico&#32;500.5w",
                @",foo/,"
            ]
        ),
        new(
            @"<a title='{{.}}'>",
            [
                @"&lt;b&gt; &#34;foo%&#34; O&#39;Reilly &amp;bar;",
                @"a[href =~ &#34;//example.com&#34;]#foo",
                // Tags stripped, entity not re-escaped.
                @"Hello, World &amp;tc!",
                @" dir=&#34;ltr&#34;",
                @"c &amp;&amp; alert(&#34;Hello, World!&#34;);",
                @"Hello, World &amp; O&#39;Reilly\u0021",
                @"greeting=H%69,&amp;addressee=(World)",
                @"greeting=H%69,&amp;addressee=(World) 2x, https://golang.org/favicon.ico 500.5w",
                @",foo/,"
            ]
        ),
        new(
            @"<textarea>{{.}}</textarea>",
            [
                @"&lt;b&gt; &#34;foo%&#34; O&#39;Reilly &amp;bar;",
                @"a[href =~ &#34;//example.com&#34;]#foo",
                // Angle brackets escaped to prevent injection of close tags, entity not re-escaped.
                @"Hello, &lt;b&gt;World&lt;/b&gt; &amp;tc!",
                @" dir=&#34;ltr&#34;",
                @"c &amp;&amp; alert(&#34;Hello, World!&#34;);",
                @"Hello, World &amp; O&#39;Reilly\u0021",
                @"greeting=H%69,&amp;addressee=(World)",
                @"greeting=H%69,&amp;addressee=(World) 2x, https://golang.org/favicon.ico 500.5w",
                @",foo/,"
            ]
        ),
        new(
            @"<script>alert({{.}})</script>",
            [
                @"""\u003cb\u003e \""foo%\"" O'Reilly \u0026bar;""",
                @"""a[href =~ \""//example.com\""]#foo""",
                @"""Hello, \u003cb\u003eWorld\u003c/b\u003e \u0026amp;tc!""",
                @""" dir=\""ltr\""""",
                // Not escaped.
                @"c && alert(""Hello, World!"");",
                // Escape sequence not over-escaped.
                @"""Hello, World & O'Reilly\u0021""",
                @"""greeting=H%69,\u0026addressee=(World)""",
                @"""greeting=H%69,\u0026addressee=(World) 2x, https://golang.org/favicon.ico 500.5w""",
                @""",foo/,"""
            ]
        ),
        new(
            @"<button onclick=""alert({{.}})"">",
            [
                @"&#34;\u003cb\u003e \&#34;foo%\&#34; O&#39;Reilly \u0026bar;&#34;",
                @"&#34;a[href =~ \&#34;//example.com\&#34;]#foo&#34;",
                @"&#34;Hello, \u003cb\u003eWorld\u003c/b\u003e \u0026amp;tc!&#34;",
                @"&#34; dir=\&#34;ltr\&#34;&#34;",
                // Not JS escaped but HTML escaped.
                @"c &amp;&amp; alert(&#34;Hello, World!&#34;);",
                // Escape sequence not over-escaped.
                @"&#34;Hello, World &amp; O&#39;Reilly\u0021&#34;",
                @"&#34;greeting=H%69,\u0026addressee=(World)&#34;",
                @"&#34;greeting=H%69,\u0026addressee=(World) 2x, https://golang.org/favicon.ico 500.5w&#34;",
                @"&#34;,foo/,&#34;"
            ]
        ),
        new(
            @"<script>alert(""{{.}}"")</script>",
            [
                @"\u003cb\u003e \u0022foo%\u0022 O\u0027Reilly \u0026bar;",
                @"a[href =~ \u0022\/\/example.com\u0022]#foo",
                @"Hello, \u003cb\u003eWorld\u003c\/b\u003e \u0026amp;tc!",
                @" dir=\u0022ltr\u0022",
                @"c \u0026\u0026 alert(\u0022Hello, World!\u0022);",
                // Escape sequence not over-escaped.
                @"Hello, World \u0026 O\u0027Reilly\u0021",
                @"greeting=H%69,\u0026addressee=(World)",
                @"greeting=H%69,\u0026addressee=(World) 2x, https:\/\/golang.org\/favicon.ico 500.5w",
                @",foo\/,"
            ]
        ),
        new(
            @"<script type=""text/javascript"">alert(""{{.}}"")</script>",
            [
                @"\u003cb\u003e \u0022foo%\u0022 O\u0027Reilly \u0026bar;",
                @"a[href =~ \u0022\/\/example.com\u0022]#foo",
                @"Hello, \u003cb\u003eWorld\u003c\/b\u003e \u0026amp;tc!",
                @" dir=\u0022ltr\u0022",
                @"c \u0026\u0026 alert(\u0022Hello, World!\u0022);",
                // Escape sequence not over-escaped.
                @"Hello, World \u0026 O\u0027Reilly\u0021",
                @"greeting=H%69,\u0026addressee=(World)",
                @"greeting=H%69,\u0026addressee=(World) 2x, https:\/\/golang.org\/favicon.ico 500.5w",
                @",foo\/,"
            ]
        ),
        new(
            @"<script type=""text/javascript"">alert({{.}})</script>",
            [
                @"""\u003cb\u003e \""foo%\"" O'Reilly \u0026bar;""",
                @"""a[href =~ \""//example.com\""]#foo""",
                @"""Hello, \u003cb\u003eWorld\u003c/b\u003e \u0026amp;tc!""",
                @""" dir=\""ltr\""""",
                // Not escaped.
                @"c && alert(""Hello, World!"");",
                // Escape sequence not over-escaped.
                @"""Hello, World & O'Reilly\u0021""",
                @"""greeting=H%69,\u0026addressee=(World)""",
                @"""greeting=H%69,\u0026addressee=(World) 2x, https://golang.org/favicon.ico 500.5w""",
                @""",foo/,"""
            ]
        ),
        new(
            // Not treated as JS. The output is same as for <div>{{.}}</div>
            @"<script type=""text/template"">{{.}}</script>",
            [
                @"&lt;b&gt; &#34;foo%&#34; O&#39;Reilly &amp;bar;",
                @"a[href =~ &#34;//example.com&#34;]#foo",
                // Not escaped.
                @"Hello, <b>World</b> &amp;tc!",
                @" dir=&#34;ltr&#34;",
                @"c &amp;&amp; alert(&#34;Hello, World!&#34;);",
                @"Hello, World &amp; O&#39;Reilly\u0021",
                @"greeting=H%69,&amp;addressee=(World)",
                @"greeting=H%69,&amp;addressee=(World) 2x, https://golang.org/favicon.ico 500.5w",
                @",foo/,"
            ]
        ),
        new(
            @"<button onclick='alert(""{{.}}"")'>",
            [
                @"\u003cb\u003e \u0022foo%\u0022 O\u0027Reilly \u0026bar;",
                @"a[href =~ \u0022\/\/example.com\u0022]#foo",
                @"Hello, \u003cb\u003eWorld\u003c\/b\u003e \u0026amp;tc!",
                @" dir=\u0022ltr\u0022",
                @"c \u0026\u0026 alert(\u0022Hello, World!\u0022);",
                // Escape sequence not over-escaped.
                @"Hello, World \u0026 O\u0027Reilly\u0021",
                @"greeting=H%69,\u0026addressee=(World)",
                @"greeting=H%69,\u0026addressee=(World) 2x, https:\/\/golang.org\/favicon.ico 500.5w",
                @",foo\/,"
            ]
        ),
        new(
            @"<a href=""?q={{.}}"">",
            [
                @"%3cb%3e%20%22foo%25%22%20O%27Reilly%20%26bar%3b",
                @"a%5bhref%20%3d~%20%22%2f%2fexample.com%22%5d%23foo",
                @"Hello%2c%20%3cb%3eWorld%3c%2fb%3e%20%26amp%3btc%21",
                @"%20dir%3d%22ltr%22",
                @"c%20%26%26%20alert%28%22Hello%2c%20World%21%22%29%3b",
                @"Hello%2c%20World%20%26%20O%27Reilly%5cu0021",
                // Quotes and parens are escaped but %69 is not over-escaped. HTML escaping is done.
                @"greeting=H%69,&amp;addressee=%28World%29",
                @"greeting%3dH%2569%2c%26addressee%3d%28World%29%202x%2c%20https%3a%2f%2fgolang.org%2ffavicon.ico%20500.5w",
                @",foo/,"
            ]
        ),
        new(
            @"<style>body { background: url('?img={{.}}') }</style>",
            [
                @"%3cb%3e%20%22foo%25%22%20O%27Reilly%20%26bar%3b",
                @"a%5bhref%20%3d~%20%22%2f%2fexample.com%22%5d%23foo",
                @"Hello%2c%20%3cb%3eWorld%3c%2fb%3e%20%26amp%3btc%21",
                @"%20dir%3d%22ltr%22",
                @"c%20%26%26%20alert%28%22Hello%2c%20World%21%22%29%3b",
                @"Hello%2c%20World%20%26%20O%27Reilly%5cu0021",
                // Quotes and parens are escaped but %69 is not over-escaped. HTML escaping is not done.
                @"greeting=H%69,&addressee=%28World%29",
                @"greeting%3dH%2569%2c%26addressee%3d%28World%29%202x%2c%20https%3a%2f%2fgolang.org%2ffavicon.ico%20500.5w",
                @",foo/,"
            ]
        ),
        new(
            @"<img srcset=""{{.}}"">",
            [
                @"#ZgotmplZ",
                @"#ZgotmplZ",
                // Commas are not escaped.
                @"Hello,#ZgotmplZ",
                // Leading spaces are not percent escapes.
                @" dir=%22ltr%22",
                // Spaces after commas are not percent escaped.
                @"#ZgotmplZ, World!%22%29;",
                @"Hello,#ZgotmplZ",
                @"greeting=H%69%2c&amp;addressee=%28World%29",
                // Metadata is not escaped.
                @"greeting=H%69,&amp;addressee=(World) 2x, https://golang.org/favicon.ico 500.5w",
                @"%2cfoo/%2c"
            ]
        ),
        new(
            @"<img srcset={{.}}>",
            [
                @"#ZgotmplZ",
                @"#ZgotmplZ",
                @"Hello,#ZgotmplZ",
                // Spaces are HTML escaped not %-escaped
                @"&#32;dir&#61;%22ltr%22",
                @"#ZgotmplZ,&#32;World!%22%29;",
                @"Hello,#ZgotmplZ",
                @"greeting&#61;H%69%2c&amp;addressee&#61;%28World%29",
                @"greeting&#61;H%69,&amp;addressee&#61;(World)&#32;2x,&#32;https://golang.org/favicon.ico&#32;500.5w",
                // Commas are escaped.
                @"%2cfoo/%2c"
            ]
        ),
        new(
            @"<img srcset=""{{.}} 2x, https://golang.org/ 500.5w"">",
            [
                @"#ZgotmplZ",
                @"#ZgotmplZ",
                @"Hello,#ZgotmplZ",
                @" dir=%22ltr%22",
                @"#ZgotmplZ, World!%22%29;",
                @"Hello,#ZgotmplZ",
                @"greeting=H%69%2c&amp;addressee=%28World%29",
                @"greeting=H%69,&amp;addressee=(World) 2x, https://golang.org/favicon.ico 500.5w",
                @"%2cfoo/%2c"
            ]
        ),
        new(
            @"<img srcset=""http://godoc.org/ {{.}}, https://golang.org/ 500.5w"">",
            [
                @"#ZgotmplZ",
                @"#ZgotmplZ",
                @"Hello,#ZgotmplZ",
                @" dir=%22ltr%22",
                @"#ZgotmplZ, World!%22%29;",
                @"Hello,#ZgotmplZ",
                @"greeting=H%69%2c&amp;addressee=%28World%29",
                @"greeting=H%69,&amp;addressee=(World) 2x, https://golang.org/favicon.ico 500.5w",
                @"%2cfoo/%2c"
            ]
        ),
        new(
            @"<img srcset=""http://godoc.org/?q={{.}} 2x, https://golang.org/ 500.5w"">",
            [
                @"#ZgotmplZ",
                @"#ZgotmplZ",
                @"Hello,#ZgotmplZ",
                @" dir=%22ltr%22",
                @"#ZgotmplZ, World!%22%29;",
                @"Hello,#ZgotmplZ",
                @"greeting=H%69%2c&amp;addressee=%28World%29",
                @"greeting=H%69,&amp;addressee=(World) 2x, https://golang.org/favicon.ico 500.5w",
                @"%2cfoo/%2c"
            ]
        ),
        new(
            @"<img srcset=""http://godoc.org/ 2x, {{.}} 500.5w"">",
            [
                @"#ZgotmplZ",
                @"#ZgotmplZ",
                @"Hello,#ZgotmplZ",
                @" dir=%22ltr%22",
                @"#ZgotmplZ, World!%22%29;",
                @"Hello,#ZgotmplZ",
                @"greeting=H%69%2c&amp;addressee=%28World%29",
                @"greeting=H%69,&amp;addressee=(World) 2x, https://golang.org/favicon.ico 500.5w",
                @"%2cfoo/%2c"
            ]
        ),
        new(
            @"<img srcset=""http://godoc.org/ 2x, https://golang.org/ {{.}}"">",
            [
                @"#ZgotmplZ",
                @"#ZgotmplZ",
                @"Hello,#ZgotmplZ",
                @" dir=%22ltr%22",
                @"#ZgotmplZ, World!%22%29;",
                @"Hello,#ZgotmplZ",
                @"greeting=H%69%2c&amp;addressee=%28World%29",
                @"greeting=H%69,&amp;addressee=(World) 2x, https://golang.org/favicon.ico 500.5w",
                @"%2cfoo/%2c"
            ]
        )
    ]);

    foreach (var (_, test) in range(tests)) {
        var (ᴛ1, ᴛ2) = New("x").Parse(test.input);
        var tmpl = Must(ᴛ1, ᴛ2);
        nint pre = strings.Index(test.input, "{{.}}");
        nint post = len(test.input) - (pre + 5);
        ref var b = ref heap(new strings.Builder(), out var Ꮡb);
        foreach (var (i, x) in range(data)) {
            b.Reset();
            var err = tmpl.Execute(new template_test_package.strings_BuilderжWriter(Ꮡb), x); if (err != nil) {
                t.Errorf("%q with %v: %s", test.input, x, err);
                continue;
            }
            @string want = test.want[i];
            @string got = b.String().slice(pre, b.Len() - post); if (want != got) {
                t.Errorf("%q with %v:\nwant\n\t%q,\ngot\n\t%q\n", test.input, x, want, got);
                continue;
            }
        }
    }
}

// Test that we print using the String method. Was issue 3073.
internal partial struct myStringer {
    internal nint v;
}

internal static @string String(this ref myStringer s) {
    return fmt.Sprintf("string=%d"u8, s.v);
}

internal partial struct errorer {
    internal nint v;
}

internal static @string Error(this ref errorer s) {
    return fmt.Sprintf("error=%d"u8, s.v);
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static ref readonly @string string3ˢ => ref string3ˢᵇ;
private static readonly @string string3ˢᵇ = "string=3"u8;
internal static ref readonly @string error7ˢ => ref error7ˢᵇ;
private static readonly @string error7ˢᵇ = "error=7"u8;

public static void TestStringer(ж<testing.T> t) {
    var s = Ꮡ(new myStringer(3));
    var b = @new<strings.Builder>();
    var (ᴛ3, ᴛ4) = New("x").Parse("{{.}}");
    var tmpl = Must(ᴛ3, ᴛ4);
    {
        var err = tmpl.Execute(new template_test_package.strings_BuilderжWriter(b), s.OrTypedNil()); if (err != nil) {
            t.Fatal(err);
        }
    }
    @string expect = string3ˢ;
    if (b.String() != expect) {
        t.Errorf("expected %q got %q", expect, b.String());
    }
    var e = Ꮡ(new errorer(7));
    b.Reset();
    {
        var err = tmpl.Execute(new template_test_package.strings_BuilderжWriter(b), e.OrTypedNil()); if (err != nil) {
            t.Fatal(err);
        }
    }
    expect = error7ˢ;
    if (b.String() != expect) {
        t.Errorf("expected %q got %q", expect, b.String());
    }
}

internal partial struct TestEscapingNilNonemptyInterfaces_testData /*dyn*/ {
    public error E;
}

internal partial struct TestEscapingNilNonemptyInterfaces_data /*dyn*/ {
    public any E;
}

// https://golang.org/issue/5982
public static void TestEscapingNilNonemptyInterfaces(ж<testing.T> t) {
    var (ᴛ5, ᴛ6) = New("x").Parse("{{.E}}");
    var tmpl = Must(ᴛ5, ᴛ6);

    var got = @new<bytes.Buffer>();
    var testData = new TestEscapingNilNonemptyInterfaces_testData(); // any non-empty interface here will do; error is just ready at hand
    tmpl.Execute(new template_test_package.bytes_BufferжWriter(got), testData);

    // A non-empty interface should print like an empty interface.
    var want = @new<bytes.Buffer>();
    var data = new TestEscapingNilNonemptyInterfaces_data();
    tmpl.Execute(new template_test_package.bytes_BufferжWriter(want), data);

    if (!bytes.Equal(want.Bytes(), got.Bytes())) {
        t.Errorf("expected %q got %q", (@string)want.Bytes(), (@string)got.Bytes());
    }
}

} // end template_internal_test_package
