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

// Go source positions are recorded here, one `GoPositionMap` attribute per converted
// source file in this compilation, so that `runtime.Caller` and the tracebacks built on it
// can name the GO file and line a frame was converted from rather than the emitted C# one.
// Each record carries the Go file's identity and an encoded C#-line to Go-line table
// TOGETHER: a frame either has a record and reports a position that exists in the Go tree,
// or has none - golib, the BCL and hand-written conversions - and reports its own C# position.

// <GoSourcePositionMaps>
[assembly: go.GoPositionMap("bytes/buffer_test.go", "buffer_test.cs", "AB4wgKSCgoKUqLKCgoKWgpaCloK+0oKCgoKUgpSClKzSgoKCgpSClIKU1oKCAAUQsoKClIKU1oKCqsKEgoKClIKUgpYADgaChIKEgoSChIKAgqSEgoSCgIKkhIKEgoSChIKCgIKkgoIACQqCgoKClIKClOaCgoKClIKClPaCgoKClNaCgoKClOaCgoKCgoKUloKCgpSmgoKCgriCgoKCgriCgoL4goKCgoKC/IKClOrGkoKClIKUhpKCgpQACQaigoKktoKS1rgACQaCgoKCgoK4goKCgoKCgpSCqIKCgoKCpoIACAiClIKCgoKCgoKUgpSUloKWlIKCgoK6loCCpICCpICCuIKCgoCCpIKCyqaCgoK4goKCgoLKgoKClIKCgpSClIKCABgugoKCgoKCgoKUgqaCyoKCgoKCgoKClIKmgsqihIKCgoKCgoLKgoKCgoKChJSCgqKCpoKmgpSC3KKCgIK4goLYkoKCgoKUggAJCIKWgIKkgIKkgIK4loCCpICCuICCpICCpIKClIK6koKCgoKCgoKCpqaCuKKCgoKCgoLKooKCgoKCgoLMsoKCgoKCgsyygoKCgoKUgoLKgoKCkoKCgoLuooKCgoKCgoKC", "105-107:1;310-313:1;319-332:1;361-368:1;561-564:1;581-585:1;724-732:1")]
[assembly: go.GoPositionMap("bytes/bytes_test.go", "bytes_test.cs", "ABksgoKClKaCgoKClAAPIIKCgoIAFCaUgoKCgriCuIKCgpSCgpSCgpaCgoKCgoIABhKigoKUgoSCgoKCgoKUAIUBhAKigoKCgoKmAAUUkoCCpICCtoK4goKCgoL6gNKA0oDSgqaCgoKUgoKCgpSCgsqCAAsQgoKCzJKCgpSClIKCgoKClIKCgriCgoKCgpSCgoK4goKCgoKUgoKC3pKUgoKUgoKCpoK4goKUgoKCpoIACgqCADp6goCCuIKSgIKkgIK2grqSgoKSgoKCgrqEgqKClIKCuIKigpSCgt6ygoKCooKmgqaCgpSCAAcQgoCCpICCpKaCgoKUgoKUggAIEoKmgtaCgoKCgoKCpriCpoL2gpSUlJSUuIKCgoKCgoKmuIKCgoKCgoKmgoK4ooKCgoK4goKC7oKSlISCgoKCqIKCgoKClISCgoKCpoIACAqCgoKCgoKCgrqEkpamgoKClIKCgoKCgoKmgriCgoKUgoKWgoKCgoKCgqKCgoKCuILKgoKCgoKCgqa4goKCgoKCgoKmguiCgoKCgoKCpriCgoKCgoKCgqaCuIKCgoKClIKCgoKmggAgQqKChpKCloKCgpaCgoKogpaAgqaCgpSCgoKmgoKCABUsgoKGkoKWgoKCloKCgqiAgqaCgpSCgoIAGDKCgoKGkoKWgoKCloKCloKUgoCC7IKCgoKCgqaC3IKChpKCloKCloKCloKUgoCCAEF+ooKCgpSClILKgoKClKiSgoKUgpT2lJaCgoKCqIKCgoKogoKCqIKCgqiCgpSUgoKCqIaCgoLogNSApIKygqKCgoLugrKCooKCggAdOIKCgoL6gAAWLIKCgoKCgoLKsoKAgpTE2oQADgiSjoKCgoKUloK6hAAIFIKClgAUKqKCgoKCgpSUgoIAVaYBooKUpKSkpKSCuIKCgoKUgoKUlIKogoKCgpSCgpSUgoKUpoKUgoKCAB06ggA1YoKCAAQSgoKClIKUggAgQoKCgoKUgoIAIUSigoKCgoCCpIKUgoKAggAWLIKCgIIADx6CgoCCABgsooKAgqSAggAWKIKCgIIAFCSCgoCCABQkgoKAggAICqKCgIK2ggAIBqKCgIK2ggAIBqKCgIK2goIAERqCgoCCABkugoKCABYqgoKC3IKylv6EgqS0goKUtLbIhIKClKYADBaCspKSkoKCgoIABxCCspKSkoKCgoIADBCCAAUSspKCAAoMogAEEIKCsqKC3ILKgoKCgpSUyqKCgpSCgriigriigriAooCioKKipoCigKKgpICigKKgpKKCuKKCgriigoK4ooKCuKKCgriigriCgoKCgoKClJKClNyCkoKChIKWgpaCgtyCgoKSkpKCAAkOgoKCkpKSgu6CgoKSkpKC7oKCgpKSkoLugoKSkoKCgu6igoK4goKCkoKClILcggAIEoKCgpaCloKWgg==", "77-84:1;284-291:1;506-513:1;523-531:1;601-607:1;624-634:1;646-649:1;650-653:2;654-657:3;661-671:1;675-687:1;710-712:1;715-742:2;746-756:1;760-762:2;761-761:2.1;768-784:1;804-812:1;820-830:1;834-846:1;850-860:1;864-876:1;880-896:1;1094-1094:1;1229-1229:1;1237-1237:2;1259-1264:3;1272-1274:4;1289-1296:1;1303-1310:1;1379-1388:1;1403-1417:1;1553-1569:1;1600-1606:2;1644-1646:1;1943-1947:1;1953-1957:1;1963-1967:1;2044-2046:1;2096-2107:1;2098-2105:1.1;2113-2124:1;2115-2122:1.1;2139-2143:1;2159-2163:1;2273-2278:1;2285-2301:1;2310-2314:1;2324-2328:1;2338-2342:1;2352-2356:1;2365-2370:1;2385-2393:1")]
[assembly: go.GoPositionMap("bytes/compare_test.go", "compare_test.cs", "ACtUgoKClIKCgoKCAAgMgoKClIK4goKClISCloKCgpSCgqaCgpSCgpSCgoKUgoKmgoKCgpSCgoKUyorCgpSCgpSCgoKCgoKUgoKCgpSC+qKCgoKC+qKCgoKCyqKCgoKCyqKCgoKC+qKCgoKCyqKCgoKCyqKCgoKUgoKCgqamgpKCyqKCgoKClIKCgoKCpqaCkoLKooKCgpSCgoKCpqaigoKClIKCgoKm", "234-236:1;260-262:1")]
[assembly: go.GoPositionMap("bytes/reader_test.go", "reader_test.cs", "ABkegoIACiiCgoKClIKClIKUgoKCgpSCgsqCgoCCpICCAAsIgoIABxqCgoKCgpSCysaCgoKCsoKC1qbYgoKCgrKCgsSygtamgoKCgpSCgoKCgIKkgpSClIL6goKCgJKkgIKkgJKkgIKkgJIACxCioqKitoKCgoCUpIKCgvqCgoCUpICUpICCABEMogAAGJKCgoK6koKCgpSC6IKCgIKmgoKAgqSCgpSAgsiCgIKmgIKmgIKmgIKmgIKmgIKmgIKmgpaCloCC", "108-112:1;125-129:1;130-133:2")]
// </GoSourcePositionMaps>

namespace go;

[GoPackage("bytes_test")]
public static partial class bytes_test_package
{
    // C# nested types declared with no access modifier are always private, and the
    // `[GoType]` declarations in this package's converted sources are deliberately
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
