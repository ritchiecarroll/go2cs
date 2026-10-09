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

// errors
internal static slice<multiParseTest> multiParseTests = new multiParseTest[]{
    new("empty"u8, ""u8, noError,
        default!,
        default!),
    new("one"u8, @"{{define ""foo""}} FOO {{end}}"u8, noError,
        new @string[]{"foo"u8}.slice(),
        new @string[]{" FOO "u8}.slice()),
    new("two"u8, @"{{define ""foo""}} FOO {{end}}{{define ""bar""}} BAR {{end}}"u8, noError,
        new @string[]{"foo"u8, "bar"u8}.slice(),
        new @string[]{" FOO "u8, " BAR "u8}.slice()),
    new("missing end"u8, @"{{define ""foo""}} FOO "u8, hasError,
        default!,
        default!),
    new("malformed name"u8, @"{{define ""foo}} FOO "u8, hasError,
        default!,
        default!)
}.slice();

public static void TestMultiParse(ж<testing.T> Ꮡt) {
    ref var t = ref Ꮡt.DerefOrNull();

    foreach (var (_, test) in multiParseTests) {
        var (template, err) = New(rootˢ).Parse(test.input);
        switch (ᐧ) {
        case {} when err == default! && !test.ok: {
            Ꮡt.Errorf("%q: expected error; got none"u8, test.name);
            continue;
            break;
        }
        case {} when err != default! && test.ok: {
            Ꮡt.Errorf("%q: unexpected error: %v"u8, test.name, err);
            continue;
            break;
        }
        case {} when err != default! && !test.ok: {
            if (debug.Value) {
                // expected error, got one
                fmt.Printf("%s: %s\n\t%s\n"u8, test.name, test.input, err);
            }
            continue;
            break;
        }}

        if (template == nil) {
            continue;
        }
        if (len((~template).tmpl) != len(test.names) + 1) {
            // +1 for root
            Ꮡt.Errorf("%s: wrong number of templates; wanted %d got %d"u8, test.name, len(test.names), len((~template).tmpl));
            continue;
        }
        foreach (var (i, name) in test.names) {
            var (tmpl, ok) = (~template).tmpl[name, ꟷ];
            if (!ok) {
                Ꮡt.Errorf("%s: can't find template %q"u8, test.name, name);
                continue;
            }
            @string result = (~tmpl).Root.String();
            if (result != test.results[i]) {
                Ꮡt.Errorf("%s=(%q): got\n\t%v\nexpected\n\t%v"u8, test.name, test.input, result, test.results[i]);
            }
        }
    }
}

// User-defined function: test argument evaluator.
internal static slice<execTest> multiExecTests;
internal static void initᴛmultiExecTests() { multiExecTests = new execTest[]{
    new("empty"u8, ""u8, ""u8, default!, true),
    new("text"u8, "some text"u8, "some text"u8, default!, true),
    new("invoke x"u8, @"{{template ""x"" .SI}}"u8, "TEXT"u8, tVal.OrTypedNil(), true),
    new("invoke x no args"u8, @"{{template ""x""}}"u8, "TEXT"u8, tVal.OrTypedNil(), true),
    new("invoke dot int"u8, @"{{template ""dot"" .I}}"u8, "17"u8, tVal.OrTypedNil(), true),
    new("invoke dot []int"u8, @"{{template ""dot"" .SI}}"u8, "[3 4 5]"u8, tVal.OrTypedNil(), true),
    new("invoke dotV"u8, @"{{template ""dotV"" .U}}"u8, "v"u8, tVal.OrTypedNil(), true),
    new("invoke nested int"u8, @"{{template ""nested"" .I}}"u8, "17"u8, tVal.OrTypedNil(), true),
    new("variable declared by template"u8, @"{{template ""nested"" $x:=.SI}},{{index $x 1}}"u8, "[3 4 5],4"u8, tVal.OrTypedNil(), true),
    new("testFunc literal"u8, @"{{oneArg ""joe""}}"u8, "oneArg=joe"u8, tVal.OrTypedNil(), true),
    new("testFunc ."u8, @"{{oneArg .}}"u8, "oneArg=joe"u8, (@string)"joe"u8, true)
}.slice(); }

// These strings are also in testdata/*.
internal static readonly @string multiText1 = """

	{{define "x"}}TEXT{{end}}
	{{define "dotV"}}{{.V}}{{end}}

"""u8;

internal static readonly @string multiText2 = """

	{{define "dot"}}{{.}}{{end}}
	{{define "nested"}}{{template "dot" .}}{{end}}

"""u8;

public static void TestMultiExecute(ж<testing.T> Ꮡt) {
    // Declare a couple of templates first.
    var (template, err) = New(rootˢ).Parse(multiText1);
    if (err != default!) {
        Ꮡt.Fatalf("parse error for 1: %s"u8, err);
    }
    (_, err) = template.Parse(multiText2);
    if (err != default!) {
        Ꮡt.Fatalf("parse error for 2: %s"u8, err);
    }
    testExecute(multiExecTests, template, Ꮡt);
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static readonly @string doesNotExistˢ = "DOES NOT EXIST"u8;
internal static readonly object expectedErrorForNonˢ = (@string)"expected error for non-existent file; got none"u8;
internal static readonly @string testdataFile1Tmplˢ = "testdata/file1.tmpl"u8;
internal static readonly @string testdataFile2Tmplˢ = "testdata/file2.tmpl"u8;

public static void TestParseFiles(ж<testing.T> Ꮡt) {
    var (_, err) = ParseFiles(doesNotExistˢ);
    if (err == default!) {
        Ꮡt.Error(expectedErrorForNonˢ);
    }
    var template = New(rootˢ);
    (_, err) = template.ParseFiles(testdataFile1Tmplˢ, testdataFile2Tmplˢ);
    if (err != default!) {
        Ꮡt.Fatalf("error parsing files: %v"u8, err);
    }
    testExecute(multiExecTests, template, Ꮡt);
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static readonly object expectedErrorForBadˢ = (@string)"expected error for bad pattern; got none"u8;
internal static readonly @string testdataFileTmplˢ = "testdata/file*.tmpl"u8;

public static void TestParseGlob(ж<testing.T> Ꮡt) {
    var (_, err) = ParseGlob(doesNotExistˢ);
    if (err == default!) {
        Ꮡt.Error(expectedErrorForNonˢ);
    }
    (_, err) = New(errorˢ).ParseGlob("[x"u8);
    if (err == default!) {
        Ꮡt.Error(expectedErrorForBadˢ);
    }
    var template = New(rootˢ);
    (_, err) = template.ParseGlob(testdataFileTmplˢ);
    if (err != default!) {
        Ꮡt.Fatalf("error parsing files: %v"u8, err);
    }
    testExecute(multiExecTests, template, Ꮡt);
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static readonly @string testdataˢ = "testdata"u8;
internal static readonly @string file1Tmplˢ = "file1.tmpl"u8;
internal static readonly @string file2Tmplˢ = "file2.tmpl"u8;
internal static readonly @string fileTmplˢ = "file*.tmpl"u8;

public static void TestParseFS(ж<testing.T> Ꮡt) {
    var fs = os.DirFS(testdataˢ);
    {
        var (_, err) = ParseFS(fs, doesNotExistˢ);
        if (err == default!) {
            Ꮡt.Error(expectedErrorForNonˢ);
        }
    }
    {
        var template = New(rootˢ);
        var (_, err) = template.ParseFS(fs, file1Tmplˢ, file2Tmplˢ);
        if (err != default!) {
            Ꮡt.Fatalf("error parsing files: %v"u8, err);
        }
        testExecute(multiExecTests, template, Ꮡt);
    }
    {
        var template = New(rootˢ);
        var (_, err) = template.ParseFS(fs, fileTmplˢ);
        if (err != default!) {
            Ꮡt.Fatalf("error parsing files: %v"u8, err);
        }
        testExecute(multiExecTests, template, Ꮡt);
    }
}

// In these tests, actual content (not just template definitions) comes from the parsed files.
internal static slice<execTest> templateFileExecTests = new execTest[]{
    new("test"u8, @"{{template ""tmpl1.tmpl""}}{{template ""tmpl2.tmpl""}}"u8, "template1\n\ny\ntemplate2\n\nx\n"u8, (nint)(0), true)
}.slice();

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static readonly @string testdataTmpl1Tmplˢ = "testdata/tmpl1.tmpl"u8;
internal static readonly @string testdataTmpl2Tmplˢ = "testdata/tmpl2.tmpl"u8;

public static void TestParseFilesWithData(ж<testing.T> Ꮡt) {
    var (template, err) = New(rootˢ).ParseFiles(testdataTmpl1Tmplˢ, testdataTmpl2Tmplˢ);
    if (err != default!) {
        Ꮡt.Fatalf("error parsing files: %v"u8, err);
    }
    testExecute(templateFileExecTests, template, Ꮡt);
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static readonly @string testdataTmplTmplˢ = "testdata/tmpl*.tmpl"u8;

public static void TestParseGlobWithData(ж<testing.T> Ꮡt) {
    var (template, err) = New(rootˢ).ParseGlob(testdataTmplTmplˢ);
    if (err != default!) {
        Ꮡt.Fatalf("error parsing files: %v"u8, err);
    }
    testExecute(templateFileExecTests, template, Ꮡt);
}

internal static readonly @string cloneText1 = @"{{define ""a""}}{{template ""b""}}{{template ""c""}}{{end}}"u8;
internal static readonly @string cloneText2 = @"{{define ""b""}}b{{end}}"u8;
internal static readonly @string cloneText3 = @"{{define ""c""}}root{{end}}"u8;
internal static readonly @string cloneText4 = @"{{define ""c""}}clone{{end}}"u8;

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static readonly object cloneDoesNotContainRootˢ = (@string)"clone does not contain root"u8;
internal static readonly object brootˢ = (@string)"broot"u8;
internal static readonly object bcloneˢ = (@string)"bclone"u8;

public static void TestClone(ж<testing.T> Ꮡt) {
    // Create some templates and clone the root.
    var (root, err) = New(rootˢ).Parse(cloneText1);
    if (err != default!) {
        Ꮡt.Fatal(err);
    }
    (_, err) = root.Parse(cloneText2);
    if (err != default!) {
        Ꮡt.Fatal(err);
    }
    var (ᴛ1, ᴛ2) = root.Clone();
    var clone = Must(ᴛ1, ᴛ2);
    // Add variants to both.
    (_, err) = root.Parse(cloneText3);
    if (err != default!) {
        Ꮡt.Fatal(err);
    }
    (_, err) = clone.Parse(cloneText4);
    if (err != default!) {
        Ꮡt.Fatal(err);
    }
    // Verify that the clone is self-consistent.
    foreach (var (k, v) in (~clone).tmpl) {
        if (k == (~clone).name && (~v).tmpl[k] != clone) {
            Ꮡt.Error(cloneDoesNotContainRootˢ);
        }
        if (v != (~v).tmpl[(~v).name]) {
            Ꮡt.Errorf("clone does not contain self for %q"u8, k);
        }
    }
    // Execute root.
    ref var b = ref heap(new strings.Builder(), out var Ꮡb);
    err = root.ExecuteTemplate(new template_test_package.strings_BuilderжWriter(Ꮡb), "a"u8, (nint)(0));
    if (err != default!) {
        Ꮡt.Fatal(err);
    }
    if (b.String() != "broot"u8) {
        Ꮡt.Errorf("expected %q got %q"u8, brootˢ, b.String());
    }
    // Execute copy.
    b.Reset();
    err = clone.ExecuteTemplate(new template_test_package.strings_BuilderжWriter(Ꮡb), "a"u8, (nint)(0));
    if (err != default!) {
        Ꮡt.Fatal(err);
    }
    if (b.String() != "bclone"u8) {
        Ꮡt.Errorf("expected %q got %q"u8, bcloneˢ, b.String());
    }
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static readonly @string cloneText3ˢ = "cloneText3"u8;

public static void TestAddParseTree(ж<testing.T> Ꮡt) {
    // Create some templates.
    var (root, err) = New(rootˢ).Parse(cloneText1);
    if (err != default!) {
        Ꮡt.Fatal(err);
    }
    (_, err) = root.Parse(cloneText2);
    if (err != default!) {
        Ꮡt.Fatal(err);
    }
    // Add a new parse tree.
    (var tree, err) = parse.Parse(cloneText3ˢ, cloneText3, ""u8, ""u8, (map<@string, any>)(default!), builtins());
    if (err != default!) {
        Ꮡt.Fatal(err);
    }
    (var added, err) = root.AddParseTree("c"u8, tree["c"u8]);
    if (err != default!) {
        Ꮡt.Fatal(err);
    }
    // Execute.
    ref var b = ref heap(new strings.Builder(), out var Ꮡb);
    err = added.ExecuteTemplate(new template_test_package.strings_BuilderжWriter(Ꮡb), "a"u8, (nint)(0));
    if (err != default!) {
        Ꮡt.Fatal(err);
    }
    if (b.String() != "broot"u8) {
        Ꮡt.Errorf("expected %q got %q"u8, brootˢ, b.String());
    }
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static readonly @string defineMasterEndˢ = "{{define \"master\"}}{{end}}"u8;
internal static readonly @string masterˢ = "master"u8;

// Issue 7032
public static void TestAddParseTreeToUnparsedTemplate(ж<testing.T> Ꮡt) {
    @string master = defineMasterEndˢ;
    var tmpl = New(masterˢ);
    var (tree, err) = parse.Parse(masterˢ, master, ""u8, ""u8, (map<@string, any>)(default!));
    if (err != default!) {
        Ꮡt.Fatalf("unexpected parse err: %v"u8, err);
    }
    var masterTree = tree[masterˢ];
    tmpl.AddParseTree(masterˢ, masterTree); // used to panic
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static readonly @string tmpl1ˢ = "tmpl1"u8;
internal static readonly @string defineTestFooEndˢ = @"{{define ""test""}}foo{{end}}"u8;
internal static readonly @string defineTestBarEndˢ = @"{{define ""test""}}bar{{end}}"u8;
internal static readonly @string tmpl2ˢ = "tmpl2"u8;

public static void TestRedefinition(ж<testing.T> Ꮡt) {
    ж<global::go.text.template_package.Template> tmpl = default!;
    error err = default!;
    {
        (tmpl, err) = New(tmpl1ˢ).Parse(defineTestFooEndˢ); if (err != default!) {
            Ꮡt.Fatalf("parse 1: %v"u8, err);
        }
    }
    {
        (_, err) = tmpl.Parse(defineTestBarEndˢ); if (err != default!) {
            Ꮡt.Fatalf("got error %v, expected nil"u8, err);
        }
    }
    {
        (_, err) = tmpl.New(tmpl2ˢ).Parse(defineTestBarEndˢ); if (err != default!) {
            Ꮡt.Fatalf("got error %v, expected nil"u8, err);
        }
    }
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static readonly @string baseˢ = "base"u8;

// Issue 10879
public static void TestEmptyTemplateCloneCrash(ж<testing.T> Ꮡt) {
    var t1 = New(baseˢ);
    t1.Clone(); // used to panic
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static readonly object lookupReturnedNonNilˢ = (@string)"Lookup returned non-nil value for undefined template foo"u8;
internal static readonly object lookupReturnedNonNilˢ2 = (@string)"Lookup returned non-nil value for undefined template bar"u8;
internal static readonly @string defineFooTestEndˢ = @"{{define ""foo""}}test{{end}}"u8;
internal static readonly object lookupReturnedNilValueˢ = (@string)"Lookup returned nil value for defined template"u8;

// Issue 10910, 10926
public static void TestTemplateLookUp(ж<testing.T> Ꮡt) {
    var t1 = New(fooˢ);
    if (t1.Lookup(fooˢ) != nil) {
        Ꮡt.Error(lookupReturnedNonNilˢ);
    }
    t1.New(barˢ);
    if (t1.Lookup(barˢ) != nil) {
        Ꮡt.Error(lookupReturnedNonNilˢ2);
    }
    t1.Parse(defineFooTestEndˢ);
    if (t1.Lookup(fooˢ) == nil) {
        Ꮡt.Error(lookupReturnedNilValueˢ);
    }
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static readonly @string testˢ = "test"u8;
internal static readonly object definedTemplateGotNilˢ = (@string)"defined template got nil Tree"u8;
internal static readonly object undefinedTemplateGotNonˢ = (@string)"undefined template got non-nil Tree"u8;
internal static readonly object templatesIncludedˢ = (@string)"Templates included undefined template"u8;
internal static readonly object templatesDidnTIncludeˢ = (@string)"Templates didn't include defined template"u8;

public static void TestNew(ж<testing.T> Ꮡt) {
    ref var t = ref Ꮡt.DerefOrNull();

    // template with same name already exists
    var (t1, _) = New(testˢ).Parse(defineTestFooEndˢ);
    var t2 = t1.New(testˢ);
    if ((~t1).common != (~t2).common) {
        Ꮡt.Errorf("t1 & t2 didn't share common struct; got %v != %v"u8, (~t1).common.OrTypedNil(), (~t2).common.OrTypedNil());
    }
    if ((~t1).Tree == nil) {
        Ꮡt.Error(definedTemplateGotNilˢ);
    }
    if ((~t2).Tree != nil) {
        Ꮡt.Error(undefinedTemplateGotNonˢ);
    }
    var containsT1 = false;
    foreach (var (_, tmpl) in t1.Templates()) {
        if (tmpl == t2) {
            Ꮡt.Error(templatesIncludedˢ);
        }
        if (tmpl == t1) {
            containsT1 = true;
        }
    }
    if (!containsT1) {
        Ꮡt.Error(templatesDidnTIncludeˢ);
    }
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static readonly @string defineTestEndˢ = @"{{define ""test""}}{{end}}"u8;
internal static readonly @string defineTestThisIsACommentˢ = @"{{define ""test""}}{{/* this is a comment */}}{{end}}"u8;

public static void TestParse(ж<testing.T> Ꮡt) {
    // In multiple calls to Parse with the same receiver template, only one call
    // can contain text other than space, comments, and template definitions
    var t1 = New(testˢ);
    {
        var (_, err) = t1.Parse(defineTestEndˢ); if (err != default!) {
            Ꮡt.Fatalf("parsing test: %s"u8, err);
        }
    }
    {
        var (_, err) = t1.Parse(defineTestThisIsACommentˢ); if (err != default!) {
            Ꮡt.Fatalf("parsing test: %s"u8, err);
        }
    }
    {
        var (_, err) = t1.Parse(defineTestFooEndˢ); if (err != default!) {
            Ꮡt.Fatalf("parsing test: %s"u8, err);
        }
    }
}

internal partial struct TestEmptyTemplate_cases /*dyn*/ {
    internal slice<@string> defn;
    internal @string @in;
    internal @string want;
}

public static void TestEmptyTemplate(ж<testing.T> Ꮡt) {
    var cases = new TestEmptyTemplate_cases[]{
        new(new @string[]{"x"u8, "y"u8}.slice(), ""u8, "y"u8),
        new(new @string[]{""u8}.slice(), "once"u8, ""u8),
        new(new @string[]{""u8, ""u8}.slice(), "twice"u8, ""u8),
        new(new @string[]{"{{.}}"u8, "{{.}}"u8}.slice(), "twice"u8, "twice"u8),
        new(new @string[]{"{{/* a comment */}}"u8, "{{/* a comment */}}"u8}.slice(), "comment"u8, ""u8),
        new(new @string[]{"{{.}}"u8, ""u8}.slice(), "twice"u8, ""u8)
    }.slice();
    foreach (var (i, c) in cases) {
        var root = New(rootˢ);
        ж<global::go.text.template_package.Template> m = default!;
        error err = default!;
        foreach (var (_, d) in c.defn) {
            (m, err) = root.New(c.@in).Parse(d);
            if (err != default!) {
                Ꮡt.Fatal(err);
            }
        }
        var buf = Ꮡ(new strings.Builder(nil));
        {
            var errΔ1 = m.Execute(new template_test_package.strings_BuilderжWriter(buf), c.@in); if (errΔ1 != default!) {
                Ꮡt.Error(i, errΔ1);
                continue;
            }
        }
        if (buf.String() != c.want) {
            Ꮡt.Errorf("expected string %q: got %q"u8, c.want, buf.String());
        }
    }
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static readonly @string titleXhtmlˢ = "title.xhtml"u8;
internal static readonly @string templateXhtmlˢ = @"{{template ""xhtml"" .}}"u8;
internal static readonly object stylesheetˢ = (@string)"stylesheet"u8;

// Issue 19249 was a regression in 1.8 caused by the handling of empty
// templates added in that release, which got different answers depending
// on the order templates appeared in the internal map.
public static void TestIssue19294(ж<testing.T> Ꮡt) {
    ref var t = ref Ꮡt.DerefOrNull();

    // The empty block in "xhtml" should be replaced during execution
    // by the contents of "stylesheet", but if the internal map associating
    // names with templates is built in the wrong order, the empty block
    // looks non-empty and this doesn't happen.
    map<@string, @string> inlined = new map<@string, @string>{
        ["stylesheet"u8] = @"{{define ""stylesheet""}}stylesheet{{end}}"u8,
        ["xhtml"u8] = @"{{block ""stylesheet"" .}}{{end}}"u8
    };
    var all = new @string[]{"stylesheet"u8, "xhtml"u8}.slice();
    for (nint i = 0; i < 100; i++) {
        var (res, err) = New(titleXhtmlˢ).Parse(templateXhtmlˢ);
        if (err != default!) {
            Ꮡt.Fatal(err);
        }
        foreach (var (_, name) in all) {
            var (_, errΔ1) = res.New(name).Parse(inlined[name]);
            if (errΔ1 != default!) {
                Ꮡt.Fatal(errΔ1);
            }
        }
        ref var buf = ref heap(new strings.Builder(), out var Ꮡbuf);
        res.Execute(new template_test_package.strings_BuilderжWriter(Ꮡbuf), (nint)(0));
        if (buf.String() != "stylesheet"u8) {
            Ꮡt.Fatalf("iteration %d: got %q; expected %q"u8, i, buf.String(), stylesheetˢ);
        }
    }
}

// Issue 48436
public static void TestAddToZeroTemplate(ж<testing.T> Ꮡt) {
    var (tree, err) = parse.Parse("c"u8, cloneText3, ""u8, ""u8, (map<@string, any>)(default!), builtins());
    if (err != default!) {
        Ꮡt.Fatal(err);
    }
    ref var tmpl = ref heap(new global::go.text.template_package.Template(), out var Ꮡtmpl);
    Ꮡtmpl.AddParseTree("x"u8, tree["c"u8]);
}

} // end template_internal_test_package
