// go2cs metadata anchor for a production-reference test project: the test assembly
// REFERENCES the colocated production project instead of
// recompiling its sources, so the production assembly is the single identity for the
// production types and no production class partial may be declared here. The first —
// and only — class is the test metadata class the go2cs-gen generators anchor
// generated adapters and partials to.
global using static global::go.crypto.x509_package;
global using static global::go.crypto.x509_internal_test_package;

// <ImportedTypeAliases>
global using bigꓸInt = go.math.big_package.ΔInt;
global using bigꓸRat = go.math.big_package.ΔRat;
global using cryptoꓸDecrypterOpts = object;
global using cryptoꓸPrivateKey = object;
global using cryptoꓸPublicKey = object;
global using ecdhꓸCurve = go.crypto.ecdh_package.ΔCurve;
global using ecdhꓸPublicKey = go.crypto.ecdh_package.ΔPublicKey;
global using execꓸError = go.os.exec_package.ΔError;
global using jsonꓸToken = object;
global using jsonꓸΔToken = object;
global using netipꓸAddr = go.net.netip_package.ΔAddr;
global using netipꓸPrefix = go.net.netip_package.ΔPrefix;
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
global using syscallꓸHandle = go.syscall_package.ΔHandle;
global using syscallꓸSignal = go.syscall_package.ΔSignal;
global using syscallꓸSockaddr = go.syscall_package.ΔSockaddr;
global using timeꓸLocation = go.time_package.ΔLocation;
global using timeꓸMonth = go.time_package.ΔMonth;
global using timeꓸWeekday = go.time_package.ΔWeekday;
global using tlsꓸConnectionState = go.crypto.tls_package.ΔConnectionState;
global using urlꓸError = go.net.url_package.ΔError;
using ecdsa = go.crypto.ecdsa_package;
using testing = go.testing_package;
// </ImportedTypeAliases>

using go;
using static global::go.crypto.x509_test_package;

// <ExportedTypeAliases>
[assembly: GoDynamicTypeLift("7374727563747b56657273696f6e20696e743b204e202a6d6174682f6269672e496e743b204520696e743b2044202a6d6174682f6269672e496e743b2050202a6d6174682f6269672e496e743b2051202a6d6174682f6269672e496e747d", "TestParsePKCS1PrivateKey_val")]
[assembly: GoDynamicTypeLift("7374727563747b636f6e73747261696e7420737472696e673b20646f6d61696e20737472696e673b206578706563744572726f7220626f6f6c3b2073686f756c644d6174636820626f6f6c7d", "nameConstraintTestsᴛ1")]
[assembly: GoDynamicTypeLift("7374727563747b64657248657820737472696e673b2073686f756c64526573657269616c697a6520626f6f6c7d", "ecKeyTestsᴛ1")]
[assembly: GoDynamicTypeLift("7374727563747b6865784b657920737472696e673b206572726f72436f6e7461696e7320737472696e677d", "pkcs8MismatchKeyTestsᴛ1")]
[assembly: GoDynamicTypeLift("7374727563747b696e20737472696e673b206c6f63616c5061727420737472696e673b20646f6d61696e20737472696e677d", "rfc2821Testsᴛ1")]
[assembly: GoDynamicTypeLift("7374727563747b6b696e642063727970746f2f783530392e50454d4369706865723b2070617373776f7264205b5d627974653b2070656d44617461205b5d627974653b20706c61696e44455220737472696e677d", "testDataᴛ1")]
[assembly: GoDynamicTypeLift("7374727563747b6e616d6520737472696e673b206365727420737472696e673b20657870656374656420737472696e677d", "unknownAuthorityErrorTestsᴛ1")]
[assembly: GoDynamicTypeLift("7374727563747b726177205b5d627974653b2076616c696420626f6f6c3b2073747220737472696e673b20696e7473205b5d75696e7436347d", "oidTestsᴛ1")]
[assembly: GoDynamicTypeLift("7374727563747b736967416c676f2063727970746f2f783530392e5369676e6174757265416c676f726974686d3b2070656d4365727420737472696e677d", "ecdsaTestsᴛ1")]
// </ExportedTypeAliases>

// <InterfaceImplementations>
[assembly: GoImplement<bytes_package.Buffer, io_package.Writer>(Pointer = true)]
[assembly: GoImplement<global::go.crypto.x509_package.OID, encoding_package.BinaryMarshaler>]
[assembly: GoImplement<global::go.crypto.x509_package.OID, encoding_package.BinaryUnmarshaler>(Pointer = true)]
[assembly: GoImplement<global::go.crypto.x509_package.OID, encoding_package.TextMarshaler>]
[assembly: GoImplement<global::go.crypto.x509_package.OID, encoding_package.TextUnmarshaler>(Pointer = true)]
[assembly: GoImplement<global::go.crypto.x509_package.SystemRootsError, error>]
[assembly: GoImplement<go.crypto.ecdsa_package.PrivateKey, crypto_package.Signer>(Pointer = true)]
[assembly: GoImplement<go.crypto.ed25519_package.PrivateKey, crypto_package.Signer>]
[assembly: GoImplement<go.crypto.rsa_package.PrivateKey, crypto_package.Signer>(Pointer = true)]
[assembly: GoImplement<testing_package.T, testing_package.TB>(Pointer = true)]
// </InterfaceImplementations>

// <ImplicitConversions>
[assembly: GoImplicitConv<ecdsa.PrivateKey, ж<ecdsa.PrivateKey>>]
[assembly: GoImplicitConv<global::go.crypto.x509_package.CertPool, ж<global::go.crypto.x509_package.CertPool>>(Indirect = true)]
[assembly: GoImplicitConv<global::go.crypto.x509_package.Certificate, ж<global::go.crypto.x509_package.Certificate>>(Indirect = true)]
[assembly: GoImplicitConv<global::go.crypto.x509_package.Certificate, ж<global::go.crypto.x509_package.Certificate>>]
[assembly: GoImplicitConv<global::go.crypto.x509_package.CertificateRequest, ж<global::go.crypto.x509_package.CertificateRequest>>(Indirect = true)]
// </ImplicitConversions>

// Go source positions are recorded here, one `GoPositionMap` attribute per converted
// source file in this compilation, so that `runtime.Caller` and the tracebacks built on it
// can name the GO file and line a frame was converted from rather than the emitted C# one.
// Each record carries the Go file's identity and an encoded C#-line to Go-line table
// TOGETHER: a frame either has a record and reports a position that exists in the Go tree,
// or has none - golib, the BCL and hand-written conversions - and reports its own C# position.

// <GoSourcePositionMaps>
[assembly: go.GoPositionMap("crypto/x509/hybrid_pool_test.go", "hybrid_pool_test.cs", "AB8qgoKClIKUAAoYgoKCgoKClIKClMyCgpSEAAcQgoKUgoKUgoKWgoKUhIKCloSCgpbcgoKUgoKWgoI=", "", "", "73=NewInt/1/1/1,Now/1/2/5,Add/1/2/5,Now/2/2/6,Add/2/2/6;107=NewInt/1/1/1,Now/1/2/2,Add/1/2/2,Now/2/2/3,Add/2/2/3")]
// </GoSourcePositionMaps>

namespace go.crypto;

[GoPackage("x509_test")]
public static partial class x509_test_package
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
    [GoInit] internal static void initᴛᴛimportꓸbytes() => builtin.initPackage(typeof(bytes_package));
    [GoInit] internal static void initᴛᴛimportꓸcrypto() => builtin.initPackage(typeof(crypto_package));
    [GoInit] internal static void initᴛᴛimportꓸcryptoꓸdsa() => builtin.initPackage(typeof(go.crypto.dsa_package));
    [GoInit] internal static void initᴛᴛimportꓸcryptoꓸecdh() => builtin.initPackage(typeof(go.crypto.ecdh_package));
    [GoInit] internal static void initᴛᴛimportꓸcryptoꓸecdsa() => builtin.initPackage(typeof(go.crypto.ecdsa_package));
    [GoInit] internal static void initᴛᴛimportꓸcryptoꓸed25519() => builtin.initPackage(typeof(go.crypto.ed25519_package));
    [GoInit] internal static void initᴛᴛimportꓸcryptoꓸelliptic() => builtin.initPackage(typeof(go.crypto.elliptic_package));
    [GoInit] internal static void initᴛᴛimportꓸcryptoꓸrand() => builtin.initPackage(typeof(go.crypto.rand_package));
    [GoInit] internal static void initᴛᴛimportꓸcryptoꓸrsa() => builtin.initPackage(typeof(go.crypto.rsa_package));
    [GoInit] internal static void initᴛᴛimportꓸcryptoꓸsha256() => builtin.initPackage(typeof(go.crypto.sha256_package));
    [GoInit] internal static void initᴛᴛimportꓸcryptoꓸsha512() => builtin.initPackage(typeof(go.crypto.sha512_package));
    [GoInit] internal static void initᴛᴛimportꓸcryptoꓸtls() => builtin.initPackage(typeof(go.crypto.tls_package));
    [GoInit] internal static void initᴛᴛimportꓸcryptoꓸx509() => builtin.initPackage(typeof(go.crypto.x509_package));
    [GoInit] internal static void initᴛᴛimportꓸcryptoꓸx509ꓸpkix() => builtin.initPackage(typeof(go.crypto.x509.pkix_package));
    [GoInit] internal static void initᴛᴛimportꓸencodingꓸasn1() => builtin.initPackage(typeof(go.encoding.asn1_package));
    [GoInit] internal static void initᴛᴛimportꓸencodingꓸbase64() => builtin.initPackage(typeof(go.encoding.base64_package));
    [GoInit] internal static void initᴛᴛimportꓸencodingꓸgob() => builtin.initPackage(typeof(go.encoding.gob_package));
    [GoInit] internal static void initᴛᴛimportꓸencodingꓸhex() => builtin.initPackage(typeof(go.encoding.hex_package));
    [GoInit] internal static void initᴛᴛimportꓸencodingꓸjson() => builtin.initPackage(typeof(go.encoding.json_package));
    [GoInit] internal static void initᴛᴛimportꓸencodingꓸpem() => builtin.initPackage(typeof(go.encoding.pem_package));
    [GoInit] internal static void initᴛᴛimportꓸerrors() => builtin.initPackage(typeof(errors_package));
    [GoInit] internal static void initᴛᴛimportꓸfmt() => builtin.initPackage(typeof(fmt_package));
    [GoInit] internal static void initᴛᴛimportꓸinternalꓸtestenv() => builtin.initPackage(typeof(go.@internal.testenv_package));
    [GoInit] internal static void initᴛᴛimportꓸio() => builtin.initPackage(typeof(io_package));
    [GoInit] internal static void initᴛᴛimportꓸlog() => builtin.initPackage(typeof(log_package));
    [GoInit] internal static void initᴛᴛimportꓸmath() => builtin.initPackage(typeof(math_package));
    [GoInit] internal static void initᴛᴛimportꓸmathꓸbig() => builtin.initPackage(typeof(math.big_package));
    [GoInit] internal static void initᴛᴛimportꓸnet() => builtin.initPackage(typeof(net_package));
    [GoInit] internal static void initᴛᴛimportꓸnetꓸurl() => builtin.initPackage(typeof(go.net.url_package));
    [GoInit] internal static void initᴛᴛimportꓸos() => builtin.initPackage(typeof(os_package));
    [GoInit] internal static void initᴛᴛimportꓸosꓸexec() => builtin.initPackage(typeof(go.os.exec_package));
    [GoInit] internal static void initᴛᴛimportꓸpathꓸfilepath() => builtin.initPackage(typeof(path.filepath_package));
    [GoInit] internal static void initᴛᴛimportꓸreflect() => builtin.initPackage(typeof(reflect_package));
    [GoInit] internal static void initᴛᴛimportꓸruntime() => builtin.initPackage(typeof(runtime_package));
    [GoInit] internal static void initᴛᴛimportꓸslices() => builtin.initPackage(typeof(slices_package));
    [GoInit] internal static void initᴛᴛimportꓸstrconv() => builtin.initPackage(typeof(strconv_package));
    [GoInit] internal static void initᴛᴛimportꓸstrings() => builtin.initPackage(typeof(strings_package));
    [GoInit] internal static void initᴛᴛimportꓸsync() => builtin.initPackage(typeof(sync_package));
    [GoInit] internal static void initᴛᴛimportꓸtesting() => builtin.initPackage(typeof(testing_package));
    [GoInit] internal static void initᴛᴛimportꓸtime() => builtin.initPackage(typeof(time_package));
    // </ImportInitializers>
    // Go runs every `init` in the package under test - the production files' included -
    // before the first test. The production package is a REFERENCED assembly here, whose
    // module constructor .NET would not run until something in it is touched, so that
    // initialization is forced before anything else in this test module runs.
    [GoInit] internal static void initᴛᴛproduction() {
        builtin.initPackage(typeof(global::go.crypto.x509_package));
    }
}
