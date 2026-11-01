// go2cs metadata anchor for the EXTERNAL test package (<name>_test): GoImplement /
// GoImplicitConv attributes recorded by its converted _test files whose GENERATED code
// (adapter classes, partial-struct implementations, conversion operators) must anchor to
// the test package class — the source generators host output in the first class of the
// attribute-bearing file, and test-file cast sites reference the adapters as members of
// the test package class. Production-anchored records stay in package_test_info.cs.

// <ImportedTypeAliases>
// </ImportedTypeAliases>

using go;
using static go.errors_package;
using static go.errors_test_package;

// <ExportedTypeAliases>
[assembly: GoDynamicTypeLift("6572726f72735f746573742e74696d656f7574", "TestAsType_timeout")]
[assembly: GoDynamicTypeLift("696e746572666163657b54696d656f7574282920626f6f6c3b206572726f727d", "TestAsType_timeout")]
[assembly: GoDynamicTypeLift("696e746572666163657b54696d656f7574282920626f6f6c7d", "TestAs_timeout")]
// </ExportedTypeAliases>

// <InterfaceImplementations>
[assembly: GoImplement<MyAsError, error>]
[assembly: GoImplement<MyError, error>]
[assembly: GoImplement<MyIsError, error>]
[assembly: GoImplement<errorT, compError>]
[assembly: GoImplement<errorUncomparable, error>(Pointer = true)]
[assembly: GoImplement<errorUncomparable, error>]
[assembly: GoImplement<multiErr, error>]
[assembly: GoImplement<poser, error>(Pointer = true)]
[assembly: GoImplement<wrapped, error>]
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
// </GoSourcePositionMaps>

namespace go;

public static partial class errors_test_package
{
    // C# nested types declared with no access modifier are always private, and the
    // Go type declarations in this package's converted sources are deliberately
    // bare so they read more like the original Go code. The real accessibility for
    // the types - public for a Go-exported name, internal otherwise - are defined
    // via declarations below.

    // <TypeAccessibility>
    [GoLocalName("timeout")] internal partial interface TestAsType_timeout {}
    internal partial interface TestAs_timeout {}
    internal partial interface compError {}
    internal partial struct TestAs_testCases {}
    internal partial struct TestIs_testCases {}
    internal partial struct TestJoinErrorMethod_type {}
    internal partial struct TestJoin_type {}
    internal partial struct TestUnwrap_testCases {}
    internal partial struct errorT {}
    internal partial struct errorUncomparable {}
    internal partial struct multiErr {}
    internal partial struct poser {}
    internal partial struct wrapped {}
    public partial struct MyAsError {}
    public partial struct MyError {}
    public partial struct MyIsError {}
    // </TypeAccessibility>

    // Go initializes an imported package before the importing package, for every import
    // form - not only the blank one. .NET would never load an assembly nothing has touched
    // yet, so each import that initializes anything is forced below: once per assembly, and
    // ahead of this package's own `init` functions, which this file being the first compile
    // item of the project guarantees.

    // <ImportInitializers>
    [GoInit] internal static void initᴛᴛimportꓸfmt() => builtin.initPackage(typeof(fmt_package));
    [GoInit] internal static void initᴛᴛimportꓸioꓸfs() => builtin.initPackage(typeof(io.fs_package));
    [GoInit] internal static void initᴛᴛimportꓸos() => builtin.initPackage(typeof(os_package));
    [GoInit] internal static void initᴛᴛimportꓸreflect() => builtin.initPackage(typeof(reflect_package));
    [GoInit] internal static void initᴛᴛimportꓸtesting() => builtin.initPackage(typeof(testing_package));
    [GoInit] internal static void initᴛᴛimportꓸtime() => builtin.initPackage(typeof(time_package));
    // </ImportInitializers>
}
