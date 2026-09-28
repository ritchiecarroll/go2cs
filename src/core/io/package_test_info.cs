// go2cs metadata anchor for a production-reference test project: the test assembly
// REFERENCES the colocated production project instead of
// recompiling its sources, so the production assembly is the single identity for the
// production types and no production class partial may be declared here. The first —
// and only — class is the test metadata class the go2cs-gen generators anchor
// generated adapters and partials to.
global using static global::go.io_package;
global using static global::go.io_internal_test_package;

// <ImportedTypeAliases>
global using osꓸDirEntry = go.io.fs_package.DirEntry;
global using osꓸFileInfo = go.io.fs_package.FileInfo;
global using osꓸFileMode = go.io.fs_package.FileMode;
global using osꓸPathError = go.io.fs_package.PathError;
global using osꓸSignal = go.os_package.ΔSignal;
global using runtimeꓸError = go.runtime_package.ΔError;
global using timeꓸLocation = go.time_package.ΔLocation;
global using timeꓸMonth = go.time_package.ΔMonth;
global using timeꓸWeekday = go.time_package.ΔWeekday;
using testing = go.testing_package;
// </ImportedTypeAliases>

using go;
using static global::go.io_test_package;

// <ExportedTypeAliases>
[assembly: GoDynamicTypeLift("696e746572666163657b696f2e5772697465723b20666d742e537472696e6765727d", "testMultiWriter_sink")]
[assembly: GoDynamicTypeLift("7374727563747b696f2e5772697465723b20666d742e537472696e6765727d", "TestMultiWriter_sink")]
// </ExportedTypeAliases>

// <InterfaceImplementations>
[assembly: GoImplement<TestMultiWriter_WriteStringSingleAlloc_simpleWriter, io_package.Writer>(Promoted = true)]
[assembly: GoImplement<TestMultiWriter_WriteStringSingleAlloc_simpleWriter, io_package.Writer>]
[assembly: GoImplement<TestMultiWriter_sink, fmt_package.Stringer>(Promoted = true)]
[assembly: GoImplement<TestMultiWriter_sink, io_package.Writer>(Promoted = true)]
[assembly: GoImplement<TestMultiWriter_sink, testMultiWriter_sink>]
[assembly: GoImplement<TestNopCloserWriterToForwarding_typeᴛ1, io_package.Reader>(Promoted = true)]
[assembly: GoImplement<TestNopCloserWriterToForwarding_typeᴛ1, io_package.Reader>]
[assembly: GoImplement<TestNopCloserWriterToForwarding_typeᴛ1, io_package.WriterTo>(Promoted = true)]
[assembly: GoImplement<TestPipeCloseError_testError1, error>(Promoted = true)]
[assembly: GoImplement<TestPipeCloseError_testError1, error>]
[assembly: GoImplement<TestPipeCloseError_testError2, error>(Promoted = true)]
[assembly: GoImplement<TestPipeCloseError_testError2, error>]
[assembly: GoImplement<byteAndEOFReader, io_package.Reader>]
[assembly: GoImplement<bytes_package.Buffer, fmt_package.Stringer>(Pointer = true)]
[assembly: GoImplement<bytes_package.Buffer, io_package.ReadWriter>(Pointer = true)]
[assembly: GoImplement<bytes_package.Buffer, io_package.Reader>(Pointer = true)]
[assembly: GoImplement<bytes_package.Buffer, io_package.Writer>(Pointer = true)]
[assembly: GoImplement<bytes_package.Buffer, testMultiWriter_sink>(Pointer = true)]
[assembly: GoImplement<bytes_package.Reader, io_package.Reader>(Pointer = true)]
[assembly: GoImplement<bytes_package.Reader, io_package.ReaderAt>(Pointer = true)]
[assembly: GoImplement<dataAndErrorBuffer, io_package.ReadWriter>(Pointer = true)]
[assembly: GoImplement<errWriter, io_package.Writer>]
[assembly: GoImplement<go.io_test_package.Buffer, io_package.Reader>(Pointer = true)]
[assembly: GoImplement<go.io_test_package.Buffer, io_package.Writer>(Pointer = true)]
[assembly: GoImplement<io_package.PipeReader, closer>(Pointer = true)]
[assembly: GoImplement<io_package.PipeWriter, closer>(Pointer = true)]
[assembly: GoImplement<largeWriter, io_package.Writer>]
[assembly: GoImplement<noReadFrom, io_package.Writer>(Pointer = true)]
[assembly: GoImplement<os_package.File, io_package.Reader>(Pointer = true)]
[assembly: GoImplement<os_package.File, io_package.WriterAt>(Pointer = true)]
[assembly: GoImplement<readerFunc, io_package.Reader>]
[assembly: GoImplement<strings_package.Builder, io_package.Writer>(Pointer = true)]
[assembly: GoImplement<strings_package.Reader, io_package.Reader>(Pointer = true)]
[assembly: GoImplement<strings_package.Reader, io_package.ReaderAt>(Pointer = true)]
[assembly: GoImplement<wantedAndErrReader, io_package.Reader>]
[assembly: GoImplement<writeStringChecker, io_package.Writer>(Pointer = true)]
[assembly: GoImplement<writeToChecker, io_package.Reader>(Pointer = true)]
[assembly: GoImplement<writerFunc, io_package.Writer>]
[assembly: GoImplement<zeroErrReader, io_package.Reader>]
// </InterfaceImplementations>

// <ImplicitConversions>
[assembly: GoImplicitConv<dataAndErrorBuffer, ж<dataAndErrorBuffer>>(Indirect = true)]
// </ImplicitConversions>

// Go source positions are recorded here, one `GoPositionMap` attribute per converted
// source file in this compilation, so that `runtime.Caller` and the tracebacks built on it
// can name the GO file and line a frame was converted from rather than the emitted C# one.
// Each record carries the Go file's identity and an encoded C#-line to Go-line table
// TOGETHER: a frame either has a record and reports a position that exists in the Go tree,
// or has none - golib, the BCL and hand-written conversions - and reports its own C# position.

// <GoSourcePositionMaps>
[assembly: go.GoPositionMap("io/io_test.go", "io_test.cs", "AB44koKCgoKC6IKCgoKCgpaCgriCgoKCgoK4goKCgoKCuIKCgoKCgriCgoKCgoIACRSCgqyygoKCgoKkAAcQgu6C7LKCgoKCuIKCgoKCgriCgoKCgoK4goKCgoKCuKKCgoKEgoK4ooKCgoSCggAHEIIACAqC1qiEgoKWgoKWgoKWgoKWgoKWgoK4goIACRSygoKUpoKCgqaCgoKmgoKCgoKUgpSCgpSClIKClIKUgoKUgpSCgoKAgqSClIK4goKCgoKCgIKkgpSClICCpIKCgoKAggARCIKCAA0sgoKCgoCCpICC2pSChIKCgoKC3oKCloKCAAgIggADEIKCgoCCAAgKgoKCgoKClIKClICCAAoUgtaCgoKCgoCCpoKCgoKAggAOCKIAAxSEgoKCABAKooKCgpSSlpKCgoKC3pKCgoKCAAYQkgAIGoKCggAMDoKCgoSigoKClJSClIKCooSCgoKU1paCgoKUgoLogoL6ooKCgoKUppSCgoKCAA4IgoKChIKCgoKUlJSCgoKUgoK6goLmgpKCgoKmqIKCkoLqgsKCkoI=", "515-524:1;527-537:2;540-561:3;569-606:1;582-591:1.1;639-646:1;647-659:2;663-683:3;688-693:4")]
[assembly: go.GoPositionMap("io/multi_test.go", "multi_test.cs", "ABomgoKCgoKCgoKCgpSCgoKCpoKCpoKmlJKCgoKUkoKCgpSSuILegoKUgoKClIKUgIIACQiClKyC+qKSiJKUgvyCgqaC1oKCgoKCAA0IiIKEgoKEgpaCloKCloLegqiygoKCgqKCgpaUgpaChJLKgoaCgpSCgoLqkoKCgoKCupKCgoKCgoKUgt6CqLKCgoKClNiygoKCgqKCgqiCloSSAAYSgqaUgtiSgoKUgoK+soKCgoLoooK4koKCgpKogoCCpoLmpoCCyIKChIKEqIKAgsqCgII=", "23-30:1;31-48:2;49-54:3;55-60:4;61-63:5;112-114:1;182-186:1;204-206:1;207-210:2;268-272:1;331-338:1;335-337:1.1")]
[assembly: go.GoPositionMap("io/pipe_test.go", "pipe_test.cs", "ABEigoKClIKUqJKCgoKCgoKklIKCpoKCgoKCgpSClLqSgoKCgoKCgoKUgpSCgqaCgoIACBSSgoKmgoKCgoKUgoKCgoKCqIKCpIKCpoKUlIKClIKUgoIAESSCAAsYgoKCgpSUgpSmgrKCgoKUlIKCgoKClIKUgpSAgtySgoKCgoKC7JKygoKClJSCgoKClIKUgpSAgtySgoKCgoKCuIKCkoKUgoKmgoKSgpSCgqaigpKCgqKCgpSCgpaCgoKClIKEgpSCAA4IgoiCgIKkgoCCpoKCgIKkgoCCAAoIgqyChIKSgoCCyoKCgILcgoKCqIKEgoKigoKAgqSogoCC3IKClIKCgoLKgoKCgpSCAAUQgoK6goI=", "267-270:1;278-281:1;292-300:1;351-377:1;355-360:1.1;379-412:2;384-391:2.1;431-433:1")]
// </GoSourcePositionMaps>

namespace go;

[GoPackage("io_test")]
public static partial class io_test_package
{
    // C# nested types declared with no access modifier are always private, and the
    // `[GoType]` declarations in this package's converted sources are deliberately
    // bare so they read more like the original Go code. The real accessibility for
    // the types - public for a Go-exported name, internal otherwise - are defined
    // via declarations below.

    // <TypeAccessibility>
    internal partial interface closer {}
    internal partial interface testMultiWriter_sink {}
    [GoLocalName("simpleWriter")] internal partial struct TestMultiWriter_WriteStringSingleAlloc_simpleWriter {}
    internal partial struct TestMultiWriter_sink {}
    internal partial struct TestNopCloserWriterToForwarding_type {}
    internal partial struct TestNopCloserWriterToForwarding_typeᴛ1 {}
    internal partial struct TestOffsetWriter_Seek_tests {}
    [GoLocalName("testError1")] internal partial struct TestPipeCloseError_testError1 {}
    [GoLocalName("testError2")] internal partial struct TestPipeCloseError_testError2 {}
    internal partial struct TestSectionReader_ReadAt_tests {}
    internal partial struct TestSectionReader_Size_tests {}
    internal partial struct byteAndEOFReader {}
    internal partial struct dataAndErrorBuffer {}
    internal partial struct errWriter {}
    internal partial struct largeWriter {}
    internal partial struct noReadFrom {}
    internal partial struct pipeReturn {}
    internal partial struct pipeTest {}
    internal partial struct wantedAndErrReader {}
    internal partial struct writeStringChecker {}
    internal partial struct writeToChecker {}
    internal partial struct zeroErrReader {}
    public partial struct Buffer {}
    // </TypeAccessibility>

    // Go initializes an imported package before the importing package, for every import
    // form - not only the blank one. .NET would never load an assembly nothing has touched
    // yet, so each import that initializes anything is forced below: once per assembly, and
    // ahead of this package's own `init` functions, which this file being the first compile
    // item of the project guarantees.

    // <ImportInitializers>
    [GoInit] internal static void initᴛᴛimportꓸbytes() => builtin.initPackage(typeof(bytes_package));
    [GoInit] internal static void initᴛᴛimportꓸcryptoꓸsha1() => builtin.initPackage(typeof(crypto.sha1_package));
    [GoInit] internal static void initᴛᴛimportꓸerrors() => builtin.initPackage(typeof(errors_package));
    [GoInit] internal static void initᴛᴛimportꓸfmt() => builtin.initPackage(typeof(fmt_package));
    [GoInit] internal static void initᴛᴛimportꓸio() => builtin.initPackage(typeof(io_package));
    [GoInit] internal static void initᴛᴛimportꓸos() => builtin.initPackage(typeof(os_package));
    [GoInit] internal static void initᴛᴛimportꓸruntime() => builtin.initPackage(typeof(runtime_package));
    [GoInit] internal static void initᴛᴛimportꓸslices() => builtin.initPackage(typeof(slices_package));
    [GoInit] internal static void initᴛᴛimportꓸstrings() => builtin.initPackage(typeof(strings_package));
    [GoInit] internal static void initᴛᴛimportꓸsync() => builtin.initPackage(typeof(sync_package));
    [GoInit] internal static void initᴛᴛimportꓸsyncꓸatomic() => builtin.initPackage(typeof(go.sync.atomic_package));
    [GoInit] internal static void initᴛᴛimportꓸtesting() => builtin.initPackage(typeof(testing_package));
    [GoInit] internal static void initᴛᴛimportꓸtime() => builtin.initPackage(typeof(time_package));
    // </ImportInitializers>
    // Go runs every `init` in the package under test - the production files' included -
    // before the first test. The production package is a REFERENCED assembly here, whose
    // module constructor .NET would not run until something in it is touched, so that
    // initialization is forced before anything else in this test module runs.
    [GoInit] internal static void initᴛᴛproduction() {
        builtin.initPackage(typeof(global::go.io_package));
    }
}
