// go2cs metadata anchor for the INTERNAL (white-box bridge) test class: GoImplement /
// GoImplicitConv attributes whose GENERATED code must merge with a bridge-declared type
// anchor here — the source generators host output in the first class of the
// attribute-bearing file, and only this file's first class is the bridge. Records for
// production and external-test types stay in package_test_info.cs.

// <ImportedTypeAliases>
// </ImportedTypeAliases>

using go;
using static go.go.constant_package;
using static go.go.constant_internal_test_package;

// <ExportedTypeAliases>
[assembly: GoDynamicTypeLift("7374727563747b696e70757420737472696e673b2073686f727420737472696e673b20657861637420737472696e677d", "stringTestsᴛ1")]
[assembly: GoDynamicTypeLift("7374727563747b76616c20696e7436343b2077616e7420696e747d", "bitLenTestsᴛ1")]
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
[assembly: global::go.GoPositionMap("go/constant/value_test.go", "value_test.cs", "ABYgACFcACZqAA4kgoKCgoKWgoKClICCgoKUpIKogoKCgpaCloLOooKCAAsGAGuSAoKCgoSSyoK0oraCgpaEgoKCgpaCgpaCgsqCgoKClMwADwYAI2KCgoKAgqSAggAFEKKClpSkpKaAlIKCpoKApIK0tLSC2KYAFDLC2LTExKTihIKWlKSCpAAKDgALGKKCyoKCgoLK7oKCgpaCloCCpoCC2oKCAAIUgoKCgpSAgqSAguyCggALGIKCqIKCuoKCgoIACRSApIIAChqCgoLcgpKCgoKCgoKClJSCAAoMAAkagoKAgg==", "692-706:1", "", "593=MakeBool/1/1/2,MakeString/1/1/3,MakeInt64/1/1/4,MakeFromLiteral/1/2/5,MakeFromLiteral/2/2/6,MakeFloat64/1/2/7,MakeFloat64/2/2/8,MakeImag/1/1/8;674=Kind/1/1/1")]
// </GoSourcePositionMaps>

namespace go.go;

[GoPackage("constant")]
public static partial class constant_internal_test_package
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
    [GoInit] internal static void initᴛᴛimportꓸgoꓸtoken() => builtin.initPackage(typeof(global::go.go.token_package));
    [GoInit] internal static void initᴛᴛimportꓸmath() => builtin.initPackage(typeof(math_package));
    [GoInit] internal static void initᴛᴛimportꓸmathꓸbig() => builtin.initPackage(typeof(global::go.math.big_package));
    [GoInit] internal static void initᴛᴛimportꓸstrings() => builtin.initPackage(typeof(strings_package));
    [GoInit] internal static void initᴛᴛimportꓸtesting() => builtin.initPackage(typeof(testing_package));
    // </ImportInitializers>
}
