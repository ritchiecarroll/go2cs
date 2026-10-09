// go2cs metadata anchor for the INTERNAL (white-box bridge) test class: GoImplement /
// GoImplicitConv attributes whose GENERATED code must merge with a bridge-declared type
// anchor here — the source generators host output in the first class of the
// attribute-bearing file, and only this file's first class is the bridge. Records for
// production and external-test types stay in package_test_info.cs.

// <ImportedTypeAliases>
// </ImportedTypeAliases>

using go;
using static go.strings_package;
using static go.strings_internal_test_package;

// <ExportedTypeAliases>
[assembly: GoDynamicTypeLift("7374727563747b6120737472696e673b206220737472696e673b206920696e747d", "compareTestsᴛ1")]
[assembly: GoDynamicTypeLift("7374727563747b6620737472696e673b20696e20737472696e673b2061726720737472696e673b206f757420737472696e677d", "trimTestsᴛ1")]
[assembly: GoDynamicTypeLift("7374727563747b6620737472696e67735f746573742e7072656469636174653b20696e20737472696e673b207472696d4f757420737472696e673b206c6566744f757420737472696e673b2072696768744f757420737472696e677d", "trimFuncTestsᴛ1")]
[assembly: GoDynamicTypeLift("7374727563747b696e20737472696e673b206620737472696e67735f746573742e7072656469636174653b20666972737420696e743b206c61737420696e747d", "indexFuncTestsᴛ1")]
[assembly: GoDynamicTypeLift("7374727563747b696e20737472696e673b206f6c6420737472696e673b206e657720737472696e673b206e20696e743b206f757420737472696e677d", "ReplaceTestsᴛ1")]
[assembly: GoDynamicTypeLift("7374727563747b696e20737472696e673b206f7574205b5d72756e653b206c6f73737920626f6f6c7d", "RunesTestsᴛ1")]
[assembly: GoDynamicTypeLift("7374727563747b696e20737472696e673b206f757420737472696e673b20636f756e7420696e747d", "RepeatTestsᴛ1")]
[assembly: GoDynamicTypeLift("7374727563747b696e20737472696e673b206f757420737472696e677d", "TitleTestsᴛ1")]
[assembly: GoDynamicTypeLift("7374727563747b696e20737472696e673b207265706c20737472696e673b206f757420737472696e677d", "toValidUTF8Testsᴛ1")]
[assembly: GoDynamicTypeLift("7374727563747b6e616d6520737472696e673b206461746120737472696e677d", "mapdataᴛ1")]
[assembly: GoDynamicTypeLift("7374727563747b6e616d6520737472696e673b20662066756e63282a737472696e67732e526561646572297d", "UnreadRuneErrorTestsᴛ1")]
[assembly: GoDynamicTypeLift("7374727563747b72202a737472696e67732e5265706c616365723b2077616e7420737472696e677d", "algorithmTestCasesᴛ1")]
[assembly: GoDynamicTypeLift("7374727563747b7320737472696e673b2073657020737472696e673b20616674657220737472696e673b20666f756e6420626f6f6c7d", "cutPrefixTestsᴛ1")]
[assembly: GoDynamicTypeLift("7374727563747b7320737472696e673b2073657020737472696e673b206265666f726520737472696e673b20616674657220737472696e673b20666f756e6420626f6f6c7d", "cutTestsᴛ1")]
[assembly: GoDynamicTypeLift("7374727563747b7320737472696e673b2073657020737472696e673b206265666f726520737472696e673b20666f756e6420626f6f6c7d", "cutSuffixTestsᴛ1")]
[assembly: GoDynamicTypeLift("7374727563747b7320737472696e673b2073657020737472696e673b206e756d20696e747d", "CountTestsᴛ1")]
[assembly: GoDynamicTypeLift("7374727563747b7320737472696e673b207420737472696e673b206f757420626f6f6c7d", "EqualFoldTestsᴛ1")]
[assembly: GoDynamicTypeLift("7374727563747b73747220737472696e673b20722072756e653b20657870656374656420626f6f6c7d", "ContainsRuneTestsᴛ1")]
[assembly: GoDynamicTypeLift("7374727563747b73747220737472696e673b2073756273747220737472696e673b20657870656374656420626f6f6c7d", "ContainsTestsᴛ1")]
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
[assembly: go.GoPositionMap("strings/export_test.go", "export_test.cs", "AAkOooKmooKCprKClJSEgoKSgoKCgrimgqaCgg==")]
// </GoSourcePositionMaps>

namespace go;

[GoPackage("strings")]
public static partial class strings_internal_test_package
{
    // C# nested types declared with no access modifier are always private, and the
    // Go type declarations in this package's converted sources are deliberately
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
    [GoInit] internal static void initᴛᴛimportꓸfmt() => builtin.initPackage(typeof(fmt_package));
    [GoInit] internal static void initᴛᴛimportꓸinternalꓸtestenv() => builtin.initPackage(typeof(@internal.testenv_package));
    [GoInit] internal static void initᴛᴛimportꓸio() => builtin.initPackage(typeof(io_package));
    [GoInit] internal static void initᴛᴛimportꓸiter() => builtin.initPackage(typeof(iter_package));
    [GoInit] internal static void initᴛᴛimportꓸmath() => builtin.initPackage(typeof(math_package));
    [GoInit] internal static void initᴛᴛimportꓸmathꓸrand() => builtin.initPackage(typeof(go.math.rand_package));
    [GoInit] internal static void initᴛᴛimportꓸslices() => builtin.initPackage(typeof(slices_package));
    [GoInit] internal static void initᴛᴛimportꓸstrconv() => builtin.initPackage(typeof(strconv_package));
    [GoInit] internal static void initᴛᴛimportꓸstrings() => builtin.initPackage(typeof(strings_package));
    [GoInit] internal static void initᴛᴛimportꓸsync() => builtin.initPackage(typeof(sync_package));
    [GoInit] internal static void initᴛᴛimportꓸtesting() => builtin.initPackage(typeof(testing_package));
    [GoInit] internal static void initᴛᴛimportꓸunicode() => builtin.initPackage(typeof(unicode_package));
    [GoInit] internal static void initᴛᴛimportꓸunicodeꓸutf8() => builtin.initPackage(typeof(go.unicode.utf8_package));
    // </ImportInitializers>
}
