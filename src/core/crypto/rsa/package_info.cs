// go2cs code converter defines `global using` statements here for imported type
// aliases as package references are encountered via `import' statements. Exported
// type aliases that need a `global using` declaration will be loaded from the
// referenced package by parsing its 'package_info.cs' source file and reading its
// defined `GoTypeAlias` attributes.

// Package name separator "dot" used in imported type aliases is extended Unicode
// character '\uA4F8' which is a valid character in a C# identifier name. This is
// used to simulate Go's package level type aliases since C# does not yet support
// importing type aliases at a namespace level.

// <ImportedTypeAliases>
global using bigꓸInt = go.math.big_package.ΔInt;
global using cryptoꓸDecrypterOpts = object;
global using cryptoꓸPrivateKey = object;
global using cryptoꓸPublicKey = object;
global using rsaꓸPublicKey = go.crypto.@internal.fips140.rsa_package.ΔPublicKey;
// </ImportedTypeAliases>

using go;
using static go.crypto.rsa_package;

// For encountered type alias declarations, e.g., `type Table = map[string]int`,
// go2cs code converter will generate a `global using` statement for the alias in
// the converted source, e.g.: `global using Table = go.map<go.@string, nint>;`.
// Although scope of `global using` is available to all files in the project, all
// converted Go code for the project targets the same package, so `global using`
// statements will effectively have package level scope.

// Additionally, `GoTypeAlias` attributes will be generated here for exported type
// aliases. This allows the type alias to be imported and used from other packages
// when referenced.

// <ExportedTypeAliases>
// </ExportedTypeAliases>

// As types are cast to interfaces in Go source code, the go2cs code converter
// will generate an assembly level `GoImplement` attribute for each unique cast.
// This allows the interface to be implemented in the C# source code using source
// code generation (see go2cs-gen). Resolving each duck-typed cast at compile time
// this way is what keeps startup free of reflection.

// <InterfaceImplementations>
// </InterfaceImplementations>

// <ImplicitConversions>
// </ImplicitConversions>

// This package's string literals are hoisted to `static readonly` fields (the standard
// library's form, or -hoist-literals), so no converted file passes a bare literal for
// go2cs-gen's literal interceptor to box. The stamp below tells the generator to skip
// the assembly.
[assembly: go.GoHoistedLiterals]

// The struct types whose fields this package reaches through a pointer, as Go spells it, `p.f`:
// go2cs-gen emits a field property on the pointer for each of their fields.
[assembly: go.GoFieldAccess(typeof(CRTValue))]
[assembly: go.GoFieldAccess(typeof(OAEPOptions))]
[assembly: go.GoFieldAccess(typeof(PKCS1v15DecryptOptions))]
[assembly: go.GoFieldAccess(typeof(PSSOptions))]
[assembly: go.GoFieldAccess(typeof(PrivateKey))]
[assembly: go.GoFieldAccess(typeof(PublicKey))]

// Go source positions are recorded here, one `GoPositionMap` attribute per converted
// source file in this compilation, so that `runtime.Caller` and the tracebacks built on it
// can name the GO file and line a frame was converted from rather than the emitted C# one.
// Each record carries the Go file's identity and an encoded C#-line to Go-line table
// TOGETHER: a frame either has a record and reports a position that exists in the Go tree,
// or has none - golib, the BCL and hand-written conversions - and reports its own C# position.

// <GoSourcePositionMaps>
[assembly: go.GoPositionMap("crypto/rsa/fips.go", "fips.cs", "ACtakqaigpQACBgADAKAgraCpoKCgpSUlJSAgqSClIKmgoKmgoKUlIKCxtiCyAADGAAJAoCCtoKCgpSAgqSmlICCpIKmgoKmgoKUlKSkAAMsABICAAIUAAkCgpSmwoCCtoKUgoKClIKClJSUlICCpIKUgqaCgpQABhoADAKC1qKAgraCgoSUgoKUgoKUpoKUgIKkgoK4goKmAAYeAA4CgoKClKaAgraCgoKUpoCCpIKmgoKUAAMWAAgCgoKClKaAgraCgoKUgIKkpoCCpIKmgoKUpoKUpKSUpoIACQaCgpSClIKUgpSClIKU5oKClICCpIKUgpQ=", "", "", "326=Size/1/1/1")]
[assembly: go.GoPositionMap("crypto/rsa/notboring.go", "notboring.cs", "AAsWgqSC")]
[assembly: go.GoPositionMap("crypto/rsa/pkcs1v15.go", "pkcs1v15.cs", "ACdkABECgqaAgraCgqaCgoKUlJSmgoKCgoKUgpSCgoKClKaCgpQAAhwADQKAgraCgoKUgoKUpoKClIKUAAJYACsCgIK2goKmgoKmpqaCggACEgALAoKmgoKCpoKCgoKUgoKmgoKUgoK4gtyUgoKCypSCgqiygoKmgoKCgri4")]
[assembly: go.GoPositionMap("crypto/rsa/rsa.go", "rsa.cs", "AER8AAog0qiSgoKUAB1EkqqigoKUgpSClIKCpqrSAAIWAAgCgIK23NKCppSClMiAgoKAgqSAgqSU6AAeQviCuIKUgpSCpoKClIKClIKUAAISwoKUgsqUgoKUgoKU1oKClAAFGgAKAoCCtoSCgpSCgoKCgoKCgoKCpgANHKaUgpSClIKmggAKFoKmgpSCABEkAAc0ABcCgpSCppSClIKmgpSmpoKCuJSCggALGIKUgoKCgpS4goKCyoKCgoKCgpS4poKClIKCgtiCgIK23Li4mKKCpoKmgpT2gpSClIKUgpSClIK4gqaClIKCgqaUgqaCgoKCgoL2gpSCgpSUgriCgpSCuIKUgpSCgqaCgoKClIKUgoKCpqamgoKClKaigpSCgpQ=", "", "", "288=bigIntEqualToBytes/2/5/1,bigIntEqualToBytes/3/5/2,bigIntEqualToBytes/4/5/3,bigIntEqualToBytes/5/5/4;415=SetBytes/1/7/2,SetBytes/2/7/5,SetBytes/3/7/7,SetBytes/4/7/8,SetBytes/5/7/12,SetBytes/6/7/13,SetBytes/7/7/14;627=Bytes/3/7/1,Bytes/4/7/1,Bytes/5/7/2,Bytes/6/7/2,Bytes/7/7/2;639=Bytes/3/4/1,Bytes/4/4/1")]
// </GoSourcePositionMaps>

namespace go.crypto;

[GoPackage("rsa")]
public static partial class rsa_package
{
    // C# nested types declared with no access modifier are always private, and the
    // Go type declarations in this package's converted sources are deliberately
    // bare so they read more like the original Go code. The real accessibility for
    // the types - public for a Go-exported name, internal otherwise - are defined
    // via declarations below.

    // <TypeAccessibility>
    public partial struct CRTValue {}
    public partial struct OAEPOptions {}
    public partial struct PKCS1v15DecryptOptions {}
    public partial struct PSSOptions {}
    public partial struct PrecomputedValues {}
    public partial struct PrivateKey {}
    public partial struct PublicKey {}
    // </TypeAccessibility>

    // Go initializes an imported package before the importing package, for every import
    // form - not only the blank one. .NET would never load an assembly nothing has touched
    // yet, so each import that initializes anything is forced below: once per assembly, and
    // ahead of this package's own `init` functions, which this file being the first compile
    // item of the project guarantees.

    // <ImportInitializers>
    [GoInit] internal static void initᴛᴛimportꓸcrypto() => builtin.initPackage(typeof(crypto_package));
    [GoInit] internal static void initᴛᴛimportꓸcryptoꓸinternalꓸboring() => builtin.initPackage(typeof(go.crypto.@internal.boring_package));
    [GoInit] internal static void initᴛᴛimportꓸcryptoꓸinternalꓸboringꓸbbig() => builtin.initPackage(typeof(go.crypto.@internal.boring.bbig_package));
    [GoInit] internal static void initᴛᴛimportꓸcryptoꓸinternalꓸfips140hash() => builtin.initPackage(typeof(go.crypto.@internal.fips140hash_package));
    [GoInit] internal static void initᴛᴛimportꓸcryptoꓸinternalꓸfips140only() => builtin.initPackage(typeof(go.crypto.@internal.fips140only_package));
    [GoInit] internal static void initᴛᴛimportꓸcryptoꓸinternalꓸfips140ꓸbigmod() => builtin.initPackage(typeof(go.crypto.@internal.fips140.bigmod_package));
    [GoInit] internal static void initᴛᴛimportꓸcryptoꓸinternalꓸfips140ꓸrsa() => builtin.initPackage(typeof(go.crypto.@internal.fips140.rsa_package));
    [GoInit] internal static void initᴛᴛimportꓸcryptoꓸinternalꓸrand() => builtin.initPackage(typeof(go.crypto.@internal.rand_package));
    [GoInit] internal static void initᴛᴛimportꓸcryptoꓸrand() => builtin.initPackage(typeof(go.crypto.rand_package));
    [GoInit] internal static void initᴛᴛimportꓸcryptoꓸsubtle() => builtin.initPackage(typeof(go.crypto.subtle_package));
    [GoInit] internal static void initᴛᴛimportꓸerrors() => builtin.initPackage(typeof(errors_package));
    [GoInit] internal static void initᴛᴛimportꓸfmt() => builtin.initPackage(typeof(fmt_package));
    [GoInit] internal static void initᴛᴛimportꓸhash() => builtin.initPackage(typeof(hash_package));
    [GoInit] internal static void initᴛᴛimportꓸinternalꓸgodebug() => builtin.initPackage(typeof(go.@internal.godebug_package));
    [GoInit] internal static void initᴛᴛimportꓸio() => builtin.initPackage(typeof(io_package));
    [GoInit] internal static void initᴛᴛimportꓸmath() => builtin.initPackage(typeof(math_package));
    [GoInit] internal static void initᴛᴛimportꓸmathꓸbig() => builtin.initPackage(typeof(go.math.big_package));
    // </ImportInitializers>
}
