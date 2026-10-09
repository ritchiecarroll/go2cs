// go2cs metadata anchor for a production-reference test project: the test assembly
// REFERENCES the colocated production project instead of
// recompiling its sources, so the production assembly is the single identity for the
// production types and no production class partial may be declared here. The first —
// and only — class is the test metadata class the go2cs-gen generators anchor
// generated adapters and partials to.
global using static global::go.crypto.tls_package;
global using static global::go.crypto.tls_internal_test_package;

// <ImportedTypeAliases>
global using bigꓸInt = go.math.big_package.ΔInt;
global using bigꓸRat = go.math.big_package.ΔRat;
global using cryptoꓸDecrypterOpts = object;
global using cryptoꓸPrivateKey = object;
global using cryptoꓸPublicKey = object;
global using ecdhꓸCurve = go.crypto.ecdh_package.ΔCurve;
global using ecdhꓸPublicKey = go.crypto.ecdh_package.ΔPublicKey;
global using execꓸError = go.os.exec_package.ΔError;
global using flagꓸErrorHandling = go.flag_package.ΔErrorHandling;
global using jsonꓸToken = object;
global using jsonꓸΔToken = object;
global using netꓸAddr = go.net_package.ΔAddr;
global using netꓸError = go.net_package.ΔError;
global using osꓸDirEntry = go.io.fs_package.DirEntry;
global using osꓸFileInfo = go.io.fs_package.FileInfo;
global using osꓸFileMode = go.io.fs_package.FileMode;
global using osꓸPathError = go.io.fs_package.PathError;
global using osꓸSignal = go.os_package.ΔSignal;
global using reflectꓸChanDir = go.reflect_package.ΔChanDir;
global using reflectꓸKind = go.reflect_package.ΔKind;
global using reflectꓸMethod = go.reflect_package.ΔMethod;
global using reflectꓸType = go.reflect_package.ΔType;
global using reflectꓸValue = go.reflect_package.ΔValue;
global using runtimeꓸError = go.runtime_package.ΔError;
global using timeꓸLocation = go.time_package.ΔLocation;
global using timeꓸMonth = go.time_package.ΔMonth;
global using timeꓸWeekday = go.time_package.ΔWeekday;
global using tls13ꓸExporterMasterSecret = go.crypto.@internal.fips140.tls13_package.ΔExporterMasterSecret;
global using tls13ꓸHandshakeSecret = go.crypto.@internal.fips140.tls13_package.ΔHandshakeSecret;
global using tls13ꓸMasterSecret = go.crypto.@internal.fips140.tls13_package.ΔMasterSecret;
using testing = go.testing_package;
// </ImportedTypeAliases>

using go;
using static global::go.crypto.tls_test_package;

// <ExportedTypeAliases>
[assembly: GoDynamicTypeLift("7374727563747b41637475616c20737472696e6720226a736f6e3a5c2261637475616c5c22223b20457870656374656420737472696e6720226a736f6e3a5c2265787065637465645c22223b204973556e657870656374656420626f6f6c20226a736f6e3a5c2269735f756e65787065637465645c22223b204572726f7220737472696e6720226a736f6e3a5c226572726f722c6f6d6974656d7074795c22227d", "bogoResults_Tests")]
[assembly: GoDynamicTypeLift("7374727563747b616c676f20737472696e673b206365727420737472696e673b206b657920737472696e677d", "keyPairTestsᴛ1")]
[assembly: GoDynamicTypeLift("7374727563747b696e205b5d627974653b20676f6f6420626f6f6c3b2065787065637465644c656e20696e747d", "paddingTestsᴛ1")]
[assembly: GoDynamicTypeLift("7374727563747b696e20737472696e673b206f757420737472696e677d", "hostnameInSNITestsᴛ1")]
[assembly: GoDynamicTypeLift("7374727563747b6d752073796e632e4d757465783b2061646472206e65742e416464723b206368206368616e206e65742e436f6e6e7d", "localListenerᴛ1")]
[assembly: GoDynamicTypeLift("7374727563747b73657475702066756e63282a63727970746f2f746c732e436f6e6669672c202a63727970746f2f746c732e436f6e666967293b206578706563746564436c69656e744572726f7220737472696e673b207665726966792066756e63282a74657374696e672e542c20696e742c202a63727970746f2f746c732e436f6e6e656374696f6e5374617465297d", "getClientCertificateTestsᴛ1")]
[assembly: GoDynamicTypeLift("7374727563747b73657475702066756e6328636f6e666967202a63727970746f2f746c732e436f6e666967293b2063616c6c6261636b2066756e6328636c69656e7448656c6c6f202a63727970746f2f746c732e436c69656e7448656c6c6f496e666f2920282a63727970746f2f746c732e436f6e6669672c206572726f72293b206572726f72537562737472696e6720737472696e673b207665726966792066756e6328636f6e666967202a63727970746f2f746c732e436f6e66696729206572726f727d", "getConfigForClientTestsᴛ1")]
[assembly: GoTypeAlias("ConnectionState", "ΔConnectionState")]
// </ExportedTypeAliases>

// <InterfaceImplementations>
[assembly: GoImplement<bytes_package.Buffer, io_package.Writer>(Pointer = true)]
[assembly: GoImplement<global::go.crypto.tls_package.AlertError, error>]
[assembly: GoImplement<global::go.crypto.tls_package.Conn, io_package.Reader>(Pointer = true)]
[assembly: GoImplement<global::go.crypto.tls_package.Conn, io_package.Writer>(Pointer = true)]
[assembly: GoImplement<global::go.crypto.tls_package.SessionState, global::go.crypto.tls_package.handshakeMessage>(Pointer = true)]
[assembly: GoImplement<global::go.crypto.tls_package.alert, error>]
[assembly: GoImplement<go.crypto.rsa_package.PrivateKey, crypto_package.Signer>(Pointer = true)]
[assembly: GoImplement<hash_package.Hash, global::go.crypto.tls_package.transcriptHash>]
[assembly: GoImplement<hash_package.Hash, go.crypto.@internal.fips140_package.Hash>]
[assembly: GoImplement<net_package.Conn, io_package.Reader>]
[assembly: GoImplement<os_package.File, io_package.Reader>(Pointer = true)]
[assembly: GoImplement<strings_package.Builder, io_package.Writer>(Pointer = true)]
[assembly: GoImplement<testing_package.B, testing_package.TB>(Pointer = true)]
[assembly: GoImplement<testing_package.T, testing_package.TB>(Pointer = true)]
// </InterfaceImplementations>

// <ImplicitConversions>
[assembly: GoImplicitConv<global::go.crypto.tls_package.Config, ж<global::go.crypto.tls_package.Config>>(Indirect = true)]
[assembly: GoImplicitConv<global::go.crypto.tls_package.QUICConfig, ж<global::go.crypto.tls_package.QUICConfig>>(Indirect = true)]
[assembly: GoImplicitConv<global::go.crypto.tls_package.clientHelloMsg, ж<global::go.crypto.tls_package.clientHelloMsg>>(Indirect = true)]
[assembly: GoImplicitConv<global::go.crypto.tls_package.serverHelloDoneMsg, ж<global::go.crypto.tls_package.serverHelloDoneMsg>>(Indirect = true)]
[assembly: GoImplicitConv<global::go.crypto.tls_package.serverHelloMsg, ж<global::go.crypto.tls_package.serverHelloMsg>>(Indirect = true)]
[assembly: GoImplicitConv<global::go.crypto.tls_package.ΔConnectionState, ж<global::go.crypto.tls_package.ΔConnectionState>>(Indirect = true)]
// </ImplicitConversions>

// Go source positions are recorded here, one `GoPositionMap` attribute per converted
// source file in this compilation, so that `runtime.Caller` and the tracebacks built on it
// can name the GO file and line a frame was converted from rather than the emitted C# one.
// Each record carries the Go file's identity and an encoded C#-line to Go-line table
// TOGETHER: a frame either has a record and reports a position that exists in the Go tree,
// or has none - golib, the BCL and hand-written conversions - and reports its own C# position.

// <GoSourcePositionMaps>
// </GoSourcePositionMaps>

namespace go.crypto;

[GoPackage("tls_test")]
public static partial class tls_test_package
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
    [GoInit] internal static void initᴛᴛimportꓸcontext() => builtin.initPackage(typeof(context_package));
    [GoInit] internal static void initᴛᴛimportꓸcrypto() => builtin.initPackage(typeof(crypto_package));
    [GoInit] internal static void initᴛᴛimportꓸcryptoꓸecdh() => builtin.initPackage(typeof(go.crypto.ecdh_package));
    [GoInit] internal static void initᴛᴛimportꓸcryptoꓸecdsa() => builtin.initPackage(typeof(go.crypto.ecdsa_package));
    [GoInit] internal static void initᴛᴛimportꓸcryptoꓸed25519() => builtin.initPackage(typeof(go.crypto.ed25519_package));
    [GoInit] internal static void initᴛᴛimportꓸcryptoꓸelliptic() => builtin.initPackage(typeof(go.crypto.elliptic_package));
    [GoInit] internal static void initᴛᴛimportꓸcryptoꓸinternalꓸcryptotest() => builtin.initPackage(typeof(go.crypto.@internal.cryptotest_package));
    [GoInit] internal static void initᴛᴛimportꓸcryptoꓸinternalꓸfips140ꓸtls13() => builtin.initPackage(typeof(go.crypto.@internal.fips140.tls13_package));
    [GoInit] internal static void initᴛᴛimportꓸcryptoꓸinternalꓸhpke() => builtin.initPackage(typeof(go.crypto.@internal.hpke_package));
    [GoInit] internal static void initᴛᴛimportꓸcryptoꓸrand() => builtin.initPackage(typeof(go.crypto.rand_package));
    [GoInit] internal static void initᴛᴛimportꓸcryptoꓸrsa() => builtin.initPackage(typeof(go.crypto.rsa_package));
    [GoInit] internal static void initᴛᴛimportꓸcryptoꓸsha256() => builtin.initPackage(typeof(go.crypto.sha256_package));
    [GoInit] internal static void initᴛᴛimportꓸcryptoꓸtlsꓸinternalꓸfips140tls() => builtin.initPackage(typeof(go.crypto.tls.@internal.fips140tls_package));
    [GoInit] internal static void initᴛᴛimportꓸcryptoꓸx509() => builtin.initPackage(typeof(go.crypto.x509_package));
    [GoInit] internal static void initᴛᴛimportꓸcryptoꓸx509ꓸpkix() => builtin.initPackage(typeof(go.crypto.x509.pkix_package));
    [GoInit] internal static void initᴛᴛimportꓸencodingꓸasn1() => builtin.initPackage(typeof(encoding.asn1_package));
    [GoInit] internal static void initᴛᴛimportꓸencodingꓸbase64() => builtin.initPackage(typeof(encoding.base64_package));
    [GoInit] internal static void initᴛᴛimportꓸencodingꓸhex() => builtin.initPackage(typeof(encoding.hex_package));
    [GoInit] internal static void initᴛᴛimportꓸencodingꓸjson() => builtin.initPackage(typeof(encoding.json_package));
    [GoInit] internal static void initᴛᴛimportꓸencodingꓸpem() => builtin.initPackage(typeof(encoding.pem_package));
    [GoInit] internal static void initᴛᴛimportꓸerrors() => builtin.initPackage(typeof(errors_package));
    [GoInit] internal static void initᴛᴛimportꓸflag() => builtin.initPackage(typeof(flag_package));
    [GoInit] internal static void initᴛᴛimportꓸfmt() => builtin.initPackage(typeof(fmt_package));
    [GoInit] internal static void initᴛᴛimportꓸinternalꓸobscuretestdata() => builtin.initPackage(typeof(go.@internal.obscuretestdata_package));
    [GoInit] internal static void initᴛᴛimportꓸinternalꓸtestenv() => builtin.initPackage(typeof(go.@internal.testenv_package));
    [GoInit] internal static void initᴛᴛimportꓸio() => builtin.initPackage(typeof(io_package));
    [GoInit] internal static void initᴛᴛimportꓸlog() => builtin.initPackage(typeof(log_package));
    [GoInit] internal static void initᴛᴛimportꓸmath() => builtin.initPackage(typeof(math_package));
    [GoInit] internal static void initᴛᴛimportꓸmathꓸbig() => builtin.initPackage(typeof(go.math.big_package));
    [GoInit] internal static void initᴛᴛimportꓸmathꓸrand() => builtin.initPackage(typeof(go.math.rand_package));
    [GoInit] internal static void initᴛᴛimportꓸnet() => builtin.initPackage(typeof(net_package));
    [GoInit] internal static void initᴛᴛimportꓸos() => builtin.initPackage(typeof(os_package));
    [GoInit] internal static void initᴛᴛimportꓸosꓸexec() => builtin.initPackage(typeof(go.os.exec_package));
    [GoInit] internal static void initᴛᴛimportꓸpathꓸfilepath() => builtin.initPackage(typeof(path.filepath_package));
    [GoInit] internal static void initᴛᴛimportꓸreflect() => builtin.initPackage(typeof(reflect_package));
    [GoInit] internal static void initᴛᴛimportꓸruntime() => builtin.initPackage(typeof(runtime_package));
    [GoInit] internal static void initᴛᴛimportꓸslices() => builtin.initPackage(typeof(slices_package));
    [GoInit] internal static void initᴛᴛimportꓸstrconv() => builtin.initPackage(typeof(strconv_package));
    [GoInit] internal static void initᴛᴛimportꓸstrings() => builtin.initPackage(typeof(strings_package));
    [GoInit] internal static void initᴛᴛimportꓸsync() => builtin.initPackage(typeof(sync_package));
    [GoInit] internal static void initᴛᴛimportꓸsyncꓸatomic() => builtin.initPackage(typeof(go.sync.atomic_package));
    [GoInit] internal static void initᴛᴛimportꓸtesting() => builtin.initPackage(typeof(testing_package));
    [GoInit] internal static void initᴛᴛimportꓸtestingꓸquick() => builtin.initPackage(typeof(go.testing.quick_package));
    [GoInit] internal static void initᴛᴛimportꓸtime() => builtin.initPackage(typeof(time_package));
    [GoInit] internal static void initᴛᴛimportꓸunicode() => builtin.initPackage(typeof(unicode_package));
    [GoInit] internal static void initᴛᴛimportꓸvendorꓸgolang_orgꓸxꓸcryptoꓸcryptobyte() => builtin.initPackage(typeof(vendor.golang.org.x.crypto.cryptobyte_package));
    // </ImportInitializers>
    // Go runs every `init` in the package under test - the production files' included -
    // before the first test. The production package is a REFERENCED assembly here, whose
    // module constructor .NET would not run until something in it is touched, so that
    // initialization is forced before anything else in this test module runs.
    [GoInit] internal static void initᴛᴛproduction() {
        builtin.initPackage(typeof(global::go.crypto.tls_package));
    }
}
