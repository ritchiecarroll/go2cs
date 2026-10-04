// go2cs metadata anchor for the INTERNAL (white-box bridge) test class: GoImplement /
// GoImplicitConv attributes whose GENERATED code must merge with a bridge-declared type
// anchor here — the source generators host output in the first class of the
// attribute-bearing file, and only this file's first class is the bridge. Records for
// production and external-test types stay in package_test_info.cs.

// <ImportedTypeAliases>
using big = go.math.big_package;
// </ImportedTypeAliases>

using go;
using static go.crypto.ecdsa_package;
using static go.crypto.ecdsa_internal_test_package;

// <ExportedTypeAliases>
// </ExportedTypeAliases>

// <InterfaceImplementations>
[assembly: GoImplement<readerFunc, io_package.Reader>]
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
[assembly: go.GoPositionMap("crypto/ecdsa/ecdsa_test.go", "ecdsa_test.cs", "ACc2ggAGFIKUsoKikoIABRTCkqaCpoKCgpSCuIKmgoSCgoKCloKWgoK4gqaChIKCgoKWgpaCgriCpoKEgoKCgpaCgoKClpSWgtyApIKCpoKmgoSCgoKCloKCgpaCloK4goKClKaC9t6CloKCloSCgoKCkoSCgoKClJSUgpSEgpaCgoSUpKSkpKaUpKSkpKSmloKWlICC1rS0tLSCgoKCgsbqgqaCgoKWgoKShIK4gqaChIKCqIKCqIK4gqaCgoKWgriCpoKEgoKUgoKWgoK4gqaChIKClIKCloKCuIKmgoSCgpSCgpaCggAMCIKC7gAHEAALGAAHEu4ABxCC7gAHEILuAAkUggAHEIKCgpSCgpSCAAgIggAEEIKCksqCgoKCgpSEgoKCgoKm+oKCgoKClIKCgpaCgoKC3IKCgoKCgoCC", "43-48:1;44-47:1.1;440-455:1;456-491:2;492-507:3;508-523:4;560-562:1;567-585:1;589-608:1;612-621:1")]
// </GoSourcePositionMaps>

namespace go.crypto;

[GoPackage("ecdsa")]
public static partial class ecdsa_internal_test_package
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
    [GoInit] internal static void initᴛᴛimportꓸbufio() => builtin.initPackage(typeof(bufio_package));
    [GoInit] internal static void initᴛᴛimportꓸbytes() => builtin.initPackage(typeof(bytes_package));
    [GoInit] internal static void initᴛᴛimportꓸcompressꓸbzip2() => builtin.initPackage(typeof(compress.bzip2_package));
    [GoInit] internal static void initᴛᴛimportꓸcrypto() => builtin.initPackage(typeof(crypto_package));
    [GoInit] internal static void initᴛᴛimportꓸcryptoꓸecdsa() => builtin.initPackage(typeof(go.crypto.ecdsa_package));
    [GoInit] internal static void initᴛᴛimportꓸcryptoꓸelliptic() => builtin.initPackage(typeof(go.crypto.elliptic_package));
    [GoInit] internal static void initᴛᴛimportꓸcryptoꓸinternalꓸcryptotest() => builtin.initPackage(typeof(go.crypto.@internal.cryptotest_package));
    [GoInit] internal static void initᴛᴛimportꓸcryptoꓸrand() => builtin.initPackage(typeof(go.crypto.rand_package));
    [GoInit] internal static void initᴛᴛimportꓸcryptoꓸsha1() => builtin.initPackage(typeof(go.crypto.sha1_package));
    [GoInit] internal static void initᴛᴛimportꓸcryptoꓸsha256() => builtin.initPackage(typeof(go.crypto.sha256_package));
    [GoInit] internal static void initᴛᴛimportꓸcryptoꓸsha512() => builtin.initPackage(typeof(go.crypto.sha512_package));
    [GoInit] internal static void initᴛᴛimportꓸcryptoꓸx509() => builtin.initPackage(typeof(go.crypto.x509_package));
    [GoInit] internal static void initᴛᴛimportꓸencodingꓸhex() => builtin.initPackage(typeof(encoding.hex_package));
    [GoInit] internal static void initᴛᴛimportꓸhash() => builtin.initPackage(typeof(hash_package));
    [GoInit] internal static void initᴛᴛimportꓸio() => builtin.initPackage(typeof(io_package));
    [GoInit] internal static void initᴛᴛimportꓸmathꓸbig() => builtin.initPackage(typeof(math.big_package));
    [GoInit] internal static void initᴛᴛimportꓸos() => builtin.initPackage(typeof(os_package));
    [GoInit] internal static void initᴛᴛimportꓸstrings() => builtin.initPackage(typeof(strings_package));
    [GoInit] internal static void initᴛᴛimportꓸtesting() => builtin.initPackage(typeof(testing_package));
    // </ImportInitializers>
}
