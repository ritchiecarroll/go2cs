// go2cs metadata anchor for a production-reference test project: the test assembly
// REFERENCES the colocated production project instead of
// recompiling its sources, so the production assembly is the single identity for the
// production types and no production class partial may be declared here. The first —
// and only — class is the test metadata class the go2cs-gen generators anchor
// generated adapters and partials to.
global using static global::go.crypto.hmac_package;

// <ImportedTypeAliases>
// </ImportedTypeAliases>

using go;
using static global::go.crypto.hmac_internal_test_package;

// <ExportedTypeAliases>
// </ExportedTypeAliases>

// <InterfaceImplementations>
[assembly: GoImplement<justHash, hash_package.Hash>(Promoted = true)]
[assembly: GoImplement<justHash, hash_package.Hash>]
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
[assembly: go.GoPositionMap("crypto/hmac/hmac_test.go", "hmac_test.cs", "ACQ4APcDlAiCsoKAgqSAgqSCgoKCqIKCgrqoggAKDKKClIKCgoKmsgANDoKCgoSClIKUgpSCuIKCgoSi6qKCgoKCgoKCgriigoKCgoKCgoK4ooKCgoKCgg==", "580-580:1;591-596:1;597-597:2;629-631:1;630-630:1.1")]
// </GoSourcePositionMaps>

namespace go.crypto;

[GoPackage("hmac")]
public static partial class hmac_internal_test_package
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
    [GoInit] internal static void initᴛᴛimportꓸcryptoꓸinternalꓸboring() => builtin.initPackage(typeof(go.crypto.@internal.boring_package));
    [GoInit] internal static void initᴛᴛimportꓸcryptoꓸinternalꓸcryptotest() => builtin.initPackage(typeof(go.crypto.@internal.cryptotest_package));
    [GoInit] internal static void initᴛᴛimportꓸcryptoꓸmd5() => builtin.initPackage(typeof(go.crypto.md5_package));
    [GoInit] internal static void initᴛᴛimportꓸcryptoꓸsha1() => builtin.initPackage(typeof(go.crypto.sha1_package));
    [GoInit] internal static void initᴛᴛimportꓸcryptoꓸsha256() => builtin.initPackage(typeof(go.crypto.sha256_package));
    [GoInit] internal static void initᴛᴛimportꓸcryptoꓸsha512() => builtin.initPackage(typeof(go.crypto.sha512_package));
    [GoInit] internal static void initᴛᴛimportꓸfmt() => builtin.initPackage(typeof(fmt_package));
    [GoInit] internal static void initᴛᴛimportꓸhash() => builtin.initPackage(typeof(hash_package));
    [GoInit] internal static void initᴛᴛimportꓸtesting() => builtin.initPackage(typeof(testing_package));
    // </ImportInitializers>
    // Go runs every `init` in the package under test - the production files' included -
    // before the first test. The production package is a REFERENCED assembly here, whose
    // module constructor .NET would not run until something in it is touched, so that
    // initialization is forced before anything else in this test module runs.
    [GoInit] internal static void initᴛᴛproduction() {
        builtin.initPackage(typeof(global::go.crypto.hmac_package));
    }
}
