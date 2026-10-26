// go2cs metadata anchor for a production-reference test project: the test assembly
// REFERENCES the colocated production project instead of
// recompiling its sources, so the production assembly is the single identity for the
// production types and no production class partial may be declared here. The first —
// and only — class is the test metadata class the go2cs-gen generators anchor
// generated adapters and partials to.
global using static global::go.crypto.ecdh_package;

// <ImportedTypeAliases>
global using cryptoꓸDecrypterOpts = object;
global using cryptoꓸPrivateKey = object;
global using cryptoꓸPublicKey = object;
global using ecdhꓸCurve = go.crypto.ecdh_package.ΔCurve;
global using ecdhꓸPublicKey = go.crypto.ecdh_package.ΔPublicKey;
global using osꓸDirEntry = go.io.fs_package.DirEntry;
global using osꓸFileInfo = go.io.fs_package.FileInfo;
global using osꓸFileMode = go.io.fs_package.FileMode;
global using osꓸPathError = go.io.fs_package.PathError;
global using osꓸSignal = go.os_package.ΔSignal;
// </ImportedTypeAliases>

using go;
using static global::go.crypto.ecdh_test_package;

// <ExportedTypeAliases>
[assembly: GoDynamicTypeLift("696e746572666163657b457175616c28782063727970746f2e507269766174654b65792920626f6f6c3b205075626c696328292063727970746f2e5075626c69634b65797d", "_ᴛ2")]
[assembly: GoDynamicTypeLift("696e746572666163657b457175616c28782063727970746f2e5075626c69634b65792920626f6f6c7d", "_ᴛ1")]
[assembly: GoDynamicTypeLift("7374727563747b507269766174654b657920737472696e673b205075626c69634b657920737472696e673b20506565725075626c69634b657920737472696e673b2053686172656453656372657420737472696e677d", "vectorsᴛ1")]
// </ExportedTypeAliases>

// <InterfaceImplementations>
[assembly: GoImplement<countingReader, io_package.Reader>(Pointer = true)]
[assembly: GoImplement<go.crypto.cipher_package.StreamReader, io_package.Reader>]
[assembly: GoImplement<go.crypto.ecdh_package.PrivateKey, _ᴛ2>(Pointer = true)]
[assembly: GoImplement<go.crypto.ecdh_package.ΔPublicKey, _ᴛ1>(Pointer = true)]
[assembly: GoImplement<testing_package.T, testing_package.TB>(Pointer = true)]
[assembly: GoImplement<zr, io_package.Reader>]
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
[assembly: go.GoPositionMap("crypto/ecdh/ecdh_test.go", "ecdh_test.cs", "ACc65uqCgoKClIKCpoKClIKUgqaCgpSClIKmgoKUgoKmggAJFIKCgqaCgoKCggAHEICSAAoKACJUgoKCgoKUgpSCgpSCgpSCAAgMgoKClKaCgoKCAAgKgoKCgpSgkqDmgoKClIKClIKClIK4ADVsgoKCgoKSgpKC3AAzaIKCgoKCkoKSggALDIKAkoCSgJKAtoKCgoKUuIKClIKClJSCgoKUgoKClIKClMqCgJKAkoCSgOySgqYAJTCygqaCgoKCppKCgoKClKaCgIK6ooKCgoKUgqaCAAsIggAGFIKCgqaygpSigoKUgoKC", "38-82:1;97-112:1;158-179:1;191-196:1;205-205:1;206-206:2;282-293:1;349-360:1;364-364:1;365-365:2;366-366:3;367-367:4;371-406:1;410-410:1;411-411:2;412-412:3;413-413:4;460-468:1;514-524:1", "", "140=P256/1/1/6,P384/1/1/14,P521/1/1/25,X25519/1/1/34;194=hexDecode/1/1/1,Sum256/2/2/1;263=P256/1/1/1,Repeat/1/4/7,P384/1/1/15,Repeat/2/4/21,P521/1/1/29,Repeat/3/4/35,X25519/1/1/44,Repeat/4/4/50;334=P256/1/1/1,Repeat/1/3/5,P384/1/1/17,Repeat/2/3/21,P521/1/1/33,Repeat/3/3/37,X25519/1/1/49;585=P256/1/1/4,P384/1/1/5,P521/1/1/6,X25519/1/1/7")]
// </GoSourcePositionMaps>

namespace go.crypto;

[GoPackage("ecdh_test")]
public static partial class ecdh_test_package
{
    // C# nested types declared with no access modifier are always private, and the
    // Go type declarations in this package's converted sources are deliberately
    // bare so they read more like the original Go code. The real accessibility for
    // the types - public for a Go-exported name, internal otherwise - are defined
    // via declarations below.

    // <TypeAccessibility>
    internal partial interface _ᴛ1 {}
    internal partial interface _ᴛ2 {}
    internal partial struct TestMismatchedCurves_curves {}
    internal partial struct countingReader {}
    internal partial struct vectorsᴛ1 {}
    internal partial struct zr {}
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
    [GoInit] internal static void initᴛᴛimportꓸcryptoꓸrand() => builtin.initPackage(typeof(go.crypto.rand_package));
    [GoInit] internal static void initᴛᴛimportꓸcryptoꓸsha256() => builtin.initPackage(typeof(go.crypto.sha256_package));
    [GoInit] internal static void initᴛᴛimportꓸencodingꓸhex() => builtin.initPackage(typeof(encoding.hex_package));
    [GoInit] internal static void initᴛᴛimportꓸfmt() => builtin.initPackage(typeof(fmt_package));
    [GoInit] internal static void initᴛᴛimportꓸinternalꓸtestenv() => builtin.initPackage(typeof(go.@internal.testenv_package));
    [GoInit] internal static void initᴛᴛimportꓸio() => builtin.initPackage(typeof(io_package));
    [GoInit] internal static void initᴛᴛimportꓸos() => builtin.initPackage(typeof(os_package));
    [GoInit] internal static void initᴛᴛimportꓸpathꓸfilepath() => builtin.initPackage(typeof(path.filepath_package));
    [GoInit] internal static void initᴛᴛimportꓸregexp() => builtin.initPackage(typeof(regexp_package));
    [GoInit] internal static void initᴛᴛimportꓸstrings() => builtin.initPackage(typeof(strings_package));
    [GoInit] internal static void initᴛᴛimportꓸtesting() => builtin.initPackage(typeof(testing_package));
    [GoInit] internal static void initᴛᴛimportꓸvendorꓸgolang_orgꓸxꓸcryptoꓸchacha20() => builtin.initPackage(typeof(vendor.golang.org.x.crypto.chacha20_package));
    // </ImportInitializers>
    // Go runs every `init` in the package under test - the production files' included -
    // before the first test. The production package is a REFERENCED assembly here, whose
    // module constructor .NET would not run until something in it is touched, so that
    // initialization is forced before anything else in this test module runs.
    [GoInit] internal static void initᴛᴛproduction() {
        builtin.initPackage(typeof(global::go.crypto.ecdh_package));
    }
}
