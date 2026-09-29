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
using aes = go.crypto.@internal.fips140.aes_package;
// </ImportedTypeAliases>

using go;
using static go.crypto.@internal.fips140.aes.gcm_package;

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
[assembly: GoImplicitConv<aes.Block, ж<aes.Block>>(Indirect = true)]
// </ImplicitConversions>

// Go source positions are recorded here, one `GoPositionMap` attribute per converted
// source file in this compilation, so that `runtime.Caller` and the tracebacks built on it
// can name the GO file and line a frame was converted from rather than the emitted C# one.
// Each record carries the Go file's identity and an encoded C#-line to Go-line table
// TOGETHER: a frame either has a record and reports a position that exists in the Go tree,
// or has none - golib, the BCL and hand-written conversions - and reports its own C# position.

// <GoSourcePositionMaps>
[assembly: go.GoPositionMap("crypto/internal/fips140/aes/gcm/cast.go", "cast.cs", "AA4cuIK4uNyCgpSCgpQ=", "18-42:1")]
[assembly: go.GoPositionMap("crypto/internal/fips140/aes/gcm/cmac.go", "cmac.cs", "ABgwooKCpqKCgoSCgqaigoKClIKCgpSCgpSUgpSUgoKClKiygoKU")]
[assembly: go.GoPositionMap("crypto/internal/fips140/aes/gcm/ctrkdf.go", "ctrkdf.cs", "ABo2kqiygoSCgoSChIKEgoI=")]
[assembly: go.GoPositionMap("crypto/internal/fips140/aes/gcm/gcm.go", "gcm.cs", "ABYslP7ygpSClIKUgoKCggAHFIKmgqaCgqaigpSClIKWgoKUgpaCppSigriCloKUgpaCgpSCloKAyoKkrvKAgpSCpII=")]
[assembly: go.GoPositionMap("crypto/internal/fips140/aes/gcm/gcm_generic.go", "gcm_generic.cs", "AA0agqKCgoSEgoKmoqKCgoSChIKCgpaErAALDoKClIKCvtKEgoKEgoKWgoKCvMKCqsKCgoKCgoI=")]
[assembly: go.GoPositionMap("crypto/internal/fips140/aes/gcm/gcm_noasm.go", "gcm_noasm.cs", "AAgS2KSCpoI=")]
[assembly: go.GoPositionMap("crypto/internal/fips140/aes/gcm/gcm_nonces.go", "gcm_nonces.cs", "ABEwAAgCgpSClIKUgpSClIKCAAIS4oKClAAKFoCkgKSigpaClIKClIKUloKUgpSEgqaCggACENKCgpQABxCApICkooKWloKUgpSEgqaCgqqigoKUAAkUgKSApKKCloKUgpSWgpSClISCpoKCrsKCgpQACRSApICkooKWgpSClJaClIKUhIKmgoI=")]
[assembly: go.GoPositionMap("crypto/internal/fips140/aes/gcm/ghash.go", "ghash.cs", "ABg20oKCgq4ACAb+uISCgpaCgpaCqJKCgqikqLKWgoIABxKClqbMkoSCgoK6goKCgoKohIKCqKqigoKCgryigoSCgoI=")]
// </GoSourcePositionMaps>

namespace go.crypto.@internal.fips140.aes;

[GoPackage("gcm")]
public static partial class gcm_package
{
    // C# nested types declared with no access modifier are always private, and the
    // `[GoType]` declarations in this package's converted sources are deliberately
    // bare so they read more like the original Go code. The real accessibility for
    // the types - public for a Go-exported name, internal otherwise - are defined
    // via declarations below.

    // <TypeAccessibility>
    internal partial struct gcmFieldElement {}
    internal partial struct gcmPlatformData {}
    [GoValueClone("k1", "k2")] public partial struct CMAC {}
    [GoValueClone("mac")] public partial struct CounterKDF {}
    public partial struct GCM {}
    public partial struct GCMForSSH {}
    public partial struct GCMForTLS12 {}
    public partial struct GCMForTLS13 {}
    public partial struct GCMWithCounterNonce {}
    // </TypeAccessibility>

    // Go initializes an imported package before the importing package, for every import
    // form - not only the blank one. .NET would never load an assembly nothing has touched
    // yet, so each import that initializes anything is forced below: once per assembly, and
    // ahead of this package's own `init` functions, which this file being the first compile
    // item of the project guarantees.

    // <ImportInitializers>
    [GoInit] internal static void initᴛᴛimportꓸcryptoꓸinternalꓸfips140() => builtin.initPackage(typeof(go.crypto.@internal.fips140_package));
    [GoInit] internal static void initᴛᴛimportꓸcryptoꓸinternalꓸfips140ꓸaes() => builtin.initPackage(typeof(go.crypto.@internal.fips140.aes_package));
    [GoInit] internal static void initᴛᴛimportꓸcryptoꓸinternalꓸfips140ꓸcheck() => builtin.initPackage(typeof(go.crypto.@internal.fips140.check_package));
    [GoInit] internal static void initᴛᴛimportꓸcryptoꓸinternalꓸfips140ꓸdrbg() => builtin.initPackage(typeof(go.crypto.@internal.fips140.drbg_package));
    [GoInit] internal static void initᴛᴛimportꓸcryptoꓸinternalꓸfips140ꓸsubtle() => builtin.initPackage(typeof(go.crypto.@internal.fips140.subtle_package));
    [GoInit] internal static void initᴛᴛimportꓸerrors() => builtin.initPackage(typeof(errors_package));
    [GoInit] internal static void initᴛᴛimportꓸmath() => builtin.initPackage(typeof(math_package));
    // </ImportInitializers>
}
