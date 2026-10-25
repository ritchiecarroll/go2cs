// go2cs metadata anchor for a production-reference test project: the test assembly
// REFERENCES the colocated production project instead of
// recompiling its sources, so the production assembly is the single identity for the
// production types and no production class partial may be declared here. The first —
// and only — class is the test metadata class the go2cs-gen generators anchor
// generated adapters and partials to.
global using static global::go.bytes_package;
global using static global::go.bytes_internal_test_package;

// <ImportedTypeAliases>
global using osꓸDirEntry = go.io.fs_package.DirEntry;
global using osꓸFileInfo = go.io.fs_package.FileInfo;
global using osꓸFileMode = go.io.fs_package.FileMode;
global using osꓸPathError = go.io.fs_package.PathError;
global using osꓸSignal = go.os_package.ΔSignal;
// </ImportedTypeAliases>

using go;
using static global::go.bytes_test_package;

// <ExportedTypeAliases>
[assembly: GoDynamicTypeLift("7374727563747b61205b5d627974653b2062205b5d627974653b206920696e747d", "compareTestsᴛ1")]
[assembly: GoDynamicTypeLift("7374727563747b62205b5d627974653b20722072756e653b20657870656374656420626f6f6c7d", "ContainsRuneTestsᴛ1")]
[assembly: GoDynamicTypeLift("7374727563747b62205b5d627974653b20737562736c696365205b5d627974653b2077616e7420626f6f6c7d", "containsTestsᴛ1")]
[assembly: GoDynamicTypeLift("7374727563747b62205b5d627974653b2073756273747220737472696e673b20657870656374656420626f6f6c7d", "ContainsAnyTestsᴛ1")]
[assembly: GoDynamicTypeLift("7374727563747b62756666657220737472696e673b2064656c696d20627974653b206578706563746564205b5d737472696e673b20657272206572726f727d", "readBytesTestsᴛ1")]
[assembly: GoDynamicTypeLift("7374727563747b62756666657220737472696e673b20736b697020696e743b206e20696e743b20657870656374656420737472696e673b20657272206572726f727d", "peekTestsᴛ1")]
[assembly: GoDynamicTypeLift("7374727563747b696e20737472696e673b207265706c20737472696e673b206f757420737472696e677d", "toValidUTF8Testsᴛ1")]
[assembly: GoDynamicTypeLift("7374727563747b6e616d6520737472696e673b2064617461205b5d627974657d", "bytesdataᴛ1")]
[assembly: GoDynamicTypeLift("7374727563747b6e616d6520737472696e673b20662066756e63282a62797465732e526561646572297d", "UnreadRuneErrorTestsᴛ1")]
[assembly: GoDynamicTypeLift("7374727563747b7320737472696e673b2073657020737472696e673b20616674657220737472696e673b20666f756e6420626f6f6c7d", "cutPrefixTestsᴛ1")]
[assembly: GoDynamicTypeLift("7374727563747b7320737472696e673b2073657020737472696e673b206265666f726520737472696e673b20616674657220737472696e673b20666f756e6420626f6f6c7d", "cutTestsᴛ1")]
[assembly: GoDynamicTypeLift("7374727563747b7320737472696e673b2073657020737472696e673b206265666f726520737472696e673b20666f756e6420626f6f6c7d", "cutSuffixTestsᴛ1")]
[assembly: GoDynamicTypeLift("7374727563747b7320737472696e673b207420737472696e673b206f757420626f6f6c7d", "EqualFoldTestsᴛ1")]
// </ExportedTypeAliases>

// <InterfaceImplementations>
[assembly: GoImplement<TestReaderCopyNothing_justReader, io_package.Reader>(Promoted = true)]
[assembly: GoImplement<TestReaderCopyNothing_justReader, io_package.Reader>]
[assembly: GoImplement<TestReaderCopyNothing_justWriter, io_package.Writer>(Promoted = true)]
[assembly: GoImplement<TestReaderCopyNothing_justWriter, io_package.Writer>]
[assembly: GoImplement<bytes_package.Buffer, io_package.Reader>(Pointer = true)]
[assembly: GoImplement<bytes_package.Buffer, io_package.Writer>(Pointer = true)]
[assembly: GoImplement<bytes_package.Reader, io_package.Reader>(Pointer = true)]
[assembly: GoImplement<negativeReader, io_package.Reader>(Pointer = true)]
[assembly: GoImplement<panicReader, io_package.Reader>]
[assembly: GoImplement<testing_package.T, testing_package.TB>(Pointer = true)]
// </InterfaceImplementations>

// <ImplicitConversions>
// </ImplicitConversions>

// This package's string literals are hoisted to `static readonly` fields (the standard
// library's form, or -hoist-literals), so no converted file passes a bare literal for
// go2cs-gen's literal interceptor to box. The stamp below tells the generator to skip
// the assembly.
[assembly: go.GoHoistedLiterals]

// Go source positions are recorded here, one `GoPositionMap` attribute per converted
// source file in this compilation, so that `runtime.Caller` and the tracebacks built on it
// can name the GO file and line a frame was converted from rather than the emitted C# one.
// Each record carries the Go file's identity and an encoded C#-line to Go-line table
// TOGETHER: a frame either has a record and reports a position that exists in the Go tree,
// or has none - golib, the BCL and hand-written conversions - and reports its own C# position.

// <GoSourcePositionMaps>
[assembly: go.GoPositionMap("bytes/buffer_test.go", "buffer_test.cs", "ACAwgKSCgoKUqLKCgoKmgqaCpoK+0oKCgoKUgpSClKzSgoKCgpSClIKU1oKCAAUQsoKClIKU1oKCqsKUgoKClIKUgqYADgaClIKUgpSClIKAgqSUgpSCgIKklIKUgpSClIKCgIKkgoIACQqCgoKClIKClOaCgoKClIKClPaCgoKClNaCgoKClOaCgoKCgoKUpoKCgpSmgoKCgriCgoKCgriCgoL4goKCgoKC/IKClOrGkoKClIKUlpKCgpSi5KKCgqTGgoLWyKLkgoKCgoKCuIKCgoKCgoKUgriCgoKCgqaCAAgIgpSCgoKCgoKClIKUlKaCppSCgoKCyqaAgqSAgqSAgsiCgoKAgqSCgsqmgoKCuIKCgoKCyoKCgpSCgoKUgpSCggANEAAJHoKCgoKCgoKClIKmgsqCgoKCgoKCgpSCpoIACwoACiKCgoKCgoKUgpSCyqKUgoKCgoKCgsqCgoKCgoKUlIKCooKmgqaClILcooKAgsiCgqKmkoKCgoKUggAJCIKmgIKkgIKkgILIpoCCpICCyICCpICCpIKClIK6koKCgoKCgoKCpqaCuKKCgoKCgoLKooKCgoKCgoLMsoKCgoKCgsyygoKCgoKUgoLKgoKCkoKCgoLuooKCgoKCgoKC", "105-107:1;310-313:1;319-332:1;361-368:1;595-598:1;615-619:1;758-766:1")]
[assembly: go.GoPositionMap("bytes/bytes_test.go", "bytes_test.cs", "ABougoKClKaCgoKClAAHEAAHEIKCgoIAFCaUgoKCgriCuIKCgpSCgpSCgqaCgoKCgoIABhKigoKUgpSCgoKCgoKU7gBAggEADyAAEygAGSyigoKCgoKmAAUUkoCCpICCtoK4goKCgoL6gNKA0oDSgqaCgoKUgoKCgpSCgsqCAAcQgoKCzJKCgpSClIKCgoKClIKCgriCgoKCgpSCgoK4goKCgoKUgoKC3pKUgoKUgoKCpoK4goKUgoKCpoIACgqCADh6goCCyIKSgIKkgIK2grqSgoKSgoKCgsqUgqKClIKCuIKigpSCgt6SgoKCooKmgqaCgpSCAAcQgoCCpICCpKaCgoKUgoKUgsqUlIKmgtaCgoKCgoKCpriCpoL2gpSUlJSUuIKCgoKCgoKmuIKCgoKCgoKmgoK4soKCgoK4goKC7oKSlJSCgoKCuIKCgoKClJSCgoKCpoIACAqCgoKCgoKCgsqUkqamgoKClIKCgoKCgoKmgriCgoKUgoKmgoKCgoKCgqKCgoKCuILKgoKCgoKCgqa4goKCgoKCgoKmguiCgoKCgoKCpriCgoKCgoKCgqaCuIKCgoKClIKCgoKmAAoWABQqooKWkoKmgoKCpoKCgriCpoCCtoKClIKCgqaCgoLcAA8ggoKWkoKmgoKCpoKCgriAgraCgpSCgoIAChYADRyCgoKWkoKmgoKCpoKCpoKUgoCC7IKCgoKCgqaC3IKClpKCpoKCpoKCpoKUgoCCAA0aAAsYAAwaABYyooKCgpSClILKgoKClKiSgoKUgpT2lKaCgoKCuIKCgoK4goKCuIKCgriCgpSUgoKCuIaCgoLogNSApIKygqKCgoLugrKCooKCggALDgAQKoKCgoL6gP6UAAwagoKCgoKCgsqygoCClMTqhAAOCJKOgoKCgpSmgsqUAAkUgoKmAAoWAAkUgoKCgoKClJSCggAKFgAnUAAiQKKClKSkpKSkgriCgoKClIKClJSCuIKCgoKUgoKUlIKClKaClIKCggAKFpKSkgAMHoIABhAALlKCggAEEoKCgpSClIIACxgAFCqCgoKClIKCAAsYABUsorLKgrCSgqaCgoKAgqSClIKCgILsooKUgoKCgpSCgoKUgoKCgoKUgqaCgoKUgqaClJSAgtqCsoKqwoKCAAkUAAsYgoKAgtoACRSCgoCCAAkKAA0iooKAgqSAggAKCgAKHoKCgIIACgoACBqCgoCCAAoKAAgagoKAggAICqKCgIK2gqLUooKAgraCotSigoCCtoKCouQABhSCgoCCAAoKAA0kgoKCAAoMAAoegoKC3IKyltyClIKCpLSCgpS0pqaClIKCgpSm5gAEEIKykpKSgoKCggAHEIKykpKSgoKCggAMEIIABRKykoIACgyiAAQQgoKyooLcgsqCgoKCgpSUppSigoKUgoK4ooK4ooK4gKKAoqCioqaAooCioKSAooCioKSigriigoK4ooKCuKKCgriigoK4ooK4goKCgoKCgpSSgpTcgpKCgpSCpoKmgoLcgoKCkpKSggAJDoKCgpKSkoLugoKCkpKSgu6CgoKSkpKC7oKCkpKCgoLuooKCuIKCgpKCgpSC3IIACBKCgoKmgqaCpoI=", "78-85:1;285-292:1;507-514:1;524-532:1;602-608:1;625-635:1;647-650:1;651-654:2;655-658:3;662-672:1;676-688:1;711-713:1;716-743:2;747-757:1;761-763:2;762-762:2.1;769-785:1;805-813:1;821-831:1;835-847:1;851-861:1;865-877:1;881-895:1;1093-1093:1;1228-1228:1;1236-1236:2;1258-1263:3;1271-1273:4;1288-1295:1;1302-1309:1;1378-1387:1;1402-1416:1;1552-1568:1;1599-1605:2;1643-1645:1;1794-1794:1;1821-1860:1;1822-1856:1.1;1871-1876:1;2016-2020:1;2026-2030:1;2036-2040:1;2117-2119:1;2171-2182:1;2173-2180:1.1;2188-2199:1;2190-2197:1.1;2214-2218:1;2234-2238:1;2349-2354:1;2361-2377:1;2386-2390:1;2400-2404:1;2414-2418:1;2428-2432:1;2441-2446:1;2461-2469:1", "", "479=Repeat/1/6/22,Repeat/2/6/23,Repeat/3/6/30,Repeat/4/6/31,Repeat/5/6/38,Repeat/6/6/39;1789=not/1/3/13,not/2/3/17,not/3/3/25;1864=not/1/6/8,not/2/6/14,not/3/6/15,not/4/6/16,not/5/6/17,not/6/6/18;2391=Invoke/1/2/4,Invoke/2/2/5;2801=Clone/1/1/3,Repeat/1/3/4,Repeat/2/3/5,Repeat/3/3/7")]
[assembly: go.GoPositionMap("bytes/compare_test.go", "compare_test.cs", "ABAaABk6goKClIKCgoKCAAgMgoKClIK4ooKClJSCpoKCgpSCgqaCgpSCgpSCgoKUgoKmgoKCgpSCgoKUyorCgpSCgpSCgoKCgoKUgoKCgpSC+qKCgoKC+qKCgoKCyqKCgoKCyqKCgoKC+qKCgoKCyqKCgoKCyqKCgoKUgoKCgqamgpKCyqKCgoKClIKCgoKCpqaCkoLKooKCgpSCgoKCpqaigoKClIKCgoKm", "234-236:1;260-262:1")]
[assembly: go.GoPositionMap("bytes/reader_test.go", "reader_test.cs", "ABsegoIACyiCgoKClIKClIKUgoKCgpSCgsqCgoCCpICCAAsIgoIABxqCgoKCgpSCyqaCgoKCsoKCoqSmuIKCgoKygoKikrKCoqSmgoKCgpSCgoKCgIKkgpSClIL6goKCgJKkgIKkgJKkgIKkgJIACAiYoqKioraCgoKAlKSCgoL6goKAlKSAlKSAggARDKIAARiSgoKCupKCgoKUguiCgoCCtoKCgIKkgoKUgILIgoCCtoCCtoCCtoCCtoCCtoCCtoCCtoKmgqaAgg==", "108-112:1;125-129:1;130-133:2")]
// </GoSourcePositionMaps>

namespace go;

[GoPackage("bytes_test")]
public static partial class bytes_test_package
{
    // C# nested types declared with no access modifier are always private, and the
    // Go type declarations in this package's converted sources are deliberately
    // bare so they read more like the original Go code. The real accessibility for
    // the types - public for a Go-exported name, internal otherwise - are defined
    // via declarations below.

    // <TypeAccessibility>
    internal partial struct BenchmarkToValidUTF8_tests {}
    internal partial struct BenchmarkTrimSpace_tests {}
    internal partial struct TestIndexRune_tests {}
    internal partial struct TestReaderAt_tests {}
    [GoLocalName("justReader")] internal partial struct TestReaderCopyNothing_justReader {}
    [GoLocalName("justWriter")] internal partial struct TestReaderCopyNothing_justWriter {}
    [GoLocalName("nErr")] internal partial struct TestReaderCopyNothing_nErr {}
    internal partial struct TestReader_tests {}
    [GoLocalName("testCase")] internal partial struct TestRepeatCatchesOverflow_testCase {}
    internal partial struct TestTrimFunc_trimmers {}
    internal partial struct bytesdataᴛ1 {}
    internal partial struct compareTestsᴛ1 {}
    internal partial struct containsTestsᴛ1 {}
    internal partial struct cutPrefixTestsᴛ1 {}
    internal partial struct cutSuffixTestsᴛ1 {}
    internal partial struct cutTestsᴛ1 {}
    internal partial struct negativeReader {}
    internal partial struct panicReader {}
    internal partial struct peekTestsᴛ1 {}
    internal partial struct predicate {}
    internal partial struct readBytesTestsᴛ1 {}
    internal partial struct runIndexTests_type {}
    internal partial struct toValidUTF8Testsᴛ1 {}
    public partial struct BinOpTest {}
    public partial struct ContainsAnyTestsᴛ1 {}
    public partial struct ContainsRuneTestsᴛ1 {}
    public partial struct EqualFoldTestsᴛ1 {}
    public partial struct FieldsTest {}
    public partial struct IndexFuncTest {}
    public partial struct LinesTest {}
    public partial struct RepeatTest {}
    public partial struct ReplaceTest {}
    public partial struct RunesTest {}
    public partial struct SplitTest {}
    public partial struct StringTest {}
    public partial struct TitleTest {}
    public partial struct TrimFuncTest {}
    public partial struct TrimNilTest {}
    public partial struct TrimTest {}
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
        builtin.initPackage(typeof(global::go.bytes_package));
    }
}
