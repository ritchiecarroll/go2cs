// go2cs metadata anchor for the INTERNAL (white-box bridge) test class: GoImplement /
// GoImplicitConv attributes whose GENERATED code must merge with a bridge-declared type
// anchor here — the source generators host output in the first class of the
// attribute-bearing file, and only this file's first class is the bridge. Records for
// production and external-test types stay in package_test_info.cs.

// <ImportedTypeAliases>
// </ImportedTypeAliases>

using go;
using static go.go.token_package;
using static go.go.token_internal_test_package;

// <ExportedTypeAliases>
[assembly: GoDynamicTypeLift("7374727563747b66696c656e616d6520737472696e673b20736f75726365205b5d627974653b2073697a6520696e743b206c696e6573205b5d696e747d", "testsᴛ1")]
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
[assembly: global::go.GoPositionMap("go/token/export_test.go", "export_test.cs", "AAkSkA==")]
[assembly: global::go.GoPositionMap("go/token/position_test.go", "position_test.cs", "ABMggoKUgpSClIL4goKUgoKCAAgGAAoggoKCgpSUpqKCgoKClIKCgriCgoKCpqaCgoKUgriCgpSClIK4goKCpoKClLiAgqSClIKUpoKUlIKClPiCgoKUgoKmgoKCgoK4goKCgqaUgoKCgpSClILMkoKClICCyIKCgoKClIKCgs6igoKUgoKCgoKCooKUpqqigryigpSmsoKClKaCAA0GggALFIKCgqaCgoKCgoKCqKKCpoKCgriCgoKUgoKClIKClIIACQiCgoKClIKCgoIACgqCgoKCgoKClJKAgraSgoCAkoK4goKCgqaCgoKmgoKCAAsGgqoARZQBspKCgoKUgty0goKCyoKCgqaAgqSAgqSAgsiAgqSAgsiCgIKkgoCCuJKCgoCCtoKAgtYACwSClJKCgILIgoKClIKUgoKClIKCpoKCgoKCgoKUgqiSlpKCgriCgsKCgoKUgoKmooK4goKCgoKUuKKWkoKCppKmgoKAggAICoKCgoCC", "183-189:1;235-240:1;255-260:1;262-268:2;357-361:1;362-368:2;364-364:2.1;470-479:1;486-490:1;545-550:1;574-578:1;597-606:1;608-612:2;635-637:1")]
[assembly: global::go.GoPositionMap("go/token/serialize_test.go", "serialize_test.cs", "ABEi8pS4goKClIKmgoKCpoKCgpSClIKUgoKCpoKCgtzWgoKGgIKCpIKGgIKCpICCyIKClIKClIKCgoKCpg==", "70-72:1;78-80:2")]
[assembly: global::go.GoPositionMap("go/token/token_test.go", "token_test.cs", "ABASogALILKSgII=", "27-31:1")]
[assembly: global::go.GoPositionMap("go/token/tree_test.go", "tree_test.cs", "AA4cpIKCpoKCgoKSgqiSgoKCgsqCgoKClJSCgpS4goKUyoKCAAcQgoK4opI=", "76-76:1;83-85:1")]
// </GoSourcePositionMaps>

namespace go.go;

[GoPackage("token")]
public static partial class token_internal_test_package
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
    [GoInit] internal static void initᴛᴛimportꓸencodingꓸgob() => builtin.initPackage(typeof(encoding.gob_package));
    [GoInit] internal static void initᴛᴛimportꓸfmt() => builtin.initPackage(typeof(fmt_package));
    [GoInit] internal static void initᴛᴛimportꓸmathꓸrand() => builtin.initPackage(typeof(math.rand_package));
    [GoInit] internal static void initᴛᴛimportꓸmathꓸrandꓸv2() => builtin.initPackage(typeof(math.rand.rand_package));
    [GoInit] internal static void initᴛᴛimportꓸslices() => builtin.initPackage(typeof(slices_package));
    [GoInit] internal static void initᴛᴛimportꓸstrings() => builtin.initPackage(typeof(strings_package));
    [GoInit] internal static void initᴛᴛimportꓸsync() => builtin.initPackage(typeof(sync_package));
    [GoInit] internal static void initᴛᴛimportꓸtesting() => builtin.initPackage(typeof(testing_package));
    // </ImportInitializers>
}
