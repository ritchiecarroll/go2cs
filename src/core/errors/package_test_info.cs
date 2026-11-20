// go2cs metadata anchor for a production-reference test project: the test assembly
// REFERENCES the colocated production project instead of
// recompiling its sources, so the production assembly is the single identity for the
// production types and no production class partial may be declared here. The first —
// and only — class is the test metadata class the go2cs-gen generators anchor
// generated adapters and partials to.
global using static global::go.errors_package;

// <ImportedTypeAliases>
// </ImportedTypeAliases>

using go;
using static global::go.errors_test_package;

// <ExportedTypeAliases>
[assembly: GoDynamicTypeLift("6572726f72735f746573742e74696d656f7574", "TestAsType_timeout")]
[assembly: GoDynamicTypeLift("696e746572666163657b54696d656f7574282920626f6f6c3b206572726f727d", "TestAsType_timeout")]
[assembly: GoDynamicTypeLift("696e746572666163657b54696d656f7574282920626f6f6c7d", "TestAs_timeout")]
[assembly: GoDynamicTypeLift("696e746572666163657b556e777261702829205b5d6572726f727d", "ExampleJoin_type")]
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

// This package's string literals are hoisted to `static readonly` fields (the standard
// library's form, or -hoist-literals), so no converted file passes a bare literal for
// go2cs-gen's literal interceptor to box. The stamp below tells the generator to skip
// the assembly.
[assembly: GoHoistedLiterals]

// The struct types whose fields this package reaches through a pointer, as Go spells it, `p.f`:
// go2cs-gen emits a field property on the pointer for each of their fields.
[assembly: GoFieldAccess(typeof(go.io.fs_package.PathError))]

// Go source positions are recorded here, one `GoPositionMap` attribute per converted
// source file in this compilation, so that `runtime.Caller` and the tracebacks built on it
// can name the GO file and line a frame was converted from rather than the emitted C# one.
// Each record carries the Go file's identity and an encoded C#-line to Go-line table
// TOGETHER: a frame either has a record and reports a position that exists in the Go tree,
// or has none - golib, the BCL and hand-written conversions - and reports its own C# position.

// <GoSourcePositionMaps>
[assembly: GoPositionMap("errors/errors_test.go", "errors_test.cs", "ABIYlIKUgriCgriCgoI=")]
[assembly: GoPositionMap("errors/example_test.go", "example_test.cs", "ABcqgsaC3IKCAAgKgoKCAAgKgqaUguyygoKClIKCgoIADRiikoKCABAKgoKCgoKClIKUAA0SgoKClAANGIKUggAICqKCggAJDoKCgoKUAAcQgoKClAAMFoKUgoKClMoACQqigoKUgoKCAAgWooKClIKCAAwSgoKCgg==", "", "", "31=Date/1/1/1;237=New/1/1/3")]
[assembly: GoPositionMap("errors/join_test.go", "join_test.cs", "AAsagoCCpICCpICCAA0IgoKCggANIIKClIIACQqCgoIAChqCgg==")]
[assembly: GoPositionMap("errors/wrap_test.go", "wrap_test.cs", "ABYggoKClJTIAB9IspKCAAoWtIKCgpS0tMSUABMGgoKCgoKClABbwAGylIKCooKClIKUggAJDIKC3IKCwoKUgoKUogAKCIKCgoqUurq6urq6urq6uLq6urq6uroAChiCgoKCgoL6ooKUgoL6ooKCgoL6ooKCggAJCoKClAAGFIKCAAoOAAoOggALCIIADQyCpoKC", "23-25:1;64-68:2;204-215:1;229-238:1;230-232:1.1;351-356:1", "", "35=New/1/1/27;137=New/1/4/46,New/2/4/66,New/3/4/71,New/4/4/81;332=New/1/1/1;348=New/1/1/1;352=New/1/1/1;360=New/1/1/1", "", "273:29,274:21")]
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
    internal partial interface ExampleJoin_type {}
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

// A Go method is an extension method on its receiver type. The methods of one type are
// in a class of their own beside the package class, so that a method and a package-level
// type, constant or variable can share a name as they do in Go. Each class is declared
// below with the type it holds the methods of; its blocks are in the converted sources,
// in Go's declaration order.

// <MethodHolders>
[GoMethodHost(typeof(errors_test_package.MyAsError)), global::System.ComponentModel.EditorBrowsable(global::System.ComponentModel.EditorBrowsableState.Never)] public static partial class errors_testᴛMyAsError;
[GoMethodHost(typeof(errors_test_package.MyError)), global::System.ComponentModel.EditorBrowsable(global::System.ComponentModel.EditorBrowsableState.Never)] public static partial class errors_testᴛMyError;
[GoMethodHost(typeof(errors_test_package.MyIsError)), global::System.ComponentModel.EditorBrowsable(global::System.ComponentModel.EditorBrowsableState.Never)] public static partial class errors_testᴛMyIsError;
[GoMethodHost(typeof(errors_test_package.errorT)), global::System.ComponentModel.EditorBrowsable(global::System.ComponentModel.EditorBrowsableState.Never)] public static partial class errors_testᴛerrorT;
[GoMethodHost(typeof(errors_test_package.errorUncomparable)), global::System.ComponentModel.EditorBrowsable(global::System.ComponentModel.EditorBrowsableState.Never)] public static partial class errors_testᴛerrorUncomparable;
[GoMethodHost(typeof(errors_test_package.multiErr)), global::System.ComponentModel.EditorBrowsable(global::System.ComponentModel.EditorBrowsableState.Never)] public static partial class errors_testᴛmultiErr;
[GoMethodHost(typeof(errors_test_package.poser)), global::System.ComponentModel.EditorBrowsable(global::System.ComponentModel.EditorBrowsableState.Never)] public static partial class errors_testᴛposer;
[GoMethodHost(typeof(errors_test_package.wrapped)), global::System.ComponentModel.EditorBrowsable(global::System.ComponentModel.EditorBrowsableState.Never)] public static partial class errors_testᴛwrapped;
// </MethodHolders>
