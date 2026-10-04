// go2cs metadata anchor for a production-reference test project: the test assembly
// REFERENCES the colocated production project instead of
// recompiling its sources, so the production assembly is the single identity for the
// production types and no production class partial may be declared here. The first —
// and only — class is the test metadata class the go2cs-gen generators anchor
// generated adapters and partials to.
global using static global::go.crypto.sha512_package;

// <ImportedTypeAliases>
global using cryptoꓸDecrypterOpts = object;
global using cryptoꓸPrivateKey = object;
global using cryptoꓸPublicKey = object;
// </ImportedTypeAliases>

using go;
using static global::go.crypto.sha512_internal_test_package;

// <ExportedTypeAliases>
// </ExportedTypeAliases>

// <InterfaceImplementations>
[assembly: GoImplement<hash_package.Hash, io_package.Writer>]
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
[assembly: go.GoPositionMap("crypto/sha512/sha512_test.go", "sha512_test.cs", "ABo0AJ0BvAIAnQG8AgCdAbwCAJ0BvAKCgIKCpoKClIKCloCCpOiCggAKCIIAAxCA/ID8gPyA2oKCgsqCggAJCIIABRaykoKChISCgoKWgoKClISCgpaCgpaAgoKmgoSAgv6C3oKCgpaChIKCgpaAguyCgoCCpIKAgqSCgIKkgoCCyIKCgIIAEiIAChjCgoCCuOaChIKAgoKmgoKCloLKgoKAgoKEgoKCgpSCgoKClIKCgoKUgoKCgpaCgoKUAAoIgoKCpoKCpoKCpoKCypLkgoKSgoKCgoKmgoKCgqaCgoKCyoKmgqaC", "681-683:1;695-695:1;701-701:2;707-707:3;713-713:4;727-729:1;745-787:1;873-877:1;905-938:1;944-948:1;945-947:1.1;949-953:2;950-952:2.1;954-958:3;955-957:3.1;959-963:4;960-962:4.1;971-979:1;980-986:2;987-993:3")]
// </GoSourcePositionMaps>

namespace go.crypto;

[GoPackage("sha512")]
public static partial class sha512_internal_test_package
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
    [GoInit] internal static void initᴛᴛimportꓸcryptoꓸinternalꓸcryptotest() => builtin.initPackage(typeof(go.crypto.@internal.cryptotest_package));
    [GoInit] internal static void initᴛᴛimportꓸencodingꓸhex() => builtin.initPackage(typeof(go.encoding.hex_package));
    [GoInit] internal static void initᴛᴛimportꓸfmt() => builtin.initPackage(typeof(fmt_package));
    [GoInit] internal static void initᴛᴛimportꓸhash() => builtin.initPackage(typeof(hash_package));
    [GoInit] internal static void initᴛᴛimportꓸio() => builtin.initPackage(typeof(io_package));
    [GoInit] internal static void initᴛᴛimportꓸtesting() => builtin.initPackage(typeof(testing_package));
    // </ImportInitializers>
    // Go runs every `init` in the package under test - the production files' included -
    // before the first test. The production package is a REFERENCED assembly here, whose
    // module constructor .NET would not run until something in it is touched, so that
    // initialization is forced before anything else in this test module runs.
    [GoInit] internal static void initᴛᴛproduction() {
        builtin.initPackage(typeof(global::go.crypto.sha512_package));
    }
}
