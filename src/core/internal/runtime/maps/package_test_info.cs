// go2cs metadata anchor for a production-reference test project: the test assembly
// REFERENCES the colocated production project instead of
// recompiling its sources, so the production assembly is the single identity for the
// production types and no production class partial may be declared here. The first —
// and only — class is the test metadata class the go2cs-gen generators anchor
// generated adapters and partials to.
global using static global::go.@internal.runtime.maps_package;
global using static global::go.@internal.runtime.maps_internal_test_package;

// <ImportedTypeAliases>
global using abiꓸArrayType = go.@internal.abi_package.ΔArrayType;
global using abiꓸChanDir = go.@internal.abi_package.ΔChanDir;
global using abiꓸFuncType = go.@internal.abi_package.ΔFuncType;
global using abiꓸInterfaceType = go.@internal.abi_package.ΔInterfaceType;
global using abiꓸKind = go.@internal.abi_package.ΔKind;
global using abiꓸName = go.@internal.abi_package.ΔName;
global using abiꓸStructType = go.@internal.abi_package.ΔStructType;
global using mapsꓸMap = go.@internal.runtime.maps_package.ΔMap;
global using reflectꓸChanDir = go.reflect_package.ΔChanDir;
global using reflectꓸKind = go.reflect_package.ΔKind;
global using reflectꓸMethod = go.reflect_package.ΔMethod;
global using reflectꓸType = go.reflect_package.ΔType;
global using reflectꓸValue = go.reflect_package.ΔValue;
// </ImportedTypeAliases>

using go;
using static global::go.@internal.runtime.maps_test_package;

// <ExportedTypeAliases>
[assembly: GoTypeAlias("CtrlGroup", "global::go.@internal.runtime.maps_package.ctrlGroup")]
[assembly: GoTypeAlias("Map", "ΔMap")]
// </ExportedTypeAliases>

// <InterfaceImplementations>
[assembly: GoImplement<bytes_package.Buffer, io_package.Writer>(Pointer = true)]
[assembly: GoImplement<bytes_package.Reader, io_package.Reader>(Pointer = true)]
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
[assembly: go.GoPositionMap("internal/runtime/maps/fuzz_test.go", "fuzz_test.cs", "ACVGAAcUgoKAgqSmpoKChJKCgIKmpoIAChiCgoKogoKCuLQADyIAOniCgoKWgoKylIKEgpSClIKCtoKkgqg=", "175-211:1")]
[assembly: go.GoPositionMap("internal/runtime/maps/map_swiss_test.go", "map_swiss_test.cs", "ABcugoKUABwQqABe0gGShIKCloKCloKWgoKWgoKoorKiooLKorKiooLKorKiooLKorKiooI=", "144-170:1;172-181:2;174-179:2.1;175-178:2.1.1;182-191:3;184-189:3.1;185-188:3.1.1;192-201:4;194-199:4.1;195-198:4.1.1;202-211:5;204-209:5.1;205-208:5.1.1")]
[assembly: go.GoPositionMap("internal/runtime/maps/map_test.go", "map_test.cs", "ABIggoKCuIKEkpSCgoKEgqiCloKEgoKCgoKUgoLMkoSSlIKCgoSCqIKWgoSCgoKCgpSCgsqChJKUgoKChIKogoSCgpaCloKEgoKCgoLKgoSSlIKCgoSCqISCloKEgoKCgoL+ooSSkpSCgpaCgoKWgpaCgoKSgpaCgoKUgroACxSEkpSCgoQABxKClIKCzKiChISChKaWgriChJKUgoKChIKohIKCgoKSgpaCgpaCloKEgoKCgoKClILMkoSSlIKCgoSCqIKCkoKCgoKSgpaCgoSCqIKUqIKWgoSCgoSCgpaCgoKUgsyyhJKUgoKChIKogoKSgoKCgpKCloKChIKogqiSlIKCgoSCugAFEIKEgoKEgoKWgoKClILKgoSSlIKCgoSCqIKCgoKCkoKWgoKClICCpJaCkpSCgoKEggAKFtSApoAACQaCABk+goKClILMkoSSlISCloKClIKCloKCgoKC6IKIkpKEgoKChIKogpaCgoSCgoKCgpSCgsyShJKUhIKWgoKUgoKWhIKC", "552-552:1;555-555:2")]
// </GoSourcePositionMaps>

namespace go.@internal.runtime;

[GoPackage("maps_test")]
public static partial class maps_test_package
{
    // C# nested types declared with no access modifier are always private, and the
    // Go type declarations in this package's converted sources are deliberately
    // bare so they read more like the original Go code. The real accessibility for
    // the types - public for a Go-exported name, internal otherwise - are defined
    // via declarations below.

    // <TypeAccessibility>
    internal partial struct TestAlignUpPow2_tests {}
    [GoLocalName("big")] internal partial struct TestMapIndirect_big {}
    [GoLocalName("mapCase")] internal partial struct TestTableGroupCount_mapCase {}
    [GoLocalName("mapCount")] internal partial struct TestTableGroupCount_mapCount {}
    internal partial struct TestTableGroupCount_type {}
    internal partial struct fuzzCommand {}
    public partial struct fuzzOp {}
    // </TypeAccessibility>

    // Go initializes an imported package before the importing package, for every import
    // form - not only the blank one. .NET would never load an assembly nothing has touched
    // yet, so each import that initializes anything is forced below: once per assembly, and
    // ahead of this package's own `init` functions, which this file being the first compile
    // item of the project guarantees.

    // <ImportInitializers>
    [GoInit] internal static void initᴛᴛimportꓸbytes() => builtin.initPackage(typeof(bytes_package));
    [GoInit] internal static void initᴛᴛimportꓸencodingꓸbinary() => builtin.initPackage(typeof(encoding.binary_package));
    [GoInit] internal static void initᴛᴛimportꓸfmt() => builtin.initPackage(typeof(fmt_package));
    [GoInit] internal static void initᴛᴛimportꓸinternalꓸabi() => builtin.initPackage(typeof(go.@internal.abi_package));
    [GoInit] internal static void initᴛᴛimportꓸinternalꓸruntimeꓸmaps() => builtin.initPackage(typeof(go.@internal.runtime.maps_package));
    [GoInit] internal static void initᴛᴛimportꓸmath() => builtin.initPackage(typeof(go.math_package));
    [GoInit] internal static void initᴛᴛimportꓸreflect() => builtin.initPackage(typeof(reflect_package));
    [GoInit] internal static void initᴛᴛimportꓸtesting() => builtin.initPackage(typeof(testing_package));
    // </ImportInitializers>
    // Go runs every `init` in the package under test - the production files' included -
    // before the first test. The production package is a REFERENCED assembly here, whose
    // module constructor .NET would not run until something in it is touched, so that
    // initialization is forced before anything else in this test module runs.
    [GoInit] internal static void initᴛᴛproduction() {
        builtin.initPackage(typeof(global::go.@internal.runtime.maps_package));
    }
}
