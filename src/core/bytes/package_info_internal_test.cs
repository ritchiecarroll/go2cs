// go2cs metadata anchor for the INTERNAL (white-box bridge) test class: GoImplement /
// GoImplicitConv attributes whose GENERATED code must merge with a bridge-declared type
// anchor here — the source generators host output in the first class of the
// attribute-bearing file, and only this file's first class is the bridge. Records for
// production and external-test types stay in package_test_info.cs.

// <ImportedTypeAliases>
// </ImportedTypeAliases>

using go;
using static go.bytes_package;
using static go.bytes_internal_test_package;

// <ExportedTypeAliases>
[assembly: GoDynamicTypeLift("7374727563747b61205b5d627974653b2062205b5d627974653b206920696e747d", "compareTestsᴛ1")]
[assembly: GoDynamicTypeLift("7374727563747b62205b5d627974653b20722072756e653b20657870656374656420626f6f6c7d", "ContainsRuneTestsᴛ1")]
[assembly: GoDynamicTypeLift("7374727563747b62205b5d627974653b20737562736c696365205b5d627974653b2077616e7420626f6f6c7d", "containsTestsᴛ1")]
[assembly: GoDynamicTypeLift("7374727563747b62205b5d627974653b2073756273747220737472696e673b20657870656374656420626f6f6c7d", "ContainsAnyTestsᴛ1")]
[assembly: GoDynamicTypeLift("7374727563747b62756666657220737472696e673b2064656c696d20627974653b206578706563746564205b5d737472696e673b20657272206572726f727d", "readBytesTestsᴛ1")]
[assembly: GoDynamicTypeLift("7374727563747b696e20737472696e673b207265706c20737472696e673b206f757420737472696e677d", "toValidUTF8Testsᴛ1")]
[assembly: GoDynamicTypeLift("7374727563747b6e616d6520737472696e673b2064617461205b5d627974657d", "bytesdataᴛ1")]
[assembly: GoDynamicTypeLift("7374727563747b6e616d6520737472696e673b20662066756e63282a62797465732e526561646572297d", "UnreadRuneErrorTestsᴛ1")]
[assembly: GoDynamicTypeLift("7374727563747b7320737472696e673b2073657020737472696e673b20616674657220737472696e673b20666f756e6420626f6f6c7d", "cutPrefixTestsᴛ1")]
[assembly: GoDynamicTypeLift("7374727563747b7320737472696e673b2073657020737472696e673b206265666f726520737472696e673b20616674657220737472696e673b20666f756e6420626f6f6c7d", "cutTestsᴛ1")]
[assembly: GoDynamicTypeLift("7374727563747b7320737472696e673b2073657020737472696e673b206265666f726520737472696e673b20666f756e6420626f6f6c7d", "cutSuffixTestsᴛ1")]
[assembly: GoDynamicTypeLift("7374727563747b7320737472696e673b207420737472696e673b206f757420626f6f6c7d", "EqualFoldTestsᴛ1")]
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
[assembly: go.GoPositionMap("bytes/export_test.go", "export_test.cs", "AAoQ")]
// </GoSourcePositionMaps>

namespace go;

[GoPackage("bytes")]
public static partial class bytes_internal_test_package
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
