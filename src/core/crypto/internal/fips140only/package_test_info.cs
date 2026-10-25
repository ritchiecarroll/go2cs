// go2cs metadata anchor for a production-reference test project: the test assembly
// REFERENCES the colocated production project instead of
// recompiling its sources, so the production assembly is the single identity for the
// production types and no production class partial may be declared here. The first —
// and only — class is the test metadata class the go2cs-gen generators anchor
// generated adapters and partials to.
global using static global::go.crypto.@internal.fips140only_package;

// <ImportedTypeAliases>
global using bigꓸInt = go.math.big_package.ΔInt;
global using bigꓸRat = go.math.big_package.ΔRat;
global using cryptoꓸDecrypterOpts = object;
global using cryptoꓸPrivateKey = object;
global using cryptoꓸPublicKey = object;
global using ecdhꓸCurve = go.crypto.ecdh_package.ΔCurve;
global using ecdhꓸPublicKey = go.crypto.ecdh_package.ΔPublicKey;
global using hpkeꓸKEM = go.crypto.hpke_package.ΔKEM;
global using hpkeꓸPublicKey = go.crypto.hpke_package.ΔPublicKey;
global using osꓸDirEntry = go.io.fs_package.DirEntry;
global using osꓸFileInfo = go.io.fs_package.FileInfo;
global using osꓸFileMode = go.io.fs_package.FileMode;
global using osꓸPathError = go.io.fs_package.PathError;
global using osꓸSignal = go.os_package.ΔSignal;
global using x509ꓸOID = go.crypto.x509_package.ΔOID;
// </ImportedTypeAliases>

using go;
using static global::go.crypto.@internal.fips140only_test_package;

// <ExportedTypeAliases>
// </ExportedTypeAliases>

// <InterfaceImplementations>
[assembly: GoImplement<blockWrap, go.crypto.cipher_package.Block>(Promoted = true)]
[assembly: GoImplement<blockWrap, go.crypto.cipher_package.Block>]
[assembly: GoImplement<go.crypto.ed25519_package.Options, crypto_package.SignerOpts>(Pointer = true)]
[assembly: GoImplement<readerWrap, io_package.Reader>(Promoted = true)]
[assembly: GoImplement<readerWrap, io_package.Reader>]
[assembly: GoImplement<testing_package.T, testing_package.TB>(Pointer = true)]
// </InterfaceImplementations>

// <ImplicitConversions>
// </ImplicitConversions>

// This package's string literals are hoisted to `static readonly` fields (the standard
// library's form, or -hoist-literals), so no converted file passes a bare literal for
// go2cs-gen's literal interceptor to box. The stamp below tells the generator to skip
// the assembly.
[assembly: go.GoHoistedLiterals]

// Go source positions are recorded here, one `GoPositionMap` attribute per converted
// source file in this compilation, so that `runtime.Caller` and the tracebacks built on it
// can name the GO file and line a frame was converted from rather than the emitted C# one.
// Each record carries the Go file's identity and an encoded C#-line to Go-line table
// TOGETHER: a frame either has a record and reports a position that exists in the Go tree,
// or has none - golib, the BCL and hand-written conversions - and reports its own C# position.

// <GoSourcePositionMaps>
[assembly: go.GoPositionMap("crypto/internal/fips140only/fips140only_test.go", "fips140only_test.cs", "AEBggoKCgoKCgoKUlIKClIKCABkIgoKUgpSCgpSSlIKgkoKgpKCSoKSCoKSgpJKSkqSSpKSSpICSkoCkgJKSgKSAkoCAtJKSgsSC1oCSgoC0gLSSgsSC1pKCxILWkoLEgtakgoKSgqaSkpKCkoKClJK2gpKCgqaSkpKSkqSSpJK2gqSCgpSSpLa2yJKSkpKClAABIJKSpKSCgpKkpIKClIKCkpKkgoKUgoKSgoKkAAIUkoKCkoKCkpKClIKCgqaSkpKSgoKSyIIACxiCgoKCgoKCgoKCpoKC1qKCgoKAgpSAgtiipJSCgoKSgriCgoK4goKClLiCpoLmgKSCgoKClA==", "60-63:1;64-67:2;85-85:1;87-87:2;89-89:3;90-90:4;93-93:5;95-95:6;110-110:7;112-112:8;114-114:9;116-116:10;118-118:11;119-119:12;119-119:12.1;123-125:13;126-128:14;130-130:15;131-133:16;132-132:16.1;134-134:17;137-139:18;140-142:19;145-147:20;148-150:21;153-155:22;156-158:23;165-167:24;196-198:25;199-201:26;290-305:27;351-360:1", "124=crypto.Hash.New;127=crypto.Hash.New;132=crypto.Hash.New;134=crypto.Hash.New;138=crypto.Hash.New;141=crypto.Hash.New;146=crypto.Hash.New;149=crypto.Hash.New;154=crypto.Hash.New;157=crypto.Hash.New")]
// </GoSourcePositionMaps>

namespace go.crypto.@internal;

[GoPackage("fips140only_test")]
public static partial class fips140only_test_package
{
    // C# nested types declared with no access modifier are always private, and the
    // Go type declarations in this package's converted sources are deliberately
    // bare so they read more like the original Go code. The real accessibility for
    // the types - public for a Go-exported name, internal otherwise - are defined
    // via declarations below.

    // <TypeAccessibility>
    internal partial struct blockWrap {}
    internal partial struct readerWrap {}
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
    [GoInit] internal static void initᴛᴛimportꓸcryptoꓸdes() => builtin.initPackage(typeof(go.crypto.des_package));
    [GoInit] internal static void initᴛᴛimportꓸcryptoꓸdsa() => builtin.initPackage(typeof(go.crypto.dsa_package));
    [GoInit] internal static void initᴛᴛimportꓸcryptoꓸecdh() => builtin.initPackage(typeof(go.crypto.ecdh_package));
    [GoInit] internal static void initᴛᴛimportꓸcryptoꓸecdsa() => builtin.initPackage(typeof(go.crypto.ecdsa_package));
    [GoInit] internal static void initᴛᴛimportꓸcryptoꓸed25519() => builtin.initPackage(typeof(go.crypto.ed25519_package));
    [GoInit] internal static void initᴛᴛimportꓸcryptoꓸelliptic() => builtin.initPackage(typeof(go.crypto.elliptic_package));
    [GoInit] internal static void initᴛᴛimportꓸcryptoꓸhkdf() => builtin.initPackage(typeof(go.crypto.hkdf_package));
    [GoInit] internal static void initᴛᴛimportꓸcryptoꓸhmac() => builtin.initPackage(typeof(go.crypto.hmac_package));
    [GoInit] internal static void initᴛᴛimportꓸcryptoꓸhpke() => builtin.initPackage(typeof(go.crypto.hpke_package));
    [GoInit] internal static void initᴛᴛimportꓸcryptoꓸinternalꓸcryptotest() => builtin.initPackage(typeof(go.crypto.@internal.cryptotest_package));
    [GoInit] internal static void initᴛᴛimportꓸcryptoꓸinternalꓸfips140() => builtin.initPackage(typeof(go.crypto.@internal.fips140_package));
    [GoInit] internal static void initᴛᴛimportꓸcryptoꓸinternalꓸfips140only() => builtin.initPackage(typeof(go.crypto.@internal.fips140only_package));
    [GoInit] internal static void initᴛᴛimportꓸcryptoꓸmd5() => builtin.initPackage(typeof(go.crypto.md5_package));
    [GoInit] internal static void initᴛᴛimportꓸcryptoꓸmlkem() => builtin.initPackage(typeof(go.crypto.mlkem_package));
    [GoInit] internal static void initᴛᴛimportꓸcryptoꓸmlkemꓸmlkemtest() => builtin.initPackage(typeof(go.crypto.mlkem.mlkemtest_package));
    [GoInit] internal static void initᴛᴛimportꓸcryptoꓸpbkdf2() => builtin.initPackage(typeof(go.crypto.pbkdf2_package));
    [GoInit] internal static void initᴛᴛimportꓸcryptoꓸrand() => builtin.initPackage(typeof(go.crypto.rand_package));
    [GoInit] internal static void initᴛᴛimportꓸcryptoꓸrc4() => builtin.initPackage(typeof(go.crypto.rc4_package));
    [GoInit] internal static void initᴛᴛimportꓸcryptoꓸrsa() => builtin.initPackage(typeof(go.crypto.rsa_package));
    [GoInit] internal static void initᴛᴛimportꓸcryptoꓸsha1() => builtin.initPackage(typeof(go.crypto.sha1_package));
    [GoInit] internal static void initᴛᴛimportꓸcryptoꓸsha256() => builtin.initPackage(typeof(go.crypto.sha256_package));
    [GoInit] internal static void initᴛᴛimportꓸcryptoꓸsha3() => builtin.initPackage(typeof(go.crypto.sha3_package));
    [GoInit] internal static void initᴛᴛimportꓸcryptoꓸsha512() => builtin.initPackage(typeof(go.crypto.sha512_package));
    [GoInit] internal static void initᴛᴛimportꓸcryptoꓸx509() => builtin.initPackage(typeof(go.crypto.x509_package));
    [GoInit] internal static void initᴛᴛimportꓸencodingꓸpem() => builtin.initPackage(typeof(encoding.pem_package));
    [GoInit] internal static void initᴛᴛimportꓸfmt() => builtin.initPackage(typeof(fmt_package));
    [GoInit] internal static void initᴛᴛimportꓸinternalꓸgodebug() => builtin.initPackage(typeof(go.@internal.godebug_package));
    [GoInit] internal static void initᴛᴛimportꓸinternalꓸtestenv() => builtin.initPackage(typeof(go.@internal.testenv_package));
    [GoInit] internal static void initᴛᴛimportꓸio() => builtin.initPackage(typeof(io_package));
    [GoInit] internal static void initᴛᴛimportꓸmathꓸbig() => builtin.initPackage(typeof(math.big_package));
    [GoInit] internal static void initᴛᴛimportꓸos() => builtin.initPackage(typeof(os_package));
    [GoInit] internal static void initᴛᴛimportꓸstrings() => builtin.initPackage(typeof(strings_package));
    [GoInit] internal static void initᴛᴛimportꓸtesting() => builtin.initPackage(typeof(testing_package));
    [GoInit] internal static void initᴛᴛimportꓸvendorꓸgolang_orgꓸxꓸcryptoꓸchacha20poly1305() => builtin.initPackage(typeof(vendor.golang.org.x.crypto.chacha20poly1305_package));
    // </ImportInitializers>
    // Go runs every `init` in the package under test - the production files' included -
    // before the first test. The production package is a REFERENCED assembly here, whose
    // module constructor .NET would not run until something in it is touched, so that
    // initialization is forced before anything else in this test module runs.
    [GoInit] internal static void initᴛᴛproduction() {
        builtin.initPackage(typeof(global::go.crypto.@internal.fips140only_package));
    }
}
