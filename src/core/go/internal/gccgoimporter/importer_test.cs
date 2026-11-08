// Copyright 2013 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.
namespace go.go.@internal;

using types = global::go.go.types_package;
using testenv = global::go.@internal.testenv_package;
using os = os_package;
using exec = global::go.os.exec_package;
using filepath = path.filepath_package;
using regexp = regexp_package;
using strconv = strconv_package;
using testing = testing_package;
using constant = global::go.go.constant_package;
using fs = global::go.io.fs_package;
using global::go.@internal;
using global::go.go;
using global::go.os;
using io = io_package;
using path;
using static global::go.go.@internal.gccgoimporter_package;

partial class gccgoimporter_internal_test_package {

internal partial struct importerTest {
    internal @string pkgpath, name, want, wantval;
    internal slice<@string> wantinits;
    internal nint gccgoVersion; // minimum gccgo version (0 => any)
}

internal static void runImporterTest(ж<testing.T> t, Func<map<@string, ж<types.Package>>, @string, @string, Func<@string, (io.ReadCloser, error)>, (ж<types.Package>, error)> imp, map<ж<types.Package>, global::go.go.@internal.gccgoimporter_package.InitData> initmap, ж<importerTest> Ꮡtest) {
    ref var test = ref Ꮡtest.DerefOrNull();

    var (pkg, err) = imp(new map<@string, ж<types.Package>>(), test.pkgpath, ".", default);
    if (err != nil) {
        t.Error(err);
        return;
    }

    if (test.name != ""u8) {
        var obj = pkg.Scope().Lookup(test.name);
        if (obj == nil) {
            t.Errorf("%s: object not found", test.name);
            return;
        }

        @string got = types.ObjectString(obj, types.RelativeTo(pkg));
        if (got != test.want) {
            t.Errorf("%s: got %q; want %q", test.name, got, test.want);
        }

        if (test.wantval != ""u8) {
            @string gotval = obj._<ж<types.Const>>().Val().String();
            if (gotval != test.wantval) {
                t.Errorf("%s: got val %q; want val %q", test.name, gotval, test.wantval);
            }
        }
    }

    if (len(test.wantinits) > 0) {
        var initdata = initmap[pkg];
        var found = false;
        // Check that the package's own init function has the package's priority
        foreach (var (_, pkginit) in range(initdata.Inits)) {
            if (pkginit.InitFunc == test.wantinits[0]) {
                found = true;
                break;
            }
        }

        if (!found) {
            t.Errorf("%s: could not find expected function %q", test.pkgpath, test.wantinits[0]);
        }

        // FIXME: the original version of this test was written against
        // the v1 export data scheme for capturing init functions, so it
        // verified the priority values. We moved away from the priority
        // scheme some time ago; it is not clear how much work it would be
        // to validate the new init export data.
    }
}

// When adding tests to this list, be sure to set the 'gccgoVersion'
// field if the testcases uses a "recent" Go addition (ex: aliases).
internal static array<importerTest> importerTests = new importerTest[]{
    new(pkgpath: "pointer", name: "Int8Ptr", want: "type Int8Ptr *int8"),
    new(pkgpath: "complexnums", name: "NN", want: "const NN untyped complex", wantval: "(-1 + -1i)"),
    new(pkgpath: "complexnums", name: "NP", want: "const NP untyped complex", wantval: "(-1 + 1i)"),
    new(pkgpath: "complexnums", name: "PN", want: "const PN untyped complex", wantval: "(1 + -1i)"),
    new(pkgpath: "complexnums", name: "PP", want: "const PP untyped complex", wantval: "(1 + 1i)"),
    new(pkgpath: "conversions", name: "Bits", want: "const Bits Units", wantval: @"""bits"""),
    new(pkgpath: "time", name: "Duration", want: "type Duration int64"),
    new(pkgpath: "time", name: "Nanosecond", want: "const Nanosecond Duration", wantval: "1"),
    new(pkgpath: "unicode", name: "IsUpper", want: "func IsUpper(r rune) bool"),
    new(pkgpath: "unicode", name: "MaxRune", want: "const MaxRune untyped rune", wantval: "1114111"),
    new(pkgpath: "imports", wantinits: ["imports..import", "fmt..import"]),
    new(pkgpath: "importsar", name: "Hello", want: "var Hello string"),
    new(pkgpath: "aliases", name: "A14", gccgoVersion: 7, want: "type A14 = func(int, T0) chan T2"),
    new(pkgpath: "aliases", name: "C0", gccgoVersion: 7, want: "type C0 struct{f1 C1; f2 C1}"),
    new(pkgpath: "escapeinfo", name: "NewT", want: "func NewT(data []byte) *T"),
    new(pkgpath: "issue27856", name: "M", gccgoVersion: 7, want: "type M struct{E F}"),
    new(pkgpath: "v1reflect", name: "Type", want: "type Type interface{Align() int; AssignableTo(u Type) bool; Bits() int; ChanDir() ChanDir; Elem() Type; Field(i int) StructField; FieldAlign() int; FieldByIndex(index []int) StructField; FieldByName(name string) (StructField, bool); FieldByNameFunc(match func(string) bool) (StructField, bool); Implements(u Type) bool; In(i int) Type; IsVariadic() bool; Key() Type; Kind() Kind; Len() int; Method(int) Method; MethodByName(string) (Method, bool); Name() string; NumField() int; NumIn() int; NumMethod() int; NumOut() int; Out(i int) Type; PkgPath() string; Size() uintptr; String() string; common() *commonType; rawString() string; runtimeType() *runtimeType; uncommon() *uncommonType}"),
    new(pkgpath: "nointerface", name: "I", want: "type I int"),
    new(pkgpath: "issue29198", name: "FooServer", gccgoVersion: 7, want: "type FooServer struct{FooServer *FooServer; user string; ctx context.Context}"),
    new(pkgpath: "issue30628", name: "Apple", want: "type Apple struct{hey sync.RWMutex; x int; RQ [517]struct{Count uintptr; NumBytes uintptr; Last uintptr}}"),
    new(pkgpath: "issue31540", name: "S", gccgoVersion: 7, want: "type S struct{b int; map[Y]Z}"), // should want "type S struct{b int; A2}" (issue  #44410)
    new(pkgpath: "issue34182", name: "T1", want: "type T1 struct{f *T2}"),
    new(pkgpath: "notinheap", name: "S", want: "type S struct{}")
}.array();

public static void TestGoxImporter(ж<testing.T> t) {
    testenv.MustHaveExec(new gccgoimporter_internal_test_package.testing_TжTB(t));
    var initmap = new map<ж<types.Package>, global::go.go.@internal.gccgoimporter_package.InitData>();
    var imp = GetImporter(["testdata"], initmap);

    foreach (var (_, vᴛ1) in range(importerTests.ΔRangeSnapshot())) {
        ref var test = ref heap(new importerTest(), out var Ꮡtest);
        test = vᴛ1;

        runImporterTest(t, imp, initmap, Ꮡtest);
    }
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static ref readonly @string gccgoˢ => ref gccgoˢᵇ;
private static readonly @string gccgoˢᵇ = "GCCGO"u8;
internal static ref readonly @string gccgoˢ2 => ref gccgoˢ2ᵇ;
private static readonly @string gccgoˢ2ᵇ = "gccgo"u8;

// gccgoPath returns a path to gccgo if it is present (either in
// path or specified via GCCGO environment variable), or an
// empty string if no gccgo is available.
internal static @string gccgoPath() {
    @string gccgoname = os.Getenv(gccgoˢ);
    if (gccgoname == ""u8) {
        gccgoname = gccgoˢ2;
    }
    var (gpath, gerr) = exec.LookPath(gccgoname); if (gerr == nil) {
        return gpath;
    }
    return ""u8;
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static ref readonly @string versionˢ => ref versionˢᵇ;
private static readonly @string versionˢᵇ = "--version"u8;
internal static ref readonly @string testdataˢ => ref testdataˢᵇ;
private static readonly @string testdataˢᵇ = "testdata"u8;

public static void TestObjImporter(ж<testing.T> t) {
    // This test relies on gccgo being around.
    @string gpath = gccgoPath();
    if (gpath == ""u8) {
        t.Skip(thisTestNeedsGccgoˢ);
    }

    var (verout, err) = testenv.Command(new gccgoimporter_internal_test_package.testing_TжTB(t), gpath, versionˢ).CombinedOutput();
    if (err != nil) {
        t.Logf("%s", verout);
        t.Fatal(err);
    }
    var vers = regexp.MustCompile(@"(\d+)\.(\d+)").FindSubmatch(verout);
    if (len(vers) == 0) {
        t.Fatalf("could not find version number in %s", verout);
    }
    (var major, err) = strconv.Atoi((@string)vers[1]);
    if (err != nil) {
        t.Fatal(err);
    }
    (var minor, err) = strconv.Atoi((@string)vers[2]);
    if (err != nil) {
        t.Fatal(err);
    }
    t.Logf("gccgo version %d.%d", major, minor);

    @string tmpdir = t.TempDir();
    var initmap = new map<ж<types.Package>, global::go.go.@internal.gccgoimporter_package.InitData>();
    var imp = GetImporter([tmpdir], initmap);

    @string artmpdir = t.TempDir();
    var arinitmap = new map<ж<types.Package>, global::go.go.@internal.gccgoimporter_package.InitData>();
    var arimp = GetImporter([artmpdir], arinitmap);

    foreach (var (_, vᴛ1) in range(importerTests.ΔRangeSnapshot())) {
        ref var test = ref heap(new importerTest(), out var Ꮡtest);
        test = vᴛ1;

        if (major < test.gccgoVersion) {
            // Support for type aliases was added in GCC 7.
            t.Logf("skipping %q: not supported before gccgo version %d", test.pkgpath, test.gccgoVersion);
            continue;
        }

        @string gofile = filepath.Join(testdataˢ, test.pkgpath + ".go");
        var (_, errΔ1) = os.Stat(gofile); if (os.IsNotExist(errΔ1)) {
            continue;
        }
        @string ofile = filepath.Join(tmpdir, test.pkgpath + ".o");
        @string afile = filepath.Join(artmpdir, "lib" + test.pkgpath + ".a");

        var cmd = testenv.Command(new gccgoimporter_internal_test_package.testing_TжTB(t), gpath, "-fgo-pkgpath="u8 + test.pkgpath, "-c", "-o", ofile, gofile);
        var (@out, errΔ2) = cmd.CombinedOutput();
        if (errΔ2 != nil) {
            t.Logf("%s", @out);
            t.Fatalf("gccgo %s failed: %s", gofile, errΔ2);
        }

        runImporterTest(t, imp, initmap, Ꮡtest);

        @string ar = os.Getenv("AR");
        if (ar == ""u8) {
            ar = "ar";
        }
        cmd = testenv.Command(new gccgoimporter_internal_test_package.testing_TжTB(t), ar, "cr", afile, ofile);
        (@out, errΔ2) = cmd.CombinedOutput();
        if (errΔ2 != nil) {
            t.Logf("%s", @out);
            t.Fatalf("%s cr %s %s failed: %s", ar, afile, ofile, errΔ2);
        }

        runImporterTest(t, arimp, arinitmap, Ꮡtest);

        errΔ2 = os.Remove(ofile); if (errΔ2 != nil) {
            t.Fatal(errΔ2);
        }
        errΔ2 = os.Remove(afile); if (errΔ2 != nil) {
            t.Fatal(errΔ2);
        }
    }
}

} // end gccgoimporter_internal_test_package
