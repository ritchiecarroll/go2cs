// go2cs metadata anchor for a production-reference test project: the test assembly
// REFERENCES the colocated production project instead of
// recompiling its sources, so the production assembly is the single identity for the
// production types and no production class partial may be declared here. The first —
// and only — class is the test metadata class the go2cs-gen generators anchor
// generated adapters and partials to.
global using static global::go.io.fs_package;

// <ImportedTypeAliases>
global using osꓸDirEntry = go.io.fs_package.DirEntry;
global using osꓸFileInfo = go.io.fs_package.FileInfo;
global using osꓸFileMode = go.io.fs_package.FileMode;
global using osꓸPathError = go.io.fs_package.PathError;
global using osꓸSignal = go.os_package.ΔSignal;
global using timeꓸLocation = go.time_package.ΔLocation;
global using timeꓸMonth = go.time_package.ΔMonth;
global using timeꓸWeekday = go.time_package.ΔWeekday;
// </ImportedTypeAliases>

using go;
using static global::go.io.fs_test_package;

// <ExportedTypeAliases>
[assembly: GoDynamicTypeLift("7374727563747b667320696f2f66732e46533b207061747465726e20737472696e673b20726573756c7420737472696e677d", "globTestsᴛ1")]
[assembly: GoDynamicTypeLift("7374727563747b696e70757420696f2f66735f746573742e666f726d6174546573743b2077616e7446696c65496e666f20737472696e673b2077616e74446972456e74727920737472696e677d", "formatTestsᴛ1")]
[assembly: GoDynamicTypeLift("7374727563747b696f2f66732e46537d", "TestReadDirPath_fsys")]
[assembly: GoDynamicTypeLift("7374727563747b6e616d6520737472696e673b206f6b20626f6f6c7d", "isValidPathTestsᴛ1")]
// </ExportedTypeAliases>

// <InterfaceImplementations>
[assembly: GoImplement<TestReadDirPath_fsys, go.io.fs_package.FS>(Promoted = true)]
[assembly: GoImplement<TestReadDirPath_fsys, go.io.fs_package.FS>]
[assembly: GoImplement<formatTest, go.io.fs_package.DirEntry>(Pointer = true)]
[assembly: GoImplement<formatTest, go.io.fs_package.FileInfo>(Pointer = true)]
[assembly: GoImplement<globOnly, go.io.fs_package.FS>]
[assembly: GoImplement<globOnly, go.io.fs_package.GlobFS>(Promoted = true)]
[assembly: GoImplement<go.testing.fstest_package.MapFS, go.io.fs_package.GlobFS>]
[assembly: GoImplement<go.testing.fstest_package.MapFS, go.io.fs_package.ReadDirFS>]
[assembly: GoImplement<go.testing.fstest_package.MapFS, go.io.fs_package.ReadFileFS>]
[assembly: GoImplement<go.testing.fstest_package.MapFS, go.io.fs_package.StatFS>]
[assembly: GoImplement<go.testing.fstest_package.MapFS, go.io.fs_package.SubFS>]
[assembly: GoImplement<openOnly, go.io.fs_package.FS>(Promoted = true)]
[assembly: GoImplement<openOnly, go.io.fs_package.FS>]
[assembly: GoImplement<readDirOnly, go.io.fs_package.FS>]
[assembly: GoImplement<readDirOnly, go.io.fs_package.ReadDirFS>(Promoted = true)]
[assembly: GoImplement<readFileOnly, go.io.fs_package.FS>]
[assembly: GoImplement<readFileOnly, go.io.fs_package.ReadFileFS>(Promoted = true)]
[assembly: GoImplement<statOnly, go.io.fs_package.FS>]
[assembly: GoImplement<statOnly, go.io.fs_package.StatFS>(Promoted = true)]
[assembly: GoImplement<subOnly, go.io.fs_package.FS>]
[assembly: GoImplement<subOnly, go.io.fs_package.SubFS>(Promoted = true)]
// </InterfaceImplementations>

// <ImplicitConversions>
// </ImplicitConversions>

// Go source positions are recorded here, one `GoPositionMap` attribute per converted
// source file in this compilation, so that `runtime.Caller` and the tracebacks built on it
// can name the GO file and line a frame was converted from rather than the emitted C# one.
// Each record carries the Go file's identity and an encoded C#-line to Go-line table
// TOGETHER: a frame either has a record and reports a position that exists in the Go tree,
// or has none - golib, the BCL and hand-written conversions - and reports its own C# position.

// <GoSourcePositionMaps>
[assembly: go.GoPositionMap("io/fs/format_test.go", "format_test.cs", "ABcugqaCpoKmgqaCpoKmgqaCAAgGAC5mgrKCgsqCsoKC", "", "", "62=Date/1/4/10,Date/2/4/21,Date/3/4/32,Date/4/4/43")]
[assembly: go.GoPositionMap("io/fs/fs_test.go", "fs_test.cs", "AA8YABk8goKCgg==")]
[assembly: go.GoPositionMap("io/fs/glob_test.go", "glob_test.cs", "ABQgAAcWgoKCgoKUgqaCgoKClILKgoKCgoLKpoKC/ID0goKCgrqCloI=", "74-79:1", "", "21=DirFS/1/5/4,DirFS/2/5/5,DirFS/3/5/6,DirFS/4/5/7,DirFS/5/5/8")]
[assembly: go.GoPositionMap("io/fs/readdir_test.go", "readdir_test.cs", "ABUkgNSCgoKCgoKUuoKWgpaCgpSCAAgGggALHgADEoKSooKCloKAgqSAkqSAguyCgoKUAAkGgoKCgoCS", "21-30:1;76-92:1", "", "62=Now/1/2/4,Now/2/2/10")]
[assembly: go.GoPositionMap("io/fs/readfile_test.go", "readfile_test.cs", "ABAeABMmgAAKCJSCgqiCgqiCgpSCgriCgoKCgJI=", "", "", "17=Now/1/2/4,Now/2/2/10")]
[assembly: go.GoPositionMap("io/fs/stat_test.go", "stat_test.cs", "ABEegOSCgoKCgoKUuoKWgg==", "18-27:1")]
[assembly: go.GoPositionMap("io/fs/sub_test.go", "sub_test.cs", "ABEegAAKBIKCgoKClIKCloKCgoKUuoKWgoSCgpSCgpSCloKC", "18-37:1")]
[assembly: go.GoPositionMap("io/fs/walk_test.go", "walk_test.cs", "ABkuABc0ooKCuIKCkoKUpqzSgoKCpoKCgpSUpoKEgpKCmIKClIKUgoKU+KKCgoCCtoKAgqSCgoKClIKUlIKUgoK0", "58-64:1;73-77:1;94-96:1;105-110:2;126-134:1")]
// </GoSourcePositionMaps>

namespace go.io;

[GoPackage("fs_test")]
public static partial class fs_test_package
{
    // C# nested types declared with no access modifier are always private, and the
    // Go type declarations in this package's converted sources are deliberately
    // bare so they read more like the original Go code. The real accessibility for
    // the types - public for a Go-exported name, internal otherwise - are defined
    // via declarations below.

    // <TypeAccessibility>
    internal partial struct TestFileInfoToDirEntry_tests {}
    internal partial struct TestReadDirPath_fsys {}
    internal partial struct formatTest {}
    internal partial struct formatTestsᴛ1 {}
    internal partial struct globOnly {}
    internal partial struct globTestsᴛ1 {}
    internal partial struct isValidPathTestsᴛ1 {}
    internal partial struct openOnly {}
    internal partial struct readDirOnly {}
    internal partial struct readFileOnly {}
    internal partial struct statOnly {}
    internal partial struct subOnly {}
    public partial struct Node {}
    // </TypeAccessibility>

    // Go initializes an imported package before the importing package, for every import
    // form - not only the blank one. .NET would never load an assembly nothing has touched
    // yet, so each import that initializes anything is forced below: once per assembly, and
    // ahead of this package's own `init` functions, which this file being the first compile
    // item of the project guarantees.

    // <ImportInitializers>
    [GoInit] internal static void initᴛᴛimportꓸerrors() => builtin.initPackage(typeof(errors_package));
    [GoInit] internal static void initᴛᴛimportꓸfmt() => builtin.initPackage(typeof(fmt_package));
    [GoInit] internal static void initᴛᴛimportꓸioꓸfs() => builtin.initPackage(typeof(go.io.fs_package));
    [GoInit] internal static void initᴛᴛimportꓸos() => builtin.initPackage(typeof(os_package));
    [GoInit] internal static void initᴛᴛimportꓸpath() => builtin.initPackage(typeof(path_package));
    [GoInit] internal static void initᴛᴛimportꓸpathꓸfilepath() => builtin.initPackage(typeof(go.path.filepath_package));
    [GoInit] internal static void initᴛᴛimportꓸslices() => builtin.initPackage(typeof(slices_package));
    [GoInit] internal static void initᴛᴛimportꓸstrings() => builtin.initPackage(typeof(strings_package));
    [GoInit] internal static void initᴛᴛimportꓸtesting() => builtin.initPackage(typeof(testing_package));
    [GoInit] internal static void initᴛᴛimportꓸtestingꓸfstest() => builtin.initPackage(typeof(go.testing.fstest_package));
    [GoInit] internal static void initᴛᴛimportꓸtime() => builtin.initPackage(typeof(time_package));
    // </ImportInitializers>
    // Go runs every `init` in the package under test - the production files' included -
    // before the first test. The production package is a REFERENCED assembly here, whose
    // module constructor .NET would not run until something in it is touched, so that
    // initialization is forced before anything else in this test module runs.
    [GoInit] internal static void initᴛᴛproduction() {
        builtin.initPackage(typeof(global::go.io.fs_package));
    }
}
