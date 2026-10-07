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

public static void TestMain(ж<testing.M> Ꮡm) {
    build.Default.GOROOT = testenv.GOROOT(default!);
    os.Exit(Ꮡm.Run());
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
private static readonly @string testdataˢ = "testdata"u8;
private static readonly @string toolˢ = "tool"u8;
private static readonly @string compileˢ = "compile"u8;
private static readonly @string importcfgˢ = "-importcfg"u8;

// compile runs the compiler on filename, with dirname as the working directory,
// and writes the output file to outdirname.
// compile gives the resulting package a packagepath of testdata/<filebasename>.
internal static @string compile(ж<testing.T> Ꮡt, @string dirname, @string filename, @string outdirname, map<@string, @string> packageFiles, params ꓸꓸꓸstring pkgImportsʗp) {
    var pkgImports = pkgImportsʗp.sslice();

    ref var t = ref Ꮡt.DerefOrNull();
    // filename must end with ".go"
    var (basename, ok) = strings.CutSuffix(filepath.Base(filename), ".go"u8);
    if (!ok) {
        Ꮡt.Fatalf("filename doesn't end in .go: %s"u8, filename);
    }
    @string objname = basename + ".o"u8;
    @string outname = filepath.Join(outdirname, objname);
    @string importcfgfile = os.DevNull;
    if (len(packageFiles) > 0 || len(pkgImports) > 0) {
        importcfgfile = filepath.Join(outdirname, basename) + ".importcfg"u8;
        testenv.WriteImportcfg(new testing_TжTB(Ꮡt), importcfgfile, packageFiles, pkgImports.ꓸꓸꓸ);
    }
    @string pkgpath = path.Join(testdataˢ, basename);
    var cmd = testenv.Command(new testing_TжTB(Ꮡt), testenv.GoToolPath(new testing_TжTB(Ꮡt)), toolˢ, compileˢ, "-p", pkgpath, "-D", testdataˢ, importcfgˢ, importcfgfile, "-o", outname, filename);
    cmd.Value.Dir = dirname;
    var (@out, err) = cmd.CombinedOutput();
    if (err != default!) {
        Ꮡt.Logf("%s"u8, @out);
        Ꮡt.Fatalf("go tool compile %s failed: %s"u8, filename, err);
    }
    return outname;
}

internal static ж<types.Package> testPath(ж<testing.T> Ꮡt, @string path, @string srcDir) {
    var t0 = time.Now();
    var fset = token.NewFileSet();
    var (pkg, err) = Import(fset, new map<@string, ж<types.Package>>(), path, srcDir, default!);
    if (err != default!) {
        Ꮡt.Errorf("testPath(%s): %s"u8, path, err);
        return default!;
    }
    Ꮡt.Logf("testPath(%s): %v"u8, path, time.Since(t0));
    return pkg;
}

internal static array<@string> pkgExts = new @string[]{".a"u8, ".o"u8}.array(); // keep in sync with gcimporter.go

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
private static readonly @string gcimporterTestˢ = "gcimporter_test"u8;
private static readonly object mktmpdirˢ = (@string)"mktmpdir:"u8;

internal static @string mktmpdir(ж<testing.T> Ꮡt) {
    var (tmpdir, err) = os.MkdirTemp(""u8, gcimporterTestˢ);
    if (err != default!) {
        Ꮡt.Fatal(mktmpdirˢ, err);
    }
    {
        var errΔ1 = os.Mkdir(filepath.Join(tmpdir, testdataˢ), 448); if (errΔ1 != default!) {
            os.RemoveAll(tmpdir);
            Ꮡt.Fatal(mktmpdirˢ, errΔ1);
        }
    }
    return tmpdir;
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
private static readonly @string exportsGoˢ = "exports.go"u8;

public static void TestImportTestdata(ж<testing.T> Ꮡt) {
    GoFrame ᒐ = default;
    try {
        // This package only handles gc export data.
        if (runtime.Compiler != "gc") {
            Ꮡt.Skipf("gc-built packages not available (compiler = %s)"u8, runtime.Compiler);
        }
        testenv.MustHaveGoBuild(new testing_TжTB(Ꮡt));
        var testfiles = new map<@string, slice<@string>>{
            ["exports.go"u8] = new @string[]{"go/ast"u8, "go/token"u8}.slice(),
            ["generics.go"u8] = default!
        };
        testfiles[exportsGoˢ] = new @string[]{"go/ast"u8}.slice();
        foreach (var (testfile, wantImports) in testfiles) {
            @string tmpdir = mktmpdir(Ꮡt);
            defer(os.RemoveAll, tmpdir, ref ᒐ);
            compile(Ꮡt, "testdata"u8, testfile, filepath.Join(tmpdir, testdataˢ), default!, wantImports.ꓸꓸꓸ);
            @string path = "./testdata/"u8 + strings.TrimSuffix(testfile, ".go"u8);
            {
                var pkg = testPath(Ꮡt, path, tmpdir); if (pkg != nil) {
                    // The package's Imports list must include all packages
                    // explicitly imported by testfile, plus all packages
                    // referenced indirectly via exported objects in testfile.
                    @string got = fmt.Sprint(pkg.Imports());
                    foreach (var (_, want) in wantImports) {
                        if (!strings.Contains(got, want)) {
                            Ꮡt.Errorf(@"Package(""exports"").Imports() = %s, does not contain %s"u8, got, want);
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
private static readonly @string testˢ = "test"u8;
private static readonly @string versionˢ = "VERSION"u8;
private static readonly @string typeparamˢ = "typeparam"u8;

public static void TestImportTypeparamTests(ж<testing.T> Ꮡt) {
    GoFrame ᒐ = default;
    try {
        ref var t = ref Ꮡt.DerefOrNull();

        if (testing.Short()) {
            Ꮡt.Skipf("in short mode, skipping test that requires export data for all of std"u8);
        }
        // This package only handles gc export data.
        if (runtime.Compiler != "gc") {
            Ꮡt.Skipf("gc-built packages not available (compiler = %s)"u8, runtime.Compiler);
        }
        // cmd/distpack removes the GOROOT/test directory, so skip if it isn't there.
        // cmd/distpack also requires the presence of GOROOT/VERSION, so use that to
        // avoid false-positive skips.
        @string gorootTest = filepath.Join(testenv.GOROOT(new testing_TжTB(Ꮡt)), testˢ);
        {
            var (_, errΔ1) = os.Stat(gorootTest); if (os.IsNotExist(errΔ1)) {
                {
                    var (_, errΔ2) = os.Stat(filepath.Join(testenv.GOROOT(new testing_TжTB(Ꮡt)), versionˢ)); if (errΔ2 == default!) {
                        Ꮡt.Skipf("skipping: GOROOT/test not present"u8);
                    }
                }
            }
        }
        testenv.MustHaveGoBuild(new testing_TжTB(Ꮡt));
        @string tmpdir = mktmpdir(Ꮡt);
        defer(os.RemoveAll, tmpdir, ref ᒐ);
        // Check go files in test/typeparam, except those that fail for a known
        // reason.
        @string rootDir = filepath.Join(gorootTest, typeparamˢ);
        var (list, err) = os.ReadDir(rootDir);
        if (err != default!) {
            Ꮡt.Fatal(err);
        }
        foreach (var (_, entry) in list) {
            if (entry.IsDir() || !strings.HasSuffix(entry.Name(), ".go"u8)) {
                // For now, only consider standalone go files.
                continue;
            }
            var entryʗ1 = entry;
            Ꮡt.Run(entry.Name(), (ж<testing.T> tΔ1) => {
                @string filename = filepath.Join(rootDir, entryʗ1.Name());
                var (src, errΔ3) = os.ReadFile(filename);
                if (errΔ3 != default!) {
                    tΔ1.Fatal(errΔ3);
                }
                if (!bytes.HasPrefix(src, slice<byte>("// run"u8)) && !bytes.HasPrefix(src, slice<byte>("// compile"u8))) {
                    // We're bypassing the logic of run.go here, so be conservative about
                    // the files we consider in an attempt to make this test more robust to
                    // changes in test/typeparams.
                    tΔ1.Skipf("not detected as a run test"u8);
                }
                // Compile and import, and compare the resulting package with the package
                // that was type-checked directly.
                compile(tΔ1, rootDir, entryʗ1.Name(), filepath.Join(tmpdir, testdataˢ), default!, filename);
                @string pkgName = strings.TrimSuffix(entryʗ1.Name(), ".go"u8);
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
                    if (checkedObj == default!) {
                        tΔ1.Fatalf("imported object %q was not type-checked"u8, name);
                    }
                    @string want = types.ObjectString(checkedObj, types.RelativeTo(@checked));
                    want = sanitizeObjectString(want);
                    if (got != want) {
                        tΔ1.Errorf("imported %q as %q, want %q"u8, name, got, want);
                    }
                }
                foreach (var (_, name) in @checked.Scope().Names()) {
                    if (!token.IsExported(name) || seen[name]) {
                        continue;
                    }
                    tΔ1.Errorf("did not import object %q"u8, name);
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
    slice<rune> runes = default!;
    foreach (var (_, r) in s) {
        if ((rune)'₀' <= r && r < (rune)'₀' + 10) {
            continue; // trim type parameter subscripts
        }
        runes = append(runes, r);
    }
    return ((@string)runes);
}

internal static ж<types.Package> checkFile(ж<testing.T> Ꮡt, @string filename, slice<byte> src) {
    var fset = token.NewFileSet();
    var (f, err) = parser.ParseFile(fset, filename, src, 0);
    if (err != default!) {
        Ꮡt.Fatal(err);
    }
    ref var config = ref heap<types.Config>(out var Ꮡconfig);
    config = new types.Config(
        Importer: importer.Default()
    );
    (var pkg, err) = Ꮡconfig.Check(""u8, fset, new ж<ast.File>[]{f}.slice(), nil);
    if (err != default!) {
        Ꮡt.Fatal(err);
    }
    return pkg;
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
private static readonly @string versionsˢ = "versions"u8;
private static readonly @string corruptedˢ = "corrupted"u8;
private static readonly @string notTheStartOfAnArchiveˢ = "not the start of an archive file"u8;
private static readonly @string noLongerSupportedˢ = "no longer supported"u8;
private static readonly @string newerVersionˢ = "newer version"u8;
private static readonly @string versionSkewˢ = "version skew"u8;

// Hoisted Go string constant (single allocation; Go keeps it in RODATA)
private static readonly @string dirᶜ = "./testdata/versions"u8;

public static void TestVersionHandling(ж<testing.T> Ꮡt) {
    GoFrame ᒐ = default;
    try {
        ref var t = ref Ꮡt.DerefOrNull();

        testenv.MustHaveGoBuild(new testing_TжTB(Ꮡt));
        // This package only handles gc export data.
        if (runtime.Compiler != "gc") {
            Ꮡt.Skipf("gc-built packages not available (compiler = %s)"u8, runtime.Compiler);
        }
        @string dir = dirᶜ;
        var (list, err) = os.ReadDir(dir);
        if (err != default!) {
            Ꮡt.Fatal(err);
        }
        @string tmpdir = mktmpdir(Ꮡt);
        defer(os.RemoveAll, tmpdir, ref ᒐ);
        @string corruptdir = filepath.Join(tmpdir, testdataˢ, versionsˢ);
        {
            var errΔ1 = os.Mkdir(corruptdir, 448); if (errΔ1 != default!) {
                Ꮡt.Fatal(errΔ1);
            }
        }
        var fset = token.NewFileSet();
        foreach (var (_, f) in list) {
            @string name = f.Name();
            if (!strings.HasSuffix(name, ".a"u8)) {
                continue; // not a package file
            }
            if (strings.Contains(name, corruptedˢ)) {
                continue; // don't process a leftover corrupted file
            }
            @string pkgpath = "./" + name.slice(0, len(name) - 2);
            if (testing.Verbose()) {
                Ꮡt.Logf("importing %s"u8, name);
            }
            // test that export data can be imported
            var (_, errΔ2) = Import(fset, new map<@string, ж<types.Package>>(), pkgpath, dir, default!);
            if (errΔ2 != default!) {
                // ok to fail if it fails with a 'not the start of an archive file' error for select files
                if (strings.Contains(errΔ2.Error(), notTheStartOfAnArchiveˢ)) {
                    var exprᴛ1 = name;
                    if (exprᴛ1 == "test_go1.8_4.a"u8 || exprᴛ1 == "test_go1.8_5.a"u8) {
                        continue;
                    }

                }
                // fall through
                // ok to fail if it fails with a 'no longer supported' error for select files
                if (strings.Contains(errΔ2.Error(), noLongerSupportedˢ)) {
                    var exprᴛ2 = name;
                    if (exprᴛ2 == "test_go1.7_0.a"u8 || exprᴛ2 == "test_go1.7_1.a"u8 || exprᴛ2 == "test_go1.8_4.a"u8 || exprᴛ2 == "test_go1.8_5.a"u8 || exprᴛ2 == "test_go1.11_0i.a"u8 || exprᴛ2 == "test_go1.11_6b.a"u8 || exprᴛ2 == "test_go1.11_999b.a"u8 || exprᴛ2 == "test_go1.11_999i.a"u8) {
                        continue;
                    }

                }
                // fall through
                // ok to fail if it fails with a 'newer version' error for select files
                if (strings.Contains(errΔ2.Error(), newerVersionˢ)) {
                    var exprᴛ3 = name;
                    if (exprᴛ3 == "test_go1.11_999i.a"u8) {
                        continue;
                    }

                }
                // fall through
                Ꮡt.Errorf("import %q failed: %v"u8, pkgpath, errΔ2);
                continue;
            }
            // create file with corrupted export data
            // 1) read file
            (var data, errΔ2) = os.ReadFile(filepath.Join(dir, name));
            if (errΔ2 != default!) {
                Ꮡt.Fatal(errΔ2);
            }
            // 2) find export data
            nint i = bytes.Index(data, slice<byte>("\n$$B\n"u8)) + 5;
            // Export data can contain "\n$$\n" in string constants, however,
            // searching for the next end of section marker "\n$$\n" is good enough for tests.
            nint j = bytes.Index(data.slice(i), slice<byte>("\n$$\n"u8)) + i;
            if (i < 0 || j < 0 || i > j) {
                Ꮡt.Fatalf("export data section not found (i = %d, j = %d)"u8, i, j);
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
            (_, errΔ2) = Import(fset, new map<@string, ж<types.Package>>(), pkgpath, corruptdir, default!);
            if (errΔ2 == default!){
                Ꮡt.Errorf("import corrupted %q succeeded"u8, pkgpath);
            } else 
            {
                @string msg = errΔ2.Error(); if (!strings.Contains(msg, versionSkewˢ)) {
                    Ꮡt.Errorf("import %q error incorrect (%s)"u8, pkgpath, msg);
                }
            }
        }
    }
    catch (Exception ᒐex) when (GoFrame.IsPanic(ᒐex, out PanicException? ᒐp)) { GoFrame.Capture(ᒐp); }
    finally { ᒐ.Run(); }
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
private static readonly object theImportsCanBeExpensiveˢ = (@string)"the imports can be expensive, and this test is especially slow when the build cache is empty"u8;
private static readonly @string listˢ = "list"u8;
private static readonly @string ifGoFilesImportPathEndˢ = "{{if .GoFiles}}{{.ImportPath}}{{end}}"u8;
private static readonly @string stdˢ = "std"u8;
private static readonly @string srcˢ = "src"u8;

public static void TestImportStdLib(ж<testing.T> Ꮡt) {
    if (testing.Short()) {
        Ꮡt.Skip(theImportsCanBeExpensiveˢ);
    }
    testenv.MustHaveGoBuild(new testing_TжTB(Ꮡt));
    // This package only handles gc export data.
    if (runtime.Compiler != "gc") {
        Ꮡt.Skipf("gc-built packages not available (compiler = %s)"u8, runtime.Compiler);
    }
    // Get list of packages in stdlib. Filter out test-only packages with {{if .GoFiles}} check.
    ref var stderr = ref heap(new bytes.Buffer(), out var Ꮡstderr);
    var cmd = exec.Command("go"u8, listˢ, "-f", ifGoFilesImportPathEndˢ, stdˢ);
    cmd.Value.Stderr = new bytes_BufferжWriter(Ꮡstderr);
    var (@out, err) = cmd.Output();
    if (err != default!) {
        Ꮡt.Fatalf("failed to run go list to determine stdlib packages: %v\nstderr:\n%v"u8, err, Ꮡstderr.String());
    }
    var pkgs = strings.Fields(((@string)@out));
    nint nimports = default!;
    foreach (var (_, pkg) in pkgs) {
        Ꮡt.Run(pkg, (ж<testing.T> tΔ1) => {
            if (testPath(tΔ1, pkg, filepath.Join(testenv.GOROOT(new testing_TжTB(tΔ1)), srcˢ, path.Dir(pkg))) != nil) {
                nimports++;
            }
        });
    }
    const nint minPkgs = 225; // 'GOOS=plan9 go1.18 list std | wc -l' reports 228; most other platforms have more.
    if (len(pkgs) < minPkgs) {
        Ꮡt.Fatalf("too few packages (%d) were imported"u8, nimports);
    }
    Ꮡt.Logf("tested %d imports"u8, nimports);
}

// non-interfaces
// interfaces

[GoType("dyn")] partial struct importedObjectTestsᴛ1 {
    internal @string name;
    internal @string want;
}
internal static slice<importedObjectTestsᴛ1> importedObjectTests = new importedObjectTestsᴛ1[]{
    new("crypto.Hash"u8, "type Hash uint"u8),
    new("go/ast.ObjKind"u8, "type ObjKind int"u8),
    new("go/types.Qualifier"u8, "type Qualifier func(*Package) string"u8),
    new("go/types.Comparable"u8, "func Comparable(T Type) bool"u8),
    new("math.Pi"u8, "const Pi untyped float"u8),
    new("math.Sin"u8, "func Sin(x float64) float64"u8),
    new("go/ast.NotNilFilter"u8, "func NotNilFilter(_ string, v reflect.Value) bool"u8),
    new("internal/exportdata.FindPkg"u8, "func FindPkg(path string, srcDir string) (filename string, id string, err error)"u8),
    new("context.Context"u8, "type Context interface{Deadline() (deadline time.Time, ok bool); Done() <-chan struct{}; Err() error; Value(key any) any}"u8),
    new("crypto.Decrypter"u8, "type Decrypter interface{Decrypt(rand io.Reader, msg []byte, opts DecrypterOpts) (plaintext []byte, err error); Public() PublicKey}"u8),
    new("encoding.BinaryMarshaler"u8, "type BinaryMarshaler interface{MarshalBinary() (data []byte, err error)}"u8),
    new("io.Reader"u8, "type Reader interface{Read(p []byte) (n int, err error)}"u8),
    new("io.ReadWriter"u8, "type ReadWriter interface{Reader; Writer}"u8),
    new("go/ast.Node"u8, "type Node interface{End() go/token.Pos; Pos() go/token.Pos}"u8),
    new("go/types.Type"u8, "type Type interface{String() string; Underlying() Type}"u8)
}.slice();

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
private static readonly object inconsistentTestDataˢ = (@string)"inconsistent test data"u8;

public static void TestImportedTypes(ж<testing.T> Ꮡt) {
    testenv.MustHaveGoBuild(new testing_TжTB(Ꮡt));
    // This package only handles gc export data.
    if (runtime.Compiler != "gc") {
        Ꮡt.Skipf("gc-built packages not available (compiler = %s)"u8, runtime.Compiler);
    }
    var fset = token.NewFileSet();
    foreach (var (_, test) in importedObjectTests) {
        var s = strings.Split(test.name, "."u8);
        if (len(s) != 2) {
            Ꮡt.Fatal(inconsistentTestDataˢ);
        }
        @string importPath = s[0];
        @string objName = s[1];
        var (pkg, err) = Import(fset, new map<@string, ж<types.Package>>(), importPath, "."u8, default!);
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
    // check embedded interfaces (if they are named, too)
    for (nint i = 0; i < iface.NumEmbeddeds(); i++) {
        // embedding of interfaces cannot have cycles; recursion will terminate
        {
            var (etype, _) = iface.EmbeddedType(i)._<ж<types.Named>>(ᐧ); if (etype != nil) {
                verifyInterfaceMethodRecvs(Ꮡt, etype, level + 1);
            }
        }
    }
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
private static readonly @string stringsˢ = "strings"u8;

public static void TestIssue5815(ж<testing.T> Ꮡt) {
    testenv.MustHaveGoBuild(new testing_TжTB(Ꮡt));
    // This package only handles gc export data.
    if (runtime.Compiler != "gc") {
        Ꮡt.Skipf("gc-built packages not available (compiler = %s)"u8, runtime.Compiler);
    }
    var pkg = importPkg(Ꮡt, stringsˢ, "."u8);
    var scope = pkg.Scope();
    foreach (var (_, name) in scope.Names()) {
        var obj = scope.Lookup(name);
        if (obj.Pkg() == nil) {
            Ꮡt.Errorf("no pkg for %s"u8, obj);
        }
        {
            var (tname, _) = obj._<ж<types.TypeName>>(ᐧ); if (tname != nil) {
                var named = tname.Type()._<ж<types.Named>>();
                for (nint i = 0; i < named.NumMethods(); i++) {
                    var m = named.Method(i);
                    if (m.Pkg() == nil) {
                        Ꮡt.Errorf("no pkg for %s"u8, m.OrTypedNil());
                    }
                }
            }
        }
    }
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
private static readonly @string netHttpˢ = "net/http"u8;
private static readonly @string syncˢ = "sync"u8;
private static readonly @string mutexˢ = "Mutex"u8;
private static readonly @string lockˢ = "Lock"u8;

// Smoke test to ensure that imported methods get the correct package.
public static void TestCorrectMethodPackage(ж<testing.T> Ꮡt) {
    testenv.MustHaveGoBuild(new testing_TжTB(Ꮡt));
    // This package only handles gc export data.
    if (runtime.Compiler != "gc") {
        Ꮡt.Skipf("gc-built packages not available (compiler = %s)"u8, runtime.Compiler);
    }
    var imports = new map<@string, ж<types.Package>>();
    var fset = token.NewFileSet();
    var (_, err) = Import(fset, imports, netHttpˢ, "."u8, default!);
    if (err != default!) {
        Ꮡt.Fatal(err);
    }
    var mutex = imports[syncˢ].Scope().Lookup(mutexˢ)._<ж<types.TypeName>>().Type();
    var mset = types.NewMethodSet(new types.PointerжΔType(types.NewPointer(mutex))); // methods of *sync.Mutex
    var sel = mset.Lookup(nil, lockˢ);
    var @lock = sel.Obj()._<ж<types.Func>>();
    {
        @string got = @lock.Pkg().Path();
        @string want = syncˢ; if (got != want) {
            Ꮡt.Errorf("got package path %q; want %q"u8, got, want);
        }
    }
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
private static readonly @string bGoˢ = "b.go"u8;
private static readonly @string aGoˢ = "a.go"u8;
private static readonly @string encodingJsonˢ = "encoding/json"u8;
private static readonly @string testdataBˢ = "./testdata/b"u8;

public static void TestIssue13566(ж<testing.T> Ꮡt) {
    GoFrame ᒐ = default;
    try {
        ref var t = ref Ꮡt.DerefOrNull();

        testenv.MustHaveGoBuild(new testing_TжTB(Ꮡt));
        // This package only handles gc export data.
        if (runtime.Compiler != "gc") {
            Ꮡt.Skipf("gc-built packages not available (compiler = %s)"u8, runtime.Compiler);
        }
        @string tmpdir = mktmpdir(Ꮡt);
        defer(os.RemoveAll, tmpdir, ref ᒐ);
        @string testoutdir = filepath.Join(tmpdir, testdataˢ);
        // b.go needs to be compiled from the output directory so that the compiler can
        // find the compiled package a. We pass the full path to compile() so that we
        // don't have to copy the file to that directory.
        var (bpath, err) = filepath.Abs(filepath.Join(testdataˢ, bGoˢ));
        if (err != default!) {
            Ꮡt.Fatal(err);
        }
        compile(Ꮡt, testdataˢ, aGoˢ, testoutdir, default!, encodingJsonˢ);
        compile(Ꮡt, testoutdir, bpath, testoutdir, new map<@string, @string>{["testdata/a"u8] = filepath.Join(testoutdir, "a.o")}, encodingJsonˢ);
        // import must succeed (test for issue at hand)
        var pkg = importPkg(Ꮡt, testdataBˢ, tmpdir);
        // make sure all indirectly imported packages have names
        foreach (var (_, imp) in pkg.Imports()) {
            if (imp.Name() == ""u8) {
                Ꮡt.Errorf("no name for %s package"u8, imp.Path());
            }
        }
    }
    catch (Exception ᒐex) when (GoFrame.IsPanic(ᒐex, out PanicException? ᒐp)) { GoFrame.Capture(ᒐp); }
    finally { ᒐ.Run(); }
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
private static readonly @string gGoˢ = "g.go"u8;
private static readonly @string testdataGˢ = "./testdata/g"u8;

public static void TestTypeNamingOrder(ж<testing.T> Ꮡt) {
    GoFrame ᒐ = default;
    try {
        testenv.MustHaveGoBuild(new testing_TжTB(Ꮡt));
        // This package only handles gc export data.
        if (runtime.Compiler != "gc") {
            Ꮡt.Skipf("gc-built packages not available (compiler = %s)"u8, runtime.Compiler);
        }
        @string tmpdir = mktmpdir(Ꮡt);
        defer(os.RemoveAll, tmpdir, ref ᒐ);
        @string testoutdir = filepath.Join(tmpdir, testdataˢ);
        compile(Ꮡt, testdataˢ, gGoˢ, testoutdir, default!);
        // import must succeed (test for issue at hand)
        _ = importPkg(Ꮡt, testdataGˢ, tmpdir);
    }
    catch (Exception ᒐex) when (GoFrame.IsPanic(ᒐex, out PanicException? ᒐp)) { GoFrame.Capture(ᒐp); }
    finally { ᒐ.Run(); }
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
private static readonly @string goInternalGcimporterˢ = "go/internal/gcimporter"u8;
private static readonly object goTypesNotFoundˢ = (@string)"go/types not found"u8;
private static readonly @string objectˢ = "Object"u8;
private static readonly @string pkgˢ = "Pkg"u8;

public static void TestIssue13898(ж<testing.T> Ꮡt) {
    testenv.MustHaveGoBuild(new testing_TжTB(Ꮡt));
    // This package only handles gc export data.
    if (runtime.Compiler != "gc") {
        Ꮡt.Skipf("gc-built packages not available (compiler = %s)"u8, runtime.Compiler);
    }
    // import go/internal/gcimporter which imports go/types partially
    var fset = token.NewFileSet();
    var imports = new map<@string, ж<types.Package>>();
    var (_, err) = Import(fset, imports, goInternalGcimporterˢ, "."u8, default!);
    if (err != default!) {
        Ꮡt.Fatal(err);
    }
    // look for go/types package
    ж<types.Package> goTypesPkg = default!;
    foreach (var (path, pkg) in imports) {
        if (path == "go/types"u8) {
            goTypesPkg = pkg;
            break;
        }
    }
    if (goTypesPkg == nil) {
        Ꮡt.Fatal(goTypesNotFoundˢ);
    }
    // look for go/types.Object type
    var obj = lookupObj(Ꮡt, goTypesPkg.Scope(), objectˢ);
    var (typ, ok) = obj.Type()._<ж<types.Named>>(ᐧ);
    if (!ok) {
        Ꮡt.Fatalf("go/types.Object type is %v; wanted named type"u8, typ.OrTypedNil());
    }
    // lookup go/types.Object.Pkg method
    var (m, index, indirect) = types.LookupFieldOrMethod(new types.NamedжΔType(typ), false, nil, pkgˢ);
    if (m == default!) {
        Ꮡt.Fatalf("go/types.Object.Pkg not found (index = %v, indirect = %v)"u8, index, indirect);
    }
    // the method must belong to go/types
    if (m.Pkg().Path() != "go/types"u8) {
        Ꮡt.Fatalf("found %v; want go/types"u8, m.Pkg().OrTypedNil());
    }
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
private static readonly @string pGoˢ = "p.go"u8;
private static readonly @string testdataPˢ = "./././testdata/p"u8;

public static void TestIssue15517(ж<testing.T> Ꮡt) {
    GoFrame ᒐ = default;
    try {
        testenv.MustHaveGoBuild(new testing_TжTB(Ꮡt));
        // This package only handles gc export data.
        if (runtime.Compiler != "gc") {
            Ꮡt.Skipf("gc-built packages not available (compiler = %s)"u8, runtime.Compiler);
        }
        @string tmpdir = mktmpdir(Ꮡt);
        defer(os.RemoveAll, tmpdir, ref ᒐ);
        compile(Ꮡt, testdataˢ, pGoˢ, filepath.Join(tmpdir, testdataˢ), default!);
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
                var (_, err) = Import(fset, imports, testdataPˢ, tmpdir, default!); if (err != default!) {
                    Ꮡt.Fatal(err);
                }
            }
        }
    }
    catch (Exception ᒐex) when (GoFrame.IsPanic(ᒐex, out PanicException? ᒐp)) { GoFrame.Capture(ᒐp); }
    finally { ᒐ.Run(); }
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
private static readonly @string issue15920ˢ = "issue15920"u8;

public static void TestIssue15920(ж<testing.T> Ꮡt) {
    testenv.MustHaveGoBuild(new testing_TжTB(Ꮡt));
    // This package only handles gc export data.
    if (runtime.Compiler != "gc") {
        Ꮡt.Skipf("gc-built packages not available (compiler = %s)"u8, runtime.Compiler);
    }
    compileAndImportPkg(Ꮡt, issue15920ˢ);
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
private static readonly @string issue20046ˢ = "issue20046"u8;

public static void TestIssue20046(ж<testing.T> Ꮡt) {
    testenv.MustHaveGoBuild(new testing_TжTB(Ꮡt));
    // This package only handles gc export data.
    if (runtime.Compiler != "gc") {
        Ꮡt.Skipf("gc-built packages not available (compiler = %s)"u8, runtime.Compiler);
    }
    // "./issue20046".V.M must exist
    var pkg = compileAndImportPkg(Ꮡt, issue20046ˢ);
    var obj = lookupObj(Ꮡt, pkg.Scope(), "V"u8);
    {
        var (m, index, indirect) = types.LookupFieldOrMethod(obj.Type(), false, nil, "M"u8); if (m == default!) {
            Ꮡt.Fatalf("V.M not found (index = %v, indirect = %v)"u8, index, indirect);
        }
    }
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
private static readonly @string issue25301ˢ = "issue25301"u8;

public static void TestIssue25301(ж<testing.T> Ꮡt) {
    testenv.MustHaveGoBuild(new testing_TжTB(Ꮡt));
    // This package only handles gc export data.
    if (runtime.Compiler != "gc") {
        Ꮡt.Skipf("gc-built packages not available (compiler = %s)"u8, runtime.Compiler);
    }
    compileAndImportPkg(Ꮡt, issue25301ˢ);
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
private static readonly @string issue25596ˢ = "issue25596"u8;

public static void TestIssue25596(ж<testing.T> Ꮡt) {
    testenv.MustHaveGoBuild(new testing_TжTB(Ꮡt));
    // This package only handles gc export data.
    if (runtime.Compiler != "gc") {
        Ꮡt.Skipf("gc-built packages not available (compiler = %s)"u8, runtime.Compiler);
    }
    compileAndImportPkg(Ꮡt, issue25596ˢ);
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
private static readonly @string issue57015ˢ = "issue57015"u8;

public static void TestIssue57015(ж<testing.T> Ꮡt) {
    testenv.MustHaveGoBuild(new testing_TжTB(Ꮡt));
    // This package only handles gc export data.
    if (runtime.Compiler != "gc") {
        Ꮡt.Skipf("gc-built packages not available (compiler = %s)"u8, runtime.Compiler);
    }
    compileAndImportPkg(Ꮡt, issue57015ˢ);
}

internal static ж<types.Package> importPkg(ж<testing.T> Ꮡt, @string path, @string srcDir) {
    var fset = token.NewFileSet();
    var (pkg, err) = Import(fset, new map<@string, ж<types.Package>>(), path, srcDir, default!);
    if (err != default!) {
        Ꮡt.Helper();
        Ꮡt.Fatal(err);
    }
    return pkg;
}

internal static ж<types.Package> compileAndImportPkg(ж<testing.T> Ꮡt, @string name) {
    GoFrame ᒐ = default;
    try {
        Ꮡt.Helper();
        @string tmpdir = mktmpdir(Ꮡt);
        defer(os.RemoveAll, tmpdir, ref ᒐ);
        compile(Ꮡt, testdataˢ, name + ".go"u8, filepath.Join(tmpdir, testdataˢ), default!);
        return importPkg(Ꮡt, "./testdata/"u8 + name, tmpdir);
    }
    catch (Exception ᒐex) when (GoFrame.IsPanic(ᒐex, out PanicException? ᒐp)) { GoFrame.Capture(ᒐp); return default!; }
    finally { ᒐ.Run(); }
}

internal static types.Object lookupObj(ж<testing.T> Ꮡt, ж<typesꓸScope> Ꮡscope, @string name) {
    ref var scope = ref Ꮡscope.DerefOrNull();

    {
        var obj = scope.Lookup(name); if (obj != default!) {
            return obj;
        }
    }
    Ꮡt.Helper();
    Ꮡt.Fatalf("%s not found"u8, name);
    return default!;
}

[GoType("map[@string, ж<types.Package>]")] partial struct importMap;

internal static (ж<types.Package>, error) ΔImport(this importMap m, @string path) {
    return (m[path], default!);
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
private static readonly @string issue69912Goˢ = "issue69912.go"u8;
private static readonly @string testdataIssue69912ˢ = "./testdata/issue69912"u8;

// Hoisted Go string constant (single allocation; Go keeps it in RODATA)
private static readonly @string pSrcᶜ = """
package p

import . "issue69912"

type S struct {
	f T
}

"""u8;

public static partial void TestIssue69912(ж<testing.T> Ꮡt) {
    testenv.MustHaveGoBuild(new testing_TжTB(Ꮡt));
    // This package only handles gc export data.
    if (runtime.Compiler != "gc") {
        Ꮡt.Skipf("gc-built packages not available (compiler = %s)"u8, runtime.Compiler);
    }
    @string tmpdir = Ꮡt.TempDir();
    @string testoutdir = filepath.Join(tmpdir, testdataˢ);
    {
        var errΔ1 = os.Mkdir(testoutdir, 448); if (errΔ1 != default!) {
            Ꮡt.Fatalf("making output dir: %v"u8, errΔ1);
        }
    }
    compile(Ꮡt, testdataˢ, issue69912Goˢ, testoutdir, default!);
    var fset = token.NewFileSet();
    var (issue69912, err) = Import(fset, new map<@string, ж<types.Package>>(), testdataIssue69912ˢ, tmpdir, default!);
    if (err != default!) {
        Ꮡt.Fatal(err);
    }
    var fsetʗ1 = fset;
    (ж<types.Package>, error) check(@string pkgname, @string src, importMap imports) {
        var (f, errΔ2) = parser.ParseFile(fsetʗ1, aGoˢ, src, 0);
        if (errΔ2 != default!) {
            return (default!, errΔ2);
        }
        var config = Ꮡ(new types.Config(
            Importer: imports
        ));
        return config.Check(pkgname, fsetʗ1, new ж<ast.File>[]{f}.slice(), nil);
    }
    // Use the resulting package concurrently, via dot-imports, to exercise the
    // race of issue #69912.
    @string pSrc = pSrcᶜ;
    var importer = new importMap(new map<@string, ж<types.Package>>{
        ["issue69912"u8] = issue69912
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
                    var (_, errΔ3) = checkʗ1("p"u8, pSrc, importerʗ1); if (errΔ3 != default!) {
                        Ꮡt.Errorf("Check failed: %v"u8, errΔ3);
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
