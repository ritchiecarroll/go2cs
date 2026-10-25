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
internal static readonly @string goTypesˢ = "go/types"u8;

public static void TestSelf(ж<testing.T> Ꮡt) {
    testenv.MustHaveGoBuild(new types_test_package.testing_TжTB(Ꮡt)); // The Go command is needed for the importer to determine the locations of stdlib .a files.

    var fset = token.NewFileSet();
    var (files, err) = pkgFiles(fset, "."u8);
    if (err != default!) {
        Ꮡt.Fatal(err);
    }

    ref var conf = ref heap<types.Config>(out var Ꮡconf);
    conf = new Config(Importer: defaultImporter(fset));
    (_, err) = Ꮡconf.Check(goTypesˢ, fset, files, nil);
    if (err != default!) {
        Ꮡt.Fatal(err);
    }
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static readonly @string internalˢ = "internal"u8;
internal static readonly @string gcimporterˢ = "gcimporter"u8;
internal static readonly @string funcbodiesˢ = "funcbodies"u8;
internal static readonly @string nofuncbodiesˢ = "nofuncbodies"u8;
internal static readonly @string infoˢ = "info"u8;
internal static readonly @string noinfoˢ = "noinfo"u8;

public static void BenchmarkCheck(ж<testing.B> Ꮡb) {
    testenv.MustHaveGoBuild(new types_test_package.testing_BжTB(Ꮡb)); // The Go command is needed for the importer to determine the locations of stdlib .a files.

    foreach (var (_, p) in new @string[]{
        "net/http"u8,
        "go/parser"u8,
        "go/constant"u8,
        "runtime"u8,
        filepath.Join("go"u8, internalˢ, gcimporterˢ)
    }.slice()) {
        Ꮡb.Run(path.Base(p), (ж<testing.B> bΔ1) => {
            @string pathΔ1 = filepath.Join(".."u8, "..", p);
            foreach (var (_, ignoreFuncBodies) in new bool[]{false, true}.slice()) {
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
internal static readonly @string linesSˢ = "lines/s"u8;

internal static void runbench(ж<testing.B> Ꮡb, @string path, bool ignoreFuncBodies, bool writeInfo) {
    ref var b = ref Ꮡb.DerefOrNull();

    var fset = token.NewFileSet();
    var (files, err) = pkgFiles(fset, path);
    if (err != default!) {
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
        ref var conf = ref heap<types.Config>(out var Ꮡconf);
        conf = new Config(
            IgnoreFuncBodies: ignoreFuncBodies,
            Importer: defaultImporter(fset)
        );
        ж<typesꓸInfo> info = default!;
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
            var (_, errΔ1) = Ꮡconf.Check(path, fset, files, info); if (errΔ1 != default!) {
                Ꮡb.Fatal(errΔ1);
            }
        }
    }
    b.StopTimer();
    b.ReportMetric((float64)lines * (float64)b.N / time.Since(start).Seconds(), linesSˢ);
}

internal static (slice<ж<ast.File>>, error) pkgFiles(ж<token.FileSet> Ꮡfset, @string path) {
    var filesᴷ = new stackAppend<ж<ast.File>>(moves: true);

    var (filenames, err) = pkgFilenames(path, true); // from stdlib_test.go
    if (err != default!) {
        return (default!, err);
    }

    slice<ж<ast.File>> files = default!;
    foreach (var (_, filename) in filenames) {
        var (@file, errΔ1) = parser.ParseFile(Ꮡfset, filename, default!, 0);
        if (errΔ1 != default!) {
            return (default!, errΔ1);
        }
        files = append(filesᴷ, files, @file);
    }

    files = move2heap(filesᴷ, files);
    return (files, default!);
}

} // end types_test_package
