// Copyright 2011 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.
namespace go.text;

// Tests for multiple-template parsing and execution.
using fmt = fmt_package;
using os = os_package;
using strings = strings_package;
using testing = testing_package;
using parse = go.text.template.parse_package;
using fs = go.io.fs_package;
using go.text.template;
using io = io_package;
using static go.text.template_package;

partial class template_internal_test_package {

internal const bool noError = true;
internal const bool hasError = false;

internal partial struct multiParseTest {
    internal @string name;
    internal @string input;
    internal bool ok;
    internal slice<@string> names;
    internal slice<@string> results;
}

internal static slice<multiParseTest> multiParseTests = slice<multiParseTest>([
    new("empty", "", noError,
        default,
        default),
    new("one", @"{{define ""foo""}} FOO {{end}}", noError,
        slice<@string>(["foo"]),
        slice<@string>([" FOO "])),
    new("two", @"{{define ""foo""}} FOO {{end}}{{define ""bar""}} BAR {{end}}", noError,
        slice<@string>(["foo", "bar"]),
        slice<@string>([" FOO ", " BAR "])),
    // errors
    new("missing end", @"{{define ""foo""}} FOO ", hasError,
        default,
        default),
    new("malformed name", @"{{define ""foo}} FOO ", hasError,
        default,
        default)
]);

public static void TestMultiParse(ж<testing.T> t) {
    foreach (var (_, test) in multiParseTests) {
        var (template, err) = New(rootˢ).Parse(test.input);
        switch (ᐧ) {
        case {} when err == nil && !test.ok: {
            t.Errorf("%q: expected error; got none", test.name);
            continue;
            break;
        }
        case {} when err != nil && test.ok: {
            t.Errorf("%q: unexpected error: %v", test.name, err);
            continue;
            break;
        }
        case {} when err != nil && !test.ok: {
            // expected error, got one
            if (debug.Value) {
                fmt.Printf("%s: %s\n\t%s\n"u8, test.name, test.input, err);
            }
            continue;
            break;
        }}
        if (template == nil) {
            continue;
        }
        if (len(template.tmpl) != len(test.names) + 1) { // +1 for root
            t.Errorf("%s: wrong number of templates; wanted %d got %d", test.name, len(test.names), len(template.tmpl));
            continue;
        }
        foreach (var (i, name) in test.names) {
            var (tmpl, ok) = template.tmpl[name, ꟷ];
            if (!ok) {
                t.Errorf("%s: can't find template %q", test.name, name);
                continue;
            }
            @string result = tmpl.Root.String();
            if (result != test.results[i]) {
                t.Errorf("%s=(%q): got\n\t%v\nexpected\n\t%v", test.name, test.input, result, test.results[i]);
            }
        }
    }
}

internal static slice<execTest> multiExecTests;
internal static void initᴛmultiExecTests() { multiExecTests = slice<execTest>([
    new("empty", "", "", default, true),
    new("text", "some text", "some text", default, true),
    new("invoke x", @"{{template ""x"" .SI}}", "TEXT", tVal.OrTypedNil(), true),
    new("invoke x no args", @"{{template ""x""}}", "TEXT", tVal.OrTypedNil(), true),
    new("invoke dot int", @"{{template ""dot"" .I}}", "17", tVal.OrTypedNil(), true),
    new("invoke dot []int", @"{{template ""dot"" .SI}}", "[3 4 5]", tVal.OrTypedNil(), true),
    new("invoke dotV", @"{{template ""dotV"" .U}}", "v", tVal.OrTypedNil(), true),
    new("invoke nested int", @"{{template ""nested"" .I}}", "17", tVal.OrTypedNil(), true),
    new("variable declared by template", @"{{template ""nested"" $x:=.SI}},{{index $x 1}}", "[3 4 5],4", tVal.OrTypedNil(), true),

    // User-defined function: test argument evaluator.
    new("testFunc literal", @"{{oneArg ""joe""}}", "oneArg=joe", tVal.OrTypedNil(), true),
    new("testFunc .", @"{{oneArg .}}", "oneArg=joe", (@string)"joe", true)
]); }

// These strings are also in testdata/*.
internal static ref readonly @string multiText1 => ref multiText1ᵇ;
private static readonly @string multiText1ᵇ = """

	{{define "x"}}TEXT{{end}}
	{{define "dotV"}}{{.V}}{{end}}

"""u8.Raw();

internal static ref readonly @string multiText2 => ref multiText2ᵇ;
private static readonly @string multiText2ᵇ = """

	{{define "dot"}}{{.}}{{end}}
	{{define "nested"}}{{template "dot" .}}{{end}}

"""u8.Raw();

public static void TestMultiExecute(ж<testing.T> t) {
    // Declare a couple of templates first.
    var (template, err) = New(rootˢ).Parse(multiText1);
    if (err != nil) {
        t.Fatalf("parse error for 1: %s", err);
    }
    (_, err) = template.Parse(multiText2);
    if (err != nil) {
        t.Fatalf("parse error for 2: %s", err);
    }
    testExecute(multiExecTests, template, t);
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static ref readonly @string doesNotExistˢ => ref doesNotExistˢᵇ;
private static readonly @string doesNotExistˢᵇ = "DOES NOT EXIST"u8;
internal static readonly object expectedErrorForNonˢ = (@string)"expected error for non-existent file; got none"u8;
internal static ref readonly @string testdataFile1Tmplˢ => ref testdataFile1Tmplˢᵇ;
private static readonly @string testdataFile1Tmplˢᵇ = "testdata/file1.tmpl"u8;
internal static ref readonly @string testdataFile2Tmplˢ => ref testdataFile2Tmplˢᵇ;
private static readonly @string testdataFile2Tmplˢᵇ = "testdata/file2.tmpl"u8;

public static void TestParseFiles(ж<testing.T> t) {
    var (_, err) = ParseFiles(doesNotExistˢ);
    if (err == nil) {
        t.Error(expectedErrorForNonˢ);
    }
    var template = New(rootˢ);
    (_, err) = template.ParseFiles(testdataFile1Tmplˢ, testdataFile2Tmplˢ);
    if (err != nil) {
        t.Fatalf("error parsing files: %v", err);
    }
    testExecute(multiExecTests, template, t);
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static readonly object expectedErrorForBadˢ = (@string)"expected error for bad pattern; got none"u8;
internal static ref readonly @string testdataFileTmplˢ => ref testdataFileTmplˢᵇ;
private static readonly @string testdataFileTmplˢᵇ = "testdata/file*.tmpl"u8;

public static void TestParseGlob(ж<testing.T> t) {
    var (_, err) = ParseGlob(doesNotExistˢ);
    if (err == nil) {
        t.Error(expectedErrorForNonˢ);
    }
    (_, err) = New(errorˢ).ParseGlob("[x");
    if (err == nil) {
        t.Error(expectedErrorForBadˢ);
    }
    var template = New(rootˢ);
    (_, err) = template.ParseGlob(testdataFileTmplˢ);
    if (err != nil) {
        t.Fatalf("error parsing files: %v", err);
    }
    testExecute(multiExecTests, template, t);
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static ref readonly @string testdataˢ => ref testdataˢᵇ;
private static readonly @string testdataˢᵇ = "testdata"u8;
internal static ref readonly @string file1Tmplˢ => ref file1Tmplˢᵇ;
private static readonly @string file1Tmplˢᵇ = "file1.tmpl"u8;
internal static ref readonly @string file2Tmplˢ => ref file2Tmplˢᵇ;
private static readonly @string file2Tmplˢᵇ = "file2.tmpl"u8;
internal static ref readonly @string fileTmplˢ => ref fileTmplˢᵇ;
private static readonly @string fileTmplˢᵇ = "file*.tmpl"u8;

public static void TestParseFS(ж<testing.T> t) {
    var fs = os.DirFS(testdataˢ);

    {
        var (_, err) = ParseFS(fs, doesNotExistˢ);
        if (err == nil) {
            t.Error(expectedErrorForNonˢ);
        }
    }

    {
        var template = New(rootˢ);
        var (_, err) = template.ParseFS(fs, file1Tmplˢ, file2Tmplˢ);
        if (err != nil) {
            t.Fatalf("error parsing files: %v", err);
        }
        testExecute(multiExecTests, template, t);
    }

    {
        var template = New(rootˢ);
        var (_, err) = template.ParseFS(fs, fileTmplˢ);
        if (err != nil) {
            t.Fatalf("error parsing files: %v", err);
        }
        testExecute(multiExecTests, template, t);
    }
}

// In these tests, actual content (not just template definitions) comes from the parsed files.
internal static slice<execTest> templateFileExecTests = slice<execTest>([
    new("test", @"{{template ""tmpl1.tmpl""}}{{template ""tmpl2.tmpl""}}", "template1\n\ny\ntemplate2\n\nx\n", (nint)0, true)
]);

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static ref readonly @string testdataTmpl1Tmplˢ => ref testdataTmpl1Tmplˢᵇ;
private static readonly @string testdataTmpl1Tmplˢᵇ = "testdata/tmpl1.tmpl"u8;
internal static ref readonly @string testdataTmpl2Tmplˢ => ref testdataTmpl2Tmplˢᵇ;
private static readonly @string testdataTmpl2Tmplˢᵇ = "testdata/tmpl2.tmpl"u8;

public static void TestParseFilesWithData(ж<testing.T> t) {
    var (template, err) = New(rootˢ).ParseFiles(testdataTmpl1Tmplˢ, testdataTmpl2Tmplˢ);
    if (err != nil) {
        t.Fatalf("error parsing files: %v", err);
    }
    testExecute(templateFileExecTests, template, t);
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static ref readonly @string testdataTmplTmplˢ => ref testdataTmplTmplˢᵇ;
private static readonly @string testdataTmplTmplˢᵇ = "testdata/tmpl*.tmpl"u8;

public static void TestParseGlobWithData(ж<testing.T> t) {
    var (template, err) = New(rootˢ).ParseGlob(testdataTmplTmplˢ);
    if (err != nil) {
        t.Fatalf("error parsing files: %v", err);
    }
    testExecute(templateFileExecTests, template, t);
}

internal static ref readonly @string cloneText1 => ref cloneText1ᵇ;
private static readonly @string cloneText1ᵇ = @"{{define ""a""}}{{template ""b""}}{{template ""c""}}{{end}}"u8;
internal static ref readonly @string cloneText2 => ref cloneText2ᵇ;
private static readonly @string cloneText2ᵇ = @"{{define ""b""}}b{{end}}"u8;
internal static ref readonly @string cloneText3 => ref cloneText3ᵇ;
private static readonly @string cloneText3ᵇ = @"{{define ""c""}}root{{end}}"u8;
internal static ref readonly @string cloneText4 => ref cloneText4ᵇ;
private static readonly @string cloneText4ᵇ = @"{{define ""c""}}clone{{end}}"u8;
internal static ref readonly @string cloneText5 => ref cloneText5ᵇ;
private static readonly @string cloneText5ᵇ = @"{{define ""e""}}{{.Foo}}{{end}}"u8;

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static readonly object cloneDoesNotContainRootˢ = (@string)"clone does not contain root"u8;
internal static readonly object brootˢ = (@string)"broot"u8;
internal static readonly object bcloneˢ = (@string)"bclone"u8;

public static void TestClone(ж<testing.T> t) {
    // Create some templates and clone the root.
    var (root, err) = New(rootˢ).Parse(cloneText1);
    if (err != nil) {
        t.Fatal(err);
    }
    (_, err) = root.Parse(cloneText2);
    if (err != nil) {
        t.Fatal(err);
    }
    root.Parse(cloneText5);
    root.Option(missingkeyErrorˢ);
    var (ᴛ1, ᴛ2) = root.Clone();
    var clone = Must(ᴛ1, ᴛ2);
    // Add variants to both.
    (_, err) = root.Parse(cloneText3);
    if (err != nil) {
        t.Fatal(err);
    }
    (_, err) = clone.Parse(cloneText4);
    if (err != nil) {
        t.Fatal(err);
    }
    // Verify that the clone is self-consistent.
    foreach (var (k, v) in clone.tmpl) {
        if (k == clone.name && v.tmpl[k] != clone) {
            t.Error(cloneDoesNotContainRootˢ);
        }
        if (v != v.tmpl[v.name]) {
            t.Errorf("clone does not contain self for %q", k);
        }
    }
    // Execute root.
    ref var b = ref heap(new strings.Builder(), out var Ꮡb);
    err = root.ExecuteTemplate(new template_test_package.strings_BuilderжWriter(Ꮡb), "a", (nint)0);
    if (err != nil) {
        t.Fatal(err);
    }
    if (b.String() != "broot"u8) {
        t.Errorf("expected %q got %q", brootˢ, b.String());
    }
    // Execute copy.
    b.Reset();
    err = clone.ExecuteTemplate(new template_test_package.strings_BuilderжWriter(Ꮡb), "a", (nint)0);
    if (err != nil) {
        t.Fatal(err);
    }
    if (b.String() != "bclone"u8) {
        t.Errorf("expected %q got %q", bcloneˢ, b.String());
    }
    b.Reset();
    var rootErr = root.ExecuteTemplate(new template_test_package.strings_BuilderжWriter(Ꮡb), "e", new map<@string, any>{});
    var cloneErr = clone.ExecuteTemplate(new template_test_package.strings_BuilderжWriter(Ꮡb), "e", new map<@string, any>{});
    if (cloneErr == nil) {
        t.Errorf("expected error from missing key in cloned template");
    } else {
        @string got = cloneErr.Error();
        @string want = rootErr.Error(); if (got != want) {
            t.Errorf("got %q, wan t %q", got, want);
        }
    }
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static ref readonly @string cloneText3ˢ => ref cloneText3ˢᵇ;
private static readonly @string cloneText3ˢᵇ = "cloneText3"u8;

public static void TestAddParseTree(ж<testing.T> t) {
    // Create some templates.
    var (root, err) = New(rootˢ).Parse(cloneText1);
    if (err != nil) {
        t.Fatal(err);
    }
    (_, err) = root.Parse(cloneText2);
    if (err != nil) {
        t.Fatal(err);
    }
    // Add a new parse tree.
    (var tree, err) = parse.Parse(cloneText3ˢ, cloneText3, ""u8, ""u8, (map<@string, any>)(default), builtins());
    if (err != nil) {
        t.Fatal(err);
    }
    (var added, err) = root.AddParseTree("c", tree["c"]);
    if (err != nil) {
        t.Fatal(err);
    }
    // Execute.
    ref var b = ref heap(new strings.Builder(), out var Ꮡb);
    err = added.ExecuteTemplate(new template_test_package.strings_BuilderжWriter(Ꮡb), "a", (nint)0);
    if (err != nil) {
        t.Fatal(err);
    }
    if (b.String() != "broot"u8) {
        t.Errorf("expected %q got %q", brootˢ, b.String());
    }
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static ref readonly @string defineMasterEndˢ => ref defineMasterEndˢᵇ;
private static readonly @string defineMasterEndˢᵇ = "{{define \"master\"}}{{end}}"u8;
internal static ref readonly @string masterˢ => ref masterˢᵇ;
private static readonly @string masterˢᵇ = "master"u8;

// Issue 7032
public static void TestAddParseTreeToUnparsedTemplate(ж<testing.T> t) {
    @string master = defineMasterEndˢ;
    var tmpl = New(masterˢ);
    var (tree, err) = parse.Parse(masterˢ, master, ""u8, ""u8, (map<@string, any>)(default));
    if (err != nil) {
        t.Fatalf("unexpected parse err: %v", err);
    }
    var masterTree = tree[masterˢ];
    tmpl.AddParseTree(masterˢ, masterTree); // used to panic
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static ref readonly @string tmpl1ˢ => ref tmpl1ˢᵇ;
private static readonly @string tmpl1ˢᵇ = "tmpl1"u8;
internal static ref readonly @string defineTestFooEndˢ => ref defineTestFooEndˢᵇ;
private static readonly @string defineTestFooEndˢᵇ = @"{{define ""test""}}foo{{end}}"u8;
internal static ref readonly @string defineTestBarEndˢ => ref defineTestBarEndˢᵇ;
private static readonly @string defineTestBarEndˢᵇ = @"{{define ""test""}}bar{{end}}"u8;
internal static ref readonly @string tmpl2ˢ => ref tmpl2ˢᵇ;
private static readonly @string tmpl2ˢᵇ = "tmpl2"u8;

public static void TestRedefinition(ж<testing.T> t) {
    ж<global::go.text.template_package.Template> tmpl = default;
    error err = default;
    {
        (tmpl, err) = New(tmpl1ˢ).Parse(defineTestFooEndˢ); if (err != nil) {
            t.Fatalf("parse 1: %v", err);
        }
    }
    {
        (_, err) = tmpl.Parse(defineTestBarEndˢ); if (err != nil) {
            t.Fatalf("got error %v, expected nil", err);
        }
    }
    {
        (_, err) = tmpl.New(tmpl2ˢ).Parse(defineTestBarEndˢ); if (err != nil) {
            t.Fatalf("got error %v, expected nil", err);
        }
    }
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static ref readonly @string baseˢ => ref baseˢᵇ;
private static readonly @string baseˢᵇ = "base"u8;

// Issue 10879
public static void TestEmptyTemplateCloneCrash(ж<testing.T> t) {
    var t1 = New(baseˢ);
    t1.Clone(); // used to panic
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static readonly object lookupReturnedNonNilˢ = (@string)"Lookup returned non-nil value for undefined template foo"u8;
internal static readonly object lookupReturnedNonNilˢ2 = (@string)"Lookup returned non-nil value for undefined template bar"u8;
internal static ref readonly @string defineFooTestEndˢ => ref defineFooTestEndˢᵇ;
private static readonly @string defineFooTestEndˢᵇ = @"{{define ""foo""}}test{{end}}"u8;
internal static readonly object lookupReturnedNilValueˢ = (@string)"Lookup returned nil value for defined template"u8;

// Issue 10910, 10926
public static void TestTemplateLookUp(ж<testing.T> t) {
    var t1 = New(fooˢ);
    if (t1.Lookup(fooˢ) != nil) {
        t.Error(lookupReturnedNonNilˢ);
    }
    t1.New(barˢ);
    if (t1.Lookup(barˢ) != nil) {
        t.Error(lookupReturnedNonNilˢ2);
    }
    t1.Parse(defineFooTestEndˢ);
    if (t1.Lookup(fooˢ) == nil) {
        t.Error(lookupReturnedNilValueˢ);
    }
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static ref readonly @string testˢ => ref testˢᵇ;
private static readonly @string testˢᵇ = "test"u8;
internal static readonly object definedTemplateGotNilˢ = (@string)"defined template got nil Tree"u8;
internal static readonly object undefinedTemplateGotNonˢ = (@string)"undefined template got non-nil Tree"u8;
internal static readonly object templatesIncludedˢ = (@string)"Templates included undefined template"u8;
internal static readonly object templatesDidnTIncludeˢ = (@string)"Templates didn't include defined template"u8;

public static void TestNew(ж<testing.T> t) {
    // template with same name already exists
    var (t1, _) = New(testˢ).Parse(defineTestFooEndˢ);
    var t2 = t1.New(testˢ);

    if (t1.common != t2.common) {
        t.Errorf("t1 & t2 didn't share common struct; got %v != %v", t1.common.OrTypedNil(), t2.common.OrTypedNil());
    }
    if (t1.Tree == nil) {
        t.Error(definedTemplateGotNilˢ);
    }
    if (t2.Tree != nil) {
        t.Error(undefinedTemplateGotNonˢ);
    }

    var containsT1 = false;
    foreach (var (_, tmpl) in t1.Templates()) {
        if (tmpl == t2) {
            t.Error(templatesIncludedˢ);
        }
        if (tmpl == t1) {
            containsT1 = true;
        }
    }
    if (!containsT1) {
        t.Error(templatesDidnTIncludeˢ);
    }
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static ref readonly @string defineTestEndˢ => ref defineTestEndˢᵇ;
private static readonly @string defineTestEndˢᵇ = @"{{define ""test""}}{{end}}"u8;
internal static ref readonly @string defineTestThisIsACommentˢ => ref defineTestThisIsACommentˢᵇ;
private static readonly @string defineTestThisIsACommentˢᵇ = @"{{define ""test""}}{{/* this is a comment */}}{{end}}"u8;

public static void TestParse(ж<testing.T> t) {
    // In multiple calls to Parse with the same receiver template, only one call
    // can contain text other than space, comments, and template definitions
    var t1 = New(testˢ);
    {
        var (_, err) = t1.Parse(defineTestEndˢ); if (err != nil) {
            t.Fatalf("parsing test: %s", err);
        }
    }
    {
        var (_, err) = t1.Parse(defineTestThisIsACommentˢ); if (err != nil) {
            t.Fatalf("parsing test: %s", err);
        }
    }
    {
        var (_, err) = t1.Parse(defineTestFooEndˢ); if (err != nil) {
            t.Fatalf("parsing test: %s", err);
        }
    }
}

internal partial struct TestEmptyTemplate_cases /*dyn*/ {
    internal slice<@string> defn;
    internal @string @in;
    internal @string want;
}

public static void TestEmptyTemplate(ж<testing.T> t) {
    var cases = slice<TestEmptyTemplate_cases>([
        new(slice<@string>(["x", "y"]), "", "y"),
        new(slice<@string>([""]), "once", ""),
        new(slice<@string>(["", ""]), "twice", ""),
        new(slice<@string>(["{{.}}", "{{.}}"]), "twice", "twice"),
        new(slice<@string>(["{{/* a comment */}}", "{{/* a comment */}}"]), "comment", ""),
        new(slice<@string>(["{{.}}", ""]), "twice", "")
    ]);

    foreach (var (i, c) in cases) {
        var root = New(rootˢ);

        ж<global::go.text.template_package.Template> m = default;
        error err = default;
        foreach (var (_, d) in c.defn) {
            (m, err) = root.New(c.@in).Parse(d);
            if (err != nil) {
                t.Fatal(err);
            }
        }
        var buf = Ꮡ(new strings.Builder());
        {
            var errΔ1 = m.Execute(new template_test_package.strings_BuilderжWriter(buf), c.@in); if (errΔ1 != nil) {
                t.Error(i, errΔ1);
                continue;
            }
        }
        if (buf.String() != c.want) {
            t.Errorf("expected string %q: got %q", c.want, buf.String());
        }
    }
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static ref readonly @string titleXhtmlˢ => ref titleXhtmlˢᵇ;
private static readonly @string titleXhtmlˢᵇ = "title.xhtml"u8;
internal static ref readonly @string templateXhtmlˢ => ref templateXhtmlˢᵇ;
private static readonly @string templateXhtmlˢᵇ = @"{{template ""xhtml"" .}}"u8;
internal static readonly object stylesheetˢ = (@string)"stylesheet"u8;

// Issue 19249 was a regression in 1.8 caused by the handling of empty
// templates added in that release, which got different answers depending
// on the order templates appeared in the internal map.
public static void TestIssue19294(ж<testing.T> t) {
    // The empty block in "xhtml" should be replaced during execution
    // by the contents of "stylesheet", but if the internal map associating
    // names with templates is built in the wrong order, the empty block
    // looks non-empty and this doesn't happen.
    map<@string, @string> inlined = new map<@string, @string>{
        ["stylesheet"] = @"{{define ""stylesheet""}}stylesheet{{end}}",
        ["xhtml"] = @"{{block ""stylesheet"" .}}{{end}}"
    };
    var all = slice<@string>(["stylesheet", "xhtml"]);
    for (nint i = 0; i < 100; i++) {
        var (res, err) = New(titleXhtmlˢ).Parse(templateXhtmlˢ);
        if (err != nil) {
            t.Fatal(err);
        }
        foreach (var (_, name) in all) {
            var (_, errΔ1) = res.New(name).Parse(inlined[name]);
            if (errΔ1 != nil) {
                t.Fatal(errΔ1);
            }
        }
        ref var buf = ref heap(new strings.Builder(), out var Ꮡbuf);
        res.Execute(new template_test_package.strings_BuilderжWriter(Ꮡbuf), (nint)0);
        if (buf.String() != "stylesheet"u8) {
            t.Fatalf("iteration %d: got %q; expected %q", i, buf.String(), stylesheetˢ);
        }
    }
}

// Issue 48436
public static void TestAddToZeroTemplate(ж<testing.T> t) {
    var (tree, err) = parse.Parse("c", cloneText3, ""u8, ""u8, (map<@string, any>)(default), builtins());
    if (err != nil) {
        t.Fatal(err);
    }
    ref var tmpl = ref heap(new global::go.text.template_package.Template(), out var Ꮡtmpl);
    Ꮡtmpl.AddParseTree("x", tree["c"]);
}

} // end template_internal_test_package
