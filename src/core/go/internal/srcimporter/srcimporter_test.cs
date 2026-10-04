// Copyright 2017 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.
namespace go.go.@internal;

using flag = flag_package;
using build = global::go.go.build_package;
using token = global::go.go.token_package;
using types = global::go.go.types_package;
using testenv = global::go.@internal.testenv_package;
using os = os_package;
using path = path_package;
using filepath = global::go.path.filepath_package;
using strings = strings_package;
using testing = testing_package;
using time = time_package;
using fs = global::go.io.fs_package;
using global::go.@internal;
using global::go.go;
using global::go.io;
using global::go.path;
using static global::go.go.@internal.srcimporter_package;

partial class srcimporter_internal_test_package {

public static void TestMain(ж<testing.M> Ꮡm) {
    flag.Parse();
    build.Default.GOROOT = testenv.GOROOT(default!);
    os.Exit(Ꮡm.Run());
}

internal static time.Duration maxTime => /* 2 * time.Second */ 2000000000;

internal static ж<global::go.go.@internal.srcimporter_package.Importer> importer = New(build.ᏑDefault, token.NewFileSet(), new map<@string, ж<types.Package>>());

internal static void doImport(ж<testing.T> Ꮡt, @string path, @string srcDir) {
    ref var t = ref Ꮡt.DerefOrNull();

    var t0 = time.Now();
    {
        var (_, err) = importer.ImportFrom(path, srcDir, 0); if (err != default!) {
            // don't report an error if there's no buildable Go files
            {
                var (_, nogo) = err._<ж<build.NoGoError>>(ᐧ); if (!nogo) {
                    Ꮡt.Errorf("import %q failed (%v)"u8, path, err);
                }
            }
            return;
        }
    }
    Ꮡt.Logf("import %q: %v"u8, path, time.Since(t0));
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static readonly object testingTimeUsedUpˢ = (@string)"testing time used up"u8;
internal static readonly @string testdataˢ = "testdata"u8;
internal static readonly @string srcˢ = "src"u8;

// walkDir imports the all the packages with the given path
// prefix recursively. It returns the number of packages
// imported and whether importing was aborted because time
// has passed endTime.
internal static (nint, bool) walkDir(ж<testing.T> Ꮡt, @string path, time.Time endTime) {
    if (time.Now().After(endTime)) {
        Ꮡt.Log(testingTimeUsedUpˢ);
        return (0, true);
    }
    // ignore fake packages and testdata directories
    if (path == "builtin"u8 || path == "unsafe"u8 || strings.HasSuffix(path, testdataˢ)) {
        return (0, false);
    }
    var (list, err) = os.ReadDir(filepath.Join(testenv.GOROOT(new srcimporter_internal_test_package.testing_TжTB(Ꮡt)), srcˢ, path));
    if (err != default!) {
        Ꮡt.Fatalf("walkDir %s failed (%v)"u8, path, err);
    }
    nint nimports = 0;
    var hasGoFiles = false;
    foreach (var (_, f) in list) {
        if (f.IsDir()){
            var (n, abort) = walkDir(Ꮡt, filepath.Join(path, f.Name()), endTime);
            nimports += n;
            if (abort) {
                return (nimports, true);
            }
        } else 
        if (strings.HasSuffix(f.Name(), ".go"u8)) {
            hasGoFiles = true;
        }
    }
    if (hasGoFiles) {
        doImport(Ꮡt, path, ""u8);
        nimports++;
    }
    return (nimports, false);
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static readonly object skippingInShortModeˢ = (@string)"skipping in -short mode"u8;

public static void TestImportStdLib(ж<testing.T> Ꮡt) {
    testenv.MustHaveSource(new srcimporter_internal_test_package.testing_TжTB(Ꮡt));
    if (testing.Short() && testenv.Builder() == ""u8) {
        Ꮡt.Skip(skippingInShortModeˢ);
    }
    var dt = maxTime;
    var (nimports, _) = walkDir(Ꮡt, ""u8, time.Now().Add(dt)); // installed packages
    Ꮡt.Logf("tested %d imports"u8, nimports);
}

// go/types.gcCompatibilityMode is off => interface not flattened

[GoType("dyn")] partial struct importedObjectTestsᴛ1 {
    internal @string name;
    internal @string want;
}
internal static slice<importedObjectTestsᴛ1> importedObjectTests = new importedObjectTestsᴛ1[]{
    new("flag.Bool"u8, "func Bool(name string, value bool, usage string) *bool"u8),
    new("io.Reader"u8, "type Reader interface{Read(p []byte) (n int, err error)}"u8),
    new("io.ReadWriter"u8, "type ReadWriter interface{Reader; Writer}"u8),
    new("math.Pi"u8, "const Pi untyped float"u8),
    new("math.Sin"u8, "func Sin(x float64) float64"u8),
    new("math/big.Int"u8, "type Int struct{neg bool; abs nat}"u8),
    new("golang.org/x/text/unicode/norm.MaxSegmentSize"u8, "const MaxSegmentSize untyped int"u8)
}.slice();

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static readonly object invalidTestDataFormatˢ = (@string)"invalid test data format"u8;

public static void TestImportedTypes(ж<testing.T> Ꮡt) {
    testenv.MustHaveSource(new srcimporter_internal_test_package.testing_TжTB(Ꮡt));
    foreach (var (_, test) in importedObjectTests) {
        nint i = strings.LastIndex(test.name, "."u8);
        if (i < 0) {
            Ꮡt.Fatal(invalidTestDataFormatˢ);
        }
        @string importPath = test.name.slice(0, i);
        @string objName = test.name.slice(i + 1);
        var (pkg, err) = importer.ImportFrom(importPath, "."u8, 0);
        if (err != default!) {
            Ꮡt.Error(err);
            continue;
        }
        var obj = pkg.Scope().Lookup(objName);
        if (obj == default!) {
            Ꮡt.Errorf("%s: object not found"u8, test.name);
            continue;
        }
        @string got = types.ObjectString(obj, types.RelativeTo(pkg));
        if (got != test.want) {
            Ꮡt.Errorf("%s: got %q; want %q"u8, test.name, got, test.want);
        }
        {
            var (named, _) = obj.Type()._<ж<types.Named>>(ᐧ); if (named != nil) {
                verifyInterfaceMethodRecvs(Ꮡt, named, 0);
            }
        }
    }
}

// verifyInterfaceMethodRecvs verifies that method receiver types
// are named if the methods belong to a named interface type.
internal static void verifyInterfaceMethodRecvs(ж<testing.T> Ꮡt, ж<types.Named> Ꮡnamed, nint level) {
    ref var named = ref Ꮡnamed.DerefOrNull();

    // avoid endless recursion in case of an embedding bug that lead to a cycle
    if (level > 10) {
        Ꮡt.Errorf("%s: embeds itself"u8, Ꮡnamed.OrTypedNil());
        return;
    }
    var (iface, _) = Ꮡnamed.Underlying()._<ж<types.Interface>>(ᐧ);
    if (iface == nil) {
        return; // not an interface
    }
    // check explicitly declared methods
    for (nint i = 0; i < iface.NumExplicitMethods(); i++) {
        var m = iface.ExplicitMethod(i);
        var recv = m.Type()._<ж<typesꓸSignature>>().Recv();
        if (recv == nil) {
            Ꮡt.Errorf("%s: missing receiver type"u8, m.OrTypedNil());
            continue;
        }
        if (!AreEqual(recv.Type(), Ꮡnamed)) {
            Ꮡt.Errorf("%s: got recv type %s; want %s"u8, m.OrTypedNil(), recv.Type(), Ꮡnamed.OrTypedNil());
        }
    }
    // check embedded interfaces (they are named, too)
    for (nint i = 0; i < iface.NumEmbeddeds(); i++) {
        // embedding of interfaces cannot have cycles; recursion will terminate
        verifyInterfaceMethodRecvs(Ꮡt, iface.Embedded(i), level + 1);
    }
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static readonly @string mathˢ = "math"u8;
internal static readonly @string reimportˢ = "reimport"u8;

public static void TestReimport(ж<testing.T> Ꮡt) {
    testenv.MustHaveSource(new srcimporter_internal_test_package.testing_TжTB(Ꮡt));
    // Reimporting a partially imported (incomplete) package is not supported (see issue #19337).
    // Make sure we recognize the situation and report an error.
    var mathPkg = types.NewPackage(mathˢ, mathˢ); // incomplete package
    var importer = New(build.ᏑDefault, token.NewFileSet(), new map<@string, ж<types.Package>>{[mathPkg.Path()] = mathPkg});
    var (_, err) = importer.ImportFrom(mathˢ, "."u8, 0);
    if (err == default! || !strings.HasPrefix(err.Error(), reimportˢ)) {
        Ꮡt.Errorf("got %v; want reimport error"u8, err);
    }
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static readonly @string goInternalSrcimporterˢ = "go/internal/srcimporter/testdata/issue20855"u8;
internal static readonly @string missingFunctionBodyˢ = "missing function body"u8;
internal static readonly object gotNoPackageDespiteNoˢ = (@string)"got no package despite no hard errors"u8;

public static void TestIssue20855(ж<testing.T> Ꮡt) {
    testenv.MustHaveSource(new srcimporter_internal_test_package.testing_TжTB(Ꮡt));
    var (pkg, err) = importer.ImportFrom(goInternalSrcimporterˢ, "."u8, 0);
    if (err == default! || !strings.Contains(err.Error(), missingFunctionBodyˢ)) {
        Ꮡt.Fatalf("got unexpected or no error: %v"u8, err);
    }
    if (pkg == nil) {
        Ꮡt.Error(gotNoPackageDespiteNoˢ);
    }
}

internal static void testImportPath(ж<testing.T> Ꮡt, @string pkgPath) {
    testenv.MustHaveSource(new srcimporter_internal_test_package.testing_TжTB(Ꮡt));
    @string pkgName = path.Base(pkgPath);
    var (pkg, err) = importer.Import(pkgPath);
    if (err != default!) {
        Ꮡt.Fatal(err);
    }
    if (pkg.Name() != pkgName) {
        Ꮡt.Errorf("got %q; want %q"u8, pkg.Name(), pkgName);
    }
    if (pkg.Path() != pkgPath) {
        Ꮡt.Errorf("got %q; want %q"u8, pkg.Path(), pkgPath);
    }
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static readonly @string testdataIssue23092ˢ = "./testdata/issue23092"u8;

// TestIssue23092 tests relative imports.
public static void TestIssue23092(ж<testing.T> Ꮡt) {
    testImportPath(Ꮡt, testdataIssue23092ˢ);
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static readonly @string goInternalSrcimporterˢ2 = "go/internal/srcimporter/testdata/issue24392"u8;

// TestIssue24392 tests imports against a path containing 'testdata'.
public static void TestIssue24392(ж<testing.T> Ꮡt) {
    testImportPath(Ꮡt, goInternalSrcimporterˢ2);
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static readonly @string cmdCgoInternalTestˢ = "cmd/cgo/internal/test"u8;

public static void TestCgo(ж<testing.T> Ꮡt) {
    testenv.MustHaveGoBuild(new srcimporter_internal_test_package.testing_TжTB(Ꮡt));
    testenv.MustHaveCGO(new srcimporter_internal_test_package.testing_TжTB(Ꮡt));
    ref var buildCtx = ref heap<build.Context>(out var ᏑbuildCtx);
    buildCtx = build.Default;
    var importer = New(ᏑbuildCtx, token.NewFileSet(), new map<@string, ж<types.Package>>());
    var (_, err) = importer.ImportFrom(cmdCgoInternalTestˢ, buildCtx.Dir, 0);
    if (err != default!) {
        Ꮡt.Fatalf("Import failed: %v"u8, err);
    }
}

} // end srcimporter_internal_test_package
