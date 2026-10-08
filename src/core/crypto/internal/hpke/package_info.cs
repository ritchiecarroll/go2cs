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
global using cryptoꓸDecrypterOpts = object;
global using cryptoꓸPrivateKey = object;
global using cryptoꓸPublicKey = object;
global using ecdhꓸCurve = go.crypto.ecdh_package.ΔCurve;
global using ecdhꓸPublicKey = go.crypto.ecdh_package.ΔPublicKey;
// </ImportedTypeAliases>

using go;
using static go.crypto.@internal.hpke_package;

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
[assembly: GoDynamicTypeLift("7374727563747b63757276652063727970746f2f656364682e43757276653b20686173682063727970746f2e486173683b206e5365637265742075696e7431367d", "SupportedKEMsᴛ1")]
[assembly: GoDynamicTypeLift("7374727563747b6b657953697a6520696e743b206e6f6e636553697a6520696e743b20616561642066756e63285b5d6279746529202863727970746f2f6369706865722e414541442c206572726f72297d", "SupportedAEADsᴛ1")]
// </ExportedTypeAliases>

// As types are cast to interfaces in Go source code, the go2cs code converter
// will generate an assembly level `GoImplement` attribute for each unique cast.
// This allows the interface to be implemented in the C# source code using source
// code generation (see go2cs-gen). Resolving each duck-typed cast at compile time
// this way is what keeps startup free of reflection.

// <InterfaceImplementations>
[assembly: GoImplement<hash_package.Hash, go.crypto.@internal.fips140_package.Hash>]
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
[assembly: go.GoPositionMap("crypto/internal/hpke/hpke.go", "hpke.cs", "ACI6ooKCgoKCtqKCgoKCgoIAFiAABhKCgoKUAAsQooKmwoKClJSClIKClISChKaygoKUgoKUhAAUMoKCgpQADxYACh4ADQqChIKClISCgpaCgoKEhJKSlIKClgAJFIKCgpSCgpaCgpamgoKClIKCloKClqaCgoKUpqaClKaCgoKmgoKClIKmgoKCgoKC1oKCgpSmgoKClO6CgqaCpoKCgoI=", "", "35=crypto.Hash.New;45=crypto.Hash.New", "79=X25519/1/1/6;91=BEAppendUint16/1/1/3")]
// </GoSourcePositionMaps>

namespace go.crypto.@internal;

[GoPackage("hpke")]
public static partial class hpke_package
{
    // C# nested types declared with no access modifier are always private, and the
    // Go type declarations in this package's converted sources are deliberately
    // bare so they read more like the original Go code. The real accessibility for
    // the types - public for a Go-exported name, internal otherwise - are defined
    // via declarations below.

    // <TypeAccessibility>
    internal partial struct context {}
    internal partial struct dhKEM {}
    internal partial struct uint128 {}
    public partial struct AEADID {}
    public partial struct KDFID {}
    public partial struct KemID {}
    public partial struct Receipient {}
    public partial struct Sender {}
    public partial struct SupportedAEADsᴛ1 {}
    public partial struct SupportedKEMsᴛ1 {}
    public partial struct hkdfKDF {}
    // </TypeAccessibility>

    // Go initializes an imported package before the importing package, for every import
    // form - not only the blank one. .NET would never load an assembly nothing has touched
    // yet, so each import that initializes anything is forced below: once per assembly, and
    // ahead of this package's own `init` functions, which this file being the first compile
    // item of the project guarantees.

    // <ImportInitializers>
    [GoInit] internal static void initᴛᴛimportꓸcrypto() => builtin.initPackage(typeof(crypto_package));
    [GoInit] internal static void initᴛᴛimportꓸcryptoꓸaes() => builtin.initPackage(typeof(go.crypto.aes_package));
    [GoInit] internal static void initᴛᴛimportꓸcryptoꓸcipher() => builtin.initPackage(typeof(go.crypto.cipher_package));
    [GoInit] internal static void initᴛᴛimportꓸcryptoꓸecdh() => builtin.initPackage(typeof(go.crypto.ecdh_package));
    [GoInit] internal static void initᴛᴛimportꓸcryptoꓸinternalꓸfips140ꓸhkdf() => builtin.initPackage(typeof(go.crypto.@internal.fips140.hkdf_package));
    [GoInit] internal static void initᴛᴛimportꓸcryptoꓸrand() => builtin.initPackage(typeof(go.crypto.rand_package));
    [GoInit] internal static void initᴛᴛimportꓸerrors() => builtin.initPackage(typeof(errors_package));
    [GoInit] internal static void initᴛᴛimportꓸmathꓸbits() => builtin.initPackage(typeof(math.bits_package));
    [GoInit] internal static void initᴛᴛimportꓸvendorꓸgolang_orgꓸxꓸcryptoꓸchacha20poly1305() => builtin.initPackage(typeof(vendor.golang.org.x.crypto.chacha20poly1305_package));
    // </ImportInitializers>
}
