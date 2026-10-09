// go2cs metadata anchor for the INTERNAL (white-box bridge) test class: GoImplement /
// GoImplicitConv attributes whose GENERATED code must merge with a bridge-declared type
// anchor here — the source generators host output in the first class of the
// attribute-bearing file, and only this file's first class is the bridge. Records for
// production and external-test types stay in package_test_info.cs.

// <ImportedTypeAliases>
// </ImportedTypeAliases>

using go;
using static go.testing.iotest_package;
using static go.testing.iotest_internal_test_package;

// <ExportedTypeAliases>
[assembly: GoDynamicTypeLift("7374727563747b696e20737472696e673b2077616e7420737472696e673b207472756e6320696e7436343b206e20696e747d", "truncateWriterTestsᴛ1")]
// </ExportedTypeAliases>

// <InterfaceImplementations>
[assembly: GoImplement<errWriter, io_package.Writer>]
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
[assembly: go.GoPositionMap("testing/iotest/logger_test.go", "logger_test.cs", "ABQoguaigoKWkoKCloKCgoSCgoCCpoCSpIKAksTUooKClpKCgpaCgoKEgoKAgqaCgJLE1KKCgpaSgoKWgoKChIKCgoSCgpaAgqaCgJLE5KKCgpaSgoKWgoKChIKEgoKCgpaCgJLE", "30-34:1;62-66:1;91-95:1;128-132:1")]
[assembly: go.GoPositionMap("testing/iotest/reader_test.go", "reader_test.cs", "AA4egoKChIKCgoKWlIKCgoKUgJKklICCpICSyIKEgoKAgqaCgoCCpICSyIKCgpSCgoKCpoKCgoKClICSpJSAgqSAksiChIKCgIKmgoKAgqSAksiCgoKUgoKCgqaCgIKkgJK2goKAgraCgIKkgJLIgpSCgoCCtoKAgqSAkraCgoCCtoKAgqSAksiCgoKEhIKCgoKCgoKCpoKUgJLIgoSCgoCCpoKCgIKkgJIACQiCAAQSgpKSgoKUggAIDIKEgoCC", "242-250:1", "", "292=New/1/1/5")]
[assembly: go.GoPositionMap("testing/iotest/writer_test.go", "writer_test.cs", "ABMYAAYYooKCgoKClICSpICS")]
// </GoSourcePositionMaps>

namespace go.testing;

[GoPackage("iotest")]
public static partial class iotest_internal_test_package
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
    [GoInit] internal static void initᴛᴛimportꓸlog() => builtin.initPackage(typeof(log_package));
    [GoInit] internal static void initᴛᴛimportꓸstrings() => builtin.initPackage(typeof(strings_package));
    [GoInit] internal static void initᴛᴛimportꓸtesting() => builtin.initPackage(typeof(testing_package));
    // </ImportInitializers>
}
