// go2cs metadata anchor for a production-reference test project: the test assembly
// REFERENCES the colocated production project instead of
// recompiling its sources, so the production assembly is the single identity for the
// production types and no production class partial may be declared here. The first —
// and only — class is the test metadata class the go2cs-gen generators anchor
// generated adapters and partials to.
global using static global::go.crypto.rsa_package;
global using static global::go.crypto.rsa_internal_test_package;

// <ImportedTypeAliases>
global using bigmodꓸNat = go.crypto.@internal.fips140.bigmod_package.ΔNat;
global using bigꓸInt = go.math.big_package.ΔInt;
global using bigꓸRat = go.math.big_package.ΔRat;
global using cryptoꓸDecrypterOpts = object;
global using cryptoꓸPrivateKey = object;
global using cryptoꓸPublicKey = object;
global using flagꓸErrorHandling = go.flag_package.ΔErrorHandling;
global using osꓸDirEntry = go.io.fs_package.DirEntry;
global using osꓸFileInfo = go.io.fs_package.FileInfo;
global using osꓸFileMode = go.io.fs_package.FileMode;
global using osꓸPathError = go.io.fs_package.PathError;
global using osꓸSignal = go.os_package.ΔSignal;
global using rsaꓸPublicKey = go.crypto.@internal.fips140.rsa_package.ΔPublicKey;
// </ImportedTypeAliases>

using go;
using static global::go.crypto.rsa_test_package;

// <ExportedTypeAliases>
// </ExportedTypeAliases>

// <InterfaceImplementations>
[assembly: GoImplement<bufio_package.Reader, io_package.Reader>(Pointer = true)]
[assembly: GoImplement<bytes_package.Reader, io_package.Reader>(Pointer = true)]
[assembly: GoImplement<go.crypto.rsa_package.PSSOptions, crypto_package.SignerOpts>(Pointer = true)]
[assembly: GoImplement<go.crypto.rsa_package.PrivateKey, crypto_package.Signer>(Pointer = true)]
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
[assembly: go.GoPositionMap("crypto/rsa/equal_test.go", "equal_test.cs", "ABIcgoSChIKUgpSCloKClIKClIKUgpaCgpSC")]
[assembly: go.GoPositionMap("crypto/rsa/pkcs1v15_test.go", "pkcs1v15_test.cs", "ABwugoKCgpQABxAAEiaChJSmqIKCgoKUgoLcgoKEkoKWgoKCloKClJSCgoKWgoKUloKClLgAEiaCgoKCgoKUgoLKgoKCgoKUgpaCyoKEgoKClIKCggALGLiCgoKCgoSCgpaCgsqCgoKCgoSEgoL6goKCgoK4goTuhIKClIKUgIL4xoKCloKAgqaCgsqCgoKClAALBoKIgoKWgoKC", "60-62:1;63-65:2;86-114:1")]
[assembly: go.GoPositionMap("crypto/rsa/pss_test.go", "pss_test.cs", "ACo04oKClJzChITCgoKChIKClIKCgpS0grSCtLTGgILElIKCgoKogpSCgoKUgoSStoKChIKChICCxtaoooSCgoKWAAgUgILIgoKCgoT2ogANJIKCgoKEgoKCgoKWgoKCgpSCyriCgoKUgriClIKCuIKCgpSmgoKClKaCgoKU5qKCgoKWgoC4yoCmyIKUgoKogII=", "42-73:1")]
[assembly: go.GoPositionMap("crypto/rsa/rsa_test.go", "rsa_test.cs", "ACE2goKClIKCgoKClJSCgpSAgqTKgoKCgpaCgpSmgoKCgpaCgpSmgoKCgoKCpoKCgpTKxoKCgoKC6JSClIKCgoKUgIIADgqCuIqmooCCpIKWgoKCgpaCgoKUgriChIKCgpaAooKClIKmyJSClIKSgqaWgoKCgpSCgoKCgoKUgIKkAA4KooCCpoKCgpKClIKCgpSCgpSCqIKCgpKClIKCgpSCqIKCgpaCgoKSgpSCgoKUgoKClIKCgoKUloKCgpKClIKCgpSCgoKUgoKCgpSWgoKCkoKUgoKClIKCgpSCgoKClLqCgoKUgoKUgoKClIKCloKClIKClIKWgoKUgoKUgviCgoKClIKmkoKWgpKSkpKCgpKSkpKSkqaApIKCgoKClJSCgpTGlAAMFAAMFgARIAAeOqQAKlIANmqCgJKAkoC2ooSCgoKWgoKCgoKUgpS4goKChIKCgoKUyoKChIKCgpaCgoKCgpSClMqCgoKEgoKCgpTKgoKEgoKCgoKUyoKCgoKCloKCgoLcgoKEgoKCgoKUyoKCgoKCloKCgoLcgoKCgILsgoKCgoKAggATJoKCgoKClIKCgoKUgtyChIKCgoKCgoKChIKCgpKCqIKCkoKmgsqChIKEgoKCgoKCgpKUhIKSgriCgoKCgoKCgoKEgoKCgoKUgoKClIIADA4AmQGkApQAABKCgpQ=", "33-49:1;176-184:1;195-198:1;211-221:2;399-407:1;408-411:2;612-612:1;613-613:2;614-614:3;641-653:1;657-678:1;682-694:1;698-710:1;714-728:1;732-744:1;748-762:1;766-772:1;776-784:1")]
// </GoSourcePositionMaps>

namespace go.crypto;

[GoPackage("rsa_test")]
public static partial class rsa_test_package
{
    // C# nested types declared with no access modifier are always private, and the
    // `[GoType]` declarations in this package's converted sources are deliberately
    // bare so they read more like the original Go code. The real accessibility for
    // the types - public for a Go-exported name, internal otherwise - are defined
    // via declarations below.

    // <TypeAccessibility>
    internal partial struct TestPSSSigning_type {}
    internal partial struct signPKCS1v15Test {}
    internal partial struct testEncryptOAEPMessage {}
    internal partial struct testEncryptOAEPStruct {}
    public partial struct DecryptPKCS1v15Test {}
    // </TypeAccessibility>

    // Go initializes an imported package before the importing package, for every import
    // form - not only the blank one. .NET would never load an assembly nothing has touched
    // yet, so each import that initializes anything is forced below: once per assembly, and
    // ahead of this package's own `init` functions, which this file being the first compile
    // item of the project guarantees.

    // <ImportInitializers>
    [GoInit] internal static void initᴛᴛimportꓸbufio() => builtin.initPackage(typeof(bufio_package));
    [GoInit] internal static void initᴛᴛimportꓸbytes() => builtin.initPackage(typeof(bytes_package));
    [GoInit] internal static void initᴛᴛimportꓸcompressꓸbzip2() => builtin.initPackage(typeof(compress.bzip2_package));
    [GoInit] internal static void initᴛᴛimportꓸcrypto() => builtin.initPackage(typeof(crypto_package));
    [GoInit] internal static void initᴛᴛimportꓸcryptoꓸinternalꓸboring() => builtin.initPackage(typeof(go.crypto.@internal.boring_package));
    [GoInit] internal static void initᴛᴛimportꓸcryptoꓸinternalꓸcryptotest() => builtin.initPackage(typeof(go.crypto.@internal.cryptotest_package));
    [GoInit] internal static void initᴛᴛimportꓸcryptoꓸinternalꓸfips140() => builtin.initPackage(typeof(go.crypto.@internal.fips140_package));
    [GoInit] internal static void initᴛᴛimportꓸcryptoꓸrand() => builtin.initPackage(typeof(go.crypto.rand_package));
    [GoInit] internal static void initᴛᴛimportꓸcryptoꓸrsa() => builtin.initPackage(typeof(go.crypto.rsa_package));
    [GoInit] internal static void initᴛᴛimportꓸcryptoꓸsha1() => builtin.initPackage(typeof(go.crypto.sha1_package));
    [GoInit] internal static void initᴛᴛimportꓸcryptoꓸsha256() => builtin.initPackage(typeof(go.crypto.sha256_package));
    [GoInit] internal static void initᴛᴛimportꓸcryptoꓸsha512() => builtin.initPackage(typeof(go.crypto.sha512_package));
    [GoInit] internal static void initᴛᴛimportꓸcryptoꓸx509() => builtin.initPackage(typeof(go.crypto.x509_package));
    [GoInit] internal static void initᴛᴛimportꓸencodingꓸbase64() => builtin.initPackage(typeof(encoding.base64_package));
    [GoInit] internal static void initᴛᴛimportꓸencodingꓸhex() => builtin.initPackage(typeof(encoding.hex_package));
    [GoInit] internal static void initᴛᴛimportꓸencodingꓸpem() => builtin.initPackage(typeof(encoding.pem_package));
    [GoInit] internal static void initᴛᴛimportꓸflag() => builtin.initPackage(typeof(flag_package));
    [GoInit] internal static void initᴛᴛimportꓸfmt() => builtin.initPackage(typeof(fmt_package));
    [GoInit] internal static void initᴛᴛimportꓸio() => builtin.initPackage(typeof(io_package));
    [GoInit] internal static void initᴛᴛimportꓸmathꓸbig() => builtin.initPackage(typeof(math.big_package));
    [GoInit] internal static void initᴛᴛimportꓸos() => builtin.initPackage(typeof(os_package));
    [GoInit] internal static void initᴛᴛimportꓸstrconv() => builtin.initPackage(typeof(strconv_package));
    [GoInit] internal static void initᴛᴛimportꓸstrings() => builtin.initPackage(typeof(strings_package));
    [GoInit] internal static void initᴛᴛimportꓸtesting() => builtin.initPackage(typeof(testing_package));
    [GoInit] internal static void initᴛᴛimportꓸtestingꓸquick() => builtin.initPackage(typeof(go.testing.quick_package));
    // </ImportInitializers>
    // Go runs every `init` in the package under test - the production files' included -
    // before the first test. The production package is a REFERENCED assembly here, whose
    // module constructor .NET would not run until something in it is touched, so that
    // initialization is forced before anything else in this test module runs.
    [GoInit] internal static void initᴛᴛproduction() {
        builtin.initPackage(typeof(global::go.crypto.rsa_package));
    }
}
