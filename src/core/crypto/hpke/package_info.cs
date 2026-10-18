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
using static go.crypto.hpke_package;

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
[assembly: GoDynamicTypeLift("696e746572666163657b42797465732829205b5d627974657d", "Bytes_type")]
[assembly: GoTypeAlias("KEM", "ΔKEM")]
[assembly: GoTypeAlias("PublicKey", "ΔPublicKey")]
// </ExportedTypeAliases>

// As types are cast to interfaces in Go source code, the go2cs code converter
// will generate an assembly level `GoImplement` attribute for each unique cast.
// This allows the interface to be implemented in the C# source code using source
// code generation (see go2cs-gen). Resolving each duck-typed cast at compile time
// this way is what keeps startup free of reflection.

// <InterfaceImplementations>
[assembly: GoImplement<dhKEM, ΔKEM>(Pointer = true)]
[assembly: GoImplement<dhKEMPrivateKey, PrivateKey>(Pointer = true)]
[assembly: GoImplement<dhKEMPublicKey, ΔPublicKey>(Pointer = true)]
[assembly: GoImplement<exportOnlyAEAD, AEAD>]
[assembly: GoImplement<go.crypto.@internal.fips140.aes.gcm_package.GCMWithXORCounterNonce, go.crypto.cipher_package.AEAD>(Pointer = true)]
[assembly: GoImplement<hkdfKDF, KDF>(Pointer = true)]
[assembly: GoImplement<hybridKEM, ΔKEM>(Pointer = true)]
[assembly: GoImplement<hybridPrivateKey, PrivateKey>(Pointer = true)]
[assembly: GoImplement<hybridPublicKey, ΔPublicKey>(Pointer = true)]
[assembly: GoImplement<mlkemKEM, ΔKEM>(Pointer = true)]
[assembly: GoImplement<mlkemPrivateKey, PrivateKey>(Pointer = true)]
[assembly: GoImplement<mlkemPublicKey, ΔPublicKey>(Pointer = true)]
[assembly: GoImplement<shakeKDF, KDF>(Pointer = true)]
[assembly: GoImplement<unsupportedCurveKEM, ΔKEM>]
[assembly: GoImplement<Δaead, AEAD>(Pointer = true)]
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
[assembly: go.GoPositionMap("crypto/hpke/aead.go", "aead.cs", "ABc4wpSkpKSkupCmkKaQrMAACRLu7u6C1oKClKaCpoLagqaCpoKmgg==")]
[assembly: go.GoPositionMap("crypto/hpke/aead_fips140v1.26.go", "aead_fips140v1.26.cs", "AA8egoKClA==")]
[assembly: go.GoPositionMap("crypto/hpke/hpke.go", "hpke.cs", "ADNWgpSCgoKClIKCgoKUlIKUgoKUgoKUuAAHEIKClIKClIKUgoKUgoKUgoKUgoKUgoKUuAAHIgALAoKClIKClAACFPKCgpSCgpQABRDSgpSCgqyygoKUgoKU2qKClAACENKClIKClILcsoKClJKCgpSqooKUpoKCgoKUpoKCgoKCgg==", "71-73:1;114-116:2", "", "67=keySize/1/1/1,nonceSize/1/1/1,size/1/1/1")]
[assembly: go.GoPositionMap("crypto/hpke/kdf.go", "kdf.cs", "AB1CwpSkpKSkpLqQppCmkAAIEJKSlIKmgqaC1oKmgoKCgoKCpoKCgoKCgoKokqiSAAgSkpSCpoKmgqaCgoKCgoKCgoKCgtaC1oI=")]
[assembly: go.GoPositionMap("crypto/hpke/kem.go", "kem.cs", "ACpcwpSkpKSkpKSkpKQAMGSCgoKClKaCpoK2oqKiAAEUAAgClKSkpNrsgtSCpIKkgqSCpIIAByoADwKCgpTcgoKClKaCpoLusoKClIKUgoKUlIKCgoKUAAcsAA4CgoKU3IKCgpSmgoKClOa0goKClIKCgpSUgoKCgpSClIKCgpSUpoLWAAkUgoKUgoKCgoKUpoLcgoKClIKClII=", "", "", "409=PublicKey/1/1/2")]
[assembly: go.GoPositionMap("crypto/hpke/pq.go", "pq.cs", "ABcoAAoYtsyipgAIFrbKkqYACBa2ypIADiKCpoKmgoKCgoKCggANLAAPApSAgqSkgIKkpICCpKToooKUkoKUgpKUgpSmgqaCyrKCgpSClIKUgoKUgpSUgoKmgoIACTIADwKmgoKC1qKCpoKUgoKSgqaCgoKCkqSClLiClICCpKSAgqSkgIKkpOiigoKClKaC1oKClKaCAAkOooKUgpKClIKSlIKUgoKUgpSCpri2tsqSpri2tsqSAAoWgqaCAAomAAsClKSkuIKCgpSmgqaCpoKCgpQAByYACwKUpKS4goKClKaCgoKUpoKCgoKUpoLmgoaClKaC3II=", "173-175:1;194-196:1;204-206:2;273-275:1;343-345:1;350-352:2", "", "24=X25519/1/1/4;50=P256/1/1/3;73=P384/1/1/3;380=PublicKey/1/1/2,Encapsulator/1/1/3;608=Encapsulator/1/1/2")]
// </GoSourcePositionMaps>

namespace go.crypto;

[GoPackage("hpke")]
public static partial class hpke_package
{
    // C# nested types declared with no access modifier are always private, and the
    // Go type declarations in this package's converted sources are deliberately
    // bare so they read more like the original Go code. The real accessibility for
    // the types - public for a Go-exported name, internal otherwise - are defined
    // via declarations below.

    // <TypeAccessibility>
    internal partial interface Bytes_type {}
    internal partial struct context {}
    internal partial struct dhKEM {}
    internal partial struct dhKEMPrivateKey {}
    internal partial struct dhKEMPublicKey {}
    internal partial struct exportOnlyAEAD {}
    internal partial struct hkdfKDF {}
    internal partial struct hybridKEM {}
    internal partial struct hybridPrivateKey {}
    internal partial struct hybridPublicKey {}
    internal partial struct mlkemKEM {}
    internal partial struct mlkemPrivateKey {}
    internal partial struct mlkemPublicKey {}
    internal partial struct shakeKDF {}
    internal partial struct unsupportedCurveKEM {}
    internal partial struct Δaead {}
    public partial interface AEAD {}
    public partial interface KDF {}
    public partial interface PrivateKey {}
    public partial interface ΔKEM {}
    public partial interface ΔPublicKey {}
    public partial struct Recipient {}
    public partial struct Sender {}
    // </TypeAccessibility>

    // Go initializes an imported package before the importing package, for every import
    // form - not only the blank one. .NET would never load an assembly nothing has touched
    // yet, so each import that initializes anything is forced below: once per assembly, and
    // ahead of this package's own `init` functions, which this file being the first compile
    // item of the project guarantees.

    // <ImportInitializers>
    [GoInit] internal static void initᴛᴛimportꓸbytes() => builtin.initPackage(typeof(bytes_package));
    [GoInit] internal static void initᴛᴛimportꓸcrypto() => builtin.initPackage(typeof(crypto_package));
    [GoInit] internal static void initᴛᴛimportꓸcryptoꓸcipher() => builtin.initPackage(typeof(go.crypto.cipher_package));
    [GoInit] internal static void initᴛᴛimportꓸcryptoꓸecdh() => builtin.initPackage(typeof(go.crypto.ecdh_package));
    [GoInit] internal static void initᴛᴛimportꓸcryptoꓸfips140() => builtin.initPackage(typeof(go.crypto.fips140_package));
    [GoInit] internal static void initᴛᴛimportꓸcryptoꓸhkdf() => builtin.initPackage(typeof(go.crypto.hkdf_package));
    [GoInit] internal static void initᴛᴛimportꓸcryptoꓸinternalꓸfips140ꓸaes() => builtin.initPackage(typeof(go.crypto.@internal.fips140.aes_package));
    [GoInit] internal static void initᴛᴛimportꓸcryptoꓸinternalꓸfips140ꓸaesꓸgcm() => builtin.initPackage(typeof(go.crypto.@internal.fips140.aes.gcm_package));
    [GoInit] internal static void initᴛᴛimportꓸcryptoꓸinternalꓸfips140ꓸdrbg() => builtin.initPackage(typeof(go.crypto.@internal.fips140.drbg_package));
    [GoInit] internal static void initᴛᴛimportꓸcryptoꓸinternalꓸrand() => builtin.initPackage(typeof(go.crypto.@internal.rand_package));
    [GoInit] internal static void initᴛᴛimportꓸcryptoꓸmlkem() => builtin.initPackage(typeof(go.crypto.mlkem_package));
    [GoInit] internal static void initᴛᴛimportꓸcryptoꓸsha256() => builtin.initPackage(typeof(go.crypto.sha256_package));
    [GoInit] internal static void initᴛᴛimportꓸcryptoꓸsha3() => builtin.initPackage(typeof(go.crypto.sha3_package));
    [GoInit] internal static void initᴛᴛimportꓸcryptoꓸsha512() => builtin.initPackage(typeof(go.crypto.sha512_package));
    [GoInit] internal static void initᴛᴛimportꓸerrors() => builtin.initPackage(typeof(errors_package));
    [GoInit] internal static void initᴛᴛimportꓸfmt() => builtin.initPackage(typeof(fmt_package));
    [GoInit] internal static void initᴛᴛimportꓸhash() => builtin.initPackage(typeof(hash_package));
    [GoInit] internal static void initᴛᴛimportꓸslices() => builtin.initPackage(typeof(slices_package));
    [GoInit] internal static void initᴛᴛimportꓸvendorꓸgolang_orgꓸxꓸcryptoꓸchacha20poly1305() => builtin.initPackage(typeof(vendor.golang.org.x.crypto.chacha20poly1305_package));
    // </ImportInitializers>
}
