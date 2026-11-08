// Copyright 2013 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.
namespace go.go;

using bytes = bytes_package;
using fmt = fmt_package;
using ast = global::go.go.ast_package;
using Δdoc = global::go.go.doc_package;
using format = global::go.go.format_package;
using parser = global::go.go.parser_package;
using token = global::go.go.token_package;
using diff = global::go.@internal.diff_package;
using txtar = global::go.@internal.txtar_package;
using filepath = path.filepath_package;
using reflect = reflect_package;
using strings = strings_package;
using testing = testing_package;
using global::go.@internal;
using global::go.go;
using io = io_package;
using path;
using static global::go.go.doc_internal_test_package;

partial class doc_test_package {

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static ref readonly @string testdataˢ => ref testdataˢᵇ;
private static readonly @string testdataˢᵇ = "testdata"u8;
internal static ref readonly @string examplesˢ => ref examplesˢᵇ;
private static readonly @string examplesˢᵇ = "examples"u8;
internal static ref readonly @string wantˢ => ref wantˢᵇ;
private static readonly @string wantˢᵇ = "want"u8;
internal static ref readonly @string gotˢ => ref gotˢᵇ;
private static readonly @string gotˢᵇ = "got"u8;

public static void TestExamples(ж<testing.T> t) {
    @string dir = filepath.Join(testdataˢ, examplesˢ);
    var (filenames, err) = filepath.Glob(filepath.Join(dir, "*.go"));
    if (err != nil) {
        t.Fatal(err);
    }
    foreach (var (_, filename) in range(filenames)) {
        t.Run(strings.TrimSuffix(filepath.Base(filename), ".go"), (ж<testing.T> tΔ1) => {
            var fset = token.NewFileSet();
            var (astFile, errΔ1) = parser.ParseFile(fset, filename, default, parser.ParseComments);
            if (errΔ1 != nil) {
                tΔ1.Fatal(errΔ1);
            }
            @string goldenFilename = strings.TrimSuffix(filename, ".go") + ".golden"u8;
            (var archive, errΔ1) = txtar.ParseFile(goldenFilename);
            if (errΔ1 != nil) {
                tΔ1.Fatal(errΔ1);
            }
            var golden = new map<@string, @string>{};
            foreach (var (_, f) in range(archive.Files)) {
                golden[f.Name] = strings.TrimSpace((@string)f.Data);
            }

            // Collect the results of doc.Examples in a map keyed by example name.
            var examples = new map<@string, ж<Δdoc.Example>>{};
            foreach (var (_, e) in range(Δdoc.Examples(astFile))) {
                examples[e.Name] = e;
                // Treat missing sections in the golden as empty.
                foreach (var (_, kind) in range(slice<@string>(["Play", "Output"]))) {
                    @string key = e.Name + "."u8 + kind;
                    var (_, ok) = golden[key, ꟷ]; if (!ok) {
                        golden[key] = ""u8;
                    }
                }
            }

            // Each section in the golden file corresponds to an example we expect
            // to see.
            foreach (var (sectionName, want) in golden) {
                var (name, kind, found) = strings.Cut(sectionName, ".");
                if (!found) {
                    tΔ1.Fatalf("bad section name %q, want EXAMPLE_NAME.KIND", sectionName);
                }
                var ex = examples[name];
                if (ex == nil) {
                    tΔ1.Fatalf("no example named %q", name);
                }

                @string got = default;
                var exprᴛ1 = kind;
                switch (ᐧ) {
                case {} when exprᴛ1 == "Play"u8: {
                    got = strings.TrimSpace(formatFile(tΔ1, fset, ex.Play));
                    break;
                }
                case {} when exprᴛ1 == "Output"u8: {
                    got = strings.TrimSpace(ex.Output);
                    break;
                }
                default: {
                    tΔ1.Fatalf("bad section kind %q", kind);
                    break;
                }}

                if (got != want) {
                    tΔ1.Errorf("%s mismatch:\n%s", sectionName,
                        diff.Diff(wantˢ, slice<byte>(want), gotˢ, slice<byte>(got)));
                }
            }
        });
    }
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static ref readonly @string nilˢ => ref nilˢᵇ;
private static readonly @string nilˢᵇ = "<nil>"u8;

internal static @string formatFile(ж<testing.T> t, ж<token.FileSet> fset, ж<ast.File> n) {
    t.Helper();
    if (n == nil) {
        return nilˢ;
    }
    ref var buf = ref heap(new bytes.Buffer(), out var Ꮡbuf);
    var err = format.Node(new doc_test_package.bytes_BufferжWriter(Ꮡbuf), fset, n.OrTypedNil()); if (err != nil) {
        t.Fatal(err);
    }
    return Ꮡbuf.String();
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static ref readonly @string srcGoˢ => ref srcGoˢᵇ;
private static readonly @string srcGoˢᵇ = "src.go"u8;
internal static ref readonly @string srcTestGoˢ => ref srcTestGoˢᵇ;
private static readonly @string srcTestGoˢᵇ = "src_test.go"u8;
internal static ref readonly @string exampleComPˢ => ref exampleComPˢᵇ;
private static readonly @string exampleComPˢᵇ = "example.com/p"u8;

// Hoisted Go string constant (single allocation; Go keeps it in RODATA)
internal static ref readonly @string srcᶜ => ref srcᶜᵇ;
private static readonly @string srcᶜᵇ = """

// This is the package comment.
package p

import "fmt"

// This comment is associated with the Greet function.
func Greet(who string) {
	fmt.Printf("Hello, %s!\n", who)
}

"""u8.Raw();

// Hoisted Go string constant (single allocation; Go keeps it in RODATA)
internal static ref readonly @string testᶜ => ref testᶜᵇ;
private static readonly @string testᶜᵇ = """

package p_test

// This comment is associated with the ExampleGreet_world example.
func ExampleGreet_world() {
	Greet("world")
}

"""u8.Raw();

// This example illustrates how to use NewFromFiles
// to compute package documentation with examples.
public static void ExampleNewFromFiles() {
    // src and test are two source files that make up
    // a package whose documentation will be computed.
    @string src = srcᶜ;
    @string test = testᶜ;

    // Create the AST by parsing src and test.
    var fset = token.NewFileSet();
    var files = slice<ж<ast.File>>([
        mustParse(fset, srcGoˢ, src),
        mustParse(fset, srcTestGoˢ, test)
    ]);

    // Compute package documentation with examples.
    var (p, err) = Δdoc.NewFromFiles(fset, files, exampleComPˢ);
    if (err != nil) {
        throw panic(err);
    }

    fmt.Printf("package %s - %s"u8, p.Name, p.Doc);
    fmt.Printf("func %s - %s"u8, p.Funcs[0].Name, p.Funcs[0].Doc);
    fmt.Printf(" ⤷ example with suffix %q - %s"u8, p.Funcs[0].Examples[0].Suffix, p.Funcs[0].Examples[0].Doc);

    // Output:
    // package p - This is the package comment.
    // func Greet - This comment is associated with the Greet function.
    //  ⤷ example with suffix "world" - This comment is associated with the ExampleGreet_world example.
}

// Hoisted Go string constant (single allocation; Go keeps it in RODATA)
internal static ref readonly @string srcᶜ1 => ref srcᶜ1ᵇ;
private static readonly @string srcᶜ1ᵇ = """

package p

const Const1 = 0
var   Var1   = 0

type (
	Type1     int
	Type1_Foo int
	Type1_foo int
	type2     int

	Embed struct { Type1 }
	Uembed struct { type2 }
)

func Func1()     {}
func Func1_Foo() {}
func Func1_foo() {}
func func2()     {}

func (Type1) Func1() {}
func (Type1) Func1_Foo() {}
func (Type1) Func1_foo() {}
func (Type1) func2() {}

func (type2) Func1() {}

type (
	Conflict          int
	Conflict_Conflict int
	Conflict_conflict int
)

func (Conflict) Conflict() {}

func GFunc[T any]() {}

type GType[T any] int

func (GType[T]) M() {}

"""u8.Raw();

// Hoisted Go string constant (single allocation; Go keeps it in RODATA)
internal static ref readonly @string testᶜ1 => ref testᶜ1ᵇ;
private static readonly @string testᶜ1ᵇ = """

package p_test

func ExampleConst1() {} // invalid - no support for consts and vars
func ExampleVar1()   {} // invalid - no support for consts and vars

func Example()               {}
func Example_()              {} // invalid - suffix must start with a lower-case letter
func Example_suffix()        {}
func Example_suffix_xX_X_x() {}
func Example_世界()           {} // invalid - suffix must start with a lower-case letter
func Example_123()           {} // invalid - suffix must start with a lower-case letter
func Example_BadSuffix()     {} // invalid - suffix must start with a lower-case letter

func ExampleType1()               {}
func ExampleType1_()              {} // invalid - suffix must start with a lower-case letter
func ExampleType1_suffix()        {}
func ExampleType1_BadSuffix()     {} // invalid - suffix must start with a lower-case letter
func ExampleType1_Foo()           {}
func ExampleType1_Foo_suffix()    {}
func ExampleType1_Foo_BadSuffix() {} // invalid - suffix must start with a lower-case letter
func ExampleType1_foo()           {}
func ExampleType1_foo_suffix()    {}
func ExampleType1_foo_Suffix()    {} // matches Type1, instead of Type1_foo
func Exampletype2()               {} // invalid - cannot match unexported

func ExampleFunc1()               {}
func ExampleFunc1_()              {} // invalid - suffix must start with a lower-case letter
func ExampleFunc1_suffix()        {}
func ExampleFunc1_BadSuffix()     {} // invalid - suffix must start with a lower-case letter
func ExampleFunc1_Foo()           {}
func ExampleFunc1_Foo_suffix()    {}
func ExampleFunc1_Foo_BadSuffix() {} // invalid - suffix must start with a lower-case letter
func ExampleFunc1_foo()           {}
func ExampleFunc1_foo_suffix()    {}
func ExampleFunc1_foo_Suffix()    {} // matches Func1, instead of Func1_foo
func Examplefunc1()               {} // invalid - cannot match unexported
func ExampleFunc1_params(a int)   {} // invalid - has parameter
func ExampleFunc1_results() int   {} // invalid - has results

func ExampleType1_Func1()               {}
func ExampleType1_Func1_()              {} // invalid - suffix must start with a lower-case letter
func ExampleType1_Func1_suffix()        {}
func ExampleType1_Func1_BadSuffix()     {} // invalid - suffix must start with a lower-case letter
func ExampleType1_Func1_Foo()           {}
func ExampleType1_Func1_Foo_suffix()    {}
func ExampleType1_Func1_Foo_BadSuffix() {} // invalid - suffix must start with a lower-case letter
func ExampleType1_Func1_foo()           {}
func ExampleType1_Func1_foo_suffix()    {}
func ExampleType1_Func1_foo_Suffix()    {} // matches Type1.Func1, instead of Type1.Func1_foo
func ExampleType1_func2()               {} // matches Type1, instead of Type1.func2

func ExampleEmbed_Func1()         {} // invalid - no support for forwarded methods from embedding exported type
func ExampleUembed_Func1()        {} // methods from embedding unexported types are OK
func ExampleUembed_Func1_suffix() {}

func ExampleConflict_Conflict()        {} // ambiguous with either Conflict or Conflict_Conflict type
func ExampleConflict_conflict()        {} // ambiguous with either Conflict or Conflict_conflict type
func ExampleConflict_Conflict_suffix() {} // ambiguous with either Conflict or Conflict_Conflict type
func ExampleConflict_conflict_suffix() {} // ambiguous with either Conflict or Conflict_conflict type

func ExampleGFunc() {}
func ExampleGFunc_suffix() {}

func ExampleGType_M() {}
func ExampleGType_M_suffix() {}

"""u8.Raw();

public static void TestClassifyExamples(ж<testing.T> t) {
    @string src = srcᶜ1;
    @string test = testᶜ1;

    // Parse literal source code as a *doc.Package.
    var fset = token.NewFileSet();
    var files = slice<ж<ast.File>>([
        mustParse(fset, srcGoˢ, src),
        mustParse(fset, srcTestGoˢ, test)
    ]);
    var (p, err) = Δdoc.NewFromFiles(fset, files, exampleComPˢ);
    if (err != nil) {
        t.Fatalf("doc.NewFromFiles: %v", err);
    }

    // Collect the association of examples to top-level identifiers.
    var got = new map<@string, slice<@string>>{};
    got[""u8] = exampleNames(p.Examples);
    foreach (var (_, f) in range(p.Funcs)) {
        got[f.Name] = exampleNames(f.Examples);
    }
    foreach (var (_, tΔ1) in range(p.Types)) {
        got[tΔ1.Name] = exampleNames(tΔ1.Examples);
        foreach (var (_, f) in range(tΔ1.Funcs)) {
            got[f.Name] = exampleNames(f.Examples);
        }
        foreach (var (_, m) in range(tΔ1.Methods)) {
            got[tΔ1.Name + "."u8 + m.Name] = exampleNames(m.Examples);
        }
    }

    var want = new map<@string, slice<@string>>{
        [""] = ["", "suffix", "suffix_xX_X_x"], // Package-level examples.

        ["Type1"] = ["", "foo_Suffix", "func2", "suffix"],
        ["Type1_Foo"] = ["", "suffix"],
        ["Type1_foo"] = ["", "suffix"],

        ["Func1"] = ["", "foo_Suffix", "suffix"],
        ["Func1_Foo"] = ["", "suffix"],
        ["Func1_foo"] = ["", "suffix"],

        ["Type1.Func1"] = ["", "foo_Suffix", "suffix"],
        ["Type1.Func1_Foo"] = ["", "suffix"],
        ["Type1.Func1_foo"] = ["", "suffix"],

        ["Uembed.Func1"] = ["", "suffix"],

        // These are implementation dependent due to the ambiguous parsing.
        ["Conflict_Conflict"] = ["", "suffix"],
        ["Conflict_conflict"] = ["", "suffix"],

        ["GFunc"] = ["", "suffix"],
        ["GType.M"] = ["", "suffix"]
    };

    foreach (var (id, _) in got) {
        if (!reflect.DeepEqual(got[id], want[id])) {
            t.Errorf("classification mismatch for %q:\ngot  %q\nwant %q", id, got[id], want[id]);
        }
        delete(want, id);
    }
    if (len(want) > 0) {
        t.Errorf("did not find:\n%q", want);
    }
}

internal static slice<@string> /*out*/ exampleNames(slice<ж<Δdoc.Example>> exs) {
    slice<@string> @out = default;

    foreach (var (_, ex) in range(exs)) {
        @out = append(@out, ex.Suffix);
    }
    return @out;
}

internal static ж<ast.File> mustParse(ж<token.FileSet> fset, @string filename, @string src) {
    var (f, err) = parser.ParseFile(fset, filename, src, (parser.Mode)(parser.ParseComments | parser.SkipObjectResolution));
    if (err != nil) {
        throw panic(err);
    }
    return f;
}

} // end doc_test_package
