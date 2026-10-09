// go2cs metadata anchor for the INTERNAL (white-box bridge) test class: GoImplement /
// GoImplicitConv attributes whose GENERATED code must merge with a bridge-declared type
// anchor here — the source generators host output in the first class of the
// attribute-bearing file, and only this file's first class is the bridge. Records for
// production and external-test types stay in package_test_info.cs.

// <ImportedTypeAliases>
// </ImportedTypeAliases>

using go;
using static go.maps_package;
using static go.maps_internal_test_package;

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
[assembly: go.GoPositionMap("maps/iter_test.go", "iter_test.cs", "AAwYgoKCgpSCgoKClJSCyoKCgoKCgpaCgpSCgsqCgoKCgoKWgoKUgoLKgriCgoK6uO6WgriC7oKC", "77-83:1")]
[assembly: go.GoPositionMap("maps/maps_test.go", "maps_test.cs", "AAwaksSCgpSClIKUgpSAgriCgrqSqJKCqJLWgoKUgpSClIKUgIK4goKmgpaCuIKCgpSCgriCgoKC+IKCgoKUgoKClqqCgoKClIKCgtyigoKUgoK4goKClIKUgoKUgoLKgoKCgpSCgpSCggAICoSIhJSCgoKEztIABBLi2paCgoKWsoKUgg==", "47-47:1;126-126:1;130-130:2")]
// </GoSourcePositionMaps>

namespace go;

[GoPackage("maps")]
public static partial class maps_internal_test_package
{
    // C# nested types declared with no access modifier are always private, and the
    // Go type declarations in this package's converted sources are deliberately
    // bare so they read more like the original Go code. The real accessibility for
    // the types - public for a Go-exported name, internal otherwise - are defined
    // via declarations below.

    // <TypeAccessibility>
    [GoLocalName("K")] partial struct TestCloneLarge_K {}
    [GoLocalName("V")] partial struct TestCloneLarge_V {}
    [GoLocalName("M1")] partial struct TestCopy_M1 {}
    [GoLocalName("M2")] partial struct TestCopy_M2 {}
    // </TypeAccessibility>

    // Go initializes an imported package before the importing package, for every import
    // form - not only the blank one. .NET would never load an assembly nothing has touched
    // yet, so each import that initializes anything is forced below: once per assembly, and
    // ahead of this package's own `init` functions, which this file being the first compile
    // item of the project guarantees.

    // <ImportInitializers>
    [GoInit] internal static void initᴛᴛimportꓸmath() => builtin.initPackage(typeof(math_package));
    [GoInit] internal static void initᴛᴛimportꓸslices() => builtin.initPackage(typeof(slices_package));
    [GoInit] internal static void initᴛᴛimportꓸstrconv() => builtin.initPackage(typeof(strconv_package));
    [GoInit] internal static void initᴛᴛimportꓸtesting() => builtin.initPackage(typeof(testing_package));
    // </ImportInitializers>
}
