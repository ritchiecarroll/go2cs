// go2cs metadata anchor for the INTERNAL (white-box bridge) test class: GoImplement /
// GoImplicitConv attributes whose GENERATED code must merge with a bridge-declared type
// anchor here — the source generators host output in the first class of the
// attribute-bearing file, and only this file's first class is the bridge. Records for
// production and external-test types stay in package_test_info.cs.

// <ImportedTypeAliases>
using testing = go.testing_package;
// </ImportedTypeAliases>

using go;
using static go.compress.flate_package;
using static go.compress.flate_internal_test_package;

// <ExportedTypeAliases>
[assembly: GoDynamicTypeLift("7374727563747b696f2e5265616465727d", "TestWriteError_src")]
[assembly: GoDynamicTypeLift("7374727563747b6e616d6520737472696e673b2066696c6520737472696e677d", "suitesᴛ1")]
[assembly: GoDynamicTypeLift("7374727563747b6e616d6520737472696e673b206c6576656c20696e747d", "levelTestsᴛ1")]
[assembly: GoDynamicTypeLift("7374727563747b6e616d6520737472696e673b206e20696e747d", "sizesᴛ1")]
// </ExportedTypeAliases>

// <InterfaceImplementations>
[assembly: GoImplement<TestReaderReusesReaderBuffer_encodedNotByteReader, io_package.Reader>(Promoted = true)]
[assembly: GoImplement<TestReaderReusesReaderBuffer_encodedNotByteReader, io_package.Reader>]
[assembly: GoImplement<TestWriteError_src, io_package.Reader>(Promoted = true)]
[assembly: GoImplement<TestWriteError_src, io_package.Reader>]
[assembly: GoImplement<errorWriter, io_package.Writer>(Pointer = true)]
[assembly: GoImplement<failWriter, io_package.Writer>(Pointer = true)]
[assembly: GoImplement<sparseReader, io_package.Reader>(Pointer = true)]
[assembly: GoImplement<syncBuffer, io_package.Reader>(Pointer = true)]
[assembly: GoImplement<syncBuffer, io_package.Writer>(Pointer = true)]
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
[assembly: go.GoPositionMap("compress/flate/deflate_test.go", "deflate_test.cs", "AClKABIqAAgSAAoWgoKClKaCgoKClIKCgoKClIKCgoKSgu6CgoKCgoKUgoKCyoKCgoKWgIKmgIKmhICCpoCCpoCCpoIAChaygpSCgoKClIKClKaC1oKClIKCgpSAgoIACxaCpsKCgoKCgpS4ggAJDLKCgqaCpqKCpoKCgqaCgpaCgoKCgoKClJaCkoKUlIKAgoKkgoCCgraAgraCgoKCgoKUgoIACxiUgoKAgqSClJaCgoKClIKCuIKCgoKClIKCgoKUgpSCgoKClIKCgpSmooKmpoKCgoKUuIKCgIIADBgADxqCgoKUooKClIKCAAoKgpiCgoKUgoKCgoSCgoKUggAJCIKYgoKClIKCgoKEgoKChIK6koKClIKClIKCgIK21qKCgoKUgoKUgoKClIKUhIKCqIKCgpSCgqaWgoKogoKCqJKCggAIDIKCgoKEgoKWgpaCgoKClJSCloKChIKCgoKEgoKUgoKCgpSCgriu4oKCgpSChAARJoKClIKCgoKChIKCgpSCgoCCgqSClICCgraAgoKmgoKCgpSEgoIACA7qgoKClNaCgoKClISCgt6igoSCgoKClIKUgpSm2IKCgpSCgoKmlIKClIKCgoiigoKClIKCloKCpqKCgoIACQqiggBn1AGCgoKCABAKooKSgoKSgqgACBSCgoKUhIKCgoKUgIKCpICCgqaCgoKCgpaCgu6mloKCgrqChIKmloKCqIKogoKogqiCloKCuMaChIKEgpSCsoKCgpSAgqSAgqSixA==", "553-555:1;558-564:2;560-562:2.1;568-580:1;932-935:1;1055-1068:1", "", "61=largeDataChunk/1/1/6")]
[assembly: go.GoPositionMap("compress/flate/dict_decoder_test.go", "dict_decoder_test.cs", "ABoaggACPAAWNpKChIKCgoKWgoK4goKCgoKCuoKEgoKClJSUhIKEgoKEgoKEgoKEgoKEgoSCgg==", "75-87:1;88-97:2")]
[assembly: go.GoPositionMap("compress/flate/flate_test.go", "flate_test.cs", "ABIokqaCgrqSlIKCupKCgoKClIL4hJKCmJKEgoL4goKCgoKUggAJCAAGEABxrAKCgoKUgoKCpoKClICCAAkOgoSCgoKCAAQY8oIABxSCgoKogoKCloKClIKEgoKCuIKUhIKCgsqCloKUgpSUgg==")]
[assembly: go.GoPositionMap("compress/flate/huffman_bit_writer_test.go", "huffman_bit_writer_test.cs", "ABMizNSCgpaCgoKUAAgIooKCgpSCgoKChIKCgpaCgoKCgoCCpKaWgoCCpJSWgoKCgoKCgoCCpJSCAAsYADh2ooLsooLswoKUgoKCgoKCgpaCgoKUkoKWgoKCgpSSgoKWgoKCgoKUgoKClIKChIKCgoCCtpaCgoKCgoKCgIKklIKUgoKCgpSCgoSCgoKAgrSCgpaWgoKCgoKCgoCCpJSCoqSilKSkpoKCloKCgvqSgpSCgoKCgoKmgoKUpKSkpIKCloKCgpSCgoKUgoKU")]
[assembly: go.GoPositionMap("compress/flate/inflate_test.go", "inflate_test.cs", "AA4egrqCgoKCloSCgoKChIKCAAgKggALGoKCgoKClILKgoK6goKCgpaEgoKClISCggALCoKClJKCgoKUgoKmooKAgqSCgqaigoKClIKAgg==", "104-114:1;115-124:2;125-136:3")]
[assembly: go.GoPositionMap("compress/flate/reader_test.go", "reader_test.cs", "AA8gpgALDAAEEoKCgoKEgoKClIKClJSCgoKCgoIACQoACxQABRKCgoKClIKUsrKy", "35-59:1;92-94:1")]
[assembly: go.GoPositionMap("compress/flate/writer_test.go", "writer_test.cs", "ABAggoKChIKCgpSUgoKClIKCgoKCAAkUgoKUggAPCJKCgoKClIKUlIKCtIKCgpSCgpSCgpSClIKClIKCloKCgpSClIIACRCigpKApIC2goSSgqiCgoKYkoKCgqaCgoKUiKKCgoKClIKClISChIK+soKEgpaSgoKCmJKCgqiCgoKEgoKCupSUgpaCgoKmgoKUgg==", "17-40:1;118-118:1;120-120:2")]
// </GoSourcePositionMaps>

namespace go.compress;

[GoPackage("flate")]
public static partial class flate_internal_test_package
{
    // C# nested types declared with no access modifier are always private, and the
    // Go type declarations in this package's converted sources are deliberately
    // bare so they read more like the original Go code. The real accessibility for
    // the types - public for a Go-exported name, internal otherwise - are defined
    // via declarations below.

    // <TypeAccessibility>
    [GoValueClone("limit")] partial struct deflateInflateStringTest {}
    // </TypeAccessibility>

    // Go initializes an imported package before the importing package, for every import
    // form - not only the blank one. .NET would never load an assembly nothing has touched
    // yet, so each import that initializes anything is forced below: once per assembly, and
    // ahead of this package's own `init` functions, which this file being the first compile
    // item of the project guarantees.

    // <ImportInitializers>
    [GoInit] internal static void initᴛᴛimportꓸbufio() => builtin.initPackage(typeof(bufio_package));
    [GoInit] internal static void initᴛᴛimportꓸbytes() => builtin.initPackage(typeof(bytes_package));
    [GoInit] internal static void initᴛᴛimportꓸencodingꓸhex() => builtin.initPackage(typeof(encoding.hex_package));
    [GoInit] internal static void initᴛᴛimportꓸerrors() => builtin.initPackage(typeof(errors_package));
    [GoInit] internal static void initᴛᴛimportꓸflag() => builtin.initPackage(typeof(flag_package));
    [GoInit] internal static void initᴛᴛimportꓸfmt() => builtin.initPackage(typeof(fmt_package));
    [GoInit] internal static void initᴛᴛimportꓸinternalꓸtestenv() => builtin.initPackage(typeof(@internal.testenv_package));
    [GoInit] internal static void initᴛᴛimportꓸio() => builtin.initPackage(typeof(io_package));
    [GoInit] internal static void initᴛᴛimportꓸmathꓸrand() => builtin.initPackage(typeof(go.math.rand_package));
    [GoInit] internal static void initᴛᴛimportꓸos() => builtin.initPackage(typeof(os_package));
    [GoInit] internal static void initᴛᴛimportꓸpathꓸfilepath() => builtin.initPackage(typeof(path.filepath_package));
    [GoInit] internal static void initᴛᴛimportꓸreflect() => builtin.initPackage(typeof(reflect_package));
    [GoInit] internal static void initᴛᴛimportꓸruntime() => builtin.initPackage(typeof(runtime_package));
    [GoInit] internal static void initᴛᴛimportꓸruntimeꓸdebug() => builtin.initPackage(typeof(go.runtime.debug_package));
    [GoInit] internal static void initᴛᴛimportꓸstrings() => builtin.initPackage(typeof(strings_package));
    [GoInit] internal static void initᴛᴛimportꓸsync() => builtin.initPackage(typeof(sync_package));
    [GoInit] internal static void initᴛᴛimportꓸtesting() => builtin.initPackage(typeof(testing_package));
    // </ImportInitializers>
}
