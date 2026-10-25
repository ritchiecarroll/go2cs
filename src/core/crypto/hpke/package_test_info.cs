// go2cs metadata anchor for a production-reference test project: the test assembly
// REFERENCES the colocated production project instead of
// recompiling its sources, so the production assembly is the single identity for the
// production types and no production class partial may be declared here. The first —
// and only — class is the test metadata class the go2cs-gen generators anchor
// generated adapters and partials to.
global using static global::go.crypto.hpke_package;

// <ImportedTypeAliases>
global using cryptoꓸDecrypterOpts = object;
global using cryptoꓸPrivateKey = object;
global using cryptoꓸPublicKey = object;
global using ecdhꓸCurve = go.crypto.ecdh_package.ΔCurve;
global using ecdhꓸPublicKey = go.crypto.ecdh_package.ΔPublicKey;
global using jsonꓸToken = object;
global using jsonꓸΔToken = object;
global using osꓸDirEntry = go.io.fs_package.DirEntry;
global using osꓸFileInfo = go.io.fs_package.FileInfo;
global using osꓸFileMode = go.io.fs_package.FileMode;
global using osꓸPathError = go.io.fs_package.PathError;
global using osꓸSignal = go.os_package.ΔSignal;
// </ImportedTypeAliases>

using go;
using static global::go.crypto.hpke_internal_test_package;

// <ExportedTypeAliases>
[assembly: GoTypeAlias("KEM", "ΔKEM")]
[assembly: GoTypeAlias("PublicKey", "ΔPublicKey")]
// </ExportedTypeAliases>

// <InterfaceImplementations>
[assembly: GoImplement<go.crypto.sha3_package.SHAKE, io_package.Reader>(Pointer = true)]
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
[assembly: go.GoPositionMap("crypto/hpke/hpke_test.go", "hpke_test.cs", "ABcqypaogoKClIKokoKCgqaCgoKmuIKCgpQAEw6CAAEWnqyCsoKClIKClIKClICCkoKkgoKUgIK2gtKC0oKClIKClIKmgoKUgoKCpoKClIKClILugoKCgpSUgoKCgsqCgoKClIKUgoKCgpSClIKmgoKClJSCgoKClIKUgoKCgpSClIKCgoKUgpSCAAgSgoKCgpTmgoKUggAqCIKCgpQAADiAgraylJKClIKUgpSCpoKClIKmgoKUgqaCgpSCpoKCgpSClIKmgpSCgoKUgqaCgqaCgoKUgpSCgqaClIKCgpSCgpSClIKmgqaCgoKUgoKUgpSCpoKCpoKCgoKCgpSCgoKUgqaCgoKCpIKCgoKUgoKUgqaCgpSCuICCpICCyIKCgoKCgpSCgoKUgqaCgoKCpoKClIKClIKmgoKUggAHEIKCgoCCpIKCgIKkpoKCgpSUgoKUuIKigoKUtoKigoKUtoKCgoKUlqKCgpTagoKCgpSWooKClLaCgoKClJaigoKUtgAVCIKClIKUgpSClIKUgpSClIKUgpSClIKUgpSClIKUgpSC", "98-245:1;125-243:1.1;127-241:1.1.1;166-173:1.1.1.1;174-181:1.1.1.2;259-261:1;262-264:2;307-530:1;549-552:1;559-561:2;564-570:3;573-579:4;587-589:5;590-596:6;606-608:7;609-615:8;623-625:9;626-632:10", "", "87=P256/1/1/1,DHKEM/1/4/1,P384/1/1/2,DHKEM/2/4/2,P521/1/1/3,DHKEM/3/4/3,X25519/1/1/4,DHKEM/4/4/4,MLKEM768/1/1/5,MLKEM1024/1/1/6,MLKEM768P256/1/1/7,MLKEM1024P384/1/1/8,MLKEM768X25519/1/1/9;89=HKDFSHA256/1/1/1,HKDFSHA384/1/1/2,HKDFSHA512/1/1/3,SHAKE128/1/1/4,SHAKE256/1/1/5;91=AES128GCM/1/1/1,AES256GCM/1/1/2,ChaCha20Poly1305/1/1/3")]
// </GoSourcePositionMaps>

namespace go.crypto;

[GoPackage("hpke")]
public static partial class hpke_internal_test_package
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
    [GoInit] internal static void initᴛᴛimportꓸcryptoꓸecdh() => builtin.initPackage(typeof(go.crypto.ecdh_package));
    [GoInit] internal static void initᴛᴛimportꓸcryptoꓸmlkem() => builtin.initPackage(typeof(go.crypto.mlkem_package));
    [GoInit] internal static void initᴛᴛimportꓸcryptoꓸmlkemꓸmlkemtest() => builtin.initPackage(typeof(go.crypto.mlkem.mlkemtest_package));
    [GoInit] internal static void initᴛᴛimportꓸcryptoꓸsha3() => builtin.initPackage(typeof(go.crypto.sha3_package));
    [GoInit] internal static void initᴛᴛimportꓸencodingꓸhex() => builtin.initPackage(typeof(encoding.hex_package));
    [GoInit] internal static void initᴛᴛimportꓸencodingꓸjson() => builtin.initPackage(typeof(encoding.json_package));
    [GoInit] internal static void initᴛᴛimportꓸfmt() => builtin.initPackage(typeof(fmt_package));
    [GoInit] internal static void initᴛᴛimportꓸio() => builtin.initPackage(typeof(io_package));
    [GoInit] internal static void initᴛᴛimportꓸos() => builtin.initPackage(typeof(os_package));
    [GoInit] internal static void initᴛᴛimportꓸtesting() => builtin.initPackage(typeof(testing_package));
    // </ImportInitializers>
    // Go runs every `init` in the package under test - the production files' included -
    // before the first test. The production package is a REFERENCED assembly here, whose
    // module constructor .NET would not run until something in it is touched, so that
    // initialization is forced before anything else in this test module runs.
    [GoInit] internal static void initᴛᴛproduction() {
        builtin.initPackage(typeof(global::go.crypto.hpke_package));
    }
}
