// go2cs metadata anchor for a production-reference test project: the test assembly
// REFERENCES the colocated production project instead of
// recompiling its sources, so the production assembly is the single identity for the
// production types and no production class partial may be declared here. The first —
// and only — class is the test metadata class the go2cs-gen generators anchor
// generated adapters and partials to.
global using static global::go.crypto.@internal.fips140.edwards25519.field_package;

// <ImportedTypeAliases>
global using bigꓸInt = go.math.big_package.ΔInt;
global using bigꓸRat = go.math.big_package.ΔRat;
global using reflectꓸChanDir = go.reflect_package.ΔChanDir;
global using reflectꓸKind = go.reflect_package.ΔKind;
global using reflectꓸMethod = go.reflect_package.ΔMethod;
global using reflectꓸType = go.reflect_package.ΔType;
global using reflectꓸValue = go.reflect_package.ΔValue;
// </ImportedTypeAliases>

using go;
using static global::go.crypto.@internal.fips140.edwards25519.field_internal_test_package;

// <ExportedTypeAliases>
// </ExportedTypeAliases>

// <InterfaceImplementations>
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
[assembly: go.GoPositionMap("crypto/internal/fips140/edwards25519/field/fe_alias_test.go", "fe_alias_test.cs", "AAwYgqK2gIK4gIK4uIKyxoCCuIKAgraCgIK4gIK4goCCtoKAgraCgIK4AAkcAAoCABAwggALIIKUtLSC", "13-28:1;32-74:1;102-104:1;111-114:2;118-120:3;124-126:4")]
[assembly: go.GoPositionMap("crypto/internal/fips140/edwards25519/field/fe_test.go", "fe_test.cs", "ABcogqqigoKUpqKCAAoYAAwYABEmogAIEqKClKrCroLEgoKWgoKCgoSWgILIgoKCgoKWgoKCgpaCgoKCgoKCggAJCIKiqISUgIKmorqCgpSAgqgACR6ygoKCyoKClKaCsoSCgoSCloKElICCyrKCloKigoKUgqiCpoKCgpSokoSCgoKClIKopoKCgIIABBL+0oKUgoKEgpaEgoKUhIKEgriCkpSCgpaCgriCkoKUgoKEgpaEgoKUhIKChIKWkoKAgpKCyIKSlJSChIKWhIKWhIK4gpKCgpaChIKClpaAggALCIQAJ1yCgoKCgoLKgpKChIKEgpaWgIKmgriCgpKUgoSClpaAgsiCgpKSkpSChIKClpiAgsiCgoKU", "107-121:1;156-164:1;169-178:2;216-231:1;411-426:1;493-505:1;517-529:1;537-553:1", "", "42=Uint64/1/5/1,Uint64/2/5/2,Uint64/3/5/3,Uint64/4/5/4,Uint64/5/5/5;87=Intn/1/5/1,Intn/2/5/2,Intn/3/5/3,Intn/4/5/4,Intn/5/5/5;110=Len64/2/5/1,Len64/3/5/2,Len64/4/5/3,Len64/5/5/4;556=isInBounds/2/2/1")]
// </GoSourcePositionMaps>

namespace go.crypto.@internal.fips140.edwards25519;

[GoPackage("field")]
public static partial class field_internal_test_package
{
    // C# nested types declared with no access modifier are always private, and the
    // Go type declarations in this package's converted sources are deliberately
    // bare so they read more like the original Go code. The real accessibility for
    // the types - public for a Go-exported name, internal otherwise - are defined
    // via declarations below.

    // <TypeAccessibility>
    [GoLocalName("target")] partial struct TestAliasing_target {}
    [GoLocalName("feRTTest")] partial struct TestSetBytesRoundTrip_feRTTest {}
    [GoLocalName("test")] partial struct TestSqrtRatio_test {}
    // </TypeAccessibility>

    // Go initializes an imported package before the importing package, for every import
    // form - not only the blank one. .NET would never load an assembly nothing has touched
    // yet, so each import that initializes anything is forced below: once per assembly, and
    // ahead of this package's own `init` functions, which this file being the first compile
    // item of the project guarantees.

    // <ImportInitializers>
    [GoInit] internal static void initᴛᴛimportꓸbytes() => builtin.initPackage(typeof(bytes_package));
    [GoInit] internal static void initᴛᴛimportꓸcryptoꓸrand() => builtin.initPackage(typeof(go.crypto.rand_package));
    [GoInit] internal static void initᴛᴛimportꓸencodingꓸhex() => builtin.initPackage(typeof(encoding.hex_package));
    [GoInit] internal static void initᴛᴛimportꓸio() => builtin.initPackage(typeof(io_package));
    [GoInit] internal static void initᴛᴛimportꓸmathꓸbig() => builtin.initPackage(typeof(math.big_package));
    [GoInit] internal static void initᴛᴛimportꓸmathꓸbits() => builtin.initPackage(typeof(math.bits_package));
    [GoInit] internal static void initᴛᴛimportꓸmathꓸrand() => builtin.initPackage(typeof(math.rand_package));
    [GoInit] internal static void initᴛᴛimportꓸreflect() => builtin.initPackage(typeof(reflect_package));
    [GoInit] internal static void initᴛᴛimportꓸtesting() => builtin.initPackage(typeof(testing_package));
    [GoInit] internal static void initᴛᴛimportꓸtestingꓸquick() => builtin.initPackage(typeof(go.testing.quick_package));
    // </ImportInitializers>
    // Go runs every `init` in the package under test - the production files' included -
    // before the first test. The production package is a REFERENCED assembly here, whose
    // module constructor .NET would not run until something in it is touched, so that
    // initialization is forced before anything else in this test module runs.
    [GoInit] internal static void initᴛᴛproduction() {
        builtin.initPackage(typeof(global::go.crypto.@internal.fips140.edwards25519.field_package));
    }
}
