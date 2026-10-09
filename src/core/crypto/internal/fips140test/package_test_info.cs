// go2cs metadata anchor for a production-reference test project: the test assembly
// REFERENCES the colocated production project instead of
// recompiling its sources, so the production assembly is the single identity for the
// production types and no production class partial may be declared here. The first —
// and only — class is the test metadata class the go2cs-gen generators anchor
// generated adapters and partials to.

// <ImportedTypeAliases>
global using abiꓸArrayType = go.@internal.abi_package.ΔArrayType;
global using abiꓸChanDir = go.@internal.abi_package.ΔChanDir;
global using abiꓸFuncType = go.@internal.abi_package.ΔFuncType;
global using abiꓸInterfaceType = go.@internal.abi_package.ΔInterfaceType;
global using abiꓸKind = go.@internal.abi_package.ΔKind;
global using abiꓸName = go.@internal.abi_package.ΔName;
global using abiꓸStructType = go.@internal.abi_package.ΔStructType;
global using bigꓸInt = go.math.big_package.ΔInt;
global using bigꓸRat = go.math.big_package.ΔRat;
global using cryptoꓸDecrypterOpts = object;
global using cryptoꓸPrivateKey = object;
global using cryptoꓸPublicKey = object;
global using ecdhꓸPublicKey = go.crypto.@internal.fips140.ecdh_package.ΔPublicKey;
global using ecdsaꓸPublicKey = go.crypto.@internal.fips140.ecdsa_package.ΔPublicKey;
global using ed25519ꓸPublicKey = go.crypto.@internal.fips140.ed25519_package.ΔPublicKey;
global using osꓸDirEntry = go.io.fs_package.DirEntry;
global using osꓸFileInfo = go.io.fs_package.FileInfo;
global using osꓸFileMode = go.io.fs_package.FileMode;
global using osꓸPathError = go.io.fs_package.PathError;
global using osꓸSignal = go.os_package.ΔSignal;
global using rsaꓸPublicKey = go.crypto.@internal.fips140.rsa_package.ΔPublicKey;
global using runtimeꓸError = go.runtime_package.ΔError;
global using tls13ꓸExporterMasterSecret = go.crypto.@internal.fips140.tls13_package.ΔExporterMasterSecret;
global using tls13ꓸHandshakeSecret = go.crypto.@internal.fips140.tls13_package.ΔHandshakeSecret;
global using tls13ꓸMasterSecret = go.crypto.@internal.fips140.tls13_package.ΔMasterSecret;
// </ImportedTypeAliases>

using go;
using static global::go.crypto.@internal.fips140test.fipstest_internal_test_package;

// <ExportedTypeAliases>
[assembly: GoDynamicTypeLift("7374727563747b78205b5d627974653b2079205b5d627974653b20616e794f7665726c617020626f6f6c3b20696e65786163744f7665726c617020626f6f6c7d", "aliasingTestsᴛ1")]
// </ExportedTypeAliases>

// <InterfaceImplementations>
[assembly: GoImplement<bufio_package.Reader, io_package.Reader>(Pointer = true)]
[assembly: GoImplement<bytes_package.Buffer, io_package.Reader>(Pointer = true)]
[assembly: GoImplement<bytes_package.Buffer, io_package.Writer>(Pointer = true)]
[assembly: GoImplement<go.crypto.@internal.fips140.nistec_package.P224Point, nistPoint<go.crypto.@internal.fips140.nistec_package.P224Point>>(ConstraintProxy = true)]
[assembly: GoImplement<go.crypto.@internal.fips140.nistec_package.P256Point, nistPoint<go.crypto.@internal.fips140.nistec_package.P256Point>>(ConstraintProxy = true)]
[assembly: GoImplement<go.crypto.@internal.fips140.nistec_package.P384Point, nistPoint<go.crypto.@internal.fips140.nistec_package.P384Point>>(ConstraintProxy = true)]
[assembly: GoImplement<go.crypto.@internal.fips140.nistec_package.P521Point, nistPoint<go.crypto.@internal.fips140.nistec_package.P521Point>>(ConstraintProxy = true)]
[assembly: GoImplement<go.crypto.@internal.fips140.sha256_package.Digest, go.crypto.@internal.fips140_package.Hash>(Pointer = true)]
[assembly: GoImplement<go.crypto.@internal.fips140.sha3_package.Digest, go.crypto.@internal.fips140_package.Hash>(Pointer = true)]
[assembly: GoImplement<go.crypto.@internal.fips140.sha512_package.Digest, go.crypto.@internal.fips140_package.Hash>(Pointer = true)]
[assembly: GoImplement<hash_package.Hash, go.crypto.@internal.fips140_package.Hash>]
[assembly: GoImplement<os_package.File, io_package.Reader>(Pointer = true)]
[assembly: GoImplement<strings_package.Builder, io_package.Writer>(Pointer = true)]
[assembly: GoImplement<testing_package.T, testing_package.TB>(Pointer = true)]
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
[assembly: go.GoPositionMap("crypto/internal/fips140test/acvp_test.go", "acvp_test.cs", "ADVagoKUuIKAgoIAEkQAMnS4goKCkoKWgoKWgIKmgoKWgIK41o7igIKkgpaCgpbcgoKEgoCCuIKCgIKklqa4goCCpoKAgriCgIK4rLIABBzytoKCgoQABCYADgK2goSAgqaCgoKChIKCgoSClgAEJAANAraCgoSCgoKWyoK2goKCgsqCuIKCloKCgoSCgpbKgoSUpKSkpKSkpKSkpKamgraEgoKoyoK2goSCgpaCloTKgraChIKCloKClsqCtoSCgqjKgraChIKCloKWhMqCtoKEgoKWgoKWyoK2goKCgoKWggAIFIKCgoQAGAqChAAHGICCuISWgoK4goKCgIK4hIKClILMAAgSgoKCgoKU5oKmgIKkqIKCgpSCguiCpoCCpKiClIKUgoLogoKCgpaCgpaC6IKCuoKCgpaCgpaCuIKEhIKAgqamgoKAgqaCgpY=", "258-260:1;274-281:1;302-326:1;346-358:1;365-371:1;379-396:1;405-405:1;407-407:2;409-409:3;411-411:4;413-413:5;415-415:6;417-417:7;419-419:8;421-421:9;423-423:10;434-444:1;451-467:1;474-489:1;496-506:1;513-529:1;536-551:1;558-584:1;662-667:1;684-689:1", "", "85=cmdGetConfig/1/1/1,New224/1/4/3,cmdHashAft/1/10/3,New224/2/4/4,cmdHashMct/1/6/4,New/1/4/5,cmdHashAft/2/10/5,New/2/4/6,cmdHashMct/2/6/6,New384/1/4/7,cmdHashAft/3/10/7,New384/2/4/8,cmdHashMct/3/6/8,New/3/4/9,cmdHashAft/4/10/9,New/4/4/10,cmdHashMct/4/6/10,New512_224/1/2/11,cmdHashAft/5/10/11,New512_224/2/2/12,cmdHashMct/5/6/12,New512_256/1/2/13,cmdHashAft/6/10/13,New512_256/2/2/14,cmdHashMct/6/6/14,New256/1/2/16,cmdHashAft/7/10/16,New256/2/2/17,cmdSha3Mct/1/4/17,New224/3/4/18,cmdHashAft/8/10/18,New224/4/4/19,cmdSha3Mct/2/4/19,New384/3/4/20,cmdHashAft/9/10/20,New384/4/4/21,cmdSha3Mct/3/4/21,New512/1/2/22,cmdHashAft/10/10/22,New512/2/2/23,cmdSha3Mct/4/4/23,cmdHmacAft/1/10/25,cmdHmacAft/2/10/26,cmdHmacAft/3/10/27,cmdHmacAft/4/10/28,cmdHmacAft/5/10/29,cmdHmacAft/6/10/30,cmdHmacAft/7/10/31,cmdHmacAft/8/10/32,cmdHmacAft/9/10/33,cmdHmacAft/10/10/34,cmdPbkdf/1/1/36,cmdMlKem768KeyGenAft/1/1/38,cmdMlKem768EncapAft/1/1/39,cmdMlKem768DecapAft/1/1/40,cmdMlKem1024KeyGenAft/1/1/41,cmdMlKem1024EncapAft/1/1/42,cmdMlKem1024DecapAft/1/1/43,cmdHmacDrbgAft/1/10/45,cmdHmacDrbgAft/2/10/46,cmdHmacDrbgAft/3/10/47,cmdHmacDrbgAft/4/10/48,cmdHmacDrbgAft/5/10/49,cmdHmacDrbgAft/6/10/50,cmdHmacDrbgAft/7/10/51,cmdHmacDrbgAft/8/10/52,cmdHmacDrbgAft/9/10/53,cmdHmacDrbgAft/10/10/54;636=Join/1/1/2")]
[assembly: go.GoPositionMap("crypto/internal/fips140test/alias_test.go", "alias_test.cs", "ABMcAA8mgoKClIKCuIKCgg==")]
[assembly: go.GoPositionMap("crypto/internal/fips140test/cast_test.go", "cast_test.cs", "ACtMAB80gqiCgoKUgoaSgoCSgpSClIKClIKUlKaCguqSgoKClJSCgpSUgoKUlIKClJTmgoKAgqaCgoKCgpaCkoKUAAkMooKAgqaCpoKUgoKCgoKClIKSgpQ=", "80-95:1;150-156:1;167-188:1")]
[assembly: go.GoPositionMap("crypto/internal/fips140test/check_test.go", "check_test.cs", "AB8ugoKCloKWgpaAgqaCgoKClAARBoKCloCCuIKUgpSClIKUgpSClICCuIKCgqaCgIKkgoKAgqSWkoKCgsyCgoKWgoKogoKUgg==", "83-88:1;101-108:2")]
[assembly: go.GoPositionMap("crypto/internal/fips140test/cmac_test.go", "cmac_test.cs", "ABUclIIADSKCgpSCgoKCgoI=")]
[assembly: go.GoPositionMap("crypto/internal/fips140test/ctrdrbg_test.go", "ctrdrbg_test.cs", "AA0ctoKCgoKCgoiigoSEgoSCgg==")]
[assembly: go.GoPositionMap("crypto/internal/fips140test/edwards25519_test.go", "edwards25519_test.cs", "ABAegoKAgoKCgoKU", "17-23:1")]
[assembly: go.GoPositionMap("crypto/internal/fips140test/fips_test.go", "fips_test.cs", "ABQogoKCgoKU")]
[assembly: go.GoPositionMap("crypto/internal/fips140test/indicator_test.go", "indicator_test.cs", "ABcYgoKCloKCgpaCgoKCloKCgpaCgoKCloKCgoKWgoKSgoKClIKCloKCgoKCgpSCgg==", "54-58:1;67-71:2")]
[assembly: go.GoPositionMap("crypto/internal/fips140test/nistec_test.go", "nistec_test.cs", "ABokgoKCgIKCgoKCgoKAgqSCgIK2toKAgoKCgoKCgoCCpIKAgra2goCCgoKCgoKCgIKkgoCCtraCgIKCgoKCgoKAgqSCgIK2AA8egoKUgpSClIIACgiihIKCgoKEgoKCgoKCgoKChIKUgpSClIKUgriCgpSClIKUggAKCKKCkoKCgoKCloKCgpSCpoKUgqiCgoKCgoKCqIKCkJKSlJKUkJKSlJKCgpSClJKSgqaSksqSksqCgoI=", "20-38:1;21-35:1.1;39-57:2;40-54:2.1;58-76:3;59-73:3.1;77-95:4;78-92:4.1;109-111:1;112-114:2;115-117:3;118-120:4;161-163:1;164-166:2;167-169:3;170-172:4;177-212:1;216-216:2;217-219:3;220-222:4;223-223:5;224-226:6;227-231:7;236-239:8;242-244:9;249-251:10")]
[assembly: go.GoPositionMap("crypto/internal/fips140test/sshkdf_test.go", "sshkdf_test.cs", "ABIelIKCloKCgoKChJSWgpSClIKUgpSClIK4goKClA==")]
[assembly: go.GoPositionMap("crypto/internal/fips140test/xaes_test.go", "xaes_test.cs", "ABgmgoKUgoCCgoKCgoKCgIK2yIKCgoSChIKCgoKChIKClIIAAxbigoKCgoKmgoKCgoKCpoKCgoKCgoCCpICCkoKmgoKCgoCCpICCkoLIgoKEgoKCgoKCgoKCgoKChIKCgpSClpSAgg==", "24-34:1")]
// </GoSourcePositionMaps>

namespace go.crypto.@internal.fips140test;

[GoPackage("fipstest", ImportPath = "crypto/internal/fips140test")]
public static partial class fipstest_internal_test_package
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
    [GoInit] internal static void initᴛᴛimportꓸcryptoꓸelliptic() => builtin.initPackage(typeof(go.crypto.elliptic_package));
    [GoInit] internal static void initᴛᴛimportꓸcryptoꓸinternalꓸcryptotest() => builtin.initPackage(typeof(go.crypto.@internal.cryptotest_package));
    [GoInit] internal static void initᴛᴛimportꓸcryptoꓸinternalꓸfips140() => builtin.initPackage(typeof(go.crypto.@internal.fips140_package));
    [GoInit] internal static void initᴛᴛimportꓸcryptoꓸinternalꓸfips140ꓸaes() => builtin.initPackage(typeof(go.crypto.@internal.fips140.aes_package));
    [GoInit] internal static void initᴛᴛimportꓸcryptoꓸinternalꓸfips140ꓸaesꓸgcm() => builtin.initPackage(typeof(go.crypto.@internal.fips140.aes.gcm_package));
    [GoInit] internal static void initᴛᴛimportꓸcryptoꓸinternalꓸfips140ꓸcheck() => builtin.initPackage(typeof(go.crypto.@internal.fips140.check_package));
    [GoInit] internal static void initᴛᴛimportꓸcryptoꓸinternalꓸfips140ꓸcheckꓸchecktest() => builtin.initPackage(typeof(go.crypto.@internal.fips140.check.checktest_package));
    [GoInit] internal static void initᴛᴛimportꓸcryptoꓸinternalꓸfips140ꓸdrbg() => builtin.initPackage(typeof(go.crypto.@internal.fips140.drbg_package));
    [GoInit] internal static void initᴛᴛimportꓸcryptoꓸinternalꓸfips140ꓸecdh() => builtin.initPackage(typeof(go.crypto.@internal.fips140.ecdh_package));
    [GoInit] internal static void initᴛᴛimportꓸcryptoꓸinternalꓸfips140ꓸecdsa() => builtin.initPackage(typeof(go.crypto.@internal.fips140.ecdsa_package));
    [GoInit] internal static void initᴛᴛimportꓸcryptoꓸinternalꓸfips140ꓸed25519() => builtin.initPackage(typeof(go.crypto.@internal.fips140.ed25519_package));
    [GoInit] internal static void initᴛᴛimportꓸcryptoꓸinternalꓸfips140ꓸedwards25519() => builtin.initPackage(typeof(go.crypto.@internal.fips140.edwards25519_package));
    [GoInit] internal static void initᴛᴛimportꓸcryptoꓸinternalꓸfips140ꓸhkdf() => builtin.initPackage(typeof(go.crypto.@internal.fips140.hkdf_package));
    [GoInit] internal static void initᴛᴛimportꓸcryptoꓸinternalꓸfips140ꓸhmac() => builtin.initPackage(typeof(go.crypto.@internal.fips140.hmac_package));
    [GoInit] internal static void initᴛᴛimportꓸcryptoꓸinternalꓸfips140ꓸmlkem() => builtin.initPackage(typeof(go.crypto.@internal.fips140.mlkem_package));
    [GoInit] internal static void initᴛᴛimportꓸcryptoꓸinternalꓸfips140ꓸnistec() => builtin.initPackage(typeof(go.crypto.@internal.fips140.nistec_package));
    [GoInit] internal static void initᴛᴛimportꓸcryptoꓸinternalꓸfips140ꓸpbkdf2() => builtin.initPackage(typeof(go.crypto.@internal.fips140.pbkdf2_package));
    [GoInit] internal static void initᴛᴛimportꓸcryptoꓸinternalꓸfips140ꓸrsa() => builtin.initPackage(typeof(go.crypto.@internal.fips140.rsa_package));
    [GoInit] internal static void initᴛᴛimportꓸcryptoꓸinternalꓸfips140ꓸsha256() => builtin.initPackage(typeof(go.crypto.@internal.fips140.sha256_package));
    [GoInit] internal static void initᴛᴛimportꓸcryptoꓸinternalꓸfips140ꓸsha3() => builtin.initPackage(typeof(go.crypto.@internal.fips140.sha3_package));
    [GoInit] internal static void initᴛᴛimportꓸcryptoꓸinternalꓸfips140ꓸsha512() => builtin.initPackage(typeof(go.crypto.@internal.fips140.sha512_package));
    [GoInit] internal static void initᴛᴛimportꓸcryptoꓸinternalꓸfips140ꓸssh() => builtin.initPackage(typeof(go.crypto.@internal.fips140.ssh_package));
    [GoInit] internal static void initᴛᴛimportꓸcryptoꓸinternalꓸfips140ꓸsubtle() => builtin.initPackage(typeof(go.crypto.@internal.fips140.subtle_package));
    [GoInit] internal static void initᴛᴛimportꓸcryptoꓸinternalꓸfips140ꓸtls12() => builtin.initPackage(typeof(go.crypto.@internal.fips140.tls12_package));
    [GoInit] internal static void initᴛᴛimportꓸcryptoꓸinternalꓸfips140ꓸtls13() => builtin.initPackage(typeof(go.crypto.@internal.fips140.tls13_package));
    [GoInit] internal static void initᴛᴛimportꓸcryptoꓸrand() => builtin.initPackage(typeof(go.crypto.rand_package));
    [GoInit] internal static void initᴛᴛimportꓸcryptoꓸsha256() => builtin.initPackage(typeof(go.crypto.sha256_package));
    [GoInit] internal static void initᴛᴛimportꓸembed() => builtin.initPackage(typeof(embed_package));
    [GoInit] internal static void initᴛᴛimportꓸencodingꓸbinary() => builtin.initPackage(typeof(encoding.binary_package));
    [GoInit] internal static void initᴛᴛimportꓸencodingꓸhex() => builtin.initPackage(typeof(encoding.hex_package));
    [GoInit] internal static void initᴛᴛimportꓸerrors() => builtin.initPackage(typeof(errors_package));
    [GoInit] internal static void initᴛᴛimportꓸfmt() => builtin.initPackage(typeof(fmt_package));
    [GoInit] internal static void initᴛᴛimportꓸinternalꓸabi() => builtin.initPackage(typeof(go.@internal.abi_package));
    [GoInit] internal static void initᴛᴛimportꓸinternalꓸgodebug() => builtin.initPackage(typeof(go.@internal.godebug_package));
    [GoInit] internal static void initᴛᴛimportꓸinternalꓸtestenv() => builtin.initPackage(typeof(go.@internal.testenv_package));
    [GoInit] internal static void initᴛᴛimportꓸio() => builtin.initPackage(typeof(io_package));
    [GoInit] internal static void initᴛᴛimportꓸioꓸfs() => builtin.initPackage(typeof(go.io.fs_package));
    [GoInit] internal static void initᴛᴛimportꓸmathꓸbig() => builtin.initPackage(typeof(math.big_package));
    [GoInit] internal static void initᴛᴛimportꓸmathꓸrand() => builtin.initPackage(typeof(math.rand_package));
    [GoInit] internal static void initᴛᴛimportꓸos() => builtin.initPackage(typeof(os_package));
    [GoInit] internal static void initᴛᴛimportꓸpathꓸfilepath() => builtin.initPackage(typeof(path.filepath_package));
    [GoInit] internal static void initᴛᴛimportꓸregexp() => builtin.initPackage(typeof(regexp_package));
    [GoInit] internal static void initᴛᴛimportꓸruntime() => builtin.initPackage(typeof(runtime_package));
    [GoInit] internal static void initᴛᴛimportꓸslices() => builtin.initPackage(typeof(slices_package));
    [GoInit] internal static void initᴛᴛimportꓸstrings() => builtin.initPackage(typeof(strings_package));
    [GoInit] internal static void initᴛᴛimportꓸtesting() => builtin.initPackage(typeof(testing_package));
    [GoInit] internal static void initᴛᴛimportꓸunicode() => builtin.initPackage(typeof(unicode_package));
    // </ImportInitializers>
}
