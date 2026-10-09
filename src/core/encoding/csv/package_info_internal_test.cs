// go2cs metadata anchor for the INTERNAL (white-box bridge) test class: GoImplement /
// GoImplicitConv attributes whose GENERATED code must merge with a bridge-declared type
// anchor here — the source generators host output in the first class of the
// attribute-bearing file, and only this file's first class is the bridge. Records for
// production and external-test types stay in package_test_info.cs.

// <ImportedTypeAliases>
// </ImportedTypeAliases>

using go;
using static go.encoding.csv_package;
using static go.encoding.csv_internal_test_package;

// <ExportedTypeAliases>
[assembly: GoDynamicTypeLift("7374727563747b496e707574205b5d5b5d737472696e673b204f757470757420737472696e673b204572726f72206572726f723b2055736543524c4620626f6f6c3b20436f6d6d612072756e657d", "writeTestsᴛ1")]
// </ExportedTypeAliases>

// <InterfaceImplementations>
[assembly: GoImplement<errorWriter, io_package.Writer>]
[assembly: GoImplement<nTimes, io_package.Reader>(Pointer = true)]
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
[assembly: go.GoPositionMap("encoding/csv/fuzz_test.go", "fuzz_test.cs", "AA8egoKEggAIEoKCgoKCgoKCgoSCgpSEgoKCgoKCgpSmlISCgoKCgoKCgoKogoLcmIKWgg==", "16-95:1;83-85:1.1")]
[assembly: go.GoPositionMap("encoding/csv/reader_test.go", "reader_test.cs", "AC5QAPEC3gWCgoKEgpSCgpSUgoKClrKigoKAgoKUgqaClILKgoKCqIKCgoKCkoKUgqaCgpSUgIKkgoKUgoKAggAIFrKCgqamgoKClIKUgoKUkoKCggACFPKCgoKShIKClIKCtIKUtIK0tIK0lAAJFLKCgpSCgoKCgoKUgs7CgoKClIKCgpSCABIigqaCgLaCgAAMBoKugoC2goCAtoKAgLaCgA==", "408-425:1;428-488:2;625-625:1;629-629:1;641-641:1;645-645:1;649-649:1;653-653:1", "", "47=Repeat/1/5/303,Repeat/2/5/303,Repeat/3/5/303,Repeat/4/5/304,Repeat/5/5/304")]
[assembly: go.GoPositionMap("encoding/csv/writer_test.go", "writer_test.cs", "ABccAB9MgoKCgoKClIKClIKCAAoOgtaCgoKCgoSCloKCgoSCuNyigoKCgpQ=")]
// </GoSourcePositionMaps>

namespace go.encoding;

[GoPackage("csv")]
public static partial class csv_internal_test_package
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
    [GoInit] internal static void initᴛᴛimportꓸerrors() => builtin.initPackage(typeof(errors_package));
    [GoInit] internal static void initᴛᴛimportꓸfmt() => builtin.initPackage(typeof(fmt_package));
    [GoInit] internal static void initᴛᴛimportꓸio() => builtin.initPackage(typeof(io_package));
    [GoInit] internal static void initᴛᴛimportꓸreflect() => builtin.initPackage(typeof(reflect_package));
    [GoInit] internal static void initᴛᴛimportꓸslices() => builtin.initPackage(typeof(slices_package));
    [GoInit] internal static void initᴛᴛimportꓸstrings() => builtin.initPackage(typeof(strings_package));
    [GoInit] internal static void initᴛᴛimportꓸtesting() => builtin.initPackage(typeof(testing_package));
    [GoInit] internal static void initᴛᴛimportꓸunicodeꓸutf8() => builtin.initPackage(typeof(go.unicode.utf8_package));
    // </ImportInitializers>
}
