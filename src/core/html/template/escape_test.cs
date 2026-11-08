// Copyright 2011 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.
namespace go.html;

using bytes = bytes_package;
using json = encoding.json_package;
using fmt = fmt_package;
using os = os_package;
using strings = strings_package;
using testing = testing_package;
using template = text.template_package;
using parse = text.template.parse_package;
using encoding;
using io = io_package;
using static go.html.template_package;
using text;
using text.template;
using ꓸꓸꓸany = Span<any>;

partial class template_internal_test_package {

internal partial struct badMarshaler {
}

internal static (slice<byte>, error) MarshalJSON(this ref badMarshaler x) {
    // Keys in valid JSON must be double quoted as must all strings.
    return (slice<byte>("{ foo: 'not quite valid JSON' }"u8), default);
}

internal partial struct goodMarshaler {
}

internal static (slice<byte>, error) MarshalJSON(this ref goodMarshaler x) {
    return (slice<byte>(@"{ ""<foo>"": ""O'Reilly"" }"u8), default);
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static ref readonly @string iexclBClassFooHelloBˢ => ref iexclBClassFooHelloBˢᵇ;
private static readonly @string iexclBClassFooHelloBˢᵇ = @"&iexcl;<b class=""foo"">Hello</b>, <textarea>O'World</textarea>!"u8;

internal partial struct TestEscape_data /*dyn*/ {
    public bool F, T;
    public @string C, G, H, I;
    public slice<@string> A, E;
    public json.Marshaler B, M;
    public nint N;
    public any U;     // untyped nil
    public ж<nint> Z; // typed nil
    public global::go.html.template_package.HTML W;
}

internal partial struct TestEscape_tests /*dyn*/ {
    internal @string name;
    internal @string input;
    internal @string output;
}

public static void TestEscape(ж<testing.T> t) {
    ref var data = ref heap<TestEscape_data>(new TestEscape_data(
        F: false,
        T: true,
        C: "<Cincinnati>",
        G: "<Goodbye>",
        H: "<Hello>",
        A: ["<a>", "<b>"],
        E: [],
        N: 42,
        B: new template_internal_test_package.badMarshalerжMarshaler(Ꮡ(new badMarshaler())),
        M: new template_internal_test_package.goodMarshalerжMarshaler(Ꮡ(new goodMarshaler())),
        U: default,
        Z: nil,
        W: ((global::go.html.template_package.HTML)(@string)iexclBClassFooHelloBˢ),
        I: "${ asd `` }"
    ), out var Ꮡdata);
    var pdata = Ꮡdata;

    var tests = slice<TestEscape_tests>([
        new(
            "if",
            "{{if .T}}Hello{{end}}, {{.C}}!",
            "Hello, &lt;Cincinnati&gt;!"
        ),
        new(
            "else",
            "{{if .F}}{{.H}}{{else}}{{.G}}{{end}}!",
            "&lt;Goodbye&gt;!"
        ),
        new(
            "overescaping1",
            "Hello, {{.C | html}}!",
            "Hello, &lt;Cincinnati&gt;!"
        ),
        new(
            "overescaping2",
            "Hello, {{html .C}}!",
            "Hello, &lt;Cincinnati&gt;!"
        ),
        new(
            "overescaping3",
            "{{with .C}}{{$msg := .}}Hello, {{$msg}}!{{end}}",
            "Hello, &lt;Cincinnati&gt;!"
        ),
        new(
            "assignment",
            "{{if $x := .H}}{{$x}}{{end}}",
            "&lt;Hello&gt;"
        ),
        new(
            "withBody",
            "{{with .H}}{{.}}{{end}}",
            "&lt;Hello&gt;"
        ),
        new(
            "withElse",
            "{{with .E}}{{.}}{{else}}{{.H}}{{end}}",
            "&lt;Hello&gt;"
        ),
        new(
            "rangeBody",
            "{{range .A}}{{.}}{{end}}",
            "&lt;a&gt;&lt;b&gt;"
        ),
        new(
            "rangeElse",
            "{{range .E}}{{.}}{{else}}{{.H}}{{end}}",
            "&lt;Hello&gt;"
        ),
        new(
            "nonStringValue",
            "{{.T}}",
            "true"
        ),
        new(
            "untypedNilValue",
            "{{.U}}",
            ""
        ),
        new(
            "typedNilValue",
            "{{.Z}}",
            "&lt;nil&gt;"
        ),
        new(
            "constant",
            @"<a href=""/search?q={{""'a<b'""}}"">",
            @"<a href=""/search?q=%27a%3cb%27"">"
        ),
        new(
            "multipleAttrs",
            "<a b=1 c={{.H}}>",
            "<a b=1 c=&lt;Hello&gt;>"
        ),
        new(
            "urlStartRel",
            @"<a href='{{""/foo/bar?a=b&c=d""}}'>",
            @"<a href='/foo/bar?a=b&amp;c=d'>"
        ),
        new(
            "urlStartAbsOk",
            @"<a href='{{""http://example.com/foo/bar?a=b&c=d""}}'>",
            @"<a href='http://example.com/foo/bar?a=b&amp;c=d'>"
        ),
        new(
            "protocolRelativeURLStart",
            @"<a href='{{""//example.com:8000/foo/bar?a=b&c=d""}}'>",
            @"<a href='//example.com:8000/foo/bar?a=b&amp;c=d'>"
        ),
        new(
            "pathRelativeURLStart",
            @"<a href=""{{""/javascript:80/foo/bar""}}"">",
            @"<a href=""/javascript:80/foo/bar"">"
        ),
        new(
            "dangerousURLStart",
            @"<a href='{{""javascript:alert(%22pwned%22)""}}'>",
            @"<a href='#ZgotmplZ'>"
        ),
        new(
            "dangerousURLStart2",
            @"<a href='  {{""javascript:alert(%22pwned%22)""}}'>",
            @"<a href='  #ZgotmplZ'>"
        ),
        new(
            "nonHierURL",
            @"<a href={{""mailto:Muhammed \""The Greatest\"" Ali <m.ali@example.com>""}}>",
            @"<a href=mailto:Muhammed%20%22The%20Greatest%22%20Ali%20%3cm.ali@example.com%3e>"
        ),
        new(
            "urlPath",
            @"<a href='http://{{""javascript:80""}}/foo'>",
            @"<a href='http://javascript:80/foo'>"
        ),
        new(
            "urlQuery",
            @"<a href='/search?q={{.H}}'>",
            @"<a href='/search?q=%3cHello%3e'>"
        ),
        new(
            "urlFragment",
            @"<a href='/faq#{{.H}}'>",
            @"<a href='/faq#%3cHello%3e'>"
        ),
        new(
            "urlBranch",
            @"<a href=""{{if .F}}/foo?a=b{{else}}/bar{{end}}"">",
            @"<a href=""/bar"">"
        ),
        new(
            "urlBranchConflictMoot",
            @"<a href=""{{if .T}}/foo?a={{else}}/bar#{{end}}{{.C}}"">",
            @"<a href=""/foo?a=%3cCincinnati%3e"">"
        ),
        new(
            "jsStrValue",
            "<button onclick='alert({{.H}})'>",
            @"<button onclick='alert(&#34;\u003cHello\u003e&#34;)'>"
        ),
        new(
            "jsNumericValue",
            "<button onclick='alert({{.N}})'>",
            @"<button onclick='alert( 42 )'>"
        ),
        new(
            "jsBoolValue",
            "<button onclick='alert({{.T}})'>",
            @"<button onclick='alert( true )'>"
        ),
        new(
            "jsNilValueTyped",
            "<button onclick='alert(typeof{{.Z}})'>",
            @"<button onclick='alert(typeof null )'>"
        ),
        new(
            "jsNilValueUntyped",
            "<button onclick='alert(typeof{{.U}})'>",
            @"<button onclick='alert(typeof null )'>"
        ),
        new(
            "jsObjValue",
            "<button onclick='alert({{.A}})'>",
            @"<button onclick='alert([&#34;\u003ca\u003e&#34;,&#34;\u003cb\u003e&#34;])'>"
        ),
        new(
            "jsObjValueScript",
            "<script>alert({{.A}})</script>",
            @"<script>alert([""\u003ca\u003e"",""\u003cb\u003e""])</script>"
        ),
        new(
            "scriptTypeSpace",
            "<script type=\" \">{{.H}}</script>",
            "<script type=\" \">\"\\u003cHello\\u003e\"</script>"
        ),
        new(
            "scriptTypeTab",
            "<script type=\"\t\">{{.H}}</script>",
            "<script type=\"\t\">\"\\u003cHello\\u003e\"</script>"
        ),
        new(
            "scriptTypeEmpty",
            "<script type=\"\">{{.H}}</script>",
            "<script type=\"\">\"\\u003cHello\\u003e\"</script>"
        ),
        new(
            "jsObjValueNotOverEscaped",
            "<button onclick='alert({{.A | html}})'>",
            @"<button onclick='alert([&#34;\u003ca\u003e&#34;,&#34;\u003cb\u003e&#34;])'>"
        ),
        new(
            "jsStr",
            "<button onclick='alert(&quot;{{.H}}&quot;)'>",
            @"<button onclick='alert(&quot;\u003cHello\u003e&quot;)'>"
        ),
        new(
            "badMarshaler",
            @"<button onclick='alert(1/{{.B}}in numbers)'>",
            @"<button onclick='alert(1/ /* json: error calling MarshalJSON for type *template.badMarshaler: invalid character &#39;f&#39; looking for beginning of object key string */null in numbers)'>"
        ),
        new(
            "jsMarshaler",
            @"<button onclick='alert({{.M}})'>",
            @"<button onclick='alert({&#34;\u003cfoo\u003e&#34;:&#34;O&#39;Reilly&#34;})'>"
        ),
        new(
            "jsStrNotUnderEscaped",
            "<button onclick='alert({{.C | urlquery}})'>",
            // URL escaped, then quoted for JS.
            @"<button onclick='alert(&#34;%3CCincinnati%3E&#34;)'>"
        ),
        new(
            "jsRe",
            @"<button onclick='alert(/{{""foo+bar""}}/.test(""""))'>",
            @"<button onclick='alert(/foo\u002bbar/.test(""""))'>"
        ),
        new(
            "jsReBlank",
            @"<script>alert(/{{""""}}/.test(""""));</script>",
            @"<script>alert(/(?:)/.test(""""));</script>"
        ),
        new(
            "jsReAmbigOk",
            @"<script>{{if true}}var x = 1{{end}}</script>",
            // The {if} ends in an ambiguous jsCtx but there is
            // no slash following so we shouldn't care.
            @"<script>var x = 1</script>"
        ),
        new(
            "styleBidiKeywordPassed",
            @"<p style=""dir: {{""ltr""}}"">",
            @"<p style=""dir: ltr"">"
        ),
        new(
            "styleBidiPropNamePassed",
            @"<p style=""border-{{""left""}}: 0; border-{{""right""}}: 1in"">",
            @"<p style=""border-left: 0; border-right: 1in"">"
        ),
        new(
            "styleExpressionBlocked",
            @"<p style=""width: {{""expression(alert(1337))""}}"">",
            @"<p style=""width: ZgotmplZ"">"
        ),
        new(
            "styleTagSelectorPassed",
            @"<style>{{""p""}} { color: pink }</style>",
            @"<style>p { color: pink }</style>"
        ),
        new(
            "styleIDPassed",
            @"<style>p{{""#my-ID""}} { font: Arial }</style>",
            @"<style>p#my-ID { font: Arial }</style>"
        ),
        new(
            "styleClassPassed",
            @"<style>p{{"".my_class""}} { font: Arial }</style>",
            @"<style>p.my_class { font: Arial }</style>"
        ),
        new(
            "styleQuantityPassed",
            @"<a style=""left: {{""2em""}}; top: {{0}}"">",
            @"<a style=""left: 2em; top: 0"">"
        ),
        new(
            "stylePctPassed",
            @"<table style=width:{{""100%""}}>",
            @"<table style=width:100%>"
        ),
        new(
            "styleColorPassed",
            @"<p style=""color: {{""#8ff""}}; background: {{""#000""}}"">",
            @"<p style=""color: #8ff; background: #000"">"
        ),
        new(
            "styleObfuscatedExpressionBlocked",
            @"<p style=""width: {{""  e\\78preS\x00Sio/**/n(alert(1337))""}}"">",
            @"<p style=""width: ZgotmplZ"">"
        ),
        new(
            "styleMozBindingBlocked",
            @"<p style=""{{""-moz-binding(alert(1337))""}}: ..."">",
            @"<p style=""ZgotmplZ: ..."">"
        ),
        new(
            "styleObfuscatedMozBindingBlocked",
            @"<p style=""{{""  -mo\\7a-B\x00I/**/nding(alert(1337))""}}: ..."">",
            @"<p style=""ZgotmplZ: ..."">"
        ),
        new(
            "styleFontNameString",
            @"<p style='font-family: ""{{""Times New Roman""}}""'>",
            @"<p style='font-family: ""Times New Roman""'>"
        ),
        new(
            "styleFontNameString",
            @"<p style='font-family: ""{{""Times New Roman""}}"", ""{{""sans-serif""}}""'>",
            @"<p style='font-family: ""Times New Roman"", ""sans-serif""'>"
        ),
        new(
            "styleFontNameUnquoted",
            @"<p style='font-family: {{""Times New Roman""}}'>",
            @"<p style='font-family: Times New Roman'>"
        ),
        new(
            "styleURLQueryEncoded",
            @"<p style=""background: url(/img?name={{""O'Reilly Animal(1)<2>.png""}})"">",
            @"<p style=""background: url(/img?name=O%27Reilly%20Animal%281%29%3c2%3e.png)"">"
        ),
        new(
            "styleQuotedURLQueryEncoded",
            @"<p style=""background: url('/img?name={{""O'Reilly Animal(1)<2>.png""}}')"">",
            @"<p style=""background: url('/img?name=O%27Reilly%20Animal%281%29%3c2%3e.png')"">"
        ),
        new(
            "styleStrQueryEncoded",
            @"<p style=""background: '/img?name={{""O'Reilly Animal(1)<2>.png""}}'"">",
            @"<p style=""background: '/img?name=O%27Reilly%20Animal%281%29%3c2%3e.png'"">"
        ),
        new(
            "styleURLBadProtocolBlocked",
            @"<a style=""background: url('{{""javascript:alert(1337)""}}')"">",
            @"<a style=""background: url('#ZgotmplZ')"">"
        ),
        new(
            "styleStrBadProtocolBlocked",
            @"<a style=""background: '{{""vbscript:alert(1337)""}}'"">",
            @"<a style=""background: '#ZgotmplZ'"">"
        ),
        new(
            "styleStrEncodedProtocolEncoded",
            @"<a style=""background: '{{""javascript\\3a alert(1337)""}}'"">",
            // The CSS string 'javascript\\3a alert(1337)' does not contain a colon.
            @"<a style=""background: 'javascript\\3a alert\28 1337\29 '"">"
        ),
        new(
            "styleURLGoodProtocolPassed",
            @"<a style=""background: url('{{""http://oreilly.com/O'Reilly Animals(1)<2>;{}.html""}}')"">",
            @"<a style=""background: url('http://oreilly.com/O%27Reilly%20Animals%281%29%3c2%3e;%7b%7d.html')"">"
        ),
        new(
            "styleStrGoodProtocolPassed",
            @"<a style=""background: '{{""http://oreilly.com/O'Reilly Animals(1)<2>;{}.html""}}'"">",
            @"<a style=""background: 'http\3a\2f\2foreilly.com\2fO\27Reilly Animals\28 1\29\3c 2\3e\3b\7b\7d.html'"">"
        ),
        new(
            "styleURLEncodedForHTMLInAttr",
            @"<a style=""background: url('{{""/search?img=foo&size=icon""}}')"">",
            @"<a style=""background: url('/search?img=foo&amp;size=icon')"">"
        ),
        new(
            "styleURLNotEncodedForHTMLInCdata",
            @"<style>body { background: url('{{""/search?img=foo&size=icon""}}') }</style>",
            @"<style>body { background: url('/search?img=foo&size=icon') }</style>"
        ),
        new(
            "styleURLMixedCase",
            @"<p style=""background: URL(#{{.H}})"">",
            @"<p style=""background: URL(#%3cHello%3e)"">"
        ),
        new(
            "stylePropertyPairPassed",
            @"<a style='{{""color: red""}}'>",
            @"<a style='color: red'>"
        ),
        new(
            "styleStrSpecialsEncoded",
            @"<a style=""font-family: '{{""/**/'\"";:// \\""}}', &quot;{{""/**/'\"";:// \\""}}&quot;"">",
            @"<a style=""font-family: '\2f**\2f\27\22\3b\3a\2f\2f  \\', &quot;\2f**\2f\27\22\3b\3a\2f\2f  \\&quot;"">"
        ),
        new(
            "styleURLSpecialsEncoded",
            @"<a style=""border-image: url({{""/**/'\"";:// \\""}}), url(&quot;{{""/**/'\"";:// \\""}}&quot;), url('{{""/**/'\"";:// \\""}}'), 'http://www.example.com/?q={{""/**/'\"";:// \\""}}''"">",
            @"<a style=""border-image: url(/**/%27%22;://%20%5c), url(&quot;/**/%27%22;://%20%5c&quot;), url('/**/%27%22;://%20%5c'), 'http://www.example.com/?q=%2f%2a%2a%2f%27%22%3b%3a%2f%2f%20%5c''"">"
        ),
        new(
            "HTML comment",
            "<b>Hello, <!-- name of world -->{{.C}}</b>",
            "<b>Hello, &lt;Cincinnati&gt;</b>"
        ),
        new(
            "HTML comment not first < in text node.",
            "<<!-- -->!--",
            "&lt;!--"
        ),
        new(
            "HTML normalization 1",
            "a < b",
            "a &lt; b"
        ),
        new(
            "HTML normalization 2",
            "a << b",
            "a &lt;&lt; b"
        ),
        new(
            "HTML normalization 3",
            "a<<!-- --><!-- -->b",
            "a&lt;b"
        ),
        new(
            "HTML doctype not normalized",
            "<!DOCTYPE html>Hello, World!",
            "<!DOCTYPE html>Hello, World!"
        ),
        new(
            "HTML doctype not case-insensitive",
            "<!doCtYPE htMl>Hello, World!",
            "<!doCtYPE htMl>Hello, World!"
        ),
        new(
            "No doctype injection",
            @"<!{{""DOCTYPE""}}",
            "&lt;!DOCTYPE"
        ),
        new(
            "Split HTML comment",
            "<b>Hello, <!-- name of {{if .T}}city -->{{.C}}{{else}}world -->{{.W}}{{end}}</b>",
            "<b>Hello, &lt;Cincinnati&gt;</b>"
        ),
        new(
            "JS line comment",
            "<script>for (;;) { if (c()) break// foo not a label\n"u8 + "foo({{.T}});}</script>"u8,
            "<script>for (;;) { if (c()) break\n"u8 + "foo( true );}</script>"u8
        ),
        new(
            "JS multiline block comment",
            "<script>for (;;) { if (c()) break/* foo not a label\n"u8 + " */foo({{.T}});}</script>"u8,
            // Newline separates break from call. If newline
            // removed, then break will consume label leaving
            // code invalid.
            "<script>for (;;) { if (c()) break\n"u8 + "foo( true );}</script>"u8
        ),
        new(
            "JS single-line block comment",
            "<script>for (;;) {\n"u8 + "if (c()) break/* foo a label */foo;"u8 + "x({{.T}});}</script>"u8,
            // Newline separates break from call. If newline
            // removed, then break will consume label leaving
            // code invalid.
            "<script>for (;;) {\n"u8 + "if (c()) break foo;"u8 + "x( true );}</script>"u8
        ),
        new(
            "JS block comment flush with mathematical division",
            "<script>var a/*b*//c\nd</script>",
            "<script>var a /c\nd</script>"
        ),
        new(
            "JS mixed comments",
            "<script>var a/*b*///c\nd</script>",
            "<script>var a \nd</script>"
        ),
        new(
            "JS HTML-like comments",
            "<script>before <!-- beep\nbetween\nbefore-->boop\n</script>",
            "<script>before \nbetween\nbefore\n</script>"
        ),
        new(
            "JS hashbang comment",
            "<script>#! beep\n</script>",
            "<script>\n</script>"
        ),
        new(
            "Special tags in <script> string literals",
            @"<script>var a = ""asd < 123 <!-- 456 < fgh <script jkl < 789 </script""</script>",
            @"<script>var a = ""asd < 123 \x3C!-- 456 < fgh \x3Cscript jkl < 789 \x3C/script""</script>"
        ),
        new(
            "Special tags in <script> string literals (mixed case)",
            @"<script>var a = ""<!-- <ScripT </ScripT""</script>",
            @"<script>var a = ""\x3C!-- \x3CScripT \x3C/ScripT""</script>"
        ),
        new(
            "Special tags in <script> regex literals (mixed case)",
            @"<script>var a = /<!-- <ScripT </ScripT/</script>",
            @"<script>var a = /\x3C!-- \x3CScripT \x3C/ScripT/</script>"
        ),
        new(
            "CSS comments",
            "<style>p// paragraph\n"u8 + @"{border: 1px/* color */{{""#00f""}}}</style>"u8,
            "<style>p\n"u8 + "{border: 1px #00f}</style>"u8
        ),
        new(
            "JS attr block comment",
            @"<a onclick=""f(&quot;&quot;); /* alert({{.H}}) */"">",
            // Attribute comment tests should pass if the comments
            // are successfully elided.
            @"<a onclick=""f(&quot;&quot;); /* alert() */"">"
        ),
        new(
            "JS attr line comment",
            @"<a onclick=""// alert({{.G}})"">",
            @"<a onclick=""// alert()"">"
        ),
        new(
            "CSS attr block comment",
            @"<a style=""/* color: {{.H}} */"">",
            @"<a style=""/* color:  */"">"
        ),
        new(
            "CSS attr line comment",
            @"<a style=""// color: {{.G}}"">",
            @"<a style=""// color: "">"
        ),
        new(
            "HTML substitution commented out",
            "<p><!-- {{.H}} --></p>",
            "<p></p>"
        ),
        new(
            "Comment ends flush with start",
            "<!--{{.}}--><script>/*{{.}}*///{{.}}\n</script><style>/*{{.}}*///{{.}}\n</style><a onclick='/*{{.}}*///{{.}}' style='/*{{.}}*///{{.}}'>",
            "<script> \n</script><style> \n</style><a onclick='/**///' style='/**///'>"
        ),
        new(
            "typed HTML in text",
            @"{{.W}}",
            @"&iexcl;<b class=""foo"">Hello</b>, <textarea>O'World</textarea>!"
        ),
        new(
            "typed HTML in attribute",
            @"<div title=""{{.W}}"">",
            @"<div title=""&iexcl;Hello, O&#39;World!"">"
        ),
        new(
            "typed HTML in script",
            @"<button onclick=""alert({{.W}})"">",
            @"<button onclick=""alert(&#34;\u0026iexcl;\u003cb class=\&#34;foo\&#34;\u003eHello\u003c/b\u003e, \u003ctextarea\u003eO&#39;World\u003c/textarea\u003e!&#34;)"">"
        ),
        new(
            "typed HTML in RCDATA",
            @"<textarea>{{.W}}</textarea>",
            @"<textarea>&iexcl;&lt;b class=&#34;foo&#34;&gt;Hello&lt;/b&gt;, &lt;textarea&gt;O&#39;World&lt;/textarea&gt;!</textarea>"
        ),
        new(
            "range in textarea",
            "<textarea>{{range .A}}{{.}}{{end}}</textarea>",
            "<textarea>&lt;a&gt;&lt;b&gt;</textarea>"
        ),
        new(
            "No tag injection",
            @"{{""10$""}}<{{""script src,evil.org/pwnd.js""}}...",
            @"10$&lt;script src,evil.org/pwnd.js..."
        ),
        new(
            "No comment injection",
            @"<{{""!--""}}",
            @"&lt;!--"
        ),
        new(
            "No RCDATA end tag injection",
            @"<textarea><{{""/textarea ""}}...</textarea>",
            @"<textarea>&lt;/textarea ...</textarea>"
        ),
        new(
            "optional attrs",
            @"<img class=""{{""iconClass""}}"""u8 + @"{{if .T}} id=""{{""<iconId>""}}""{{end}}"u8 +
                // Double quotes inside if/else.
                @" src="u8 + @"{{if .T}}""?{{""<iconPath>""}}"""u8 + @"{{else}}""images/cleardot.gif""{{end}}"u8 +
                // Missing space before title, but it is not a
                // part of the src attribute.
                @"{{if .T}}title=""{{""<title>""}}""{{end}}"u8 +
                // Quotes outside if/else.
                @" alt="""u8 + @"{{if .T}}{{""<alt>""}}"u8 + @"{{else}}{{if .F}}{{""<title>""}}{{end}}"u8 + @"{{end}}"""u8 + @">"u8,
            @"<img class=""iconClass"" id=""&lt;iconId&gt;"" src=""?%3ciconPath%3e""title=""&lt;title&gt;"" alt=""&lt;alt&gt;"">"
        ),
        new(
            "conditional valueless attr name",
            @"<input{{if .T}} checked{{end}} name=n>",
            @"<input checked name=n>"
        ),
        new(
            "conditional dynamic valueless attr name 1",
            @"<input{{if .T}} {{""checked""}}{{end}} name=n>",
            @"<input checked name=n>"
        ),
        new(
            "conditional dynamic valueless attr name 2",
            @"<input {{if .T}}{{""checked""}} {{end}}name=n>",
            @"<input checked name=n>"
        ),
        new(
            "dynamic attribute name",
            @"<img on{{""load""}}=""alert({{""loaded""}})"">",
            // Treated as JS since quotes are inserted.
            @"<img onload=""alert(&#34;loaded&#34;)"">"
        ),
        new(
            "bad dynamic attribute name 1",
            // Allow checked, selected, disabled, but not JS or
            // CSS attributes.
            @"<input {{""onchange""}}=""{{""doEvil()""}}"">",
            @"<input ZgotmplZ=""doEvil()"">"
        ),
        new(
            "bad dynamic attribute name 2",
            @"<div {{""sTyle""}}=""{{""color: expression(alert(1337))""}}"">",
            @"<div ZgotmplZ=""color: expression(alert(1337))"">"
        ),
        new(
            "bad dynamic attribute name 3",
            // Allow title or alt, but not a URL.
            @"<img {{""src""}}=""{{""javascript:doEvil()""}}"">",
            @"<img ZgotmplZ=""javascript:doEvil()"">"
        ),
        new(
            "bad dynamic attribute name 4",
            // Structure preservation requires values to associate
            // with a consistent attribute.
            @"<input checked {{""""}}=""Whose value am I?"">",
            @"<input checked ZgotmplZ=""Whose value am I?"">"
        ),
        new(
            "dynamic element name",
            @"<h{{3}}><table><t{{""head""}}>...</h{{3}}>",
            @"<h3><table><thead>...</h3>"
        ),
        new(
            "bad dynamic element name",
            // Dynamic element names are typically used to switch
            // between (thead, tfoot, tbody), (ul, ol), (th, td),
            // and other replaceable sets.
            // We do not currently easily support (ul, ol).
            // If we do change to support that, this test should
            // catch failures to filter out special tag names which
            // would violate the structure preservation property --
            // if any special tag name could be substituted, then
            // the content could be raw text/RCDATA for some inputs
            // and regular HTML content for others.
            @"<{{""script""}}>{{""doEvil()""}}</{{""script""}}>",
            @"&lt;script>doEvil()&lt;/script>"
        ),
        new(
            "srcset bad URL in second position",
            @"<img srcset=""{{""/not-an-image#,javascript:alert(1)""}}"">",
            // The second URL is also filtered.
            @"<img srcset=""/not-an-image#,#ZgotmplZ"">"
        ),
        new(
            "srcset buffer growth",
            @"<img srcset={{"",,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,""}}>",
            @"<img srcset=,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,>"
        ),
        new(
            "unquoted empty attribute value (plaintext)",
            "<p name={{.U}}>",
            "<p name=ZgotmplZ>"
        ),
        new(
            "unquoted empty attribute value (url)",
            "<p href={{.U}}>",
            "<p href=ZgotmplZ>"
        ),
        new(
            "quoted empty attribute value",
            "<p name=\"{{.U}}\">",
            "<p name=\"\">"
        ),
        new(
            "JS template lit special characters",
            "<script>var a = `{{.I}}`</script>",
            "<script>var a = `\\u0024\\u007b asd \\u0060\\u0060 \\u007d`</script>"
        ),
        new(
            "JS template lit special characters, nested lit",
            "<script>var a = `${ `{{.I}}` }`</script>",
            "<script>var a = `${ `\\u0024\\u007b asd \\u0060\\u0060 \\u007d` }`</script>"
        ),
        new(
            "JS template lit, nested JS",
            "<script>var a = `${ var a = \"{{\"a \\\" d\"}}\" }`</script>",
            "<script>var a = `${ var a = \"a \\u0022 d\" }`</script>"
        ),
        new(
            "meta content attribute url",
            @"<meta http-equiv=""refresh"" content=""asd; url={{""javascript:alert(1)""}}; asd; url={{""vbscript:alert(1)""}}; asd"">",
            @"<meta http-equiv=""refresh"" content=""asd; url=#ZgotmplZ; asd; url=#ZgotmplZ; asd"">"
        ),
        new(
            "meta content string",
            @"<meta http-equiv=""refresh"" content=""{{""asd: 123""}}"">",
            @"<meta http-equiv=""refresh"" content=""asd: 123"">"
        ),
        new(
            "meta content url with whitespace before equals",
            @"<meta http-equiv=""refresh"" content=""0;url ={{""javascript:alert(1)""}}"">",
            @"<meta http-equiv=""refresh"" content=""0;url =#ZgotmplZ"">"
        ),
        new(
            "meta content url with tab before equals",
            "<meta http-equiv=\"refresh\" content=\"0;url\t={{\"javascript:alert(1)\"}}\">",
            "<meta http-equiv=\"refresh\" content=\"0;url\t=#ZgotmplZ\">"
        ),
        new(
            "meta content url with space after equals",
            @"<meta http-equiv=""refresh"" content=""0;url= {{""javascript:alert(1)""}}"">",
            @"<meta http-equiv=""refresh"" content=""0;url= #ZgotmplZ"">"
        ),
        new(
            "meta content url with whitespace both sides of equals",
            "<meta http-equiv=\"refresh\" content=\"0;url \t= {{\"javascript:alert(1)\"}}\">",
            "<meta http-equiv=\"refresh\" content=\"0;url \t= #ZgotmplZ\">"
        )
    ]);

    foreach (var (_, vᴛ1) in range(tests)) {
        ref var test = ref heap(new TestEscape_tests(), out var Ꮡtest);
        test = vᴛ1;

        var pdataʗ1 = pdata;
        var testʗ1 = test;
        t.Run(test.name, (ж<testing.T> tΔ1) => {
            var tmpl = New(testʗ1.name);
            var (ᴛ1, ᴛ2) = tmpl.Parse(testʗ1.input);
            tmpl = Must(ᴛ1, ᴛ2);
            // Check for bug 6459: Tree field was not set in Parse.
            if (tmpl.Tree != tmpl.text.Tree) {
                tΔ1.Fatalf("%s: tree not set properly", testʗ1.name);
            }
            var b = @new<strings.Builder>();
            {
                var err = tmpl.Execute(new template_test_package.strings_BuilderжWriter(b), Ꮡdata.Value); if (err != nil) {
                    tΔ1.Fatalf("%s: template execution failed: %s", testʗ1.name, err);
                }
            }
            {
                @string w = testʗ1.output;
                @string g = b.String(); if (w != g) {
                    tΔ1.Fatalf("%s: escaped output: want\n\t%q\ngot\n\t%q", testʗ1.name, w, g);
                }
            }
            b.Reset();
            {
                var err = tmpl.Execute(new template_test_package.strings_BuilderжWriter(b), pdataʗ1.OrTypedNil()); if (err != nil) {
                    tΔ1.Fatalf("%s: template execution failed for pointer: %s", testʗ1.name, err);
                }
            }
            {
                @string w = testʗ1.output;
                @string g = b.String(); if (w != g) {
                    tΔ1.Fatalf("%s: escaped output for pointer: want\n\t%q\ngot\n\t%q", testʗ1.name, w, g);
                }
            }
            if (tmpl.Tree != tmpl.text.Tree) {
                tΔ1.Fatalf("%s: tree mismatch", testʗ1.name);
            }
        });
    }
}

internal partial struct TestEscapeMap_type /*dyn*/ {
    internal @string desc, input, output;
}

public static void TestEscapeMap(ж<testing.T> t) {
    var data = new map<@string, @string>{
        ["html"] = @"<h1>Hi!</h1>",
        ["urlquery"] = @"http://www.foo.com/index.html?title=main"
    };
    foreach (var (_, test) in range(new TestEscapeMap_type[]{
        // covering issue 20323
        new(
            "field with predefined escaper name 1",
            @"{{.html | print}}",
            @"&lt;h1&gt;Hi!&lt;/h1&gt;"
        ),
        // covering issue 20323
        new(
            "field with predefined escaper name 2",
            @"{{.urlquery | print}}",
            @"http://www.foo.com/index.html?title=main"
        )
    }.array())) {
        var (ᴛ3, ᴛ4) = New(""u8).Parse(test.input);
        var tmpl = Must(ᴛ3, ᴛ4);
        var b = @new<strings.Builder>();
        var err = tmpl.Execute(new template_test_package.strings_BuilderжWriter(b), data); if (err != nil) {
            t.Errorf("%s: template execution failed: %s", test.desc, err);
            continue;
        }
        @string w = test.output;
        @string g = b.String(); if (w != g) {
            t.Errorf("%s: escaped output: want\n\t%q\ngot\n\t%q", test.desc, w, g);
            continue;
        }
    }
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static ref readonly @string mainˢ => ref mainˢᵇ;
private static readonly @string mainˢᵇ = "main"u8;

internal partial struct TestEscapeSet_dataItem /*dyn*/ {
    public slice<ж<TestEscapeSet_dataItem>> Children;
    public @string X;
}

internal partial struct TestEscapeSet_tests /*dyn*/ {
    internal map<@string, @string> inputs;
    internal @string want;
}

public static void TestEscapeSet(ж<testing.T> t) {
    var data = new TestEscapeSet_dataItem(
        Children: [
            Ꮡ(new TestEscapeSet_dataItem(X: "foo")),
            Ꮡ(new TestEscapeSet_dataItem(X: "<bar>")),
            Ꮡ(new TestEscapeSet_dataItem(
                Children: slice<ж<TestEscapeSet_dataItem>>([
                    Ꮡ(new TestEscapeSet_dataItem(X: "baz"))
                ])))
        ]
    );

    var tests = slice<TestEscapeSet_tests>([
        // The trivial set.
        new(
            new map<@string, @string>{
                ["main"] = @""
            },
            @""
        ),
        // A template called in the start context.
        new(
            new map<@string, @string>{
                ["main"] = @"Hello, {{template ""helper""}}!",
                // Not a valid top level HTML template.
                // "<b" is not a full tag.
                ["helper"] = @"{{""<World>""}}"
            },
            @"Hello, &lt;World&gt;!"
        ),
        // A template called in a context other than the start.
        new(
            new map<@string, @string>{
                ["main"] = @"<a onclick='a = {{template ""helper""}};'>",
                // Not a valid top level HTML template.
                // "<b" is not a full tag.
                ["helper"] = @"{{""<a>""}}<b"
            },
            @"<a onclick='a = &#34;\u003ca\u003e&#34;<b;'>"
        ),
        // A recursive template that ends in its start context.
        new(
            new map<@string, @string>{
                ["main"] = @"{{range .Children}}{{template ""main"" .}}{{else}}{{.X}} {{end}}"
            },
            @"foo &lt;bar&gt; baz "
        ),
        // A recursive helper template that ends in its start context.
        new(
            new map<@string, @string>{
                ["main"] = @"{{template ""helper"" .}}",
                ["helper"] = @"{{if .Children}}<ul>{{range .Children}}<li>{{template ""main"" .}}</li>{{end}}</ul>{{else}}{{.X}}{{end}}"
            },
            @"<ul><li>foo</li><li>&lt;bar&gt;</li><li><ul><li>baz</li></ul></li></ul>"
        ),
        // Co-recursive templates that end in its start context.
        new(
            new map<@string, @string>{
                ["main"] = @"<blockquote>{{range .Children}}{{template ""helper"" .}}{{end}}</blockquote>",
                ["helper"] = @"{{if .Children}}{{template ""main"" .}}{{else}}{{.X}}<br>{{end}}"
            },
            @"<blockquote>foo<br>&lt;bar&gt;<br><blockquote>baz<br></blockquote></blockquote>"
        ),
        // A template that is called in two different contexts.
        new(
            new map<@string, @string>{
                ["main"] = @"<button onclick=""title='{{template ""helper""}}'; ..."">{{template ""helper""}}</button>",
                ["helper"] = @"{{11}} of {{""<100>""}}"
            },
            @"<button onclick=""title='11 of \u003c100\u003e'; ..."">11 of &lt;100&gt;</button>"
        ),
        // A non-recursive template that ends in a different context.
        // helper starts in jsCtxRegexp and ends in jsCtxDivOp.
        new(
            new map<@string, @string>{
                ["main"] = @"<script>var x={{template ""helper""}}/{{""42""}};</script>",
                ["helper"] = "{{126}}"
            },
            @"<script>var x= 126 /""42"";</script>"
        ),
        // A recursive template that ends in a similar context.
        new(
            new map<@string, @string>{
                ["main"] = @"<script>var x=[{{template ""countdown"" 4}}];</script>",
                ["countdown"] = @"{{.}}{{if .}},{{template ""countdown"" . | pred}}{{end}}"
            },
            @"<script>var x=[ 4 , 3 , 2 , 1 , 0 ];</script>"
        )
        // A recursive template that ends in a different context.
        /*
			{
				map[string]string{
					"main":   `<a href="/foo{{template "helper" .}}">`,
					"helper": `{{if .Children}}{{range .Children}}{{template "helper" .}}{{end}}{{else}}?x={{.X}}{{end}}`,
				},
				`<a href="/foo?x=foo?x=%3cbar%3e?x=baz">`,
			},
		*/
    ]);

    // pred is a template function that returns the predecessor of a
    // natural number for testing recursive templates.
    var fns = new FuncMap(new map<@string, any>{["pred"] = ((Funcꓸꓸꓸ<any, (any, error)>)((any, error) (params ꓸꓸꓸany aʗp) => {
        var a = aʗp.slice();
        if (len(a) == 1) {
            var (i, _) = a[0]._<nint>(ᐧ); if (i > 0) {
                return (i - 1, default);
            }
        }
        return (default, fmt.Errorf("undefined pred(%v)"u8, (any)(a)));
    }))
    });

    foreach (var (_, test) in range(tests)) {
        @string source = ""u8;
        foreach (var (name, body) in test.inputs) {
            source += fmt.Sprintf("{{define %q}}%s{{end}} "u8, name, body);
        }
        var (tmpl, err) = New(rootˢ).Funcs(fns).Parse(source);
        if (err != nil) {
            t.Errorf("error parsing %q: %v", source, err);
            continue;
        }
        ref var b = ref heap(new strings.Builder(), out var Ꮡb);

        var errΔ1 = tmpl.ExecuteTemplate(new template_test_package.strings_BuilderжWriter(Ꮡb), mainˢ, data); if (errΔ1 != nil) {
            t.Errorf("%q executing %v", errΔ1.Error(), tmpl.Lookup(mainˢ).OrTypedNil());
            continue;
        }
        @string got = b.String(); if (test.want != got) {
            t.Errorf("want\n\t%q\ngot\n\t%q", test.want, got);
        }
    }
}

internal partial struct TestErrors_tests /*dyn*/ {
    internal @string input;
    internal @string err;
}

public static void TestErrors(ж<testing.T> t) {
    var tests = slice<TestErrors_tests>([
        // Non-error cases.
        new(
            "{{if .Cond}}<a>{{else}}<b>{{end}}",
            ""
        ),
        new(
            "{{if .Cond}}<a>{{end}}",
            ""
        ),
        new(
            "{{if .Cond}}{{else}}<b>{{end}}",
            ""
        ),
        new(
            "{{with .Cond}}<div>{{end}}",
            ""
        ),
        new(
            "{{range .Items}}<a>{{end}}",
            ""
        ),
        new(
            "<a href='/foo?{{range .Items}}&{{.K}}={{.V}}{{end}}'>",
            ""
        ),
        new(
            "{{range .Items}}<a{{if .X}}{{end}}>{{end}}",
            ""
        ),
        new(
            "{{range .Items}}<a{{if .X}}{{end}}>{{continue}}{{end}}",
            ""
        ),
        new(
            "{{range .Items}}<a{{if .X}}{{end}}>{{break}}{{end}}",
            ""
        ),
        new(
            "{{range .Items}}<a{{if .X}}{{end}}>{{if .X}}{{break}}{{end}}{{end}}",
            ""
        ),
        new(
            "<script>var a = `${a+b}`</script>`",
            ""
        ),
        new(
            "<script>var tmpl = `asd`;</script>",
            @""
        ),
        new(
            "<script>var tmpl = `${1}`;</script>",
            @""
        ),
        new(
            "<script>var tmpl = `${return ``}`;</script>",
            @""
        ),
        new(
            "<script>var tmpl = `${return {{.}} }`;</script>",
            @""
        ),
        new(
            "<script>var tmpl = `${ let a = {1:1} {{.}} }`;</script>",
            @""
        ),
        new(
            "<script>var tmpl = `asd ${return \"{\"}`;</script>",
            @""
        ),
        new(
            @"{{if eq """" """"}}<meta>{{end}}",
            @""
        ),
        new(
            @"{{if eq """" """"}}<meta content=""url={{""asd""}}"">{{end}}",
            @""
        ),

        // Error cases.
        new(
            "{{if .Cond}}<a{{end}}",
            "z:1:5: {{if}} branches"
        ),
        new(
            "{{if .Cond}}\n{{else}}\n<a{{end}}",
            "z:1:5: {{if}} branches"
        ),
        new(
            // Missing quote in the else branch.
            @"{{if .Cond}}<a href=""foo"">{{else}}<a href=""bar>{{end}}",
            "z:1:5: {{if}} branches"
        ),
        new(
            // Different kind of attribute: href implies a URL.
            "<a {{if .Cond}}href='{{else}}title='{{end}}{{.X}}'>",
            "z:1:8: {{if}} branches"
        ),
        new(
            "\n{{with .X}}<a{{end}}",
            "z:2:7: {{with}} branches"
        ),
        new(
            "\n{{with .X}}<a>{{else}}<a{{end}}",
            "z:2:7: {{with}} branches"
        ),
        new(
            "{{range .Items}}<a{{end}}",
            @"z:1: on range loop re-entry: ""<"" in attribute name: ""<a"""
        ),
        new(
            "\n{{range .Items}} x='<a{{end}}",
            "z:2:8: on range loop re-entry: {{range}} branches"
        ),
        new(
            "{{range .Items}}<a{{if .X}}{{break}}{{end}}>{{end}}",
            "z:1:29: at range loop break: {{range}} branches end in different contexts"
        ),
        new(
            "{{range .Items}}<a{{if .X}}{{continue}}{{end}}>{{end}}",
            "z:1:29: at range loop continue: {{range}} branches end in different contexts"
        ),
        new(
            "{{range .Items}}{{if .X}}{{break}}{{end}}<a{{if .Y}}{{continue}}{{end}}>{{if .Z}}{{continue}}{{end}}{{end}}",
            "z:1:54: at range loop continue: {{range}} branches end in different contexts"
        ),
        new(
            "<a b=1 c={{.H}}",
            "z: ends in a non-text context: {stateAttr delimSpaceOrTagEnd"
        ),
        new(
            "<script>foo();",
            "z: ends in a non-text context: {stateJS"
        ),
        new(
            @"<a href=""{{if .F}}/foo?a={{else}}/bar/{{end}}{{.H}}"">",
            "z:1:47: {{.H}} appears in an ambiguous context within a URL"
        ),
        new(
            @"<a onclick=""alert('Hello \",
            @"unfinished escape sequence in JS string: ""Hello \\"""
        ),
        new(
            @"<a onclick='alert(""Hello\, World\",
            @"unfinished escape sequence in JS string: ""Hello\\, World\\"""
        ),
        new(
            @"<a onclick='alert(/x+\",
            @"unfinished escape sequence in JS string: ""x+\\"""
        ),
        new(
            @"<a onclick=""/foo[\]/",
            @"unfinished JS regexp charset: ""foo[\\]/"""
        ),
        new(
            // It is ambiguous whether 1.5 should be 1\.5 or 1.5.
            // Either `var x = 1/- 1.5 /i.test(x)`
            // where `i.test(x)` is a method call of reference i,
            // or `/-1\.5/i.test(x)` which is a method call on a
            // case insensitive regular expression.
            @"<script>{{if false}}var x = 1{{end}}/-{{""1.5""}}/i.test(x)</script>",
            @"'/' could start a division or regexp: ""/-"""
        ),
        new(
            @"{{template ""foo""}}",
            "z:1:11: no such template \"foo\""
        ),
        new(
            @"<div{{template ""y""}}>"u8 +
                // Illegal starting in stateTag but not in stateText.
                @"{{define ""y""}} foo<b{{end}}"u8,
            @"""<"" in attribute name: "" foo<b"""
        ),
        new(
            @"<script>reverseList = [{{template ""t""}}]</script>"u8 +
                // Missing " after recursive call.
                @"{{define ""t""}}{{if .Tail}}{{template ""t"" .Tail}}{{end}}{{.Head}}"",{{end}}"u8,
            @": cannot compute output context for template t$htmltemplate_stateJS_elementScript"
        ),
        new(
            @"<input type=button value=onclick=>",
            @"html/template:z: ""="" in unquoted attr: ""onclick="""
        ),
        new(
            @"<input type=button value= onclick=>",
            @"html/template:z: ""="" in unquoted attr: ""onclick="""
        ),
        new(
            @"<input type=button value= 1+1=2>",
            @"html/template:z: ""="" in unquoted attr: ""1+1=2"""
        ),
        new(
            "<a class=`foo>",
            "html/template:z: \"`\" in unquoted attr: \"`foo\""
        ),
        new(
            @"<a style=font:'Arial'>",
            @"html/template:z: ""'"" in unquoted attr: ""font:'Arial'"""
        ),
        new(
            @"<a=foo>",
            @": expected space, attr name, or end of tag, but got ""=foo>"""
        ),
        new(
            @"Hello, {{. | urlquery | print}}!",
            // urlquery is disallowed if it is not the last command in the pipeline.
            @"predefined escaper ""urlquery"" disallowed in template"
        ),
        new(
            @"Hello, {{. | html | print}}!",
            // html is disallowed if it is not the last command in the pipeline.
            @"predefined escaper ""html"" disallowed in template"
        ),
        new(
            @"Hello, {{html . | print}}!",
            // A direct call to html is disallowed if it is not the last command in the pipeline.
            @"predefined escaper ""html"" disallowed in template"
        ),
        new(
            @"<div class={{. | html}}>Hello<div>",
            // html is disallowed in a pipeline that is in an unquoted attribute context,
            // even if it is the last command in the pipeline.
            @"predefined escaper ""html"" disallowed in template"
        ),
        new(
            @"Hello, {{. | urlquery | html}}!",
            // html is allowed since it is the last command in the pipeline, but urlquery is not.
            @"predefined escaper ""urlquery"" disallowed in template"
        ),
        new(
            "<script>var a = `{{if .X}}`{{end}}",
            @"{{if}} branches end in different contexts"
        ),
        new(
            "<script>var a = `{{if .X}}a{{else}}`{{end}}",
            @"{{if}} branches end in different contexts"
        ),
        new(
            "<script>var a = `{{if .X}}a{{else}}b{{end}}`</script>",
            @""
        )
    ]);
    foreach (var (_, test) in range(tests)) {
        var buf = @new<bytes.Buffer>();
        var (tmpl, err) = New("z").Parse(test.input);
        if (err != nil) {
            t.Errorf("input=%q: unexpected parse error %s\n", test.input, err);
            continue;
        }
        err = tmpl.Execute(new template_test_package.bytes_BufferжWriter(buf), default);
        @string got = default;
        if (err != nil) {
            got = err.Error();
        }
        if (test.err == ""u8) {
            if (got != ""u8) {
                t.Errorf("input=%q: unexpected error %q", test.input, got);
            }
            continue;
        }
        if (!strings.Contains(got, test.err)) {
            t.Errorf("input=%q: error\n\t%q\ndoes not contain expected string\n\t%q", test.input, got, test.err);
            continue;
        }
        // Check that we get the same error if we call Execute again.
        var errΔ1 = tmpl.Execute(new template_test_package.bytes_BufferжWriter(buf), default); if (errΔ1 == nil || errΔ1.Error() != got) {
            t.Errorf("input=%q: unexpected error on second call %q", test.input, errΔ1);
        }
    }
}

internal partial struct TestEscapeText_tests /*dyn*/ {
    internal @string input;
    internal global::go.html.template_package.context output;
}

public static void TestEscapeText(ж<testing.T> t) {
    var tests = slice<TestEscapeText_tests>([
        new(
            @"",
            new context()
        ),
        new(
            @"Hello, World!",
            new context()
        ),
        new(
            // An orphaned "<" is OK.
            @"I <3 Ponies!",
            new context()
        ),
        new(
            @"<a",
            new context(state: stateTag)
        ),
        new(
            @"<a ",
            new context(state: stateTag)
        ),
        new(
            @"<a>",
            new context(state: stateText)
        ),
        new(
            @"<a href",
            new context(state: stateAttrName, attr: attrURL)
        ),
        new(
            @"<a on",
            new context(state: stateAttrName, attr: attrScript)
        ),
        new(
            @"<a href ",
            new context(state: stateAfterName, attr: attrURL)
        ),
        new(
            @"<a style  =  ",
            new context(state: stateBeforeValue, attr: attrStyle)
        ),
        new(
            @"<a href=",
            new context(state: stateBeforeValue, attr: attrURL)
        ),
        new(
            @"<a href=x",
            new context(state: stateURL, delim: delimSpaceOrTagEnd, urlPart: urlPartPreQuery, attr: attrURL)
        ),
        new(
            @"<a href=x ",
            new context(state: stateTag)
        ),
        new(
            @"<a href=>",
            new context(state: stateText)
        ),
        new(
            @"<a href=x>",
            new context(state: stateText)
        ),
        new(
            @"<a href ='",
            new context(state: stateURL, delim: delimSingleQuote, attr: attrURL)
        ),
        new(
            @"<a href=''",
            new context(state: stateTag)
        ),
        new(
            @"<a href= """,
            new context(state: stateURL, delim: delimDoubleQuote, attr: attrURL)
        ),
        new(
            @"<a href=""""",
            new context(state: stateTag)
        ),
        new(
            @"<a title=""",
            new context(state: stateAttr, delim: delimDoubleQuote)
        ),
        new(
            @"<a HREF='http:",
            new context(state: stateURL, delim: delimSingleQuote, urlPart: urlPartPreQuery, attr: attrURL)
        ),
        new(
            @"<a Href='/",
            new context(state: stateURL, delim: delimSingleQuote, urlPart: urlPartPreQuery, attr: attrURL)
        ),
        new(
            @"<a href='""",
            new context(state: stateURL, delim: delimSingleQuote, urlPart: urlPartPreQuery, attr: attrURL)
        ),
        new(
            @"<a href=""'",
            new context(state: stateURL, delim: delimDoubleQuote, urlPart: urlPartPreQuery, attr: attrURL)
        ),
        new(
            @"<a href='&apos;",
            new context(state: stateURL, delim: delimSingleQuote, urlPart: urlPartPreQuery, attr: attrURL)
        ),
        new(
            @"<a href=""&quot;",
            new context(state: stateURL, delim: delimDoubleQuote, urlPart: urlPartPreQuery, attr: attrURL)
        ),
        new(
            @"<a href=""&#34;",
            new context(state: stateURL, delim: delimDoubleQuote, urlPart: urlPartPreQuery, attr: attrURL)
        ),
        new(
            @"<a href=&quot;",
            new context(state: stateURL, delim: delimSpaceOrTagEnd, urlPart: urlPartPreQuery, attr: attrURL)
        ),
        new(
            @"<img alt=""1"">",
            new context(state: stateText)
        ),
        new(
            @"<img alt=""1>""",
            new context(state: stateTag)
        ),
        new(
            @"<img alt=""1>"">",
            new context(state: stateText)
        ),
        new(
            @"<input checked type=""checkbox""",
            new context(state: stateTag)
        ),
        new(
            @"<a onclick=""",
            new context(state: stateJS, delim: delimDoubleQuote, attr: attrScript)
        ),
        new(
            @"<a onclick=""//foo",
            new context(state: stateJSLineCmt, delim: delimDoubleQuote, attr: attrScript)
        ),
        new(
            "<a onclick='//\n",
            new context(state: stateJS, delim: delimSingleQuote, attr: attrScript)
        ),
        new(
            "<a onclick='//\r\n",
            new context(state: stateJS, delim: delimSingleQuote, attr: attrScript)
        ),
        new(
            "<a onclick='//\u2028",
            new context(state: stateJS, delim: delimSingleQuote, attr: attrScript)
        ),
        new(
            @"<a onclick=""/*",
            new context(state: stateJSBlockCmt, delim: delimDoubleQuote, attr: attrScript)
        ),
        new(
            @"<a onclick=""/*/",
            new context(state: stateJSBlockCmt, delim: delimDoubleQuote, attr: attrScript)
        ),
        new(
            @"<a onclick=""/**/",
            new context(state: stateJS, delim: delimDoubleQuote, attr: attrScript)
        ),
        new(
            @"<a onkeypress=""&quot;",
            new context(state: stateJSDqStr, delim: delimDoubleQuote, attr: attrScript)
        ),
        new(
            @"<a onclick='&quot;foo&quot;",
            new context(state: stateJS, delim: delimSingleQuote, jsCtx: jsCtxDivOp, attr: attrScript)
        ),
        new(
            @"<a onclick=&#39;foo&#39;",
            new context(state: stateJS, delim: delimSpaceOrTagEnd, jsCtx: jsCtxDivOp, attr: attrScript)
        ),
        new(
            @"<a onclick=&#39;foo",
            new context(state: stateJSSqStr, delim: delimSpaceOrTagEnd, attr: attrScript)
        ),
        new(
            @"<a onclick=""&quot;foo'",
            new context(state: stateJSDqStr, delim: delimDoubleQuote, attr: attrScript)
        ),
        new(
            @"<a onclick=""'foo&quot;",
            new context(state: stateJSSqStr, delim: delimDoubleQuote, attr: attrScript)
        ),
        new(
            "<a onclick=\"`foo",
            new context(state: stateJSTmplLit, delim: delimDoubleQuote, attr: attrScript)
        ),
        new(
            @"<A ONCLICK=""'",
            new context(state: stateJSSqStr, delim: delimDoubleQuote, attr: attrScript)
        ),
        new(
            @"<a onclick=""/",
            new context(state: stateJSRegexp, delim: delimDoubleQuote, attr: attrScript)
        ),
        new(
            @"<a onclick=""'foo'",
            new context(state: stateJS, delim: delimDoubleQuote, jsCtx: jsCtxDivOp, attr: attrScript)
        ),
        new(
            @"<a onclick=""'foo\'",
            new context(state: stateJSSqStr, delim: delimDoubleQuote, attr: attrScript)
        ),
        new(
            @"<a onclick=""'foo\'",
            new context(state: stateJSSqStr, delim: delimDoubleQuote, attr: attrScript)
        ),
        new(
            @"<a onclick=""/foo/",
            new context(state: stateJS, delim: delimDoubleQuote, jsCtx: jsCtxDivOp, attr: attrScript)
        ),
        new(
            @"<script>/foo/ /=",
            new context(state: stateJS, element: elementScript)
        ),
        new(
            @"<a onclick=""1 /foo",
            new context(state: stateJS, delim: delimDoubleQuote, jsCtx: jsCtxDivOp, attr: attrScript)
        ),
        new(
            @"<a onclick=""1 /*c*/ /foo",
            new context(state: stateJS, delim: delimDoubleQuote, jsCtx: jsCtxDivOp, attr: attrScript)
        ),
        new(
            @"<a onclick=""/foo[/]",
            new context(state: stateJSRegexp, delim: delimDoubleQuote, attr: attrScript)
        ),
        new(
            @"<a onclick=""/foo\/",
            new context(state: stateJSRegexp, delim: delimDoubleQuote, attr: attrScript)
        ),
        new(
            @"<a onclick=""/foo/",
            new context(state: stateJS, delim: delimDoubleQuote, jsCtx: jsCtxDivOp, attr: attrScript)
        ),
        new(
            @"<input checked style=""",
            new context(state: stateCSS, delim: delimDoubleQuote, attr: attrStyle)
        ),
        new(
            @"<a style=""//",
            new context(state: stateCSSLineCmt, delim: delimDoubleQuote, attr: attrStyle)
        ),
        new(
            @"<a style=""//</script>",
            new context(state: stateCSSLineCmt, delim: delimDoubleQuote, attr: attrStyle)
        ),
        new(
            "<a style='//\n",
            new context(state: stateCSS, delim: delimSingleQuote, attr: attrStyle)
        ),
        new(
            "<a style='//\r",
            new context(state: stateCSS, delim: delimSingleQuote, attr: attrStyle)
        ),
        new(
            @"<a style=""/*",
            new context(state: stateCSSBlockCmt, delim: delimDoubleQuote, attr: attrStyle)
        ),
        new(
            @"<a style=""/*/",
            new context(state: stateCSSBlockCmt, delim: delimDoubleQuote, attr: attrStyle)
        ),
        new(
            @"<a style=""/**/",
            new context(state: stateCSS, delim: delimDoubleQuote, attr: attrStyle)
        ),
        new(
            @"<a style=""background: '",
            new context(state: stateCSSSqStr, delim: delimDoubleQuote, attr: attrStyle)
        ),
        new(
            @"<a style=""background: &quot;",
            new context(state: stateCSSDqStr, delim: delimDoubleQuote, attr: attrStyle)
        ),
        new(
            @"<a style=""background: '/foo?img=",
            new context(state: stateCSSSqStr, delim: delimDoubleQuote, urlPart: urlPartQueryOrFrag, attr: attrStyle)
        ),
        new(
            @"<a style=""background: '/",
            new context(state: stateCSSSqStr, delim: delimDoubleQuote, urlPart: urlPartPreQuery, attr: attrStyle)
        ),
        new(
            @"<a style=""background: url(&#x22;/",
            new context(state: stateCSSDqURL, delim: delimDoubleQuote, urlPart: urlPartPreQuery, attr: attrStyle)
        ),
        new(
            @"<a style=""background: url('/",
            new context(state: stateCSSSqURL, delim: delimDoubleQuote, urlPart: urlPartPreQuery, attr: attrStyle)
        ),
        new(
            @"<a style=""background: url('/)",
            new context(state: stateCSSSqURL, delim: delimDoubleQuote, urlPart: urlPartPreQuery, attr: attrStyle)
        ),
        new(
            @"<a style=""background: url('/ ",
            new context(state: stateCSSSqURL, delim: delimDoubleQuote, urlPart: urlPartPreQuery, attr: attrStyle)
        ),
        new(
            @"<a style=""background: url(/",
            new context(state: stateCSSURL, delim: delimDoubleQuote, urlPart: urlPartPreQuery, attr: attrStyle)
        ),
        new(
            @"<a style=""background: url( ",
            new context(state: stateCSSURL, delim: delimDoubleQuote, attr: attrStyle)
        ),
        new(
            @"<a style=""background: url( /image?name=",
            new context(state: stateCSSURL, delim: delimDoubleQuote, urlPart: urlPartQueryOrFrag, attr: attrStyle)
        ),
        new(
            @"<a style=""background: url(x)",
            new context(state: stateCSS, delim: delimDoubleQuote, attr: attrStyle)
        ),
        new(
            @"<a style=""background: url('x'",
            new context(state: stateCSS, delim: delimDoubleQuote, attr: attrStyle)
        ),
        new(
            @"<a style=""background: url( x ",
            new context(state: stateCSS, delim: delimDoubleQuote, attr: attrStyle)
        ),
        new(
            @"<!-- foo",
            new context(state: stateHTMLCmt)
        ),
        new(
            @"<!-->",
            new context(state: stateHTMLCmt)
        ),
        new(
            @"<!--->",
            new context(state: stateHTMLCmt)
        ),
        new(
            @"<!-- foo -->",
            new context(state: stateText)
        ),
        new(
            @"<script",
            new context(state: stateTag, element: elementScript)
        ),
        new(
            @"<script ",
            new context(state: stateTag, element: elementScript)
        ),
        new(
            @"<script src=""foo.js"" ",
            new context(state: stateTag, element: elementScript)
        ),
        new(
            @"<script src='foo.js' ",
            new context(state: stateTag, element: elementScript)
        ),
        new(
            @"<script type=text/javascript ",
            new context(state: stateTag, element: elementScript)
        ),
        new(
            @"<script>",
            new context(state: stateJS, jsCtx: jsCtxRegexp, element: elementScript)
        ),
        new(
            @"<script>foo",
            new context(state: stateJS, jsCtx: jsCtxDivOp, element: elementScript)
        ),
        new(
            @"<script>foo</script>",
            new context(state: stateText)
        ),
        new(
            @"<script>foo</script><!--",
            new context(state: stateHTMLCmt)
        ),
        new(
            @"<script>document.write(""<p>foo</p>"");",
            new context(state: stateJS, element: elementScript)
        ),
        new(
            @"<script>document.write(""<p>foo<\/script>"");",
            new context(state: stateJS, element: elementScript)
        ),
        new(
            // <script and </script tags are escaped, so </script> should not
            // cause us to exit the JS state.
            @"<script>document.write(""<script>alert(1)</script>"");",
            new context(state: stateJS, element: elementScript)
        ),
        new(
            @"<script>document.write(""<script>",
            new context(state: stateJSDqStr, element: elementScript)
        ),
        new(
            @"<script>document.write(""<script>alert(1)</script>",
            new context(state: stateJSDqStr, element: elementScript)
        ),
        new(
            @"<script>document.write(""<script>alert(1)<!--",
            new context(state: stateJSDqStr, element: elementScript)
        ),
        new(
            @"<script>document.write(""<script>alert(1)</Script>"");",
            new context(state: stateJS, element: elementScript)
        ),
        new(
            @"<script>document.write(""<!--"");",
            new context(state: stateJS, element: elementScript)
        ),
        new(
            @"<script>let a = /</script",
            new context(state: stateJSRegexp, element: elementScript)
        ),
        new(
            @"<script>let a = /</script/",
            new context(state: stateJS, element: elementScript, jsCtx: jsCtxDivOp)
        ),
        new(
            @"<script type=""text/template"">",
            new context(state: stateText)
        ),
        // covering issue 19968
        new(
            @"<script type=""TEXT/JAVASCRIPT"">",
            new context(state: stateJS, element: elementScript)
        ),
        // covering issue 19965
        new(
            @"<script TYPE=""text/template"">",
            new context(state: stateText)
        ),
        new(
            @"<script type=""notjs"">",
            new context(state: stateText)
        ),
        new(
            @"<Script>",
            new context(state: stateJS, element: elementScript)
        ),
        new(
            @"<SCRIPT>foo",
            new context(state: stateJS, jsCtx: jsCtxDivOp, element: elementScript)
        ),
        new(
            @"<textarea>value",
            new context(state: stateRCDATA, element: elementTextarea)
        ),
        new(
            @"<textarea>value</TEXTAREA>",
            new context(state: stateText)
        ),
        new(
            @"<textarea name=html><b",
            new context(state: stateRCDATA, element: elementTextarea)
        ),
        new(
            @"<title>value",
            new context(state: stateRCDATA, element: elementTitle)
        ),
        new(
            @"<style>value",
            new context(state: stateCSS, element: elementStyle)
        ),
        new(
            @"<a xlink:href",
            new context(state: stateAttrName, attr: attrURL)
        ),
        new(
            @"<a xmlns",
            new context(state: stateAttrName, attr: attrURL)
        ),
        new(
            @"<a xmlns:foo",
            new context(state: stateAttrName, attr: attrURL)
        ),
        new(
            @"<a xmlnsxyz",
            new context(state: stateAttrName)
        ),
        new(
            @"<a data-url",
            new context(state: stateAttrName, attr: attrURL)
        ),
        new(
            @"<a data-iconUri",
            new context(state: stateAttrName, attr: attrURL)
        ),
        new(
            @"<a data-urlItem",
            new context(state: stateAttrName, attr: attrURL)
        ),
        new(
            @"<a g:",
            new context(state: stateAttrName)
        ),
        new(
            @"<a g:url",
            new context(state: stateAttrName, attr: attrURL)
        ),
        new(
            @"<a g:iconUri",
            new context(state: stateAttrName, attr: attrURL)
        ),
        new(
            @"<a g:urlItem",
            new context(state: stateAttrName, attr: attrURL)
        ),
        new(
            @"<a g:value",
            new context(state: stateAttrName)
        ),
        new(
            @"<a svg:style='",
            new context(state: stateCSS, delim: delimSingleQuote, attr: attrStyle)
        ),
        new(
            @"<svg:font-face",
            new context(state: stateTag)
        ),
        new(
            @"<svg:a svg:onclick=""",
            new context(state: stateJS, delim: delimDoubleQuote, attr: attrScript)
        ),
        new(
            @"<svg:a svg:onclick=""x()"">",
            new context()
        ),
        new(
            "<script>var a = `",
            new context(state: stateJSTmplLit, element: elementScript)
        ),
        new(
            "<script>var a = `${",
            new context(state: stateJS, element: elementScript, jsBraceDepth: [0])
        ),
        new(
            "<script>var a = `${}",
            new context(state: stateJSTmplLit, element: elementScript)
        ),
        new(
            "<script>var a = `${`",
            new context(state: stateJSTmplLit, element: elementScript, jsBraceDepth: [0])
        ),
        new(
            "<script>var a = `${var a = \"",
            new context(state: stateJSDqStr, element: elementScript, jsBraceDepth: [0])
        ),
        new(
            "<script>var a = `${var a = \"`",
            new context(state: stateJSDqStr, element: elementScript, jsBraceDepth: [0])
        ),
        new(
            "<script>var a = `${var a = \"}",
            new context(state: stateJSDqStr, element: elementScript, jsBraceDepth: [0])
        ),
        new(
            "<script>var a = `${``",
            new context(state: stateJS, element: elementScript, jsBraceDepth: [0])
        ),
        new(
            "<script>var a = `${`}",
            new context(state: stateJSTmplLit, element: elementScript, jsBraceDepth: [0])
        ),
        new(
            "<script>`${ {} } asd`</script><script>`${ {} }",
            new context(state: stateJSTmplLit, element: elementScript)
        ),
        new(
            "<script>var foo = `${ (_ => { return \"x\" })() + \"${",
            new context(state: stateJSDqStr, element: elementScript, jsBraceDepth: [0])
        ),
        new(
            "<script>var a = `${ {</script><script>var b = `${ x }",
            new context(state: stateJSTmplLit, element: elementScript, jsCtx: jsCtxDivOp)
        ),
        new(
            "<script>var foo = `x` + \"${",
            new context(state: stateJSDqStr, element: elementScript)
        ),
        new(
            "<script>function f() { var a = `${}`; }",
            new context(state: stateJS, element: elementScript)
        ),
        new(
            "<script>{`${}`}",
            new context(state: stateJS, element: elementScript)
        ),
        new(
            "<script>`${ function f() { return `${1}` }() }`",
            new context(state: stateJS, element: elementScript, jsCtx: jsCtxDivOp)
        ),
        new(
            "<script>function f() {`${ function f() { `${1}` } }`}",
            new context(state: stateJS, element: elementScript, jsCtx: jsCtxRegexp)
        ),
        new(
            "<script>`${ { `` }",
            new context(state: stateJS, element: elementScript, jsBraceDepth: [0])
        ),
        new(
            "<script>`${ { }`",
            new context(state: stateJSTmplLit, element: elementScript, jsBraceDepth: [0])
        ),
        new(
            "<script>var foo = `${ foo({ a: { c: `${",
            new context(state: stateJS, element: elementScript, jsBraceDepth: [2, 0])
        ),
        new(
            "<script>var foo = `${ foo({ a: { c: `${ {{.}} }` }, b: ",
            new context(state: stateJS, element: elementScript, jsBraceDepth: [1])
        ),
        new(
            "<script>`${ `}",
            new context(state: stateJSTmplLit, element: elementScript, jsBraceDepth: [0])
        ),
        new(
            "<script>`${1}${",
            new context(state: stateJS, element: elementScript, jsCtx: jsCtxRegexp, jsBraceDepth: [0])
        ),
        new(
            "<script>`${`${1}${",
            new context(state: stateJS, element: elementScript, jsCtx: jsCtxRegexp, jsBraceDepth: [0, 0])
        )
    ]);

    foreach (var (_, test) in range(tests)) {
        var (b, e) = (slice<byte>(test.input), makeEscaper(nil));
        var c = e.escapeText(new context(), Ꮡ(new parse.TextNode(NodeType: parse.NodeText, Text: b)));
        if (!test.output.eq(c)) {
            t.Errorf("input %q: want context\n\t%v\ngot\n\t%v", test.input, test.output, c);
            continue;
        }
        if (test.input != ((sstring)b)) {
            t.Errorf("input %q: text node was modified: want %q got %q", test.input, test.input, b);
            continue;
        }
    }
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static ref readonly @string testˢ => ref testˢᵇ;
private static readonly @string testˢᵇ = "test"u8;

internal partial struct TestEnsurePipelineContains_tests /*dyn*/ {
    internal @string input, output;
    internal slice<@string> ids;
}

public static void TestEnsurePipelineContains(ж<testing.T> t) {
    var tests = slice<TestEnsurePipelineContains_tests>([
        new(
            "{{.X}}",
            ".X",
            []
        ),
        new(
            "{{.X | html}}",
            ".X | html",
            []
        ),
        new(
            "{{.X}}",
            ".X | html",
            ["html"]
        ),
        new(
            "{{html .X}}",
            "_eval_args_ .X | html | urlquery",
            ["html", "urlquery"]
        ),
        new(
            "{{html .X .Y .Z}}",
            "_eval_args_ .X .Y .Z | html | urlquery",
            ["html", "urlquery"]
        ),
        new(
            "{{.X | print}}",
            ".X | print | urlquery",
            ["urlquery"]
        ),
        new(
            "{{.X | print | urlquery}}",
            ".X | print | urlquery",
            ["urlquery"]
        ),
        new(
            "{{.X | urlquery}}",
            ".X | html | urlquery",
            ["html", "urlquery"]
        ),
        new(
            "{{.X | print 2 | .f 3}}",
            ".X | print 2 | .f 3 | urlquery | html",
            ["urlquery", "html"]
        ),
        new(
            // covering issue 10801
            "{{.X | println.x }}",
            ".X | println.x | urlquery | html",
            ["urlquery", "html"]
        ),
        new(
            // covering issue 10801
            "{{.X | (print 12 | println).x }}",
            ".X | (print 12 | println).x | urlquery | html",
            ["urlquery", "html"]
        ),
        // The following test cases ensure that the merging of internal escapers
        // with the predefined "html" and "urlquery" escapers is correct.
        new(
            "{{.X | urlquery}}",
            ".X | _html_template_urlfilter | urlquery",
            ["_html_template_urlfilter", "_html_template_urlnormalizer"]
        ),
        new(
            "{{.X | urlquery}}",
            ".X | urlquery | _html_template_urlfilter | _html_template_cssescaper",
            ["_html_template_urlfilter", "_html_template_cssescaper"]
        ),
        new(
            "{{.X | urlquery}}",
            ".X | urlquery",
            ["_html_template_urlnormalizer"]
        ),
        new(
            "{{.X | urlquery}}",
            ".X | urlquery",
            ["_html_template_urlescaper"]
        ),
        new(
            "{{.X | html}}",
            ".X | html",
            ["_html_template_htmlescaper"]
        ),
        new(
            "{{.X | html}}",
            ".X | html",
            ["_html_template_rcdataescaper"]
        )
    ]);
    foreach (var (i, test) in range(tests)) {
        var (ᴛ5, ᴛ6) = text.template_package.New(testˢ).Parse(test.input);
        var tmpl = text.template_package.Must(ᴛ5, ᴛ6);
        var (action, ok) = (tmpl.Tree.Root.Nodes[0]._<ж<parse.ActionNode>>(ᐧ));
        if (!ok) {
            t.Errorf("First node is not an action: %s", test.input);
            continue;
        }
        var pipe = action.Pipe;
        var originalIDs = new slice<@string>(len(test.ids));
        copy(originalIDs, test.ids);
        ensurePipelineContains(pipe, test.ids);
        @string got = pipe.String();
        if (got != test.output) {
            t.Errorf("#%d: %s, %v: want\n\t%s\ngot\n\t%s", i, test.input, originalIDs, test.output, got);
        }
    }
}

public static void TestEscapeMalformedPipelines(ж<testing.T> t) {
    var tests = slice<@string>([
        "{{ 0 | $ }}",
        "{{ 0 | $ | urlquery }}",
        "{{ 0 | (nil) }}",
        "{{ 0 | (nil) | html }}"
    ]);
    foreach (var (_, test) in range(tests)) {
        ref var b = ref heap(new bytes.Buffer(), out var Ꮡb);
        var (tmpl, err) = New(testˢ).Parse(test);
        if (err != nil) {
            t.Errorf("failed to parse set: %q", err);
        }
        err = tmpl.Execute(new template_test_package.bytes_BufferжWriter(Ꮡb), default);
        if (err == nil) {
            t.Errorf("Expected error for %q", test);
        }
    }
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static ref readonly @string dangerousˢ => ref dangerousˢᵇ;
private static readonly @string dangerousˢᵇ = "dangerous"u8;

public static void TestEscapeErrorsNotIgnorable(ж<testing.T> t) {
    ref var b = ref heap(new bytes.Buffer(), out var Ꮡb);
    var (tmpl, _) = New(dangerousˢ).Parse("<a");
    var err = tmpl.Execute(new template_test_package.bytes_BufferжWriter(Ꮡb), default);
    if (err == nil) {
        t.Errorf("Expected error");
    } else if (b.Len() != 0) {
        t.Errorf("Emitted output despite escaping failure");
    }
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static ref readonly @string defineTAEndˢ => ref defineTAEndˢᵇ;
private static readonly @string defineTAEndˢᵇ = @"{{define ""t""}}<a{{end}}"u8;

public static void TestEscapeSetErrorsNotIgnorable(ж<testing.T> t) {
    ref var b = ref heap(new bytes.Buffer(), out var Ꮡb);
    var (tmpl, err) = New(rootˢ).Parse(defineTAEndˢ);
    if (err != nil) {
        t.Errorf("failed to parse set: %q", err);
    }
    err = tmpl.ExecuteTemplate(new template_test_package.bytes_BufferжWriter(Ꮡb), "t", default);
    if (err == nil) {
        t.Errorf("Expected error");
    } else if (b.Len() != 0) {
        t.Errorf("Emitted output despite escaping failure");
    }
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static ref readonly @string helloWorldOReillyX21ˢ => ref helloWorldOReillyX21ˢᵇ;
private static readonly @string helloWorldOReillyX21ˢᵇ = @"Hello, World & O'Reilly\x21"u8;
internal static ref readonly @string greetingH69Addresseeˢ3 => ref greetingH69Addresseeˢ3ᵇ;
private static readonly @string greetingH69Addresseeˢ3ᵇ = @"greeting=H%69&addressee=(World)"u8;

public static void TestRedundantFuncs(ж<testing.T> t) {
    var inputs = slice<any>([
        (@string)("\x00\x01\x02\x03\x04\x05\x06\x07\x08\t\n\x0b\x0c\r\x0e\x0f" + "\x10\x11\x12\x13\x14\x15\x16\x17\x18\x19\x1a\x1b\x1c\x1d\x1e\x1f" + @" !""#$%&'()*+,-./" + @"0123456789:;<=>?" + @"@ABCDEFGHIJKLMNO" + @"PQRSTUVWXYZ[\]^_" + "`abcdefghijklmno" + "pqrstuvwxyz{|}~\x7f" + "\u00A0\u0100\u2028\u2029\ufeff\ufdec\ufffd\uffff\U0001D11E" + "&amp;%22\\"),
        (global::go.html.template_package.CSS)(@string)aHrefExampleComFooˢ,
        (global::go.html.template_package.HTML)(@string)helloBWorldBAmpTcˢ,
        (global::go.html.template_package.HTMLAttr)(@string)dirLtrˢ,
        (global::go.html.template_package.JS)(@string)cAlertHelloWorldˢ,
        (global::go.html.template_package.JSStr)(@string)helloWorldOReillyX21ˢ,
        (global::go.html.template_package.URL)(@string)greetingH69Addresseeˢ3
    ]);

    foreach (var (n0, m) in redundantFuncs) {
        var f0 = funcMap[n0]._<Funcꓸꓸꓸ<any, @string>>();
        foreach (var (n1, _) in m) {
            var f1 = funcMap[n1]._<Funcꓸꓸꓸ<any, @string>>();
            foreach (var (_, input) in range(inputs)) {
                @string want = f0(input);
                @string got = f1(want); if (want != got) {
                    t.Errorf("%s %s with %T %q: want\n\t%q,\ngot\n\t%q", n0, n1, input, input, want, got);
                }
            }
        }
    }
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static ref readonly @string helloˢ => ref helloˢᵇ;
private static readonly @string helloˢᵇ = "hello"u8;

public static void TestIndirectPrint(ж<testing.T> t) {
    ref var a = ref heap<nint>(3, out var Ꮡa);
    var ap = Ꮡa;
    ref var b = ref heap<@string>(helloˢ, out var Ꮡb);
    ref var bp = ref heap<ж<@string>>(Ꮡb, out var Ꮡbp);
    var bpp = Ꮡbp;
    var (ᴛ7, ᴛ8) = New("t").Parse(@"{{.}}");
    var tmpl = Must(ᴛ7, ᴛ8);
    ref var buf = ref heap(new strings.Builder(), out var Ꮡbuf);
    var err = tmpl.Execute(new template_test_package.strings_BuilderжWriter(Ꮡbuf), ap.OrTypedNil());
    if (err != nil) {
        t.Errorf("Unexpected error: %s", err);
    } else if (buf.String() != "3"u8) {
        t.Errorf(@"Expected ""3""; got %q", buf.String());
    }
    buf.Reset();
    err = tmpl.Execute(new template_test_package.strings_BuilderжWriter(Ꮡbuf), bpp.OrTypedNil());
    if (err != nil) {
        t.Errorf("Unexpected error: %s", err);
    } else if (buf.String() != "hello"u8) {
        t.Errorf(@"Expected ""hello""; got %q", buf.String());
    }
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static ref readonly @string pageˢ => ref pageˢᵇ;
private static readonly @string pageˢᵇ = "page"u8;
internal static readonly object nothingˢ = (@string)"nothing"u8;
internal static readonly object expectedErrorˢ = (@string)"expected error"u8;

// This is a test for issue 3272.
public static void TestEmptyTemplateHTML(ж<testing.T> t) {
    var (ᴛ9, ᴛ10) = New(pageˢ).ParseFiles(os.DevNull);
    var page = Must(ᴛ9, ᴛ10);
    var err = page.ExecuteTemplate(new os.FileжWriter(os.Stdout), pageˢ, nothingˢ); if (err == nil) {
        t.Fatal(expectedErrorˢ);
    }
}

public partial struct Issue7379 /*num:nint*/;

public static @string SomeMethod(this Issue7379 _, nint x) {
    return fmt.Sprintf("<%d>"u8, x);
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static ref readonly @string html0SomeMethodHtmlˢ => ref html0SomeMethodHtmlˢᵇ;
private static readonly @string html0SomeMethodHtmlˢᵇ = "<html>{{0 | .SomeMethod}}</html>\n"u8;

// Hoisted Go string constant (single allocation; Go keeps it in RODATA)
internal static ref readonly @string expectᶜ => ref expectᶜᵇ;
private static readonly @string expectᶜᵇ = "<html>&lt;0&gt;</html>\n"u8;

// This is a test for issue 7379: type assertion error caused panic, and then
// the code to handle the panic breaks escaping. It's hard to see the second
// problem once the first is fixed, but its fix is trivial so we let that go. See
// the discussion for issue 7379.
public static void TestPipeToMethodIsEscaped(ж<testing.T> t) {
    var (ᴛ11, ᴛ12) = New("x").Parse(html0SomeMethodHtmlˢ);
    var tmpl = Must(ᴛ11, ᴛ12);
    var tmplʗ1 = tmpl;
    @string tryExec() {
        GoFrame ᒐ = default;
        try {
            defer(() => {
                var panicValue = recover();
                if (panicValue != nil) {
                    t.Errorf("panicked: %v\n", panicValue);
                }
            }, ref ᒐ);
            ref var b = ref heap(new strings.Builder(), out var Ꮡb);
            tmplʗ1.Execute(new template_test_package.strings_BuilderжWriter(Ꮡb), (Issue7379)0);
            return b.String();
        }
        catch (Exception ᒐex) when (GoFrame.IsPanic(ᒐex, out PanicException? ᒐp)) { GoFrame.Capture(ᒐp); return default; }
        finally { ᒐ.Run(); }
    }
    for (nint i = 0; i < 3; i++) {
        @string str = tryExec();
        @string expect = expectᶜ;
        if (str != expect) {
            t.Errorf("expected %q got %q", expect, str);
        }
    }
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static ref readonly @string undefinedˢ => ref undefinedˢᵇ;
private static readonly @string undefinedˢᵇ = "undefined"u8;
internal static ref readonly @string incompleteˢ => ref incompleteˢᵇ;
private static readonly @string incompleteˢᵇ = "incomplete"u8;

// Unlike text/template, html/template crashed if given an incomplete
// template, that is, a template that had been named but not given any content.
// This is issue #10204.
public static void TestErrorOnUndefined(ж<testing.T> t) {
    var tmpl = New(undefinedˢ);

    var err = tmpl.Execute(default, default);
    if (err == nil) {
        t.Error(expectedErrorˢ);
    } else if (!strings.Contains(err.Error(), incompleteˢ)) {
        t.Errorf("expected error about incomplete template; got %s", err);
    }
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static ref readonly @string defineMainBodyTemplateˢ => ref defineMainBodyTemplateˢᵇ;
private static readonly @string defineMainBodyTemplateˢᵇ = @"{{define ""main""}}<body>{{template ""hello""}}</body>{{end}}"u8;
internal static ref readonly @string defineHelloHelloLadiesˢ => ref defineHelloHelloLadiesˢᵇ;
private static readonly @string defineHelloHelloLadiesˢᵇ = @"{{define ""hello""}}Hello, {{""Ladies & Gentlemen!""}}{{end}}"u8;
internal static ref readonly @string helloLadiesAmpGentlemenˢ => ref helloLadiesAmpGentlemenˢᵇ;
private static readonly @string helloLadiesAmpGentlemenˢᵇ = "Hello, Ladies &amp; Gentlemen!"u8;
internal static ref readonly @string bodyHelloLadiesAmpˢ => ref bodyHelloLadiesAmpˢᵇ;
private static readonly @string bodyHelloLadiesAmpˢᵇ = "<body>Hello, Ladies &amp; Gentlemen!</body>"u8;

// This covers issue #20842.
public static void TestIdempotentExecute(ж<testing.T> t) {
    var (ᴛ13, ᴛ14) = New(""u8).Parse(defineMainBodyTemplateˢ);
    var tmpl = Must(ᴛ13, ᴛ14);
    var (ᴛ15, ᴛ16) = tmpl.Parse(defineHelloHelloLadiesˢ);
    Must(ᴛ15, ᴛ16);
    var got = @new<strings.Builder>();
    error err = default;
    // Ensure that "hello" produces the same output when executed twice.
    @string want = helloLadiesAmpGentlemenˢ;
    for (nint i = 0; i < 2; i++) {
        err = tmpl.ExecuteTemplate(new template_test_package.strings_BuilderжWriter(got), helloˢ, default);
        if (err != nil) {
            t.Errorf("unexpected error: %s", err);
        }
        if (got.String() != want) {
            t.Errorf("after executing template \"hello\", got:\n\t%q\nwant:\n\t%q\n", got.String(), want);
        }
        got.Reset();
    }
    // Ensure that the implicit re-execution of "hello" during the execution of
    // "main" does not cause the output of "hello" to change.
    err = tmpl.ExecuteTemplate(new template_test_package.strings_BuilderжWriter(got), mainˢ, default);
    if (err != nil) {
        t.Errorf("unexpected error: %s", err);
    }
    // If the HTML escaper is added again to the action {{"Ladies & Gentlemen!"}},
    // we would expected to see the ampersand overescaped to "&amp;amp;".
    want = bodyHelloLadiesAmpˢ;
    if (got.String() != want) {
        t.Errorf("after executing template \"main\", got:\n\t%q\nwant:\n\t%q\n", got.String(), want);
    }
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static ref readonly @string aOnclickAlertAˢ => ref aOnclickAlertAˢᵇ;
private static readonly @string aOnclickAlertAˢᵇ = @"<a onclick=""alert('{{.}}')"">{{.}}</a>"u8;
internal static readonly object fooBarBazˢ = (@string)"foo & 'bar' & baz"u8;

public static void BenchmarkEscapedExecute(ж<testing.B> Ꮡb) {
    ref var b = ref Ꮡb.DerefOrNull();

    var (ᴛ17, ᴛ18) = New("t").Parse(aOnclickAlertAˢ);
    var tmpl = Must(ᴛ17, ᴛ18);
    ref var buf = ref heap(new bytes.Buffer(), out var Ꮡbuf);
    b.ResetTimer();
    for (nint i = 0; i < b.N; i++) {
        tmpl.Execute(new template_test_package.bytes_BufferжWriter(Ꮡbuf), fooBarBazˢ);
        buf.Reset();
    }
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static ref readonly @string fooˢ2 => ref fooˢ2ᵇ;
private static readonly @string fooˢ2ᵇ = "foo"u8;
internal static ref readonly @string aHrefLink1Aˢ => ref aHrefLink1Aˢᵇ;
private static readonly @string aHrefLink1Aˢᵇ = @"<a href=""{{.}}"">link1</a>"u8;
internal static ref readonly @string barˢ => ref barˢᵇ;
private static readonly @string barˢᵇ = @"bar"u8;
internal static readonly object javascriptAlert1ˢ = (@string)"javascript:alert(1)"u8;
internal static readonly object expectedErrorExecutingT1ˢ = (@string)"expected error executing t1"u8;

// Hoisted Go string constant (single allocation; Go keeps it in RODATA)
internal static ref readonly @string wantErrorᶜ => ref wantErrorᶜᵇ;
private static readonly @string wantErrorᶜᵇ = @"template: ""foo"" is an incomplete or empty template"u8;

// Hoisted Go string constant (single allocation; Go keeps it in RODATA)
internal static ref readonly @string wantᶜ => ref wantᶜᵇ;
private static readonly @string wantᶜᵇ = "bar"u8;

// Covers issue 22780.
public static void TestOrphanedTemplate(ж<testing.T> t) {
    var (ᴛ19, ᴛ20) = New(fooˢ2).Parse(aHrefLink1Aˢ);
    var t1 = Must(ᴛ19, ᴛ20);
    var (ᴛ21, ᴛ22) = t1.New(fooˢ2).Parse(barˢ);
    var t2 = Must(ᴛ21, ᴛ22);

    ref var b = ref heap(new strings.Builder(), out var Ꮡb);
    @string wantError = wantErrorᶜ;
    {
        var err = t1.Execute(new template_test_package.strings_BuilderжWriter(Ꮡb), javascriptAlert1ˢ); if (err == nil) {
            t.Fatal(expectedErrorExecutingT1ˢ);
        } else {
            @string gotError = err.Error(); if (gotError != wantError) {
                t.Fatalf("got t1 execution error:\n\t%s\nwant:\n\t%s", gotError, wantError);
            }
        }
    }
    b.Reset();
    {
        var err = t2.Execute(new template_test_package.strings_BuilderжWriter(Ꮡb), default); if (err != nil) {
            t.Fatalf("error executing t2: %s", err);
        }
    }
    @string want = wantᶜ;
    @string got = b.String(); if (got != want) {
        t.Fatalf("t2 rendered %q, want %q", got, want);
    }
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static ref readonly @string barˢ2 => ref barˢ2ᵇ;
private static readonly @string barˢ2ᵇ = "bar"u8;

// Hoisted Go string constant (single allocation; Go keeps it in RODATA)
internal static ref readonly @string tmplTextᶜ => ref tmplTextᶜᵇ;
private static readonly @string tmplTextᶜᵇ = @"{{.}}"u8;

// Hoisted Go string constant (single allocation; Go keeps it in RODATA)
internal static ref readonly @string dataᶜ => ref dataᶜᵇ;
private static readonly @string dataᶜᵇ = @"<baz>"u8;

// Hoisted Go string constant (single allocation; Go keeps it in RODATA)
internal static ref readonly @string wantᶜ1 => ref wantᶜ1ᵇ;
private static readonly @string wantᶜ1ᵇ = @"&lt;baz&gt;"u8;

// Covers issue 21844.
public static void TestAliasedParseTreeDoesNotOverescape(ж<testing.T> t) {
    @string tmplText = tmplTextᶜ;
    @string data = dataᶜ;
    @string want = wantᶜ1;
    // Templates "foo" and "bar" both alias the same underlying parse tree.
    var (ᴛ23, ᴛ24) = New(fooˢ2).Parse(tmplText);
    var tpl = Must(ᴛ23, ᴛ24);
    {
        var (_, err) = tpl.AddParseTree(barˢ2, tpl.Tree); if (err != nil) {
            t.Fatalf("AddParseTree error: %v", err);
        }
    }
    ref var b1 = ref heap(new strings.Builder(), out var Ꮡb1);
    ref var b2 = ref heap(new strings.Builder(), out var Ꮡb2);
    {
        var err = tpl.ExecuteTemplate(new template_test_package.strings_BuilderжWriter(Ꮡb1), fooˢ2, data); if (err != nil) {
            t.Fatalf(@"ExecuteTemplate failed for ""foo"": %v", err);
        }
    }
    {
        var err = tpl.ExecuteTemplate(new template_test_package.strings_BuilderжWriter(Ꮡb2), barˢ2, data); if (err != nil) {
            t.Fatalf(@"ExecuteTemplate failed for ""foo"": %v", err);
        }
    }
    @string got1 = b1.String();
    @string got2 = b2.String();
    if (got1 != want) {
        t.Fatalf(@"Template ""foo"" rendered %q, want %q", got1, want);
    }
    if (got1 != got2) {
        t.Fatalf(@"Template ""foo"" and ""bar"" rendered %q and %q respectively, expected equal values", got1, got2);
    }
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static ref readonly @string godebugˢ => ref godebugˢᵇ;
private static readonly @string godebugˢᵇ = "GODEBUG"u8;
internal static ref readonly @string metaHttpEquivRefreshˢ => ref metaHttpEquivRefreshˢᵇ;
private static readonly @string metaHttpEquivRefreshˢᵇ = @"<meta http-equiv=""refresh"" content=""asd; url={{""javascript:alert(1)""}}; asd; url={{""vbscript:alert(1)""}}; asd"">"u8;
internal static ref readonly @string metaHttpEquivRefreshˢ2 => ref metaHttpEquivRefreshˢ2ᵇ;
private static readonly @string metaHttpEquivRefreshˢ2ᵇ = @"<meta http-equiv=""refresh"" content=""asd; url=javascript:alert(1); asd; url=vbscript:alert(1); asd"">"u8;

public static void TestMetaContentEscapeGODEBUG(ж<testing.T> t) {
    GoFrame ᒐ = default;
    try {
        @string savedGODEBUG = os.Getenv(godebugˢ);
        os.Setenv(godebugˢ, savedGODEBUG + ",htmlmetacontenturlescape=0"u8);
        defer(() => {
            os.Setenv(godebugˢ, savedGODEBUG);
        }, ref ᒐ);

        var (ᴛ25, ᴛ26) = New(""u8).Parse(metaHttpEquivRefreshˢ);
        var tmpl = Must(ᴛ25, ᴛ26);
        ref var b = ref heap(new strings.Builder(), out var Ꮡb);
        var err = tmpl.Execute(new template_test_package.strings_BuilderжWriter(Ꮡb), default); if (err != nil) {
            t.Fatalf("unexpected error: %s", err);
        }
        @string want = metaHttpEquivRefreshˢ2;
        @string got = b.String(); if (got != want) {
            t.Fatalf("got %q, want %q", got, want);
        }
    }
    catch (Exception ᒐex) when (GoFrame.IsPanic(ᒐex, out PanicException? ᒐp)) { GoFrame.Capture(ᒐp); }
    finally { ᒐ.Run(); }
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static ref readonly @string scriptConstS1GScriptˢ => ref scriptConstS1GScriptˢᵇ;
private static readonly @string scriptConstS1GScriptˢᵇ = "<script>const s = `${1}${/{{.}}/g}`</script>"u8;
internal static readonly object xExecAlert1ˢ = (@string)@"x/.exec(alert(1))}"u8;
internal static ref readonly @string scriptConstS1XExecAlert1ˢ => ref scriptConstS1XExecAlert1ˢᵇ;
private static readonly @string scriptConstS1XExecAlert1ˢᵇ = "<script>const s = `${1}${/x\\/\\.exec\\(alert\\(1\\)\\)\\}/g}`</script>"u8;

public static void TestIssue81821(ж<testing.T> t) {
    var (ᴛ27, ᴛ28) = New(testˢ).Parse(scriptConstS1GScriptˢ);
    var tmpl = Must(ᴛ27, ᴛ28);
    ref var buf = ref heap(new bytes.Buffer(), out var Ꮡbuf);
    var err = tmpl.Execute(new template_test_package.bytes_BufferжWriter(Ꮡbuf), xExecAlert1ˢ); if (err != nil) {
        t.Fatalf("Execute: %v", err);
    }
    @string want = scriptConstS1XExecAlert1ˢ;
    @string got = Ꮡbuf.String(); if (got != want) {
        t.Errorf("got:  %s\nwant: %s", got, want);
    }
}

internal partial struct TestCVE202656858_tests /*dyn*/ {
    internal @string name;
    internal @string tmpl;
    internal @string input;
    internal @string want;
}

public static void TestCVE202656858(ж<testing.T> t) {
    var tests = slice<TestCVE202656858_tests>([
        new(
            name: "regexp after open brace in if block",
            tmpl: @"<script>if(true){/{{.}}/g.test(""x"")}</script>",
            input: "a.b",
            want: @"<script>if(true){/a\.b/g.test(""x"")}</script>"
        ),
        new(
            name: "regexp after close brace",
            tmpl: @"<script>if(true){x=1}/{{.}}/g.test(""x"")</script>",
            input: "a.b",
            want: @"<script>if(true){x=1}/a\.b/g.test(""x"")</script>"
        ),
        new(
            name: "regexp pathological attacker input",
            tmpl: @"<script>if(true){/{{.}}/g.test(""x"")}</script>",
            input: @"./;alert(1);var q=/.",
            want: @"<script>if(true){/\.\/;alert\(1\);var q=\/\./g.test(""x"")}</script>"
        ),
        new(
            name: "regexp after open brace in template literal",
            tmpl: "<script>`${ (function(){/{{.}}/g.test(x)}) }`</script>",
            input: "a.b",
            want: "<script>`${ (function(){/a\\.b/g.test(x)}) }`</script>"
        ),
        new(
            name: "regexp after close brace in template literal",
            tmpl: "<script>`${ (function(){}/{{.}}/g.test(x)) }`</script>",
            input: "a.b",
            want: "<script>`${ (function(){}/a\\.b/g.test(x)) }`</script>"
        )
    ]);
    foreach (var (_, vᴛ1) in range(tests)) {
        ref var tt = ref heap(new TestCVE202656858_tests(), out var Ꮡtt);
        tt = vᴛ1;

        var ttʗ1 = tt;
        t.Run(tt.name, (ж<testing.T> tΔ1) => {
            var (ᴛ29, ᴛ30) = New(testˢ).Parse(ttʗ1.tmpl);
            var tmpl = Must(ᴛ29, ᴛ30);
            ref var buf = ref heap(new bytes.Buffer(), out var Ꮡbuf);
            var err = tmpl.Execute(new template_test_package.bytes_BufferжWriter(Ꮡbuf), ttʗ1.input); if (err != nil) {
                tΔ1.Fatalf("Execute: %v", err);
            }
            @string got = Ꮡbuf.String(); if (got != ttʗ1.want) {
                tΔ1.Errorf("got:  %s\nwant: %s", got, ttʗ1.want);
            }
        });
    }
}

internal partial struct TestIssue81823_tests /*dyn*/ {
    internal @string name;
    internal @string tmpl;
    internal @string input;
    internal @string want;
}

public static void TestIssue81823(ж<testing.T> t) {
    var tests = slice<TestIssue81823_tests>([
        new(
            name: "yield",
            tmpl: @"<script>function* rules(){yield /{{.}}/g}</script>",
            input: @"/;alert(1)//",
            want: @"<script>function* rules(){yield /\/;alert\(1\)\/\//g}</script>"
        ),
        new(
            name: "property yield",
            tmpl: "<script>const pct = bond.yield / 100;\nconst user = {{.}}; // state\n</script>",
            input: @"1;pwned=1;0",
            want: "<script>const pct = bond.yield / 100;\nconst user = \"1;pwned=1;0\"; \n</script>"
        ),
        new(
            name: "private yield",
            tmpl: "<script>class Bond { #yield = 4; pct() { return this.#yield / 100; } }\nconst user = {{.}}; // state\n</script>",
            input: @"1;pwned=1;0",
            want: "<script>class Bond { #yield = 4; pct() { return this.#yield / 100; } }\nconst user = \"1;pwned=1;0\"; \n</script>"
        ),
        new(
            name: "property in",
            tmpl: "<script>const kb = traffic.in / 1024;\nconst user = {{.}}; // state\n</script>",
            input: @"1;pwned=1;0",
            want: "<script>const kb = traffic.in / 1024;\nconst user = \"1;pwned=1;0\"; \n</script>"
        )
    ]);
    foreach (var (_, vᴛ1) in range(tests)) {
        ref var tt = ref heap(new TestIssue81823_tests(), out var Ꮡtt);
        tt = vᴛ1;

        var ttʗ1 = tt;
        t.Run(tt.name, (ж<testing.T> tΔ1) => {
            var (ᴛ31, ᴛ32) = New(testˢ).Parse(ttʗ1.tmpl);
            var tmpl = Must(ᴛ31, ᴛ32);
            ref var buf = ref heap(new strings.Builder(), out var Ꮡbuf);
            var err = tmpl.Execute(new template_test_package.strings_BuilderжWriter(Ꮡbuf), ttʗ1.input); if (err != nil) {
                tΔ1.Fatalf("Execute: %v", err);
            }
            @string got = buf.String(); if (got != ttʗ1.want) {
                tΔ1.Errorf("got:  %s\nwant: %s", got, ttʗ1.want);
            }
        });
    }
}

} // end template_internal_test_package
