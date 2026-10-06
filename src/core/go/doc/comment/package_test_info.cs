// go2cs metadata anchor for a production-reference test project: the test assembly
// REFERENCES the colocated production project instead of
// recompiling its sources, so the production assembly is the single identity for the
// production types and no production class partial may be declared here. The first —
// and only — class is the test metadata class the go2cs-gen generators anchor
// generated adapters and partials to.
global using static global::go.go.doc.comment_package;

// <ImportedTypeAliases>
global using flagꓸErrorHandling = go.flag_package.ΔErrorHandling;
global using jsonꓸToken = object;
global using jsonꓸΔToken = object;
global using timeꓸLocation = go.time_package.ΔLocation;
global using timeꓸMonth = go.time_package.ΔMonth;
global using timeꓸWeekday = go.time_package.ΔWeekday;
using bytes = go.bytes_package;
// </ImportedTypeAliases>

using go;
using static global::go.go.doc.comment_internal_test_package;

// <ExportedTypeAliases>
[assembly: GoDynamicTypeLift("7374727563747b696e20737472696e673b206f757420737472696e677d", "autoURLTestsᴛ1")]
[assembly: GoDynamicTypeLift("7374727563747b6c696e6520737472696e673b206f6b20626f6f6c7d", "oldHeadingTestsᴛ1")]
[assembly: GoTypeAlias("Text", "ΔText")]
// </ExportedTypeAliases>

// <InterfaceImplementations>
[assembly: GoImplement<bytes_package.Buffer, io_package.Writer>(Pointer = true)]
[assembly: GoImplement<testing_package.T, testing_package.TB>(Pointer = true)]
// </InterfaceImplementations>

// <ImplicitConversions>
[assembly: GoImplicitConv<global::go.go.doc.comment_package.Doc, ж<global::go.go.doc.comment_package.Doc>>(Indirect = true)]
[assembly: GoImplicitConv<global::go.go.doc.comment_package.LinkDef, ж<global::go.go.doc.comment_package.LinkDef>>(Indirect = true)]
[assembly: GoImplicitConv<global::go.go.doc.comment_package.ListItem, ж<global::go.go.doc.comment_package.ListItem>>(Indirect = true)]
// </ImplicitConversions>

// Go source positions are recorded here, one `GoPositionMap` attribute per converted
// source file in this compilation, so that `runtime.Caller` and the tracebacks built on it
// can name the GO file and line a frame was converted from rather than the emitted C# one.
// Each record carries the Go file's identity and an encoded C#-line to Go-line table
// TOGETHER: a frame either has a record and reports a position that exists in the Go tree,
// or has none - golib, the BCL and hand-written conversions - and reports its own C# position.

// <GoSourcePositionMaps>
[assembly: global::go.GoPositionMap("go/doc/comment/old_test.go", "old_test.cs", "ABAWABMugoKCAAoKABw+goKCgg==")]
[assembly: global::go.GoPositionMap("go/doc/comment/parse_test.go", "parse_test.cs", "AAoUkg==")]
[assembly: global::go.GoPositionMap("go/doc/comment/std_test.go", "std_test.cs", "ABcegoKCloKCgqaEgoKC")]
[assembly: global::go.GoPositionMap("go/doc/comment/testdata_test.go", "testdata_test.cs", "ABskgoKClIK4goKUlIKGlJbKlIKSgoKClIKCgqaClIKCgoKUgpikpKSkAAITAAIYgu6CgoKmgqS2goKCgoKUtoKCyLaCgsiCtoK2goLIgraCtoK2goLIgpSCyIKUgsiCgoLagraCyKKCgg==", "28-33:1;34-41:2;43-49:3;51-91:4")]
[assembly: global::go.GoPositionMap("go/doc/comment/wrap_test.go", "wrap_test.cs", "ABEg1IKClIKWgoKCgoKClIKUlpKSgoKSkpaCgoKUgpSCgpSClIKClJSSkoKUlIKogoIABxgACgqCgoKClIKUgoKCgoKClIKClIKCgsyCgpSCgoKU", "41-88:1;45-86:1.1")]
// </GoSourcePositionMaps>

namespace go.go.doc;

[GoPackage("comment")]
public static partial class comment_internal_test_package
{
    // C# nested types declared with no access modifier are always private, and the
    // `[GoType]` declarations in this package's converted sources are deliberately
    // bare so they read more like the original Go code. The real accessibility for
    // the types - public for a Go-exported name, internal otherwise - are defined
    // via declarations below.

    // <TypeAccessibility>
    // </TypeAccessibility>

    // Go initializes an imported package before the importing package, for every import
    // form - not only the blank one. .NET would never load an assembly nothing has touched
    // yet, so each import that initializes anything is forced below: once per assembly, and
    // ahead of this package's own `init` functions, which this file being the first compile
    // item of the project guarantees.

    // <ImportInitializers>
    [GoInit] internal static void initᴛᴛimportꓸbytes() => builtin.initPackage(typeof(bytes_package));
    [GoInit] internal static void initᴛᴛimportꓸencodingꓸjson() => builtin.initPackage(typeof(encoding.json_package));
    [GoInit] internal static void initᴛᴛimportꓸflag() => builtin.initPackage(typeof(flag_package));
    [GoInit] internal static void initᴛᴛimportꓸfmt() => builtin.initPackage(typeof(fmt_package));
    [GoInit] internal static void initᴛᴛimportꓸinternalꓸdiff() => builtin.initPackage(typeof(@internal.diff_package));
    [GoInit] internal static void initᴛᴛimportꓸinternalꓸtestenv() => builtin.initPackage(typeof(@internal.testenv_package));
    [GoInit] internal static void initᴛᴛimportꓸinternalꓸtxtar() => builtin.initPackage(typeof(@internal.txtar_package));
    [GoInit] internal static void initᴛᴛimportꓸmathꓸrand() => builtin.initPackage(typeof(math.rand_package));
    [GoInit] internal static void initᴛᴛimportꓸpathꓸfilepath() => builtin.initPackage(typeof(path.filepath_package));
    [GoInit] internal static void initᴛᴛimportꓸslices() => builtin.initPackage(typeof(slices_package));
    [GoInit] internal static void initᴛᴛimportꓸstrings() => builtin.initPackage(typeof(strings_package));
    [GoInit] internal static void initᴛᴛimportꓸtesting() => builtin.initPackage(typeof(testing_package));
    [GoInit] internal static void initᴛᴛimportꓸtime() => builtin.initPackage(typeof(time_package));
    [GoInit] internal static void initᴛᴛimportꓸunicodeꓸutf8() => builtin.initPackage(typeof(global::go.unicode.utf8_package));
    // </ImportInitializers>
    // Go runs every `init` in the package under test - the production files' included -
    // before the first test. The production package is a REFERENCED assembly here, whose
    // module constructor .NET would not run until something in it is touched, so that
    // initialization is forced before anything else in this test module runs.
    [GoInit] internal static void initᴛᴛproduction() {
        builtin.initPackage(typeof(global::go.go.doc.comment_package));
    }
}
