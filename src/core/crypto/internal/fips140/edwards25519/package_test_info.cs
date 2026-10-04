// go2cs metadata anchor for a production-reference test project: the test assembly
// REFERENCES the colocated production project instead of
// recompiling its sources, so the production assembly is the single identity for the
// production types and no production class partial may be declared here. The first —
// and only — class is the test metadata class the go2cs-gen generators anchor
// generated adapters and partials to.
global using static global::go.crypto.@internal.fips140.edwards25519_package;

// <ImportedTypeAliases>
global using bigꓸInt = go.math.big_package.ΔInt;
global using bigꓸRat = go.math.big_package.ΔRat;
global using reflectꓸChanDir = go.reflect_package.ΔChanDir;
global using reflectꓸKind = go.reflect_package.ΔKind;
global using reflectꓸMethod = go.reflect_package.ΔMethod;
global using reflectꓸType = go.reflect_package.ΔType;
global using reflectꓸValue = go.reflect_package.ΔValue;
using testing = go.testing_package;
// </ImportedTypeAliases>

using go;
using static global::go.crypto.@internal.fips140.edwards25519_internal_test_package;

// <ExportedTypeAliases>
// </ExportedTypeAliases>

// <InterfaceImplementations>
// </InterfaceImplementations>

// <ImplicitConversions>
[assembly: GoImplicitConv<global::go.crypto.@internal.fips140.edwards25519_package.Point, ж<global::go.crypto.@internal.fips140.edwards25519_package.Point>>(Indirect = true)]
[assembly: GoImplicitConv<global::go.crypto.@internal.fips140.edwards25519_package.affineCached, ж<global::go.crypto.@internal.fips140.edwards25519_package.affineCached>>(Indirect = true)]
[assembly: GoImplicitConv<global::go.crypto.@internal.fips140.edwards25519_package.projCached, ж<global::go.crypto.@internal.fips140.edwards25519_package.projCached>>(Indirect = true)]
// </ImplicitConversions>

// Go source positions are recorded here, one `GoPositionMap` attribute per converted
// source file in this compilation, so that `runtime.Caller` and the tracebacks built on it
// can name the GO file and line a frame was converted from rather than the emitted C# one.
// Each record carries the Go file's identity and an encoded C#-line to Go-line table
// TOGETHER: a frame either has a record and reports a position that exists in the Go tree,
// or has none - golib, the BCL and hand-written conversions - and reports its own C# position.

// <GoSourcePositionMaps>
[assembly: go.GoPositionMap("crypto/internal/fips140/edwards25519/edwards25519_test.go", "edwards25519_test.cs", "ABAcopSigoKygoKCitKCgoKmgoKCysaCgoCCpICCpIKmAAgGgoSCgoKCgpSEgoKCgpSClIKU1oKCAAgIlIKCgIKkpKT2ggCOAagCspKCgpSCgpSClICCpMqCgoKUpqKCgoKCgg==", "259-275:1")]
[assembly: go.GoPositionMap("crypto/internal/fips140/edwards25519/scalar_alias_test.go", "scalar_alias_test.cs", "AAwYgqK2gIK4gIK4lrLGgIK4goCCtoKAgriAgriCgIK2goCCtoKAgriWABAE1tbW5urqqoKC", "13-28:1;30-72:2;75-77:3;78-80:4;81-83:5;84-86:6;87-91:7;88-90:7.1;92-96:8;93-95:8.1;97-101:9;98-100:9.1")]
[assembly: go.GoPositionMap("crypto/internal/fips140/edwards25519/scalar_test.go", "scalar_test.cs", "ABQmooKClKayspjCgoLGtLaCtoKCuoIACAaSkpSmgpKUgILIgrSCgIKkgpSAgqaigIKklICCpoKCgoCCpKTIgoKCsoKCgpSCgpSAgsiWgoKCgoCCpoKCgoKAgqaCgoKCgILIgoKClKaCtJKChpKCgoKEhJaAgsiCpJKGkoKEloCCyILeAAkWhIKC7oKCgoKCpKaCgpSC", "67-69:1;76-84:1;89-94:2;114-123:1;166-182:1;190-201:1")]
[assembly: go.GoPositionMap("crypto/internal/fips140/edwards25519/scalarmult_test.go", "scalarmult_test.cs", "AA4cpJaCgoKCgpSEgoKClNaCgoKClKaCgoKClKaCgoKCgpSCgoKUpoKigoKygoKCgoKWgILIqJKSgoSChISWgILIpoSCgoKCgpSUgqiCgoKUgoK4gpKSgoKCloCC+IKChIK4gqK0hIKChIKWgILMsoSCuKKEgriihII=", "70-80:1;91-102:1;140-146:1;163-174:1")]
[assembly: go.GoPositionMap("crypto/internal/fips140/edwards25519/tables_test.go", "tables_test.cs", "AAoWgoKEooKCloKEgoKCgoKEgriCgoSigoKWgoSCgoKCgoSCuIKChLKCgoKWgoKEgoKChIKCgoSCuIKChLKCgoKWgoKEgoKChIKCgoSC")]
// </GoSourcePositionMaps>

namespace go.crypto.@internal.fips140;

[GoPackage("edwards25519")]
public static partial class edwards25519_internal_test_package
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
    [GoInit] internal static void initᴛᴛimportꓸcryptoꓸinternalꓸfips140ꓸedwards25519ꓸfield() => builtin.initPackage(typeof(go.crypto.@internal.fips140.edwards25519.field_package));
    [GoInit] internal static void initᴛᴛimportꓸencodingꓸhex() => builtin.initPackage(typeof(encoding.hex_package));
    [GoInit] internal static void initᴛᴛimportꓸmathꓸbig() => builtin.initPackage(typeof(math.big_package));
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
        builtin.initPackage(typeof(global::go.crypto.@internal.fips140.edwards25519_package));
    }
}
