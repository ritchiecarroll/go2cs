// Copyright 2011 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.
namespace go.go.@internal;

using bytes = bytes_package;
using fmt = fmt_package;
using testenv = global::go.@internal.testenv_package;
using os = os_package;
using exec = global::go.os.exec_package;
using path = path_package;
using filepath = global::go.path.filepath_package;
using runtime = runtime_package;
using strings = strings_package;
using sync = sync_package;
using testing = testing_package;
using time = time_package;
using ast = global::go.go.ast_package;
using build = global::go.go.build_package;
using importer = global::go.go.importer_package;
using parser = global::go.go.parser_package;
using token = global::go.go.token_package;
using types = global::go.go.types_package;
using static global::go.go.@internal.gcimporter_package;
using MethodImplAttribute = global::System.Runtime.CompilerServices.MethodImplAttribute;
using MethodImplOptions = global::System.Runtime.CompilerServices.MethodImplOptions;
using fs = io.fs_package;
using global::go.@internal;
using global::go.go;
using global::go.os;
using global::go.path;
using io;
using io = io_package;
using ꓸꓸꓸstring = Span<@string>;

partial class gcimporter_test_package {

public static void TestMain(ж<testing.M> m) {
    build.Default.GOROOT = testenv.GOROOT(default);
    os.Exit(m.Run());
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
private static ref readonly @string testdataˢ => ref testdataˢᵇ;
private static readonly @string testdataˢᵇ = "testdata"u8;
private static ref readonly @string toolˢ => ref toolˢᵇ;
private static readonly @string toolˢᵇ = "tool"u8;
private static ref readonly @string compileˢ => ref compileˢᵇ;
private static readonly @string compileˢᵇ = "compile"u8;
private static ref readonly @string importcfgˢ => ref importcfgˢᵇ;
private static readonly @string importcfgˢᵇ = "-importcfg"u8;

// compile runs the compiler on filename, with dirname as the working directory,
// and writes the output file to outdirname.
// compile gives the resulting package a packagepath of testdata/<filebasename>.
internal static @string compile(ж<testing.T> t, @string dirname, @string filename, @string outdirname, map<@string, @string> packageFiles, params ꓸꓸꓸstring pkgImportsʗp) {
    var pkgImports = pkgImportsʗp.sslice();

    // filename must end with ".go"
    var (basename, ok) = strings.CutSuffix(filepath.Base(filename), ".go");
    if (!ok) {
        t.Fatalf("filename doesn't end in .go: %s", filename);
    }
    @string objname = basename + ".o"u8;
    @string outname = filepath.Join(outdirname, objname);

    @string importcfgfile = os.DevNull;
    if (len(packageFiles) > 0 || len(pkgImports) > 0) {
        importcfgfile = filepath.Join(outdirname, basename) + ".importcfg"u8;
        testenv.WriteImportcfg(new testing_TжTB(t), importcfgfile, packageFiles, pkgImports.ꓸꓸꓸ);
    }

    @string pkgpath = path.Join(testdataˢ, basename);
    var cmd = testenv.Command(new testing_TжTB(t), testenv.GoToolPath(new testing_TжTB(t)), toolˢ, compileˢ, "-p", pkgpath, "-D", testdataˢ, importcfgˢ, importcfgfile, "-o", outname, filename);
    cmd.Dir = dirname;
    var (@out, err) = cmd.CombinedOutput();
    if (err != nil) {
        t.Logf("%s", @out);
        t.Fatalf("go tool compile %s failed: %s", filename, err);
    }
    return outname;
}

internal static ж<types.Package> testPath(ж<testing.T> t, @string path, @string srcDir) {
    var t0 = time.Now();
    var fset = token.NewFileSet();
    var (pkg, err) = Import(fset, new map<@string, ж<types.Package>>(), path, srcDir, default);
    if (err != nil) {
        t.Errorf("testPath(%s): %s", path, err);
        return default;
    }
    t.Logf("testPath(%s): %v", path, time.Since(t0));
    return pkg;
}

internal static array<@string> pkgExts = new @string[]{".a", ".o"}.array(); // keep in sync with gcimporter.go

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
private static ref readonly @string gcimporterTestˢ => ref gcimporterTestˢᵇ;
private static readonly @string gcimporterTestˢᵇ = "gcimporter_test"u8;
private static readonly object mktmpdirˢ = (@string)"mktmpdir:"u8;

internal static @string mktmpdir(ж<testing.T> t) {
    var (tmpdir, err) = os.MkdirTemp(""u8, gcimporterTestˢ);
    if (err != nil) {
        t.Fatal(mktmpdirˢ, err);
    }
    {
        var errΔ1 = os.Mkdir(filepath.Join(tmpdir, testdataˢ), 448); if (errΔ1 != nil) {
            os.RemoveAll(tmpdir);
            t.Fatal(mktmpdirˢ, errΔ1);
        }
    }
    return tmpdir;
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
private static ref readonly @string exportsGoˢ => ref exportsGoˢᵇ;
private static readonly @string exportsGoˢᵇ = "exports.go"u8;

public static void TestImportTestdata(ж<testing.T> t) {
    GoFrame ᒐ = default;
    try {
        // This package only handles gc export data.
        if (runtime.Compiler != "gc") {
            t.Skipf("gc-built packages not available (compiler = %s)", runtime.Compiler);
        }

        testenv.MustHaveGoBuild(new testing_TжTB(t));

        var testfiles = new map<@string, slice<@string>>{
            ["exports.go"] = slice<@string>(["go/ast", "go/token"]),
            ["generics.go"] = default
        };
        testfiles[exportsGoˢ] = slice<@string>(["go/ast"]);

        foreach (var (testfile, wantImports) in testfiles) {
            @string tmpdir = mktmpdir(t);
            defer(os.RemoveAll, tmpdir, ref ᒐ);

            compile(t, "testdata"u8, testfile, filepath.Join(tmpdir, testdataˢ), default, wantImports.ꓸꓸꓸ);
            @string path = "./testdata/"u8 + strings.TrimSuffix(testfile, ".go");

            {
                var pkg = testPath(t, path, tmpdir); if (pkg != nil) {
                    // The package's Imports list must include all packages
                    // explicitly imported by testfile, plus all packages
                    // referenced indirectly via exported objects in testfile.
                    @string got = fmt.Sprint(pkg.Imports());
                    foreach (var (_, want) in wantImports) {
                        if (!strings.Contains(got, want)) {
                            t.Errorf(@"Package(""exports"").Imports() = %s, does not contain %s", got, want);
                        }
                    }
                }
            }
        }
    }
    catch (Exception ᒐex) when (GoFrame.IsPanic(ᒐex, out PanicException? ᒐp)) { GoFrame.Capture(ᒐp); }
    finally { ᒐ.Run(); }
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
private static ref readonly @string testˢ => ref testˢᵇ;
private static readonly @string testˢᵇ = "test"u8;
private static ref readonly @string versionˢ => ref versionˢᵇ;
private static readonly @string versionˢᵇ = "VERSION"u8;
private static ref readonly @string typeparamˢ => ref typeparamˢᵇ;
private static readonly @string typeparamˢᵇ = "typeparam"u8;

public static void TestImportTypeparamTests(ж<testing.T> t) {
    GoFrame ᒐ = default;
    try {
        if (testing.Short()) {
            t.Skipf("in short mode, skipping test that requires export data for all of std");
        }

        // This package only handles gc export data.
        if (runtime.Compiler != "gc") {
            t.Skipf("gc-built packages not available (compiler = %s)", runtime.Compiler);
        }

        // cmd/distpack removes the GOROOT/test directory, so skip if it isn't there.
        // cmd/distpack also requires the presence of GOROOT/VERSION, so use that to
        // avoid false-positive skips.
        @string gorootTest = filepath.Join(testenv.GOROOT(new testing_TжTB(t)), testˢ);
        {
            var (_, errΔ1) = os.Stat(gorootTest); if (os.IsNotExist(errΔ1)) {
                {
                    var (_, errΔ2) = os.Stat(filepath.Join(testenv.GOROOT(new testing_TжTB(t)), versionˢ)); if (errΔ2 == nil) {
                        t.Skipf("skipping: GOROOT/test not present");
                    }
                }
            }
        }

        testenv.MustHaveGoBuild(new testing_TжTB(t));

        @string tmpdir = mktmpdir(t);
        defer(os.RemoveAll, tmpdir, ref ᒐ);

        // Check go files in test/typeparam, except those that fail for a known
        // reason.
        @string rootDir = filepath.Join(gorootTest, typeparamˢ);
        var (list, err) = os.ReadDir(rootDir);
        if (err != nil) {
            t.Fatal(err);
        }

        foreach (var (_, entry) in list) {
            if (entry.IsDir() || !strings.HasSuffix(entry.Name(), ".go")) {
                // For now, only consider standalone go files.
                continue;
            }

            var entryʗ1 = entry;
            t.Run(entry.Name(), (ж<testing.T> tΔ1) => {
                @string filename = filepath.Join(rootDir, entryʗ1.Name());
                var (src, errΔ3) = os.ReadFile(filename);
                if (errΔ3 != nil) {
                    tΔ1.Fatal(errΔ3);
                }
                if (!bytes.HasPrefix(src, slice<byte>("// run"u8)) && !bytes.HasPrefix(src, slice<byte>("// compile"u8))) {
                    // We're bypassing the logic of run.go here, so be conservative about
                    // the files we consider in an attempt to make this test more robust to
                    // changes in test/typeparams.
                    tΔ1.Skipf("not detected as a run test");
                }

                // Compile and import, and compare the resulting package with the package
                // that was type-checked directly.
                compile(tΔ1, rootDir, entryʗ1.Name(), filepath.Join(tmpdir, testdataˢ), default, filename);
                @string pkgName = strings.TrimSuffix(entryʗ1.Name(), ".go");
                var imported = importPkg(tΔ1, "./testdata/"u8 + pkgName, tmpdir);
                var @checked = checkFile(tΔ1, filename, src);

                var seen = new map<@string, bool>();
                foreach (var (_, name) in imported.Scope().Names()) {
                    if (!token.IsExported(name)) {
                        continue; // ignore synthetic names like .inittask and .dict.*
                    }
                    seen[name] = true;

                    var importedObj = imported.Scope().Lookup(name);
                    @string got = types.ObjectString(importedObj, types.RelativeTo(imported));
                    got = sanitizeObjectString(got);

                    var checkedObj = @checked.Scope().Lookup(name);
                    if (checkedObj == nil) {
                        tΔ1.Fatalf("imported object %q was not type-checked", name);
                    }
                    @string want = types.ObjectString(checkedObj, types.RelativeTo(@checked));
                    want = sanitizeObjectString(want);

                    if (got != want) {
                        tΔ1.Errorf("imported %q as %q, want %q", name, got, want);
                    }
                }

                foreach (var (_, name) in @checked.Scope().Names()) {
                    if (!token.IsExported(name) || seen[name]) {
                        continue;
                    }
                    tΔ1.Errorf("did not import object %q", name);
                }
            });
        }
    }
    catch (Exception ᒐex) when (GoFrame.IsPanic(ᒐex, out PanicException? ᒐp)) { GoFrame.Capture(ᒐp); }
    finally { ᒐ.Run(); }
}

// sanitizeObjectString removes type parameter debugging markers from an object
// string, to normalize it for comparison.
// TODO(rfindley): this should not be necessary.
internal static @string sanitizeObjectString(@string s) {
    var runesᴷ = new stackAppend<rune>();

    slice<rune> runes = default;
    foreach (var (_, r) in s) {
        if ('₀' <= r && r < (rune)'₀' + 10) {
            continue; // trim type parameter subscripts
        }
        runes = append(runesᴷ, runes, r);
    }
    return (@string)runes;
}

internal static ж<types.Package> checkFile(ж<testing.T> t, @string filename, slice<byte> src) {
    var fset = token.NewFileSet();
    var (f, err) = parser.ParseFile(fset, filename, src, 0);
    if (err != nil) {
        t.Fatal(err);
    }
    ref var config = ref heap<types.Config>(new types.Config(
        Importer: importer.Default()
    ), out var Ꮡconfig);
    (var pkg, err) = Ꮡconfig.Check(""u8, fset, slice<ж<ast.File>>([f]), nil);
    if (err != nil) {
        t.Fatal(err);
    }
    return pkg;
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
private static ref readonly @string versionsˢ => ref versionsˢᵇ;
private static readonly @string versionsˢᵇ = "versions"u8;
private static ref readonly @string corruptedˢ => ref corruptedˢᵇ;
private static readonly @string corruptedˢᵇ = "corrupted"u8;
private static ref readonly @string notTheStartOfAnArchiveˢ => ref notTheStartOfAnArchiveˢᵇ;
private static readonly @string notTheStartOfAnArchiveˢᵇ = "not the start of an archive file"u8;
private static ref readonly @string noLongerSupportedˢ => ref noLongerSupportedˢᵇ;
private static readonly @string noLongerSupportedˢᵇ = "no longer supported"u8;
private static ref readonly @string newerVersionˢ => ref newerVersionˢᵇ;
private static readonly @string newerVersionˢᵇ = "newer version"u8;
private static ref readonly @string versionSkewˢ => ref versionSkewˢᵇ;
private static readonly @string versionSkewˢᵇ = "version skew"u8;

// Hoisted Go string constant (single allocation; Go keeps it in RODATA)
private static ref readonly @string dirᶜ => ref dirᶜᵇ;
private static readonly @string dirᶜᵇ = "./testdata/versions"u8;

public static void TestVersionHandling(ж<testing.T> t) {
    GoFrame ᒐ = default;
    try {
        testenv.MustHaveGoBuild(new testing_TжTB(t));

        // This package only handles gc export data.
        if (runtime.Compiler != "gc") {
            t.Skipf("gc-built packages not available (compiler = %s)", runtime.Compiler);
        }

        @string dir = dirᶜ;
        var (list, err) = os.ReadDir(dir);
        if (err != nil) {
            t.Fatal(err);
        }

        @string tmpdir = mktmpdir(t);
        defer(os.RemoveAll, tmpdir, ref ᒐ);
        @string corruptdir = filepath.Join(tmpdir, testdataˢ, versionsˢ);
        {
            var errΔ1 = os.Mkdir(corruptdir, 448); if (errΔ1 != nil) {
                t.Fatal(errΔ1);
            }
        }

        var fset = token.NewFileSet();

        foreach (var (_, f) in list) {
            @string name = f.Name();
            if (!strings.HasSuffix(name, ".a")) {
                continue; // not a package file
            }
            if (strings.Contains(name, corruptedˢ)) {
                continue; // don't process a leftover corrupted file
            }
            @string pkgpath = "./" + name.slice(0, len(name) - 2);

            if (testing.Verbose()) {
                t.Logf("importing %s", name);
            }

            // test that export data can be imported
            var (_, errΔ2) = Import(fset, new map<@string, ж<types.Package>>(), pkgpath, dir, default);
            if (errΔ2 != nil) {
                // ok to fail if it fails with a 'not the start of an archive file' error for select files
                if (strings.Contains(errΔ2.Error(), notTheStartOfAnArchiveˢ)) {
                    var exprᴛ1 = name;
                    switch (ᐧ) {
                    case {} when exprᴛ1 == "test_go1.8_4.a"u8 || exprᴛ1 == "test_go1.8_5.a"u8: {
                        continue;
                        break;
                    }}
                    // fall through
                }
                // ok to fail if it fails with a 'no longer supported' error for select files
                if (strings.Contains(errΔ2.Error(), noLongerSupportedˢ)) {
                    var exprᴛ2 = name;
                    switch (ᐧ) {
                    case {} when exprᴛ2 == "test_go1.7_0.a"u8 || exprᴛ2 == "test_go1.7_1.a"u8 || exprᴛ2 == "test_go1.8_4.a"u8 || exprᴛ2 == "test_go1.8_5.a"u8 || exprᴛ2 == "test_go1.11_0i.a"u8 || exprᴛ2 == "test_go1.11_6b.a"u8 || exprᴛ2 == "test_go1.11_999b.a"u8 || exprᴛ2 == "test_go1.11_999i.a"u8: {
                        continue;
                        break;
                    }}
                    // fall through
                }
                // ok to fail if it fails with a 'newer version' error for select files
                if (strings.Contains(errΔ2.Error(), newerVersionˢ)) {
                    var exprᴛ3 = name;
                    switch (ᐧ) {
                    case {} when exprᴛ3 == "test_go1.11_999i.a"u8: {
                        continue;
                        break;
                    }}
                    // fall through
                }
                t.Errorf("import %q failed: %v", pkgpath, errΔ2);
                continue;
            }

            // create file with corrupted export data
            // 1) read file
            (var data, errΔ2) = os.ReadFile(filepath.Join(dir, name));
            if (errΔ2 != nil) {
                t.Fatal(errΔ2);
            }
            // 2) find export data
            nint i = bytes.Index(data, slice<byte>("\n$$B\n"u8)) + 5;
            // Export data can contain "\n$$\n" in string constants, however,
            // searching for the next end of section marker "\n$$\n" is good enough for tests.
            nint j = bytes.Index(data.slice(i), slice<byte>("\n$$\n"u8)) + i;
            if (i < 0 || j < 0 || i > j) {
                t.Fatalf("export data section not found (i = %d, j = %d)", i, j);
            }
            // 3) corrupt the data (increment every 7th byte)
            for (nint k = j - 13; k >= i; k -= 7) {
                data[k]++;
            }
            // 4) write the file
            pkgpath += "_corrupted"u8;
            @string filename = filepath.Join(corruptdir, pkgpath) + ".a"u8;
            os.WriteFile(filename, data, 438);

            // test that importing the corrupted file results in an error
            (_, errΔ2) = Import(fset, new map<@string, ж<types.Package>>(), pkgpath, corruptdir, default);
            if (errΔ2 == nil) {
                t.Errorf("import corrupted %q succeeded", pkgpath);
            } else {
                @string msg = errΔ2.Error(); if (!strings.Contains(msg, versionSkewˢ)) {
                    t.Errorf("import %q error incorrect (%s)", pkgpath, msg);
                }
            }
        }
    }
    catch (Exception ᒐex) when (GoFrame.IsPanic(ᒐex, out PanicException? ᒐp)) { GoFrame.Capture(ᒐp); }
    finally { ᒐ.Run(); }
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
private static readonly object theImportsCanBeExpensiveˢ = (@string)"the imports can be expensive, and this test is especially slow when the build cache is empty"u8;
private static ref readonly @string listˢ => ref listˢᵇ;
private static readonly @string listˢᵇ = "list"u8;
private static ref readonly @string ifGoFilesImportPathEndˢ => ref ifGoFilesImportPathEndˢᵇ;
private static readonly @string ifGoFilesImportPathEndˢᵇ = "{{if .GoFiles}}{{.ImportPath}}{{end}}"u8;
private static ref readonly @string stdˢ => ref stdˢᵇ;
private static readonly @string stdˢᵇ = "std"u8;
private static ref readonly @string srcˢ => ref srcˢᵇ;
private static readonly @string srcˢᵇ = "src"u8;

public static void TestImportStdLib(ж<testing.T> t) {
    if (testing.Short()) {
        t.Skip(theImportsCanBeExpensiveˢ);
    }
    testenv.MustHaveGoBuild(new testing_TжTB(t));

    // This package only handles gc export data.
    if (runtime.Compiler != "gc") {
        t.Skipf("gc-built packages not available (compiler = %s)", runtime.Compiler);
    }

    // Get list of packages in stdlib. Filter out test-only packages with {{if .GoFiles}} check.
    ref var stderr = ref heap(new bytes.Buffer(), out var Ꮡstderr);
    var cmd = exec.Command("go", listˢ, "-f", ifGoFilesImportPathEndˢ, stdˢ);
    cmd.Stderr = new bytes_BufferжWriter(Ꮡstderr);
    var (@out, err) = cmd.Output();
    if (err != nil) {
        t.Fatalf("failed to run go list to determine stdlib packages: %v\nstderr:\n%v", err, Ꮡstderr.String());
    }
    var pkgs = strings.Fields((@string)@out);

    nint nimports = default;
    foreach (var (_, pkg) in pkgs) {
        t.Run(pkg, (ж<testing.T> tΔ1) => {
            if (testPath(tΔ1, pkg, filepath.Join(testenv.GOROOT(new testing_TжTB(tΔ1)), srcˢ, path.Dir(pkg))) != nil) {
                nimports++;
            }
        });
    }
    const nint minPkgs = 225; // 'GOOS=plan9 go1.18 list std | wc -l' reports 228; most other platforms have more.
    if (len(pkgs) < minPkgs) {
        t.Fatalf("too few packages (%d) were imported", nimports);
    }

    t.Logf("tested %d imports", nimports);
}

partial struct importedObjectTestsᴛ1 /*dyn*/ {
    internal @string name;
    internal @string want;
}
internal static slice<importedObjectTestsᴛ1> importedObjectTests = slice<importedObjectTestsᴛ1>([
    // non-interfaces
    new("crypto.Hash", "type Hash uint"),
    new("go/ast.ObjKind", "type ObjKind int"),
    new("go/types.Qualifier", "type Qualifier func(*Package) string"),
    new("go/types.Comparable", "func Comparable(T Type) bool"),
    new("math.Pi", "const Pi untyped float"),
    new("math.Sin", "func Sin(x float64) float64"),
    new("go/ast.NotNilFilter", "func NotNilFilter(_ string, v reflect.Value) bool"),
    new("internal/exportdata.FindPkg", "func FindPkg(path string, srcDir string) (filename string, id string, err error)"),

    // interfaces
    new("context.Context", "type Context interface{Deadline() (deadline time.Time, ok bool); Done() <-chan struct{}; Err() error; Value(key any) any}"),
    new("crypto.Decrypter", "type Decrypter interface{Decrypt(rand io.Reader, msg []byte, opts DecrypterOpts) (plaintext []byte, err error); Public() PublicKey}"),
    new("encoding.BinaryMarshaler", "type BinaryMarshaler interface{MarshalBinary() (data []byte, err error)}"),
    new("io.Reader", "type Reader interface{Read(p []byte) (n int, err error)}"),
    new("io.ReadWriter", "type ReadWriter interface{Reader; Writer}"),
    new("go/ast.Node", "type Node interface{End() go/token.Pos; Pos() go/token.Pos}"),
    new("go/types.Type", "type Type interface{String() string; Underlying() Type}")
]);

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
private static readonly object inconsistentTestDataˢ = (@string)"inconsistent test data"u8;

public static void TestImportedTypes(ж<testing.T> t) {
    testenv.MustHaveGoBuild(new testing_TжTB(t));

    // This package only handles gc export data.
    if (runtime.Compiler != "gc") {
        t.Skipf("gc-built packages not available (compiler = %s)", runtime.Compiler);
    }

    var fset = token.NewFileSet();
    foreach (var (_, test) in importedObjectTests) {
        var s = strings.Split(test.name, ".");
        if (len(s) != 2) {
            t.Fatal(inconsistentTestDataˢ);
        }
        @string importPath = s[0];
        @string objName = s[1];

        var (pkg, err) = Import(fset, new map<@string, ж<types.Package>>(), importPath, ".", default);
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

        {
            var (named, _) = obj.Type()._<ж<types.Named>>(ᐧ); if (named != nil) {
                verifyInterfaceMethodRecvs(t, named, 0);
            }
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

    // check embedded interfaces (if they are named, too)
    for (nint i = 0; i < iface.NumEmbeddeds(); i++) {
        // embedding of interfaces cannot have cycles; recursion will terminate
        {
            var (etype, _) = iface.EmbeddedType(i)._<ж<types.Named>>(ᐧ); if (etype != nil) {
                verifyInterfaceMethodRecvs(t, etype, level + 1);
            }
        }
    }
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
private static ref readonly @string stringsˢ => ref stringsˢᵇ;
private static readonly @string stringsˢᵇ = "strings"u8;

public static void TestIssue5815(ж<testing.T> t) {
    testenv.MustHaveGoBuild(new testing_TжTB(t));

    // This package only handles gc export data.
    if (runtime.Compiler != "gc") {
        t.Skipf("gc-built packages not available (compiler = %s)", runtime.Compiler);
    }

    var pkg = importPkg(t, stringsˢ, ".");

    var scope = pkg.Scope();
    foreach (var (_, name) in scope.Names()) {
        var obj = scope.Lookup(name);
        if (obj.Pkg() == nil) {
            t.Errorf("no pkg for %s", obj);
        }
        {
            var (tname, _) = obj._<ж<types.TypeName>>(ᐧ); if (tname != nil) {
                var named = tname.Type()._<ж<types.Named>>();
                for (nint i = 0; i < named.NumMethods(); i++) {
                    var m = named.Method(i);
                    if (m.Pkg() == nil) {
                        t.Errorf("no pkg for %s", m.OrTypedNil());
                    }
                }
            }
        }
    }
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
private static ref readonly @string netHttpˢ => ref netHttpˢᵇ;
private static readonly @string netHttpˢᵇ = "net/http"u8;
private static ref readonly @string syncˢ => ref syncˢᵇ;
private static readonly @string syncˢᵇ = "sync"u8;
private static ref readonly @string mutexˢ => ref mutexˢᵇ;
private static readonly @string mutexˢᵇ = "Mutex"u8;
private static ref readonly @string lockˢ => ref lockˢᵇ;
private static readonly @string lockˢᵇ = "Lock"u8;

// Smoke test to ensure that imported methods get the correct package.
public static void TestCorrectMethodPackage(ж<testing.T> t) {
    testenv.MustHaveGoBuild(new testing_TжTB(t));

    // This package only handles gc export data.
    if (runtime.Compiler != "gc") {
        t.Skipf("gc-built packages not available (compiler = %s)", runtime.Compiler);
    }

    var imports = new map<@string, ж<types.Package>>();
    var fset = token.NewFileSet();
    var (_, err) = Import(fset, imports, netHttpˢ, ".", default);
    if (err != nil) {
        t.Fatal(err);
    }

    var mutex = imports[syncˢ].Scope().Lookup(mutexˢ)._<ж<types.TypeName>>().Type();
    var mset = types.NewMethodSet(new types.PointerжΔType(types.NewPointer(mutex))); // methods of *sync.Mutex
    var sel = mset.Lookup(nil, lockˢ);
    var @lock = sel.Obj()._<ж<types.Func>>();
    {
        @string got = @lock.Pkg().Path();
        @string want = syncˢ; if (got != want) {
            t.Errorf("got package path %q; want %q", got, want);
        }
    }
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
private static ref readonly @string bGoˢ => ref bGoˢᵇ;
private static readonly @string bGoˢᵇ = "b.go"u8;
private static ref readonly @string aGoˢ => ref aGoˢᵇ;
private static readonly @string aGoˢᵇ = "a.go"u8;
private static ref readonly @string encodingJsonˢ => ref encodingJsonˢᵇ;
private static readonly @string encodingJsonˢᵇ = "encoding/json"u8;
private static ref readonly @string testdataBˢ => ref testdataBˢᵇ;
private static readonly @string testdataBˢᵇ = "./testdata/b"u8;

public static void TestIssue13566(ж<testing.T> t) {
    GoFrame ᒐ = default;
    try {
        testenv.MustHaveGoBuild(new testing_TжTB(t));

        // This package only handles gc export data.
        if (runtime.Compiler != "gc") {
            t.Skipf("gc-built packages not available (compiler = %s)", runtime.Compiler);
        }

        @string tmpdir = mktmpdir(t);
        defer(os.RemoveAll, tmpdir, ref ᒐ);
        @string testoutdir = filepath.Join(tmpdir, testdataˢ);

        // b.go needs to be compiled from the output directory so that the compiler can
        // find the compiled package a. We pass the full path to compile() so that we
        // don't have to copy the file to that directory.
        var (bpath, err) = filepath.Abs(filepath.Join(testdataˢ, bGoˢ));
        if (err != nil) {
            t.Fatal(err);
        }

        compile(t, testdataˢ, aGoˢ, testoutdir, default, encodingJsonˢ);
        compile(t, testoutdir, bpath, testoutdir, new map<@string, @string>{["testdata/a"] = filepath.Join(testoutdir, "a.o")}, encodingJsonˢ);

        // import must succeed (test for issue at hand)
        var pkg = importPkg(t, testdataBˢ, tmpdir);

        // make sure all indirectly imported packages have names
        foreach (var (_, imp) in pkg.Imports()) {
            if (imp.Name() == ""u8) {
                t.Errorf("no name for %s package", imp.Path());
            }
        }
    }
    catch (Exception ᒐex) when (GoFrame.IsPanic(ᒐex, out PanicException? ᒐp)) { GoFrame.Capture(ᒐp); }
    finally { ᒐ.Run(); }
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
private static ref readonly @string gGoˢ => ref gGoˢᵇ;
private static readonly @string gGoˢᵇ = "g.go"u8;
private static ref readonly @string testdataGˢ => ref testdataGˢᵇ;
private static readonly @string testdataGˢᵇ = "./testdata/g"u8;

public static void TestTypeNamingOrder(ж<testing.T> t) {
    GoFrame ᒐ = default;
    try {
        testenv.MustHaveGoBuild(new testing_TжTB(t));

        // This package only handles gc export data.
        if (runtime.Compiler != "gc") {
            t.Skipf("gc-built packages not available (compiler = %s)", runtime.Compiler);
        }

        @string tmpdir = mktmpdir(t);
        defer(os.RemoveAll, tmpdir, ref ᒐ);
        @string testoutdir = filepath.Join(tmpdir, testdataˢ);

        compile(t, testdataˢ, gGoˢ, testoutdir, default);

        // import must succeed (test for issue at hand)
        _ = importPkg(t, testdataGˢ, tmpdir);
    }
    catch (Exception ᒐex) when (GoFrame.IsPanic(ᒐex, out PanicException? ᒐp)) { GoFrame.Capture(ᒐp); }
    finally { ᒐ.Run(); }
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
private static ref readonly @string goInternalGcimporterˢ => ref goInternalGcimporterˢᵇ;
private static readonly @string goInternalGcimporterˢᵇ = "go/internal/gcimporter"u8;
private static readonly object goTypesNotFoundˢ = (@string)"go/types not found"u8;
private static ref readonly @string objectˢ => ref objectˢᵇ;
private static readonly @string objectˢᵇ = "Object"u8;
private static ref readonly @string pkgˢ => ref pkgˢᵇ;
private static readonly @string pkgˢᵇ = "Pkg"u8;

public static void TestIssue13898(ж<testing.T> t) {
    testenv.MustHaveGoBuild(new testing_TжTB(t));

    // This package only handles gc export data.
    if (runtime.Compiler != "gc") {
        t.Skipf("gc-built packages not available (compiler = %s)", runtime.Compiler);
    }

    // import go/internal/gcimporter which imports go/types partially
    var fset = token.NewFileSet();
    var imports = new map<@string, ж<types.Package>>();
    var (_, err) = Import(fset, imports, goInternalGcimporterˢ, ".", default);
    if (err != nil) {
        t.Fatal(err);
    }

    // look for go/types package
    ж<types.Package> goTypesPkg = default;
    foreach (var (path, pkg) in imports) {
        if (path == "go/types"u8) {
            goTypesPkg = pkg;
            break;
        }
    }
    if (goTypesPkg == nil) {
        t.Fatal(goTypesNotFoundˢ);
    }

    // look for go/types.Object type
    var obj = lookupObj(t, goTypesPkg.Scope(), objectˢ);
    var (typ, ok) = obj.Type()._<ж<types.Named>>(ᐧ);
    if (!ok) {
        t.Fatalf("go/types.Object type is %v; wanted named type", typ.OrTypedNil());
    }

    // lookup go/types.Object.Pkg method
    (var sel, ok) = types.LookupSelection(new types.NamedжΔType(typ), false, nil, pkgˢ);
    if (!ok) {
        t.Fatalf("go/types.Object.Pkg not found");
    }

    // the method must belong to go/types
    if (sel.Obj().Pkg().Path() != "go/types"u8) {
        t.Fatalf("found %v; want go/types", sel.Obj().Pkg().OrTypedNil());
    }
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
private static ref readonly @string pGoˢ => ref pGoˢᵇ;
private static readonly @string pGoˢᵇ = "p.go"u8;
private static ref readonly @string testdataPˢ => ref testdataPˢᵇ;
private static readonly @string testdataPˢᵇ = "./././testdata/p"u8;

public static void TestIssue15517(ж<testing.T> t) {
    GoFrame ᒐ = default;
    try {
        testenv.MustHaveGoBuild(new testing_TжTB(t));

        // This package only handles gc export data.
        if (runtime.Compiler != "gc") {
            t.Skipf("gc-built packages not available (compiler = %s)", runtime.Compiler);
        }

        @string tmpdir = mktmpdir(t);
        defer(os.RemoveAll, tmpdir, ref ᒐ);

        compile(t, testdataˢ, pGoˢ, filepath.Join(tmpdir, testdataˢ), default);

        // Multiple imports of p must succeed without redeclaration errors.
        // We use an import path that's not cleaned up so that the eventual
        // file path for the package is different from the package path; this
        // will expose the error if it is present.
        //
        // (Issue: Both the textual and the binary importer used the file path
        // of the package to be imported as key into the shared packages map.
        // However, the binary importer then used the package path to identify
        // the imported package to mark it as complete; effectively marking the
        // wrong package as complete. By using an "unclean" package path, the
        // file and package path are different, exposing the problem if present.
        // The same issue occurs with vendoring.)
        var imports = new map<@string, ж<types.Package>>();
        var fset = token.NewFileSet();
        for (nint i = 0; i < 3; i++) {
            {
                var (_, err) = Import(fset, imports, testdataPˢ, tmpdir, default); if (err != nil) {
                    t.Fatal(err);
                }
            }
        }
    }
    catch (Exception ᒐex) when (GoFrame.IsPanic(ᒐex, out PanicException? ᒐp)) { GoFrame.Capture(ᒐp); }
    finally { ᒐ.Run(); }
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
private static ref readonly @string issue15920ˢ => ref issue15920ˢᵇ;
private static readonly @string issue15920ˢᵇ = "issue15920"u8;

public static void TestIssue15920(ж<testing.T> t) {
    testenv.MustHaveGoBuild(new testing_TжTB(t));

    // This package only handles gc export data.
    if (runtime.Compiler != "gc") {
        t.Skipf("gc-built packages not available (compiler = %s)", runtime.Compiler);
    }

    compileAndImportPkg(t, issue15920ˢ);
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
private static ref readonly @string issue20046ˢ => ref issue20046ˢᵇ;
private static readonly @string issue20046ˢᵇ = "issue20046"u8;

public static void TestIssue20046(ж<testing.T> t) {
    testenv.MustHaveGoBuild(new testing_TжTB(t));

    // This package only handles gc export data.
    if (runtime.Compiler != "gc") {
        t.Skipf("gc-built packages not available (compiler = %s)", runtime.Compiler);
    }

    // "./issue20046".V.M must exist
    var pkg = compileAndImportPkg(t, issue20046ˢ);
    var obj = lookupObj(t, pkg.Scope(), "V");
    {
        var (_, ok) = types.LookupSelection(obj.Type(), false, nil, "M"); if (!ok) {
            t.Fatalf("V.M not found");
        }
    }
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
private static ref readonly @string issue25301ˢ => ref issue25301ˢᵇ;
private static readonly @string issue25301ˢᵇ = "issue25301"u8;

public static void TestIssue25301(ж<testing.T> t) {
    testenv.MustHaveGoBuild(new testing_TжTB(t));

    // This package only handles gc export data.
    if (runtime.Compiler != "gc") {
        t.Skipf("gc-built packages not available (compiler = %s)", runtime.Compiler);
    }

    compileAndImportPkg(t, issue25301ˢ);
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
private static ref readonly @string issue25596ˢ => ref issue25596ˢᵇ;
private static readonly @string issue25596ˢᵇ = "issue25596"u8;

public static void TestIssue25596(ж<testing.T> t) {
    testenv.MustHaveGoBuild(new testing_TжTB(t));

    // This package only handles gc export data.
    if (runtime.Compiler != "gc") {
        t.Skipf("gc-built packages not available (compiler = %s)", runtime.Compiler);
    }

    compileAndImportPkg(t, issue25596ˢ);
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
private static ref readonly @string issue57015ˢ => ref issue57015ˢᵇ;
private static readonly @string issue57015ˢᵇ = "issue57015"u8;

public static void TestIssue57015(ж<testing.T> t) {
    testenv.MustHaveGoBuild(new testing_TжTB(t));

    // This package only handles gc export data.
    if (runtime.Compiler != "gc") {
        t.Skipf("gc-built packages not available (compiler = %s)", runtime.Compiler);
    }

    compileAndImportPkg(t, issue57015ˢ);
}

internal static ж<types.Package> importPkg(ж<testing.T> t, @string path, @string srcDir) {
    var fset = token.NewFileSet();
    var (pkg, err) = Import(fset, new map<@string, ж<types.Package>>(), path, srcDir, default);
    if (err != nil) {
        t.Helper();
        t.Fatal(err);
    }
    return pkg;
}

internal static ж<types.Package> compileAndImportPkg(ж<testing.T> t, @string name) {
    GoFrame ᒐ = default;
    try {
        t.Helper();
        @string tmpdir = mktmpdir(t);
        defer(os.RemoveAll, tmpdir, ref ᒐ);
        compile(t, testdataˢ, name + ".go"u8, filepath.Join(tmpdir, testdataˢ), default);
        return importPkg(t, "./testdata/"u8 + name, tmpdir);
    }
    catch (Exception ᒐex) when (GoFrame.IsPanic(ᒐex, out PanicException? ᒐp)) { GoFrame.Capture(ᒐp); return default; }
    finally { ᒐ.Run(); }
}

internal static types.Object lookupObj(ж<testing.T> t, ж<typesꓸScope> Ꮡscope, @string name) {
    ref var scope = ref Ꮡscope.DerefOrNull();

    {
        var obj = scope.Lookup(name); if (obj != nil) {
            return obj;
        }
    }
    t.Helper();
    t.Fatalf("%s not found", name);
    return default;
}

// importMap implements the types.Importer interface.
partial struct importMap /*map[@string, ж<types.Package>]*/;

internal static (ж<types.Package>, error) ΔImport(this importMap m, @string path) {
    return (m[path], default);
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
private static ref readonly @string issue69912Goˢ => ref issue69912Goˢᵇ;
private static readonly @string issue69912Goˢᵇ = "issue69912.go"u8;
private static ref readonly @string testdataIssue69912ˢ => ref testdataIssue69912ˢᵇ;
private static readonly @string testdataIssue69912ˢᵇ = "./testdata/issue69912"u8;

// Hoisted Go string constant (single allocation; Go keeps it in RODATA)
private static ref readonly @string pSrcᶜ => ref pSrcᶜᵇ;
private static readonly @string pSrcᶜᵇ = """
package p

import . "issue69912"

type S struct {
	f T
}

"""u8.Raw();

public static partial void TestIssue69912(ж<testing.T> t) {
    testenv.MustHaveGoBuild(new testing_TжTB(t));

    // This package only handles gc export data.
    if (runtime.Compiler != "gc") {
        t.Skipf("gc-built packages not available (compiler = %s)", runtime.Compiler);
    }

    @string tmpdir = t.TempDir();
    @string testoutdir = filepath.Join(tmpdir, testdataˢ);
    {
        var errΔ1 = os.Mkdir(testoutdir, 448); if (errΔ1 != nil) {
            t.Fatalf("making output dir: %v", errΔ1);
        }
    }

    compile(t, testdataˢ, issue69912Goˢ, testoutdir, default);

    var fset = token.NewFileSet();

    var (issue69912, err) = Import(fset, new map<@string, ж<types.Package>>(), testdataIssue69912ˢ, tmpdir, default);
    if (err != nil) {
        t.Fatal(err);
    }

    var fsetʗ1 = fset;
    (ж<types.Package>, error) check(@string pkgname, @string src, importMap imports) {
        var (f, errΔ2) = parser.ParseFile(fsetʗ1, aGoˢ, src, 0);
        if (errΔ2 != nil) {
            return (default, errΔ2);
        }
        var config = Ꮡ(new types.Config(
            Importer: imports
        ));
        return config.Check(pkgname, fsetʗ1, slice<ж<ast.File>>([f]), nil);
    }

    // Use the resulting package concurrently, via dot-imports, to exercise the
    // race of issue #69912.
    @string pSrc = pSrcᶜ;
    var importer = new importMap(new map<@string, ж<types.Package>>{
        ["issue69912"] = issue69912
    });
    ref var wg = ref heap(new sync.WaitGroup(), out var Ꮡwg);
    foreach (var _ᴛ1 in range(10)) {
        Ꮡwg.Add(1);
        var checkʗ1 = check;
        var importerʗ1 = importer;
        goǃ(() => {
            GoFrame ᒐ = default;
            try {
                defer(Ꮡwg.Done, ref ᒐ);
                {
                    var (_, errΔ3) = checkʗ1("p", pSrc, importerʗ1); if (errΔ3 != nil) {
                        t.Errorf("Check failed: %v", errΔ3);
                    }
                }
            }
            catch (Exception ᒐex) when (GoFrame.IsPanic(ᒐex, out PanicException? ᒐp)) { GoFrame.Capture(ᒐp); }
            finally { ᒐ.Run(); }
        });
    }
    Ꮡwg.Wait();
}

} // end gcimporter_test_package
