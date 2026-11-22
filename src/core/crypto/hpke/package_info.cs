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
using cipher = go.crypto.cipher_package;
// </ImportedTypeAliases>

using go;
using static go.crypto.hpke_package;
using System.ComponentModel;
using System.Diagnostics;
using System.Runtime.CompilerServices;

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
// </ExportedTypeAliases>

// As types are cast to interfaces in Go source code, the go2cs code converter
// will generate an assembly level `GoImplement` attribute for each unique cast.
// This allows the interface to be implemented in the C# source code using source
// code generation (see go2cs-gen). Resolving each duck-typed cast at compile time
// this way is what keeps startup free of reflection.

// <InterfaceImplementations>
[assembly: GoImplement<aead, AEAD>(Pointer = true)]
[assembly: GoImplement<dhKEM, KEM>(Pointer = true)]
[assembly: GoImplement<dhKEMPrivateKey, PrivateKey>(Pointer = true)]
[assembly: GoImplement<dhKEMPublicKey, PublicKey>(Pointer = true)]
[assembly: GoImplement<exportOnlyAEAD, AEAD>]
[assembly: GoImplement<go.crypto.@internal.fips140.aes.gcm_package.GCMWithXORCounterNonce, go.crypto.cipher_package.AEAD>(Pointer = true)]
[assembly: GoImplement<hkdfKDF, KDF>(Pointer = true)]
[assembly: GoImplement<hybridKEM, KEM>(Pointer = true)]
[assembly: GoImplement<hybridPrivateKey, PrivateKey>(Pointer = true)]
[assembly: GoImplement<hybridPublicKey, PublicKey>(Pointer = true)]
[assembly: GoImplement<mlkemKEM, KEM>(Pointer = true)]
[assembly: GoImplement<mlkemPrivateKey, PrivateKey>(Pointer = true)]
[assembly: GoImplement<mlkemPublicKey, PublicKey>(Pointer = true)]
[assembly: GoImplement<shakeKDF, KDF>(Pointer = true)]
[assembly: GoImplement<unsupportedCurveKEM, KEM>]
// </InterfaceImplementations>

// <ImplicitConversions>
// </ImplicitConversions>

// This package's string literals are hoisted to `static readonly` fields (the standard
// library's form, or -hoist-literals), so no converted file passes a bare literal for
// go2cs-gen's literal interceptor to box. The stamp below tells the generator to skip
// the assembly.
[assembly: GoHoistedLiterals]

// The struct types whose fields this package reaches through a pointer, as Go spells it, `p.f`:
// go2cs-gen emits a field property on the pointer for each of their fields.
[assembly: GoFieldAccess(typeof(hybridKEM))]

// Go source positions are recorded here, one `GoPositionMap` attribute per converted
// source file in this compilation, so that `runtime.Caller` and the tracebacks built on it
// can name the GO file and line a frame was converted from rather than the emitted C# one.
// Each record carries the Go file's identity and an encoded C#-line to Go-line table
// TOGETHER: a frame either has a record and reports a position that exists in the Go tree,
// or has none - golib, the BCL and hand-written conversions - and reports its own C# position.

// <GoSourcePositionMaps>
[assembly: GoPositionMap("crypto/hpke/aead.go", "aead.cs", "ABc4wpSkpKSkuoCWgJaAnLAACBLu7u6C5oKClKaCpoLagqaCpoKmgg==")]
[assembly: GoPositionMap("crypto/hpke/aead_fips140v1.26.go", "aead_fips140v1.26.cs", "AA8egoKClA==")]
[assembly: GoPositionMap("crypto/hpke/hpke.go", "hpke.cs", "ADpWgpSCgoKClIKCgoKUlIKUgoKUgoKUuAAHEIKClIKClIKUgoKUgoKUgoKUgoKUgoKUuAAHIgALAoKClIKClAACFPKCgpSCgpQABhDSgpSCgqyygoKUgoKU6qKClAACENKClIKClILssoKClJKCgpSqooKUpoKCgoKUpoKCgoKCgg==", "71-73:1;114-116:2", "", "74=keySize/1/1/1,nonceSize/1/1/1,size/1/1/1")]
[assembly: GoPositionMap("crypto/hpke/kdf.go", "kdf.cs", "AB1CwpSkpKSkpLqAloCWgAAHEIKClIKmgqaC5oKmgoKCgoKCpoKCgoKCgoKokqiSAAgSgpSCpoKmgqaCgoKCgoKCgoKCguaC5oI=")]
[assembly: GoPositionMap("crypto/hpke/kem.go", "kem.cs", "ACtcwpSkpKSkpKSkpKQAMmSCgoKClKaCpoK2kpKSAAEUAAgClKSkpNrsgtSClIKUgpSClIIAByoADwKCgpTcgoKClKaCpoLusoKClIKUgoKUlIKCgoKUAAcsAA4CgoKU3IKCgpSmgoKClAAIBrSCgoKUgoKClJSCgoKClIKUgoKClJSmguYACRSCgpSCgoKCgpSmgtyCgoKUgoKUgg==", "", "", "408=PublicKey/1/1/2")]
[assembly: GoPositionMap("crypto/hpke/pq.go", "pq.cs", "ABcoAAwYtsyipgAKFrbKkqYACha2ypIADiKCpoKmgoKCgoKCggAQLAAPApSClKSClKSClKT4ooKUkoKUgpKUgpSmgqaCyrKCgpSClIKUgoKUgpSUgoKmgoIACTIADwKmgoKC5qKCpoKUgoKSgqaCgoKCkqSClLiClIKUpIKUpIKUpPiigoKClKaC5oKClKaCAAoOooKUgpKClIKSlIKUgoKUgpSCpri2tsqSpri2tsqSAAoWgqaCAAsmAAsClKSkuIKCgpSmgqaCpoKCgpQAByYACwKUpKS4goKClKaCgoKUpoKCgoKUpoLmgoaClKaC3II=", "173-175:1;194-196:1;204-206:2;273-275:1;343-345:1;350-352:2", "", "24=X25519/1/1/4;52=P256/1/1/3;77=P384/1/1/3;381=PublicKey/1/1/2,Encapsulator/1/1/3;611=Encapsulator/1/1/2")]
// </GoSourcePositionMaps>

namespace go.crypto;

using static go.crypto.hpke_package;
using cipher = go.crypto.cipher_package;

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
    internal partial struct aead {}
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
    public partial interface AEAD {}
    public partial interface KDF {}
    public partial interface KEM {}
    public partial interface PrivateKey {}
    public partial interface PublicKey {}
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

// A Go method is an extension method on its receiver type. The methods of one type are
// in a class of their own beside the package class, so that a method and a package-level
// type, constant or variable can share a name as they do in Go. Each class is declared
// below with the type it holds the methods of; its blocks are in the converted sources,
// in Go's declaration order.

// <MethodHolders>
[GoMethodHost(typeof(hpke_package.Recipient)), EditorBrowsable(EditorBrowsableState.Never)] public static partial class hpkeᴛRecipient;
[GoMethodHost(typeof(hpke_package.Sender)), EditorBrowsable(EditorBrowsableState.Never)] public static partial class hpkeᴛSender;
[GoMethodHost(typeof(hpke_package.aead)), EditorBrowsable(EditorBrowsableState.Never)] public static partial class hpkeᴛaead;
[GoMethodHost(typeof(hpke_package.context)), EditorBrowsable(EditorBrowsableState.Never)] public static partial class hpkeᴛcontext;
[GoMethodHost(typeof(hpke_package.dhKEM)), EditorBrowsable(EditorBrowsableState.Never)] public static partial class hpkeᴛdhKEM;
[GoMethodHost(typeof(hpke_package.dhKEMPrivateKey)), EditorBrowsable(EditorBrowsableState.Never)] public static partial class hpkeᴛdhKEMPrivateKey;
[GoMethodHost(typeof(hpke_package.dhKEMPublicKey)), EditorBrowsable(EditorBrowsableState.Never)] public static partial class hpkeᴛdhKEMPublicKey;
[GoMethodHost(typeof(hpke_package.exportOnlyAEAD)), EditorBrowsable(EditorBrowsableState.Never)] public static partial class hpkeᴛexportOnlyAEAD;
[GoMethodHost(typeof(hpke_package.hkdfKDF)), EditorBrowsable(EditorBrowsableState.Never)] public static partial class hpkeᴛhkdfKDF;
[GoMethodHost(typeof(hpke_package.hybridKEM)), EditorBrowsable(EditorBrowsableState.Never)] public static partial class hpkeᴛhybridKEM;
[GoMethodHost(typeof(hpke_package.hybridPrivateKey)), EditorBrowsable(EditorBrowsableState.Never)] public static partial class hpkeᴛhybridPrivateKey;
[GoMethodHost(typeof(hpke_package.hybridPublicKey)), EditorBrowsable(EditorBrowsableState.Never)] public static partial class hpkeᴛhybridPublicKey;
[GoMethodHost(typeof(hpke_package.mlkemKEM)), EditorBrowsable(EditorBrowsableState.Never)] public static partial class hpkeᴛmlkemKEM;
[GoMethodHost(typeof(hpke_package.mlkemPrivateKey)), EditorBrowsable(EditorBrowsableState.Never)] public static partial class hpkeᴛmlkemPrivateKey;
[GoMethodHost(typeof(hpke_package.mlkemPublicKey)), EditorBrowsable(EditorBrowsableState.Never)] public static partial class hpkeᴛmlkemPublicKey;
[GoMethodHost(typeof(hpke_package.shakeKDF)), EditorBrowsable(EditorBrowsableState.Never)] public static partial class hpkeᴛshakeKDF;
[GoMethodHost(typeof(hpke_package.unsupportedCurveKEM)), EditorBrowsable(EditorBrowsableState.Never)] public static partial class hpkeᴛunsupportedCurveKEM;
// </MethodHolders>

// A Go method's body sits in its package class beside its type, named with a trailing U+2054 (`String⁔`),
// which no Go identifier can spell. The Go-named extension method its holder declares is a one-line
// forwarder to that body: every call, interface and method set reads the forwarder, and the runtime names
// the body's frame as the Go method.
// <MethodForwarders>
partial class hpkeᴛRecipient {
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] public static (slice<byte>, error) Open(this ref Recipient r, slice<byte> aad, slice<byte> ciphertext) => hpke_package.Open⁔(ref r, aad, ciphertext);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] public static (slice<byte>, error) Export(this ref Recipient r, @string exporterContext, nint length) => hpke_package.Export⁔(ref r, exporterContext, length);
}
partial class hpkeᴛSender {
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] public static (slice<byte>, error) Seal(this ref Sender s, slice<byte> aad, slice<byte> plaintext) => hpke_package.Seal⁔(ref s, aad, plaintext);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] public static (slice<byte>, error) Export(this ref Sender s, @string exporterContext, nint length) => hpke_package.Export⁔(ref s, exporterContext, length);
}
partial class hpkeᴛaead {
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static uint16 ID(this ref aead a) => hpke_package.ID⁔(ref a);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static (cipher.AEAD, error) aead(this ref aead a, slice<byte> key) => hpke_package.aead⁔(ref a, key);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static nint keySize(this ref aead a) => hpke_package.keySize⁔(ref a);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static nint nonceSize(this ref aead a) => hpke_package.nonceSize⁔(ref a);
}
partial class hpkeᴛcontext {
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static slice<byte> nextNonce(this ref context ctx) => hpke_package.nextNonce⁔(ref ctx);
}
partial class hpkeᴛdhKEM {
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static (slice<byte>, error) extractAndExpand(this ref dhKEM kem, slice<byte> dhKey, slice<byte> kemContext) => hpke_package.extractAndExpand⁔(ref kem, dhKey, kemContext);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static uint16 ID(this ref dhKEM kem) => hpke_package.ID⁔(ref kem);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static nint encSize(this ref dhKEM kem) => hpke_package.encSize⁔(ref kem);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static (PublicKey, error) NewPublicKey(this ref dhKEM kem, slice<byte> data) => hpke_package.NewPublicKey⁔(ref kem, data);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static (PrivateKey, error) GenerateKey(this ref dhKEM kem) => hpke_package.GenerateKey⁔(ref kem);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static (PrivateKey, error) NewPrivateKey(this ref dhKEM kem, slice<byte> ikm) => hpke_package.NewPrivateKey⁔(ref kem, ikm);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static (PrivateKey, error) DeriveKeyPair(this ж<dhKEM> Ꮡkem, slice<byte> ikm) => hpke_package.DeriveKeyPair⁔(Ꮡkem, ikm);
}
partial class hpkeᴛdhKEMPrivateKey {
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static KEM KEM(this ref dhKEMPrivateKey k) => hpke_package.KEM⁔(ref k);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static (slice<byte>, error) Bytes(this ref dhKEMPrivateKey k) => hpke_package.Bytes⁔(ref k);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static PublicKey PublicKey(this ref dhKEMPrivateKey k) => hpke_package.PublicKey⁔(ref k);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static (slice<byte>, error) decap(this ref dhKEMPrivateKey k, slice<byte> encPubEph) => hpke_package.decap⁔(ref k, encPubEph);
}
partial class hpkeᴛdhKEMPublicKey {
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static KEM KEM(this ref dhKEMPublicKey pk) => hpke_package.KEM⁔(ref pk);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static slice<byte> Bytes(this ref dhKEMPublicKey pk) => hpke_package.Bytes⁔(ref pk);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static (slice<byte> sharedSecret, slice<byte> encapPub, error err) encap(this ref dhKEMPublicKey pk) => hpke_package.encap⁔(ref pk);
}
partial class hpkeᴛexportOnlyAEAD {
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static uint16 ID(this exportOnlyAEAD _) => hpke_package.ID⁔(_);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static (cipher.AEAD, error) aead(this exportOnlyAEAD _, slice<byte> key) => hpke_package.aead⁔(_, key);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static nint keySize(this exportOnlyAEAD _) => hpke_package.keySize⁔(_);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static nint nonceSize(this exportOnlyAEAD _) => hpke_package.nonceSize⁔(_);
}
partial class hpkeᴛhkdfKDF {
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static uint16 ID(this ref hkdfKDF kdf) => hpke_package.ID⁔(ref kdf);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static nint size(this ref hkdfKDF kdf) => hpke_package.size⁔(ref kdf);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static bool oneStage(this ref hkdfKDF kdf) => hpke_package.oneStage⁔(ref kdf);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static (slice<byte>, error) labeledDerive(this ref hkdfKDF kdf, slice<byte> _Δp1, slice<byte> _Δp2, @string _Δp3, slice<byte> _Δp4, uint16 _Δp5) => hpke_package.labeledDerive⁔(ref kdf, _Δp1, _Δp2, _Δp3, _Δp4, _Δp5);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static (slice<byte>, error) labeledExtract(this ref hkdfKDF kdf, slice<byte> suiteID, slice<byte> salt, @string label, slice<byte> inputKey) => hpke_package.labeledExtract⁔(ref kdf, suiteID, salt, label, inputKey);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static (slice<byte>, error) labeledExpand(this ref hkdfKDF kdf, slice<byte> suiteID, slice<byte> randomKey, @string label, slice<byte> info, uint16 length) => hpke_package.labeledExpand⁔(ref kdf, suiteID, randomKey, label, info, length);
}
partial class hpkeᴛhybridKEM {
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static uint16 ID(this ref hybridKEM kem) => hpke_package.ID⁔(ref kem);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static nint encSize(this ref hybridKEM kem) => hpke_package.encSize⁔(ref kem);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static slice<byte> sharedSecret(this ref hybridKEM kem, slice<byte> ssPQ, slice<byte> ssT, slice<byte> ctT, slice<byte> ekT) => hpke_package.sharedSecret⁔(ref kem, ssPQ, ssT, ctT, ekT);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static (PublicKey, error) NewPublicKey(this ж<hybridKEM> Ꮡkem, slice<byte> data) => hpke_package.NewPublicKey⁔(Ꮡkem, data);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static (PrivateKey, error) GenerateKey(this ж<hybridKEM> Ꮡkem) => hpke_package.GenerateKey⁔(Ꮡkem);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static (PrivateKey, error) NewPrivateKey(this ж<hybridKEM> Ꮡkem, slice<byte> priv) => hpke_package.NewPrivateKey⁔(Ꮡkem, priv);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static (PrivateKey, error) DeriveKeyPair(this ж<hybridKEM> Ꮡkem, slice<byte> ikm) => hpke_package.DeriveKeyPair⁔(Ꮡkem, ikm);
}
partial class hpkeᴛhybridPrivateKey {
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static KEM KEM(this ref hybridPrivateKey k) => hpke_package.KEM⁔(ref k);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static (slice<byte>, error) Bytes(this ref hybridPrivateKey k) => hpke_package.Bytes⁔(ref k);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static PublicKey PublicKey(this ref hybridPrivateKey k) => hpke_package.PublicKey⁔(ref k);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static (slice<byte>, error) decap(this ж<hybridPrivateKey> Ꮡk, slice<byte> enc) => hpke_package.decap⁔(Ꮡk, enc);
}
partial class hpkeᴛhybridPublicKey {
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static KEM KEM(this ref hybridPublicKey pk) => hpke_package.KEM⁔(ref pk);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static slice<byte> Bytes(this ref hybridPublicKey pk) => hpke_package.Bytes⁔(ref pk);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static (slice<byte> sharedSecret, slice<byte> encapPub, error err) encap(this ж<hybridPublicKey> Ꮡpk) => hpke_package.encap⁔(Ꮡpk);
}
partial class hpkeᴛmlkemKEM {
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static uint16 ID(this ref mlkemKEM kem) => hpke_package.ID⁔(ref kem);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static nint encSize(this ref mlkemKEM kem) => hpke_package.encSize⁔(ref kem);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static (PublicKey, error) NewPublicKey(this ref mlkemKEM kem, slice<byte> data) => hpke_package.NewPublicKey⁔(ref kem, data);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static (PrivateKey, error) GenerateKey(this ref mlkemKEM kem) => hpke_package.GenerateKey⁔(ref kem);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static (PrivateKey, error) NewPrivateKey(this ref mlkemKEM kem, slice<byte> priv) => hpke_package.NewPrivateKey⁔(ref kem, priv);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static (PrivateKey, error) DeriveKeyPair(this ref mlkemKEM kem, slice<byte> ikm) => hpke_package.DeriveKeyPair⁔(ref kem, ikm);
}
partial class hpkeᴛmlkemPrivateKey {
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static KEM KEM(this ref mlkemPrivateKey k) => hpke_package.KEM⁔(ref k);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static (slice<byte>, error) Bytes(this ref mlkemPrivateKey k) => hpke_package.Bytes⁔(ref k);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static PublicKey PublicKey(this ref mlkemPrivateKey k) => hpke_package.PublicKey⁔(ref k);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static (slice<byte>, error) decap(this ref mlkemPrivateKey k, slice<byte> enc) => hpke_package.decap⁔(ref k, enc);
}
partial class hpkeᴛmlkemPublicKey {
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static KEM KEM(this ref mlkemPublicKey pk) => hpke_package.KEM⁔(ref pk);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static slice<byte> Bytes(this ref mlkemPublicKey pk) => hpke_package.Bytes⁔(ref pk);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static (slice<byte> sharedSecret, slice<byte> encapPub, error err) encap(this ref mlkemPublicKey pk) => hpke_package.encap⁔(ref pk);
}
partial class hpkeᴛshakeKDF {
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static uint16 ID(this ref shakeKDF kdf) => hpke_package.ID⁔(ref kdf);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static nint size(this ref shakeKDF kdf) => hpke_package.size⁔(ref kdf);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static bool oneStage(this ref shakeKDF kdf) => hpke_package.oneStage⁔(ref kdf);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static (slice<byte>, error) labeledDerive(this ref shakeKDF kdf, slice<byte> suiteID, slice<byte> inputKey, @string label, slice<byte> context, uint16 length) => hpke_package.labeledDerive⁔(ref kdf, suiteID, inputKey, label, context, length);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static (slice<byte>, error) labeledExtract(this ref shakeKDF kdf, slice<byte> _Δp1, slice<byte> _Δp2, @string _Δp3, slice<byte> _Δp4) => hpke_package.labeledExtract⁔(ref kdf, _Δp1, _Δp2, _Δp3, _Δp4);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static (slice<byte>, error) labeledExpand(this ref shakeKDF kdf, slice<byte> _Δp1, slice<byte> _Δp2, @string _Δp3, slice<byte> _Δp4, uint16 _Δp5) => hpke_package.labeledExpand⁔(ref kdf, _Δp1, _Δp2, _Δp3, _Δp4, _Δp5);
}
partial class hpkeᴛunsupportedCurveKEM {
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static uint16 ID(this unsupportedCurveKEM _) => hpke_package.ID⁔(_);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static (PrivateKey, error) GenerateKey(this unsupportedCurveKEM _) => hpke_package.GenerateKey⁔(_);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static (PublicKey, error) NewPublicKey(this unsupportedCurveKEM _Δp0, slice<byte> _Δp1) => hpke_package.NewPublicKey⁔(_Δp0, _Δp1);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static (PrivateKey, error) NewPrivateKey(this unsupportedCurveKEM _Δp0, slice<byte> _Δp1) => hpke_package.NewPrivateKey⁔(_Δp0, _Δp1);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static (PrivateKey, error) DeriveKeyPair(this unsupportedCurveKEM _Δp0, slice<byte> _Δp1) => hpke_package.DeriveKeyPair⁔(_Δp0, _Δp1);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static nint encSize(this unsupportedCurveKEM _) => hpke_package.encSize⁔(_);
}
// </MethodForwarders>
