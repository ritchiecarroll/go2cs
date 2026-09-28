// Copyright 2017 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.
namespace go.go;

using Δbuild = global::go.go.build_package;
using token = global::go.go.token_package;
using testenv = global::go.@internal.testenv_package;
using io = io_package;
using os = os_package;
using strings = strings_package;
using testing = testing_package;
using exec = global::go.os.exec_package;
using global::go.@internal;
using global::go.go;
using global::go.os;
using static global::go.go.importer_package;
using types = global::go.go.types_package;

partial class importer_internal_test_package {

public static void TestMain(ж<testing.M> Ꮡm) {
    Δbuild.Default.GOROOT = testenv.GOROOT(default!);
    os.Exit(Ꮡm.Run());
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static readonly @string listˢ = "list"u8;
internal static readonly @string exportˢ = "-export"u8;
internal static readonly @string fContextCompilerExportˢ = "-f={{context.Compiler}}:{{.Export}}"u8;
internal static readonly object golangOrgIssue22500ˢ = (@string)"golang.org/issue/22500"u8;
internal static readonly @string lookupDefaultˢ = "LookupDefault"u8;
internal static readonly @string intˢ = "Int"u8;
internal static readonly @string gorootˢ = "$GOROOT"u8;
internal static readonly @string typeIntˢ = "type Int"u8;
internal static readonly @string lookupCustomˢ = "LookupCustom"u8;
internal static readonly object notSupportedByˢ = (@string)"not supported by GOEXPERIMENT=unified; see go.dev/cl/406319"u8;
internal static readonly @string mathBiggerˢ = "math/bigger"u8;

// Hoisted Go string constant (single allocation; Go keeps it in RODATA)
internal static readonly @string thePackageᶜ = "math/big"u8;

public static void TestForCompiler(ж<testing.T> Ꮡt) {
    ref var t = ref Ꮡt.DerefOrNull();

    testenv.MustHaveGoBuild(new importer_internal_test_package.testing_TжTB(Ꮡt));
    @string thePackage = thePackageᶜ;
    var (@out, err) = testenv.Command(new importer_internal_test_package.testing_TжTB(Ꮡt), testenv.GoToolPath(new importer_internal_test_package.testing_TжTB(Ꮡt)), listˢ, exportˢ, fContextCompilerExportˢ, thePackage).CombinedOutput();
    if (err != default!) {
        Ꮡt.Fatalf("go list %s: %v\n%s"u8, thePackage, err, @out);
    }
    @string export = strings.TrimSpace(((@string)@out));
    var (compiler, target, _) = strings.Cut(export, ":"u8);
    if (compiler == "gccgo"u8) {
        Ꮡt.Skip(golangOrgIssue22500ˢ);
    }
    var fset = token.NewFileSet();
    var fsetʗ1 = fset;
    Ꮡt.Run(lookupDefaultˢ, (ж<testing.T> tΔ1) => {
        var imp = ForCompiler(fsetʗ1, compiler, default!);
        var (pkg, errΔ1) = imp.Import(thePackage);
        if (errΔ1 != default!) {
            tΔ1.Fatal(errΔ1);
        }
        if (pkg.Path() != thePackage) {
            tΔ1.Fatalf("Path() = %q, want %q"u8, pkg.Path(), thePackage);
        }
        // Check that the fileset positions are accurate.
        // https://github.com/golang/go#28995
        var mathBigInt = pkg.Scope().Lookup(intˢ);
        var posn = fsetʗ1.Position(mathBigInt.Pos()); // "$GOROOT/src/math/big/int.go:25:1"
        @string filename = strings.Replace(posn.Filename, gorootˢ, testenv.GOROOT(new importer_internal_test_package.testing_TжTB(tΔ1)), 1);
        (var data, errΔ1) = os.ReadFile(filename);
        if (errΔ1 != default!) {
            tΔ1.Fatalf("can't read file containing declaration of math/big.Int: %v"u8, errΔ1);
        }
        var lines = strings.Split(((@string)data), "\n"u8);
        if (posn.Line > len(lines) || !strings.HasPrefix(lines[posn.Line - 1], typeIntˢ)) {
            tΔ1.Fatalf("Object %v position %s does not contain its declaration"u8,
                mathBigInt, posn);
        }
    });
    var fsetʗ2 = fset;
    Ꮡt.Run(lookupCustomˢ, (ж<testing.T> tΔ2) => {
        // TODO(mdempsky): Decide whether to remove this test, or to fix
        // support for it in unified IR. It's not clear that we actually
        // need to support importing "math/big" as "math/bigger", for
        // example. cmd/link no longer supports that.
        if (true) {
            /* was buildcfg.Experiment.Unified */
            tΔ2.Skip(notSupportedByˢ);
        }
        var lookup = (io.ReadCloser, error) (@string path) => {
            if (path != "math/bigger"u8) {
                tΔ2.Fatalf("lookup called with unexpected path %q"u8, path);
            }
            var (f, errΔ2) = os.Open(target);
            if (errΔ2 != default!) {
                tΔ2.Fatal(errΔ2);
            }
            return (new importer_internal_test_package.os_FileжReadCloser(f), default!);
        };
        var imp = ForCompiler(fsetʗ2, compiler, new Func<@string, (io.ReadCloser, error)>(lookup));
        var (pkg, errΔ3) = imp.Import(mathBiggerˢ);
        if (errΔ3 != default!) {
            tΔ2.Fatal(errΔ3);
        }
        // Even though we open math/big.a, the import request was for math/bigger
        // and that should be recorded in pkg.Path(), at least for the gc toolchain.
        if (pkg.Path() != "math/bigger"u8) {
            tΔ2.Fatalf("Path() = %q, want %q"u8, pkg.Path(), mathBiggerˢ);
        }
    });
}

} // end importer_internal_test_package
