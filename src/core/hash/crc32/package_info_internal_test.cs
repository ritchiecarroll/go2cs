// go2cs metadata anchor for the INTERNAL (white-box bridge) test class: GoImplement /
// GoImplicitConv attributes whose GENERATED code must merge with a bridge-declared type
// anchor here — the source generators host output in the first class of the
// attribute-bearing file, and only this file's first class is the bridge. Records for
// production and external-test types stay in package_test_info.cs.

// <ImportedTypeAliases>
// </ImportedTypeAliases>

using go;
using static go.hash.crc32_package;
using static go.hash.crc32_internal_test_package;

// <ExportedTypeAliases>
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
[assembly: go.GoPositionMap("hash/crc32/crc32_test.go", "crc32_test.cs", "ABUmyIKCpoIACRQAI0yigoCC3qKCgILe2LiCgoKCgoKCzJKSmILqgoKCgpSUgoKCpoKCgpSUgoKmgoKmgIKCtoKUgriCgoKClJSCgoKmgoKClJSCgqaCgqaAgoK2gpSC3IKClIKCpoCCypKSmIKqgoKWgpbogoKUgoLqgoKUgoK6griSgoKCgpSCgriCgqaSgoLKkpKCgoKUgoIACQqCgoKmgoKCgoKUgpKCAAcQooKCgoKUpoKClJSCgoKCgg==", "29-29:1;120-122:1;125-127:2;131-173:1;174-217:2;237-239:1;242-244:2;249-251:3;253-255:4;266-268:1;277-279:1;288-297:1;305-309:2;314-323:3;334-348:1;340-346:1.1;342-344:1.1.1", "", "43=Repeat/1/2/32,Repeat/2/2/33")]
// </GoSourcePositionMaps>

namespace go.hash;

[GoPackage("crc32")]
public static partial class crc32_internal_test_package
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
    [GoInit] internal static void initᴛᴛimportꓸfmt() => builtin.initPackage(typeof(fmt_package));
    [GoInit] internal static void initᴛᴛimportꓸhash() => builtin.initPackage(typeof(hash_package));
    [GoInit] internal static void initᴛᴛimportꓸinternalꓸtesthash() => builtin.initPackage(typeof(@internal.testhash_package));
    [GoInit] internal static void initᴛᴛimportꓸio() => builtin.initPackage(typeof(io_package));
    [GoInit] internal static void initᴛᴛimportꓸmathꓸrand() => builtin.initPackage(typeof(math.rand_package));
    [GoInit] internal static void initᴛᴛimportꓸstrings() => builtin.initPackage(typeof(strings_package));
    [GoInit] internal static void initᴛᴛimportꓸtesting() => builtin.initPackage(typeof(testing_package));
    // </ImportInitializers>
}
