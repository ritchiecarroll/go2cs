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
internal static readonly @string aHrefExampleComFooˢ = @"a[href =~ ""//example.com""]#foo"u8;
internal static readonly @string helloBWorldBAmpTcˢ = @"Hello, <b>World</b> &amp;tc!"u8;
internal static readonly @string dirLtrˢ = @" dir=""ltr"""u8;
internal static readonly @string cAlertHelloWorldˢ = @"c && alert(""Hello, World!"");"u8;
internal static readonly @string helloWorldOReillyU0021ˢ = @"Hello, World & O'Reilly\u0021"u8;
internal static readonly @string greetingH69Addresseeˢ = @"greeting=H%69,&addressee=(World)"u8;
internal static readonly @string greetingH69Addresseeˢ2 = @"greeting=H%69,&addressee=(World) 2x, https://golang.org/favicon.ico 500.5w"u8;
internal static readonly @string fooˢ = @",foo/,"u8;

[GoType("dyn")] internal partial struct TestTypedContent_tests {
    // A template containing a single {{.}}.
    internal @string input;
    internal slice<@string> want;
}

public static void TestTypedContent(ж<testing.T> Ꮡt) {
    ref var t = ref Ꮡt.DerefOrNull();

    var data = new any[]{
        (@string)@"<b> ""foo%"" O'Reilly &bar;"u8,
        ((global::go.html.template_package.CSS)(@string)aHrefExampleComFooˢ),
        ((global::go.html.template_package.HTML)(@string)helloBWorldBAmpTcˢ),
        ((global::go.html.template_package.HTMLAttr)(@string)dirLtrˢ),
        ((global::go.html.template_package.JS)(@string)cAlertHelloWorldˢ),
        ((global::go.html.template_package.JSStr)(@string)helloWorldOReillyU0021ˢ),
        ((global::go.html.template_package.URL)(@string)greetingH69Addresseeˢ),
        ((global::go.html.template_package.Srcset)(@string)greetingH69Addresseeˢ2),
        ((global::go.html.template_package.URL)(@string)fooˢ)
    }.slice();
    // For each content sensitive escaper, see how it does on
    // each of the typed strings above.
    var tests = new TestTypedContent_tests[]{
        new(
            @"<style>{{.}} { color: blue }</style>"u8,
            new @string[]{
                @"ZgotmplZ"u8, // Allowed but not escaped.

                @"a[href =~ ""//example.com""]#foo"u8,
                @"ZgotmplZ"u8,
                @"ZgotmplZ"u8,
                @"ZgotmplZ"u8,
                @"ZgotmplZ"u8,
                @"ZgotmplZ"u8,
                @"ZgotmplZ"u8,
                @"ZgotmplZ"u8
            }.slice()
        ),
        new(
            @"<div style=""{{.}}"">"u8,
            new @string[]{
                @"ZgotmplZ"u8, // Allowed and HTML escaped.

                @"a[href =~ &#34;//example.com&#34;]#foo"u8,
                @"ZgotmplZ"u8,
                @"ZgotmplZ"u8,
                @"ZgotmplZ"u8,
                @"ZgotmplZ"u8,
                @"ZgotmplZ"u8,
                @"ZgotmplZ"u8,
                @"ZgotmplZ"u8
            }.slice()
        ),
        new(
            @"{{.}}"u8,
            new @string[]{
                @"&lt;b&gt; &#34;foo%&#34; O&#39;Reilly &amp;bar;"u8,
                @"a[href =~ &#34;//example.com&#34;]#foo"u8, // Not escaped.

                @"Hello, <b>World</b> &amp;tc!"u8,
                @" dir=&#34;ltr&#34;"u8,
                @"c &amp;&amp; alert(&#34;Hello, World!&#34;);"u8,
                @"Hello, World &amp; O&#39;Reilly\u0021"u8,
                @"greeting=H%69,&amp;addressee=(World)"u8,
                @"greeting=H%69,&amp;addressee=(World) 2x, https://golang.org/favicon.ico 500.5w"u8,
                @",foo/,"u8
            }.slice()
        ),
        new(
            @"<a{{.}}>"u8,
            new @string[]{
                @"ZgotmplZ"u8,
                @"ZgotmplZ"u8,
                @"ZgotmplZ"u8, // Allowed and HTML escaped.

                @" dir=""ltr"""u8,
                @"ZgotmplZ"u8,
                @"ZgotmplZ"u8,
                @"ZgotmplZ"u8,
                @"ZgotmplZ"u8,
                @"ZgotmplZ"u8
            }.slice()
        ),
        new(
            @"<a title={{.}}>"u8,
            new @string[]{
                @"&lt;b&gt;&#32;&#34;foo%&#34;&#32;O&#39;Reilly&#32;&amp;bar;"u8,
                @"a[href&#32;&#61;~&#32;&#34;//example.com&#34;]#foo"u8, // Tags stripped, spaces escaped, entity not re-escaped.

                @"Hello,&#32;World&#32;&amp;tc!"u8,
                @"&#32;dir&#61;&#34;ltr&#34;"u8,
                @"c&#32;&amp;&amp;&#32;alert(&#34;Hello,&#32;World!&#34;);"u8,
                @"Hello,&#32;World&#32;&amp;&#32;O&#39;Reilly\u0021"u8,
                @"greeting&#61;H%69,&amp;addressee&#61;(World)"u8,
                @"greeting&#61;H%69,&amp;addressee&#61;(World)&#32;2x,&#32;https://golang.org/favicon.ico&#32;500.5w"u8,
                @",foo/,"u8
            }.slice()
        ),
        new(
            @"<a title='{{.}}'>"u8,
            new @string[]{
                @"&lt;b&gt; &#34;foo%&#34; O&#39;Reilly &amp;bar;"u8,
                @"a[href =~ &#34;//example.com&#34;]#foo"u8, // Tags stripped, entity not re-escaped.

                @"Hello, World &amp;tc!"u8,
                @" dir=&#34;ltr&#34;"u8,
                @"c &amp;&amp; alert(&#34;Hello, World!&#34;);"u8,
                @"Hello, World &amp; O&#39;Reilly\u0021"u8,
                @"greeting=H%69,&amp;addressee=(World)"u8,
                @"greeting=H%69,&amp;addressee=(World) 2x, https://golang.org/favicon.ico 500.5w"u8,
                @",foo/,"u8
            }.slice()
        ),
        new(
            @"<textarea>{{.}}</textarea>"u8,
            new @string[]{
                @"&lt;b&gt; &#34;foo%&#34; O&#39;Reilly &amp;bar;"u8,
                @"a[href =~ &#34;//example.com&#34;]#foo"u8, // Angle brackets escaped to prevent injection of close tags, entity not re-escaped.

                @"Hello, &lt;b&gt;World&lt;/b&gt; &amp;tc!"u8,
                @" dir=&#34;ltr&#34;"u8,
                @"c &amp;&amp; alert(&#34;Hello, World!&#34;);"u8,
                @"Hello, World &amp; O&#39;Reilly\u0021"u8,
                @"greeting=H%69,&amp;addressee=(World)"u8,
                @"greeting=H%69,&amp;addressee=(World) 2x, https://golang.org/favicon.ico 500.5w"u8,
                @",foo/,"u8
            }.slice()
        ),
        new(
            @"<script>alert({{.}})</script>"u8,
            new @string[]{
                @"""\u003cb\u003e \""foo%\"" O'Reilly \u0026bar;"""u8,
                @"""a[href =~ \""//example.com\""]#foo"""u8,
                @"""Hello, \u003cb\u003eWorld\u003c/b\u003e \u0026amp;tc!"""u8,
                @""" dir=\""ltr\"""""u8, // Not escaped.

                @"c && alert(""Hello, World!"");"u8, // Escape sequence not over-escaped.

                @"""Hello, World & O'Reilly\u0021"""u8,
                @"""greeting=H%69,\u0026addressee=(World)"""u8,
                @"""greeting=H%69,\u0026addressee=(World) 2x, https://golang.org/favicon.ico 500.5w"""u8,
                @""",foo/,"""u8
            }.slice()
        ),
        new(
            @"<button onclick=""alert({{.}})"">"u8,
            new @string[]{
                @"&#34;\u003cb\u003e \&#34;foo%\&#34; O&#39;Reilly \u0026bar;&#34;"u8,
                @"&#34;a[href =~ \&#34;//example.com\&#34;]#foo&#34;"u8,
                @"&#34;Hello, \u003cb\u003eWorld\u003c/b\u003e \u0026amp;tc!&#34;"u8,
                @"&#34; dir=\&#34;ltr\&#34;&#34;"u8, // Not JS escaped but HTML escaped.

                @"c &amp;&amp; alert(&#34;Hello, World!&#34;);"u8, // Escape sequence not over-escaped.

                @"&#34;Hello, World &amp; O&#39;Reilly\u0021&#34;"u8,
                @"&#34;greeting=H%69,\u0026addressee=(World)&#34;"u8,
                @"&#34;greeting=H%69,\u0026addressee=(World) 2x, https://golang.org/favicon.ico 500.5w&#34;"u8,
                @"&#34;,foo/,&#34;"u8
            }.slice()
        ),
        new(
            @"<script>alert(""{{.}}"")</script>"u8,
            new @string[]{
                @"\u003cb\u003e \u0022foo%\u0022 O\u0027Reilly \u0026bar;"u8,
                @"a[href =~ \u0022\/\/example.com\u0022]#foo"u8,
                @"Hello, \u003cb\u003eWorld\u003c\/b\u003e \u0026amp;tc!"u8,
                @" dir=\u0022ltr\u0022"u8,
                @"c \u0026\u0026 alert(\u0022Hello, World!\u0022);"u8, // Escape sequence not over-escaped.

                @"Hello, World \u0026 O\u0027Reilly\u0021"u8,
                @"greeting=H%69,\u0026addressee=(World)"u8,
                @"greeting=H%69,\u0026addressee=(World) 2x, https:\/\/golang.org\/favicon.ico 500.5w"u8,
                @",foo\/,"u8
            }.slice()
        ),
        new(
            @"<script type=""text/javascript"">alert(""{{.}}"")</script>"u8,
            new @string[]{
                @"\u003cb\u003e \u0022foo%\u0022 O\u0027Reilly \u0026bar;"u8,
                @"a[href =~ \u0022\/\/example.com\u0022]#foo"u8,
                @"Hello, \u003cb\u003eWorld\u003c\/b\u003e \u0026amp;tc!"u8,
                @" dir=\u0022ltr\u0022"u8,
                @"c \u0026\u0026 alert(\u0022Hello, World!\u0022);"u8, // Escape sequence not over-escaped.

                @"Hello, World \u0026 O\u0027Reilly\u0021"u8,
                @"greeting=H%69,\u0026addressee=(World)"u8,
                @"greeting=H%69,\u0026addressee=(World) 2x, https:\/\/golang.org\/favicon.ico 500.5w"u8,
                @",foo\/,"u8
            }.slice()
        ),
        new(
            @"<script type=""text/javascript"">alert({{.}})</script>"u8,
            new @string[]{
                @"""\u003cb\u003e \""foo%\"" O'Reilly \u0026bar;"""u8,
                @"""a[href =~ \""//example.com\""]#foo"""u8,
                @"""Hello, \u003cb\u003eWorld\u003c/b\u003e \u0026amp;tc!"""u8,
                @""" dir=\""ltr\"""""u8, // Not escaped.

                @"c && alert(""Hello, World!"");"u8, // Escape sequence not over-escaped.

                @"""Hello, World & O'Reilly\u0021"""u8,
                @"""greeting=H%69,\u0026addressee=(World)"""u8,
                @"""greeting=H%69,\u0026addressee=(World) 2x, https://golang.org/favicon.ico 500.5w"""u8,
                @""",foo/,"""u8
            }.slice()
        ),
        new(
            @"<script type=""text/template"">{{.}}</script>"u8, // Not treated as JS. The output is same as for <div>{{.}}</div>

            new @string[]{
                @"&lt;b&gt; &#34;foo%&#34; O&#39;Reilly &amp;bar;"u8,
                @"a[href =~ &#34;//example.com&#34;]#foo"u8, // Not escaped.

                @"Hello, <b>World</b> &amp;tc!"u8,
                @" dir=&#34;ltr&#34;"u8,
                @"c &amp;&amp; alert(&#34;Hello, World!&#34;);"u8,
                @"Hello, World &amp; O&#39;Reilly\u0021"u8,
                @"greeting=H%69,&amp;addressee=(World)"u8,
                @"greeting=H%69,&amp;addressee=(World) 2x, https://golang.org/favicon.ico 500.5w"u8,
                @",foo/,"u8
            }.slice()
        ),
        new(
            @"<button onclick='alert(""{{.}}"")'>"u8,
            new @string[]{
                @"\u003cb\u003e \u0022foo%\u0022 O\u0027Reilly \u0026bar;"u8,
                @"a[href =~ \u0022\/\/example.com\u0022]#foo"u8,
                @"Hello, \u003cb\u003eWorld\u003c\/b\u003e \u0026amp;tc!"u8,
                @" dir=\u0022ltr\u0022"u8,
                @"c \u0026\u0026 alert(\u0022Hello, World!\u0022);"u8, // Escape sequence not over-escaped.

                @"Hello, World \u0026 O\u0027Reilly\u0021"u8,
                @"greeting=H%69,\u0026addressee=(World)"u8,
                @"greeting=H%69,\u0026addressee=(World) 2x, https:\/\/golang.org\/favicon.ico 500.5w"u8,
                @",foo\/,"u8
            }.slice()
        ),
        new(
            @"<a href=""?q={{.}}"">"u8,
            new @string[]{
                @"%3cb%3e%20%22foo%25%22%20O%27Reilly%20%26bar%3b"u8,
                @"a%5bhref%20%3d~%20%22%2f%2fexample.com%22%5d%23foo"u8,
                @"Hello%2c%20%3cb%3eWorld%3c%2fb%3e%20%26amp%3btc%21"u8,
                @"%20dir%3d%22ltr%22"u8,
                @"c%20%26%26%20alert%28%22Hello%2c%20World%21%22%29%3b"u8,
                @"Hello%2c%20World%20%26%20O%27Reilly%5cu0021"u8, // Quotes and parens are escaped but %69 is not over-escaped. HTML escaping is done.

                @"greeting=H%69,&amp;addressee=%28World%29"u8,
                @"greeting%3dH%2569%2c%26addressee%3d%28World%29%202x%2c%20https%3a%2f%2fgolang.org%2ffavicon.ico%20500.5w"u8,
                @",foo/,"u8
            }.slice()
        ),
        new(
            @"<style>body { background: url('?img={{.}}') }</style>"u8,
            new @string[]{
                @"%3cb%3e%20%22foo%25%22%20O%27Reilly%20%26bar%3b"u8,
                @"a%5bhref%20%3d~%20%22%2f%2fexample.com%22%5d%23foo"u8,
                @"Hello%2c%20%3cb%3eWorld%3c%2fb%3e%20%26amp%3btc%21"u8,
                @"%20dir%3d%22ltr%22"u8,
                @"c%20%26%26%20alert%28%22Hello%2c%20World%21%22%29%3b"u8,
                @"Hello%2c%20World%20%26%20O%27Reilly%5cu0021"u8, // Quotes and parens are escaped but %69 is not over-escaped. HTML escaping is not done.

                @"greeting=H%69,&addressee=%28World%29"u8,
                @"greeting%3dH%2569%2c%26addressee%3d%28World%29%202x%2c%20https%3a%2f%2fgolang.org%2ffavicon.ico%20500.5w"u8,
                @",foo/,"u8
            }.slice()
        ),
        new(
            @"<img srcset=""{{.}}"">"u8,
            new @string[]{
                @"#ZgotmplZ"u8,
                @"#ZgotmplZ"u8, // Commas are not escaped.

                @"Hello,#ZgotmplZ"u8, // Leading spaces are not percent escapes.

                @" dir=%22ltr%22"u8, // Spaces after commas are not percent escaped.

                @"#ZgotmplZ, World!%22%29;"u8,
                @"Hello,#ZgotmplZ"u8,
                @"greeting=H%69%2c&amp;addressee=%28World%29"u8, // Metadata is not escaped.

                @"greeting=H%69,&amp;addressee=(World) 2x, https://golang.org/favicon.ico 500.5w"u8,
                @"%2cfoo/%2c"u8
            }.slice()
        ),
        new(
            @"<img srcset={{.}}>"u8,
            new @string[]{
                @"#ZgotmplZ"u8,
                @"#ZgotmplZ"u8,
                @"Hello,#ZgotmplZ"u8, // Spaces are HTML escaped not %-escaped

                @"&#32;dir&#61;%22ltr%22"u8,
                @"#ZgotmplZ,&#32;World!%22%29;"u8,
                @"Hello,#ZgotmplZ"u8,
                @"greeting&#61;H%69%2c&amp;addressee&#61;%28World%29"u8,
                @"greeting&#61;H%69,&amp;addressee&#61;(World)&#32;2x,&#32;https://golang.org/favicon.ico&#32;500.5w"u8, // Commas are escaped.

                @"%2cfoo/%2c"u8
            }.slice()
        ),
        new(
            @"<img srcset=""{{.}} 2x, https://golang.org/ 500.5w"">"u8,
            new @string[]{
                @"#ZgotmplZ"u8,
                @"#ZgotmplZ"u8,
                @"Hello,#ZgotmplZ"u8,
                @" dir=%22ltr%22"u8,
                @"#ZgotmplZ, World!%22%29;"u8,
                @"Hello,#ZgotmplZ"u8,
                @"greeting=H%69%2c&amp;addressee=%28World%29"u8,
                @"greeting=H%69,&amp;addressee=(World) 2x, https://golang.org/favicon.ico 500.5w"u8,
                @"%2cfoo/%2c"u8
            }.slice()
        ),
        new(
            @"<img srcset=""http://godoc.org/ {{.}}, https://golang.org/ 500.5w"">"u8,
            new @string[]{
                @"#ZgotmplZ"u8,
                @"#ZgotmplZ"u8,
                @"Hello,#ZgotmplZ"u8,
                @" dir=%22ltr%22"u8,
                @"#ZgotmplZ, World!%22%29;"u8,
                @"Hello,#ZgotmplZ"u8,
                @"greeting=H%69%2c&amp;addressee=%28World%29"u8,
                @"greeting=H%69,&amp;addressee=(World) 2x, https://golang.org/favicon.ico 500.5w"u8,
                @"%2cfoo/%2c"u8
            }.slice()
        ),
        new(
            @"<img srcset=""http://godoc.org/?q={{.}} 2x, https://golang.org/ 500.5w"">"u8,
            new @string[]{
                @"#ZgotmplZ"u8,
                @"#ZgotmplZ"u8,
                @"Hello,#ZgotmplZ"u8,
                @" dir=%22ltr%22"u8,
                @"#ZgotmplZ, World!%22%29;"u8,
                @"Hello,#ZgotmplZ"u8,
                @"greeting=H%69%2c&amp;addressee=%28World%29"u8,
                @"greeting=H%69,&amp;addressee=(World) 2x, https://golang.org/favicon.ico 500.5w"u8,
                @"%2cfoo/%2c"u8
            }.slice()
        ),
        new(
            @"<img srcset=""http://godoc.org/ 2x, {{.}} 500.5w"">"u8,
            new @string[]{
                @"#ZgotmplZ"u8,
                @"#ZgotmplZ"u8,
                @"Hello,#ZgotmplZ"u8,
                @" dir=%22ltr%22"u8,
                @"#ZgotmplZ, World!%22%29;"u8,
                @"Hello,#ZgotmplZ"u8,
                @"greeting=H%69%2c&amp;addressee=%28World%29"u8,
                @"greeting=H%69,&amp;addressee=(World) 2x, https://golang.org/favicon.ico 500.5w"u8,
                @"%2cfoo/%2c"u8
            }.slice()
        ),
        new(
            @"<img srcset=""http://godoc.org/ 2x, https://golang.org/ {{.}}"">"u8,
            new @string[]{
                @"#ZgotmplZ"u8,
                @"#ZgotmplZ"u8,
                @"Hello,#ZgotmplZ"u8,
                @" dir=%22ltr%22"u8,
                @"#ZgotmplZ, World!%22%29;"u8,
                @"Hello,#ZgotmplZ"u8,
                @"greeting=H%69%2c&amp;addressee=%28World%29"u8,
                @"greeting=H%69,&amp;addressee=(World) 2x, https://golang.org/favicon.ico 500.5w"u8,
                @"%2cfoo/%2c"u8
            }.slice()
        )
    }.slice();
    foreach (var (_, test) in tests) {
        var (ᴛ1, ᴛ2) = New("x"u8).Parse(test.input);
        var tmpl = Must(ᴛ1, ᴛ2);
        nint pre = strings.Index(test.input, "{{.}}"u8);
        nint post = len(test.input) - (pre + 5);
        ref var b = ref heap(new strings.Builder(), out var Ꮡb);
        foreach (var (i, x) in data) {
            b.Reset();
            {
                var err = tmpl.Execute(new template_test_package.strings_BuilderжWriter(Ꮡb), x); if (err != default!) {
                    Ꮡt.Errorf("%q with %v: %s"u8, test.input, x, err);
                    continue;
                }
            }
            {
                @string want = test.want[i];
                @string got = b.String().slice(pre, b.Len() - post); if (want != got) {
                    Ꮡt.Errorf("%q with %v:\nwant\n\t%q,\ngot\n\t%q\n"u8, test.input, x, want, got);
                    continue;
                }
            }
        }
    }
}

// Test that we print using the String method. Was issue 3073.
[GoType] internal partial struct myStringer {
    internal nint v;
}

[GoRecv] internal static @string String(this ref myStringer s) {
    return fmt.Sprintf("string=%d"u8, s.v);
}

[GoType] internal partial struct errorer {
    internal nint v;
}

[GoRecv] internal static @string Error(this ref errorer s) {
    return fmt.Sprintf("error=%d"u8, s.v);
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static readonly @string string3ˢ = "string=3"u8;
internal static readonly @string error7ˢ = "error=7"u8;

public static void TestStringer(ж<testing.T> Ꮡt) {
    var s = Ꮡ(new myStringer(3));
    var b = @new<strings.Builder>();
    var (ᴛ3, ᴛ4) = New("x"u8).Parse("{{.}}"u8);
    var tmpl = Must(ᴛ3, ᴛ4);
    {
        var err = tmpl.Execute(new template_test_package.strings_BuilderжWriter(b), s.OrTypedNil()); if (err != default!) {
            Ꮡt.Fatal(err);
        }
    }
    @string expect = string3ˢ;
    if (b.String() != expect) {
        Ꮡt.Errorf("expected %q got %q"u8, expect, b.String());
    }
    var e = Ꮡ(new errorer(7));
    b.Reset();
    {
        var err = tmpl.Execute(new template_test_package.strings_BuilderжWriter(b), e.OrTypedNil()); if (err != default!) {
            Ꮡt.Fatal(err);
        }
    }
    expect = error7ˢ;
    if (b.String() != expect) {
        Ꮡt.Errorf("expected %q got %q"u8, expect, b.String());
    }
}

[GoType("dyn")] internal partial struct TestEscapingNilNonemptyInterfaces_testData {
    public error E;
}

[GoType("dyn")] internal partial struct TestEscapingNilNonemptyInterfaces_data {
    public any E;
}

// https://golang.org/issue/5982
public static void TestEscapingNilNonemptyInterfaces(ж<testing.T> Ꮡt) {
    var (ᴛ5, ᴛ6) = New("x"u8).Parse("{{.E}}"u8);
    var tmpl = Must(ᴛ5, ᴛ6);
    var got = @new<bytes.Buffer>();
    var testData = new TestEscapingNilNonemptyInterfaces_testData(); // any non-empty interface here will do; error is just ready at hand
    tmpl.Execute(new template_test_package.bytes_BufferжWriter(got), testData);
    // A non-empty interface should print like an empty interface.
    var want = @new<bytes.Buffer>();
    var data = new TestEscapingNilNonemptyInterfaces_data();
    tmpl.Execute(new template_test_package.bytes_BufferжWriter(want), data);
    if (!bytes.Equal(want.Bytes(), got.Bytes())) {
        Ꮡt.Errorf("expected %q got %q"u8, ((@string)want.Bytes()), ((@string)got.Bytes()));
    }
}

} // end template_internal_test_package
