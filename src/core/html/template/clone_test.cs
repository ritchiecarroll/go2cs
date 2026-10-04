// Copyright 2011 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.
namespace go.html;

using errors = errors_package;
using fmt = fmt_package;
using io = io_package;
using strings = strings_package;
using sync = sync_package;
using testing = testing_package;
using parse = text.template.parse_package;
using MethodImplAttribute = global::System.Runtime.CompilerServices.MethodImplAttribute;
using MethodImplOptions = global::System.Runtime.CompilerServices.MethodImplOptions;
using static go.html.template_package;
using template = text.template_package;
using text;
using text.template;

partial class template_internal_test_package {

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static readonly @string rootˢ = "root"u8;
internal static readonly @string defineATemplateBAEndˢ = @"{{define ""a""}} {{.}} {{template ""b""}} {{.}} ""></a>{{end}}"u8;
internal static readonly @string defineBAHrefEndˢ = @"{{define ""b""}}<a href=""{{end}}"u8;
internal static readonly @string gt0AHref13e0Aˢ = @" 1&gt;0 <a href="" 1%3e0 ""></a>"u8;

public static void TestAddParseTreeHTML(ж<testing.T> Ꮡt) {
    ref var t = ref Ꮡt.DerefOrNull();

    var (ᴛ1, ᴛ2) = New(rootˢ).Parse(defineATemplateBAEndˢ);
    var root = Must(ᴛ1, ᴛ2);
    var (tree, err) = parse.Parse("t"u8, defineBAHrefEndˢ, ""u8, ""u8, (map<@string, any>)(default!), (map<@string, any>)(default!));
    if (err != default!) {
        Ꮡt.Fatal(err);
    }
    var (ᴛ3, ᴛ4) = root.AddParseTree("b"u8, tree["b"u8]);
    var added = Must(ᴛ3, ᴛ4);
    var b = @new<strings.Builder>();
    err = added.ExecuteTemplate(new template_test_package.strings_BuilderжWriter(b), "a"u8, (@string)"1>0"u8);
    if (err != default!) {
        Ꮡt.Fatal(err);
    }
    {
        @string got = b.String();
        @string want = gt0AHref13e0Aˢ; if (got != want) {
            Ꮡt.Errorf("got %q want %q"u8, got, want);
        }
    }
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static readonly @string defineLhsAHrefEndˢ = @"{{define ""lhs""}} <a href="" {{end}}"u8;
internal static readonly @string defineRhsAEndˢ = @"{{define ""rhs""}} ""></a> {{end}}"u8;
internal static readonly @string aHref3ci3eAˢ = @" <a href="" %3ci%3e*/ ""></a> "u8;
internal static readonly @string defineLhsPOnclickˢ = @"{{define ""lhs""}} <p onclick=""javascript: {{end}}"u8;
internal static readonly @string defineRhsPEndˢ = @"{{define ""rhs""}} ""></p> {{end}}"u8;
internal static readonly @string pOnclickJavascript34ˢ = @" <p onclick=""javascript: &#34;\u003ci\u003e*/&#34; ""></p> "u8;
internal static readonly @string defineLhsStyleEndˢ = @"{{define ""lhs""}} <style> {{end}}"u8;
internal static readonly @string defineRhsStyleEndˢ = @"{{define ""rhs""}} </style> {{end}}"u8;
internal static readonly @string defineLhsEndˢ = @"{{define ""lhs""}} ( {{end}}"u8;
internal static readonly @string defineRhsEndˢ = @"{{define ""rhs""}} ) {{end}}"u8;
internal static readonly @string defineLhsOkEndˢ = @"{{define ""lhs""}} OK {{end}}"u8;
internal static readonly object cloningT1GotNilErrWantˢ = (@string)"cloning t1: got nil err want non-nil"u8;
internal static readonly object redefineLhsGotNilErrWantˢ = (@string)@"redefine ""lhs"": got nil err want non-nil"u8;
internal static readonly @string ltIGtˢ = @" ( &lt;i&gt;*/ ) "u8;
internal static readonly object t0CloneGotNilErrWantNonˢ = (@string)@"t0.Clone(): got nil err want non-nil"u8;
internal static readonly object t0LookupACloneGotNilErrˢ = (@string)@"t0.Lookup(""a"").Clone(): got nil err want non-nil"u8;
internal static readonly @string lhsˢ = "lhs"u8;
internal static readonly object t0LookupLhsCloneGotNilˢ = (@string)@"t0.Lookup(""lhs"").Clone(): got nil err want non-nil"u8;
internal static readonly @string styleZgotmplZStyleˢ = @" <style> ZgotmplZ </style> "u8;

// Hoisted Go string constant (single allocation; Go keeps it in RODATA)
internal static readonly @string tmplᶜ = @"{{define ""a""}}{{template ""lhs""}}{{.}}{{template ""rhs""}}{{end}}"u8;

public static void TestClone(ж<testing.T> Ꮡt) {
    // The {{.}} will be executed with data "<i>*/" in different contexts.
    // In the t0 template, it will be in a text context.
    // In the t1 template, it will be in a URL context.
    // In the t2 template, it will be in a JavaScript context.
    // In the t3 template, it will be in a CSS context.
    @string tmpl = tmplᶜ;
    var b = @new<strings.Builder>();
    // Create an incomplete template t0.
    var (ᴛ5, ᴛ6) = New("t0"u8).Parse(tmpl);
    var t0 = Must(ᴛ5, ᴛ6);
    // Clone t0 as t1.
    var (ᴛ7, ᴛ8) = t0.Clone();
    var t1 = Must(ᴛ7, ᴛ8);
    var (ᴛ9, ᴛ10) = t1.Parse(defineLhsAHrefEndˢ);
    Must(ᴛ9, ᴛ10);
    var (ᴛ11, ᴛ12) = t1.Parse(defineRhsAEndˢ);
    Must(ᴛ11, ᴛ12);
    // Execute t1.
    b.Reset();
    {
        var err = t1.ExecuteTemplate(new template_test_package.strings_BuilderжWriter(b), "a"u8, (@string)"<i>*/"u8); if (err != default!) {
            Ꮡt.Fatal(err);
        }
    }
    {
        @string got = b.String();
        @string want = aHref3ci3eAˢ; if (got != want) {
            Ꮡt.Errorf("t1: got %q want %q"u8, got, want);
        }
    }
    // Clone t0 as t2.
    var (ᴛ13, ᴛ14) = t0.Clone();
    var t2 = Must(ᴛ13, ᴛ14);
    var (ᴛ15, ᴛ16) = t2.Parse(defineLhsPOnclickˢ);
    Must(ᴛ15, ᴛ16);
    var (ᴛ17, ᴛ18) = t2.Parse(defineRhsPEndˢ);
    Must(ᴛ17, ᴛ18);
    // Execute t2.
    b.Reset();
    {
        var err = t2.ExecuteTemplate(new template_test_package.strings_BuilderжWriter(b), "a"u8, (@string)"<i>*/"u8); if (err != default!) {
            Ꮡt.Fatal(err);
        }
    }
    {
        @string got = b.String();
        @string want = pOnclickJavascript34ˢ; if (got != want) {
            Ꮡt.Errorf("t2: got %q want %q"u8, got, want);
        }
    }
    // Clone t0 as t3, but do not execute t3 yet.
    var (ᴛ19, ᴛ20) = t0.Clone();
    var t3 = Must(ᴛ19, ᴛ20);
    var (ᴛ21, ᴛ22) = t3.Parse(defineLhsStyleEndˢ);
    Must(ᴛ21, ᴛ22);
    var (ᴛ23, ᴛ24) = t3.Parse(defineRhsStyleEndˢ);
    Must(ᴛ23, ᴛ24);
    // Complete t0.
    var (ᴛ25, ᴛ26) = t0.Parse(defineLhsEndˢ);
    Must(ᴛ25, ᴛ26);
    var (ᴛ27, ᴛ28) = t0.Parse(defineRhsEndˢ);
    Must(ᴛ27, ᴛ28);
    // Clone t0 as t4. Redefining the "lhs" template should not fail.
    var (ᴛ29, ᴛ30) = t0.Clone();
    var t4 = Must(ᴛ29, ᴛ30);
    {
        var (_, err) = t4.Parse(defineLhsOkEndˢ); if (err != default!) {
            Ꮡt.Errorf(@"redefine ""lhs"": got err %v want nil"u8, err);
        }
    }
    // Cloning t1 should fail as it has been executed.
    {
        var (_, err) = t1.Clone(); if (err == default!) {
            Ꮡt.Error(cloningT1GotNilErrWantˢ);
        }
    }
    // Redefining the "lhs" template in t1 should fail as it has been executed.
    {
        var (_, err) = t1.Parse(defineLhsOkEndˢ); if (err == default!) {
            Ꮡt.Error(redefineLhsGotNilErrWantˢ);
        }
    }
    // Execute t0.
    b.Reset();
    {
        var err = t0.ExecuteTemplate(new template_test_package.strings_BuilderжWriter(b), "a"u8, (@string)"<i>*/"u8); if (err != default!) {
            Ꮡt.Fatal(err);
        }
    }
    {
        @string got = b.String();
        @string want = ltIGtˢ; if (got != want) {
            Ꮡt.Errorf("t0: got %q want %q"u8, got, want);
        }
    }
    // Clone t0. This should fail, as t0 has already executed.
    {
        var (_, err) = t0.Clone(); if (err == default!) {
            Ꮡt.Error(t0CloneGotNilErrWantNonˢ);
        }
    }
    // Similarly, cloning sub-templates should fail.
    {
        var (_, err) = t0.Lookup("a"u8).Clone(); if (err == default!) {
            Ꮡt.Error(t0LookupACloneGotNilErrˢ);
        }
    }
    {
        var (_, err) = t0.Lookup(lhsˢ).Clone(); if (err == default!) {
            Ꮡt.Error(t0LookupLhsCloneGotNilˢ);
        }
    }
    // Execute t3.
    b.Reset();
    {
        var err = t3.ExecuteTemplate(new template_test_package.strings_BuilderжWriter(b), "a"u8, (@string)"<i>*/"u8); if (err != default!) {
            Ꮡt.Fatal(err);
        }
    }
    {
        @string got = b.String();
        @string want = styleZgotmplZStyleˢ; if (got != want) {
            Ꮡt.Errorf("t3: got %q want %q"u8, got, want);
        }
    }
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static readonly object couldNotFindTemplateˢ = (@string)"could not find template"u8;

// Hoisted Go string constant (single allocation; Go keeps it in RODATA)
internal static readonly @string tmplᶜ1 = """

		{{define "a"}}{{template "lhs"}}{{.}}{{template "rhs"}}{{end}}
		{{define "lhs"}} <a href=" {{end}}
		{{define "rhs"}} "></a> {{end}}
"""u8;

public static void TestTemplates(ж<testing.T> Ꮡt) {
    var names = new @string[]{"t0"u8, "a"u8, "lhs"u8, "rhs"u8}.slice();
    // Some template definitions borrowed from TestClone.
    @string tmpl = tmplᶜ1;
    var (ᴛ31, ᴛ32) = New("t0"u8).Parse(tmpl);
    var t0 = Must(ᴛ31, ᴛ32);
    var templates = t0.Templates();
    if (len(templates) != len(names)) {
        Ꮡt.Errorf("expected %d templates; got %d"u8, len(names), len(templates));
    }
    foreach (var (_, name) in names) {
        var found = false;
        foreach (var (_, tmplΔ1) in templates) {
            if (name == (~tmplΔ1).text.Name()) {
                found = true;
                break;
            }
        }
        if (!found) {
            Ꮡt.Error(couldNotFindTemplateˢ, name);
        }
    }
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static readonly @string allˢ = "all"u8;
internal static readonly @string defineFooFooEndˢ = @"{{define ""foo""}}foo{{end}}"u8;

// This used to crash; https://golang.org/issue/3281
public static void TestCloneCrash(ж<testing.T> Ꮡt) {
    var t1 = New(allˢ);
    var (ᴛ33, ᴛ34) = t1.New("t1"u8).Parse(defineFooFooEndˢ);
    Must(ᴛ33, ᴛ34);
    t1.Clone();
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static readonly @string defineATemplateEmbeddedˢ = @"{{define ""a""}}{{template ""embedded""}}{{end}}"u8;
internal static readonly @string defineEmbeddedT1Endˢ = @"{{define ""embedded""}}t1{{end}}"u8;
internal static readonly object addingATemplateToACloneˢ = (@string)"adding a template to a clone added it to the original"u8;
internal static readonly object expectedNoSuchTemplateˢ = (@string)"expected 'no such template' error"u8;

// Ensure that this guarantee from the docs is upheld:
// "Further calls to Parse in the copy will add templates
// to the copy but not to the original."
public static void TestCloneThenParse(ж<testing.T> Ꮡt) {
    ref var t = ref Ꮡt.DerefOrNull();

    var (ᴛ35, ᴛ36) = New("t0"u8).Parse(defineATemplateEmbeddedˢ);
    var t0 = Must(ᴛ35, ᴛ36);
    var (ᴛ37, ᴛ38) = t0.Clone();
    var t1 = Must(ᴛ37, ᴛ38);
    var (ᴛ39, ᴛ40) = t1.Parse(defineEmbeddedT1Endˢ);
    Must(ᴛ39, ᴛ40);
    if (len(t0.Templates()) + 1 != len(t1.Templates())) {
        Ꮡt.Error(addingATemplateToACloneˢ);
    }
    // double check that the embedded template isn't available in the original
    var err = t0.ExecuteTemplate(io.Discard, "a"u8, default!);
    if (err == default!) {
        Ꮡt.Error(expectedNoSuchTemplateˢ);
    }
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static readonly @string issue5980ˢ = "issue5980"u8;
internal static readonly @string customFuncˢ = "{{customFunc}}"u8;

// https://golang.org/issue/5980
public static void TestFuncMapWorksAfterClone(ж<testing.T> Ꮡt) {
    var funcs = new FuncMap(new map<@string, any>{["customFunc"u8] = (@string, error) () => ("", errors.New(issue5980ˢ))
    });
    // get the expected error output (no clone)
    var (ᴛ41, ᴛ42) = New(""u8).Funcs(funcs).Parse(customFuncˢ);
    var uncloned = Must(ᴛ41, ᴛ42);
    var wantErr = uncloned.Execute(io.Discard, default!);
    // toClone must be the same as uncloned. It has to be recreated from scratch,
    // since cloning cannot occur after execution.
    var (ᴛ43, ᴛ44) = New(""u8).Funcs(funcs).Parse(customFuncˢ);
    var toClone = Must(ᴛ43, ᴛ44);
    var (ᴛ45, ᴛ46) = toClone.Clone();
    var cloned = Must(ᴛ45, ᴛ46);
    var gotErr = cloned.Execute(io.Discard, default!);
    if (wantErr.Error() != gotErr.Error()) {
        Ꮡt.Errorf("clone error message mismatch want %q got %q"u8, wantErr, gotErr);
    }
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static readonly @string outerˢ = "outer"u8;
internal static readonly object dataˢ = (@string)"data"u8;

// Hoisted Go string constant (single allocation; Go keeps it in RODATA)
internal static readonly @string inputᶜ = @"<title>{{block ""a"" .}}a{{end}}</title><body>{{block ""b"" .}}b{{end}}<body>"u8;

// Hoisted Go string constant (single allocation; Go keeps it in RODATA)
internal static readonly @string overlayᶜ = @"{{define ""b""}}A{{end}}"u8;

// https://golang.org/issue/16101
[MethodImpl(MethodImplOptions.NoInlining)] public static void TestTemplateCloneExecuteRace(ж<testing.T> Ꮡt) {
    @string input = inputᶜ;
    @string overlay = overlayᶜ;
    var (ᴛ47, ᴛ48) = New(outerˢ).Parse(input);
    var outer = Must(ᴛ47, ᴛ48);
    var (ᴛ49, ᴛ50) = outer.Clone();

    var (ᴛ51, ᴛ52) = Must(ᴛ49, ᴛ50).Parse(overlay);
    var tmpl = Must(ᴛ51, ᴛ52);
    ref var wg = ref heap(new sync.WaitGroup(), out var Ꮡwg);
    for (nint i = 0; i < 10; i++) {
        Ꮡwg.Add(1);
        var tmplʗ1 = tmpl;
        goǃ(() => {
            GoFrame ᒐ = default;
            try {
                defer(Ꮡwg.Done, ref ᒐ);
                for (nint iΔ1 = 0; iΔ1 < 100; iΔ1++) {
                    {
                        var err = tmplʗ1.Execute(io.Discard, dataˢ); if (err != default!) {
                            throw panic(err);
                        }
                    }
                }
            }
            catch (Exception ᒐex) when (GoFrame.IsPanic(ᒐex, out PanicException? ᒐp)) { GoFrame.Capture(ᒐp); }
            finally { ᒐ.Run(); }
        });
    }
    Ꮡwg.Wait();
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static readonly object afterCloneTmplLookupTmplˢ = (@string)"after Clone, tmpl.Lookup(tmpl.Name()) != tmpl"u8;

public static void TestTemplateCloneLookup(ж<testing.T> Ꮡt) {
    ref var t = ref Ꮡt.DerefOrNull();

    // Template.escape makes an assumption that the template associated
    // with t.Name() is t. Check that this holds.
    var (ᴛ53, ᴛ54) = New("x"u8).Parse("a"u8);
    var tmpl = Must(ᴛ53, ᴛ54);
    var (ᴛ55, ᴛ56) = tmpl.Clone();
    tmpl = Must(ᴛ55, ᴛ56);
    if (tmpl.Lookup(tmpl.Name()) != tmpl) {
        Ꮡt.Error(afterCloneTmplLookupTmplˢ);
    }
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static readonly @string titleBlockBArgEndTitleˢ = @"<title>{{block ""B"". }}Arg{{end}}</title>"u8;
internal static readonly @string defineBTextEndˢ = @"{{define ""B""}}Text{{end}}"u8;

public static void TestCloneGrowth(ж<testing.T> Ꮡt) {
    var (ᴛ57, ᴛ58) = New(rootˢ).Parse(titleBlockBArgEndTitleˢ);
    var tmpl = Must(ᴛ57, ᴛ58);
    var (ᴛ59, ᴛ60) = tmpl.Clone();
    tmpl = Must(ᴛ59, ᴛ60);
    var (ᴛ61, ᴛ62) = tmpl.Parse(defineBTextEndˢ);
    Must(ᴛ61, ᴛ62);
    for (nint i = 0; i < 10; i++) {
        tmpl.Execute(io.Discard, default!);
    }
    if (len(tmpl.DefinedTemplates()) > 200) {
        Ꮡt.Fatalf("too many templates: %v"u8, len(tmpl.DefinedTemplates()));
    }
}

// Hoisted Go string constant (single allocation; Go keeps it in RODATA)
internal static readonly @string @baseᶜ = """

{{ define "a" -}}<title>{{ template "b" . -}}</title>{{ end -}}
{{ define "b" }}{{ end -}}

"""u8;

// Hoisted Go string constant (single allocation; Go keeps it in RODATA)
internal static readonly @string pageᶜ = @"{{ template ""a"" . }}"u8;

// https://golang.org/issue/17735
public static void TestCloneRedefinedName(ж<testing.T> Ꮡt) {
    @string @base = @baseᶜ;
    @string page = pageᶜ;
    var (ᴛ63, ᴛ64) = New("a"u8).Parse(@base);
    var t1 = Must(ᴛ63, ᴛ64);
    for (nint i = 0; i < 2; i++) {
        var (ᴛ65, ᴛ66) = t1.Clone();
        var t2 = Must(ᴛ65, ᴛ66);
        var (ᴛ67, ᴛ68) = t2.New(fmt.Sprintf("%d"u8, i)).Parse(page);
        t2 = Must(ᴛ67, ᴛ68);
        var err = t2.Execute(io.Discard, default!);
        if (err != default!) {
            Ꮡt.Fatal(err);
        }
    }
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static readonly @string defineARangeVAVEndEndˢ = @"{{define ""a""}}{{range $v := .A}}{{$v}}{{end}}{{end}}"u8;

[GoType("dyn")] internal partial struct TestClonePipe_data {
    public slice<@string> A;
}

// Issue 24791.
public static void TestClonePipe(ж<testing.T> Ꮡt) {
    var (ᴛ69, ᴛ70) = New("a"u8).Parse(defineARangeVAVEndEndˢ);
    var a = Must(ᴛ69, ᴛ70);
    ref var data = ref heap<TestClonePipe_data>(out var Ꮡdata);
    data = new TestClonePipe_data(A: new @string[]{"hi"u8}.slice());
    var (ᴛ71, ᴛ72) = a.Clone();
    var b = Must(ᴛ71, ᴛ72);
    ref var buf = ref heap(new strings.Builder(), out var Ꮡbuf);
    {
        var err = b.Execute(new template_test_package.strings_BuilderжWriter(Ꮡbuf), Ꮡdata); if (err != default!) {
            Ꮡt.Fatal(err);
        }
    }
    {
        @string got = buf.String();
        @string want = "hi"u8; if (got != want) {
            Ꮡt.Errorf("got %q want %q"u8, got, want);
        }
    }
}

} // end template_internal_test_package
