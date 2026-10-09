// go2cs metadata anchor for a production-reference test project: the test assembly
// REFERENCES the colocated production project instead of
// recompiling its sources, so the production assembly is the single identity for the
// production types and no production class partial may be declared here. The first —
// and only — class is the test metadata class the go2cs-gen generators anchor
// generated adapters and partials to.
global using static global::go.hash_package;

// <ImportedTypeAliases>
// </ImportedTypeAliases>

using go;
using static global::go.hash_test_package;

// <ExportedTypeAliases>
[assembly: GoDynamicTypeLift("7374727563747b6e616d6520737472696e673b206e65772066756e63282920686173682e486173683b20676f6c64656e205b5d627974657d", "marshalTestsᴛ1")]
// </ExportedTypeAliases>

// <InterfaceImplementations>
[assembly: GoImplement<hash_package.Hash32, hash_package.Hash>]
[assembly: GoImplement<hash_package.Hash64, hash_package.Hash>]
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
[assembly: go.GoPositionMap("hash/marshal_test.go", "marshal_test.cs", "ABs2goKClAAIBgATMIKykoKCloKChIKCgoKUgoKUgoKUgpSCgpSAgqSCgoKCgpSC", "61-105:1", "", "42=fromHex/1/17/5,fromHex/2/17/6,fromHex/3/17/7,fromHex/4/17/8,fromHex/5/17/9,fromHex/6/17/10,fromHex/7/17/11,fromHex/8/17/12,fromHex/9/17/13,fromHex/10/17/14,fromHex/11/17/15,fromHex/12/17/16,fromHex/13/17/17,fromHex/14/17/18,fromHex/15/17/19,fromHex/16/17/20,fromHex/17/17/21")]
// </GoSourcePositionMaps>

namespace go;

[GoPackage("hash_test")]
public static partial class hash_test_package
{
    // C# nested types declared with no access modifier are always private, and the
    // Go type declarations in this package's converted sources are deliberately
    // bare so they read more like the original Go code. The real accessibility for
    // the types - public for a Go-exported name, internal otherwise - are defined
    // via declarations below.

    // <TypeAccessibility>
    internal partial struct marshalTestsᴛ1 {}
    // </TypeAccessibility>

    // Go initializes an imported package before the importing package, for every import
    // form - not only the blank one. .NET would never load an assembly nothing has touched
    // yet, so each import that initializes anything is forced below: once per assembly, and
    // ahead of this package's own `init` functions, which this file being the first compile
    // item of the project guarantees.

    // <ImportInitializers>
    [GoInit] internal static void initᴛᴛimportꓸbytes() => builtin.initPackage(typeof(bytes_package));
    [GoInit] internal static void initᴛᴛimportꓸcryptoꓸmd5() => builtin.initPackage(typeof(crypto.md5_package));
    [GoInit] internal static void initᴛᴛimportꓸcryptoꓸsha1() => builtin.initPackage(typeof(crypto.sha1_package));
    [GoInit] internal static void initᴛᴛimportꓸcryptoꓸsha256() => builtin.initPackage(typeof(crypto.sha256_package));
    [GoInit] internal static void initᴛᴛimportꓸcryptoꓸsha512() => builtin.initPackage(typeof(crypto.sha512_package));
    [GoInit] internal static void initᴛᴛimportꓸencodingꓸhex() => builtin.initPackage(typeof(go.encoding.hex_package));
    [GoInit] internal static void initᴛᴛimportꓸhash() => builtin.initPackage(typeof(hash_package));
    [GoInit] internal static void initᴛᴛimportꓸhashꓸadler32() => builtin.initPackage(typeof(go.hash.adler32_package));
    [GoInit] internal static void initᴛᴛimportꓸhashꓸcrc32() => builtin.initPackage(typeof(go.hash.crc32_package));
    [GoInit] internal static void initᴛᴛimportꓸhashꓸcrc64() => builtin.initPackage(typeof(go.hash.crc64_package));
    [GoInit] internal static void initᴛᴛimportꓸhashꓸfnv() => builtin.initPackage(typeof(go.hash.fnv_package));
    [GoInit] internal static void initᴛᴛimportꓸtesting() => builtin.initPackage(typeof(testing_package));
    // </ImportInitializers>
    // Go runs every `init` in the package under test - the production files' included -
    // before the first test. The production package is a REFERENCED assembly here, whose
    // module constructor .NET would not run until something in it is touched, so that
    // initialization is forced before anything else in this test module runs.
    [GoInit] internal static void initᴛᴛproduction() {
        builtin.initPackage(typeof(global::go.hash_package));
    }
}
