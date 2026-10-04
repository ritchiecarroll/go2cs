// go2cs metadata anchor for a production-reference test project: the test assembly
// REFERENCES the colocated production project instead of
// recompiling its sources, so the production assembly is the single identity for the
// production types and no production class partial may be declared here. The first —
// and only — class is the test metadata class the go2cs-gen generators anchor
// generated adapters and partials to.
global using static global::go.hash.fnv_package;

// <ImportedTypeAliases>
// </ImportedTypeAliases>

using go;
using static global::go.hash.fnv_internal_test_package;

// <ExportedTypeAliases>
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
[assembly: go.GoPositionMap("hash/fnv/fnv_test.go", "fnv_test.cs", "ABYs7u7u7u7ugqaCpoKmgqaCpoKmgoKCgoKUgpSAggALCoIABxiykoKChISCgoKWgoKClISCgpaCgpaAgoKmgoSAgv6CpoKmgqaCpIKmgqaCgoKEgIKmgIKmgoKAgqaCgoKAgqaUgoLGgoIACQ6SpoKmgqaCpoKmgqaigoKClISCgoKC", "110-110:1;111-111:2;112-112:3;113-113:4;114-114:5;115-115:6;118-160:7")]
// </GoSourcePositionMaps>

namespace go.hash;

[GoPackage("fnv")]
public static partial class fnv_internal_test_package
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
    [GoInit] internal static void initᴛᴛimportꓸencodingꓸbinary() => builtin.initPackage(typeof(go.encoding.binary_package));
    [GoInit] internal static void initᴛᴛimportꓸhash() => builtin.initPackage(typeof(hash_package));
    [GoInit] internal static void initᴛᴛimportꓸio() => builtin.initPackage(typeof(io_package));
    [GoInit] internal static void initᴛᴛimportꓸtesting() => builtin.initPackage(typeof(testing_package));
    // </ImportInitializers>
    // Go runs every `init` in the package under test - the production files' included -
    // before the first test. The production package is a REFERENCED assembly here, whose
    // module constructor .NET would not run until something in it is touched, so that
    // initialization is forced before anything else in this test module runs.
    [GoInit] internal static void initᴛᴛproduction() {
        builtin.initPackage(typeof(global::go.hash.fnv_package));
    }
}
