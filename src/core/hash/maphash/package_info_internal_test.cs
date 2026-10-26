// go2cs metadata anchor for the INTERNAL (white-box bridge) test class: GoImplement /
// GoImplicitConv attributes whose GENERATED code must merge with a bridge-declared type
// anchor here — the source generators host output in the first class of the
// attribute-bearing file, and only this file's first class is the bridge. Records for
// production and external-test types stay in package_test_info.cs.

// <ImportedTypeAliases>
// </ImportedTypeAliases>

using go;
using static go.hash.maphash_package;
using static go.hash.maphash_internal_test_package;

// <ExportedTypeAliases>
[assembly: GoDynamicTypeLift("7374727563747b6120616e793b206220616e797d", "TestComparable_v1ᴛ1")]
[assembly: GoDynamicTypeLift("7374727563747b6120737472696e673b206220737472696e677d", "TestComparable_v1")]
[assembly: GoDynamicTypeLift("7374727563747b6920696e743b20752075696e743b206220626f6f6c3b206620666c6f617436343b2070202a696e743b206120616e797d", "TestComparable_v")]
// </ExportedTypeAliases>

// <InterfaceImplementations>
[assembly: GoImplement<bytesKey, key>(Pointer = true)]
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
[assembly: go.GoPositionMap("hash/maphash/maphash_test.go", "maphash_test.cs", "ABYogoKCgpSCuIKCgoKCgpSCuIKCgoKUgpSClIKCgqaCgoKmgoKCuIKCgoKUlIKClLiCgoK4gIK2gIL4goKCgoKCgoKUgoKUgriEkoKCgoKUguiCgoKUgoKUgqaCgoKUgriigoKCgoKCgoK4ooKCgoKCgoKCgriigoKCgoKCgoKCuKKCgoKCgoKCgoKCuKKCggAXBoKCgoKCgoKCgoKCAAAQiIKClIKUgoKCgoKCgoKCgoKCpoKCgriUgtaikrKClIKCgpSCgoKCAAYQkoKUlIKC5oKCgoKCgoKCgoKCAAAQiIKClIKUgoKCgoKCgqaCgpKSgoKCguqikrKClIKSkoKCgoKClIKCgoKCgvqigoKCgoKClIKAgrai1IKCkoKUgoKClIIADAiCgpSClJKCkoKUgpaagpSCypKUgvaCgoKUooKCgoK4ooKCgriSgoKCyoKCgoLKgoKCggANCoIAABKCgoKCgoKCgoI=", "56-61:1;62-67:2;68-73:3;284-300:1;371-394:1;400-410:1;439-442:1;451-454:2;465-465:1;473-480:1;482-488:2;490-496:3;502-504:1;509-514:1")]
[assembly: go.GoPositionMap("hash/maphash/smhasher_test.go", "smhasher_test.cs", "ABs4mrKCgoKCgoKCgoKCgoKCgoLugoKCgqSCgoKCyoIABxCCpIKkgqSCpIKCgoKkgoKClIKCgqaUgoKCgoKmqJKCgoKClKiSgoKCgoKCgoKCgoKCyqiSgoKClIKCgpTokoKUgpSCgoKUpKKmpoKCgoLKgoKCgoKCgqbMkoKUgoKCgoKCgoKCgoKCgoKUlLqSgpSClIKCgoKCgoKCgqSigoKCqLKCgpSCgoK8ooKUgpSCgoKCgoKkooKCgqSigoKUgoKCgoIAECKCpIKkgqSCpIKkgqiSgpSClIKCgoKCgqSCgoKCuJSUgqaCgoKmgoKCAAkUgpSUgoKCgoKCgoKC3pKCpIKClIKUlIKCgoKCgqaUAAgKkoKUgoKCgtSigoKCgoKCgoKCgoKCgoKCgsrokoKUgoKCgoKClA==")]
// </GoSourcePositionMaps>

namespace go.hash;

[GoPackage("maphash")]
public static partial class maphash_internal_test_package
{
    // C# nested types declared with no access modifier are always private, and the
    // Go type declarations in this package's converted sources are deliberately
    // bare so they read more like the original Go code. The real accessibility for
    // the types - public for a Go-exported name, internal otherwise - are defined
    // via declarations below.

    // <TypeAccessibility>
    [GoLocalName("testStruct")] partial struct BenchmarkComparable_testStruct {}
    [GoLocalName("S")] partial struct TestComparableAllocations_S {}
    [GoLocalName("S")] partial struct TestComparable_S {}
    [GoLocalName("S")] partial struct TestWriteComparable_S {}
    // </TypeAccessibility>

    // Go initializes an imported package before the importing package, for every import
    // form - not only the blank one. .NET would never load an assembly nothing has touched
    // yet, so each import that initializes anything is forced below: once per assembly, and
    // ahead of this package's own `init` functions, which this file being the first compile
    // item of the project guarantees.

    // <ImportInitializers>
    [GoInit] internal static void initᴛᴛimportꓸbytes() => builtin.initPackage(typeof(bytes_package));
    [GoInit] internal static void initᴛᴛimportꓸfmt() => builtin.initPackage(typeof(fmt_package));
    [GoInit] internal static void initᴛᴛimportꓸhash() => builtin.initPackage(typeof(hash_package));
    [GoInit] internal static void initᴛᴛimportꓸinternalꓸruntimeꓸmaps() => builtin.initPackage(typeof(@internal.runtime.maps_package));
    [GoInit] internal static void initᴛᴛimportꓸinternalꓸtestenv() => builtin.initPackage(typeof(@internal.testenv_package));
    [GoInit] internal static void initᴛᴛimportꓸinternalꓸtesthash() => builtin.initPackage(typeof(@internal.testhash_package));
    [GoInit] internal static void initᴛᴛimportꓸmath() => builtin.initPackage(typeof(math_package));
    [GoInit] internal static void initᴛᴛimportꓸmathꓸrand() => builtin.initPackage(typeof(go.math.rand_package));
    [GoInit] internal static void initᴛᴛimportꓸreflect() => builtin.initPackage(typeof(reflect_package));
    [GoInit] internal static void initᴛᴛimportꓸruntime() => builtin.initPackage(typeof(runtime_package));
    [GoInit] internal static void initᴛᴛimportꓸslices() => builtin.initPackage(typeof(slices_package));
    [GoInit] internal static void initᴛᴛimportꓸstrings() => builtin.initPackage(typeof(strings_package));
    [GoInit] internal static void initᴛᴛimportꓸtesting() => builtin.initPackage(typeof(testing_package));
    // </ImportInitializers>
}
