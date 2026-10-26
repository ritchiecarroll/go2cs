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
global using x509ꓸOID = go.crypto.x509_package.ΔOID;
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
[assembly: GoImplement<testPrimeReader, io_package.Reader>(Pointer = true)]
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
[assembly: go.GoPositionMap("crypto/rsa/equal_test.go", "equal_test.cs", "ABIcgpSClIKUgpSCpoKClIKClIKUgqaCgpSCppKCgg==")]
[assembly: go.GoPositionMap("crypto/rsa/pkcs1v15_test.go", "pkcs1v15_test.cs", "ABwugoKCgpQABxAAEiaClJSmuIKCgoKUgoLcgoKUkoKmgoKCpoKClJSCgoKmgoKUpoKClLgAEiaCgoKCgoKUgoLKgoKCgoKUgqaCyoKUgoKClIKCggALGLiCgoKCgpSCgqaCgsqCgoKCgpSUgoL6goKCgoK4gpTulIKClIKUgIL4poKCpoKAgraCgsqCgoKClAALBoKIgoKmgoKC", "60-62:1;63-65:2;86-114:1")]
[assembly: go.GoPositionMap("crypto/rsa/pss_test.go", "pss_test.cs", "ACo0woKClKzClJTCgoKClIKClIKCgpS0grSCtLS2gILEpIKCgoK4gpSCgoKUgpSCtoKClIKClICCxsaoopSCgoKmAAkUgILIgoKCgpT2ogAOJIKCgoKUgoKCgoKmgoKCgpSCyriCgoKUgriClIKCuIKCgpSmgoKClKaCgoKU5oKCgoKmgoC42oCmyIKUgoK4gII=", "42-73:1")]
[assembly: go.GoPositionMap("crypto/rsa/rsa_test.go", "rsa_test.cs", "ACdAgoKClIKCgoKClJSCgpSAgqTKgoKCgqaCgpSmgoKCgqaCgpSmgoKCgoKCpoKCgpTKpoKCgoKC6JSClIKCgoKUgIIADgqCuIqmwoKCuICCpIKmgoKCgqaCgoKUgrSkgpSCgoKmgKKCgpSCpsiUgpSCkoKmpoKCgoKUkoKCgoKUgIKkAA4KsoKCpoKCgpKClIKCgpSCgpSCuIKCgpKClIKCgpSCuIKCgqaCgoKSgpSCgoKUgoKClIKCgoKUpoKCgpKClIKCgpSCgoKUgoKCgpSmgoKCkoKUgoKClIKCgpSCgoKClMqCgoKUgoKUgoKClIKCpoKCgpSCgpSCuIKClIKClIL4goKCgpSCppKCpoKSkpKSgoKSkpKSkpKmgKSCgoKCgpSUgoKUxpQADBQADBYAESAAHjqkygAHEAAKFgAqUgA2aoKAkoCSgLailIKCgqaCgoKCgpSClLiCgoKUgoKCgpTKgoKUgoKCpoKCgoKClIKUyoKCgpSCgoKClAAJCoKAkoKUgriCuKKUgoKCgoKUuIKCgoKCpoKCgoLcgoKUgoKCgoKUyoKCgoKCpoKCgoLcgoKCgoKAggAJDIKCgoKUgoKAggAMGAAJEIKmgoKCgoKmgoKUgpSCAA8ggoKCgoKkgoKCgpSC3IKUgoKCgoKCgoKUgoKCkoK4goKSgqaCyoKUgpSCgoKCgoKCkqSUgpKCuIKCgoKCgoKCgpSCgoKCgpSCgoKUgriCgoKUgoKClIK4goKClIKCgpSC7gCfAaQClAAAEoKClAAnBpQAAiQAASCClIIACAaCgoKSgpSCgpKCgpSUgoKSgoKUAA0IgoKmgoKmgoK4goKUgqaCgqaCgriCgqaCgqaCgriCgqaCgqaCgriCgqaCgqaCgriCgqaCggAICoKCgoKCgIKkgoCC", "38-54:1;148-152:1;187-195:1;206-209:1;221-231:2;412-420:1;421-424:2;649-649:1;650-650:2;651-651:3;678-690:1;694-715:1;719-731:1;735-735:1;736-738:2;739-741:3;744-746:4;764-778:1;782-794:1;798-812:1;816-824:1;828-839:1;1241-1246:1;1247-1255:2;1256-1264:3;1272-1276:1;1273-1275:1.1;1277-1281:2;1278-1280:2.1;1283-1290:3;1287-1289:3.1;1291-1295:4;1292-1294:4.1;1296-1300:5;1297-1299:5.1;1302-1306:6;1303-1305:6.1;1307-1311:7;1308-1310:7.1;1312-1316:8;1313-1315:8.1;1318-1322:9;1319-1321:9.1;1323-1327:10;1324-1326:10.1;1328-1332:11;1329-1331:11.1;1334-1338:12;1335-1337:12.1;1339-1343:13;1340-1342:13.1;1344-1348:14;1345-1347:14.1;1350-1354:15;1351-1353:15.1;1355-1359:16;1356-1358:16.1")]
// </GoSourcePositionMaps>

namespace go.crypto;

[GoPackage("rsa_test")]
public static partial class rsa_test_package
{
    // C# nested types declared with no access modifier are always private, and the
    // Go type declarations in this package's converted sources are deliberately
    // bare so they read more like the original Go code. The real accessibility for
    // the types - public for a Go-exported name, internal otherwise - are defined
    // via declarations below.

    // <TypeAccessibility>
    internal partial struct TestPSSSigning_type {}
    internal partial struct signPKCS1v15Test {}
    internal partial struct testEncryptOAEPMessage {}
    internal partial struct testEncryptOAEPStruct {}
    internal partial struct testPrimeReader {}
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
    [GoInit] internal static void initᴛᴛimportꓸslices() => builtin.initPackage(typeof(slices_package));
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
