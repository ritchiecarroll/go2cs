// go2cs metadata anchor for a production-reference test project: the test assembly
// REFERENCES the colocated production project instead of
// recompiling its sources, so the production assembly is the single identity for the
// production types and no production class partial may be declared here. The first —
// and only — class is the test metadata class the go2cs-gen generators anchor
// generated adapters and partials to.
global using static global::go.crypto.@internal.fips140.nistec_package;
global using static global::go.crypto.@internal.fips140.nistec_internal_test_package;

// <ImportedTypeAliases>
using testing = go.testing_package;
// </ImportedTypeAliases>

using go;
using static global::go.crypto.@internal.fips140.nistec_test_package;

// <ExportedTypeAliases>
// </ExportedTypeAliases>

// <InterfaceImplementations>
[assembly: GoImplement<go.crypto.@internal.fips140.nistec_package.P224Point, nistPoint<go.crypto.@internal.fips140.nistec_package.P224Point>>(ConstraintProxy = true)]
[assembly: GoImplement<go.crypto.@internal.fips140.nistec_package.P256Point, nistPoint<go.crypto.@internal.fips140.nistec_package.P256Point>>(ConstraintProxy = true)]
[assembly: GoImplement<go.crypto.@internal.fips140.nistec_package.P384Point, nistPoint<go.crypto.@internal.fips140.nistec_package.P384Point>>(ConstraintProxy = true)]
[assembly: GoImplement<go.crypto.@internal.fips140.nistec_package.P521Point, nistPoint<go.crypto.@internal.fips140.nistec_package.P521Point>>(ConstraintProxy = true)]
// </InterfaceImplementations>

// <ImplicitConversions>
[assembly: GoImplicitConv<global::go.crypto.@internal.fips140.nistec_package.P256Point, ж<global::go.crypto.@internal.fips140.nistec_package.P256Point>>(Indirect = true)]
// </ImplicitConversions>

// Go source positions are recorded here, one `GoPositionMap` attribute per converted
// source file in this compilation, so that `runtime.Caller` and the tracebacks built on it
// can name the GO file and line a frame was converted from rather than the emitted C# one.
// Each record carries the Go file's identity and an encoded C#-line to Go-line table
// TOGETHER: a frame either has a record and reports a position that exists in the Go tree,
// or has none - golib, the BCL and hand-written conversions - and reports its own C# position.

// <GoSourcePositionMaps>
[assembly: go.GoPositionMap("crypto/internal/fips140/nistec/benchmark_test.go", "benchmark_test.cs", "AB4ugoKUgpSClIK4woKCgoKCuIKClIKUgpSCuMKCgoKCgg==", "24-26:1;27-29:2;30-32:3;33-35:4;49-51:1;52-54:2;55-57:3;58-60:4")]
// </GoSourcePositionMaps>

namespace go.crypto.@internal.fips140;

[GoPackage("nistec_test")]
public static partial class nistec_test_package
{
    // C# nested types declared with no access modifier are always private, and the
    // Go type declarations in this package's converted sources are deliberately
    // bare so they read more like the original Go code. The real accessibility for
    // the types - public for a Go-exported name, internal otherwise - are defined
    // via declarations below.

    // <TypeAccessibility>
    internal partial interface nistPoint<T> {}
    // </TypeAccessibility>

    // Go initializes an imported package before the importing package, for every import
    // form - not only the blank one. .NET would never load an assembly nothing has touched
    // yet, so each import that initializes anything is forced below: once per assembly, and
    // ahead of this package's own `init` functions, which this file being the first compile
    // item of the project guarantees.

    // <ImportInitializers>
    [GoInit] internal static void initᴛᴛimportꓸbytes() => builtin.initPackage(typeof(bytes_package));
    [GoInit] internal static void initᴛᴛimportꓸcryptoꓸinternalꓸfips140ꓸnistec() => builtin.initPackage(typeof(go.crypto.@internal.fips140.nistec_package));
    [GoInit] internal static void initᴛᴛimportꓸcryptoꓸinternalꓸfips140ꓸnistecꓸfiat() => builtin.initPackage(typeof(go.crypto.@internal.fips140.nistec.fiat_package));
    [GoInit] internal static void initᴛᴛimportꓸcryptoꓸrand() => builtin.initPackage(typeof(go.crypto.rand_package));
    [GoInit] internal static void initᴛᴛimportꓸfmt() => builtin.initPackage(typeof(fmt_package));
    [GoInit] internal static void initᴛᴛimportꓸtesting() => builtin.initPackage(typeof(testing_package));
    // </ImportInitializers>
    // Go runs every `init` in the package under test - the production files' included -
    // before the first test. The production package is a REFERENCED assembly here, whose
    // module constructor .NET would not run until something in it is touched, so that
    // initialization is forced before anything else in this test module runs.
    [GoInit] internal static void initᴛᴛproduction() {
        builtin.initPackage(typeof(global::go.crypto.@internal.fips140.nistec_package));
    }
}
