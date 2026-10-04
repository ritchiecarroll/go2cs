// go2cs metadata anchor for the INTERNAL (white-box bridge) test class: GoImplement /
// GoImplicitConv attributes whose GENERATED code must merge with a bridge-declared type
// anchor here — the source generators host output in the first class of the
// attribute-bearing file, and only this file's first class is the bridge. Records for
// production and external-test types stay in package_test_info.cs.

// <ImportedTypeAliases>
using bytes = go.bytes_package;
// </ImportedTypeAliases>

using go;
using static go.fmt_package;
using static go.fmt_internal_test_package;

// <ExportedTypeAliases>
[assembly: GoDynamicTypeLift("696e746572666163657b556e777261702829205b5d6572726f727d", "splitErr_type")]
[assembly: GoDynamicTypeLift("7374727563747b636f756e7420696e743b206465736320737472696e673b20666e2066756e6328297d", "mallocTestᴛ1")]
[assembly: GoDynamicTypeLift("7374727563747b666d7420737472696e673b20696e205b5d616e793b206f757420737472696e677d", "startestsᴛ1")]
[assembly: GoDynamicTypeLift("7374727563747b666d7420737472696e673b20696e20616e793b206f757420737472696e677d", "panictestsᴛ1")]
[assembly: GoDynamicTypeLift("7374727563747b666d7420737472696e673b2076616c20616e793b206f757420737472696e677d", "fmtTestsᴛ1")]
[assembly: GoDynamicTypeLift("7374727563747b666d7420737472696e673b2076616c20666d745f746573742e53453b206f757420737472696e677d", "reorderTestsᴛ1")]
[assembly: GoDynamicTypeLift("7374727563747b666f726d617420737472696e673b207620616e797d", "eofTestsᴛ1")]
[assembly: GoDynamicTypeLift("7374727563747b696e20737472696e673b206f757420737472696e677d", "flagtestsᴛ1")]
[assembly: GoDynamicTypeLift("7374727563747b696e20737472696e673b2076616c20616e793b206f757420737472696e677d", "formatterFlagTestsᴛ1")]
[assembly: GoDynamicTypeLift("7374727563747b6e616d6520737472696e673b20662066756e6328737472696e672920696f2e5265616465727d", "readersᴛ1")]
// </ExportedTypeAliases>

// <InterfaceImplementations>
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
[assembly: go.GoPositionMap("fmt/export_test.go", "export_test.cs", "AAoOkg==")]
// </GoSourcePositionMaps>

namespace go;

[GoPackage("fmt")]
public static partial class fmt_internal_test_package
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
    [GoInit] internal static void initᴛᴛimportꓸbufio() => builtin.initPackage(typeof(bufio_package));
    [GoInit] internal static void initᴛᴛimportꓸbytes() => builtin.initPackage(typeof(bytes_package));
    [GoInit] internal static void initᴛᴛimportꓸerrors() => builtin.initPackage(typeof(errors_package));
    [GoInit] internal static void initᴛᴛimportꓸfmt() => builtin.initPackage(typeof(fmt_package));
    [GoInit] internal static void initᴛᴛimportꓸinternalꓸrace() => builtin.initPackage(typeof(@internal.race_package));
    [GoInit] internal static void initᴛᴛimportꓸio() => builtin.initPackage(typeof(io_package));
    [GoInit] internal static void initᴛᴛimportꓸmath() => builtin.initPackage(typeof(math_package));
    [GoInit] internal static void initᴛᴛimportꓸreflect() => builtin.initPackage(typeof(reflect_package));
    [GoInit] internal static void initᴛᴛimportꓸregexp() => builtin.initPackage(typeof(regexp_package));
    [GoInit] internal static void initᴛᴛimportꓸstrings() => builtin.initPackage(typeof(strings_package));
    [GoInit] internal static void initᴛᴛimportꓸtesting() => builtin.initPackage(typeof(testing_package));
    [GoInit] internal static void initᴛᴛimportꓸtestingꓸiotest() => builtin.initPackage(typeof(go.testing.iotest_package));
    [GoInit] internal static void initᴛᴛimportꓸtime() => builtin.initPackage(typeof(time_package));
    [GoInit] internal static void initᴛᴛimportꓸunicode() => builtin.initPackage(typeof(unicode_package));
    [GoInit] internal static void initᴛᴛimportꓸunicodeꓸutf8() => builtin.initPackage(typeof(go.unicode.utf8_package));
    // </ImportInitializers>
}
