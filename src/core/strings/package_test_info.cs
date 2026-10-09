// go2cs metadata anchor for a production-reference test project: the test assembly
// REFERENCES the colocated production project instead of
// recompiling its sources, so the production assembly is the single identity for the
// production types and no production class partial may be declared here. The first —
// and only — class is the test metadata class the go2cs-gen generators anchor
// generated adapters and partials to.
global using static global::go.strings_package;
global using static global::go.strings_internal_test_package;

// <ImportedTypeAliases>
global using abiꓸArrayType = go.@internal.abi_package.ΔArrayType;
global using abiꓸChanDir = go.@internal.abi_package.ΔChanDir;
global using abiꓸFuncType = go.@internal.abi_package.ΔFuncType;
global using abiꓸInterfaceType = go.@internal.abi_package.ΔInterfaceType;
global using abiꓸKind = go.@internal.abi_package.ΔKind;
global using abiꓸName = go.@internal.abi_package.ΔName;
global using abiꓸStructType = go.@internal.abi_package.ΔStructType;
// </ImportedTypeAliases>

using go;
using static global::go.strings_test_package;

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
[assembly: GoImplement<bytes_package.Buffer, io_package.Writer>(Pointer = true)]
[assembly: GoImplement<errWriter, io_package.Writer>]
[assembly: GoImplement<strings_package.Reader, io_package.Reader>(Pointer = true)]
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
[assembly: go.GoPositionMap("strings/builder_test.go", "builder_test.cs", "ABMeooKCgoKUgIKkgIIACgiCgoKCgpSCgIKkgoKClAAJBqKCgoKCgoKCgoKWgIKkgIKkgIIACAiigoKCgoKCqIKCgILIooKCgpSCkoKCgpSCgqaCgpSAkriSgoKAgraiAAwEgoIAHD6SgoKClIKUhIKClIKUyoKCgIKkgIKk5qKCuIKCgoKUggAJCIIABBKCgoLugoKC7oKCgu6CgpKC7oKCku6CgpLugoKS7oKCku6CgpK4soLCkJKikoCC2qaCgoK4AAgIgoKClIKClIKCuIKCgoKClIKUyoKCgoKCgpSClMqCgoKCgpSClMqCgpSCgoKCgpSC", "98-108:1;120-124:2;138-138:1;144-144:2;150-150:3;156-156:4;161-180:5;201-206:1;221-226:1;231-236:2;241-246:3;251-257:4;262-267:5;272-277:6;282-287:7;292-297:8;302-307:9;312-315:10;313-313:10.1;337-340:1;341-344:2;345-348:3;352-363:1;368-379:1;383-394:1;402-407:1")]
[assembly: go.GoPositionMap("strings/clone_test.go", "clone_test.cs", "AA4egu6CgoKWgpaCyqKCgoI=", "", "", "16=Clone/1/1/2,Repeat/1/2/3,Repeat/2/2/5")]
[assembly: go.GoPositionMap("strings/compare_test.go", "compare_test.cs", "ABYiAA8ogoKCggAICoKCgpSCuKaIgoKUhIKWgoKCgpSCgqaCgpaSgoKUgoKClIKCpoKCgoKUgoKClJQ=", "59-61:1")]
[assembly: go.GoPositionMap("strings/reader_test.go", "reader_test.cs", "ABsggoIACiiCgoKClIKClIKUgoKCgpSCgsqCgoCCpICCAAsIgoIABxqCgoKCgpSCysaCgoKCsoKCoqSm2IKCgoKygoKikrKCoqTWgoKCgoKCgoCCpIKUgpSC/JKCgoKUguiCgoCCpoKCgIKkgoKUgILIgoCCpoCCpoCCpoCCpoCCpoCCpoCCpoKWgpaAgg==", "109-113:1;126-130:1;131-134:2")]
[assembly: go.GoPositionMap("strings/replace_test.go", "replace_test.cs", "AA8cAAYQAAsSkoKCgoKCpgArBrKehpaCgpSWAAgagoKCgpSUlgAJHgAIHAALGgAGErq83gAGErrMzKgACR6CgpSCugAEEIKCgoKCgoIAFkCEAAYUggAFEoK+goCCpIKCgoKUgoKClIIACgwACBqSgoKC/oKqooKCggAIENIAM1qCgoKClpSCgoKmhILKooKCgriigoKC6KKCgriigoKCgriipqLmgqaigoK4ooKCuKKCguiigoK4ooKCuKKCgoKCuKKCgoKCurKCgrqygoKUpKSUgvgACAqCiJKyooLMgoKUgpSWkrKigg==", "51-53:1;530-538:1;550-552:1;554-562:2;556-560:2.1;564-572:3;574-582:4;576-580:4.1", "", "104=Repeat/1/2/2,Repeat/2/2/2,NewReplacer/1/1/9;123=NewReplacer/1/1/10;133=NewReplacer/1/3/5,NewReplacer/2/3/7,NewReplacer/3/3/9;302=NewReplacer/1/4/6,NewReplacer/2/4/7,NewReplacer/3/4/8,NewReplacer/4/4/9")]
[assembly: go.GoPositionMap("strings/search_test.go", "search_test.cs", "ABEaggAQKoKCgoIACgqCABY4ooSCgoKUgqiC")]
[assembly: go.GoPositionMap("strings/strings_test.go", "strings_test.cs", "ABkugoKCgpQABxAABxCCgoKCABggAF3IAQAPIAATKAATLKKCgoL6gNKA0oDSgqaCgoKUgoLKggALEIKCgsqCgoKCptaCgoKSgoKUgoKCgoKCgpSCgoIADA6CADl4goCCyoKCgIKkgIK2gtyigIKkgriUooCCpIK4ooCCpIK4ooCCpIK4ooCCpIK4ooCCpIIAChYAFi6CgoKCgpSCgoKmgpSCgpSCgoLcAA8ggoKCgoKUgoKCpoKClIKCggAKFgARJIKCgoKClIKCyu6CgoKCgqaCgoKClIKCAAkYooKCguoADhoADRwAEiaCgoKUqJKCgpSClAAMBpSUgoKCgqiCgoKCqIKCgqiCgoKogoKUlIKCgqiGgoKCqIKClJSCgoKogpSkpJSCgoKClIKCqIKClJSCgoLogNSAAAgEABAqgoKCgsqCspKCgoLugrKSgoKCAAkOooaC+IKCgoKClIKClIKClIKC6IAACAQAH0iCgoKClKSkpKSkpIKCyqKEgoKCgpSkpKSkpKSCggAKDKIABBCCgrKSggAKFpKSks6CAA4QACxagoIABBKCgoIADQwAEzSCgoKClIKCyoKClIKCgoKUgoKCpgAKBpSCgpSCgpSUgpaAgqSAgqSClIIAESSSgoKClAAIBgAULIKCgoKCyrKCgIKUxNqEAA4Iko6CgoKClJaCuoQACBSCgpbKgoKUgoKmAAgGAAkcooKCgoKUlIKC3IKCgoKAgqSCgoKClIKClJSCgoKUgoKClIKCpoLKgoKCgoCCpIKCgoKUgoKUlIKCgpSCgoKUgoKUgoKmggAJCpiioqKitoKCgoCUpIKCggALCgAVNoKCgIKkgoKCAAkMAAscgoKAggAYCgA6lAGCgoLcAA0igoKCAAsMAAoegoKC3IKylgAKDAAPJqKCgIKkgIIAEgqCgoKCgILcgoSCgqiCgqiCggAJCgAMIIKCgIIACwoACh6CgoCCAAsKAAgagoKAggALCgAIGoKCgILagsqCgoKClJSmlKKCuKKCuKKCuIDSgNKA0oKmgKKAooCkgKKAooCkkpSigriiguiigoKCuIKClIKCgqaCktyChIKktIKClLS2toKEgoKUpqbKgrKSkpKCgoKCAAcQgrKSkpKCgoKCAAcQooK4ooK4ooK4ooK4ooK4goKCgoKC7oKCgoKCgoKUgoKU3KKCgriCgoKSkoKCAAkOgoKCkpKCgu6CgoKSkoKC7oKCgpKSgoLugoKSkoKCggAJDqKCgriCgoKCgoIACAyCgpKSgoKCAAkMgtyykoIAChCigoI=", "374-381:1;598-598:1;697-697:1;705-705:2;727-732:3;740-742:4;750-755:5;763-771:6;784-789:7;833-840:1;846-853:1;858-860:1;991-995:1;1016-1018:1;1232-1241:1;1256-1270:1;1645-1647:1;1685-1693:1;1698-1702:2;1704-1708:3;1710-1714:4;1885-1890:1;1892-1894:2;1939-1950:1;1941-1948:1.1;1956-1967:1;1958-1965:1.1;2005-2009:1;2023-2028:1;2045-2049:1;2059-2063:1;2073-2077:1;2087-2091:1;2100-2105:1;2120-2125:1;2132-2138:1;2150-2154:1", "", "347=Repeat/1/6/34,Repeat/2/6/35,Repeat/3/6/42,Repeat/4/6/43,Repeat/5/6/50,Repeat/6/6/51;1160=not/1/3/19,not/2/3/23,not/3/3/31;1229=not/1/7/12,not/2/7/18,not/3/7/19,not/4/7/20,not/5/7/21,not/6/7/22,not/7/7/23;2218=Invoke/1/2/1,Invoke/2/2/2")]
// </GoSourcePositionMaps>

namespace go;

[GoPackage("strings_test")]
public static partial class strings_test_package
{
    // C# nested types declared with no access modifier are always private, and the
    // Go type declarations in this package's converted sources are deliberately
    // bare so they read more like the original Go code. The real accessibility for
    // the types - public for a Go-exported name, internal otherwise - are defined
    // via declarations below.

    // <TypeAccessibility>
    internal partial struct BenchmarkToValidUTF8_tests {}
    internal partial struct BenchmarkTrimSpace_tests {}
    internal partial struct TestBuilderCopyPanic_tests {}
    internal partial struct TestBuilderWrite2_type {}
    [GoValueClone("bad")] internal partial struct TestFinderCreation_testCases {}
    internal partial struct TestFinderNext_testCases {}
    internal partial struct TestGenericTrieBuilding_testCases {}
    internal partial struct TestIndexRune_tests {}
    internal partial struct TestReaderAt_tests {}
    internal partial struct TestReader_tests {}
    [GoLocalName("testCase")] internal partial struct TestRepeatCatchesOverflow_testCase {}
    [GoLocalName("testCase")] internal partial struct TestReplacer_testCase {}
    internal partial struct TestTrimFunc_trimmers {}
    internal partial struct algorithmTestCasesᴛ1 {}
    internal partial struct compareTestsᴛ1 {}
    internal partial struct cutPrefixTestsᴛ1 {}
    internal partial struct cutSuffixTestsᴛ1 {}
    internal partial struct cutTestsᴛ1 {}
    internal partial struct errWriter {}
    internal partial struct indexFuncTestsᴛ1 {}
    internal partial struct mapdataᴛ1 {}
    internal partial struct predicate {}
    internal partial struct toValidUTF8Testsᴛ1 {}
    internal partial struct trimFuncTestsᴛ1 {}
    internal partial struct trimTestsᴛ1 {}
    public partial struct ContainsRuneTestsᴛ1 {}
    public partial struct ContainsTestsᴛ1 {}
    public partial struct CountTestsᴛ1 {}
    public partial struct EqualFoldTestsᴛ1 {}
    public partial struct FieldsTest {}
    public partial struct IndexTest {}
    public partial struct LinesTest {}
    public partial struct RepeatTestsᴛ1 {}
    public partial struct ReplaceTestsᴛ1 {}
    public partial struct RunesTestsᴛ1 {}
    public partial struct SplitTest {}
    public partial struct StringTest {}
    public partial struct TitleTestsᴛ1 {}
    public partial struct UnreadRuneErrorTestsᴛ1 {}
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
    // Go runs every `init` in the package under test - the production files' included -
    // before the first test. The production package is a REFERENCED assembly here, whose
    // module constructor .NET would not run until something in it is touched, so that
    // initialization is forced before anything else in this test module runs.
    [GoInit] internal static void initᴛᴛproduction() {
        builtin.initPackage(typeof(global::go.strings_package));
    }
}
