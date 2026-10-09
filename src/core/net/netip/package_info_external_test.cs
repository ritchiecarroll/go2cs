// go2cs metadata anchor for the EXTERNAL test package (<name>_test): GoImplement /
// GoImplicitConv attributes recorded by its converted _test files whose GENERATED code
// (adapter classes, partial-struct implementations, conversion operators) must anchor to
// the test package class — the source generators host output in the first class of the
// attribute-bearing file, and test-file cast sites reference the adapters as members of
// the test package class. Production-anchored records stay in package_test_info.cs.

// <ImportedTypeAliases>
// </ImportedTypeAliases>

using go;
using static go.net.netip_package;
using static go.net.netip_test_package;

// <ExportedTypeAliases>
[assembly: GoDynamicTypeLift("7374727563747b6e616d6520737472696e673b20697020737472696e677d", "parseBenchInputsᴛ1")]
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
// </GoSourcePositionMaps>

namespace go.net;

public static partial class netip_test_package
{
    // C# nested types declared with no access modifier are always private, and the
    // Go type declarations in this package's converted sources are deliberately
    // bare so they read more like the original Go code. The real accessibility for
    // the types - public for a Go-exported name, internal otherwise - are defined
    // via declarations below.

    // <TypeAccessibility>
    internal partial interface appendMarshaler {}
    internal partial interface netipType {}
    internal partial interface netipTypeCmp {}
    internal partial struct BenchmarkBinaryMarshalRoundTrip_tests {}
    internal partial struct BenchmarkPrefixMasking_tests {}
    internal partial struct TestAddrAppendText_tests {}
    [GoValueClone("@in")] internal partial struct TestAddrFrom16_tests {}
    internal partial struct TestAddrFromSlice_tests {}
    internal partial struct TestAddrLessCompare_tests {}
    internal partial struct TestAddrMarshalUnmarshalBinary_tests {}
    internal partial struct TestAddrPortCompare_tests {}
    internal partial struct TestAddrPortMarshalTextString_tests {}
    internal partial struct TestAddrPortMarshalUnmarshalBinary_tests {}
    internal partial struct TestAddrPortString_tests {}
    internal partial struct TestAddrStringAllocs_tests {}
    internal partial struct TestAddrWellKnown_tests {}
    [GoValueClone("want")] internal partial struct TestAs4_tests {}
    internal partial struct TestAsSlice_tests {}
    internal partial struct TestIPProperties_tests {}
    internal partial struct TestIPStringExpanded_tests {}
    internal partial struct TestIs4AndIs6_tests {}
    internal partial struct TestIs4In6_tests {}
    internal partial struct TestParseAddr_type {}
    internal partial struct TestParsePrefixAllocs_tests {}
    internal partial struct TestParsePrefixError_tests {}
    internal partial struct TestPrefixCompare_tests {}
    internal partial struct TestPrefixFromInvalidBits_tests {}
    internal partial struct TestPrefixIsSingleIP_tests {}
    internal partial struct TestPrefixMarshalTextString_tests {}
    [GoLocalName("testCase")] internal partial struct TestPrefixMarshalUnmarshalBinary_testCase {}
    internal partial struct TestPrefixMasked_tests {}
    [GoLocalName("subtest")] internal partial struct TestPrefixMasking_subtest {}
    internal partial struct TestPrefixMasking_tests {}
    internal partial struct TestPrefixOverlaps_tests {}
    internal partial struct TestPrefixString_tests {}
    internal partial struct TestPrefix_tests {}
    [GoValueClone("ip4")] internal partial struct ip4i {}
    internal partial struct parseBenchInputsᴛ1 {}
    // </TypeAccessibility>

    // Go initializes an imported package before the importing package, for every import
    // form - not only the blank one. .NET would never load an assembly nothing has touched
    // yet, so each import that initializes anything is forced below: once per assembly, and
    // ahead of this package's own `init` functions, which this file being the first compile
    // item of the project guarantees.

    // <ImportInitializers>
    [GoInit] internal static void initᴛᴛimportꓸbytes() => builtin.initPackage(typeof(bytes_package));
    [GoInit] internal static void initᴛᴛimportꓸencodingꓸjson() => builtin.initPackage(typeof(go.encoding.json_package));
    [GoInit] internal static void initᴛᴛimportꓸflag() => builtin.initPackage(typeof(flag_package));
    [GoInit] internal static void initᴛᴛimportꓸfmt() => builtin.initPackage(typeof(fmt_package));
    [GoInit] internal static void initᴛᴛimportꓸinternalꓸtestenv() => builtin.initPackage(typeof(@internal.testenv_package));
    [GoInit] internal static void initᴛᴛimportꓸnet() => builtin.initPackage(typeof(net_package));
    [GoInit] internal static void initᴛᴛimportꓸreflect() => builtin.initPackage(typeof(reflect_package));
    [GoInit] internal static void initᴛᴛimportꓸslices() => builtin.initPackage(typeof(slices_package));
    [GoInit] internal static void initᴛᴛimportꓸstrconv() => builtin.initPackage(typeof(strconv_package));
    [GoInit] internal static void initᴛᴛimportꓸstrings() => builtin.initPackage(typeof(strings_package));
    [GoInit] internal static void initᴛᴛimportꓸtesting() => builtin.initPackage(typeof(testing_package));
    [GoInit] internal static void initᴛᴛimportꓸunique() => builtin.initPackage(typeof(unique_package));
    // </ImportInitializers>
}
