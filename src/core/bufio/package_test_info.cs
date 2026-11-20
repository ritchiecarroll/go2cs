// go2cs metadata anchor for a production-reference test project: the test assembly
// REFERENCES the colocated production project instead of
// recompiling its sources, so the production assembly is the single identity for the
// production types and no production class partial may be declared here. The first —
// and only — class is the test metadata class the go2cs-gen generators anchor
// generated adapters and partials to.
global using static global::go.bufio_package;
global using static global::go.bufio_internal_test_package;

// <ImportedTypeAliases>
// </ImportedTypeAliases>

using go;
using static global::go.bufio_test_package;

// <ExportedTypeAliases>
[assembly: GoDynamicTypeLift("7374727563747b696e70757420737472696e673b20657870656374205b5d627566696f5f746573742e726561644c696e65526573756c747d", "readLineNewlinesTestsᴛ1")]
// </ExportedTypeAliases>

// <InterfaceImplementations>
[assembly: GoImplement<StringReader, io_package.Reader>(Pointer = true)]
[assembly: GoImplement<alwaysError, io_package.Reader>]
[assembly: GoImplement<bufio_package.Reader, io_package.Reader>(Pointer = true)]
[assembly: GoImplement<bufio_package.Writer, io_package.Writer>(Pointer = true)]
[assembly: GoImplement<bytes_package.Buffer, io_package.Reader>(Pointer = true)]
[assembly: GoImplement<bytes_package.Buffer, io_package.Writer>(Pointer = true)]
[assembly: GoImplement<bytes_package.Reader, io_package.Reader>(Pointer = true)]
[assembly: GoImplement<dataAndEOFReader, io_package.Reader>]
[assembly: GoImplement<emptyThenNonEmptyReader, io_package.Reader>(Pointer = true)]
[assembly: GoImplement<endlessZeros, io_package.Reader>]
[assembly: GoImplement<eofReader, io_package.Reader>(Pointer = true)]
[assembly: GoImplement<errorReaderFromTest, io_package.Reader>]
[assembly: GoImplement<errorReaderFromTest, io_package.Writer>]
[assembly: GoImplement<errorThenGoodReader, io_package.Reader>(Pointer = true)]
[assembly: GoImplement<errorWriterTest, io_package.Writer>]
[assembly: GoImplement<errorWriterToTest, io_package.Reader>]
[assembly: GoImplement<errorWriterToTest, io_package.Writer>]
[assembly: GoImplement<largeReader, io_package.Reader>]
[assembly: GoImplement<negativeEOFReader, io_package.Reader>(Pointer = true)]
[assembly: GoImplement<negativeReader, io_package.Reader>(Pointer = true)]
[assembly: GoImplement<onlyReader, io_package.Reader>(Promoted = true)]
[assembly: GoImplement<onlyReader, io_package.Reader>]
[assembly: GoImplement<onlyWriter, io_package.Writer>(Promoted = true)]
[assembly: GoImplement<onlyWriter, io_package.Writer>]
[assembly: GoImplement<readFromWriter, io_package.Writer>(Pointer = true)]
[assembly: GoImplement<rot13Reader, io_package.Reader>(Pointer = true)]
[assembly: GoImplement<scriptedReader, io_package.Reader>(Pointer = true)]
[assembly: GoImplement<slowReader, io_package.Reader>(Pointer = true)]
[assembly: GoImplement<strings_package.Builder, io_package.Writer>(Pointer = true)]
[assembly: GoImplement<strings_package.Reader, io_package.Reader>(Pointer = true)]
[assembly: GoImplement<testReader, io_package.Reader>(Pointer = true)]
[assembly: GoImplement<teststringwriter, io_package.Writer>(Pointer = true)]
[assembly: GoImplement<writeCountingDiscard, io_package.Writer>(Pointer = true)]
[assembly: GoImplement<writeErrorOnlyWriter, io_package.Writer>]
[assembly: GoImplement<writerWithReadFromError, io_package.Writer>]
[assembly: GoImplement<zeroReader, io_package.Reader>]
// </InterfaceImplementations>

// <ImplicitConversions>
// </ImplicitConversions>

// This package's string literals are hoisted to `static readonly` fields (the standard
// library's form, or -hoist-literals), so no converted file passes a bare literal for
// go2cs-gen's literal interceptor to box. The stamp below tells the generator to skip
// the assembly.
[assembly: GoHoistedLiterals]

// The struct types whose fields this package reaches through a pointer, as Go spells it, `p.f`:
// go2cs-gen emits a field property on the pointer for each of their fields.
[assembly: GoFieldAccess(typeof(eofReader))]
[assembly: GoFieldAccess(typeof(errorThenGoodReader))]
[assembly: GoFieldAccess(typeof(readFromWriter))]
[assembly: GoFieldAccess(typeof(rot13Reader))]

// Go source positions are recorded here, one `GoPositionMap` attribute per converted
// source file in this compilation, so that `runtime.Caller` and the tracebacks built on it
// can name the GO file and line a frame was converted from rather than the emitted C# one.
// Each record carries the Go file's identity and an encoded C#-line to Go-line table
// TOGETHER: a frame either has a record and reports a position that exists in the Go tree,
// or has none - golib, the BCL and hand-written conversions - and reports its own C# position.

// <GoSourcePositionMaps>
[assembly: GoPositionMap("bufio/bufio_test.go", "bufio_test.cs", "ACE4goKCxoKCgoKCgoKmyLKCgoKCgpSCgoKCpuaCgoKAgraCgIIACRIABxTCgoKCgpSClJSosoKCgoKCgqYABxAADBq4goKCgoKCgpSUgoKCgoKCgoKCgoKCAA0WggAJBoKClIKigqa0goKCxgAMFLKCgoKUlMaCgoKCgoKCgpSUlIK4AAoWgoIACAiCgoKClIKCgoKUlJSClIKClIKmggAICIKCgoKC6IKCgoKC6IKCgoKC6IKCgoKC6IKCgoLogoKCgviCgoKClIKCgoKUlJSClIKClIKmgriCgoKClIKCgpSCuIKCuICCAA0KhMiCAAkUhJKCgqaCooKClILKgoKCgpSUuIKCABYMkoKCgpSCgpSClIKmgoKUgoKUgqaCgpSCgoKmgqaCgpSCgpSCgpSCpoKClIKClIL4lIKCgoKCgoKCuIKCgpSCgoKCgpSCppSUgoKCgsqmgoKCgoKCAAkKgoKUgoKigpSCgqaCuIKUgpSCgoKC3IKCgoKCgpSCpoKClIKCgoLugoKCgoKUgoLKgriCgoKUlIIADBiCxgAIEoKCgoKCgqaCgoIACQyCgpSCgqaCgviCgpSCgqaCguiCgoKCgoKCgoKClIKCAAkIgoKCgoKCgoKCgpSCgoKClIKCgoKCgpSCgoKCgoIAChKCgqaCgqaCgoKUggAJCIKCgoKClIKCAAsIgpSCgIKkgIKkgIKkgIKkgIKkgIKkgIKkgIKkgIKkgIKkgIKkgILIgoCCpICCpICCpICCAAgMgsaUgoKCgqaCggAJELKCgpSClIKCgpTGgoKCgoKCgoKUgpSCgpSUgpSUgsqCgqaCgoKUgoKCgpSCgoKUgoKClIKCAAkIgoKCgoKCgoKClIKClIK4goKCgriCgoKCgqaCgviCgoKCggAPGAAPJoKCuIKCgoKCgpSCgpSCgsqCgriCgqamgoKCgoKmgoIADBaCpoLG7oKCgoLKgsrKgoKCgoKCgIKCpICCgqSSAA0YgqaCxgAHEIKCgoIABBLCgoKCgpSCgpSCgpSCgqaCgoKCgpSCgpSCgpSCgpSCggAJEIKCAAgKAAgExoKCpLSC1raipAAIDoKCgoKUxoKCgoKAgqSAgqSAgqSAgqSC+pKCpoKCuIKCAAoSgoKUgsiSgqaCgriCgoKUgpK4goKmgoK4goKCAAsUgoKCpoKCgoLKopSCgpSCgpSCgpSCpoCSpICSyIKCgoKCkoKCgpSUgoKCAA4KgoKCgoKUgriCgoKCpoKUgoKmgoKCgoIACAaCwoKUgoKCgpSCpoKCgoKClIK4goKCgoKUgoKCgoIAEgiCACRigpQACRSClAAJFIKUABgygoKCgoKClIKCpoKSgpSCzIKAkqSAksiCgJKkgJIAEyKCgpSCgsaiggAJEIKClMqmxoKCgqaCgoKUgILIgIK2gILIgoKUggAMDIKmggAKBoKCgIKkgIIADQyCAAoKooKCgoCCpICCpICCpIK4tIKCgoKCgoKCuLSCgoKCgoKCgriigoKCgoKCgoKC6KKCgoKCgpSCgoKCgpSCyqKCgoKCgpSCgsq0goKCgoKCgoK4ooKCgoKCgoKCuKKCgoKCgoKCgoLoooKCgoKCgpSCyqKCgoKCgoKCgoKCgoKCuKKCgoKCgg==", "188-191:1;407-410:1;428-436:2;594-602:1;1167-1167:1;1168-1168:2;1173-1173:3;1294-1305:1;1468-1483:1;1471-1478:1.1;1488-1497:1;1614-1619:1;1627-1632:2;1640-1645:3", "", "1879=NewReader/1/4/15,NewReader/2/4/23,NewReader/3/4/30,NewReader/4/4/37,newScriptedReader/1/5/48,newScriptedReader/2/5/61,newScriptedReader/3/5/74,New/1/1/82,newScriptedReader/4/5/88,newScriptedReader/5/5/96", "", "1530:21,1531:13")]
[assembly: GoPositionMap("bufio/scan_test.go", "scan_test.cs", "ABYqkoKCygALGIKCgoKCgoKCpoKUgoLMkoKCgoKSlLKClIKCgqaClIKClIKCygAKGJKCgoKCgoKCgpSCgqaClIKUgoIADRiCgpTM0oKCgpSCgoKUlIKClLqkgoKCgoKCgpSUgpSCgoKCgoKClJSCgqaCgrqSlIKCgoKCgoKClIKCgoKCgoKUlIKCpoKCupKCgoKCgoKmgoL6koK46JKCuOiSgsrokoLKptakgoKCgpSClIKWkoKCgoKCgriClIKC+pKUwoKCgpSUlIKUggAJDoL2goKClIKCAAkOgsaCgoKUgoL4goKCgoKCgpSCvqKCgqamgoKCgoKClIKmgpSCuIKmgqaCgpQACQaigpSCgoKUgqaCgqaCtKSCgoKCpoL8goKClMiSgoKClIKUgrqSgoKCgoKCpoIACRCCgoKClIKUgoKU+qKCgoKCgoKCpoIAChCCyqKClII=", "315-324:1;350-359:1;469-477:1", "", "", "", "535:21,536:13")]
// </GoSourcePositionMaps>

namespace go;

[GoPackage("bufio_test")]
public static partial class bufio_test_package
{
    // C# nested types declared with no access modifier are always private, and the
    // Go type declarations in this package's converted sources are deliberately
    // bare so they read more like the original Go code. The real accessibility for
    // the types - public for a Go-exported name, internal otherwise - are defined
    // via declarations below.

    // <TypeAccessibility>
    internal partial struct TestReaderDiscard_tests {}
    internal partial struct alwaysError {}
    internal partial struct bufReader {}
    internal partial struct countdown {}
    internal partial struct dataAndEOFReader {}
    internal partial struct emptyThenNonEmptyReader {}
    internal partial struct endlessZeros {}
    internal partial struct eofReader {}
    internal partial struct errorReaderFromTest {}
    internal partial struct errorThenGoodReader {}
    internal partial struct errorWriterTest {}
    internal partial struct errorWriterToTest {}
    internal partial struct largeReader {}
    internal partial struct negativeEOFReader {}
    internal partial struct negativeReader {}
    internal partial struct onlyReader {}
    internal partial struct onlyWriter {}
    internal partial struct readFromWriter {}
    internal partial struct readLineNewlinesTestsᴛ1 {}
    internal partial struct readLineResult {}
    internal partial struct readMaker {}
    internal partial struct rot13Reader {}
    internal partial struct scriptedReader {}
    internal partial struct slowReader {}
    internal partial struct testReader {}
    internal partial struct teststringwriter {}
    internal partial struct writeCountingDiscard {}
    internal partial struct writeErrorOnlyWriter {}
    internal partial struct writerWithReadFromError {}
    internal partial struct zeroReader {}
    public partial struct StringReader {}
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
    [GoInit] internal static void initᴛᴛimportꓸio() => builtin.initPackage(typeof(io_package));
    [GoInit] internal static void initᴛᴛimportꓸmathꓸrand() => builtin.initPackage(typeof(math.rand_package));
    [GoInit] internal static void initᴛᴛimportꓸstrconv() => builtin.initPackage(typeof(strconv_package));
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
        builtin.initPackage(typeof(global::go.bufio_package));
    }
}

// A Go method is an extension method on its receiver type. The methods of one type are
// in a class of their own beside the package class, so that a method and a package-level
// type, constant or variable can share a name as they do in Go. Each class is declared
// below with the type it holds the methods of; its blocks are in the converted sources,
// in Go's declaration order.

// <MethodHolders>
[GoMethodHost(typeof(bufio_test_package.StringReader)), global::System.ComponentModel.EditorBrowsable(global::System.ComponentModel.EditorBrowsableState.Never)] public static partial class bufio_testᴛStringReader;
[GoMethodHost(typeof(bufio_test_package.alwaysError)), global::System.ComponentModel.EditorBrowsable(global::System.ComponentModel.EditorBrowsableState.Never)] public static partial class bufio_testᴛalwaysError;
[GoMethodHost(typeof(bufio_test_package.countdown)), global::System.ComponentModel.EditorBrowsable(global::System.ComponentModel.EditorBrowsableState.Never)] public static partial class bufio_testᴛcountdown;
[GoMethodHost(typeof(bufio_test_package.dataAndEOFReader)), global::System.ComponentModel.EditorBrowsable(global::System.ComponentModel.EditorBrowsableState.Never)] public static partial class bufio_testᴛdataAndEOFReader;
[GoMethodHost(typeof(bufio_test_package.emptyThenNonEmptyReader)), global::System.ComponentModel.EditorBrowsable(global::System.ComponentModel.EditorBrowsableState.Never)] public static partial class bufio_testᴛemptyThenNonEmptyReader;
[GoMethodHost(typeof(bufio_test_package.endlessZeros)), global::System.ComponentModel.EditorBrowsable(global::System.ComponentModel.EditorBrowsableState.Never)] public static partial class bufio_testᴛendlessZeros;
[GoMethodHost(typeof(bufio_test_package.eofReader)), global::System.ComponentModel.EditorBrowsable(global::System.ComponentModel.EditorBrowsableState.Never)] public static partial class bufio_testᴛeofReader;
[GoMethodHost(typeof(bufio_test_package.errorReaderFromTest)), global::System.ComponentModel.EditorBrowsable(global::System.ComponentModel.EditorBrowsableState.Never)] public static partial class bufio_testᴛerrorReaderFromTest;
[GoMethodHost(typeof(bufio_test_package.errorThenGoodReader)), global::System.ComponentModel.EditorBrowsable(global::System.ComponentModel.EditorBrowsableState.Never)] public static partial class bufio_testᴛerrorThenGoodReader;
[GoMethodHost(typeof(bufio_test_package.errorWriterTest)), global::System.ComponentModel.EditorBrowsable(global::System.ComponentModel.EditorBrowsableState.Never)] public static partial class bufio_testᴛerrorWriterTest;
[GoMethodHost(typeof(bufio_test_package.errorWriterToTest)), global::System.ComponentModel.EditorBrowsable(global::System.ComponentModel.EditorBrowsableState.Never)] public static partial class bufio_testᴛerrorWriterToTest;
[GoMethodHost(typeof(bufio_test_package.largeReader)), global::System.ComponentModel.EditorBrowsable(global::System.ComponentModel.EditorBrowsableState.Never)] public static partial class bufio_testᴛlargeReader;
[GoMethodHost(typeof(bufio_test_package.negativeEOFReader)), global::System.ComponentModel.EditorBrowsable(global::System.ComponentModel.EditorBrowsableState.Never)] public static partial class bufio_testᴛnegativeEOFReader;
[GoMethodHost(typeof(bufio_test_package.negativeReader)), global::System.ComponentModel.EditorBrowsable(global::System.ComponentModel.EditorBrowsableState.Never)] public static partial class bufio_testᴛnegativeReader;
[GoMethodHost(typeof(bufio_test_package.readFromWriter)), global::System.ComponentModel.EditorBrowsable(global::System.ComponentModel.EditorBrowsableState.Never)] public static partial class bufio_testᴛreadFromWriter;
[GoMethodHost(typeof(bufio_test_package.rot13Reader)), global::System.ComponentModel.EditorBrowsable(global::System.ComponentModel.EditorBrowsableState.Never)] public static partial class bufio_testᴛrot13Reader;
[GoMethodHost(typeof(bufio_test_package.scriptedReader)), global::System.ComponentModel.EditorBrowsable(global::System.ComponentModel.EditorBrowsableState.Never)] public static partial class bufio_testᴛscriptedReader;
[GoMethodHost(typeof(bufio_test_package.slowReader)), global::System.ComponentModel.EditorBrowsable(global::System.ComponentModel.EditorBrowsableState.Never)] public static partial class bufio_testᴛslowReader;
[GoMethodHost(typeof(bufio_test_package.testReader)), global::System.ComponentModel.EditorBrowsable(global::System.ComponentModel.EditorBrowsableState.Never)] public static partial class bufio_testᴛtestReader;
[GoMethodHost(typeof(bufio_test_package.teststringwriter)), global::System.ComponentModel.EditorBrowsable(global::System.ComponentModel.EditorBrowsableState.Never)] public static partial class bufio_testᴛteststringwriter;
[GoMethodHost(typeof(bufio_test_package.writeCountingDiscard)), global::System.ComponentModel.EditorBrowsable(global::System.ComponentModel.EditorBrowsableState.Never)] public static partial class bufio_testᴛwriteCountingDiscard;
[GoMethodHost(typeof(bufio_test_package.writeErrorOnlyWriter)), global::System.ComponentModel.EditorBrowsable(global::System.ComponentModel.EditorBrowsableState.Never)] public static partial class bufio_testᴛwriteErrorOnlyWriter;
[GoMethodHost(typeof(bufio_test_package.writerWithReadFromError)), global::System.ComponentModel.EditorBrowsable(global::System.ComponentModel.EditorBrowsableState.Never)] public static partial class bufio_testᴛwriterWithReadFromError;
[GoMethodHost(typeof(bufio_test_package.zeroReader)), global::System.ComponentModel.EditorBrowsable(global::System.ComponentModel.EditorBrowsableState.Never)] public static partial class bufio_testᴛzeroReader;
// </MethodHolders>
