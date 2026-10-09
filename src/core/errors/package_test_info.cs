// go2cs metadata anchor for a production-reference test project: the test assembly
// REFERENCES the colocated production project instead of
// recompiling its sources, so the production assembly is the single identity for the
// production types and no production class partial may be declared here. The first —
// and only — class is the test metadata class the go2cs-gen generators anchor
// generated adapters and partials to.
global using static global::go.errors_package;

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
// </ImportedTypeAliases>

using go;
using static global::go.errors_test_package;

// <ExportedTypeAliases>
[assembly: GoDynamicTypeLift("696e746572666163657b54696d656f7574282920626f6f6c7d", "TestAs_timeout")]
[assembly: GoDynamicTypeLift("696e746572666163657b556e777261702829205b5d6572726f727d", "TestJoin_typeᴛ1")]
// </ExportedTypeAliases>

// <InterfaceImplementations>
[assembly: GoImplement<MyError, error>]
[assembly: GoImplement<errorT, error>]
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
[assembly: go.GoPositionMap("errors/errors_test.go", "errors_test.cs", "AA8YlIKUgqiCgriCgoI=")]
[assembly: go.GoPositionMap("errors/example_test.go", "example_test.cs", "ABQqgqaC3IKAgvqSgoLuwpKCggAJCpKCgoKCgpSCAAcS0oCCgpQACBCigIKCgpQACBCigoKC", "", "", "26=Date/1/1/1")]
[assembly: go.GoPositionMap("errors/join_test.go", "join_test.cs", "AAsagoCCpICCpICCAA0IgoKCAAoagoKUggAJCoKCggAKGoKC")]
[assembly: go.GoPositionMap("errors/wrap_test.go", "wrap_test.cs", "ABUggoKChIS4AB9IspKAggALFpSAooCigpS0tMSUABAGgoKCgoKChABbwAGylJKCooKClIKUgIIACQyCgtyCgsKClIKClKL4ooKEgoL6ooKCgoIACQqCgoQABhSCgIIACQ6A/oCigPiAooAACQyCpoKC", "23-25:1;64-68:2;204-215:1;229-238:1;230-232:1.1", "", "31=New/1/1/27;135=New/1/4/46,New/2/4/66,New/3/4/71,New/4/4/81")]
// </GoSourcePositionMaps>

namespace go;

[GoPackage("errors_test")]
public static partial class errors_test_package
{
    // C# nested types declared with no access modifier are always private, and the
    // Go type declarations in this package's converted sources are deliberately
    // bare so they read more like the original Go code. The real accessibility for
    // the types - public for a Go-exported name, internal otherwise - are defined
    // via declarations below.

    // <TypeAccessibility>
    internal partial interface TestAs_timeout {}
    internal partial interface TestJoin_typeᴛ1 {}
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
    public partial struct MyError {}
    // </TypeAccessibility>

    // Go initializes an imported package before the importing package, for every import
    // form - not only the blank one. .NET would never load an assembly nothing has touched
    // yet, so each import that initializes anything is forced below: once per assembly, and
    // ahead of this package's own `init` functions, which this file being the first compile
    // item of the project guarantees.

    // <ImportInitializers>
    [GoInit] internal static void initᴛᴛimportꓸerrors() => builtin.initPackage(typeof(errors_package));
    [GoInit] internal static void initᴛᴛimportꓸfmt() => builtin.initPackage(typeof(fmt_package));
    [GoInit] internal static void initᴛᴛimportꓸioꓸfs() => builtin.initPackage(typeof(io.fs_package));
    [GoInit] internal static void initᴛᴛimportꓸos() => builtin.initPackage(typeof(os_package));
    [GoInit] internal static void initᴛᴛimportꓸreflect() => builtin.initPackage(typeof(reflect_package));
    [GoInit] internal static void initᴛᴛimportꓸtesting() => builtin.initPackage(typeof(testing_package));
    [GoInit] internal static void initᴛᴛimportꓸtime() => builtin.initPackage(typeof(time_package));
    // </ImportInitializers>
    // Go runs every `init` in the package under test - the production files' included -
    // before the first test. The production package is a REFERENCED assembly here, whose
    // module constructor .NET would not run until something in it is touched, so that
    // initialization is forced before anything else in this test module runs.
    [GoInit] internal static void initᴛᴛproduction() {
        builtin.initPackage(typeof(global::go.errors_package));
    }
}
