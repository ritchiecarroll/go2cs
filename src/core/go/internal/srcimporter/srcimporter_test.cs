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

public static void TestMain(ж<testing.M> m) {
    flag.Parse();
    build.Default.GOROOT = testenv.GOROOT(default);
    os.Exit(m.Run());
}

internal static time.Duration maxTime => /* 2 * time.Second */ 2000000000;

internal static ж<global::go.go.@internal.srcimporter_package.Importer> importer = New(build.ᏑDefault, token.NewFileSet(), new map<@string, ж<types.Package>>());

internal static void doImport(ж<testing.T> t, @string path, @string srcDir) {
    var t0 = time.Now();
    var (_, err) = importer.ImportFrom(path, srcDir, 0); if (err != nil) {
        // don't report an error if there's no buildable Go files
        var (_, nogo) = err._<ж<build.NoGoError>>(ᐧ); if (!nogo) {
            t.Errorf("import %q failed (%v)", path, err);
        }
        return;
    }
    t.Logf("import %q: %v", path, time.Since(t0));
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static readonly object testingTimeUsedUpˢ = (@string)"testing time used up"u8;
internal static ref readonly @string testdataˢ => ref testdataˢᵇ;
private static readonly @string testdataˢᵇ = "testdata"u8;
internal static ref readonly @string srcˢ => ref srcˢᵇ;
private static readonly @string srcˢᵇ = "src"u8;

// walkDir imports the all the packages with the given path
// prefix recursively. It returns the number of packages
// imported and whether importing was aborted because time
// has passed endTime.
internal static (nint, bool) walkDir(ж<testing.T> t, @string path, time.Time endTime) {
    if (time.Now().After(endTime)) {
        t.Log(testingTimeUsedUpˢ);
        return (0, true);
    }

    // ignore fake packages and testdata directories
    if (path == "builtin"u8 || path == "unsafe"u8 || strings.HasSuffix(path, testdataˢ)) {
        return (0, false);
    }

    var (list, err) = os.ReadDir(filepath.Join(testenv.GOROOT(new srcimporter_internal_test_package.testing_TжTB(t)), srcˢ, path));
    if (err != nil) {
        t.Fatalf("walkDir %s failed (%v)", path, err);
    }

    nint nimports = 0;
    var hasGoFiles = false;
    foreach (var (_, f) in range(list)) {
        if (f.IsDir()) {
            var (n, abort) = walkDir(t, filepath.Join(path, f.Name()), endTime);
            nimports += n;
            if (abort) {
                return (nimports, true);
            }
        } else if (strings.HasSuffix(f.Name(), ".go")) {
            hasGoFiles = true;
        }
    }

    if (hasGoFiles) {
        doImport(t, path, ""u8);
        nimports++;
    }

    return (nimports, false);
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static readonly object skippingInShortModeˢ = (@string)"skipping in -short mode"u8;

public static void TestImportStdLib(ж<testing.T> t) {
    testenv.MustHaveSource(new srcimporter_internal_test_package.testing_TжTB(t));

    if (testing.Short() && testenv.Builder() == ""u8) {
        t.Skip(skippingInShortModeˢ);
    }
    var dt = maxTime;
    var (nimports, _) = walkDir(t, ""u8, time.Now().Add(dt)); // installed packages
    t.Logf("tested %d imports", nimports);
}

partial struct importedObjectTestsᴛ1 /*dyn*/ {
    internal @string name;
    internal @string want;
}
internal static slice<importedObjectTestsᴛ1> importedObjectTests = [
    new("flag.Bool", "func Bool(name string, value bool, usage string) *bool"),
    new("io.Reader", "type Reader interface{Read(p []byte) (n int, err error)}"),
    new("io.ReadWriter", "type ReadWriter interface{Reader; Writer}"), // go/types.gcCompatibilityMode is off => interface not flattened
    new("math.Pi", "const Pi untyped float"),
    new("math.Sin", "func Sin(x float64) float64"),
    new("math/big.Int", "type Int struct{neg bool; abs nat}"),
    new("golang.org/x/text/unicode/norm.MaxSegmentSize", "const MaxSegmentSize untyped int")
];

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static readonly object invalidTestDataFormatˢ = (@string)"invalid test data format"u8;

public static void TestImportedTypes(ж<testing.T> t) {
    testenv.MustHaveSource(new srcimporter_internal_test_package.testing_TжTB(t));

    foreach (var (_, test) in range(importedObjectTests)) {
        nint i = strings.LastIndex(test.name, ".");
        if (i < 0) {
            t.Fatal(invalidTestDataFormatˢ);
        }
        @string importPath = test.name.slice(0, i);
        @string objName = test.name.slice(i + 1);

        var (pkg, err) = importer.ImportFrom(importPath, ".", 0);
        if (err != nil) {
            t.Error(err);
            continue;
        }

        var obj = pkg.Scope().Lookup(objName);
        if (obj == nil) {
            t.Errorf("%s: object not found", test.name);
            continue;
        }

        @string got = types.ObjectString(obj, types.RelativeTo(pkg));
        if (got != test.want) {
            t.Errorf("%s: got %q; want %q", test.name, got, test.want);
        }

        var (named, _) = obj.Type()._<ж<types.Named>>(ᐧ); if (named != nil) {
            verifyInterfaceMethodRecvs(t, named, 0);
        }
    }
}

// verifyInterfaceMethodRecvs verifies that method receiver types
// are named if the methods belong to a named interface type.
internal static void verifyInterfaceMethodRecvs(ж<testing.T> t, ж<types.Named> named, nint level) {
    // avoid endless recursion in case of an embedding bug that lead to a cycle
    if (level > 10) {
        t.Errorf("%s: embeds itself", named.OrTypedNil());
        return;
    }

    var (iface, _) = named.Underlying()._<ж<types.Interface>>(ᐧ);
    if (iface == nil) {
        return; // not an interface
    }

    // check explicitly declared methods
    for (nint i = 0; i < iface.NumExplicitMethods(); i++) {
        var m = iface.ExplicitMethod(i);
        var recv = m.Signature().Recv();
        if (recv == nil) {
            t.Errorf("%s: missing receiver type", m.OrTypedNil());
            continue;
        }
        if (!AreEqual(recv.Type(), named)) {
            t.Errorf("%s: got recv type %s; want %s", m.OrTypedNil(), recv.Type(), named.OrTypedNil());
        }
    }

    // check embedded interfaces (they are named, too)
    for (nint i = 0; i < iface.NumEmbeddeds(); i++) {
        // embedding of interfaces cannot have cycles; recursion will terminate
        verifyInterfaceMethodRecvs(t, iface.Embedded(i), level + 1);
    }
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static ref readonly @string mathˢ => ref mathˢᵇ;
private static readonly @string mathˢᵇ = "math"u8;
internal static ref readonly @string reimportˢ => ref reimportˢᵇ;
private static readonly @string reimportˢᵇ = "reimport"u8;

public static void TestReimport(ж<testing.T> t) {
    testenv.MustHaveSource(new srcimporter_internal_test_package.testing_TжTB(t));

    // Reimporting a partially imported (incomplete) package is not supported (see issue #19337).
    // Make sure we recognize the situation and report an error.

    var mathPkg = types.NewPackage(mathˢ, mathˢ); // incomplete package
    var importer = New(build.ᏑDefault, token.NewFileSet(), new map<@string, ж<types.Package>>{[mathPkg.Path()] = mathPkg});
    var (_, err) = importer.ImportFrom(mathˢ, ".", 0);
    if (err == nil || !strings.HasPrefix(err.Error(), reimportˢ)) {
        t.Errorf("got %v; want reimport error", err);
    }
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static ref readonly @string goInternalSrcimporterˢ => ref goInternalSrcimporterˢᵇ;
private static readonly @string goInternalSrcimporterˢᵇ = "go/internal/srcimporter/testdata/issue20855"u8;
internal static ref readonly @string funcInitMustHaveABodyˢ => ref funcInitMustHaveABodyˢᵇ;
private static readonly @string funcInitMustHaveABodyˢᵇ = "func init must have a body"u8;
internal static readonly object gotNoPackageDespiteNoˢ = (@string)"got no package despite no hard errors"u8;

public static void TestIssue20855(ж<testing.T> t) {
    testenv.MustHaveSource(new srcimporter_internal_test_package.testing_TжTB(t));

    var (pkg, err) = importer.ImportFrom(goInternalSrcimporterˢ, ".", 0);
    if (err == nil || !strings.Contains(err.Error(), funcInitMustHaveABodyˢ)) {
        t.Fatalf("got unexpected or no error: %v", err);
    }
    if (pkg == nil) {
        t.Error(gotNoPackageDespiteNoˢ);
    }
}

internal static void testImportPath(ж<testing.T> t, @string pkgPath) {
    testenv.MustHaveSource(new srcimporter_internal_test_package.testing_TжTB(t));

    @string pkgName = path.Base(pkgPath);

    var (pkg, err) = importer.Import(pkgPath);
    if (err != nil) {
        t.Fatal(err);
    }

    if (pkg.Name() != pkgName) {
        t.Errorf("got %q; want %q", pkg.Name(), pkgName);
    }

    if (pkg.Path() != pkgPath) {
        t.Errorf("got %q; want %q", pkg.Path(), pkgPath);
    }
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static ref readonly @string testdataIssue23092ˢ => ref testdataIssue23092ˢᵇ;
private static readonly @string testdataIssue23092ˢᵇ = "./testdata/issue23092"u8;

// TestIssue23092 tests relative imports.
public static void TestIssue23092(ж<testing.T> t) {
    testImportPath(t, testdataIssue23092ˢ);
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static ref readonly @string goInternalSrcimporterˢ2 => ref goInternalSrcimporterˢ2ᵇ;
private static readonly @string goInternalSrcimporterˢ2ᵇ = "go/internal/srcimporter/testdata/issue24392"u8;

// TestIssue24392 tests imports against a path containing 'testdata'.
public static void TestIssue24392(ж<testing.T> t) {
    testImportPath(t, goInternalSrcimporterˢ2);
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static ref readonly @string cmdCgoInternalTestˢ => ref cmdCgoInternalTestˢᵇ;
private static readonly @string cmdCgoInternalTestˢᵇ = "cmd/cgo/internal/test"u8;

public static void TestCgo(ж<testing.T> t) {
    testenv.MustHaveGoBuild(new srcimporter_internal_test_package.testing_TжTB(t));
    testenv.MustHaveCGO(new srcimporter_internal_test_package.testing_TжTB(t));

    ref var buildCtx = ref heap<build.Context>(build.Default, out var ᏑbuildCtx);
    var importer = New(ᏑbuildCtx, token.NewFileSet(), new map<@string, ж<types.Package>>());
    var (_, err) = importer.ImportFrom(cmdCgoInternalTestˢ, buildCtx.Dir, 0);
    if (err != nil) {
        t.Fatalf("Import failed: %v", err);
    }
}

} // end srcimporter_internal_test_package
