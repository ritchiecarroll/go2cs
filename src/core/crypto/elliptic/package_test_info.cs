// go2cs metadata anchor for a production-reference test project: the test assembly
// REFERENCES the colocated production project instead of
// recompiling its sources, so the production assembly is the single identity for the
// production types and no production class partial may be declared here. The first —
// and only — class is the test metadata class the go2cs-gen generators anchor
// generated adapters and partials to.
global using static global::go.crypto.elliptic_package;

// <ImportedTypeAliases>
global using bigꓸInt = go.math.big_package.ΔInt;
global using bigꓸRat = go.math.big_package.ΔRat;
// </ImportedTypeAliases>

using go;
using static global::go.crypto.elliptic_internal_test_package;

// <ExportedTypeAliases>
[assembly: GoDynamicTypeLift("63727970746f2f656c6c69707469632e636f6d62696e65644d756c74", "TestP256CombinedMult_combinedMult")]
[assembly: GoDynamicTypeLift("696e746572666163657b436f6d62696e65644d756c742862696758202a6d6174682f6269672e496e742c2062696759202a6d6174682f6269672e496e742c20626173655363616c6172205b5d627974652c207363616c6172205b5d6279746529202878202a6d6174682f6269672e496e742c2079202a6d6174682f6269672e496e74293b2063727970746f2f656c6c69707469632e43757276657d", "TestP256CombinedMult_combinedMult")]
// </ExportedTypeAliases>

// <InterfaceImplementations>
[assembly: GoImplement<synthCombinedMult, TestP256CombinedMult_combinedMult>(Pointer = true)]
[assembly: GoImplement<synthCombinedMult, global::go.crypto.elliptic_package.Curve>(Promoted = true)]
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
[assembly: go.GoPositionMap("crypto/elliptic/elliptic_test.go", "elliptic_test.cs", "ABImwpL2ggAJGoKUgoKSgvqCgoKCyoKCgoKCloKCgoKEgoLKgoKmsqaigoSSlJKWkpSSlpKUkpaSuoKCgoKUgoKUgoKWgpaAgsiAgqSCgoKAggAICIKCgoKClIKCgpSCyoKU1oKCuoKChIKCgoSAgqamgoKEgqiChICCAAUQsoIACwaigoKogoKWgoKCloKCgpaCgoKWgoKC3oCCgpQACwiCgoKCgoKUgoKCgpaCgoKCqIKWgoKClPqigpSCgpaCgpaClIK4goKCgoKCAAkKggAFEoKCksqCgoKCgoKUyoKCgoKCgoIACQqCgoKigoKCgoK4ooKCgoKC", "43-46:1;52-56:1;61-77:1;157-170:1;229-233:1;278-283:1;284-289:2;291-297:3;303-309:4;337-343:1;358-360:1;365-374:1;378-386:1;390-412:1;392-401:1.1;402-411:1.2", "", "35=P256/1/2/4,P256/2/2/5,genericParamsForCurve/1/4/5,P224/1/2/6,P224/2/2/7,genericParamsForCurve/2/4/7,P384/1/2/8,P384/2/2/9,genericParamsForCurve/3/4/9,P521/1/2/10,P521/2/2/11,genericParamsForCurve/4/4/11;390=P256/1/1/4,P224/1/1/5,P384/1/1/6,P521/1/1/7")]
[assembly: go.GoPositionMap("crypto/elliptic/p224_test.go", "p224_test.cs", "ABQmAIYCjgSCgoKCgpSCgpSCypSCgoKClIKClIL6lIKCgoI=")]
[assembly: go.GoPositionMap("crypto/elliptic/p256_test.go", "p256_test.cs", "ABIkABAigoKEgoKClIKChIKCgoKWgsqCgoKCgoKChIKCAAgSgoKCAAgGgoyCloKEgoKCgpaCgqiCgqiCgqiCgoKWlIKCuIKCgoKCgpSCgg==")]
// </GoSourcePositionMaps>

namespace go.crypto;

[GoPackage("elliptic")]
public static partial class elliptic_internal_test_package
{
    // C# nested types declared with no access modifier are always private, and the
    // Go type declarations in this package's converted sources are deliberately
    // bare so they read more like the original Go code. The real accessibility for
    // the types - public for a Go-exported name, internal otherwise - are defined
    // via declarations below.

    // <TypeAccessibility>
    [GoLocalName("combinedMult")] partial interface TestP256CombinedMult_combinedMult {}
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
    [GoInit] internal static void initᴛᴛimportꓸfmt() => builtin.initPackage(typeof(fmt_package));
    [GoInit] internal static void initᴛᴛimportꓸmathꓸbig() => builtin.initPackage(typeof(math.big_package));
    [GoInit] internal static void initᴛᴛimportꓸtesting() => builtin.initPackage(typeof(testing_package));
    // </ImportInitializers>
    // Go runs every `init` in the package under test - the production files' included -
    // before the first test. The production package is a REFERENCED assembly here, whose
    // module constructor .NET would not run until something in it is touched, so that
    // initialization is forced before anything else in this test module runs.
    [GoInit] internal static void initᴛᴛproduction() {
        builtin.initPackage(typeof(global::go.crypto.elliptic_package));
    }
}
