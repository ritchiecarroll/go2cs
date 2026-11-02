// Copyright 2013 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.
namespace go.go;

using ast = global::go.go.ast_package;
using parser = global::go.go.parser_package;
using token = global::go.go.token_package;
using testenv = global::go.@internal.testenv_package;
using path = path_package;
using filepath = global::go.path.filepath_package;
using testing = testing_package;
using time = time_package;
using static global::go.go.types_package;
using global::go.@internal;
using global::go.go;
using global::go.path;
using static global::go.go.types_internal_test_package;
using types = global::go.go.types_package;

partial class types_test_package {

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static ref readonly @string goTypesˢ => ref goTypesˢᵇ;
private static readonly @string goTypesˢᵇ = "go/types"u8;

public static void TestSelf(ж<testing.T> t) {
    testenv.MustHaveGoBuild(new types_test_package.testing_TжTB(t)); // The Go command is needed for the importer to determine the locations of stdlib .a files.

    var fset = token.NewFileSet();
    var (files, err) = pkgFiles(fset, ".");
    if (err != nil) {
        t.Fatal(err);
    }

    ref var conf = ref heap<types.Config>(new Config(Importer: defaultImporter(fset)), out var Ꮡconf);
    (_, err) = Ꮡconf.Check(goTypesˢ, fset, files, nil);
    if (err != nil) {
        t.Fatal(err);
    }
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static ref readonly @string internalˢ => ref internalˢᵇ;
private static readonly @string internalˢᵇ = "internal"u8;
internal static ref readonly @string gcimporterˢ => ref gcimporterˢᵇ;
private static readonly @string gcimporterˢᵇ = "gcimporter"u8;
internal static ref readonly @string funcbodiesˢ => ref funcbodiesˢᵇ;
private static readonly @string funcbodiesˢᵇ = "funcbodies"u8;
internal static ref readonly @string nofuncbodiesˢ => ref nofuncbodiesˢᵇ;
private static readonly @string nofuncbodiesˢᵇ = "nofuncbodies"u8;
internal static ref readonly @string infoˢ => ref infoˢᵇ;
private static readonly @string infoˢᵇ = "info"u8;
internal static ref readonly @string noinfoˢ => ref noinfoˢᵇ;
private static readonly @string noinfoˢᵇ = "noinfo"u8;

public static void BenchmarkCheck(ж<testing.B> b) {
    testenv.MustHaveGoBuild(new types_test_package.testing_BжTB(b)); // The Go command is needed for the importer to determine the locations of stdlib .a files.

    foreach (var (_, p) in slice<@string>([
        "net/http",
        "go/parser",
        "go/constant",
        "runtime",
        filepath.Join("go", internalˢ, gcimporterˢ)
    ])) {
        b.Run(path.Base(p), (ж<testing.B> bΔ1) => {
            @string pathΔ1 = filepath.Join("..", "..", p);
            foreach (var (_, ignoreFuncBodies) in slice<bool>([false, true])) {
                @string name = funcbodiesˢ;
                if (ignoreFuncBodies) {
                    name = nofuncbodiesˢ;
                }
                bΔ1.Run(name, (ж<testing.B> bΔ2) => {
                    bΔ2.Run(infoˢ, (ж<testing.B> bΔ3) => {
                        runbench(bΔ3, pathΔ1, ignoreFuncBodies, true);
                    });
                    bΔ2.Run(noinfoˢ, (ж<testing.B> bΔ4) => {
                        runbench(bΔ4, pathΔ1, ignoreFuncBodies, false);
                    });
                });
            }
        });
    }
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static ref readonly @string linesSˢ => ref linesSˢᵇ;
private static readonly @string linesSˢᵇ = "lines/s"u8;

internal static void runbench(ж<testing.B> Ꮡb, @string path, bool ignoreFuncBodies, bool writeInfo) {
    ref var b = ref Ꮡb.DerefOrNull();

    var fset = token.NewFileSet();
    var (files, err) = pkgFiles(fset, path);
    if (err != nil) {
        Ꮡb.Fatal(err);
    }
    // determine line count
    nint lines = 0;
    fset.Iterate((ж<tokenꓸFile> f) => {
        lines += f.LineCount();
        return true;
    });

    b.ResetTimer();
    var start = time.Now();
    for (nint i = 0; i < b.N; i++) {
        ref var conf = ref heap<types.Config>(new Config(
            IgnoreFuncBodies: ignoreFuncBodies,
            Importer: defaultImporter(fset)
        ), out var Ꮡconf);
        ж<typesꓸInfo> info = default;
        if (writeInfo) {
            info = Ꮡ(new typesꓸInfo(
                Types: new map<ast.Expr, types.TypeAndValue>(),
                Defs: new map<ж<ast.Ident>, types.Object>(),
                Uses: new map<ж<ast.Ident>, types.Object>(),
                Implicits: new map<ast.Node, types.Object>(),
                Selections: new map<ж<ast.SelectorExpr>, ж<types.Selection>>(),
                Scopes: new map<ast.Node, ж<typesꓸScope>>()
            ));
        }
        {
            var (_, errΔ1) = Ꮡconf.Check(path, fset, files, info); if (errΔ1 != nil) {
                Ꮡb.Fatal(errΔ1);
            }
        }
    }
    b.StopTimer();
    b.ReportMetric((float64)lines * (float64)b.N / time.Since(start).Seconds(), linesSˢ);
}

internal static (slice<ж<ast.File>>, error) pkgFiles(ж<token.FileSet> fset, @string path) {
    var filesᴷ = new stackAppend<ж<ast.File>>(moves: true);

    var (filenames, err) = pkgFilenames(path, true); // from stdlib_test.go
    if (err != nil) {
        return (default, err);
    }

    slice<ж<ast.File>> files = default;
    foreach (var (_, filename) in filenames) {
        var (@file, errΔ1) = parser.ParseFile(fset, filename, default, 0);
        if (errΔ1 != nil) {
            return (default, errΔ1);
        }
        files = append(filesᴷ, files, @file);
    }

    files = move2heap(filesᴷ, files);
    return (files, default);
}

} // end types_test_package
