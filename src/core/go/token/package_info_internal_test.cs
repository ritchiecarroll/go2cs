// go2cs metadata anchor for the INTERNAL (white-box bridge) test class: GoImplement /
// GoImplicitConv attributes whose GENERATED code must merge with a bridge-declared type
// anchor here — the source generators host output in the first class of the
// attribute-bearing file, and only this file's first class is the bridge. Records for
// production and external-test types stay in package_test_info.cs.

// <ImportedTypeAliases>
using testing = go.testing_package;
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
[assembly: global::go.GoPositionMap("go/token/position_test.go", "position_test.cs", "AA8egoKUgpSClIL4goKUgoKCABQmgoKCgpSUpqKCgoKClIKCgriCgoKCpqaCgoKUgqiCgpSClIKogoKCpoKClKiAgqSClIKUloKUlIKClPiCgoKUgoKmgoKCgoK4goKCgqaUgoKCgpSClILMkoKClICCyIKCgoKClIKCgs6igoKUgoKCgoKCooKUpqqigqyigpSWsoKClJaCAA0GggAKFIKCgpaCgoKCgoKCmKKCloKCgqiCgoKUgoKClIKClIIACQiCgoKChIKCgoIACgqCgoKCgoKChJKAgraSgoCAkoKogoKCgpaCgoKWgoKCAAsGgpoARJQBspKCgoKUgty0goKCuoKCgpaAgqSAgqSAgriAgqSAgriCgIKkgoCCqJKCgoCCpoKAgg==", "182-188:1;234-239:1;254-259:1;261-267:2;356-360:1;361-367:2;363-363:2.1;469-478:1;485-489:1")]
[assembly: global::go.GoPositionMap("go/token/serialize_test.go", "serialize_test.cs", "AA8g8pSogoKChIKWgpaCgoKUgpSClIKCgqaCgoLM1oKChoCCgqSChoCCgqSAgsiCgpSCgpSCgoKCgqY=", "67-69:1;75-77:2")]
[assembly: global::go.GoPositionMap("go/token/token_test.go", "token_test.cs", "ABASogAKILKSgII=", "27-31:1")]
// </GoSourcePositionMaps>

namespace go.go;

[GoPackage("token")]
public static partial class token_internal_test_package
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
    [GoInit] internal static void initᴛᴛimportꓸencodingꓸgob() => builtin.initPackage(typeof(encoding.gob_package));
    [GoInit] internal static void initᴛᴛimportꓸfmt() => builtin.initPackage(typeof(fmt_package));
    [GoInit] internal static void initᴛᴛimportꓸmathꓸrand() => builtin.initPackage(typeof(math.rand_package));
    [GoInit] internal static void initᴛᴛimportꓸslices() => builtin.initPackage(typeof(slices_package));
    [GoInit] internal static void initᴛᴛimportꓸsync() => builtin.initPackage(typeof(sync_package));
    [GoInit] internal static void initᴛᴛimportꓸtesting() => builtin.initPackage(typeof(testing_package));
    // </ImportInitializers>
}
