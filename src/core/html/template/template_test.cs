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
internal static ref readonly @string nameˢ => ref nameˢᵇ;
private static readonly @string nameˢᵇ = "name"u8;

// Hoisted Go string constant (single allocation; Go keeps it in RODATA)
internal static ref readonly @string wantᶜ => ref wantᶜᵇ;
private static readonly @string wantᶜᵇ = "stuff"u8;

public static void TestTemplateClone(ж<testing.T> t) {
    // https://golang.org/issue/12996
    var orig = New(nameˢ);
    var (clone, err) = orig.Clone();
    if (err != nil) {
        t.Fatal(err);
    }
    if (len(clone.Templates()) != len(orig.Templates())) {
        t.Fatalf("Invalid length of t.Clone().Templates()");
    }

    @string want = wantᶜ;
    var (ᴛ1, ᴛ2) = clone.Parse(want);
    var parsed = Must(ᴛ1, ᴛ2);
    ref var buf = ref heap(new strings.Builder(), out var Ꮡbuf);
    err = parsed.Execute(new template_test_package.strings_BuilderжWriter(Ꮡbuf), default);
    if (err != nil) {
        t.Fatal(err);
    }
    @string got = buf.String(); if (got != want) {
        t.Fatalf("got %q; want %q", got, want);
    }
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static ref readonly @string fooˢ => ref fooˢᵇ;
private static readonly @string fooˢᵇ = @"foo"u8;
internal static ref readonly @string fooˢ2 => ref fooˢ2ᵇ;
private static readonly @string fooˢ2ᵇ = "foo"u8;
internal static ref readonly @string barˢ => ref barˢᵇ;
private static readonly @string barˢᵇ = @"bar"u8;

public static void TestRedefineNonEmptyAfterExecution(ж<testing.T> t) {
    var c = newTestCase(t);
    c.mustParse(c.root, fooˢ);
    c.mustExecute(c.root, default, fooˢ2);
    c.mustNotParse(c.root, barˢ);
}

public static void TestRedefineEmptyAfterExecution(ж<testing.T> t) {
    var c = newTestCase(t);
    c.mustParse(c.root, @""u8);
    c.mustExecute(c.root, default, ""u8);
    c.mustNotParse(c.root, fooˢ);
    c.mustExecute(c.root, default, ""u8);
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static ref readonly @string ifTemplateXEndDefineXFooˢ => ref ifTemplateXEndDefineXFooˢᵇ;
private static readonly @string ifTemplateXEndDefineXFooˢᵇ = @"{{if .}}<{{template ""X""}}>{{end}}{{define ""X""}}foo{{end}}"u8;
internal static ref readonly @string defineXBarEndˢ => ref defineXBarEndˢᵇ;
private static readonly @string defineXBarEndˢᵇ = @"{{define ""X""}}bar{{end}}"u8;
internal static ref readonly @string ltFooˢ => ref ltFooˢᵇ;
private static readonly @string ltFooˢᵇ = "&lt;foo>"u8;

public static void TestRedefineAfterNonExecution(ж<testing.T> t) {
    var c = newTestCase(t);
    c.mustParse(c.root, ifTemplateXEndDefineXFooˢ);
    c.mustExecute(c.root, (nint)0, ""u8);
    c.mustNotParse(c.root, defineXBarEndˢ);
    c.mustExecute(c.root, (nint)1, ltFooˢ);
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static ref readonly @string templateXDefineXFooEndˢ => ref templateXDefineXFooEndˢᵇ;
private static readonly @string templateXDefineXFooEndˢᵇ = @"<{{template ""X"" .}}>{{define ""X""}}foo{{end}}"u8;

public static void TestRedefineAfterNamedExecution(ж<testing.T> t) {
    var c = newTestCase(t);
    c.mustParse(c.root, templateXDefineXFooEndˢ);
    c.mustExecute(c.root, default, ltFooˢ);
    c.mustNotParse(c.root, defineXBarEndˢ);
    c.mustExecute(c.root, default, ltFooˢ);
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static ref readonly @string defineXFooEndˢ => ref defineXFooEndˢᵇ;
private static readonly @string defineXFooEndˢᵇ = @"{{define ""X""}}foo{{end}}"u8;

public static void TestRedefineNestedByNameAfterExecution(ж<testing.T> t) {
    var c = newTestCase(t);
    c.mustParse(c.root, defineXFooEndˢ);
    c.mustExecute(c.lookup("X"), default, fooˢ2);
    c.mustNotParse(c.root, defineXBarEndˢ);
    c.mustExecute(c.lookup("X"), default, fooˢ2);
}

public static void TestRedefineNestedByTemplateAfterExecution(ж<testing.T> t) {
    var c = newTestCase(t);
    c.mustParse(c.root, defineXFooEndˢ);
    c.mustExecute(c.lookup("X"), default, fooˢ2);
    c.mustNotParse(c.lookup("X"), barˢ);
    c.mustExecute(c.lookup("X"), default, fooˢ2);
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static ref readonly @string htmlAHrefTemplateXDefineˢ => ref htmlAHrefTemplateXDefineˢᵇ;
private static readonly @string htmlAHrefTemplateXDefineˢᵇ = @"<html><a href=""{{template ""X""}}"">{{define ""X""}}{{end}}"u8;
internal static ref readonly @string htmlAHrefˢ => ref htmlAHrefˢᵇ;
private static readonly @string htmlAHrefˢᵇ = @"<html><a href="""">"u8;
internal static ref readonly @string defineXBarBazEndˢ => ref defineXBarBazEndˢᵇ;
private static readonly @string defineXBarBazEndˢᵇ = @"{{define ""X""}}"" bar=""baz{{end}}"u8;

public static void TestRedefineSafety(ж<testing.T> t) {
    var c = newTestCase(t);
    c.mustParse(c.root, htmlAHrefTemplateXDefineˢ);
    c.mustExecute(c.root, default, htmlAHrefˢ);
    // Note: Every version of Go prior to Go 1.8 accepted the redefinition of "X"
    // on the next line, but luckily kept it from being used in the outer template.
    // Now we reject it, which makes clearer that we're not going to use it.
    c.mustNotParse(c.root, defineXBarBazEndˢ);
    c.mustExecute(c.root, default, htmlAHrefˢ);
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static ref readonly @string templateXDefineXEndˢ => ref templateXDefineXEndˢᵇ;
private static readonly @string templateXDefineXEndˢᵇ = @"{{template ""X""}}{{.}}{{define ""X""}}{{end}}"u8;
internal static ref readonly @string defineXScriptEndˢ => ref defineXScriptEndˢᵇ;
private static readonly @string defineXScriptEndˢᵇ = @"{{define ""X""}}<script>{{end}}"u8;

public static void TestRedefineTopUse(ж<testing.T> t) {
    var c = newTestCase(t);
    c.mustParse(c.root, templateXDefineXEndˢ);
    c.mustExecute(c.root, (nint)42, @"42");
    c.mustNotParse(c.root, defineXScriptEndˢ);
    c.mustExecute(c.root, (nint)42, @"42");
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static ref readonly @string noTemplateˢ => ref noTemplateˢᵇ;
private static readonly @string noTemplateˢᵇ = "no.template"u8;
internal static ref readonly @string executeˢ => ref executeˢᵇ;
private static readonly @string executeˢᵇ = "Execute"u8;
internal static ref readonly @string noTemplateˢ2 => ref noTemplateˢ2ᵇ;
private static readonly @string noTemplateˢ2ᵇ = "*.no.template"u8;

public static void TestRedefineOtherParsers(ж<testing.T> t) {
    var c = newTestCase(t);
    c.mustParse(c.root, @""u8);
    c.mustExecute(c.root, default, @""u8);
    {
        var (_, err) = c.root.ParseFiles(noTemplateˢ); if (err == nil || !strings.Contains(err.Error(), executeˢ)) {
            t.Errorf("ParseFiles: %v\nwanted error about already having Executed", err);
        }
    }
    {
        var (_, err) = c.root.ParseGlob(noTemplateˢ2); if (err == nil || !strings.Contains(err.Error(), executeˢ)) {
            t.Errorf("ParseGlob: %v\nwanted error about already having Executed", err);
        }
    }
    {
        var (_, err) = c.root.AddParseTree("t1", c.root.Tree); if (err == nil || !strings.Contains(err.Error(), executeˢ)) {
            t.Errorf("AddParseTree: %v\nwanted error about already having Executed", err);
        }
    }
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static ref readonly @string print1234Print0x01E0p02ˢ => ref print1234Print0x01E0p02ˢᵇ;
private static readonly @string print1234Print0x01E0p02ˢᵇ = @"{{print 1_2.3_4}} {{print 0x0_1.e_0p+02}}"u8;

public static void TestNumbers(ж<testing.T> t) {
    var c = newTestCase(t);
    c.mustParse(c.root, print1234Print0x01E0p02ˢ);
    c.mustExecute(c.root, default, "12.34 7.5");
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static ref readonly @string jsStringIsJsonStringˢ => ref jsStringIsJsonStringˢᵇ;
private static readonly @string jsStringIsJsonStringˢᵇ = "JS string is JSON string"u8;

internal partial struct TestStringsInScriptsWithJsonContentTypeAreCorrectlyEscaped_tests /*dyn*/ {
    internal @string name, @in;
}

// Hoisted Go string constant (single allocation; Go keeps it in RODATA)
internal static ref readonly @string prefixᶜ => ref prefixᶜᵇ;
private static readonly @string prefixᶜᵇ = @"<script type=""application/ld+json"">"u8;

// Hoisted Go string constant (single allocation; Go keeps it in RODATA)
internal static ref readonly @string suffixᶜ => ref suffixᶜᵇ;
private static readonly @string suffixᶜᵇ = @"</script>"u8;

// Hoisted Go string constant (single allocation; Go keeps it in RODATA)
internal static ref readonly @string templᶜ => ref templᶜᵇ;
private static readonly @string templᶜᵇ = "<script type=\"application/ld+json\">\"{{.}}\"</script>";

public static void TestStringsInScriptsWithJsonContentTypeAreCorrectlyEscaped(ж<testing.T> t) {
    // See #33671 and #37634 for more context on this.
    var tests = slice<TestStringsInScriptsWithJsonContentTypeAreCorrectlyEscaped_tests>([
        new("empty", ""),
        new("invalid", (@string)(rune)(-1)),
        new("null", "\u0000"),
        new("unit separator", "\u001F"),
        new("tab", "\t"),
        new("gt and lt", "<>"),
        new("quotes", @"'"""),
        new("ASCII letters", "ASCII letters"),
        new("Unicode", "ʕ⊙ϖ⊙ʔ"),
        new("Pizza", "🍕")
    ]);
    @string prefix = prefixᶜ;
    @string suffix = suffixᶜ;
    @string templ = templᶜ;
    var (ᴛ3, ᴛ4) = New(jsStringIsJsonStringˢ).Parse(templ);
    var tpl = Must(ᴛ3, ᴛ4);
    foreach (var (_, vᴛ1) in range(tests)) {
        ref var tt = ref heap(new TestStringsInScriptsWithJsonContentTypeAreCorrectlyEscaped_tests(), out var Ꮡtt);
        tt = vᴛ1;

        var tplʗ1 = tpl;
        var ttʗ1 = tt;
        t.Run(tt.name, (ж<testing.T> tΔ1) => {
            ref var buf = ref heap(new bytes.Buffer(), out var Ꮡbuf);
            {
                var err = tplʗ1.Execute(new template_test_package.bytes_BufferжWriter(Ꮡbuf), ttʗ1.@in); if (err != nil) {
                    tΔ1.Fatalf("Cannot render template: %v", err);
                }
            }
            var trimmed = bytes.TrimSuffix(bytes.TrimPrefix(buf.Bytes(), slice<byte>(prefix)), slice<byte>(suffix));
            ref var got = ref heap(new @string(), out var Ꮡgot);
            {
                var err = json.Unmarshal(trimmed, Ꮡgot); if (err != nil) {
                    tΔ1.Fatalf("Cannot parse JS string %q as JSON: %v", trimmed.slice(1, len(trimmed) - 1), err);
                }
            }
            if (got != ttʗ1.@in) {
                tΔ1.Errorf("Serialization changed the string value: got %q want %q", got, ttʗ1.@in);
            }
        });
    }
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static ref readonly @string rootˢ => ref rootˢᵇ;
private static readonly @string rootˢᵇ = "root"u8;
internal static ref readonly @string aComment1AnotherCommentˢ => ref aComment1AnotherCommentˢᵇ;
private static readonly @string aComment1AnotherCommentˢᵇ = "{{/* A comment */}}{{ 1 }}{{/* Another comment */}}"u8;

public static void TestSkipEscapeComments(ж<testing.T> t) {
    var c = newTestCase(t);
    var tr = parse.New(rootˢ);
    tr.Mode = parse.ParseComments;
    var (newT, err) = tr.Parse(aComment1AnotherCommentˢ, ""u8, ""u8, new map<@string, ж<parse.Tree>>());
    if (err != nil) {
        t.Fatalf("Cannot parse template text: %v", err);
    }
    (c.root, err) = c.root.AddParseTree(rootˢ, newT);
    if (err != nil) {
        t.Fatalf("Cannot add parse tree to template: %v", err);
    }
    c.mustExecute(c.root, default, "1");
}

partial struct testCase {
    internal ж<testing.T> t;
    internal ж<template.Template> root;
}

internal static ж<testCase> newTestCase(ж<testing.T> t) {
    return Ꮡ(new testCase(
        t: t,
        root: New(rootˢ)
    ));
}

internal static ж<template.Template> lookup(this ref testCase c, @string name) {
    return c.root.Lookup(name);
}

internal static partial void mustParse(this ref testCase c, ж<template.Template> Ꮡt, @string text) {
    ref var t = ref Ꮡt.DerefOrNull();

    var (_, err) = Ꮡt.Parse(text);
    if (err != nil) {
        c.t.Fatalf("parse: %v", err);
    }
}

internal static partial void mustNotParse(this ref testCase c, ж<template.Template> Ꮡt, @string text) {
    ref var t = ref Ꮡt.DerefOrNull();

    var (_, err) = Ꮡt.Parse(text);
    if (err == nil) {
        c.t.Fatalf("parse: unexpected success");
    }
}

internal static void mustExecute(this ref testCase c, ж<template.Template> Ꮡt, any val, @string want) {
    ref var t = ref Ꮡt.DerefOrNull();

    ref var buf = ref heap(new strings.Builder(), out var Ꮡbuf);
    var err = Ꮡt.Execute(new template_test_package.strings_BuilderжWriter(Ꮡbuf), val);
    if (err != nil) {
        c.t.Fatalf("execute: %v", err);
    }
    if (buf.String() != want) {
        c.t.Fatalf("template output:\n%s\nwant:\n%s", buf.String(), want);
    }
}

} // end template_test_package
