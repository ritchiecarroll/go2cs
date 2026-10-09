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

internal static void runImporterTest(ж<testing.T> Ꮡt, Func<map<@string, ж<types.Package>>, @string, @string, Func<@string, (io.ReadCloser, error)>, (ж<types.Package>, error)> imp, map<ж<types.Package>, global::go.go.@internal.gccgoimporter_package.InitData> initmap, ж<importerTest> Ꮡtest) {
    ref var test = ref Ꮡtest.DerefOrNull();

    var (pkg, err) = imp(new map<@string, ж<types.Package>>(), test.pkgpath, "."u8, default!);
    if (err != default!) {
        Ꮡt.Error(err);
        return;
    }
    if (test.name != ""u8) {
        var obj = pkg.Scope().Lookup(test.name);
        if (obj == default!) {
            Ꮡt.Errorf("%s: object not found"u8, test.name);
            return;
        }
        @string got = types.ObjectString(obj, types.RelativeTo(pkg));
        if (got != test.want) {
            Ꮡt.Errorf("%s: got %q; want %q"u8, test.name, got, test.want);
        }
        if (test.wantval != ""u8) {
            @string gotval = obj._<ж<types.Const>>().Val().String();
            if (gotval != test.wantval) {
                Ꮡt.Errorf("%s: got val %q; want val %q"u8, test.name, gotval, test.wantval);
            }
        }
    }
    if (len(test.wantinits) > 0) {
        var initdata = initmap[pkg];
        var found = false;
        // Check that the package's own init function has the package's priority
        foreach (var (_, pkginit) in initdata.Inits) {
            if (pkginit.InitFunc == test.wantinits[0]) {
                found = true;
                break;
            }
        }
        if (!found) {
            Ꮡt.Errorf("%s: could not find expected function %q"u8, test.pkgpath, test.wantinits[0]);
        }
    }
}

// FIXME: the original version of this test was written against
// the v1 export data scheme for capturing init functions, so it
// verified the priority values. We moved away from the priority
// scheme some time ago; it is not clear how much work it would be
// to validate the new init export data.
// should want "type S struct{b int; A2}" (issue  #44410)
// When adding tests to this list, be sure to set the 'gccgoVersion'
// field if the testcases uses a "recent" Go addition (ex: aliases).
internal static array<importerTest> importerTests = new importerTest[]{
    new(pkgpath: "pointer"u8, name: "Int8Ptr"u8, want: "type Int8Ptr *int8"u8),
    new(pkgpath: "complexnums"u8, name: "NN"u8, want: "const NN untyped complex"u8, wantval: "(-1 + -1i)"u8),
    new(pkgpath: "complexnums"u8, name: "NP"u8, want: "const NP untyped complex"u8, wantval: "(-1 + 1i)"u8),
    new(pkgpath: "complexnums"u8, name: "PN"u8, want: "const PN untyped complex"u8, wantval: "(1 + -1i)"u8),
    new(pkgpath: "complexnums"u8, name: "PP"u8, want: "const PP untyped complex"u8, wantval: "(1 + 1i)"u8),
    new(pkgpath: "conversions"u8, name: "Bits"u8, want: "const Bits Units"u8, wantval: @"""bits"""u8),
    new(pkgpath: "time"u8, name: "Duration"u8, want: "type Duration int64"u8),
    new(pkgpath: "time"u8, name: "Nanosecond"u8, want: "const Nanosecond Duration"u8, wantval: "1"u8),
    new(pkgpath: "unicode"u8, name: "IsUpper"u8, want: "func IsUpper(r rune) bool"u8),
    new(pkgpath: "unicode"u8, name: "MaxRune"u8, want: "const MaxRune untyped rune"u8, wantval: "1114111"u8),
    new(pkgpath: "imports"u8, wantinits: new @string[]{"imports..import"u8, "fmt..import"u8}.slice()),
    new(pkgpath: "importsar"u8, name: "Hello"u8, want: "var Hello string"u8),
    new(pkgpath: "aliases"u8, name: "A14"u8, gccgoVersion: 7, want: "type A14 = func(int, T0) chan T2"u8),
    new(pkgpath: "aliases"u8, name: "C0"u8, gccgoVersion: 7, want: "type C0 struct{f1 C1; f2 C1}"u8),
    new(pkgpath: "escapeinfo"u8, name: "NewT"u8, want: "func NewT(data []byte) *T"u8),
    new(pkgpath: "issue27856"u8, name: "M"u8, gccgoVersion: 7, want: "type M struct{E F}"u8),
    new(pkgpath: "v1reflect"u8, name: "Type"u8, want: "type Type interface{Align() int; AssignableTo(u Type) bool; Bits() int; ChanDir() ChanDir; Elem() Type; Field(i int) StructField; FieldAlign() int; FieldByIndex(index []int) StructField; FieldByName(name string) (StructField, bool); FieldByNameFunc(match func(string) bool) (StructField, bool); Implements(u Type) bool; In(i int) Type; IsVariadic() bool; Key() Type; Kind() Kind; Len() int; Method(int) Method; MethodByName(string) (Method, bool); Name() string; NumField() int; NumIn() int; NumMethod() int; NumOut() int; Out(i int) Type; PkgPath() string; Size() uintptr; String() string; common() *commonType; rawString() string; runtimeType() *runtimeType; uncommon() *uncommonType}"u8),
    new(pkgpath: "nointerface"u8, name: "I"u8, want: "type I int"u8),
    new(pkgpath: "issue29198"u8, name: "FooServer"u8, gccgoVersion: 7, want: "type FooServer struct{FooServer *FooServer; user string; ctx context.Context}"u8),
    new(pkgpath: "issue30628"u8, name: "Apple"u8, want: "type Apple struct{hey sync.RWMutex; x int; RQ [517]struct{Count uintptr; NumBytes uintptr; Last uintptr}}"u8),
    new(pkgpath: "issue31540"u8, name: "S"u8, gccgoVersion: 7, want: "type S struct{b int; map[Y]Z}"u8),
    new(pkgpath: "issue34182"u8, name: "T1"u8, want: "type T1 struct{f *T2}"u8),
    new(pkgpath: "notinheap"u8, name: "S"u8, want: "type S struct{}"u8)
}.array();

public static void TestGoxImporter(ж<testing.T> Ꮡt) {
    testenv.MustHaveExec(new gccgoimporter_internal_test_package.testing_TжTB(Ꮡt));
    var initmap = new map<ж<types.Package>, global::go.go.@internal.gccgoimporter_package.InitData>();
    var imp = GetImporter(new @string[]{"testdata"u8}.slice(), initmap);
    foreach (var (_, vᴛ1) in importerTests.ΔRangeSnapshot()) {
        ref var test = ref heap(new importerTest(), out var Ꮡtest);
        test = vᴛ1;

        runImporterTest(Ꮡt, imp, initmap, Ꮡtest);
    }
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static readonly @string gccgoˢ = "GCCGO"u8;
internal static readonly @string gccgoˢ2 = "gccgo"u8;

// gccgoPath returns a path to gccgo if it is present (either in
// path or specified via GCCGO environment variable), or an
// empty string if no gccgo is available.
internal static @string gccgoPath() {
    @string gccgoname = os.Getenv(gccgoˢ);
    if (gccgoname == ""u8) {
        gccgoname = gccgoˢ2;
    }
    {
        var (gpath, gerr) = exec.LookPath(gccgoname); if (gerr == default!) {
            return gpath;
        }
    }
    return ""u8;
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static readonly @string versionˢ = "--version"u8;
internal static readonly @string testdataˢ = "testdata"u8;

public static void TestObjImporter(ж<testing.T> Ꮡt) {
    // This test relies on gccgo being around.
    @string gpath = gccgoPath();
    if (gpath == ""u8) {
        Ꮡt.Skip(thisTestNeedsGccgoˢ);
    }
    var (verout, err) = testenv.Command(new gccgoimporter_internal_test_package.testing_TжTB(Ꮡt), gpath, versionˢ).CombinedOutput();
    if (err != default!) {
        Ꮡt.Logf("%s"u8, verout);
        Ꮡt.Fatal(err);
    }
    var vers = regexp.MustCompile(@"(\d+)\.(\d+)"u8).FindSubmatch(verout);
    if (len(vers) == 0) {
        Ꮡt.Fatalf("could not find version number in %s"u8, verout);
    }
    (var major, err) = strconv.Atoi(((@string)vers[1]));
    if (err != default!) {
        Ꮡt.Fatal(err);
    }
    (var minor, err) = strconv.Atoi(((@string)vers[2]));
    if (err != default!) {
        Ꮡt.Fatal(err);
    }
    Ꮡt.Logf("gccgo version %d.%d"u8, major, minor);
    @string tmpdir = Ꮡt.TempDir();
    var initmap = new map<ж<types.Package>, global::go.go.@internal.gccgoimporter_package.InitData>();
    var imp = GetImporter(new @string[]{tmpdir}.slice(), initmap);
    @string artmpdir = Ꮡt.TempDir();
    var arinitmap = new map<ж<types.Package>, global::go.go.@internal.gccgoimporter_package.InitData>();
    var arimp = GetImporter(new @string[]{artmpdir}.slice(), arinitmap);
    foreach (var (_, vᴛ1) in importerTests.ΔRangeSnapshot()) {
        ref var test = ref heap(new importerTest(), out var Ꮡtest);
        test = vᴛ1;

        if (major < test.gccgoVersion) {
            // Support for type aliases was added in GCC 7.
            Ꮡt.Logf("skipping %q: not supported before gccgo version %d"u8, test.pkgpath, test.gccgoVersion);
            continue;
        }
        @string gofile = filepath.Join(testdataˢ, test.pkgpath + ".go");
        {
            var (_, errΔ1) = os.Stat(gofile); if (os.IsNotExist(errΔ1)) {
                continue;
            }
        }
        @string ofile = filepath.Join(tmpdir, test.pkgpath + ".o");
        @string afile = filepath.Join(artmpdir, "lib" + test.pkgpath + ".a");
        var cmd = testenv.Command(new gccgoimporter_internal_test_package.testing_TжTB(Ꮡt), gpath, "-fgo-pkgpath="u8 + test.pkgpath, "-c", "-o", ofile, gofile);
        var (@out, errΔ2) = cmd.CombinedOutput();
        if (errΔ2 != default!) {
            Ꮡt.Logf("%s"u8, @out);
            Ꮡt.Fatalf("gccgo %s failed: %s"u8, gofile, errΔ2);
        }
        runImporterTest(Ꮡt, imp, initmap, Ꮡtest);
        @string ar = os.Getenv("AR"u8);
        if (ar == ""u8) {
            ar = "ar"u8;
        }
        cmd = testenv.Command(new gccgoimporter_internal_test_package.testing_TжTB(Ꮡt), ar, "cr"u8, afile, ofile);
        (@out, errΔ2) = cmd.CombinedOutput();
        if (errΔ2 != default!) {
            Ꮡt.Logf("%s"u8, @out);
            Ꮡt.Fatalf("%s cr %s %s failed: %s"u8, ar, afile, ofile, errΔ2);
        }
        runImporterTest(Ꮡt, arimp, arinitmap, Ꮡtest);
        {
            errΔ2 = os.Remove(ofile); if (errΔ2 != default!) {
                Ꮡt.Fatal(errΔ2);
            }
        }
        {
            errΔ2 = os.Remove(afile); if (errΔ2 != default!) {
                Ꮡt.Fatal(errΔ2);
            }
        }
    }
}

} // end gccgoimporter_internal_test_package
