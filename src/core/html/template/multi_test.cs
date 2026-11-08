// Copyright 2011 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.
// Tests for multiple-template execution, copied from text/template.
namespace go.html;

using zip = archive.zip_package;
using os = os_package;
using strings = strings_package;
using testing = testing_package;
using parse = text.template.parse_package;
using archive;
using fs = go.io.fs_package;
using io = io_package;
using static go.html.template_package;
using text.template;

partial class template_internal_test_package {

internal static slice<execTest> multiExecTests;
internal static void initᴛmultiExecTests() { multiExecTests = [
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
]; }

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
internal static slice<execTest> templateFileExecTests = [
    new("test", @"{{template ""tmpl1.tmpl""}}{{template ""tmpl2.tmpl""}}", "template1\n\ny\ntemplate2\n\nx\n", (nint)0, true)
];

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

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static ref readonly @string testdataFsZipˢ => ref testdataFsZipˢᵇ;
private static readonly @string testdataFsZipˢᵇ = "testdata/fs.zip"u8;
internal static ref readonly @string tmplTmplˢ => ref tmplTmplˢᵇ;
private static readonly @string tmplTmplˢᵇ = "tmpl*.tmpl"u8;

public static void TestParseZipFS(ж<testing.T> t) {
    var (z, err) = zip.OpenReader(testdataFsZipˢ);
    if (err != nil) {
        t.Fatalf("error parsing zip: %v", err);
    }
    (var template, err) = New(rootˢ).ParseFS(new template_test_package.zip_ReadCloserжFS(z), tmplTmplˢ);
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
    ж<global::go.html.template_package.Template> tmpl = default;
    error err = default;
    (tmpl, err) = New(tmpl1ˢ).Parse(defineTestFooEndˢ); if (err != nil) {
        t.Fatalf("parse 1: %v", err);
    }
    (_, err) = tmpl.Parse(defineTestBarEndˢ); if (err != nil) {
        t.Fatalf("got error %v, expected nil", err);
    }
    (_, err) = tmpl.New(tmpl2ˢ).Parse(defineTestBarEndˢ); if (err != nil) {
        t.Fatalf("got error %v, expected nil", err);
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
internal static readonly object brokenOnHtmlTemplateˢ = (@string)"broken on html/template"u8;
internal static readonly object lookupReturnedNonNilˢ = (@string)"Lookup returned non-nil value for undefined template foo"u8;
internal static readonly object lookupReturnedNonNilˢ2 = (@string)"Lookup returned non-nil value for undefined template bar"u8;
internal static ref readonly @string defineFooTestEndˢ => ref defineFooTestEndˢᵇ;
private static readonly @string defineFooTestEndˢᵇ = @"{{define ""foo""}}test{{end}}"u8;
internal static readonly object lookupReturnedNilValueˢ = (@string)"Lookup returned nil value for defined template"u8;

// Issue 10910, 10926
public static void TestTemplateLookUp(ж<testing.T> t) {
    t.Skip(brokenOnHtmlTemplateˢ); // TODO
    var t1 = New(fooˢ2);
    if (t1.Lookup(fooˢ2) != nil) {
        t.Error(lookupReturnedNonNilˢ);
    }
    t1.New(barˢ2);
    if (t1.Lookup(barˢ2) != nil) {
        t.Error(lookupReturnedNonNilˢ2);
    }
    t1.Parse(defineFooTestEndˢ);
    if (t1.Lookup(fooˢ2) == nil) {
        t.Error(lookupReturnedNilValueˢ);
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
        new(["x", "y"], "", "y"),
        new([""], "once", ""),
        new(["", ""], "twice", ""),
        new(["{{.}}", "{{.}}"], "twice", "twice"),
        new(["{{/* a comment */}}", "{{/* a comment */}}"], "comment", ""),
        new(["{{.}}", ""], "twice", "twice") // TODO: should want "" not "twice"
    ]);

    foreach (var (i, c) in range(cases)) {
        var root = New(rootˢ);

        ж<global::go.html.template_package.Template> m = default;
        error err = default;
        foreach (var (_, d) in range(c.defn)) {
            (m, err) = root.New(c.@in).Parse(d);
            if (err != nil) {
                t.Fatal(err);
            }
        }
        var buf = Ꮡ(new strings.Builder());
        var errΔ1 = m.Execute(new template_test_package.strings_BuilderжWriter(buf), c.@in); if (errΔ1 != nil) {
            t.Error(i, errΔ1);
            continue;
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
        foreach (var (_, name) in range(all)) {
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

} // end template_internal_test_package
