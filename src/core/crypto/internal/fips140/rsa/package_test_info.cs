// go2cs metadata anchor for a production-reference test project: the test assembly
// REFERENCES the colocated production project instead of
// recompiling its sources, so the production assembly is the single identity for the
// production types and no production class partial may be declared here. The first —
// and only — class is the test metadata class the go2cs-gen generators anchor
// generated adapters and partials to.
global using static global::go.crypto.@internal.fips140.rsa_package;

// <ImportedTypeAliases>
global using bigmodꓸNat = go.crypto.@internal.fips140.bigmod_package.ΔNat;
global using bigꓸInt = go.math.big_package.ΔInt;
global using bigꓸRat = go.math.big_package.ΔRat;
global using cryptoꓸDecrypterOpts = object;
global using cryptoꓸPrivateKey = object;
global using cryptoꓸPublicKey = object;
global using osꓸDirEntry = go.io.fs_package.DirEntry;
global using osꓸFileInfo = go.io.fs_package.FileInfo;
global using osꓸFileMode = go.io.fs_package.FileMode;
global using osꓸPathError = go.io.fs_package.PathError;
global using osꓸSignal = go.os_package.ΔSignal;
// </ImportedTypeAliases>

using go;
using static global::go.crypto.@internal.fips140.rsa_internal_test_package;

// <ExportedTypeAliases>
[assembly: GoDynamicTypeLift("7374727563747b48617368416c676f726974686d2063727970746f2f783530392f706b69782e416c676f726974686d4964656e7469666965723b2048617368205b5d627974657d", "TestHashPrefixes_val")]
[assembly: GoTypeAlias("PublicKey", "ΔPublicKey")]
// </ExportedTypeAliases>

// <InterfaceImplementations>
[assembly: GoImplement<hash_package.Hash, go.crypto.@internal.fips140_package.Hash>]
[assembly: GoImplement<os_package.File, io_package.Reader>(Pointer = true)]
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
[assembly: go.GoPositionMap("crypto/internal/fips140/rsa/keygen_test.go", "keygen_test.cs", "ABgkgoKCloKSgoKCgoKCloKUlKSkxqSEgoKUgpaCgoKCloKCgoKWgoKCyLaAggAKCKKCgpaygoKCgoKCloKUpKSkhIKClIKWgoKChJSCgIK4goKClJSClICC2LaAgsiCgoKmgoKCpoKCgpSCgpQ=", "51-78:1;115-149:1")]
[assembly: go.GoPositionMap("crypto/internal/fips140/rsa/pkcs1v15_test.go", "pkcs1v15_test.cs", "ABUeggAlUIIABhSClIKCgg==", "", "", "62=Size/1/1/8")]
[assembly: go.GoPositionMap("crypto/internal/fips140/rsa/pkcs1v22_test.go", "pkcs1v22_test.cs", "AA8alAANHLgADiCCgoSCgpSCloCC")]
// </GoSourcePositionMaps>

namespace go.crypto.@internal.fips140;

[GoPackage("rsa")]
public static partial class rsa_internal_test_package
{
    // C# nested types declared with no access modifier are always private, and the
    // Go type declarations in this package's converted sources are deliberately
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
    [GoInit] internal static void initᴛᴛimportꓸbufio() => builtin.initPackage(typeof(bufio_package));
    [GoInit] internal static void initᴛᴛimportꓸbytes() => builtin.initPackage(typeof(bytes_package));
    [GoInit] internal static void initᴛᴛimportꓸcrypto() => builtin.initPackage(typeof(crypto_package));
    [GoInit] internal static void initᴛᴛimportꓸcryptoꓸinternalꓸfips140ꓸbigmod() => builtin.initPackage(typeof(go.crypto.@internal.fips140.bigmod_package));
    [GoInit] internal static void initᴛᴛimportꓸcryptoꓸsha1() => builtin.initPackage(typeof(go.crypto.sha1_package));
    [GoInit] internal static void initᴛᴛimportꓸcryptoꓸx509ꓸpkix() => builtin.initPackage(typeof(go.crypto.x509.pkix_package));
    [GoInit] internal static void initᴛᴛimportꓸencodingꓸasn1() => builtin.initPackage(typeof(encoding.asn1_package));
    [GoInit] internal static void initᴛᴛimportꓸencodingꓸhex() => builtin.initPackage(typeof(encoding.hex_package));
    [GoInit] internal static void initᴛᴛimportꓸfmt() => builtin.initPackage(typeof(fmt_package));
    [GoInit] internal static void initᴛᴛimportꓸmathꓸbig() => builtin.initPackage(typeof(math.big_package));
    [GoInit] internal static void initᴛᴛimportꓸos() => builtin.initPackage(typeof(os_package));
    [GoInit] internal static void initᴛᴛimportꓸstrings() => builtin.initPackage(typeof(strings_package));
    [GoInit] internal static void initᴛᴛimportꓸtesting() => builtin.initPackage(typeof(testing_package));
    // </ImportInitializers>
    // Go runs every `init` in the package under test - the production files' included -
    // before the first test. The production package is a REFERENCED assembly here, whose
    // module constructor .NET would not run until something in it is touched, so that
    // initialization is forced before anything else in this test module runs.
    [GoInit] internal static void initᴛᴛproduction() {
        builtin.initPackage(typeof(global::go.crypto.@internal.fips140.rsa_package));
    }
}
