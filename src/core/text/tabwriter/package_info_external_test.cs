// go2cs metadata anchor for the EXTERNAL test package (<name>_test): GoImplement /
// GoImplicitConv attributes recorded by its converted _test files whose GENERATED code
// (adapter classes, partial-struct implementations, conversion operators) must anchor to
// the test package class — the source generators host output in the first class of the
// attribute-bearing file, and test-file cast sites reference the adapters as members of
// the test package class. Production-anchored records stay in package_test_info.cs.

// <ImportedTypeAliases>
using testing = go.testing_package;
// </ImportedTypeAliases>

using go;
using static go.text.tabwriter_package;
using static go.text.tabwriter_test_package;

// <ExportedTypeAliases>
[assembly: GoDynamicTypeLift("7374727563747b746573746e616d6520737472696e673b206d696e776964746820696e743b20746162776964746820696e743b2070616464696e6720696e743b207061646368617220627974653b20666c6167732075696e743b2073726320737472696e673b20657870656374656420737472696e677d", "testsᴛ1")]
// </ExportedTypeAliases>

// <InterfaceImplementations>
[assembly: GoImplement<buffer, io_package.Writer>(Pointer = true)]
[assembly: GoImplement<panicWriter, io_package.Writer>]
// </InterfaceImplementations>

// <ImplicitConversions>
[assembly: GoImplicitConv<buffer, ж<buffer>>(Indirect = true)]
// </ImplicitConversions>

// Go source positions are recorded here, one `GoPositionMap` attribute per converted
// source file in this compilation, so that `runtime.Caller` and the tracebacks built on it
// can name the GO file and line a frame was converted from rather than the emitted C# one.
// Each record carries the Go file's identity and an encoded C#-line to Go-line table
// TOGETHER: a frame either has a record and reports a position that exists in the Go tree,
// or has none - golib, the BCL and hand-written conversions - and reports its own C# position.

// <GoSourcePositionMaps>
// </GoSourcePositionMaps>

namespace go.text;

public static partial class tabwriter_test_package
{
    // C# nested types declared with no access modifier are always private, and the
    // Go type declarations in this package's converted sources are deliberately
    // bare so they read more like the original Go code. The real accessibility for
    // the types - public for a Go-exported name, internal otherwise - are defined
    // via declarations below.

    // <TypeAccessibility>
    internal partial struct buffer {}
    internal partial struct panicWriter {}
    internal partial struct testsᴛ1 {}
    // </TypeAccessibility>

    // Go initializes an imported package before the importing package, for every import
    // form - not only the blank one. .NET would never load an assembly nothing has touched
    // yet, so each import that initializes anything is forced below: once per assembly, and
    // ahead of this package's own `init` functions, which this file being the first compile
    // item of the project guarantees.

    // <ImportInitializers>
    [GoInit] internal static void initᴛᴛimportꓸbytes() => builtin.initPackage(typeof(bytes_package));
    [GoInit] internal static void initᴛᴛimportꓸfmt() => builtin.initPackage(typeof(fmt_package));
    [GoInit] internal static void initᴛᴛimportꓸio() => builtin.initPackage(typeof(io_package));
    [GoInit] internal static void initᴛᴛimportꓸtesting() => builtin.initPackage(typeof(testing_package));
    // </ImportInitializers>
}
