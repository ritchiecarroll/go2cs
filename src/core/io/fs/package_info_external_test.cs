// go2cs metadata anchor for the EXTERNAL test package (<name>_test): GoImplement /
// GoImplicitConv attributes recorded by its converted _test files whose GENERATED code
// (adapter classes, partial-struct implementations, conversion operators) must anchor to
// the test package class — the source generators host output in the first class of the
// attribute-bearing file, and test-file cast sites reference the adapters as members of
// the test package class. Production-anchored records stay in package_test_info.cs.

// <ImportedTypeAliases>
// </ImportedTypeAliases>

using go;
using static go.io.fs_package;
using static go.io.fs_test_package;

// <ExportedTypeAliases>
[assembly: GoDynamicTypeLift("7374727563747b667320696f2f66732e46533b207061747465726e20737472696e673b20726573756c7420737472696e677d", "globTestsᴛ1")]
[assembly: GoDynamicTypeLift("7374727563747b696e70757420696f2f66735f746573742e666f726d6174546573743b2077616e7446696c65496e666f20737472696e673b2077616e74446972456e74727920737472696e677d", "formatTestsᴛ1")]
[assembly: GoDynamicTypeLift("7374727563747b696f2f66732e46537d", "TestReadDirPath_fsys")]
[assembly: GoDynamicTypeLift("7374727563747b6e616d6520737472696e673b206f6b20626f6f6c7d", "isValidPathTestsᴛ1")]
// </ExportedTypeAliases>

// <InterfaceImplementations>
[assembly: GoImplement<TestReadDirPath_fsys, FS>(Promoted = true)]
[assembly: GoImplement<TestReadDirPath_fsys, FS>]
[assembly: GoImplement<formatTest, DirEntry>(Pointer = true)]
[assembly: GoImplement<formatTest, FileInfo>(Pointer = true)]
[assembly: GoImplement<globOnly, FS>]
[assembly: GoImplement<globOnly, GlobFS>(Promoted = true)]
[assembly: GoImplement<go.testing.fstest_package.MapFS, FS>]
[assembly: GoImplement<go.testing.fstest_package.MapFS, GlobFS>]
[assembly: GoImplement<go.testing.fstest_package.MapFS, ReadDirFS>]
[assembly: GoImplement<go.testing.fstest_package.MapFS, ReadFileFS>]
[assembly: GoImplement<go.testing.fstest_package.MapFS, StatFS>]
[assembly: GoImplement<go.testing.fstest_package.MapFS, SubFS>]
[assembly: GoImplement<openOnly, FS>(Promoted = true)]
[assembly: GoImplement<openOnly, FS>]
[assembly: GoImplement<readDirOnly, FS>]
[assembly: GoImplement<readDirOnly, ReadDirFS>(Promoted = true)]
[assembly: GoImplement<readFileOnly, FS>]
[assembly: GoImplement<readFileOnly, ReadFileFS>(Promoted = true)]
[assembly: GoImplement<statOnly, FS>]
[assembly: GoImplement<statOnly, StatFS>(Promoted = true)]
[assembly: GoImplement<subOnly, FS>]
[assembly: GoImplement<subOnly, SubFS>(Promoted = true)]
// </InterfaceImplementations>

// <ImplicitConversions>
// </ImplicitConversions>

// The struct types whose fields this package reaches through a pointer, as Go spells it, `p.f`:
// go2cs-gen emits a field property on the pointer for each of their fields.
[assembly: GoFieldAccess(typeof(Node))]

// Go source positions are recorded here, one `GoPositionMap` attribute per converted
// source file in this compilation, so that `runtime.Caller` and the tracebacks built on it
// can name the GO file and line a frame was converted from rather than the emitted C# one.
// Each record carries the Go file's identity and an encoded C#-line to Go-line table
// TOGETHER: a frame either has a record and reports a position that exists in the Go tree,
// or has none - golib, the BCL and hand-written conversions - and reports its own C# position.

// <GoSourcePositionMaps>
// </GoSourcePositionMaps>

namespace go.io;

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
    [GoInit] internal static void initᴛᴛimportꓸos() => builtin.initPackage(typeof(os_package));
    [GoInit] internal static void initᴛᴛimportꓸpath() => builtin.initPackage(typeof(path_package));
    [GoInit] internal static void initᴛᴛimportꓸpathꓸfilepath() => builtin.initPackage(typeof(go.path.filepath_package));
    [GoInit] internal static void initᴛᴛimportꓸslices() => builtin.initPackage(typeof(slices_package));
    [GoInit] internal static void initᴛᴛimportꓸstrings() => builtin.initPackage(typeof(strings_package));
    [GoInit] internal static void initᴛᴛimportꓸtesting() => builtin.initPackage(typeof(testing_package));
    [GoInit] internal static void initᴛᴛimportꓸtestingꓸfstest() => builtin.initPackage(typeof(go.testing.fstest_package));
    [GoInit] internal static void initᴛᴛimportꓸtime() => builtin.initPackage(typeof(time_package));
    // </ImportInitializers>
}
