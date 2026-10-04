// Copyright 2016 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.
namespace go.html;

using bytes = bytes_package;
using json = encoding.json_package;
using static go.html.template_package;
using strings = strings_package;
using testing = testing_package;
using parse = text.template.parse_package;
using encoding;
using go.html;
using io = io_package;
using static go.html.template_internal_test_package;
using template = go.html.template_package;
using text.template;

partial class template_test_package {

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static readonly @string nameˢ = "name"u8;

// Hoisted Go string constant (single allocation; Go keeps it in RODATA)
internal static readonly @string wantᶜ = "stuff"u8;

public static void TestTemplateClone(ж<testing.T> Ꮡt) {
    ref var t = ref Ꮡt.DerefOrNull();

    // https://golang.org/issue/12996
    var orig = New(nameˢ);
    var (clone, err) = orig.Clone();
    if (err != default!) {
        Ꮡt.Fatal(err);
    }
    if (len(clone.Templates()) != len(orig.Templates())) {
        Ꮡt.Fatalf("Invalid length of t.Clone().Templates()"u8);
    }
    @string want = wantᶜ;
    var (ᴛ1, ᴛ2) = clone.Parse(want);
    var parsed = Must(ᴛ1, ᴛ2);
    ref var buf = ref heap(new strings.Builder(), out var Ꮡbuf);
    err = parsed.Execute(new template_test_package.strings_BuilderжWriter(Ꮡbuf), default!);
    if (err != default!) {
        Ꮡt.Fatal(err);
    }
    {
        @string got = buf.String(); if (got != want) {
            Ꮡt.Fatalf("got %q; want %q"u8, got, want);
        }
    }
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static readonly @string fooˢ = @"foo"u8;
internal static readonly @string fooˢ2 = "foo"u8;
internal static readonly @string barˢ = @"bar"u8;

public static void TestRedefineNonEmptyAfterExecution(ж<testing.T> Ꮡt) {
    var c = newTestCase(Ꮡt);
    c.mustParse((~c).root, fooˢ);
    c.mustExecute((~c).root, default!, fooˢ2);
    c.mustNotParse((~c).root, barˢ);
}

public static void TestRedefineEmptyAfterExecution(ж<testing.T> Ꮡt) {
    var c = newTestCase(Ꮡt);
    c.mustParse((~c).root, @""u8);
    c.mustExecute((~c).root, default!, ""u8);
    c.mustNotParse((~c).root, fooˢ);
    c.mustExecute((~c).root, default!, ""u8);
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static readonly @string ifTemplateXEndDefineXFooˢ = @"{{if .}}<{{template ""X""}}>{{end}}{{define ""X""}}foo{{end}}"u8;
internal static readonly @string defineXBarEndˢ = @"{{define ""X""}}bar{{end}}"u8;
internal static readonly @string ltFooˢ = "&lt;foo>"u8;

public static void TestRedefineAfterNonExecution(ж<testing.T> Ꮡt) {
    var c = newTestCase(Ꮡt);
    c.mustParse((~c).root, ifTemplateXEndDefineXFooˢ);
    c.mustExecute((~c).root, (nint)(0), ""u8);
    c.mustNotParse((~c).root, defineXBarEndˢ);
    c.mustExecute((~c).root, (nint)(1), ltFooˢ);
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static readonly @string templateXDefineXFooEndˢ = @"<{{template ""X"" .}}>{{define ""X""}}foo{{end}}"u8;

public static void TestRedefineAfterNamedExecution(ж<testing.T> Ꮡt) {
    var c = newTestCase(Ꮡt);
    c.mustParse((~c).root, templateXDefineXFooEndˢ);
    c.mustExecute((~c).root, default!, ltFooˢ);
    c.mustNotParse((~c).root, defineXBarEndˢ);
    c.mustExecute((~c).root, default!, ltFooˢ);
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static readonly @string defineXFooEndˢ = @"{{define ""X""}}foo{{end}}"u8;

public static void TestRedefineNestedByNameAfterExecution(ж<testing.T> Ꮡt) {
    var c = newTestCase(Ꮡt);
    c.mustParse((~c).root, defineXFooEndˢ);
    c.mustExecute(c.lookup("X"u8), default!, fooˢ2);
    c.mustNotParse((~c).root, defineXBarEndˢ);
    c.mustExecute(c.lookup("X"u8), default!, fooˢ2);
}

public static void TestRedefineNestedByTemplateAfterExecution(ж<testing.T> Ꮡt) {
    var c = newTestCase(Ꮡt);
    c.mustParse((~c).root, defineXFooEndˢ);
    c.mustExecute(c.lookup("X"u8), default!, fooˢ2);
    c.mustNotParse(c.lookup("X"u8), barˢ);
    c.mustExecute(c.lookup("X"u8), default!, fooˢ2);
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static readonly @string htmlAHrefTemplateXDefineˢ = @"<html><a href=""{{template ""X""}}"">{{define ""X""}}{{end}}"u8;
internal static readonly @string htmlAHrefˢ = @"<html><a href="""">"u8;
internal static readonly @string defineXBarBazEndˢ = @"{{define ""X""}}"" bar=""baz{{end}}"u8;

public static void TestRedefineSafety(ж<testing.T> Ꮡt) {
    var c = newTestCase(Ꮡt);
    c.mustParse((~c).root, htmlAHrefTemplateXDefineˢ);
    c.mustExecute((~c).root, default!, htmlAHrefˢ);
    // Note: Every version of Go prior to Go 1.8 accepted the redefinition of "X"
    // on the next line, but luckily kept it from being used in the outer template.
    // Now we reject it, which makes clearer that we're not going to use it.
    c.mustNotParse((~c).root, defineXBarBazEndˢ);
    c.mustExecute((~c).root, default!, htmlAHrefˢ);
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static readonly @string templateXDefineXEndˢ = @"{{template ""X""}}{{.}}{{define ""X""}}{{end}}"u8;
internal static readonly @string defineXScriptEndˢ = @"{{define ""X""}}<script>{{end}}"u8;

public static void TestRedefineTopUse(ж<testing.T> Ꮡt) {
    var c = newTestCase(Ꮡt);
    c.mustParse((~c).root, templateXDefineXEndˢ);
    c.mustExecute((~c).root, (nint)(42), @"42"u8);
    c.mustNotParse((~c).root, defineXScriptEndˢ);
    c.mustExecute((~c).root, (nint)(42), @"42"u8);
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static readonly @string noTemplateˢ = "no.template"u8;
internal static readonly @string executeˢ = "Execute"u8;
internal static readonly @string noTemplateˢ2 = "*.no.template"u8;

public static void TestRedefineOtherParsers(ж<testing.T> Ꮡt) {
    var c = newTestCase(Ꮡt);
    c.mustParse((~c).root, @""u8);
    c.mustExecute((~c).root, default!, @""u8);
    {
        var (_, err) = (~c).root.ParseFiles(noTemplateˢ); if (err == default! || !strings.Contains(err.Error(), executeˢ)) {
            Ꮡt.Errorf("ParseFiles: %v\nwanted error about already having Executed"u8, err);
        }
    }
    {
        var (_, err) = (~c).root.ParseGlob(noTemplateˢ2); if (err == default! || !strings.Contains(err.Error(), executeˢ)) {
            Ꮡt.Errorf("ParseGlob: %v\nwanted error about already having Executed"u8, err);
        }
    }
    {
        var (_, err) = (~c).root.AddParseTree("t1"u8, (~(~c).root).Tree); if (err == default! || !strings.Contains(err.Error(), executeˢ)) {
            Ꮡt.Errorf("AddParseTree: %v\nwanted error about already having Executed"u8, err);
        }
    }
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static readonly @string print1234Print0x01E0p02ˢ = @"{{print 1_2.3_4}} {{print 0x0_1.e_0p+02}}"u8;

public static void TestNumbers(ж<testing.T> Ꮡt) {
    var c = newTestCase(Ꮡt);
    c.mustParse((~c).root, print1234Print0x01E0p02ˢ);
    c.mustExecute((~c).root, default!, "12.34 7.5"u8);
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static readonly @string jsStringIsJsonStringˢ = "JS string is JSON string"u8;

[GoType("dyn")] internal partial struct TestStringsInScriptsWithJsonContentTypeAreCorrectlyEscaped_tests {
    internal @string name, @in;
}

// Hoisted Go string constant (single allocation; Go keeps it in RODATA)
internal static readonly @string prefixᶜ = @"<script type=""application/ld+json"">"u8;

// Hoisted Go string constant (single allocation; Go keeps it in RODATA)
internal static readonly @string suffixᶜ = @"</script>"u8;

// Hoisted Go string constant (single allocation; Go keeps it in RODATA)
internal static readonly @string templᶜ = "<script type=\"application/ld+json\">\"{{.}}\"</script>";

public static void TestStringsInScriptsWithJsonContentTypeAreCorrectlyEscaped(ж<testing.T> Ꮡt) {
    ref var t = ref Ꮡt.DerefOrNull();

    // See #33671 and #37634 for more context on this.
    var tests = new TestStringsInScriptsWithJsonContentTypeAreCorrectlyEscaped_tests[]{
        new("empty"u8, ""u8),
        new("invalid"u8, ((@string)(rune)(-1))),
        new("null"u8, "\u0000"u8),
        new("unit separator"u8, "\u001F"u8),
        new("tab"u8, "\t"u8),
        new("gt and lt"u8, "<>"u8),
        new("quotes"u8, @"'"""u8),
        new("ASCII letters"u8, "ASCII letters"u8),
        new("Unicode"u8, "ʕ⊙ϖ⊙ʔ"u8),
        new("Pizza"u8, "🍕"u8)
    }.slice();
    @string prefix = prefixᶜ;
    @string suffix = suffixᶜ;
    @string templ = templᶜ;
    var (ᴛ3, ᴛ4) = New(jsStringIsJsonStringˢ).Parse(templ);
    var tpl = Must(ᴛ3, ᴛ4);
    foreach (var (_, vᴛ1) in tests) {
        ref var tt = ref heap(new TestStringsInScriptsWithJsonContentTypeAreCorrectlyEscaped_tests(), out var Ꮡtt);
        tt = vᴛ1;

        var tplʗ1 = tpl;
        var ttʗ1 = tt;
        Ꮡt.Run(tt.name, (ж<testing.T> tΔ1) => {
            ref var buf = ref heap(new bytes.Buffer(), out var Ꮡbuf);
            {
                var err = tplʗ1.Execute(new template_test_package.bytes_BufferжWriter(Ꮡbuf), ttʗ1.@in); if (err != default!) {
                    tΔ1.Fatalf("Cannot render template: %v"u8, err);
                }
            }
            var trimmed = bytes.TrimSuffix(bytes.TrimPrefix(buf.Bytes(), slice<byte>(prefix)), slice<byte>(suffix));
            ref var got = ref heap(new @string(), out var Ꮡgot);
            {
                var err = json.Unmarshal(trimmed, Ꮡgot); if (err != default!) {
                    tΔ1.Fatalf("Cannot parse JS string %q as JSON: %v"u8, trimmed.slice(1, len(trimmed) - 1), err);
                }
            }
            if (got != ttʗ1.@in) {
                tΔ1.Errorf("Serialization changed the string value: got %q want %q"u8, got, ttʗ1.@in);
            }
        });
    }
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static readonly @string rootˢ = "root"u8;
internal static readonly @string aComment1AnotherCommentˢ = "{{/* A comment */}}{{ 1 }}{{/* Another comment */}}"u8;

public static void TestSkipEscapeComments(ж<testing.T> Ꮡt) {
    var c = newTestCase(Ꮡt);
    var tr = parse.New(rootˢ);
    tr.Value.Mode = parse.ParseComments;
    var (newT, err) = tr.Parse(aComment1AnotherCommentˢ, ""u8, ""u8, new map<@string, ж<parse.Tree>>());
    if (err != default!) {
        Ꮡt.Fatalf("Cannot parse template text: %v"u8, err);
    }
    (c.Value.root, err) = (~c).root.AddParseTree(rootˢ, newT);
    if (err != default!) {
        Ꮡt.Fatalf("Cannot add parse tree to template: %v"u8, err);
    }
    c.mustExecute((~c).root, default!, "1"u8);
}

[GoType] partial struct testCase {
    internal ж<testing.T> t;
    internal ж<template.Template> root;
}

internal static ж<testCase> newTestCase(ж<testing.T> Ꮡt) {
    return Ꮡ(new testCase(
        t: Ꮡt,
        root: New(rootˢ)
    ));
}

[GoRecv] internal static ж<template.Template> lookup(this ref testCase c, @string name) {
    return c.root.Lookup(name);
}

[GoRecv] internal static void mustParse(this ref testCase c, ж<template.Template> Ꮡt, @string text) {
    ref var t = ref Ꮡt.DerefOrNull();

    var (_, err) = Ꮡt.Parse(text);
    if (err != default!) {
        c.t.Fatalf("parse: %v"u8, err);
    }
}

[GoRecv] internal static void mustNotParse(this ref testCase c, ж<template.Template> Ꮡt, @string text) {
    ref var t = ref Ꮡt.DerefOrNull();

    var (_, err) = Ꮡt.Parse(text);
    if (err == default!) {
        c.t.Fatalf("parse: unexpected success"u8);
    }
}

[GoRecv] internal static void mustExecute(this ref testCase c, ж<template.Template> Ꮡt, any val, @string want) {
    ref var t = ref Ꮡt.DerefOrNull();

    ref var buf = ref heap(new strings.Builder(), out var Ꮡbuf);
    var err = Ꮡt.Execute(new template_test_package.strings_BuilderжWriter(Ꮡbuf), val);
    if (err != default!) {
        c.t.Fatalf("execute: %v"u8, err);
    }
    if (buf.String() != want) {
        c.t.Fatalf("template output:\n%s\nwant:\n%s"u8, buf.String(), want);
    }
}

} // end template_test_package
