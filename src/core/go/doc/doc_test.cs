// Copyright 2012 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.
namespace go.go;

using bytes = bytes_package;
using flag = flag_package;
using fmt = fmt_package;
using ast = global::go.go.ast_package;
using parser = global::go.go.parser_package;
using printer = global::go.go.printer_package;
using token = global::go.go.token_package;
using fs = global::go.io.fs_package;
using os = os_package;
using filepath = global::go.path.filepath_package;
using regexp = regexp_package;
using strings = strings_package;
using testing = testing_package;
using template = text.template_package;
using global::go.go;
using global::go.io;
using global::go.path;
using io = io_package;
using static global::go.go.doc_package;
using text;

partial class doc_internal_test_package {

internal static ж<bool> update = flag.Bool("update", false, "update golden (.out) files");

internal static ж<@string> files = flag.String("files", "", "consider only Go test files matching this regular expression");

internal static ref readonly @string dataDir => ref dataDirᵇ;
private static readonly @string dataDirᵇ = "testdata"u8;

internal static ж<template.Template> templateTxt = readTemplate("template.txt");

internal static ж<template.Template> readTemplate(@string filename) {
    var t = template.New(filename);
    t.Funcs(new template.FuncMap(new map<@string, any>{
        ["node"] = nodeFmt,
        ["synopsis"] = synopsisFmt,
        ["indent"] = indentFmt
    }));
    var (ᴛ1, ᴛ2) = t.ParseFiles(filepath.Join(dataDir, filename));
    return template.Must(ᴛ1, ᴛ2);
}

internal static @string nodeFmt(any node, ж<token.FileSet> fset) {
    ref var buf = ref heap(new bytes.Buffer(), out var Ꮡbuf);
    printer.Fprint(new doc_test_package.bytes_BufferжWriter(Ꮡbuf), fset, node);
    return strings.ReplaceAll(strings.TrimSpace(Ꮡbuf.String()), "\n", "\n\t");
}

internal static @string synopsisFmt(@string s) {
    const nint n = 64;
    if (len(s) > n) {
        // cut off excess text and go back to a word boundary
        s = s[0..(int)(n)];
        {
            nint i = strings.LastIndexAny(s, "\t\n "); if (i >= 0) {
                s = s.slice(0, i);
            }
        }
        s = strings.TrimSpace(s) + " ..."u8;
    }
    return "// "u8 + strings.ReplaceAll(s, "\n", " ");
}

internal static @string indentFmt(@string indent, @string s) {
    @string end = "";
    if (strings.HasSuffix(s, "\n")) {
        end = "\n";
        s = s.slice(0, len(s) - 1);
    }
    return indent + strings.ReplaceAll(s, "\n", "\n"u8 + indent) + end;
}

internal static bool isGoFile(fs.FileInfo fi) {
    @string name = fi.Name();
    return !fi.IsDir() && len(name) > 0 && name[0] != '.' && // ignore .files
        filepath.Ext(name) == ".go"u8;
}

internal partial struct bundle {
    /*embed*/ public ж<global::go.go.doc_package.Package> Package;
    public ж<token.FileSet> FSet;
}

internal static void test(ж<testing.T> t, global::go.go.doc_package.Mode mode) {
    // determine file filter
    var filter = isGoFile;
    if (files.Value != ""u8) {
        var (rx, errΔ1) = regexp.Compile(files.Value);
        if (errΔ1 != nil) {
            t.Fatal(errΔ1);
        }
        var rxʗ1 = rx;
        filter = (fs.FileInfo fi) => isGoFile(fi) && rxʗ1.MatchString(fi.Name());
    }

    // get packages
    var fset = token.NewFileSet();
    var (pkgs, err) = parser.ParseDir(fset, dataDir, filter, parser.ParseComments);
    if (err != nil) {
        t.Fatal(err);
    }

    // test packages
    foreach (var (_, pkg) in pkgs) {
        var fsetʗ1 = fset;
        var pkgʗ1 = pkg;
        t.Run(pkg.Name, (ж<testing.T> tΔ1) => {
            var filesΔ1ᴷ = new stackAppend<ж<ast.File>>();
            @string importPath = dataDir + "/" + pkgʗ1.Name;
            slice<ж<ast.File>> filesΔ1 = default;
            foreach (var (_, f) in pkgʗ1.Files) {
                filesΔ1 = append(filesΔ1ᴷ, filesΔ1, f);
            }
            var (doc, errΔ2) = NewFromFiles(fsetʗ1, filesΔ1, importPath, mode);
            if (errΔ2 != nil) {
                tΔ1.Fatal(errΔ2);
            }

            // golden files always use / in filenames - canonicalize them
            foreach (var (i, filename) in doc.Filenames) {
                doc.Filenames[i] = filepath.ToSlash(filename);
            }

            // print documentation
            ref var buf = ref heap(new bytes.Buffer(), out var Ꮡbuf);
            {
                var errΔ3 = templateTxt.Execute(new doc_test_package.bytes_BufferжWriter(Ꮡbuf), new bundle(doc, fsetʗ1)); if (errΔ3 != nil) {
                    tΔ1.Fatal(errΔ3);
                }
            }
            var got = buf.Bytes();

            // update golden file if necessary
            @string golden = filepath.Join(dataDir, fmt.Sprintf("%s.%d.golden"u8, pkgʗ1.Name, mode));
            if (update.Value) {
                var errΔ4 = os.WriteFile(golden, got, 420);
                if (errΔ4 != nil) {
                    tΔ1.Fatal(errΔ4);
                }
            }

            // get golden file
            (var want, errΔ2) = os.ReadFile(golden);
            if (errΔ2 != nil) {
                tΔ1.Fatal(errΔ2);
            }

            // compare
            if (!bytes.Equal(got, want)) {
                tΔ1.Errorf("package %s\n\tgot:\n%s\n\twant:\n%s", pkgʗ1.Name, got, want);
            }
        });
    }
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static ref readonly @string defaultˢ => ref defaultˢᵇ;
private static readonly @string defaultˢᵇ = "default"u8;
internal static ref readonly @string allDeclsˢ => ref allDeclsˢᵇ;
private static readonly @string allDeclsˢᵇ = "AllDecls"u8;
internal static ref readonly @string allMethodsˢ => ref allMethodsˢᵇ;
private static readonly @string allMethodsˢᵇ = "AllMethods"u8;

public static void Test(ж<testing.T> t) {
    t.Run(defaultˢ, (ж<testing.T> tΔ1) => {
        test(tΔ1, 0);
    });
    t.Run(allDeclsˢ, (ж<testing.T> tΔ2) => {
        test(tΔ2, AllDecls);
    });
    t.Run(allMethodsˢ, (ж<testing.T> tΔ3) => {
        test(tΔ3, AllMethods);
    });
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static ref readonly @string funcsGoˢ => ref funcsGoˢᵇ;
private static readonly @string funcsGoˢᵇ = "funcs.go"u8;
internal static ref readonly @string importPathˢ => ref importPathˢᵇ;
private static readonly @string importPathˢᵇ = "importPath"u8;
internal static ref readonly @string funcsˢ => ref funcsˢᵇ;
private static readonly @string funcsˢᵇ = "Funcs"u8;
internal static ref readonly @string typesˢ => ref typesˢᵇ;
private static readonly @string typesˢᵇ = "Types"u8;

public static void TestFuncs(ж<testing.T> t) {
    var fset = token.NewFileSet();
    var (@file, err) = parser.ParseFile(fset, funcsGoˢ, strings.NewReader(funcsTestFile).OrTypedNil(), parser.ParseComments);
    if (err != nil) {
        t.Fatal(err);
    }
    (var doc, err) = NewFromFiles(fset, slice<ж<ast.File>>([@file]), importPathˢ, (global::go.go.doc_package.Mode)0);
    if (err != nil) {
        t.Fatal(err);
    }

    foreach (var (_, f) in doc.Funcs) {
        f.Decl = default;
    }
    foreach (var (_, ty) in doc.Types) {
        foreach (var (_, f) in ty.Funcs) {
            f.Decl = default;
        }
        foreach (var (_, m) in ty.Methods) {
            m.Decl = default;
        }
    }

    var compareFuncs = (ж<testing.T> tΔ1, @string msg, ж<global::go.go.doc_package.Func> got, ж<global::go.go.doc_package.Func> want) => {
        // ignore Decl and Examples
        got.Decl = default;
        got.Examples = default;
        if (!(got.Doc == want.Doc && got.Name == want.Name && got.Recv == want.Recv && got.Orig == want.Orig && got.Level == want.Level)) {
            tΔ1.Errorf("%s:\ngot  %+v\nwant %+v", msg, got.OrTypedNil(), want.OrTypedNil());
        }
    };

    compareSlices(t, funcsˢ, doc.Funcs, funcsPackage.Funcs, compareFuncs);
    var compareFuncsʗ1 = compareFuncs;
    compareSlices(t, typesˢ, doc.Types, funcsPackage.Types, (ж<testing.T> tΔ2, @string msg, ж<global::go.go.doc_package.Type> got, ж<global::go.go.doc_package.Type> want) => {
        if (got.Name != want.Name) {
            tΔ2.Errorf("%s.Name: got %q, want %q", msg, got.Name, want.Name);
        } else {
            compareSlices(tΔ2, got.Name + ".Funcs"u8, got.Funcs, want.Funcs, compareFuncsʗ1);
            compareSlices(tΔ2, got.Name + ".Methods"u8, got.Methods, want.Methods, compareFuncsʗ1);
        }
    });
}

internal static void compareSlices<E>(ж<testing.T> t, @string name, slice<E> got, slice<E> want, Action<ж<testing.T>, @string, E, E> compareElem) {
    if (len(got) != len(want)) {
        t.Errorf("%s: got %d, want %d", name, len(got), len(want));
    }
    for (nint i = 0; i < len(got) && i < len(want); i++) {
        compareElem(t, fmt.Sprintf("%s[%d]"u8, name, i), got[i], want[i]);
    }
}

internal static ref readonly @string funcsTestFile => ref funcsTestFileᵇ;
private static readonly @string funcsTestFileᵇ = """

package funcs

func F() {}

type S1 struct {
	S2  // embedded, exported
	s3  // embedded, unexported
}

func NewS1()  S1 {return S1{} }
func NewS1p() *S1 { return &S1{} }

func (S1) M1() {}
func (r S1) M2() {}
func(S1) m3() {}		// unexported not shown
func (*S1) P1() {}		// pointer receiver

type S2 int
func (S2) M3() {}		// shown on S2

type s3 int
func (s3) M4() {}		// shown on S1

type G1[T any] struct {
	*s3
}

func NewG1[T any]() G1[T] { return G1[T]{} }

func (G1[T]) MG1() {}
func (*G1[U]) MG2() {}

type G2[T, U any] struct {}

func NewG2[T, U any]() G2[T, U] { return G2[T, U]{} }

func (G2[T, U]) MG3() {}
func (*G2[A, B]) MG4() {}



"""u8.Raw();

internal static ж<global::go.go.doc_package.Package> funcsPackage = Ꮡ(new Package(
    Funcs: slice<ж<global::go.go.doc_package.Func>>([Ꮡ(new global::go.go.doc_package.Func(Name: "F"))]),
    Types: slice<ж<global::go.go.doc_package.Type>>([
        Ꮡ(new global::go.go.doc_package.Type(
            Name: "G1",
            Funcs: slice<ж<global::go.go.doc_package.Func>>([Ꮡ(new global::go.go.doc_package.Func(Name: "NewG1"))]),
            Methods: slice<ж<global::go.go.doc_package.Func>>([
                Ꮡ(new global::go.go.doc_package.Func(Name: "M4", Recv: "G1", // TODO: synthesize a param for G1?
                    Orig: "s3", Level: 1)),
                Ꮡ(new global::go.go.doc_package.Func(Name: "MG1", Recv: "G1[T]", Orig: "G1[T]", Level: 0)),
                Ꮡ(new global::go.go.doc_package.Func(Name: "MG2", Recv: "*G1[U]", Orig: "*G1[U]", Level: 0))
            ]))),
        Ꮡ(new global::go.go.doc_package.Type(
            Name: "G2",
            Funcs: slice<ж<global::go.go.doc_package.Func>>([Ꮡ(new global::go.go.doc_package.Func(Name: "NewG2"))]),
            Methods: slice<ж<global::go.go.doc_package.Func>>([
                Ꮡ(new global::go.go.doc_package.Func(Name: "MG3", Recv: "G2[T, U]", Orig: "G2[T, U]", Level: 0)),
                Ꮡ(new global::go.go.doc_package.Func(Name: "MG4", Recv: "*G2[A, B]", Orig: "*G2[A, B]", Level: 0))
            ]))),
        Ꮡ(new global::go.go.doc_package.Type(
            Name: "S1",
            Funcs: slice<ж<global::go.go.doc_package.Func>>([Ꮡ(new global::go.go.doc_package.Func(Name: "NewS1")), Ꮡ(new global::go.go.doc_package.Func(Name: "NewS1p"))]),
            Methods: slice<ж<global::go.go.doc_package.Func>>([
                Ꮡ(new global::go.go.doc_package.Func(Name: "M1", Recv: "S1", Orig: "S1", Level: 0)),
                Ꮡ(new global::go.go.doc_package.Func(Name: "M2", Recv: "S1", Orig: "S1", Level: 0)),
                Ꮡ(new global::go.go.doc_package.Func(Name: "M4", Recv: "S1", Orig: "s3", Level: 1)),
                Ꮡ(new global::go.go.doc_package.Func(Name: "P1", Recv: "*S1", Orig: "*S1", Level: 0))
            ]))),
        Ꮡ(new global::go.go.doc_package.Type(
            Name: "S2",
            Methods: slice<ж<global::go.go.doc_package.Func>>([
                Ꮡ(new global::go.go.doc_package.Func(Name: "M3", Recv: "S2", Orig: "S2", Level: 0))
            ])))
    ])
));

} // end doc_internal_test_package
