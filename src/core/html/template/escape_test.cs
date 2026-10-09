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
    return (slice<byte>("{ foo: 'not quite valid JSON' }"u8), default!);
}

internal partial struct goodMarshaler {
}

internal static (slice<byte>, error) MarshalJSON(this ref goodMarshaler x) {
    return (slice<byte>(@"{ ""<foo>"": ""O'Reilly"" }"u8), default!);
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static readonly @string iexclBClassFooHelloBˢ = @"&iexcl;<b class=""foo"">Hello</b>, <textarea>O'World</textarea>!"u8;

internal partial struct TestEscape_data /*dyn*/ {
    public bool F, T;
    public @string C, G, H, I;
    public slice<@string> A, E;
    public json.Marshaler B, M;
    public nint N;
    public any U;  // untyped nil
    public ж<nint> Z; // typed nil
    public global::go.html.template_package.HTML W;
}

internal partial struct TestEscape_tests /*dyn*/ {
    internal @string name;
    internal @string input;
    internal @string output;
}

public static void TestEscape(ж<testing.T> Ꮡt) {
    ref var t = ref Ꮡt.DerefOrNull();

    ref var data = ref heap<TestEscape_data>(out var Ꮡdata);
    data = new TestEscape_data(
        F: false,
        T: true,
        C: "<Cincinnati>"u8,
        G: "<Goodbye>"u8,
        H: "<Hello>"u8,
        A: new @string[]{"<a>"u8, "<b>"u8}.slice(),
        E: new @string[]{}.slice(),
        N: 42,
        B: new template_internal_test_package.badMarshalerжMarshaler(Ꮡ(new badMarshaler(nil))),
        M: new template_internal_test_package.goodMarshalerжMarshaler(Ꮡ(new goodMarshaler(nil))),
        U: default!,
        Z: nil,
        W: ((global::go.html.template_package.HTML)(@string)iexclBClassFooHelloBˢ),
        I: "${ asd `` }"u8
    );
    var pdata = Ꮡdata;
    var tests = new TestEscape_tests[]{
        new(
            "if"u8,
            "{{if .T}}Hello{{end}}, {{.C}}!"u8,
            "Hello, &lt;Cincinnati&gt;!"u8
        ),
        new(
            "else"u8,
            "{{if .F}}{{.H}}{{else}}{{.G}}{{end}}!"u8,
            "&lt;Goodbye&gt;!"u8
        ),
        new(
            "overescaping1"u8,
            "Hello, {{.C | html}}!"u8,
            "Hello, &lt;Cincinnati&gt;!"u8
        ),
        new(
            "overescaping2"u8,
            "Hello, {{html .C}}!"u8,
            "Hello, &lt;Cincinnati&gt;!"u8
        ),
        new(
            "overescaping3"u8,
            "{{with .C}}{{$msg := .}}Hello, {{$msg}}!{{end}}"u8,
            "Hello, &lt;Cincinnati&gt;!"u8
        ),
        new(
            "assignment"u8,
            "{{if $x := .H}}{{$x}}{{end}}"u8,
            "&lt;Hello&gt;"u8
        ),
        new(
            "withBody"u8,
            "{{with .H}}{{.}}{{end}}"u8,
            "&lt;Hello&gt;"u8
        ),
        new(
            "withElse"u8,
            "{{with .E}}{{.}}{{else}}{{.H}}{{end}}"u8,
            "&lt;Hello&gt;"u8
        ),
        new(
            "rangeBody"u8,
            "{{range .A}}{{.}}{{end}}"u8,
            "&lt;a&gt;&lt;b&gt;"u8
        ),
        new(
            "rangeElse"u8,
            "{{range .E}}{{.}}{{else}}{{.H}}{{end}}"u8,
            "&lt;Hello&gt;"u8
        ),
        new(
            "nonStringValue"u8,
            "{{.T}}"u8,
            "true"u8
        ),
        new(
            "untypedNilValue"u8,
            "{{.U}}"u8,
            ""u8
        ),
        new(
            "typedNilValue"u8,
            "{{.Z}}"u8,
            "&lt;nil&gt;"u8
        ),
        new(
            "constant"u8,
            @"<a href=""/search?q={{""'a<b'""}}"">"u8,
            @"<a href=""/search?q=%27a%3cb%27"">"u8
        ),
        new(
            "multipleAttrs"u8,
            "<a b=1 c={{.H}}>"u8,
            "<a b=1 c=&lt;Hello&gt;>"u8
        ),
        new(
            "urlStartRel"u8,
            @"<a href='{{""/foo/bar?a=b&c=d""}}'>"u8,
            @"<a href='/foo/bar?a=b&amp;c=d'>"u8
        ),
        new(
            "urlStartAbsOk"u8,
            @"<a href='{{""http://example.com/foo/bar?a=b&c=d""}}'>"u8,
            @"<a href='http://example.com/foo/bar?a=b&amp;c=d'>"u8
        ),
        new(
            "protocolRelativeURLStart"u8,
            @"<a href='{{""//example.com:8000/foo/bar?a=b&c=d""}}'>"u8,
            @"<a href='//example.com:8000/foo/bar?a=b&amp;c=d'>"u8
        ),
        new(
            "pathRelativeURLStart"u8,
            @"<a href=""{{""/javascript:80/foo/bar""}}"">"u8,
            @"<a href=""/javascript:80/foo/bar"">"u8
        ),
        new(
            "dangerousURLStart"u8,
            @"<a href='{{""javascript:alert(%22pwned%22)""}}'>"u8,
            @"<a href='#ZgotmplZ'>"u8
        ),
        new(
            "dangerousURLStart2"u8,
            @"<a href='  {{""javascript:alert(%22pwned%22)""}}'>"u8,
            @"<a href='  #ZgotmplZ'>"u8
        ),
        new(
            "nonHierURL"u8,
            @"<a href={{""mailto:Muhammed \""The Greatest\"" Ali <m.ali@example.com>""}}>"u8,
            @"<a href=mailto:Muhammed%20%22The%20Greatest%22%20Ali%20%3cm.ali@example.com%3e>"u8
        ),
        new(
            "urlPath"u8,
            @"<a href='http://{{""javascript:80""}}/foo'>"u8,
            @"<a href='http://javascript:80/foo'>"u8
        ),
        new(
            "urlQuery"u8,
            @"<a href='/search?q={{.H}}'>"u8,
            @"<a href='/search?q=%3cHello%3e'>"u8
        ),
        new(
            "urlFragment"u8,
            @"<a href='/faq#{{.H}}'>"u8,
            @"<a href='/faq#%3cHello%3e'>"u8
        ),
        new(
            "urlBranch"u8,
            @"<a href=""{{if .F}}/foo?a=b{{else}}/bar{{end}}"">"u8,
            @"<a href=""/bar"">"u8
        ),
        new(
            "urlBranchConflictMoot"u8,
            @"<a href=""{{if .T}}/foo?a={{else}}/bar#{{end}}{{.C}}"">"u8,
            @"<a href=""/foo?a=%3cCincinnati%3e"">"u8
        ),
        new(
            "jsStrValue"u8,
            "<button onclick='alert({{.H}})'>"u8,
            @"<button onclick='alert(&#34;\u003cHello\u003e&#34;)'>"u8
        ),
        new(
            "jsNumericValue"u8,
            "<button onclick='alert({{.N}})'>"u8,
            @"<button onclick='alert( 42 )'>"u8
        ),
        new(
            "jsBoolValue"u8,
            "<button onclick='alert({{.T}})'>"u8,
            @"<button onclick='alert( true )'>"u8
        ),
        new(
            "jsNilValueTyped"u8,
            "<button onclick='alert(typeof{{.Z}})'>"u8,
            @"<button onclick='alert(typeof null )'>"u8
        ),
        new(
            "jsNilValueUntyped"u8,
            "<button onclick='alert(typeof{{.U}})'>"u8,
            @"<button onclick='alert(typeof null )'>"u8
        ),
        new(
            "jsObjValue"u8,
            "<button onclick='alert({{.A}})'>"u8,
            @"<button onclick='alert([&#34;\u003ca\u003e&#34;,&#34;\u003cb\u003e&#34;])'>"u8
        ),
        new(
            "jsObjValueScript"u8,
            "<script>alert({{.A}})</script>"u8,
            @"<script>alert([""\u003ca\u003e"",""\u003cb\u003e""])</script>"u8
        ),
        new(
            "jsObjValueNotOverEscaped"u8,
            "<button onclick='alert({{.A | html}})'>"u8,
            @"<button onclick='alert([&#34;\u003ca\u003e&#34;,&#34;\u003cb\u003e&#34;])'>"u8
        ),
        new(
            "jsStr"u8,
            "<button onclick='alert(&quot;{{.H}}&quot;)'>"u8,
            @"<button onclick='alert(&quot;\u003cHello\u003e&quot;)'>"u8
        ),
        new(
            "badMarshaler"u8,
            @"<button onclick='alert(1/{{.B}}in numbers)'>"u8,
            @"<button onclick='alert(1/ /* json: error calling MarshalJSON for type *template.badMarshaler: invalid character &#39;f&#39; looking for beginning of object key string */null in numbers)'>"u8
        ),
        new(
            "jsMarshaler"u8,
            @"<button onclick='alert({{.M}})'>"u8,
            @"<button onclick='alert({&#34;\u003cfoo\u003e&#34;:&#34;O&#39;Reilly&#34;})'>"u8
        ),
        new(
            "jsStrNotUnderEscaped"u8,
            "<button onclick='alert({{.C | urlquery}})'>"u8, // URL escaped, then quoted for JS.

            @"<button onclick='alert(&#34;%3CCincinnati%3E&#34;)'>"u8
        ),
        new(
            "jsRe"u8,
            @"<button onclick='alert(/{{""foo+bar""}}/.test(""""))'>"u8,
            @"<button onclick='alert(/foo\u002bbar/.test(""""))'>"u8
        ),
        new(
            "jsReBlank"u8,
            @"<script>alert(/{{""""}}/.test(""""));</script>"u8,
            @"<script>alert(/(?:)/.test(""""));</script>"u8
        ),
        new(
            "jsReAmbigOk"u8,
            @"<script>{{if true}}var x = 1{{end}}</script>"u8, // The {if} ends in an ambiguous jsCtx but there is
 // no slash following so we shouldn't care.

            @"<script>var x = 1</script>"u8
        ),
        new(
            "styleBidiKeywordPassed"u8,
            @"<p style=""dir: {{""ltr""}}"">"u8,
            @"<p style=""dir: ltr"">"u8
        ),
        new(
            "styleBidiPropNamePassed"u8,
            @"<p style=""border-{{""left""}}: 0; border-{{""right""}}: 1in"">"u8,
            @"<p style=""border-left: 0; border-right: 1in"">"u8
        ),
        new(
            "styleExpressionBlocked"u8,
            @"<p style=""width: {{""expression(alert(1337))""}}"">"u8,
            @"<p style=""width: ZgotmplZ"">"u8
        ),
        new(
            "styleTagSelectorPassed"u8,
            @"<style>{{""p""}} { color: pink }</style>"u8,
            @"<style>p { color: pink }</style>"u8
        ),
        new(
            "styleIDPassed"u8,
            @"<style>p{{""#my-ID""}} { font: Arial }</style>"u8,
            @"<style>p#my-ID { font: Arial }</style>"u8
        ),
        new(
            "styleClassPassed"u8,
            @"<style>p{{"".my_class""}} { font: Arial }</style>"u8,
            @"<style>p.my_class { font: Arial }</style>"u8
        ),
        new(
            "styleQuantityPassed"u8,
            @"<a style=""left: {{""2em""}}; top: {{0}}"">"u8,
            @"<a style=""left: 2em; top: 0"">"u8
        ),
        new(
            "stylePctPassed"u8,
            @"<table style=width:{{""100%""}}>"u8,
            @"<table style=width:100%>"u8
        ),
        new(
            "styleColorPassed"u8,
            @"<p style=""color: {{""#8ff""}}; background: {{""#000""}}"">"u8,
            @"<p style=""color: #8ff; background: #000"">"u8
        ),
        new(
            "styleObfuscatedExpressionBlocked"u8,
            @"<p style=""width: {{""  e\\78preS\x00Sio/**/n(alert(1337))""}}"">"u8,
            @"<p style=""width: ZgotmplZ"">"u8
        ),
        new(
            "styleMozBindingBlocked"u8,
            @"<p style=""{{""-moz-binding(alert(1337))""}}: ..."">"u8,
            @"<p style=""ZgotmplZ: ..."">"u8
        ),
        new(
            "styleObfuscatedMozBindingBlocked"u8,
            @"<p style=""{{""  -mo\\7a-B\x00I/**/nding(alert(1337))""}}: ..."">"u8,
            @"<p style=""ZgotmplZ: ..."">"u8
        ),
        new(
            "styleFontNameString"u8,
            @"<p style='font-family: ""{{""Times New Roman""}}""'>"u8,
            @"<p style='font-family: ""Times New Roman""'>"u8
        ),
        new(
            "styleFontNameString"u8,
            @"<p style='font-family: ""{{""Times New Roman""}}"", ""{{""sans-serif""}}""'>"u8,
            @"<p style='font-family: ""Times New Roman"", ""sans-serif""'>"u8
        ),
        new(
            "styleFontNameUnquoted"u8,
            @"<p style='font-family: {{""Times New Roman""}}'>"u8,
            @"<p style='font-family: Times New Roman'>"u8
        ),
        new(
            "styleURLQueryEncoded"u8,
            @"<p style=""background: url(/img?name={{""O'Reilly Animal(1)<2>.png""}})"">"u8,
            @"<p style=""background: url(/img?name=O%27Reilly%20Animal%281%29%3c2%3e.png)"">"u8
        ),
        new(
            "styleQuotedURLQueryEncoded"u8,
            @"<p style=""background: url('/img?name={{""O'Reilly Animal(1)<2>.png""}}')"">"u8,
            @"<p style=""background: url('/img?name=O%27Reilly%20Animal%281%29%3c2%3e.png')"">"u8
        ),
        new(
            "styleStrQueryEncoded"u8,
            @"<p style=""background: '/img?name={{""O'Reilly Animal(1)<2>.png""}}'"">"u8,
            @"<p style=""background: '/img?name=O%27Reilly%20Animal%281%29%3c2%3e.png'"">"u8
        ),
        new(
            "styleURLBadProtocolBlocked"u8,
            @"<a style=""background: url('{{""javascript:alert(1337)""}}')"">"u8,
            @"<a style=""background: url('#ZgotmplZ')"">"u8
        ),
        new(
            "styleStrBadProtocolBlocked"u8,
            @"<a style=""background: '{{""vbscript:alert(1337)""}}'"">"u8,
            @"<a style=""background: '#ZgotmplZ'"">"u8
        ),
        new(
            "styleStrEncodedProtocolEncoded"u8,
            @"<a style=""background: '{{""javascript\\3a alert(1337)""}}'"">"u8, // The CSS string 'javascript\\3a alert(1337)' does not contain a colon.

            @"<a style=""background: 'javascript\\3a alert\28 1337\29 '"">"u8
        ),
        new(
            "styleURLGoodProtocolPassed"u8,
            @"<a style=""background: url('{{""http://oreilly.com/O'Reilly Animals(1)<2>;{}.html""}}')"">"u8,
            @"<a style=""background: url('http://oreilly.com/O%27Reilly%20Animals%281%29%3c2%3e;%7b%7d.html')"">"u8
        ),
        new(
            "styleStrGoodProtocolPassed"u8,
            @"<a style=""background: '{{""http://oreilly.com/O'Reilly Animals(1)<2>;{}.html""}}'"">"u8,
            @"<a style=""background: 'http\3a\2f\2foreilly.com\2fO\27Reilly Animals\28 1\29\3c 2\3e\3b\7b\7d.html'"">"u8
        ),
        new(
            "styleURLEncodedForHTMLInAttr"u8,
            @"<a style=""background: url('{{""/search?img=foo&size=icon""}}')"">"u8,
            @"<a style=""background: url('/search?img=foo&amp;size=icon')"">"u8
        ),
        new(
            "styleURLNotEncodedForHTMLInCdata"u8,
            @"<style>body { background: url('{{""/search?img=foo&size=icon""}}') }</style>"u8,
            @"<style>body { background: url('/search?img=foo&size=icon') }</style>"u8
        ),
        new(
            "styleURLMixedCase"u8,
            @"<p style=""background: URL(#{{.H}})"">"u8,
            @"<p style=""background: URL(#%3cHello%3e)"">"u8
        ),
        new(
            "stylePropertyPairPassed"u8,
            @"<a style='{{""color: red""}}'>"u8,
            @"<a style='color: red'>"u8
        ),
        new(
            "styleStrSpecialsEncoded"u8,
            @"<a style=""font-family: '{{""/**/'\"";:// \\""}}', &quot;{{""/**/'\"";:// \\""}}&quot;"">"u8,
            @"<a style=""font-family: '\2f**\2f\27\22\3b\3a\2f\2f  \\', &quot;\2f**\2f\27\22\3b\3a\2f\2f  \\&quot;"">"u8
        ),
        new(
            "styleURLSpecialsEncoded"u8,
            @"<a style=""border-image: url({{""/**/'\"";:// \\""}}), url(&quot;{{""/**/'\"";:// \\""}}&quot;), url('{{""/**/'\"";:// \\""}}'), 'http://www.example.com/?q={{""/**/'\"";:// \\""}}''"">"u8,
            @"<a style=""border-image: url(/**/%27%22;://%20%5c), url(&quot;/**/%27%22;://%20%5c&quot;), url('/**/%27%22;://%20%5c'), 'http://www.example.com/?q=%2f%2a%2a%2f%27%22%3b%3a%2f%2f%20%5c''"">"u8
        ),
        new(
            "HTML comment"u8,
            "<b>Hello, <!-- name of world -->{{.C}}</b>"u8,
            "<b>Hello, &lt;Cincinnati&gt;</b>"u8
        ),
        new(
            "HTML comment not first < in text node."u8,
            "<<!-- -->!--"u8,
            "&lt;!--"u8
        ),
        new(
            "HTML normalization 1"u8,
            "a < b"u8,
            "a &lt; b"u8
        ),
        new(
            "HTML normalization 2"u8,
            "a << b"u8,
            "a &lt;&lt; b"u8
        ),
        new(
            "HTML normalization 3"u8,
            "a<<!-- --><!-- -->b"u8,
            "a&lt;b"u8
        ),
        new(
            "HTML doctype not normalized"u8,
            "<!DOCTYPE html>Hello, World!"u8,
            "<!DOCTYPE html>Hello, World!"u8
        ),
        new(
            "HTML doctype not case-insensitive"u8,
            "<!doCtYPE htMl>Hello, World!"u8,
            "<!doCtYPE htMl>Hello, World!"u8
        ),
        new(
            "No doctype injection"u8,
            @"<!{{""DOCTYPE""}}"u8,
            "&lt;!DOCTYPE"u8
        ),
        new(
            "Split HTML comment"u8,
            "<b>Hello, <!-- name of {{if .T}}city -->{{.C}}{{else}}world -->{{.W}}{{end}}</b>"u8,
            "<b>Hello, &lt;Cincinnati&gt;</b>"u8
        ),
        new(
            "JS line comment"u8,
            "<script>for (;;) { if (c()) break// foo not a label\n"u8 + "foo({{.T}});}</script>"u8,
            "<script>for (;;) { if (c()) break\n"u8 + "foo( true );}</script>"u8
        ),
        new(
            "JS multiline block comment"u8,
            "<script>for (;;) { if (c()) break/* foo not a label\n"u8 + " */foo({{.T}});}</script>"u8, // Newline separates break from call. If newline
 // removed, then break will consume label leaving
 // code invalid.

            "<script>for (;;) { if (c()) break\n"u8 + "foo( true );}</script>"u8
        ),
        new(
            "JS single-line block comment"u8,
            "<script>for (;;) {\n"u8 + "if (c()) break/* foo a label */foo;"u8 + "x({{.T}});}</script>"u8, // Newline separates break from call. If newline
 // removed, then break will consume label leaving
 // code invalid.

            "<script>for (;;) {\n"u8 + "if (c()) break foo;"u8 + "x( true );}</script>"u8
        ),
        new(
            "JS block comment flush with mathematical division"u8,
            "<script>var a/*b*//c\nd</script>"u8,
            "<script>var a /c\nd</script>"u8
        ),
        new(
            "JS mixed comments"u8,
            "<script>var a/*b*///c\nd</script>"u8,
            "<script>var a \nd</script>"u8
        ),
        new(
            "JS HTML-like comments"u8,
            "<script>before <!-- beep\nbetween\nbefore-->boop\n</script>"u8,
            "<script>before \nbetween\nbefore\n</script>"u8
        ),
        new(
            "JS hashbang comment"u8,
            "<script>#! beep\n</script>"u8,
            "<script>\n</script>"u8
        ),
        new(
            "Special tags in <script> string literals"u8,
            @"<script>var a = ""asd < 123 <!-- 456 < fgh <script jkl < 789 </script""</script>"u8,
            @"<script>var a = ""asd < 123 \x3C!-- 456 < fgh \x3Cscript jkl < 789 \x3C/script""</script>"u8
        ),
        new(
            "Special tags in <script> string literals (mixed case)"u8,
            @"<script>var a = ""<!-- <ScripT </ScripT""</script>"u8,
            @"<script>var a = ""\x3C!-- \x3CScripT \x3C/ScripT""</script>"u8
        ),
        new(
            "Special tags in <script> regex literals (mixed case)"u8,
            @"<script>var a = /<!-- <ScripT </ScripT/</script>"u8,
            @"<script>var a = /\x3C!-- \x3CScripT \x3C/ScripT/</script>"u8
        ),
        new(
            "CSS comments"u8,
            "<style>p// paragraph\n"u8 + @"{border: 1px/* color */{{""#00f""}}}</style>"u8,
            "<style>p\n"u8 + "{border: 1px #00f}</style>"u8
        ),
        new(
            "JS attr block comment"u8,
            @"<a onclick=""f(&quot;&quot;); /* alert({{.H}}) */"">"u8, // Attribute comment tests should pass if the comments
 // are successfully elided.

            @"<a onclick=""f(&quot;&quot;); /* alert() */"">"u8
        ),
        new(
            "JS attr line comment"u8,
            @"<a onclick=""// alert({{.G}})"">"u8,
            @"<a onclick=""// alert()"">"u8
        ),
        new(
            "CSS attr block comment"u8,
            @"<a style=""/* color: {{.H}} */"">"u8,
            @"<a style=""/* color:  */"">"u8
        ),
        new(
            "CSS attr line comment"u8,
            @"<a style=""// color: {{.G}}"">"u8,
            @"<a style=""// color: "">"u8
        ),
        new(
            "HTML substitution commented out"u8,
            "<p><!-- {{.H}} --></p>"u8,
            "<p></p>"u8
        ),
        new(
            "Comment ends flush with start"u8,
            "<!--{{.}}--><script>/*{{.}}*///{{.}}\n</script><style>/*{{.}}*///{{.}}\n</style><a onclick='/*{{.}}*///{{.}}' style='/*{{.}}*///{{.}}'>"u8,
            "<script> \n</script><style> \n</style><a onclick='/**///' style='/**///'>"u8
        ),
        new(
            "typed HTML in text"u8,
            @"{{.W}}"u8,
            @"&iexcl;<b class=""foo"">Hello</b>, <textarea>O'World</textarea>!"u8
        ),
        new(
            "typed HTML in attribute"u8,
            @"<div title=""{{.W}}"">"u8,
            @"<div title=""&iexcl;Hello, O&#39;World!"">"u8
        ),
        new(
            "typed HTML in script"u8,
            @"<button onclick=""alert({{.W}})"">"u8,
            @"<button onclick=""alert(&#34;\u0026iexcl;\u003cb class=\&#34;foo\&#34;\u003eHello\u003c/b\u003e, \u003ctextarea\u003eO&#39;World\u003c/textarea\u003e!&#34;)"">"u8
        ),
        new(
            "typed HTML in RCDATA"u8,
            @"<textarea>{{.W}}</textarea>"u8,
            @"<textarea>&iexcl;&lt;b class=&#34;foo&#34;&gt;Hello&lt;/b&gt;, &lt;textarea&gt;O&#39;World&lt;/textarea&gt;!</textarea>"u8
        ),
        new(
            "range in textarea"u8,
            "<textarea>{{range .A}}{{.}}{{end}}</textarea>"u8,
            "<textarea>&lt;a&gt;&lt;b&gt;</textarea>"u8
        ),
        new(
            "No tag injection"u8,
            @"{{""10$""}}<{{""script src,evil.org/pwnd.js""}}..."u8,
            @"10$&lt;script src,evil.org/pwnd.js..."u8
        ),
        new(
            "No comment injection"u8,
            @"<{{""!--""}}"u8,
            @"&lt;!--"u8
        ),
        new(
            "No RCDATA end tag injection"u8,
            @"<textarea><{{""/textarea ""}}...</textarea>"u8,
            @"<textarea>&lt;/textarea ...</textarea>"u8
        ),
        new(
            "optional attrs"u8,
            @"<img class=""{{""iconClass""}}"""u8 + @"{{if .T}} id=""{{""<iconId>""}}""{{end}}"u8 + @" src="u8 + @"{{if .T}}""?{{""<iconPath>""}}"""u8 + @"{{else}}""images/cleardot.gif""{{end}}"u8 + @"{{if .T}}title=""{{""<title>""}}""{{end}}"u8 + @" alt="""u8 + @"{{if .T}}{{""<alt>""}}"u8 + @"{{else}}{{if .F}}{{""<title>""}}{{end}}"u8 + @"{{end}}"""u8 + @">"u8, // Double quotes inside if/else.
 // Missing space before title, but it is not a
 // part of the src attribute.
 // Quotes outside if/else.

            @"<img class=""iconClass"" id=""&lt;iconId&gt;"" src=""?%3ciconPath%3e""title=""&lt;title&gt;"" alt=""&lt;alt&gt;"">"u8
        ),
        new(
            "conditional valueless attr name"u8,
            @"<input{{if .T}} checked{{end}} name=n>"u8,
            @"<input checked name=n>"u8
        ),
        new(
            "conditional dynamic valueless attr name 1"u8,
            @"<input{{if .T}} {{""checked""}}{{end}} name=n>"u8,
            @"<input checked name=n>"u8
        ),
        new(
            "conditional dynamic valueless attr name 2"u8,
            @"<input {{if .T}}{{""checked""}} {{end}}name=n>"u8,
            @"<input checked name=n>"u8
        ),
        new(
            "dynamic attribute name"u8,
            @"<img on{{""load""}}=""alert({{""loaded""}})"">"u8, // Treated as JS since quotes are inserted.

            @"<img onload=""alert(&#34;loaded&#34;)"">"u8
        ),
        new(
            "bad dynamic attribute name 1"u8, // Allow checked, selected, disabled, but not JS or
 // CSS attributes.

            @"<input {{""onchange""}}=""{{""doEvil()""}}"">"u8,
            @"<input ZgotmplZ=""doEvil()"">"u8
        ),
        new(
            "bad dynamic attribute name 2"u8,
            @"<div {{""sTyle""}}=""{{""color: expression(alert(1337))""}}"">"u8,
            @"<div ZgotmplZ=""color: expression(alert(1337))"">"u8
        ),
        new(
            "bad dynamic attribute name 3"u8, // Allow title or alt, but not a URL.

            @"<img {{""src""}}=""{{""javascript:doEvil()""}}"">"u8,
            @"<img ZgotmplZ=""javascript:doEvil()"">"u8
        ),
        new(
            "bad dynamic attribute name 4"u8, // Structure preservation requires values to associate
 // with a consistent attribute.

            @"<input checked {{""""}}=""Whose value am I?"">"u8,
            @"<input checked ZgotmplZ=""Whose value am I?"">"u8
        ),
        new(
            "dynamic element name"u8,
            @"<h{{3}}><table><t{{""head""}}>...</h{{3}}>"u8,
            @"<h3><table><thead>...</h3>"u8
        ),
        new(
            "bad dynamic element name"u8, // Dynamic element names are typically used to switch
 // between (thead, tfoot, tbody), (ul, ol), (th, td),
 // and other replaceable sets.
 // We do not currently easily support (ul, ol).
 // If we do change to support that, this test should
 // catch failures to filter out special tag names which
 // would violate the structure preservation property --
 // if any special tag name could be substituted, then
 // the content could be raw text/RCDATA for some inputs
 // and regular HTML content for others.

            @"<{{""script""}}>{{""doEvil()""}}</{{""script""}}>"u8,
            @"&lt;script>doEvil()&lt;/script>"u8
        ),
        new(
            "srcset bad URL in second position"u8,
            @"<img srcset=""{{""/not-an-image#,javascript:alert(1)""}}"">"u8, // The second URL is also filtered.

            @"<img srcset=""/not-an-image#,#ZgotmplZ"">"u8
        ),
        new(
            "srcset buffer growth"u8,
            @"<img srcset={{"",,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,""}}>"u8,
            @"<img srcset=,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,>"u8
        ),
        new(
            "unquoted empty attribute value (plaintext)"u8,
            "<p name={{.U}}>"u8,
            "<p name=ZgotmplZ>"u8
        ),
        new(
            "unquoted empty attribute value (url)"u8,
            "<p href={{.U}}>"u8,
            "<p href=ZgotmplZ>"u8
        ),
        new(
            "quoted empty attribute value"u8,
            "<p name=\"{{.U}}\">"u8,
            "<p name=\"\">"u8
        ),
        new(
            "JS template lit special characters"u8,
            "<script>var a = `{{.I}}`</script>"u8,
            "<script>var a = `\\u0024\\u007b asd \\u0060\\u0060 \\u007d`</script>"u8
        ),
        new(
            "JS template lit special characters, nested lit"u8,
            "<script>var a = `${ `{{.I}}` }`</script>"u8,
            "<script>var a = `${ `\\u0024\\u007b asd \\u0060\\u0060 \\u007d` }`</script>"u8
        ),
        new(
            "JS template lit, nested JS"u8,
            "<script>var a = `${ var a = \"{{\"a \\\" d\"}}\" }`</script>"u8,
            "<script>var a = `${ var a = \"a \\u0022 d\" }`</script>"u8
        )
    }.slice();
    foreach (var (_, vᴛ1) in tests) {
        ref var test = ref heap(new TestEscape_tests(), out var Ꮡtest);
        test = vᴛ1;

        var pdataʗ1 = pdata;
        var testʗ1 = test;
        Ꮡt.Run(test.name, (ж<testing.T> tΔ1) => {
            var tmpl = New(testʗ1.name);
            var (ᴛ1, ᴛ2) = tmpl.Parse(testʗ1.input);
            tmpl = Must(ᴛ1, ᴛ2);
            // Check for bug 6459: Tree field was not set in Parse.
            if ((~tmpl).Tree != (~(~tmpl).text).Tree) {
                tΔ1.Fatalf("%s: tree not set properly"u8, testʗ1.name);
            }
            var b = @new<strings.Builder>();
            {
                var err = tmpl.Execute(new template_test_package.strings_BuilderжWriter(b), Ꮡdata.Value); if (err != default!) {
                    tΔ1.Fatalf("%s: template execution failed: %s"u8, testʗ1.name, err);
                }
            }
            {
                @string w = testʗ1.output;
                @string g = b.String(); if (w != g) {
                    tΔ1.Fatalf("%s: escaped output: want\n\t%q\ngot\n\t%q"u8, testʗ1.name, w, g);
                }
            }
            b.Reset();
            {
                var err = tmpl.Execute(new template_test_package.strings_BuilderжWriter(b), pdataʗ1.OrTypedNil()); if (err != default!) {
                    tΔ1.Fatalf("%s: template execution failed for pointer: %s"u8, testʗ1.name, err);
                }
            }
            {
                @string w = testʗ1.output;
                @string g = b.String(); if (w != g) {
                    tΔ1.Fatalf("%s: escaped output for pointer: want\n\t%q\ngot\n\t%q"u8, testʗ1.name, w, g);
                }
            }
            if ((~tmpl).Tree != (~(~tmpl).text).Tree) {
                tΔ1.Fatalf("%s: tree mismatch"u8, testʗ1.name);
            }
        });
    }
}

internal partial struct TestEscapeMap_type /*dyn*/ {
    internal @string desc, input, output;
}

public static void TestEscapeMap(ж<testing.T> Ꮡt) {
    ref var t = ref Ꮡt.DerefOrNull();

    var data = new map<@string, @string>{
        ["html"u8] = @"<h1>Hi!</h1>"u8,
        ["urlquery"u8] = @"http://www.foo.com/index.html?title=main"u8
    };
    foreach (var (_, test) in new TestEscapeMap_type[]{ // covering issue 20323

        new(
            "field with predefined escaper name 1"u8,
            @"{{.html | print}}"u8,
            @"&lt;h1&gt;Hi!&lt;/h1&gt;"u8
        ), // covering issue 20323

        new(
            "field with predefined escaper name 2"u8,
            @"{{.urlquery | print}}"u8,
            @"http://www.foo.com/index.html?title=main"u8
        )
    }.array()) {
        var (ᴛ3, ᴛ4) = New(""u8).Parse(test.input);
        var tmpl = Must(ᴛ3, ᴛ4);
        var b = @new<strings.Builder>();
        {
            var err = tmpl.Execute(new template_test_package.strings_BuilderжWriter(b), data); if (err != default!) {
                Ꮡt.Errorf("%s: template execution failed: %s"u8, test.desc, err);
                continue;
            }
        }
        {
            @string w = test.output;
            @string g = b.String(); if (w != g) {
                Ꮡt.Errorf("%s: escaped output: want\n\t%q\ngot\n\t%q"u8, test.desc, w, g);
                continue;
            }
        }
    }
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static readonly @string mainˢ = "main"u8;

internal partial struct TestEscapeSet_dataItem /*dyn*/ {
    public slice<ж<TestEscapeSet_dataItem>> Children;
    public @string X;
}

internal partial struct TestEscapeSet_tests /*dyn*/ {
    internal map<@string, @string> inputs;
    internal @string want;
}

public static void TestEscapeSet(ж<testing.T> Ꮡt) {
    ref var t = ref Ꮡt.DerefOrNull();

    var data = new TestEscapeSet_dataItem(
        Children: new ж<TestEscapeSet_dataItem>[]{
            Ꮡ(new TestEscapeSet_dataItem(X: "foo"u8)),
            Ꮡ(new TestEscapeSet_dataItem(X: "<bar>"u8)),
            Ꮡ(new TestEscapeSet_dataItem(
                Children: new ж<TestEscapeSet_dataItem>[]{
                    Ꮡ(new TestEscapeSet_dataItem(X: "baz"u8))
                }.slice()))
        }.slice()
    );
    var tests = new TestEscapeSet_tests[]{ // The trivial set.

        new(
            new map<@string, @string>{
                ["main"u8] = @""u8
            },
            @""u8
        ), // A template called in the start context.

        new(
            new map<@string, @string>{
                ["main"u8] = @"Hello, {{template ""helper""}}!"u8, // Not a valid top level HTML template.
 // "<b" is not a full tag.

                ["helper"u8] = @"{{""<World>""}}"u8
            },
            @"Hello, &lt;World&gt;!"u8
        ), // A template called in a context other than the start.

        new(
            new map<@string, @string>{
                ["main"u8] = @"<a onclick='a = {{template ""helper""}};'>"u8, // Not a valid top level HTML template.
 // "<b" is not a full tag.

                ["helper"u8] = @"{{""<a>""}}<b"u8
            },
            @"<a onclick='a = &#34;\u003ca\u003e&#34;<b;'>"u8
        ), // A recursive template that ends in its start context.

        new(
            new map<@string, @string>{
                ["main"u8] = @"{{range .Children}}{{template ""main"" .}}{{else}}{{.X}} {{end}}"u8
            },
            @"foo &lt;bar&gt; baz "u8
        ), // A recursive helper template that ends in its start context.

        new(
            new map<@string, @string>{
                ["main"u8] = @"{{template ""helper"" .}}"u8,
                ["helper"u8] = @"{{if .Children}}<ul>{{range .Children}}<li>{{template ""main"" .}}</li>{{end}}</ul>{{else}}{{.X}}{{end}}"u8
            },
            @"<ul><li>foo</li><li>&lt;bar&gt;</li><li><ul><li>baz</li></ul></li></ul>"u8
        ), // Co-recursive templates that end in its start context.

        new(
            new map<@string, @string>{
                ["main"u8] = @"<blockquote>{{range .Children}}{{template ""helper"" .}}{{end}}</blockquote>"u8,
                ["helper"u8] = @"{{if .Children}}{{template ""main"" .}}{{else}}{{.X}}<br>{{end}}"u8
            },
            @"<blockquote>foo<br>&lt;bar&gt;<br><blockquote>baz<br></blockquote></blockquote>"u8
        ), // A template that is called in two different contexts.

        new(
            new map<@string, @string>{
                ["main"u8] = @"<button onclick=""title='{{template ""helper""}}'; ..."">{{template ""helper""}}</button>"u8,
                ["helper"u8] = @"{{11}} of {{""<100>""}}"u8
            },
            @"<button onclick=""title='11 of \u003c100\u003e'; ..."">11 of &lt;100&gt;</button>"u8
        ), // A non-recursive template that ends in a different context.
 // helper starts in jsCtxRegexp and ends in jsCtxDivOp.

        new(
            new map<@string, @string>{
                ["main"u8] = @"<script>var x={{template ""helper""}}/{{""42""}};</script>"u8,
                ["helper"u8] = "{{126}}"u8
            },
            @"<script>var x= 126 /""42"";</script>"u8
        ), // A recursive template that ends in a similar context.

        new(
            new map<@string, @string>{
                ["main"u8] = @"<script>var x=[{{template ""countdown"" 4}}];</script>"u8,
                ["countdown"u8] = @"{{.}}{{if .}},{{template ""countdown"" . | pred}}{{end}}"u8
            },
            @"<script>var x=[ 4 , 3 , 2 , 1 , 0 ];</script>"u8
        )
    }.slice();
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
    // pred is a template function that returns the predecessor of a
    // natural number for testing recursive templates.
    var fns = new FuncMap(new map<@string, any>{["pred"u8] = ((Funcꓸꓸꓸ<any, (any, error)>)((any, error) (params ꓸꓸꓸany aʗp) => {
        var a = aʗp.slice();
        if (len(a) == 1) {
            {
                var (i, _) = a[0]._<nint>(ᐧ); if (i > 0) {
                    return (i - 1, default!);
                }
            }
        }
        return (default!, fmt.Errorf("undefined pred(%v)"u8, (any)(a)));
    }))
    });
    foreach (var (_, test) in tests) {
        @string source = ""u8;
        foreach (var (name, body) in test.inputs) {
            source += fmt.Sprintf("{{define %q}}%s{{end}} "u8, name, body);
        }
        var (tmpl, err) = New(rootˢ).Funcs(fns).Parse(source);
        if (err != default!) {
            Ꮡt.Errorf("error parsing %q: %v"u8, source, err);
            continue;
        }
        ref var b = ref heap(new strings.Builder(), out var Ꮡb);
        {
            var errΔ1 = tmpl.ExecuteTemplate(new template_test_package.strings_BuilderжWriter(Ꮡb), mainˢ, data); if (errΔ1 != default!) {
                Ꮡt.Errorf("%q executing %v"u8, errΔ1.Error(), tmpl.Lookup(mainˢ).OrTypedNil());
                continue;
            }
        }
        {
            @string got = b.String(); if (test.want != got) {
                Ꮡt.Errorf("want\n\t%q\ngot\n\t%q"u8, test.want, got);
            }
        }
    }
}

internal partial struct TestErrors_tests /*dyn*/ {
    internal @string input;
    internal @string err;
}

public static void TestErrors(ж<testing.T> Ꮡt) {
    ref var t = ref Ꮡt.DerefOrNull();

    var tests = new TestErrors_tests[]{ // Non-error cases.

        new(
            "{{if .Cond}}<a>{{else}}<b>{{end}}"u8,
            ""u8
        ),
        new(
            "{{if .Cond}}<a>{{end}}"u8,
            ""u8
        ),
        new(
            "{{if .Cond}}{{else}}<b>{{end}}"u8,
            ""u8
        ),
        new(
            "{{with .Cond}}<div>{{end}}"u8,
            ""u8
        ),
        new(
            "{{range .Items}}<a>{{end}}"u8,
            ""u8
        ),
        new(
            "<a href='/foo?{{range .Items}}&{{.K}}={{.V}}{{end}}'>"u8,
            ""u8
        ),
        new(
            "{{range .Items}}<a{{if .X}}{{end}}>{{end}}"u8,
            ""u8
        ),
        new(
            "{{range .Items}}<a{{if .X}}{{end}}>{{continue}}{{end}}"u8,
            ""u8
        ),
        new(
            "{{range .Items}}<a{{if .X}}{{end}}>{{break}}{{end}}"u8,
            ""u8
        ),
        new(
            "{{range .Items}}<a{{if .X}}{{end}}>{{if .X}}{{break}}{{end}}{{end}}"u8,
            ""u8
        ),
        new(
            "<script>var a = `${a+b}`</script>`"u8,
            ""u8
        ),
        new(
            "<script>var tmpl = `asd`;</script>"u8,
            @""u8
        ),
        new(
            "<script>var tmpl = `${1}`;</script>"u8,
            @""u8
        ),
        new(
            "<script>var tmpl = `${return ``}`;</script>"u8,
            @""u8
        ),
        new(
            "<script>var tmpl = `${return {{.}} }`;</script>"u8,
            @""u8
        ),
        new(
            "<script>var tmpl = `${ let a = {1:1} {{.}} }`;</script>"u8,
            @""u8
        ),
        new(
            "<script>var tmpl = `asd ${return \"{\"}`;</script>"u8,
            @""u8
        ), // Error cases.

        new(
            "{{if .Cond}}<a{{end}}"u8,
            "z:1:5: {{if}} branches"u8
        ),
        new(
            "{{if .Cond}}\n{{else}}\n<a{{end}}"u8,
            "z:1:5: {{if}} branches"u8
        ),
        new(
            @"{{if .Cond}}<a href=""foo"">{{else}}<a href=""bar>{{end}}"u8, // Missing quote in the else branch.

            "z:1:5: {{if}} branches"u8
        ),
        new(
            "<a {{if .Cond}}href='{{else}}title='{{end}}{{.X}}'>"u8, // Different kind of attribute: href implies a URL.

            "z:1:8: {{if}} branches"u8
        ),
        new(
            "\n{{with .X}}<a{{end}}"u8,
            "z:2:7: {{with}} branches"u8
        ),
        new(
            "\n{{with .X}}<a>{{else}}<a{{end}}"u8,
            "z:2:7: {{with}} branches"u8
        ),
        new(
            "{{range .Items}}<a{{end}}"u8,
            @"z:1: on range loop re-entry: ""<"" in attribute name: ""<a"""u8
        ),
        new(
            "\n{{range .Items}} x='<a{{end}}"u8,
            "z:2:8: on range loop re-entry: {{range}} branches"u8
        ),
        new(
            "{{range .Items}}<a{{if .X}}{{break}}{{end}}>{{end}}"u8,
            "z:1:29: at range loop break: {{range}} branches end in different contexts"u8
        ),
        new(
            "{{range .Items}}<a{{if .X}}{{continue}}{{end}}>{{end}}"u8,
            "z:1:29: at range loop continue: {{range}} branches end in different contexts"u8
        ),
        new(
            "{{range .Items}}{{if .X}}{{break}}{{end}}<a{{if .Y}}{{continue}}{{end}}>{{if .Z}}{{continue}}{{end}}{{end}}"u8,
            "z:1:54: at range loop continue: {{range}} branches end in different contexts"u8
        ),
        new(
            "<a b=1 c={{.H}}"u8,
            "z: ends in a non-text context: {stateAttr delimSpaceOrTagEnd"u8
        ),
        new(
            "<script>foo();"u8,
            "z: ends in a non-text context: {stateJS"u8
        ),
        new(
            @"<a href=""{{if .F}}/foo?a={{else}}/bar/{{end}}{{.H}}"">"u8,
            "z:1:47: {{.H}} appears in an ambiguous context within a URL"u8
        ),
        new(
            @"<a onclick=""alert('Hello \"u8,
            @"unfinished escape sequence in JS string: ""Hello \\"""u8
        ),
        new(
            @"<a onclick='alert(""Hello\, World\"u8,
            @"unfinished escape sequence in JS string: ""Hello\\, World\\"""u8
        ),
        new(
            @"<a onclick='alert(/x+\"u8,
            @"unfinished escape sequence in JS string: ""x+\\"""u8
        ),
        new(
            @"<a onclick=""/foo[\]/"u8,
            @"unfinished JS regexp charset: ""foo[\\]/"""u8
        ),
        new(
            @"<script>{{if false}}var x = 1{{end}}/-{{""1.5""}}/i.test(x)</script>"u8, // It is ambiguous whether 1.5 should be 1\.5 or 1.5.
 // Either `var x = 1/- 1.5 /i.test(x)`
 // where `i.test(x)` is a method call of reference i,
 // or `/-1\.5/i.test(x)` which is a method call on a
 // case insensitive regular expression.

            @"'/' could start a division or regexp: ""/-"""u8
        ),
        new(
            @"{{template ""foo""}}"u8,
            "z:1:11: no such template \"foo\""u8
        ),
        new(
            @"<div{{template ""y""}}>"u8 + @"{{define ""y""}} foo<b{{end}}"u8, // Illegal starting in stateTag but not in stateText.

            @"""<"" in attribute name: "" foo<b"""u8
        ),
        new(
            @"<script>reverseList = [{{template ""t""}}]</script>"u8 + @"{{define ""t""}}{{if .Tail}}{{template ""t"" .Tail}}{{end}}{{.Head}}"",{{end}}"u8, // Missing " after recursive call.

            @": cannot compute output context for template t$htmltemplate_stateJS_elementScript"u8
        ),
        new(
            @"<input type=button value=onclick=>"u8,
            @"html/template:z: ""="" in unquoted attr: ""onclick="""u8
        ),
        new(
            @"<input type=button value= onclick=>"u8,
            @"html/template:z: ""="" in unquoted attr: ""onclick="""u8
        ),
        new(
            @"<input type=button value= 1+1=2>"u8,
            @"html/template:z: ""="" in unquoted attr: ""1+1=2"""u8
        ),
        new(
            "<a class=`foo>"u8,
            "html/template:z: \"`\" in unquoted attr: \"`foo\""u8
        ),
        new(
            @"<a style=font:'Arial'>"u8,
            @"html/template:z: ""'"" in unquoted attr: ""font:'Arial'"""u8
        ),
        new(
            @"<a=foo>"u8,
            @": expected space, attr name, or end of tag, but got ""=foo>"""u8
        ),
        new(
            @"Hello, {{. | urlquery | print}}!"u8, // urlquery is disallowed if it is not the last command in the pipeline.

            @"predefined escaper ""urlquery"" disallowed in template"u8
        ),
        new(
            @"Hello, {{. | html | print}}!"u8, // html is disallowed if it is not the last command in the pipeline.

            @"predefined escaper ""html"" disallowed in template"u8
        ),
        new(
            @"Hello, {{html . | print}}!"u8, // A direct call to html is disallowed if it is not the last command in the pipeline.

            @"predefined escaper ""html"" disallowed in template"u8
        ),
        new(
            @"<div class={{. | html}}>Hello<div>"u8, // html is disallowed in a pipeline that is in an unquoted attribute context,
 // even if it is the last command in the pipeline.

            @"predefined escaper ""html"" disallowed in template"u8
        ),
        new(
            @"Hello, {{. | urlquery | html}}!"u8, // html is allowed since it is the last command in the pipeline, but urlquery is not.

            @"predefined escaper ""urlquery"" disallowed in template"u8
        )
    }.slice();
    foreach (var (_, test) in tests) {
        var buf = @new<bytes.Buffer>();
        var (tmpl, err) = New("z"u8).Parse(test.input);
        if (err != default!) {
            Ꮡt.Errorf("input=%q: unexpected parse error %s\n"u8, test.input, err);
            continue;
        }
        err = tmpl.Execute(new template_test_package.bytes_BufferжWriter(buf), default!);
        @string got = default!;
        if (err != default!) {
            got = err.Error();
        }
        if (test.err == ""u8) {
            if (got != ""u8) {
                Ꮡt.Errorf("input=%q: unexpected error %q"u8, test.input, got);
            }
            continue;
        }
        if (!strings.Contains(got, test.err)) {
            Ꮡt.Errorf("input=%q: error\n\t%q\ndoes not contain expected string\n\t%q"u8, test.input, got, test.err);
            continue;
        }
        // Check that we get the same error if we call Execute again.
        {
            var errΔ1 = tmpl.Execute(new template_test_package.bytes_BufferжWriter(buf), default!); if (errΔ1 == default! || errΔ1.Error() != got) {
                Ꮡt.Errorf("input=%q: unexpected error on second call %q"u8, test.input, errΔ1);
            }
        }
    }
}

internal partial struct TestEscapeText_tests /*dyn*/ {
    internal @string input;
    internal global::go.html.template_package.context output;
}

public static void TestEscapeText(ж<testing.T> Ꮡt) {
    ref var t = ref Ꮡt.DerefOrNull();

    var tests = new TestEscapeText_tests[]{
        new(
            @""u8,
            new context(nil)
        ),
        new(
            @"Hello, World!"u8,
            new context(nil)
        ),
        new(
            @"I <3 Ponies!"u8, // An orphaned "<" is OK.

            new context(nil)
        ),
        new(
            @"<a"u8,
            new context(state: stateTag)
        ),
        new(
            @"<a "u8,
            new context(state: stateTag)
        ),
        new(
            @"<a>"u8,
            new context(state: stateText)
        ),
        new(
            @"<a href"u8,
            new context(state: stateAttrName, attr: attrURL)
        ),
        new(
            @"<a on"u8,
            new context(state: stateAttrName, attr: attrScript)
        ),
        new(
            @"<a href "u8,
            new context(state: stateAfterName, attr: attrURL)
        ),
        new(
            @"<a style  =  "u8,
            new context(state: stateBeforeValue, attr: attrStyle)
        ),
        new(
            @"<a href="u8,
            new context(state: stateBeforeValue, attr: attrURL)
        ),
        new(
            @"<a href=x"u8,
            new context(state: stateURL, delim: delimSpaceOrTagEnd, urlPart: urlPartPreQuery, attr: attrURL)
        ),
        new(
            @"<a href=x "u8,
            new context(state: stateTag)
        ),
        new(
            @"<a href=>"u8,
            new context(state: stateText)
        ),
        new(
            @"<a href=x>"u8,
            new context(state: stateText)
        ),
        new(
            @"<a href ='"u8,
            new context(state: stateURL, delim: delimSingleQuote, attr: attrURL)
        ),
        new(
            @"<a href=''"u8,
            new context(state: stateTag)
        ),
        new(
            @"<a href= """u8,
            new context(state: stateURL, delim: delimDoubleQuote, attr: attrURL)
        ),
        new(
            @"<a href="""""u8,
            new context(state: stateTag)
        ),
        new(
            @"<a title="""u8,
            new context(state: stateAttr, delim: delimDoubleQuote)
        ),
        new(
            @"<a HREF='http:"u8,
            new context(state: stateURL, delim: delimSingleQuote, urlPart: urlPartPreQuery, attr: attrURL)
        ),
        new(
            @"<a Href='/"u8,
            new context(state: stateURL, delim: delimSingleQuote, urlPart: urlPartPreQuery, attr: attrURL)
        ),
        new(
            @"<a href='"""u8,
            new context(state: stateURL, delim: delimSingleQuote, urlPart: urlPartPreQuery, attr: attrURL)
        ),
        new(
            @"<a href=""'"u8,
            new context(state: stateURL, delim: delimDoubleQuote, urlPart: urlPartPreQuery, attr: attrURL)
        ),
        new(
            @"<a href='&apos;"u8,
            new context(state: stateURL, delim: delimSingleQuote, urlPart: urlPartPreQuery, attr: attrURL)
        ),
        new(
            @"<a href=""&quot;"u8,
            new context(state: stateURL, delim: delimDoubleQuote, urlPart: urlPartPreQuery, attr: attrURL)
        ),
        new(
            @"<a href=""&#34;"u8,
            new context(state: stateURL, delim: delimDoubleQuote, urlPart: urlPartPreQuery, attr: attrURL)
        ),
        new(
            @"<a href=&quot;"u8,
            new context(state: stateURL, delim: delimSpaceOrTagEnd, urlPart: urlPartPreQuery, attr: attrURL)
        ),
        new(
            @"<img alt=""1"">"u8,
            new context(state: stateText)
        ),
        new(
            @"<img alt=""1>"""u8,
            new context(state: stateTag)
        ),
        new(
            @"<img alt=""1>"">"u8,
            new context(state: stateText)
        ),
        new(
            @"<input checked type=""checkbox"""u8,
            new context(state: stateTag)
        ),
        new(
            @"<a onclick="""u8,
            new context(state: stateJS, delim: delimDoubleQuote, attr: attrScript)
        ),
        new(
            @"<a onclick=""//foo"u8,
            new context(state: stateJSLineCmt, delim: delimDoubleQuote, attr: attrScript)
        ),
        new(
            "<a onclick='//\n"u8,
            new context(state: stateJS, delim: delimSingleQuote, attr: attrScript)
        ),
        new(
            "<a onclick='//\r\n"u8,
            new context(state: stateJS, delim: delimSingleQuote, attr: attrScript)
        ),
        new(
            "<a onclick='//\u2028"u8,
            new context(state: stateJS, delim: delimSingleQuote, attr: attrScript)
        ),
        new(
            @"<a onclick=""/*"u8,
            new context(state: stateJSBlockCmt, delim: delimDoubleQuote, attr: attrScript)
        ),
        new(
            @"<a onclick=""/*/"u8,
            new context(state: stateJSBlockCmt, delim: delimDoubleQuote, attr: attrScript)
        ),
        new(
            @"<a onclick=""/**/"u8,
            new context(state: stateJS, delim: delimDoubleQuote, attr: attrScript)
        ),
        new(
            @"<a onkeypress=""&quot;"u8,
            new context(state: stateJSDqStr, delim: delimDoubleQuote, attr: attrScript)
        ),
        new(
            @"<a onclick='&quot;foo&quot;"u8,
            new context(state: stateJS, delim: delimSingleQuote, jsCtx: jsCtxDivOp, attr: attrScript)
        ),
        new(
            @"<a onclick=&#39;foo&#39;"u8,
            new context(state: stateJS, delim: delimSpaceOrTagEnd, jsCtx: jsCtxDivOp, attr: attrScript)
        ),
        new(
            @"<a onclick=&#39;foo"u8,
            new context(state: stateJSSqStr, delim: delimSpaceOrTagEnd, attr: attrScript)
        ),
        new(
            @"<a onclick=""&quot;foo'"u8,
            new context(state: stateJSDqStr, delim: delimDoubleQuote, attr: attrScript)
        ),
        new(
            @"<a onclick=""'foo&quot;"u8,
            new context(state: stateJSSqStr, delim: delimDoubleQuote, attr: attrScript)
        ),
        new(
            "<a onclick=\"`foo"u8,
            new context(state: stateJSTmplLit, delim: delimDoubleQuote, attr: attrScript)
        ),
        new(
            @"<A ONCLICK=""'"u8,
            new context(state: stateJSSqStr, delim: delimDoubleQuote, attr: attrScript)
        ),
        new(
            @"<a onclick=""/"u8,
            new context(state: stateJSRegexp, delim: delimDoubleQuote, attr: attrScript)
        ),
        new(
            @"<a onclick=""'foo'"u8,
            new context(state: stateJS, delim: delimDoubleQuote, jsCtx: jsCtxDivOp, attr: attrScript)
        ),
        new(
            @"<a onclick=""'foo\'"u8,
            new context(state: stateJSSqStr, delim: delimDoubleQuote, attr: attrScript)
        ),
        new(
            @"<a onclick=""'foo\'"u8,
            new context(state: stateJSSqStr, delim: delimDoubleQuote, attr: attrScript)
        ),
        new(
            @"<a onclick=""/foo/"u8,
            new context(state: stateJS, delim: delimDoubleQuote, jsCtx: jsCtxDivOp, attr: attrScript)
        ),
        new(
            @"<script>/foo/ /="u8,
            new context(state: stateJS, element: elementScript)
        ),
        new(
            @"<a onclick=""1 /foo"u8,
            new context(state: stateJS, delim: delimDoubleQuote, jsCtx: jsCtxDivOp, attr: attrScript)
        ),
        new(
            @"<a onclick=""1 /*c*/ /foo"u8,
            new context(state: stateJS, delim: delimDoubleQuote, jsCtx: jsCtxDivOp, attr: attrScript)
        ),
        new(
            @"<a onclick=""/foo[/]"u8,
            new context(state: stateJSRegexp, delim: delimDoubleQuote, attr: attrScript)
        ),
        new(
            @"<a onclick=""/foo\/"u8,
            new context(state: stateJSRegexp, delim: delimDoubleQuote, attr: attrScript)
        ),
        new(
            @"<a onclick=""/foo/"u8,
            new context(state: stateJS, delim: delimDoubleQuote, jsCtx: jsCtxDivOp, attr: attrScript)
        ),
        new(
            @"<input checked style="""u8,
            new context(state: stateCSS, delim: delimDoubleQuote, attr: attrStyle)
        ),
        new(
            @"<a style=""//"u8,
            new context(state: stateCSSLineCmt, delim: delimDoubleQuote, attr: attrStyle)
        ),
        new(
            @"<a style=""//</script>"u8,
            new context(state: stateCSSLineCmt, delim: delimDoubleQuote, attr: attrStyle)
        ),
        new(
            "<a style='//\n"u8,
            new context(state: stateCSS, delim: delimSingleQuote, attr: attrStyle)
        ),
        new(
            "<a style='//\r"u8,
            new context(state: stateCSS, delim: delimSingleQuote, attr: attrStyle)
        ),
        new(
            @"<a style=""/*"u8,
            new context(state: stateCSSBlockCmt, delim: delimDoubleQuote, attr: attrStyle)
        ),
        new(
            @"<a style=""/*/"u8,
            new context(state: stateCSSBlockCmt, delim: delimDoubleQuote, attr: attrStyle)
        ),
        new(
            @"<a style=""/**/"u8,
            new context(state: stateCSS, delim: delimDoubleQuote, attr: attrStyle)
        ),
        new(
            @"<a style=""background: '"u8,
            new context(state: stateCSSSqStr, delim: delimDoubleQuote, attr: attrStyle)
        ),
        new(
            @"<a style=""background: &quot;"u8,
            new context(state: stateCSSDqStr, delim: delimDoubleQuote, attr: attrStyle)
        ),
        new(
            @"<a style=""background: '/foo?img="u8,
            new context(state: stateCSSSqStr, delim: delimDoubleQuote, urlPart: urlPartQueryOrFrag, attr: attrStyle)
        ),
        new(
            @"<a style=""background: '/"u8,
            new context(state: stateCSSSqStr, delim: delimDoubleQuote, urlPart: urlPartPreQuery, attr: attrStyle)
        ),
        new(
            @"<a style=""background: url(&#x22;/"u8,
            new context(state: stateCSSDqURL, delim: delimDoubleQuote, urlPart: urlPartPreQuery, attr: attrStyle)
        ),
        new(
            @"<a style=""background: url('/"u8,
            new context(state: stateCSSSqURL, delim: delimDoubleQuote, urlPart: urlPartPreQuery, attr: attrStyle)
        ),
        new(
            @"<a style=""background: url('/)"u8,
            new context(state: stateCSSSqURL, delim: delimDoubleQuote, urlPart: urlPartPreQuery, attr: attrStyle)
        ),
        new(
            @"<a style=""background: url('/ "u8,
            new context(state: stateCSSSqURL, delim: delimDoubleQuote, urlPart: urlPartPreQuery, attr: attrStyle)
        ),
        new(
            @"<a style=""background: url(/"u8,
            new context(state: stateCSSURL, delim: delimDoubleQuote, urlPart: urlPartPreQuery, attr: attrStyle)
        ),
        new(
            @"<a style=""background: url( "u8,
            new context(state: stateCSSURL, delim: delimDoubleQuote, attr: attrStyle)
        ),
        new(
            @"<a style=""background: url( /image?name="u8,
            new context(state: stateCSSURL, delim: delimDoubleQuote, urlPart: urlPartQueryOrFrag, attr: attrStyle)
        ),
        new(
            @"<a style=""background: url(x)"u8,
            new context(state: stateCSS, delim: delimDoubleQuote, attr: attrStyle)
        ),
        new(
            @"<a style=""background: url('x'"u8,
            new context(state: stateCSS, delim: delimDoubleQuote, attr: attrStyle)
        ),
        new(
            @"<a style=""background: url( x "u8,
            new context(state: stateCSS, delim: delimDoubleQuote, attr: attrStyle)
        ),
        new(
            @"<!-- foo"u8,
            new context(state: stateHTMLCmt)
        ),
        new(
            @"<!-->"u8,
            new context(state: stateHTMLCmt)
        ),
        new(
            @"<!--->"u8,
            new context(state: stateHTMLCmt)
        ),
        new(
            @"<!-- foo -->"u8,
            new context(state: stateText)
        ),
        new(
            @"<script"u8,
            new context(state: stateTag, element: elementScript)
        ),
        new(
            @"<script "u8,
            new context(state: stateTag, element: elementScript)
        ),
        new(
            @"<script src=""foo.js"" "u8,
            new context(state: stateTag, element: elementScript)
        ),
        new(
            @"<script src='foo.js' "u8,
            new context(state: stateTag, element: elementScript)
        ),
        new(
            @"<script type=text/javascript "u8,
            new context(state: stateTag, element: elementScript)
        ),
        new(
            @"<script>"u8,
            new context(state: stateJS, jsCtx: jsCtxRegexp, element: elementScript)
        ),
        new(
            @"<script>foo"u8,
            new context(state: stateJS, jsCtx: jsCtxDivOp, element: elementScript)
        ),
        new(
            @"<script>foo</script>"u8,
            new context(state: stateText)
        ),
        new(
            @"<script>foo</script><!--"u8,
            new context(state: stateHTMLCmt)
        ),
        new(
            @"<script>document.write(""<p>foo</p>"");"u8,
            new context(state: stateJS, element: elementScript)
        ),
        new(
            @"<script>document.write(""<p>foo<\/script>"");"u8,
            new context(state: stateJS, element: elementScript)
        ),
        new(
            @"<script>document.write(""<script>alert(1)</script>"");"u8, // <script and </script tags are escaped, so </script> should not
 // cause us to exit the JS state.

            new context(state: stateJS, element: elementScript)
        ),
        new(
            @"<script>document.write(""<script>"u8,
            new context(state: stateJSDqStr, element: elementScript)
        ),
        new(
            @"<script>document.write(""<script>alert(1)</script>"u8,
            new context(state: stateJSDqStr, element: elementScript)
        ),
        new(
            @"<script>document.write(""<script>alert(1)<!--"u8,
            new context(state: stateJSDqStr, element: elementScript)
        ),
        new(
            @"<script>document.write(""<script>alert(1)</Script>"");"u8,
            new context(state: stateJS, element: elementScript)
        ),
        new(
            @"<script>document.write(""<!--"");"u8,
            new context(state: stateJS, element: elementScript)
        ),
        new(
            @"<script>let a = /</script"u8,
            new context(state: stateJSRegexp, element: elementScript)
        ),
        new(
            @"<script>let a = /</script/"u8,
            new context(state: stateJS, element: elementScript, jsCtx: jsCtxDivOp)
        ),
        new(
            @"<script type=""text/template"">"u8,
            new context(state: stateText)
        ), // covering issue 19968

        new(
            @"<script type=""TEXT/JAVASCRIPT"">"u8,
            new context(state: stateJS, element: elementScript)
        ), // covering issue 19965

        new(
            @"<script TYPE=""text/template"">"u8,
            new context(state: stateText)
        ),
        new(
            @"<script type=""notjs"">"u8,
            new context(state: stateText)
        ),
        new(
            @"<Script>"u8,
            new context(state: stateJS, element: elementScript)
        ),
        new(
            @"<SCRIPT>foo"u8,
            new context(state: stateJS, jsCtx: jsCtxDivOp, element: elementScript)
        ),
        new(
            @"<textarea>value"u8,
            new context(state: stateRCDATA, element: elementTextarea)
        ),
        new(
            @"<textarea>value</TEXTAREA>"u8,
            new context(state: stateText)
        ),
        new(
            @"<textarea name=html><b"u8,
            new context(state: stateRCDATA, element: elementTextarea)
        ),
        new(
            @"<title>value"u8,
            new context(state: stateRCDATA, element: elementTitle)
        ),
        new(
            @"<style>value"u8,
            new context(state: stateCSS, element: elementStyle)
        ),
        new(
            @"<a xlink:href"u8,
            new context(state: stateAttrName, attr: attrURL)
        ),
        new(
            @"<a xmlns"u8,
            new context(state: stateAttrName, attr: attrURL)
        ),
        new(
            @"<a xmlns:foo"u8,
            new context(state: stateAttrName, attr: attrURL)
        ),
        new(
            @"<a xmlnsxyz"u8,
            new context(state: stateAttrName)
        ),
        new(
            @"<a data-url"u8,
            new context(state: stateAttrName, attr: attrURL)
        ),
        new(
            @"<a data-iconUri"u8,
            new context(state: stateAttrName, attr: attrURL)
        ),
        new(
            @"<a data-urlItem"u8,
            new context(state: stateAttrName, attr: attrURL)
        ),
        new(
            @"<a g:"u8,
            new context(state: stateAttrName)
        ),
        new(
            @"<a g:url"u8,
            new context(state: stateAttrName, attr: attrURL)
        ),
        new(
            @"<a g:iconUri"u8,
            new context(state: stateAttrName, attr: attrURL)
        ),
        new(
            @"<a g:urlItem"u8,
            new context(state: stateAttrName, attr: attrURL)
        ),
        new(
            @"<a g:value"u8,
            new context(state: stateAttrName)
        ),
        new(
            @"<a svg:style='"u8,
            new context(state: stateCSS, delim: delimSingleQuote, attr: attrStyle)
        ),
        new(
            @"<svg:font-face"u8,
            new context(state: stateTag)
        ),
        new(
            @"<svg:a svg:onclick="""u8,
            new context(state: stateJS, delim: delimDoubleQuote, attr: attrScript)
        ),
        new(
            @"<svg:a svg:onclick=""x()"">"u8,
            new context(nil)
        ),
        new(
            "<script>var a = `"u8,
            new context(state: stateJSTmplLit, element: elementScript)
        ),
        new(
            "<script>var a = `${"u8,
            new context(state: stateJS, element: elementScript)
        ),
        new(
            "<script>var a = `${}"u8,
            new context(state: stateJSTmplLit, element: elementScript)
        ),
        new(
            "<script>var a = `${`"u8,
            new context(state: stateJSTmplLit, element: elementScript)
        ),
        new(
            "<script>var a = `${var a = \""u8,
            new context(state: stateJSDqStr, element: elementScript)
        ),
        new(
            "<script>var a = `${var a = \"`"u8,
            new context(state: stateJSDqStr, element: elementScript)
        ),
        new(
            "<script>var a = `${var a = \"}"u8,
            new context(state: stateJSDqStr, element: elementScript)
        ),
        new(
            "<script>var a = `${``"u8,
            new context(state: stateJS, element: elementScript)
        ),
        new(
            "<script>var a = `${`}"u8,
            new context(state: stateJSTmplLit, element: elementScript)
        ),
        new(
            "<script>`${ {} } asd`</script><script>`${ {} }"u8,
            new context(state: stateJSTmplLit, element: elementScript)
        ),
        new(
            "<script>var foo = `${ (_ => { return \"x\" })() + \"${"u8,
            new context(state: stateJSDqStr, element: elementScript)
        ),
        new(
            "<script>var a = `${ {</script><script>var b = `${ x }"u8,
            new context(state: stateJSTmplLit, element: elementScript, jsCtx: jsCtxDivOp)
        ),
        new(
            "<script>var foo = `x` + \"${"u8,
            new context(state: stateJSDqStr, element: elementScript)
        ),
        new(
            "<script>function f() { var a = `${}`; }"u8,
            new context(state: stateJS, element: elementScript)
        ),
        new(
            "<script>{`${}`}"u8,
            new context(state: stateJS, element: elementScript)
        ),
        new(
            "<script>`${ function f() { return `${1}` }() }`"u8,
            new context(state: stateJS, element: elementScript, jsCtx: jsCtxDivOp)
        ),
        new(
            "<script>function f() {`${ function f() { `${1}` } }`}"u8,
            new context(state: stateJS, element: elementScript, jsCtx: jsCtxDivOp)
        ),
        new(
            "<script>`${ { `` }"u8,
            new context(state: stateJS, element: elementScript)
        ),
        new(
            "<script>`${ { }`"u8,
            new context(state: stateJSTmplLit, element: elementScript)
        ),
        new(
            "<script>var foo = `${ foo({ a: { c: `${"u8,
            new context(state: stateJS, element: elementScript)
        ),
        new(
            "<script>var foo = `${ foo({ a: { c: `${ {{.}} }` }, b: "u8,
            new context(state: stateJS, element: elementScript)
        ),
        new(
            "<script>`${ `}"u8,
            new context(state: stateJSTmplLit, element: elementScript)
        )
    }.slice();
    foreach (var (_, test) in tests) {
        var (b, e) = (slice<byte>(test.input), makeEscaper(nil));
        var c = e.escapeText(new context(nil), Ꮡ(new parse.TextNode(NodeType: parse.NodeText, Text: b)));
        if (!test.output.eq(c)) {
            Ꮡt.Errorf("input %q: want context\n\t%v\ngot\n\t%v"u8, test.input, test.output, c);
            continue;
        }
        if (test.input != ((sstring)b)) {
            Ꮡt.Errorf("input %q: text node was modified: want %q got %q"u8, test.input, test.input, b);
            continue;
        }
    }
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static readonly @string testˢ = "test"u8;

internal partial struct TestEnsurePipelineContains_tests /*dyn*/ {
    internal @string input, output;
    internal slice<@string> ids;
}

public static void TestEnsurePipelineContains(ж<testing.T> Ꮡt) {
    ref var t = ref Ꮡt.DerefOrNull();

    var tests = new TestEnsurePipelineContains_tests[]{
        new(
            "{{.X}}"u8,
            ".X"u8,
            new @string[]{}.slice()
        ),
        new(
            "{{.X | html}}"u8,
            ".X | html"u8,
            new @string[]{}.slice()
        ),
        new(
            "{{.X}}"u8,
            ".X | html"u8,
            new @string[]{"html"u8}.slice()
        ),
        new(
            "{{html .X}}"u8,
            "_eval_args_ .X | html | urlquery"u8,
            new @string[]{"html"u8, "urlquery"u8}.slice()
        ),
        new(
            "{{html .X .Y .Z}}"u8,
            "_eval_args_ .X .Y .Z | html | urlquery"u8,
            new @string[]{"html"u8, "urlquery"u8}.slice()
        ),
        new(
            "{{.X | print}}"u8,
            ".X | print | urlquery"u8,
            new @string[]{"urlquery"u8}.slice()
        ),
        new(
            "{{.X | print | urlquery}}"u8,
            ".X | print | urlquery"u8,
            new @string[]{"urlquery"u8}.slice()
        ),
        new(
            "{{.X | urlquery}}"u8,
            ".X | html | urlquery"u8,
            new @string[]{"html"u8, "urlquery"u8}.slice()
        ),
        new(
            "{{.X | print 2 | .f 3}}"u8,
            ".X | print 2 | .f 3 | urlquery | html"u8,
            new @string[]{"urlquery"u8, "html"u8}.slice()
        ),
        new(
            "{{.X | println.x }}"u8, // covering issue 10801

            ".X | println.x | urlquery | html"u8,
            new @string[]{"urlquery"u8, "html"u8}.slice()
        ),
        new(
            "{{.X | (print 12 | println).x }}"u8, // covering issue 10801

            ".X | (print 12 | println).x | urlquery | html"u8,
            new @string[]{"urlquery"u8, "html"u8}.slice()
        ), // The following test cases ensure that the merging of internal escapers
 // with the predefined "html" and "urlquery" escapers is correct.

        new(
            "{{.X | urlquery}}"u8,
            ".X | _html_template_urlfilter | urlquery"u8,
            new @string[]{"_html_template_urlfilter"u8, "_html_template_urlnormalizer"u8}.slice()
        ),
        new(
            "{{.X | urlquery}}"u8,
            ".X | urlquery | _html_template_urlfilter | _html_template_cssescaper"u8,
            new @string[]{"_html_template_urlfilter"u8, "_html_template_cssescaper"u8}.slice()
        ),
        new(
            "{{.X | urlquery}}"u8,
            ".X | urlquery"u8,
            new @string[]{"_html_template_urlnormalizer"u8}.slice()
        ),
        new(
            "{{.X | urlquery}}"u8,
            ".X | urlquery"u8,
            new @string[]{"_html_template_urlescaper"u8}.slice()
        ),
        new(
            "{{.X | html}}"u8,
            ".X | html"u8,
            new @string[]{"_html_template_htmlescaper"u8}.slice()
        ),
        new(
            "{{.X | html}}"u8,
            ".X | html"u8,
            new @string[]{"_html_template_rcdataescaper"u8}.slice()
        )
    }.slice();
    foreach (var (i, test) in tests) {
        var (ᴛ5, ᴛ6) = text.template_package.New(testˢ).Parse(test.input);
        var tmpl = text.template_package.Must(ᴛ5, ᴛ6);
        var (action, ok) = ((~(~(~tmpl).Tree).Root).Nodes[0]._<ж<parse.ActionNode>>(ᐧ));
        if (!ok) {
            Ꮡt.Errorf("First node is not an action: %s"u8, test.input);
            continue;
        }
        var pipe = action.Value.Pipe;
        var originalIDs = new slice<@string>(len(test.ids));
        copy(originalIDs, test.ids);
        ensurePipelineContains(pipe, test.ids);
        @string got = pipe.String();
        if (got != test.output) {
            Ꮡt.Errorf("#%d: %s, %v: want\n\t%s\ngot\n\t%s"u8, i, test.input, originalIDs, test.output, got);
        }
    }
}

public static void TestEscapeMalformedPipelines(ж<testing.T> Ꮡt) {
    var tests = new @string[]{
        "{{ 0 | $ }}"u8,
        "{{ 0 | $ | urlquery }}"u8,
        "{{ 0 | (nil) }}"u8,
        "{{ 0 | (nil) | html }}"u8
    }.slice();
    foreach (var (_, test) in tests) {
        ref var b = ref heap(new bytes.Buffer(), out var Ꮡb);
        var (tmpl, err) = New(testˢ).Parse(test);
        if (err != default!) {
            Ꮡt.Errorf("failed to parse set: %q"u8, err);
        }
        err = tmpl.Execute(new template_test_package.bytes_BufferжWriter(Ꮡb), default!);
        if (err == default!) {
            Ꮡt.Errorf("Expected error for %q"u8, test);
        }
    }
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static readonly @string dangerousˢ = "dangerous"u8;

public static void TestEscapeErrorsNotIgnorable(ж<testing.T> Ꮡt) {
    ref var b = ref heap(new bytes.Buffer(), out var Ꮡb);
    var (tmpl, _) = New(dangerousˢ).Parse("<a"u8);
    var err = tmpl.Execute(new template_test_package.bytes_BufferжWriter(Ꮡb), default!);
    if (err == default!){
        Ꮡt.Errorf("Expected error"u8);
    } else 
    if (b.Len() != 0) {
        Ꮡt.Errorf("Emitted output despite escaping failure"u8);
    }
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static readonly @string defineTAEndˢ = @"{{define ""t""}}<a{{end}}"u8;

public static void TestEscapeSetErrorsNotIgnorable(ж<testing.T> Ꮡt) {
    ref var t = ref Ꮡt.DerefOrNull();

    ref var b = ref heap(new bytes.Buffer(), out var Ꮡb);
    var (tmpl, err) = New(rootˢ).Parse(defineTAEndˢ);
    if (err != default!) {
        Ꮡt.Errorf("failed to parse set: %q"u8, err);
    }
    err = tmpl.ExecuteTemplate(new template_test_package.bytes_BufferжWriter(Ꮡb), "t"u8, default!);
    if (err == default!){
        Ꮡt.Errorf("Expected error"u8);
    } else 
    if (b.Len() != 0) {
        Ꮡt.Errorf("Emitted output despite escaping failure"u8);
    }
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static readonly @string helloWorldOReillyX21ˢ = @"Hello, World & O'Reilly\x21"u8;
internal static readonly @string greetingH69Addresseeˢ3 = @"greeting=H%69&addressee=(World)"u8;

public static void TestRedundantFuncs(ж<testing.T> Ꮡt) {
    ref var t = ref Ꮡt.DerefOrNull();

    var inputs = new any[]{
        (@string)("\x00\x01\x02\x03\x04\x05\x06\x07\x08\t\n\x0b\x0c\r\x0e\x0f" + "\x10\x11\x12\x13\x14\x15\x16\x17\x18\x19\x1a\x1b\x1c\x1d\x1e\x1f" + @" !""#$%&'()*+,-./" + @"0123456789:;<=>?" + @"@ABCDEFGHIJKLMNO" + @"PQRSTUVWXYZ[\]^_" + "`abcdefghijklmno" + "pqrstuvwxyz{|}~\x7f" + "\u00A0\u0100\u2028\u2029\ufeff\ufdec\ufffd\uffff\U0001D11E" + "&amp;%22\\"),
        ((global::go.html.template_package.CSS)(@string)aHrefExampleComFooˢ),
        ((global::go.html.template_package.HTML)(@string)helloBWorldBAmpTcˢ),
        ((global::go.html.template_package.HTMLAttr)(@string)dirLtrˢ),
        ((global::go.html.template_package.JS)(@string)cAlertHelloWorldˢ),
        ((global::go.html.template_package.JSStr)(@string)helloWorldOReillyX21ˢ),
        ((global::go.html.template_package.URL)(@string)greetingH69Addresseeˢ3)
    }.slice();
    foreach (var (n0, m) in redundantFuncs) {
        var f0 = funcMap[n0]._<Funcꓸꓸꓸ<any, @string>>();
        foreach (var (n1, _) in m) {
            var f1 = funcMap[n1]._<Funcꓸꓸꓸ<any, @string>>();
            foreach (var (_, input) in inputs) {
                @string want = f0(input);
                {
                    @string got = f1(want); if (want != got) {
                        Ꮡt.Errorf("%s %s with %T %q: want\n\t%q,\ngot\n\t%q"u8, n0, n1, input, input, want, got);
                    }
                }
            }
        }
    }
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static readonly @string helloˢ = "hello"u8;

public static void TestIndirectPrint(ж<testing.T> Ꮡt) {
    ref var t = ref Ꮡt.DerefOrNull();

    ref var a = ref heap<nint>(out var Ꮡa);
    a = 3;
    var ap = Ꮡa;
    ref var b = ref heap<@string>(out var Ꮡb);
    b = helloˢ;
    ref var bp = ref heap<ж<@string>>(out var Ꮡbp);
    bp = Ꮡb;
    var bpp = Ꮡbp;
    var (ᴛ7, ᴛ8) = New("t"u8).Parse(@"{{.}}"u8);
    var tmpl = Must(ᴛ7, ᴛ8);
    ref var buf = ref heap(new strings.Builder(), out var Ꮡbuf);
    var err = tmpl.Execute(new template_test_package.strings_BuilderжWriter(Ꮡbuf), ap.OrTypedNil());
    if (err != default!){
        Ꮡt.Errorf("Unexpected error: %s"u8, err);
    } else 
    if (buf.String() != "3"u8) {
        Ꮡt.Errorf(@"Expected ""3""; got %q"u8, buf.String());
    }
    buf.Reset();
    err = tmpl.Execute(new template_test_package.strings_BuilderжWriter(Ꮡbuf), bpp.OrTypedNil());
    if (err != default!){
        Ꮡt.Errorf("Unexpected error: %s"u8, err);
    } else 
    if (buf.String() != "hello"u8) {
        Ꮡt.Errorf(@"Expected ""hello""; got %q"u8, buf.String());
    }
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static readonly @string pageˢ = "page"u8;
internal static readonly object nothingˢ = (@string)"nothing"u8;
internal static readonly object expectedErrorˢ = (@string)"expected error"u8;

// This is a test for issue 3272.
public static void TestEmptyTemplateHTML(ж<testing.T> Ꮡt) {
    var (ᴛ9, ᴛ10) = New(pageˢ).ParseFiles(os.DevNull);
    var page = Must(ᴛ9, ᴛ10);
    {
        var err = page.ExecuteTemplate(new os.FileжWriter(os.Stdout), pageˢ, nothingˢ); if (err == default!) {
            Ꮡt.Fatal(expectedErrorˢ);
        }
    }
}

public partial struct Issue7379 /*num:nint*/;

public static @string SomeMethod(this Issue7379 _, nint x) {
    return fmt.Sprintf("<%d>"u8, x);
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static readonly @string html0SomeMethodHtmlˢ = "<html>{{0 | .SomeMethod}}</html>\n"u8;

// Hoisted Go string constant (single allocation; Go keeps it in RODATA)
internal static readonly @string expectᶜ = "<html>&lt;0&gt;</html>\n"u8;

// This is a test for issue 7379: type assertion error caused panic, and then
// the code to handle the panic breaks escaping. It's hard to see the second
// problem once the first is fixed, but its fix is trivial so we let that go. See
// the discussion for issue 7379.
public static void TestPipeToMethodIsEscaped(ж<testing.T> Ꮡt) {
    var (ᴛ11, ᴛ12) = New("x"u8).Parse(html0SomeMethodHtmlˢ);
    var tmpl = Must(ᴛ11, ᴛ12);
    var tmplʗ1 = tmpl;
    @string tryExec() {
        GoFrame ᒐ = default;
        try {
            defer(() => {
                var panicValue = recover();
                if (panicValue != default!) {
                    Ꮡt.Errorf("panicked: %v\n"u8, panicValue);
                }
            }, ref ᒐ);
            ref var b = ref heap(new strings.Builder(), out var Ꮡb);
            tmplʗ1.Execute(new template_test_package.strings_BuilderжWriter(Ꮡb), ((Issue7379)0));
            return b.String();
        }
        catch (Exception ᒐex) when (GoFrame.IsPanic(ᒐex, out PanicException? ᒐp)) { GoFrame.Capture(ᒐp); return default!; }
        finally { ᒐ.Run(); }
    }
    for (nint i = 0; i < 3; i++) {
        @string str = tryExec();
        @string expect = expectᶜ;
        if (str != expect) {
            Ꮡt.Errorf("expected %q got %q"u8, expect, str);
        }
    }
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static readonly @string undefinedˢ = "undefined"u8;
internal static readonly @string incompleteˢ = "incomplete"u8;

// Unlike text/template, html/template crashed if given an incomplete
// template, that is, a template that had been named but not given any content.
// This is issue #10204.
public static void TestErrorOnUndefined(ж<testing.T> Ꮡt) {
    var tmpl = New(undefinedˢ);
    var err = tmpl.Execute(default!, default!);
    if (err == default!){
        Ꮡt.Error(expectedErrorˢ);
    } else 
    if (!strings.Contains(err.Error(), incompleteˢ)) {
        Ꮡt.Errorf("expected error about incomplete template; got %s"u8, err);
    }
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static readonly @string defineMainBodyTemplateˢ = @"{{define ""main""}}<body>{{template ""hello""}}</body>{{end}}"u8;
internal static readonly @string defineHelloHelloLadiesˢ = @"{{define ""hello""}}Hello, {{""Ladies & Gentlemen!""}}{{end}}"u8;
internal static readonly @string helloLadiesAmpGentlemenˢ = "Hello, Ladies &amp; Gentlemen!"u8;
internal static readonly @string bodyHelloLadiesAmpˢ = "<body>Hello, Ladies &amp; Gentlemen!</body>"u8;

// This covers issue #20842.
public static void TestIdempotentExecute(ж<testing.T> Ꮡt) {
    ref var t = ref Ꮡt.DerefOrNull();

    var (ᴛ13, ᴛ14) = New(""u8).Parse(defineMainBodyTemplateˢ);
    var tmpl = Must(ᴛ13, ᴛ14);
    var (ᴛ15, ᴛ16) = tmpl.Parse(defineHelloHelloLadiesˢ);
    Must(ᴛ15, ᴛ16);
    var got = @new<strings.Builder>();
    error err = default!;
    // Ensure that "hello" produces the same output when executed twice.
    @string want = helloLadiesAmpGentlemenˢ;
    for (nint i = 0; i < 2; i++) {
        err = tmpl.ExecuteTemplate(new template_test_package.strings_BuilderжWriter(got), helloˢ, default!);
        if (err != default!) {
            Ꮡt.Errorf("unexpected error: %s"u8, err);
        }
        if (got.String() != want) {
            Ꮡt.Errorf("after executing template \"hello\", got:\n\t%q\nwant:\n\t%q\n"u8, got.String(), want);
        }
        got.Reset();
    }
    // Ensure that the implicit re-execution of "hello" during the execution of
    // "main" does not cause the output of "hello" to change.
    err = tmpl.ExecuteTemplate(new template_test_package.strings_BuilderжWriter(got), mainˢ, default!);
    if (err != default!) {
        Ꮡt.Errorf("unexpected error: %s"u8, err);
    }
    // If the HTML escaper is added again to the action {{"Ladies & Gentlemen!"}},
    // we would expected to see the ampersand overescaped to "&amp;amp;".
    want = bodyHelloLadiesAmpˢ;
    if (got.String() != want) {
        Ꮡt.Errorf("after executing template \"main\", got:\n\t%q\nwant:\n\t%q\n"u8, got.String(), want);
    }
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static readonly @string aOnclickAlertAˢ = @"<a onclick=""alert('{{.}}')"">{{.}}</a>"u8;
internal static readonly object fooBarBazˢ = (@string)"foo & 'bar' & baz"u8;

public static void BenchmarkEscapedExecute(ж<testing.B> Ꮡb) {
    ref var b = ref Ꮡb.DerefOrNull();

    var (ᴛ17, ᴛ18) = New("t"u8).Parse(aOnclickAlertAˢ);
    var tmpl = Must(ᴛ17, ᴛ18);
    ref var buf = ref heap(new bytes.Buffer(), out var Ꮡbuf);
    b.ResetTimer();
    for (nint i = 0; i < b.N; i++) {
        tmpl.Execute(new template_test_package.bytes_BufferжWriter(Ꮡbuf), fooBarBazˢ);
        buf.Reset();
    }
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static readonly @string fooˢ2 = "foo"u8;
internal static readonly @string aHrefLink1Aˢ = @"<a href=""{{.}}"">link1</a>"u8;
internal static readonly @string barˢ = @"bar"u8;
internal static readonly object javascriptAlert1ˢ = (@string)"javascript:alert(1)"u8;
internal static readonly object expectedErrorExecutingT1ˢ = (@string)"expected error executing t1"u8;

// Hoisted Go string constant (single allocation; Go keeps it in RODATA)
internal static readonly @string wantErrorᶜ = @"template: ""foo"" is an incomplete or empty template"u8;

// Hoisted Go string constant (single allocation; Go keeps it in RODATA)
internal static readonly @string wantᶜ = "bar"u8;

// Covers issue 22780.
public static void TestOrphanedTemplate(ж<testing.T> Ꮡt) {
    ref var t = ref Ꮡt.DerefOrNull();

    var (ᴛ19, ᴛ20) = New(fooˢ2).Parse(aHrefLink1Aˢ);
    var t1 = Must(ᴛ19, ᴛ20);
    var (ᴛ21, ᴛ22) = t1.New(fooˢ2).Parse(barˢ);
    var t2 = Must(ᴛ21, ᴛ22);
    ref var b = ref heap(new strings.Builder(), out var Ꮡb);
    @string wantError = wantErrorᶜ;
    {
        var err = t1.Execute(new template_test_package.strings_BuilderжWriter(Ꮡb), javascriptAlert1ˢ); if (err == default!){
            Ꮡt.Fatal(expectedErrorExecutingT1ˢ);
        } else 
        {
            @string gotError = err.Error(); if (gotError != wantError) {
                Ꮡt.Fatalf("got t1 execution error:\n\t%s\nwant:\n\t%s"u8, gotError, wantError);
            }
        }
    }
    b.Reset();
    {
        var err = t2.Execute(new template_test_package.strings_BuilderжWriter(Ꮡb), default!); if (err != default!) {
            Ꮡt.Fatalf("error executing t2: %s"u8, err);
        }
    }
    @string want = wantᶜ;
    {
        @string got = b.String(); if (got != want) {
            Ꮡt.Fatalf("t2 rendered %q, want %q"u8, got, want);
        }
    }
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static readonly @string barˢ2 = "bar"u8;

// Hoisted Go string constant (single allocation; Go keeps it in RODATA)
internal static readonly @string tmplTextᶜ = @"{{.}}"u8;

// Hoisted Go string constant (single allocation; Go keeps it in RODATA)
internal static readonly @string dataᶜ = @"<baz>"u8;

// Hoisted Go string constant (single allocation; Go keeps it in RODATA)
internal static readonly @string wantᶜ1 = @"&lt;baz&gt;"u8;

// Covers issue 21844.
public static void TestAliasedParseTreeDoesNotOverescape(ж<testing.T> Ꮡt) {
    @string tmplText = tmplTextᶜ;
    @string data = dataᶜ;
    @string want = wantᶜ1;
    // Templates "foo" and "bar" both alias the same underlying parse tree.
    var (ᴛ23, ᴛ24) = New(fooˢ2).Parse(tmplText);
    var tpl = Must(ᴛ23, ᴛ24);
    {
        var (_, err) = tpl.AddParseTree(barˢ2, (~tpl).Tree); if (err != default!) {
            Ꮡt.Fatalf("AddParseTree error: %v"u8, err);
        }
    }
    ref var b1 = ref heap(new strings.Builder(), out var Ꮡb1);
    ref var b2 = ref heap(new strings.Builder(), out var Ꮡb2);
    {
        var err = tpl.ExecuteTemplate(new template_test_package.strings_BuilderжWriter(Ꮡb1), fooˢ2, data); if (err != default!) {
            Ꮡt.Fatalf(@"ExecuteTemplate failed for ""foo"": %v"u8, err);
        }
    }
    {
        var err = tpl.ExecuteTemplate(new template_test_package.strings_BuilderжWriter(Ꮡb2), barˢ2, data); if (err != default!) {
            Ꮡt.Fatalf(@"ExecuteTemplate failed for ""foo"": %v"u8, err);
        }
    }
    @string got1 = b1.String();
    @string got2 = b2.String();
    if (got1 != want) {
        Ꮡt.Fatalf(@"Template ""foo"" rendered %q, want %q"u8, got1, want);
    }
    if (got1 != got2) {
        Ꮡt.Fatalf(@"Template ""foo"" and ""bar"" rendered %q and %q respectively, expected equal values"u8, got1, got2);
    }
}

} // end template_internal_test_package
