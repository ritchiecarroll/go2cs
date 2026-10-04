// go2cs metadata anchor for a production-reference test project: the test assembly
// REFERENCES the colocated production project instead of
// recompiling its sources, so the production assembly is the single identity for the
// production types and no production class partial may be declared here. The first —
// and only — class is the test metadata class the go2cs-gen generators anchor
// generated adapters and partials to.
global using static global::go.fmt_package;
global using static global::go.fmt_internal_test_package;

// <ImportedTypeAliases>
global using osꓸDirEntry = go.io.fs_package.DirEntry;
global using osꓸFileInfo = go.io.fs_package.FileInfo;
global using osꓸFileMode = go.io.fs_package.FileMode;
global using osꓸPathError = go.io.fs_package.PathError;
global using osꓸSignal = go.os_package.ΔSignal;
global using reflectꓸChanDir = go.reflect_package.ΔChanDir;
global using reflectꓸKind = go.reflect_package.ΔKind;
global using reflectꓸMethod = go.reflect_package.ΔMethod;
global using reflectꓸType = go.reflect_package.ΔType;
global using reflectꓸValue = go.reflect_package.ΔValue;
global using timeꓸLocation = go.time_package.ΔLocation;
global using timeꓸMonth = go.time_package.ΔMonth;
global using timeꓸWeekday = go.time_package.ΔWeekday;
using bytes = go.bytes_package;
// </ImportedTypeAliases>

using go;
using static global::go.fmt_test_package;

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
[assembly: GoImplement<BenchmarkScanRecursiveIntReaderWrapper_buf, io_package.Reader>(Promoted = true)]
[assembly: GoImplement<BenchmarkScanRecursiveIntReaderWrapper_buf, io_package.Reader>]
[assembly: GoImplement<TestLineByLineFscanf_r, io_package.Reader>(Promoted = true)]
[assembly: GoImplement<TestLineByLineFscanf_r, io_package.Reader>]
[assembly: GoImplement<bufio_package.Reader, io_package.Reader>(Pointer = true)]
[assembly: GoImplement<bytes_package.Buffer, io_package.Reader>(Pointer = true)]
[assembly: GoImplement<bytes_package.Buffer, io_package.Writer>(Pointer = true)]
[assembly: GoImplement<eofCounter, io_package.Reader>(Pointer = true)]
[assembly: GoImplement<errString, error>]
[assembly: GoImplement<fmt_package.ScanState, io_package.Reader>]
[assembly: GoImplement<fmt_package.State, io_package.Writer>]
[assembly: GoImplement<readers_type, io_package.Reader>(Promoted = true)]
[assembly: GoImplement<readers_type, io_package.Reader>]
[assembly: GoImplement<strings_package.Reader, io_package.Reader>(Pointer = true)]
[assembly: GoImplement<testState, fmt_package.State>]
// </InterfaceImplementations>

// <ImplicitConversions>
[assembly: GoImplicitConv<bytes.Buffer, ж<bytes.Buffer>>(Indirect = true)]
// </ImplicitConversions>

// Go source positions are recorded here, one `GoPositionMap` attribute per converted
// source file in this compilation, so that `runtime.Caller` and the tracebacks built on it
// can name the GO file and line a frame was converted from rather than the emitted C# one.
// Each record carries the Go file's identity and an encoded C#-line to Go-line table
// TOGETHER: a frame either has a record and reports a position that exists in the Go tree,
// or has none - golib, the BCL and hand-written conversions - and reports its own C# position.

// <GoSourcePositionMaps>
[assembly: go.GoPositionMap("fmt/errors_test.go", "errors_test.cs", "ABscpoSCAD6EAYCCpICCpICSAAkKgoCCpMqA")]
[assembly: go.GoPositionMap("fmt/fmt_test.go", "fmt_test.cs", "ADpSgoKCgoK6goKokpKSAAsYgAAOHILKggATIoLMgAAKFKLIgqa4gqa4goCCAKgBCACvBsQPogAKBqKCgoKCkpSCtIK0grSCtIK0grSCgoKCppSCgKaUAAYQooKCgoKCgoKClIKUgpSClIKClIKCgoKCgoKCABkYACJUgoKCgsqCgoLKgoKC+oKCgsqCgoLKgoKSgsqCgoLKgoKCyoKCgsqCgoLKgoKCyoKCgsqCgoLKgoKCyoKCgsqCgpKCyoKCkoLKgoKCgsqCgpKC+oKCgoKCyqKCgoK4ooKCgoK4ooKCgoIAFg6aoqKioqKioqKAooCigKKAgKKAgKKAooCmggAJDIKUtLSCgoCCAAgOgoKCgqaAgqSAgqSC9gAPJoKCgoKCABMKgoyCgoIABBCCgoKmgoLKgpKCgpSCgoKUgoK8ooKCgqaCgvqCgoKClJKCgpSClILWgoKCgoKUgoKCvKKCgoK8ooKCguqSgoKCgpSCgpSCgrigAAwEABlCgoKCggAJFpIABxKSAAcSkgAPBgAMKoKCgoIADxqigIKC2uaCkpKCgpSCgoKCuKaCgsqAAA4EooaCgoKCAA0IADaEAYKCgoIADAqCAAccgoKCAAkagoKCgoKUgriCgoKCgpSC6IKCgoKClII=", "1297-1301:1;1305-1309:1;1313-1317:1;1321-1325:1;1330-1334:1;1338-1342:1;1346-1350:1;1354-1358:1;1362-1366:1;1370-1374:1;1378-1382:1;1386-1390:1;1394-1398:1;1402-1406:1;1411-1415:1;1420-1424:1;1429-1433:1;1438-1442:1;1446-1452:1")]
[assembly: go.GoPositionMap("fmt/scan_test.go", "scan_test.cs", "AH2sAYKCgpSCgpSCAAwYooCCpoKClIIADQoAiQHEAQCqAZoDADYuACVKAAYkgoKCgoKCgpSClIKCpoKAgqSCgsqCspLKgrKSyoKCgoKClJSCgpSCgqaCgIKkgoL6lIKCgoKClILKooKCgoKCgpSClIK4goK4ooKCgoKCgpSClIKClIK4goK4goKCgoKCgqSUlIKUgoKmgoKClIKCyoKykvqCgoKCgpSClIKUgoKUgpSC+pKSgoKUgpSClIKClIKmgoKUgviCgoKCgqT4goKCgqTogoKSgoKkAAkUsoKClAAKCqKCgoKClIKUgoKUgoKUgpSCvKKSgoKUgpSCgpSCpoKClIIACAgAEy6CgoCCpICC3qKCgoKCgpSClIKClIIABRCigoKCgpSCgqaC5oKEkoKClIKUgpaSgoKUgpSClpKCgpSCpPyigpKCgpSCgpSCAAsYgoKCgqaCooKClIKUgpSCAAsYsoKClIKCgoKUlIKs8oKCgpSCgoKUlIKUgoKClKaCgoKClKaCgpKCAAcQgoKCgoKClIKCgpSUgriigoKCgoKCgriigoKCgoKCgviigoKCgoKCggALDKKSgoKUgoKUgoK4goSCgpSCgoKEgoKUgoSCgpSCgoKogoIACQiCkgAFFIKCgpSCAAsKgpIABRaCgoKUgpSCAAwKgpIALmCCgoKClJSClIKUggAGEpKCgoKUgqaCgoKClIKUgg==", "87-87:1;534-536:1;542-544:1;677-679:1;1068-1071:1;1142-1151:1")]
[assembly: go.GoPositionMap("fmt/state_test.go", "state_test.cs", "ABMolIKmgqaCpoLKgoKCgpSCgpSCgpQACAaCAAsggoKC")]
[assembly: go.GoPositionMap("fmt/stringer_test.go", "stringer_test.cs", "ACs6gKKAooCigKKAooCigKKAooCigKKAooCigKKAooCigKSCggAJCIKCgoKCgoKC")]
// </GoSourcePositionMaps>

namespace go;

[GoPackage("fmt_test")]
public static partial class fmt_test_package
{
    // C# nested types declared with no access modifier are always private, and the
    // `[GoType]` declarations in this package's converted sources are deliberately
    // bare so they read more like the original Go code. The real accessibility for
    // the types - public for a Go-exported name, internal otherwise - are defined
    // via declarations below.

    // <TypeAccessibility>
    internal partial interface splitErr_type {}
    internal partial struct BenchmarkScanRecursiveIntReaderWrapper_buf {}
    internal partial struct TestErrorf_type {}
    internal partial struct TestFormatString_type {}
    internal partial struct TestLineByLineFscanf_r {}
    [GoLocalName("A")] internal partial struct TestNilDoesNotBecomeTyped_A {}
    [GoLocalName("B")] internal partial struct TestNilDoesNotBecomeTyped_B {}
    internal partial struct TestParsenum_testCases {}
    internal partial struct TestScanNewlinesAreSpaces_type {}
    internal partial struct TestScanfNewlineMatchFormat_type {}
    internal partial struct TestScanlnNewlinesTerminate_type {}
    [GoLocalName("T")] internal partial struct TestStructPrinter_T {}
    internal partial struct TestStructPrinter_type {}
    internal partial struct byteFormatter {}
    internal partial struct byteStringer {}
    internal partial struct eofCounter {}
    internal partial struct eofTestsᴛ1 {}
    internal partial struct errString {}
    internal partial struct flagPrinter {}
    internal partial struct flagtestsᴛ1 {}
    internal partial struct fmtTestsᴛ1 {}
    internal partial struct formatterFlagTestsᴛ1 {}
    internal partial struct hexBytes {}
    [GoLocalName("P")] internal partial struct mallocTest_P {}
    internal partial struct mallocTestᴛ1 {}
    internal partial struct panictestsᴛ1 {}
    internal partial struct readers_type {}
    internal partial struct readersᴛ1 {}
    internal partial struct renamedBool {}
    internal partial struct renamedBytes {}
    internal partial struct renamedComplex128 {}
    internal partial struct renamedComplex64 {}
    internal partial struct renamedFloat32 {}
    internal partial struct renamedFloat64 {}
    internal partial struct renamedInt {}
    internal partial struct renamedInt16 {}
    internal partial struct renamedInt32 {}
    internal partial struct renamedInt64 {}
    internal partial struct renamedInt8 {}
    internal partial struct renamedString {}
    internal partial struct renamedUint {}
    internal partial struct renamedUint16 {}
    internal partial struct renamedUint32 {}
    internal partial struct renamedUint64 {}
    internal partial struct renamedUint8 {}
    internal partial struct renamedUintptr {}
    internal partial struct reorderTestsᴛ1 {}
    internal partial struct runeScanner {}
    internal partial struct startestsᴛ1 {}
    internal partial struct testState {}
    internal partial struct writeStringFormatter {}
    public partial struct A {}
    public partial struct B {}
    public partial struct C {}
    public partial struct F {}
    public partial struct G {}
    public partial struct I {}
    public partial struct IntString {}
    public partial struct P {}
    public partial struct PanicF {}
    public partial struct PanicGo {}
    public partial struct PanicS {}
    public partial struct Recur {}
    public partial struct RecursiveInt {}
    public partial struct S {}
    public partial struct SE {}
    public partial struct SI {}
    public partial struct ScanTest {}
    public partial struct ScanfMultiTest {}
    public partial struct ScanfTest {}
    public partial struct TB {}
    public partial struct TF {}
    public partial struct TF32 {}
    public partial struct TF64 {}
    public partial struct TI {}
    public partial struct TI16 {}
    public partial struct TI32 {}
    public partial struct TI64 {}
    public partial struct TI8 {}
    public partial struct TS {}
    public partial struct TU {}
    public partial struct TU16 {}
    public partial struct TU32 {}
    public partial struct TU64 {}
    public partial struct TU8 {}
    public partial struct TUI {}
    public partial struct TwoLines {}
    public partial struct U {}
    public partial struct Xs {}
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
    // Go runs every `init` in the package under test - the production files' included -
    // before the first test. The production package is a REFERENCED assembly here, whose
    // module constructor .NET would not run until something in it is touched, so that
    // initialization is forced before anything else in this test module runs.
    [GoInit] internal static void initᴛᴛproduction() {
        builtin.initPackage(typeof(global::go.fmt_package));
    }
}
