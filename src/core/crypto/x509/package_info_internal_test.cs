// go2cs metadata anchor for the INTERNAL (white-box bridge) test class: GoImplement /
// GoImplicitConv attributes whose GENERATED code must merge with a bridge-declared type
// anchor here — the source generators host output in the first class of the
// attribute-bearing file, and only this file's first class is the bridge. Records for
// production and external-test types stay in package_test_info.cs.

// <ImportedTypeAliases>
using ecdsa = go.crypto.ecdsa_package;
using testing = go.testing_package;
// </ImportedTypeAliases>

using go;
using static go.crypto.x509_package;
using static go.crypto.x509_internal_test_package;

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
[assembly: go.GoPositionMap("crypto/x509/cert_pool_test.go", "cert_pool_test.cs", "ABESooKEgoKCgoKCgpSCgpSCgoKUggA9iAGykoKC", "101-106:1")]
[assembly: go.GoPositionMap("crypto/x509/name_constraints_test.go", "name_constraints_test.cs", "AMYBfgCaCqAZooKEAAoYgIKmgpSCgpaCgpYACQaigoQACxqClLaCgpS2goKUtraCgpS8spYADxi4goCCpoKWgoKWpoKSopKSkpKCloKCpKaCkoKE3KLSgpS2goKUtra2uJaClMamloKCqIKCqIKCgpaCgpaAgqamwoKUpKSkpKSkpLjmgqSCgpSospKCgqgAABDihIKCgpaCloSCgoKChIKCgpaCloKWgoSCgrqCgoKCgqiCgIKkgoLc3ISCgoKUpoKklKiCgoKClIKAgoK2loIACAyCgoKWgoKClqbihIKClJSEgoKClJSWgoKUlISCgoKEggALBgAWOIKCgoSCgpaCgpaCloIADgqCgoKWmgAKHoKCloKogoKUggAIDLjMgoKWgoSCgpaAgg==", "1803-1811:1;1832-1857:1;1859-1869:2;1928-1934:1;1938-2062:2;2045-2049:2.1;2173-2176:1;2178-2180:2")]
[assembly: go.GoPositionMap("crypto/x509/oid_test.go", "oid_test.cs", "ABUcABtKgoKCgoKWgpaAgqaCgoKClJaCgIKCpoKWgoKClIIACgyCAAcYgoKCloKCloKCggAKCoIABxqCgILcgoKC9oIAECyCgoKUAAUWgoKCgpaCgoKCloKWgoKWgoKWgoKCloKCgoKCloKCloKAgqaCgpaCgoKCloKAgqaCggAKCoKCABxGgoCC2oKCgoSCgpaCyqKCgoKClIKAgg==")]
[assembly: go.GoPositionMap("crypto/x509/parser_test.go", "parser_test.cs", "AB0oogBDlgGykoKCpJSCADNagoKCgpSClIKUgpSClILogriCgoKUgoKCAAwMogAtYpKCgqaCgpSCgtzYgoKU3LiCgpSCgriCgoI=", "97-107:1;178-188:1;242-256:1;289-292:1")]
[assembly: go.GoPositionMap("crypto/x509/pem_decrypt_test.go", "pem_decrypt_test.cs", "ABoggoKCgoKUgoKClICCpIKClIIACwqCgoKCgpSCgoKClIKUgpSClIKCgpSCAA0KAKgBsAIAEBKmgoKClIKAgsiA")]
[assembly: go.GoPositionMap("crypto/x509/pkcs8_test.go", "pkcs8_test.cs", "AEJ0ogAoXoKCgoKUgoKClIKClICCgqSCgoKUgoKWgIKCgoKUlIKCgpSCggAPEgAEEIKCgoKC")]
[assembly: go.GoPositionMap("crypto/x509/pkits_test.go", "pkits_test.cs", "ABEeABcQ3oQAABSCgpSAgqYAWbYBsoKUooKCgoKUgoKUlISCgoKClJbcgoKUlII=", "145-184:1")]
[assembly: go.GoPositionMap("crypto/x509/platform_test.go", "platform_test.cs", "ADZMgoKWgoKUgoKCloKClIKCgpaAgqaEAHWEAoKCloKSwoKCgpSCgpSCgpaCgpSClIKWgoKkloKCpKQ=", "206-247:1")]
[assembly: go.GoPositionMap("crypto/x509/root_test.go", "root_test.cs", "AA0WooKCpoKiABIExoKCkoCmAClkspKCgoKUgpSClJaChISCpNg=", "12-16:1;27-27:1;81-106:2")]
[assembly: go.GoPositionMap("crypto/x509/sec1_test.go", "sec1_test.cs", "ABocAAUeooKCgoKUgoKUgoIACBAABBCCgoKCgg==")]
[assembly: go.GoPositionMap("crypto/x509/verify_test.go", "verify_test.cs", "AF9gAIkCigWCgoCCpILKgoCCyIKAgsiCgoKUgriigpSAgsiCgILIgoCCyIKAgviCgoKUpqLegoKCgoK6goKCqIKCloSCgpSUgoKCpqiCgpaCgqbegoKCgoKogrqCgoKCuIKCgoLugrKSyoKClrKSgpTKgoKCgpSUpqKmgsiAgpSkgoKUgoKUALUG3gsABRSCspKCgpSCgpTKgoIACwwADiaigoSCgpaCgpaCAJMB+gGCABAogoCCpICC2qKCgpaChAAJFoKCloKClIKClgAJBoKCuoSCgpSEgoKClJaCgpaCuISCuIKCloSCgpSEgoKCgpSWgoKWgoC4pNaigpaAlMyAgqaCgpaEgoCCxOSCgoKC+IKCgoKAgqSUgoKClgANBoKCloKAgoCClMaqopKClpKCgoKCqIKClqKmgoKCggAYMqKEgoKU3IKCpJSCloKCloKClIKClKaChIKCgoKCgpSCgpSUgpaCgoKCgpSCgpaCgoKSgpSUgoKClIKUuqaCgoKCgpSUggAMBqIAOHwAS5gBggCxAeQCAB9AAAcQAAcQABEkAAoWAAcQABMoggAHEAAMGoKCpgAmTIKCpgATKLKSgriClIKUgoIAFgyCAEKeAYKClrKigpKClISCgrKSgpSClpKCloKCpAAODIKCgpYAIESiAAcQgoKUgoKUgoSCgqTegriCgriEgoCC+IKCgpTugoKUgoKWgIIAJQgACBCCgoKChIKCgpSCgoKUloKCgoKCgoKCgoKCgoKCgoKCgoKCgoQAzQGwA4KCgpSUAFCiAZayktyCAAkMgoKCgpSCgoKUloKEhIKChNyCpOiCgoKUAAgSgoKUgoKWggAIEoKClIKCloKCloKChIKCggAHFvKCgoCCpoKCgIKkhISClIKClgAGEIKCgpSEggAIEsqCgpQ=", "374-381:1;489-500:1;542-544:1;554-559:1;1359-1377:1;1690-1690:1;1765-1772:1;1779-1789:2;1984-1986:1;2060-2063:2;2239-2241:3;2271-2273:4;2279-2281:5;2287-2289:6;2305-2307:7;2316-2318:8;2324-2326:9;2344-2347:10;2353-2355:11;2366-2373:12;2409-2416:13;2436-2452:14;2542-2572:1;2544-2547:1.1;2553-2556:1.2;2561-2564:1.3;2616-2642:1;2706-2717:1;3050-3060:2;3065-3076:1")]
[assembly: go.GoPositionMap("crypto/x509/x509_test.go", "x509_test.cs", "ADhSgoKCgoKUirqCgIK4AAYcgoKUgpSCAAkIhIKCgoKCuIKCgpS4griygoKCloKCgpSClPaCgoKCgqaCgoKCpoKCgoIAESAAK0CEgoKCgoLsgoSCgILIgoKCpoKCgqaCgoKmAAsYggALGoSCgoKUjpaCACUIggABOISShIKCloSCgqaCAAgIgriCgoKUgsyCgqSUgoKClIKWAFGCAYKCgoKCpKaCgpSCggAMFgAoUqKCgoKC+pTcgoK63IKClqaCgpSCgpSCguiCgoKClIKCloKCloCCpoKAggAKCIKCkoKUgpSClIIACgiCgoKWgoKWgIKmgoIABWKCgoKUpoKCgpQADwaihIKCloKClgAJIIKChIKCADiEAYKCgpaCgoKWgpaCloKWgpaCloKWgpaCloKWgpaCloKCgrqCgoKCpoKWgpaCloKWgpaCloKWgpaCloKWgpaCloKWgpaCgoIAXaIBAAUSgoKCgoKClICCpICCpICCpICCACVAggAHEIKCgpSClIKClIi4goKCgpSCuIKCgoKmgIIASIgBgoKCgoKWgoKCloCCggAyYIKCgpaCgpaClIKClIKWgIIATJABAA4IgoKCgoSCgoKEAAMShIKChAAKFgAJFoKCgoKWgoKClILcgoKCgpSmooKCgoKUgoKCloK8koKCgpSCuKKCgoKClIKCgpaCAAgMkoKUhICCAA4QooSCgpaCgpaCgpaCgpYABxqCAAoWgoKCloKCgpaCgoKWgqSkpKTKgoKCgpaCgpamgoKClgAQJISCloLOAAscgoCCpoaWgoKouoSCuIKCgoKCloKWgpaCloKCgoKmggAQCoakgoKClgADEICCpoKCloLOooKCgoKWgoKCluaCAAkWgIKmgoCCpoKAgqaCgoCCpoKAgsiiAAkYgoCCpIKWgoKAgqSCloKCgoCCpIK4ggAKGoCCpoKAgsiiAAkWgIKmgoCCpoKAggAJCIIACRyCgILagoCCAAkIggAIGIKAggAUJAAWLoKCgoKUgIKkgIKkgILIgoKCgpSAgqSAgqSAggAbKoKCgIKkABQkgoKAgqQALVCCgoKCloCCpoCCAAoIgoKUgoKUgoKUgpSAABYqpIIAHDCCgoKCloKAggAJCIKCgpSCgqjagpaU3gAtZIKAgriCAAgIqL4AECqCgIIAHDSCgoKCloCCAB0wgoKCgpaCggAuWriCgILIgriCgoKWgoKWgIKmgoKmgIIACQqCyoKCloKCloKCgpSoACtUgoKCgpa4gIIACA4ABBCCgoKCggAMCqKCgpSCgrqCgoSCgoKEAMMClgWykoKCpKSUgpaCgpaEqIKCuIK4goKCpoKmgsyClIKClLiCpoKClLiCAAcSgoKClILukoKCgqimlIKogqaCpoLuooKElKSkpgAKGLqCgoKWgoKWloKClIKC+oKEzIKCABMIgoKCloKClIQAGT6yooKCpLoAECyCgoKUgoKkAAkKori+goKU3IKClLqCgoKigoKCAAsWgqaC5oK4goKCpOiCksqCgriCgoKUpqKCgpSq0oKUgpSElIKCgpSCgpSCgpSCqOaCgoKUgoKU3ISClIKUhpSCAAgIggBu3AGykoKCgoKCgu6CgoKClIIAHziigoKClIKClIKCACVIooKClIKCAFiuAbSCgpSCggAnTKKCgpSCgpSC6KKEAAgSgoKUgoKWgoKigqbcgpSCgpaAgtyCgoKWgoK4goKCgoKUgoKCggAJCIKCgpSCgpQAQY4BAAcQsrKCgpSCgpSCgqQACAyCgoKUAAcQgoKWgoIAERqCgoKCuIKCgpQABxCCgoIAFSKigoKUgoIAEx6igoKUgoIAEx6igoKUgoLogoQABxCoqIKCloKCloKWgviCgpYACBKCgoKWgoKWgpaChIKCloKCloK4poKCgriCAA0agoKUgoKCuIIADRqCgpSCgoK4ggANGoKClIKCgg==", "134-140:1;141-147:2;148-154:3;2895-3023:1;3028-3069:1;3141-3148:1;3198-3204:1;3208-3214:2;3221-3228:3;3466-3476:1;3882-3897:1")]
// </GoSourcePositionMaps>

namespace go.crypto;

[GoPackage("x509")]
public static partial class x509_internal_test_package
{
    // C# nested types declared with no access modifier are always private, and the
    // `[GoType]` declarations in this package's converted sources are deliberately
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
}
