// go2cs metadata anchor for the INTERNAL (white-box bridge) test class: GoImplement /
// GoImplicitConv attributes whose GENERATED code must merge with a bridge-declared type
// anchor here — the source generators host output in the first class of the
// attribute-bearing file, and only this file's first class is the bridge. Records for
// production and external-test types stay in package_test_info.cs.

// <ImportedTypeAliases>
// </ImportedTypeAliases>

using go;
using static go.crypto.mlkem_package;
using static go.crypto.mlkem_internal_test_package;

// <ExportedTypeAliases>
// </ExportedTypeAliases>

// <InterfaceImplementations>
[assembly: GoImplement<global::go.crypto.mlkem_package.EncapsulationKey1024, encapsulationKey>(Pointer = true)]
[assembly: GoImplement<global::go.crypto.mlkem_package.EncapsulationKey768, encapsulationKey>(Pointer = true)]
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
[assembly: go.GoPositionMap("crypto/mlkem/mlkem_test.go", "mlkem_test.cs", "AB84goKUgri4goKUgoKCgpSCloKClIKUgoKUgpSCgoKUgpaCgpSClIKWgoKUgriCgpSCuLiCgoKUgoKEgoCCtoKCgoCCuIKAgraCgoKAgriCgIK2goKCgILamKKCgoKClIKCloKCgoKEgoKCgpSChIKCgoSCgpSCloKCgpSWgoLcopKCgoKCgriigoKCgoKClIKCgoKClIK4ooKClIKCgoKC+IKCgpSCgoKClKKCgoKUgoSCgpSmkoKCgpSCgpTMkoKWgpaCloKWgpaC", "29-31:1;32-34:2;99-101:1;102-104:2;277-292:1;293-305:2")]
// </GoSourcePositionMaps>

namespace go.crypto;

[GoPackage("mlkem")]
public static partial class mlkem_internal_test_package
{
    // C# nested types declared with no access modifier are always private, and the
    // `[GoType]` declarations in this package's converted sources are deliberately
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
    [GoInit] internal static void initᴛᴛimportꓸcryptoꓸinternalꓸfips140ꓸmlkem() => builtin.initPackage(typeof(go.crypto.@internal.fips140.mlkem_package));
    [GoInit] internal static void initᴛᴛimportꓸcryptoꓸinternalꓸfips140ꓸsha3() => builtin.initPackage(typeof(go.crypto.@internal.fips140.sha3_package));
    [GoInit] internal static void initᴛᴛimportꓸcryptoꓸmlkem() => builtin.initPackage(typeof(go.crypto.mlkem_package));
    [GoInit] internal static void initᴛᴛimportꓸcryptoꓸrand() => builtin.initPackage(typeof(go.crypto.rand_package));
    [GoInit] internal static void initᴛᴛimportꓸencodingꓸhex() => builtin.initPackage(typeof(encoding.hex_package));
    [GoInit] internal static void initᴛᴛimportꓸflag() => builtin.initPackage(typeof(flag_package));
    [GoInit] internal static void initᴛᴛimportꓸlog() => builtin.initPackage(typeof(log_package));
    [GoInit] internal static void initᴛᴛimportꓸtesting() => builtin.initPackage(typeof(testing_package));
    // </ImportInitializers>
}
